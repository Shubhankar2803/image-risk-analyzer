import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

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

  deleteAnalysis(): void {
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

  /**
   * Determine risk level based on analysis data
   */
  getRiskLevel(): 'high' | 'medium' | 'safe' {
    // Check classification first
    const classification = this.analysis?.classification?.toLowerCase() || '';
    if (classification.includes('high')) return 'high';
    if (classification.includes('medium') || classification.includes('moderate')) return 'medium';
    
    // Fallback to risk score
    const riskScore = this.analysis?.riskScore || 0;
    if (riskScore >= 70) return 'high';
    if (riskScore >= 40) return 'medium';
    return 'safe';
  }

  /**
   * Get the color class for the current risk level
   */
  getRiskColorClass(): string {
    const level = this.getRiskLevel();
    return `risk-${level}`;
  }

  /**
   * Get risk display text
   */
  getRiskDisplayText(): string {
    const level = this.getRiskLevel();
    switch (level) {
      case 'high': return 'HIGH RISK';
      case 'medium': return 'MEDIUM RISK';
      case 'safe': return 'SAFE';
    }
  }
}
