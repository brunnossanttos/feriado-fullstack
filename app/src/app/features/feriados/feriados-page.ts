import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { extractErrorMessage } from '../../core/http/problem-details';
import { Feriado } from '../../core/models/feriado.model';
import { PagedResult } from '../../core/models/paged-result.model';
import { FeriadoService } from '../../core/services/feriado.service';
import { FeriadoForm } from './feriado-form';

@Component({
  selector: 'app-feriados-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, FeriadoForm],
  templateUrl: './feriados-page.html',
  styleUrl: './feriados-page.scss',
})
export class FeriadosPage {
  private readonly service = inject(FeriadoService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly fb = inject(FormBuilder);

  private readonly pageSize = 5;

  readonly page = signal(1);
  readonly result = signal<PagedResult<Feriado> | null>(null);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly feedback = signal<string | null>(null);

  readonly pendingDelete = signal<Feriado | null>(null);
  readonly deleting = signal(false);

  readonly showForm = signal(false);
  readonly editing = signal<Feriado | null>(null);

  readonly filterForm = this.fb.nonNullable.group({
    title: '',
    date: '',
  });

  constructor() {
    this.filterForm.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged((a, b) => a.title === b.title && a.date === b.date),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => {
        this.page.set(1);
        this.load();
      });

    this.load();
  }

  goToPage(page: number): void {
    if (page < 1) {
      return;
    }
    this.page.set(page);
    this.load();
  }

  clearFilters(): void {
    this.filterForm.reset({ title: '', date: '' });
  }

  reload(): void {
    this.load();
  }

  openCreate(): void {
    this.feedback.set(null);
    this.editing.set(null);
    this.showForm.set(true);
  }

  openEdit(feriado: Feriado): void {
    this.feedback.set(null);
    this.editing.set(feriado);
    this.showForm.set(true);
  }

  onFormSaved(): void {
    const message = this.editing() ? 'Feriado atualizado.' : 'Feriado criado.';
    this.showForm.set(false);
    this.editing.set(null);
    this.feedback.set(message);
    this.load();
  }

  onFormCancelled(): void {
    this.showForm.set(false);
    this.editing.set(null);
  }

  askDelete(feriado: Feriado): void {
    this.feedback.set(null);
    this.pendingDelete.set(feriado);
  }

  cancelDelete(): void {
    this.pendingDelete.set(null);
  }

  confirmDelete(): void {
    const target = this.pendingDelete();
    if (!target) {
      return;
    }

    this.deleting.set(true);
    this.service
      .delete(target.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.afterDelete(`Feriado "${target.title}" excluído.`),
        error: (err: HttpErrorResponse) => {
          if (err.status === 404) {
            this.afterDelete(`Feriado "${target.title}" já havia sido removido.`);
          } else {
            this.deleting.set(false);
            this.pendingDelete.set(null);
            this.feedback.set(extractErrorMessage(err, 'Não foi possível excluir o feriado.'));
          }
        },
      });
  }

  private afterDelete(message: string): void {
    this.deleting.set(false);
    this.pendingDelete.set(null);
    this.feedback.set(message);

    const current = this.result();
    if (current && current.items.length === 1 && this.page() > 1) {
      this.page.set(this.page() - 1);
    }

    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.error.set(null);

    const { title, date } = this.filterForm.getRawValue();

    this.service
      .list({
        page: this.page(),
        pageSize: this.pageSize,
        title: title || undefined,
        date: date || undefined,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.result.set(result);
          this.loading.set(false);
        },
        error: (err: HttpErrorResponse) => {
          this.error.set(extractErrorMessage(err, 'Não foi possível carregar os feriados.'));
          this.loading.set(false);
        },
      });
  }
}
