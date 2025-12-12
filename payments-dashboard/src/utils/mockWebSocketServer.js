// Mock WebSocket server for testing message handling
import { apiResponseSample } from './sampleData';

export const createMockWebSocketServer = (port = 8080) => {
  console.log(`Mock WebSocket server would run on ws://localhost:${port}/ws`);
  
  // Simulate periodic data updates
  const simulateDataUpdates = (onMessage) => {
    const interval = setInterval(() => {
      // Create variations of the sample data
      const mockData = {
        ...apiResponseSample,
        paymentSuccessRate: {
          ...apiResponseSample.paymentSuccessRate,
          dailySuccessRate: 85 + Math.random() * 10, // 85-95%
          totalTransactions: 1200 + Math.floor(Math.random() * 100),
          approvedCount: Math.floor((1200 + Math.random() * 100) * 0.87),
          declinedCount: Math.floor((1200 + Math.random() * 100) * 0.13)
        },
        paymentHealthHeatIndex: {
          ...apiResponseSample.paymentHealthHeatIndex,
          totalScore: 80 + Math.floor(Math.random() * 20),
          unusualFailuresScore: Math.floor(Math.random() * 20),
          settlementDelayScore: Math.floor(Math.random() * 15),
          highRiskCardScore: Math.floor(Math.random() * 18),
          refundSpikeScore: Math.floor(Math.random() * 10)
        },
        generatedAt: new Date().toISOString()
      };
      
      onMessage(mockData);
    }, 5000); // Update every 5 seconds
    
    return () => clearInterval(interval);
  };
  
  return { simulateDataUpdates };
};

// Test function to simulate WebSocket messages
export const testWebSocketMessages = (messageHandler) => {
  console.log('Testing WebSocket message handling...');
  
  // Test with sample data
  setTimeout(() => {
    console.log('Sending test message 1...');
    messageHandler(apiResponseSample);
  }, 1000);
  
  // Test with modified data
  setTimeout(() => {
    console.log('Sending test message 2...');
    const modifiedData = {
      ...apiResponseSample,
      paymentSuccessRate: {
        ...apiResponseSample.paymentSuccessRate,
        dailySuccessRate: 92.5,
        totalTransactions: 1350
      }
    };
    messageHandler(modifiedData);
  }, 3000);
};