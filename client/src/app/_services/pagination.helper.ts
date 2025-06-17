import { HttpParams, HttpResponse } from '@angular/common/http';
import { signal } from '@angular/core';

export interface Pagination {
  currentPage: number;
  itemsPerPage: number;
  totalItems: number;
  totalPages: number;
}

export interface PaginatedResult<T> {
  items: T;
  pagination: Pagination;
}

export function setPaginatedResponse<T>(
  response: HttpResponse<T>,
  paginatedResultSignal: ReturnType<typeof signal<PaginatedResult<T> | null>>
) {
  paginatedResultSignal.set({
    items: response.body as T,
    pagination: JSON.parse(response.headers.get('Pagination')!)
  });
}

export function setPaginationHeaders(pageNumber: number, pageSize: number): HttpParams {
  let params = new HttpParams();
  if (pageNumber && pageSize) {
    params = params.append('pageNumber', pageNumber);
    params = params.append('pageSize', pageSize);
  }
  return params;
}
