import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormControl, FormGroup } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatNativeDateModule } from '@angular/material/core';
import { ReporteService } from '../../../_services/reporte.service';
import { TipoReporte} from '../../../_models/exportacion-reporte.model';
import { ExcelPdfService } from '../../../_services/excel-pdf.service';
import {
  MAT_MOMENT_DATE_ADAPTER_OPTIONS,
  MomentDateAdapter,
} from '@angular/material-moment-adapter';
import * as _moment from 'moment';
import 'moment/locale/es';

const moment = (_moment as any).default || _moment;
import {
  MAT_DATE_LOCALE,
  DateAdapter,
  MAT_DATE_FORMATS,
} from '@angular/material/core';
import { NotificationService } from '../../../_services/notification.service';
@Component({
  selector: 'app-configuracion-reportes',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatDatepickerModule,
    MatInputModule,
    MatFormFieldModule,
    MatNativeDateModule
  ],
  providers: [
    { provide: MAT_DATE_LOCALE, useValue: 'es' },
    { provide: MAT_MOMENT_DATE_ADAPTER_OPTIONS, useValue: { useUtc: false } },
    {
      provide: DateAdapter,
      useClass: MomentDateAdapter,
      deps: [MAT_DATE_LOCALE, MAT_MOMENT_DATE_ADAPTER_OPTIONS],
    },
    {
      provide: MAT_DATE_FORMATS,
      useValue: {
        parse: {
          dateInput: 'L',
        },
        display: {
          dateInput: 'L',
          monthYearLabel: 'MMMM YYYY',
          dateA11yLabel: 'LL',
          monthYearA11yLabel: 'MMMM YYYY',
        },
      },
    },
  ],
  templateUrl: './configuracion-reportes.component.html',
  styleUrls: ['./configuracion-reportes.component.css']
})
export class ConfiguracionReportesComponent implements OnInit {
  private reporteService = inject(ReporteService);
  private notificacionService = inject(NotificationService);
  private excelPdfService = inject(ExcelPdfService);

  tipoReporteSeleccionado = signal<TipoReporte | null>(null);
  isLoading = signal<boolean>(false);

  fechaInicioControl = new FormControl<Date | null>(null);
  fechaFinControl = new FormControl<Date | null>(null);

  filtrosFechaForm = new FormGroup({
    fechaInicio: this.fechaInicioControl,
    fechaFin: this.fechaFinControl
  });

  readonly tiposReporte: TipoReporte[] = [
    {
      id: 'ventas',
      nombre: 'Lista de Ventas',
      descripcion: 'Exportar historial completo de ventas realizadas',
      icono: 'fas fa-shopping-cart'
    },
    {
      id: 'productos',
      nombre: 'Lista de Productos',
      descripcion: 'Exportar inventario actual con precios y stock',
      icono: 'fas fa-box'
    },
    {
      id: 'compras',
      nombre: 'Lista de Compras',
      descripcion: 'Exportar historial de compras a proveedores',
      icono: 'fas fa-truck'
    }
  ];

  ngOnInit(): void {
    _moment.locale('es');
  }

  seleccionarTipoReporte(tipo: TipoReporte): void {
    this.tipoReporteSeleccionado.set(tipo);
  }

  private obtenerFechasComoISOString(): { fechaInicio?: string; fechaFin?: string } {
    const fechaInicio = this.fechaInicioControl.value;
    const fechaFin = this.fechaFinControl.value;

    return {
      fechaInicio: fechaInicio ? moment(fechaInicio).startOf('day').toISOString() : undefined,
      fechaFin: fechaFin ? moment(fechaFin).endOf('day').toISOString() : undefined
    };
  }

  async exportarExcel(): Promise<void> {
    const tipoSeleccionado = this.tipoReporteSeleccionado();
    if (!tipoSeleccionado) return;

    this.isLoading.set(true);

    try {
      const { fechaInicio, fechaFin } = this.obtenerFechasComoISOString();
      if(fechaFin && fechaInicio && moment(fechaFin).isBefore(fechaInicio)) {
        this.notificacionService.showError('La fecha de fin no puede ser anterior a la fecha de inicio.');
        return;
      }
      let data: any[] = [];
      switch (tipoSeleccionado.id) {
        case 'ventas':
          data = await firstValueFrom(this.reporteService.getVentasParaExportar(
            fechaInicio,
            fechaFin
          )) || [];
          break;
        case 'productos':
          data = await firstValueFrom(this.reporteService.getProductosParaExportar()) || [];
          break;
        case 'compras':
          data = await firstValueFrom(this.reporteService.getComprasParaExportar(
            fechaInicio,
            fechaFin
          )) || [];
          break;
      }

      this.excelPdfService.exportarExcel(tipoSeleccionado.id, data);
    } catch (error) {
      this.notificacionService.showError('Error al exportar el reporte a Excel.');
    } finally {
      this.isLoading.set(false);
    }
  }

  async exportarPDF(): Promise<void> {
    const tipoSeleccionado = this.tipoReporteSeleccionado();
    if (!tipoSeleccionado) return;

    this.isLoading.set(true);
    try {
      const { fechaInicio, fechaFin } = this.obtenerFechasComoISOString();

      if(fechaFin && fechaInicio && moment(fechaFin).isBefore(fechaInicio)) {
        this.notificacionService.showError('La fecha de fin no puede ser anterior a la fecha de inicio.');
        return;
      }
      let data: any[] = [];
      switch (tipoSeleccionado.id) {
        case 'ventas':
          data = await firstValueFrom(this.reporteService.getVentasParaExportar(
            fechaInicio,
            fechaFin
          )) || [];
          break;
        case 'productos':
          data = await firstValueFrom(this.reporteService.getProductosParaExportar()) || [];
          break;
        case 'compras':
          data = await firstValueFrom(this.reporteService.getComprasParaExportar(
            fechaInicio,
            fechaFin
          )) || [];
          break;
      }
      this.excelPdfService.exportarPDF(tipoSeleccionado.id, data);
    } catch (error) {
      this.notificacionService.showError('Error al exportar el reporte a PDF.');
    } finally {
      this.isLoading.set(false);
    }
  }

  limpiarSeleccion(): void {
    this.tipoReporteSeleccionado.set(null);
    this.fechaInicioControl.setValue(null);
    this.fechaFinControl.setValue(null);
  }

  get puedeExportar(): boolean {
    return this.tipoReporteSeleccionado() !== null && !this.isLoading();
  }

  get textoFiltroFechas(): string {
    const fechaInicio = this.fechaInicioControl.value;
    const fechaFin = this.fechaFinControl.value;

    if (fechaInicio && fechaFin) {
      return `Del ${moment(fechaInicio).format('L')} al ${moment(fechaFin).format('L')}`;
    }
    if (fechaInicio) {
      return `Desde ${moment(fechaInicio).format('L')}`;
    }
    if (fechaFin) {
      return `Hasta ${moment(fechaFin).format('L')}`;
    }
    return 'Sin filtros de fecha';
  }
}
