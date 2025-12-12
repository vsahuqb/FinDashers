import React from 'react';
import './ChartSkeleton.css';

const ChartSkeleton = ({ type = 'default' }) => {
  return (
    <div className="chart-skeleton">
      <div className="skeleton-header">
        <div className="skeleton-line skeleton-title"></div>
      </div>
      <div className={`skeleton-content skeleton-${type}`}>
        {type === 'bar' && (
          <div className="skeleton-bars">
            {[1, 2, 3, 4].map(i => (
              <div key={i} className="skeleton-bar" style={{ height: `${40 + i * 15}%` }}></div>
            ))}
          </div>
        )}
        {type === 'gauge' && (
          <div className="skeleton-gauge">
            <div className="skeleton-circle"></div>
          </div>
        )}
        {type === 'line' && (
          <div className="skeleton-line-chart">
            <div className="skeleton-line skeleton-chart-line"></div>
          </div>
        )}
        {type === 'default' && (
          <div className="skeleton-default">
            <div className="skeleton-line"></div>
            <div className="skeleton-line"></div>
            <div className="skeleton-line short"></div>
          </div>
        )}
      </div>
    </div>
  );
};

export default ChartSkeleton;