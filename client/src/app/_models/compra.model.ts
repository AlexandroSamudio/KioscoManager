export interface CompraDetalleView {
  id: number;
  productoId: number;
  productoNombre?: string;
  productoSku?: string;
  cantidad: number;
  costoUnitario: number;
  subtotal: number;
}

export interface Compra {
  id: number;
  fecha: Date;
  costoTotal: number;
  proveedor?: string;
  nota?: string;
  usuarioId: number;
  detalles: CompraDetalleView[];
}
