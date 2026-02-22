import { useState, useEffect } from 'react';
import { Plus, Edit, Trash2, Search, AlertCircle } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import InventoryForm from '../components/InventoryForm';

const InventoryManager = () => {
    const [books, setBooks] = useState([]);
    const [inventory, setInventory] = useState({});
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
    const [showForm, setShowForm] = useState(false);
    const [editingBook, setEditingBook] = useState(null);

    const { user, token } = useAuth();

    useEffect(() => {
        fetchData();
    }, []);

    const fetchData = async () => {
        setLoading(true);
        try {
            // Fetch books from Catalog Service
            const booksResponse = await fetch('/api/books?pageSize=100');
            const booksData = await booksResponse.json();

            // Fetch inventory from Inventory Service
            // Note: In a real app, we might want to fetch this per book or in batches
            // For now, we'll fetch a page and map it, or fetch individually if needed
            // Since there's no bulk fetch for specific IDs, we'll fetch the list
            const inventoryResponse = await fetch('/api/inventory?pageSize=100');
            const inventoryData = await inventoryResponse.json();

            // Create a map of bookId -> inventory details
            const invMap = {};
            if (Array.isArray(inventoryData)) {
                inventoryData.forEach(item => {
                    invMap[item.bookId] = item;
                });
            } else if (inventoryData.items) {
                inventoryData.items.forEach(item => {
                    invMap[item.bookId] = item;
                });
            }

            setBooks(booksData.items || []);
            setInventory(invMap);
        } catch (error) {
            console.error('Failed to fetch data:', error);
            alert('Failed to load inventory data');
        } finally {
            setLoading(false);
        }
    };

    const handleDelete = async (bookId) => {
        if (!window.confirm('Are you sure you want to delete this book? This action cannot be undone.')) {
            return;
        }

        try {
            const response = await fetch(`/api/books/${bookId}`, {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (response.ok) {
                alert('Book deleted successfully');
                fetchData(); // Refresh list
            } else {
                alert('Failed to delete book');
            }
        } catch (error) {
            console.error('Error deleting book:', error);
            alert('An error occurred while deleting the book');
        }
    };

    const handleEdit = (book) => {
        setEditingBook(book);
        setShowForm(true);
    };

    const handleAddNew = () => {
        setEditingBook(null);
        setShowForm(true);
    };

    const handleFormClose = (refresh = false) => {
        setShowForm(false);
        setEditingBook(null);
        if (refresh) {
            fetchData();
        }
    };

    const filteredBooks = books.filter(book =>
        book.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
        book.author.toLowerCase().includes(searchTerm.toLowerCase()) ||
        book.isbn.includes(searchTerm)
    );

    if (!user || !user.roles.some(r => r === 'Admin' || r === 'Librarian')) {
        return <div className="container py-8 text-center text-red-500">Access Denied</div>;
    }

    return (
        <div className="container">
            <div className="flex justify-between items-center mb-8">
                <div>
                    <h1 className="text-3xl font-bold mb-2">Inventory Management</h1>
                    <p className="text-slate-400">Manage library catalog and stock levels</p>
                </div>
                <button
                    onClick={handleAddNew}
                    className="btn btn-primary flex items-center gap-2"
                >
                    <Plus size={20} /> Add New Book
                </button>
            </div>

            <div className="mb-6 relative">
                <input
                    type="text"
                    placeholder="Search by title, author, or ISBN..."
                    className="input pl-10 w-full md:w-96"
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                />
                <Search size={18} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
            </div>

            {loading ? (
                <div className="flex justify-center py-12">
                    <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-500"></div>
                </div>
            ) : (
                <div className="overflow-x-auto bg-slate-800/50 rounded-xl border border-slate-700/50">
                    <table className="w-full text-left border-collapse">
                        <thead>
                            <tr className="border-b border-slate-700/50 text-slate-400 text-sm uppercase tracking-wider">
                                <th className="p-4">Book Details</th>
                                <th className="p-4">ISBN / Genre</th>
                                <th className="p-4 text-center">Physical Stock</th>
                                <th className="p-4 text-center">Digital Stock</th>
                                <th className="p-4 text-right">Actions</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-slate-700/50">
                            {filteredBooks.map(book => {
                                const inv = inventory[book.id] || { physicalAvailable: 0, physicalTotal: 0, digitalAvailable: 0, digitalTotal: 0 };
                                return (
                                    <tr key={book.id} className="hover:bg-slate-700/30 transition-colors">
                                        <td className="p-4">
                                            <div className="font-bold text-white">{book.title}</div>
                                            <div className="text-sm text-slate-400">{book.author}</div>
                                        </td>
                                        <td className="p-4">
                                            <div className="text-sm text-slate-300">{book.isbn}</div>
                                            <div className="text-xs text-slate-500">{book.genre}</div>
                                        </td>
                                        <td className="p-4 text-center">
                                            <div className="flex flex-col items-center">
                                                <span className={`font-mono font-bold ${inv.physicalAvailable > 0 ? 'text-green-400' : 'text-red-400'}`}>
                                                    {inv.physicalAvailable} / {inv.physicalTotal}
                                                </span>
                                            </div>
                                        </td>
                                        <td className="p-4 text-center">
                                            <div className="flex flex-col items-center">
                                                <span className="font-mono font-bold text-blue-400">
                                                    {inv.digitalAvailable} / {inv.digitalTotal}
                                                </span>
                                            </div>
                                        </td>
                                        <td className="p-4 text-right">
                                            <div className="flex justify-end gap-2">
                                                <button
                                                    onClick={() => handleEdit(book)}
                                                    className="p-2 text-slate-400 hover:text-indigo-400 hover:bg-indigo-400/10 rounded-lg transition-colors"
                                                    title="Edit Book"
                                                >
                                                    <Edit size={18} />
                                                </button>
                                                <button
                                                    onClick={() => handleDelete(book.id)}
                                                    className="p-2 text-slate-400 hover:text-red-400 hover:bg-red-400/10 rounded-lg transition-colors"
                                                    title="Delete Book"
                                                >
                                                    <Trash2 size={18} />
                                                </button>
                                            </div>
                                        </td>
                                    </tr>
                                );
                            })}
                        </tbody>
                    </table>

                    {filteredBooks.length === 0 && (
                        <div className="p-8 text-center text-slate-500 flex flex-col items-center gap-2">
                            <AlertCircle size={32} />
                            <p>No books found matching your search.</p>
                        </div>
                    )}
                </div>
            )}

            {showForm && (
                <InventoryForm
                    book={editingBook}
                    inventory={editingBook ? inventory[editingBook.id] : null}
                    onClose={handleFormClose}
                />
            )}
        </div>
    );
};

export default InventoryManager;
