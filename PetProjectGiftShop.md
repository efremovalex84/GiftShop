# GiftShop — How to Run

A pet project with an ASP.NET Core (.NET 10) Web API backend and a React + Vite (TypeScript) frontend, backed by PostgreSQL.

## Prerequisites

- [.NET SDK 10.0](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/) (includes npm)
- [PostgreSQL](https://www.postgresql.org/download/) running locally

## 1. Set up the database

The API expects PostgreSQL on `localhost:5432` with the following credentials (see [src/GiftShop.Api/appsettings.json](src/GiftShop.Api/appsettings.json)):

- Database: `giftshop`
- Username: `postgres`
- Password: `postgres`

Create the database (the API applies migrations and seeds data automatically on startup in Development):

```powershell
createdb -U postgres giftshop
```

To use different credentials, edit the `ConnectionStrings:Default` value in [src/GiftShop.Api/appsettings.json](src/GiftShop.Api/appsettings.json).

## 2. Run the backend API

From the repository root:

```powershell
dotnet run --project src/GiftShop.Api
```

- API runs at: `http://localhost:5284`
- OpenAPI (Development): `http://localhost:5284/openapi/v1.json`

On first run in the Development environment, the app automatically applies EF Core migrations and seeds sample data.

## 3. Run the frontend

In a separate terminal:

```powershell
cd frontend
npm install
npm run dev
```

- Frontend runs at: `http://localhost:5173`

The frontend calls the API at `http://localhost:5284` (configured in [frontend/src/api.ts](frontend/src/api.ts)), and the API allows CORS from `http://localhost:5173`.

## 4. Open the app

Navigate to `http://localhost:5173` in your browser. The frontend provides Products, Cart, and Order pages.

## Running the tests

Integration tests live in [tests/GiftShop.IntegrationTests](tests/GiftShop.IntegrationTests):

```powershell
dotnet test
```

## Production build (frontend)

```powershell
cd frontend
npm run build
npm run preview
```

## Ports summary

| Service      | URL                       |
| ------------ | ------------------------- |
| Backend API  | http://localhost:5284     |
| Backend HTTPS| https://localhost:7146    |
| Frontend     | http://localhost:5173     |
| PostgreSQL   | localhost:5432            |
