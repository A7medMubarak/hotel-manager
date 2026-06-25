import { useTheme } from '../context/ThemeContext';

export default function ThemeToggle() {
  const { dark, toggle } = useTheme();

  return (
    <button
      onClick={toggle}
      className="text-sm px-2 py-1 rounded border border-gray-300 dark:border-gray-600 text-gray-600 dark:text-dark-muted hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
      title={dark ? 'Light mode' : 'Dark mode'}
    >
      {dark ? '☀️' : '🌙'}
    </button>
  );
}
