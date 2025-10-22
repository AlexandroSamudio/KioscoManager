import { HttpHandlerFn, HttpHeaders, HttpRequest, HttpResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { jwtInterceptor } from './jwt.interceptor';
import { AccountService } from '../_services/account.service';
import { firstValueFrom, of } from 'rxjs';


describe('jwtInterceptor', () => {
  it('adds Authorization header when token present and not already set', async () => {
    const accountStub = {
      currentUser: () => ({ token: 'abc' })
    } as any as AccountService;

    TestBed.configureTestingModule({ providers: [
      { provide: AccountService, useValue: accountStub },
    ]});

    const req = new HttpRequest('GET', '/test');
    let seen: string | null = null;
    let forwardedReq: HttpRequest<any> | null = null;
    const next: HttpHandlerFn = (r) => {
      forwardedReq = r;
      seen = r.headers.get('Authorization');
      return of(new HttpResponse({ status: 200 }));
    };

    await firstValueFrom(TestBed.runInInjectionContext(() => jwtInterceptor(req, next)));
    expect(seen as any).toBe('Bearer abc');
    expect(req.headers.get('Authorization')).toBeNull();
    expect(forwardedReq!).not.toBe(req);
  });

  it('does not override existing Authorization header', async () => {
    const accountStub = {
      currentUser: () => ({ token: 'abc' })
    } as any as AccountService;

    TestBed.configureTestingModule({ providers: [
      { provide: AccountService, useValue: accountStub },
    ]});

    const req = new HttpRequest('GET', '/test', { headers: new HttpHeaders({ Authorization: 'Bearer existing' }) });
    let seen: string | null = null;
    let forwardedReq: HttpRequest<any> | null = null;
    const next: HttpHandlerFn = (r) => {
      forwardedReq = r;
      seen = r.headers.get('Authorization');
      return of(new HttpResponse({ status: 200 }));
    };

    await firstValueFrom(TestBed.runInInjectionContext(() => jwtInterceptor(req, next)));
    expect(seen as any).toBe('Bearer existing');
    expect(forwardedReq!).toBe(req);
  });

  it('does nothing when no token in current user', async () => {
    const accountStub = {
      currentUser: () => ({ token: undefined })
    } as any as AccountService;

    TestBed.configureTestingModule({ providers: [
      { provide: AccountService, useValue: accountStub },
    ]});

    const req = new HttpRequest('GET', '/test');
    let seen: string | null = null;
    let forwardedReq: HttpRequest<any> | null = null;
    const next: HttpHandlerFn = (r) => {
      forwardedReq = r;
      seen = r.headers.get('Authorization');
      return of(new HttpResponse({ status: 200 }));
    };

    await firstValueFrom(TestBed.runInInjectionContext(() => jwtInterceptor(req, next)));
    expect(seen).toBeNull();
    expect(forwardedReq!).toBe(req);
  });
});
