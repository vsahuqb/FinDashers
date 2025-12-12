import React, { useState } from 'react';
import { useConnection } from '../hooks/useConnection';
import { useData } from '../context/DataContext';
import './ConnectionControl.css';

function ConnectionControl() {
  const [url, setUrl] = useState('ws://localhost:8080/ws');
  const { connectionState, connectionType, error, connect, disconnect, setMessageHandler, isReconnecting, reconnectAttempts } = useConnection();
  const { updateFromApi, setIsManualConnection } = useData();

  const validateUrl = (url) => {
    try {
      const urlObj = new URL(url);
      return ['ws:', 'wss:', 'http:', 'https:'].includes(urlObj.protocol);
    } catch {
      return false;
    }
  };

  const handleConnect = () => {
    const trimmedUrl = url.trim();
    if (trimmedUrl && validateUrl(trimmedUrl)) {
      setIsManualConnection(true);
      setMessageHandler((data) => {
        console.log('Manual connection received data:', data);
        updateFromApi(data);
      });
      connect(trimmedUrl);
    }
  };

  const handleDisconnect = () => {
    setIsManualConnection(false);
    disconnect();
  };

  const getStatusColor = () => {
    switch (connectionState) {
      case 'connected': return '#4CAF50';
      case 'connecting': return '#FF9800';
      case 'reconnecting': return '#FF5722';
      case 'disconnected': return '#f44336';
      default: return '#9E9E9E';
    }
  };

  const getStatusText = () => {
    const typeText = connectionType ? ` (${connectionType})` : '';
    switch (connectionState) {
      case 'connected': return `Connected${typeText}`;
      case 'connecting': return 'Connecting...';
      case 'reconnecting': return 'Reconnecting...';
      case 'disconnected': return 'Disconnected';
      default: return 'Unknown';
    }
  };

  return (
    <div className="connection-control">
      <div className="connection-status">
        <div 
          className="status-indicator" 
          style={{ backgroundColor: getStatusColor() }}
        ></div>
        <span className="status-text">{getStatusText()}</span>
      </div>
      
      <div className="connection-form">
        <input
          type="text"
          placeholder="URL (ws://localhost:8080/ws)"
          value={url}
          onChange={(e) => setUrl(e.target.value)}
          disabled={connectionState === 'connecting'}
          className={!validateUrl(url) && url ? 'invalid' : ''}
        />
        
        <div className="connection-buttons">
          <button 
            onClick={handleConnect}
            disabled={!url.trim() || !validateUrl(url) || connectionState === 'connected' || connectionState === 'connecting'}
            className={!validateUrl(url) && url ? 'invalid' : ''}
          >
            Connect
          </button>
          <button 
            onClick={handleDisconnect}
            disabled={connectionState === 'disconnected'}
          >
            Disconnect
          </button>
        </div>
      </div>
      
      {error && <div className="connection-error">{error}</div>}
      
      {connectionState === 'reconnecting' && (
        <div className="reconnection-info">
          Reconnecting... (attempt {connectionType === 'websocket' ? reconnectAttempts : 0})
        </div>
      )}
    </div>
  );
}

export default ConnectionControl;