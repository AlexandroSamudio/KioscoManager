import {
  Component,
  WritableSignal,
  inject,
  signal,
  DestroyRef,
  ViewChild,
  computed,
  OnInit,
} from '@angular/core';
import { ProductoService } from '../../_services/producto.service';
import { Producto } from '../../_models/producto.model';
import { Venta } from '../../_models/venta.model';
import { ProductoMasVendido } from '../../_models/producto-mas-vendido.model';
import { NavbarComponent } from '../navbar/navbar.component';
import { CommonModule } from '@angular/common';
import { VentaService } from '../../_services/venta.service';
import { forkJoin } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { LineChartComponent } from '../line-chart/line-chart.component';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [NavbarComponent, CommonModule, LineChartComponent, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})
export class DashboardComponent implements OnInit {
  @ViewChild(LineChartComponent) lineChartComponent?: LineChartComponent;

  productoService = inject(ProductoService);
  ventaService = inject(VentaService);
  destroyRef = inject(DestroyRef);
  router = inject(Router);

  lowestStockProducts: WritableSignal<Producto[]> = signal([]);
  totalVentasDia: WritableSignal<number> = signal(0);
  ventasRecientes: WritableSignal<Venta[]> = signal([]);
  productosMasVendidosDelDia: WritableSignal<ProductoMasVendido[]> = signal([]);
  capitalInvertido: WritableSignal<number> = signal(0);
  totalProductos: WritableSignal<number> = signal(0);

  hayDatosDisponibles = computed(() => {
    return (
      this.lowestStockProducts().length > 0 ||
      this.ventasRecientes().length > 0 ||
      this.productosMasVendidosDelDia().length > 0
    );
  });

  ngOnInit(): void {
    this.loadDashboardData();
  }

  trackById(index: number, item: any): number {
    return item?.id || index;
  }

  loadDashboardData(): void {
    forkJoin({
      lowStock: this.productoService.getProductosByLowestStock(),
      totalVentas: this.ventaService.getTotalVentasDia(),
      recientes: this.ventaService.getVentasRecientes(),
      masVendidos: this.productoService.getProductosMasVendidosDelDia(),
      capital: this.productoService.getCapitalInvertido(),
      productosTotal: this.productoService.getTotalProductos(),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ lowStock, totalVentas, recientes, masVendidos, capital, productosTotal }) => {
          this.lowestStockProducts.set(lowStock);
          this.totalVentasDia.set(totalVentas);
          this.ventasRecientes.set(recientes);
          this.productosMasVendidosDelDia.set(masVendidos);
          this.capitalInvertido.set(capital);
          this.totalProductos.set(productosTotal);
        },
        error: (error) => {
          console.error('Error cargando datos del dashboard:', error);
        },
      });
  }

  getProgressPercentage(producto: ProductoMasVendido): number {
    const topProductos = this.productosMasVendidosDelDia();
    if (topProductos.length === 0) return 0;
    if (topProductos.length === 0 || topProductos[0].cantidadVendida === 0)
      return 0;
    return (producto.cantidadVendida / topProductos[0].cantidadVendida) * 100;
  }

}
