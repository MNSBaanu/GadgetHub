# GadgetHub - Service-Oriented Architecture (SOA) Solution

A comprehensive e-commerce platform that specializes in selling the latest gadgets through a network of three distributors: ElectroCom, TechWorld, and GadgetCentral. The system automatically compares prices and availability across all distributors to provide customers with the best deals.

## 🔐 User Credentials

### Admin Accounts
**Main Admin:**
- **Email:** admin@gadgethub.com
- **Password:** admin123

### Demo User Accounts
**Test User:**
- **Email:** user1@gmail.com
- **Password:** user123

### User Account Creation
Users can create their own accounts through the web interface:
1. Navigate to the GadgetHub Web application
2. Click on "Sign Up" or "Register" button
3. Fill in the registration form with Full Name, Email, Password, and Contact Information
4. Submit the form to create account

## Architecture Overview

This solution implements a Service-Oriented Architecture (SOA) with the following components:

### Core Services
- **GadgetHub API** - Main orchestrator service that handles customer orders and quotation comparison
- **ElectroCom API** - Distributor service with competitive pricing and fast delivery
- **TechWorld API** - Premium distributor with quality-focused pricing
- **GadgetCentral API** - Budget-friendly distributor with volume discounts
- **GadgetHub Web** - Modern web client with clean, responsive design

### Database Architecture
- **GadgetHubDB** - Main database for orders, customers, and quotation comparisons
- **ElectroComDB** - ElectroCom's product inventory and quotations
- **TechWorldDB** - TechWorld's product inventory and quotations  
- **GadgetCentralDB** - GadgetCentral's product inventory and quotations

## Features

### For Customers
- **Modern Web Interface** - Clean, responsive design with intuitive navigation
- **Product Catalog** - Browse latest gadgets from all distributors
- **Price Comparison** - Automatic comparison of prices across distributors
- **Best Deal Selection** - System automatically selects the best distributor for each product
- **Order Tracking** - Real-time order status updates

### For Business
- **Automated Quotation System** - Requests quotes from all distributors simultaneously
- **Intelligent Selection** - Chooses best distributor based on price, availability, and delivery time
- **Order Management** - Complete order lifecycle management
- **Customer Management** - Customer registration and profile management

## Technology Stack

### Backend
- **.NET 8.0** - Core framework
- **ASP.NET Core Web API** - RESTful API services
- **Entity Framework Core 9.0.9** - ORM for database operations
- **SQL Server** - Database management system
- **AutoMapper 15.0.0** - Object-to-object mapping
- **Swagger/OpenAPI** - API documentation

### Frontend
- **ASP.NET Core Razor Pages** - Server-side rendering
- **Modern CSS3** - Custom styling with CSS Grid and Flexbox
- **Vanilla JavaScript** - Interactive functionality
- **Responsive Design** - Mobile-first approach

### Development Tools
- **Visual Studio 2022** - IDE
- **SQL Server Management Studio** - Database management

## Prerequisites

Before running the application, ensure you have:

1. **Visual Studio 2022** with the following workloads:
   - ASP.NET and web development
   - .NET Desktop Development
   - Data Storage and Processing

2. **SQL Server** (LocalDB or Express)
   - Server: `LAPTOP-KK271TP3\\LOCALHOST` 
   - Or update connection strings in `appsettings.json` files

3. **.NET 8.0 SDK**

## Database Configuration

The application uses SQL Server with Windows Authentication by default. Connection strings are configured in each project's `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "GadgetHubDB": "Server=LAPTOP-KK271TP3\\LOCALHOST;Database=GadgetHubDB;Trusted_Connection=true;TrustServerCertificate=true;",
    "ElectroComDB": "Server=LAPTOP-KK271TP3\\LOCALHOST;Database=ElectroComDB;Trusted_Connection=true;TrustServerCertificate=true;",
    "TechWorldDB": "Server=LAPTOP-KK271TP3\\LOCALHOST;Database=TechWorldDB;Trusted_Connection=true;TrustServerCertificate=true;",
    "GadgetCentralDB": "Server=LAPTOP-KK271TP3\\LOCALHOST;Database=GadgetCentralDB;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```


## How to Run the Code

### 1. Database Setup

You have two options for setting up the databases:

#### Option A: Create New Databases
Create the databases in SQL Server:

```sql
-- Create databases
CREATE DATABASE GadgetHubDB;
CREATE DATABASE ElectroComDB;
CREATE DATABASE TechWorldDB;
CREATE DATABASE GadgetCentralDB;
```

#### Option B: Attach Provided Database Files
If you have the database files (.mdf and .ldf files), attach them using SQL Server Management Studio:

1. **Open SQL Server Management Studio**
2. **Right-click on "Databases"** → **Attach**
3. **Click "Add"** and browse to your database files
4. **Attach the following databases from the Database folder:**
   - Database/GadgetHubDB.mdf / Database/GadgetHubDB.ldf
   - Database/ElectroComDB.mdf / Database/ElectroComDB.ldf
   - Database/TechWorldDB.mdf / Database/TechWorldDB.ldf
   - Database/GadgetCentralDB.mdf / Database/GadgetCentralDB.ldf

> **Note:** If you attach the provided databases, you can skip the migration step (Step 2) as the databases will already contain the required tables and data.

### 2. Run Database Migrations

**Skip this step if you attached the provided database files (Option B above).**

Navigate to each API project and run migrations:

```bash
# GadgetHub API
cd GadgetHubAPI/GadgetHubAPI
dotnet ef database update

# ElectroCom API
cd ElectroComAPI/ElectroComAPI
dotnet ef database update

# TechWorld API
cd TechWorldAPI/TechWorldAPI
dotnet ef database update

# GadgetCentral API
cd GadgetCentralAPI/GadgetCentralAPI
dotnet ef database update
```

### 3. Start the Services

Start the services in the following order:

#### Option A: Using Visual Studio
1. Open the solution in Visual Studio 2022
2. Set multiple startup projects:
   - GadgetHubAPI
   - ElectroComAPI
   - TechWorldAPI
   - GadgetCentralAPI
   - GadgetHubWeb
3. Press F5 to start all services

#### Option B: Using Command Line
```bash
# Terminal 1 - GadgetHub API (Port 7091)
cd GadgetHubAPI/GadgetHubAPI
dotnet run --launch-profile https

# Terminal 2 - ElectroCom API (Port 7077)
cd ElectroComAPI/ElectroComAPI
dotnet run --urls="https://localhost:7077"

# Terminal 3 - TechWorld API (Port 7102)
cd TechWorldAPI/TechWorldAPI
dotnet run --urls="https://localhost:7102"

# Terminal 4 - GadgetCentral API (Port 7007)
cd GadgetCentralAPI/GadgetCentralAPI
dotnet run --urls="https://localhost:7007"

# Terminal 5 - GadgetHub Web (Port 7324)
cd GadgetHubWeb/GadgetHubWeb
dotnet run
```

### 4. Access the Application

- **GadgetHub Web**: https://localhost:7324
- **GadgetHub API**: https://localhost:7091/swagger
- **ElectroCom API**: https://localhost:7077/swagger
- **TechWorld API**: https://localhost:7102/swagger
- **GadgetCentral API**: https://localhost:7007/swagger




### Port Configuration

The services are configured to run on the following ports:
- GadgetHub API: 7091(HTTPS)
- ElectroCom API: 7077 (HTTPS)
- TechWorld API: 7102 (HTTPS)
- GadgetCentral API: 7007 (HTTPS)
- GadgetHub Web: 7324 (HTTPS)

## Troubleshooting

### SSL Certificate Issues
```bash
dotnet dev-certs https --trust
```

### Database Connection Issues
- Ensure SQL Server is running
- Check connection strings match your SQL Server instance
- Verify database permissions

### Port Conflicts
- If ports are in use, update `launchSettings.json` in each project
- Or use different ports in the command line: `dotnet run --urls="https://localhost:NEW_PORT"`

## Project Structure

```
GadgetHub/
├── GadgetHubAPI/           # Main orchestrator API
│   ├── Controllers/        # API controllers
│   ├── Data/              # Database context and repositories
│   ├── DTO/               # Data transfer objects
│   ├── Model/             # Entity models
│   ├── Profiles/          # AutoMapper profiles
│   └── Services/          # Business logic services
├── ElectroComAPI/         # ElectroCom distributor API
├── TechWorldAPI/          # TechWorld distributor API
├── GadgetCentralAPI/      # GadgetCentral distributor API
├── GadgetHubWeb/          # Web client application
│   ├── Pages/             # Razor pages
│   ├── wwwroot/           # Static files (CSS, JS)
│   └── Shared/            # Shared layouts and components
```

## Workflow

### Order Processing Flow
1. **Customer Places Order** - Customer selects products and places order through web interface
2. **Quotation Request** - GadgetHub API requests quotations from all three distributors
3. **Distributor Response** - Each distributor responds with pricing and availability
4. **Comparison & Selection** - System compares all quotations and selects the best option
5. **Order Placement** - Order is placed with the selected distributor(s)
6. **Confirmation** - Customer receives order confirmation with delivery details

### Quotation Logic
- **ElectroCom**: Competitive pricing with bulk discounts (5% for 10+ items, 2% for 5+ items)
- **TechWorld**: Premium pricing with loyalty discounts (10% for 20+ items, 5% for 10+ items)
- **GadgetCentral**: Budget-friendly with volume discounts (15% for 50+ items, 10% for 25+ items)

## Design Philosophy

The web interface follows modern design principles:
- **Clean Interface**: Minimalist design with focus on typography and whitespace
- **Card-based Layout**: Product display using card components for better organization
- **Modern Web Standards**: CSS Grid, Flexbox, and responsive design principles

## Future Enhancements

1. **Authentication & Authorization** - User login and role-based access
2. **Payment Integration** - Stripe, PayPal, or other payment processors
3. **Email Notifications** - Order confirmations and status updates
4. **Inventory Management** - Real-time stock updates
5. **Analytics Dashboard** - Business intelligence and reporting
6. **Mobile App** - Native mobile application
7. **API Rate Limiting** - Protect against abuse
8. **Caching** - Redis for improved performance
9. **Logging** - Structured logging with Serilog
10. **Testing** - Unit and integration tests

## Support

For technical support or questions about this implementation, please refer to the code comments and API documentation available through the Swagger UI endpoints.