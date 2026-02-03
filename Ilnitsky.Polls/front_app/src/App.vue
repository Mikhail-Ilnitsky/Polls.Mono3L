<template>
  <v-app>
    <v-navigation-drawer
      class="pt-4 border-e-lg border-primary"
      width="160"
      permanent
      :rail="isSmall"
    >
      <v-list-item
        color="primary"
        exact
        link
        :to="{ name: 'LastPollsPage' }"
      >
        <template #prepend>
          <v-icon>mdi-script-text-outline</v-icon>
        </template>

        <v-list-item-title>Опросы</v-list-item-title>
      </v-list-item>

      <v-list-item
        v-if="isPollPage"
        color="primary"
        active
        exact
        link
      >
        <template #prepend>
          <v-icon>mdi-format-list-checks</v-icon>
        </template>

        <v-list-item-title>Опрос</v-list-item-title>
      </v-list-item>

      <template v-if="!isAuthorized">
        <v-list-item
          color="primary"
          exact
          link
          :to="{ name: 'LoginPage' }"
        >
          <template #prepend>
            <v-icon>mdi-login-variant</v-icon>
          </template>

          <v-list-item-title>Вход</v-list-item-title>
        </v-list-item>
      </template>
      <template v-else>
        <v-list-item
          color="primary"
          exact
          link
          :to="{ name: 'AccountPage' }"
        >
          <template #prepend>
            <v-icon>mdi-account-box-outline</v-icon>
          </template>

          <v-list-item-title>Кабинет</v-list-item-title>
        </v-list-item>
      </template>

    </v-navigation-drawer>

    <v-main>
      <router-view />
    </v-main>
  </v-app>

  <v-overlay
    v-model="isLoading"
    class="align-center justify-center"
  >
    <v-progress-circular
      indeterminate
      color="primary"
      size="128"
      width="12"
    />
  </v-overlay>

</template>

<script>
  import { toast } from 'vue3-toastify';
  import { mapGetters, mapState } from 'vuex';

  import 'vue3-toastify/dist/index.css';

  export default {
    name: 'App',

    data: () => ({}),

    computed: {
      ...mapGetters(['isAuthorized', 'isLoading']),
      ...mapState(['toasts', 'userName']),

      gridName() {
        return this.$vuetify.display.name;
      },

      isXS() {
        return this.gridName === 'xs';
      },

      isSM() {
        return this.gridName === 'sm';
      },

      isSmall() {
        return this.isXS || this.isSM;
      },

      pageName() {
        return this.$route.name;
      },

      isPollPage() {
        return this.pageName === 'PollPage';
      },
    },

    watch: {
      toasts(newToasts) {
        if (newToasts && newToasts.length > 0) {
          for (const data of newToasts) {
            toast(
              data.message,
              {
                theme: 'colored',
                type: data.type,
                autoClose: data.timeout,
                position: toast.POSITION.BOTTOM_RIGHT,
              });
          }

          this.$store.commit('clearToasts');
        }
      },
    },
  };
</script>
