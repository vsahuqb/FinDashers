const API_URL = process.env.REACT_APP_NL2SQL_API_URL || 'http://localhost:5144';

class NLSearchService {
  async searchQuery(naturalLanguageQuery) {
    console.log('NL2SQL API call started:', naturalLanguageQuery);
    try {
      this.validateQuery(naturalLanguageQuery);
      
      const requestBody = {
        query: naturalLanguageQuery,
        domain: 'payments'
      };
      
      console.log('Making API request to:', `${API_URL}/api/query/query`);
      console.log('Request body:', requestBody);
      
      const response = await fetch(`${API_URL}/api/query/query`, {
        method: 'POST',
        headers: {
          'accept': '*/*',
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(requestBody)
      });

      console.log('API response status:', response.status);
      
      if (!response.ok) {
        throw new Error(`API Error: ${response.status} ${response.statusText}`);
      }

      const data = await response.json();
      console.log('API response data:', data);
      return this.validateResponse(data);
    } catch (error) {
      console.error('NL2SQL API error:', error);
      return this.handleError(error);
    }
  }

  validateQuery(query) {
    if (!query || typeof query !== 'string') {
      throw new Error('Query must be a non-empty string');
    }
    if (query.trim().length < 3) {
      throw new Error('Query must be at least 3 characters long');
    }
    if (query.length > 500) {
      throw new Error('Query must be less than 500 characters');
    }
  }

  validateResponse(data) {
    if (!data || typeof data !== 'object') {
      throw new Error('Invalid response format');
    }
    
    if (data.error) {
      throw new Error(data.error);
    }
    
    const { success, data: results, sqlScript, rowCount } = data;
    
    if (!success) {
      throw new Error('Query execution failed');
    }

    return {
      sql: Array.isArray(sqlScript) ? sqlScript.join('\n') : sqlScript || 'No SQL provided',
      results: results || [],
      metadata: { total_count: rowCount || 0, execution_time: '0.1s' },
      timestamp: new Date().toISOString()
    };
  }

  getSchemaContext() {
    return {
      tables: {
        payments: ['id', 'amount', 'currency', 'status', 'method', 'created_at', 'country'],
        transactions: ['id', 'payment_id', 'type', 'amount', 'fee', 'processed_at'],
        merchants: ['id', 'name', 'category', 'country', 'created_at']
      },
      relationships: [
        'payments.id = transactions.payment_id',
        'payments.merchant_id = merchants.id'
      ]
    };
  }

  handleError(error) {
    console.error('NL2SQL Service Error:', error);
    
    if (error.name === 'TypeError' && error.message.includes('fetch')) {
      return {
        error: 'Network error - please check your connection',
        type: 'network'
      };
    }
    
    if (error.message.includes('API Error: 401')) {
      return {
        error: 'Authentication failed - invalid API key',
        type: 'auth'
      };
    }
    
    if (error.message.includes('API Error: 429')) {
      return {
        error: 'Rate limit exceeded - please try again later',
        type: 'rate_limit'
      };
    }
    
    return {
      error: error.message || 'An unexpected error occurred',
      type: 'general'
    };
  }

  // Mock implementation for development/demo
  async mockSearch(query) {
    await new Promise(resolve => setTimeout(resolve, 1000)); // Simulate API delay
    
    const mockResponses = {
      'failed payments': {
        sql: "SELECT * FROM payments WHERE status = 'failed' ORDER BY created_at DESC LIMIT 100",
        results: [
          { id: 1, amount: 150.00, status: 'failed', method: 'credit_card', created_at: '2024-01-15' },
          { id: 2, amount: 75.50, status: 'failed', method: 'paypal', created_at: '2024-01-14' }
        ],
        metadata: { total_count: 2, execution_time: '0.05s' }
      },
      'success rate': {
        sql: "SELECT (COUNT(CASE WHEN status = 'success' THEN 1 END) * 100.0 / COUNT(*)) as success_rate FROM payments",
        results: [{ success_rate: 94.2 }],
        metadata: { total_count: 1, execution_time: '0.03s' }
      },
      'payment methods': {
        sql: "SELECT method, COUNT(*) as count, SUM(amount) as total_amount FROM payments GROUP BY method ORDER BY count DESC",
        results: [
          { method: 'credit_card', count: 1250, total_amount: 125000.00 },
          { method: 'paypal', count: 890, total_amount: 89000.00 },
          { method: 'bank_transfer', count: 450, total_amount: 67500.00 }
        ],
        metadata: { total_count: 3, execution_time: '0.08s' }
      }
    };

    const lowerQuery = query.toLowerCase();
    for (const [key, response] of Object.entries(mockResponses)) {
      if (lowerQuery.includes(key)) {
        return { ...response, timestamp: new Date().toISOString() };
      }
    }

    return {
      sql: `SELECT * FROM payments WHERE description LIKE '%${query}%' LIMIT 10`,
      results: [],
      metadata: { total_count: 0, execution_time: '0.02s' },
      timestamp: new Date().toISOString()
    };
  }
}

export default new NLSearchService();