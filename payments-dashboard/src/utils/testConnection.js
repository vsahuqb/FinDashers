// Test utility to verify API and WebSocket connections
export const testApiConnection = async () => {
  try {
    const response = await fetch('http://localhost:5144/api/dashboard?startDate=2024-01-01&endDate=2024-01-02');
    if (response.ok) {
      const data = await response.json();
      console.log('API Connection Test - Success:', data);
      return { success: true, data };
    } else {
      console.error('API Connection Test - Failed:', response.status, response.statusText);
      return { success: false, error: `HTTP ${response.status}` };
    }
  } catch (error) {
    console.error('API Connection Test - Error:', error);
    return { success: false, error: error.message };
  }
};

export const testWebSocketConnection = () => {
  return new Promise((resolve) => {
    try {
      const ws = new WebSocket('ws://localhost:5144/ws');
      
      const timeout = setTimeout(() => {
        ws.close();
        resolve({ success: false, error: 'Connection timeout' });
      }, 5000);
      
      ws.onopen = () => {
        clearTimeout(timeout);
        console.log('WebSocket Connection Test - Success');
        ws.close();
        resolve({ success: true });
      };
      
      ws.onerror = (error) => {
        clearTimeout(timeout);
        console.error('WebSocket Connection Test - Error:', error);
        resolve({ success: false, error: 'Connection failed' });
      };
    } catch (error) {
      console.error('WebSocket Connection Test - Exception:', error);
      resolve({ success: false, error: error.message });
    }
  });
};