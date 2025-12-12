import { useState, useRef, useCallback } from 'react';

export const useHttpPolling = () => {
  const [connectionState, setConnectionState] = useState('disconnected');
  const [error, setError] = useState(null);
  const intervalRef = useRef(null);
  const handlerRef = useRef(null);

  const startPolling = useCallback((url, interval = 5000) => {
    if (intervalRef.current) {
      return;
    }

    setConnectionState('connecting');
    setError(null);

    const poll = async () => {
      try {
        const response = await fetch(url);
        if (!response.ok) {
          throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }
        
        const data = await response.json();
        if (handlerRef.current) {
          handlerRef.current(data);
        }
        
        if (connectionState !== 'connected') {
          setConnectionState('connected');
        }
        setError(null);
      } catch (err) {
        console.error('Polling error:', err);
        setError(err.message);
        setConnectionState('disconnected');
      }
    };

    // Initial poll
    poll();
    
    // Set up interval
    intervalRef.current = setInterval(poll, interval);
    
  }, [connectionState]);

  const stopPolling = useCallback(() => {
    if (intervalRef.current) {
      clearInterval(intervalRef.current);
      intervalRef.current = null;
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
    startPolling,
    stopPolling,
    setMessageHandler
  };
};