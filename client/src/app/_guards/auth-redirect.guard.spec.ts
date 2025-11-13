import { TestBed } from '@angular/core/testing';
import { authRedirectGuard } from './auth-redirect.guard';
import { Router, UrlTree } from '@angular/router';
import { AccountService } from '../_services/account.service';


describe('authRedirectGuard', () => {
  const routerStub = { createUrlTree: jest.fn((_: any, __?: any) => new UrlTree()) } as unknown as Router & { createUrlTree: jest.Mock };

  it('redirects home when not logged', () => {
    const accountStub = { isLoggedIn: jest.fn(() => false), kioscoId: jest.fn(() => null) } as any as AccountService;
    TestBed.configureTestingModule({ providers: [
      { provide: AccountService, useValue: accountStub },
      { provide: Router, useValue: routerStub },
    ]});


    routerStub.createUrlTree.mockClear();
    const res = TestBed.runInInjectionContext(() => authRedirectGuard({} as any, { url: '/a' } as any));
    expect(res instanceof UrlTree).toBe(true);
    expect(routerStub.createUrlTree).toHaveBeenCalledWith(['/home']);
  });

  it('redirects dashboard when logged and has kioscoId', () => {
    const accountStub = { isLoggedIn: jest.fn(() => true), kioscoId: jest.fn(() => '1') } as any as AccountService;

    TestBed.configureTestingModule({ providers: [
      { provide: AccountService, useValue: accountStub },
      { provide: Router, useValue: routerStub },
    ]});

    routerStub.createUrlTree.mockClear();
    const res = TestBed.runInInjectionContext(() => authRedirectGuard({} as any, { url: '/a' } as any));
    expect(res instanceof UrlTree).toBe(true);
    expect(routerStub.createUrlTree).toHaveBeenCalledWith(['/dashboard']);
  });

  it('redirects bienvenida when logged but kioscoId is missing', () => {
    const accountStub = { isLoggedIn: jest.fn(() => true), kioscoId: jest.fn(() => null) } as any as AccountService;

    TestBed.configureTestingModule({ providers: [
      { provide: AccountService, useValue: accountStub },
      { provide: Router, useValue: routerStub },
    ]});

    routerStub.createUrlTree.mockClear();
    const res = TestBed.runInInjectionContext(() => authRedirectGuard({} as any, { url: '/a' } as any));
    expect(res instanceof UrlTree).toBe(true);
    expect(routerStub.createUrlTree).toHaveBeenCalledWith(['/bienvenida']);
  });
});
