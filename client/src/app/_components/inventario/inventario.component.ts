import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from '../navbar/navbar.component';
import { ProductoService } from '../../_services/producto.service';
import { Producto } from '../../_models/producto.model';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-inventario',
  imports: [NavbarComponent, CommonModule, FormsModule],
  templateUrl: './inventario.component.html',
  styleUrl: './inventario.component.css'
})
export class InventarioComponent implements OnInit {
  private productoService = inject(ProductoService);

  currentPage = signal<number>(1);
  pageSize = signal<number>(4);
  isLoading = signal<boolean>(false);
  selectedCategoriaId = signal<number | undefined>(undefined);
  stockStatus = signal<string>('');
  searchTerm: string = '';

  onSearch(): void {
    this.currentPage.set(1);
    this.loadProductos();
  }

  get productosPaginados() {
    return this.productoService.productosPaginados();
  }

  loadProductos(): void {
    this.isLoading.set(true);
    this.productoService.getProductos(
      this.currentPage(),
      this.pageSize(),
      this.selectedCategoriaId(),
      this.stockStatus(),
      this.searchTerm
    );
    this.isLoading.set(false);
  }

  ngOnInit(): void {
    this.loadProductos();
    console.log(this.productosPaginados?.pagination?.currentPage);
  }

  onPageChange(page: number): void {
    this.currentPage.set(page);
    this.loadProductos();
  }

  onPageSizeChange(pageSize: number): void {
    this.pageSize.set(pageSize);
    this.currentPage.set(1);
    this.loadProductos();
  }

  onCategoriaChange(categoriaId: string): void {
    this.selectedCategoriaId.set(parseInt(categoriaId));
    this.currentPage.set(1);
    this.loadProductos();
  }

  onStockStatusChange(status: string): void {
    this.stockStatus.set(status);
    this.currentPage.set(1);
    this.loadProductos();
  }

  getStockStatusClass(stock: number): string {
    if (stock <= 3) {
      return 'inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-red-100 text-red-800';
    } else if (stock <= 10) {
      return 'inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-yellow-100 text-yellow-800';
    } else {
      return 'inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-green-100 text-green-800';
    }
  }

  getVisiblePages(currentPage: number, totalPages: number): (number | string)[] {
    const visiblePages: (number | string)[] = [];
    const maxVisiblePages = 5;

    if (totalPages <= maxVisiblePages) {
      for (let i = 1; i <= totalPages; i++) {
        visiblePages.push(i);
      }
    } else {
      if (currentPage <= 3) {
        for (let i = 1; i <= 4; i++) {
          visiblePages.push(i);
        }
        visiblePages.push('...');
        visiblePages.push(totalPages);
      } else if (currentPage >= totalPages - 2) {
        visiblePages.push(1);
        visiblePages.push('...');
        for (let i = totalPages - 3; i <= totalPages; i++) {
          visiblePages.push(i);
        }
      } else {
        visiblePages.push(1);
        visiblePages.push('...');
        for (let i = currentPage - 1; i <= currentPage + 1; i++) {
          visiblePages.push(i);
        }
        visiblePages.push('...');
        visiblePages.push(totalPages);
      }
    }

    return visiblePages;
  }

  trackByPage(index: number, page: number | string): number | string {
    return page;
  }

  trackById(index: number, item: Producto): number {
    return item.id;
  }
}
