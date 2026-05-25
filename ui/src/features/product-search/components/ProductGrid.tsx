import { useState } from "react";
import type { ProductDto } from "../types";
import { colorToHex } from "../../../shared/utils/colorUtils";
import { API_BASE } from "../../../shared/api/apiBase";

interface GridProps {
  items: ProductDto[];
  loading: boolean;
}

export function ProductGrid({ items, loading }: GridProps) {
  if (loading) {
    return (
      <div className="product-grid">
        {Array.from({ length: 12 }).map((_, i) => (
          <div key={i} className="product-card skeleton" />
        ))}
      </div>
    );
  }

  if (items.length === 0) {
    return (
      <div className="empty-state">
        <span className="empty-icon">🔍</span>
        <p>No products found</p>
        <small>Try adjusting your search or filters</small>
      </div>
    );
  }

  return (
    <div className="product-grid">
      {items.map((item) => (
        <ProductCard key={item.productId} product={item} />
      ))}
    </div>
  );
}

function ProductCard({ product }: { product: ProductDto }) {
  const hasPrice = product.listPrice > 0;
  const [imgError, setImgError] = useState(false);
  const thumbnailUrl = `${API_BASE}/api/products/${product.productId}/thumbnail`;

  return (
    <div className="product-card">
      <div className="card-image">
        {!imgError ? (
          <img
            src={thumbnailUrl}
            alt={product.name}
            className="product-thumbnail"
            onError={() => setImgError(true)}
          />
        ) : (
          <div className="product-thumbnail-fallback">
            <span className="product-icon">{categoryIcon(product.categoryName)}</span>
          </div>
        )}
        {product.isDiscontinued && (
          <span className="badge badge-discontinued">Discontinued</span>
        )}
        {product.color && (
          <span
            className="color-dot"
            style={{ backgroundColor: colorToHex(product.color) }}
            title={product.color}
          />
        )}
      </div>

      <div className="card-body">
        <p className="product-number">{product.productNumber}</p>
        <h3 className="product-name" title={product.name}>{product.name}</h3>

        {product.categoryName && (
          <p className="product-meta">
            {product.categoryName}
            {product.subcategoryName && ` › ${product.subcategoryName}`}
          </p>
        )}

        <div className="card-footer">
          {hasPrice ? (
            <span className="price">
              ${product.listPrice.toLocaleString("en-US", { minimumFractionDigits: 2 })}
            </span>
          ) : (
            <span className="price price-zero">No Price</span>
          )}
          <div className="product-tags">
            {product.size && <span className="tag">Size: {product.size}</span>}
            {product.productLine && (
              <span className="tag">{productLineName(product.productLine)}</span>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

function categoryIcon(category?: string): string {
  const icons: Record<string, string> = {
    Bikes: "🚲",
    Components: "⚙️",
    Clothing: "👕",
    Accessories: "🎽",
  };
  return icons[category ?? ""] ?? "📦";
}

function productLineName(code: string): string {
  return { R: "Road", M: "Mountain", T: "Touring", S: "Standard" }[code] ?? code;
}
