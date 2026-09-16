import { useEffect, useState } from "react";
import { createOrder, getCustomers } from "../api";
import { useCart } from "../cart";
import type { Customer, OrderResponse } from "../types";

export function OrderPage() {
  const { lines, total, clear } = useCart();
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [customerId, setCustomerId] = useState<number | "">("");
  const [method, setMethod] = useState("Nova Poshta");
  const [address, setAddress] = useState("");
  const [branch, setBranch] = useState("");
  const [placed, setPlaced] = useState<OrderResponse | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getCustomers()
      .then((c) => {
        setCustomers(c);
        if (c.length > 0) setCustomerId(c[0].id);
      })
      .catch((e: Error) => setError(e.message));
  }, []);

  const submit = async () => {
    setError(null);
    if (customerId === "") {
      setError("Select a customer.");
      return;
    }
    try {
      const order = await createOrder({
        customerId,
        items: lines.map((l) => ({ productId: l.product.id, quantity: l.quantity })),
        delivery: { method, address, branch }
      });
      setPlaced(order);
      clear();
    } catch (e) {
      setError((e as Error).message);
    }
  };

  if (placed) {
    return (
      <div>
        <h1>Order placed</h1>
        <p>Order #{placed.id} — {placed.status}</p>
        <p>Total: {placed.total}</p>
      </div>
    );
  }

  return (
    <div>
      <h1>Order</h1>
      {lines.length === 0 && <p>Your cart is empty.</p>}

      <label>
        Customer
        <select
          value={customerId}
          onChange={(e) => setCustomerId(Number(e.target.value))}
        >
          {customers.map((c) => (
            <option key={c.id} value={c.id}>{c.name}</option>
          ))}
        </select>
      </label>

      <label>
        Delivery method
        <input value={method} onChange={(e) => setMethod(e.target.value)} />
      </label>
      <label>
        Address
        <input value={address} onChange={(e) => setAddress(e.target.value)} />
      </label>
      <label>
        Branch
        <input value={branch} onChange={(e) => setBranch(e.target.value)} />
      </label>

      <p className="total">Total: {total}</p>
      {error && <p className="error">{error}</p>}
      <button onClick={submit} disabled={lines.length === 0}>Place order</button>
    </div>
  );
}
