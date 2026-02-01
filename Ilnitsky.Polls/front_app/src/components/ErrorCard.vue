<template>
  <div class="error-card">
    <v-card :title="title" variant="outlined" class="text-primary">
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
        required: true,
      },
      message: {
        type: String,
        required: true,
      },
    },
  };
</script>
