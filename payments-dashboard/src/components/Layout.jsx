import React from 'react';
import './Layout.css';
import Header from './Header';
import { useData } from '../context/DataContext';

function Layout({ children }) {
  const { isLoading, error, connectionStatus } = useData();

  return (
    <div className="layout">
      <Header />
      <main className="main-content">
        {isLoading && connectionStatus !== 'connected' && (
          <div style={{
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            minHeight: '400px',
            color: 'white'
          }}>
            <div className="spinner-border" role="status">
              <span className="visually-hidden">Loading dashboard data...</span>
            </div>
          </div>
        )}
        {error && connectionStatus === 'error' && (
          <div style={{
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            minHeight: '400px',
            color: '#ef4444',
            textAlign: 'center'
          }}>
            <div>
              <h3>Connection Error</h3>
              <p>{error}</p>
              <p>Please ensure the FinDashers API is running on http://localhost:5144</p>
            </div>
          </div>
        )}
        {(!isLoading || connectionStatus === 'connected') && !error && children}
      </main>
    </div>
  );
}

export default Layout;