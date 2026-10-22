import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';

import { ApiError } from '../api-error';
import { FormSummary } from '../form.models';
import { FormsApi } from '../forms-api';

@Component({
  selector: 'app-form-list',
  imports: [RouterLink],
  templateUrl: './form-list.html',
})
export class FormList implements OnInit {
  private readonly formsApi = inject(FormsApi);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly forms = signal<FormSummary[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<ApiError | null>(null);

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.formsApi
      .getForms()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (forms) => {
          this.forms.set(forms);
          this.loading.set(false);
        },
        error: (error: ApiError) => {
          this.error.set(error);
          this.loading.set(false);
        },
      });
  }
}
