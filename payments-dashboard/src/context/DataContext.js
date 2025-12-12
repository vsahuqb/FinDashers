import React, { createContext, useContext, useState } from 'react';
import { currentDashboardData } from '../utils/sampleData';
import { processApiData, validateApiResponse } from '../utils/dataProcessor';

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
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const [isManualConnection, setIsManualConnection] = useState(false);

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

  return (
    <DataContext.Provider value={{ 
      dashboardData, 
      isLoading,
      error,
      isManualConnection,
      updateData, 
      updateFromApi,
      resetData,
      setIsManualConnection
    }}>
      {children}
    </DataContext.Provider>
  );
};