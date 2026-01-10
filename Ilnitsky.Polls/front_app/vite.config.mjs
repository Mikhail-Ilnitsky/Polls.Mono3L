// Plugins
import Components from 'unplugin-vue-components/vite'
import Vue from '@vitejs/plugin-vue'
import Vuetify, { transformAssetUrls } from 'vite-plugin-vuetify'
import Fonts from 'unplugin-fonts/vite'
import VueRouter from 'unplugin-vue-router/vite'

// Utilities
import { defineConfig } from 'vite'
import { fileURLToPath, URL } from 'node:url'

// https://vitejs.dev/config/
export default defineConfig({
    base: './', //process.env.NODE_ENV === 'production' ? './' : '/',
    build: {
        outDir: '../wwwroot',
        assetsDir: 'assets',
        emptyOutDir: true,
        rollupOptions: {
            output: {
                manualChunks: undefined,
                assetFileNames: 'assets/[name]-[hash][extname]',
                chunkFileNames: 'assets/[name]-[hash].js',
                entryFileNames: 'assets/[name]-[hash].js',
            }
        },
        // Для разработки с ASP.NET бэкендом
        server: {
            proxy: {
                '/api': {
                    target: 'http://localhost:44356', // порт ASP.NET
                    changeOrigin: true
                }
            }
        }
    },
    plugins: [
        VueRouter(),
        Vue({
            template: { transformAssetUrls },
        }),
        // https://github.com/vuetifyjs/vuetify-loader/tree/master/packages/vite-plugin#readme
        Vuetify({
            autoImport: true,
            styles: {
                configFile: 'src/styles/settings.scss',
            },
        }),
        Components(),
        Fonts({
            fontsource: {
                families: [
                    {
                        name: 'Roboto',
                        weights: [100, 300, 400, 500, 700, 900],
                        styles: ['normal', 'italic'],
                    },
                ],
            },
        }),
    ],
    optimizeDeps: {
        exclude: [
            'vuetify',
            'vue-router',
            'unplugin-vue-router/runtime',
            'unplugin-vue-router/data-loaders',
            'unplugin-vue-router/data-loaders/basic',
        ],
    },
    define: { 'process.env': {} },
    resolve: {
        alias: {
            '@': fileURLToPath(new URL('src', import.meta.url)),
        },
        extensions: [
            '.js',
            '.json',
            '.jsx',
            '.mjs',
            '.ts',
            '.tsx',
            '.vue',
        ],
    },
    server: {
        port: 3000,
    },
})
