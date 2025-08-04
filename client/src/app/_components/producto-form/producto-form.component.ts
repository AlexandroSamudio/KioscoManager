import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Output, Input, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { inject, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ProductoService } from '../../_services/producto.service';
import { Producto } from '../../_models/producto.model';
import { CategoriaService } from '../../_services/categoria.service';
import { Categoria } from '../../_models/categoria.model';
import { finalize } from 'rxjs';
import { NotificationService } from '../../_services/notification.service';

@Component({
  selector: 'app-producto-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './producto-form.component.html',
  styleUrl: './producto-form.component.css',
})
export class ProductoFormComponent implements OnInit {
  private destroyRef = inject(DestroyRef);
  private productoService = inject(ProductoService);
  private categoriaService = inject(CategoriaService);
  private notificationService = inject(NotificationService);
  private fb = inject(FormBuilder);

  @Input() isVisible = false;
  @Input() isEditMode = false;

  private _initialProduct: Producto | null = null;
  @Input() set initialProduct(value: Producto | null) {
    this._initialProduct = value;
    if (this.isEditMode && value) {
      this.populateForm(value);
    } else {
      this.resetForm();
    }
  }
  get initialProduct(): Producto | null {
    return this._initialProduct;
  }

  @Output() close = new EventEmitter<void>();
  @Output() save = new EventEmitter<Producto>();

  isLoading = false;
  categorias: Categoria[] = [];

  productoForm: FormGroup = this.fb.group({
    nombre: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    sku: ['', [Validators.required, Validators.pattern(/^\d{13}$/)]],
    descripcion: ['', [Validators.maxLength(500)]],
    precioCompra: [0, [Validators.required, Validators.min(0),Validators.max(1000000)]],
    precioVenta: [0, [Validators.required, Validators.min(0),Validators.max(1000000)]],
    stock: [0, [Validators.required, Validators.min(0)]],
    categoriaId: [null, [Validators.required]]
  });

  ngOnInit(): void {
    this.loadCategorias();
  }

  private populateForm(producto: Producto): void {
    this.productoForm.patchValue({
      nombre: producto.nombre,
      sku: producto.sku,
      descripcion: producto.descripcion,
      precioCompra: producto.precioCompra,
      precioVenta: producto.precioVenta,
      stock: producto.stock,
      categoriaId: producto.categoriaId ?? null
    });
  }

  private resetForm(): void {
    this.productoForm.reset({
      nombre: '',
      sku: '',
      descripcion: '',
      precioCompra: 0,
      precioVenta: 0,
      stock: 0,
      categoriaId: null
    });
  }

  private loadCategorias(): void {
    this.categoriaService
      .getCategorias()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (categorias) => {
          this.categorias = categorias;
        },
        error: (error) => {
          console.error('Error al cargar las categorías', error);
          this.notificationService.error(
            'Error', 'No se pudieron cargar las categorías'
          );
        },
      });
  }

  isFieldInvalid(fieldName:string):boolean{
    const field = this.productoForm.get(fieldName);
    return !!(field && field.invalid && field.touched);
  }

  getFieldError(fieldName: string, errorType: string): boolean {
    const field = this.productoForm.get(fieldName);
    return !!(field && field.hasError(errorType) && field.touched);
  }

  onSubmit(): void {
    if (this.productoForm.invalid || this.isLoading) {
      this.productoForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    const formValue = this.productoForm.value;

    const productoData = {
      sku: formValue.sku,
      nombre: formValue.nombre,
      descripcion: formValue.descripcion,
      precioCompra: formValue.precioCompra,
      precioVenta: formValue.precioVenta,
      stock: formValue.stock,
      categoriaId: Number(formValue.categoriaId)
    };

    const obs = this.isEditMode && this._initialProduct?.id
      ? this.productoService.updateProducto(this._initialProduct.id, productoData)
      : this.productoService.createProducto(productoData);

    obs
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isLoading = false;
        })
      )
      .subscribe({
        next: (result) => {
          this.save.emit(result);
          this.closeModal();
        },
        error: (error) => {
          console.error('Error al guardar el producto', error);
          this.notificationService.error(
            'Error', 'No se pudo guardar el producto'
          );
        },
      });
  }

  onCategoryChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    const categoriaId = target.value;
    this.productoForm.patchValue({ categoriaId });
  }

  closeModal(): void {
    this.close.emit();
    this.resetForm();
    this.isLoading = false;
  }

  get nombreValue(): string {
    return this.productoForm.get('nombre')?.value || '';
  }

  get precioCompraValue(): number {
    return this.productoForm.get('precioCompra')?.value || 0;
  }

  get precioVentaValue(): number {
    return this.productoForm.get('precioVenta')?.value || 0;
  }

  get stockValue(): number {
    return this.productoForm.get('stock')?.value || 0;
  }

  get margenGanancia(): number {
    return this.precioVentaValue - this.precioCompraValue;
  }

  get porcentajeMargen(): number {
    if (this.precioCompraValue === 0) return 0;
    return (this.margenGanancia / this.precioCompraValue) * 100;
  }

  get shouldShowMargen(): boolean {
    return this.precioCompraValue > 0 && this.precioVentaValue > 0;
  }
}
