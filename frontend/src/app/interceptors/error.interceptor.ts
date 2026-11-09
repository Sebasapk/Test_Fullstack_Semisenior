import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { EMPTY, catchError } from 'rxjs';

import { ErrorMessageService } from '../services/error-message.service';

interface ProblemDetails {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

function toMessage(error: HttpErrorResponse): string {
  if (error.status === 0) {
    return 'Cannot reach the server. Please try again later.';
  }

  const problem = error.error as ProblemDetails | null;

  // Model validation failures list one or more messages per field.
  const validationMessages = Object.values(problem?.errors ?? {}).flat();
  if (validationMessages.length > 0) {
    return validationMessages.join(' ');
  }

  return problem?.detail ?? problem?.title ?? 'An unexpected error occurred.';
}

export const errorInterceptor: HttpInterceptorFn = (request, next) => {
  const errorMessages = inject(ErrorMessageService);
  errorMessages.clear();

  return next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      errorMessages.show(toMessage(error));
      // The banner is the only place errors are handled, so rethrowing would just log an unhandled error in the console.
      return EMPTY;
    })
  );
};
