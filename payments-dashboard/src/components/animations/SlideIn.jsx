import React from 'react';
import { motion } from 'framer-motion';
import { animations } from '../../utils/animations';

const SlideIn = ({ 
  children, 
  direction = 'left', 
  delay = 0, 
  duration = 0.3,
  distance = 30,
  className = '',
  ...props 
}) => {
  const getVariant = (dir, dist) => {
    const variants = {
      left: {
        initial: { opacity: 0, x: -dist },
        animate: { opacity: 1, x: 0 },
        exit: { opacity: 0, x: -dist }
      },
      right: {
        initial: { opacity: 0, x: dist },
        animate: { opacity: 1, x: 0 },
        exit: { opacity: 0, x: dist }
      },
      up: {
        initial: { opacity: 0, y: dist },
        animate: { opacity: 1, y: 0 },
        exit: { opacity: 0, y: dist }
      },
      down: {
        initial: { opacity: 0, y: -dist },
        animate: { opacity: 1, y: 0 },
        exit: { opacity: 0, y: -dist }
      }
    };
    return variants[dir] || variants.left;
  };

  const variant = getVariant(direction, distance);
  const transition = { duration, delay, ease: "easeOut" };

  return (
    <motion.div
      initial={variant.initial}
      animate={variant.animate}
      exit={variant.exit}
      transition={transition}
      className={className}
      {...props}
    >
      {children}
    </motion.div>
  );
};

export default SlideIn;