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
</template>

<script>
  import { mapGetters, mapState } from 'vuex';

  export default {
    name: 'App',

    components: {},

    data: () => ({}),

    computed: {
      ...mapGetters(['isAuthorized']),
      ...mapState(['userName']),

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
  };
</script>
