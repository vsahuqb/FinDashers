import React from 'react';
import Plot from 'react-plotly.js';
import { useData } from '../../context/DataContext';

function SankeyChart() {
  const { dashboardData } = useData();
  const { sankeyData } = dashboardData;

  const data = [{
    type: 'sankey',
    orientation: 'h',
    node: {
      label: sankeyData.nodes,
      pad: 15,
      thickness: 18,
      color: ['#dff7ff', '#bfe8ff', '#cfe7ff', '#e8f7ff', '#fff3d9', '#ffdede']
    },
    link: {
      source: sankeyData.links.source,
      target: sankeyData.links.target,
      value: sankeyData.links.value
    }
  }];

  const layout = {
    title: 'Payment Flow',
    margin: { t: 40, l: 20, r: 20, b: 20 },
    height: 250
  };

  return <Plot data={data} layout={layout} style={{width: '100%', height: '100%'}} />;
}

export default SankeyChart;