# AgriGreen Database Seeding

This document explains how to seed your AgriGreen database with sample data for testing and development purposes.

## Option 1: Automatic Seeding on Startup

The application can automatically seed the database with sample data during startup in development mode.

### Configuration

To enable or disable automatic seeding, update the `appsettings.Development.json` file:

```json
{
  "SeedSettings": {
    "SeedSampleData": "true"  // Set to "false" to disable seeding
  }
}
```

With this setting enabled, the database will be seeded automatically when you run the application in development mode.

## Option 2: Manual Seeding Using the Tool

You can also use the dedicated seeding tool to populate the database:

1. Navigate to the project root folder in your terminal
2. Build the seeding tool:

```bash
dotnet build Tools/SeedDatabase.csproj
```

3. Run the seeding tool:

```bash
dotnet run --project Tools/SeedDatabase.csproj
```

## Sample Data Included

The seeder creates the following sample data:

### Users
- **Employees**:
  - sarah.johnson@agrigreen.com (Password: Employee123!)
  - michael.smith@agrigreen.com (Password: Employee123!)
  - jessica.williams@agrigreen.com (Password: Employee123!)

- **Farmers**:
  - john.doe@farm.com (Password: Farmer123!)
  - emily.wilson@greenfarms.com (Password: Farmer123!)
  - robert.brown@organicvalley.com (Password: Farmer123!)
  - lisa.martinez@sunrisefarms.com (Password: Farmer123!)
  - david.taylor@naturegrowers.com (Password: Farmer123!)

### Farmers
- John Doe's Farm
- Green Valley Organics
- Brown Family Farms
- Sunrise Produce
- Nature's Best Growers

### Products
- 5 products per farmer (25 total)
- Various categories including Vegetables, Fruits, Dairy, Grains, Herbs, Meat, and Eggs
- Production dates randomly distributed over the last 6 months

## Notes

- The seeder checks if data already exists and will only add new records if needed
- All passwords are already set and accounts are pre-confirmed for easy testing
- The seeder will automatically create the "Farmer" and "Employee" roles if they don't exist 