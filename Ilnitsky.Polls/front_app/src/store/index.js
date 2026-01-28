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
  },

  actions: {
    loadPolls({ state, commit }, params) {
      state.pollsService.getPolls(params)
        .then(response => {
          commit('setPolls', response.data);
        });
    },

    uploadAnswer({ state }, response) {
      state.answersService.postAnswer(response);
    },
  },
});
