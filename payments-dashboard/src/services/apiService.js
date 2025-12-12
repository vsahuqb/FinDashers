const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5144';

class ApiService {
  async getDashboardData(startDate, endDate) {
    const params = new URLSearchParams({
      startDate: startDate,
      endDate: endDate
    });

    const response = await fetch(`${API_BASE_URL}/api/dashboard?${params}`);
    
    if (!response.ok) {
      throw new Error(`API Error: ${response.status} ${response.statusText}`);
    }
    
    return await response.json();
  }

  createWebSocketConnection() {
    const wsUrl = API_BASE_URL.replace('http', 'ws') + '/ws';
    return new WebSocket(wsUrl);
  }
}

export default new ApiService();