/**
 * plugins/index.js
 *
 * Automatically included in `./src/main.js`
 */

// Plugins
import { createRouterWithStore } from '@/router';
import store from '@/store';
import vuetify from './vuetify';

const router = createRouterWithStore(store);

export function registerPlugins(app) {
  app
    .use(store)
    .use(vuetify)
    .use(router);
}
