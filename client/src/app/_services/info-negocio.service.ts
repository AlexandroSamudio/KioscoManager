import { DestroyRef, Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, catchError, map, tap } from 'rxjs';
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
      catchError((error) => {
        const errorMessage =
          error instanceof Error ? error.message : 'Error desconocido';

        this.notificationService.error('Error al guardar', errorMessage);

        throw error;
      }),
      tap({
        finalize: () => this.loadingSubject.next(false),
      })
    );
  }

  getBusinessInfo(): Observable<KioscoBasicInfo> {
    this.loadingSubject.next(true);

    return this.configuracionService.getKioscoBasicInfo().pipe(
      takeUntilDestroyed(this.destroyRef),
      catchError((error) => {
        const errorMessage =
          error instanceof Error ? error.message : 'Error desconocido';

        this.notificationService.error(
          'Error al cargar',
          `No se pudo cargar la información del negocio: ${errorMessage}`
        );

        throw error;
      }),
      tap({
        finalize: () => this.loadingSubject.next(false),
      })
    );
  }

  isLoading(): boolean {
    return this.loadingSubject.value;
  }
}
