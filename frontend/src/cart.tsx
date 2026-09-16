import { createContext, useContext, useMemo, useState, type ReactNode } from "react";
import type { CartLine, Product } from "./types";

interface CartContextValue {
  lines: CartLine[];
  total: number;
  add: (product: Product) => void;
  setQuantity: (productId: number, quantity: number) => void;
  remove: (productId: number) => void;
  clear: () => void;
}

const CartContext = createContext<CartContextValue | undefined>(undefined);

export function CartProvider({ children }: { children: ReactNode }) {
  const [lines, setLines] = useState<CartLine[]>([]);

  const add = (product: Product) =>
    setLines((current) => {
      const existing = current.find((l) => l.product.id === product.id);
      if (existing) {
        return current.map((l) =>
          l.product.id === product.id ? { ...l, quantity: l.quantity + 1 } : l
        );
      }
      return [...current, { product, quantity: 1 }];
    });

  const setQuantity = (productId: number, quantity: number) =>
    setLines((current) =>
      current.map((l) =>
        l.product.id === productId ? { ...l, quantity: Math.max(1, quantity) } : l
      )
    );

  const remove = (productId: number) =>
    setLines((current) => current.filter((l) => l.product.id !== productId));

  const clear = () => setLines([]);

  const total = useMemo(
    () => lines.reduce((sum, l) => sum + l.product.price * l.quantity, 0),
    [lines]
  );

  return (
    <CartContext.Provider value={{ lines, total, add, setQuantity, remove, clear }}>
      {children}
    </CartContext.Provider>
  );
}

export function useCart(): CartContextValue {
  const context = useContext(CartContext);
  if (!context) {
    throw new Error("useCart must be used within a CartProvider");
  }
  return context;
}
