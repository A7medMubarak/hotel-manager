import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import client from '../api/client';

export default function NewRoom() {
  const navigate = useNavigate();
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
      setError(err.response?.data?.detail || 'Failed to create room.');
    }
  };

  return (
    <div className="px-4 py-4">
      <button onClick={() => navigate(-1)} className="text-blue-600 text-sm mb-4">&larr; Back</button>
      <h1 className="text-xl font-bold text-gray-800 mb-4">New Room</h1>

      {error && <p className="text-sm text-red-600 bg-red-50 rounded-lg p-3 mb-4">{error}</p>}

      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Room Number</label>
          <input name="number" value={form.number} onChange={handleChange} className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" required />
        </div>
        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Floor</label>
            <select name="floor" value={form.floor} onChange={handleChange} className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm">
              {[1, 2, 3].map((f) => <option key={f} value={f}>Floor {f}</option>)}
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Bed Count</label>
            <select name="bedCount" value={form.bedCount} onChange={handleChange} className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm">
              {[1, 2, 3, 4].map((b) => <option key={b} value={b}>{b} bed{b > 1 ? 's' : ''}</option>)}
            </select>
          </div>
        </div>
        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Bathroom Type</label>
            <select name="bathroomType" value={form.bathroomType} onChange={handleChange} className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm">
              <option value={0}>Ensuite</option>
              <option value={1}>Shared</option>
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Base Price (EGP)</label>
            <input name="basePricePerNight" type="number" step="0.01" min="0.01" value={form.basePricePerNight} onChange={handleChange} className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" required />
          </div>
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Notes (optional)</label>
          <textarea name="notes" value={form.notes} onChange={handleChange} rows={2} className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" />
        </div>
        <button type="submit" className="w-full bg-blue-600 text-white py-2.5 rounded-lg text-sm font-medium hover:bg-blue-700">Create Room</button>
      </form>
    </div>
  );
}
