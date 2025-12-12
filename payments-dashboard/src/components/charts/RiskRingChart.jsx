import React from 'react';
import Plot from 'react-plotly.js';
import { useData } from '../../context/DataContext';

function RiskRingChart() {
  const { dashboardData } = useData();
  const { riskScores } = dashboardData;
  
  const labels = ['Unusual Failures', 'Settlement Delay', 'High-Risk Cards', 'Refund Spike'];
  const colors = ['#ff6b6b', '#ffa64d', '#ffec4d', '#7CFC00'];

  const data = riskScores.map((score, i) => {
    const holeOuter = 0.85 - i * 0.18;
    const holeInner = holeOuter - 0.18;
    return {
      type: 'pie',
      values: [score, 25 - score],
      labels: [labels[i], ''],
      marker: { colors: [colors[i], 'rgba(0,0,0,0)'] },
      hole: holeInner,
      direction: 'clockwise',
      sort: false,
      textinfo: 'none',
      hoverinfo: 'label+value',
      showlegend: i === 0
    };
  });

  const layout = {
    title: 'Risk Components',
    margin: { t: 40, b: 20, l: 20, r: 20 },
    height: 250
  };

  return <Plot data={data} layout={layout} style={{width: '100%', height: '100%'}} />;
}

export default RiskRingChart;