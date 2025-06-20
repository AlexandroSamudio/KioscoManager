import { Injectable } from '@angular/core';
import Swal from 'sweetalert2';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {

  success(title: string, text: string): void {
    Swal.fire({
      icon: 'success',
      title,
      text,
      confirmButtonText: 'Aceptar'
    });
  }

  error(title: string, text: string): void {
    Swal.fire({
      icon: 'error',
      title,
      text,
      confirmButtonText: 'Aceptar'
    });
  }

  info(title: string, text: string): void {
    Swal.fire({
      icon: 'info',
      title,
      text,
      confirmButtonText: 'Cerrar',
      confirmButtonColor: '#3b82f6'
    });
  }

  warning(title: string, text: string): void {
    Swal.fire({
      icon: 'warning',
      title,
      text,
      confirmButtonText: 'Aceptar',
      confirmButtonColor: '#f59e42'
    });
  }

  async confirm(title: string, text: string): Promise<boolean> {
    const result = await Swal.fire({
      title,
      text,
      icon: 'question',
      showCancelButton: true,
      confirmButtonText: 'Confirmar',
      cancelButtonText: 'Cancelar'
    });

    return result.isConfirmed;
  }

  async promptTextarea(title: string, inputPlaceholder: string,msjError:string): Promise<string | null> {
    const result = await Swal.fire({
      title,
      input: 'textarea',
      inputPlaceholder,
      showCancelButton: true,
      confirmButtonText: 'Confirmar',
      cancelButtonText: 'Cancelar',
      inputValidator: (value) => {
        if (!value) {
          return msjError;
        }
        return null;
      }
    });

    return result.isConfirmed && typeof result.value === 'string' && result.value.trim() !== ''
      ? result.value.trim()
      : null;
  }
}
