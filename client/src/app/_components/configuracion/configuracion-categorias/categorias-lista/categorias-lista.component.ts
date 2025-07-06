import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CategoriaService } from '../../../../_services/categoria.service';
import { Categoria } from '../../../../_models/categoria.model';
import { Pagination } from '../../../../_services/pagination.helper';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { CategoriaFormComponent } from '../categoria-form/categoria-form.component';
import { NotificationService } from '../../../../_services/notification.service';

@Component({
  selector: 'app-categorias-lista',
  standalone: true,
  imports: [CommonModule, FormsModule, CategoriaFormComponent],
  templateUrl: './categorias-lista.component.html',
  styleUrls: ['./categorias-lista.component.css'],
})
export class CategoriasListaComponent implements OnInit {
  private categoriaService = inject(CategoriaService);
  private destroyRef = inject(DestroyRef);
  private notificationService = inject(NotificationService);

  protected Math = Math;

  categorias = signal<Categoria[]>([]);
  pagination = signal<Pagination | null>(null);
  isLoading = signal<boolean>(false);
  showForm = signal<boolean>(false);
  successMessage = signal<string | null>(null);
  categoriaToEdit = signal<Categoria | null>(null);
  isDeletingCategoria = signal<boolean>(false);

  currentPage = signal<number>(1);
  pageSize = signal<number>(5);
  pages = computed<number[]>(() => {
    const totalPages = this.pagination()?.totalPages || 1;
    const currentPage = this.pagination()?.currentPage || 1;

    const pages: number[] = [];

    pages.push(1);

    const startPage = Math.max(2, currentPage - 1);
    const endPage = Math.min(totalPages - 1, currentPage + 1);

    if (startPage > 2) {
      pages.push(-1);
    }

    for (let i = startPage; i <= endPage; i++) {
      if (i !== 1 && i !== totalPages) {
        pages.push(i);
      }
    }

    if (endPage < totalPages - 1) {
      pages.push(-1);
    }

    if (totalPages > 1) {
      pages.push(totalPages);
    }

    return pages;
  });

  ngOnInit(): void {
    this.loadCategorias();
  }

  loadCategorias(): void {
    this.isLoading.set(true);

    this.categoriaService
      .getCategoriasConPaginacion(
        this.currentPage(),
        this.pageSize()
      )
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: (result) => {
          this.categorias.set(result.result);
          this.pagination.set(result.pagination);
        },
        error: (error) => {
          console.error('Error al cargar categorías', error);
          this.notificationService.error(
            'Error al cargar categorías',
            'Ocurrió un error al intentar cargar las categorías. Por favor, inténtalo de nuevo más tarde.'
          );
        },
      });
  }

  onPageChange(page: number): void {
    this.currentPage.set(page);
    this.loadCategorias();
  }

  onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.currentPage.set(1);
    this.loadCategorias();
  }


  toggleForm(): void {
    this.categoriaToEdit.set(null);
    this.showForm.update((current) => !current);
  }

  editCategoria(categoria: Categoria): void {
    this.categoriaToEdit.set(categoria);
    this.showForm.set(true);
  }

  handleCategoriaSaved(): void {
    const wasEditing = !!this.categoriaToEdit();
    this.loadCategorias();
    this.showForm.set(false);
    this.categoriaToEdit.set(null);
    this.notificationService.showSuccess(
      `Categoría ${wasEditing ? 'actualizada' : 'guardada'} exitosamente.`
    );
  }

  handleFormCancelled(): void {
    this.showForm.set(false);
    this.categoriaToEdit.set(null);
  }

  async confirmDeleteCategoria(categoria: Categoria): Promise<void> {
    const confirmed = await this.notificationService.confirm(
      'Confirmar eliminación',
      `¿Estás seguro que deseas eliminar la categoría? Esta acción no se puede deshacer.`
    );

    if (confirmed) {
      this.deleteCategoria(categoria);
    }
  }

  private deleteCategoria(categoria: Categoria): void {
    if (this.isDeletingCategoria()) return;

    this.isDeletingCategoria.set(true);

    this.categoriaService
      .deleteCategoria(categoria.id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isDeletingCategoria.set(false);
        })
      )
      .subscribe({
        next: () => {
          this.loadCategorias();
          this.notificationService.showSuccess(
            `Categoría "${categoria.nombre}" eliminada exitosamente.`
          );
        },
        error: (error) => {
          console.error('Error al eliminar categoría:', error);
          if (error.status === 400) {
            this.notificationService.error(
              'No se puede eliminar la categoría',
              'Esta categoría está siendo utilizada por productos existentes.'
            );
          } else {
            this.notificationService.error(
              'Error',
              'Ocurrió un error al eliminar la categoría. Por favor, inténtalo de nuevo.'
            );
          }
        },
      });
  }
}
