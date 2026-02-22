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
dotnet ef database update --project InfrastructureLayer --startup-project PresentationLayer
```

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
2. Register as organization user
3. **Organization Admin**: Manage all clients and view all projects
4. **Client Accounts**: View only your assigned client data

---
#### Creating Users 

1- For client users, after you create a client, you can add a user and assign them to that client. This can be done through the admin interface.



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
- Verify SQL Server is running
- Check server name in your connection string
- Ensure your Windows user has permissions to create databases

**Migration Errors:**
- Delete any pending migrations and start fresh
- Ensure `LedgerDbContext` is properly configured
