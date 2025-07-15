import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { Categoria } from '../_models/categoria.model';
import { environment } from '../../environments/environment';
import {
  PaginatedResult,
  setPaginatedResponseSignal,
  setPaginationHeaders,
} from './pagination.helper';
import { catchError, map } from 'rxjs/operators';
import { NotificationService } from './notification.service';

@Injectable({
  providedIn: 'root',
})
export class CategoriaService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;
  private notificationService = inject(NotificationService);

  private handleError<T>(message: string) {
    return (error: any) => {
      this.notificationService.error(
        message,
        error.error ?? 'Inténtelo de nuevo'
      );
      return throwError(() => error);
    };
  }

  getCategorias(): Observable<Categoria[]> {
    return this.http
      .get<Categoria[]>(`${this.baseUrl}categorias`)
      .pipe(
        catchError(
          this.handleError<Categoria[]>('Error al cargar las categorías')
        )
      );
  }

  private paginatedCategorias = signal<PaginatedResult<Categoria[]> | null>(
    null
  );

  getCategoriasConPaginacion(
    page: number = 1,
    pageSize: number = 10
  ): Observable<PaginatedResult<Categoria[]>> {
    const params = setPaginationHeaders(page, pageSize);

    return this.http
      .get<Categoria[]>(`${this.baseUrl}categorias`, {
        observe: 'response',
        params,
      })
      .pipe(
        map((response) => {
          setPaginatedResponseSignal(response, this.paginatedCategorias);
          return this.paginatedCategorias() as PaginatedResult<Categoria[]>;
        })
      );
  }

  getCategoria(id: number): Observable<Categoria> {
    return this.http
      .get<Categoria>(`${this.baseUrl}categorias/${id}`)
      .pipe(
        catchError(
          this.handleError<Categoria>(
            `Error al cargar la categoría con ID ${id}`
          )
        )
      );
  }

  createCategoria(categoria: Partial<Categoria>): Observable<Categoria> {
    return this.http
      .post<Categoria>(`${this.baseUrl}categorias`, categoria)
      .pipe(
        catchError(this.handleError<Categoria>('Error al crear la categoría'))
      );
  }

  updateCategoria(
    id: number,
    categoria: Partial<Categoria>
  ): Observable<Categoria> {
    return this.http
      .put<Categoria>(`${this.baseUrl}categorias/${id}`, categoria)
      .pipe(
        catchError(
          this.handleError<Categoria>(
            `Error al actualizar la categoría`
          )
        )
      );
  }

  deleteCategoria(id: number): Observable<void> {
    return this.http
      .delete<void>(`${this.baseUrl}categorias/${id}`)
      .pipe(
        catchError(
          this.handleError<void>(`Error al eliminar la categoría con ID ${id}`)
        )
      );
  }
}
