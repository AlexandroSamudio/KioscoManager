import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { CategoriaService } from '../../_services/categoria.service';
import { Categoria } from '../../_models/categoria.model';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from '../navbar/navbar.component';
import { ProductoService } from '../../_services/producto.service';
import { Producto } from '../../_models/producto.model';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, finalize } from 'rxjs/operators';
import { setPaginatedResponseSignal } from '../../_services/pagination.helper';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ProductoFormComponent } from '../producto-form/producto-form.component';
import { NotificationService } from '../../_services/notification.service';

export enum StockStatusEnum {
  LOW = 'low',
  OUT = 'out',
  IN = 'in',
}

@Component({
  selector: 'app-inventario',
  standalone: true,
  imports: [NavbarComponent, CommonModule, FormsModule, ProductoFormComponent],
  templateUrl: './inventario.component.html',
  styleUrl: './inventario.component.css',
})
export class InventarioComponent implements OnInit {
  categorias: Categoria[] = [];
  private productoService = inject(ProductoService);
  private destroyRef = inject(DestroyRef);
  private categoriaService = inject(CategoriaService);
  private notificationService = inject(NotificationService);

  currentPage = signal<number>(1);
  pageSize = signal<number>(5);
  isLoading = signal<boolean>(false);
  selectedCategoriaId = signal<number | undefined>(undefined);
  stockStatus = signal<string>('');
  searchTerm: string = '';
  private searchTermSubject = new Subject<string>();
  stockStatusEnum = StockStatusEnum;

  isProductoFormVisible = false;
  isEditMode = false;
  productoAEditar: Producto | null = null;

  sortColumn: string = '';
  sortDirection: 'asc' | 'desc' = 'asc';

  constructor() {
    this.searchTermSubject
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => {
        this.onSearch();
      });
  }



  onSearch(): void {
    this.currentPage.set(1);
    this.loadProductos();
  }

  get productosPaginados() {
    return this.productoService.productosPaginados();
  }

  loadProductos(): void {
    this.isLoading.set(true);
    this.productoService
      .getProductos(
        this.currentPage(),
        this.pageSize(),
        this.selectedCategoriaId(),
        this.stockStatus(),
        this.searchTerm,
        this.sortColumn,
        this.sortDirection
      )
      .pipe(
        finalize(() => this.isLoading.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (response) => {
          setPaginatedResponseSignal(
            response,
            this.productoService.productosPaginados
          );
        },
      });
  }

  ngOnInit(): void {
    this.categoriaService.getCategorias().subscribe({
      next: (data) => {
        this.categorias = data;
      },
      error: () => {
        this.categorias = [];
      }
    });
    this.loadProductos();
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
    if (!categoriaId) {
      this.selectedCategoriaId.set(undefined);
    } else {
      const parsedId = parseInt(categoriaId, 10);
      this.selectedCategoriaId.set(isNaN(parsedId) ? undefined : parsedId);
    }
    this.currentPage.set(1);
    this.loadProductos();
  }

  onStockStatusChange(status: string): void {
    this.stockStatus.set(status);
    this.currentPage.set(1);
    this.loadProductos();
  }

  onSearchTermChanged(term: string): void {
    this.searchTermSubject.next(term);
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

  getVisiblePages(
    currentPage: number,
    totalPages: number
  ): (number | string)[] {
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
    return page === '...' ? `ellipsis-${index}` : page;
  }

  trackById(index: number, item: Producto): number {
    return item.id;
  }

  openProductoForm(producto?: Producto): void {
    this.isProductoFormVisible = true;
    if (producto) {
      this.isEditMode = true;
      this.productoAEditar = { ...producto };
    } else {
      this.isEditMode = false;
      this.productoAEditar = null;
    }
  }

  closeProductoForm(): void {
    this.isProductoFormVisible = false;
    this.isEditMode = false;
    this.productoAEditar = null;
  }

  onProductoGuardado(producto: Producto): void {
    const mensaje = this.isEditMode
      ? 'El producto fue actualizado correctamente.'
      : 'El producto fue agregado correctamente.';
    this.closeProductoForm();
    this.loadProductos();
    this.notificationService.success('¡Éxito!', mensaje);
  }

  async onDeleteProducto(id: number): Promise<void> {
    const confirmed = await this.notificationService.confirm(
      '¿Estás seguro de que quieres eliminar este producto?',
      'Esta acción no se puede deshacer.'
    );
    if (confirmed) {
      this.productoService
        .deleteProducto(id)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(() => {
          this.loadProductos();
          this.notificationService.warning(
            'Producto eliminado',
            'El producto fue eliminado correctamente.'
          );
        });
    }
  }

  onSort(column: string): void {
    if (this.sortColumn === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }
    this.loadProductos();
  }
}
