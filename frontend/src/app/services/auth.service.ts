import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from './api.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject = new BehaviorSubject<any>(this.getUserFromStorage());
  public currentUser$ = this.currentUserSubject.asObservable();
  
  private tokenSubject = new BehaviorSubject<string | null>(this.getTokenFromStorage());
  public token$ = this.tokenSubject.asObservable();

  constructor(private apiService: ApiService) {}

  register(email: string, username: string, password: string, fullName: string): Observable<any> {
    return this.apiService.register({ email, username, password, fullName }).pipe(
      tap(response => {
        if (response.success) {
          this.storeToken(response.data.token);
          this.storeUser(response.data.user);
          this.currentUserSubject.next(response.data.user);
          this.tokenSubject.next(response.data.token);
        }
      })
    );
  }

  login(email: string, password: string): Observable<any> {
    return this.apiService.login({ email, password }).pipe(
      tap(response => {
        if (response.success) {
          this.storeToken(response.data.token);
          this.storeUser(response.data.user);
          this.currentUserSubject.next(response.data.user);
          this.tokenSubject.next(response.data.token);
        }
      })
    );
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.currentUserSubject.next(null);
    this.tokenSubject.next(null);
  }

  getToken(): string | null {
    return this.getTokenFromStorage();
  }

  getCurrentUser(): any {
    return this.getUserFromStorage();
  }

  isAuthenticated(): boolean {
    return !!this.getTokenFromStorage();
  }

  private storeToken(token: string): void {
    localStorage.setItem('token', token);
  }

  private storeUser(user: any): void {
    localStorage.setItem('user', JSON.stringify(user));
  }

  private getTokenFromStorage(): string | null {
    return localStorage.getItem('token');
  }

  private getUserFromStorage(): any {
    try {
      const user = localStorage.getItem('user');
      return user ? JSON.parse(user) : null;
    } catch (error) {
      console.error('Error parsing user from storage:', error);
      localStorage.removeItem('user');
      return null;
    }
  }
}
