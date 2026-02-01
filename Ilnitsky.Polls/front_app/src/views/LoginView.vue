<template>
  <div class="login-view">
    <CommonGrid>
      <v-card title="Вход" variant="outlined" class="w-100 text-primary">
        <v-form class="pa-4 text-tabular">
          <v-text-field
            v-model="username"
            autocomplete="username"
            clearable
            label="Логин"
            prepend-icon="mdi-account"
          />
          <v-text-field
            v-model="password"
            autocomplete="new-password"
            clearable
            label="Пароль"
            :prepend-icon="showPassword ? 'mdi-eye' : 'mdi-eye-off'"
            :type="showPassword ? 'text' : 'password'"
            @click:prepend="showPassword = !showPassword"
          />
        </v-form>
        <v-card-actions class="mx-2 mb-2 mt-n6">
          <v-spacer />
          <v-btn
            color="secondary"
            text="Очистить"
            variant="elevated"
            @click="clear"
          />
          <v-btn
            class="ml-2"
            color="primary"
            :disabled="!isValid"
            text="Войти"
            variant="elevated"
            @click="login"
          />
        </v-card-actions>
      </v-card>
    </CommonGrid>
  </div>
</template>

<script>
  import { mapGetters, mapState } from 'vuex';

  export default {
    data() {
      return {
        username: null,
        password: null,
        showPassword: false,
      };
    },

    computed: {
      ...mapGetters(['isAuthorized']),
      ...mapState(['users']),

      isValid() {
        return this.username && this.password;
      },
    },

    methods: {
      clear() {
        this.username = null;
        this.password = null;
      },
      login() {
        if (!this.isValid) {
          return;
        }

        const user = this.users.find(u => u.name === this.username && u.password === this.password);

        if (user) {
          this.$store.commit('loginUser', user);
          this.$router.push('/account');
        }
      },
    },
  };
</script>
