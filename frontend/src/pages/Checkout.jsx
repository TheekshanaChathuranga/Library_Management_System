import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { Calendar, AlertCircle, CheckCircle, BookOpen, Clock } from 'lucide-react';

const Checkout = () => {
    const { bookId } = useParams();
    const navigate = useNavigate();
    const { user, token, parseJwt } = useAuth();

    const [book, setBook] = useState(null);
    const [inventory, setInventory] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [activeLoans, setActiveLoans] = useState(0);
    const [unpaidFees, setUnpaidFees] = useState(0);
    const [processing, setProcessing] = useState(false);
    const [borrowChannel, setBorrowChannel] = useState('Physical');

    useEffect(() => {
        if (!user) {
            navigate('/login');
            return;
        }
        fetchData();
    }, [bookId, user]);

    const fetchData = async () => {
        setLoading(true);
        try {
            const decoded = parseJwt(token);
            const userId = decoded.sub;

            // Helper function to handle fetch responses
            const handleResponse = async (res, name) => {
                const contentType = res.headers.get("content-type");
                if (!res.ok || (contentType && !contentType.includes("application/json"))) {
                    const text = await res.text();
                    console.error(`[${name}] Failed. Status: ${res.status}. Content-Type: ${contentType}. Body:`, text);
                    throw new Error(`${name} failed: ${res.status} ${res.statusText}. See console for details.`);
                }
                return res.json();
            };

            // 1. Fetch Book Details
            const bookRes = await fetch(`/api/books/${bookId}`);
            const bookData = await handleResponse(bookRes, "Book fetch");
            setBook(bookData);

            // 2. Fetch Inventory Details
            try {
                const invRes = await fetch(`/api/inventory/${bookId}`);
                if (invRes.ok) {
                    const invData = await invRes.json();
                    setInventory(invData);
                    // Default to Digital if Physical is out of stock
                    if (invData.physicalAvailable <= 0 && invData.digitalAvailable > 0) {
                        setBorrowChannel('Digital');
                    }
                }
            } catch (e) {
                console.warn("Failed to fetch inventory", e);
            }

            // 3. Fetch User Borrowings
            const borrowingsRes = await fetch(`/api/borrowing/user/${userId}`, {
                headers: { 'Authorization': `Bearer ${token}` }
            });
            // Borrowings might be empty or fail, but we should handle it gracefully
            if (borrowingsRes.ok) {
                const borrowings = await borrowingsRes.json();
                const active = borrowings.filter(b => !b.isReturned).length;
                setActiveLoans(active);
            } else {
                console.warn("Borrowings fetch failed", await borrowingsRes.text());
            }

            // 4. Fetch Unpaid Fees
            const feesRes = await fetch(`/api/borrowing/fees/user/${userId}`, {
                headers: { 'Authorization': `Bearer ${token}` }
            });
            if (feesRes.ok) {
                const feesData = await feesRes.json();
                setUnpaidFees(feesData.totalUnpaidAmount);
            } else {
                console.warn("Fees fetch failed", await feesRes.text());
            }

        } catch (err) {
            console.error("Fetch Data Error:", err);
            setError(err.message);
        } finally {
            setLoading(false);
        }
    };

    const handleConfirmBorrow = async () => {
        setProcessing(true);
        setError('');

        try {
            const decoded = parseJwt(token);
            const userId = decoded.sub;

            const response = await fetch('/api/borrowing', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({
                    userId: userId,
                    bookId: bookId,
                    channel: borrowChannel
                })
            });

            const contentType = response.headers.get("content-type");
            if (!response.ok || (contentType && !contentType.includes("application/json"))) {
                const text = await response.text();
                console.error("Borrow request failed. Body:", text);

                let errMessage = 'Failed to borrow book';
                try {
                    const errData = JSON.parse(text);
                    errMessage = errData.detail || errData.message || errData.title || errMessage;
                } catch (e) {
                    // If parsing fails, it's likely HTML. Use a generic message or snippet.
                    errMessage = `Error ${response.status}: Server returned non-JSON response. Check console.`;
                }
                throw new Error(errMessage);
            }

            // Success
            navigate('/my-books');

        } catch (err) {
            console.error("Confirm Borrow Error:", err);
            setError(err.message);
        } finally {
            setProcessing(false);
        }
    };

    if (loading) return <div className="flex justify-center py-12"><div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-500"></div></div>;
    if (error) return <div className="text-center py-12 text-red-400">{error}</div>;
    if (!book) return <div className="text-center py-12 text-red-400">Book not found</div>;

    const dueDate = new Date();
    dueDate.setDate(dueDate.getDate() + 14);

    const canBorrow = activeLoans < 5 && unpaidFees === 0;

    // Check stock for selected channel
    const isOutOfStock = inventory && (
        borrowChannel === 'Physical' ? inventory.physicalAvailable <= 0 : inventory.digitalAvailable <= 0
    );

    return (
        <div className="container max-w-2xl py-8">
            <h1 className="text-3xl font-bold mb-8 flex items-center gap-3">
                <BookOpen className="text-indigo-400" />
                Checkout
            </h1>

            <div className="bg-slate-800/50 rounded-2xl border border-slate-700/50 overflow-hidden shadow-xl">
                <div className="p-8">
                    <div className="flex flex-col md:flex-row gap-8 mb-8">
                        <div className="w-32 h-48 bg-slate-700 rounded-lg flex-shrink-0 flex items-center justify-center text-slate-500">
                            <BookOpen size={48} />
                        </div>

                        <div>
                            <h2 className="text-2xl font-bold text-white mb-2">{book.title}</h2>
                            <p className="text-lg text-slate-400 mb-4">{book.author}</p>
                            <div className="flex items-center gap-2 text-sm text-slate-500 mb-1">
                                <span className="px-2 py-1 bg-slate-700 rounded text-xs uppercase tracking-wider">{book.genre}</span>
                                <span>ISBN: {book.isbn}</span>
                            </div>
                        </div>
                    </div>

                    {/* Format Selection */}
                    <div className="mb-8">
                        <h3 className="text-lg font-semibold text-white mb-4">Select Format</h3>
                        <div className="grid grid-cols-2 gap-4">
                            <button
                                onClick={() => setBorrowChannel('Physical')}
                                className={`p-4 rounded-xl border flex flex-col items-center gap-2 transition-all ${borrowChannel === 'Physical'
                                        ? 'bg-indigo-600/20 border-indigo-500 text-white'
                                        : 'bg-slate-900/50 border-slate-700 text-slate-400 hover:bg-slate-800'
                                    }`}
                            >
                                <BookOpen size={24} />
                                <span className="font-medium">Physical Copy</span>
                                {inventory && (
                                    <span className={`text-xs ${inventory.physicalAvailable > 0 ? 'text-green-400' : 'text-red-400'}`}>
                                        {inventory.physicalAvailable > 0 ? `${inventory.physicalAvailable} Available` : 'Out of Stock'}
                                    </span>
                                )}
                            </button>

                            <button
                                onClick={() => setBorrowChannel('Digital')}
                                className={`p-4 rounded-xl border flex flex-col items-center gap-2 transition-all ${borrowChannel === 'Digital'
                                        ? 'bg-indigo-600/20 border-indigo-500 text-white'
                                        : 'bg-slate-900/50 border-slate-700 text-slate-400 hover:bg-slate-800'
                                    }`}
                            >
                                <div className="relative">
                                    <BookOpen size={24} />
                                    <div className="absolute -top-1 -right-1 w-2 h-2 bg-blue-400 rounded-full animate-pulse" />
                                </div>
                                <span className="font-medium">Digital Copy</span>
                                {inventory && (
                                    <span className={`text-xs ${inventory.digitalAvailable > 0 ? 'text-green-400' : 'text-red-400'}`}>
                                        {inventory.digitalAvailable > 0 ? `${inventory.digitalAvailable} Available` : 'Out of Stock'}
                                    </span>
                                )}
                            </button>
                        </div>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
                        <div className="bg-slate-900/50 p-4 rounded-xl border border-slate-700/50">
                            <div className="flex items-center gap-2 text-slate-400 mb-2">
                                <Calendar size={18} />
                                <span className="text-sm font-medium">Due Date</span>
                            </div>
                            <div className="text-xl font-bold text-white">
                                {dueDate.toLocaleDateString(undefined, { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })}
                            </div>
                            <p className="text-xs text-slate-500 mt-1">14 days loan period</p>
                        </div>

                        <div className="bg-slate-900/50 p-4 rounded-xl border border-slate-700/50">
                            <div className="flex items-center gap-2 text-slate-400 mb-2">
                                <Clock size={18} />
                                <span className="text-sm font-medium">Late Fee Policy</span>
                            </div>
                            <div className="text-xl font-bold text-white">$1.00 / day</div>
                            <p className="text-xs text-slate-500 mt-1">Charged for each day overdue</p>
                        </div>
                    </div>

                    <div className="border-t border-slate-700/50 pt-6 mb-6">
                        <h3 className="text-lg font-semibold text-white mb-4">Account Status</h3>

                        <div className="space-y-3">
                            <div className="flex justify-between items-center">
                                <span className="text-slate-400">Active Loans</span>
                                <span className={`font-mono font-bold ${activeLoans >= 5 ? 'text-red-400' : 'text-green-400'}`}>
                                    {activeLoans} / 5
                                </span>
                            </div>
                            <div className="flex justify-between items-center">
                                <span className="text-slate-400">Unpaid Fees</span>
                                <span className={`font-mono font-bold ${unpaidFees > 0 ? 'text-red-400' : 'text-green-400'}`}>
                                    ${unpaidFees.toFixed(2)}
                                </span>
                            </div>
                        </div>
                    </div>

                    {error && (
                        <div className="bg-red-500/10 border border-red-500/50 text-red-400 p-4 rounded-xl mb-6 flex items-start gap-3">
                            <AlertCircle className="flex-shrink-0 mt-0.5" size={20} />
                            <span>{error}</span>
                        </div>
                    )}

                    <div className="flex gap-4">
                        <button
                            onClick={() => navigate('/catalog')}
                            className="flex-1 py-3 px-4 rounded-xl font-medium text-slate-300 hover:text-white hover:bg-slate-700 transition-colors"
                        >
                            Cancel
                        </button>
                        <button
                            onClick={handleConfirmBorrow}
                            disabled={!canBorrow || processing || isOutOfStock}
                            className={`flex-1 py-3 px-4 rounded-xl font-bold text-white flex items-center justify-center gap-2 transition-all
                                ${!canBorrow || isOutOfStock
                                    ? 'bg-slate-700 cursor-not-allowed opacity-50'
                                    : 'bg-indigo-600 hover:bg-indigo-500 shadow-lg shadow-indigo-600/20'
                                }`}
                        >
                            {processing ? (
                                <div className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                            ) : (
                                <>
                                    <CheckCircle size={20} />
                                    Confirm Borrow
                                </>
                            )}
                        </button>
                    </div>

                    {!canBorrow && (
                        <p className="text-center text-sm text-red-400 mt-4">
                            {activeLoans >= 5
                                ? "You have reached the maximum limit of 5 active loans."
                                : "Please pay outstanding fees before borrowing."}
                        </p>
                    )}

                    {canBorrow && isOutOfStock && (
                        <p className="text-center text-sm text-red-400 mt-4">
                            Selected format is out of stock.
                        </p>
                    )}
                </div>
            </div>
        </div>
    );
};

export default Checkout;
