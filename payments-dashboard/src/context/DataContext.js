import React, { createContext, useContext, useState, useEffect } from 'react';
import { currentDashboardData } from '../utils/sampleData';
import { processApiData, validateApiResponse } from '../utils/dataProcessor';
import apiService from '../services/apiService';
import { transformApiResponse } from '../utils/dataTransformer';

const DataContext = createContext();

export const useData = () => {
  const context = useContext(DataContext);
  if (!context) {
    throw new Error('useData must be used within a DataProvider');
  }
  return context;
};

export const DataProvider = ({ children }) => {
  const [dashboardData, setDashboardData] = useState(currentDashboardData);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  const [isManualConnection, setIsManualConnection] = useState(false);
  const [connectionStatus, setConnectionStatus] = useState('disconnected');
  const [webSocket, setWebSocket] = useState(null);
  const [filters, setFilters] = useState({ timeRange: '24h' });

  const updateData = (newData) => {
    setDashboardData(prev => ({ ...prev, ...newData }));
  };

  const updateFromApi = (apiResponse) => {
    setIsLoading(true);
    setError(null);
    
    try {
      if (!validateApiResponse(apiResponse)) {
        throw new Error('Invalid API response format');
      }
      
      const processedData = processApiData(apiResponse);
      if (processedData) {
        setDashboardData(processedData);
      }
    } catch (err) {
      setError(err.message);
      console.error('Error processing API data:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const resetData = () => {
    setDashboardData(currentDashboardData);
    setError(null);
  };

  const connectWebSocket = () => {
    try {
      setConnectionStatus('connecting');
      const ws = apiService.createWebSocketConnection();
      
      ws.onopen = () => {
        console.log('WebSocket connected');
        setConnectionStatus('connected');
        setError(null);
      };
      
      ws.onmessage = (event) => {
        try {
          const apiResponse = JSON.parse(event.data);
          const transformedData = transformApiResponse(apiResponse);
          if (transformedData) {
            setDashboardData(transformedData);
            setIsLoading(false);
          }
        } catch (err) {
          console.error('Error processing WebSocket message:', err);
        }
      };
      
      ws.onclose = () => {
        console.log('WebSocket disconnected');
        setConnectionStatus('disconnected');
        // Attempt to reconnect after 5 seconds
        setTimeout(connectWebSocket, 5000);
      };
      
      ws.onerror = (error) => {
        console.error('WebSocket error:', error);
        setConnectionStatus('error');
        setError('WebSocket connection failed');
      };
      
      setWebSocket(ws);
    } catch (err) {
      console.error('Failed to create WebSocket connection:', err);
      setConnectionStatus('error');
      setError('Failed to connect to server');
    }
  };

  const getDateRangeFromFilters = (filters) => {
    const endDate = new Date();
    let startDate;

    if (filters.timeRange === 'custom' && filters.startDate && filters.endDate) {
      return {
        startDate: new Date(filters.startDate).toISOString(),
        endDate: new Date(filters.endDate).toISOString()
      };
    }

    switch (filters.timeRange) {
      case '1h':
        startDate = new Date(endDate.getTime() - 60 * 60 * 1000);
        break;
      case '7d':
        startDate = new Date(endDate.getTime() - 7 * 24 * 60 * 60 * 1000);
        break;
      case '30d':
        startDate = new Date(endDate.getTime() - 30 * 24 * 60 * 60 * 1000);
        break;
      default: // '24h'
        startDate = new Date(endDate.getTime() - 24 * 60 * 60 * 1000);
    }

    return {
      startDate: startDate.toISOString(),
      endDate: endDate.toISOString()
    };
  };

  const loadDataWithFilters = async (currentFilters = filters) => {
    try {
      setIsLoading(true);
      const { startDate, endDate } = getDateRangeFromFilters(currentFilters);
      
      const apiResponse = await apiService.getDashboardData(startDate, endDate);
      
      const transformedData = transformApiResponse(apiResponse);
      if (transformedData) {
        setDashboardData(transformedData);
      }
    } catch (err) {
      console.error('Failed to load data:', err);
      setError('Failed to load dashboard data');
    } finally {
      setIsLoading(false);
    }
  };

  const loadInitialData = () => loadDataWithFilters();

  useEffect(() => {
    loadInitialData();
    connectWebSocket();
    
    return () => {
      if (webSocket) {
        webSocket.close();
      }
    };
  }, []);

  useEffect(() => {
    loadDataWithFilters(filters);
  }, [filters]);

  return (
    <DataContext.Provider value={{ 
      dashboardData, 
      isLoading,
      error,
      isManualConnection,
      connectionStatus,
      filters,
      updateData, 
      updateFromApi,
      resetData,
      setIsManualConnection,
      connectWebSocket,
      loadInitialData,
      setFilters,
      loadDataWithFilters
    }}>
      {children}
    </DataContext.Provider>
  );
};