import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, catchError, throwError, map } from 'rxjs';
import { environment } from '../environments/environment.development';
import { UserManagement, UserRoleAssignment, UserRoleResponse, PasswordChangeRequest, PasswordChangeResponse, ProfileUpdateRequest } from '../_models/user.model';
import { PaginatedResult, setPaginatedResponse } from './pagination.helper';



@Injectable({
  providedIn: 'root'
})
export class UserService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl + 'users';

  private handleError(operation: string, errorMessage?: string) {
    return (error: HttpErrorResponse) => {
      console.error(`${operation} failed:`, error);

      const message = errorMessage ||
        (error.error?.message || 'Ha ocurrido un error. Por favor, inténtalo de nuevo.');

      return throwError(() => ({
        error,
        message
      }));
    };
  }

  getUsers(): Observable<UserManagement[]> {
    return this.http.get<UserManagement[]>(this.baseUrl)
      .pipe(
        catchError(this.handleError('Obtener usuarios', 'No se pudieron cargar los usuarios.'))
      );
  }

  getUsersPaginated(pageNumber: number, pageSize: number): Observable<PaginatedResult<UserManagement[]>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<UserManagement[]>(this.baseUrl, { observe: 'response', params })
      .pipe(
        map(response => {
          return setPaginatedResponse<UserManagement[]>(response);
        }),
        catchError(this.handleError('Obtener usuarios paginados', 'No se pudieron cargar los usuarios.'))
      );
  }

  getUser(id: number): Observable<UserManagement> {
    return this.http.get<UserManagement>(`${this.baseUrl}/${id}`)
      .pipe(
        catchError(this.handleError('Obtener usuario', `No se pudo cargar la información del usuario ID ${id}.`))
      );
  }

  getUsersByKiosco(kioscoId: number): Observable<UserManagement[]> {
    return this.http.get<UserManagement[]>(`${this.baseUrl}/kiosco/${kioscoId}`)
      .pipe(
        catchError(this.handleError('Obtener usuarios por kiosco', 'No se pudieron cargar los usuarios de este kiosco.'))
      );
  }

  getUsersByKioscoPaginated(kioscoId: number, pageNumber: number, pageSize: number): Observable<PaginatedResult<UserManagement[]>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<UserManagement[]>(`${this.baseUrl}/kiosco/${kioscoId}`, { observe: 'response', params })
      .pipe(
        map(response => {
          return setPaginatedResponse<UserManagement[]>(response);
        }),
        catchError(this.handleError('Obtener usuarios del kiosco paginados', 'No se pudieron cargar los usuarios de este kiosco.'))
      );
  }

  assignRole(userId: number, roleAssignment: UserRoleAssignment): Observable<UserRoleResponse> {
    return this.http.post<UserRoleResponse>(`${this.baseUrl}/${userId}/role`, roleAssignment)
      .pipe(
        catchError((error: HttpErrorResponse) => {
          let errorMessage = 'Error al asignar el rol.';

          if (error.status === 403) {
            errorMessage = 'No tienes permisos para asignar roles a usuarios.';
          } else if (error.status === 400) {
            errorMessage = error.error?.message || 'El rol seleccionado no es válido.';
          } else if (error.status === 404) {
            errorMessage = 'Usuario no encontrado. Refresca la página e inténtalo de nuevo.';
          } else if (error.status === 409) {
            errorMessage = 'El usuario ya tiene asignado este rol.';
          } else if (error.status === 0) {
            errorMessage = 'Error de conexión. Compruebe su conexión e inténtelo de nuevo.';
          }

          return this.handleError('Asignar rol', errorMessage)(error);
        })
      );
  }

  getUserRoles(userId: number): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/${userId}/roles`)
      .pipe(
        catchError(this.handleError('Obtener roles de usuario', 'No se pudieron cargar los roles del usuario.'))
      );
  }

  isUserAdmin(userId: number): Observable<boolean> {
    return this.http.get<boolean>(`${this.baseUrl}/${userId}/is-admin`)
      .pipe(
        catchError(this.handleError('Verificar permisos de administrador', 'No se pudo verificar si el usuario es administrador.'))
      );
  }

  updateProfile(userId: number, profileData: ProfileUpdateRequest): Observable<UserManagement> {
    return this.http.put<UserManagement>(`${this.baseUrl}/${userId}/perfil`, profileData)
      .pipe(
        catchError(this.handleError('Actualizar perfil', 'No se pudo actualizar la información del perfil.'))
      );
  }

  changePassword(userId: number, passwordData: PasswordChangeRequest): Observable<PasswordChangeResponse> {
    return this.http.put<PasswordChangeResponse>(`${this.baseUrl}/${userId}/password`, passwordData)
      .pipe(
        catchError(this.handleError('Cambiar contraseña', 'No se pudo cambiar la contraseña. Verifica que los datos sean correctos.'))
      );
  }
}
