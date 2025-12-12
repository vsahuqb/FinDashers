import React from 'react';
import Plot from 'react-plotly.js';
import { useData } from '../../context/DataContext';

function HeatmapChart() {
  const { dashboardData } = useData();
  const { hourlyFailures } = dashboardData;
  
  const hours = Array.from({length: 24}, (_, i) => i + ':00');

  const data = [{
    z: [hourlyFailures],
    x: hours,
    y: ['Failures'],
    type: 'heatmap',
    colorscale: 'YlOrRd'
  }];

  const layout = {
    title: 'Failures by Hour',
    margin: { t: 30, b: 40, l: 60, r: 20 },
    height: 250
  };

  return <Plot data={data} layout={layout} style={{width: '100%', height: '100%'}} />;
}

export default HeatmapChart;