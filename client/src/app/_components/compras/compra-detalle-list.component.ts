import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CompraDetalleCreate } from '../../_models/compra-create.model';

@Component({
  selector: 'app-compra-detalle-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="bg-white rounded-xl shadow-lg border border-amber-200 p-6 mb-6 hover:shadow-xl transition-shadow">
      <h3 class="text-xl font-bold text-amber-800 mb-4">Productos en esta compra</h3>
      <div class="overflow-x-auto">
        <table class="min-w-full divide-y divide-amber-200">
          <thead class="bg-amber-50">
            <tr>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-amber-700 uppercase tracking-wider">
                #
              </th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-amber-700 uppercase tracking-wider">
                SKU
              </th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-amber-700 uppercase tracking-wider">
                Cantidad
              </th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-amber-700 uppercase tracking-wider">
                Costo Unitario
              </th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-amber-700 uppercase tracking-wider">
                Subtotal
              </th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-amber-700 uppercase tracking-wider">
                Acciones
              </th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-amber-100">
            <tr *ngIf="items.length === 0">
              <td colspan="6" class="px-6 py-4 text-center text-sm text-amber-600">
                No hay productos agregados a esta compra
              </td>
            </tr>
            <tr *ngFor="let item of items; let i = index">
              <td class="px-6 py-4 whitespace-nowrap text-sm text-amber-800">
                {{ i + 1 }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-amber-800">
                {{ item.productoSku || 'N/A' }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-amber-800">
                {{ item.cantidad }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-amber-800">
                {{ '$' + (item.costoUnitario | number:'1.2-2') }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-amber-800">
                {{ '$' + (item.cantidad * item.costoUnitario | number:'1.2-2') }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                <button
                  (click)="onEliminar(i)"
                  class="text-red-600 hover:text-red-800 focus:outline-none focus:ring-2 focus:ring-red-500 rounded-sm px-2 py-1"
                >
                  Eliminar
                </button>
              </td>
            </tr>
          </tbody>
          <tfoot *ngIf="items.length > 0" class="bg-amber-50">
            <tr>
              <td colspan="4" class="px-6 py-4 text-right font-bold text-amber-800">
                Total:
              </td>
              <td colspan="2" class="px-6 py-4 text-left font-bold text-amber-800">
                {{ '$' + (calcularTotal() | number:'1.2-2') }}
              </td>
            </tr>
          </tfoot>
        </table>
      </div>
    </div>
  `,
  styles: []
})
export class CompraDetalleListComponent {
  @Input() items: CompraDetalleCreate[] = [];
  @Output() eliminar = new EventEmitter<number>();

  onEliminar(index: number): void {
    this.eliminar.emit(index);
  }

  calcularTotal(): number {
    return this.items.reduce(
      (total, item) => total + (item.cantidad * item.costoUnitario),
      0
    );
  }
}
