# OnlyCars

> **This project is currently in active development.** The final goal is to build a complete, production-ready microservices backend with ASP.NET Core and a Next.js frontend for a real-time car auction platform.

---

## What is OnlyCars?

OnlyCars is a car auction platform where users can list vehicles for sale, search and filter active auctions, and place bids. The system is designed around a microservices architecture with event-driven communication, enabling each service to scale and deploy independently.

---

## Architecture Overview

```
┌──────────────────┐     RabbitMQ      ┌──────────────────┐
│  Auction Service │ ────────────────► │  Search Service  │
│  (PostgreSQL)    │                   │  (MongoDB)       │
└──────────────────┘                   └──────────────────┘
        ▲                                       ▲
        │                HTTP (Polly retry)     │
        └───────────────────────────────────────┘
                   (initial data sync)

Infrastructure:
  PostgreSQL  ·  MongoDB  ·  RabbitMQ
```

---

## Services

### AuctionService

Manages the core auction lifecycle. Exposes a REST API for creating, reading, updating, and deleting auctions. Publishes domain events to RabbitMQ when auctions change.

- **Runtime**: .NET 10 Web API
- **Database**: PostgreSQL via Entity Framework Core
- **Messaging**: MassTransit + RabbitMQ
- **Port**: `7001`

**Endpoints**

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/auctions` | List all auctions (optional `date` filter) |
| GET | `/api/auctions/{id}` | Get a single auction |
| POST | `/api/auctions` | Create a new auction |
| PUT | `/api/auctions/{id}` | Update auction details |
| DELETE | `/api/auctions/{id}` | Delete an auction |

**Events Published**

- `AuctionCreated`
- `AuctionUpdated`
- `AuctionDeleted`

---

### SearchService

Maintains a denormalized, searchable view of all auctions in MongoDB. Consumes events from RabbitMQ to stay in sync with AuctionService. On startup, it also pulls data directly from AuctionService via HTTP (with Polly exponential backoff) to handle any events missed while offline.

- **Runtime**: .NET 10 Web API
- **Database**: MongoDB (with full-text indexes on Make, Model, Color)
- **Messaging**: MassTransit + RabbitMQ (consumer)
- **Resilience**: Polly retry policy on HTTP sync

**Endpoints**

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/search` | Search and filter auctions |

**Query Parameters**

| Parameter | Description |
|-----------|-------------|
| `searchTerm` | Full-text search across Make, Model, Color |
| `seller` | Filter by seller username |
| `winner` | Filter by winner username |
| `filterBy` | `finished`, `endingSoon`, `active` |
| `orderBy` | `make`, `new`, `endingSoon` |
| `pageNumber` | Page number (default: 1) |
| `pageSize` | Results per page (default: 4) |

---

### Contracts

A shared class library with no external dependencies, containing the MassTransit event contracts used by both services.

- `AuctionCreated`
- `AuctionUpdated`
- `AuctionDeleted`

---

## Data Models

### Auction (PostgreSQL)

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | Primary key |
| ReservePrice | int | Minimum acceptable sale price |
| Seller | string | Seller username |
| Winner | string? | Set when auction closes |
| SoldAmount | int? | Final sale price |
| CurrentPrice | int? | Highest bid so far |
| CreatedAt | DateTime | |
| UpdatedAt | DateTime | |
| AuctionEnd | DateTime | When bidding closes |
| Status | enum | `Live`, `Finished`, `ReserveNotMet` |
| Item | Item | Car details (Make, Model, Year, Color, Mileage, ImageUrl) |

---

## Infrastructure

All infrastructure is defined in `docker-compose.yml` and runs locally via Docker.

| Service | Image | Port |
|---------|-------|------|
| PostgreSQL | `postgres` | `5432` |
| MongoDB | `mongo` | `27017` |
| RabbitMQ | `rabbitmq:3-management-alpine` | `5672` (AMQP), `15672` (UI) |

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Run Infrastructure

```bash
docker compose up -d
```

### Run AuctionService

```bash
cd src/AuctionService
dotnet run
```

### Run SearchService

```bash
cd src/SearchService
dotnet run
```

The services will automatically apply database migrations and seed sample data on first run.

---

## Project Structure

```
OnlyCars/
├── docker-compose.yml
├── OnlyCars.slnx
└── src/
    ├── AuctionService/
    │   ├── Controllers/         # REST API controllers
    │   ├── Data/                # DbContext, migrations, seed data
    │   ├── DTOs/                # Request/response models
    │   ├── Entities/            # Domain models (Auction, Item)
    │   └── RequestHelpers/      # AutoMapper profiles
    ├── SearchService/
    │   ├── Controllers/         # Search endpoint
    │   ├── Consumers/           # RabbitMQ event consumers
    │   ├── Data/                # MongoDB initializer
    │   ├── Models/              # MongoDB document models
    │   ├── RequestHelpers/      # AutoMapper profiles, search params
    │   └── Services/            # HTTP client for data sync
    └── Contracts/               # Shared MassTransit event contracts
```

---

## Roadmap

The following services and features are planned for future implementation:

### Backend Services
- [ ] **Identity Service** — Authentication and authorization (JWT / OAuth2)
- [ ] **Bidding Service** — Real-time bid processing and validation
- [ ] **Notification Service** — Push notifications for bid events
- [ ] **Gateway Service** — API gateway for routing and auth middleware

### Frontend
- [ ] **Next.js App** — Full-featured frontend using Next.js with server-side rendering, real-time bid updates, and user authentication

### Infrastructure
- [ ] Kubernetes deployment manifests
- [ ] CI/CD pipeline
- [ ] Centralized logging and distributed tracing

---

## Tech Stack

| Layer | Technology |
|-------|------------|
| Backend framework | ASP.NET Core (.NET 10) |
| ORM | Entity Framework Core 10 |
| Relational DB | PostgreSQL |
| Document DB | MongoDB |
| Message broker | RabbitMQ |
| Messaging library | MassTransit 8 |
| Object mapping | AutoMapper 16 |
| HTTP resilience | Polly |
| Frontend (planned) | Next.js |
| Containerization | Docker / Docker Compose |
