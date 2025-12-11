import React from 'react';
import {
  Paper,
  Typography,
  Box,
  Alert,
  Divider,
  Chip,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  styled
} from '@mui/material';
import { Code as CodeIcon, CheckCircle as SuccessIcon, Error as ErrorIcon, TableView as TableIcon } from '@mui/icons-material';
import { NL2SQLQueryResponse } from '../services/nl2sqlService';

interface QueryResultsProps {
  result: NL2SQLQueryResponse | null;
}

const StyledTableCell = styled(TableCell)(({ theme }) => ({
  fontSize: '0.875rem',
  padding: theme.spacing(1),
}));

const QueryResults: React.FC<QueryResultsProps> = ({ result }) => {
  if (!result) return null;

  if (!result.success) {
    return (
      <Alert severity="error" sx={{ mt: 3 }}>
        {result.error || 'An error occurred while processing the query'}
      </Alert>
    );
  }

  if (!result.data || result.data.length === 0) {
    return null;
  }

  // Fields to hide
  const hiddenFields = [
    'merchant_reference',
    'merchant_order_reference', 
    'amount_currency',
    'employee_id',
    'amount_value',
    'subtotal_amount',
    'tax_amount',
    'tender_reference',
    'created_at'
  ];

  // Priority fields to show first
  const priorityFields = ['company_id', 'location_id', 'check_id', 'psp_reference'];

  // Filter out hidden fields and reorder
  const visibleColumns = result.columnNames.filter(column => 
    !hiddenFields.includes(column.toLowerCase())
  );

  // Separate priority and other fields
  const priorityColumns = priorityFields.filter(field => 
    visibleColumns.some(col => col.toLowerCase() === field)
  );
  const otherColumns = visibleColumns.filter(column => 
    !priorityFields.includes(column.toLowerCase())
  );

  // Final column order: priority fields first, then others
  const orderedColumns = [...priorityColumns, ...otherColumns];

  return (
    <TableContainer 
      component={Paper} 
      sx={{ 
        mt: 3, 
        maxHeight: 400, 
        border: '1px solid #e9ecef',
        borderRadius: 3,
        boxShadow: '0 4px 6px rgba(0, 0, 0, 0.1)'
      }}
    >
      <Table stickyHeader size="small">
        <TableHead>
          <TableRow>
            {orderedColumns.map((column) => (
              <TableCell 
                key={column}
                sx={{ 
                  fontWeight: 600, 
                  backgroundColor: '#f8f9fa',
                  textTransform: 'capitalize'
                }}
              >
                {column.replace(/_/g, ' ')}
              </TableCell>
            ))}
          </TableRow>
        </TableHead>
        <TableBody>
          {result.data.map((row, index) => (
            <TableRow key={index} hover>
              {orderedColumns.map((column) => (
                <TableCell key={`${index}-${column}`}>
                  {typeof row[column] === 'number' && column.includes('amount') 
                    ? `$${row[column].toFixed(2)}` 
                    : String(row[column] || '')}
                </TableCell>
              ))}
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
};

export default QueryResults;
