# API Documentation

## Overview

The Payment Health Dashboard supports multiple connection methods for receiving real-time payment data:

- **WebSocket** (Primary) - Real-time bidirectional communication
- **Server-Sent Events** (Fallback) - Real-time server-to-client updates  
- **HTTP Polling** (Final Fallback) - Periodic data fetching

## Connection Endpoints

### WebSocket
```
ws://your-server:8080/ws
```

### Server-Sent Events
```
http://your-server:8080/events
```

### HTTP Polling
```
GET http://your-server:8080/ws
```

## Data Format

### Complete API Response

```json
{
  "paymentSuccessRate": {
    "dailySuccessRate": 87.3,
    "weeklySuccessRate": 85.2,
    "netSales": 125847.50,
    "avgTicket": 115.42,
    "approvedCount": 1089,
    "declinedCount": 158,
    "totalTransactions": 1247,
    "hourlyTrends": [...],
    "funnelMetrics": {...},
    "paymentMethodRates": [...],
    "terminalRates": [...]
  },
  "paymentHealthHeatIndex": {
    "totalScore": 85,
    "unusualFailuresScore": 15,
    "settlementDelayScore": 8,
    "highRiskCardScore": 12,
    "refundSpikeScore": 5,
    "healthStatus": "Good"
  },
  "generatedAt": "2025-12-11T12:53:08.9207492Z"
}
```

### Required Fields

#### paymentSuccessRate (object)
- `dailySuccessRate` (number) - Success rate percentage (0-100)
- `totalTransactions` (number) - Total transaction count
- `approvedCount` (number) - Number of approved transactions
- `declinedCount` (number) - Number of declined transactions

#### paymentHealthHeatIndex (object)
- `totalScore` (number) - Overall health score (0-100)
- `healthStatus` (string) - Status text ("Good", "Warning", "Critical")
- `unusualFailuresScore` (number) - Risk score (0-25)
- `settlementDelayScore` (number) - Risk score (0-25)
- `highRiskCardScore` (number) - Risk score (0-25)
- `refundSpikeScore` (number) - Risk score (0-25)

### Optional Fields

#### Funnel Metrics
```json
"funnelMetrics": {
  "initiated": 1247,
  "authorized": 1089,
  "captured": 1089,
  "submittedForSettlement": 1050,
  "cancelledOrRefunded": 39
}
```

#### Payment Method Rates
```json
"paymentMethodRates": [
  {
    "paymentMethod": "visa",
    "successRate": 89.2,
    "totalTransactions": 450,
    "successCount": 401,
    "failureCount": 49
  }
]
```

#### Hourly Trends
```json
"hourlyTrends": [
  {
    "hour": 0,
    "successRate": 95.0,
    "totalTransactions": 20,
    "successCount": 19,
    "failureCount": 1
  }
]
```

## Implementation Examples

### WebSocket Server (Node.js)

```javascript
const WebSocket = require('ws');
const wss = new WebSocket.Server({ port: 8080 });

wss.on('connection', (ws) => {
  console.log('Dashboard connected');
  
  // Send initial data
  ws.send(JSON.stringify(getPaymentData()));
  
  // Send updates every 5 seconds
  const interval = setInterval(() => {
    if (ws.readyState === WebSocket.OPEN) {
      ws.send(JSON.stringify(getPaymentData()));
    }
  }, 5000);
  
  ws.on('close', () => {
    clearInterval(interval);
  });
});
```

### Server-Sent Events (Express.js)

```javascript
const express = require('express');
const app = express();

app.get('/events', (req, res) => {
  res.writeHead(200, {
    'Content-Type': 'text/event-stream',
    'Cache-Control': 'no-cache',
    'Connection': 'keep-alive',
    'Access-Control-Allow-Origin': '*'
  });
  
  // Send data every 5 seconds
  const interval = setInterval(() => {
    const data = JSON.stringify(getPaymentData());
    res.write(`data: ${data}\n\n`);
  }, 5000);
  
  req.on('close', () => {
    clearInterval(interval);
  });
});
```

### HTTP Polling (Express.js)

```javascript
app.get('/ws', (req, res) => {
  res.json(getPaymentData());
});
```

## Data Processing

The dashboard automatically processes incoming data:

1. **Validation** - Checks required fields are present
2. **Transformation** - Converts API format to internal format
3. **Chart Updates** - Updates all charts with new data
4. **Error Handling** - Gracefully handles malformed data

### Payment Method Mapping

The dashboard maps payment method codes to display names:

```javascript
{
  'visa': 'Visa',
  'mc': 'Mastercard', 
  'amex': 'American Express',
  'discover': 'Discover',
  'paypal': 'PayPal'
}
```

### Health Status Mapping

Health status affects UI colors and indicators:

- `"Good"` - Green indicators
- `"Warning"` - Yellow indicators  
- `"Critical"` - Red indicators

## Error Handling

### Invalid Data Response

If data validation fails, the dashboard:
1. Logs error to browser console
2. Continues displaying previous valid data
3. Shows error in connection status
4. Attempts reconnection if connection drops

### Connection Failures

The dashboard implements automatic fallback:
1. WebSocket fails → Try SSE after 10 seconds
2. SSE fails → Try HTTP polling after 5 seconds
3. All methods fail → Show disconnected status

## Testing

### Mock Data Server

Use the provided WebSocket server example:

```bash
cd docs/sample-data
npm install ws
node websocket-server-example.js
```

### Manual Testing

Test endpoints with curl:

```bash
# Test HTTP endpoint
curl http://localhost:8080/ws

# Test SSE endpoint  
curl -N http://localhost:8080/events
```

### Browser Testing

Use browser dev tools:
1. Network tab → Check API requests
2. Console tab → Check for errors
3. WebSocket tab → Monitor WebSocket messages

## Security Considerations

### CORS Configuration

Configure your server to allow dashboard origin:

```javascript
app.use(cors({
  origin: ['http://localhost:3000', 'https://your-dashboard.com'],
  credentials: true
}));
```

### Authentication

For production deployments, implement authentication:

```javascript
// WebSocket authentication example
wss.on('connection', (ws, req) => {
  const token = req.headers.authorization;
  if (!validateToken(token)) {
    ws.close(1008, 'Unauthorized');
    return;
  }
  // ... handle connection
});
```

### Rate Limiting

Implement rate limiting to prevent abuse:

```javascript
const rateLimit = require('express-rate-limit');

const limiter = rateLimit({
  windowMs: 15 * 60 * 1000, // 15 minutes
  max: 100 // limit each IP to 100 requests per windowMs
});

app.use('/ws', limiter);
```