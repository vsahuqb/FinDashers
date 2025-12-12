import React from 'react';
import Plot from 'react-plotly.js';
import { useData } from '../../context/DataContext';

function GaugeChart() {
  const { dashboardData } = useData();
  const { kpi } = dashboardData;

  const data = [{
    type: 'indicator',
    mode: 'gauge+number',
    value: kpi.successRate,
    number: { suffix: '%' },
    gauge: {
      axis: { range: [0, 100] },
      bar: { color: '#4facfe' },
      steps: [
        { range: [0, 50], color: '#ffe0e0' },
        { range: [50, 80], color: '#fff4d1' },
        { range: [80, 100], color: '#d1f7e0' }
      ]
    },
    title: { text: 'Success Rate' }
  }];

  const layout = {
    margin: { t: 50, b: 20, l: 20, r: 20 },
    height: 250
  };

  return <Plot data={data} layout={layout} style={{width: '100%', height: '100%'}} />;
}

export default GaugeChart;