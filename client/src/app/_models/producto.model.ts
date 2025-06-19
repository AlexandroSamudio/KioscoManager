export interface Producto {
  id: number;
  sku: string;
  nombre: string;
  descripcion: string;
  precioCompra: number;
  precioVenta: number;
  stock: number;
  categoriaNombre: string;
  categoriaId: number;
}

export interface ProductoCreate extends Omit<Producto, 'id' | 'categoriaNombre'> {
  categoriaId: number;
}
