import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { catchError, Observable, throwError } from 'rxjs';
import { Venta } from '../_models/venta.model';
import { NotificationService } from './notification.service';
import { Producto } from '../_models/producto.model';
import { VentaCreate } from '../_models/venta-create.model';

@Injectable({
  providedIn: 'root'
})
export class VentaService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;
  private notificationService = inject(NotificationService);

  private handleError<T>(message: string) {
    return (error: any) => {
      this.notificationService.error(message, error?.message ?? 'Inténtelo de nuevo');
      return throwError(() => error);
    };
  }

  getProductoBySku(sku: string): Observable<Producto> {
    return this.http.get<Producto>(`${this.baseUrl}productos/by-sku/${sku}`).pipe(
      catchError((error) => {
        return throwError(() => error);
      })
    );
  }

  finalizarVenta(venta: VentaCreate): Observable<Venta> {
    return this.http.post<Venta>(`${this.baseUrl}ventas/finalizar`, venta).pipe(
      catchError(this.handleError<Venta>('Error al finalizar la venta'))
    );
  }

  getTotalVentasDia(): Observable<number> {
    return this.http.get<number>(`${this.baseUrl}ventas/total-dia`).pipe(
      catchError(this.handleError<number>('Error al obtener el total de ventas del día'))
    );
  }

  getVentasRecientes(): Observable<Venta[]> {
    return this.http.get<Venta[]>(`${this.baseUrl}ventas/recientes`).pipe(
      catchError(this.handleError<Venta[]>('Error al obtener las ventas recientes'))
    );
  }
}
