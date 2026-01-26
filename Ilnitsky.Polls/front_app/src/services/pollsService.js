import axios from 'axios';

export default class PollsService {
  _url = '/api/v1/polls/';

  getPolls(params) {
    return axios.get(this._url, { params });
  }

  getPoll(pollId) {
    return axios.get(this._url + pollId);
  }

  postPoll(poll) {
    return axios.post(this._url, poll);
  }

  putPoll(pollId, poll) {
    return axios.post(this._url + pollId, poll);
  }

  deletePoll(pollId) {
    return axios.delete(this._url + pollId);
  }
}
