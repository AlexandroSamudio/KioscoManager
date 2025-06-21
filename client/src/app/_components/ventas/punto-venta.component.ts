import { Component, DestroyRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NavbarComponent } from '../navbar/navbar.component';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { VentaService } from '../../_services/venta.service';
import { NotificationService } from '../../_services/notification.service';
import { CartService } from '../../_services/cart.service';
import { CartItem } from '../../_models/venta-create.model';

@Component({
  selector: 'app-pos',
  standalone: true,
  imports: [CommonModule, FormsModule, NavbarComponent],
  templateUrl: './punto-venta.component.html',
  styleUrl: './punto-venta.component.css',
})
export class PuntoVentaComponent {
  private destroyRef = inject(DestroyRef);
  private ventaService = inject(VentaService);
  private notificationService = inject(NotificationService);
  private cartService = inject(CartService);

  skuInput = '';
  isLoading = false;
  finalizingSale = false;
  lastCompletedSale: { id: number; total: number } | null = null;

  readonly cartItems = this.cartService.items;
  readonly cartEmpty = this.cartService.isEmpty;
  readonly totalProductos = this.cartService.totalItems;
  readonly totalVenta = this.cartService.totalAmount;

  onSkuSubmit(): void {
    if (!this.skuInput.trim()) {
      this.notificationService.warning(
        'Ingrese un SKU',
        'El campo SKU está vacío'
      );
      return;
    }

    this.isLoading = true;
    const skuBuscado = this.skuInput.trim();

    this.ventaService
      .getProductoBySku(skuBuscado)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (producto) => {
          this.cartService.addProduct(producto);
          this.skuInput = '';
          this.isLoading = false;
        },
        error: () => {
          this.notificationService.error(
            'Producto no encontrado',
            `No existe un producto con SKU "${skuBuscado}" o no está disponible`
          );
          this.isLoading = false;
        },
      });
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
          this.skuInput = '';

          setTimeout(() => {
            const skuInput = document.getElementById('sku');
            if (skuInput) skuInput.focus();
          }, 500);
        },
        error: () => {
          this.finalizingSale = false;
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
