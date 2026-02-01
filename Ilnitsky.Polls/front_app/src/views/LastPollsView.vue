<template>
  <div class="last-polls-view">
    <CommonGrid>
      <v-card title="Последние опросы" variant="outlined" class="text-primary">
        <v-list class="mt-n2">
          <v-list-item
            v-for="poll in polls"
            :key="poll.pollId"
          >
            <v-icon class="mr-2 text-primary">mdi-help-circle-outline</v-icon>
            <router-link :to="{ name: 'PollPage', params: { pollId: poll.pollId} }">
              {{ poll.name }} ({{ poll.questionsCount }} {{ questionWord(poll.questionsCount) }})
            </router-link>
          </v-list-item>
        </v-list>
        <v-card-actions class="mt-n2">
          <v-btn
            class="w-100"
            color="primary"
            variant="elevated"
            @click="loadMore"
            text="Загрузить ещё"
          />
        </v-card-actions>
      </v-card>
    </CommonGrid>
  </div>
</template>

<script>
  import { mapState } from 'vuex';

  export default {
    data() {
      return {
        pageSize: 5,
      };
    },

    computed: {
      ...mapState(['polls']),
    },

    created() {
      if (!this.polls || this.polls.length === 0) {
        this.$store.dispatch('loadPolls', { offset: 0, limit: this.pageSize });
      }
    },

    methods: {
      loadMore() {
        const params = {
          offset: this.polls.length,
          limit: this.pageSize,
        };

        this.$store.dispatch('loadMorePolls', params);
      },
      questionWord(questionsCount) {
        const twoLastDigits = questionsCount % 100;
        const lastDigit = questionsCount % 10;

        if (twoLastDigits >= 5 && twoLastDigits <= 20) {
          return 'вопросов';
        }
        if (lastDigit >= 2 && twoLastDigits <= 4) {
          return 'вопроса';
        }
        if (lastDigit === 1) {
          return 'вопрос';
        }
        return 'вопросов';
      },
    },
  };
</script>
