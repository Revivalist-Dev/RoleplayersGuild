import { create } from 'zustand';

/**
 * Interface for the theme state.
 */
interface ThemeState {
  /**
   * The current theme, can be 'light' or 'dark'.
   */
  theme: 'light' | 'dark';
  /**
   * Function to toggle the theme between 'light' and 'dark'.
   */
  toggleTheme: () => void;
}

/**
 * A Zustand store for managing global theme state.
 *
 * This store holds the current theme and provides an action to toggle it.
 * Components can subscribe to this store to react to theme changes.
 *
 * @example
 * import { useThemeStore } from './stores/themeStore';
 *
 * // Get the current theme
 * const theme = useThemeStore((state) => state.theme);
 *
 * // Get the toggle function
 * const toggleTheme = useThemeStore((state) => state.toggleTheme);
 */
export const useThemeStore = create<ThemeState>((set) => ({
  theme: 'light',
  toggleTheme: () =>
    set((state) => ({
      theme: state.theme === 'light' ? 'dark' : 'light',
    })),
}));