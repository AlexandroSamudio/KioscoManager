import {
  Component,
  DestroyRef,
  OnInit,
  WritableSignal,
  inject,
  signal,
  computed,
  ViewChild,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from '../navbar/navbar.component';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ReporteService } from '../../_services/reporte.service';
import { Reporte } from '../../_models/reporte.model';
import { VentaPorDia } from '../../_models/venta-por-dia.model';
import { CategoriaRentabilidad } from '../../_models/categoria-rentabilidad.model';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import {
  MatMomentDateModule,
  MAT_MOMENT_DATE_ADAPTER_OPTIONS,
  MomentDateAdapter,
} from '@angular/material-moment-adapter';
import * as _moment from 'moment';
import 'moment/locale/es';
import {
  MAT_DATE_LOCALE,
  DateAdapter,
  MAT_DATE_FORMATS,
} from '@angular/material/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import {
  DateRangePickerComponent,
  DateRange,
} from './date-range-picker/date-range-picker.component';
import { VentasChartComponent } from './ventas-chart/ventas-chart.component';
import { CategoriasChartComponent } from './categorias-chart/categorias-chart.component';
import { Observable } from 'rxjs';
import { NotificationService } from '../../_services/notification.service';
import { setPaginatedResponseSignal } from '../../_services/pagination.helper';
import { TipoAgrupacion } from '../../_models/venta-por-dia.model';

export interface ReportesErrores {
  resumen: string | null;
  productos: string | null;
  ventas: string | null;
  categorias: string | null;
}

@Component({
  selector: 'app-reportes-page',
  standalone: true,
  imports: [
    CommonModule,
    NavbarComponent,
    FormsModule,
    ReactiveFormsModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatInputModule,
    MatMomentDateModule,
    DateRangePickerComponent,
    VentasChartComponent,
    CategoriasChartComponent,
  ],
  templateUrl: './reportes-page.component.html',
  styleUrl: './reportes-page.component.css',
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
})
export class ReportesPageComponent implements OnInit {
  @ViewChild(DateRangePickerComponent)
  dateRangePicker!: DateRangePickerComponent;

  productosWidthPercentages: WritableSignal<number[]> = signal([]);
  private reporteService = inject(ReporteService);
  private destroyRef = inject(DestroyRef);
  private fb = inject(FormBuilder);
  private notificationService = inject(NotificationService);

  reporteSummary: WritableSignal<Reporte | null> = signal(null);
  ventasPorDia: WritableSignal<VentaPorDia[]> = signal([]);
  categoriasRentabilidad: WritableSignal<CategoriaRentabilidad[]> = signal([]);

  isLoading: WritableSignal<boolean> = signal(false);
  isSummaryLoading: WritableSignal<boolean> = signal(false);
  isProductosLoading: WritableSignal<boolean> = signal(false);
  isVentasLoading: WritableSignal<boolean> = signal(false);
  isCategoriasLoading: WritableSignal<boolean> = signal(false);
  get minDate(): Date {
    const year = new Date().getFullYear();
    const date = new Date(year, 0, 1);
    return this.getStartOfDay(date);
  }

  get maxDate(): Date {
    const year = new Date().getFullYear();
    const date = new Date(year, 11, 31);
    return this.getEndOfDay(date);
  }

  errores: WritableSignal<ReportesErrores> = signal({
    resumen: null,
    productos: null,
    ventas: null,
    categorias: null,
  });

  currentPage = signal<number>(1);
  pageSize = signal<number>(5);

  tipoAgrupacionInfo = computed(() => {
    const ventas = this.ventasPorDia();
    if (ventas.length === 0) {
      return {
        tipo: 'daily' as TipoAgrupacion,
        descripcion: 'Diario',
        titulo: 'Monto total de Ventas por Período',
      };
    }

    const tipo = ventas[0]?.tipoAgrupacion ?? 'daily';
    switch (tipo) {
      case 'weekly':
        return { tipo, descripcion: 'Semanal', titulo: 'Ventas por Semana' };
      case 'monthly':
        return { tipo, descripcion: 'Mensual', titulo: 'Ventas por Mes' };
      default:
        return {
          tipo: 'daily' as TipoAgrupacion,
          descripcion: 'Diario',
          titulo: 'Monto total de Ventas por Período',
        };
    }
  });

  rangoFechasInfo = computed(() => {
    const fechaInicio = this.getFechaFromFormControl('fechaInicio');
    const fechaFin = this.getFechaFromFormControl('fechaFin', true);

    if (!fechaInicio || !fechaFin) return '';

    const fechaInicioDate =
      typeof fechaInicio === 'string' ? new Date(fechaInicio) : fechaInicio;
    const fechaFinDate =
      typeof fechaFin === 'string' ? new Date(fechaFin) : fechaFin;

    const dias =
      Math.ceil(
        (fechaFinDate.getTime() - fechaInicioDate.getTime()) /
          (1000 * 60 * 60 * 24)
      ) + 1;

    if (dias <= 31) {
      return `${dias} días seleccionados - Vista diaria`;
    } else if (dias <= 90) {
      return `${dias} días seleccionados - Vista semanal para mejor visualización`;
    } else {
      return `${dias} días seleccionados - Vista mensual para mejor visualización`;
    }
  });

  isUsingCustomFilter = computed(() => {
    return this.dateRangePicker && !this.dateRangePicker.hasActiveRange();
  });

  fechasForm: FormGroup = this.fb.group(
    {
      fechaInicio: [this.formatDateForInput(this.getStartOfDay(this.getNow()))],
      fechaFin: [this.formatDateForInput(this.getEndOfDay(this.getNow()))],
    },
    { validators: [this.fechaFinPosteriorValidator] }
  );

  ngOnInit(): void {
    _moment.locale('es');
    const fechaInicio = this.getFechaFromFormControl('fechaInicio');
    const fechaFin = this.getFechaFromFormControl('fechaFin', true);
    this.cargarReportesConRango(fechaInicio!, fechaFin!);
  }

  cargarReportes(): void {
    this.isLoading.set(true);
    this.errores.set({
      resumen: null,
      productos: null,
      ventas: null,
      categorias: null,
    });

    const fechaInicio = this.getFechaFromFormControl('fechaInicio');
    const fechaFin = this.getFechaFromFormControl('fechaFin', true);
    if (!this.validarFechasSeleccionadas(fechaInicio, fechaFin)) {
      this.isLoading.set(false);
      return;
    }
    this.cargarResumenConRango(fechaInicio!, fechaFin!);
    this.cargarTopProductosConRango(fechaInicio!, fechaFin!);
    this.cargarVentasPorDiaConRango(fechaInicio!, fechaFin!);
    this.cargarCategoriasRentabilidadConRango(fechaInicio!, fechaFin!);
  }

  private validarFechasSeleccionadas(
    fechaInicio?: string,
    fechaFin?: string
  ): boolean {
    if (!fechaInicio || !fechaFin) {
      this.errores.set({
        resumen: 'Fechas inválidas',
        productos: 'Fechas inválidas',
        ventas: 'Fechas inválidas',
        categorias: 'Fechas inválidas',
      });
      return false;
    }
    return true;
  }

  private cargarDato<T>(
    observable: Observable<T>,
    setLoading: (value: boolean) => void,
    errorKey: keyof ReportesErrores,
    defaultErrorMsg: string,
    setData?: (value: T) => void,
    setPaginated?: boolean
  ): void {
    setLoading(true);
    observable.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (data: T | any) => {
        if (setPaginated) {
          setPaginatedResponseSignal(
            data,
            this.reporteService.productosPaginados
          );
        }
        if (setData) {
          setData(data);
        }
        setLoading(false);
        this.checkLoadingComplete();
      },
      error: (error: unknown) => {
        this.errores.update((e) => ({
          ...e,
          [errorKey]: typeof error === 'string' ? error : defaultErrorMsg,
        }));
        setLoading(false);
        this.checkLoadingComplete();
      },
    });
  }

  private cargarResumenConRango(fechaInicio: string, fechaFin: string): void {
    this.cargarDato(
      this.reporteService.getReporteSummary(fechaInicio, fechaFin),
      this.isSummaryLoading.set,
      'resumen',
      'Error al cargar el resumen',
      this.reporteSummary.set
    );
  }

  private cargarTopProductosConRango(
    fechaInicio: string,
    fechaFin: string
  ): void {
    this.cargarDato(
      this.reporteService.getTopProductos(
        10,
        this.currentPage(),
        this.pageSize(),
        fechaInicio,
        fechaFin
      ),
      this.isProductosLoading.set,
      'productos',
      'Error al cargar productos más vendidos',
      undefined,
      true
    );

    setTimeout(() => {
      const productos = this.productosPaginados?.result ?? [];
      const topCantidad =
        productos.length > 0 && productos[0].cantidadVendida > 0
          ? productos[0].cantidadVendida
          : 0;
      const percentages = productos.map((producto) => {
        if (topCantidad === 0) return 0;
        return (producto.cantidadVendida / topCantidad) * 100;
      });
      this.productosWidthPercentages.set(percentages);
    }, 0);
  }

  private cargarVentasPorDiaConRango(
    fechaInicio: string,
    fechaFin: string
  ): void {
    this.cargarDato(
      this.reporteService.getVentasPorDia(fechaInicio, fechaFin),
      this.isVentasLoading.set,
      'ventas',
      'Error al cargar ventas por día',
      this.ventasPorDia.set
    );
  }

  private cargarCategoriasRentabilidadConRango(
    fechaInicio: string,
    fechaFin: string
  ): void {
    this.cargarDato(
      this.reporteService.getCategoriasRentabilidad(fechaInicio, fechaFin),
      this.isCategoriasLoading.set,
      'categorias',
      'Error al cargar rentabilidad por categorías',
      this.categoriasRentabilidad.set
    );
  }

  cargarReportesConRango(fechaInicio: string, fechaFin: string): void {
    this.isLoading.set(true);
    this.errores.set({
      resumen: null,
      productos: null,
      ventas: null,
      categorias: null,
    });

    this.cargarResumenConRango(fechaInicio, fechaFin);
    this.cargarTopProductosConRango(fechaInicio, fechaFin);
    this.cargarVentasPorDiaConRango(fechaInicio, fechaFin);
    this.cargarCategoriasRentabilidadConRango(fechaInicio, fechaFin);
  }

  private fechaFinPosteriorValidator(form: FormGroup) {
    const inicio = form.get('fechaInicio')?.value;
    const fin = form.get('fechaFin')?.value;
    if (inicio && fin && inicio > fin) {
      return { fechaFinAnterior: true };
    }
    return null;
  }

  aplicarFiltros(): void {
    const fechaInicio = this.getFechaFromFormControl('fechaInicio');
    const fechaFin = this.getFechaFromFormControl('fechaFin', true);
    if (this.fechasForm.invalid) {
      this.notificationService.showError(
        'Por favor, verifica las fechas ingresadas.'
      );
      return;
    }

    if (this.dateRangePicker) {
      this.dateRangePicker.clearActiveRange();
    }

    this.cargarReportesConRango(fechaInicio!, fechaFin!);
  }

  onRangeSelected(range: DateRange): void {
    this.setFechasFormFromRange(range);
    var fechaInicio = range.startDate;
    var fechaFin = range.endDate;
    console.log('Rango seleccionado:', fechaInicio, fechaFin);
    this.cargarReportesConRango(fechaInicio, fechaFin);
  }

  private setFechasFormFromRange(range: DateRange): void {
    this.fechasForm.patchValue({
      fechaInicio: this.formatDateForInput(range.startDate),
      fechaFin: this.formatDateForInput(range.endDate),
    });
  }

  private checkLoadingComplete(): void {
    if (
      !this.isSummaryLoading() &&
      !this.isProductosLoading() &&
      !this.isVentasLoading() &&
      !this.isCategoriasLoading()
    ) {
      this.isLoading.set(false);
    }
  }

  hasErrors(): boolean {
    return Object.values(this.errores()).some((e) => e !== null);
  }

  recargarDatos(): void {
    const fechaInicio = this.getFechaFromFormControl('fechaInicio');
    const fechaFin = this.getFechaFromFormControl('fechaFin', true);
    this.cargarReportesConRango(fechaInicio!, fechaFin!);
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('es-ES');
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('es-ES', {
      style: 'currency',
      currency: 'ARS',
    }).format(value);
  }

  private getFechaFromFormControl(
    controlName: string,
    isEndDate = false
  ): string | undefined {
    const control = this.fechasForm.get(controlName);
    if (!control?.value) {
      return undefined;
    }

    let date: Date;
    const value = control.value;

    if (value instanceof Date) {
      date = value;
    } else if (typeof value === 'string') {
      const parts = value.split('-').map(Number);
      if (parts.length === 3) {
        date = new Date(Date.UTC(parts[0], parts[1] - 1, parts[2]));
      } else {
        const parsedDate = new Date(value);
        if (!isNaN(parsedDate.getTime())) {
          date = parsedDate;
        } else {
          return undefined;
        }
      }
    } else if (
      typeof value === 'object' &&
      value !== null &&
      typeof value.toDate === 'function'
    ) {
      date = value.toDate();
    } else {
      return undefined;
    }

    if (isNaN(date.getTime())) {
      return undefined;
    }

    const year = date.getUTCFullYear();
    const month = date.getUTCMonth();
    const day = date.getUTCDate();

    if (isEndDate) {
      return new Date(
        Date.UTC(year, month, day, 23, 59, 59, 999)
      ).toISOString();
    } else {
      return new Date(Date.UTC(year, month, day, 0, 0, 0, 0)).toISOString();
    }
  }

  private formatDateForInput(date: any): string {
    const d: Date = typeof date === 'string' ? new Date(date) : date;
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  private getNow(): Date {
    return new Date();
  }

  private getStartOfDay(date: Date): Date {
    const d = new Date(date);
    d.setHours(0, 0, 0, 0);
    return d;
  }

  private getEndOfDay(date: Date): Date {
    const d = new Date(date);
    d.setHours(23, 59, 59, 999);
    return d;
  }

  get productosPaginados() {
    return this.reporteService.productosPaginados();
  }

  cambiarPagina(page: number) {
    const fechaInicio = this.getFechaFromFormControl('fechaInicio');
    const fechaFin = this.getFechaFromFormControl('fechaFin', true);
    this.currentPage.set(page);
    this.cargarTopProductosConRango(fechaInicio!, fechaFin!);
  }
}
