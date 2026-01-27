import axios from 'axios';

export default class AnswersService {
  _url = '/api/v1/answers/';

  postAnswer(answer) {
    return axios.post(this._url, answer);
  }
}
