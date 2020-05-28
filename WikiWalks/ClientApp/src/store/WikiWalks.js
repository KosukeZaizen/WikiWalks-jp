const requestType = 'REQUEST';
const receiveDatesType = 'RECEIVE_DATES';
const receiveTitlesType = 'RECEIVE_TITLES';
const receivePagesType = 'RECEIVE_PAGES';
const initialState = { dates: [], titles: [], pages: [], isLoading: false };

export const actionCreators = {
    requestAllDates: () => async (dispatch, getState) => {
        try {
            dispatch({ type: requestType });

            const url = `api/WikiWalks/getAllDates`;
            const response = await fetch(url);
            const dates = await response.json();
            //if (!dates || dates.length <= 0) window.location.href = `/not-found?p=${window.location.pathname}`;

            dispatch({ type: receiveDatesType, dates });
        } catch (e) {
            //window.location.href = `/not-found?p=${window.location.pathname}`;
        }
    },
    requestTitlesForTheDate: publishDate => async (dispatch, getState) => {
        try {
            dispatch({ type: requestType });

            const url = `api/WikiWalks/getTitlesForTheDay?publishDate=${publishDate}`;
            const response = await fetch(url);
            const titles = await response.json();
            //if (!titles || titles.length <= 0) window.location.href = `/not-found?p=${window.location.pathname}`;

            dispatch({ type: receiveTitlesType, titles });
        } catch (e) {
            //window.location.href = `/not-found?p=${window.location.pathname}`;
        }
    },
    requestPagesForTheTitle: titleId => async (dispatch, getState) => {
        try {
            const url = `api/WikiWalks/getPagesForTitle?titleId=${titleId}`;
            const response = await fetch(url);
            const pages = await response.json();
            //if (!pages || pages.length <= 0) window.location.href = `/not-found?p=${window.location.pathname}`;

            dispatch({ type: receivePagesType, pages });
        } catch (e) {
            //window.location.href = `/not-found?p=${window.location.pathname}`;
        }
    }
};

export const reducer = (state, action) => {
    state = state || initialState;

    if (action.type === requestType) {
        return {
            ...state,
            isLoading: true
        };
    }

    if (action.type === receiveDatesType) {
        return {
            ...state,
            dates: action.dates,
            isLoading: false
        };
    }

    if (action.type === receiveTitlesType) {
        return {
            ...state,
            titles: action.titles,
            isLoading: false
        };
    }

    if (action.type === receivePagesType) {
        return {
            ...state,
            pages: action.pages,
            isLoading: false
        };
    }

    return state;
};
