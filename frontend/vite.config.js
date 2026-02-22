import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [react()],
    server: {
        proxy: {
            '/api/books': {
                target: 'http://localhost:5001',
                changeOrigin: true,
                secure: false,
            },
            '/api/inventory': {
                target: 'http://localhost:5002',
                changeOrigin: true,
                secure: false,
            },
            '/api/auth': {
                target: 'http://localhost:5003',
                changeOrigin: true,
                secure: false,
            },
            '/api/users': {
                target: 'http://localhost:5003',
                changeOrigin: true,
                secure: false,
            },
            '/api/roles': {
                target: 'http://localhost:5003',
                changeOrigin: true,
                secure: false,
            },
            '/api/borrowing': {
                target: 'http://localhost:5004',
                changeOrigin: true,
                secure: false,
            },
            '/api/latefee': {
                target: 'http://localhost:5004',
                changeOrigin: true,
                secure: false,
            }
        }
    }
})
