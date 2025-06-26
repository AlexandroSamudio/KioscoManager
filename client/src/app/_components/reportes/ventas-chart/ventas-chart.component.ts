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
  @Input() ventas: VentaPorDia[] = [];
  @Input() isLoading = false;
  @ViewChild(BaseChartDirective) chart?: BaseChartDirective;

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
              currency: 'EUR'
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

    const labels = ventasOrdenadas.map(venta => {
      const fecha = new Date(venta.fecha);
      return fecha.toLocaleDateString('es-ES', { day: '2-digit', month: '2-digit' });
    });
    const data = ventasOrdenadas.map(venta => venta.totalVentas);

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
