import { useEffect, useState } from "react";
import { getProducts } from "../api";
import { useCart } from "../cart";
import type { Product } from "../types";

export function ProductsPage() {
  const [products, setProducts] = useState<Product[]>([]);
  const [error, setError] = useState<string | null>(null);
  const { add } = useCart();

  useEffect(() => {
    getProducts()
      .then(setProducts)
      .catch((e: Error) => setError(e.message));
  }, []);

  if (error) {
    return <p className="error">Failed to load products: {error}</p>;
  }

  return (
    <div>
      <h1>Products</h1>
      <ul className="list">
        {products.map((p) => (
          <li key={p.id} className="row">
            <span>
              <strong>{p.name}</strong> — {p.price} ({p.stock} in stock)
            </span>
            <button onClick={() => add(p)}>Add to cart</button>
          </li>
        ))}
      </ul>
    </div>
  );
}
