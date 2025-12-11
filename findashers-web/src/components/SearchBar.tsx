import React, { useState } from 'react';
import { 
  TextField, 
  Button, 
  Box, 
  Paper, 
  InputAdornment, 
  CircularProgress 
} from '@mui/material';
import { Search as SearchIcon } from '@mui/icons-material';

interface SearchBarProps {
  onSearch: (query: string) => void;
  loading?: boolean;
  placeholder?: string;
}

const SearchBar: React.FC<SearchBarProps> = ({ 
  onSearch, 
  loading = false, 
  placeholder = "Ask a question about your payment data..." 
}) => {
  const [query, setQuery] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (query.trim() && !loading) {
      onSearch(query.trim());
    }
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSubmit(e as any);
    }
  };

  return (
    <Box component="form" onSubmit={handleSubmit} sx={{ width: '100%' }}>
      <Box sx={{ position: 'relative' }}>
        <TextField
          fullWidth
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          onKeyPress={handleKeyPress}
          placeholder={placeholder}
          disabled={loading}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon sx={{ color: '#666', fontSize: 24 }} />
              </InputAdornment>
            ),
            endAdornment: (
              <InputAdornment position="end">
                {loading ? (
                  <CircularProgress size={24} sx={{ color: '#666' }} />
                ) : (
                  <Button
                    type="submit"
                    disabled={!query.trim() || loading}
                    sx={{
                      minWidth: 'auto',
                      p: 1,
                      borderRadius: '8px',
                      backgroundColor: query.trim() ? '#2196F3' : 'transparent',
                      color: query.trim() ? 'white' : '#666',
                      '&:hover': {
                        backgroundColor: query.trim() ? '#1976D2' : '#f5f5f5',
                      },
                      '&:disabled': {
                        backgroundColor: 'transparent',
                        color: '#ccc'
                      }
                    }}
                  >
                    <SearchIcon fontSize="small" />
                  </Button>
                )}
              </InputAdornment>
            ),
            sx: {
              fontSize: '1.1rem',
              borderRadius: '16px',
              backgroundColor: 'white',
              border: '2px solid #e8e8e8',
              transition: 'all 0.2s ease-in-out',
              '&:hover': {
                border: '2px solid #ddd',
                boxShadow: '0 4px 20px rgba(0,0,0,0.08)'
              },
              '&.Mui-focused': {
                border: '2px solid #2196F3',
                boxShadow: '0 4px 20px rgba(33,150,243,0.15)'
              },
              '& .MuiOutlinedInput-notchedOutline': {
                border: 'none'
              },
              '& .MuiInputBase-input': {
                py: 2,
                fontSize: '1.1rem',
                fontWeight: 400,
                '&::placeholder': {
                  color: '#999',
                  opacity: 1
                }
              }
            }
          }}
        />
      </Box>
    </Box>
  );
};

export default SearchBar;
