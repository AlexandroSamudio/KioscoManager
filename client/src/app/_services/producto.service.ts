import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../environments/environment.development';
import { Producto } from '../_models/producto.model';
import { catchError, Observable, throwError } from 'rxjs';
import { ProductoMasVendido } from '../_models/producto-mas-vendido.model';
import { NotificationService } from './notification.service';
import { setPaginatedResponse, setPaginationHeaders, PaginatedResult } from './pagination.helper';

@Injectable({
  providedIn: 'root',
})
export class ProductoService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;
  private notificationService = inject(NotificationService);

  productosPaginados = signal<PaginatedResult<Producto[]> | null>(null);

  private handleError<T>(message: string) {
    return (error: any) => {
      this.notificationService.error(message, error?.message ?? 'Inténtelo de nuevo');
      return throwError(() => error);
    };
  }

  getProductos(pageNumber: number = 1, pageSize: number = 10, categoriaId?: number, stockStatus?: string, searchTerm?: string) {
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
    return this.http
      .get<Producto[]>(`${this.baseUrl}productos`, { params, observe: 'response' })
      .pipe(
        catchError(this.handleError<Producto[]>('Error al obtener los productos'))
      );
  }

  getProducto(id: number): Observable<Producto> {
    return this.http.get<Producto>(`${this.baseUrl}productos/${id}`).pipe(
      catchError(this.handleError<Producto>(`Error al obtener el producto con ID ${id}`))
    );
  }

  getProductosByLowestStock(): Observable<Producto[]> {
    return this.http.get<Producto[]>(`${this.baseUrl}productos/low-stock`).pipe(
      catchError(this.handleError<Producto[]>('Error al obtener los productos con bajo stock'))
    );
  }

  getProductosMasVendidosDelDia(): Observable<ProductoMasVendido[]> {
    return this.http
      .get<ProductoMasVendido[]>(`${this.baseUrl}ventas/productos-mas-vendidos`)
      .pipe(
        catchError(this.handleError<ProductoMasVendido[]>('Error al obtener los productos más vendidos del día'))
      );
  }
}
