import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable, signal, WritableSignal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Producto } from '../_models/producto.model';
import { catchError, Observable, throwError } from 'rxjs';
import { ProductoMasVendido } from '../_models/producto-mas-vendido.model';
import { NotificationService } from './notification.service';
import { setPaginationHeaders, PaginatedResult } from './pagination.helper';

@Injectable({
  providedIn: 'root',
})
export class ProductoService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;
  private notificationService = inject(NotificationService);

  productosPaginados: WritableSignal<PaginatedResult<Producto[]> | null> =
    signal(null);

  private handleError<T>(message: string) {
    return (error: any) => {
      this.notificationService.error(
        message,
        error?.message ?? 'Inténtelo de nuevo'
      );
      return throwError(() => error);
    };
  }

  getProductos(
    pageNumber: number = 1,
    pageSize: number = 10,
    categoriaId?: number,
    stockStatus?: string,
    searchTerm?: string,
    sortColumn?: string,
    sortDirection?: 'asc' | 'desc'
  ) {
    let params = setPaginationHeaders(pageNumber, pageSize);
    if (categoriaId) {
      params = params.append('categoriaId', categoriaId.toString());
    }
    if (stockStatus) {
      params = params.append('stockStatus', stockStatus);
    }
    if (searchTerm && searchTerm.trim() !== '') {
      params = params.append('searchTerm', searchTerm.trim());
    }
    if (sortColumn) {
      params = params.append('sortColumn', sortColumn);
    }
    if (sortDirection) {
      params = params.append('sortDirection', sortDirection);
    }
    return this.http
      .get<Producto[]>(`${this.baseUrl}productos`, {
        params,
        observe: 'response',
      })
      .pipe(
        catchError(
          this.handleError<Producto[]>('Error al obtener los productos')
        )
      );
  }

  getProducto(id: number): Observable<Producto> {
    return this.http
      .get<Producto>(`${this.baseUrl}productos/${id}`)
      .pipe(
        catchError(
          this.handleError<Producto>(
            `Error al obtener el producto con ID ${id}`
          )
        )
      );
  }

  getProductosByLowestStock(): Observable<Producto[]> {
    return this.http
      .get<Producto[]>(`${this.baseUrl}productos/low-stock`)
      .pipe(
        catchError(
          this.handleError<Producto[]>(
            'Error al obtener los productos con bajo stock'
          )
        )
      );
  }

  getProductosMasVendidosDelDia(): Observable<ProductoMasVendido[]> {
    return this.http
      .get<ProductoMasVendido[]>(`${this.baseUrl}ventas/productos-mas-vendidos`)
      .pipe(
        catchError(
          this.handleError<ProductoMasVendido[]>(
            'Error al obtener los productos más vendidos del día'
          )
        )
      );
  }

  createProducto(producto: FormData): Observable<Producto> {
    return this.http
      .post<Producto>(`${this.baseUrl}productos`, producto)
      .pipe(
        catchError(this.handleError<Producto>('Error al crear el producto'))
      );
  }

  updateProducto(id: number, producto: FormData): Observable<Producto> {
    return this.http
      .put<Producto>(`${this.baseUrl}productos/${id}`, producto)
      .pipe(
        catchError(
          this.handleError<Producto>(
            `Error al actualizar el producto con ID ${id}`
          )
        )
      );
  }

  deleteProducto(id: number): Observable<object> {
    return this.http
      .delete(`${this.baseUrl}productos/${id}`)
      .pipe(
        catchError(
          this.handleError<object>(`Error al eliminar el producto con ID ${id}`)
        )
      );
  }

  getCapitalInvertido(): Observable<number> {
    return this.http
      .get<number>(`${this.baseUrl}productos/capital-invertido`)
      .pipe(
        catchError(
          this.handleError<number>('Error al obtener el capital invertido')
        )
      );
  }

  getTotalProductos(): Observable<number> {
    return this.http
      .get<number>(`${this.baseUrl}productos/total`)
      .pipe(
        catchError(
          this.handleError<number>('Error al obtener el total de productos')
        )
      );
  }

  getProductosParaExportar(limite?: number): Observable<Producto[]> {
    let params = new HttpParams();

    if (limite !== undefined && limite > 0) {
      params = params.append('limite', limite.toString());
    }

    return this.http
      .get<Producto[]>(`${this.baseUrl}productos/export`, { params })
      .pipe(
        catchError(
          this.handleError<Producto[]>(
            'Error al obtener productos para exportar'
          )
        )
      );
  }
}
