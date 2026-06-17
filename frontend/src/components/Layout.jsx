import { Outlet, Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function Layout() {
  const { user, logout, isOwner } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const nav = [
    { to: '/bookings', label: 'Bookings', icon: '📋' },
    { to: '/guests', label: 'Guests', icon: '👤' },
    { to: '/rooms', label: 'Rooms', icon: '🚪' },
    ...(isOwner ? [{ to: '/reports', label: 'Reports', icon: '📊' }] : []),
  ];

  return (
    <div className="min-h-screen flex flex-col bg-gray-50">
      <header className="bg-white border-b border-gray-200 px-4 py-3 flex items-center justify-between sticky top-0 z-10">
        <Link to="/bookings" className="text-lg font-bold text-blue-600">Hotel Manager</Link>
        <div className="flex items-center gap-3 text-sm">
          <span className="text-gray-500">{user?.username}</span>
          <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${isOwner ? 'bg-purple-100 text-purple-700' : 'bg-green-100 text-green-700'}`}>
            {user?.role}
          </span>
          <button onClick={handleLogout} className="text-red-500 hover:text-red-700">Logout</button>
        </div>
      </header>

      <main className="flex-1 pb-20">
        <Outlet />
      </main>

      <nav className="fixed bottom-0 left-0 right-0 bg-white border-t border-gray-200 flex justify-around py-2 text-xs">
        {nav.map((item) => (
          <Link
            key={item.to}
            to={item.to}
            className={`flex flex-col items-center gap-0.5 px-3 py-1 rounded-lg transition-colors ${
              location.pathname.startsWith(item.to)
                ? 'text-blue-600 font-semibold'
                : 'text-gray-400'
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
