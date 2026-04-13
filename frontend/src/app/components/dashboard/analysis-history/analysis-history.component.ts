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
        // Handle wrapped API response
        const data = response?.data || response || [];
        this.analyses = Array.isArray(data) ? data : [];
        this.loading = false;
        console.log('Analyses loaded:', this.analyses);
        this.cdr.markForCheck();
      },
      error: (err: any) => {
        console.error('Failed to load analyses', err);
        this.analyses = [];
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  onToggleHistory(): void {
    this.toggleHistory.emit();
    if (this.showHistory && this.resultsSection) {
      // Reload analyses when showing history
      this.loadAnalyses();
      setTimeout(() => {
        this.resultsSection?.nativeElement.scrollIntoView({ behavior: 'smooth' });
      }, 100);
    }
  }

  deleteAnalysis(analysisId: string): void {
    this.imageAnalysisService.deleteAnalysis(analysisId).subscribe({
      next: () => {
        this.analyses = this.analyses.filter(a => a.id !== analysisId);
        this.analysisDeleted.emit();
        this.cdr.markForCheck();
      },
      error: (err: any) => {
        console.error('Failed to delete analysis', err);
      }
    });
  }

  trackByAnalysisId(index: number, analysis: any): string {
    return analysis.id;
  }

  reloadAnalyses(): void {
    this.loadAnalyses();
  }
}
