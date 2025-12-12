import React from 'react';
import Plot from 'react-plotly.js';
import { useData } from '../../context/DataContext';

function BarChart() {
  const { dashboardData } = useData();
  const { paymentMethods } = dashboardData;

  const data = [{
    x: paymentMethods.map(pm => pm.method),
    y: paymentMethods.map(pm => pm.count),
    type: 'bar',
    marker: { color: '#4facfe' }
  }];

  const layout = {
    title: 'Payment Methods',
    margin: { t: 40, l: 60, r: 20, b: 60 },
    height: 250
  };

  return <Plot data={data} layout={layout} style={{width: '100%', height: '100%'}} />;
}

export default BarChart;