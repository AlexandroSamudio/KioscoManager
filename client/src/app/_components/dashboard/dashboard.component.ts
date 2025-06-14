import { Component, OnInit, WritableSignal, inject, signal } from '@angular/core';
import { ProductoService } from '../../_services/producto.service';
import { Producto } from '../../_models/producto.model';
import { Venta } from '../../_models/venta.model';
import { ProductoMasVendido } from '../../_models/producto-mas-vendido.model';
import { NavbarComponent } from '../navbar/navbar.component';
import { CommonModule } from '@angular/common';
import { VentaService } from '../../_services/venta.service';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [NavbarComponent, CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  productoService = inject(ProductoService);
  ventaService = inject(VentaService);
  lowestStockProducts: WritableSignal<Producto[]> = signal([]);
  totalVentasDia: WritableSignal<number> = signal(0);
  ventasRecientes: WritableSignal<Venta[]> = signal([]);
  productosMasVendidosDelDia: WritableSignal<ProductoMasVendido[]> = signal([]);

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    forkJoin({
      lowStock: this.productoService.getProductosByLowestStock(),
      totalVentas: this.ventaService.getTotalVentasDia(),
      recientes: this.ventaService.getVentasRecientes(),
      masVendidos: this.productoService.getProductosMasVendidosDelDia()
    }).subscribe({
      next: ({ lowStock, totalVentas, recientes, masVendidos }) => {
        this.lowestStockProducts.set(lowStock as Producto[]);
        this.totalVentasDia.set(totalVentas as number);
        this.ventasRecientes.set(recientes as Venta[]);
        this.productosMasVendidosDelDia.set(masVendidos as ProductoMasVendido[]);
      },
      error: err => console.error('Error cargando dashboard', err)
    });
  }

  getTotalVentasDia():void{
    this.ventaService.getTotalVentasDia().subscribe({
      next: (data) => {
        console.log('Total ventas del día:', data);
        this.totalVentasDia.set(data as number);
      },
      error: (error) => {
        console.error('Error fetching total ventas del día:', error);
      }
    });
  }

  getProductosByLowestStock(): void {
    this.productoService.getProductosByLowestStock().subscribe({
      next: (data) => {
        console.log('Productos con menor stock:', data);
        this.lowestStockProducts.set(data as Producto[]);
      },
      error: (error) => {
        console.error('Error fetching productos by lowest stock:', error);
      }
    });
  }

  getVentasRecientes(): void {
    this.ventaService.getVentasRecientes().subscribe({
      next: (data) => {
        console.log('Ventas recientes:', data);
        this.ventasRecientes.set(data as Venta[]);
      },
      error: (error) => {
        console.error('Error fetching ventas recientes:', error);
      }
    });
  }

  getProductosMasVendidosDelDia(): void {
    this.productoService.getProductosMasVendidosDelDia().subscribe({
      next: (data) => {
        console.log('Productos más vendidos del día:', data);
        this.productosMasVendidosDelDia.set(data as ProductoMasVendido[]);
      },
      error: (error) => {
        console.error('Error fetching productos más vendidos del día:', error);
      }
    });
  }
}
