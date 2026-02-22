## Getting Started for Development

### Database Setup

1. **Update Connection String**
   - Open `appsettings.Development.json`
   - Update the connection string to point to your local SQL Server instance

2. **Run Database Migrations**

3. **Seed Development Data**
   - On first run, the application will automatically seed sample data in Development mode
   - Sample data includes:
     - 1 Organization: "Demo Company Ltd"
     - 2 Clients: "Acme Corporation" and "Tech Startup Inc"
     - 2 Projects with invoices and payments
     - 3 Users:
       - **OrgUser**: demo@clearledger.com / Demo123! (Organization admin)
       - **Customer 1**: john.doe@acme.com / Customer123! (Acme Corporation client)
       - **Customer 2**: jane.smith@techstartup.com / Customer123! (Tech Startup Inc client)
   - The seeder will run automatically on next startup.

4. **Test API Endpoints**
   - Use the seeded API key for testing: `dev-api-key-12345`
   - Swagger UI: `https://localhost:[port]/swagger`
   - Example API call:
     ```bash
     curl -X GET "https://localhost:5001/api/v1/invoices/{invoice-id}" \
          -H "X-API-KEY: ApiKey dev-api-key-12345"
     ```

5. **Test Razor Pages Login**
   - Navigate to `/Identity/Account/Login`
   - Login as OrgUser to manage all clients and projects
   - Login as Customer to view your specific client data

### Reset Database

To start fresh: