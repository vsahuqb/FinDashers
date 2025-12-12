// Current dashboard format (for immediate use)
export const currentDashboardData = {
  kpi: {
    healthScore: 85,
    healthStatus: 'Good',
    totalTransactions: 1247,
    approvedCount: 1089,
    declinedCount: 158,
    successRate: 87.3
  },
  paymentMethods: [
    { method: 'Credit Card', count: 45 },
    { method: 'Debit Card', count: 32 },
    { method: 'PayPal', count: 18 },
    { method: 'Bank Transfer', count: 12 }
  ],
  hourlyFailures: [2, 1, 1, 0, 1, 2, 4, 6, 8, 10, 12, 15, 12, 10, 8, 6, 5, 4, 3, 2, 2, 1, 1, 1],
  riskScores: [15, 8, 12, 5],
  funnelData: {
    stages: ['Initiated', 'Authorized', 'Captured', 'Settled', 'Refunded'],
    values: [1247, 1089, 1089, 1050, 39]
  },
  sankeyData: {
    nodes: ['Attempt', 'Authorized', 'Captured', 'Settled', 'Cancelled', 'Declined'],
    links: {
      source: [0, 0, 1, 1, 1],
      target: [1, 5, 2, 3, 4],
      value: [1089, 158, 1089, 1050, 39]
    }
  }
};

// Future API response format (matches your API structure)
export const apiResponseSample = {
  paymentSuccessRate: {
    dailySuccessRate: 87.3,
    weeklySuccessRate: 85.2,
    netSales: 125847.50,
    avgTicket: 115.42,
    approvedCount: 1089,
    declinedCount: 158,
    totalTransactions: 1247,
    hourlyTrends: [
      { hour: 0, successRate: 95.0, totalTransactions: 20, successCount: 19, failureCount: 1 },
      { hour: 1, successRate: 96.7, totalTransactions: 15, successCount: 14, failureCount: 1 },
      { hour: 12, successRate: 82.5, totalTransactions: 120, successCount: 99, failureCount: 21 },
      { hour: 18, successRate: 78.9, totalTransactions: 95, successCount: 75, failureCount: 20 }
    ],
    funnelMetrics: {
      initiated: 1247,
      authorized: 1089,
      captured: 1089,
      submittedForSettlement: 1050,
      cancelledOrRefunded: 39
    },
    paymentMethodRates: [
      { paymentMethod: 'visa', successRate: 89.2, totalTransactions: 450, successCount: 401, failureCount: 49 },
      { paymentMethod: 'mc', successRate: 85.1, totalTransactions: 320, successCount: 272, failureCount: 48 },
      { paymentMethod: 'amex', successRate: 91.5, totalTransactions: 180, successCount: 165, failureCount: 15 }
    ],
    terminalRates: [
      { terminalId: '43951', successRate: 87.3, totalTransactions: 425, successCount: 371, failureCount: 54 }
    ]
  },
  paymentHealthHeatIndex: {
    totalScore: 85,
    unusualFailuresScore: 15,
    settlementDelayScore: 8,
    highRiskCardScore: 12,
    refundSpikeScore: 5,
    healthStatus: 'Good'
  },
  generatedAt: '2025-12-11T12:53:08.9207492Z'
};