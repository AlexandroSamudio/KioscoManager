import { CommonModule, CurrencyPipe } from '@angular/common';
import { Component, OnInit, inject, signal, computed, HostListener, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { Producto } from '../../_models/producto.model';
import { CompraCreate, CompraDetalleCreate } from '../../_models/compra-create.model';
import { CompraService } from '../../_services/compra.service';
import { ProductoService } from '../../_services/producto.service';
import { NotificationService } from '../../_services/notification.service';
import { CompraDetalleListComponent } from './compra-detalle-list.component';
import { NavbarComponent } from '../../_components/navbar/navbar.component';
import { debounceTime, distinctUntilChanged, filter, Subject, switchMap, catchError, of } from 'rxjs';

@Component({
  selector: 'app-registrar-compra',
  standalone: true,
  imports: [CommonModule, FormsModule, CompraDetalleListComponent, NavbarComponent, CurrencyPipe],
  templateUrl: './registrar-compra.component.html',
  styleUrl: './registrar-compra.component.css'
})
export class RegistrarCompraComponent implements OnInit {
  private compraService = inject(CompraService);
  private notificationService = inject(NotificationService);
  private productoService = inject(ProductoService);
  private destroyRef = inject(DestroyRef);

  productoActual = signal<Producto | null>(null);
  searchTerm = signal<string>('');
  cantidad = signal<number>(1);
  costoUnitario = signal<number>(0);
  proveedor = signal<string>('');
  nota = signal<string>('');
  itemsCompra = signal<CompraDetalleCreate[]>([]);
  cargando = signal<boolean>(false);
  buscando = signal<boolean>(false);
  productosEncontrados = signal<Producto[]>([]);
  mostrarResultados = computed(() =>
    this.productosEncontrados().length > 0 && this.searchTerm().length > 2
  );
  selectedProductIndex = signal<number>(-1);

  private searchTerms = new Subject<string>();

  ngOnInit(): void {
    this.searchTerms.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      filter(term => term.length >= 3),
      switchMap(term => {
        this.buscando.set(true);
        return this.productoService.getProductos(1, 10, undefined, undefined, term).pipe(
          catchError(() => {
            this.notificationService.showError('Error al buscar productos');
            return of(null);
          })
        );
      }),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (response) => {
        if (response && response.body) {
          this.productosEncontrados.set(response.body);
          this.productoService.productosPaginados.update(value => {
            if (value && response.body) {
              return {
                ...value,
                items: response.body
              };
            }
            return value;
          });
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

  agregarProducto(): void {
    if (!this.productoActual()) {
      this.notificationService.showWarning('Debe seleccionar un producto primero');
      return;
    }

    if (this.cantidad() <= 0) {
      this.notificationService.showWarning('La cantidad debe ser mayor que cero');
      return;
    }

    if (this.costoUnitario() <= 0) {
      this.notificationService.showWarning('El costo unitario debe ser mayor que cero');
      return;
    }

    const nuevoItem: CompraDetalleCreate = {
      productoId: this.productoActual()!.id,
      cantidad: this.cantidad(),
      costoUnitario: this.costoUnitario(),
      productoSku: this.productoActual()!.sku
    };

    const index = this.itemsCompra().findIndex(item => item.productoId === nuevoItem.productoId);

    if (index >= 0) {
      const itemsActualizados = [...this.itemsCompra()];
      itemsActualizados[index] = {
        ...itemsActualizados[index],
        cantidad: itemsActualizados[index].cantidad + nuevoItem.cantidad,
        costoUnitario: nuevoItem.costoUnitario,
        productoSku: nuevoItem.productoSku
      };
      this.itemsCompra.set(itemsActualizados);
      this.notificationService.showSuccess('Producto actualizado en la lista');
    } else {
      this.itemsCompra.set([...this.itemsCompra(), nuevoItem]);
      this.notificationService.showSuccess('Producto agregado a la lista');
    }

    this.cantidad.set(1);
    this.costoUnitario.set(0);
    this.productoActual.set(null);
  }

  eliminarProducto(index: number): void {
    const itemsActualizados = this.itemsCompra().filter((_, i) => i !== index);
    this.itemsCompra.set(itemsActualizados);
    this.notificationService.showInfo('Producto eliminado de la lista');
  }

  calcularTotal(): number {
    return this.itemsCompra().reduce(
      (total, item) => total + item.cantidad * item.costoUnitario,
      0
    );
  }

  registrarCompra(): void {
    if (this.itemsCompra().length === 0) {
      this.notificationService.showWarning('Debe agregar al menos un producto a la compra');
      return;
    }

    this.cargando.set(true);

    const compra: CompraCreate = {
      detalles: this.itemsCompra(),
      proveedor: this.proveedor() || undefined,
      nota: this.nota() || undefined
    };

    this.compraService.createCompra(compra).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.notificationService.showSuccess('Compra registrada correctamente');
        this.resetForm();
        this.cargando.set(false);
      },
      error: (error) => {
        console.error('Error al registrar la compra:', error);
        this.notificationService.showError('Error al registrar la compra: ' + (error.error || 'Ocurrió un error inesperado'));
        this.cargando.set(false);
      }
    });
  }

  cancelarCompra(): void {
    if (this.itemsCompra().length > 0) {
      this.notificationService.showConfirmation(
        '¿Está seguro que desea cancelar esta compra? Se perderán todos los datos ingresados.',
        () => {
          this.resetForm();
          this.notificationService.showInfo('Compra cancelada');
        }
      );
    } else {
      this.resetForm();
    }
  }

  private resetForm(): void {
    this.searchTerm.set('');
    this.cantidad.set(1);
    this.costoUnitario.set(0);
    this.productoActual.set(null);
    this.proveedor.set('');
    this.nota.set('');
    this.itemsCompra.set([]);
  }

  buscarProductos(term: string): void {
    this.searchTerm.set(term);
    this.searchTerms.next(term);
  }

  seleccionarProducto(producto: Producto): void {
    this.productoActual.set(producto);
    this.costoUnitario.set(producto.precioCompra || 0);
    this.searchTerm.set('');
    this.productosEncontrados.set([]);
    this.selectedProductIndex.set(-1);
  }

  limpiarBusqueda(): void {
    this.searchTerm.set('');
    this.productosEncontrados.set([]);
    this.buscando.set(false);
    this.selectedProductIndex.set(-1);
  }

  @HostListener('document:click', ['$event'])
  clickOutside(event: Event) {
    if (this.mostrarResultados()) {
      this.productosEncontrados.set([]);
      this.selectedProductIndex.set(-1);
    }
  }

  @HostListener('keydown', ['$event'])
  handleKeyboardEvent(event: KeyboardEvent) {
    if (!this.mostrarResultados()) return;

    const productos = this.productosEncontrados();
    const currentIndex = this.selectedProductIndex();

    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault();
        this.selectedProductIndex.set(Math.min(currentIndex + 1, productos.length - 1));
        break;
      case 'ArrowUp':
        event.preventDefault();
        this.selectedProductIndex.set(Math.max(currentIndex - 1, -1));
        break;
      case 'Enter':
        event.preventDefault();
        if (currentIndex >= 0 && currentIndex < productos.length) {
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
