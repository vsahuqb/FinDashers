import React, { useState, useRef, useEffect } from 'react';
import { useSearch } from '../context/SearchContext';
import './SearchBar.css';

const SearchBar = ({ onSearch, placeholder = "Ask anything about your payments..." }) => {
  const [query, setQuery] = useState('');
  const [showSuggestions, setShowSuggestions] = useState(false);
  const [filteredSuggestions, setFilteredSuggestions] = useState([]);
  const { search, getSuggestions, loading, error } = useSearch();
  const inputRef = useRef(null);

  useEffect(() => {
    if (query.length > 0) {
      const suggestions = getSuggestions(query);
      setFilteredSuggestions(suggestions);
      setShowSuggestions(suggestions.length > 0);
    } else {
      setShowSuggestions(false);
    }
  }, [query, getSuggestions]);

  const handleSubmit = (e) => {
    e.preventDefault();
    if (query.trim() && !loading) {
      search(query.trim());
      onSearch?.(query.trim());
      setShowSuggestions(false);
    }
  };

  const handleSuggestionClick = (suggestion) => {
    if (!loading) {
      setQuery(suggestion);
      search(suggestion);
      onSearch?.(suggestion);
      setShowSuggestions(false);
      inputRef.current?.blur();
    }
  };

  const handleClear = () => {
    setQuery('');
    setShowSuggestions(false);
    inputRef.current?.focus();
  };

  return (
    <div className="search-bar-container">
      <form className={`search-bar ${loading ? 'loading' : ''}`} onSubmit={handleSubmit}>
        <div className="search-icon">{loading ? '⏳' : '🔍'}</div>
        <input
          ref={inputRef}
          type="text"
          className="search-input"
          placeholder={loading ? "Searching..." : placeholder}
          value={query}
          disabled={loading}
          onChange={(e) => setQuery(e.target.value)}
          onFocus={() => !loading && query.length > 0 && setShowSuggestions(filteredSuggestions.length > 0)}
          onBlur={() => setTimeout(() => setShowSuggestions(false), 200)}
        />
        {query && (
          <button type="button" className="clear-button" onClick={handleClear}>
            ×
          </button>
        )}
      </form>
      
      {showSuggestions && !loading && (
        <div className="suggestions-dropdown">
          {filteredSuggestions.map((suggestion, index) => (
            <div
              key={index}
              className="suggestion-item"
              onClick={() => handleSuggestionClick(suggestion)}
            >
              {suggestion}
            </div>
          ))}
        </div>
      )}
      
      {error && (
        <div className="search-error">
          ❌ {error}
        </div>
      )}
    </div>
  );
};

export default SearchBar;