import { createStore } from 'vuex';

import AnswersService from '@/services/answersService';
import PollsService from '@/services/pollsService';

function addToast(state, message, type) {
  let timeout;

  switch (type) {
    case 'error':
    case 'warning': {
      timeout = 9000;
      break;
    }
    case 'success': {
      timeout = 2000;
      break;
    }
    default: {
      return;
    }
  }

  state.toasts.push({
    message,
    type,
    timeout,
  });

  state.toasts = state.toasts.slice();
}

export default createStore({
  state: {
    // Services:
    pollsService: new PollsService(),
    answersService: new AnswersService(),

    // Infrastructure:
    loadingsCount: 0,
    toasts: [],

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

    // Polls:
    polls: [],
    currentPoll: null,
  },

  getters: {
    isAuthorized: state => state.userId !== null,

    isLoading: state => state.loadingsCount > 0,
  },

  mutations: {
    // Infrastructure:

    startLoading(state) {
      state.loadingsCount++;
    },

    stopLoading(state) {
      state.loadingsCount = Math.max(state.loadingsCount - 1, 0);
    },

    clearToasts(state) {
      state.toasts = [];
    },

    success(state, message) {
      addToast(state, message, 'success');
    },

    warning(state, message) {
      addToast(state, message, 'warning');
    },

    error(state, message) {
      addToast(state, message, 'error');
    },

    axiosError(state, error) {
      // console.log('JSON:', error.toJSON());
      // console.log('error:', error);
      // console.log('error?.response:', error?.response);
      // console.log('error?.response?.data:', error?.response?.data);

      const response = error?.response;
      const data = error?.response?.data;

      if (response && data && data.errors && typeof data.errors === 'object') {
        for (const key in data.errors) {
          for (const message of data.errors[key]) {
            addToast(state, `[${response.status}] ${message}`, 'error');
          }
        }
        return;
      }

      if (response && data && typeof data === 'string') {
        addToast(state, `[${response.status}] ${data}`, 'error');
        return;
      }

      if (response) {
        addToast(state, `[${response.status}] ${response.statusText}`, 'error');
        return;
      }

      addToast(state, error.message, 'error');
    },

    // User:

    loginUser(state, user) {
      state.userId = user.id;
      state.userName = user.name;
    },

    logoutUser(state) {
      state.userId = null;
      state.userName = null;
    },

    // Polls:

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
    // Polls:

    loadPolls({ state, commit }, queryParams) {
      commit('startLoading');
      state.pollsService.getPolls(queryParams)
        .then(response => {
          commit('setPolls', response.data);
        })
        .catch(error => commit('axiosError', error))
        .finally(() => commit('stopLoading'));
    },

    loadMorePolls({ state, commit }, queryParams) {
      commit('startLoading');
      state.pollsService.getPolls(queryParams)
        .then(response => {
          if (response.data.length > 0) {
            commit('addPolls', response.data);
          }
        })
        .catch(error => commit('axiosError', error))
        .finally(() => commit('stopLoading'));
    },

    loadPollById({ state, commit }, pollId) {
      commit('startLoading');
      state.pollsService.getPollById(pollId)
        .then(response => {
          commit('setCurrentPoll', response.data);
        })
        .catch(error => commit('axiosError', error))
        .finally(() => commit('stopLoading'));
    },

    // Answers:

    uploadAnswer({ state, commit }, response) {
      state.answersService.postAnswer(response)
        .then(response => commit('success', response.data))
        .catch(error => commit('axiosError', error));
    },
  },
});
