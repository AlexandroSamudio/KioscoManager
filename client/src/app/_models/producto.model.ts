export interface Producto {
  id: number;
  nombre: string;
  descripcion?: string;
  precio: number;
  stock: number;
  categoriaNombre?: string;
}
