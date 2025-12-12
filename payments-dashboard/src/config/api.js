export const API_CONFIG = {
  baseURL: "https://api.example.com",
  endpoints: {
    paymentHealth: "/api/v1/payments/health",
    paymentMetrics: "/api/v1/payments/metrics",
    paymentAnalytics: "/api/v1/payments/analytics"
  },
  timeout: 5000,
  retryAttempts: 3
};