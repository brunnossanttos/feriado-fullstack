import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../http/api.tokens';
import {
  CreateFeriadoRequest,
  Feriado,
  FeriadoQuery,
  UpdateFeriadoRequest,
} from '../models/feriado.model';
import { PagedResult } from '../models/paged-result.model';

@Injectable({ providedIn: 'root' })
export class FeriadoService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${inject(API_BASE_URL)}/feriados`;

  list(query: FeriadoQuery | null = {}): Observable<PagedResult<Feriado>> {
    const { page, pageSize, title, date } = query ?? {};
    const trimmedTitle = title?.trim();
    const trimmedDate = date?.trim();

    const params = new HttpParams({
      fromObject: {
        ...(page != null && { page }),
        ...(pageSize != null && { pageSize }),
        ...(trimmedTitle && { title: trimmedTitle }),
        ...(trimmedDate && { date: trimmedDate }),
      },
    });

    return this.http.get<PagedResult<Feriado>>(this.baseUrl, { params });
  }

  getById(id: string): Observable<Feriado> {
    return this.http.get<Feriado>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateFeriadoRequest): Observable<Feriado> {
    return this.http.post<Feriado>(this.baseUrl, request);
  }

  update(id: string, request: UpdateFeriadoRequest): Observable<Feriado> {
    return this.http.put<Feriado>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
