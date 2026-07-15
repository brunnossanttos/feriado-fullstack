export interface Feriado {
  id: string;
  date: string | null;
  title: string;
  description: string | null;
  legislation: string | null;
  type: string | null;
  startTime: string | null;
  endTime: string | null;
  variableDates: Record<string, string>;
}

export interface CreateFeriadoRequest {
  title: string;
  date?: string | null;
  description?: string | null;
  legislation?: string | null;
  type?: string | null;
  startTime?: string | null;
  endTime?: string | null;
  variableDates?: Record<string, string>;
}

export type UpdateFeriadoRequest = CreateFeriadoRequest;

export interface FeriadoQuery {
  page?: number;
  pageSize?: number;
  title?: string;
  date?: string;
}
