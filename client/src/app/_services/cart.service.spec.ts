import { TestBed } from '@angular/core/testing';
import { CartService } from './cart.service';
import { NotificationService } from './notification.service';
import { Producto } from '../_models/producto.model';


describe('CartService (state)', () => {
  let service: CartService;
  const notifySpy = ({
    warning: jest.fn(),
    success: jest.fn(),
    info: jest.fn(),
  } as unknown) as NotificationService & {
    warning: jest.Mock;
    success: jest.Mock;
    info: jest.Mock;
  };

  const producto = (over: Partial<Producto> = {}): Producto => ({
    id: 1,
    sku: 'A',
    nombre: 'Prod',
    descripcion: 'd',
    precioCompra: 5,
    precioVenta: 10,
    stock: 5,
    categoriaId: 1,
    categoriaNombre: 'Cat',
    imageUrl: undefined,
    ...over,
  });

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [CartService, { provide: NotificationService, useValue: notifySpy }],
    });
    service = TestBed.inject(CartService);
    notifySpy.warning.mockReset();
    notifySpy.success.mockReset();
    notifySpy.info.mockReset();
  });

  it('adds product when stock available, updates totals', () => {
    expect(service.isEmpty()).toBe(true);
    const ok = service.addProduct(producto());
    expect(ok).toBe(true);
    expect(service.totalItems()).toBe(1);
    expect(service.totalAmount()).toBe(10);
  });

  it('adding same product increments quantity and totals within stock', () => {
    const p = producto({ stock: 3, precioVenta: 10 });
    expect(service.addProduct(p)).toBe(true);
    expect(service.addProduct(p)).toBe(true);
    expect(service.totalItems()).toBe(2);
    expect(service.totalAmount()).toBe(20);
  });

  it('prevents adding when no stock', () => {
    const ok = service.addProduct(producto({ stock: 0 }));
    expect(ok).toBe(false);
    expect(notifySpy.warning).toHaveBeenCalled();
  });

  it('prevents increment beyond stock when adding existing item', () => {
    const p = producto({ stock: 1 });
    expect(service.addProduct(p)).toBe(true);
    const ok2 = service.addProduct(p);
    expect(ok2).toBe(false);
    expect(service.totalItems()).toBe(1);
    expect(notifySpy.warning).toHaveBeenCalled();
  });

  it('updateQuantity respects stock and removal on <=0', () => {
    service.addProduct(producto());
    expect(service.updateQuantity(1, 10)).toBe(false);
    expect(service.updateQuantity(1, 0)).toBe(true);
    expect(service.isEmpty()).toBe(true);
  });

  it('removeItem removes and notifies', () => {
    service.addProduct(producto());
    const removed = service.removeItem(1);
    expect(removed).toBe(true);
    expect(notifySpy.info).toHaveBeenCalled();
  });

  it('prepareVentaData throws on empty cart', () => {
    expect(() => service.prepareVentaData()).toThrow();
  });

  it('prepareVentaData returns expected payload on non-empty cart', () => {
    service.addProduct(producto({ id: 1 }));
    service.addProduct(producto({ id: 2, sku: 'B', nombre: 'P2', precioVenta: 15 }));
    service.addProduct(producto({ id: 1 }));
    const payload = service.prepareVentaData();
    expect(payload.productos).toEqual([
      { productoId: 1, cantidad: 2 },
      { productoId: 2, cantidad: 1 },
    ]);
  });
});
