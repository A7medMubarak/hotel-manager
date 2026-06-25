import { useState, useEffect } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import client from '../api/client';
import { useAuth } from '../context/AuthContext';
import { useTranslation } from 'react-i18next';

const STATUS_OPTIONS = ['', 'Available', 'Occupied', 'Maintenance'];

export default function Rooms() {
  const [searchParams, setSearchParams] = useSearchParams();
  const { isOwner } = useAuth();
  const { t } = useTranslation();
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
      alert(err.response?.data?.detail || t('rooms.updateFailed'));
    }
  };

  const statusLabel = (s) => {
    switch (s) {
      case 'Available': return t('rooms.available');
      case 'Occupied': return t('rooms.occupied');
      case 'Maintenance': return t('rooms.maintenance');
      default: return t('rooms.all');
    }
  };

  const statusColor = (s) => {
    switch (s) {
      case 'Available': return 'bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-300';
      case 'Occupied': return 'bg-orange-100 dark:bg-orange-900/30 text-orange-700 dark:text-orange-300';
      case 'Maintenance': return 'bg-red-100 dark:bg-red-900/30 text-red-700 dark:text-red-300';
      default: return 'bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-dark-muted';
    }
  };

  return (
    <div className="px-4 py-4">
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-xl font-bold text-gray-800 dark:text-dark-text">{t('rooms.title')}</h1>
        {isOwner && (
          <Link to="/rooms/new" className="bg-blue-600 dark:bg-dark-accent dark:text-gray-900 text-white text-sm px-4 py-2 rounded-lg font-medium hover:bg-blue-700 dark:hover:bg-blue-300 transition-colors">
            {t('rooms.new')}
          </Link>
        )}
      </div>

      <div className="flex gap-2 mb-4 overflow-x-auto pb-2">
        {STATUS_OPTIONS.map((s) => (
          <button key={s} onClick={() => updateFilter('status', s)}
            className={`px-3 py-1.5 rounded-full text-xs font-medium whitespace-nowrap transition-colors ${status === s ? 'bg-blue-600 dark:bg-dark-accent dark:text-gray-900 text-white' : 'bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-dark-muted'}`}>
            {statusLabel(s)}
          </button>
        ))}
      </div>

      <div className="flex gap-2 mb-4">
        {[1, 2, 3].map((f) => (
          <button key={f} onClick={() => updateFilter('floor', floor === String(f) ? '' : String(f))}
            className={`px-3 py-1.5 rounded-lg text-xs font-medium border transition-colors ${floor === String(f) ? 'bg-blue-600 dark:bg-dark-accent dark:text-gray-900 text-white border-blue-600 dark:border-dark-accent' : 'bg-white dark:bg-gray-800 text-gray-600 dark:text-dark-muted border-gray-300 dark:border-gray-600'}`}>
            {t('rooms.floor')}{f}
          </button>
        ))}
      </div>

      {loading ? (
        <p className="text-center text-gray-400 dark:text-dark-muted py-8">{t('app.loading')}</p>
      ) : (
        <div className="grid grid-cols-2 gap-3">
          {rooms.map((r) => (
            editingRoomId === r.id ? (
              <div key={r.id} className="col-span-2 bg-white dark:bg-dark-surface rounded-xl border-2 border-blue-300 dark:border-dark-accent p-4 shadow-sm dark:shadow-gray-900 transition-colors">
                <p className="text-xs font-semibold text-blue-600 dark:text-dark-accent mb-3">{t('rooms.editing')}{r.number}</p>
                <div className="grid grid-cols-2 gap-3 mb-3">
                  <input name="number" value={editForm.number} onChange={handleEditChange} placeholder={t('rooms.roomNumber')}
                    className="border border-gray-300 dark:border-gray-600 dark:bg-gray-800 dark:text-dark-text rounded-lg px-3 py-2 text-sm" />
                  <select name="floor" value={editForm.floor} onChange={handleEditChange}
                    className="border border-gray-300 dark:border-gray-600 dark:bg-gray-800 dark:text-dark-text rounded-lg px-3 py-2 text-sm">
                    {[1, 2, 3].map((f) => <option key={f} value={f}>{t('rooms.floor')}{f}</option>)}
                  </select>
                  <select name="bedCount" value={editForm.bedCount} onChange={handleEditChange}
                    className="border border-gray-300 dark:border-gray-600 dark:bg-gray-800 dark:text-dark-text rounded-lg px-3 py-2 text-sm">
                    {[1, 2, 3, 4].map((b) => <option key={b} value={b}>{b} {b > 1 ? t('rooms.bedCount') + 's' : t('rooms.bedCount')}</option>)}
                  </select>
                  <select name="bathroomType" value={editForm.bathroomType} onChange={handleEditChange}
                    className="border border-gray-300 dark:border-gray-600 dark:bg-gray-800 dark:text-dark-text rounded-lg px-3 py-2 text-sm">
                    <option value={0}>{t('rooms.ensuite')}</option>
                    <option value={1}>{t('rooms.shared')}</option>
                  </select>
                  <input name="basePricePerNight" type="number" step="0.01" min="0.01" value={editForm.basePricePerNight}
                    onChange={handleEditChange} className="border border-gray-300 dark:border-gray-600 dark:bg-gray-800 dark:text-dark-text rounded-lg px-3 py-2 text-sm" />
                  <input name="notes" value={editForm.notes} onChange={handleEditChange} placeholder={t('app.notesOptional')}
                    className="border border-gray-300 dark:border-gray-600 dark:bg-gray-800 dark:text-dark-text rounded-lg px-3 py-2 text-sm" />
                </div>
                <div className="flex gap-2">
                  <button onClick={() => saveEdit(r.id)} className="flex-1 bg-blue-600 dark:bg-dark-accent dark:text-gray-900 text-white py-2 rounded-lg text-sm font-medium hover:bg-blue-700 dark:hover:bg-blue-300 transition-colors">{t('rooms.save')}</button>
                  <button onClick={cancelEdit} className="flex-1 bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-dark-muted py-2 rounded-lg text-sm hover:bg-gray-200 dark:hover:bg-gray-600 transition-colors">{t('rooms.cancel')}</button>
                </div>
              </div>
            ) : (
              <div key={r.id} className={`bg-white dark:bg-dark-surface rounded-xl border-2 p-4 shadow-sm dark:shadow-gray-900 transition-colors ${statusColor(r.status).split(' ')[0].replace('bg-', '').replace('dark:', '').replace('/30', '') ? '' : 'border-gray-200 dark:border-gray-700'}`}>
                <div className="flex items-start justify-between mb-2">
                  <p className="text-lg font-bold text-gray-800 dark:text-dark-text">{r.number}</p>
                  <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${statusColor(r.status)}`}>{statusLabel(r.status)}</span>
                </div>
                <p className="text-xs text-gray-500 dark:text-dark-muted">{t('rooms.floor')}{r.floor} | {r.bedCount} {r.bedCount > 1 ? t('rooms.bedCount') + 's' : t('rooms.bedCount')} | {r.bathroomType === 'Ensuite' ? t('rooms.ensuite') : t('rooms.shared')}</p>
                <p className="text-sm font-medium text-gray-700 dark:text-dark-text mt-1">EGP {r.basePricePerNight}/night</p>
                {isOwner && r.status !== 'Occupied' && (
                  <div className="flex gap-2 mt-2">
                    <button onClick={() => toggleMaintenance(r.id)} className="text-xs text-red-500 dark:text-red-400 hover:text-red-700 dark:hover:text-red-300">
                      {r.isUnderMaintenance ? t('rooms.setAvailable') : t('rooms.setMaintenance')}
                    </button>
                    <button onClick={() => startEdit(r)} className="text-xs text-blue-500 dark:text-dark-accent hover:text-blue-700 dark:hover:text-blue-300">
                      {t('rooms.edit')}
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
