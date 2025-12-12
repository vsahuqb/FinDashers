import React, { createContext, useContext, useReducer, useEffect } from 'react';
import nlSearchService from '../services/nlSearchService';

const SearchContext = createContext();

const initialState = {
  query: '',
  results: null,
  loading: false,
  error: null,
  history: [],
  cache: new Map(),
  suggestions: []
};

const searchReducer = (state, action) => {
  switch (action.type) {
    case 'SET_QUERY':
      return { ...state, query: action.payload };
    case 'SEARCH_START':
      return { ...state, loading: true, error: null };
    case 'SEARCH_SUCCESS':
      return { 
        ...state, 
        loading: false, 
        results: action.payload,
        history: [action.query, ...state.history.filter(h => h !== action.query)].slice(0, 10)
      };
    case 'SEARCH_ERROR':
      return { ...state, loading: false, error: action.payload, results: null };
    case 'CLEAR_RESULTS':
      return { ...state, results: null, error: null };
    case 'SET_SUGGESTIONS':
      return { ...state, suggestions: action.payload };
    case 'CLEAR_HISTORY':
      return { ...state, history: [] };
    default:
      return state;
  }
};

export const SearchProvider = ({ children }) => {
  const [state, dispatch] = useReducer(searchReducer, initialState);

  useEffect(() => {
    const savedHistory = localStorage.getItem('searchHistory');
    if (savedHistory) {
      dispatch({ type: 'SEARCH_SUCCESS', payload: null, query: '' });
      const history = JSON.parse(savedHistory);
      dispatch({ type: 'SET_SUGGESTIONS', payload: history });
    }
  }, []);

  useEffect(() => {
    localStorage.setItem('searchHistory', JSON.stringify(state.history));
  }, [state.history]);

  const search = async (query) => {
    console.log('Search function called with query:', query);
    if (!query.trim()) {
      console.log('Empty query, returning');
      return;
    }

    const cacheKey = query.toLowerCase().trim();
    
    // Check cache first
    if (state.cache.has(cacheKey)) {
      console.log('Using cached result for:', query);
      dispatch({ type: 'SEARCH_SUCCESS', payload: state.cache.get(cacheKey), query });
      return;
    }

    console.log('Starting new search for:', query);
    dispatch({ type: 'SEARCH_START' });
    
    try {
      // Use real NL2SQL API
      const results = await nlSearchService.searchQuery(query);
      
      if (results.error) {
        dispatch({ type: 'SEARCH_ERROR', payload: results.error });
      } else {
        // Cache the results
        state.cache.set(cacheKey, results);
        dispatch({ type: 'SEARCH_SUCCESS', payload: results, query });
      }
    } catch (error) {
      // Fallback to mock if API fails
      console.warn('API failed, using mock data:', error.message);
      try {
        const mockResults = await nlSearchService.mockSearch(query);
        dispatch({ type: 'SEARCH_SUCCESS', payload: mockResults, query });
      } catch (mockError) {
        dispatch({ type: 'SEARCH_ERROR', payload: error.message });
      }
    }
  };

  const setQuery = (query) => {
    dispatch({ type: 'SET_QUERY', payload: query });
  };

  const clearResults = () => {
    dispatch({ type: 'CLEAR_RESULTS' });
  };

  const clearHistory = () => {
    dispatch({ type: 'CLEAR_HISTORY' });
    localStorage.removeItem('searchHistory');
  };

  const getSuggestions = (input) => {
    if (!input) return state.history.slice(0, 5);
    
    const filtered = state.history.filter(item => 
      item.toLowerCase().includes(input.toLowerCase())
    );
    
    const defaultSuggestions = [
      'last 10 payments',
      'failed payments today',
      'success rate this week',
      'top payment methods',
      'average transaction amount',
      'payments by country'
    ];
    
    return [...filtered, ...defaultSuggestions.filter(s => 
      s.toLowerCase().includes(input.toLowerCase()) && !filtered.includes(s)
    )].slice(0, 5);
  };

  const value = {
    ...state,
    search,
    setQuery,
    clearResults,
    clearHistory,
    getSuggestions
  };

  return (
    <SearchContext.Provider value={value}>
      {children}
    </SearchContext.Provider>
  );
};

export const useSearch = () => {
  const context = useContext(SearchContext);
  if (!context) {
    throw new Error('useSearch must be used within SearchProvider');
  }
  return context;
};