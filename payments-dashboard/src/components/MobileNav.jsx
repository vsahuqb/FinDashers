import React, { useState } from 'react';
import './MobileNav.css';

function MobileNav({ isOpen, onClose, activeTab, onTabChange }) {
  const tabs = [
    { id: 'overview', label: 'Overview', icon: '📊' },
    { id: 'analytics', label: 'Analytics', icon: '📈' },
    { id: 'search', label: 'Search', icon: '🔍' },
    { id: 'settings', label: 'Settings', icon: '⚙️' }
  ];

  const handleTabClick = (tabId) => {
    onTabChange(tabId);
    onClose();
  };

  return (
    <>
      {isOpen && <div className="mobile-nav-overlay" onClick={onClose} />}
      <nav className={`mobile-nav ${isOpen ? 'mobile-nav-open' : ''}`}>
        <div className="mobile-nav-header">
          <h3>Navigation</h3>
          <button className="mobile-nav-close" onClick={onClose}>×</button>
        </div>
        <div className="mobile-nav-content">
          {tabs.map(tab => (
            <button
              key={tab.id}
              className={`mobile-nav-item ${activeTab === tab.id ? 'active' : ''}`}
              onClick={() => handleTabClick(tab.id)}
            >
              <span className="mobile-nav-icon">{tab.icon}</span>
              <span className="mobile-nav-label">{tab.label}</span>
            </button>
          ))}
        </div>
      </nav>
    </>
  );
}

export default MobileNav;