import { Component, signal, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { AccountService } from '../../_services/account.service';
import { NotificationService } from '../../_services/notification.service';
import { CreateKiosco } from '../../_models/create-kiosco.model';

@Component({
  selector: 'app-crear-kiosco',
  imports: [FormsModule],
  templateUrl: './crear-kiosco.component.html',
  styleUrl: './crear-kiosco.component.css'
})
export class CrearKioscoComponent {
  private accountService = inject(AccountService);
  private router = inject(Router);
  private notificationService = inject(NotificationService);

  kioscoName = signal<string>('');
  isSubmitting = signal<boolean>(false);
  hasError = signal<boolean>(false);
  errorMessage = signal<string>('');

  onKioscoNameChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.kioscoName.set(target.value);
    this.validateKioscoName();
  }

  validateKioscoName(): void {
    const name = this.kioscoName().trim();

    if (!name) {
      this.setError('El nombre del kiosco es obligatorio');
      return;
    }

    if (/\d/.test(name)) {
      this.setError('El nombre del kiosco no puede contener números');
      return;
    }

    this.clearError();
  }

  private setError(message: string): void {
    this.hasError.set(true);
    this.errorMessage.set(message);
  }

  private clearError(): void {
    this.hasError.set(false);
    this.errorMessage.set('');
  }

  onCreateKiosco(): void {
    this.validateKioscoName();

    if (this.hasError()) {
      return;
    }

    this.isSubmitting.set(true);

    const createKioscoData: CreateKiosco = {
      nombre: this.kioscoName().trim()
    };

    this.accountService.createKiosco(createKioscoData)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (user) => {
          if (user) {
            this.notificationService.success(
              '¡Kiosco creado exitosamente!',
              `Tu kiosco "${this.kioscoName()}" ha sido creado. Ahora eres el administrador.`
            );
            this.router.navigate(['/dashboard']);
          }
        },
        error: (error) => {
          console.error('Error al crear el kiosco:', error);
          const errorMessage = this.getErrorMessage(error);
          this.notificationService.error('Error al crear el kiosco', errorMessage);
        }
      });
  }

  private getErrorMessage(error: any): string {
    if (error.status === 400) {
      if (typeof error.error === 'string') {
        return error.error;
      }
      if (error.error?.errors) {
        const errorMessages = Object.values(error.error.errors).flat();
        return errorMessages[0] as string;
      }
      return 'Los datos proporcionados no son válidos';
    }
    if (error.status === 401) {
      return 'No tienes autorización para realizar esta acción';
    }
    if (error.status === 0) {
      return 'Error de conexión. Verifica tu conexión a internet';
    }
    return 'Ha ocurrido un error inesperado. Inténtalo de nuevo';
  }

  isFormValid(): boolean {
    return !this.hasError() && this.kioscoName().trim().length > 0;
  }
}
