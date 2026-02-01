<template>
  <div class="poll-view">
    <CommonGrid>
      <PollCard
        v-if="currentPoll"
        :poll="currentPoll"
        @select-answer="saveAnswer"
      />
      <ErrorCard
        v-else
        title="Ошибка 404"
        message="Упс... извините, мы не смогли найти страницу"
      />
    </CommonGrid>
  </div>
</template>

<script>
  import { mapState } from 'vuex';

  export default {
    components: {},

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
      ...mapState(['currentPoll']),
    },

    mounted() {
      this.$store.dispatch('loadPollById', this.pollId);
    },

    methods: {
      saveAnswer(answer) {
        this.$store.dispatch('uploadAnswer', answer);
      },
    },
  };
</script>

<styles>

</styles>
