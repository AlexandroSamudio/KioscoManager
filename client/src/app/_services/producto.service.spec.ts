import { TestBed } from '@angular/core/testing';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ProductoService } from './producto.service';
import { environment } from '../../environments/environment';
import { NotificationService } from './notification.service';

describe('ProductoService (HTTP)', () => {
  let service: ProductoService;
  let httpMock: HttpTestingController;
  const baseUrl = environment.apiUrl;

  const notificationSpy = ({ error: jest.fn() } as unknown) as NotificationService & { error: jest.Mock };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ProductoService,
        { provide: NotificationService, useValue: notificationSpy },
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
      ],
    });

  service = TestBed.inject(ProductoService);
  httpMock = TestBed.inject(HttpTestingController);
  notificationSpy.error.mockReset();
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should GET productos with query params and pagination headers', () => {
    service.getProductos(2, 20, 5, 'in-stock', 'abc', 'nombre', 'asc').subscribe();

    const req = httpMock.expectOne(r => r.method === 'GET' && r.url === `${baseUrl}productos`);
    expect(req.request.params.get('pageNumber')).toBe('2');
    expect(req.request.params.get('pageSize')).toBe('20');
    expect(req.request.params.get('categoriaId')).toBe('5');
    expect(req.request.params.get('stockStatus')).toBe('in-stock');
    expect(req.request.params.get('searchTerm')).toBe('abc');
    expect(req.request.params.get('sortColumn')).toBe('nombre');
    expect(req.request.params.get('sortDirection')).toBe('asc');

    req.flush([], { headers: { 'Pagination': JSON.stringify({ currentPage: 2, totalPages: 1, totalItems: 0, itemsPerPage: 20 }) } as any });
    expect(notificationSpy.error).not.toHaveBeenCalled();
  });

  it('should handle error on getProducto and notify', () => {
    service.getProducto(10).subscribe({
      next: () => { throw new Error('should error'); },
      error: (err) => expect(err.status).toBe(404),
    });

    const req = httpMock.expectOne(`${baseUrl}productos/10`);
    expect(req.request.method).toBe('GET');

    req.flush({ message: 'not found' }, { status: 404, statusText: 'Not Found' });
    expect(notificationSpy.error).toHaveBeenCalled();
  });

  it('should POST createProducto', () => {
    const fd = new FormData();
    fd.append('nombre', 'Prod');
    service.createProducto(fd).subscribe();

    const req = httpMock.expectOne(`${baseUrl}productos`);
    expect(req.request.method).toBe('POST');
    req.flush({ id: 1 });
    expect(notificationSpy.error).not.toHaveBeenCalled();
  });

  it('should GET productos/export with limite param when provided', () => {
    service.getProductosParaExportar(100).subscribe();

    const req = httpMock.expectOne(r => r.url === `${baseUrl}productos/export`);
    expect(req.request.params.get('limite')).toBe('100');
    req.flush([]);
    expect(notificationSpy.error).not.toHaveBeenCalled();
  });

  it('should GET productos/export without limite when not provided', () => {
    service.getProductosParaExportar().subscribe();

    const req = httpMock.expectOne(r => r.url === `${baseUrl}productos/export`);
    expect(req.request.params.get('limite')).toBeNull();
    req.flush([]);
    expect(notificationSpy.error).not.toHaveBeenCalled();
  });
});
