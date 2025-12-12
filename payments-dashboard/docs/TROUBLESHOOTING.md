# Troubleshooting Guide

## Connection Issues

### Red Connection Indicator

**Symptoms**: Red dot in header, no data updates, charts show sample data

**Causes & Solutions**:

1. **API Server Not Running**
   - Start your WebSocket/API server
   - Verify server is listening on the correct port
   - Check server logs for errors

2. **Incorrect Endpoint Configuration**
   - Check `.env` file for correct `REACT_APP_WS_URL`
   - Verify WebSocket URL format: `ws://localhost:8080/ws`
   - Ensure no trailing slashes or extra characters

3. **Firewall/Network Issues**
   - Check if firewall blocks WebSocket connections
   - Try HTTP polling fallback (should happen automatically)
   - Test with `curl` or browser dev tools

4. **CORS Issues**
   - Configure server to allow cross-origin requests
   - Add appropriate CORS headers to your API server

### Constant Reconnecting

**Symptoms**: Orange dot, "Reconnecting..." message, frequent connection attempts

**Causes & Solutions**:

1. **Unstable Network Connection**
   - Check internet connectivity
   - Try switching networks (WiFi to mobile hotspot)
   - Wait for automatic reconnection (up to 5 attempts)

2. **Server Overload**
   - Check server resources (CPU, memory)
   - Reduce update frequency on server side
   - Implement rate limiting

3. **WebSocket Server Issues**
   - Check server logs for connection errors
   - Verify WebSocket implementation handles connections properly
   - Test with simple WebSocket client

## Data Issues

### Charts Show "No Data"

**Symptoms**: Empty charts with "No Data" placeholder

**Causes & Solutions**:

1. **Invalid Data Format**
   - Check browser console for data processing errors
   - Verify API response matches expected format (see README)
   - Use sample data file to test format

2. **Missing Required Fields**
   - Ensure all required fields are present in API response
   - Check `paymentSuccessRate` and `paymentHealthHeatIndex` objects
   - Verify nested objects have correct structure

3. **Data Processing Errors**
   - Open browser dev tools → Console tab
   - Look for JavaScript errors during data processing
   - Check network tab for failed API requests

### Charts Not Updating

**Symptoms**: Charts display initial data but don't update with new data

**Causes & Solutions**:

1. **Connection Issues**
   - Check connection status indicator
   - Verify WebSocket messages are being sent from server
   - Test with browser dev tools → Network → WS tab

2. **Data Validation Failures**
   - Check console for validation errors
   - Ensure new data has same structure as initial data
   - Verify all required fields are present in updates

3. **React State Issues**
   - Refresh the page to reset state
   - Check for JavaScript errors in console
   - Verify React components are re-rendering

## Performance Issues

### Slow Dashboard Performance

**Symptoms**: Laggy interactions, slow chart updates, high CPU usage

**Causes & Solutions**:

1. **Too Frequent Updates**
   - Reduce server update frequency (recommended: 5-10 seconds)
   - Implement data throttling on client side
   - Use smaller data payloads

2. **Large Data Payloads**
   - Reduce amount of data sent per update
   - Send only changed data, not full dataset
   - Implement data compression

3. **Memory Leaks**
   - Check browser dev tools → Memory tab
   - Look for increasing memory usage over time
   - Refresh page periodically for long-running sessions

### Browser Crashes or Freezes

**Symptoms**: Browser becomes unresponsive, tab crashes

**Causes & Solutions**:

1. **Memory Issues**
   - Close other browser tabs
   - Restart browser
   - Check available system memory

2. **JavaScript Errors**
   - Check console for unhandled errors
   - Look for infinite loops or recursive calls
   - Update to latest browser version

## Development Issues

### Build Errors

**Common Errors & Solutions**:

```bash
# Module not found errors
npm install

# Dependency conflicts
rm -rf node_modules package-lock.json
npm install

# TypeScript errors (if using TypeScript)
npm run type-check
```

### Hot Reload Not Working

**Solutions**:
1. Restart development server (`npm start`)
2. Clear browser cache
3. Check for file watcher limits on Linux/Mac

### Environment Variables Not Loading

**Solutions**:
1. Ensure `.env` file is in project root
2. Restart development server after changing `.env`
3. Verify variable names start with `REACT_APP_`

## Browser Compatibility

### Supported Browsers
- Chrome 80+
- Firefox 75+
- Safari 13+
- Edge 80+

### Known Issues
- **Internet Explorer**: Not supported (use modern browser)
- **Safari < 13**: WebSocket issues (use HTTP polling)
- **Mobile browsers**: Some chart interactions may be limited

## Getting Help

### Debug Information to Collect

When reporting issues, include:

1. **Browser Information**
   - Browser name and version
   - Operating system
   - Screen resolution

2. **Console Logs**
   - JavaScript errors from browser console
   - Network requests from dev tools
   - Server logs if available

3. **Configuration**
   - Environment variables (remove sensitive data)
   - API endpoint URLs
   - Sample data being sent

### Testing Steps

1. **Test with Sample Data**
   - Use provided WebSocket server example
   - Verify dashboard works with known good data

2. **Isolate the Issue**
   - Test in incognito/private browsing mode
   - Try different browsers
   - Test on different devices/networks

3. **Check Network**
   - Use browser dev tools → Network tab
   - Verify API requests are successful
   - Check WebSocket connection status

### Common Quick Fixes

1. **Hard Refresh**: Ctrl+F5 (Windows) or Cmd+Shift+R (Mac)
2. **Clear Cache**: Browser settings → Clear browsing data
3. **Restart Server**: Stop and restart your API server
4. **Check Ports**: Ensure no port conflicts (3000 for React, 8080 for API)
5. **Update Dependencies**: `npm update` to get latest versions