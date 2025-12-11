# FinDashers Setup Guide

## Prerequisites
- .NET 8.0 SDK
- Redis Server
- PostgreSQL (or SQLite for development)

## Quick Start

### 1. Redis Server
```bash
# Windows (using Docker)
docker run -d -p 6379:6379 redis:latest

# Or install Redis for Windows
# Download from: https://github.com/microsoftarchive/redis/releases
```

### 2. Database Setup
```bash
# Run the dummy data script (optional for testing)
psql -U your_username -d your_database -f insert_adyen_dummy_data.sql
```

### 3. API Configuration
Update `FinDashers.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "your_database_connection_string"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "LLMProviders": {
    "OpenAI": {
      "ApiKey": "your_openai_key"
    }
  }
}
```

### 4. Run Components

#### Terminal 1 - API Server
```bash
cd FinDashers.API
dotnet run
# API available at: http://localhost:5144
```

#### Terminal 2 - Background Worker
```bash
cd FinDashers.Worker
dotnet run
# Worker processes Redis streams
```

#### Terminal 3 - Dashboard (Optional)
```bash
cd FinDashers.API
# Open ai_payment_dashboard.html in browser
# Or serve via: python -m http.server 8000
```

## API Endpoints

### Dashboard
```
GET /api/Dashboard?StartDate=2025-12-01&EndDate=2025-12-31&LocationId=115
```

### NL2SQL
```
POST /api/Query
{
  "question": "Show me all payments for location 115 today",
  "provider": "openai"
}
```

## Testing Redis Streams
```bash
# Add test webhook event
redis-cli XADD webhook_events * eventCode AUTHORISATION success true amount 1000 locationId 115
```

## Troubleshooting

### Port Conflicts
- API: Change port in `launchSettings.json`
- Redis: Use different port with `--port 6380`

### Database Connection
- Ensure connection string is correct
- Check database server is running
- Verify user permissions

### Redis Connection
- Verify Redis server is running: `redis-cli ping`
- Check connection string format
- Ensure no firewall blocking port 6379