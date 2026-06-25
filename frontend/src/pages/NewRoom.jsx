import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import client from '../api/client';
import { useTranslation } from 'react-i18next';

export default function NewRoom() {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const [form, setForm] = useState({
    number: '', floor: 1, bedCount: 1,
    bathroomType: 0, basePricePerNight: '', notes: ''
  });
  const [error, setError] = useState('');

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm({ ...form, [name]: value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    try {
      const { data } = await client.post('/rooms', {
        number: form.number,
        floor: parseInt(form.floor),
        bedCount: parseInt(form.bedCount),
        bathroomType: parseInt(form.bathroomType),
        basePricePerNight: parseFloat(form.basePricePerNight),
        notes: form.notes || null
      });
      navigate(`/rooms`);
    } catch (err) {
      setError(err.response?.data?.detail || t('newRoom.createFailed'));
    }
  };

  return (
    <div className="px-4 py-4">
      <button onClick={() => navigate(-1)} className="text-blue-600 dark:text-dark-accent text-sm mb-4">&larr; {t('app.back')}</button>
      <h1 className="text-xl font-bold text-gray-800 dark:text-dark-text mb-4">{t('newRoom.title')}</h1>

      {error && <p className="text-sm text-red-600 dark:text-red-400 bg-red-50 dark:bg-red-900/20 rounded-lg p-3 mb-4">{error}</p>}

      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-dark-muted mb-1">{t('newRoom.roomNumber')}</label>
          <input name="number" value={form.number} onChange={handleChange} className="w-full border border-gray-300 dark:border-gray-600 dark:bg-gray-800 dark:text-dark-text rounded-lg px-3 py-2 text-sm transition-colors" required />
        </div>
        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-dark-muted mb-1">{t('newRoom.floor')}</label>
            <select name="floor" value={form.floor} onChange={handleChange} className="w-full border border-gray-300 dark:border-gray-600 dark:bg-gray-800 dark:text-dark-text rounded-lg px-3 py-2 text-sm transition-colors">
              {[1, 2, 3].map((f) => <option key={f} value={f}>{t('rooms.floor')}{f}</option>)}
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-dark-muted mb-1">{t('newRoom.bedCount')}</label>
            <select name="bedCount" value={form.bedCount} onChange={handleChange} className="w-full border border-gray-300 dark:border-gray-600 dark:bg-gray-800 dark:text-dark-text rounded-lg px-3 py-2 text-sm transition-colors">
              {[1, 2, 3, 4].map((b) => <option key={b} value={b}>{b} {b > 1 ? t('newRoom.bedCount') + 's' : t('newRoom.bedCount')}</option>)}
            </select>
          </div>
        </div>
        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-dark-muted mb-1">{t('newRoom.bathroomType')}</label>
            <select name="bathroomType" value={form.bathroomType} onChange={handleChange} className="w-full border border-gray-300 dark:border-gray-600 dark:bg-gray-800 dark:text-dark-text rounded-lg px-3 py-2 text-sm transition-colors">
              <option value={0}>{t('newRoom.ensuite')}</option>
              <option value={1}>{t('newRoom.shared')}</option>
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-dark-muted mb-1">{t('newRoom.basePrice')}</label>
            <input name="basePricePerNight" type="number" step="0.01" min="0.01" value={form.basePricePerNight} onChange={handleChange} className="w-full border border-gray-300 dark:border-gray-600 dark:bg-gray-800 dark:text-dark-text rounded-lg px-3 py-2 text-sm transition-colors" required />
          </div>
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-dark-muted mb-1">{t('newRoom.notes')}</label>
          <textarea name="notes" value={form.notes} onChange={handleChange} rows={2} className="w-full border border-gray-300 dark:border-gray-600 dark:bg-gray-800 dark:text-dark-text rounded-lg px-3 py-2 text-sm transition-colors" />
        </div>
        <button type="submit" className="w-full bg-blue-600 dark:bg-dark-accent dark:text-gray-900 text-white py-2.5 rounded-lg text-sm font-medium hover:bg-blue-700 dark:hover:bg-blue-300 transition-colors">{t('newRoom.create')}</button>
      </form>
    </div>
  );
}
