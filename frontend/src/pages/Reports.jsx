import { useState } from 'react';
import client from '../api/client';

export default function Reports() {
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
    { key: 'daily', label: 'Daily', fetch: fetchDaily },
    { key: 'weekly', label: 'Weekly', fetch: () => { fetchWeekly(); fetchOutstanding(); } },
    { key: 'monthly', label: 'Monthly', fetch: fetchMonthly },
    { key: 'outstanding', label: 'Outstanding', fetch: fetchOutstanding },
  ];

  return (
    <div className="px-4 py-4">
      <h1 className="text-xl font-bold text-gray-800 mb-4">Reports</h1>

      <div className="flex gap-2 mb-4 overflow-x-auto pb-2">
        {tabs.map((t) => (
          <button key={t.key} onClick={() => { setTab(t.key); t.fetch(); }}
            className={`px-3 py-1.5 rounded-full text-xs font-medium whitespace-nowrap ${tab === t.key ? 'bg-blue-600 text-white' : 'bg-gray-100 text-gray-600'}`}>
            {t.label}
          </button>
        ))}
      </div>

      {tab === 'daily' && daily && (
        <div className="bg-white rounded-xl border border-gray-200 p-4 shadow-sm space-y-3">
          <p className="text-sm text-gray-500">Business Date: {daily.businessDate}</p>
          <div className="grid grid-cols-2 gap-3">
            <div className="bg-green-50 rounded-lg p-3"><p className="text-2xl font-bold text-green-700">{daily.activeBookings}</p><p className="text-xs text-green-600">Active Bookings</p></div>
            <div className="bg-blue-50 rounded-lg p-3"><p className="text-2xl font-bold text-blue-700">{daily.newCheckIns}</p><p className="text-xs text-blue-600">Check Ins</p></div>
            <div className="bg-orange-50 rounded-lg p-3"><p className="text-2xl font-bold text-orange-700">{daily.newCheckOuts}</p><p className="text-xs text-orange-600">Check Outs</p></div>
            <div className="bg-purple-50 rounded-lg p-3"><p className="text-2xl font-bold text-purple-700">EGP {daily.totalCollected.toFixed(2)}</p><p className="text-xs text-purple-600">Collected</p></div>
          </div>
          <div className="border-t border-gray-100 pt-3 space-y-1 text-sm">
            <p className="flex justify-between"><span className="text-gray-500">Theoretical Revenue</span><span className="font-medium">EGP {daily.theoreticalRevenue.toFixed(2)}</span></p>
            <p className="flex justify-between"><span className="text-gray-500">Outstanding Balance</span><span className="font-medium text-red-600">EGP {daily.outstandingBalance.toFixed(2)}</span></p>
          </div>
        </div>
      )}

      {tab === 'weekly' && weekly && (
        <div className="bg-white rounded-xl border border-gray-200 p-4 shadow-sm space-y-3">
          <p className="text-sm text-gray-500">{weekly.startDate} → {weekly.endDate}</p>
          <div className="grid grid-cols-2 gap-3">
            <div className="bg-blue-50 rounded-lg p-3"><p className="text-2xl font-bold text-blue-700">EGP {weekly.totalCollected.toFixed(2)}</p><p className="text-xs text-blue-600">Collected</p></div>
            <div className="bg-green-50 rounded-lg p-3"><p className="text-2xl font-bold text-green-700">EGP {weekly.totalRevenue.toFixed(2)}</p><p className="text-xs text-green-600">Revenue</p></div>
            <div className="bg-purple-50 rounded-lg p-3"><p className="text-2xl font-bold text-purple-700">{weekly.totalBookings}</p><p className="text-xs text-purple-600">Total</p></div>
            <div className="bg-orange-50 rounded-lg p-3"><p className="text-2xl font-bold text-orange-700">{weekly.completedBookings}</p><p className="text-xs text-orange-600">Completed</p></div>
          </div>
        </div>
      )}

      {tab === 'monthly' && monthly && (
        <div className="bg-white rounded-xl border border-gray-200 p-4 shadow-sm space-y-3">
          <p className="text-sm text-gray-500">{monthly.startDate} → {monthly.endDate}</p>
          <div className="grid grid-cols-2 gap-3">
            <div className="bg-blue-50 rounded-lg p-3"><p className="text-2xl font-bold text-blue-700">EGP {monthly.totalCollected.toFixed(2)}</p><p className="text-xs text-blue-600">Collected</p></div>
            <div className="bg-green-50 rounded-lg p-3"><p className="text-2xl font-bold text-green-700">EGP {monthly.totalRevenue.toFixed(2)}</p><p className="text-xs text-green-600">Revenue</p></div>
            <div className="bg-purple-50 rounded-lg p-3"><p className="text-2xl font-bold text-purple-700">{monthly.totalBookings}</p><p className="text-xs text-purple-600">Total</p></div>
            <div className="bg-orange-50 rounded-lg p-3"><p className="text-2xl font-bold text-orange-700">{monthly.completedBookings}</p><p className="text-xs text-orange-600">Completed</p></div>
          </div>
        </div>
      )}

      {tab === 'outstanding' && outstanding && (
        <div className="space-y-3">
          {outstanding.length === 0 ? (
            <p className="text-center text-gray-400 py-8">No outstanding balances.</p>
          ) : outstanding.map((o) => (
            <div key={o.bookingId} className="bg-white rounded-xl border border-gray-200 p-4 shadow-sm">
              <div className="flex items-start justify-between mb-2">
                <div>
                  <p className="font-semibold text-gray-800">Room {o.roomNumber}</p>
                  <p className="text-sm text-gray-500">{o.guestName}</p>
                </div>
                <span className="text-sm font-bold text-red-600">EGP {o.balance.toFixed(2)}</span>
              </div>
              <div className="flex justify-between text-xs text-gray-400">
                <span>{o.checkIn} → {o.checkOut}</span>
                <span>Paid: EGP {o.totalPaid.toFixed(2)} / EGP {o.totalCost.toFixed(2)}</span>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
