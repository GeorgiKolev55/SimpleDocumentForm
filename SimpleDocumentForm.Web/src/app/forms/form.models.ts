export type FieldType = 'user' | 'calculated';

/** Invalid forms (for example with a circular dependency) are listed but cannot be calculated. */
export type FormStatus = 'valid' | 'invalid';

export interface FormSummary {
  id: number;
  name: string;
  status: FormStatus;
  /** Only present for invalid forms: why the form cannot be calculated. */
  error?: string;
}

export interface FormField {
  name: string;
  type: FieldType;
  /** Only present for calculated fields: the two fields whose values are summed. */
  operands?: string[];
}

export interface DocumentForm {
  id: number;
  name: string;
  status: FormStatus;
  fields: FormField[];
  /** Only present for invalid forms: why the form cannot be calculated. */
  error?: string;
  /** Only present for circular dependencies: the fields forming the cycle, e.g. ["A", "C", "A"]. */
  cycle?: string[];
}

export interface CompleteFormRequest {
  values: Record<string, number>;
}

export interface CompletedField {
  name: string;
  type: FieldType;
  value: number;
}

export interface CompletedForm {
  id: number;
  name: string;
  fields: CompletedField[];
}
