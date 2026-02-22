/** @type {import('tailwindcss').Config} */
export default {
    content: [
        "./index.html",
        "./src/**/*.{js,ts,jsx,tsx}",
    ],
    theme: {
        container: {
            center: true,
            padding: '1rem',
        },
        extend: {
            colors: {
                background: '#0f172a',
                surface: '#1e293b',
                primary: '#6366f1',
                'primary-hover': '#4f46e5',
                border: '#334155',
            }
        },
    },
    plugins: [],
}
