import { Component, DestroyRef, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { ClipboardModule } from '@angular/cdk/clipboard';

import { NotificationService } from '../../../../_services/notification.service';
import { AccountService } from '../../../../_services/account.service';
import { GeneratedInvitationCode } from '../../../../_models/invitation-code.model';

@Component({
  selector: 'app-codigo-invitacion',
  standalone: true,
  imports: [CommonModule, ClipboardModule],
  templateUrl: './codigo-invitacion.component.html',
  styleUrls: ['./codigo-invitacion.component.css']
})
export class CodigoInvitacionComponent {
  private accountService = inject(AccountService);
  private notificationService = inject(NotificationService);
  private destroyRef = inject(DestroyRef);

  isLoading = signal<boolean>(false);
  generatedCode = signal<GeneratedInvitationCode | null>(null);

  generateInvitationCode(): void {
    this.isLoading.set(true);

    this.accountService.generateInvitationCode()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: (code: GeneratedInvitationCode) => {
          this.generatedCode.set(code);
          this.notificationService.success(
            'Éxito',
            'Código de invitación generado correctamente'
          );
        },
        error: (error: unknown) => {
          console.error('Error al generar código de invitación', error);
          this.notificationService.error(
            'Error',
            'No se pudo generar el código de invitación. Por favor, inténtalo de nuevo.'
          );
        }
      });
  }

  copyToClipboard(): void {
    if (this.generatedCode()) {
      this.notificationService.success(
        'Éxito',
        'Código copiado al portapapeles'
      );
    }
  }

  clearCode(): void {
    this.generatedCode.set(null);
  }
}
