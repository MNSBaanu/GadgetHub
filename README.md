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
- **GadgetHub Web** - Modern web client built with Next.js

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
- **Node.js** - Runtime environment
- **Express.js** - RESTful API framework
- **Prisma ORM** - Database access and schema management
- **PostgreSQL** - Database (hosted on [Neon](https://neon.tech))

### Frontend
- **Next.js** - React framework with server-side rendering
- **Modern CSS3** - Custom styling with CSS Grid and Flexbox
- **Responsive Design** - Mobile-first approach

### Hosting
- **Vercel** - Frontend (GadgetHub Web / Next.js)
- **Railway / Render** - Backend APIs (Node.js + Express)
- **Neon** - PostgreSQL databases (free tier, 4 databases)

### Development Tools
- **VS Code** - IDE
- **Prisma Studio** - Database management GUI
- **Postman / Thunder Client** - API testing

## Prerequisites

Before running the application, ensure you have:

1. **Node.js 18+** - [Download here](https://nodejs.org)
2. **npm or yarn**
3. **PostgreSQL** - or a free [Neon](https://neon.tech) account for cloud DB
4. **Git**

## Environment Variables

Each service requires a `.env` file. Create one in each project folder:

### Distributor APIs (ElectroCom, TechWorld, GadgetCentral)
```env
DATABASE_URL="postgresql://user:password@host/dbname"
PORT=4001
```

### GadgetHub API (main orchestrator)
```env
DATABASE_URL="postgresql://user:password@host/gadgethubdb"
PORT=4000
ELECTROCOM_API_URL=http://localhost:4001
TECHWORLD_API_URL=http://localhost:4002
GADGETCENTRAL_API_URL=http://localhost:4003
```

### GadgetHub Web (Next.js)
```env
NEXT_PUBLIC_API_URL=http://localhost:4000
```

## Database Setup

### Option A: Local PostgreSQL
Create the databases locally:
```sql
CREATE DATABASE gadgethubdb;
CREATE DATABASE electrocomdb;
CREATE DATABASE techWorlddb;
CREATE DATABASE gadgetcentraldb;
```

Then run Prisma migrations in each service folder:
```bash
npx prisma migrate dev --name init
```

### Option B: Neon (Recommended for cloud)
1. Create a free account at [neon.tech](https://neon.tech)
2. Create 4 databases (one per service)
3. Copy each connection string into the corresponding `.env` file
4. Run `npx prisma migrate deploy` in each service folder

## How to Run Locally

### 1. Install dependencies in each service
```bash
# Run this in each folder: GadgetHubAPI, ElectroComAPI, TechWorldAPI, GadgetCentralAPI, GadgetHubWeb
npm install
```

### 2. Run database migrations
```bash
# Run in each API folder
npx prisma migrate dev
```

### 3. Start the services

Open a terminal for each service:

```bash
# Terminal 1 - ElectroCom API (Port 4001)
cd ElectroComAPI
npm run dev

# Terminal 2 - TechWorld API (Port 4002)
cd TechWorldAPI
npm run dev

# Terminal 3 - GadgetCentral API (Port 4003)
cd GadgetCentralAPI
npm run dev

# Terminal 4 - GadgetHub API (Port 4000)
cd GadgetHubAPI
npm run dev

# Terminal 5 - GadgetHub Web (Port 3000)
cd GadgetHubWeb
npm run dev
```

### 4. Access the Application

- **GadgetHub Web**: http://localhost:3000
- **GadgetHub API**: http://localhost:4000
- **ElectroCom API**: http://localhost:4001
- **TechWorld API**: http://localhost:4002
- **GadgetCentral API**: http://localhost:4003

## Deployment

### Frontend — Vercel
1. Push your code to GitHub
2. Go to [vercel.com](https://vercel.com) and import the `GadgetHubWeb` folder
3. Set environment variables in the Vercel dashboard
4. Deploy — Vercel handles everything automatically

### Backend APIs — Railway or Render
1. Go to [railway.app](https://railway.app) or [render.com](https://render.com)
2. Create a new Web Service and connect your GitHub repo
3. Set the root directory to the specific API folder (e.g. `ElectroComAPI`)
4. Add environment variables
5. Deploy

> Repeat for each of the 4 APIs. Each gets its own service on Railway/Render.

### Databases — Neon
1. Create a project at [neon.tech](https://neon.tech)
2. Create a database for each service
3. Use the provided connection strings in your deployment environment variables

## Project Structure

```
GadgetHub/
├── GadgetHubAPI/           # Main orchestrator API (Express)
│   ├── controllers/        # Route handlers
│   ├── prisma/             # Prisma schema and migrations
│   ├── routes/             # Express routers
│   ├── services/           # Business logic
│   └── index.js            # Entry point
├── ElectroComAPI/          # ElectroCom distributor API (Express)
├── TechWorldAPI/           # TechWorld distributor API (Express)
├── GadgetCentralAPI/       # GadgetCentral distributor API (Express)
└── GadgetHubWeb/           # Frontend (Next.js)
    ├── app/                # Next.js app router pages
    ├── components/         # Reusable React components
    └── public/             # Static assets
```

## Workflow

### Order Processing Flow
1. **Customer Places Order** - Customer selects products and places order through the web interface
2. **Quotation Request** - GadgetHub API requests quotations from all three distributors
3. **Distributor Response** - Each distributor responds with pricing and availability
4. **Comparison & Selection** - System compares all quotations and selects the best option
5. **Order Placement** - Order is placed with the selected distributor(s)
6. **Confirmation** - Customer receives order confirmation with delivery details

### Quotation Logic
- **ElectroCom**: Competitive pricing with bulk discounts (5% for 10+ items, 2% for 5+ items)
- **TechWorld**: Premium pricing with loyalty discounts (10% for 20+ items, 5% for 10+ items)
- **GadgetCentral**: Budget-friendly with volume discounts (15% for 50+ items, 10% for 25+ items)

## Future Enhancements

1. **Authentication & Authorization** - JWT-based auth with role management
2. **Payment Integration** - Stripe or PayPal
3. **Email Notifications** - Order confirmations and status updates via Resend or Nodemailer
4. **Inventory Management** - Real-time stock updates
5. **Analytics Dashboard** - Business intelligence and reporting
6. **Mobile App** - React Native application
7. **API Rate Limiting** - express-rate-limit for abuse protection
8. **Caching** - Redis for improved performance
9. **Logging** - Structured logging with Winston or Pino
10. **Testing** - Jest + Supertest for unit and integration tests

## Support

For technical support or questions about this implementation, refer to the API route files and Prisma schema in each service folder.
