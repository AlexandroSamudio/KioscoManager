import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { environment } from '../environments/environment.development';
import { CompraCreate } from '../_models/compra-create.model';
import { Compra } from '../_models/compra.model';
import { PaginatedResult, setPaginationHeaders } from './pagination.helper';
import { NotificationService } from './notification.service';

@Injectable({
  providedIn: 'root'
})
export class CompraService {
  private http = inject(HttpClient);
  private notificationService = inject(NotificationService);
  private baseUrl = environment.apiUrl + 'compras';

  private handleError<T>(message: string) {
      return (error: any) => {
        this.notificationService.error(message, error?.message ?? 'Inténtelo de nuevo');
        return throwError(() => error);
      };
  }

  getCompras(
    pageNumber: number = 1,
    pageSize: number = 10,
    fechaDesde?: Date,
    fechaHasta?: Date,
    sortColumn?: string,
    sortDirection?: string
  ): Observable<PaginatedResult<Compra[]>> {
    let params = setPaginationHeaders(pageNumber, pageSize);

    if (fechaDesde) {
      params = params.append('fechaDesde', fechaDesde.toISOString());
    }

    if (fechaHasta) {
      params = params.append('fechaHasta', fechaHasta.toISOString());
    }

    if (sortColumn) {
      params = params.append('sortColumn', sortColumn);
    }

    if (sortDirection) {
      params = params.append('sortDirection', sortDirection);
    }

    return this.http.get<Compra[]>(this.baseUrl, {
      observe: 'response',
      params
    }).pipe(
      map((response: any) => {
        const result: PaginatedResult<Compra[]> = {
          items: response.body as Compra[],
          pagination: JSON.parse(response.headers.get('Pagination') || '{}')
        };
        return result;
      }),
      catchError(this.handleError<PaginatedResult<Compra[]>>('Error al obtener compras'))
    );
  }

  getCompra(id: number): Observable<Compra> {
    return this.http.get<Compra>(`${this.baseUrl}/${id}`)
      .pipe(
        catchError(this.handleError<Compra>('Error al obtener la compra'))
      );
  }

  getComprasRecientes(cantidad: number = 5): Observable<Compra[]> {
    return this.http.get<Compra[]>(`${this.baseUrl}/recientes?cantidad=${cantidad}`)
      .pipe(
        catchError(this.handleError<Compra[]>('Error al obtener compras recientes'))
      );
  }

  getTotalComprasPeriodo(fechaDesde?: Date, fechaHasta?: Date): Observable<number> {
    let params = new HttpParams();

    if (fechaDesde) {
      params = params.append('fechaDesde', fechaDesde.toISOString());
    }

    if (fechaHasta) {
      params = params.append('fechaHasta', fechaHasta.toISOString());
    }

    return this.http.get<number>(`${this.baseUrl}/total`, { params })
      .pipe(
        catchError(this.handleError<number>('Error al obtener el total de compras del período'))
      );
  }

  createCompra(compra: CompraCreate): Observable<Compra> {
    return this.http.post<Compra>(this.baseUrl, compra)
      .pipe(
        catchError(this.handleError<Compra>('Error al crear la compra'))
      );
  }
}
