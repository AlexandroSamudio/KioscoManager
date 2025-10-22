import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AccountService } from '../../_services/account.service';
import { RegisterComponent } from './register.component';

describe('RegisterComponent (shallow)', () => {
  let accountService: { register: jest.Mock };
  let router: Router;

  beforeEach(() => {
    jest.useFakeTimers();
    accountService = { register: jest.fn() } as any;

    TestBed.configureTestingModule({
      imports: [RegisterComponent],
      providers: [
        { provide: AccountService, useValue: accountService },
        provideRouter([]),
      ],
    });

    router = TestBed.inject(Router);
    jest.spyOn(router, 'navigate').mockResolvedValue(true as any);
  });

  it('should validate password strength and matching', () => {
    const fixture = TestBed.createComponent(RegisterComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    component.registerForm.setValue({
      username: 'ab', // minlength fail (3)
      email: 'bad',
      password: 'weak', // no strength
      confirmPassword: 'nope',
    });

    component.registerForm.get('username')?.markAsTouched();
    component.registerForm.get('email')?.markAsTouched();
    component.registerForm.get('password')?.markAsTouched();
    component.registerForm.get('confirmPassword')?.markAsTouched();

    expect(component.getFieldError('username')).toContain('al menos 3');
    expect(component.getFieldError('email')).toBe('Ingresa un email válido');
    expect(component.getFieldError('password')).toContain('contraseña debe contener');
    expect(component.getFieldError('confirmPassword')).toBe('Las contraseñas no coinciden');
  });

  it('should not submit when invalid and marks controls', async () => {
    const fixture = TestBed.createComponent(RegisterComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    component.registerForm.patchValue({ username: '' });
    await component.onSubmit();

    expect(accountService.register).not.toHaveBeenCalled();
    expect(component.registerForm.get('username')?.touched).toBe(true);
  });

  it('should submit and navigate on success', async () => {
    const fixture = TestBed.createComponent(RegisterComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    component.registerForm.setValue({
      username: 'user',
      email: 'a@a.com',
      password: 'Str0ng!Pass',
      confirmPassword: 'Str0ng!Pass',
    });

    accountService.register.mockReturnValue(of({ id: 1 }));

    await component.onSubmit();

    expect(component.successMessage()).toContain('Cuenta creada');

    jest.advanceTimersByTime(1500);
    expect(router.navigate).toHaveBeenCalledWith(['/bienvenida']);
  });

  it('should set error message from 400 with errors', async () => {
    const fixture = TestBed.createComponent(RegisterComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges();

    component.registerForm.setValue({
      username: 'user',
      email: 'a@a.com',
      password: 'Str0ng!Pass',
      confirmPassword: 'Str0ng!Pass',
    });

    accountService.register.mockReturnValue(throwError(() => ({ status: 400, error: { errors: { Email: ['Taken'] } } })));

    await component.onSubmit();
    expect(component.errorMessage()).toBe('Taken');
  });
});
