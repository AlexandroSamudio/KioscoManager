import { Component, Input, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CategoriaRentabilidad } from '../../../_models/categoria-rentabilidad.model';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartData, ChartType } from 'chart.js';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-categorias-chart',
  standalone: true,
  imports: [CommonModule, BaseChartDirective],
  templateUrl: './categorias-chart.component.html',
  styleUrl: './categorias-chart.component.css'
})
export class CategoriasChartComponent implements OnChanges {
  @Input() categorias: CategoriaRentabilidad[] = [];
  @Input() isLoading = false;
  @ViewChild(BaseChartDirective) chart?: BaseChartDirective;

  public pieChartType: ChartType = 'doughnut';

  public pieChartData: ChartData<'doughnut'> = {
    labels: [],
    datasets: [
      {
        data: [],
        backgroundColor: [
          'rgba(251, 146, 60, 0.7)',
          'rgba(249, 115, 22, 0.7)',
          'rgba(234, 88, 12, 0.7)',
          'rgba(217, 119, 6, 0.7)',
          'rgba(245, 158, 11, 0.7)',
          'rgba(202, 138, 4, 0.7)',
          'rgba(180, 83, 9, 0.7)',
          'rgba(146, 64, 14, 0.7)',
          'rgba(194, 65, 12, 0.7)',
          'rgba(154, 52, 18, 0.7)',
        ],
        borderColor: [
          'rgb(251, 146, 60)',
          'rgb(249, 115, 22)',
          'rgb(234, 88, 12)',
          'rgb(217, 119, 6)',
          'rgb(245, 158, 11)',
          'rgb(202, 138, 4)',
          'rgb(180, 83, 9)',
          'rgb(146, 64, 14)',
          'rgb(194, 65, 12)',
          'rgb(154, 52, 18)',
        ],
        borderWidth: 1
      }
    ]
  };

  public pieChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
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
            return `${context.label}: ${context.parsed}%`;
          }
        }
      }
    }
  };

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['categorias'] && this.categorias) {
      this.updateChartData();
    }
  }

  private updateChartData(): void {
    const categoriasOrdenadas = [...this.categorias].sort((a, b) => b.porcentajeVentas - a.porcentajeVentas);

    const topCategorias = categoriasOrdenadas.slice(0, 10);

    const labels = topCategorias.map(cat => cat.nombre);
    const data = topCategorias.map(cat => cat.porcentajeVentas);

    this.pieChartData = {
      labels: labels,
      datasets: [
        {
          data: data,
          backgroundColor: this.pieChartData.datasets[0].backgroundColor,
          borderColor: this.pieChartData.datasets[0].borderColor,
          borderWidth: 1
        }
      ]
    };

    if (this.chart) {
      this.chart.update();
    }
  }
}
