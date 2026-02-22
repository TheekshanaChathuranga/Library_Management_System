import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const Register = () => {
    const [formData, setFormData] = useState({
        email: '',
        password: '',
        firstName: '',
        lastName: '',
        role: 'Member'
    });
    const [error, setError] = useState('');
    const { register, user } = useAuth();
    const navigate = useNavigate();

    useEffect(() => {
        const canRegister = user?.roles?.some(role => role === 'Admin' || role === 'Librarian');
        if (!canRegister) {
            alert('Access Denied: Only administrators can register new members.');
            navigate('/');
        }
    }, [user, navigate]);

    const handleChange = (e) => {
        setFormData({
            ...formData,
            [e.target.name]: e.target.value
        });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');

        const result = await register(formData);
        if (result.success) {
            alert('Registration successful! Please login.');
            navigate('/login');
        } else {
            setError(result.message);
        }
    };

    return (
        <div className="container flex justify-center items-center" style={{ minHeight: '80vh' }}>
            <div className="card" style={{ width: '100%', maxWidth: '500px' }}>
                <h2 style={{ marginBottom: '1.5rem', textAlign: 'center' }}>Create Account</h2>

                {error && (
                    <div style={{
                        backgroundColor: 'rgba(239, 68, 68, 0.1)',
                        color: 'var(--color-danger)',
                        padding: '0.75rem',
                        borderRadius: 'var(--radius-md)',
                        marginBottom: '1rem',
                        fontSize: '0.875rem'
                    }}>
                        {error}
                    </div>
                )}

                <form onSubmit={handleSubmit} className="flex flex-col gap-4">
                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <label className="label">First Name</label>
                            <input
                                type="text"
                                name="firstName"
                                className="input"
                                value={formData.firstName}
                                onChange={handleChange}
                                required
                            />
                        </div>
                        <div>
                            <label className="label">Last Name</label>
                            <input
                                type="text"
                                name="lastName"
                                className="input"
                                value={formData.lastName}
                                onChange={handleChange}
                                required
                            />
                        </div>
                    </div>

                    <div>
                        <label className="label">Email</label>
                        <input
                            type="email"
                            name="email"
                            className="input"
                            value={formData.email}
                            onChange={handleChange}
                            required
                        />
                    </div>

                    <div>
                        <label className="label">Role</label>
                        <select
                            name="role"
                            className="input"
                            value={formData.role}
                            onChange={handleChange}
                        >
                            <option value="Member">Member</option>
                            <option value="Librarian">Librarian</option>
                            <option value="Admin">Admin</option>
                        </select>
                    </div>

                    <div>
                        <label className="label">Password</label>
                        <input
                            type="password"
                            name="password"
                            className="input"
                            value={formData.password}
                            onChange={handleChange}
                            required
                            minLength={6}
                        />
                    </div>

                    <button type="submit" className="btn btn-primary mt-4">
                        Register
                    </button>
                </form>

                <div style={{ marginTop: '1rem', textAlign: 'center', fontSize: '0.875rem', color: 'var(--color-text-muted)' }}>
                    Already have an account? <a href="/login">Login</a>
                </div>
            </div>
        </div>
    );
};

export default Register;
