import { useState, useRef, useCallback } from 'react';
import { useAutoReconnect } from './useAutoReconnect';

export const useWebSocket = () => {
  const [connectionState, setConnectionState] = useState('disconnected');
  const [error, setError] = useState(null);
  const wsRef = useRef(null);
  const urlRef = useRef(null);
  const { isReconnecting, reconnectAttempts, startReconnection, stopReconnection, resetReconnection } = useAutoReconnect();

  const connect = useCallback((url) => {
    if (wsRef.current?.readyState === WebSocket.OPEN) {
      return;
    }

    urlRef.current = url;
    setConnectionState(isReconnecting ? 'reconnecting' : 'connecting');
    setError(null);

    try {
      wsRef.current = new WebSocket(url);

      wsRef.current.onopen = () => {
        setConnectionState('connected');
        setError(null);
        resetReconnection();
      };

      wsRef.current.onclose = (event) => {
        if (event.wasClean) {
          setConnectionState('disconnected');
        } else {
          setConnectionState('disconnected');
          if (urlRef.current) {
            startReconnection(connect, urlRef.current);
          }
        }
      };

      wsRef.current.onerror = () => {
        setError('WebSocket connection failed');
        setConnectionState('disconnected');
      };

    } catch (err) {
      setError(err.message);
      setConnectionState('disconnected');
    }
  }, [isReconnecting, startReconnection, resetReconnection]);

  const disconnect = useCallback(() => {
    stopReconnection();
    urlRef.current = null;
    if (wsRef.current) {
      wsRef.current.close();
      wsRef.current = null;
    }
    setConnectionState('disconnected');
    setError(null);
  }, [stopReconnection]);

  const sendMessage = useCallback((message) => {
    if (wsRef.current?.readyState === WebSocket.OPEN) {
      wsRef.current.send(JSON.stringify(message));
      return true;
    }
    return false;
  }, []);

  const setMessageHandler = useCallback((handler) => {
    if (wsRef.current) {
      wsRef.current.onmessage = (event) => {
        try {
          const data = JSON.parse(event.data);
          console.log('WebSocket message received:', data);
          handler(data);
          setError(null); // Clear any previous errors
        } catch (err) {
          console.error('WebSocket message parsing error:', err);
          setError('Invalid message format: ' + err.message);
        }
      };
    }
  }, []);

  return {
    connectionState,
    error,
    isReconnecting,
    reconnectAttempts,
    connect,
    disconnect,
    sendMessage,
    setMessageHandler
  };
};