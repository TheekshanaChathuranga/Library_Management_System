import { Book, CheckCircle, XCircle } from 'lucide-react';

const BookCard = ({ book, onBorrow }) => {
    return (
        <div className="card flex flex-col h-full">
            <div className="flex items-start justify-between mb-4">
                <div className="p-3 rounded-lg bg-indigo-500/10 text-indigo-400">
                    <Book size={24} />
                </div>
                {book.isAvailable ? (
                    <span className="flex items-center gap-1 text-xs font-medium text-green-400 bg-green-400/10 px-2 py-1 rounded-full">
                        <CheckCircle size={12} /> Available
                    </span>
                ) : (
                    <span className="flex items-center gap-1 text-xs font-medium text-red-400 bg-red-400/10 px-2 py-1 rounded-full">
                        <XCircle size={12} /> Unavailable
                    </span>
                )}
            </div>

            <h3 className="text-lg font-bold mb-1 line-clamp-2">{book.title}</h3>
            <p className="text-slate-400 text-sm mb-4">{book.author}</p>

            <div className="mt-auto pt-4 border-t border-slate-700/50 flex items-center justify-between">
                <div className="flex flex-col gap-1">
                    <span className="text-xs text-slate-500 bg-slate-800 px-2 py-1 rounded w-fit">
                        {book.genre}
                    </span>
                    {book.inventory && (
                        <div className="flex gap-2 text-[10px] font-mono text-slate-400 mt-1">
                            <span title="Physical Copies">P: {book.inventory.physicalAvailable}</span>
                            <span className="text-slate-600">|</span>
                            <span title="Digital Copies">D: {book.inventory.digitalAvailable}</span>
                        </div>
                    )}
                </div>

                {book.isAvailable && (
                    <button
                        onClick={() => onBorrow(book)}
                        className="btn btn-primary"
                        style={{ padding: '0.25rem 0.75rem', fontSize: '0.875rem' }}
                    >
                        Borrow
                    </button>
                )}
            </div>
        </div>
    );
};

export default BookCard;
