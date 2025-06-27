import { Component, DestroyRef, inject, HostListener, computed, signal, Output, EventEmitter, Input, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, debounceTime, distinctUntilChanged, filter, switchMap, catchError, of } from 'rxjs';
import { ProductoService } from '../../../_services/producto.service';
import { Producto } from '../../../_models/producto.model';

@Component({
  selector: 'app-product-search',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './product-search.component.html',
  styleUrl: './product-search.component.css'
})
export class ProductSearchComponent {
  private productoService = inject(ProductoService);
  private destroyRef = inject(DestroyRef);

  @ViewChild('searchInput') searchInputRef!: ElementRef<HTMLInputElement>;

  @Input() placeholder: string = 'Ingrese nombre o SKU del producto';
  @Input() label: string = 'Buscar Producto (por Nombre o SKU)';
  @Input() showPriceInfo: boolean = true;
  @Input() showStockInfo: boolean = true;
  @Input() showDescription: boolean = true;

  @Output() productSelected = new EventEmitter<Producto>();
  @Output() searchCleared = new EventEmitter<void>();

  searchTerm = signal<string>('');
  productosEncontrados = signal<Producto[]>([]);
  buscando = signal<boolean>(false);
  selectedProductIndex = signal<number>(-1);

  mostrarResultados = computed(() =>
    this.productosEncontrados().length > 0 && this.searchTerm().length > 2
  );

  private searchTerms = new Subject<string>();

  constructor() {
    this.initializeSearch();
  }

  private initializeSearch(): void {
    this.searchTerms.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      filter(term => term.length >= 3),
      switchMap(term => {
        this.buscando.set(true);
        return this.productoService.getProductos(1, 10, undefined, undefined, term).pipe(
          catchError(() => of({ body: [] }))
        );
      }),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (response) => {
        if (response && response.body) {
          this.productosEncontrados.set(response.body);
        } else {
          this.productosEncontrados.set([]);
        }
        this.buscando.set(false);
      },
      error: () => {
        this.buscando.set(false);
        this.productosEncontrados.set([]);
      }
    });
  }

  buscarProductos(term: string): void {
    this.searchTerm.set(term);
    if (term.length < 3) {
      this.productosEncontrados.set([]);
      this.selectedProductIndex.set(-1);
      return;
    }
    this.searchTerms.next(term);
  }

  seleccionarProducto(producto: Producto): void {
    this.productSelected.emit(producto);
    this.limpiarBusqueda();
  }

  limpiarBusqueda(): void {
    this.searchTerm.set('');
    this.productosEncontrados.set([]);
    this.buscando.set(false);
    this.selectedProductIndex.set(-1);
    this.searchCleared.emit();
  }

  focusSearchInput(): void {
    this.searchInputRef?.nativeElement?.focus();
  }

  @HostListener('document:click', ['$event'])
  clickOutside(event: Event): void {
    if (this.mostrarResultados()) {
      const target = event.target as HTMLElement;
      if (!target.closest('.search-container')) {
        this.productosEncontrados.set([]);
        this.selectedProductIndex.set(-1);
      }
    }
  }

  @HostListener('keydown', ['$event'])
  handleKeyboardEvent(event: KeyboardEvent): void {
    if (!this.mostrarResultados()) return;

    const productos = this.productosEncontrados();
    const currentIndex = this.selectedProductIndex();

    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault();
        const nextIndex = currentIndex < productos.length - 1 ? currentIndex + 1 : 0;
        this.selectedProductIndex.set(nextIndex);
        break;

      case 'ArrowUp':
        event.preventDefault();
        const prevIndex = currentIndex > 0 ? currentIndex - 1 : productos.length - 1;
        this.selectedProductIndex.set(prevIndex);
        break;

      case 'Enter':
        event.preventDefault();
        if (currentIndex >= 0 && productos[currentIndex]) {
          this.seleccionarProducto(productos[currentIndex]);
        }
        break;

      case 'Escape':
        event.preventDefault();
        this.limpiarBusqueda();
        break;
    }
  }
}
