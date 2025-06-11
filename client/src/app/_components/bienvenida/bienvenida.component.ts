import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AccountService } from '../../_services/account.service';
import { NotificationService } from '../../_services/notification.service';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-bienvenida',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './bienvenida.component.html',
  styleUrl: './bienvenida.component.css'
})
export class BienvenidaComponent implements OnInit {
  private accountService = inject(AccountService);
  private router = inject(Router);
  private notificationService = inject(NotificationService);

  invitationCode = signal('');
  isSubmitting = signal(false);

  ngOnInit() {
    const kioscoId = this.accountService.kioscoId();
    if (kioscoId) {
      this.router.navigate(['/dashboard']);
    }
  }

  async onJoinKiosco() {
    const code = this.invitationCode();

    if (!code.trim()) {
      this.notificationService.error(
        'Código requerido',
        'Por favor, ingresa un código de invitación válido.'
      );
      return;
    }

    this.isSubmitting.set(true);

    try {
      await firstValueFrom(
        this.accountService.joinKiosco({ codigoInvitacion: code.trim() })
      );
      this.notificationService.success(
        'Unión exitosa',
        'Te has unido al kiosco exitosamente.'
      );
      this.router.navigate(['/dashboard']);
    } catch {
      this.notificationService.error(
        'Error al unirse al kiosco',
        'Por favor, verifica el código de invitación e inténtalo de nuevo.'
      );
    } finally {
      this.isSubmitting.set(false);
    }
  }

  navigateTo(){
    this.router.navigate(['/crear-kiosco']);
  }
}
