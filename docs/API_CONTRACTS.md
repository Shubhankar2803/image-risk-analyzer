# API Contracts - Image Risk Analyzer

## Base URL
```
Development: http://localhost:5193
Production: https://api.yourdomain.com
```

## Authentication
All protected endpoints require a JWT Bearer token in the Authorization header:
```
Authorization: Bearer <jwt_token>
```

---

## Authentication Endpoints

### 1. User Registration
**Endpoint**: `POST /api/auth/register`

**Request**:
```json
{
  "email": "user@example.com",
  "username": "johndoe",
  "password": "SecurePassword123!",
  "fullName": "John Doe"
}
```

**Response** (201 Created):
```json
{
  "success": true,
  "message": "User registered successfully",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "user@example.com",
    "username": "johndoe",
    "fullName": "John Doe",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

**Error** (400 Bad Request):
```json
{
  "success": false,
  "message": "Email already exists",
  "data": null
}
```

---

### 2. User Login
**Endpoint**: `POST /api/auth/login`

**Request**:
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "user@example.com",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

**Error** (401 Unauthorized):
```json
{
  "success": false,
  "message": "Invalid email or password",
  "data": null
}
```

---

## Image Analysis Endpoints

### 3. Upload & Analyze Image
**Endpoint**: `POST /api/imageanalysis`  
**Authentication**: Required ✅  
**Content-Type**: `multipart/form-data`

**Request**:
- `file` (FormFile): Image file (JPG, PNG, WEBP, GIF, BMP)
- Max size: 10MB

**cURL Example**:
```bash
curl -X POST http://localhost:5193/api/imageanalysis \
  -H "Authorization: Bearer <token>" \
  -F "file=@image.jpg"
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Image analyzed successfully",
  "data": {
    "id": "660e8400-e29b-41d4-a716-446655440000",
    "fileName": "image.jpg",
    "riskScore": 45,
    "confidenceScore": 92,
    "classification": "Safe",
    "analysisDetails": "Image contains normal content with low risk factors",
    "categories": [
      {
        "name": "Violence & Physical Harm",
        "riskLevel": "Low",
        "evidence": "No violent content detected"
      },
      {
        "name": "Harassment & Hate Speech",
        "riskLevel": "Very Low",
        "evidence": "No offensive content detected"
      }
    ],
    "analyzedAt": "2026-04-24T10:30:00Z",
    "createdAt": "2026-04-24T10:30:00Z"
  }
}
```

**Error** (400 Bad Request):
```json
{
  "success": false,
  "message": "File size exceeds maximum limit of 10MB",
  "data": null
}
```

**Error** (500 Internal Server Error):
```json
{
  "success": false,
  "message": "API authentication failed. Please check your Gemini API key in the server configuration.",
  "data": {
    "riskScore": 0,
    "classification": "Pending Analysis",
    "confidenceScore": 0
  }
}
```

---

### 4. Get User's Analysis History
**Endpoint**: `GET /api/imageanalysis`  
**Authentication**: Required ✅  
**Query Parameters**: None

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Analysis history retrieved successfully",
  "data": [
    {
      "id": "660e8400-e29b-41d4-a716-446655440000",
      "fileName": "image1.jpg",
      "riskScore": 25,
      "confidenceScore": 88,
      "classification": "Safe",
      "analyzedAt": "2026-04-24T10:30:00Z",
      "createdAt": "2026-04-24T10:30:00Z"
    },
    {
      "id": "770e8400-e29b-41d4-a716-446655440000",
      "fileName": "image2.png",
      "riskScore": 62,
      "confidenceScore": 95,
      "classification": "Potentially Unsafe",
      "analyzedAt": "2026-04-24T09:15:00Z",
      "createdAt": "2026-04-24T09:15:00Z"
    }
  ]
}
```

---

### 5. Get Single Analysis Details
**Endpoint**: `GET /api/imageanalysis/{analysisId}`  
**Authentication**: Required ✅  
**Path Parameters**:
- `analysisId` (Guid): ID of the analysis record

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Analysis retrieved successfully",
  "data": {
    "id": "660e8400-e29b-41d4-a716-446655440000",
    "fileName": "image.jpg",
    "riskScore": 45,
    "confidenceScore": 92,
    "classification": "Safe",
    "analysisDetails": "Detailed analysis description",
    "categories": [...],
    "analyzedAt": "2026-04-24T10:30:00Z",
    "createdAt": "2026-04-24T10:30:00Z"
  }
}
```

**Error** (404 Not Found):
```json
{
  "success": false,
  "message": "Analysis not found",
  "data": null
}
```

---

### 6. Delete Analysis
**Endpoint**: `DELETE /api/imageanalysis/{analysisId}`  
**Authentication**: Required ✅  
**Path Parameters**:
- `analysisId` (Guid): ID of the analysis record

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Analysis deleted successfully",
  "data": true
}
```

**Error** (404 Not Found):
```json
{
  "success": false,
  "message": "Analysis not found",
  "data": null
}
```

---

## Response Format

All API responses follow this standard format:

```json
{
  "success": boolean,
  "message": "Human-readable message",
  "data": null | object | array
}
```

- **success**: `true` if operation succeeded, `false` otherwise
- **message**: Description of the result or error
- **data**: Actual response payload (null if error)

---

## HTTP Status Codes

| Code | Meaning |
|------|---------|
| 200  | OK - Request succeeded |
| 201  | Created - Resource created |
| 400  | Bad Request - Invalid input |
| 401  | Unauthorized - Missing/invalid token |
| 403  | Forbidden - No permission |
| 404  | Not Found - Resource not found |
| 409  | Conflict - Resource already exists |
| 500  | Internal Server Error |
| 503  | Service Unavailable |

---

## Rate Limiting

Currently: **No rate limiting** (Good to have for production)

Recommended for production:
- 100 requests/minute per user
- 1000 requests/hour per IP

---

## Error Codes

| Code | Description | HTTP Status |
|------|-------------|-------------|
| `EMAIL_EXISTS` | Email already registered | 409 |
| `USERNAME_EXISTS` | Username already taken | 409 |
| `INVALID_CREDENTIALS` | Wrong email or password | 401 |
| `FILE_TOO_LARGE` | Uploaded file exceeds limit | 400 |
| `INVALID_FILE_TYPE` | File format not supported | 400 |
| `API_AUTH_FAILED` | Gemini API key invalid | 500 |
| `API_RATE_LIMIT` | Too many API requests | 429 |
| `UNAUTHORIZED` | Invalid/missing JWT token | 401 |

---

## JWT Token Format

JWT tokens contain three parts separated by dots:
```
header.payload.signature
```

**Payload example**:
```json
{
  "sub": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com",
  "iat": 1619273600,
  "exp": 1619360000,
  "iss": "RiskAnalyzerApi",
  "aud": "RiskAnalyzerApp"
}
```

- `sub`: User ID (Subject)
- `email`: User email
- `iat`: Issued at (timestamp)
- `exp`: Expiration (timestamp)
- `iss`: Issuer
- `aud`: Audience

---

## Example Workflows

### Complete User Journey

```
1. Register User
   POST /api/auth/register
   ↓ returns token
   
2. Store token in localStorage
   
3. Upload Image for Analysis
   POST /api/imageanalysis (with Bearer token)
   ↓ returns analysis result
   
4. View Analysis Results
   GET /api/imageanalysis (with Bearer token)
   ↓ returns history
   
5. Delete Old Analysis
   DELETE /api/imageanalysis/{id} (with Bearer token)
   ↓ returns success
```
