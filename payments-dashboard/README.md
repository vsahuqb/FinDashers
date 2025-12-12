# Payment Health Dashboard

A real-time payment analytics dashboard built with React and Plotly.js that provides comprehensive insights into payment performance, success rates, and risk metrics.

## Features

- **Real-time Data Updates** - WebSocket, SSE, and HTTP polling support
- **Interactive Charts** - Bar charts, gauges, heatmaps, funnels, sankey diagrams, and risk rings
- **Auto-reconnection** - Intelligent fallback with exponential backoff
- **Responsive Design** - Works on desktop and mobile devices
- **Glass Morphism UI** - Modern, elegant interface design

## Quick Start

### Prerequisites

- Node.js 16+ and npm
- Modern web browser (Chrome, Firefox, Safari, Edge)

### Installation

```bash
# Clone the repository
git clone <repository-url>
cd payments-dashboard

# Install dependencies
npm install

# Start development server
npm start
```

The dashboard will open at `http://localhost:3000`

## Configuration

### Environment Variables

Create a `.env` file in the root directory:

```env
# WebSocket endpoint (optional)
REACT_APP_WS_URL=ws://localhost:8080/ws

# API endpoints (optional)
REACT_APP_API_URL=http://localhost:8080/api
REACT_APP_SSE_URL=http://localhost:8080/events
```

### Default Endpoints

If no environment variables are set, the dashboard uses:
- WebSocket: `ws://localhost:8080/ws`
- SSE: `http://localhost:8080/events`
- HTTP Polling: `http://localhost:8080/ws`

## Usage

### Dashboard Overview

The dashboard displays:

1. **KPI Card** - Overall payment health score and key metrics
2. **Payment Flow Analysis** - Transaction funnel and payment flow diagrams
3. **Risk & Health Metrics** - Risk components and success rate gauge
4. **Failure Analysis** - Hourly failure trends and heatmaps
5. **Payment Method Performance** - Transaction counts by payment method

### Connection Status

The connection status indicator in the header shows:
- 🟢 **Green** - Connected and receiving data
- 🟠 **Orange** - Connecting or reconnecting
- 🔴 **Red** - Disconnected

### Data Sources

The dashboard automatically tries multiple connection methods:
1. **WebSocket** (primary) - Real-time bidirectional communication
2. **Server-Sent Events** (fallback) - Real-time server-to-client updates
3. **HTTP Polling** (final fallback) - Periodic data fetching

## API Integration

### Expected Data Format

The dashboard expects JSON data in this format:

```json
{
  "paymentSuccessRate": {
    "dailySuccessRate": 87.3,
    "totalTransactions": 1247,
    "approvedCount": 1089,
    "declinedCount": 158,
    "funnelMetrics": {
      "initiated": 1247,
      "authorized": 1089,
      "captured": 1089,
      "submittedForSettlement": 1050,
      "cancelledOrRefunded": 39
    },
    "paymentMethodRates": [
      {
        "paymentMethod": "visa",
        "totalTransactions": 450,
        "successCount": 401,
        "failureCount": 49
      }
    ],
    "hourlyTrends": [
      {
        "hour": 0,
        "failureCount": 2,
        "totalTransactions": 20
      }
    ]
  },
  "paymentHealthHeatIndex": {
    "totalScore": 85,
    "unusualFailuresScore": 15,
    "settlementDelayScore": 8,
    "highRiskCardScore": 12,
    "refundSpikeScore": 5,
    "healthStatus": "Good"
  }
}
```

### WebSocket Server Example

```javascript
const WebSocket = require('ws');
const wss = new WebSocket.Server({ port: 8080 });

wss.on('connection', (ws) => {
  // Send data every 5 seconds
  const interval = setInterval(() => {
    ws.send(JSON.stringify(paymentData));
  }, 5000);
  
  ws.on('close', () => clearInterval(interval));
});
```

## Troubleshooting

### Connection Issues

**Problem**: Red connection indicator, no data updates
**Solutions**:
1. Check if your API server is running
2. Verify the WebSocket endpoint is accessible
3. Check browser console for error messages
4. Ensure firewall allows WebSocket connections

**Problem**: Dashboard shows "Reconnecting..."
**Solutions**:
1. Wait for automatic reconnection (up to 5 attempts)
2. Check network connectivity
3. Verify server is responding to requests

### Data Issues

**Problem**: Charts show "No Data"
**Solutions**:
1. Verify API is sending data in the correct format
2. Check browser console for data processing errors
3. Ensure all required fields are present in the API response

**Problem**: Charts not updating
**Solutions**:
1. Check connection status indicator
2. Verify WebSocket/SSE messages are being sent
3. Check browser console for JavaScript errors

### Performance Issues

**Problem**: Dashboard is slow or unresponsive
**Solutions**:
1. Reduce data update frequency
2. Check for memory leaks in browser dev tools
3. Ensure data payloads aren't too large

## Development

### Project Structure

```
src/
├── components/          # React components
│   ├── charts/         # Chart components
│   ├── Header.jsx      # Main header with status
│   ├── KPICard.jsx     # Key metrics display
│   └── ...
├── context/            # React Context for state
├── hooks/              # Custom React hooks
├── utils/              # Utility functions
└── styles/             # CSS files
```

### Adding New Charts

1. Create component in `src/components/charts/`
2. Connect to DataContext using `useData()` hook
3. Use chart helpers from `src/utils/chartHelpers.js`
4. Add to main App.js

### Customizing Styles

- Global styles: `src/styles/globals.css`
- Component styles: Individual `.css` files
- Theme colors and gradients defined in CSS variables

## Deployment

### Build for Production

```bash
npm run build
```

### Deploy to Netlify

```bash
# Install Netlify CLI
npm install -g netlify-cli

# Deploy
netlify deploy --prod --dir=build
```

### Deploy to Vercel

```bash
# Install Vercel CLI
npm install -g vercel

# Deploy
vercel --prod
```

## Sample Data

See `docs/sample-data/` directory for example API responses and test data files.

## Support

For issues and questions:
1. Check this README and troubleshooting guide
2. Review browser console for error messages
3. Verify API server is running and accessible
4. Check network connectivity and firewall settings

## License

MIT License - see LICENSE file for details.