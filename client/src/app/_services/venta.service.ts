import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../environments/environment.development';
import { catchError, Observable, of, throwError } from 'rxjs';
import { Venta } from '../_models/venta.model';
import { NotificationService } from './notification.service';

@Injectable({
  providedIn: 'root'
})
export class VentaService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;
  private notificationService = inject(NotificationService);

  getTotalVentasDia(): Observable<number> {
    return this.http.get<number>(`${this.baseUrl}ventas/total-dia`).pipe(
      catchError(error => {
        this.notificationService.error('Error al obtener el total de ventas del día', error);
        return throwError(() => error);
      })
    );
  }

  getVentasRecientes(): Observable<Venta[]> {
    return this.http.get<Venta[]>(`${this.baseUrl}ventas/recientes`).pipe(
      catchError(error => {
        this.notificationService.error('Error al obtener las ventas recientes', error);
        return throwError(() => error);
      })
    );
  }
}
