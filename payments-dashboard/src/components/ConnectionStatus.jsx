import React from 'react';
import { useData } from '../context/DataContext';

const ConnectionStatus = () => {
  const { connectionStatus, error } = useData();

  const getStatusColor = () => {
    switch (connectionStatus) {
      case 'connected': return '#10b981'; // green
      case 'connecting': return '#f59e0b'; // orange
      case 'disconnected':
      case 'error':
      default: return '#ef4444'; // red
    }
  };

  const getStatusText = () => {
    switch (connectionStatus) {
      case 'connected': return 'Connected';
      case 'connecting': return 'Connecting...';
      case 'disconnected': return 'Disconnected';
      case 'error': return 'Connection Error';
      default: return 'Unknown';
    }
  };

  return (
    <div style={{
      display: 'flex',
      alignItems: 'center',
      gap: '8px',
      padding: '8px 12px',
      background: 'rgba(255, 255, 255, 0.1)',
      borderRadius: '6px',
      fontSize: '14px',
      color: 'white'
    }}>
      <div style={{
        width: '8px',
        height: '8px',
        borderRadius: '50%',
        backgroundColor: getStatusColor()
      }} />
      <span>{getStatusText()}</span>
      {error && (
        <span style={{ 
          fontSize: '12px', 
          color: '#fca5a5',
          marginLeft: '4px'
        }}>
          ({error})
        </span>
      )}
    </div>
  );
};

export default ConnectionStatus;