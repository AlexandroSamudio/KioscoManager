import { TestBed } from '@angular/core/testing';
import { authGuard } from './auth.guard';
import { Router, UrlTree } from '@angular/router';
import { AccountService } from '../_services/account.service';


describe('authGuard', () => {
  const routerStub = { createUrlTree: jest.fn((_: any, __?: any) => new UrlTree()) } as unknown as Router & { createUrlTree: jest.Mock };

  it('allows when logged in', () => {
    const accountStub = { isLoggedIn: jest.fn(() => true) } as Partial<AccountService> as AccountService;
    TestBed.configureTestingModule({ providers: [
      { provide: AccountService, useValue: accountStub },
      { provide: Router, useValue: routerStub },
    ]});

    const result = TestBed.runInInjectionContext(() => authGuard({} as any, { url: '/x' } as any));
    expect(result).toBe(true);
    expect(accountStub.isLoggedIn).toHaveBeenCalled();
    expect(routerStub.createUrlTree).not.toHaveBeenCalled();
  });

  it('redirects to login when not logged', () => {
    const accountStub = { isLoggedIn: jest.fn(() => false) } as Partial<AccountService> as AccountService;
    TestBed.configureTestingModule({ providers: [
      { provide: AccountService, useValue: accountStub },
      { provide: Router, useValue: routerStub },
    ]});

    routerStub.createUrlTree.mockClear();
    const result = TestBed.runInInjectionContext(() => authGuard({} as any, { url: '/x' } as any));
    expect(result instanceof UrlTree).toBe(true);
    expect(accountStub.isLoggedIn).toHaveBeenCalled();
    expect(routerStub.createUrlTree).toHaveBeenCalledWith(['/login'], { queryParams: { returnUrl: '/x' } });
  });
});
