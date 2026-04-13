import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  register(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/auth/register`, data);
  }

  login(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/auth/login`, data);
  }

  uploadImage(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(`${this.apiUrl}/imageanalysis/upload`, formData);
  }

  getAllAnalyses(): Observable<any> {
    return this.http.get(`${this.apiUrl}/imageanalysis`);
  }

  getAnalysis(id: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/imageanalysis/${id}`);
  }

  deleteAnalysis(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/imageanalysis/${id}`);
  }
}
