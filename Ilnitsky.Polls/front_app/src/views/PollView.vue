<template>
  <div class="poll-view">
    <CommonGrid>
      <PollCard
        :poll="poll"
        @select-answer="saveAnswer"
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
      ...mapState(['polls']),

      poll() {
        return this.polls.find(p => p.pollId === this.pollId);
      },
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
