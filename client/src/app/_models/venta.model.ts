export interface Venta {
  id: number;
  fecha: Date;
  total: number;
  nombreUsuario?: string;
  cantidadProductos: number;
}
