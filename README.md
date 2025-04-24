# BillFlow API - ASP.NET Core Web API

BillFlow is a full-featured, multi-tenant invoicing platform built with ASP.NET Core. It allows users to create, send, manage, and track invoices in real-time. The API is designed to handle complex invoicing operations, payments, and provide real-time analytics for businesses. It integrates with Stripe for payment processing and offers secure authentication for users.

## Key Features

- **Create, Send, and Manage Invoices**: Generate and track invoices with support for subscriptions and one-time payments.
- **Multi-Tenant Support**: Scalable user roles including Admin, Business Owner, and Accountant.
- **Secure Payments Integration**: Built-in Stripe integration for seamless payment handling.
- **Real-Time Analytics**: Dashboard to monitor invoice trends, payment status, and more using SignalR and Chart.js.
- **Email Verification**: Secure user registration with email confirmation.
- **PDF Invoice Generation**: Generate professional PDF invoices for clients.
- **Admin Dashboard**: Manage users, transactions, and view system insights.

## Tech Stack

- **Backend**: ASP.NET Core, Entity Framework Core, SQL Server
- **Frontend**: React.js, Tailwind CSS (for front-end integration with API)
- **Authentication**: ASP.NET Identity (role-based access)
- **Payments**: Stripe API (Subscription + One-time billing)
- **Real-time Analytics**: SignalR, Chart.js
- **PDF Generation**: iTextSharp or similar
- **Database**: SQL Server with Entity Framework Core (Code-First)

## Getting Started

### Prerequisites

- .NET SDK (5.0 or later)
- SQL Server (or use a cloud database service like Azure)
- A Stripe account for payment integration

#Running project
- To run the API without HTTPS certificate issues, use the HTTP profile by running `dotnet run --launch-profile http`, then open `http://localhost:5166/swagger` in your browser to access the Swagger UI for testing and exploring the API.

