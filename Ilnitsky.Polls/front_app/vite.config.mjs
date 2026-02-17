import { fileURLToPath, URL } from 'node:url';
import Vue from '@vitejs/plugin-vue';
import Fonts from 'unplugin-fonts/vite';
import Components from 'unplugin-vue-components/vite';
import VueRouter from 'unplugin-vue-router/vite';
import { defineConfig } from 'vite';
import Vuetify, { transformAssetUrls } from 'vite-plugin-vuetify';

// https://vitejs.dev/config/
export default defineConfig({
  base: process.env.NODE_ENV === 'production' ? '/' : './',
  build: {
    outDir: '../wwwroot',
    assetsDir: 'assets',
    emptyOutDir: true,
    sourcemap: false, // Отключаем sourcemaps для production
    modulePreload: false, // Отключаем генерацию <link rel="modulepreload">
    cssCodeSplit: false, // Объединяем CSS, чтобы уменьшить количество линков
    rollupOptions: {
      output: {
        manualChunks: undefined,
        // Оптимизация имён файлов
        chunkFileNames: 'assets/[name]-[hash].js',
        entryFileNames: 'assets/[name]-[hash].js',
        assetFileNames: ({ fileName }) => {
          if (/\.(eot|ttf|woff|woff2)$/.test(fileName)) {
            return 'assets/fonts/[name]-[hash][extname]';
          }
          if (/\.css$/.test(fileName)) {
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
    {
      name: 'cleanup-index-html', // Удаляем <link rel="preload" ...> теги из конечного index.html
      enforce: 'post',
      transformIndexHtml: {
        order: 'post',
        handler(html) {
          return html.replace(/\s*<link rel="preload" as="font".*crossorigin="anonymous">\n/g, '');
        },
      },
    },
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
    port: 3003,
    proxy: {
      '/api': {
        target: 'https://localhost:44356',
        changeOrigin: true,
        secure: false,
      },
    },
  },
});
