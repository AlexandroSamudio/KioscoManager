import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { Categoria } from '../../_models/categoria.model';
import { Producto } from '../../_models/producto.model';
import { CategoriaService } from '../../_services/categoria.service';
import { NotificationService } from '../../_services/notification.service';
import { ProductoService } from '../../_services/producto.service';
import { ProductoFormComponent } from './producto-form.component';

@Component({ selector: 'app-photo-upload', standalone: true, template: '' })
class DummyPhotoUploadComponent {}

describe('ProductoFormComponent (shallow)', () => {
  let productoService: { createProducto: jest.Mock; updateProducto: jest.Mock };
  let categoriaService: { getCategorias: jest.Mock };
  let notificationService: { error: jest.Mock };

  beforeEach(() => {
    productoService = { createProducto: jest.fn(), updateProducto: jest.fn() };
    categoriaService = { getCategorias: jest.fn().mockReturnValue(of<Categoria[]>([])) };
    notificationService = { error: jest.fn() };

    TestBed.configureTestingModule({
      imports: [ProductoFormComponent, DummyPhotoUploadComponent],
      providers: [
        { provide: ProductoService, useValue: productoService },
        { provide: CategoriaService, useValue: categoriaService },
        { provide: NotificationService, useValue: notificationService },
      ],
    });
  });

  function makeValidForm(component: ProductoFormComponent) {
    component.productoForm.setValue({
      nombre: 'Prod',
      sku: '1234567890123',
      descripcion: 'desc',
      precioCompra: 10,
      precioVenta: 20,
      stock: 5,
      categoriaId: 1,
      imageFile: null,
    });
  }

  it('should not submit and mark controls when form invalid', () => {
    const fixture = TestBed.createComponent(ProductoFormComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    component.productoForm.patchValue({ nombre: '' });
    expect(component.productoForm.invalid).toBe(true);

    component.onSubmit();

    expect(productoService.createProducto).not.toHaveBeenCalled();
    expect(component.productoForm.get('nombre')?.touched).toBe(true);
  });

  it('should call createProducto and emit save/close on valid submit (create)', () => {
    const fixture = TestBed.createComponent(ProductoFormComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    const result: Partial<Producto> = { id: 10, nombre: 'Prod' };
    productoService.createProducto.mockReturnValue(of(result));

    const saveSpy = jest.fn();
    const closeSpy = jest.fn();
    component.save.subscribe(saveSpy);
    component.close.subscribe(closeSpy);

    makeValidForm(component);
    component.isEditMode = false;

    component.onSubmit();

    expect(productoService.createProducto).toHaveBeenCalled();
    expect(saveSpy).toHaveBeenCalled();
    const savedArg = saveSpy.mock.calls[0][0];
    expect(savedArg.id).toBe(result.id);
    expect(savedArg.nombre).toBe(result.nombre);
    expect(closeSpy).toHaveBeenCalled();
  });

  it('should call updateProducto when in edit mode with initialProduct', () => {
    const fixture = TestBed.createComponent(ProductoFormComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    const existing: Producto = {
      id: 5,
      nombre: 'Prod', sku: '1234567890123', descripcion: '', precioCompra: 1, precioVenta: 2, stock: 1,
      categoriaId: 2,
    } as any;

    component.isEditMode = true;
    component.initialProduct = existing;
    productoService.updateProducto.mockReturnValue(of(existing));

    const saveSpy = jest.fn();
    component.save.subscribe(saveSpy);

    component.productoForm.markAsUntouched();
    component.productoForm.updateValueAndValidity();

    component.onSubmit();

      expect(productoService.updateProducto).toHaveBeenCalled();
      const updateCall = productoService.updateProducto.mock.calls[0];
      expect(updateCall[0]).toBe(5);
      const fd = updateCall[1];
      expect(fd instanceof FormData).toBe(true);
      expect(saveSpy).toHaveBeenCalled();
  });

  it('should emit close on closeModal()', () => {
    const fixture = TestBed.createComponent(ProductoFormComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    const closeSpy = jest.fn();
    component.close.subscribe(closeSpy);

    component.closeModal();

    expect(closeSpy).toHaveBeenCalled();
  });

  it('should validate sku pattern and required fields', () => {
    const fixture = TestBed.createComponent(ProductoFormComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    component.productoForm.patchValue({ sku: '123' });
    component.productoForm.get('sku')?.markAsTouched();
    expect(component.isFieldInvalid('sku')).toBe(true);
    expect(component.getFieldError('sku', 'pattern')).toBe(true);

    component.productoForm.patchValue({ nombre: '' });
    component.productoForm.get('nombre')?.markAsTouched();
    expect(component.getFieldError('nombre', 'required')).toBe(true);
  });

  it('should notify on error from service', () => {
    const fixture = TestBed.createComponent(ProductoFormComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    makeValidForm(component);
    productoService.createProducto.mockReturnValue(throwError(() => new Error('fail')));
    const consoleSpy = jest.spyOn(console, 'error').mockImplementation(() => {});

    component.onSubmit();

    expect(notificationService.error).toHaveBeenCalled();
    expect(consoleSpy).toHaveBeenCalled();
    consoleSpy.mockRestore();
  });
});
