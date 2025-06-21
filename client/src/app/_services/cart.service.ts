import { computed, Injectable, signal } from '@angular/core';
import { CartItem, VentaCreate } from '../_models/venta-create.model';
import { NotificationService } from './notification.service';
import { Producto } from '../_models/producto.model';

@Injectable({
  providedIn: 'root',
})
export class CartService {
  private cartItems = signal<CartItem[]>([]);

  readonly items = this.cartItems.asReadonly();
  readonly isEmpty = computed(() => this.cartItems().length === 0);
  readonly totalItems = computed(() =>
    this.cartItems().reduce((sum, item) => sum + item.cantidad, 0)
  );
  readonly totalAmount = computed(() =>
    this.cartItems().reduce(
      (sum, item) => sum + item.precioVenta * item.cantidad,
      0
    )
  );

  constructor(private notificationService: NotificationService) {}

  addProduct(producto: Producto): boolean {
    if (producto.stock <= 0) {
      this.notificationService.warning(
        'Sin stock',
        `El producto ${producto.nombre} no tiene stock disponible.`
      );
      return false;
    }

    const items = this.cartItems();
    const existingItemIndex = items.findIndex(
      (item) => item.id === producto.id
    );

    if (existingItemIndex !== -1) {
      const existingItem = items[existingItemIndex];

      if (existingItem.cantidad + 1 > producto.stock) {
        this.notificationService.warning(
          'Stock insuficiente',
          `Solo hay ${producto.stock} unidades disponibles de ${producto.nombre}.`
        );
        return false;
      }

      const updatedItems = [...items];
      updatedItems[existingItemIndex] = {
        ...existingItem,
        cantidad: existingItem.cantidad + 1,
      };

      this.cartItems.set(updatedItems);

      this.notificationService.success(
        'Producto agregado',
        `Se aumentó la cantidad de ${producto.nombre} a ${updatedItems[existingItemIndex].cantidad}`
      );

      return true;
    } else {
      const newItem: CartItem = {
        id: producto.id,
        sku: producto.sku,
        nombre: producto.nombre,
        precioVenta: producto.precioVenta,
        cantidad: 1,
        stock: producto.stock,
        categoriaId: producto.categoriaId,
        categoriaNombre: producto.categoriaNombre,
      };

      this.cartItems.update((items) => [...items, newItem]);

      this.notificationService.success(
        'Producto agregado',
        `${producto.nombre} fue agregado al carrito.`
      );

      return true;
    }
  }

  updateQuantity(itemId: number, quantity: number): boolean {
    const items = this.cartItems();
    const itemIndex = items.findIndex((item) => item.id === itemId);

    if (itemIndex === -1) {
      return false;
    }

    const item = items[itemIndex];

    if (quantity <= 0) {
      this.removeItem(itemId);
      return true;
    }

    if (quantity > item.stock) {
      this.notificationService.warning(
        'Stock insuficiente',
        `Solo hay ${item.stock} unidades disponibles.`
      );
      return false;
    }

    const updatedItems = [...items];
    updatedItems[itemIndex] = {
      ...item,
      cantidad: quantity,
    };

    this.cartItems.set(updatedItems);
    return true;
  }

  removeItem(itemId: number): boolean {
    const items = this.cartItems();
    const item = items.find((item) => item.id === itemId);

    if (!item) {
      return false;
    }

    this.cartItems.update((items) => items.filter((i) => i.id !== itemId));

    this.notificationService.info(
      'Producto eliminado',
      `${item.nombre} fue eliminado del carrito.`
    );

    return true;
  }

  clear(): void {
    this.cartItems.set([]);
  }

  prepareVentaData(): VentaCreate {
    if (this.cartItems().length === 0) {
      throw new Error('El carrito está vacío, no se puede preparar la venta');
    }

    return {
      productos: this.cartItems().map((item) => ({
        productoId: item.id,
        cantidad: item.cantidad,
      })),
    };
  }
}
