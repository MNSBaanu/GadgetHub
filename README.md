# GadgetHub

A Service-Oriented Architecture (SOA) e-commerce platform for gadgets. The system connects to three distributors — ElectroCom, TechWorld, and GadgetCentral — and automatically compares prices to get the best deal for customers.

## Tech Stack

- **Backend:** ASP.NET Core Web API (.NET 8)
- **Frontend:** ASP.NET Core Razor Pages (.NET 8)
- **Database:** SQL Server + Entity Framework Core

## Services

| Service | Description |
|---|---|
| GadgetHubAPI | Main API — orders, customers, quotation comparison |
| ElectroComAPI | Distributor API |
| TechWorldAPI | Distributor API |
| GadgetCentralAPI | Distributor API |
| GadgetHubWeb | Customer-facing web app |

## Credentials

| Role | Email | Password |
|---|---|---|
| Admin | admin@gadgethub.com | admin123 |
| User | user1@gmail.com | user123 |

## Run Locally

1. Restore the `.bacpac` files from `Database/` into SQL Server
2. Update connection strings in each `appsettings.json`
3. Run all 5 services with `dotnet run`
