// Simple WebSocket server example for testing the dashboard
const WebSocket = require('ws');
const fs = require('fs');
const path = require('path');

// Load sample data
const sampleData = JSON.parse(
  fs.readFileSync(path.join(__dirname, 'sample-api-response.json'), 'utf8')
);

// Create WebSocket server
const wss = new WebSocket.Server({ port: 8080 });

console.log('WebSocket server running on ws://localhost:8080');

wss.on('connection', (ws) => {
  console.log('Client connected');

  // Send initial data
  ws.send(JSON.stringify(sampleData));

  // Send updated data every 5 seconds
  const interval = setInterval(() => {
    // Create variations of the sample data
    const updatedData = {
      ...sampleData,
      paymentSuccessRate: {
        ...sampleData.paymentSuccessRate,
        dailySuccessRate: 85 + Math.random() * 10, // 85-95%
        totalTransactions: 1200 + Math.floor(Math.random() * 100),
        approvedCount: Math.floor((1200 + Math.random() * 100) * 0.87),
        declinedCount: Math.floor((1200 + Math.random() * 100) * 0.13)
      },
      paymentHealthHeatIndex: {
        ...sampleData.paymentHealthHeatIndex,
        totalScore: 80 + Math.floor(Math.random() * 20),
        unusualFailuresScore: Math.floor(Math.random() * 20),
        settlementDelayScore: Math.floor(Math.random() * 15),
        highRiskCardScore: Math.floor(Math.random() * 18),
        refundSpikeScore: Math.floor(Math.random() * 10)
      },
      generatedAt: new Date().toISOString()
    };

    if (ws.readyState === WebSocket.OPEN) {
      ws.send(JSON.stringify(updatedData));
    }
  }, 5000);

  ws.on('close', () => {
    console.log('Client disconnected');
    clearInterval(interval);
  });

  ws.on('error', (error) => {
    console.error('WebSocket error:', error);
    clearInterval(interval);
  });
});

// Graceful shutdown
process.on('SIGINT', () => {
  console.log('\nShutting down WebSocket server...');
  wss.close(() => {
    console.log('WebSocket server closed');
    process.exit(0);
  });
});

// Usage:
// 1. npm install ws
// 2. node websocket-server-example.js
// 3. Open dashboard at http://localhost:3000