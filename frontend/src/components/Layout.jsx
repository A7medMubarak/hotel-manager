import { Outlet, Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useTranslation } from 'react-i18next';
import LangSwitcher from './LangSwitcher';
import ThemeToggle from './ThemeToggle';

export default function Layout() {
  const { user, logout, isOwner } = useAuth();
  const { t, i18n } = useTranslation();
  const location = useLocation();
  const navigate = useNavigate();
  const isRtl = i18n.language === 'ar';

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const nav = [
    { to: '/bookings', label: t('nav.bookings'), icon: '📋' },
    { to: '/guests', label: t('nav.guests'), icon: '👤' },
    { to: '/rooms', label: t('nav.rooms'), icon: '🚪' },
    ...(isOwner ? [{ to: '/reports', label: t('nav.reports'), icon: '📊' }] : []),
  ];

  return (
    <div className="min-h-screen flex flex-col bg-gray-50 dark:bg-dark-bg transition-colors">
      <header className="bg-white dark:bg-dark-surface border-b border-gray-200 dark:border-gray-700 px-4 py-3 flex items-center justify-between sticky top-0 z-10 transition-colors">
        <Link to="/bookings" className="text-lg font-bold text-blue-600 dark:text-dark-accent">Hotel Manager</Link>
        <div className={`flex items-center gap-3 text-sm ${isRtl ? 'flex-row-reverse' : ''}`}>
          <LangSwitcher />
          <ThemeToggle />
          <span className="text-gray-500 dark:text-dark-muted">{user?.username}</span>
          <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${isOwner ? 'bg-purple-100 dark:bg-purple-900/30 text-purple-700 dark:text-purple-300' : 'bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-300'}`}>
            {user?.role === 'Owner' ? t('role.owner') : t('role.employee')}
          </span>
          <button onClick={handleLogout} className="text-red-500 dark:text-red-400 hover:text-red-700 dark:hover:text-red-300">{t('app.logout')}</button>
        </div>
      </header>

      <main className="flex-1 pb-20">
        <Outlet />
      </main>

      <nav className="fixed bottom-0 left-0 right-0 bg-white dark:bg-dark-surface border-t border-gray-200 dark:border-gray-700 flex justify-around py-2 text-xs transition-colors">
        {nav.map((item) => (
          <Link
            key={item.to}
            to={item.to}
            className={`flex flex-col items-center gap-0.5 px-3 py-1 rounded-lg transition-colors ${
              location.pathname.startsWith(item.to)
                ? 'text-blue-600 dark:text-dark-accent font-semibold'
                : 'text-gray-400 dark:text-dark-muted'
            }`}
          >
            <span className="text-xl">{item.icon}</span>
            <span>{item.label}</span>
          </Link>
        ))}
      </nav>
    </div>
  );
}
