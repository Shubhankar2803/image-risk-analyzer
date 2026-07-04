import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-analysis-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './analysis-card.component.html',
  styleUrl: './analysis-card.component.css'
})
export class AnalysisCardComponent {
  @Input() analysis: any;
  @Output() deleteRequested = new EventEmitter<string>();

  constructor(private router: Router) {}

  navigateToDetail(): void {
    this.router.navigate(['/analysis', this.analysis.imageId || this.analysis.id]);
  }

  deleteAnalysis(event: MouseEvent): void {
    event.stopPropagation();
    if (confirm('Are you sure you want to delete this analysis?')) {
      this.deleteRequested.emit(this.analysis.id);
    }
  }

  parseCategories(categoriesJson: string): any[] {
    try {
      if (typeof categoriesJson === 'string') {
        return JSON.parse(categoriesJson);
      }
      return categoriesJson || [];
    } catch (e) {
      console.error('Failed to parse categories', e);
      return [];
    }
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

  getRiskColorClass(): string {
    return `risk-${this.getRiskLevel()}`;
  }

  getRiskDisplayText(): string {
    const level = this.getRiskLevel();
    switch (level) {
      case 'high': return 'HIGH RISK';
      case 'medium': return 'MEDIUM RISK';
      case 'safe': return 'SAFE';
    }
  }
}