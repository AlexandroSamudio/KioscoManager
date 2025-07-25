import {
  Component,
  DestroyRef,
  inject,
  ViewChild,
  ElementRef,
  AfterViewInit,
  WritableSignal,
  signal,
  computed,
  OnDestroy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReporteService } from '../../_services/reporte.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import Chart from 'chart.js/auto';
import { VentaChart } from '../../_models/venta-chart.model';

@Component({
  selector: 'app-line-chart',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './line-chart.component.html',
  styleUrls: ['./line-chart.component.css'],
})
export class LineChartComponent implements AfterViewInit, OnDestroy {
  @ViewChild('chartCanvas') chartCanvas!: ElementRef<HTMLCanvasElement>;

  private reporteService = inject(ReporteService);
  private destroyRef = inject(DestroyRef);
  private chart: Chart | null = null;

  ventasPorDia: WritableSignal<VentaChart[]> = signal([]);

  readonly hasVentasData = computed(() => this.ventasPorDia().length > 0);
  readonly ventasDataArray = computed(() => this.ventasPorDia());

  ngAfterViewInit(): void {
    this.loadVentasPorDia();
  }

  ngOnDestroy(): void {
    if (this.chart) {
      this.chart.destroy();
      this.chart = null;
    }
  }

  public refreshChart(): void {
    this.loadVentasPorDia();
  }

  loadVentasPorDia(): void {
    const fechaActual = new Date();

    this.reporteService
      .getVentasParaChart(fechaActual)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data: VentaChart[]) => {
          this.ventasPorDia.set(data);
          if (data.length > 0) {
            setTimeout(() => this.createChart(), 100);
          }
        },
        error: (error) => {
          console.error(
            'Error al cargar datos de ventas para el gráfico:',
            error
          );
          this.ventasPorDia.set([]);
        },
      });
  }

  createChart(): void {
    if (!this.chartCanvas?.nativeElement) {
      console.warn('Canvas no disponible para crear el gráfico');
      return;
    }

    const ctx = this.chartCanvas.nativeElement.getContext('2d');
    if (!ctx) {
      console.error('No se pudo obtener el contexto 2D del canvas');
      return;
    }

    if (this.chart) {
      this.chart.destroy();
      this.chart = null;
    }

    const sortedData = this.ventasDataArray();

    if (sortedData.length === 0) {
      return;
    }

    const dataOrdenada = [...sortedData].sort(
      (a, b) => new Date(a.fecha).getTime() - new Date(b.fecha).getTime()
    );

    const fullDates = dataOrdenada.map((item) => new Date(item.fecha));

    const labels = fullDates.map((date) => {
      const options: Intl.DateTimeFormatOptions = {
        hour: '2-digit',
        minute: '2-digit',
        timeZone: 'America/Argentina/Buenos_Aires',
      };

      return date.toLocaleString('es-AR', options);
    });

    const data = dataOrdenada.map((item) => item.total);

    const primaryColor = '#d97706';
    const fillColor = 'rgba(217, 119, 6, 0.2)';
    const pointColor = '#b45309';
    const pointHoverColor = '#92400e';

    this.chart = new Chart(ctx, {
      type: 'line',
      data: {
        labels: labels,
        datasets: [
          {
            label: 'Ventas',
            data: data,
            borderColor: primaryColor,
            backgroundColor: fillColor,
            borderWidth: 2,
            tension: 0.2,
            fill: true,
            pointBackgroundColor: pointColor,
            pointBorderColor: '#ffffff',
            pointHoverBackgroundColor: pointHoverColor,
            pointHoverBorderColor: '#ffffff',
            pointRadius: 4,
            pointHoverRadius: 6,
            pointBorderWidth: 2,
            pointHoverBorderWidth: 2,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        font: {
          family:
            "'Inter', ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif",
        },
        plugins: {
          title: {
            display: true,
            font: {
              family: "'Inter', sans-serif",
              size: 18,
              weight: 'bold',
            },
            color: '#78350f',
            padding: 20,
          },
          legend: {
            display: false,
          },
          tooltip: {
            backgroundColor: 'rgba(255, 255, 255, 0.95)',
            titleColor: '#78350f',
            bodyColor: '#78350f',
            borderColor: '#fbbf24',
            borderWidth: 2,
            padding: 12,
            displayColors: false,
            titleFont: {
              family: "'Inter', sans-serif",
              size: 14,
              weight: 'bold',
            },
            bodyFont: {
              family: "'Inter', sans-serif",
              size: 14,
            },
            callbacks: {
              title: (tooltipItems) => {
                const dataIndex = tooltipItems[0].dataIndex;
                const fecha = dataOrdenada[dataIndex].fecha;

                const date = new Date(fecha);
                const options: Intl.DateTimeFormatOptions = {
                  weekday: 'long',
                  day: 'numeric',
                  month: 'long',
                  hour: '2-digit',
                  minute: '2-digit',
                  timeZone: 'America/Argentina/Buenos_Aires',
                };

                return date.toLocaleString('es-AR', options);
              },
              label: (context) => {
                const formatter = new Intl.NumberFormat('es-AR', {
                  style: 'currency',
                  currency: 'ARS',
                  minimumFractionDigits: 0,
                  maximumFractionDigits: 0,
                });
                const value = typeof context.raw === 'number' ? context.raw : 0;
                return `Total: ${formatter.format(value)}`;
              },
            },
          },
        },
        scales: {
          y: {
            beginAtZero: true,
            min: 0,
            max: Math.max(...data) * 1.1 || 1000,
            grid: {
              display: true,
              color: 'rgba(226, 232, 240, 0.7)',
            },
            ticks: {
              color: '#78350f',
              font: {
                family: "'Inter', sans-serif",
                weight: 500,
                size: 12,
              },
              stepSize: Math.ceil((Math.max(...data) || 1000) / 5),
              callback: (value) => {
                const numericValue =
                  typeof value === 'number' ? value : parseFloat(String(value));
                return `$${numericValue.toLocaleString('es-AR', {
                  maximumFractionDigits: 0,
                })}`;
              },
            },
          },
          x: {
            grid: {
              display: false,
            },
            ticks: {
              color: '#78350f',
              font: {
                family: "'Inter', sans-serif",
                weight: 600,
                size: 14,
              },
              maxRotation: 0,
              minRotation: 0,
              autoSkip: true,
              autoSkipPadding: 30,
              maxTicksLimit: dataOrdenada.length > 10 ? 6 : 10,
            },
          },
        },
      },
    });
  }
}
