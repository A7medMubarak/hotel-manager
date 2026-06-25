import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import client from '../api/client';
import { useTranslation } from 'react-i18next';

export default function GuestDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const [guest, setGuest] = useState(null);
  const [editing, setEditing] = useState(false);
  const [form, setForm] = useState({});
  const [error, setError] = useState('');

  const fetchGuest = async () => {
    try {
      const { data } = await client.get(`/guests/${id}`);
      setGuest(data);
      setForm({ fullName: data.fullName, nationalId: data.nationalId, address: data.address, phone: data.phone || '' });
    } catch {
      setError(t('guestDetail.notFound'));
    }
  };

  useEffect(() => { fetchGuest(); }, [id]);

  const handleUpdate = async (e) => {
    e.preventDefault();
    setError('');
    try {
      const { data } = await client.put(`/guests/${id}`, form);
      setGuest(data);
      setEditing(false);
    } catch (err) {
      setError(err.response?.data?.detail || t('app.updateFailed'));
    }
  };

  const fieldLabel = (field) => {
    switch (field) {
      case 'fullName': return t('newGuest.fullName');
      case 'nationalId': return t('newGuest.nationalId');
      case 'address': return t('newGuest.address');
      case 'phone': return t('newGuest.phone');
      default: return field;
    }
  };

  if (error) return <p className="text-center text-red-500 dark:text-red-400 py-8">{error}</p>;
  if (!guest) return <p className="text-center text-gray-400 dark:text-dark-muted py-8">{t('app.loading')}</p>;

  return (
    <div className="px-4 py-4">
      <button onClick={() => navigate(-1)} className="text-blue-600 dark:text-dark-accent text-sm mb-4">&larr; {t('app.back')}</button>

      {editing ? (
        <form onSubmit={handleUpdate} className="bg-white dark:bg-dark-surface rounded-xl border border-gray-200 dark:border-gray-700 p-4 shadow-sm dark:shadow-gray-900 space-y-4 transition-colors">
          <h1 className="text-xl font-bold text-gray-800 dark:text-dark-text">{t('guestDetail.editTitle')}</h1>
          {error && <p className="text-sm text-red-600 dark:text-red-400 bg-red-50 dark:bg-red-900/20 rounded-lg p-3">{error}</p>}
          {['fullName', 'nationalId', 'address', 'phone'].map((field) => (
            <div key={field}>
              <label className="block text-sm font-medium text-gray-700 dark:text-dark-muted mb-1">{fieldLabel(field)}</label>
              <input name={field} value={form[field]} onChange={(e) => setForm({ ...form, [e.target.name]: e.target.value })} className="w-full border border-gray-300 dark:border-gray-600 dark:bg-gray-800 dark:text-dark-text rounded-lg px-3 py-2 text-sm transition-colors" required={field !== 'phone'} />
            </div>
          ))}
          <div className="flex gap-2">
            <button type="submit" className="flex-1 bg-blue-600 dark:bg-dark-accent dark:text-gray-900 text-white py-2 rounded-lg text-sm font-medium hover:bg-blue-700 dark:hover:bg-blue-300 transition-colors">{t('guestDetail.save')}</button>
            <button type="button" onClick={() => setEditing(false)} className="flex-1 bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-dark-muted py-2 rounded-lg text-sm hover:bg-gray-200 dark:hover:bg-gray-600 transition-colors">{t('guestDetail.cancel')}</button>
          </div>
        </form>
      ) : (
        <div className="bg-white dark:bg-dark-surface rounded-xl border border-gray-200 dark:border-gray-700 p-4 shadow-sm dark:shadow-gray-900 transition-colors">
          <div className="flex items-start justify-between mb-4">
            <div className="flex items-center gap-3">
              <div className="w-12 h-12 rounded-full bg-blue-100 dark:bg-blue-900/30 flex items-center justify-center text-blue-600 dark:text-blue-300 text-lg font-medium">{guest.fullName.charAt(0)}</div>
              <div>
                <h1 className="text-xl font-bold text-gray-800 dark:text-dark-text">{guest.fullName}</h1>
                <p className="text-sm text-gray-400 dark:text-dark-muted">{guest.nationalId}</p>
              </div>
            </div>
            <button onClick={() => setEditing(true)} className="text-blue-600 dark:text-dark-accent text-sm font-medium">{t('guestDetail.edit')}</button>
          </div>
          <div className="space-y-2 text-sm">
            <p><span className="text-gray-500 dark:text-dark-muted">{t('guestDetail.address')}</span> <span className="text-gray-800 dark:text-dark-text">{guest.address}</span></p>
            <p><span className="text-gray-500 dark:text-dark-muted">{t('guestDetail.phone')}</span> <span className="text-gray-800 dark:text-dark-text">{guest.phone || '—'}</span></p>
            <p><span className="text-gray-500 dark:text-dark-muted">{t('guestDetail.registered')}</span> <span className="text-gray-800 dark:text-dark-text">{new Date(guest.createdAt).toLocaleDateString()}</span></p>
          </div>
        </div>
      )}
    </div>
  );
}
