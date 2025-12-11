import React, { useState } from 'react';
import {
  Box,
  Typography,
} from '@mui/material';
import SearchBar from './SearchBar';
import QueryResults from './QueryResults';
import { nl2sqlService, NL2SQLQueryResponse } from '../services/nl2sqlService';
import qubeyondLogo from '../assets/images/qubeyond-logo.png';

const HomePage: React.FC = () => {
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState<NL2SQLQueryResponse | null>(null);
  const [error, setError] = useState<string | null>(null);



  const handleSearch = async (query: string) => {
    setLoading(true);
    setError(null);
    setResult(null);

    try {
      const response = await nl2sqlService.processQuery(query);
      setResult(response);
    } catch (err) {
      console.error('Search error:', err);
      setError('Failed to process your query. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box sx={{ 
      minHeight: '100vh', 
      backgroundColor: '#fafafa',
    }}>
      {/* Header */}
      <Box sx={{ 
        display: 'flex', 
        alignItems: 'center', 
        p: 3,
        borderBottom: '1px solid #e8e8e8',
        backgroundColor: 'white'
      }}>
        <Box
          component="img"
          src={qubeyondLogo}
          alt="QuBeyond Logo"
          sx={{
            width: 32,
            height: 32,
            mr: 2,
          }}
        />
        <Typography 
          variant="h5" 
          sx={{ 
            fontWeight: 600,
            color: '#333',
            letterSpacing: '-0.01em'
          }}
        >
          QuPay
        </Typography>
      </Box>

      {/* Main Content */}
      <Box sx={{ 
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        p: 3,
        minHeight: 'calc(100vh - 80px)'
      }}>
        <Box sx={{ width: '100%', maxWidth: 800 }}>
          <SearchBar 
            onSearch={handleSearch} 
            loading={loading}
            placeholder="Ask about your payment data..."
          />
          
          {/* Query Results */}
          {result && (
            <Box sx={{ mt: 4 }}>
              <QueryResults result={result} />
            </Box>
          )}
        </Box>
      </Box>
    </Box>
  );
};

export default HomePage;
