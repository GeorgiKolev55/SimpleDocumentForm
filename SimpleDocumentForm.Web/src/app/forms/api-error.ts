import { HttpErrorResponse } from '@angular/common/http';

export type ApiErrorKind = 'network' | 'notFound' | 'validation' | 'conflict' | 'server' | 'unknown';

/** A user-friendly representation of a failed API call. */
export interface ApiError {
  kind: ApiErrorKind;
  message: string;
  /** Validation messages keyed by form field name (e.g. "B"). */
  fieldErrors: Record<string, string[]>;
  /** Validation messages that do not belong to a specific form field. */
  generalErrors: string[];
}

interface ProblemDetails {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

const VALUES_KEY_PATTERN = /^(?:\$\.)?values\.(.+)$/i;

export function toApiError(error: unknown): ApiError {
  if (!(error instanceof HttpErrorResponse)) {
    return createError('unknown', 'An unexpected error occurred. Please try again.');
  }

  const problem = asProblemDetails(error.error);

  if (error.status === 0) {
    return createError(
      'network',
      'Unable to reach the server. Please check your connection and make sure the API is running.',
    );
  }

  if (error.status === 404) {
    return createError('notFound', problem?.detail ?? 'The requested resource was not found.');
  }

  if (error.status === 400 || error.status === 422) {
    return toValidationError(problem);
  }

  if (error.status === 409) {
    return createError('conflict', problem?.detail ?? 'This form cannot be calculated.');
  }

  // A 5xx without a problem details body comes from a proxy/gateway that could not reach the API.
  const unavailable = [502, 503, 504].includes(error.status) || (error.status >= 500 && !problem);
  if (unavailable) {
    return createError('server', 'The server is currently unavailable. Please try again later.');
  }

  if (error.status >= 500) {
    return createError('server', 'The server could not process the request. Please try again later.');
  }

  return createError(
    'unknown',
    problem?.detail ?? problem?.title ?? `The request failed (HTTP ${error.status}). Please try again.`,
  );
}

function toValidationError(problem: ProblemDetails | undefined): ApiError {
  const apiError = createError('validation', problem?.detail ?? 'Please correct the highlighted values and try again.');

  for (const [key, messages] of Object.entries(problem?.errors ?? {})) {
    const fieldName = VALUES_KEY_PATTERN.exec(key)?.[1];
    if (fieldName) {
      apiError.fieldErrors[fieldName] = [...(apiError.fieldErrors[fieldName] ?? []), ...messages];
    } else {
      apiError.generalErrors.push(...messages);
    }
  }

  return apiError;
}

function createError(kind: ApiErrorKind, message: string): ApiError {
  return { kind, message, fieldErrors: {}, generalErrors: [] };
}

export function notFoundError(message: string): ApiError {
  return createError('notFound', message);
}

function asProblemDetails(body: unknown): ProblemDetails | undefined {
  return body !== null && typeof body === 'object' ? (body as ProblemDetails) : undefined;
}
