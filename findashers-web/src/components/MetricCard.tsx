import React from 'react';
import { Card, CardContent, Typography, Box } from '@mui/material';
import { TrendingUp, TrendingDown } from '@mui/icons-material';

interface MetricCardProps {
  title: string;
  value: string;
  change?: string;
  trend?: 'up' | 'down';
  variant?: 'primary' | 'secondary';
}

const MetricCard: React.FC<MetricCardProps> = ({
  title,
  value,
  change,
  trend,
  variant = 'secondary'
}) => {
  const isPrimary = variant === 'primary';

  return (
    <Card
      sx={{
        background: isPrimary
          ? 'linear-gradient(135deg, #2196F3 0%, #21CBF3 100%)'
          : '#fff',
        color: isPrimary ? '#fff' : '#333',
        borderRadius: 3,
        boxShadow: '0 4px 20px rgba(0,0,0,0.1)',
        height: '160px',
      }}
    >
      <CardContent sx={{ p: 3, height: '100%', display: 'flex', flexDirection: 'column', justifyContent: 'space-between' }}>
        <Typography
          variant="body2"
          sx={{
            opacity: isPrimary ? 0.9 : 0.7,
            fontSize: '14px',
            fontWeight: 500,
          }}
        >
          {title}
        </Typography>
        
        <Typography
          variant="h4"
          sx={{
            fontWeight: 700,
            fontSize: '2rem',
          }}
        >
          {value}
        </Typography>

        {change && (
          <Box sx={{ display: 'flex', alignItems: 'center', mt: 1 }}>
            {trend === 'up' ? (
              <TrendingUp sx={{ fontSize: 16, mr: 0.5, color: isPrimary ? '#fff' : '#4caf50' }} />
            ) : (
              <TrendingDown sx={{ fontSize: 16, mr: 0.5, color: isPrimary ? '#fff' : '#f44336' }} />
            )}
            <Typography
              variant="body2"
              sx={{
                fontSize: '12px',
                opacity: isPrimary ? 0.9 : 0.8,
                color: isPrimary ? '#fff' : (trend === 'up' ? '#4caf50' : '#f44336'),
              }}
            >
              {change}
            </Typography>
          </Box>
        )}
      </CardContent>
    </Card>
  );
};

export default MetricCard;