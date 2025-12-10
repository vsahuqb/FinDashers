# Docker Setup Guide for FinDashers

This guide explains how to set up and run Redis using Docker Compose.

## Prerequisites

- Docker Desktop installed and running
- Docker Compose (usually comes with Docker Desktop)

## Quick Start

### 1. Start Redis

From the project root directory, run:

```bash
docker-compose up -d
```

This will:
- Pull the latest Redis image
- Create a Redis container named `findashers-redis`
- Expose Redis on port `6379`
- Enable data persistence with AOF (Append Only File)
- Create a health check that verifies Redis is running

### 2. Verify Redis is Running

Check the status of the container:

```bash
docker-compose ps
```

You should see the `findashers-redis` container with status `Up`.

Test the connection:

```bash
docker exec findashers-redis redis-cli ping
```

Expected output: `PONG`

### 3. Stop Redis

```bash
docker-compose down
```

This stops and removes the containers but preserves the data volume.

### 4. Remove Everything (including data)

```bash
docker-compose down -v
```

This removes containers, networks, and volumes.

## Configuration

### Redis Connection String

The default connection string in `appsettings.json` is:

```
localhost:6379
```

If you need to change the port, update both:
1. `docker-compose.yml` - Change the port mapping (e.g., `"6380:6379"`)
2. `appsettings.json` - Update the Redis connection string accordingly

### Data Persistence

Redis data is stored in a Docker volume named `redis_data`. This means:
- Data persists even if the container is stopped
- Data is lost only when you run `docker-compose down -v`

## Running the Applications

### 1. Start Redis

```bash
docker-compose up -d
```

### 2. Run FinDashers.API

```bash
dotnet run --project FinDashers.API
```

The API will be available at `https://localhost:5001`

### 3. Run FinDashers.Worker

In a new terminal:

```bash
dotnet run --project FinDashers.Worker
```

The worker will start consuming webhook events from the Redis Stream.

## Monitoring Redis

### View Redis Logs

```bash
docker logs findashers-redis
```

### Connect to Redis CLI

```bash
docker exec -it findashers-redis redis-cli
```

Then you can run Redis commands:

```
> XINFO STREAM webhook-events
> XLEN webhook-events
> XRANGE webhook-events - +
> XINFO GROUPS webhook-events
```

## Troubleshooting

### Redis Connection Failed

1. Verify Docker is running:
   ```bash
   docker ps
   ```

2. Check if the container is running:
   ```bash
   docker-compose ps
   ```

3. View container logs:
   ```bash
   docker logs findashers-redis
   ```

4. Restart the container:
   ```bash
   docker-compose restart
   ```

### Port Already in Use

If port 6379 is already in use:

1. Find what's using the port:
   ```bash
   netstat -ano | findstr :6379
   ```

2. Either stop that process or change the port in `docker-compose.yml`:
   ```yaml
   ports:
     - "6380:6379"  # Use 6380 instead
   ```

3. Update `appsettings.json`:
   ```json
   "Redis": "localhost:6380"
   ```

### Data Persistence Issues

To reset Redis data:

```bash
docker-compose down -v
docker-compose up -d
```

## Architecture Overview

```
┌─────────────────────────────────────────────────────────┐
│                    FinDashers.API                       │
│  (Receives Adyen webhooks, publishes to Redis Stream)   │
└──────────────────┬──────────────────────────────────────┘
                   │ Publishes to
                   │ webhook-events stream
                   ▼
        ┌──────────────────────┐
        │   Redis Container    │
        │  (Stream Storage)     │
        │  Port: 6379          │
        └──────────────────────┘
                   │ Consumes from
                   │ webhook-events stream
                   ▼
┌─────────────────────────────────────────────────────────┐
│                  FinDashers.Worker                      │
│  (Consumes from Redis, saves to PostgreSQL)             │
└─────────────────────────────────────────────────────────┘
```

## Next Steps

1. Ensure PostgreSQL is running (update docker-compose.yml if needed)
2. Run database migrations
3. Start the API and Worker services
4. Send test webhook events to verify the flow

For more information, see the main [README.md](README.md)
