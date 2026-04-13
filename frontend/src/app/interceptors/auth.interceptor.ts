import { inject } from '@angular/core';
import { HttpInterceptorFn } from '@angular/common/http';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();
  
  console.log('AuthInterceptor: Checking token...', token ? 'Token found' : 'No token');
  
  if (token && !req.url.includes('/login') && !req.url.includes('/register')) {
    console.log('AuthInterceptor: Adding Authorization header');
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  } else if (token) {
    console.log('AuthInterceptor: Skipping auth header for login/register endpoints');
  }
  
  return next(req);
};
