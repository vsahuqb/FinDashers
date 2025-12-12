// Test utility for NL2SQL API
const testNL2SQLApi = async () => {
  const API_URL = process.env.REACT_APP_NL2SQL_API_URL || 'http://localhost:5144';
  
  try {
    const response = await fetch(`${API_URL}/api/query/query`, {
      method: 'POST',
      headers: {
        'accept': '*/*',
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        query: 'last 10 payments',
        domain: 'payments'
      })
    });

    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    }

    const data = await response.json();
    console.log('NL2SQL API Response:', data);
    return data;
  } catch (error) {
    console.error('NL2SQL API Test Failed:', error);
    return { error: error.message };
  }
};

export default testNL2SQLApi;