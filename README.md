# AdventureWorks Elasticsearch Search

Full-stack product search for the AdventureWorks database. SQL Server remains the source of truth, Elasticsearch provides fast full-text search and facets, ASP.NET Core exposes the API, and Angular renders the product search UI.

## Stack

| Layer | Tech |
| --- | --- |
| Database | SQL Server 2025 container with AdventureWorks2022 |
| Search | Elasticsearch 9.4.1 |
| Backend | ASP.NET Core 10 Minimal API |
| Data access | EF Core 10 |
| Background work | `BackgroundService` + bounded `Channel<bool>` |
| API docs | OpenAPI + Scalar |
| Frontend | Angular 21 + TypeScript 5.9 |
| UI state | NgRx Signals |
| UI components | PrimeNG 21, Angular CDK, Tailwind CSS 4 CLI |
| UI runtime | nginx with `/api` reverse proxy and runtime config |
| Logging | Serilog console + rolling file |

## Quick Start

Create local environment settings, then build and start the stack:

```bash
cp .env.example .env
docker compose up -d --build
```

On first startup, `docker/sqlserver/entrypoint.sh` downloads and restores `AdventureWorks2022.bak`. Later restarts reuse the Docker volume and skip the restore when the database already exists.

### Service URLs

With the default `.env.example` values:

| Service | URL |
| --- | --- |
| Angular UI | http://localhost:3000 |
| Search API | http://localhost:5001/api/products/search?q=bike |
| API docs | http://localhost:5001/docs |
| Health | http://localhost:5001/health |
| Elasticsearch | http://localhost:9200 |
| SQL Server | localhost:1433 |

The API docs are enabled when `ASPNETCORE_ENVIRONMENT=Development`.

## Project Structure

```text
.
+-- api
|   +-- AW.Api              # Minimal API endpoints, DTOs, validation, problem details
|   +-- AW.Application      # Use-case services and persistence/search interfaces
|   +-- AW.Domain           # Product entities, search models, Result/Error primitives
|   +-- AW.Infrastructure   # EF Core, Elasticsearch, indexing background job
|   +-- AW.slnx
|   +-- Directory.Packages.props
+-- docker
|   +-- sqlserver           # AdventureWorks restore entrypoint
+-- ui
|   +-- public              # Runtime config template and static assets
|   +-- src/app
|   |   +-- core            # App runtime configuration
|   |   +-- features
|   |   |   +-- products    # Product search page, store, API service, components
|   |   +-- shared          # Shared UI components
|   +-- angular.json
|   +-- Dockerfile
|   +-- nginx.conf
|   +-- proxy.conf.json
+-- docker-compose.yml
```

## Backend Design

The backend is split into `Api`, `Application`, `Domain`, and `Infrastructure`.

`AW.Application` depends on interfaces such as `IProductRepository`, `IProductSearchStore`, `IProductSearchService`, and `IIndexingTrigger`. `AW.Infrastructure` supplies the EF Core, Elasticsearch, and background job implementations. This keeps endpoint code independent from storage details.

Expected failures use `Result<T>` and `Error`, then endpoints convert those errors to RFC 7807 `ProblemDetails`. Unexpected exceptions are handled by `GlobalExceptionHandler`.

### Data Paths

Indexing reads products from SQL Server and writes documents to Elasticsearch:

```text
SQL Server -> ProductRepository -> IndexingService -> IProductSearchStore -> Elasticsearch
```

Search reads from Elasticsearch, while thumbnails are still served from SQL Server:

```text
HTTP request -> ProductSearchEndpoints -> ProductSearchService -> Elasticsearch
HTTP thumbnail request -> ProductSearchService -> ProductRepository -> SQL Server
```

## Frontend Design

The UI is an Angular application using route-backed search state.

- `ProductsPageComponent` owns the filter form, URL query params, pagination, and page events.
- `ProductsStore` is an NgRx Signals store for query state, loading, errors, totals, and computed ranges.
- `ProductsApiService` calls `/api/products/search`, validates backend responses with Zod, and maps API products into UI products.
- `ProductFilters`, `ProductCard`, and `Pagination` provide the feature UI.
- Tailwind CSS is generated into `src/tailwind.generated.css` before Angular builds.

The browser reads `window.__APP_CONFIG__.apiUrl` from `/runtime-config.js`. In Docker, `ui/docker-entrypoint.sh` writes that file from `ui/public/runtime-config.template.js` using the `API_URL` environment variable. If `API_URL` is empty, the UI uses same-origin `/api` calls through nginx.

## API Endpoints

API versioning uses the `X-Api-Version` request header. If the header is omitted, the API assumes version `1.0`.

```text
GET  /api/products/search?q={query}&category={cat}&color={color}&productLine={line}&minPrice={min}&maxPrice={max}&page={n}&pageSize={n}
GET  /api/products/{id}
GET  /api/products/{id}/thumbnail
GET  /api/indexing/status
POST /api/indexing/trigger
GET  /health
```

Search request validation:

| Field | Rule |
| --- | --- |
| `q` | Max 200 characters |
| `page` | Greater than 0 |
| `pageSize` | Greater than 0 and at most 100 |
| `minPrice` / `maxPrice` | Non-negative, and `minPrice <= maxPrice` when both are supplied |

## Search Behavior

Search uses Elasticsearch `multi_match` with `best_fields` and `AUTO` fuzziness. Configured field boosts are:

| Field | Boost |
| --- | --- |
| `name` | 3.0 |
| `productNumber` | 2.0 |
| `modelName` | 2.0 |
| `description` | 1.0 |
| `categoryName` | 1.0 |
| `subcategoryName` | 1.0 |

Filters are applied through `bool.filter` for category, color, product line, and price range. Results sort by `_score desc`, then `listPrice asc`.

Returned facets:

- `categories`
- `colors`
- `productLines`

## Indexing

`ProductIndexingBackgroundJob` waits 15 seconds after startup, ensures the Elasticsearch index exists, then runs the first indexing pass. After that, it re-indexes on the configured interval or when `POST /api/indexing/trigger` queues a manual run.

Default settings:

| Setting | Default |
| --- | --- |
| `Indexing:BatchSize` | `500` |
| `Indexing:IntervalMinutes` | `60` |
| Elasticsearch index | `aw-products` |

### Elasticsearch Index Shape

```text
aw-products
+-- productId          integer
+-- name               text with product_analyzer + keyword
+-- productNumber      keyword
+-- color              keyword
+-- listPrice          float
+-- standardCost       float
+-- size               keyword
+-- productLine        keyword
+-- class              keyword
+-- categoryName       keyword
+-- subcategoryName    keyword
+-- modelName          text with product_analyzer
+-- description        text with product_analyzer
+-- isDiscontinued     boolean
+-- sellStartDate      date
+-- indexedAt          date
```

`product_analyzer` uses the `standard` tokenizer with `lowercase` and `asciifolding` filters.

## Configuration

Docker Compose reads these values from `.env`:

| Key | Purpose |
| --- | --- |
| `UI_PORT` | Host port for nginx/frontend |
| `API_PORT` | Host port for the API container |
| `API_URL` | Runtime frontend API origin. Leave empty for same-origin `/api` through nginx, or set `http://localhost:5001` to call the exposed API directly. |
| `ASPNETCORE_ENVIRONMENT` | API environment |
| `SQLSERVER_SA_PASSWORD` | SQL Server `sa` password |
| `ELASTICSEARCH_URI` | API-to-Elasticsearch URI |
| `INDEXING_BATCH_SIZE` | Products per bulk indexing batch |
| `INDEXING_INTERVAL_MINUTES` | Scheduled re-index interval |

API app settings:

| Key | Purpose |
| --- | --- |
| `ConnectionStrings:AdventureWorks` | SQL Server connection string |
| `Elasticsearch:Uri` | Elasticsearch node URI |
| `Elasticsearch:EnableDebugMode` | Logs Elasticsearch request and response bodies |
| `Indexing:BatchSize` | Products per bulk indexing batch |
| `Indexing:IntervalMinutes` | Scheduled re-index interval |

## Local Development

### Backend

Start dependencies:

```bash
docker compose up -d sqlserver elasticsearch
```

Run the API:

```bash
cd api
dotnet restore AW.slnx
dotnet run --project AW.Api
```

The Docker API container listens on port `8080` internally and is exposed as `http://localhost:5001` by the default `.env.example`.

### Frontend

Install packages and run Angular dev server:

```bash
cd ui
npm install
npm start
```

`npm start` builds Tailwind once, starts the Tailwind watcher, and runs `ng serve --host 0.0.0.0`. Angular uses `proxy.conf.json` to proxy `/api/**` to `http://localhost:5001`.

## Verification

Backend:

```bash
cd api
dotnet build AW.slnx --no-restore -warnaserror
```

Frontend:

```bash
cd ui
npm run build
```
