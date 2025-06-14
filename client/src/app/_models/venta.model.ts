export interface Venta {
  id: number;
  fecha: Date|string;
  total: number;
  nombreUsuario?: string;
  cantidadProductos: number;
}
