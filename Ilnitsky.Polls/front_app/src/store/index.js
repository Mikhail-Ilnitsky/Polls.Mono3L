import { createStore } from 'vuex';

import AnswersService from '@/services/answersService';
import PollsService from '@/services/pollsService';

export default createStore({
  state: {
    // Services:
    pollsService: new PollsService(),
    answersService: new AnswersService(),

    // User:
    userId: null,
    userName: null,

    users: [
      {
        id: '1',
        name: 'Mike',
        password: '1234',
        isAdmin: true,
      },
      {
        id: '2',
        name: 'July',
        password: '1111',
        isAdmin: false,
      },
      {
        id: '3',
        name: 'Alex',
        password: '3333',
        isAdmin: false,
      },
    ],

    polls: [],
    currentPoll: null,
  },

  getters: {
    isAuthorized: state => {
      return state.userId !== null;
    },
  },

  mutations: {
    loginUser(state, user) {
      state.userId = user.id;
      state.userName = user.name;
    },

    logoutUser(state) {
      state.userId = null;
      state.userName = null;
    },

    setPolls(state, newValue) {
      state.polls = newValue;
    },

    addPolls(state, addedPolls) {
      state.polls.push(...addedPolls);
    },

    setCurrentPoll(state, newValue) {
      state.currentPoll = newValue;
    },
  },

  actions: {
    loadPolls({ state, commit }, queryParams) {
      state.pollsService.getPolls(queryParams)
        .then(response => {
          commit('setPolls', response.data);
        });
    },

    loadMorePolls({ state, commit }, queryParams) {
      state.pollsService.getPolls(queryParams)
        .then(response => {
          if (response.data.length > 0) {
            commit('addPolls', response.data);
          }
        });
    },

    loadPollById({ state, commit }, pollId) {
      state.pollsService.getPollById(pollId)
        .then(response => {
          commit('setCurrentPoll', response.data);
        });
    },

    uploadAnswer({ state }, response) {
      state.answersService.postAnswer(response);
    },
  },
});
