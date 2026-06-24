import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import client from '../api/client';

export default function NewBooking() {
  const navigate = useNavigate();
  const [rooms, setRooms] = useState([]);
  const [guests, setGuests] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const [roomId, setRoomId] = useState('');
  const [checkIn, setCheckIn] = useState('');
  const [checkOut, setCheckOut] = useState('');
  const [pricePerNight, setPricePerNight] = useState('');
  const [primaryGuestId, setPrimaryGuestId] = useState('');
  const [notes, setNotes] = useState('');
  const [guestSearch, setGuestSearch] = useState('');

  const [showNewGuest, setShowNewGuest] = useState(false);
  const [newGuestForm, setNewGuestForm] = useState({ fullName: '', nationalId: '', address: '', phone: '' });
  const [newGuestError, setNewGuestError] = useState('');

  const [additionalSearch, setAdditionalSearch] = useState('');
  const [additionalResults, setAdditionalResults] = useState([]);
  const [additionalGuests, setAdditionalGuests] = useState([]);

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

  const handleAdditionalSearch = async () => {
    if (!additionalSearch || additionalSearch.length < 2) return;
    const { data } = await client.get('/guests/search', { params: { q: additionalSearch } });
    const selectedIds = [parseInt(primaryGuestId), ...additionalGuests.map((g) => g.id)];
    setAdditionalResults(data.filter((g) => !selectedIds.includes(g.id)));
  };

  const addAdditionalGuest = (guest) => {
    setAdditionalGuests([...additionalGuests, guest]);
    setAdditionalResults([]);
    setAdditionalSearch('');
  };

  const removeAdditionalGuest = (id) => {
    setAdditionalGuests(additionalGuests.filter((g) => g.id !== id));
  };

  const handleCreateGuest = async (e) => {
    e.preventDefault();
    setNewGuestError('');
    try {
      const { data } = await client.post('/guests', newGuestForm);
      setGuests((prev) => [...prev, data]);
      setPrimaryGuestId(String(data.id));
      setShowNewGuest(false);
      setNewGuestForm({ fullName: '', nationalId: '', address: '', phone: '' });
    } catch (err) {
      setNewGuestError(err.response?.data?.detail || 'Failed to create guest.');
    }
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
        additionalGuestIds: additionalGuests.map((g) => g.id)
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
              <option key={r.id} value={r.id}>Room {r.number} (EGP {r.basePricePerNight}/night)</option>
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
          <label className="block text-sm font-medium text-gray-700 mb-1">Price per Night (EGP)</label>
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
            <button type="button" onClick={() => setShowNewGuest(true)} className="bg-blue-600 text-white px-3 py-2 rounded-lg text-sm hover:bg-blue-700">+ New</button>
          </div>
          <select value={primaryGuestId} onChange={(e) => setPrimaryGuestId(e.target.value)} className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" required>
            <option value="">Select guest...</option>
            {guests.map((g) => (
              <option key={g.id} value={g.id}>{g.fullName} ({g.nationalId})</option>
            ))}
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Additional Guests (optional)</label>
          <div className="flex gap-2 mb-2">
            <input
              placeholder="Search by name..."
              value={additionalSearch}
              onChange={(e) => setAdditionalSearch(e.target.value)}
              className="flex-1 border border-gray-300 rounded-lg px-3 py-2 text-sm"
            />
            <button type="button" onClick={handleAdditionalSearch} className="bg-gray-100 text-gray-700 px-3 py-2 rounded-lg text-sm hover:bg-gray-200">Search</button>
          </div>
          {additionalResults.length > 0 && (
            <div className="border border-gray-200 rounded-lg mb-2 max-h-40 overflow-y-auto">
              {additionalResults.map((g) => (
                <button key={g.id} type="button" onClick={() => addAdditionalGuest(g)}
                  className="w-full text-left px-3 py-2 text-sm border-b border-gray-100 last:border-0 hover:bg-gray-50 flex items-center gap-2">
                  <span className="text-gray-600">{g.fullName}</span>
                  <span className="text-gray-400 text-xs">{g.nationalId}</span>
                  <span className="ml-auto text-blue-500 text-xs font-medium">+ Add</span>
                </button>
              ))}
            </div>
          )}
          {additionalGuests.length > 0 && (
            <div className="flex flex-wrap gap-2">
              {additionalGuests.map((g) => (
                <span key={g.id} className="inline-flex items-center gap-1 bg-blue-50 text-blue-700 text-xs px-2.5 py-1.5 rounded-full">
                  {g.fullName}
                  <button type="button" onClick={() => removeAdditionalGuest(g.id)} className="text-blue-400 hover:text-blue-700 leading-none">&times;</button>
                </span>
              ))}
            </div>
          )}
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Notes (optional)</label>
          <textarea value={notes} onChange={(e) => setNotes(e.target.value)} rows={2} className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" />
        </div>

        <button type="submit" className="w-full bg-blue-600 text-white py-2.5 rounded-lg text-sm font-medium hover:bg-blue-700">
          Create Booking
        </button>
      </form>

      {showNewGuest && (
        <div className="fixed inset-0 z-50 flex items-end sm:items-center justify-center bg-black/40">
          <div className="bg-white w-full sm:max-w-md rounded-t-2xl sm:rounded-2xl p-6 max-h-[90vh] overflow-y-auto">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-bold text-gray-800">New Guest</h2>
              <button onClick={() => setShowNewGuest(false)} className="text-gray-400 text-xl leading-none">&times;</button>
            </div>

            {newGuestError && <p className="text-sm text-red-600 bg-red-50 rounded-lg p-3 mb-4">{newGuestError}</p>}

            <form onSubmit={handleCreateGuest} className="space-y-3">
              <input name="fullName" value={newGuestForm.fullName} onChange={(e) => setNewGuestForm({ ...newGuestForm, fullName: e.target.value })} placeholder="Full Name" className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" required />
              <input name="nationalId" value={newGuestForm.nationalId} onChange={(e) => setNewGuestForm({ ...newGuestForm, nationalId: e.target.value })} placeholder="National ID" className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" required />
              <input name="address" value={newGuestForm.address} onChange={(e) => setNewGuestForm({ ...newGuestForm, address: e.target.value })} placeholder="Address" className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" required />
              <input name="phone" value={newGuestForm.phone} onChange={(e) => setNewGuestForm({ ...newGuestForm, phone: e.target.value })} placeholder="Phone (optional)" className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" />
              <button type="submit" className="w-full bg-blue-600 text-white py-2.5 rounded-lg text-sm font-medium hover:bg-blue-700">Add Guest</button>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
