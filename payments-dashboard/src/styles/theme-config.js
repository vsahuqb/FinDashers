// Theme Configuration - Easy customization for UX designers
export const themeConfig = {
  // === BRAND COLORS ===
  // Change these to match your brand identity
  brand: {
    primary: '#3b82f6',    // Main brand color
    secondary: '#6b7280',  // Secondary brand color
    accent: '#60a5fa',     // Accent color for highlights
  },

  // === SEMANTIC COLORS ===
  // Status and feedback colors
  semantic: {
    success: '#34d399',    // Green for success states
    warning: '#fbbf24',    // Orange for warnings
    error: '#f87171',      // Red for errors
    info: '#60a5fa',       // Blue for information
  },

  // === SURFACE COLORS ===
  // Background and container colors
  surfaces: {
    background: '#0f172a',  // Main page background
    card: '#1e293b',        // Card/container background
    elevated: '#334155',    // Elevated surface (modals, dropdowns)
  },

  // === TEXT COLORS ===
  // Text hierarchy colors
  text: {
    primary: '#f8fafc',     // Main text color
    secondary: '#cbd5e1',   // Secondary text
    tertiary: '#94a3b8',    // Muted text
    inverse: '#0f172a',     // Text on light backgrounds
  },

  // === BORDER COLORS ===
  borders: {
    light: '#334155',      // Light borders
    medium: '#475569',     // Medium borders
    focus: '#60a5fa',      // Focus state borders
  },

  // === SPACING SCALE ===
  // Consistent spacing throughout the app
  spacing: {
    xs: '4px',
    sm: '8px',
    md: '16px',
    lg: '24px',
    xl: '32px',
    xxl: '48px',
  },

  // === TYPOGRAPHY ===
  typography: {
    fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif",
    sizes: {
      xs: '12px',
      sm: '14px',
      base: '16px',
      lg: '18px',
      xl: '20px',
      '2xl': '24px',
      '3xl': '30px',
      '4xl': '36px',
    },
    weights: {
      normal: 400,
      medium: 500,
      semibold: 600,
      bold: 700,
    },
  },

  // === SHADOWS ===
  shadows: {
    sm: '0 1px 2px 0 rgb(0 0 0 / 0.3)',
    base: '0 1px 3px 0 rgb(0 0 0 / 0.4), 0 1px 2px -1px rgb(0 0 0 / 0.4)',
    md: '0 4px 6px -1px rgb(0 0 0 / 0.4), 0 2px 4px -2px rgb(0 0 0 / 0.4)',
    lg: '0 10px 15px -3px rgb(0 0 0 / 0.4), 0 4px 6px -4px rgb(0 0 0 / 0.4)',
  },

  // === BORDER RADIUS ===
  radius: {
    sm: '4px',
    base: '8px',
    md: '12px',
    lg: '16px',
    xl: '24px',
    full: '9999px',
  },
};

// Function to apply theme to CSS variables
export const applyTheme = (theme = themeConfig) => {
  const root = document.documentElement;
  
  // Apply brand colors
  root.style.setProperty('--color-primary-600', theme.brand.primary);
  root.style.setProperty('--color-neutral-500', theme.brand.secondary);
  root.style.setProperty('--color-primary-500', theme.brand.accent);
  
  // Apply semantic colors
  root.style.setProperty('--color-success-500', theme.semantic.success);
  root.style.setProperty('--color-warning-500', theme.semantic.warning);
  root.style.setProperty('--color-error-500', theme.semantic.error);
  
  // Apply surface colors
  root.style.setProperty('--color-surface-secondary', theme.surfaces.background);
  root.style.setProperty('--color-surface-primary', theme.surfaces.card);
  root.style.setProperty('--color-surface-tertiary', theme.surfaces.elevated);
  
  // Apply text colors
  root.style.setProperty('--color-text-primary', theme.text.primary);
  root.style.setProperty('--color-text-secondary', theme.text.secondary);
  root.style.setProperty('--color-text-tertiary', theme.text.tertiary);
  root.style.setProperty('--color-text-inverse', theme.text.inverse);
  
  // Apply border colors
  root.style.setProperty('--color-border-primary', theme.borders.light);
  root.style.setProperty('--color-border-secondary', theme.borders.medium);
  root.style.setProperty('--color-border-focus', theme.borders.focus);
};

export default themeConfig;