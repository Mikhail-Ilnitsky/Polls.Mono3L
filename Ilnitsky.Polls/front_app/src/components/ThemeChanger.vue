<template>
  <div class="theme-changer">
    <v-card>
      <v-card-text class="pt-3 pb-0">
        <v-radio-group v-model="themeName">
          <v-radio
            v-for="(value, name) in themes"
            :key="value.name ?? name"
            :style="{ 'color': value.colors.primary, 'background-color': value.colors.background}"
            :label="value.name ?? name"
            :value="name"
            class="font-weight-bold"
          />
        </v-radio-group>
      </v-card-text>
    </v-card>
  </div>
</template>

<script>
  export default {
    computed: {
      themes() {
        return this.$vuetify.theme.themes;
      },
      themeName: {
        get() {
          return this.$vuetify.theme.global.name;
        },
        set(newValue) {
          this.$vuetify.theme.change(newValue);
        },
      },
    },
  };
</script>
