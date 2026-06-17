import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import client from '../api/client';

export default function GuestDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
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
      setError('Guest not found.');
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
      setError(err.response?.data?.detail || 'Update failed.');
    }
  };

  if (error) return <p className="text-center text-red-500 py-8">{error}</p>;
  if (!guest) return <p className="text-center text-gray-400 py-8">Loading...</p>;

  return (
    <div className="px-4 py-4">
      <button onClick={() => navigate(-1)} className="text-blue-600 text-sm mb-4">&larr; Back</button>

      {editing ? (
        <form onSubmit={handleUpdate} className="bg-white rounded-xl border border-gray-200 p-4 shadow-sm space-y-4">
          <h1 className="text-xl font-bold text-gray-800">Edit Guest</h1>
          {error && <p className="text-sm text-red-600 bg-red-50 rounded-lg p-3">{error}</p>}
          {['fullName', 'nationalId', 'address', 'phone'].map((field) => (
            <div key={field}>
              <label className="block text-sm font-medium text-gray-700 mb-1 capitalize">{field.replace(/([A-Z])/g, ' $1')}</label>
              <input name={field} value={form[field]} onChange={(e) => setForm({ ...form, [e.target.name]: e.target.value })} className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" required={field !== 'phone'} />
            </div>
          ))}
          <div className="flex gap-2">
            <button type="submit" className="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm font-medium hover:bg-blue-700">Save</button>
            <button type="button" onClick={() => setEditing(false)} className="flex-1 bg-gray-100 text-gray-700 py-2 rounded-lg text-sm hover:bg-gray-200">Cancel</button>
          </div>
        </form>
      ) : (
        <div className="bg-white rounded-xl border border-gray-200 p-4 shadow-sm">
          <div className="flex items-start justify-between mb-4">
            <div className="flex items-center gap-3">
              <div className="w-12 h-12 rounded-full bg-blue-100 flex items-center justify-center text-blue-600 text-lg font-medium">{guest.fullName.charAt(0)}</div>
              <div>
                <h1 className="text-xl font-bold text-gray-800">{guest.fullName}</h1>
                <p className="text-sm text-gray-400">{guest.nationalId}</p>
              </div>
            </div>
            <button onClick={() => setEditing(true)} className="text-blue-600 text-sm font-medium">Edit</button>
          </div>
          <div className="space-y-2 text-sm">
            <p><span className="text-gray-500">Address:</span> <span className="text-gray-800">{guest.address}</span></p>
            <p><span className="text-gray-500">Phone:</span> <span className="text-gray-800">{guest.phone || '—'}</span></p>
            <p><span className="text-gray-500">Registered:</span> <span className="text-gray-800">{new Date(guest.createdAt).toLocaleDateString()}</span></p>
          </div>
        </div>
      )}
    </div>
  );
}
