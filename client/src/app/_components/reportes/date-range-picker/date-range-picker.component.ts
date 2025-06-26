import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface DateRange {
  startDate: Date;
  endDate: Date;
  label: string;
}

@Component({
  selector: 'app-date-range-picker',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './date-range-picker.component.html',
  styleUrl: './date-range-picker.component.css'
})
export class DateRangePickerComponent {
  @Output() rangeSelected = new EventEmitter<DateRange>();

  activeRange: string | null = null;
  ranges: Record<string, DateRange>;

  constructor() {
    const now = DateRangePickerComponent.getNow();
    this.ranges = {
      today: {
        label: 'Hoy',
        startDate: DateRangePickerComponent.getStartOfDay(now),
        endDate: DateRangePickerComponent.getEndOfDay(now)
      },
      last7Days: {
        label: 'Últimos 7 días',
        startDate: DateRangePickerComponent.getStartOfDay(DateRangePickerComponent.addDays(now, -7)),
        endDate: DateRangePickerComponent.getEndOfDay(now)
      },
      thisMonth: {
        label: 'Este Mes',
        startDate: DateRangePickerComponent.getFirstDayOfMonth(now),
        endDate: DateRangePickerComponent.getEndOfDay(now)
      },
      lastMonth: {
        label: 'Mes Pasado',
        startDate: DateRangePickerComponent.getFirstDayOfLastMonth(now),
        endDate: DateRangePickerComponent.getLastDayOfLastMonth(now)
      },
      thisYear: {
        label: 'Este Año',
        startDate: DateRangePickerComponent.getFirstDayOfYear(now),
        endDate: DateRangePickerComponent.getEndOfDay(now)
      }
    };
  }

  selectRange(rangeKey: string): void {
    const range = this.ranges[rangeKey];
    if (range) {
      this.activeRange = rangeKey;
      this.rangeSelected.emit(range);
    }
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

  private static getFirstDayOfLastMonth(date: Date): Date {
    const d = new Date(date);
    d.setMonth(d.getMonth() - 1);
    d.setDate(1);
    d.setHours(0, 0, 0, 0);
    return d;
  }

  private static getLastDayOfLastMonth(date: Date): Date {
    const d = new Date(date);
    d.setDate(0);
    d.setHours(23, 59, 59, 999);
    return d;
  }

  private static getFirstDayOfYear(date: Date): Date {
    const d = new Date(date);
    d.setMonth(0);
    d.setDate(1);
    d.setHours(0, 0, 0, 0);
    return d;
  }
}
