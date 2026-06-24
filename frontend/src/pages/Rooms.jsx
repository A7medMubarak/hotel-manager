import { useState, useEffect } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import client from '../api/client';
import { useAuth } from '../context/AuthContext';

export default function Rooms() {
  const [searchParams, setSearchParams] = useSearchParams();
  const { isOwner } = useAuth();
  const [rooms, setRooms] = useState([]);
  const [loading, setLoading] = useState(true);

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
            <div key={r.id} className={`bg-white rounded-xl border-2 p-4 shadow-sm ${statusColor(r.status).split(' ')[3] || 'border-gray-200'}`}>
              <div className="flex items-start justify-between mb-2">
                <p className="text-lg font-bold text-gray-800">{r.number}</p>
                <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${statusColor(r.status)}`}>{r.status}</span>
              </div>
              <p className="text-xs text-gray-500">Floor {r.floor} | {r.bedCount} bed{r.bedCount > 1 ? 's' : ''} | {r.bathroomType}</p>
              <p className="text-sm font-medium text-gray-700 mt-1">EGP {r.basePricePerNight}/night</p>
              {isOwner && r.status !== 'Occupied' && (
                <button onClick={() => toggleMaintenance(r.id)} className="mt-2 text-xs text-red-500 hover:text-red-700">
                  {r.isUnderMaintenance ? 'Set Available' : 'Set Maintenance'}
                </button>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
