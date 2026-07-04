import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ImageAnalysisService } from '../../services/image-analysis.service';

@Component({
  selector: 'app-analysis-detail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './analysis-detail.component.html',
  styleUrl: './analysis-detail.component.css'
})
export class AnalysisDetailComponent implements OnInit {
  analysis: any = null;
  loading = true;
  error: string | null = null;
  toast: { message: string; type: 'success' | 'error' } | null = null;
  private toastTimer: any;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private imageAnalysisService: ImageAnalysisService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.router.navigate(['/dashboard']);
      return;
    }

    this.imageAnalysisService.getAnalysis(id).subscribe({
      next: (response: any) => {
        this.analysis = response?.data || response;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err: any) => {
        console.error('Failed to load analysis', err);
        this.error = 'Failed to load analysis: ' + (err?.error?.message || err?.message || err?.status);
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  deleteAnalysis(): void {
    if (!confirm('Are you sure you want to delete this analysis?')) return;

    const id = this.analysis?.imageId || this.analysis?.id;
    this.imageAnalysisService.deleteAnalysis(id).subscribe({
      next: () => {
        this.showToast('Analysis deleted', 'success');
        setTimeout(() => this.router.navigate(['/dashboard']), 1500);
      },
      error: (err: any) => {
        console.error('Failed to delete', err);
        this.showToast('Failed to delete analysis', 'error');
        this.cdr.markForCheck();
      }
    });
  }

  showToast(message: string, type: 'success' | 'error'): void {
    if (this.toastTimer) clearTimeout(this.toastTimer);
    this.toast = { message, type };
    this.cdr.markForCheck();
    this.toastTimer = setTimeout(() => {
      this.toast = null;
      this.cdr.markForCheck();
    }, 3000);
  }

  getTags(): any[] {
    return this.analysis?.analysisResult?.tags || [];
  }

  getRiskLevel(): 'high' | 'medium' | 'safe' {
    const classification = this.analysis?.classification?.toLowerCase() || '';
    if (classification.includes('high')) return 'high';
    if (classification.includes('medium') || classification.includes('moderate')) return 'medium';

    const riskScore = this.analysis?.analysisResult?.overallRiskScore ?? this.analysis?.riskScore ?? 0;
    if (riskScore >= 0.7) return 'high';
    if (riskScore >= 0.4) return 'medium';
    return 'safe';
  }

  getRiskDisplayText(): string {
    const level = this.getRiskLevel();
    switch (level) {
      case 'high': return 'HIGH RISK';
      case 'medium': return 'MEDIUM RISK';
      case 'safe': return 'SAFE';
    }
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }
}