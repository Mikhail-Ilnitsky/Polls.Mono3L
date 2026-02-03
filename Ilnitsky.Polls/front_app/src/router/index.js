/**
 * router/index.ts
 *
 * Automatic routes for `./src/pages/*.vue`
 */

// Composables
import { createRouter, createWebHistory } from 'vue-router';
// import { routes } from 'vue-router/auto-routes';

import AccountView from '@/views/AccountView.vue';
import LastPollsView from '@/views/LastPollsView.vue';
import LoginView from '@/views/LoginView.vue';
import PollView from '@/views/PollView.vue';

const routes = [
  {
    path: '/',
    name: 'LastPollsPage',
    component: LastPollsView,
    meta: {
      pageTitle: 'Опросы',
    },
  },
  {
    path: '/poll/:pollId',
    name: 'PollPage',
    component: PollView,
    meta: {
      pageTitle: 'Опросы',
    },
    props: route => ({
      pollId: String(route.params.pollId),
    }),
  },
  {
    path: '/login',
    name: 'LoginPage',
    component: LoginView,
    meta: {
      pageTitle: 'Вход в личный кабинет',
    },
  },
  {
    path: '/account',
    name: 'AccountPage',
    component: AccountView,
    meta: {
      pageTitle: 'Личный кабинет пользователя',
    },
  },
];

export function createRouterWithStore(store) {
  const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes,
  });

  router.beforeEach((to, from, next) => {
    if (!store.getters.isAuthorized && to.path.startsWith('/account')) {
      next('/login');
    } else if (store.getters.isAuthorized && to.path === '/login') {
      next('/account');
    } else if (to.path !== '/'
      && to.path !== '/login'
      && !to.path.startsWith('/account')
      && !to.path.startsWith('/poll')) {
      next('/');
    } else {
      next();
    }
  });

  // Workaround for https://github.com/vitejs/vite/issues/11804
  router.onError((err, to) => {
    if (err?.message?.includes?.('Failed to fetch dynamically imported module')) {
      if (localStorage.getItem('vuetify:dynamic-reload')) {
        console.error('Dynamic import error, reloading page did not fix it', err);
      } else {
        console.log('Reloading page to fix dynamic import error');
        localStorage.setItem('vuetify:dynamic-reload', 'true');
        location.assign(to.fullPath);
      }
    } else {
      console.error(err);
    }
  });

  router.isReady().then(() => {
    localStorage.removeItem('vuetify:dynamic-reload');
  });

  return router;
}
