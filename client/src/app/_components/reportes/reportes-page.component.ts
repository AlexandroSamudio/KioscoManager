import {
  Component,
  DestroyRef,
  OnInit,
  WritableSignal,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from '../navbar/navbar.component';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ReporteService } from '../../_services/reporte.service';
import { Reporte } from '../../_models/reporte.model';
import { VentaPorDia } from '../../_models/venta-por-dia.model';
import { CategoriaRentabilidad } from '../../_models/categoria-rentabilidad.model';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { FormBuilder, FormGroup } from '@angular/forms';
import {DateRangePickerComponent,DateRange,} from './date-range-picker/date-range-picker.component';
import { KpiCardComponent } from './kpi-card/kpi-card.component';
import { VentasChartComponent } from './ventas-chart/ventas-chart.component';
import { CategoriasChartComponent } from './categorias-chart/categorias-chart.component';
import { Observable } from 'rxjs';
import { NotificationService } from '../../_services/notification.service';
import { setPaginatedResponse, setPaginatedResponseSignal } from '../../_services/pagination.helper';

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
    DateRangePickerComponent,
    KpiCardComponent,
    VentasChartComponent,
    CategoriasChartComponent,
  ],
  templateUrl: './reportes-page.component.html',
  styleUrl: './reportes-page.component.css',
})
export class ReportesPageComponent implements OnInit {
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

  errores: WritableSignal<ReportesErrores> = signal({
    resumen: null,
    productos: null,
    ventas: null,
    categorias: null,
  });

  currentPage = signal<number>(1);
  pageSize = signal<number>(6);

  fechasForm: FormGroup = this.fb.group(
    {
      fechaInicio: [this.formatDateForInput(this.getStartOfDay(this.getNow()))],
      fechaFin: [this.formatDateForInput(this.getEndOfDay(this.getNow()))],
    },
    { validators: [this.fechaFinPosteriorValidator] }
  );

  ngOnInit(): void {
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
    fechaInicio?: Date,
    fechaFin?: Date
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
          setPaginatedResponseSignal(data, this.reporteService.productosPaginados);
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
        console.error(defaultErrorMsg, error);
      },
    });
  }

  private cargarResumenConRango(fechaInicio: Date, fechaFin: Date): void {
    this.cargarDato(
      this.reporteService.getReporteSummary(fechaInicio, fechaFin),
      this.isSummaryLoading.set,
      'resumen',
      'Error al cargar el resumen',
      this.reporteSummary.set
    );
  }

  private cargarTopProductosConRango(fechaInicio: Date, fechaFin: Date): void {
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
  }

  private cargarVentasPorDiaConRango(fechaInicio: Date, fechaFin: Date): void {
    this.cargarDato(
      this.reporteService.getVentasPorDia(fechaInicio, fechaFin),
      this.isVentasLoading.set,
      'ventas',
      'Error al cargar ventas por día',
      this.ventasPorDia.set
    );
  }

  private cargarCategoriasRentabilidadConRango(
    fechaInicio: Date,
    fechaFin: Date
  ): void {
    this.cargarDato(
      this.reporteService.getCategoriasRentabilidad(fechaInicio, fechaFin),
      this.isCategoriasLoading.set,
      'categorias',
      'Error al cargar rentabilidad por categorías',
      this.categoriasRentabilidad.set
    );
  }

  cargarReportesConRango(fechaInicio: Date, fechaFin: Date): void {
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
    this.cargarReportesConRango(fechaInicio!, fechaFin!);
  }

  onRangeSelected(range: DateRange): void {
    this.setFechasFormFromRange(range);
    this.cargarReportesConRango(range.startDate, range.endDate);
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
  ): Date | undefined {
    const value = this.fechasForm.get(controlName)?.value;
    if (!value) return undefined;

    if (value instanceof Date) {
      if (isEndDate) {
        value.setHours(23, 59, 59, 999);
      } else {
        value.setHours(0, 0, 0, 0);
      }
      return value;
    } else if (typeof value === 'string') {
      const [year, month, day] = value.split('-').map(Number);
      if (isEndDate) {
        return new Date(year, month - 1, day, 23, 59, 59, 999);
      }
      return new Date(year, month - 1, day, 0, 0, 0, 0);
    }
    return undefined;
  }

  private formatDateForInput(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
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
