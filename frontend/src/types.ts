export interface Product {
  id: number;
  name: string;
  price: number;
  stock: number;
}

export interface Customer {
  id: number;
  name: string;
  phone: string;
}

export interface CartLine {
  product: Product;
  quantity: number;
}

export interface OrderItemResponse {
  id: number;
  productId: number;
  quantity: number;
  unitPrice: number;
}

export interface DeliveryResponse {
  method: string;
  address: string;
  branch: string;
}

export interface OrderResponse {
  id: number;
  customerId: number;
  status: string;
  total: number;
  createdAt: string;
  items: OrderItemResponse[];
  delivery: DeliveryResponse | null;
}
