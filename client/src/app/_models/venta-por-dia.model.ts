export interface VentaPorDia {
  fecha: Date;
  totalVentas: number;
  tipoAgrupacion?: 'daily' | 'weekly' | 'monthly';
}

export type TipoAgrupacion = 'daily' | 'weekly' | 'monthly';

export interface VentaPorDiaResponse {
  fecha: string | Date;
  totalVentas: number;
  tipoAgrupacion?: TipoAgrupacion;
}
