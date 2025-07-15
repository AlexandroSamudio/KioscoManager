import {
  Component,
  OnInit,
  WritableSignal,
  inject,
  signal,
  DestroyRef,
  ViewChild,
  HostListener,
  AfterViewInit,
  OnDestroy,
  PLATFORM_ID,
  computed,
} from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Router, NavigationEnd } from '@angular/router';
import { ProductoService } from '../../_services/producto.service';
import { Producto } from '../../_models/producto.model';
import { Venta } from '../../_models/venta.model';
import { ProductoMasVendido } from '../../_models/producto-mas-vendido.model';
import { NavbarComponent } from '../navbar/navbar.component';
import { CommonModule } from '@angular/common';
import { VentaService } from '../../_services/venta.service';
import { forkJoin, filter, Subscription } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { LineChartComponent } from '../line-chart/line-chart.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [NavbarComponent, CommonModule, LineChartComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})
export class DashboardComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild(LineChartComponent) lineChartComponent?: LineChartComponent;

  private platformId = inject(PLATFORM_ID);
  productoService = inject(ProductoService);
  ventaService = inject(VentaService);
  destroyRef = inject(DestroyRef);
  router = inject(Router);

  private routerSubscription?: Subscription;
  private updateInProgress = false;

  lowestStockProducts: WritableSignal<Producto[]> = signal([]);
  totalVentasDia: WritableSignal<number> = signal(0);
  ventasRecientes: WritableSignal<Venta[]> = signal([]);
  productosMasVendidosDelDia: WritableSignal<ProductoMasVendido[]> = signal([]);

  hayDatosDisponibles = computed(() => {
    return (
      this.lowestStockProducts().length > 0 ||
      this.ventasRecientes().length > 0 ||
      this.productosMasVendidosDelDia().length > 0
    );
  });

  @HostListener('window:focus', ['$event'])
  onWindowFocus(): void {
    if (isPlatformBrowser(this.platformId) && !this.updateInProgress) {
      this.updateCharts();
    }
  }

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.loadDashboardData();

      this.routerSubscription = this.router.events
        .pipe(
          filter((event) => event instanceof NavigationEnd),
          filter(
            (event: NavigationEnd) =>
              event.urlAfterRedirects === '/' ||
              event.urlAfterRedirects === '/dashboard'
          ),
          takeUntilDestroyed(this.destroyRef)
        )
        .subscribe(() => {
          if (!this.updateInProgress) {
            this.loadDashboardData();
            this.updateCharts();
          }
        });
    }
  }

  ngAfterViewInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      setTimeout(() => this.updateCharts(), 100);
    }
  }

  ngOnDestroy(): void {
    if (this.routerSubscription) {
      this.routerSubscription.unsubscribe();
    }
  }

  trackById(index: number, item: any): number {
    return item?.id || index;
  }

  navigateTo(route: string): void {
    this.router.navigate([route]);
  }

  loadDashboardData(): void {
    this.updateInProgress = true;

    forkJoin({
      lowStock: this.productoService.getProductosByLowestStock(),
      totalVentas: this.ventaService.getTotalVentasDia(),
      recientes: this.ventaService.getVentasRecientes(),
      masVendidos: this.productoService.getProductosMasVendidosDelDia(),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ lowStock, totalVentas, recientes, masVendidos }) => {
          this.lowestStockProducts.set(lowStock);
          this.totalVentasDia.set(totalVentas);
          this.ventasRecientes.set(recientes);
          this.productosMasVendidosDelDia.set(masVendidos);
          this.updateInProgress = false;
        },
        error: () => {
          this.updateInProgress = false;
        },
      });
  }

  getTotalVentasDia(): void {
    this.ventaService
      .getTotalVentasDia()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((data) => this.totalVentasDia.set(data as number));
  }

  getProductosByLowestStock(): void {
    this.productoService
      .getProductosByLowestStock()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((data) => this.lowestStockProducts.set(data as Producto[]));
  }

  getVentasRecientes(): void {
    this.ventaService
      .getVentasRecientes()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((data) => this.ventasRecientes.set(data as Venta[]));
  }

  getProductosMasVendidosDelDia(): void {
    this.productoService
      .getProductosMasVendidosDelDia()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((data) =>
        this.productosMasVendidosDelDia.set(data as ProductoMasVendido[])
      );
  }

  private updateCharts(): void {
    if (isPlatformBrowser(this.platformId) && this.lineChartComponent) {
      this.lineChartComponent.refreshChart();
    }
  }
}
