import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import Layout from './components/Layout';
import Login from './pages/Login';
import Bookings from './pages/Bookings';
import BookingDetail from './pages/BookingDetail';
import NewBooking from './pages/NewBooking';
import Guests from './pages/Guests';
import GuestDetail from './pages/GuestDetail';
import NewGuest from './pages/NewGuest';
import Rooms from './pages/Rooms';
import Reports from './pages/Reports';

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route element={<ProtectedRoute><Layout /></ProtectedRoute>}>
            <Route path="/bookings" element={<Bookings />} />
            <Route path="/bookings/new" element={<NewBooking />} />
            <Route path="/bookings/:id" element={<BookingDetail />} />
            <Route path="/guests" element={<Guests />} />
            <Route path="/guests/new" element={<NewGuest />} />
            <Route path="/guests/:id" element={<GuestDetail />} />
            <Route path="/rooms" element={<Rooms />} />
            <Route path="/reports" element={<ProtectedRoute role="Owner"><Reports /></ProtectedRoute>} />
            <Route path="/" element={<Navigate to="/bookings" replace />} />
          </Route>
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}
