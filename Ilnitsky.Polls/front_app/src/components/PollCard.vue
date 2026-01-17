<template>
  <div class="poll-card">
    <template v-if="currentQuestion">
      <h3>{{ poll.name }}</h3>
      <div v-if="poll?.html" v-html="poll.html" />
      <v-card :title="currentQuestion?.question" variant="outlined">
        <v-card-text>
          <v-radio-group v-model="userAnswer">
            <v-radio v-for="answer in currentQuestion?.answers" :key="answer" :label="answer" :value="answer" />
            <v-radio v-if="currentQuestion?.allowCustomAnswer" :label="customAnswer" :value="customAnswer" />
          </v-radio-group>
          <v-text-field v-if="userAnswer === customAnswer" label="мой вариант" v-model.trim="userCustomAnswer" />
        </v-card-text>

        <v-card-actions class="mt-n8 mr-2 mb-2">
          <v-spacer />
          <v-btn
            :disabled="!isAnswer"
            color="primary"
            variant="elevated"
            @click="setAnswer"
            text="Выбрать"
          />
        </v-card-actions>
      </v-card>
    </template>

    <v-card v-else variant="outlined">
      <v-card-text>
        <h1>Спасибо за ответы!</h1>
      </v-card-text>

      <v-card-actions class="mt-0">
        <v-btn
          color="primary"
          class="w-100"
          variant="elevated"
          :to="{ name: 'LastPollsPage' }"
          text="Перейти к другим опросам"
        />
      </v-card-actions>
    </v-card>
  </div>
</template>

<script>
  export default {
    components: {},

    props: {
      poll: {
        typeof: Object,
        required: true,
      },
    },

    emits: ['select-answer', 'end-poll'],

    data() {
      return {
        currentQuestionNumber: 1,
        userAnswer: null,
        userCustomAnswer: null,
      };
    },

    computed: {
      customAnswer() {
        return 'другое';
      },
      currentQuestion() {
        return this.poll.questions.find(q => q.number === this.currentQuestionNumber);
      },
      isAnswer() {
        return (!this.currentQuestion.allowCustomAnswer && this.userAnswer)
          || (this.currentQuestion.allowCustomAnswer && this.userAnswer && this.userAnswer !== this.customAnswer)
          || (this.currentQuestion.allowCustomAnswer && this.userAnswer === this.customAnswer && this.userCustomAnswer);
      },
      resultAnswer() {
        if (!this.isAnswer) {
          return null;
        }

        return this.currentQuestion.allowCustomAnswer && this.userAnswer === this.customAnswer
          ? this.userCustomAnswer
          : this.userAnswer;
      },
      nextNumber() {
        if (this.currentQuestion.conditionAnswer === null) {
          return this.currentQuestion.ifNotConditionNextNumber;
        }
        if (this.currentQuestion.conditionAnswer === this.userAnswer) {
          return this.currentQuestion.ifConditionNextNumber;
        }
        return this.currentQuestion.ifNotConditionNextNumber;
      },
    },

    methods: {
      setAnswer() {
        if (!this.isAnswer) {
          return;
        }

        this.$emit('select-answer', this.resultAnswer);

        if (this.nextNumber === null) {
          this.$emit('end-poll');
          this.currentQuestionNumber = null;
        } else {
          this.currentQuestionNumber = this.nextNumber;
          this.userAnswer = null;
          this.userCustomAnswer = null;
        }
      },
    },
  };
</script>

<styles>

</styles>
