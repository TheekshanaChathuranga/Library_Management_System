import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import Navbar from './components/Navbar';
import Login from './pages/Login';
import Register from './pages/Register';
import Catalog from './pages/Catalog';
import MyBooks from './pages/MyBooks';
import InventoryManager from './pages/InventoryManager';
import Checkout from './pages/Checkout';
import './App.css';

function App() {
    return (
        <AuthProvider>
            <Router>
                <div className="app">
                    <Navbar />
                    <main style={{ padding: '2rem 0' }}>
                        <Routes>
                            <Route path="/" element={
                                <div className="container text-center py-12">
                                    <h1 className="text-4xl font-bold mb-4">Welcome to LMS</h1>
                                    <p className="text-slate-400 text-lg mb-8">
                                        Manage your library resources efficiently.
                                    </p>
                                    <div className="flex justify-center gap-4">
                                        <a href="/catalog" className="btn btn-primary">Browse Catalog</a>
                                        <a href="/login" className="btn btn-outline">Login</a>
                                    </div>
                                </div>
                            } />
                            <Route path="/login" element={<Login />} />
                            <Route path="/register" element={<Register />} />
                            <Route path="/catalog" element={<Catalog />} />
                            <Route path="/my-books" element={<MyBooks />} />
                            <Route path="/inventory" element={<InventoryManager />} />
                            <Route path="/checkout/:bookId" element={<Checkout />} />
                        </Routes>
                    </main>
                </div>
            </Router>
        </AuthProvider>
    );
}

export default App;
