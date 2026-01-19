<template>
  <div class="account-view">
    <CommonGrid>
      <v-card variant="outlined" class="text-primary">
        <v-expansion-panels variant="accordion" class="border border-sm border-primary">

          <v-expansion-panel value="1">
            <v-expansion-panel-title class="text-primary">
              Изменение темы
            </v-expansion-panel-title>
            <v-expansion-panel-text>
              <ThemeChanger />
            </v-expansion-panel-text>
          </v-expansion-panel>

          <v-expansion-panel value="2">
            <v-expansion-panel-title class="text-primary">
              Выход из аккаунта
            </v-expansion-panel-title>
            <v-expansion-panel-text>
              <v-btn color="primary" class="w-100" @click="logout">Выход</v-btn>
            </v-expansion-panel-text>
          </v-expansion-panel>

        </v-expansion-panels>
      </v-card>
    </CommonGrid>
  </div>
</template>

<script>
  export default {
    methods: {
      logout() {
        this.$store.commit('logoutUser');
        this.$router.push('/login');
      },
    },
  };
</script>

<styles>

</styles>
