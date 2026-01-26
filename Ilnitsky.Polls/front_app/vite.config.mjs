import { fileURLToPath, URL } from 'node:url';
import Vue from '@vitejs/plugin-vue';
import Fonts from 'unplugin-fonts/vite';
import Components from 'unplugin-vue-components/vite';
import VueRouter from 'unplugin-vue-router/vite';
import { defineConfig } from 'vite';
import Vuetify, { transformAssetUrls } from 'vite-plugin-vuetify';

// https://vitejs.dev/config/
export default defineConfig({
  base: './', // process.env.NODE_ENV === 'production' ? './' : '/',
  build: {
    outDir: '../wwwroot',
    assetsDir: 'assets',
    emptyOutDir: true,
    sourcemap: false, // Отключаем sourcemaps для production
    cssCodeSplit: true,
    rollupOptions: {
      output: {
        manualChunks: undefined,
        // Оптимизация имён файлов
        chunkFileNames: 'assets/[name]-[hash].js',
        entryFileNames: 'assets/[name]-[hash].js',
        assetFileNames: ({ name }) => {
          if (/\.(eot|ttf|woff|woff2)$/.test(name)) {
            return 'assets/fonts/[name]-[hash][extname]';
          }
          if (/\.css$/.test(name)) {
            return 'assets/css/[name]-[hash][extname]';
          }
          return 'assets/[name]-[hash][extname]';
        },
      },
    },
    // Для разработки с ASP.NET бэкендом
    server: {
      proxy: {
        '/api': {
          target: 'http://localhost:44356', // порт ASP.NET
          changeOrigin: true,
        },
      },
    },
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
    proxy: {
      '/api': {
        target: 'https://localhost:44356',
        changeOrigin: true,
        secure: false,
      },
    },
  },
});
