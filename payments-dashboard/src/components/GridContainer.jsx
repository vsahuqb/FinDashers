import React from 'react';
import './GridContainer.css';

const GridContainer = ({ children, columns = 'auto', gap = '20px', className = '' }) => {
  return (
    <div 
      className={`grid-container ${className}`}
      style={{
        '--grid-columns': columns,
        '--grid-gap': gap
      }}
    >
      {children}
    </div>
  );
};

export default GridContainer;