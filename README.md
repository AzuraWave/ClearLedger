## Getting Started for Development

### Prerequisites
- SQL Server (local or remote instance)
- .NET 6+ SDK
- Visual Studio 2022 or VS Code

### Initial Setup

#### 1. Configure Database Connection
Open `appsettings.Development.json` and update the connection string to match your SQL Server instance:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=LedgerDb;Trusted_Connection=True;"
}
```

#### 2. Create and Migrate Database
Run the following command in the Package Manager Console:

```powershell
Update-Database
```

Or using the .NET CLI:

```bash
dotnet ef database update
```

#### 3. Seed Development Data
Run the application with the seed flag:

```bash
dotnet run --seed-dev
```

This will populate your database with sample data:

| Item | Details |
|------|---------|
| **Organization** | Demo Company Ltd |
| **Clients** | Acme Corporation, Tech Startup Inc |
| **Projects** | 2 sample projects with invoices and payments |
| **API Key** | `dev-api-key-12345` |

##### Sample User Accounts

| Role | Email | Password | Access |
|------|-------|----------|--------|
| Organization Admin | `demo@clearledger.com` | `Demo123!` | Manage all clients & projects |
| Client 1 | `john.doe@acme.com` | `Customer123!` | View Acme Corporation data |
| Client 2 | `jane.smith@techstartup.com` | `Customer123!` | View Tech Startup Inc data |

---

### Testing the Application

#### API Testing
Use the seeded API key to test endpoints via Swagger or cURL:

**Swagger UI:**
```
https://localhost:[port]/swagger
```

**Example API Request:**
```bash
curl -X GET "https://localhost:5001/api/v1/invoices/{invoice-id}" \
     -H "X-API-KEY: ApiKey dev-api-key-12345"
```

#### Web Application Testing
1. Navigate to `https://localhost:[port]/Identity/Account/Login`
2. Login with one of the sample accounts above
3. **Organization Admin**: Manage all clients and view all projects
4. **Client Accounts**: View only your assigned client data

---
#### Creating Users 

1- For organization admin, use the seeded account or create a new one via the registration page.
2- For client users, after you create a client, you can add a user and assign them to that client. This can be done through the admin interface.



### Reset Database

To start with a fresh database:

```powershell
# Drop existing database
Drop-Database

# Reapply migrations
Update-Database

# Reseed data
dotnet run --seed-dev
```

Or remove the database manually in SQL Server Management Studio and re-run migrations.

---

### Troubleshooting

**Connection String Issues:**
- Verify SQL Server is running
- Check server name in your connection string
- Ensure your Windows user has permissions to create databases

**Migration Errors:**
- Delete any pending migrations and start fresh
- Ensure `LedgerDbContext` is properly configured

**Seeding Not Working:**
- Confirm application is running in `Development` environment
- Check `appsettings.Development.json` exists
- Review console output for error messages