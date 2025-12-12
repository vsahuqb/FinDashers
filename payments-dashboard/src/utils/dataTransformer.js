// Transform FinDashers API response to payments-dashboard format
export const transformApiResponse = (apiResponse) => {
  if (!apiResponse) return null;

  const { paymentSuccessRate, paymentHealthHeatIndex } = apiResponse;

  return {
    kpi: {
      healthScore: paymentHealthHeatIndex?.overallScore || 0,
      healthStatus: paymentHealthHeatIndex?.healthStatus || 'Unknown',
      totalTransactions: paymentSuccessRate?.totalTransactions || 0,
      approvedCount: paymentSuccessRate?.approvedCount || 0,
      declinedCount: paymentSuccessRate?.declinedCount || 0,
      successRate: paymentSuccessRate?.totalTransactions > 0 
        ? (paymentSuccessRate.approvedCount / paymentSuccessRate.totalTransactions * 100).toFixed(1)
        : 0
    },
    paymentMethods: transformPaymentMethods(paymentSuccessRate?.paymentMethodRates || []),
    hourlyFailures: transformHourlyFailures(paymentSuccessRate?.hourlyTrends || []),
    riskScores: transformRiskScores(paymentHealthHeatIndex?.components || []),
    funnelData: transformFunnelData(paymentSuccessRate?.funnelMetrics),
    sankeyData: transformSankeyData(paymentSuccessRate?.funnelMetrics),
    statusCounts: paymentSuccessRate?.statusCounts || [],
    healthComponents: paymentHealthHeatIndex?.components || []
  };
};

const transformPaymentMethods = (paymentMethodRates) => {
  return paymentMethodRates.map(pm => ({
    method: formatPaymentMethod(pm.paymentMethod),
    count: pm.totalTransactions
  }));
};

const transformHourlyFailures = (hourlyTrends) => {
  const failures = new Array(24).fill(0);
  hourlyTrends.forEach(trend => {
    if (trend.hour >= 0 && trend.hour < 24) {
      failures[trend.hour] = trend.failureCount || 0;
    }
  });
  return failures;
};

const transformRiskScores = (components) => {
  return components.map(comp => comp.score || 0);
};

const transformFunnelData = (funnelMetrics) => {
  if (!funnelMetrics) return { stages: [], values: [] };
  
  return {
    stages: ['Initiated', 'Authorized', 'Captured', 'Settled', 'Refunded'],
    values: [
      funnelMetrics.initiated || 0,
      funnelMetrics.authorized || 0,
      funnelMetrics.captured || 0,
      funnelMetrics.submittedForSettlement || 0,
      funnelMetrics.cancelledOrRefunded || 0
    ]
  };
};

const transformSankeyData = (funnelMetrics) => {
  if (!funnelMetrics) return { nodes: [], links: { source: [], target: [], value: [] } };
  
  return {
    nodes: ['Attempt', 'Authorized', 'Captured', 'Settled', 'Cancelled', 'Declined'],
    links: {
      source: [0, 0, 1, 1, 1],
      target: [1, 5, 2, 3, 4],
      value: [
        funnelMetrics.authorized || 0,
        (funnelMetrics.initiated || 0) - (funnelMetrics.authorized || 0),
        funnelMetrics.captured || 0,
        funnelMetrics.submittedForSettlement || 0,
        funnelMetrics.cancelledOrRefunded || 0
      ]
    }
  };
};

const formatPaymentMethod = (method) => {
  const methodMap = {
    'visa': 'Visa',
    'mc': 'Mastercard',
    'amex': 'American Express',
    'paypal': 'PayPal',
    'applepay': 'Apple Pay',
    'googlepay': 'Google Pay'
  };
  return methodMap[method?.toLowerCase()] || method || 'Unknown';
};