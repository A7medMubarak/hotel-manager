import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import client from '../api/client';

export default function NewGuest() {
  const navigate = useNavigate();
  const [form, setForm] = useState({ fullName: '', nationalId: '', address: '', phone: '' });
  const [error, setError] = useState('');

  const handleChange = (e) => setForm({ ...form, [e.target.name]: e.target.value });

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    try {
      const { data } = await client.post('/guests', form);
      navigate(`/guests/${data.id}`);
    } catch (err) {
      setError(err.response?.data?.detail || 'Failed to create guest.');
    }
  };

  return (
    <div className="px-4 py-4">
      <button onClick={() => navigate(-1)} className="text-blue-600 text-sm mb-4">&larr; Back</button>
      <h1 className="text-xl font-bold text-gray-800 mb-4">New Guest</h1>

      {error && <p className="text-sm text-red-600 bg-red-50 rounded-lg p-3 mb-4">{error}</p>}

      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Full Name</label>
          <input name="fullName" value={form.fullName} onChange={handleChange} className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" required />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">National ID</label>
          <input name="nationalId" value={form.nationalId} onChange={handleChange} className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" required />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Address</label>
          <input name="address" value={form.address} onChange={handleChange} className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" required />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Phone (optional)</label>
          <input name="phone" value={form.phone} onChange={handleChange} className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" />
        </div>
        <button type="submit" className="w-full bg-blue-600 text-white py-2.5 rounded-lg text-sm font-medium hover:bg-blue-700">Create Guest</button>
      </form>
    </div>
  );
}
