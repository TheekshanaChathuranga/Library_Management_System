import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { LogOut, Book, User, Library, UserPlus } from 'lucide-react';

const Navbar = () => {
    const { user, logout } = useAuth();
    const navigate = useNavigate();

    const handleLogout = () => {
        logout();
        navigate('/login');
    };

    const canRegister = user?.roles?.some(role => role === 'Admin' || role === 'Librarian');

    return (
        <nav style={{
            backgroundColor: 'var(--color-surface)',
            borderBottom: '1px solid var(--color-border)',
            padding: '1rem 0'
        }}>
            <div className="container flex items-center justify-between">
                <Link to="/" className="flex items-center gap-2" style={{ fontSize: '1.25rem', fontWeight: 'bold' }}>
                    <Library size={24} color="var(--color-primary)" />
                    <span>LMS</span>
                </Link>

                <div className="flex items-center gap-4">
                    <Link to="/catalog" className="btn btn-outline">Catalog</Link>

                    {user ? (
                        <>
                            {canRegister && (
                                <Link to="/register" className="btn btn-outline flex items-center gap-2">
                                    <UserPlus size={16} />
                                    Register Member
                                </Link>
                            )}
                            {canRegister && (
                                <Link to="/inventory" className="btn btn-outline flex items-center gap-2">
                                    <Book size={16} />
                                    Inventory
                                </Link>
                            )}
                            <Link to="/my-books" className="btn btn-outline">My Books</Link>
                            <div className="flex items-center gap-2" style={{ marginLeft: '1rem' }}>
                                <span style={{ fontSize: '0.875rem', color: 'var(--color-text-muted)' }}>
                                    {user.name}
                                </span>
                                <button onClick={handleLogout} className="btn btn-primary flex items-center gap-2">
                                    <LogOut size={16} />
                                    Logout
                                </button>
                            </div>
                        </>
                    ) : (
                        <Link to="/login" className="btn btn-primary flex items-center gap-2">
                            <User size={16} />
                            Login
                        </Link>
                    )}
                </div>
            </div>
        </nav>
    );
};

export default Navbar;
