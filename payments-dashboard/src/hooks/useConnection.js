import { useState, useCallback } from 'react';
import { useWebSocket } from './useWebSocket';
import { useHttpPolling } from './useHttpPolling';
import { useSSE } from './useSSE';

export const useConnection = () => {
  const [connectionType, setConnectionType] = useState('websocket');
  const [pollingInterval, setPollingInterval] = useState(5000);
  
  const websocket = useWebSocket();
  const polling = useHttpPolling();
  const sse = useSSE();

  const getCurrentConnection = () => {
    switch (connectionType) {
      case 'websocket': return websocket;
      case 'polling': return polling;
      case 'sse': return sse;
      default: return websocket;
    }
  };

  const currentConnection = getCurrentConnection();

  const connect = useCallback((url) => {
    switch (connectionType) {
      case 'websocket':
        websocket.connect(url);
        break;
      case 'polling':
        const httpUrl = url.replace('ws://', 'http://').replace('wss://', 'https://');
        polling.startPolling(httpUrl, pollingInterval);
        break;
      case 'sse':
        const sseUrl = url.replace('ws://', 'http://').replace('wss://', 'https://').replace('/ws', '/events');
        sse.connect(sseUrl);
        break;
      default:
        websocket.connect(url);
    }
  }, [connectionType, websocket, polling, sse, pollingInterval]);

  const disconnect = useCallback(() => {
    websocket.disconnect();
    polling.stopPolling();
    sse.disconnect();
  }, [websocket, polling, sse]);

  const switchToPolling = useCallback(() => {
    disconnect();
    setConnectionType('polling');
  }, [disconnect]);

  const switchToWebSocket = useCallback(() => {
    disconnect();
    setConnectionType('websocket');
  }, [disconnect]);

  const switchToSSE = useCallback(() => {
    disconnect();
    setConnectionType('sse');
  }, [disconnect]);

  const setMessageHandler = useCallback((handler) => {
    websocket.setMessageHandler(handler);
    polling.setMessageHandler(handler);
    sse.setMessageHandler(handler);
  }, [websocket, polling, sse]);

  return {
    connectionState: currentConnection.connectionState,
    error: currentConnection.error,
    connectionType,
    pollingInterval,
    isReconnecting: currentConnection.isReconnecting || false,
    reconnectAttempts: currentConnection.reconnectAttempts || 0,
    connect,
    disconnect,
    switchToPolling,
    switchToWebSocket,
    switchToSSE,
    setPollingInterval,
    setMessageHandler,
    sendMessage: websocket.sendMessage
  };
};