import { Component, computed, DestroyRef, effect, inject, input, signal, untracked } from '@angular/core';
import { FormControl, FormRecord, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';

import { ApiError, notFoundError } from '../api-error';
import { DocumentForm } from '../form.models';
import { FormsApi } from '../forms-api';
import {
  createUserValuesForm,
  toUserValues,
  UserValuesForm,
  validationMessages,
} from '../user-values-form';

@Component({
  selector: 'app-form-detail',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './form-detail.html',
})
export class FormDetail {
  private readonly formsApi = inject(FormsApi);

  /** Form id, bound from the `:id` route parameter. */
  readonly id = input.required<string>();

  protected readonly form = signal<DocumentForm | null>(null);
  protected readonly loading = signal(false);
  protected readonly loadError = signal<ApiError | null>(null);
  /** Invalid forms (e.g. with a circular dependency) are shown but cannot be calculated. */
  protected readonly isInvalid = computed(() => this.form()?.status === 'invalid');

  protected readonly valuesForm = signal<UserValuesForm>(new FormRecord<FormControl<string>>({}));
  protected readonly showValidationSummary = signal(false);
  protected readonly calculating = signal(false);
  protected readonly calculationError = signal<ApiError | null>(null);
  protected readonly calculatedValues = signal<Record<string, number> | null>(null);

  private loadSubscription?: Subscription;
  private calculateSubscription?: Subscription;

  constructor() {
    effect(() => {
      const id = this.id();
      untracked(() => this.loadForm(id));
    });

    inject(DestroyRef).onDestroy(() => {
      this.loadSubscription?.unsubscribe();
      this.calculateSubscription?.unsubscribe();
    });
  }

  protected reload(): void {
    this.loadForm(this.id());
  }

  protected calculate(): void {
    const form = this.form();
    const valuesForm = this.valuesForm();
    if (!form || this.isInvalid() || this.calculating()) {
      return;
    }

    this.clearResults();
    valuesForm.markAllAsTouched();

    if (valuesForm.invalid) {
      this.showValidationSummary.set(true);
      return;
    }

    this.calculating.set(true);
    this.calculateSubscription = this.formsApi.completeForm(form.id, toUserValues(valuesForm)).subscribe({
      next: (completedForm) => {
        this.calculatedValues.set(
          Object.fromEntries(completedForm.fields.map((field) => [field.name, field.value])),
        );
        this.calculating.set(false);
      },
      error: (error: ApiError) => {
        this.calculationError.set(error);
        this.calculating.set(false);
      },
    });
  }

  protected clear(): void {
    this.valuesForm().reset();
    this.clearResults();
  }

  /** Previous results and errors no longer match the input once a value changes. */
  protected onValueChanged(): void {
    this.clearResults();
  }

  protected fieldErrors(fieldName: string): string[] {
    return [
      ...validationMessages(this.valuesForm().get(fieldName)),
      ...(this.calculationError()?.fieldErrors[fieldName] ?? []),
    ];
  }

  protected calculatedValue(fieldName: string): number | undefined {
    return this.calculatedValues()?.[fieldName];
  }

  protected isInCycle(fieldName: string): boolean {
    return this.form()?.cycle?.includes(fieldName) ?? false;
  }

  private loadForm(id: string): void {
    this.loadSubscription?.unsubscribe();
    this.calculateSubscription?.unsubscribe();
    this.form.set(null);
    this.loadError.set(null);
    this.calculating.set(false);
    this.clearResults();

    if (!/^\d+$/.test(id)) {
      this.loadError.set(notFoundError(`Form with id '${id}' does not exist.`));
      return;
    }

    this.loading.set(true);
    this.loadSubscription = this.formsApi.getForm(Number(id)).subscribe({
      next: (form) => {
        const valuesForm = createUserValuesForm(form);
        if (form.status === 'invalid') {
          valuesForm.disable();
        }

        this.valuesForm.set(valuesForm);
        this.form.set(form);
        this.loading.set(false);
      },
      error: (error: ApiError) => {
        this.loadError.set(error);
        this.loading.set(false);
      },
    });
  }

  private clearResults(): void {
    this.showValidationSummary.set(false);
    this.calculationError.set(null);
    this.calculatedValues.set(null);
  }
}
