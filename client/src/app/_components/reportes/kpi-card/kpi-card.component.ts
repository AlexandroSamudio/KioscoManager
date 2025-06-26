import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export type KpiVariant = 'default' | 'success' | 'warning' | 'danger' | 'info';

@Component({
  selector: 'app-kpi-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './kpi-card.component.html',
  styleUrl: './kpi-card.component.css'
})
export class KpiCardComponent {
  @Input() title: string = '';
  @Input() value: string | number = '';
  @Input() subtitle: string = '';
  @Input() variant: KpiVariant = 'default';
  @Input() isLoading: boolean = false;

  get containerClasses(): string {
    return this.variant;
  }

  getValueTextColorClass(): string {
    switch(this.variant) {
      case 'success': return 'text-green-600';
      case 'warning': return 'text-amber-600';
      case 'danger': return 'text-red-600';
      case 'info': return 'text-blue-600';
      default: return 'text-amber-900';
    }
  }
}
