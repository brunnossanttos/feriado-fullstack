import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

export const errorLoggingInterceptor: HttpInterceptorFn = (req, next) =>
  next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      console.error(`[HTTP] ${req.method} ${req.urlWithParams} falhou (${err.status}).`, err.error);
      return throwError(() => err);
    }),
  );
