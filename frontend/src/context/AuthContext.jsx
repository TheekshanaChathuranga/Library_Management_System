import { createContext, useContext, useState, useEffect } from 'react';

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(null);
    const [token, setToken] = useState(localStorage.getItem('token'));
    const [loading, setLoading] = useState(true);

    // Helper to decode JWT
    const parseJwt = (token) => {
        try {
            return JSON.parse(atob(token.split('.')[1]));
        } catch (e) {
            return null;
        }
    };

    useEffect(() => {
        if (token) {
            const decoded = parseJwt(token);
            if (decoded) {
                // Extract role. It might be under a long URI key or just 'role'
                const roleKey = Object.keys(decoded).find(key => key.includes('role'));
                const roles = decoded[roleKey] ? (Array.isArray(decoded[roleKey]) ? decoded[roleKey] : [decoded[roleKey]]) : [];

                setUser({
                    ...decoded,
                    email: decoded.sub || decoded.email, // Fallback
                    roles: roles
                });
            } else {
                localStorage.removeItem('token');
                setToken(null);
            }
        }
        setLoading(false);
    }, [token]);

    const login = async (email, password) => {
        try {
            const response = await fetch('/api/auth/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email, password }),
            });

            if (!response.ok) {
                throw new Error('Login failed');
            }

            const data = await response.json();
            setToken(data.accessToken);
            localStorage.setItem('token', data.accessToken);

            // The useEffect will handle setting the user from the token
            return true;
        } catch (error) {
            console.error(error);
            return false;
        }
    };

    const logout = () => {
        setToken(null);
        setUser(null);
        localStorage.removeItem('token');
    };

    const register = async (userData) => {
        try {
            const headers = { 'Content-Type': 'application/json' };
            if (token) {
                headers['Authorization'] = `Bearer ${token}`;
            }

            const response = await fetch('/api/auth/register', {
                method: 'POST',
                headers: headers,
                body: JSON.stringify(userData),
            });

            if (!response.ok) {
                const errorData = await response.json().catch(() => ({}));
                throw new Error(errorData.message || 'Registration failed');
            }
            return { success: true };
        } catch (error) {
            console.error(error);
            return { success: false, message: error.message };
        }
    };

    return (
        <AuthContext.Provider value={{ user, token, login, logout, register, loading, parseJwt }}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => useContext(AuthContext);
