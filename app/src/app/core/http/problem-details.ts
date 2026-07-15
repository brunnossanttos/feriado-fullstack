import { HttpErrorResponse } from '@angular/common/http';

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
}

export function extractErrorMessage(
  err: HttpErrorResponse,
  fallback = 'Ocorreu um erro inesperado.',
): string {
  const problem = err.error as ProblemDetails | undefined;

  if (err.status === 400 && problem?.errors) {
    const messages = Object.values(problem.errors).flat();
    if (messages.length > 0) {
      return messages.join(' ');
    }
  }

  if (problem?.detail) {
    return problem.detail;
  }

  if (err.status === 404) {
    return 'Registro não encontrado.';
  }

  if (err.status === 0) {
    return 'Não foi possível conectar ao servidor.';
  }

  return fallback;
}
