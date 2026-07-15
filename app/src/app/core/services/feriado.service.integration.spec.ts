import { provideHttpClient, withFetch } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';
import { API_BASE_URL } from '../http/api.tokens';
import { FeriadoService } from './feriado.service';

const API_URL = 'http://localhost:5010/api';

describe('FeriadoService (integração com a API real)', () => {
  let service: FeriadoService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withFetch()),
        { provide: API_BASE_URL, useValue: API_URL },
      ],
    });
    service = TestBed.inject(FeriadoService);
  });

  it('faz o CRUD completo contra a API e o Postgres reais', async () => {
    const title = `APP-IT-${crypto.randomUUID()}`;

    const created = await firstValueFrom(
      service.create({ title, date: '09/07', variableDates: { '2026': '09/07' } }),
    );
    expect(created.id).toBeTruthy();
    expect(created.title).toBe(title);

    const fetched = await firstValueFrom(service.getById(created.id));
    expect(fetched.date).toBe('09/07');
    expect(fetched.variableDates['2026']).toBe('09/07');

    const listed = await firstValueFrom(service.list({ title, pageSize: 5 }));
    expect(listed.totalCount).toBeGreaterThanOrEqual(1);
    expect(listed.items.some((f) => f.id === created.id)).toBe(true);

    const updated = await firstValueFrom(service.update(created.id, { title, date: '10/07' }));
    expect(updated.date).toBe('10/07');

    await firstValueFrom(service.delete(created.id));

    await expect(firstValueFrom(service.getById(created.id))).rejects.toMatchObject({
      status: 404,
    });
  });

  it('rejeita título duplicado com 409', async () => {
    const title = `APP-IT-DUP-${crypto.randomUUID()}`;

    const created = await firstValueFrom(service.create({ title }));

    await expect(firstValueFrom(service.create({ title }))).rejects.toMatchObject({
      status: 409,
    });

    await firstValueFrom(service.delete(created.id));
  });
});
