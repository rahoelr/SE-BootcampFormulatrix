/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        'monopoly-brown': '#D97706',
        'monopoly-light-blue': '#7DD3FC',
        'monopoly-pink': '#EC4899',
        'monopoly-orange': '#F97316',
        'monopoly-red': '#DC2626',
        'monopoly-yellow': '#FACC15',
        'monopoly-green': '#16A34A',
        'monopoly-dark-blue': '#1E3A8A',
        'brutal-yellow': '#FACC15',
      },
      fontFamily: {
        'display': ['Space Grotesk', 'sans-serif'],
        'body': ['Inter', 'sans-serif'],
        'mono': ['JetBrains Mono', 'monospace'],
      },
      boxShadow: {
        'brutal-sm': '2px 2px 0px #000000',
        'brutal': '4px 4px 0px #000000',
        'brutal-lg': '4px 4px 0px #000000, 8px 8px 0px #000000',
        'brutal-tile': '2px 2px 0px #000000, 4px 4px 0px #000000, 6px 6px 0px #000000',
        'brutal-tile-lg': '3px 3px 0px #000000, 6px 6px 0px #000000, 9px 9px 0px #000000',
        'brutal-modal': '8px 8px 0px #000000, 16px 16px 0px #000000',
      },
      borderWidth: {
        '3': '3px',
      },
    },
  },
  plugins: [],
}
