<template>
  <div class="poll-view">
    <CommonGrid>
      <PollCard :poll="poll" />
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
  };
</script>

<styles>

</styles>
