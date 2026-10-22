import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';

import { toApiError } from './api-error';
import { CompletedForm, CompleteFormRequest, DocumentForm, FormSummary } from './form.models';

/** Client for the document form API. Failed calls error with an {@link ApiError}. */
@Injectable({ providedIn: 'root' })
export class FormsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/forms';

  getForms(): Observable<FormSummary[]> {
    return this.http.get<FormSummary[]>(this.baseUrl).pipe(mapApiError());
  }

  getForm(formId: number): Observable<DocumentForm> {
    return this.http.get<DocumentForm>(`${this.baseUrl}/${formId}`).pipe(mapApiError());
  }

  completeForm(formId: number, values: Record<string, number>): Observable<CompletedForm> {
    const request: CompleteFormRequest = { values };
    return this.http
      .post<CompletedForm>(`${this.baseUrl}/${formId}/completions`, request)
      .pipe(mapApiError());
  }
}

function mapApiError<T>() {
  return catchError<T, Observable<never>>((error: unknown) => throwError(() => toApiError(error)));
}
