export interface TipoReporte {
  id: 'ventas' | 'productos' | 'compras';
  nombre: string;
  descripcion: string;
  icono: string;
}

export interface FiltrosFecha {
  fechaInicio?: Date;
  fechaFin?: Date;
}

export interface ConfiguracionExportacion {
  tipoReporte: TipoReporte;
  filtros: FiltrosFecha;
  formato: 'excel' | 'pdf';
}
