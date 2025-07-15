import {
  Component,
  DestroyRef,
  EventEmitter,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { CategoriaService } from '../../../../_services/categoria.service';
import { Categoria } from '../../../../_models/categoria.model';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-categoria-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './categoria-form.component.html',
  styleUrls: ['./categoria-form.component.css'],
})
export class CategoriaFormComponent implements OnInit, OnChanges {
  private categoriaService = inject(CategoriaService);
  private destroyRef = inject(DestroyRef);
  private fb = inject(FormBuilder);

  @Input() categoria: Categoria | null = null;
  @Output() categoriaSaved = new EventEmitter<Categoria>();
  @Output() formCancelled = new EventEmitter<void>();

  isEditMode = signal<boolean>(false);
  categoriaForm: FormGroup;
  isSubmitting = signal<boolean>(false);

  constructor() {
    this.categoriaForm = this.fb.group({
      nombre: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(50),
          Validators.pattern(/^(?!\s*$)(?!.*\d).*$/),
        ],
      ],
    });
  }

  ngOnInit(): void {
    this.updateFormForEditMode();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['categoria']) {
      this.updateFormForEditMode();
    }
  }

  private updateFormForEditMode(): void {
    this.isEditMode.set(!!this.categoria);
    if (this.categoria) {
      this.categoriaForm.patchValue({
        nombre: this.categoria.nombre,
      });
    }
  }

  onSubmit(): void {
    if (this.categoriaForm.invalid || this.isSubmitting()) {
      this.categoriaForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);

    const formValue = this.categoriaForm.value;

    if (this.isEditMode() && this.categoria) {
      this.categoriaService
        .updateCategoria(this.categoria.id, { nombre: formValue.nombre })
        .pipe(
          takeUntilDestroyed(this.destroyRef),
          finalize(() => this.isSubmitting.set(false))
        )
        .subscribe({
          next: (categoria: Categoria) => {
            this.categoriaSaved.emit(categoria);
            this.resetForm();
          }
        });
    } else {
      this.categoriaService
        .createCategoria({ nombre: formValue.nombre })
        .pipe(
          takeUntilDestroyed(this.destroyRef),
          finalize(() => this.isSubmitting.set(false))
        )
        .subscribe({
          next: (categoria: Categoria) => {
            this.categoriaSaved.emit(categoria);
            this.resetForm();
          }
        });
    }
  }

  resetForm(): void {
    this.categoriaForm.reset();
    if (this.isEditMode()) {
      this.isEditMode.set(false);
    }
  }

  onCancel(): void {
    this.resetForm();
    this.formCancelled.emit();
  }

  hasFieldError(fieldName: string): boolean {
    const field = this.categoriaForm.get(fieldName);
    return field ? field.invalid && (field.dirty || field.touched) : false;
  }

  getFieldError(fieldName: string): string {
    const field = this.categoriaForm.get(fieldName);

    if (!field || !field.errors || !(field.dirty || field.touched)) {
      return '';
    }

    if (field.errors['required']) {
      return 'Este campo es obligatorio.';
    }
    if (field.errors['minlength']) {
      return `Debe tener al menos ${field.errors['minlength'].requiredLength} caracteres.`;
    }
    if (field.errors['maxlength']) {
      return `No puede exceder ${field.errors['maxlength'].requiredLength} caracteres.`;
    }
    if (field.errors['pattern']) {
      return 'El nombre no puede contener números.';
    }

    return 'Campo inválido.';
  }
}
