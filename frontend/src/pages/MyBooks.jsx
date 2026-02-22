import { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { Clock, CheckCircle } from 'lucide-react';

const MyBooks = () => {
    const { user, token, parseJwt } = useAuth();
    const [borrowings, setBorrowings] = useState([]);
    const [books, setBooks] = useState({});
    const [loading, setLoading] = useState(true);
    const [totalUnpaid, setTotalUnpaid] = useState(0);

    useEffect(() => {
        if (user) {
            fetchBorrowings();
        }
    }, [user]);

    const fetchBorrowings = async () => {
        try {
            const decoded = parseJwt(token);
            const userId = decoded.sub;

            // Fetch borrowings
            const response = await fetch(`/api/borrowing/user/${userId}`, {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (response.ok) {
                const data = await response.json();
                setBorrowings(data);

                // Fetch book details for each borrowing
                const bookDetails = {};
                for (const borrowing of data) {
                    if (!books[borrowing.bookId]) {
                        const bookRes = await fetch(`/api/books/${borrowing.bookId}`);
                        if (bookRes.ok) {
                            bookDetails[borrowing.bookId] = await bookRes.json();
                        }
                    }
                }
                setBooks(prev => ({ ...prev, ...bookDetails }));
            }

            // Fetch fees
            const feesRes = await fetch(`/api/borrowing/fees/user/${userId}`, {
                headers: { 'Authorization': `Bearer ${token}` }
            });
            if (feesRes.ok) {
                const feesData = await feesRes.json();
                setTotalUnpaid(feesData.totalUnpaidAmount);
            }

        } catch (error) {
            console.error('Failed to fetch data', error);
        } finally {
            setLoading(false);
        }
    };

    const handleReturn = async (borrowingId) => {
        try {
            const response = await fetch(`/api/borrowing/${borrowingId}/return`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (response.ok) {
                // Refresh list
                fetchBorrowings();
                alert('Book returned successfully!');
            } else {
                alert('Failed to return book');
            }
        } catch (error) {
            console.error('Error returning book:', error);
        }
    };

    if (!user) return <div className="container py-8">Please login to view your books.</div>;

    return (
        <div className="container">
            <div className="flex justify-between items-center mb-8">
                <h1 className="text-3xl font-bold">My Borrowed Books</h1>
                {totalUnpaid > 0 && (
                    <div className="bg-red-500/20 text-red-400 px-4 py-2 rounded-lg border border-red-500/50 font-bold">
                        Unpaid Fees: ${totalUnpaid.toFixed(2)}
                    </div>
                )}
            </div>

            {loading ? (
                <div className="flex justify-center">Loading...</div>
            ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                    {borrowings.length === 0 ? (
                        <p className="text-slate-400 col-span-full text-center py-8">You haven't borrowed any books yet.</p>
                    ) : (
                        borrowings.map(borrowing => {
                            const book = books[borrowing.bookId];
                            const isOverdue = !borrowing.isReturned && new Date(borrowing.dueDate) < new Date();

                            return (
                                <div key={borrowing.id} className="card p-6 relative overflow-hidden">
                                    {isOverdue && (
                                        <div className="absolute top-0 right-0 bg-red-500 text-white text-xs font-bold px-3 py-1 rounded-bl-lg">
                                            OVERDUE
                                        </div>
                                    )}

                                    <h3 className="text-xl font-bold mb-2 text-white">{book ? book.title : 'Loading...'}</h3>
                                    <p className="text-slate-400 mb-4">{book ? book.author : '...'}</p>

                                    <div className="space-y-2 text-sm text-slate-300 mb-6">
                                        <div className="flex justify-between">
                                            <span>Borrowed:</span>
                                            <span>{new Date(borrowing.borrowedAt).toLocaleDateString()}</span>
                                        </div>
                                        <div className="flex justify-between">
                                            <span>Due Date:</span>
                                            <span className={isOverdue ? 'text-red-400 font-bold' : ''}>
                                                {new Date(borrowing.dueDate).toLocaleDateString()}
                                            </span>
                                        </div>
                                        <div className="flex justify-between">
                                            <span>Status:</span>
                                            <span className={borrowing.isReturned ? 'text-green-400' : 'text-blue-400'}>
                                                {borrowing.isReturned ? 'Returned' : 'Active Loan'}
                                            </span>
                                        </div>
                                    </div>

                                    {!borrowing.isReturned && (
                                        <button
                                            onClick={() => handleReturn(borrowing.id)}
                                            className="btn btn-primary w-full"
                                        >
                                            Return Book
                                        </button>
                                    )}
                                </div>
                            );
                        })
                    )}
                </div>
            )}
        </div>
    );
};

export default MyBooks;
