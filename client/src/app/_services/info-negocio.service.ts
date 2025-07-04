import { DestroyRef, Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, catchError, map, tap, throwError } from 'rxjs';
import { ConfiguracionService } from './configuracion.service';
import { NotificationService } from './notification.service';
import { KioscoBasicInfo } from '../_models/configuracion.model';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Injectable({
  providedIn: 'root',
})
export class InfoNegocioService {
  private configuracionService = inject(ConfiguracionService);
  private notificationService = inject(NotificationService);
  private destroyRef = inject(DestroyRef);

  private loadingSubject = new BehaviorSubject<boolean>(false);
  public loading$ = this.loadingSubject.asObservable();

  private handleError<T>(message: string) {
      return (error: any) => {
        this.notificationService.error(
          message,
          error?.message ?? 'Inténtelo de nuevo'
        );
        return throwError(() => error);
      };
    }

  saveBusinessInfo(businessInfo: KioscoBasicInfo): Observable<KioscoBasicInfo> {
    this.loadingSubject.next(true);

    return this.configuracionService.updateKioscoBasicInfo(businessInfo).pipe(
      takeUntilDestroyed(this.destroyRef),
      map((_) => {
        this.notificationService.success(
          'Información guardada',
          'La información básica del negocio se ha actualizado correctamente'
        );
        return businessInfo;
      }),
      catchError(this.handleError<KioscoBasicInfo>('Error al guardar la información del negocio')),
      tap({
        finalize: () => this.loadingSubject.next(false),
      })
    );
  }

  getBusinessInfo(): Observable<KioscoBasicInfo> {
    this.loadingSubject.next(true);

    return this.configuracionService.getKioscoBasicInfo().pipe(
      takeUntilDestroyed(this.destroyRef),
      catchError(this.handleError<KioscoBasicInfo>('Error al cargar la información del negocio')),
      tap({
        finalize: () => this.loadingSubject.next(false),
      })
    );
  }

  isLoading(): boolean {
    return this.loadingSubject.value;
  }
}
