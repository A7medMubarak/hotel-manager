import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import client from '../api/client';
import { useAuth } from '../context/AuthContext';
import { useTranslation } from 'react-i18next';

export default function BookingDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { isOwner } = useAuth();
  const { t } = useTranslation();
  const [booking, setBooking] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [payAmount, setPayAmount] = useState('');
  const [payNote, setPayNote] = useState('');
  const [extendDate, setExtendDate] = useState('');

  const fetchBooking = async () => {
    try {
      const { data } = await client.get(`/bookings/${id}`);
      setBooking(data);
    } catch (err) {
      setError(t('bookingDetail.notFound'));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchBooking(); }, [id]);

  const handleComplete = async () => {
    if (!confirm(t('bookingDetail.confirmComplete'))) return;
    await client.patch(`/bookings/${id}/complete`);
    fetchBooking();
  };

  const handleCancel = async () => {
    if (!confirm(t('bookingDetail.confirmCancel'))) return;
    await client.patch(`/bookings/${id}/cancel`);
    fetchBooking();
  };

  const handleExtend = async (e) => {
    e.preventDefault();
    await client.patch(`/bookings/${id}/extend`, { newCheckOut: extendDate });
    setExtendDate('');
    fetchBooking();
  };

  const handleAddPayment = async (e) => {
    e.preventDefault();
    await client.post('/payments', { bookingId: booking.id, amount: parseFloat(payAmount), notes: payNote || null });
    setPayAmount('');
    setPayNote('');
    fetchBooking();
  };

  if (loading) return <p className="text-center text-gray-400 dark:text-dark-muted py-8">{t('app.loading')}</p>;
  if (error) return <p className="text-center text-red-500 dark:text-red-400 py-8">{error}</p>;
  if (!booking) return null;

  const isActive = booking.status === 'Active';

  const statusLabel = (s) => {
    switch (s) {
      case 'Active': return t('status.active');
      case 'Completed': return t('status.completed');
      case 'Cancelled': return t('status.cancelled');
      default: return s;
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
      <button onClick={() => navigate(-1)} className="text-blue-600 dark:text-dark-accent text-sm mb-4">&larr; {t('app.back')}</button>

      <div className="bg-white dark:bg-dark-surface rounded-xl border border-gray-200 dark:border-gray-700 p-4 shadow-sm dark:shadow-gray-900 mb-4 transition-colors">
        <div className="flex items-start justify-between mb-3">
          <div>
            <h1 className="text-xl font-bold text-gray-800 dark:text-dark-text">{t('bookingDetail.room')}{booking.roomNumber}</h1>
            <p className="text-sm text-gray-500 dark:text-dark-muted">{booking.primaryGuestName || booking.guests?.[0]?.fullName}</p>
          </div>
          <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${statusColor(booking.status)}`}>
            {statusLabel(booking.status)}
          </span>
        </div>

        <div className="grid grid-cols-2 gap-3 text-sm mb-4">
          <div><span className="text-gray-500 dark:text-dark-muted">{t('bookingDetail.checkIn')}</span><p className="font-medium text-gray-800 dark:text-dark-text">{booking.checkIn}</p></div>
          <div><span className="text-gray-500 dark:text-dark-muted">{t('bookingDetail.checkOut')}</span><p className="font-medium text-gray-800 dark:text-dark-text">{booking.checkOut}</p></div>
          <div><span className="text-gray-500 dark:text-dark-muted">{t('bookingDetail.nights')}</span><p className="font-medium text-gray-800 dark:text-dark-text">{booking.nights}</p></div>
          <div><span className="text-gray-500 dark:text-dark-muted">{t('bookingDetail.pricePerNight')}</span><p className="font-medium text-gray-800 dark:text-dark-text">EGP {booking.pricePerNight}</p></div>
          <div><span className="text-gray-500 dark:text-dark-muted">{t('bookingDetail.totalCost')}</span><p className="font-medium text-gray-800 dark:text-dark-text">EGP {booking.totalCost}</p></div>
          <div><span className="text-gray-500 dark:text-dark-muted">{t('bookingDetail.balance')}</span><p className={`font-medium ${booking.balance > 0 ? 'text-red-600 dark:text-red-400' : 'text-green-600 dark:text-green-400'}`}>EGP {booking.balance}</p></div>
        </div>

        {booking.notes && <p className="text-sm text-gray-500 dark:text-dark-muted mb-2">{t('bookingDetail.note')}{booking.notes}</p>}

        {isActive && (
          <div className="flex gap-2">
            <button onClick={handleComplete} className="flex-1 bg-green-600 dark:bg-green-700 text-white text-sm py-2 rounded-lg hover:bg-green-700 dark:hover:bg-green-600 transition-colors">{t('bookingDetail.complete')}</button>
            {isOwner && <button onClick={handleCancel} className="flex-1 bg-red-600 dark:bg-red-700 text-white text-sm py-2 rounded-lg hover:bg-red-700 dark:hover:bg-red-600 transition-colors">{t('bookingDetail.cancel')}</button>}
          </div>
        )}
      </div>

      <div className="bg-white dark:bg-dark-surface rounded-xl border border-gray-200 dark:border-gray-700 p-4 shadow-sm dark:shadow-gray-900 mb-4 transition-colors">
        <h2 className="font-semibold text-gray-800 dark:text-dark-text mb-3">{t('bookingDetail.guests')}</h2>
        {booking.guests?.map((g) => (
          <div key={g.id} className="flex items-center gap-3 py-2 border-b border-gray-100 dark:border-gray-700 last:border-0">
            <div className="w-8 h-8 rounded-full bg-blue-100 dark:bg-blue-900/30 flex items-center justify-center text-blue-600 dark:text-blue-300 text-sm font-medium">
              {g.fullName.charAt(0)}
            </div>
            <div className="flex-1">
              <p className="text-sm font-medium text-gray-800 dark:text-dark-text">{g.fullName} {g.isPrimary && <span className="text-xs text-blue-500 dark:text-dark-accent">{t('bookingDetail.primary')}</span>}</p>
              <p className="text-xs text-gray-400 dark:text-dark-muted">{g.nationalId}</p>
            </div>
          </div>
        ))}
      </div>

      <div className="bg-white dark:bg-dark-surface rounded-xl border border-gray-200 dark:border-gray-700 p-4 shadow-sm dark:shadow-gray-900 mb-4 transition-colors">
        <h2 className="font-semibold text-gray-800 dark:text-dark-text mb-3">{t('bookingDetail.payments')}</h2>
        {booking.payments?.length === 0 ? (
          <p className="text-sm text-gray-400 dark:text-dark-muted">{t('bookingDetail.noPayments')}</p>
        ) : (
          <div className="space-y-2 mb-4">
            {booking.payments?.map((p) => (
              <div key={p.id} className="flex justify-between items-center text-sm py-2 border-b border-gray-100 dark:border-gray-700">
                <div>
                  <p className="font-medium text-gray-800 dark:text-dark-text">EGP {p.amount}</p>
                  {p.notes && <p className="text-xs text-gray-400 dark:text-dark-muted">{p.notes}</p>}
                </div>
                <span className="text-xs text-gray-400 dark:text-dark-muted">{new Date(p.createdAt).toLocaleDateString()}</span>
              </div>
            ))}
          </div>
        )}

        {isActive && (
          <form onSubmit={handleAddPayment} className="flex gap-2">
            <input
              type="number"
              step="0.01"
              min="0.01"
              placeholder={t('bookingDetail.amount')}
              value={payAmount}
              onChange={(e) => setPayAmount(e.target.value)}
              className="flex-1 border border-gray-300 dark:border-gray-600 dark:bg-gray-800 dark:text-dark-text rounded-lg px-3 py-1.5 text-sm transition-colors"
              required
            />
            <button type="submit" className="bg-blue-600 dark:bg-dark-accent dark:text-gray-900 text-white px-4 py-1.5 rounded-lg text-sm font-medium hover:bg-blue-700 dark:hover:bg-blue-300 transition-colors">
              {t('bookingDetail.pay')}
            </button>
          </form>
        )}
      </div>

      {isActive && (
        <div className="bg-white dark:bg-dark-surface rounded-xl border border-gray-200 dark:border-gray-700 p-4 shadow-sm dark:shadow-gray-900 transition-colors">
          <h2 className="font-semibold text-gray-800 dark:text-dark-text mb-3">{t('bookingDetail.extendTitle')}</h2>
          <form onSubmit={handleExtend} className="flex gap-2">
            <input
              type="date"
              value={extendDate}
              onChange={(e) => setExtendDate(e.target.value)}
              className="flex-1 border border-gray-300 dark:border-gray-600 dark:bg-gray-800 dark:text-dark-text rounded-lg px-3 py-1.5 text-sm transition-colors"
              required
            />
            <button type="submit" className="bg-blue-600 dark:bg-dark-accent dark:text-gray-900 text-white px-4 py-1.5 rounded-lg text-sm font-medium hover:bg-blue-700 dark:hover:bg-blue-300 transition-colors">
              {t('bookingDetail.extend')}
            </button>
          </form>
        </div>
      )}
    </div>
  );
}
