import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Search } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import BookCard from '../components/BookCard';

const Catalog = () => {
    const [books, setBooks] = useState([]);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');

    const { user } = useAuth();
    const navigate = useNavigate();

    useEffect(() => {
        fetchBooks();
    }, []);

    const fetchBooks = async (query = '') => {
        setLoading(true);
        try {
            const url = query
                ? `/api/books?q=${encodeURIComponent(query)}`
                : '/api/books';

            const response = await fetch(url);
            const data = await response.json();
            let items = data.items || [];

            // Fetch inventory for these books
            if (items.length > 0) {
                try {
                    const bookIds = items.map(b => b.id);
                    const invResponse = await fetch('/api/inventory/batch', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(bookIds)
                    });

                    if (invResponse.ok) {
                        const inventories = await invResponse.json();
                        // Merge inventory data
                        items = items.map(book => {
                            const inv = inventories.find(i => i.bookId === book.id);
                            return {
                                ...book,
                                inventory: inv || null,
                                // Override isAvailable based on actual stock
                                isAvailable: inv ? (inv.physicalAvailable > 0 || inv.digitalAvailable > 0) : book.isAvailable
                            };
                        });
                    }
                } catch (invErr) {
                    console.warn("Failed to fetch batch inventory", invErr);
                }
            }

            setBooks(items);
        } catch (error) {
            console.error('Failed to fetch books:', error);
        } finally {
            setLoading(false);
        }
    };

    const handleSearch = (e) => {
        e.preventDefault();
        fetchBooks(searchTerm);
    };

    const handleBorrow = (book) => {
        navigate(`/checkout/${book.id}`);
    };

    return (
        <div className="container">
            <div className="flex flex-col md:flex-row justify-between items-center mb-8 gap-4">
                <div>
                    <h1 className="text-3xl font-bold mb-2">Library Catalog</h1>
                    <p className="text-slate-400">Browse our collection of books</p>
                </div>

                <form onSubmit={handleSearch} className="flex gap-2 w-full md:w-auto">
                    <div className="relative">
                        <input
                            type="text"
                            placeholder="Search books..."
                            className="input pl-10"
                            style={{ minWidth: '300px' }}
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                        />
                        <Search size={18} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
                    </div>
                    <button type="submit" className="btn btn-primary">Search</button>
                </form>
            </div>

            {loading ? (
                <div className="flex justify-center py-12">
                    <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-500"></div>
                </div>
            ) : (
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
                    {books.map((book) => (
                        <BookCard key={book.id} book={book} onBorrow={handleBorrow} />
                    ))}
                </div>
            )}

            {!loading && books.length === 0 && (
                <div className="text-center py-12 text-slate-500">
                    No books found. Try a different search term.
                </div>
            )}
        </div>
    );
};

export default Catalog;
