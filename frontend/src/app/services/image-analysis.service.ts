import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

@Injectable({
  providedIn: 'root'
})
export class ImageAnalysisService {
  constructor(private apiService: ApiService) {}

  uploadImage(file: File): Observable<any> {
    return this.apiService.uploadImage(file);
  }

  getAllAnalyses(): Observable<any> {
    return this.apiService.getAllAnalyses();
  }

  getAnalysis(id: string): Observable<any> {
    return this.apiService.getAnalysis(id);
  }

  deleteAnalysis(id: string): Observable<any> {
    return this.apiService.deleteAnalysis(id);
  }
}
