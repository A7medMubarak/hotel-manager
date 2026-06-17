import { useState, useEffect } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import client from '../api/client';

export default function Guests() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [guests, setGuests] = useState([]);
  const [totalPages, setTotalPages] = useState(0);
  const [loading, setLoading] = useState(true);

  const page = parseInt(searchParams.get('page') || '1');
  const search = searchParams.get('search') || '';
  const phone = searchParams.get('phone') || '';

  const fetchGuests = async () => {
    setLoading(true);
    try {
      const params = { page, pageSize: 20 };
      if (search) params.search = search;
      if (phone) params.phone = phone;
      const { data } = await client.get('/guests', { params });
      setGuests(data.items);
      setTotalPages(data.totalPages);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchGuests(); }, [searchParams]);

  const updateFilter = (key, value) => {
    const params = new URLSearchParams(searchParams);
    if (value) params.set(key, value);
    else params.delete(key);
    if (key !== 'page') params.set('page', '1');
    setSearchParams(params);
  };

  return (
    <div className="px-4 py-4">
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-xl font-bold text-gray-800">Guests</h1>
        <Link to="/guests/new" className="bg-blue-600 text-white text-sm px-4 py-2 rounded-lg font-medium">+ New</Link>
      </div>

      <div className="flex gap-2 mb-4">
        <input
          placeholder="Search name or National ID..."
          value={search}
          onChange={(e) => updateFilter('search', e.target.value)}
          className="flex-1 border border-gray-300 rounded-lg px-3 py-1.5 text-sm"
        />
        <input
          placeholder="Phone..."
          value={phone}
          onChange={(e) => updateFilter('phone', e.target.value)}
          className="w-28 border border-gray-300 rounded-lg px-3 py-1.5 text-sm"
        />
      </div>

      {loading ? (
        <p className="text-center text-gray-400 py-8">Loading...</p>
      ) : guests.length === 0 ? (
        <p className="text-center text-gray-400 py-8">No guests found.</p>
      ) : (
        <div className="space-y-3">
          {guests.map((g) => (
            <Link key={g.id} to={`/guests/${g.id}`} className="block bg-white rounded-xl border border-gray-200 p-4 shadow-sm hover:shadow-md transition-shadow">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-full bg-blue-100 flex items-center justify-center text-blue-600 font-medium">
                  {g.fullName.charAt(0)}
                </div>
                <div className="flex-1">
                  <p className="font-medium text-gray-800">{g.fullName}</p>
                  <p className="text-xs text-gray-400">{g.nationalId} {g.phone && `| ${g.phone}`}</p>
                </div>
              </div>
            </Link>
          ))}
        </div>
      )}

      {totalPages > 1 && (
        <div className="flex justify-center items-center gap-2 mt-6">
          <button onClick={() => updateFilter('page', String(page - 1))} disabled={page <= 1} className="px-3 py-1.5 text-sm border rounded-lg disabled:opacity-30">Prev</button>
          <span className="text-sm text-gray-500">{page} / {totalPages}</span>
          <button onClick={() => updateFilter('page', String(page + 1))} disabled={page >= totalPages} className="px-3 py-1.5 text-sm border rounded-lg disabled:opacity-30">Next</button>
        </div>
      )}
    </div>
  );
}
