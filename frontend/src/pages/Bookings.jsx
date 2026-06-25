import { useState, useEffect } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import client from '../api/client';
import { useTranslation } from 'react-i18next';

const STATUS_OPTIONS = ['', 'Active', 'Completed', 'Cancelled'];

export default function Bookings() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [bookings, setBookings] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [loading, setLoading] = useState(true);
  const { t } = useTranslation();

  const page = parseInt(searchParams.get('page') || '1');
  const status = searchParams.get('status') ?? 'Active';
  const guestName = searchParams.get('guestName') || '';
  const roomNumber = searchParams.get('roomNumber') || '';
  const checkInFrom = searchParams.get('checkInFrom') || '';
  const checkInTo = searchParams.get('checkInTo') || '';

  const fetchBookings = async () => {
    setLoading(true);
    try {
      const params = { page, pageSize: 20 };
      if (status) params.status = status;
      if (guestName) params.guestName = guestName;
      if (roomNumber) params.roomNumber = roomNumber;
      if (checkInFrom) params.checkInFrom = checkInFrom;
      if (checkInTo) params.checkInTo = checkInTo;

      const { data } = await client.get('/bookings', { params });
      setBookings(data.items);
      setTotalCount(data.totalCount);
      setTotalPages(data.totalPages);
    } catch (err) {
      console.error(t('bookings.loadFailed'), err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchBookings(); }, [searchParams]);

  const updateFilter = (key, value) => {
    const params = new URLSearchParams(searchParams);
    if (value) params.set(key, value);
    else if (key === 'status') params.set(key, '');
    else params.delete(key);
    if (key !== 'page') params.set('page', '1');
    setSearchParams(params);
  };

  const statusLabel = (s) => {
    switch (s) {
      case 'Active': return t('bookings.active');
      case 'Completed': return t('bookings.completed');
      case 'Cancelled': return t('bookings.cancelled');
      default: return t('bookings.all');
    }
  };

  const statusColor = (s) => {
    switch (s) {
      case 'Active': return 'bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-300';
      case 'Completed': return 'bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300';
      case 'Cancelled': return 'bg-red-100 dark:bg-red-900/30 text-red-700 dark:text-red-300';
      default: return 'bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-dark-muted';
    }
  };

  return (
    <div className="px-4 py-4">
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-xl font-bold text-gray-800 dark:text-dark-text">{t('bookings.title')}</h1>
        <Link to="/bookings/new" className="bg-blue-600 dark:bg-dark-accent dark:text-gray-900 text-white text-sm px-4 py-2 rounded-lg font-medium hover:bg-blue-700 dark:hover:bg-blue-300 transition-colors">
          {t('bookings.new')}
        </Link>
      </div>

      <div className="flex gap-2 mb-4 overflow-x-auto pb-2">
        {STATUS_OPTIONS.map((s) => (
          <button
            key={s}
            onClick={() => updateFilter('status', s)}
            className={`px-3 py-1.5 rounded-full text-xs font-medium whitespace-nowrap transition-colors ${
              status === s ? 'bg-blue-600 dark:bg-dark-accent dark:text-gray-900 text-white' : 'bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-dark-muted'
            }`}
          >
            {statusLabel(s)}
          </button>
        ))}
      </div>

      <div className="flex flex-wrap gap-2 mb-4">
        <input
          placeholder={t('bookings.guestName')}
          value={guestName}
          onChange={(e) => updateFilter('guestName', e.target.value)}
          className="flex-1 min-w-[140px] border border-gray-300 dark:border-gray-600 dark:bg-gray-800 dark:text-dark-text rounded-lg px-3 py-1.5 text-sm transition-colors"
        />
        <input
          placeholder={t('bookings.room')}
          value={roomNumber}
          onChange={(e) => updateFilter('roomNumber', e.target.value)}
          className="w-24 border border-gray-300 dark:border-gray-600 dark:bg-gray-800 dark:text-dark-text rounded-lg px-3 py-1.5 text-sm transition-colors"
        />
      </div>

      {loading ? (
        <p className="text-center text-gray-400 dark:text-dark-muted py-8">{t('app.loading')}</p>
      ) : bookings.length === 0 ? (
        <p className="text-center text-gray-400 dark:text-dark-muted py-8">{t('bookings.empty')}</p>
      ) : (
        <>
          <div className="space-y-3">
            {bookings.map((b) => (
              <Link
                key={b.id}
                to={`/bookings/${b.id}`}
                className="block bg-white dark:bg-dark-surface rounded-xl border border-gray-200 dark:border-gray-700 p-4 shadow-sm dark:shadow-gray-900 hover:shadow-md dark:hover:shadow-gray-900 transition-all"
              >
                <div className="flex items-start justify-between mb-2">
                  <div>
                    <p className="font-semibold text-gray-800 dark:text-dark-text">{t('bookings.roomPrefix')}{b.roomNumber}</p>
                    <p className="text-sm text-gray-500 dark:text-dark-muted">{b.primaryGuestName}</p>
                  </div>
                  <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${statusColor(b.status)}`}>
                    {statusLabel(b.status)}
                  </span>
                </div>
                <div className="flex justify-between text-sm text-gray-500 dark:text-dark-muted">
                  <span>{b.checkIn} → {b.checkOut}</span>
                  <span className="font-medium text-gray-700 dark:text-dark-text">{b.balance > 0 ? `EGP ${b.balance}` : t('bookings.paid')}</span>
                </div>
              </Link>
            ))}
          </div>

          {totalPages > 1 && (
            <div className="flex justify-center items-center gap-2 mt-6">
              <button
                onClick={() => updateFilter('page', String(page - 1))}
                disabled={page <= 1}
                className="px-3 py-1.5 text-sm border border-gray-300 dark:border-gray-600 dark:text-dark-text rounded-lg disabled:opacity-30 transition-colors"
              >
                {t('app.prev')}
              </button>
              <span className="text-sm text-gray-500 dark:text-dark-muted">{page} / {totalPages}</span>
              <button
                onClick={() => updateFilter('page', String(page + 1))}
                disabled={page >= totalPages}
                className="px-3 py-1.5 text-sm border border-gray-300 dark:border-gray-600 dark:text-dark-text rounded-lg disabled:opacity-30 transition-colors"
              >
                {t('app.next')}
              </button>
            </div>
          )}
        </>
      )}
    </div>
  );
}
