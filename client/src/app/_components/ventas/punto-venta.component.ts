import { Component, DestroyRef, inject} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NavbarComponent } from '../navbar/navbar.component';
import { ProductSearchComponent } from '../shared/product-search/product-search.component';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { VentaService } from '../../_services/venta.service';
import { NotificationService } from '../../_services/notification.service';
import { CartService } from '../../_services/cart.service';
import { CartItem } from '../../_models/venta-create.model';
import { Producto } from '../../_models/producto.model';

@Component({
  selector: 'app-pos',
  standalone: true,
  imports: [CommonModule, FormsModule, NavbarComponent, ProductSearchComponent],
  templateUrl: './punto-venta.component.html',
  styleUrl: './punto-venta.component.css',
})
export class PuntoVentaComponent {
  private destroyRef = inject(DestroyRef);
  private ventaService = inject(VentaService);
  private notificationService = inject(NotificationService);
  private cartService = inject(CartService);

  isLoading = false;
  finalizingSale = false;
  lastCompletedSale: { id: number; total: number } | null = null;

  readonly cartItems = this.cartService.items;
  readonly cartEmpty = this.cartService.isEmpty;
  readonly totalProductos = this.cartService.totalItems;
  readonly totalVenta = this.cartService.totalAmount;

  onProductSelected(producto: Producto): void {
    this.cartService.addProduct(producto);
  }

  trackById(index: number, item: CartItem): number {
    return item.id;
  }

  updateCantidad(item: CartItem, cantidad: number): void {
    this.cartService.updateQuantity(item.id, cantidad);
  }

  removeProductFromCart(item: CartItem): void {
    this.cartService.removeItem(item.id);
  }

  async onFinalizeSale(): Promise<void> {
    if (this.cartService.isEmpty()) {
      return;
    }

    const confirmed = await this.notificationService.confirm(
      '¿Está seguro de finalizar esta venta?',
      'Confirme para procesar la venta'
    );

    if (confirmed) {
      this.procesarFinalizarVenta();
    }
  }

  private procesarFinalizarVenta(): void {
    this.finalizingSale = true;
    const ventaData = this.cartService.prepareVentaData();

    this.ventaService
      .finalizarVenta(ventaData)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (ventaFinalizada) => {
          this.lastCompletedSale = {
            id: ventaFinalizada.id,
            total: ventaFinalizada.total,
          };

          this.notificationService.success(
            'Venta completada',
            `La venta #${
              ventaFinalizada.id
            } se ha registrado correctamente por $${ventaFinalizada.total.toFixed(
              2
            )}`
          );

          this.cartService.clear();
          this.finalizingSale = false;
        },
        error: () => {
          this.finalizingSale = false;
          this.notificationService.error(
            'Error al procesar la venta',
            'Ocurrió un error al procesar la venta. Por favor, intente nuevamente.'
          );
        },
      });
  }

  async onCancelSale(): Promise<void> {
    if (this.cartService.isEmpty()) {
      return;
    }

    const confirmed = await this.notificationService.confirm(
      '¿Está seguro de cancelar la venta?',
      'Se perderán todos los productos del carrito'
    );

    if (confirmed) {
      this.cartService.clear();
      this.notificationService.info(
        'Venta cancelada',
        'La venta ha sido cancelada'
      );
    }
  }
}
