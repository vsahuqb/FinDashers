// Test function to verify data flow from API to charts
import { apiResponseSample } from './sampleData';
import { processApiData, validateApiResponse } from './dataProcessor';

export const testDataFlow = () => {
  console.log('Testing data flow...');
  
  // Test 1: Validate API response
  const isValid = validateApiResponse(apiResponseSample);
  console.log('API validation:', isValid ? 'PASS' : 'FAIL');
  
  // Test 2: Process API data
  const processedData = processApiData(apiResponseSample);
  console.log('Data processing:', processedData ? 'PASS' : 'FAIL');
  
  // Test 3: Check data structure
  if (processedData) {
    const requiredFields = ['kpi', 'paymentMethods', 'hourlyFailures', 'riskScores', 'funnelData', 'sankeyData'];
    const hasAllFields = requiredFields.every(field => processedData.hasOwnProperty(field));
    console.log('Data structure:', hasAllFields ? 'PASS' : 'FAIL');
    
    console.log('Processed data sample:', {
      kpi: processedData.kpi,
      paymentMethodsCount: processedData.paymentMethods.length,
      hourlyFailuresLength: processedData.hourlyFailures.length,
      riskScoresCount: processedData.riskScores.length
    });
  }
  
  return processedData;
};