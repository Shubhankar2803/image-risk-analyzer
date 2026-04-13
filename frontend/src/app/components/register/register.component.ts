import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule, CommonModule],
  template: `
    <div class="register-container">
      <div class="register-wrapper">
        <div class="register-card">
          <div class="card-header">
            <div class="logo">
              <span class="logo-icon">🖼️</span>
            </div>
            <h1>AI Image Analyzer</h1>
            <p class="subtitle">Create Your Account</p>
          </div>

          <form (ngSubmit)="register()" class="register-form">
            <div class="form-group">
              <label for="fullName">Full Name</label>
              <div class="input-wrapper">
                <span class="input-icon">👤</span>
                <input 
                  id="fullName"
                  type="text" 
                  [(ngModel)]="fullName" 
                  name="fullName" 
                  placeholder="John Doe"
                  required
                />
              </div>
            </div>

            <div class="form-group">
              <label for="email">Email Address</label>
              <div class="input-wrapper">
                <span class="input-icon">✉️</span>
                <input 
                  id="email"
                  type="email" 
                  [(ngModel)]="email" 
                  name="email" 
                  placeholder="john@example.com"
                  required
                />
              </div>
            </div>

            <div class="form-group">
              <label for="username">Username</label>
              <div class="input-wrapper">
                <span class="input-icon">@</span>
                <input 
                  id="username"
                  type="text" 
                  [(ngModel)]="username" 
                  name="username" 
                  placeholder="johndoe"
                  required
                />
              </div>
            </div>

            <div class="form-group">
              <label for="password">Password</label>
              <div class="input-wrapper">
                <span class="input-icon">🔒</span>
                <input 
                  id="password"
                  type="password" 
                  [(ngModel)]="password" 
                  name="password" 
                  placeholder="At least 8 characters"
                  required
                />
              </div>
            </div>

            <button type="submit" [disabled]="loading" class="submit-btn">
              <span *ngIf="!loading">Create Account</span>
              <span *ngIf="loading" class="loading-spinner">
                <span class="spinner"></span> Creating...
              </span>
            </button>
          </form>

          <p class="error-message" *ngIf="error">
            <span class="error-icon">⚠️</span> {{ error }}
          </p>

          <div class="divider"></div>

          <p class="auth-link">
            Already have an account? 
            <a href="/login" class="link">Sign in</a>
          </p>
        </div>

        <div class="register-decoration">
          <div class="shape shape-1"></div>
          <div class="shape shape-2"></div>
          <div class="shape shape-3"></div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .register-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 100vh;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      padding: 20px;
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
    }

    .register-wrapper {
      position: relative;
      width: 100%;
      max-width: 420px;
    }

    .register-card {
      background: white;
      border-radius: 20px;
      padding: 50px 40px;
      box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
      position: relative;
      z-index: 10;
      animation: slideIn 0.6s ease-out;
    }

    @keyframes slideIn {
      from {
        opacity: 0;
        transform: translateY(20px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }

    .card-header {
      text-align: center;
      margin-bottom: 40px;
    }

    .logo {
      margin-bottom: 15px;
    }

    .logo-icon {
      font-size: 48px;
      display: inline-block;
    }

    h1 {
      color: #1a1a2e;
      font-size: 28px;
      margin: 15px 0 8px;
      font-weight: 700;
      letter-spacing: -0.5px;
    }

    .subtitle {
      color: #9ca3af;
      font-size: 14px;
      margin: 0;
      font-weight: 400;
    }

    .register-form {
      margin-bottom: 30px;
    }

    .form-group {
      margin-bottom: 20px;
    }

    label {
      display: block;
      color: #374151;
      font-weight: 600;
      font-size: 14px;
      margin-bottom: 8px;
      letter-spacing: 0.3px;
    }

    .input-wrapper {
      position: relative;
      display: flex;
      align-items: center;
    }

    .input-icon {
      position: absolute;
      left: 14px;
      font-size: 18px;
      pointer-events: none;
    }

    input {
      width: 100%;
      padding: 12px 14px 12px 44px;
      border: 2px solid #e5e7eb;
      border-radius: 10px;
      font-size: 14px;
      background: #f9fafb;
      transition: all 0.3s ease;
      box-sizing: border-box;
      font-family: inherit;
    }

    input::placeholder {
      color: #9ca3af;
    }

    input:focus {
      outline: none;
      border-color: #667eea;
      background: white;
      box-shadow: 0 0 0 4px rgba(102, 126, 234, 0.1);
    }

    .submit-btn {
      width: 100%;
      padding: 12px;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      border: none;
      border-radius: 10px;
      font-size: 16px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.3s ease;
      letter-spacing: 0.3px;
      text-transform: uppercase;
      margin-top: 10px;
    }

    .submit-btn:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 10px 30px rgba(102, 126, 234, 0.3);
    }

    .submit-btn:disabled {
      opacity: 0.7;
      cursor: not-allowed;
    }

    .loading-spinner {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
    }

    .spinner {
      display: inline-block;
      width: 14px;
      height: 14px;
      border: 2px solid rgba(255, 255, 255, 0.3);
      border-top-color: white;
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }

    .error-message {
      background: #fee;
      color: #d32f2f;
      padding: 12px 14px;
      border-radius: 8px;
      margin-bottom: 20px;
      font-size: 14px;
      display: flex;
      align-items: center;
      gap: 8px;
      margin-top: -10px;
    }

    .error-icon {
      font-size: 16px;
    }

    .divider {
      height: 1px;
      background: linear-gradient(90deg, transparent, #e5e7eb, transparent);
      margin: 30px 0;
    }

    .auth-link {
      text-align: center;
      color: #6b7280;
      font-size: 14px;
      margin: 0;
    }

    .link {
      color: #667eea;
      text-decoration: none;
      font-weight: 600;
      transition: all 0.3s ease;
    }

    .link:hover {
      color: #764ba2;
      text-decoration: underline;
    }

    .register-decoration {
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      pointer-events: none;
      z-index: 0;
    }

    .shape {
      position: absolute;
      opacity: 0.1;
      border-radius: 50%;
    }

    .shape-1 {
      width: 300px;
      height: 300px;
      background: #667eea;
      top: -150px;
      right: -150px;
    }

    .shape-2 {
      width: 200px;
      height: 200px;
      background: #764ba2;
      bottom: -100px;
      left: -100px;
    }

    .shape-3 {
      width: 150px;
      height: 150px;
      background: #667eea;
      top: 50%;
      left: -75px;
    }

    @media (max-width: 480px) {
      .register-card {
        padding: 40px 24px;
      }

      h1 {
        font-size: 24px;
      }

      .submit-btn {
        padding: 10px;
        font-size: 14px;
      }
    }
  `]
})
export class RegisterComponent {
  fullName: string = '';
  email: string = '';
  username: string = '';
  password: string = '';
  loading: boolean = false;
  error: string = '';

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  register(): void {
    if (!this.fullName || !this.email || !this.username || !this.password) {
      this.error = 'Please fill in all fields';
      return;
    }

    this.loading = true;
    this.error = '';

    this.authService.register(this.email, this.username, this.password, this.fullName).subscribe({
      next: (response) => {
        this.loading = false;
        if (response.success) {
          this.router.navigate(['/dashboard']);
        }
      },
      error: (error) => {
        this.loading = false;
        this.error = error.error?.error?.message || 'Registration failed. Please try again.';
      }
    });
  }
}
