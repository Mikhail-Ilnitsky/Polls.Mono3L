<template>
  <div class="poll-view">
    <v-container>
      <v-row>
        <v-spacer />
        <v-col cols="12" sm="8" md="4" xl="2">
          <PollCard :poll="poll" />
        </v-col>
        <v-spacer />
      </v-row>
    </v-container>
  </div>
</template>

<script>
  import { mapState } from 'vuex';
  import PollCard from '@/components/PollCard.vue';

  export default {
    components: {
      PollCard,
    },

    props: {
      pollId: {
        typeof: String,
        required: true,
      },
    },

    data() {
      return {};
    },

    computed: {
      ...mapState(['polls']),

      poll() {
        return this.polls.find(p => p.pollId === this.pollId);
      },
    },
  };
</script>

<styles>

</styles>
