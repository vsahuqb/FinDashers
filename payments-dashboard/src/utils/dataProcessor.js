// Process real-time API data into dashboard format
export const processApiData = (apiResponse) => {
  if (!apiResponse || !apiResponse.paymentSuccessRate) {
    return null;
  }

  const { paymentSuccessRate, paymentHealthHeatIndex } = apiResponse;

  return {
    kpi: {
      healthScore: paymentHealthHeatIndex?.totalScore || 0,
      healthStatus: paymentHealthHeatIndex?.healthStatus || 'Unknown',
      totalTransactions: paymentSuccessRate.totalTransactions || 0,
      approvedCount: paymentSuccessRate.approvedCount || 0,
      declinedCount: paymentSuccessRate.declinedCount || 0,
      successRate: paymentSuccessRate.dailySuccessRate || 0
    },
    paymentMethods: processPaymentMethods(paymentSuccessRate.paymentMethodRates || []),
    hourlyFailures: processHourlyFailures(paymentSuccessRate.hourlyTrends || []),
    riskScores: [
      paymentHealthHeatIndex?.unusualFailuresScore || 0,
      paymentHealthHeatIndex?.settlementDelayScore || 0,
      paymentHealthHeatIndex?.highRiskCardScore || 0,
      paymentHealthHeatIndex?.refundSpikeScore || 0
    ],
    funnelData: processFunnelData(paymentSuccessRate.funnelMetrics),
    sankeyData: processSankeyData(paymentSuccessRate)
  };
};

const processPaymentMethods = (paymentMethodRates) => {
  return paymentMethodRates.map(pm => ({
    method: formatPaymentMethod(pm.paymentMethod),
    count: pm.totalTransactions
  }));
};

const processHourlyFailures = (hourlyTrends) => {
  const failures = new Array(24).fill(0);
  hourlyTrends.forEach(trend => {
    if (trend.hour >= 0 && trend.hour < 24) {
      failures[trend.hour] = trend.failureCount;
    }
  });
  return failures;
};

const processFunnelData = (funnelMetrics) => {
  if (!funnelMetrics) {
    return {
      stages: ['Initiated', 'Authorized', 'Captured', 'Settled', 'Refunded'],
      values: [0, 0, 0, 0, 0]
    };
  }

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

const processSankeyData = (paymentSuccessRate) => {
  const total = paymentSuccessRate.totalTransactions || 0;
  const approved = paymentSuccessRate.approvedCount || 0;
  const declined = paymentSuccessRate.declinedCount || 0;
  const settled = paymentSuccessRate.funnelMetrics?.submittedForSettlement || 0;
  const cancelled = paymentSuccessRate.funnelMetrics?.cancelledOrRefunded || 0;

  return {
    nodes: ['Attempt', 'Authorized', 'Captured', 'Settled', 'Cancelled', 'Declined'],
    links: {
      source: [0, 0, 1, 1, 1],
      target: [1, 5, 2, 3, 4],
      value: [approved, declined, approved, settled, cancelled]
    }
  };
};

const formatPaymentMethod = (method) => {
  const methodMap = {
    'visa': 'Visa',
    'mc': 'Mastercard',
    'amex': 'American Express',
    'discover': 'Discover',
    'paypal': 'PayPal'
  };
  return methodMap[method] || method.charAt(0).toUpperCase() + method.slice(1);
};

// Validate API response structure
export const validateApiResponse = (data) => {
  if (!data || typeof data !== 'object') {
    return false;
  }

  return !!(data.paymentSuccessRate && data.paymentHealthHeatIndex);
};