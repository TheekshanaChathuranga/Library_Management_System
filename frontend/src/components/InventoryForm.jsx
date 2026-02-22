import { useState, useEffect } from 'react';
import { X, Save } from 'lucide-react';
import { useAuth } from '../context/AuthContext';

const InventoryForm = ({ book, inventory, onClose }) => {
    const { token } = useAuth();
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');

    const [formData, setFormData] = useState({
        title: '',
        author: '',
        isbn: '',
        genre: '',
        isAvailable: true,
        physicalTotal: 0,
        digitalTotal: 0
    });

    useEffect(() => {
        if (book) {
            setFormData({
                title: book.title,
                author: book.author,
                isbn: book.isbn,
                genre: book.genre,
                isAvailable: book.isAvailable,
                physicalTotal: inventory ? inventory.physicalTotal : 0,
                digitalTotal: inventory ? inventory.digitalTotal : 0
            });
        }
    }, [book, inventory]);

    const handleSubmit = async (e) => {
        e.preventDefault();
        setLoading(true);
        setError('');

        try {
            let bookId = book ? book.id : null;

            // Step 1: Create or Update Book in Catalog
            const bookPayload = {
                title: formData.title,
                author: formData.author,
                isbn: formData.isbn,
                genre: formData.genre,
                isAvailable: formData.isAvailable
            };

            const bookUrl = book ? `/api/books/${book.id}` : '/api/books';
            const bookMethod = book ? 'PUT' : 'POST';

            const bookResponse = await fetch(bookUrl, {
                method: bookMethod,
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify(bookPayload)
            });

            if (!bookResponse.ok) {
                const err = await bookResponse.json();
                throw new Error(err.message || 'Failed to save book details');
            }

            if (!book) {
                const newBook = await bookResponse.json();
                bookId = newBook.id;
            }

            // Step 2: Create or Update Inventory
            // Note: Inventory API uses POST for creation and PUT for updates
            // For updates, we use the specific endpoint to update totals

            if (book) {
                // Update existing inventory
                const invPayload = {
                    physicalTotal: parseInt(formData.physicalTotal),
                    digitalTotal: parseInt(formData.digitalTotal)
                };

                const invResponse = await fetch(`/api/inventory/${bookId}`, {
                    method: 'PUT',
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': `Bearer ${token}`
                    },
                    body: JSON.stringify(invPayload)
                });

                if (!invResponse.ok) {
                    // If 404, it might be missing inventory record, try creating it
                    if (invResponse.status === 404) {
                        await createInventory(bookId);
                    } else {
                        throw new Error('Failed to update inventory');
                    }
                }
            } else {
                // Create new inventory
                await createInventory(bookId);
            }

            onClose(true); // Close and refresh
        } catch (err) {
            console.error(err);
            setError(err.message);
        } finally {
            setLoading(false);
        }
    };

    const createInventory = async (bookId) => {
        const invPayload = {
            bookId: bookId,
            physicalTotal: parseInt(formData.physicalTotal),
            physicalAvailable: parseInt(formData.physicalTotal), // Initial available = total
            digitalTotal: parseInt(formData.digitalTotal),
            digitalAvailable: parseInt(formData.digitalTotal)
        };

        const response = await fetch('/api/inventory', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify(invPayload)
        });

        if (!response.ok) {
            throw new Error('Failed to create inventory record');
        }
    };

    return (
        <div className="fixed inset-0 bg-black/80 backdrop-blur-sm flex items-center justify-center z-50 p-4">
            <div className="bg-slate-900 border border-slate-700 rounded-2xl w-full max-w-2xl shadow-2xl flex flex-col max-h-[90vh]">
                <div className="p-6 border-b border-slate-700 flex justify-between items-center">
                    <h2 className="text-xl font-bold text-white">
                        {book ? 'Edit Book & Inventory' : 'Add New Book'}
                    </h2>
                    <button onClick={() => onClose(false)} className="text-slate-400 hover:text-white transition-colors">
                        <X size={24} />
                    </button>
                </div>

                <div className="p-6 overflow-y-auto custom-scrollbar">
                    {error && (
                        <div className="bg-red-500/10 border border-red-500/50 text-red-400 p-4 rounded-xl mb-6">
                            {error}
                        </div>
                    )}

                    <form id="inventory-form" onSubmit={handleSubmit} className="space-y-6">
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div className="space-y-2">
                                <label className="text-sm font-medium text-slate-300">Title</label>
                                <input
                                    type="text"
                                    required
                                    className="input w-full"
                                    value={formData.title}
                                    onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                                />
                            </div>
                            <div className="space-y-2">
                                <label className="text-sm font-medium text-slate-300">Author</label>
                                <input
                                    type="text"
                                    required
                                    className="input w-full"
                                    value={formData.author}
                                    onChange={(e) => setFormData({ ...formData, author: e.target.value })}
                                />
                            </div>
                            <div className="space-y-2">
                                <label className="text-sm font-medium text-slate-300">ISBN</label>
                                <input
                                    type="text"
                                    required
                                    className="input w-full"
                                    value={formData.isbn}
                                    onChange={(e) => setFormData({ ...formData, isbn: e.target.value })}
                                />
                            </div>
                            <div className="space-y-2">
                                <label className="text-sm font-medium text-slate-300">Genre</label>
                                <input
                                    type="text"
                                    required
                                    className="input w-full"
                                    value={formData.genre}
                                    onChange={(e) => setFormData({ ...formData, genre: e.target.value })}
                                />
                            </div>
                        </div>

                        <div className="border-t border-slate-700/50 pt-6">
                            <h3 className="text-lg font-semibold text-white mb-4">Inventory Settings</h3>
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                                <div className="space-y-2">
                                    <label className="text-sm font-medium text-slate-300">Physical Total Copies</label>
                                    <input
                                        type="number"
                                        min="0"
                                        required
                                        className="input w-full"
                                        value={formData.physicalTotal}
                                        onChange={(e) => setFormData({ ...formData, physicalTotal: e.target.value })}
                                    />
                                    <p className="text-xs text-slate-500">Total physical copies owned by the library.</p>
                                </div>
                                <div className="space-y-2">
                                    <label className="text-sm font-medium text-slate-300">Digital Licenses</label>
                                    <input
                                        type="number"
                                        min="0"
                                        required
                                        className="input w-full"
                                        value={formData.digitalTotal}
                                        onChange={(e) => setFormData({ ...formData, digitalTotal: e.target.value })}
                                    />
                                    <p className="text-xs text-slate-500">Total concurrent digital licenses available.</p>
                                </div>
                            </div>
                        </div>
                    </form>
                </div>

                <div className="p-6 border-t border-slate-700 bg-slate-900/50 rounded-b-2xl flex justify-end gap-3">
                    <button
                        type="button"
                        onClick={() => onClose(false)}
                        className="px-4 py-2 text-slate-300 hover:text-white transition-colors"
                    >
                        Cancel
                    </button>
                    <button
                        type="submit"
                        form="inventory-form"
                        disabled={loading}
                        className="btn btn-primary flex items-center gap-2"
                    >
                        {loading ? (
                            <div className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                        ) : (
                            <Save size={18} />
                        )}
                        Save Changes
                    </button>
                </div>
            </div>
        </div>
    );
};

export default InventoryForm;
