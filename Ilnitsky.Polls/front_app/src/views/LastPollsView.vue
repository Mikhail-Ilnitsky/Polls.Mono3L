<template>
  <div class="last-polls-view">
    <CommonGrid>
      <v-card title="Последние опросы" variant="outlined">
        <v-list class="mt-n2">
          <v-list-item
            v-for="poll in polls"
            :key="poll.id"
          >
            <router-link color="red" :to="{ name: 'PollPage', params: { pollId: poll.pollId} }">{{ poll.name }}</router-link>
          </v-list-item>
        </v-list>
      </v-card>
    </CommonGrid>
  </div>
</template>

<script>
  import { mapState } from 'vuex';

  export default {
    components: {},

    props: {},

    data() {
      return {};
    },

    computed: {
      ...mapState(['polls']),
    },

    methods: {},

  };
</script>

<style lang="scss">
</style>
