import React from 'react';
import './KPICard.css';
import { useData } from '../context/DataContext';

function KPICard() {
  const { dashboardData } = useData();
  const { kpi } = dashboardData;

  return (
    <div className="kpi-card">
      <div className="kpi-main">
        <h2>{kpi.healthScore}/100</h2>
        <p>Payment Health Score - {kpi.healthStatus}</p>
      </div>
      <div className="kpi-metrics">
        <div className="kpi-metric">
          <div className="kpi-value">{kpi.totalTransactions.toLocaleString()}</div>
          <div className="kpi-label">Total Transactions</div>
        </div>
        <div className="kpi-metric">
          <div className="kpi-value">{kpi.approvedCount.toLocaleString()}</div>
          <div className="kpi-label">Approved</div>
        </div>
        <div className="kpi-metric">
          <div className="kpi-value">{kpi.declinedCount.toLocaleString()}</div>
          <div className="kpi-label">Declined</div>
        </div>
        <div className="kpi-metric">
          <div className="kpi-value">{kpi.successRate}%</div>
          <div className="kpi-label">Success Rate</div>
        </div>
      </div>
    </div>
  );
}

export default KPICard;