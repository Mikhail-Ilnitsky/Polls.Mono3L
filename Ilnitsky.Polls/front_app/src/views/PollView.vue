<template>
  <div class="poll-view">
    <CommonGrid>
      <PollCard
        v-if="currentPoll && !isLoading"
        :poll="currentPoll"
        @select-answer="saveAnswer"
      />
      <ErrorCard
        v-else-if="!isLoading"
        title="Упс... извините, мы не смогли найти страницу"
      />
    </CommonGrid>
  </div>
</template>

<script>
  import { mapGetters, mapState } from 'vuex';

  export default {
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
      ...mapGetters(['isLoading']),
      ...mapState(['currentPoll']),
    },

    mounted() {
      this.$store.commit('setCurrentPoll', null);
      this.$store.dispatch('loadPollById', this.pollId);
    },

    methods: {
      saveAnswer(answer) {
        this.$store.dispatch('uploadAnswer', answer);
      },
    },
  };
</script>
