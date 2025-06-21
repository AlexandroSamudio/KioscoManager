// Interfaz para los productos dentro de una venta
export interface ProductoVentaDto {
  productoId: number;
  cantidad: number;
}

// Interfaz para crear una venta
export interface VentaCreate {
  productos: ProductoVentaDto[];
}

// Interfaz para el ítem en el carrito
export interface CartItem {
  id: number;
  sku: string;
  nombre: string;
  precioVenta: number;
  cantidad: number;
  stock: number;
  categoriaId: number;
  categoriaNombre: string;
}
