import React, { useState } from 'react';
import './Widget.css';
import { usePreferences } from '../context/PreferencesContext';

const Widget = ({ 
  id, 
  title, 
  children, 
  defaultMinimized = false, 
  closable = true, 
  minimizable = true,
  maximizable = true 
}) => {
  const { preferences, setWidgetState } = usePreferences();
  const [isMaximized, setIsMaximized] = useState(false);
  
  const widgetState = preferences.widgetStates[id] || {
    minimized: defaultMinimized,
    visible: true
  };
  
  const { minimized: isMinimized, visible: isVisible } = widgetState;



  if (!isVisible) return null;

  return (
    <div className={`widget ${isMinimized ? 'minimized' : ''} ${isMaximized ? 'maximized' : ''}`}>
      <div className="widget-header">
        <h3 className="widget-title">{title}</h3>
        <div className="widget-controls">
          {minimizable && (
            <button 
              className="widget-control minimize"
              onClick={() => setWidgetState(id, { ...widgetState, minimized: !isMinimized })}
              title={isMinimized ? 'Expand' : 'Minimize'}
            >
              {isMinimized ? '□' : '−'}
            </button>
          )}
          {maximizable && !isMinimized && (
            <button 
              className="widget-control maximize"
              onClick={() => setIsMaximized(!isMaximized)}
              title={isMaximized ? 'Restore' : 'Maximize'}
            >
              {isMaximized ? '⧉' : '□'}
            </button>
          )}
          {closable && (
            <button 
              className="widget-control close"
              onClick={() => setWidgetState(id, { ...widgetState, visible: false })}
              title="Close"
            >
              ×
            </button>
          )}
        </div>
      </div>
      
      {!isMinimized && (
        <div className="widget-content">
          {children}
        </div>
      )}
    </div>
  );
};

export default Widget;