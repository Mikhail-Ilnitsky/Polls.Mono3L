<template>
  <div class="poll-view">
    <v-container>
      <v-row>
        <v-col cols="12">
          Html с описанием опроса
        </v-col>
      </v-row>
      <v-row>
        <v-col>
          <v-card>
            <h1>pollId: {{ pollId }}</h1>
          </v-card>
        </v-col>
      </v-row>
    </v-container>
  </div>
</template>

<script>
  export default {
    components: {},

    props: {
      pollId: {
        typeof: String,
        required: true,
      },
    },

    data () {
      return {};
    },

    computed: {},
  };
</script>

<styles>

</styles>
