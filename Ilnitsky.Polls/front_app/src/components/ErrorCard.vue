<template>
  <div class="error-card">
    <v-card
      :title="title"
      variant="outlined"
      class="text-primary"
      :class="{ 'pt-3 px-3': !message }"
    >
      <v-card-text class="text-primary">
        {{ message }}
      </v-card-text>
    </v-card>
  </div>
</template>

<script>
  export default {
    props: {
      title: {
        type: String,
        required: false,
      },
      message: {
        type: String,
        required: false,
      },
    },
  };
</script>
