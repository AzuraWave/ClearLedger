## Getting Started for Development

### Prerequisites
- SQL Server LocalDB or SQL Server instance
- .NET 10 SDK
- Visual Studio 2022 or VS Code

### Initial Setup

#### 1. Configure Database Connection
Open `appsettings.Development.json` and update the connection string if needed (default uses LocalDB):

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LedgerDb;Trusted_Connection=True;"
}
```

#### 2. Create and Migrate Database
Run the following command in the Package Manager Console:

```powershell
Update-Database
```

Or using the .NET CLI:

```bash
dotnet ef database update --project InfrastructureLayer --startup-project PresentationLayer
```

#### 3. Seed Development Data
After creating the database, seed it with sample data using the seeding command:

```bash
cd PresentationLayer dotnet run --seed-dev
```


This will create:
- **Roles**: OrgUser, Customer
- **Admin User**: `admin@democompany.com` / `Admin@123`
- **Organization**: Demo Company Ltd
- **API Key**: Displayed in console output (save it!)
- **Clients**: Acme Corporation, TechStart Inc
- **Projects**: Website Redesign, Mobile App Development, Monthly Consulting
- **Customer Users**: `acme.user@acme.com` / `Customer@123`, `tech.user@techstart.io` / `Customer@123`

**Note:** The seeding command will exit after completion. To start the application normally, run `dotnet run` without the `--seed-dev` flag.

---

### Testing the Application

#### Web Application Testing

**Login as Admin User:**
1. Navigate to `https://localhost:[port]/Identity/Account/Login`
2. Email: `admin@democompany.com`
3. Password: `Admin@123`
4. **Organization Admin**: Manage all clients, projects, invoices, and payments

**Login as Customer User:**
1. Navigate to `https://localhost:[port]/Identity/Account/Login`
2. Email: `acme.user@acme.com` or `tech.user@techstart.io`
3. Password: `Customer@123`
4. **Client Users**: View only their assigned client's projects and transactions

#### API Testing
Use the seeded API key to test automation endpoints via Swagger or cURL:

You can also generate a new Apikey from the admin 'ApiKey' page that will replace the previous one

**Swagger UI:**
```
https://localhost:[port]/swagger
```

**Example API Request:**
```bash
curl -X GET "https://localhost:5001/api/v1/invoices/{invoice-id}" \
     -H "X-API-KEY: ApiKey dev-api-key-12345"
```

**Get Organization ID:**
The organization ID is displayed in the console output when you run `--seed-dev`. You'll need it for API requests.

---

#### Creating Additional Users 

**Organization Users (Admins):**
1. Register through `/Identity/Account/Register`
2. Assign to the "OrgUser" role
3. Link to an organization

**Client Users (Customers):**
1. Create a client first through the admin interface
2. Create a user and assign them to that client
3. Assign to the "Customer" role
4. They will only see data for their assigned client

---


### Reset Database

To start with a fresh database:

```powershell
# Drop existing database
Drop-Database

# Reapply migrations
Update-Database

---

### Troubleshooting

**Connection String Issues:**
- Verify SQL Server LocalDB is running: `sqllocaldb info`
- Start LocalDB if needed: `sqllocaldb start mssqllocaldb`
- Check server name in your connection string
- Ensure your Windows user has permissions to create databases

**Migration Errors:**
- Ensure you're in the PresentationLayer directory
- Delete the database and recreate: `dotnet ef database drop` then `dotnet ef database update`
- Ensure `LedgerDbContext` is properly configured

**Seeding Fails:**
- Check if database already has data (seeding will skip if it detects existing organizations)
- Ensure all migrations are applied before seeding
- Check console output for detailed error messages

**"Cannot open database" Error:**
- Run `dotnet ef database update` first to create the database
- Verify LocalDB is installed and running

---