import { Pipe, PipeTransform } from '@angular/core';
import { TipoAgrupacion } from '../_models/venta-por-dia.model';

@Pipe({
  name: 'fechaAgrupacion',
  standalone: true
})
export class FechaAgrupacionPipe implements PipeTransform {

  transform(fecha: Date, tipoAgrupacion: TipoAgrupacion = 'daily'): string {
    if (!fecha || !(fecha instanceof Date) || isNaN(fecha.getTime())) {
      return '';
    }

    const options: Intl.DateTimeFormatOptions = {
      timeZone: 'America/Argentina/Buenos_Aires'
    };

    switch (tipoAgrupacion) {
      case 'daily':
        return fecha.toLocaleDateString('es-ES', {
          ...options,
          day: '2-digit',
          month: '2-digit'
        });

      case 'weekly':
        return `Semana del ${fecha.toLocaleDateString('es-ES', {
          ...options,
          day: '2-digit',
          month: '2-digit'
        })}`;

      case 'monthly':
        return fecha.toLocaleDateString('es-ES', {
          ...options,
          month: 'long',
          year: 'numeric'
        });

      default:
        return fecha.toLocaleDateString('es-ES', {
          ...options,
          day: '2-digit',
          month: '2-digit'
        });
    }
  }
}
