/**
 * plugins/vuetify.js
 *
 * Framework documentation: https://vuetifyjs.com`
 */

// Composables
import { createVuetify } from 'vuetify';
import colors from 'vuetify/util/colors';

// Styles
import '@mdi/font/css/materialdesignicons.css';
import 'vuetify/styles';

const lightVariables = {
  'border-color': '#000',
  'border-opacity': 0.12,
  'high-emphasis-opacity': 0.87,
  'medium-emphasis-opacity': 0.6,
  'disabled-opacity': 0.38,
  'idle-opacity': 0.04,
  'hover-opacity': 0.04,
  'focus-opacity': 0.12,
  'selected-opacity': 0.08,
  'activated-opacity': 0.12,
  'pressed-opacity': 0.12,
  'dragged-opacity': 0.08,
  'theme-kbd': '#212529',
  'theme-on-kbd': '#FFF',
  'theme-code': '#F5F5F5',
  'theme-on-code': '#000',
};

const darkVariables = {
  'border-color': '#FFF',
  'border-opacity': 0.12,
  'high-emphasis-opacity': 1,
  'medium-emphasis-opacity': 0.7,
  'disabled-opacity': 0.5,
  'idle-opacity': 0.1,
  'hover-opacity': 0.04,
  'focus-opacity': 0.12,
  'selected-opacity': 0.08,
  'activated-opacity': 0.12,
  'pressed-opacity': 0.16,
  'dragged-opacity': 0.08,
  'theme-kbd': '#212529',
  'theme-on-kbd': '#FFF',
  'theme-code': '#343434',
  'theme-on-code': '#CCC',
};

const light = {
  name: 'Синяя (светлая)',
  dark: false,
  colors: {
    'background': '#FFF',
    'surface': '#FFF',
    'surface-bright': '#FFF',
    'surface-variant': '#424242',
    'on-surface-variant': '#EEE',

    'primary': colors.blue.darken2,
    'primary-darken-1': colors.blue.darken4,
    'secondary': colors.grey.darken3,
    'secondary-darken-1': colors.grey.darken4,

    'lonely': colors.grey.darken1,
    'tabular': '#111',

    'error': '#B00020',
    'info': colors.blue.base,
    'success': colors.green.base,
    'warning': colors.orange.darken1,
  },
  variables: lightVariables,
};

const dark = {
  name: 'Синяя (тёмная)',
  dark: true,
  colors: {
    'background': '#121212',
    'surface': '#121212',
    'surface-bright': '#ccbfd6',
    'surface-variant': '#a3a3a3',
    'on-surface-variant': colors.grey.darken3,

    'primary': colors.blue.base,
    'primary-darken-1': '#277CC1',
    'secondary': '#54B6B2',
    'secondary-darken-1': '#48A9A6',

    'lonely': colors.grey.darken1,
    'tabular': '#FFF',

    'error': '#CF6679',
    'info': colors.blue.base,
    'success': colors.green.base,
    'warning': colors.orange.darken1,
  },
  variables: darkVariables,
};

const orange = {
  name: 'Оранжевая (светлая)',
  dark: false,
  colors: {
    'background': '#FFF',
    'surface': '#FFF',
    'surface-bright': '#FFF',
    'surface-variant': '#424242',
    'on-surface-variant': '#EEE',

    'primary': colors.deepOrange.base,
    'primary-darken-1': colors.deepOrange.darken1,
    'secondary': colors.green.base,
    'secondary-darken-1': colors.green.darken1,

    'lonely': colors.grey.darken1,
    'tabular': '#111',

    'error': '#B00020',
    'info': colors.blue.base,
    'success': colors.green.base,
    'warning': colors.orange.darken1,
  },
  variables: lightVariables,
};

const darkPink = {
  name: 'Розовая (тёмная)',
  dark: true,
  colors: {
    'background': '#121212',
    'surface': '#121212',
    'surface-bright': '#ccbfd6',
    'surface-variant': '#a3a3a3',
    'on-surface-variant': colors.grey.darken3,

    'primary': colors.pink.accent2,
    'primary-darken-1': colors.pink.base,
    'secondary': '#FFF',
    'secondary-darken-1': '#EEE',

    'lonely': colors.grey.darken1,
    'tabular': '#DDD',

    'error': '#CF6679',
    'info': colors.blue.base,
    'success': colors.green.accent4,
    'warning': colors.orange.darken1,
  },
  variables: darkVariables,
};

// https://vuetifyjs.com/en/introduction/why-vuetify/#feature-guides
export default createVuetify({
  theme: {
    defaultTheme: 'orange',
    themes: {
      orange,
      darkPink,
      light,
      dark,
    },
  },
});
