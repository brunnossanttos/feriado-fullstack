import { HttpErrorResponse } from '@angular/common/http';
import { extractErrorMessage } from './problem-details';

describe('extractErrorMessage', () => {
  it('400 com errors: junta as mensagens de validação', () => {
    const err = new HttpErrorResponse({
      status: 400,
      error: { errors: { Title: ['O título é obrigatório.'] } },
    });

    expect(extractErrorMessage(err)).toContain('O título é obrigatório.');
  });

  it('usa o detail do ProblemDetails quando presente (ex.: 409)', () => {
    const err = new HttpErrorResponse({
      status: 409,
      error: { detail: 'Já existe um feriado com o título X.' },
    });

    expect(extractErrorMessage(err)).toBe('Já existe um feriado com o título X.');
  });

  it('404 sem corpo → mensagem de não encontrado', () => {
    const err = new HttpErrorResponse({ status: 404, error: null });

    expect(extractErrorMessage(err)).toBe('Registro não encontrado.');
  });

  it('status 0 → mensagem de sem conexão', () => {
    const err = new HttpErrorResponse({ status: 0, error: null });

    expect(extractErrorMessage(err)).toContain('conectar');
  });

  it('usa o fallback informado quando não reconhece o erro', () => {
    const err = new HttpErrorResponse({ status: 500, error: null });

    expect(extractErrorMessage(err, 'Falhou.')).toBe('Falhou.');
  });
});
