import { useState, useEffect } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import client from '../api/client';
import { useAuth } from '../context/AuthContext';

export default function Rooms() {
  const [searchParams, setSearchParams] = useSearchParams();
  const { isOwner } = useAuth();
  const [rooms, setRooms] = useState([]);
  const [loading, setLoading] = useState(true);
  const [editingRoomId, setEditingRoomId] = useState(null);
  const [editForm, setEditForm] = useState({});

  const page = parseInt(searchParams.get('page') || '1');
  const status = searchParams.get('status') || '';
  const floor = searchParams.get('floor') || '';

  const fetchRooms = async () => {
    setLoading(true);
    try {
      const params = { page, pageSize: 50 };
      if (status) params.status = status;
      if (floor) params.floor = parseInt(floor);
      const { data } = await client.get('/rooms', { params });
      setRooms(data.items);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchRooms(); }, [searchParams]);

  const updateFilter = (key, value) => {
    const params = new URLSearchParams(searchParams);
    if (value) params.set(key, value);
    else params.delete(key);
    if (key !== 'page') params.set('page', '1');
    setSearchParams(params);
  };

  const toggleMaintenance = async (id) => {
    await client.patch(`/rooms/${id}/maintenance`);
    fetchRooms();
  };

  const startEdit = (room) => {
    setEditingRoomId(room.id);
    setEditForm({
      number: room.number,
      floor: room.floor,
      bedCount: room.bedCount,
      bathroomType: room.bathroomType === 'Ensuite' ? 0 : 1,
      basePricePerNight: room.basePricePerNight,
      notes: room.notes || ''
    });
  };

  const cancelEdit = () => {
    setEditingRoomId(null);
    setEditForm({});
  };

  const handleEditChange = (e) => {
    const { name, value } = e.target;
    setEditForm({ ...editForm, [name]: value });
  };

  const saveEdit = async (id) => {
    try {
      await client.put(`/rooms/${id}`, {
        number: editForm.number,
        floor: parseInt(editForm.floor),
        bedCount: parseInt(editForm.bedCount),
        bathroomType: parseInt(editForm.bathroomType),
        basePricePerNight: parseFloat(editForm.basePricePerNight),
        notes: editForm.notes || null
      });
      cancelEdit();
      fetchRooms();
    } catch (err) {
      alert(err.response?.data?.detail || 'Failed to update room.');
    }
  };

  const statusColor = (s) => {
    switch (s) {
      case 'Available': return 'bg-green-100 text-green-700 ring-green-300';
      case 'Occupied': return 'bg-orange-100 text-orange-700 ring-orange-300';
      case 'Maintenance': return 'bg-red-100 text-red-700 ring-red-300';
      default: return 'bg-gray-100 text-gray-700';
    }
  };

  return (
    <div className="px-4 py-4">
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-xl font-bold text-gray-800">Rooms</h1>
        {isOwner && (
          <Link to="/rooms/new" className="bg-blue-600 text-white text-sm px-4 py-2 rounded-lg font-medium hover:bg-blue-700">
            + New
          </Link>
        )}
      </div>

      <div className="flex gap-2 mb-4 overflow-x-auto pb-2">
        {['', 'Available', 'Occupied', 'Maintenance'].map((s) => (
          <button key={s} onClick={() => updateFilter('status', s)}
            className={`px-3 py-1.5 rounded-full text-xs font-medium whitespace-nowrap ${status === s ? 'bg-blue-600 text-white' : 'bg-gray-100 text-gray-600'}`}>
            {s || 'All'}
          </button>
        ))}
      </div>

      <div className="flex gap-2 mb-4">
        {[1, 2, 3].map((f) => (
          <button key={f} onClick={() => updateFilter('floor', floor === String(f) ? '' : String(f))}
            className={`px-3 py-1.5 rounded-lg text-xs font-medium border ${floor === String(f) ? 'bg-blue-600 text-white border-blue-600' : 'bg-white text-gray-600 border-gray-300'}`}>
            Floor {f}
          </button>
        ))}
      </div>

      {loading ? (
        <p className="text-center text-gray-400 py-8">Loading...</p>
      ) : (
        <div className="grid grid-cols-2 gap-3">
          {rooms.map((r) => (
            editingRoomId === r.id ? (
              <div key={r.id} className="col-span-2 bg-white rounded-xl border-2 border-blue-300 p-4 shadow-sm">
                <p className="text-xs font-semibold text-blue-600 mb-3">Editing Room {r.number}</p>
                <div className="grid grid-cols-2 gap-3 mb-3">
                  <input name="number" value={editForm.number} onChange={handleEditChange} placeholder="Room Number"
                    className="border border-gray-300 rounded-lg px-3 py-2 text-sm" />
                  <select name="floor" value={editForm.floor} onChange={handleEditChange}
                    className="border border-gray-300 rounded-lg px-3 py-2 text-sm">
                    {[1, 2, 3].map((f) => <option key={f} value={f}>Floor {f}</option>)}
                  </select>
                  <select name="bedCount" value={editForm.bedCount} onChange={handleEditChange}
                    className="border border-gray-300 rounded-lg px-3 py-2 text-sm">
                    {[1, 2, 3, 4].map((b) => <option key={b} value={b}>{b} bed{b > 1 ? 's' : ''}</option>)}
                  </select>
                  <select name="bathroomType" value={editForm.bathroomType} onChange={handleEditChange}
                    className="border border-gray-300 rounded-lg px-3 py-2 text-sm">
                    <option value={0}>Ensuite</option>
                    <option value={1}>Shared</option>
                  </select>
                  <input name="basePricePerNight" type="number" step="0.01" min="0.01" value={editForm.basePricePerNight}
                    onChange={handleEditChange} className="border border-gray-300 rounded-lg px-3 py-2 text-sm" />
                  <input name="notes" value={editForm.notes} onChange={handleEditChange} placeholder="Notes"
                    className="border border-gray-300 rounded-lg px-3 py-2 text-sm" />
                </div>
                <div className="flex gap-2">
                  <button onClick={() => saveEdit(r.id)} className="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm font-medium hover:bg-blue-700">Save</button>
                  <button onClick={cancelEdit} className="flex-1 bg-gray-100 text-gray-700 py-2 rounded-lg text-sm hover:bg-gray-200">Cancel</button>
                </div>
              </div>
            ) : (
              <div key={r.id} className={`bg-white rounded-xl border-2 p-4 shadow-sm ${statusColor(r.status).split(' ')[3] || 'border-gray-200'}`}>
                <div className="flex items-start justify-between mb-2">
                  <p className="text-lg font-bold text-gray-800">{r.number}</p>
                  <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${statusColor(r.status)}`}>{r.status}</span>
                </div>
                <p className="text-xs text-gray-500">Floor {r.floor} | {r.bedCount} bed{r.bedCount > 1 ? 's' : ''} | {r.bathroomType}</p>
                <p className="text-sm font-medium text-gray-700 mt-1">EGP {r.basePricePerNight}/night</p>
                {isOwner && r.status !== 'Occupied' && (
                  <div className="flex gap-2 mt-2">
                    <button onClick={() => toggleMaintenance(r.id)} className="text-xs text-red-500 hover:text-red-700">
                      {r.isUnderMaintenance ? 'Set Available' : 'Set Maintenance'}
                    </button>
                    <button onClick={() => startEdit(r)} className="text-xs text-blue-500 hover:text-blue-700">
                      Edit
                    </button>
                  </div>
                )}
              </div>
            )
          ))}
        </div>
      )}
    </div>
  );
}
