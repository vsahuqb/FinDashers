import React from 'react';
import {
  Drawer,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Box,
  Typography,
} from '@mui/material';
import qubeyondLogo from '../assets/images/qubeyond-logo.png';
import {
  Dashboard,
  Receipt,
  Devices,
  Store,
  Analytics,
  Settings
} from '@mui/icons-material';

const drawerWidth = 280;

interface SidebarProps {
  selectedItem: string;
  onItemSelect: (item: string) => void;
}

const Sidebar: React.FC<SidebarProps> = ({ selectedItem, onItemSelect }) => {
  const menuItems = [
    { id: 'dashboard', label: 'Dashboard', icon: <Dashboard /> },
    { id: 'transactions', label: 'Transactions', icon: <Receipt /> },
    { id: 'terminals', label: 'Terminals', icon: <Devices /> },
    { id: 'stores', label: 'Stores', icon: <Store /> },
    { id: 'analytics', label: 'Analytics', icon: <Analytics /> },
    { id: 'settings', label: 'Settings', icon: <Settings /> },
  ];

  return (
    <Drawer
      variant="permanent"
      sx={{
        width: drawerWidth,
        flexShrink: 0,
        '& .MuiDrawer-paper': {
          width: drawerWidth,
          boxSizing: 'border-box',
          backgroundColor: '#f8fafc',
          border: 'none',
          boxShadow: '2px 0 10px rgba(0,0,0,0.1)',
        },
      }}
    >
      <Box sx={{ p: 3 }}>
        {/* Logo */}
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 4 }}>
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
          <Typography variant="h6" fontWeight="600">
            FinDashers
          </Typography>
        </Box>

        {/* Navigation Menu */}
        <List sx={{ px: 0 }}>
          {menuItems.map((item) => (
            <ListItem
              key={item.id}
              onClick={() => onItemSelect(item.id)}
              sx={{
                mb: 1,
                borderRadius: 2,
                cursor: 'pointer',
                backgroundColor: selectedItem === item.id ? '#e3f2fd' : 'transparent',
                '&:hover': {
                  backgroundColor: selectedItem === item.id ? '#e3f2fd' : '#f0f0f0',
                },
              }}
            >
              <ListItemIcon
                sx={{
                  color: selectedItem === item.id ? '#2196F3' : '#666',
                  minWidth: 40,
                }}
              >
                {item.icon}
              </ListItemIcon>
              <ListItemText
                primary={item.label}
                sx={{
                  '& .MuiTypography-root': {
                    fontWeight: selectedItem === item.id ? 600 : 400,
                    color: selectedItem === item.id ? '#2196F3' : '#333',
                  },
                }}
              />
            </ListItem>
          ))}
        </List>
      </Box>
    </Drawer>
  );
};

export default Sidebar;