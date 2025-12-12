import React from 'react';

const TabPanel = ({ tabId, children }) => {
  return (
    <div className="tab-panel">
      {children}
    </div>
  );
};

export default TabPanel;