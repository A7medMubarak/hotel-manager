import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import client from '../api/client';
import { useAuth } from '../context/AuthContext';

export default function BookingDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { isOwner } = useAuth();
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
      setError('Booking not found.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchBooking(); }, [id]);

  const handleComplete = async () => {
    if (!confirm('Complete this booking?')) return;
    await client.patch(`/bookings/${id}/complete`);
    fetchBooking();
  };

  const handleCancel = async () => {
    if (!confirm('Cancel this booking?')) return;
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

  if (loading) return <p className="text-center text-gray-400 py-8">Loading...</p>;
  if (error) return <p className="text-center text-red-500 py-8">{error}</p>;
  if (!booking) return null;

  const isActive = booking.status === 'Active';

  return (
    <div className="px-4 py-4">
      <button onClick={() => navigate(-1)} className="text-blue-600 text-sm mb-4">&larr; Back</button>

      <div className="bg-white rounded-xl border border-gray-200 p-4 shadow-sm mb-4">
        <div className="flex items-start justify-between mb-3">
          <div>
            <h1 className="text-xl font-bold text-gray-800">Room {booking.roomNumber}</h1>
            <p className="text-sm text-gray-500">{booking.primaryGuestName || booking.guests?.[0]?.fullName}</p>
          </div>
          <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${
            booking.status === 'Active' ? 'bg-green-100 text-green-700' :
            booking.status === 'Completed' ? 'bg-blue-100 text-blue-700' :
            'bg-red-100 text-red-700'
          }`}>{booking.status}</span>
        </div>

        <div className="grid grid-cols-2 gap-3 text-sm mb-4">
          <div><span className="text-gray-500">Check In</span><p className="font-medium">{booking.checkIn}</p></div>
          <div><span className="text-gray-500">Check Out</span><p className="font-medium">{booking.checkOut}</p></div>
          <div><span className="text-gray-500">Nights</span><p className="font-medium">{booking.nights}</p></div>
          <div><span className="text-gray-500">Price/Night</span><p className="font-medium">${booking.pricePerNight}</p></div>
          <div><span className="text-gray-500">Total Cost</span><p className="font-medium">${booking.totalCost}</p></div>
          <div><span className="text-gray-500">Balance</span><p className={`font-medium ${booking.balance > 0 ? 'text-red-600' : 'text-green-600'}`}>${booking.balance}</p></div>
        </div>

        {booking.notes && <p className="text-sm text-gray-500 mb-2">Note: {booking.notes}</p>}

        {isActive && (
          <div className="flex gap-2">
            <button onClick={handleComplete} className="flex-1 bg-green-600 text-white text-sm py-2 rounded-lg hover:bg-green-700">Complete</button>
            {isOwner && <button onClick={handleCancel} className="flex-1 bg-red-600 text-white text-sm py-2 rounded-lg hover:bg-red-700">Cancel</button>}
          </div>
        )}
      </div>

      <div className="bg-white rounded-xl border border-gray-200 p-4 shadow-sm mb-4">
        <h2 className="font-semibold text-gray-800 mb-3">Guests</h2>
        {booking.guests?.map((g) => (
          <div key={g.id} className="flex items-center gap-3 py-2 border-b border-gray-100 last:border-0">
            <div className="w-8 h-8 rounded-full bg-blue-100 flex items-center justify-center text-blue-600 text-sm font-medium">
              {g.fullName.charAt(0)}
            </div>
            <div className="flex-1">
              <p className="text-sm font-medium text-gray-800">{g.fullName} {g.isPrimary && <span className="text-xs text-blue-500">(Primary)</span>}</p>
              <p className="text-xs text-gray-400">{g.nationalId}</p>
            </div>
          </div>
        ))}
      </div>

      <div className="bg-white rounded-xl border border-gray-200 p-4 shadow-sm mb-4">
        <h2 className="font-semibold text-gray-800 mb-3">Payments</h2>
        {booking.payments?.length === 0 ? (
          <p className="text-sm text-gray-400">No payments yet.</p>
        ) : (
          <div className="space-y-2 mb-4">
            {booking.payments?.map((p) => (
              <div key={p.id} className="flex justify-between items-center text-sm py-2 border-b border-gray-100">
                <div>
                  <p className="font-medium text-gray-800">${p.amount}</p>
                  {p.notes && <p className="text-xs text-gray-400">{p.notes}</p>}
                </div>
                <span className="text-xs text-gray-400">{new Date(p.createdAt).toLocaleDateString()}</span>
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
              placeholder="Amount"
              value={payAmount}
              onChange={(e) => setPayAmount(e.target.value)}
              className="flex-1 border border-gray-300 rounded-lg px-3 py-1.5 text-sm"
              required
            />
            <button type="submit" className="bg-blue-600 text-white px-4 py-1.5 rounded-lg text-sm font-medium hover:bg-blue-700">
              Pay
            </button>
          </form>
        )}
      </div>

      {isActive && (
        <div className="bg-white rounded-xl border border-gray-200 p-4 shadow-sm">
          <h2 className="font-semibold text-gray-800 mb-3">Extend Booking</h2>
          <form onSubmit={handleExtend} className="flex gap-2">
            <input
              type="date"
              value={extendDate}
              onChange={(e) => setExtendDate(e.target.value)}
              className="flex-1 border border-gray-300 rounded-lg px-3 py-1.5 text-sm"
              required
            />
            <button type="submit" className="bg-blue-600 text-white px-4 py-1.5 rounded-lg text-sm font-medium hover:bg-blue-700">
              Extend
            </button>
          </form>
        </div>
      )}
    </div>
  );
}
