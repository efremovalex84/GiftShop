import { Link } from "react-router-dom";
import { useCart } from "../cart";

export function CartPage() {
  const { lines, total, setQuantity, remove } = useCart();

  if (lines.length === 0) {
    return (
      <div>
        <h1>Cart</h1>
        <p>Your cart is empty. <Link to="/">Browse products</Link>.</p>
      </div>
    );
  }

  return (
    <div>
      <h1>Cart</h1>
      <ul className="list">
        {lines.map((l) => (
          <li key={l.product.id} className="row">
            <span>{l.product.name} ({l.product.price})</span>
            <span className="controls">
              <input
                type="number"
                min={1}
                value={l.quantity}
                onChange={(e) => setQuantity(l.product.id, Number(e.target.value))}
              />
              <button onClick={() => remove(l.product.id)}>Remove</button>
            </span>
          </li>
        ))}
      </ul>
      <p className="total">Total: {total}</p>
      <Link to="/order"><button>Proceed to order</button></Link>
    </div>
  );
}
