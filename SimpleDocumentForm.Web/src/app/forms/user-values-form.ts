import { AbstractControl, FormControl, FormRecord, ValidationErrors } from '@angular/forms';

import { DocumentForm } from './form.models';

const NUMBER_PATTERN = /^[-+]?(\d+(\.\d*)?|\.\d+)$/;

/** Slightly below the .NET decimal range (±7.9228e28) used by the API. */
const MAX_ABSOLUTE_VALUE = 7.9e28;

export type UserValuesForm = FormRecord<FormControl<string>>;

/** Creates a form control for every user field of the document form. */
export function createUserValuesForm(form: DocumentForm): UserValuesForm {
  const valuesForm: UserValuesForm = new FormRecord<FormControl<string>>({});

  for (const field of form.fields) {
    if (field.type === 'user') {
      valuesForm.addControl(field.name, new FormControl('', { nonNullable: true, validators: [numericValue] }));
    }
  }

  return valuesForm;
}

/** Converts the entered text values to numbers. Call only when the form is valid. */
export function toUserValues(valuesForm: UserValuesForm): Record<string, number> {
  return Object.fromEntries(
    Object.entries(valuesForm.getRawValue()).map(([name, value]) => [name, Number(value.trim())]),
  );
}

/** Returns user-friendly messages for the client-side validation errors of a control. */
export function validationMessages(control: AbstractControl | null): string[] {
  if (!control?.errors || !control.touched) {
    return [];
  }

  if (control.hasError('required')) {
    return ['A value is required.'];
  }

  if (control.hasError('number')) {
    return ['Enter a valid number, for example 12, -3 or 4.75.'];
  }

  if (control.hasError('range')) {
    return ['The value is too large.'];
  }

  return [];
}

function numericValue(control: AbstractControl<string>): ValidationErrors | null {
  const value = control.value.trim();

  if (value === '') {
    return { required: true };
  }

  if (!NUMBER_PATTERN.test(value)) {
    return { number: true };
  }

  if (Math.abs(Number(value)) >= MAX_ABSOLUTE_VALUE) {
    return { range: true };
  }

  return null;
}
