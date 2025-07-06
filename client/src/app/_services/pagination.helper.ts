import { HttpParams, HttpResponse } from '@angular/common/http';
import { WritableSignal } from '@angular/core';

export interface Pagination {
  currentPage: number;
  itemsPerPage: number;
  totalItems: number;
  totalPages: number;
}

export interface PaginatedResult<T> {
  result: T;
  pagination: Pagination;
}

export function setPaginatedResponse<T>(response: HttpResponse<T>): PaginatedResult<T> {
  const paginationHeader = response.headers.get('Pagination');
  const result: PaginatedResult<T> = {
    result: response.body as T,
    pagination: paginationHeader ? JSON.parse(paginationHeader) : null
  };
  return result;
}

export function setPaginatedResponseSignal<T>(
  response: HttpResponse<T>,
  paginatedResultSignal: WritableSignal<PaginatedResult<T> | null>
) {
  const paginatedResult = setPaginatedResponse(response);
  paginatedResultSignal.set(paginatedResult);
}

export function setPaginationHeaders(pageNumber: number, pageSize: number): HttpParams {
  let params = new HttpParams();
  if (pageNumber !== null && pageNumber !== undefined && pageSize !== null && pageSize !== undefined) {
    params = params.append('pageNumber', pageNumber.toString());
    params = params.append('pageSize', pageSize.toString());
  }
  return params;
}
