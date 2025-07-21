import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { CompraCreate } from '../_models/compra-create.model';
import { Compra } from '../_models/compra.model';
import { NotificationService } from './notification.service';

@Injectable({
  providedIn: 'root',
})
export class CompraService {
  private http = inject(HttpClient);
  private notificationService = inject(NotificationService);
  private baseUrl = environment.apiUrl + 'compras';

  private handleError<T>(message: string) {
    return (error: any) => {
      this.notificationService.error(
        message,
        error?.message ?? 'Inténtelo de nuevo'
      );
      return throwError(() => error);
    };
  }

  createCompra(compra: CompraCreate): Observable<Compra> {
    return this.http
      .post<Compra>(this.baseUrl, compra)
      .pipe(catchError(this.handleError<Compra>('Error al crear la compra')));
  }

  getComprasParaExportar(
    fechaInicio?: string,
    fechaFin?: string,
    limite?: number
  ): Observable<Compra[]> {
    let params = new HttpParams();

    if (fechaInicio) {
      params = params.append('fechaInicio', fechaInicio);
    }

    if (fechaFin) {
      params = params.append('fechaFin', fechaFin);
    }

    if (limite !== undefined && limite > 0) {
      params = params.append('limite', limite.toString());
    }

    return this.http
      .get<Compra[]>(`${this.baseUrl}/export`, { params })
      .pipe(
        catchError(
          this.handleError<Compra[]>('Error al obtener compras para exportar')
        )
      );
  }
}
