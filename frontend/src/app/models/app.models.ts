/**
 * Frontend DTO Models - Match backend API contracts
 * These interfaces define the shape of data returned from API endpoints
 */

/**
 * User model - returned from auth and user endpoints
 */
export interface UserDto {
  id: string;
  email: string;
  username: string;
  fullName: string;
  createdAt: string;
  lastLoginAt: string | null;
  isActive: boolean;
}

/**
 * Analysis tag model
 */
export interface AnalysisTagDto {
  name: string;
  confidence: number;
  category: string;
}

/**
 * Image analysis result - core model
 */
export interface ImageAnalysisResponseDto {
  id: string;
  fileName: string;
  fileSizeBytes: number;
  riskScore: number;
  classification: string;
  analysisDetails: string;
  confidenceScore: number;
  analyzedAt: string;
  createdAt: string;
  tags: AnalysisTagDto[];
}

/**
 * File upload request payload
 */
export interface FileUploadRequest {
  file: File;
  fileName: string;
}

/**
 * File upload response
 */
export interface FileUploadResponse {
  id: string;
  fileName: string;
  filePath: string;
  uploadedAt: string;
}

/**
 * Authentication credentials for login
 */
export interface LoginRequest {
  email: string;
  password: string;
}

/**
 * Authentication response with token
 */
export interface AuthResponse {
  token: string;
  user: UserDto;
  expiresIn: number;
}

/**
 * Pagination parameters
 */
export interface PaginationParams {
  page: number;
  pageSize: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

/**
 * Paginated response wrapper
 */
export interface PaginatedResponse<T> {
  data: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

/**
 * API error response
 */
export interface ApiErrorResponse {
  status: number;
  message: string;
  errors?: Record<string, string[]>;
  timestamp: string;
}
