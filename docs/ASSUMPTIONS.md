# Assumptions & Constraints

## System Assumptions

### Functional Assumptions

1. **Single User Analysis Context**
   - Each image analysis is isolated per authenticated user
   - Users can only see their own analysis history
   - No shared or public analyses

2. **Image Processing Model**
   - One image processed at a time (sequential, not parallel)
   - Analysis completes before result is returned
   - No queuing or background processing

3. **Gemini API Availability**
   - Google Gemini API is accessible and responsive
   - Sufficient quota available for user's API calls
   - Network connectivity to external API is reliable

4. **Database Persistence**
   - MySQL server is always accessible
   - Database schema maintained via Entity Framework migrations
   - No automatic data retention policies (data kept indefinitely)

5. **File System Access**
   - Server has write permissions to `/uploads` folder
   - File system is persistent across application restarts
   - Sufficient disk space for image storage

6. **Authentication & Sessions**
   - No concurrent login sessions per user
   - Token expiration handled by frontend refresh
   - No refresh token mechanism (stateless design)

### Technical Assumptions

1. **Network Connectivity**
   - Frontend and backend communicate over HTTP/HTTPS
   - Client has internet access for external AI API
   - No offline capability required

2. **Browser Support**
   - Modern browsers with ES2020+ support
   - JavaScript enabled
   - localStorage available for token storage

3. **Hosting Environment**
   - Server has .NET 10.0 runtime installed
   - Node.js 18+ available for frontend build
   - MySQL 8.0+ accessible

4. **Image Format Handling**
   - Supported formats: JPG, PNG, WEBP, GIF, BMP
   - Maximum file size: 10MB
   - Images can be compressed without data loss
   - MIME type validation sufficient for security

5. **AI Model Behavior**
   - Gemini API returns consistent classification categories
   - Risk scores are always 0-100
   - Confidence scores are always 0-100
   - Response latency: < 30 seconds (configurable timeout)

---

## Performance Constraints

### Throughput
- Supports single user per instance
- No horizontal scaling implemented
- Database connection pool: 10 connections
- API response time target: < 5 seconds

### Storage
- Image storage unlimited (assuming sufficient disk)
- Database query optimization for < 1000 records
- No data archiving or cleanup scheduled

### Scalability
- Single instance deployment only
- No load balancing configured
- No caching layer (Redis)
- Database indexed on UserId and CreatedAt only

---

## Security Constraints

### Authentication
- No multi-factor authentication (MFA)
- No password complexity enforcement
- Tokens expire after 24 hours (no refresh tokens)
- No session revocation capability

### Authorization
- Basic role-based access (user only)
- No admin dashboard or management features
- User can only access their own data
- No API scope limiting

### Data Protection
- No encryption at rest
- No field-level encryption for sensitive data
- Passwords hashed with default algorithm
- API keys stored in config files (dev only)

### API Security
- No rate limiting implemented
- No request signing or nonce validation
- CORS configured for all origins in development
- No API versioning strategy

---

## Operational Constraints

### Deployment
- Manual deployment required
- No CI/CD pipeline configured
- No automated testing in production
- Environment-specific config required

### Monitoring
- No health check endpoints
- No structured logging
- No performance metrics collection
- No error tracking service

### Maintenance
- No automated backups
- No database migration automation
- Manual schema updates required
- No database replication

### Support
- No API documentation generation
- No changelog maintained
- No deprecation warnings
- No API versioning

---

## Data Model Constraints

### Users Table
- Email must be unique
- Username must be unique
- No soft deletes
- Password not retrievable (hashed only)

### ImageAnalyses Table
- One analysis per uploaded image
- Analysis data immutable after creation
- Metadata stored as JSON (Categories)
- File paths hardcoded to /uploads folder

### No Tables For:
- API usage analytics
- Error logs
- Audit trails
- User preferences

---

## Integration Constraints

### External Services
- **Gemini API**: Single model (gemini-2.5-flash), no model switching
- **Email**: No email notifications implemented
- **Storage**: Local filesystem only (no cloud storage)
- **Analytics**: No Google Analytics or tracking

### Third-Party Dependencies
- **Frontend**: No UI component libraries (pure CSS)
- **Backend**: Only necessary .NET packages
- **Build**: Standard Angular CLI and .NET tooling
- **Testing**: Standard xUnit and Jasmine

---


### Known Limitations

1. **Single Tenant**: Not multi-tenant architecture
2. **No Offline Support**: Requires server connectivity
3. **Limited File Types**: Only image formats supported
4. **No Advanced Search**: Basic ID and timestamp only
5. **No Export**: Cannot export analysis results
6. **No Webhooks**: No external integrations
7. **No API Rate Limiting**: Vulnerable to abuse
8. **No Real-time Sync**: Multiple clients not synchronized

### Recommended Improvements

1. **Performance**
   - [ ] Implement Redis caching
   - [ ] Add database query optimization
   - [ ] Implement image processing queue

2. **Security**
   - [ ] Add MFA support
   - [ ] Implement API rate limiting
   - [ ] Add request signing
   - [ ] Enable encryption at rest

3. **Features**
   - [ ] Batch image processing
   - [ ] Advanced filtering and search
   - [ ] Custom classification rules
   - [ ] Webhook integrations
   - [ ] Export functionality

4. **Operations**
   - [ ] Automated backups
   - [ ] Health monitoring
   - [ ] Error tracking
   - [ ] Performance analytics
   - [ ] CI/CD pipeline

5. **Scalability**
   - [ ] Load balancing
   - [ ] Database replication
   - [ ] Cloud storage integration
   - [ ] Microservices architecture

---

## Test Coverage Expectations

### Backend
- **Unit Tests**: 70%+ coverage target
- **Integration Tests**: Critical paths covered
- **E2E Tests**: Not automated

### Frontend
- **Component Tests**: 80%+ coverage target
- **Service Tests**: 85%+ coverage target
- **E2E Tests**: Manual testing only

---

## Data Retention Assumptions

### User Data
- Kept indefinitely (no auto-delete)
- Users cannot export data
- Users can manually delete analyses
- No GDPR data export implemented

### Logs
- Application logs: 7 days retention
- Database logs: 30 days retention
- API audit logs: Not implemented

### Temporary Files
- Uploaded images: Indefinite
- Cache files: None
- Session files: localStorage only

---

## Compliance & Standards

### Not Implemented
-  GDPR compliance
-  HIPAA compliance
-  PCI-DSS compliance
-  CCPA compliance
-  SOC 2 certification
-  Penetration testing
-  Security audit

### Recommendations for Production
- [ ] Conduct security audit
- [ ] Implement compliance checks
- [ ] Add data privacy controls
- [ ] Enable audit logging
- [ ] Setup incident response
