import { Link, Route, Routes } from "react-router-dom";
import { CartProvider, useCart } from "./cart";
import { ProductsPage } from "./pages/ProductsPage";
import { CartPage } from "./pages/CartPage";
import { OrderPage } from "./pages/OrderPage";

function Nav() {
  const { lines } = useCart();
  const count = lines.reduce((sum, l) => sum + l.quantity, 0);
  return (
    <nav className="nav">
      <Link to="/">Products</Link>
      <Link to="/cart">Cart ({count})</Link>
      <Link to="/order">Order</Link>
    </nav>
  );
}

export default function App() {
  return (
    <CartProvider>
      <div className="container">
        <Nav />
        <Routes>
          <Route path="/" element={<ProductsPage />} />
          <Route path="/cart" element={<CartPage />} />
          <Route path="/order" element={<OrderPage />} />
        </Routes>
      </div>
    </CartProvider>
  );
}
