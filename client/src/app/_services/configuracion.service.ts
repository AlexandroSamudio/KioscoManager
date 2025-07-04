import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import {
  KioscoConfig,
  UserPreferences,
  KioscoBasicInfo,
} from '../_models/configuracion.model';
import { environment } from '../environments/environment.development';
import { NotificationService } from './notification.service';

@Injectable({
  providedIn: 'root',
})
export class ConfiguracionService {
  private baseUrl = environment.apiUrl;
  private notificationService = inject(NotificationService);
  private http = inject(HttpClient);

  private handleError<T>(message: string) {
    return (error: any) => {
      this.notificationService.error(
        message,
        error?.message ?? 'Inténtelo de nuevo'
      );
      return throwError(() => error);
    };
  }

  getKioscoConfig(): Observable<KioscoConfig> {
    return this.http.get<KioscoConfig>(`${this.baseUrl}config/kiosco`);
  }

  updateKioscoConfig(config: KioscoConfig): Observable<KioscoConfig> {
    return this.http.put<KioscoConfig>(`${this.baseUrl}config/kiosco`, config);
  }

  updateKioscoBasicInfo(
    basicInfo: KioscoBasicInfo
  ): Observable<KioscoBasicInfo> {
    return this.http
      .put<KioscoBasicInfo>(
        `${this.baseUrl}config/kiosko/info-basico`,
        basicInfo
      )
      .pipe(
        catchError(
          this.handleError<KioscoBasicInfo>(
            `Error al actualizar la información básica del kiosco`
          )
        )
      );
  }

  getKioscoBasicInfo(): Observable<KioscoBasicInfo> {
    return this.http
      .get<KioscoBasicInfo>(`${this.baseUrl}config/kiosko/info-basico`)
      .pipe(
        catchError(
          this.handleError<KioscoBasicInfo>(
            `Error al cargar la información básica del kiosco`
          )
        )
      );
  }

  getUserPreferences(): Observable<UserPreferences> {
    return this.http.get<UserPreferences>(
      `${this.baseUrl}config/user/preferences`
    );
  }

  updateUserPreferences(
    preferences: UserPreferences
  ): Observable<UserPreferences> {
    return this.http.put<UserPreferences>(
      `${this.baseUrl}config/user/preferences`,
      preferences
    );
  }
}
