<template>
  <div class="last-polls-view">
    <v-container>
      <v-row>
        <v-spacer />
        <v-col cols="12" sm="8" md="4" xl="2">
          <v-list>
            <v-list-item
              v-for="poll in polls"
              :key="poll.id"
            >
              <router-link color="red" :to="{ name: 'PollPage', params: { pollId: poll.pollId} }">{{ poll.name }}</router-link>
            </v-list-item>
          </v-list>
        </v-col>
        <v-spacer />
      </v-row>
    </v-container>
  </div>
</template>

<script>
  import { mapState } from 'vuex';

  export default {
    components: {},

    props: {},

    data () {
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
