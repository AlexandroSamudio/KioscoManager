import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UsuariosListaComponent } from './usuarios-lista/usuarios-lista.component';

@Component({
  selector: 'app-usuarios-permisos',
  standalone: true,
  imports: [CommonModule, UsuariosListaComponent],
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
              class="border-amber-500 text-amber-700 border-b-2 py-3 px-2 font-medium text-sm"
            >
              Usuarios
            </button>
            <button
              class="text-amber-600 hover:text-amber-800 py-3 px-2 font-medium text-sm"
            >
              Códigos de Invitación
            </button>
          </nav>
        </div>

        <app-usuarios-lista></app-usuarios-lista>
      </div>
    </div>
  `,
  styles: [],
})
export class UsuariosPermisosComponent {
  constructor() {}
}
