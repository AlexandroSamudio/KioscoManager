import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../environments/environment.development';

@Injectable({
  providedIn: 'root'
})
export class VentaService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  getTotalVentasDia() {
    return this.http.get(`${this.baseUrl}ventas/total-dia`);
  }

  getVentasRecientes(){
    return this.http.get(`${this.baseUrl}ventas/recientes`);
  }
}
