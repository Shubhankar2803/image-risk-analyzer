import { Component, Input, Output, EventEmitter, ChangeDetectorRef, ViewChild, ElementRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ImageAnalysisService } from '../../../services/image-analysis.service';
import { AnalysisCardComponent } from '../analysis-card/analysis-card.component';

@Component({
  selector: 'app-analysis-history',
  standalone: true,
  imports: [CommonModule, AnalysisCardComponent],
  templateUrl: './analysis-history.component.html',
  styleUrl: './analysis-history.component.css'
})
export class AnalysisHistoryComponent implements OnInit {
  @ViewChild('resultsSection') resultsSection: ElementRef | undefined;
  @Input() showHistory: boolean = false;
  @Output() toggleHistory = new EventEmitter<void>();
  @Output() analysisDeleted = new EventEmitter<void>();

  analyses: any[] = [];
  loading: boolean = true;
  loaded: boolean = false;
  toast: { message: string; type: 'success' | 'error' } | null = null;
  private toastTimer: any;

  constructor(
    private imageAnalysisService: ImageAnalysisService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadAnalyses();
  }

  loadAnalyses(): void {
    this.loading = true;
    this.imageAnalysisService.getAllAnalyses().subscribe({
      next: (response: any) => {
        const data = response?.data || response || [];
        this.analyses = Array.isArray(data) ? data : [];
        this.loading = false;
        this.loaded = true;
        this.cdr.markForCheck();
      },
      error: (err: any) => {
        console.error('Failed to load analyses', err);
        this.analyses = [];
        this.loading = false;
        this.loaded = true;
        this.cdr.markForCheck();
      }
    });
  }

  onToggleHistory(): void {
    this.toggleHistory.emit();
    if (!this.showHistory && !this.loaded) {
      this.loadAnalyses();
    }
    if (this.showHistory && this.resultsSection) {
      setTimeout(() => {
        this.resultsSection?.nativeElement.scrollIntoView({ behavior: 'smooth' });
      }, 100);
    }
  }

  deleteAnalysis(analysisId: string): void {
    this.imageAnalysisService.deleteAnalysis(analysisId).subscribe({
      next: () => {
        this.analyses = this.analyses.filter(a => a.imageId !== analysisId && a.id !== analysisId);
        this.analysisDeleted.emit();
        this.showToast('Analysis deleted successfully', 'success');
        this.cdr.markForCheck();
      },
      error: (err: any) => {
        console.error('Failed to delete analysis', err);
        this.showToast('Failed to delete analysis', 'error');
        this.cdr.markForCheck();
      }
    });
  }

  showToast(message: string, type: 'success' | 'error'): void {
    if (this.toastTimer) clearTimeout(this.toastTimer);
    this.toast = { message, type };
    this.toastTimer = setTimeout(() => {
      this.toast = null;
      this.cdr.markForCheck();
    }, 3000);
  }

  trackByAnalysisId(index: number, analysis: any): string {
    return analysis.imageId || analysis.id;
  }

  reloadAnalyses(): void {
    this.loaded = false;
    this.loadAnalyses();
  }
}