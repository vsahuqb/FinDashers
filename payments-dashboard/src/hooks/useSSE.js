import { useState, useRef, useCallback } from 'react';

export const useSSE = () => {
  const [connectionState, setConnectionState] = useState('disconnected');
  const [error, setError] = useState(null);
  const eventSourceRef = useRef(null);
  const handlerRef = useRef(null);

  const connect = useCallback((url) => {
    if (eventSourceRef.current?.readyState === EventSource.OPEN) {
      return;
    }

    setConnectionState('connecting');
    setError(null);

    try {
      eventSourceRef.current = new EventSource(url);

      eventSourceRef.current.onopen = () => {
        setConnectionState('connected');
        setError(null);
      };

      eventSourceRef.current.onmessage = (event) => {
        try {
          const data = JSON.parse(event.data);
          console.log('SSE message received:', data);
          if (handlerRef.current) {
            handlerRef.current(data);
          }
        } catch (err) {
          console.error('SSE message parsing error:', err);
          setError('Invalid message format: ' + err.message);
        }
      };

      eventSourceRef.current.onerror = () => {
        setError('SSE connection failed');
        setConnectionState('disconnected');
      };

    } catch (err) {
      setError(err.message);
      setConnectionState('disconnected');
    }
  }, []);

  const disconnect = useCallback(() => {
    if (eventSourceRef.current) {
      eventSourceRef.current.close();
      eventSourceRef.current = null;
    }
    setConnectionState('disconnected');
    setError(null);
  }, []);

  const setMessageHandler = useCallback((handler) => {
    handlerRef.current = handler;
  }, []);

  return {
    connectionState,
    error,
    connect,
    disconnect,
    setMessageHandler
  };
};