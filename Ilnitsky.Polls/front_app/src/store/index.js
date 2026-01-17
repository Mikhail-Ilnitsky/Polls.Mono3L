import { createStore } from 'vuex';

export default createStore({
  state: {
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

    polls: [
      {
        name: 'Как вы варите картошку',
        pollId: '1111',
        questions: [
          {
            questionId: '111111',
            question: 'Как вы варите картошку?',
            answers: ['В кожуре', 'Почищенную', 'Почищенную и порезанную'],
            allowCustomAnswer: true,
            number: 1,
            conditionAnswer: null,
            ifConditionNextNumber: null,
            ifNotConditionNextNumber: null,
          },
        ],
      },
      {
        name: 'Чем вы заправляете оливье',
        pollId: '2222',
        questions: [
          {
            questionId: '111222',
            question: 'Чем вы заправляете оливье?',
            answers: ['Сметаной', 'Майонезом', 'Оливковым маслом'],
            allowCustomAnswer: true,
            number: 1,
            conditionAnswer: null,
            ifConditionNextNumber: null,
            ifNotConditionNextNumber: null,
          },
        ],
      },
      {
        name: 'Как ваши дети питаются в школе',
        pollId: '3333',
        questions: [
          {
            questionId: '111333',
            question: 'Как ваши дети питаются в школе?',
            answers: ['Платим за столовую в Кенгу.ру', 'Покупает снеки в соседнем магазине', 'Приносит еду из дома'],
            allowCustomAnswer: true,
            number: 1,
            conditionAnswer: null,
            ifConditionNextNumber: null,
            ifNotConditionNextNumber: null,
          },
        ],
      },
      {
        name: 'За какую футбольную команду вы болеете',
        pollId: '4444',
        questions: [
          {
            questionId: '111444',
            question: 'За какую футбольную команду вы болеете?',
            answers: ['Реал Мадрид', 'Барселона', 'Манчестер Юнайтед', 'Ливерпуль', 'Бавария', 'Манчестер Сити', 'ПСЖ'],
            allowCustomAnswer: true,
            number: 1,
            conditionAnswer: null,
            ifConditionNextNumber: null,
            ifNotConditionNextNumber: null,
          },
        ],
      },
      {
        name: 'Брак и семья',
        pollId: '5555',
        questions: [
          {
            questionId: '112211',
            question: 'Сколько вам лет?',
            answers: ['младше 16', 'от 16 до 17', 'от 18 до 19', 'от 20 до 21', 'от 22 до 24', 'от 25 до 30', 'от 31 до 35', 'от 36 до 40', 'после 40', 'от 41 до 45', 'от 46 до 50', 'от 51 до 60', 'от 61 до 70', 'от 71 до 80', '81 и больше'],
            allowCustomAnswer: false,
            number: 1,
            conditionAnswer: null,
            ifConditionNextNumber: null,
            ifNotConditionNextNumber: 2,
          },
          {
            questionId: '112222',
            question: 'Ваш пол?',
            answers: ['мужчина', 'женщина'],
            allowCustomAnswer: false,
            number: 2,
            conditionAnswer: null,
            ifConditionNextNumber: null,
            ifNotConditionNextNumber: 3,
          },
          {
            questionId: '112233',
            question: 'В каком возрасте вы вступили в брак (в первый раз)?',
            answers: ['младше 16', 'от 16 до 17', 'от 18 до 19', 'от 20 до 21', 'от 22 до 24', 'от 25 до 30', 'от 31 до 35', 'от 36 до 40', 'после 40', 'никогда не был(а) в браке'],
            allowCustomAnswer: false,
            number: 3,
            conditionAnswer: 'никогда не был(а) в браке',
            ifConditionNextNumber: 7,
            ifNotConditionNextNumber: 4,
          },
          {
            questionId: '112244',
            question: 'Вы развелись?',
            answers: ['да, в итоге развелись', 'нет, не разводились'],
            allowCustomAnswer: false,
            number: 4,
            conditionAnswer: 'да, в итоге развелись',
            ifConditionNextNumber: 5,
            ifNotConditionNextNumber: 7,
          },
          {
            questionId: '112255',
            question: 'Сколько раз вы были в браке?',
            answers: ['1', '2', '3', '4', '5', '6', 'больше 6'],
            allowCustomAnswer: false,
            number: 5,
            conditionAnswer: '1',
            ifConditionNextNumber: 7,
            ifNotConditionNextNumber: 6,
          },
          {
            questionId: '112266',
            question: 'Сейчас вы состоите в браке?',
            answers: ['да', 'нет'],
            allowCustomAnswer: false,
            number: 6,
            conditionAnswer: null,
            ifConditionNextNumber: null,
            ifNotConditionNextNumber: 7,
          },
          {
            questionId: '112277',
            question: 'У вас есть родные дети?',
            answers: ['да', 'нет'],
            allowCustomAnswer: false,
            number: 7,
            conditionAnswer: 'да',
            ifConditionNextNumber: 8,
            ifNotConditionNextNumber: 10,
          },
          {
            questionId: '112288',
            question: 'Сколько у вас родных детей?',
            answers: ['1', '2', '3', '4', '5', '6', 'больше 6'],
            allowCustomAnswer: false,
            number: 8,
            conditionAnswer: null,
            ifConditionNextNumber: null,
            ifNotConditionNextNumber: 9,
          },
          {
            questionId: '112299',
            question: 'В каком возрасте у вас родился первый ребёнок?',
            answers: ['младше 16', 'от 16 до 17', 'от 18 до 19', 'от 20 до 21', 'от 22 до 24', 'от 25 до 30', 'от 31 до 35', 'от 36 до 40', 'после 40', 'у меня нет детей'],
            allowCustomAnswer: false,
            number: 9,
            conditionAnswer: null,
            ifConditionNextNumber: null,
            ifNotConditionNextNumber: 10,
          },
          {
            questionId: '113311',
            question: 'У вас есть приёмные дети?',
            answers: ['да', 'нет'],
            allowCustomAnswer: false,
            number: 10,
            conditionAnswer: 'да',
            ifConditionNextNumber: 11,
            ifNotConditionNextNumber: null,
          },
          {
            questionId: '113322',
            question: 'Сколько у вас приёмных детей?',
            answers: ['1', '2', '3', '4', '5', '6', 'больше 6'],
            allowCustomAnswer: false,
            number: 11,
            conditionAnswer: null,
            ifConditionNextNumber: null,
            ifNotConditionNextNumber: null,
          },
        ],
      },
    ],
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
  },

  actions: {},
});
