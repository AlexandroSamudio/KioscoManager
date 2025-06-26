import {
  HttpClient,
  HttpParams,
  HttpErrorResponse,
  HttpResponse,
} from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { environment } from '../../app/environments/environment.development';
import { Reporte } from '../_models/reporte.model';
import { ProductoMasVendido } from '../_models/producto-mas-vendido.model';
import { Observable, catchError, retry, throwError } from 'rxjs';
import { VentaPorDia } from '../_models/venta-por-dia.model';
import { CategoriaRentabilidad } from '../_models/categoria-rentabilidad.model';
import { setPaginationHeaders, PaginatedResult } from './pagination.helper';

@Injectable({
  providedIn: 'root',
})
export class ReporteService {
  private baseUrl = environment.apiUrl;
  private http = inject(HttpClient);
  productosPaginados = signal<PaginatedResult<ProductoMasVendido[]> | null>(
    null
  );

  getReporteSummary(fechaInicio?: Date, fechaFin?: Date): Observable<Reporte> {
    let params = new HttpParams();

    if (fechaInicio) {
      params = params.append('fechaInicio', fechaInicio.toISOString());
    }

    if (fechaFin) {
      params = params.append('fechaFin', fechaFin.toISOString());
    }

    return this.http
      .get<Reporte>(`${this.baseUrl}reportes`, { params })
      .pipe(retry(2), catchError(this.handleError));
  }

  getTopProductos(
    limit: number = 10,
    pageNumber: number = 1,
    pageSize: number = 10,
    fechaInicio?: Date,
    fechaFin?: Date
  ): Observable<HttpResponse<ProductoMasVendido[]>>  {
    let params = setPaginationHeaders(pageNumber, pageSize);

    if (limit) {
      params = params.append('limit', limit.toString());
    }

    if (fechaInicio) {
      params = params.append('fechaInicio', fechaInicio.toISOString());
    }

    if (fechaFin) {
      params = params.append('fechaFin', fechaFin.toISOString());
    }

    return this.http
      .get<ProductoMasVendido[]>(`${this.baseUrl}reportes/top-productos`, {
        params,
        observe: 'response',
      })
      .pipe(retry(2), catchError(this.handleError));
  }

  getVentasPorDia(fechaInicio?: Date, fechaFin?: Date): Observable<VentaPorDia[]> {
    let params = new HttpParams();

    if (fechaInicio) {
      params = params.append('fechaInicio', fechaInicio.toISOString());
    }

    if (fechaFin) {
      params = params.append('fechaFin', fechaFin.toISOString());
    }

    return this.http
      .get<VentaPorDia[]>(`${this.baseUrl}reportes/ventas-por-dia`, { params })
      .pipe(retry(2), catchError(this.handleError));
  }

  getCategoriasRentabilidad(
    fechaInicio?: Date,
    fechaFin?: Date
  ): Observable<CategoriaRentabilidad[]> {
    let params = new HttpParams();

    if (fechaInicio) {
      params = params.append('fechaInicio', fechaInicio.toISOString());
    }

    if (fechaFin) {
      params = params.append('fechaFin', fechaFin.toISOString());
    }

    return this.http
      .get<CategoriaRentabilidad[]>(
        `${this.baseUrl}reportes/rentabilidad-categorias`,
        { params }
      )
      .pipe(retry(2), catchError(this.handleError));
  }

  private handleError(error: HttpErrorResponse) {
    let errorMessage = 'Error desconocido';
    if (error.error instanceof ErrorEvent) {
      errorMessage = `Error: ${error.error.message}`;
    } else {
      errorMessage = `Código de error ${error.status}: ${error.message}`;
    }
    console.error(errorMessage);
    return throwError(() => errorMessage);
  }
}
