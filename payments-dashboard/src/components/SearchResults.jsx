import React from 'react';
import { useSearch } from '../context/SearchContext';
import searchProcessor from '../utils/searchProcessor';
import './SearchResults.css';

const SearchResults = ({ isOpen, onClose }) => {
  const { results, loading, error, query } = useSearch();

  if (!isOpen) return null;

  const handleExport = () => {
    if (!results?.results) return;
    
    const csv = searchProcessor.exportToCSV(results.results);
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `search-results-${Date.now()}.csv`;
    a.click();
    URL.revokeObjectURL(url);
  };

  return (
    <div className="search-results-overlay">
      <div className="search-results-modal">
        <div className="search-results-header">
          <h3>Search Results: "{query}"</h3>
          <button className="close-btn" onClick={onClose}>×</button>
        </div>
        
        <div className="search-results-content">
          {loading && <div className="loading">Searching...</div>}
          
          {error && <div className="error">Error: {error}</div>}
          
          {results && !loading && (
            <>
              <div className="results-info">
                <span>{results.metadata?.total_count || 0} results</span>
                <span>Execution time: {results.metadata?.execution_time}</span>
                <button onClick={handleExport} className="export-btn">Export CSV</button>
              </div>
              
              <div className="sql-query">
                <strong>SQL:</strong> <code>{results.sql}</code>
              </div>
              
              {results.results?.length > 0 ? (
                <div className="results-table">
                  <table>
                    <thead>
                      <tr>
                        {Object.keys(results.results[0]).map(key => (
                          <th key={key}>{key}</th>
                        ))}
                      </tr>
                    </thead>
                    <tbody>
                      {results.results.map((row, i) => (
                        <tr key={i}>
                          {Object.values(row).map((value, j) => (
                            <td key={j}>{searchProcessor.formatValue(value)}</td>
                          ))}
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              ) : (
                <div className="no-results">No results found</div>
              )}
            </>
          )}
        </div>
      </div>
    </div>
  );
};

export default SearchResults;