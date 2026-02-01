# Customer Spending Insights

## Overview
Customer Spending Insights is a full‑stack financial analytics platform designed to provide near‑real‑time insight into customer spending behaviour.  

It simulates a modern enterprise banking analytics stack with a strongly‑typed .NET API backend and a high‑performance React + TypeScript frontend.

---

## Dashboard

![Dashboard](screenshots/dashboard.png)

## High‑Level Architecture

The platform consists of three primary layers:

1. **Frontend (React + TypeScript)**
   - Data visualisation (charts, tables, KPIs)
   - Period and date‑range filtering
   - Category and transaction exploration

2. **Backend API (.NET 10, ASP.NET Core)**
   - Query orchestration
   - Business‑logic driven aggregation
   - Deterministic date‑range resolution
   - Data shaping and contract enforcement

3. **Container Layer (Docker Compose)**
   - NGINX‑based frontend delivery
   - Kestrel‑hosted API
   - Network‑isolated service communication

---

## Technology Stack

| Layer | Technology |
|------|-----------|
| Frontend | React, TypeScript, Vite, Tailwind, Recharts |
| API | ASP.NET Core (.NET 10), C# |
| Data | In‑memory simulated dataset |
| Networking | REST, JSON |
| Containerisation | Docker, Docker Compose |
| Reverse Proxy | NGINX |

---

## Data Flow

1. The frontend requests data from `/api/customers/{id}` endpoints.
2. The API resolves the requested date range using `DateRangeService`.
3. Transactions are filtered, aggregated and grouped server‑side.
4. Domain services return strongly‑typed DTOs.
5. The frontend renders KPIs, charts and tables from the API response.

This guarantees:
- A single source of truth for time windows
- No duplicated aggregation logic in the UI
- Predictable analytics results

---

## Running the System

### Prerequisites
- Docker Desktop
- Git

### Start the full stack

```bash
docker compose up --build
```

This will start:
- API on `http://localhost:8080`
- Web UI on `http://localhost:5173`

---

## Local Development (Optional)

### Backend
```bash
cd apps/api
dotnet run
```

### Frontend
```bash
cd apps/web
npm install
npm run dev
```

---

## Testing

### API Tests
```bash
cd apps/api
dotnet test
```

### Frontend Type Safety
```bash
cd apps/web
npm run build
```

---

## API Overview

| Endpoint | Description |
|--------|-------------|
| `/customers/{id}/profile` | Customer metadata |
| `/customers/{id}/spending/summary` | KPIs |
| `/customers/{id}/spending/categories` | Category breakdown |
| `/customers/{id}/spending/trends` | Monthly trends |
| `/customers/{id}/transactions` | Transaction listing |

All endpoints share a unified date‑range model.

---

## Date Range Engine

The backend supports:
- `7d`, `30d`, `90d`, `1y`
- Custom `startDate` / `endDate`

Rules:
- Custom ranges override periods
- Missing endDate defaults to today
- Missing startDate defaults to 30‑day lookback
- Validation guarantees `startDate ≤ endDate`

This prevents UI‑driven data corruption.

---

## Security & Stability Considerations

- Backend performs all filtering and aggregation
- UI never computes financial totals
- All queries validated server‑side
- Strict typing on all API contracts

---

## Enterprise Design Principles Demonstrated

- Separation of concerns
- Deterministic time windows
- Domain‑driven service boundaries
- Stateless API design
- Contract‑first API usage
- Container‑first deployment model

---
