import React, { useState } from 'react';
import { useSearch } from '../context/SearchContext';
import './SearchHistory.css';

const SearchHistory = ({ isOpen, onClose, onSelectQuery }) => {
  const { history, clearHistory } = useSearch();
  const [favorites, setFavorites] = useState(() => {
    const saved = localStorage.getItem('favoriteQueries');
    return saved ? JSON.parse(saved) : [];
  });

  const addToFavorites = (query) => {
    if (!favorites.includes(query)) {
      const newFavorites = [...favorites, query];
      setFavorites(newFavorites);
      localStorage.setItem('favoriteQueries', JSON.stringify(newFavorites));
    }
  };

  const removeFromFavorites = (query) => {
    const newFavorites = favorites.filter(f => f !== query);
    setFavorites(newFavorites);
    localStorage.setItem('favoriteQueries', JSON.stringify(newFavorites));
  };

  const handleQuerySelect = (query) => {
    onSelectQuery(query);
    onClose();
  };

  if (!isOpen) return null;

  return (
    <div className="search-history-overlay">
      <div className="search-history-modal">
        <div className="search-history-header">
          <h3>Search History</h3>
          <button className="close-btn" onClick={onClose}>×</button>
        </div>
        
        <div className="search-history-content">
          <div className="history-section">
            <div className="section-header">
              <h4>Favorites</h4>
            </div>
            {favorites.length > 0 ? (
              <div className="query-list">
                {favorites.map((query, index) => (
                  <div key={index} className="query-item favorite">
                    <span onClick={() => handleQuerySelect(query)}>{query}</span>
                    <button 
                      className="remove-btn"
                      onClick={() => removeFromFavorites(query)}
                      title="Remove from favorites"
                    >
                      ★
                    </button>
                  </div>
                ))}
              </div>
            ) : (
              <div className="empty-state">No favorite queries yet</div>
            )}
          </div>

          <div className="history-section">
            <div className="section-header">
              <h4>Recent Searches</h4>
              {history.length > 0 && (
                <button className="clear-btn" onClick={clearHistory}>
                  Clear All
                </button>
              )}
            </div>
            {history.length > 0 ? (
              <div className="query-list">
                {history.map((query, index) => (
                  <div key={index} className="query-item">
                    <span onClick={() => handleQuerySelect(query)}>{query}</span>
                    <button 
                      className="favorite-btn"
                      onClick={() => addToFavorites(query)}
                      title="Add to favorites"
                      disabled={favorites.includes(query)}
                    >
                      {favorites.includes(query) ? '★' : '☆'}
                    </button>
                  </div>
                ))}
              </div>
            ) : (
              <div className="empty-state">No recent searches</div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default SearchHistory;