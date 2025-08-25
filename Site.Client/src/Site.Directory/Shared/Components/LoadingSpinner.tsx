import React from 'react';
import styles from './LoadingSpinner.module.css';

const LoadingSpinner: React.FC = () => {
    // CORRECTED: This array now creates a smooth diagonal wave effect.
    // The cells on the main diagonal (top-right, center, bottom-left) all share the 'd-2' delay.
    const delayClasses = ['d-0', 'd-1', 'd-2', 'd-1', 'd-2', 'd-3', 'd-2', 'd-3', 'd-4'];

    return (
        <div className={styles.loader}>
            {/* We now generate the 9 cells dynamically with a map */}
            {delayClasses.map((delayClass, index) => (
                <div key={index} className={`${styles.cell} ${styles[delayClass]}`}></div>
            ))}
        </div>
    );
};

export default LoadingSpinner;
