// Chart data transformation helpers

// Bar Chart - Payment Methods
export const prepareBarChartData = (paymentMethods = []) => {
  if (!Array.isArray(paymentMethods) || paymentMethods.length === 0) {
    return {
      x: ['No Data'],
      y: [0],
      type: 'bar',
      marker: { color: '#cccccc' }
    };
  }

  return {
    x: paymentMethods.map(pm => pm.method || 'Unknown'),
    y: paymentMethods.map(pm => pm.count || 0),
    type: 'bar',
    marker: { color: '#4facfe' }
  };
};

// Gauge Chart - Success Rate
export const prepareGaugeData = (successRate = 0) => {
  const rate = Math.max(0, Math.min(100, successRate || 0));
  
  return {
    type: 'indicator',
    mode: 'gauge+number',
    value: rate,
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
  };
};

// Line Chart - Hourly Failures
export const prepareLineChartData = (hourlyFailures = []) => {
  const hours = Array.from({length: 24}, (_, i) => `${i}:00`);
  const failures = Array.isArray(hourlyFailures) && hourlyFailures.length === 24 
    ? hourlyFailures 
    : new Array(24).fill(0);

  return {
    x: hours,
    y: failures,
    type: 'scatter',
    mode: 'lines+markers',
    line: { color: '#764ba2', width: 2 },
    marker: { size: 6, color: '#667eea' },
    fill: 'tozeroy',
    fillcolor: 'rgba(102,126,234,0.1)'
  };
};

// Heatmap Chart - Hourly Failures
export const prepareHeatmapData = (hourlyFailures = []) => {
  const hours = Array.from({length: 24}, (_, i) => `${i}:00`);
  const failures = Array.isArray(hourlyFailures) && hourlyFailures.length === 24 
    ? hourlyFailures 
    : new Array(24).fill(0);

  return {
    z: [failures],
    x: hours,
    y: ['Failures'],
    type: 'heatmap',
    colorscale: 'YlOrRd'
  };
};

// Funnel Chart - Transaction Stages
export const prepareFunnelData = (funnelData = {}) => {
  const { stages = [], values = [] } = funnelData;
  
  if (!Array.isArray(stages) || !Array.isArray(values) || stages.length === 0) {
    return {
      type: 'funnel',
      y: ['No Data'],
      x: [0],
      marker: { color: ['#cccccc'] }
    };
  }

  return {
    type: 'funnel',
    y: stages,
    x: values.map(v => Math.max(0, v || 0)),
    marker: {
      color: ['#4facfe', '#00f2fe', '#96ceb4', '#feca57', '#ff6b6b']
    }
  };
};

// Sankey Chart - Payment Flow
export const prepareSankeyData = (sankeyData = {}) => {
  const { nodes = [], links = {} } = sankeyData;
  const { source = [], target = [], value = [] } = links;

  if (!Array.isArray(nodes) || nodes.length === 0) {
    return {
      type: 'sankey',
      orientation: 'h',
      node: {
        label: ['No Data'],
        pad: 15,
        thickness: 18,
        color: ['#cccccc']
      },
      link: {
        source: [],
        target: [],
        value: []
      }
    };
  }

  return {
    type: 'sankey',
    orientation: 'h',
    node: {
      label: nodes,
      pad: 15,
      thickness: 18,
      color: ['#dff7ff', '#bfe8ff', '#cfe7ff', '#e8f7ff', '#fff3d9', '#ffdede']
    },
    link: {
      source: source,
      target: target,
      value: value.map(v => Math.max(0, v || 0))
    }
  };
};

// Risk Ring Chart - Risk Scores
export const prepareRiskRingData = (riskScores = []) => {
  const labels = ['Unusual Failures', 'Settlement Delay', 'High-Risk Cards', 'Refund Spike'];
  const colors = ['#ff6b6b', '#ffa64d', '#ffec4d', '#7CFC00'];
  const scores = Array.isArray(riskScores) && riskScores.length === 4 
    ? riskScores 
    : [0, 0, 0, 0];

  return scores.map((score, i) => {
    const normalizedScore = Math.max(0, Math.min(25, score || 0));
    const holeOuter = 0.85 - i * 0.18;
    const holeInner = holeOuter - 0.18;
    
    return {
      type: 'pie',
      values: [normalizedScore, 25 - normalizedScore],
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
};

// Common layout helpers
export const getDefaultLayout = (title, height = 250) => ({
  title,
  margin: { t: 40, l: 60, r: 20, b: 60 },
  height
});

export const getCompactLayout = (title, height = 250) => ({
  title,
  margin: { t: 30, b: 40, l: 60, r: 20 },
  height
});