import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Reporte } from '../_models/reporte.model';
import { ProductoMasVendido } from '../_models/producto-mas-vendido.model';
import { Observable, catchError, throwError, map } from 'rxjs';
import {
  VentaPorDia,
  VentaPorDiaResponse,
} from '../_models/venta-por-dia.model';
import { CategoriaRentabilidad } from '../_models/categoria-rentabilidad.model';
import { VentaChart } from '../_models/venta-chart.model';
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
      this.notificationService.error(
        message,
        error?.message ?? 'Inténtelo de nuevo'
      );
      console.log(error);
      return throwError(() => error);
    };
  }

  getReporteSummary(
    fechaInicio?: string,
    fechaFin?: string
  ): Observable<Reporte> {
    let params = new HttpParams();

    if (fechaInicio) {
      params = params.append('fechaInicio', fechaInicio);
    }

    if (fechaFin) {
      params = params.append('fechaFin', fechaFin);
    }

    return this.http
      .get<Reporte>(`${this.baseUrl}reportes`, { params })
      .pipe(
        catchError(
          this.handleError<Reporte>('Error al obtener resumen de reporte')
        )
      );
  }

  getTopProductos(
    limit: number = 10,
    pageNumber: number = 1,
    pageSize: number = 10,
    fechaInicio?: string,
    fechaFin?: string
  ): Observable<HttpResponse<ProductoMasVendido[]>> {
    let params = setPaginationHeaders(pageNumber, pageSize);

    if (limit) {
      params = params.append('limit', limit.toString());
    }

    if (fechaInicio) {
      params = params.append('fechaInicio', fechaInicio);
    }

    if (fechaFin) {
      params = params.append('fechaFin', fechaFin);
    }


    return this.http
      .get<ProductoMasVendido[]>(`${this.baseUrl}reportes/top-productos`, {
        params,
        observe: 'response',
      })
      .pipe(
        catchError(
          this.handleError<ProductoMasVendido[]>(
            'Error al obtener productos más vendidos'
          )
        )
      );
  }

  getVentasPorDia(
    fechaInicio?: string,
    fechaFin?: string
  ): Observable<VentaPorDia[]> {
    let params = new HttpParams();

    if (fechaInicio) {
      params = params.append('fechaInicio', fechaInicio);
    }

    if (fechaFin) {
      params = params.append('fechaFin', fechaFin);
    }

    return this.http
      .get<VentaPorDiaResponse[]>(`${this.baseUrl}reportes/ventas-por-dia`, {
        params,
      })
      .pipe(
        map((response: VentaPorDiaResponse[]) =>
          response.map((item) => ({
            fecha: new Date(item.fecha),
            totalVentas: item.totalVentas,
            tipoAgrupacion: item.tipoAgrupacion ?? 'daily',
          }))
        ),
        catchError(
          this.handleError<VentaPorDia[]>('Error al obtener ventas por día')
        )
      );
  }

  getCategoriasRentabilidad(
    fechaInicio?: string,
    fechaFin?: string
  ): Observable<CategoriaRentabilidad[]> {
    let params = new HttpParams();

    if (fechaInicio) {
      params = params.append('fechaInicio', fechaInicio);
    }

    if (fechaFin) {
      params = params.append('fechaFin', fechaFin);
    }

    return this.http
      .get<CategoriaRentabilidad[]>(
        `${this.baseUrl}reportes/rentabilidad-categorias`,
        { params }
      )
      .pipe(
        catchError(
          this.handleError<CategoriaRentabilidad[]>(
            'Error al obtener categorías de rentabilidad'
          )
        )
      );
  }

  getVentasParaChart(fecha?: string): Observable<VentaChart[]> {
    let params = new HttpParams();

    if(fecha){
      params = params.append('fecha', fecha);
    }

    return this.http
      .get<VentaChart[]>(`${this.baseUrl}reportes/ventas-chart`, { params })
      .pipe(
        catchError(
          this.handleError<VentaChart[]>('Error al obtener ventas para gráfico')
        )
      );
  }
}
