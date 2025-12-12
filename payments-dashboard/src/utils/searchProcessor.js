class SearchProcessor {
  parseQuery(query) {
    const lowerQuery = query.toLowerCase();
    
    // Detect chart type based on query patterns
    if (lowerQuery.includes('trend') || lowerQuery.includes('over time') || lowerQuery.includes('timeline')) {
      return { chartType: 'line', timeSeriesData: true };
    }
    
    if (lowerQuery.includes('by') || lowerQuery.includes('group') || lowerQuery.includes('breakdown')) {
      return { chartType: 'bar', groupedData: true };
    }
    
    if (lowerQuery.includes('rate') || lowerQuery.includes('percentage') || lowerQuery.includes('%')) {
      return { chartType: 'gauge', singleValue: true };
    }
    
    if (lowerQuery.includes('distribution') || lowerQuery.includes('spread')) {
      return { chartType: 'pie', distributionData: true };
    }
    
    return { chartType: 'table', rawData: true };
  }

  mapResultsToChartType(results, chartType) {
    if (!results || !Array.isArray(results)) return null;
    
    switch (chartType) {
      case 'line':
        return this.formatTimeSeriesData(results);
      case 'bar':
        return this.formatBarChartData(results);
      case 'gauge':
        return this.formatGaugeData(results);
      case 'pie':
        return this.formatPieChartData(results);
      default:
        return results;
    }
  }

  formatTimeSeriesData(results) {
    // Assume first column is date/time, second is value
    const keys = Object.keys(results[0] || {});
    if (keys.length < 2) return results;
    
    return results.map(row => ({
      x: row[keys[0]],
      y: parseFloat(row[keys[1]]) || 0
    }));
  }

  formatBarChartData(results) {
    // Assume first column is category, second is value
    const keys = Object.keys(results[0] || {});
    if (keys.length < 2) return results;
    
    return results.map(row => ({
      category: row[keys[0]],
      value: parseFloat(row[keys[1]]) || 0
    }));
  }

  formatGaugeData(results) {
    // Return single value for gauge
    if (results.length === 0) return 0;
    
    const firstRow = results[0];
    const values = Object.values(firstRow);
    return parseFloat(values[0]) || 0;
  }

  formatPieChartData(results) {
    // Similar to bar chart but for pie visualization
    const keys = Object.keys(results[0] || {});
    if (keys.length < 2) return results;
    
    return results.map(row => ({
      label: row[keys[0]],
      value: parseFloat(row[keys[1]]) || 0
    }));
  }

  formatValue(value) {
    if (value === null || value === undefined) return '-';
    
    if (typeof value === 'number') {
      if (value % 1 === 0) return value.toLocaleString();
      return value.toFixed(2);
    }
    
    if (typeof value === 'string' && value.match(/^\d{4}-\d{2}-\d{2}/)) {
      return new Date(value).toLocaleDateString();
    }
    
    return String(value);
  }

  exportToCSV(results) {
    if (!results || results.length === 0) return '';
    
    const headers = Object.keys(results[0]);
    const csvContent = [
      headers.join(','),
      ...results.map(row => 
        headers.map(header => {
          const value = row[header];
          return typeof value === 'string' && value.includes(',') 
            ? `"${value}"` 
            : value;
        }).join(',')
      )
    ].join('\n');
    
    return csvContent;
  }

  detectDataFormat(results) {
    if (!results || results.length === 0) return 'empty';
    
    const keys = Object.keys(results[0]);
    const firstRow = results[0];
    
    // Check for time series data
    const hasDateColumn = keys.some(key => 
      key.toLowerCase().includes('date') || 
      key.toLowerCase().includes('time') ||
      (typeof firstRow[key] === 'string' && firstRow[key].match(/^\d{4}-\d{2}-\d{2}/))
    );
    
    if (hasDateColumn) return 'timeseries';
    
    // Check for categorical data
    const hasStringColumn = keys.some(key => typeof firstRow[key] === 'string');
    const hasNumericColumn = keys.some(key => typeof firstRow[key] === 'number');
    
    if (hasStringColumn && hasNumericColumn) return 'categorical';
    
    // Check for single value
    if (results.length === 1 && keys.length === 1) return 'single_value';
    
    return 'tabular';
  }
}

export default new SearchProcessor();