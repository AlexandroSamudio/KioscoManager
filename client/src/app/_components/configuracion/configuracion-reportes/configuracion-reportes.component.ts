import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-configuracion-reportes',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="p-6">
      <div class="max-w-4xl mx-auto">
        <!-- Header -->
        <div class="mb-6">
          <h2 class="text-xl font-semibold text-amber-900">Configuración de Reportes</h2>
          <p class="text-amber-700 mt-1">Configura exportaciones y backups automáticos</p>
        </div>

        <!-- Content placeholder -->
        <div class="bg-gradient-to-br from-amber-50 to-orange-50 rounded-xl p-8 text-center border border-amber-200">
          <div class="text-amber-400 mb-4">
          </div>
          <h3 class="text-lg font-medium text-amber-900 mb-2">
            Configuración de Reportes
          </h3>
          <p class="text-amber-700 mb-4">
            Aquí podrás configurar formatos de exportación de datos y habilitar backups automáticos.
          </p>
          <div class="text-sm text-amber-600 bg-amber-100 rounded-lg px-3 py-2 inline-flex items-center">
            Funcionalidad en desarrollo
          </div>
        </div>
      </div>
    </div>
  `,
  styles: []
})
export class ConfiguracionReportesComponent {
  constructor() {}
}
