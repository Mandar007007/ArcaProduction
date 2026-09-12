import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    port: 3000,
    watch: {
      usePolling: true,
    },
    proxy: {
      '/api': {
        // In Docker, localhost is this container. Compose sets DLP_API_PROXY=http://server:8000.
        target: process.env.DLP_API_PROXY || 'http://localhost:8000',
        changeOrigin: true,
      },
    },
  },
})
