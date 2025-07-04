import {
  Component,
  signal,
  inject,
  ChangeDetectionStrategy,
  OnInit,
  output,
  DestroyRef,
} from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { KioscoBasicInfo } from '../../_models/configuracion.model';
import { InfoNegocioService } from '../../_services/info-negocio.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-info-negocio',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './info-negocio.component.html',
  styleUrl: './info-negocio.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InfoNegocioComponent implements OnInit {
  private readonly infoNegocioService = inject(InfoNegocioService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly fb = inject(FormBuilder);

  formValid = output<boolean>();

  businessForm!: FormGroup;

  businessInfo = signal<KioscoBasicInfo>({
    nombre: '',
    direccion: '',
    telefono: '',
  });

  originalBusinessInfo = signal<KioscoBasicInfo>({
    nombre: '',
    direccion: '',
    telefono: '',
  });

  isSubmitting = signal<boolean>(false);
  isLoading = signal<boolean>(false);

  ngOnInit(): void {
    this.initForm();
    this.loadBusinessInfo();

    this.businessForm.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        this.businessInfo.set({
          nombre: value.nombre,
          direccion: value.direccion,
          telefono: value.telefono,
        });
        this.emitFormState();
      });
  }

  private initForm(): void {
    this.businessForm = this.fb.group({
      nombre: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(100),
          Validators.pattern(/^(?!\d+$).*$/),
        ],
      ],
      direccion: ['', [Validators.maxLength(200)]],
      telefono: [
        '',
        [Validators.pattern(/^[\+]?[\d\s\-\(\)]+$/), Validators.maxLength(20)],
      ],
    });
  }

  private emitFormState(): void {
    const isValid = this.isFormValid();
    this.formValid.emit(isValid);
  }

  isFormValid(): boolean {
    return (
      this.businessForm.valid &&
      this.businessForm.get('nombre')?.value?.trim().length > 0
    );
  }

  hasFieldError(field: keyof KioscoBasicInfo): boolean {
    const control = this.businessForm.get(field);
    return control
      ? control.invalid && (control.dirty || control.touched)
      : false;
  }

  getFieldError(field: keyof KioscoBasicInfo): string {
    const control = this.businessForm.get(field);
    if (!control || !control.errors || !(control.dirty || control.touched)) {
      return '';
    }

    if (control.errors['required']) {
      return 'Este campo es obligatorio';
    }
    if (control.errors['minlength']) {
      return `Debe tener al menos ${control.errors['minlength'].requiredLength} caracteres`;
    }
    if (control.errors['maxlength']) {
      return `No puede exceder ${control.errors['maxlength'].requiredLength} caracteres`;
    }
    if (control.errors['pattern']) {
      if (field === 'nombre') {
        return 'El nombre no puede contener solo números';
      }
      if (field === 'telefono') {
        return 'Formato de teléfono inválido';
      }
    }

    return 'Campo inválido';
  }

  get kioscoName(): string {
    return this.businessForm.get('nombre')?.value || '';
  }

  get address(): string {
    return this.businessForm.get('direccion')?.value || '';
  }

  get phone(): string {
    return this.businessForm.get('telefono')?.value || '';
  }

  loadBusinessInfo(): void {
    if (this.isLoading()) return;

    this.isLoading.set(true);

    this.infoNegocioService
      .getBusinessInfo()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: (info) => {
          this.setBusinessInfo(info);
          this.originalBusinessInfo.set({ ...info });
        },
        error: (_) => {
          this.isLoading.set(false);
        },
      });
  }

  saveBusinessInfo(): void {
    if (!this.isFormValid() || this.isSubmitting()) {
      return;
    }

    this.isSubmitting.set(true);

    const formValues = this.businessForm.value;
    const businessInfo: KioscoBasicInfo = {
      nombre: formValues.nombre,
      direccion: formValues.direccion,
      telefono: formValues.telefono,
    };


    this.infoNegocioService
      .saveBusinessInfo(businessInfo)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isSubmitting.set(false))
      )
      .subscribe({
        next: (savedInfo) => {
          this.businessInfo.set(savedInfo);
          this.originalBusinessInfo.set({ ...savedInfo });
        },
        error: (_) => {
          this.isSubmitting.set(false);
        },
      });
  }

  cancelChanges(): void {
    const original = this.originalBusinessInfo();

    this.businessForm.patchValue(
      {
        nombre: original.nombre,
        direccion: original.direccion || '',
        telefono: original.telefono || '',
      },
      { emitEvent: true }
    );

    this.businessForm.markAsPristine();
    this.emitFormState();
  }

  hasChanges(): boolean {
    if (!this.businessForm || !this.originalBusinessInfo()) return false;

    const original = this.originalBusinessInfo();
    const current = this.businessForm.value;

    return (
      current.nombre !== original.nombre ||
      current.direccion !== original.direccion ||
      current.telefono !== original.telefono
    );
  }

  private setBusinessInfo(info: Partial<KioscoBasicInfo>): void {
    const currentInfo = this.businessInfo();
    this.businessInfo.set({
      ...currentInfo,
      ...info,
    });

    this.businessForm.patchValue(
      {
        nombre: info.nombre || '',
        direccion: info.direccion || '',
        telefono: info.telefono || '',
      },
      { emitEvent: true }
    );

    this.businessForm.markAsPristine();
    this.emitFormState();
  }

  get isLoadingData(): boolean {
    return this.isLoading();
  }

  get isBusy(): boolean {
    return this.isSubmitting() || this.isLoading();
  }
}
