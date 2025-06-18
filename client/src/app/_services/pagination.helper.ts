import { HttpParams, HttpResponse } from '@angular/common/http';
import { signal, WritableSignal } from '@angular/core';

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
  paginatedResultSignal: WritableSignal<PaginatedResult<T> | null>
) {
  const paginationHeader = response.headers.get('Pagination');
  paginatedResultSignal.set({
    items: response.body as T,
    pagination: paginationHeader ? JSON.parse(paginationHeader) : null
  });
}

export function setPaginationHeaders(pageNumber: number, pageSize: number): HttpParams {
  let params = new HttpParams();
  if (pageNumber !== null && pageNumber !== undefined && pageSize !== null && pageSize !== undefined) {
    params = params.append('pageNumber', pageNumber.toString());
    params = params.append('pageSize', pageSize.toString());
  }
  return params;
}
