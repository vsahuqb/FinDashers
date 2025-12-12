import React, { useState, useEffect } from 'react';
import './FilterBar.css';

const FilterBar = ({ onFiltersChange, initialFilters = {} }) => {
  const [filters, setFilters] = useState({
    timeRange: '24h',
    paymentMethod: 'all',
    status: 'all',
    ...initialFilters
  });

  const timeRanges = [
    { value: '1h', label: '1 Hour' },
    { value: '24h', label: '24 Hours' },
    { value: '7d', label: '7 Days' },
    { value: '30d', label: '30 Days' }
  ];

  const paymentMethods = [
    { value: 'all', label: 'All Methods' },
    { value: 'credit_card', label: 'Credit Card' },
    { value: 'debit_card', label: 'Debit Card' },
    { value: 'paypal', label: 'PayPal' },
    { value: 'bank_transfer', label: 'Bank Transfer' },
    { value: 'crypto', label: 'Cryptocurrency' }
  ];

  const statusOptions = [
    { value: 'all', label: 'All Status' },
    { value: 'success', label: 'Success' },
    { value: 'failed', label: 'Failed' },
    { value: 'pending', label: 'Pending' }
  ];

  useEffect(() => {
    onFiltersChange?.(filters);
  }, [filters, onFiltersChange]);

  const handleFilterChange = (filterType, value) => {
    setFilters(prev => ({
      ...prev,
      [filterType]: value
    }));
  };

  const resetFilters = () => {
    const defaultFilters = {
      timeRange: '24h',
      paymentMethod: 'all',
      status: 'all'
    };
    setFilters(defaultFilters);
  };

  return (
    <div className="filter-bar">
      <div className="filter-group">
        <label className="filter-label">Time Range</label>
        <select 
          className="filter-select"
          value={filters.timeRange}
          onChange={(e) => handleFilterChange('timeRange', e.target.value)}
        >
          {timeRanges.map(range => (
            <option key={range.value} value={range.value}>
              {range.label}
            </option>
          ))}
        </select>
      </div>

      <div className="filter-group">
        <label className="filter-label">Payment Method</label>
        <select 
          className="filter-select"
          value={filters.paymentMethod}
          onChange={(e) => handleFilterChange('paymentMethod', e.target.value)}
        >
          {paymentMethods.map(method => (
            <option key={method.value} value={method.value}>
              {method.label}
            </option>
          ))}
        </select>
      </div>

      <div className="filter-group">
        <label className="filter-label">Status</label>
        <select 
          className="filter-select"
          value={filters.status}
          onChange={(e) => handleFilterChange('status', e.target.value)}
        >
          {statusOptions.map(status => (
            <option key={status.value} value={status.value}>
              {status.label}
            </option>
          ))}
        </select>
      </div>

      <button className="reset-filters-btn" onClick={resetFilters}>
        Reset
      </button>
    </div>
  );
};

export default FilterBar;