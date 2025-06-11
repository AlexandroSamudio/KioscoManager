import { Component, inject, OnDestroy, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AccountService } from '../../_services/account.service';
import { Subject, firstValueFrom } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

interface RegisterForm {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, RouterModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent implements OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly accountService = inject(AccountService);
  private readonly router = inject(Router);

  readonly isLoading = signal(false);
  readonly showPassword = signal(false);
  readonly showConfirmPassword = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  private destroy$ = new Subject<void>();

  readonly registerForm = this.fb.nonNullable.group({
    username: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, this.passwordStrengthValidator]],
    confirmPassword: ['', [Validators.required, this.matchValues('password')]]
  });

  constructor() {
    this.registerForm.controls['password'].valueChanges.pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: () => this.registerForm.controls['confirmPassword'].updateValueAndValidity()
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private passwordStrengthValidator(control: AbstractControl): ValidationErrors | null {
    const value = control.value;

    if (!value) {
      return null;
    }

    // La contraseña debe tener 1 mayúscula, 1 minúscula, 1 número, 1 carácter especial y tener entre 8 y 128 caracteres de longitud.
    const passwordRegex = /^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[\W_]).{8,128}$/;

    if (!passwordRegex.test(value)) {
      return { passwordStrength: true };
    }

    return null;
  }

  togglePasswordVisibility(): void {
    this.showPassword.update(show => !show);
  }

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword.update(show => !show);
  }

  async onSubmit(): Promise<void> {
    if (this.registerForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    const formValue = this.registerForm.getRawValue() as RegisterForm;
    const { confirmPassword, ...registerData } = formValue;

    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    try {
      const user = await firstValueFrom(this.accountService.register(registerData));
      if (user) {
        this.successMessage.set('Cuenta creada exitosamente. Redirigiendo...');
        setTimeout(() => {
          this.router.navigate(['/bienvenida']);
        }, 1500);
      }
    } catch (error:any) {
      this.errorMessage.set(this.getErrorMessage(error));
    } finally {
      this.isLoading.set(false);
    }
  }

  private markFormGroupTouched(): void {
    Object.keys(this.registerForm.controls).forEach(key => {
      const control = this.registerForm.get(key);
      control?.markAsTouched();
    });
  }

  private getErrorMessage(error: any): string {
    if (error.status === 400) {
      if (error.error?.errors) {
        const errorMessages = Object.values(error.error.errors).flat();
        return errorMessages[0] as string;
      }
      return error.error || 'Los datos proporcionados no son válidos';
    }
    if (error.status === 0) {
      return 'Error de conexión. Verifica tu conexión a internet';
    }
    return 'Ha ocurrido un error inesperado. Inténtalo de nuevo';
  }

  getFieldError(fieldName: string): string | null {
    const field = this.registerForm.get(fieldName);
    if (field?.touched && field?.errors) {
      if (field.errors['required']) {
        return 'El campo es requerido';
      }
      if (field.errors['email']) {
        return 'Ingresa un email válido';
      }
      if (field.errors['minlength']) {
        const minLength = field.errors['minlength'].requiredLength;
        return `Debe tener al menos ${minLength} caracteres`;
      }
      if (field.errors['maxlength']) {
        const maxLength = field.errors['maxlength'].requiredLength;
        return `No debe exceder ${maxLength} caracteres`;
      }
      if (field.errors['passwordStrength']) {
        return 'La contraseña debe contener al menos una letra mayúscula, una letra minúscula, un número, un carácter especial y tener mínimo 8 caracteres';
      }
      if (fieldName === 'confirmPassword' && field.errors['isMatching']) {
        return 'Las contraseñas no coinciden';
      }
    }
    return null;
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.registerForm.get(fieldName);
    let isInvalid = !!(field?.touched && field?.invalid);

    if (fieldName === 'confirmPassword' && field?.touched && field?.hasError('isMatching')) {
      isInvalid = true;
    }

    return isInvalid;
  }

  passwordsMatch(): boolean {
    const password = this.registerForm.get('password')?.value;
    const confirmPassword = this.registerForm.get('confirmPassword')?.value;
    const confirmPasswordTouched = this.registerForm.get('confirmPassword')?.touched;

    return !!(password && confirmPassword && password === confirmPassword && confirmPasswordTouched);
  }

  passwordsDontMatch(): boolean {
    const password = this.registerForm.get('password')?.value;
    const confirmPassword = this.registerForm.get('confirmPassword')?.value;
    const confirmPasswordTouched = this.registerForm.get('confirmPassword')?.touched;

    return !!(password && confirmPassword && password !== confirmPassword && confirmPasswordTouched);
  }

  matchValues(matchTo:string): ValidatorFn{
     return (control:AbstractControl) =>{
      return control.value === control.parent?.get(matchTo)?.value ? null : {isMatching:true};
     }
  }
}
