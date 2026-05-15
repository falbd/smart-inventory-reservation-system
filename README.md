# Smart Inventory Reservation System

A full-stack real-time inventory reservation system built with .NET 8, Vue 3, Redis, SignalR, Hangfire, Docker, and SQL Server.

---

## Features

- JWT Authentication & Authorization
- Admin and Staff roles
- Product management
- Inventory reservation system
- Real-time stock updates with SignalR
- Background reservation expiration jobs using Hangfire
- Redis distributed locking
- Dockerized backend services
- Responsive Vue 3 dashboard UI
- Toast notifications
- SQL Server database integration

---

## Tech Stack

### Backend
- .NET 8 Minimal API
- Entity Framework Core
- SQL Server
- Redis
- Hangfire
- SignalR
- JWT Authentication

### Frontend
- Vue 3
- Vue Router
- Pinia
- Axios
- Vue Sonner

### DevOps
- Docker
- Docker Compose

---

## Architecture

```text
Vue 3 Frontend
       |
       v
.NET 8 Minimal API
       |
-------------------------
| SQL Server | Redis |
-------------------------
       |
   Hangfire Jobs
       |
    SignalR Hub
```

---

## Running the Project

### Backend

```bash
docker compose up --build
```

Backend URLs:

```text
http://localhost:8080/swagger
http://localhost:8080/hangfire
```

---

### Frontend

```bash
cd smart-inventory-dashboard
npm install
npm run dev
```

Frontend URL:

```text
http://localhost:5173
```

---

## Docker Services

- API Container
- SQL Server Container
- Redis Container

---

## Screenshots

_Add screenshots here later._

---

## Future Improvements

- Kubernetes deployment
- Unit and integration tests
- Email notifications
- Reservation analytics dashboard

---

## Author

Fahad Albedah
