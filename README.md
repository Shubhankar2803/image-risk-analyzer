# Image Risk Analyzer

A full-stack application for analyzing image safety risks using Google Gemini AI. Users can upload images, get risk assessments with detailed categorization, and view their analysis history.

## Quick Start

Get the application running locally in minutes.

### Backend

```bash
cd backend/src/RiskAnalyzer.Api
dotnet restore
dotnet run
```

Backend runs on `http://localhost:5193`

### Frontend

```bash
cd frontend
npm install
ng serve
```

Frontend runs on `http://localhost:4200`

Visit `http://localhost:4200` to access the application.

## Overview

This application helps teams analyze images for safety risks. Users can upload images and receive detailed risk assessments powered by AI.

### Features

- User registration and authentication
- Drag-and-drop image upload
- AI-powered risk assessment with multiple categories
- Detailed evidence and confidence scores
- Complete analysis history
- Brutalist black and yellow interface
- JWT-based authentication

## Tech Stack

### Backend
- .NET Core 10.0 with ASP.NET Core
- C# with Entity Framework Core
- MySQL 8.0+ with Pomelo connector
- JWT Bearer authentication
- Google Gemini API 2.5-flash

### Frontend
- Angular 19
- TypeScript
- Custom CSS3 styling
- Angular HttpClient for API calls

### Database
- MySQL 8.0+
- Entity Framework Core ORM

## Project Structure

```
image-risk-analyser/
├── backend/
│   ├── global.json
│   ├── RiskAnalyzer.slnx
│   └── src/
│       └── RiskAnalyzer.Api/
│           ├── Controllers/
│           ├── Services/
│           ├── Repositories/
│           ├── Models/
│           ├── DTOs/
│           ├── Config/
│           ├── Data/
│           └── Tests/
│
├── frontend/
│   ├── src/
│   │   ├── app/
│   │   │   ├── components/
│   │   │   ├── services/
│   │   │   ├── models/
│   │   │   ├── guards/
│   │   │   └── interceptors/
│   │   └── styles.css
│   └── tests/
│
└── docs/
    ├── ARCHITECTURE.md
    ├── API_CONTRACTS.md
    ├── DATA_FLOW.md
    └── ASSUMPTIONS.md
```

## Setup

### Requirements

- Node.js 18+
- npm 9+
- .NET 10.0 SDK
- MySQL 8.0+

### Backend

1. Copy configuration template and update values:
   ```bash
   cd backend/src/RiskAnalyzer.Api
   cp appsettings.Development.json.example appsettings.Development.json
   ```

2. Edit `appsettings.Development.json` with your database and API credentials:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Port=3306;Database=risk_analyzer_db_dev;Uid=root;Pwd=your_password;"
     },
     "GeminiSettings": {
       "ApiKey": "YOUR_GEMINI_API_KEY"
     }
   }
   ```

3. Get your Gemini API key from [Google AI Studio](https://aistudio.google.com/app/apikey)

4. Create the database:
   ```bash
   mysql -u root -p -e "CREATE DATABASE risk_analyzer_db_dev;"
   ```

5. Run migrations:
   ```bash
   dotnet ef database update
   ```

6. Start the backend:
   ```bash
   dotnet run
   ```

Backend runs on `http://localhost:5193`

### Frontend

1. Copy environment template:
   ```bash
   cd frontend
   cp .env.example .env
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Start the development server:
   ```bash
   ng serve
   ```

Frontend runs on `http://localhost:4200`

## Running the Application

### Development

Terminal 1 - Backend:
```bash
cd backend/src/RiskAnalyzer.Api
dotnet run
```

Terminal 2 - Frontend:
```bash
cd frontend
ng serve
```

### Production Build

```bash
# Frontend
ng build --configuration production

# Backend
dotnet publish -c Release
```

## Testing

### Backend Tests

```bash
cd backend/src/RiskAnalyzer.Api

# Run all tests
dotnet test

# With coverage
dotnet test /p:CollectCoverage=true
```

### Frontend Tests

```bash
cd frontend

# Run tests
ng test

# Headless mode
ng test --watch=false

# With coverage
ng test --code-coverage
```

## API Documentation

Base URLs:
- Development: `http://localhost:5193/api`
- Production: `https://api.yourdomain.com/api`

Key endpoints:
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - User login
- `POST /api/imageanalysis` - Upload and analyze image
- `GET /api/imageanalysis` - Get analysis history
- `DELETE /api/imageanalysis/{id}` - Delete analysis

See [Full API Documentation](docs/API_CONTRACTS.md)

## User Interface

The application uses a brutalist design with black and yellow styling:

- Clean, minimal layout
- Monospace fonts (Courier New)
- Black background with yellow accents
- Simple ASCII-like interface
- No rounded corners or gradients

## Security

- JWT authentication with 24-hour token expiration
- Passwords hashed with cryptographic algorithms
- Route guards on protected pages
- CORS configured for specific origins
- API keys in environment variables (not committed)
- Sensitive configuration excluded from version control
- Use HTTPS in production

## Configuration

Use `.env.example` and `appsettings.*.example` files as templates.

Configuration files are excluded from version control for security. Copy the example files and fill in your values:

```bash
# Backend
cp backend/src/RiskAnalyzer.Api/appsettings.Development.json.example \
   backend/src/RiskAnalyzer.Api/appsettings.Development.json

# Frontend
cp frontend/.env.example frontend/.env
```

## Documentation

- [Architecture Overview](docs/ARCHITECTURE.md)
- [API Contracts](docs/API_CONTRACTS.md)
- [Data Flow & Diagrams](docs/DATA_FLOW.md)
- [Assumptions & Constraints](docs/ASSUMPTIONS.md)

## Contributing

1. Create a feature branch
2. Make your changes
3. Write or update tests
4. Run tests to ensure they pass
5. Commit with a clear message
6. Create a pull request

## Troubleshooting

**Backend won't connect to MySQL**
- Verify MySQL is running
- Check connection string in `appsettings.Development.json`
- Ensure database exists: `CREATE DATABASE risk_analyzer_db_dev;`

**Frontend npm errors**
```bash
rm -rf node_modules package-lock.json
npm install
```

**CORS errors**
- Verify frontend is running on `http://localhost:4200`
- Backend CORS settings include frontend URL
- Check both backend and frontend are running

**API key errors**
- Get new key from [Google AI Studio](https://aistudio.google.com/app/apikey)
- Update `appsettings.Development.json`

## Performance

Backend:
- Connection pooling enabled by default
- Add database indexes for frequently queried columns
- Cache API responses where possible
- Consider rate limiting in production

Frontend:
- Use production build for deployment
- Lazy load routes
- Minimize bundle size

## License

MIT License

## Support

For issues or questions, open an issue on GitHub or check the documentation in the `/docs` folder.

## Version

1.0.0 - April 2026
