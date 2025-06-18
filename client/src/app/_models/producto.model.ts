export interface Producto {
  id: number;
  sku: string;
  nombre: string;
  descripcion?: string;
  precioCompra: number;
  precioVenta: number;
  stock: number;
  categoriaNombre?: string;
}
