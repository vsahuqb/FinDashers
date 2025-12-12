import React from 'react';
import { motion } from 'framer-motion';
import { animations } from '../../utils/animations';

const FadeIn = ({ 
  children, 
  delay = 0, 
  duration = 0.3, 
  direction = 'up',
  className = '',
  ...props 
}) => {
  const getVariant = () => {
    switch (direction) {
      case 'up': return animations.fadeInUp;
      case 'left': return animations.slideInLeft;
      case 'right': return animations.slideInRight;
      default: return animations.fadeIn;
    }
  };

  const variant = getVariant();
  const customTransition = {
    ...variant.transition,
    duration,
    delay
  };

  return (
    <motion.div
      initial={variant.initial}
      animate={variant.animate}
      exit={variant.exit}
      transition={customTransition}
      className={className}
      {...props}
    >
      {children}
    </motion.div>
  );
};

export default FadeIn;