import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface DateRange {
  startDate: any;
  endDate: any;
  label: string;
}

@Component({
  selector: 'app-date-range-picker',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './date-range-picker.component.html',
  styleUrl: './date-range-picker.component.css',
})
export class DateRangePickerComponent {
  @Output() rangeSelected = new EventEmitter<DateRange>();

  activeRange: string | null = null;
  ranges: Record<string, DateRange>;
  readonly rangeOrder: string[] = [
    'lastMonth',
    'thisMonth',
    'last7Days',
    'yesterday',
    'today',
  ];

  constructor() {
    const now = DateRangePickerComponent.getNow();
    this.ranges = {
      lastMonth: {
        label: 'Mes Pasado',
        startDate: DateRangePickerComponent.getFirstDayOfLastMonth(),
        endDate: DateRangePickerComponent.getLastDayOfLastMonth(),
      },
      thisMonth: {
        label: 'Este Mes',
        startDate:
          DateRangePickerComponent.getFirstDayOfMonth(now).toISOString(),
        endDate: DateRangePickerComponent.getEndOfDay(now).toISOString(),
      },
      last7Days: {
        label: 'Últimos 7 días',
        startDate: DateRangePickerComponent.getStartOfDay(
          DateRangePickerComponent.addDays(now, -7)
        ).toISOString(),
        endDate: DateRangePickerComponent.getEndOfDay(now).toISOString(),
      },
      yesterday: {
        label: 'Ayer',
        startDate: DateRangePickerComponent.getStartOfDay(
          DateRangePickerComponent.addDays(now, -1)
        ).toISOString(),
        endDate: DateRangePickerComponent.getEndOfDay(
          DateRangePickerComponent.addDays(now, -1)
        ).toISOString(),
      },
      today: {
        label: 'Hoy',
        startDate: DateRangePickerComponent.getStartOfDay(now).toISOString(),
        endDate: DateRangePickerComponent.getEndOfDay(now).toISOString(),
      },
    };
  }

  selectRange(rangeKey: string): void {
    const range = this.ranges[rangeKey];
    if (range) {
      this.activeRange = rangeKey;
      this.rangeSelected.emit(range);
    }
  }

  clearActiveRange(): void {
    this.activeRange = null;
  }

  hasActiveRange(): boolean {
    return this.activeRange !== null;
  }

  getActiveRange(): string | null {
    return this.activeRange;
  }

  getButtonClasses(rangeKey: string): string {
    const baseClasses =
      'px-4 py-2 text-sm font-medium rounded-xl transition-all duration-200 transform hover:scale-105 border-2 shadow-sm';
    const activeClasses =
      'bg-gradient-to-r from-amber-500 to-orange-500 text-white border-amber-600 shadow-lg';
    const inactiveClasses =
      'bg-white/80 backdrop-blur-sm text-amber-700 border-amber-200 hover:bg-amber-50 hover:border-amber-300';

    return this.activeRange === rangeKey
      ? `${baseClasses} ${activeClasses}`
      : `${baseClasses} ${inactiveClasses}`;
  }

  private static getNow(): Date {
    return new Date();
  }

  private static getStartOfDay(date: Date): Date {
    const d = new Date(date);
    d.setHours(0, 0, 0, 0);
    return d;
  }

  private static getEndOfDay(date: Date): Date {
    const d = new Date(date);
    d.setHours(23, 59, 59, 999);
    return d;
  }

  private static addDays(date: Date, days: number): Date {
    const d = new Date(date);
    d.setDate(d.getDate() + days);
    return d;
  }

  private static getFirstDayOfMonth(date: Date): Date {
    const d = new Date(date);
    d.setDate(1);
    d.setHours(0, 0, 0, 0);
    return d;
  }

  private static getFirstDayOfLastMonth(): any {
    const d = new Date();
    d.setMonth(d.getMonth() - 1);
    d.setDate(1);
    d.setHours(0, 0, 0, 0);
    return d.toISOString();
  }

  private static getLastDayOfLastMonth(): any {
    const now = new Date();
    const d = new Date(now.getFullYear(), now.getMonth(), 1);
    d.setDate(0);
    return d.toISOString();
  }
}
