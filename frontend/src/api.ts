import type { Customer, OrderResponse, Product } from "./types";

const BASE_URL = "http://localhost:5284";

async function handle<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || `Request failed with ${response.status}`);
  }
  return response.json() as Promise<T>;
}

export function getProducts(): Promise<Product[]> {
  return fetch(`${BASE_URL}/api/products`).then((r) => handle<Product[]>(r));
}

export function getCustomers(): Promise<Customer[]> {
  return fetch(`${BASE_URL}/api/customers`).then((r) => handle<Customer[]>(r));
}

export interface CreateOrderPayload {
  customerId: number;
  items: { productId: number; quantity: number }[];
  delivery: { method: string; address: string; branch: string };
}

export function createOrder(payload: CreateOrderPayload): Promise<OrderResponse> {
  return fetch(`${BASE_URL}/api/orders`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload)
  }).then((r) => handle<OrderResponse>(r));
}
