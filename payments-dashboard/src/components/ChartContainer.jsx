import React, { useState, useEffect } from 'react';
import { useLazyLoad } from '../hooks/useLazyLoad';
import ChartSkeleton from './ChartSkeleton';
import './ChartContainer.css';

function ChartContainer({ children, title, size = "regular", skeletonType = 'default', className = '' }) {
  const { elementRef, isVisible, hasLoaded } = useLazyLoad();
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    if (isVisible) {
      const timer = setTimeout(() => {
        setIsLoading(false);
      }, 300);
      return () => clearTimeout(timer);
    }
  }, [isVisible]);

  return (
    <div 
      ref={elementRef} 
      className={`chart-container ${size} ${className} ${isLoading ? 'loading' : ''}`}
    >
      {title && <h3 className="chart-title">{title}</h3>}
      <div className="chart-content">
        {!hasLoaded || isLoading ? (
          <ChartSkeleton type={skeletonType} />
        ) : (
          children
        )}
      </div>
    </div>
  );
}

export default ChartContainer;