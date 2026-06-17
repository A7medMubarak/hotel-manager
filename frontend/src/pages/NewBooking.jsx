import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import client from '../api/client';

export default function NewBooking() {
  const navigate = useNavigate();
  const [rooms, setRooms] = useState([]);
  const [guests, setGuests] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [step, setStep] = useState(1);

  const [roomId, setRoomId] = useState('');
  const [checkIn, setCheckIn] = useState('');
  const [checkOut, setCheckOut] = useState('');
  const [pricePerNight, setPricePerNight] = useState('');
  const [primaryGuestId, setPrimaryGuestId] = useState('');
  const [notes, setNotes] = useState('');
  const [guestSearch, setGuestSearch] = useState('');

  useEffect(() => {
    const init = async () => {
      try {
        const [roomsRes, guestsRes] = await Promise.all([
          client.get('/rooms', { params: { pageSize: 100 } }),
          client.get('/guests', { params: { pageSize: 100 } })
        ]);
        setRooms(roomsRes.data.items || []);
        setGuests(guestsRes.data.items || []);
      } catch (err) {
        setError('Failed to load data.');
      } finally {
        setLoading(false);
      }
    };
    init();
  }, []);

  const handleSearchGuest = async () => {
    if (!guestSearch || guestSearch.length < 2) return;
    const { data } = await client.get('/guests/search', { params: { q: guestSearch } });
    setGuests(data);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    try {
      const { data } = await client.post('/bookings', {
        roomId: parseInt(roomId),
        checkIn,
        checkOut,
        pricePerNight: parseFloat(pricePerNight),
        primaryGuestId: parseInt(primaryGuestId),
        notes: notes || null,
        additionalGuestIds: []
      });
      navigate(`/bookings/${data.id}`);
    } catch (err) {
      setError(err.response?.data?.detail || 'Failed to create booking.');
    }
  };

  const filteredRooms = rooms.filter((r) => r.status === 'Available');

  if (loading) return <p className="text-center text-gray-400 py-8">Loading...</p>;

  return (
    <div className="px-4 py-4">
      <button onClick={() => navigate(-1)} className="text-blue-600 text-sm mb-4">&larr; Back</button>
      <h1 className="text-xl font-bold text-gray-800 mb-4">New Booking</h1>

      {error && <p className="text-sm text-red-600 bg-red-50 rounded-lg p-3 mb-4">{error}</p>}

      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Room</label>
          <select value={roomId} onChange={(e) => {
            setRoomId(e.target.value);
            const room = rooms.find((r) => r.id === parseInt(e.target.value));
            if (room) setPricePerNight(String(room.basePricePerNight));
          }} className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" required>
            <option value="">Select room...</option>
            {filteredRooms.map((r) => (
              <option key={r.id} value={r.id}>Room {r.number} (${r.basePricePerNight}/night)</option>
            ))}
          </select>
          {filteredRooms.length === 0 && <p className="text-xs text-red-500 mt-1">No available rooms.</p>}
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Check In</label>
            <input type="date" value={checkIn} onChange={(e) => setCheckIn(e.target.value)} className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" required />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Check Out</label>
            <input type="date" value={checkOut} onChange={(e) => setCheckOut(e.target.value)} className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" required />
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Price per Night ($)</label>
          <input type="number" step="0.01" min="0.01" value={pricePerNight} onChange={(e) => setPricePerNight(e.target.value)} className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" required />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Primary Guest</label>
          <div className="flex gap-2 mb-2">
            <input
              placeholder="Search by name or National ID..."
              value={guestSearch}
              onChange={(e) => setGuestSearch(e.target.value)}
              className="flex-1 border border-gray-300 rounded-lg px-3 py-2 text-sm"
            />
            <button type="button" onClick={handleSearchGuest} className="bg-gray-100 text-gray-700 px-3 py-2 rounded-lg text-sm hover:bg-gray-200">Search</button>
          </div>
          <select value={primaryGuestId} onChange={(e) => setPrimaryGuestId(e.target.value)} className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" required>
            <option value="">Select guest...</option>
            {guests.map((g) => (
              <option key={g.id} value={g.id}>{g.fullName} ({g.nationalId})</option>
            ))}
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Notes (optional)</label>
          <textarea value={notes} onChange={(e) => setNotes(e.target.value)} rows={2} className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" />
        </div>

        <button type="submit" className="w-full bg-blue-600 text-white py-2.5 rounded-lg text-sm font-medium hover:bg-blue-700">
          Create Booking
        </button>
      </form>
    </div>
  );
}
