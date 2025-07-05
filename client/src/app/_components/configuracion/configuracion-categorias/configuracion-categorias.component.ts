import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CategoriasListaComponent } from './categorias-lista/categorias-lista.component';

@Component({
  selector: 'app-configuracion-categorias',
  standalone: true,
  imports: [CommonModule, CategoriasListaComponent],
  template: `
    <div class="p-6">
      <div class="max-w-4xl mx-auto">
        <div class="mb-6">
          <h2 class="text-xl font-semibold text-amber-900">Configuración de Categorías</h2>
          <p class="text-amber-700 mt-1">Gestión de categorías de productos</p>
        </div>

        <app-categorias-lista></app-categorias-lista>
      </div>
    </div>
  `,
  styles: []
})
export class ConfiguracionCategoriasComponent {
  constructor() {}
}
