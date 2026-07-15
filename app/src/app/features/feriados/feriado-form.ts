import { HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  effect,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormBuilder,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { extractErrorMessage } from '../../core/http/problem-details';
import { CreateFeriadoRequest, Feriado } from '../../core/models/feriado.model';
import { FeriadoService } from '../../core/services/feriado.service';

type VariableDateForm = FormGroup<{ year: FormControl<string>; date: FormControl<string> }>;

@Component({
  selector: 'app-feriado-form',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule],
  templateUrl: './feriado-form.html',
  styleUrl: './feriado-form.scss',
})
export class FeriadoForm {
  private readonly service = inject(FeriadoService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly fb = inject(FormBuilder);

  readonly feriado = input<Feriado | null>(null);
  readonly saved = output<void>();
  readonly cancelled = output<void>();

  readonly saving = signal(false);
  readonly serverError = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    date: ['', [Validators.pattern(/^(0[1-9]|[12][0-9]|3[01])\/(0[1-9]|1[0-2])$/)]],
    description: [''],
    legislation: [''],
    type: ['', [Validators.maxLength(50)]],
    startTime: ['', [Validators.maxLength(10)]],
    endTime: ['', [Validators.maxLength(10)]],
  });

  readonly variableDates = this.fb.array<VariableDateForm>([]);

  constructor() {
    effect(() => {
      const current = this.feriado();
      if (!current) {
        return;
      }

      this.form.patchValue({
        title: current.title,
        date: current.date ?? '',
        description: current.description ?? '',
        legislation: current.legislation ?? '',
        type: current.type ?? '',
        startTime: current.startTime ?? '',
        endTime: current.endTime ?? '',
      });

      this.variableDates.clear();
      for (const [year, date] of Object.entries(current.variableDates ?? {})) {
        this.addVariableDate(year, date);
      }
    });
  }

  get isEditing(): boolean {
    return this.feriado() !== null;
  }

  addVariableDate(year = '', date = ''): void {
    this.variableDates.push(
      this.fb.nonNullable.group({
        year: [year, Validators.required],
        date: [date, Validators.required],
      }),
    );
  }

  removeVariableDate(index: number): void {
    this.variableDates.removeAt(index);
  }

  cancel(): void {
    this.cancelled.emit();
  }

  submit(): void {
    if (this.form.invalid || this.variableDates.invalid) {
      this.form.markAllAsTouched();
      this.variableDates.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.serverError.set(null);

    const raw = this.form.getRawValue();
    const request: CreateFeriadoRequest = {
      title: raw.title.trim(),
      date: raw.date || null,
      description: raw.description || null,
      legislation: raw.legislation || null,
      type: raw.type || null,
      startTime: raw.startTime || null,
      endTime: raw.endTime || null,
      variableDates: this.buildVariableDates(),
    };

    const editing = this.feriado();
    const operation = editing
      ? this.service.update(editing.id, request)
      : this.service.create(request);

    operation.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.saving.set(false);
        this.saved.emit();
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.serverError.set(extractErrorMessage(err, 'Não foi possível salvar o feriado.'));
      },
    });
  }

  private buildVariableDates(): Record<string, string> {
    const result: Record<string, string> = {};
    for (const group of this.variableDates.controls) {
      const { year, date } = group.getRawValue();
      const y = year.trim();
      const d = date.trim();
      if (y && d) {
        result[y] = d;
      }
    }
    return result;
  }
}
