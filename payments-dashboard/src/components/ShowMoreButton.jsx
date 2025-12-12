import React from 'react';
import './ShowMoreButton.css';

const ShowMoreButton = ({ showMore, onToggle, moreCount = 0 }) => {
  return (
    <button className="show-more-button" onClick={onToggle}>
      <span className="show-more-icon">
        {showMore ? '▲' : '▼'}
      </span>
      <span className="show-more-text">
        {showMore ? 'Show Less' : `Show More (${moreCount})`}
      </span>
    </button>
  );
};

export default ShowMoreButton;