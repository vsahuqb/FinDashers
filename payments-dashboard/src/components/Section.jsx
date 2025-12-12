import React, { useState, useEffect } from 'react';
import './Section.css';
import { usePreferences } from '../context/PreferencesContext';

function Section({ title, children, collapsible = false, defaultCollapsed = false, sectionId }) {
  const { preferences, toggleSection } = usePreferences();
  
  const isCollapsed = collapsible && sectionId 
    ? preferences.collapsedSections[sectionId] || false
    : defaultCollapsed;



  const toggleCollapse = () => {
    if (collapsible && sectionId) {
      toggleSection(sectionId);
    }
  };

  return (
    <div className="section">
      {title && (
        <div className="section-header" onClick={toggleCollapse}>
          <h2 className="section-title">{title}</h2>
          {collapsible && (
            <button className="collapse-button">
              {isCollapsed ? '▼' : '▲'}
            </button>
          )}
        </div>
      )}
      <div className={`section-content ${isCollapsed ? 'collapsed' : ''}`}>
        {children}
      </div>
    </div>
  );
}

export default Section;