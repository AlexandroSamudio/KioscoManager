import {
  Component,
  Input,
  OnChanges,
  SimpleChanges,
  ViewChild,
  computed,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  VentaPorDia,
  TipoAgrupacion,
} from '../../../_models/venta-por-dia.model';
import { FechaAgrupacionPipe } from '../../../_pipes/fecha-agrupacion.pipe';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartData, ChartType } from 'chart.js';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-ventas-chart',
  standalone: true,
  imports: [CommonModule, BaseChartDirective],
  templateUrl: './ventas-chart.component.html',
  styleUrl: './ventas-chart.component.css',
})
export class VentasChartComponent implements OnChanges {
  @ViewChild(BaseChartDirective) chart?: BaseChartDirective;

  private _ventas = signal<VentaPorDia[]>([]);
  private _isLoading = signal<boolean>(false);

  tipoAgrupacion = computed<TipoAgrupacion>(() => {
    const ventas = this._ventas();
    if (ventas.length === 0) return 'daily';
    return ventas[0]?.tipoAgrupacion ?? 'daily';
  });

  @Input()
  set ventas(value: unknown) {
    if (
      Array.isArray(value) &&
      value.every(
        (v) => v && typeof v === 'object' && 'fecha' in v && 'totalVentas' in v
      )
    ) {
      this._ventas.set(value as VentaPorDia[]);
    } else {
      this._ventas.set([]);
      console.warn(
        'El input de ventas es invalido, se esperaba un array de VentaPorDia[].'
      );
    }
    this.updateChartData();
  }

  get ventas(): VentaPorDia[] {
    return this._ventas();
  }

  @Input()
  set isLoading(value: unknown) {
    this._isLoading.set(typeof value === 'boolean' ? value : false);
    if (typeof value !== 'boolean') {
      console.warn('El input isLoading es invalido, se esperaba un booleano.');
    }
  }

  get isLoading(): boolean {
    return this._isLoading();
  }

  public barChartType: ChartType = 'bar';

  public barChartData: ChartData<'bar'> = {
    labels: [],
    datasets: [
      {
        data: [],
        label: 'Ventas por Período',
        backgroundColor: 'rgba(251, 146, 60, 0.7)',
        borderColor: 'rgb(249, 115, 22)',
        borderWidth: 1,
      },
    ],
  };

  public barChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    font: {
      family:
        "'Inter', ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif",
    },
    scales: {
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
        },
      },
      y: {
        beginAtZero: true,
        grid: {
          color: 'rgba(0,0,0,0.05)',
        },
        ticks: {
          color: '#78350f',
          font: {
            family: "'Inter', sans-serif",
            weight: 600,
            size: 14,
          },
        },
      },
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
        display: true,
        position: 'top',
        labels: {
          color: '#78350f',
          font: {
            size: 14,
            family: "'Inter', sans-serif",
          },
        },
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
          title: (context) => {
            const tipoAgrupacion = this.tipoAgrupacion();
            const label = context[0]?.label || '';

            switch (tipoAgrupacion) {
              case 'weekly':
                return `Ventas de la ${label}`;
              case 'monthly':
                return `Ventas de ${label}`;
              default:
                return `Ventas del ${label}`;
            }
          },
          label: (context) => {
            const tipoAgrupacion = this.tipoAgrupacion();
            const monto = new Intl.NumberFormat('es-ES', {
              style: 'currency',
              currency: 'ARS',
            }).format(context.parsed.y);

            switch (tipoAgrupacion) {
              case 'weekly':
                return `Total semanal: ${monto}`;
              case 'monthly':
                return `Total mensual: ${monto}`;
              default:
                return `Total diario: ${monto}`;
            }
          },
        },
      },
    },
  };

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['ventas'] && this.ventas) {
      this.updateChartData();
    }
  }

  private updateChartData(): void {
    const ventas = this._ventas();
    if (ventas.length === 0) {
      this.resetChartData();
      return;
    }

    const ventasOrdenadas = [...ventas].sort((a, b) => {
      return new Date(a.fecha).getTime() - new Date(b.fecha).getTime();
    });

    const tipoAgrupacion = this.tipoAgrupacion();
    const labels: string[] = [];
    const data: number[] = [];

    ventasOrdenadas.forEach((venta) => {
      const fechaObj = this.procesarFecha(venta.fecha);

      if (isNaN(fechaObj.getTime())) {
        console.warn(`Fecha inválida en venta:`, venta);
        return;
      }

      const labelFormateado = this.formatearLabelFecha(
        fechaObj,
        tipoAgrupacion
      );
      labels.push(labelFormateado);
      data.push(venta.totalVentas);
    });

    this.barChartData = {
      labels,
      datasets: [
        {
          ...this.barChartData.datasets[0],
          data,
        },
      ],
    };

    if (this.chart) {
      this.chart.update();
    }
  }

  private resetChartData(): void {
    this.barChartData = {
      labels: [],
      datasets: [
        {
          ...this.barChartData.datasets[0],
          data: [],
        },
      ],
    };

    if (this.chart) {
      this.chart.update();
    }
  }

  private procesarFecha(fecha: Date | string): Date {
    if (typeof fecha === 'string') {
      const fechaString = fecha.split('T')[0];
      const [year, month, day] = fechaString.split('-').map(Number);
      return new Date(year, month - 1, day);
    }

    const fechaDate = fecha as Date;
    const year = fechaDate.getUTCFullYear();
    const month = fechaDate.getUTCMonth();
    const day = fechaDate.getUTCDate();
    return new Date(year, month, day);
  }

  private formatearLabelFecha(
    fecha: Date,
    tipoAgrupacion: TipoAgrupacion
  ): string {
    const pipe = new FechaAgrupacionPipe();
    return pipe.transform(fecha, tipoAgrupacion);
  }
}
