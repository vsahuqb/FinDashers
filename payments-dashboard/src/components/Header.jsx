import React, { useEffect, useRef, useState } from 'react';
import { useConnection } from '../hooks/useConnection';
import { useData } from '../context/DataContext';
import SearchBar from './SearchBar';
import SearchResults from './SearchResults';
import MobileNav from './MobileNav';
import './Header.css';

function Header({ activeTab, onTabChange }) {
  // Disabled connection hooks to prevent infinite loops
  const connectionState = 'disconnected';
  const connectionType = 'none';
  const { updateFromApi } = useData();
  const searchRef = useRef(null);
  const [isMobileNavOpen, setIsMobileNavOpen] = useState(false);
  const [showSearchResults, setShowSearchResults] = useState(false);

  const handleSearch = (query) => {
    console.log('Search query:', query);
    setShowSearchResults(true);
  };

  const searchSuggestions = [
    'Show me failed payments today',
    'What is the success rate this week?',
    'Top payment methods by volume',
    'Average transaction amount',
    'Payment failures by country'
  ];

  // Disabled connection logic to prevent infinite loops
  // useEffect(() => {
  //   console.log('Connection disabled for stability');
  // }, []);

  useEffect(() => {
    const handleKeyDown = (e) => {
      if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
        e.preventDefault();
        searchRef.current?.querySelector('.search-input')?.focus();
      }
    };
    
    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, []);

  const getStatusColor = () => {
    switch (connectionState) {
      case 'connected': return '#4CAF50';
      case 'connecting': return '#FF9800';
      default: return '#f44336';
    }
  };

  return (
    <header className="header">
      <div className="header-left">
        <img src="/qubeyond-logo.png" alt="Qubeyond Logo" className="logo" />
        <div className="header-content">
          <h1>Payment Health Dashboard</h1>
          <p>Real-time Payment Flow & Performance Analytics</p>
        </div>
      </div>
      
      <div className="header-center" ref={searchRef}>
        <SearchBar 
          onSearch={handleSearch}
          suggestions={searchSuggestions}
          placeholder="Ask anything about your payments... (Ctrl+K)"
        />
      </div>
      
      <div className="header-right">
        <button 
          className="mobile-menu-btn"
          onClick={() => setIsMobileNavOpen(true)}
        >
          <div className="mobile-menu-icon">
            <span></span>
            <span></span>
            <span></span>
          </div>
        </button>
        
        <div className="connection-status-mini">
          <div 
            className="status-dot" 
            style={{ backgroundColor: getStatusColor() }}
            title={`Connection: ${connectionState} (${connectionType})`}
          ></div>
        </div>
      </div>
      
      <MobileNav 
        isOpen={isMobileNavOpen}
        onClose={() => setIsMobileNavOpen(false)}
        activeTab={activeTab}
        onTabChange={onTabChange}
      />
      
      <SearchResults 
        isOpen={showSearchResults}
        onClose={() => setShowSearchResults(false)}
      />
    </header>
  );
}

export default Header;