import React, { createContext, useContext, useReducer, useEffect } from 'react';

const PreferencesContext = createContext();

const defaultPreferences = {
  activeTab: 'overview',
  collapsedSections: {},
  widgetStates: {},
  filters: {
    timeRange: '24h',
    paymentMethod: 'all',
    status: 'all'
  },
  isFirstVisit: true
};

const preferencesReducer = (state, action) => {
  switch (action.type) {
    case 'SET_ACTIVE_TAB':
      return { ...state, activeTab: action.payload };
    case 'TOGGLE_SECTION':
      return {
        ...state,
        collapsedSections: {
          ...state.collapsedSections,
          [action.payload]: !state.collapsedSections[action.payload]
        }
      };
    case 'SET_WIDGET_STATE':
      return {
        ...state,
        widgetStates: {
          ...state.widgetStates,
          [action.payload.id]: action.payload.state
        }
      };
    case 'SET_FILTERS':
      return { ...state, filters: action.payload };
    case 'RESET_PREFERENCES':
      return { ...defaultPreferences, isFirstVisit: false };
    case 'MARK_VISITED':
      return { ...state, isFirstVisit: false };
    default:
      return state;
  }
};

export const PreferencesProvider = ({ children }) => {
  const [preferences, dispatch] = useReducer(preferencesReducer, defaultPreferences);

  useEffect(() => {
    const saved = localStorage.getItem('dashboardPreferences');
    if (saved) {
      const parsedPrefs = JSON.parse(saved);
      Object.keys(parsedPrefs).forEach(key => {
        if (key === 'activeTab') {
          dispatch({ type: 'SET_ACTIVE_TAB', payload: parsedPrefs[key] });
        } else if (key === 'collapsedSections') {
          Object.keys(parsedPrefs[key]).forEach(sectionId => {
            if (parsedPrefs[key][sectionId]) {
              dispatch({ type: 'TOGGLE_SECTION', payload: sectionId });
            }
          });
        } else if (key === 'widgetStates') {
          Object.keys(parsedPrefs[key]).forEach(widgetId => {
            dispatch({
              type: 'SET_WIDGET_STATE',
              payload: { id: widgetId, state: parsedPrefs[key][widgetId] }
            });
          });
        } else if (key === 'filters') {
          dispatch({ type: 'SET_FILTERS', payload: parsedPrefs[key] });
        } else if (key === 'isFirstVisit') {
          dispatch({ type: 'MARK_VISITED' });
        }
      });
    }
  }, []);

  useEffect(() => {
    localStorage.setItem('dashboardPreferences', JSON.stringify(preferences));
  }, [preferences]);

  const setActiveTab = (tab) => {
    dispatch({ type: 'SET_ACTIVE_TAB', payload: tab });
  };

  const toggleSection = (sectionId) => {
    dispatch({ type: 'TOGGLE_SECTION', payload: sectionId });
  };

  const setWidgetState = (widgetId, state) => {
    dispatch({ type: 'SET_WIDGET_STATE', payload: { id: widgetId, state } });
  };

  const setFilters = (filters) => {
    dispatch({ type: 'SET_FILTERS', payload: filters });
  };

  const resetPreferences = () => {
    dispatch({ type: 'RESET_PREFERENCES' });
  };

  const value = {
    preferences,
    setActiveTab,
    toggleSection,
    setWidgetState,
    setFilters,
    resetPreferences
  };

  return (
    <PreferencesContext.Provider value={value}>
      {children}
    </PreferencesContext.Provider>
  );
};

export const usePreferences = () => {
  const context = useContext(PreferencesContext);
  if (!context) {
    throw new Error('usePreferences must be used within PreferencesProvider');
  }
  return context;
};