# FinDashers - NL2SQL for Adyen Payment Data

## Overview
FinDashers is a Natural Language to SQL system specifically designed for querying Adyen payment webhook data in QSR (Quick Service Restaurant) environments. It allows users to ask natural language questions about payment transactions, revenue analytics, and operational metrics.

## Project Structure

```
FinDashers/
├── FinDashers.API/          # Web API project
├── FinDashers.Core/         # Core business logic
├── data/
│   ├── domains/             # Domain context configurations
│   ├── entities/            # Entity-to-ID mappings
│   └── databases/           # SQLite databases
├── FinDashers.sln          # Solution file
└── README.md               # This file
```

## Features
- Multi-LLM provider support (OpenAI, Anthropic, Azure OpenAI)
- Domain-specific query processing for payment data
- Entity resolution for locations, employees, terminals
- Real-time payment analytics
- RESTful API endpoints

## Getting Started
1. Clone the repository
2. Add your LLM provider API keys to appsettings.json
3. Run `dotnet build` to build the solution
4. Run `dotnet run --project FinDashers.API` to start the API

## Example Queries
- "Show me all payments for location 115 today"
- "What's the total revenue processed by employee 3836?"
- "How many failed payments do we have?"
- "Which terminal processed the most transactions?"