import React from 'react';
import Plot from 'react-plotly.js';
import { useData } from '../../context/DataContext';

function LineChart() {
  const { dashboardData } = useData();
  const { hourlyFailures } = dashboardData;
  
  const hours = Array.from({length: 24}, (_, i) => i + ':00');

  const data = [{
    x: hours,
    y: hourlyFailures,
    type: 'scatter',
    mode: 'lines+markers',
    line: { color: '#764ba2', width: 2 },
    marker: { size: 6, color: '#667eea' },
    fill: 'tozeroy',
    fillcolor: 'rgba(102,126,234,0.1)'
  }];

  const layout = {
    title: 'Hourly Failure Trend',
    xaxis: { title: 'Hour' },
    yaxis: { title: 'Failures' },
    margin: { t: 50, b: 50, l: 50, r: 20 },
    height: 250
  };

  return <Plot data={data} layout={layout} style={{width: '100%', height: '100%'}} />;
}

export default LineChart;