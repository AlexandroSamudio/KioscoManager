import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UsuariosListaComponent } from './usuarios-lista/usuarios-lista.component';
import { CodigoInvitacionComponent } from './codigo-invitacion/codigo-invitacion.component';

@Component({
  selector: 'app-usuarios-permisos',
  standalone: true,
  imports: [CommonModule, UsuariosListaComponent, CodigoInvitacionComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="p-6">
      <div class="mx-auto">
        <div class="mb-6">
          <h2 class="text-xl font-semibold text-amber-900">
            Usuarios y Permisos
          </h2>
          <p class="text-amber-700 mt-1">
            Gestiona empleados, códigos de invitación y roles
          </p>
        </div>

        <div class="border-b border-amber-200 mb-6">
          <nav class="-mb-px flex space-x-6">
            <button
              (click)="activeTab.set('usuarios')"
              [class.border-amber-500]="activeTab() === 'usuarios'"
              [class.border-b-2]="activeTab() === 'usuarios'"
              class="py-3 px-2 font-medium text-sm"
              [class.text-amber-700]="activeTab() === 'usuarios'"
              [class.text-amber-600]="activeTab() !== 'usuarios'"
              [class.hover:text-amber-800]="activeTab() !== 'usuarios'"
            >
              Usuarios
            </button>
            <button
              (click)="activeTab.set('codigos')"
              [class.border-amber-500]="activeTab() === 'codigos'"
              [class.border-b-2]="activeTab() === 'codigos'"
              class="py-3 px-2 font-medium text-sm"
              [class.text-amber-700]="activeTab() === 'codigos'"
              [class.text-amber-600]="activeTab() !== 'codigos'"
              [class.hover:text-amber-800]="activeTab() !== 'codigos'"
            >
              Códigos de Invitación
            </button>
          </nav>
        </div>

        <ng-container *ngIf="activeTab() === 'usuarios'">
          <app-usuarios-lista></app-usuarios-lista>
        </ng-container>

        <ng-container *ngIf="activeTab() === 'codigos'">
          <app-codigo-invitacion></app-codigo-invitacion>
        </ng-container>
      </div>
    </div>
  `,
  styles: [],
})
export class UsuariosPermisosComponent {
  activeTab = signal<'usuarios' | 'codigos'>('usuarios');
}
