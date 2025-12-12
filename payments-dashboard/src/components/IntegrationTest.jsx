import React, { useState, useEffect } from 'react';
import { testApiConnection, testWebSocketConnection } from '../utils/testConnection';

const IntegrationTest = () => {
  const [apiStatus, setApiStatus] = useState('testing');
  const [wsStatus, setWsStatus] = useState('testing');
  const [apiData, setApiData] = useState(null);

  useEffect(() => {
    const runTests = async () => {
      // Test API
      const apiResult = await testApiConnection();
      setApiStatus(apiResult.success ? 'success' : 'failed');
      if (apiResult.success) {
        setApiData(apiResult.data);
      }

      // Test WebSocket
      const wsResult = await testWebSocketConnection();
      setWsStatus(wsResult.success ? 'success' : 'failed');
    };

    runTests();
  }, []);

  const getStatusColor = (status) => {
    switch (status) {
      case 'success': return '#10b981';
      case 'failed': return '#ef4444';
      case 'testing': return '#f59e0b';
      default: return '#6b7280';
    }
  };

  return (
    <div style={{
      position: 'fixed',
      top: '20px',
      right: '20px',
      background: 'rgba(0, 0, 0, 0.8)',
      color: 'white',
      padding: '16px',
      borderRadius: '8px',
      fontSize: '14px',
      zIndex: 1000,
      minWidth: '250px'
    }}>
      <h4 style={{ margin: '0 0 12px 0' }}>Integration Status</h4>
      
      <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '8px' }}>
        <div style={{
          width: '8px',
          height: '8px',
          borderRadius: '50%',
          backgroundColor: getStatusColor(apiStatus)
        }} />
        <span>API Connection: {apiStatus}</span>
      </div>
      
      <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '8px' }}>
        <div style={{
          width: '8px',
          height: '8px',
          borderRadius: '50%',
          backgroundColor: getStatusColor(wsStatus)
        }} />
        <span>WebSocket: {wsStatus}</span>
      </div>
      
      {apiData && (
        <div style={{ marginTop: '12px', fontSize: '12px', opacity: 0.8 }}>
          <div>Transactions: {apiData.paymentSuccessRate?.totalTransactions || 0}</div>
          <div>Success Rate: {apiData.paymentSuccessRate?.approvedCount || 0}/{apiData.paymentSuccessRate?.totalTransactions || 0}</div>
          <div>Health Score: {apiData.paymentHealthHeatIndex?.overallScore || 0}</div>
        </div>
      )}
    </div>
  );
};

export default IntegrationTest;