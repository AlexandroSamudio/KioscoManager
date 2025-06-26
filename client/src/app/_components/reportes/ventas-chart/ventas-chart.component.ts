import { Component, Input, OnChanges, SimpleChanges, ViewChild} from '@angular/core';
import { CommonModule } from '@angular/common';
import { VentaPorDia } from '../../../_models/venta-por-dia.model';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartData, ChartType } from 'chart.js';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-ventas-chart',
  standalone: true,
  imports: [CommonModule, BaseChartDirective],
  templateUrl: './ventas-chart.component.html',
  styleUrl: './ventas-chart.component.css'
})
export class VentasChartComponent implements OnChanges {
  @ViewChild(BaseChartDirective) chart?: BaseChartDirective;

  private _ventas: VentaPorDia[] = [];
  private _isLoading = false;

  @Input()
  set ventas(value: unknown) {
    if (Array.isArray(value) && value.every(v => v && typeof v === 'object' && 'fecha' in v && 'totalVentas' in v)) {
      this._ventas = value as VentaPorDia[];
    } else {
      this._ventas = [];
      console.warn('El input de ventas es invalido, se esperaba un array de VentaPorDia[].');
    }
    this.updateChartData();
  }
  get ventas(): VentaPorDia[] {
    return this._ventas;
  }

  @Input()
  set isLoading(value: unknown) {
    this._isLoading = typeof value === 'boolean' ? value : false;
    if (typeof value !== 'boolean') {
      console.warn('El input isLoading es invalido, se esperaba un booleano.');
    }
  }
  get isLoading(): boolean {
    return this._isLoading;
  }

  public barChartType: ChartType = 'bar';

  public barChartData: ChartData<'bar'> = {
    labels: [],
    datasets: [
      {
        data: [],
        label: 'Ventas por Día',
        backgroundColor: 'rgba(251, 146, 60, 0.7)',
        borderColor: 'rgb(249, 115, 22)',
        borderWidth: 1
      }
    ]
  };

  public barChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      x: {
        grid: {
          display: false
        }
      },
      y: {
        beginAtZero: true,
        grid: {
          color: 'rgba(0,0,0,0.05)'
        }
      }
    },
    plugins: {
      legend: {
        display: true,
        position: 'top',
      },
      tooltip: {
        backgroundColor: 'rgba(0,0,0,0.7)',
        bodyFont: {
          size: 13
        },
        titleFont: {
          size: 13,
          weight: 'bold'
        },
        callbacks: {
          label: function(context) {
            return `Monto: ${new Intl.NumberFormat('es-ES', {
              style: 'currency',
              currency: 'ARS'
            }).format(context.parsed.y)}`;
          }
        }
      }
    }
  };

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['ventas'] && this.ventas) {
      this.updateChartData();
    }
  }

  private updateChartData(): void {
    const ventasOrdenadas = [...this.ventas].sort((a, b) => {
      return new Date(a.fecha).getTime() - new Date(b.fecha).getTime();
    });

    const labels: string[] = [];
    const data: number[] = [];

    ventasOrdenadas.forEach(venta => {
      const fechaObj = new Date(venta.fecha);
      if (isNaN(fechaObj.getTime())) {
        console.warn(`Fecha inválida en venta:`, venta);
        return;
      }
      labels.push(fechaObj.toLocaleDateString('es-ES', { day: '2-digit', month: '2-digit' }));
      data.push(venta.totalVentas);
    });

    this.barChartData = {
      labels,
      datasets: [
        {
          ...this.barChartData.datasets[0],
          data
        }
      ]
    };

    if (this.chart) {
      this.chart.update();
    }
  }
}
