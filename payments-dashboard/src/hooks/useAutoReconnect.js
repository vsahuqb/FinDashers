import { useState, useRef, useCallback } from 'react';

export const useAutoReconnect = () => {
  const [isReconnecting, setIsReconnecting] = useState(false);
  const [reconnectAttempts, setReconnectAttempts] = useState(0);
  const reconnectTimeoutRef = useRef(null);
  const maxReconnectAttempts = 5;
  const baseDelay = 1000; // 1 second

  const getReconnectDelay = useCallback((attempt) => {
    return Math.min(baseDelay * Math.pow(2, attempt), 30000); // Max 30 seconds
  }, []);

  const startReconnection = useCallback((reconnectFn, url) => {
    setReconnectAttempts(prev => {
      if (prev >= maxReconnectAttempts) {
        console.log('Max reconnection attempts reached');
        setIsReconnecting(false);
        return prev;
      }

      setIsReconnecting(true);
      const delay = getReconnectDelay(prev);
      
      console.log(`Reconnecting in ${delay}ms (attempt ${prev + 1}/${maxReconnectAttempts})`);
      
      reconnectTimeoutRef.current = setTimeout(() => {
        reconnectFn(url);
      }, delay);
      
      return prev + 1;
    });
  }, [maxReconnectAttempts, getReconnectDelay]);

  const stopReconnection = useCallback(() => {
    if (reconnectTimeoutRef.current) {
      clearTimeout(reconnectTimeoutRef.current);
      reconnectTimeoutRef.current = null;
    }
    setIsReconnecting(false);
    setReconnectAttempts(0);
  }, []);

  const resetReconnection = useCallback(() => {
    setReconnectAttempts(0);
    setIsReconnecting(false);
  }, []);

  return {
    isReconnecting,
    reconnectAttempts,
    maxReconnectAttempts,
    startReconnection,
    stopReconnection,
    resetReconnection
  };
};