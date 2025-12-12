# Design System Documentation

## 🎨 For UX Designers

This design system is built to be easily customizable. Here's how to modify the visual appearance:

### Quick Theme Changes

1. **Edit `theme-config.js`** - Change colors, spacing, typography
2. **Colors automatically update** throughout the entire app
3. **No CSS knowledge required** - just change the values

### Color Customization

```javascript
// In theme-config.js, change these values:
brand: {
  primary: '#your-brand-color',    // Main buttons, links, highlights
  secondary: '#your-secondary',    // Secondary elements
  accent: '#your-accent',          // Special highlights
},

semantic: {
  success: '#your-success-color',  // Success messages, positive states
  warning: '#your-warning-color',  // Warnings, caution states
  error: '#your-error-color',      // Errors, negative states
}
```

### Surface Colors (Backgrounds)

```javascript
surfaces: {
  background: '#page-background',   // Main page background
  card: '#card-background',         // Cards, containers
  elevated: '#modal-background',    // Modals, dropdowns
}
```

### Typography

```javascript
typography: {
  fontFamily: "'Your-Font', sans-serif",
  sizes: {
    base: '16px',    // Base text size
    lg: '18px',      // Large text
    xl: '20px',      // Extra large text
  }
}
```

## 🔧 For Developers

### Using Design Tokens

All components use CSS custom properties (variables) that map to the design tokens:

```css
/* Use semantic color names */
.my-component {
  background: var(--color-surface-primary);
  color: var(--color-text-primary);
  border: 1px solid var(--color-border-primary);
  padding: var(--spacing-4);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-base);
}
```

### Component Classes

Pre-built utility classes are available:

```jsx
// Typography
<h1 className="text-2xl font-semibold text-primary">Title</h1>
<p className="text-base text-secondary">Description</p>

// Buttons
<button className="btn btn-primary">Primary Action</button>
<button className="btn btn-secondary">Secondary Action</button>

// Cards
<div className="card">
  <div className="card-header">
    <h2 className="card-title">Card Title</h2>
  </div>
  <p>Card content</p>
</div>

// Layout
<div className="grid grid-2">
  <div className="card">Item 1</div>
  <div className="card">Item 2</div>
</div>
```

### Applying Custom Themes

```javascript
import { applyTheme } from './styles/theme-config';

// Apply default theme
applyTheme();

// Apply custom theme
const customTheme = {
  brand: { primary: '#ff6b6b' },
  // ... other overrides
};
applyTheme(customTheme);
```

## 📱 Responsive Design

The system includes responsive utilities:

```css
/* Mobile-first approach */
.grid-4 { grid-template-columns: 1fr; }

@media (min-width: 768px) {
  .grid-4 { grid-template-columns: repeat(2, 1fr); }
}

@media (min-width: 1024px) {
  .grid-4 { grid-template-columns: repeat(4, 1fr); }
}
```

## 🎯 Design Principles

1. **Consistency** - Use design tokens for all spacing, colors, typography
2. **Accessibility** - High contrast ratios, semantic HTML, keyboard navigation
3. **Flexibility** - Easy to customize without breaking the design
4. **Performance** - Minimal CSS, efficient selectors, optimized for speed

## 🚀 Migration from Old Styles

The old gradient-heavy styles are being gradually replaced:

- ✅ `design-tokens.css` - New token system
- ✅ `modern-globals.css` - Clean base styles  
- ✅ `components.css` - Reusable component styles
- ✅ `theme-config.js` - Easy customization
- 🔄 Individual component CSS files - Being updated to use tokens

## 📋 Checklist for New Components

When creating new components:

- [ ] Use design tokens instead of hardcoded values
- [ ] Follow the established naming conventions
- [ ] Include hover and focus states
- [ ] Test on mobile devices
- [ ] Ensure accessibility compliance
- [ ] Add to the component documentation