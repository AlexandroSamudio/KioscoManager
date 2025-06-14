import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../environments/environment.development';

@Injectable({
  providedIn: 'root'
})
export class ProductoService {

  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  getProductos() {
    return this.http.get(`${this.baseUrl}productos`);
  }

  getProducto(id: number) {
    return this.http.get(`${this.baseUrl}productos/${id}`);
  }

  getProductosByLowestStock() {
    return this.http.get(`${this.baseUrl}productos/low-stock`);
  }

  getProductosMasVendidosDelDia() {
    return this.http.get(`${this.baseUrl}ventas/productos-mas-vendidos`);
  }


}
