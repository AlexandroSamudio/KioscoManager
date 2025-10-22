import { TestBed } from '@angular/core/testing';
import { adminGuard } from './admin.guard';
import { Router, UrlTree } from '@angular/router';
import { AccountService } from '../_services/account.service';
import { RoleService } from '../_services/role.service';
import { NotificationService } from '../_services/notification.service';


describe('adminGuard', () => {
  const routerStub = { createUrlTree: jest.fn((_: any, __?: any) => new UrlTree()) } as unknown as Router & { createUrlTree: jest.Mock };
  const notifySpy = ({ showError: jest.fn() } as unknown) as NotificationService & { showError: jest.Mock };

  beforeEach(() => {
    notifySpy.showError.mockReset();
  });

  it('redirects to login if not logged', () => {
    const accountStub = { isLoggedIn: jest.fn(() => false) } as unknown as AccountService;
    const roleStub = { isAdmin: jest.fn(() => false) } as unknown as RoleService;

    TestBed.configureTestingModule({ providers: [
      { provide: AccountService, useValue: accountStub },
      { provide: RoleService, useValue: roleStub },
      { provide: Router, useValue: routerStub },
      { provide: NotificationService, useValue: notifySpy },
    ]});

  routerStub.createUrlTree.mockClear();
  const res = TestBed.runInInjectionContext(() => adminGuard({} as any, { url: '/a' } as any));
  expect(res instanceof UrlTree).toBe(true);
    expect(routerStub.createUrlTree).toHaveBeenCalledWith(['/login'], { queryParams: { returnUrl: '/a' } });
    expect(notifySpy.showError).not.toHaveBeenCalled();
  });

  it('allows when admin', () => {
    const accountStub = { isLoggedIn: jest.fn(() => true) } as unknown as AccountService;
    const roleStub = { isAdmin: jest.fn(() => true) } as unknown as RoleService;

    TestBed.configureTestingModule({ providers: [
      { provide: AccountService, useValue: accountStub },
      { provide: RoleService, useValue: roleStub },
      { provide: Router, useValue: routerStub },
      { provide: NotificationService, useValue: notifySpy },
    ]});

  routerStub.createUrlTree.mockClear();
  const res = TestBed.runInInjectionContext(() => adminGuard({} as any, { url: '/a' } as any));
  expect(res).toBe(true);
    expect(routerStub.createUrlTree).not.toHaveBeenCalled();
    expect(notifySpy.showError).not.toHaveBeenCalled();
  });

  it('denies and notifies when not admin but logged', () => {
    const accountStub = { isLoggedIn: jest.fn(() => true) } as unknown as AccountService;
    const roleStub = { isAdmin: jest.fn(() => false) } as unknown as RoleService;

    TestBed.configureTestingModule({ providers: [
      { provide: AccountService, useValue: accountStub },
      { provide: RoleService, useValue: roleStub },
      { provide: Router, useValue: routerStub },
      { provide: NotificationService, useValue: notifySpy },
    ]});
  routerStub.createUrlTree.mockClear();
  const res = TestBed.runInInjectionContext(() => adminGuard({} as any, { url: '/a' } as any));
  expect(res instanceof UrlTree).toBe(true);
    expect(notifySpy.showError).toHaveBeenCalled();
    const call = (routerStub.createUrlTree as any).mock.calls[0];
    expect(call[0]).toEqual(['/dashboard']);
    expect(call[1]?.queryParams?.error).toBeTruthy();
  });

  it('handles logged-in users with undefined role as non-admin (redirect)', () => {
    const accountStub = { isLoggedIn: jest.fn(() => true) } as unknown as AccountService;
    const roleStub = { isAdmin: jest.fn(() => undefined as unknown as boolean) } as unknown as RoleService;

    TestBed.configureTestingModule({ providers: [
      { provide: AccountService, useValue: accountStub },
      { provide: RoleService, useValue: roleStub },
      { provide: Router, useValue: routerStub },
      { provide: NotificationService, useValue: notifySpy },
    ]});

    routerStub.createUrlTree.mockClear();
    const res = TestBed.runInInjectionContext(() => adminGuard({} as any, { url: '/a' } as any));
  expect(res instanceof UrlTree).toBe(true);
  const call = (routerStub.createUrlTree as any).mock.calls[0];
  expect(call[0]).toEqual(['/dashboard']);
  });
});
