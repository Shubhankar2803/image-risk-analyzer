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
}
