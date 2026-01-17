import vuetify from 'eslint-config-vuetify';

export default vuetify(
  {
    rules: {
      // Указываем стилистическому плагину всегда требовать точки с запятой
      '@stylistic/semi': ['error', 'always'],

      // На всякий случай явно настраиваем и базовое правило
      'semi': ['error', 'always'],

      // Отключаем правило, которое может ругаться на "лишние" точки
      '@stylistic/no-extra-semi': 'error',

      // Отключаем сортировку атрибутов тегов для vue-файлов
      'vue/attributes-order': 'off',

      // Отключаем пробелы между именем функции и круглыми скобками с аргументами
      '@stylistic/space-before-function-paren': ['error', 'never'],
    },
  },
);
