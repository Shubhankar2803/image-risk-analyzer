import { Component, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { NavbarComponent } from './navbar/navbar.component';
import { UploadSectionComponent } from './upload-section/upload-section.component';
import { AnalysisHistoryComponent } from './analysis-history/analysis-history.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, NavbarComponent, UploadSectionComponent, AnalysisHistoryComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  @ViewChild(AnalysisHistoryComponent) analysisHistory: AnalysisHistoryComponent | undefined;

  currentUser: any;
  showAnalysisHistory: boolean = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadCurrentUser();
  }

  loadCurrentUser(): void {
    const user = this.authService.getCurrentUser();
    if (user) {
      this.currentUser = user;
    } else {
      this.router.navigate(['/login']);
    }
  }

  toggleAnalysisHistory(): void {
    this.showAnalysisHistory = !this.showAnalysisHistory;
  }

  onUploadComplete(): void {
    this.showAnalysisHistory = true;
    // Reload analyses after upload
    if (this.analysisHistory) {
      setTimeout(() => {
        this.analysisHistory?.reloadAnalyses();
      }, 500);
    }
  }
}
