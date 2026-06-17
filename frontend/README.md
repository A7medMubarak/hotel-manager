# Hotel Manager — Frontend

React SPA for the Hotel Manager system. Built with React 19, Vite 8, Tailwind CSS 4, and React Router 7.

## Overview

Mobile-first front-desk interface for hotel staff. Features include:

- **Login & Role-based UI** — JWT authentication with Owner/Employee role gating
- **Bookings** — View, search, create, extend, complete, and cancel reservations
- **Guests** — Register, search, and manage guest profiles
- **Rooms** — Browse room inventory with real-time status (Available / Occupied / Maintenance)
- **Reports** — Daily, weekly, monthly, and outstanding balance reports (Owner only)

## Tech Stack

- **React 19** with hooks and context-based state management
- **Vite 8** for development server and optimized builds
- **Tailwind CSS 4** for utility-first responsive styling
- **React Router 7** for declarative routing with nested layouts
- **Axios** for HTTP client with automatic JWT interceptor

## Development

```bash
npm install
npm run dev
```

The dev server runs at `http://localhost:5174` and proxies API requests to the backend.

## Build

```bash
npm run build    # outputs to dist/
npm run preview  # preview the production build
```

## Project Structure

```
src/
├── api/
│   └── client.js          ← Axios instance with JWT interceptor
├── components/
│   ├── Layout.jsx         ← App shell with navigation bar
│   └── ProtectedRoute.jsx ← Auth guard component
├── context/
│   └── AuthContext.jsx    ← Authentication state & login/logout
├── pages/
│   ├── Login.jsx          ← Login form
│   ├── Bookings.jsx       ← Booking list with filters
│   ├── BookingDetail.jsx  ← Single booking view with payments
│   ├── NewBooking.jsx     ← Create booking form
│   ├── Guests.jsx         ← Guest list with search
│   ├── GuestDetail.jsx    ← Single guest view
│   ├── NewGuest.jsx       ← Register guest form
│   ├── Rooms.jsx          ← Room inventory & status
│   └── Reports.jsx        ← Operational reports (Owner only)
├── App.jsx                ← Route definitions
├── main.jsx               ← Entry point
└── index.css              ← Tailwind imports & global styles
```
