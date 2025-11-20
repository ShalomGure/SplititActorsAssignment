# Splitit Actors API

A RESTful API for managing actor information scraped from IMDb's "Most Popular Celebs" list. Built with .NET 10.0 and following Clean Architecture principles.

## Features

- Web scraping from IMDb (https://www.imdb.com/list/ls054840033/)
- In-memory database with Entity Framework Core
- Full CRUD operations with filtering and pagination
- OAuth2 Client Credentials authentication
- Swagger/OpenAPI documentation
- Global exception handling
- Extensible provider pattern for multiple data sources

## Architecture

The solution follows Clean Architecture with clear separation of concerns:

```
SplititActorsApi/
├── SplititActorsApi.API/          # Presentation Layer
│   ├── Controllers/                # API endpoints
│   ├── Middleware/                 # Global exception handler
│   └── Program.cs                  # App configuration & DI
├── SplititActorsApi.Business/     # Business Logic Layer
│   ├── Services/                   # Business logic implementation
│   └── Mappers/                    # DTO mapping
├── SplititActorsApi.Core/         # Core/Domain Layer
│   ├── Models/                     # Domain entities
│   ├── DTOs/                       # Data transfer objects
│   └── Interfaces/                 # Contracts/abstractions
└── SplititActorsApi.Data/         # Data Access Layer
    ├── DbContexts/                 # EF Core context
    ├── Repositories/               # Data access implementation
    ├── Scrapers/                   # Web scraping providers
    └── Services/                   # Data seeding
```

### Design Patterns

- **Repository Pattern**: Abstracts data access logic
- **Strategy Pattern**: IScraperProvider allows multiple data sources (IMDb, Rotten Tomatoes, etc.)
- **Dependency Injection**: Loose coupling between layers
- **DTO Pattern**: Separates domain models from API contracts

## Prerequisites

- .NET 10.0 SDK
- Any IDE (Visual Studio 2022, VS Code, JetBrains Rider)

## Setup Instructions

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd SplititActorsApi/SplititActorsApi
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Run the API**
   ```bash
   dotnet run --project SplititActorsApi.API/SplititActorsApi.API.csproj
   ```

   The API will start on `http://localhost:5081`

5. **Access Swagger UI**

   Open your browser and navigate to: `http://localhost:5081/`

## Authentication

The API uses OAuth2 Client Credentials flow with JWT Bearer tokens.

**Authorization Server**: https://id.sandbox.splitit.com
**Audience**: job.assignment.api

### Getting a Token

To obtain an access token, send a request to the authorization server:

```bash
curl -X POST https://id.sandbox.splitit.com/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials" \
  -d "client_id=YOUR_CLIENT_ID" \
  -d "client_secret=YOUR_CLIENT_SECRET" \
  -d "scope=job.assignment.api"
```

### Using the Token

Include the access token in the Authorization header:

```bash
curl -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  http://localhost:5081/api/actors
```

**Note**: All API endpoints require authentication. Requests without a valid token will receive a 401 Unauthorized response.

## API Endpoints

All endpoints are prefixed with `/api/actors` and require authentication.

### 1. Get Actors (List with Filtering and Pagination)

**Endpoint**: `GET /api/actors`

**Query Parameters**:
- `name` (optional): Filter by actor name (partial match, case-insensitive)
- `minRank` (optional): Minimum rank (inclusive)
- `maxRank` (optional): Maximum rank (inclusive)
- `pageNumber` (default: 1): Page number (must be >= 1)
- `pageSize` (default: 10): Items per page (1-100)

**Response**: Returns a paginated list with actor name and ID only.

**Example**:
```bash
curl -H "Authorization: Bearer YOUR_TOKEN" \
  "http://localhost:5081/api/actors?name=tom&minRank=1&maxRank=50&pageNumber=1&pageSize=10"
```

**Response**:
```json
{
  "items": [
    {
      "id": 1,
      "name": "Tom Hanks"
    }
  ],
  "totalCount": 1,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

### 2. Get Actor by ID

**Endpoint**: `GET /api/actors/{id}`

**Response**: Returns full actor details including bio, birth date, image, and known works.

**Example**:
```bash
curl -H "Authorization: Bearer YOUR_TOKEN" \
  http://localhost:5081/api/actors/1
```

**Response**:
```json
{
  "id": 1,
  "name": "Tom Hanks",
  "rank": 1,
  "bio": "Thomas Jeffrey Hanks was born in Concord, California...",
  "birthDate": "1956-07-09T00:00:00",
  "imageUrl": "https://...",
  "knownFor": [
    "Forrest Gump",
    "Cast Away",
    "Saving Private Ryan"
  ]
}
```

### 3. Create Actor

**Endpoint**: `POST /api/actors`

**Request Body**:
```json
{
  "name": "Leonardo DiCaprio",
  "rank": 100,
  "bio": "Few actors in the world...",
  "birthDate": "1974-11-11",
  "imageUrl": "https://...",
  "knownFor": [
    "Titanic",
    "Inception",
    "The Wolf of Wall Street"
  ]
}
```

**Validation**:
- `name`: Required, max 200 characters
- `rank`: Required, must be > 0, must be unique
- `bio`: Optional, max 2000 characters
- `birthDate`: Optional, must be in the past
- `imageUrl`: Optional, max 500 characters
- `knownFor`: Optional, max 10 items, each max 200 characters

**Responses**:
- `201 Created`: Actor created successfully (returns full actor details)
- `400 Bad Request`: Validation errors
- `409 Conflict`: Rank already exists

### 4. Update Actor

**Endpoint**: `PUT /api/actors/{id}`

**Request Body**: Same as Create Actor (all fields required)

**Responses**:
- `200 OK`: Actor updated successfully
- `400 Bad Request`: Validation errors
- `404 Not Found`: Actor not found
- `409 Conflict`: Rank already exists (for a different actor)

### 5. Delete Actor

**Endpoint**: `DELETE /api/actors/{id}`

**Responses**:
- `204 No Content`: Actor deleted successfully
- `404 Not Found`: Actor not found

## Data Seeding

On application startup, the API automatically scrapes IMDb and populates the in-memory database with actor data. This process:

1. Checks if the database already contains data
2. If empty, scrapes https://www.imdb.com/list/ls054840033/
3. Parses actor information (name, rank, bio, birth date, image, known works)
4. Seeds the database with the scraped data (typically 75 actors)

The seeding process is logged in the console output.

## Error Handling

The API uses a global exception handler that provides consistent error responses:

- **400 Bad Request**: Validation errors, invalid input
- **401 Unauthorized**: Missing or invalid authentication token
- **404 Not Found**: Resource not found
- **409 Conflict**: Duplicate rank constraint violation
- **500 Internal Server Error**: Unexpected server errors

Error Response Format:
```json
{
  "error": "Error message",
  "details": "Additional details if available"
}
```

## Testing

### Manual Testing with curl

1. **Test without authentication** (should return 401):
   ```bash
   curl -w "\nStatus: %{http_code}\n" http://localhost:5081/api/actors
   ```

2. **Test with authentication**:
   ```bash
   # Get access token first
   TOKEN=$(curl -X POST ... | jq -r .access_token)

   # Use token
   curl -H "Authorization: Bearer $TOKEN" \
     http://localhost:5081/api/actors
   ```

3. **Test filtering**:
   ```bash
   curl -H "Authorization: Bearer $TOKEN" \
     "http://localhost:5081/api/actors?name=robert&minRank=1&maxRank=30"
   ```

4. **Test pagination**:
   ```bash
   curl -H "Authorization: Bearer $TOKEN" \
     "http://localhost:5081/api/actors?pageNumber=2&pageSize=5"
   ```

5. **Test create with duplicate rank** (should return 409):
   ```bash
   curl -X POST \
     -H "Authorization: Bearer $TOKEN" \
     -H "Content-Type: application/json" \
     -d '{"name":"Test Actor","rank":1,"bio":"Test"}' \
     http://localhost:5081/api/actors
   ```

### Testing with Swagger UI

**Note**: Due to package compatibility limitations in .NET 10.0, Swagger UI does not display an "Authorize" button or authentication indicators. However, the API authentication is fully functional and all endpoints require valid JWT tokens.

**To test authenticated endpoints in Swagger:**

1. Navigate to `http://localhost:5081/`
2. Select any endpoint and click "Try it out"
3. Add an Authorization header manually:
   - Click "Add header" or find the header section
   - Header name: `Authorization`
   - Header value: `Bearer YOUR_ACCESS_TOKEN`
4. Execute the request

**Alternative**: Use curl commands (recommended) as shown above for easier testing with authentication.

## Technical Details

### Dependencies

- **Microsoft.AspNetCore.Authentication.JwtBearer** (10.0.0): OAuth2/JWT authentication
- **Microsoft.AspNetCore.OpenApi** (10.0.0): OpenAPI specification generation
- **Swashbuckle.AspNetCore** (10.0.1): Swagger UI
- **Microsoft.EntityFrameworkCore.InMemory** (10.0.0): In-memory database provider
- **HtmlAgilityPack** (1.11.73): HTML parsing for web scraping

### Database Schema

**Actor Table**:
- `Id` (int, PK, auto-increment)
- `Name` (string, required, max 200)
- `Rank` (int, required, unique)
- `Bio` (string, max 2000)
- `BirthDate` (DateTime?, nullable)
- `ImageUrl` (string, max 500)
- `KnownFor` (List<string>, serialized as JSON)

### Configuration

Authentication and API configuration is in `SplititActorsApi.API/Program.cs`:

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://id.sandbox.splitit.com";
        options.Audience = "job.assignment.api";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });
```

## Extensibility

### Adding New Scraper Providers

To add support for additional actor data sources (e.g., Rotten Tomatoes):

1. Create a new class implementing `IScraperProvider`:
   ```csharp
   public class RottenTomatoesScraperProvider : IScraperProvider
   {
       public string ProviderName => "Rotten Tomatoes";
       public async Task<List<Actor>> ScrapeActorsAsync() { ... }
   }
   ```

2. Register in `Program.cs`:
   ```csharp
   builder.Services.AddHttpClient<IScraperProvider, RottenTomatoesScraperProvider>();
   ```

The architecture supports switching between providers without modifying other layers.

## Known Limitations

1. **In-Memory Database**: Data is lost on application restart. For production, switch to a persistent database (SQL Server, PostgreSQL, etc.)
2. **Web Scraping**: IMDb's HTML structure may change, requiring scraper updates
3. **Authentication**: Currently configured for Splitit's sandbox environment
4. **Rate Limiting**: No rate limiting implemented for API endpoints

## License

This project was created as a home assignment for Splitit.

## Contact

For questions or issues, please contact the development team.
