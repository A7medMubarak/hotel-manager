import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import client from '../api/client';
import { useTranslation } from 'react-i18next';

export default function Users() {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [deletingId, setDeletingId] = useState(null);
  const { t } = useTranslation();

  const fetchUsers = async () => {
    setLoading(true);
    try {
      const { data } = await client.get('/users');
      setUsers(data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchUsers(); }, []);

  const handleDelete = async (id) => {
    if (!window.confirm(t('users.deleteConfirm'))) return;
    setDeletingId(id);
    try {
      await client.delete(`/users/${id}`);
      setUsers((prev) => prev.filter((u) => u.id !== id));
    } catch {
      alert(t('users.deleteFailed'));
    } finally {
      setDeletingId(null);
    }
  };

  return (
    <div className="px-4 py-4">
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-xl font-bold text-gray-800 dark:text-dark-text">{t('users.title')}</h1>
        <Link to="/users/new" className="bg-blue-600 dark:bg-dark-accent dark:text-gray-900 text-white text-sm px-4 py-2 rounded-lg font-medium hover:bg-blue-700 dark:hover:bg-blue-300 transition-colors">{t('users.new')}</Link>
      </div>

      {loading ? (
        <p className="text-center text-gray-400 dark:text-dark-muted py-8">{t('app.loading')}</p>
      ) : users.length === 0 ? (
        <p className="text-center text-gray-400 dark:text-dark-muted py-8">{t('users.empty')}</p>
      ) : (
        <div className="space-y-3">
          {users.map((u) => (
            <div key={u.id} className="bg-white dark:bg-dark-surface rounded-xl border border-gray-200 dark:border-gray-700 p-4 shadow-sm dark:shadow-gray-900 transition-all">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <div className="w-10 h-10 rounded-full bg-blue-100 dark:bg-blue-900/30 flex items-center justify-center text-blue-600 dark:text-blue-300 font-medium">
                    {u.username.charAt(0).toUpperCase()}
                  </div>
                  <div>
                    <p className="font-medium text-gray-800 dark:text-dark-text">{u.username}</p>
                    <span className={`inline-block px-2 py-0.5 rounded-full text-xs font-medium mt-1 ${
                      u.role === 'Owner' ? 'bg-purple-100 dark:bg-purple-900/30 text-purple-700 dark:text-purple-300' : 'bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-300'
                    }`}>
                      {u.role === 'Owner' ? t('role.owner') : t('role.employee')}
                    </span>
                  </div>
                </div>
                {u.role !== 'Owner' && (
                  <button
                    onClick={() => handleDelete(u.id)}
                    disabled={deletingId === u.id}
                    className="text-red-500 dark:text-red-400 hover:text-red-700 dark:hover:text-red-300 text-sm font-medium disabled:opacity-50 transition-colors"
                  >
                    {deletingId === u.id ? '...' : t('app.delete')}
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
