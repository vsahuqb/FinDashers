import React from 'react';
import Plot from 'react-plotly.js';
import { useData } from '../../context/DataContext';

function FunnelChart() {
  const { dashboardData } = useData();
  const { funnelData } = dashboardData;

  const data = [{
    type: 'funnel',
    y: funnelData.stages,
    x: funnelData.values,
    marker: {
      color: ['#4facfe', '#00f2fe', '#96ceb4', '#feca57', '#ff6b6b']
    }
  }];

  const layout = {
    title: 'Transaction Funnel',
    margin: { t: 40, l: 80, r: 20, b: 40 },
    height: 250
  };

  return <Plot data={data} layout={layout} style={{width: '100%', height: '100%'}} />;
}

export default FunnelChart;