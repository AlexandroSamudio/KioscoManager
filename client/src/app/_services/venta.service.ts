import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../environments/environment.development';
import { catchError, Observable, throwError } from 'rxjs';
import { Venta } from '../_models/venta.model';
import { NotificationService } from './notification.service';

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
