# AdventureWorks Elasticsearch Search

Full-stack product search for the AdventureWorks database. SQL Server remains the source of truth, Elasticsearch serves fast faceted search, ASP.NET Core exposes the API, and React renders the search UI.

## Stack

| Layer | Tech |
| --- | --- |
| Database | SQL Server 2025 container with AdventureWorks2022 |
| Search | Elasticsearch 9.4.1 |
| Backend | ASP.NET Core 10 Minimal API |
| Data access | EF Core 10 |
| Background work | `BackgroundService` + bounded `Channel<bool>` |
| Frontend | React 19 + TypeScript 6 + Vite 8 |
| UI runtime | nginx proxy for `/api` in Docker |
| Logging | Serilog console + rolling file |

## Quick Start

```bash
docker compose up -d --build
```

On first startup, `docker/sqlserver/entrypoint.sh` downloads and restores AdventureWorks2022. Later restarts skip the restore when the database already exists.

### Service URLs

| Service | URL |
| --- | --- |
| React UI | http://localhost:3000 |
| Search API | http://localhost:5001/api/products/search?q=bike |
| API docs | http://localhost:5001/docs |
| Health | http://localhost:5001/health |
| Elasticsearch | http://localhost:9200 |

## Current Structure

```text
.
+-- api
|   +-- AW.Api              # Minimal API endpoints, DTOs, validators, problem details
|   +-- AW.Application      # Use-case services and persistence/search interfaces
|   +-- AW.Domain           # Product entities, search models, Result/Error primitives
|   +-- AW.Infrastructure   # EF Core, Elasticsearch, indexing background job
|   +-- AW.slnx
|   +-- Directory.Packages.props
+-- docker
|   +-- sqlserver           # AdventureWorks restore entrypoint
+-- ui
|   +-- src
|   |   +-- features
|   |   |   +-- indexing
|   |   |   +-- product-search
|   |   +-- shared
|   +-- Dockerfile
|   +-- nginx.conf
|   +-- vite.config.ts
+-- docker-compose.yml
```

## Why These Patterns

### Clean Architecture

The backend is split into `Api`, `Application`, `Domain`, and `Infrastructure`.

`AW.Application` depends on interfaces such as `IProductRepository` and `IProductSearchStore`, not on EF Core or Elasticsearch directly. `AW.Infrastructure` provides those implementations. This keeps business flow independent from storage details, so SQL Server or Elasticsearch changes do not leak into endpoint code.

MediatR is intentionally not used. The use cases are small enough that direct services are clearer: fewer moving parts, easier navigation, and less ceremony.

### Result/Error Instead Of Expected Exceptions

Expected failures use `Result<T>` and `Error`:

- validation errors
- product not found
- manual indexing already queued
- Elasticsearch unavailable

Endpoints convert those errors to RFC 7807 `ProblemDetails` in one place. Exceptions are reserved for unexpected failures and handled by `GlobalExceptionHandler`.

### Repository And Search Store Boundaries

`IProductRepository` reads product data from SQL Server for indexing. `IProductSearchStore` owns Elasticsearch index creation, bulk indexing, search, thumbnail lookup, and stats.

This separation matters because indexing is a pipeline:

```text
SQL Server -> ProductRepository -> IndexingService -> IProductSearchStore -> Elasticsearch
```

Search is a separate read path:

```text
HTTP request -> Endpoint -> ProductSearchService -> IProductSearchStore -> Elasticsearch
```

### BackgroundService + Channel

`ProductIndexingBackgroundJob` runs the first index after startup, repeats on an interval, and also accepts manual triggers from `POST /api/indexing/trigger`.

The trigger queue is a bounded `Channel<bool>` with capacity `1`. That prevents overlapping re-index requests. If a trigger is already queued, the API returns a conflict instead of stacking duplicate work.

### Feature Folders In The UI

The React app groups code by feature:

- `features/product-search` contains the search API client, hook, components, and types.
- `features/indexing` contains index status UI and API calls.
- `shared` contains cross-feature utilities and API base configuration.

This keeps feature code close to the UI that uses it and avoids a flat `components/`, `hooks/`, `api/` layout that becomes harder to scan as the app grows.

### Relative `/api` Calls

The UI calls `/api/...` by default. Vite proxies that to `http://localhost:5000` during local development, and nginx proxies it to the API container in Docker.

Set `VITE_API_URL` only when you want to bypass the proxy and call a specific API origin directly.

## API Endpoints

```text
GET  /api/products/search?q={query}&category={cat}&color={color}&productLine={line}&minPrice={min}&maxPrice={max}&page={n}&pageSize={n}
GET  /api/products/{id}
GET  /api/products/{id}/thumbnail
GET  /api/indexing/status
POST /api/indexing/trigger
GET  /health
```

## Development

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

The API listens on `http://localhost:5000` when run directly. Docker Compose exposes it at `http://localhost:5001`.

### Frontend

Requires Node.js `^20.19.0` or `>=22.12.0` for Vite 8. The Docker image uses Node 26.

```bash
cd ui
npm install
npm run dev
```

Vite starts at `http://localhost:3000` when the port is available. If `3000` is busy, Vite prints the alternate URL, for example `http://localhost:3001`.

## Indexing Flow

1. `ProductIndexingBackgroundJob` waits 15 seconds after startup.
2. `ElasticsearchProductSearchStore.EnsureIndexExistsAsync()` creates `aw-products` if needed.
3. `IndexingService.RunIndexingAsync()` reads products from SQL Server in batches.
4. `ProductDocumentMapper.ToDocument()` maps domain products to Elasticsearch documents.
5. `BulkIndexAsync()` upserts documents with `ProductId` as the Elasticsearch document ID.
6. The job waits for either the configured interval or a manual trigger.

Default settings:

| Setting | Default |
| --- | --- |
| `Indexing:BatchSize` | `500` |
| `Indexing:IntervalMinutes` | `60` |
| Elasticsearch index | `aw-products` |

## Search Flow

```text
GET /api/products/search
  -> ProductSearchRequestDto
  -> FluentValidation
  -> ProductSearchMapper.ToFilter()
  -> ProductSearchService
  -> ElasticsearchQueryHelper.BuildQuery()
  -> ElasticsearchProductSearchStore.SearchAsync()
  -> ProductDocumentMapper.ToResult()
  -> ProductSearchMapper.ToDto()
```

Query behavior:

- text search uses `multi_match` with `best_fields`
- `name` is boosted highest because product names are the strongest relevance signal
- `productNumber` and `modelName` are also boosted because users often search by code or model
- filters use `bool.filter`, so category/color/price filtering does not distort relevance scores
- facets are returned for categories, colors, and product lines
- results sort by `_score desc`, then `listPrice asc`

Thumbnail images are stored in Elasticsearch but excluded from search responses. The UI fetches each image through `GET /api/products/{id}/thumbnail`, which keeps normal search payloads small.

## Elasticsearch Index Shape

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
+-- thumbnailPhoto     binary
```

`product_analyzer` uses `standard` tokenization plus `lowercase` and `asciifolding`, so searches are case-insensitive and more tolerant of accents.

## Configuration

| Key | Purpose |
| --- | --- |
| `ConnectionStrings:AdventureWorks` | SQL Server connection string |
| `Elasticsearch:Uri` | Elasticsearch node URI |
| `Elasticsearch:EnableDebugMode` | Log Elasticsearch request and response bodies |
| `Indexing:BatchSize` | Products per bulk indexing batch |
| `Indexing:IntervalMinutes` | Scheduled re-index interval |
| `VITE_API_URL` | Optional frontend API origin override |

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
