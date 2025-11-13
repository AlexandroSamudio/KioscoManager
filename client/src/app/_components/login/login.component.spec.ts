import { TestBed } from '@angular/core/testing';
import { Router, ActivatedRoute, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AccountService } from '../../_services/account.service';
import { LoginComponent } from './login.component';

describe('LoginComponent (shallow)', () => {
  let accountService: { login: jest.Mock; kioscoId: jest.Mock };
  let router: Router;

  beforeEach(() => {
    accountService = { login: jest.fn(), kioscoId: jest.fn().mockReturnValue(null) } as any;

    TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        { provide: AccountService, useValue: accountService },
        provideRouter([]),
        { provide: ActivatedRoute, useValue: { snapshot: { queryParams: {} } } },
      ],
    });

    router = TestBed.inject(Router);
    jest.spyOn(router, 'navigate').mockResolvedValue(true as any);
    jest.spyOn(router, 'navigateByUrl').mockResolvedValue(true as any);
  });

  it('should validate form controls', async () => {
    const fixture = TestBed.createComponent(LoginComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    component.loginForm.setValue({ email: 'bad', password: '123' });
    component.loginForm.get('email')?.markAsTouched();
    component.loginForm.get('password')?.markAsTouched();

    expect(component.getFieldError('email')).toBe('Ingresa un email válido');
    expect(component.getFieldError('password')).toBe('La contraseña debe tener al menos 6 caracteres');
  });

  it('should not submit when invalid and marks controls', async () => {
    const fixture = TestBed.createComponent(LoginComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    component.loginForm.patchValue({ email: '' });
    await component.onSubmit();

    expect(accountService.login).not.toHaveBeenCalled();
    expect(component.loginForm.get('email')?.touched).toBe(true);
  });

  it('should submit and navigate based on kioscoId', async () => {
    const fixture = TestBed.createComponent(LoginComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    component.loginForm.setValue({ email: 'a@a.com', password: '123456' });
    accountService.login.mockReturnValue(of({}));

    // First: no kioscoId -> /bienvenida
    accountService.kioscoId.mockReturnValue(null);
    await component.onSubmit();
    expect(router.navigate).toHaveBeenCalledWith(['/bienvenida']);

    // Second: kioscoId -> /dashboard
    accountService.kioscoId.mockReturnValue('k1');
    await component.onSubmit();
    expect(router.navigate).toHaveBeenCalledWith(['/dashboard']);
  });

  it('should navigate to returnUrl when present', async () => {
    const route = TestBed.inject(ActivatedRoute) as any;
    route.snapshot.queryParams = { returnUrl: '/back' };
    const fixture = TestBed.createComponent(LoginComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    component.loginForm.setValue({ email: 'a@a.com', password: '123456' });
    accountService.login.mockReturnValue(of({}));

    await component.onSubmit();
    expect(router.navigateByUrl).toHaveBeenCalledWith('/back');
  });

  it('should set errorMessage for HTTP errors', async () => {
    const fixture = TestBed.createComponent(LoginComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    component.loginForm.setValue({ email: 'a@a.com', password: '123456' });

    accountService.login.mockReturnValue(throwError(() => ({ status: 401 })));
    await component.onSubmit();
    expect(component.errorMessage()).toBe('Email o contraseña incorrectos');

    accountService.login.mockReturnValue(throwError(() => ({ status: 400, error: {} })));
    await component.onSubmit();
    expect(component.errorMessage()).toBe('Por favor, verifica tus datos');

    accountService.login.mockReturnValue(throwError(() => ({ status: 0 })));
    await component.onSubmit();
    expect(component.errorMessage()).toBe('Error de conexión. Verifica tu conexión a internet');

    accountService.login.mockReturnValue(throwError(() => ({ status: 500 })));
    await component.onSubmit();
    expect(component.errorMessage()).toBe('Ha ocurrido un error inesperado. Inténtalo de nuevo');
  });
});
