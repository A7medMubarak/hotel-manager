import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import client from '../api/client';
import { useTranslation } from 'react-i18next';

export default function NewUser() {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const [form, setForm] = useState({ username: '', password: '' });
  const [error, setError] = useState('');

  const handleChange = (e) => setForm({ ...form, [e.target.name]: e.target.value });

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    try {
      await client.post('/users', form);
      navigate('/users');
    } catch (err) {
      setError(err.response?.data?.detail || t('newUser.createFailed'));
    }
  };

  return (
    <div className="px-4 py-4">
      <button onClick={() => navigate(-1)} className="text-blue-600 dark:text-dark-accent text-sm mb-4">&larr; {t('app.back')}</button>
      <h1 className="text-xl font-bold text-gray-800 dark:text-dark-text mb-4">{t('newUser.title')}</h1>

      {error && <p className="text-sm text-red-600 dark:text-red-400 bg-red-50 dark:bg-red-900/20 rounded-lg p-3 mb-4">{error}</p>}

      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-dark-muted mb-1">{t('newUser.username')}</label>
          <input name="username" value={form.username} onChange={handleChange} className="w-full border border-gray-300 dark:border-gray-600 dark:bg-gray-800 dark:text-dark-text rounded-lg px-3 py-2 text-sm transition-colors" required />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-dark-muted mb-1">{t('newUser.password')}</label>
          <input name="password" type="password" value={form.password} onChange={handleChange} className="w-full border border-gray-300 dark:border-gray-600 dark:bg-gray-800 dark:text-dark-text rounded-lg px-3 py-2 text-sm transition-colors" required />
        </div>
        <button type="submit" className="w-full bg-blue-600 dark:bg-dark-accent dark:text-gray-900 text-white py-2.5 rounded-lg text-sm font-medium hover:bg-blue-700 dark:hover:bg-blue-300 transition-colors">{t('newUser.create')}</button>
      </form>
    </div>
  );
}
