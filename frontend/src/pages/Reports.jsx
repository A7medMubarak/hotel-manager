import { useState } from 'react';
import client from '../api/client';
import { useTranslation } from 'react-i18next';

export default function Reports() {
  const { t } = useTranslation();
  const [daily, setDaily] = useState(null);
  const [outstanding, setOutstanding] = useState(null);
  const [weekly, setWeekly] = useState(null);
  const [monthly, setMonthly] = useState(null);
  const [tab, setTab] = useState('daily');

  const fetchDaily = async () => {
    const { data } = await client.get('/reports/daily');
    setDaily(data);
  };

  const fetchOutstanding = async () => {
    const { data } = await client.get('/reports/outstanding');
    setOutstanding(data);
  };

  const fetchWeekly = async () => {
    const { data } = await client.get('/reports/weekly');
    setWeekly(data);
  };

  const fetchMonthly = async () => {
    const { data } = await client.get('/reports/monthly');
    setMonthly(data);
  };

  useState(() => { fetchDaily(); }, []);

  const tabs = [
    { key: 'daily', label: t('reports.daily'), fetch: fetchDaily },
    { key: 'weekly', label: t('reports.weekly'), fetch: () => { fetchWeekly(); fetchOutstanding(); } },
    { key: 'monthly', label: t('reports.monthly'), fetch: fetchMonthly },
    { key: 'outstanding', label: t('reports.outstanding'), fetch: fetchOutstanding },
  ];

  return (
    <div className="px-4 py-4">
      <h1 className="text-xl font-bold text-gray-800 dark:text-dark-text mb-4">{t('reports.title')}</h1>

      <div className="flex gap-2 mb-4 overflow-x-auto pb-2">
        {tabs.map((t) => (
          <button key={t.key} onClick={() => { setTab(t.key); t.fetch(); }}
            className={`px-3 py-1.5 rounded-full text-xs font-medium whitespace-nowrap transition-colors ${tab === t.key ? 'bg-blue-600 dark:bg-dark-accent dark:text-gray-900 text-white' : 'bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-dark-muted'}`}>
            {t.label}
          </button>
        ))}
      </div>

      {tab === 'daily' && daily && (
        <div className="bg-white dark:bg-dark-surface rounded-xl border border-gray-200 dark:border-gray-700 p-4 shadow-sm dark:shadow-gray-900 space-y-3 transition-colors">
          <p className="text-sm text-gray-500 dark:text-dark-muted">{t('reports.businessDate')}{daily.businessDate}</p>
          <div className="grid grid-cols-2 gap-3">
            <div className="bg-green-50 dark:bg-green-900/20 rounded-lg p-3"><p className="text-2xl font-bold text-green-700 dark:text-green-300">{daily.activeBookings}</p><p className="text-xs text-green-600 dark:text-green-400">{t('reports.activeBookings')}</p></div>
            <div className="bg-blue-50 dark:bg-blue-900/20 rounded-lg p-3"><p className="text-2xl font-bold text-blue-700 dark:text-blue-300">{daily.newCheckIns}</p><p className="text-xs text-blue-600 dark:text-blue-400">{t('reports.checkIns')}</p></div>
            <div className="bg-orange-50 dark:bg-orange-900/20 rounded-lg p-3"><p className="text-2xl font-bold text-orange-700 dark:text-orange-300">{daily.newCheckOuts}</p><p className="text-xs text-orange-600 dark:text-orange-400">{t('reports.checkOuts')}</p></div>
            <div className="bg-purple-50 dark:bg-purple-900/20 rounded-lg p-3"><p className="text-2xl font-bold text-purple-700 dark:text-purple-300">EGP {daily.totalCollected.toFixed(2)}</p><p className="text-xs text-purple-600 dark:text-purple-400">{t('reports.collected')}</p></div>
          </div>
          <div className="border-t border-gray-100 dark:border-gray-700 pt-3 space-y-1 text-sm">
            <p className="flex justify-between"><span className="text-gray-500 dark:text-dark-muted">{t('reports.theoreticalRevenue')}</span><span className="font-medium text-gray-800 dark:text-dark-text">EGP {daily.theoreticalRevenue.toFixed(2)}</span></p>
            <p className="flex justify-between"><span className="text-gray-500 dark:text-dark-muted">{t('reports.outstandingBalance')}</span><span className="font-medium text-red-600 dark:text-red-400">EGP {daily.outstandingBalance.toFixed(2)}</span></p>
          </div>
        </div>
      )}

      {tab === 'weekly' && weekly && (
        <div className="bg-white dark:bg-dark-surface rounded-xl border border-gray-200 dark:border-gray-700 p-4 shadow-sm dark:shadow-gray-900 space-y-3 transition-colors">
          <p className="text-sm text-gray-500 dark:text-dark-muted">{weekly.startDate} → {weekly.endDate}</p>
          <div className="grid grid-cols-2 gap-3">
            <div className="bg-blue-50 dark:bg-blue-900/20 rounded-lg p-3"><p className="text-2xl font-bold text-blue-700 dark:text-blue-300">EGP {weekly.totalCollected.toFixed(2)}</p><p className="text-xs text-blue-600 dark:text-blue-400">{t('reports.collected')}</p></div>
            <div className="bg-green-50 dark:bg-green-900/20 rounded-lg p-3"><p className="text-2xl font-bold text-green-700 dark:text-green-300">EGP {weekly.totalRevenue.toFixed(2)}</p><p className="text-xs text-green-600 dark:text-green-400">{t('reports.revenue')}</p></div>
            <div className="bg-purple-50 dark:bg-purple-900/20 rounded-lg p-3"><p className="text-2xl font-bold text-purple-700 dark:text-purple-300">{weekly.totalBookings}</p><p className="text-xs text-purple-600 dark:text-purple-400">{t('reports.total')}</p></div>
            <div className="bg-orange-50 dark:bg-orange-900/20 rounded-lg p-3"><p className="text-2xl font-bold text-orange-700 dark:text-orange-300">{weekly.completedBookings}</p><p className="text-xs text-orange-600 dark:text-orange-400">{t('reports.completed')}</p></div>
          </div>
        </div>
      )}

      {tab === 'monthly' && monthly && (
        <div className="bg-white dark:bg-dark-surface rounded-xl border border-gray-200 dark:border-gray-700 p-4 shadow-sm dark:shadow-gray-900 space-y-3 transition-colors">
          <p className="text-sm text-gray-500 dark:text-dark-muted">{monthly.startDate} → {monthly.endDate}</p>
          <div className="grid grid-cols-2 gap-3">
            <div className="bg-blue-50 dark:bg-blue-900/20 rounded-lg p-3"><p className="text-2xl font-bold text-blue-700 dark:text-blue-300">EGP {monthly.totalCollected.toFixed(2)}</p><p className="text-xs text-blue-600 dark:text-blue-400">{t('reports.collected')}</p></div>
            <div className="bg-green-50 dark:bg-green-900/20 rounded-lg p-3"><p className="text-2xl font-bold text-green-700 dark:text-green-300">EGP {monthly.totalRevenue.toFixed(2)}</p><p className="text-xs text-green-600 dark:text-green-400">{t('reports.revenue')}</p></div>
            <div className="bg-purple-50 dark:bg-purple-900/20 rounded-lg p-3"><p className="text-2xl font-bold text-purple-700 dark:text-purple-300">{monthly.totalBookings}</p><p className="text-xs text-purple-600 dark:text-purple-400">{t('reports.total')}</p></div>
            <div className="bg-orange-50 dark:bg-orange-900/20 rounded-lg p-3"><p className="text-2xl font-bold text-orange-700 dark:text-orange-300">{monthly.completedBookings}</p><p className="text-xs text-orange-600 dark:text-orange-400">{t('reports.completed')}</p></div>
          </div>
        </div>
      )}

      {tab === 'outstanding' && outstanding && (
        <div className="space-y-3">
          {outstanding.length === 0 ? (
            <p className="text-center text-gray-400 dark:text-dark-muted py-8">{t('reports.noOutstanding')}</p>
          ) : outstanding.map((o) => (
            <div key={o.bookingId} className="bg-white dark:bg-dark-surface rounded-xl border border-gray-200 dark:border-gray-700 p-4 shadow-sm dark:shadow-gray-900 transition-colors">
              <div className="flex items-start justify-between mb-2">
                <div>
                  <p className="font-semibold text-gray-800 dark:text-dark-text">{t('bookings.roomPrefix')}{o.roomNumber}</p>
                  <p className="text-sm text-gray-500 dark:text-dark-muted">{o.guestName}</p>
                </div>
                <span className="text-sm font-bold text-red-600 dark:text-red-400">EGP {o.balance.toFixed(2)}</span>
              </div>
              <div className="flex justify-between text-xs text-gray-400 dark:text-dark-muted">
                <span>{o.checkIn} → {o.checkOut}</span>
                <span>{t('reports.paid')}{o.totalPaid.toFixed(2)} / EGP {o.totalCost.toFixed(2)}</span>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
