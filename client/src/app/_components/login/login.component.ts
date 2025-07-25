import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AccountService } from '../../_services/account.service';
import { firstValueFrom } from 'rxjs';

interface LoginForm {
  email: string;
  password: string;
}

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly accountService = inject(AccountService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly isLoading = signal(false);
  readonly showPassword = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly loginForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  togglePasswordVisibility(): void {
    this.showPassword.update((show) => !show);
  }

  async onSubmit(): Promise<void> {
    if (this.loginForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    const formValue = this.loginForm.getRawValue() as LoginForm;
    this.isLoading.set(true);
    this.errorMessage.set(null);
    try {
      await firstValueFrom(this.accountService.login(formValue));
      await this.navigateAfterLogin(this.route.snapshot.queryParams['returnUrl']);
    } catch (error) {
      this.errorMessage.set(this.getErrorMessage(error));
    } finally {
      this.isLoading.set(false);
    }
  }

  private markFormGroupTouched(): void {
    Object.keys(this.loginForm.controls).forEach((key) => {
      const control = this.loginForm.get(key);
      control?.markAsTouched();
    });
  }

  private getErrorMessage(error: any): string {
    if (error.status === 401) {
      return 'Email o contraseña incorrectos';
    }
    if (error.status === 400) {
      return 'Por favor, verifica tus datos';
    }
    if (error.status === 0) {
      return 'Error de conexión. Verifica tu conexión a internet';
    }
    return 'Ha ocurrido un error inesperado. Inténtalo de nuevo';
  }

  getFieldError(fieldName: string): string | null {
    const field = this.loginForm.get(fieldName);
    if (field?.touched && field?.errors) {
      if (field.errors['required']) {
        return `Este campo es requerido`;
      }
      if (field.errors['email']) {
        return 'Ingresa un email válido';
      }
      if (field.errors['minlength']) {
        return 'La contraseña debe tener al menos 6 caracteres';
      }
    }
    return null;
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.loginForm.get(fieldName);
    return !!(field?.touched && field?.invalid);
  }

  private navigateAfterLogin(returnUrl?: string) {
    if (returnUrl) return this.router.navigateByUrl(returnUrl);
    const target = this.accountService.kioscoId()
      ? '/dashboard'
      : '/bienvenida';
    return this.router.navigate([target]);
  }
}
