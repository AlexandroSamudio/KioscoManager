import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Output, Input, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { inject, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ProductoService } from '../../_services/producto.service';
import { Producto } from '../../_models/producto.model';
import { CategoriaService } from '../../_services/categoria.service';
import { Categoria } from '../../_models/categoria.model';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-producto-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './producto-form.component.html',
  styleUrl: './producto-form.component.css'
})
export class ProductoFormComponent implements OnInit {
  private destroyRef = inject(DestroyRef);
  private productoService = inject(ProductoService);
  private categoriaService = inject(CategoriaService);
  @Input() isVisible = false;
  @Input() isEditMode = false;
  private _initialProduct: Producto | null = null;
  @Input() set initialProduct(value: Producto | null) {
    this._initialProduct = value;
    if (this.isEditMode && value) {
      this.producto = {
        ...value,
        categoriaId: this.parseCategoriaId(value.categoriaId)
      };
    } else {
      this.producto = this.getEmptyProduct();
    }
  }
  get initialProduct(): Producto | null {
    return this._initialProduct;
  }
  @Output() close = new EventEmitter<void>();
  @Output() save = new EventEmitter<Producto>();

  isLoading = false;
  producto: Producto = this.getEmptyProduct();
  categorias: Categoria[] = [];

  ngOnInit(): void {
    this.loadCategorias();
  }

  private loadCategorias(): void {
    this.categoriaService.getCategorias()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (categorias) => {
          this.categorias = categorias;
        },
        error: (error) => {
          console.error('Error al cargar las categorías', error);
        }
      });
  }

  private parseCategoriaId(value: unknown): number {
    const parsed = Number(value);
    return isNaN(parsed) ? 0 : parsed;
  }

  private getEmptyProduct(): Producto {
    return {
      id: 0,
      sku: '',
      nombre: '',
      descripcion: '',
      precioCompra: 0,
      precioVenta: 0,
      stock: 0,
      categoriaNombre: '',
      categoriaId: 0
    };
  }

  onSubmit(): void {
    if (!this.producto.categoriaId || this.producto.categoriaId === 0) {
      const categoria = this.categorias.find(c => c.nombre === this.producto.categoriaNombre);
      if (categoria) {
        this.producto.categoriaId = categoria.id;
      }
    }
    if (this.isLoading) return;
    this.isLoading = true;
    const { id, categoriaNombre, ...productoSinId } = this.producto;
    const obs = this.isEditMode && this.producto.id
      ? this.productoService.updateProducto(this.producto.id, productoSinId)
      : this.productoService.createProducto(productoSinId);
    obs
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => { this.isLoading = false; })
      )
      .subscribe({
        next: (result) => {
          this.save.emit(result);
          this.closeModal();
        },
        error: (error) => {
          console.error('Error al guardar el producto', error);
        }
      });
  }

  onCategoryChange(nombre: string): void {
    const cat = this.categorias.find(c => c.nombre === nombre);
    this.producto.categoriaId = cat ? cat.id : 0;
    this.producto.categoriaNombre = nombre;
  }

  closeModal(): void {
    this.close.emit();
    this.producto = this.getEmptyProduct();
    this.isLoading = false;
  }
}
