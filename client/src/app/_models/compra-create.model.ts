export interface CompraDetalleCreate {
  productoId: number;
  cantidad: number;
  costoUnitario: number;
  productoSku?: string;
}

export interface CompraCreate {
  detalles: CompraDetalleCreate[];
  proveedor?: string;
  nota?: string;
}
