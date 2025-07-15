import {
  HttpClient,
  HttpParams,
  HttpResponse,
} from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { Reporte } from '../_models/reporte.model';
import { ProductoMasVendido } from '../_models/producto-mas-vendido.model';
import { Observable, catchError, throwError } from 'rxjs';
import { VentaPorDia } from '../_models/venta-por-dia.model';
import { CategoriaRentabilidad } from '../_models/categoria-rentabilidad.model';
import { setPaginationHeaders, PaginatedResult } from './pagination.helper';
import { NotificationService } from './notification.service';

@Injectable({
  providedIn: 'root',
})
export class ReporteService {
  private baseUrl = environment.apiUrl;
  private http = inject(HttpClient);
  private notificationService = inject(NotificationService);
  productosPaginados = signal<PaginatedResult<ProductoMasVendido[]> | null>(
    null
  );

  private handleError<T>(message: string) {
    return (error: any) => {
      this.notificationService.error(message, error?.message ?? 'Inténtelo de nuevo');
      return throwError(() => error);
    };
  }


  getReporteSummary(fechaInicio?: Date, fechaFin?: Date): Observable<Reporte> {
    let params = new HttpParams();

    if (fechaInicio) {
      params = params.append('fechaInicio', this.formatDateToISO(fechaInicio));
    }

    if (fechaFin) {
      params = params.append('fechaFin', this.formatDateToISO(fechaFin));
    }

    return this.http
      .get<Reporte>(`${this.baseUrl}reportes`, { params })
      .pipe(catchError(this.handleError<Reporte>('Error al obtener resumen de reporte')));
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
      params = params.append('fechaInicio', this.formatDateToISO(fechaInicio));
    }

    if (fechaFin) {
      params = params.append('fechaFin', this.formatDateToISO(fechaFin));
    }

    return this.http
      .get<ProductoMasVendido[]>(`${this.baseUrl}reportes/top-productos`, {
        params,
        observe: 'response',
      })
      .pipe(catchError(this.handleError<ProductoMasVendido[]>('Error al obtener productos más vendidos')));
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
      .pipe(catchError(this.handleError<VentaPorDia[]>('Error al obtener ventas por día')));
  }

  getCategoriasRentabilidad(
    fechaInicio?: Date,
    fechaFin?: Date
  ): Observable<CategoriaRentabilidad[]> {
    let params = new HttpParams();

    if (fechaInicio) {
      params = params.append('fechaInicio', this.formatDateToISO(fechaInicio));
    }

    if (fechaFin) {
      params = params.append('fechaFin', this.formatDateToISO(fechaFin));
    }

    return this.http
      .get<CategoriaRentabilidad[]>(
        `${this.baseUrl}reportes/rentabilidad-categorias`,
        { params }
      )
      .pipe(catchError(this.handleError<CategoriaRentabilidad[]>('Error al obtener categorías de rentabilidad')));
  }

  private formatDateToISO(date: Date): string {
    const isoString = new Date(
      Date.UTC(
        date.getFullYear(),
        date.getMonth(),
        date.getDate(),
        date.getHours(),
        date.getMinutes(),
        date.getSeconds(),
        date.getMilliseconds()
      )
    ).toISOString();

    return isoString;
  }
}
