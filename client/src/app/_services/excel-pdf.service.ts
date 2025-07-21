import { inject, Injectable } from '@angular/core';
import { NotificationService } from './notification.service';

@Injectable({
  providedIn: 'root'
})
export class ExcelPdfService {

  private notificationService = inject(NotificationService);

  private escapeHtml(text: string | number | undefined | null): string {
    if (text === null || text === undefined) {
      return '';
    }

    const str = String(text);
    const entityMap: { [key: string]: string } = {
      '&': '&amp;',
      '<': '&lt;',
      '>': '&gt;',
      '"': '&quot;',
      "'": '&#x27;'
    };

    return str.replace(/[&<>"']/g, (char) => entityMap[char]);
  }

  exportarCsv(tipoReporte: 'ventas' | 'productos' | 'compras', data: any[]): void {
    this.notificationService.info('Preparando exportación...', 'Por favor espere');

    try {
      let csvContent = '';
      let headers: string[] = [];

      if (data.length === 0) {
        this.notificationService.warning('No hay datos para exportar', 'Lista vacía');
        return;
      }

      switch (tipoReporte) {
        case 'ventas':
          headers = ['Fecha', 'Total', 'Cantidad Productos'];
          break;
        case 'productos':
          headers = ['SKU', 'Nombre', 'Descripción', 'Precio Compra', 'Precio Venta', 'Stock', 'Categoría'];
          break;
        case 'compras':
          headers = ['Fecha', 'Costo Total', 'Proveedor', 'Nota', 'Cantidad Items'];
          break;
      }

      csvContent += headers.join(',') + '\n';

      data.forEach(item => {
        let row: string[] = [];

        switch (tipoReporte) {
          case 'ventas':
            row = [
              item.fecha ? new Date(item.fecha).toLocaleDateString() : '',
              (item.total || 0).toString(),
              (item.cantidadProductos || 0).toString()
            ];
            break;
          case 'productos':
            row = [
              item.sku || '',
              item.nombre || '',
              item.descripcion || '',
              item.precioCompra?.toString() || '0',
              item.precioVenta?.toString() || '0',
              item.stock?.toString() || '0',
              item.categoriaNombre || ''
            ];
            break;
          case 'compras':
            row = [
              item.fecha ? new Date(item.fecha).toLocaleDateString() : '',
              item.costoTotal?.toString() || '0',
              item.proveedor || '',
              item.nota || '',
              item.detalles?.length?.toString() || '0'
            ];
            break;
        }

        const escapedRow = row.map(field => {
          const fieldStr = String(field);
          if (fieldStr.includes(',') || fieldStr.includes('"') || fieldStr.includes('\n')) {
            return `"${fieldStr.replace(/"/g, '""')}"`;
          }
          return fieldStr;
        });

        csvContent += escapedRow.join(',') + '\n';
      });

      const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
      const link = document.createElement('a');
      const url = URL.createObjectURL(blob);
      link.setAttribute('href', url);
      link.setAttribute('download', `${tipoReporte}_${new Date().toISOString().split('T')[0]}.csv`);
      link.style.visibility = 'hidden';
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);

      URL.revokeObjectURL(url);

      this.notificationService.showSuccess('Archivo exportado exitosamente');
    } catch (error) {
      this.notificationService.error('Error al exportar archivo', 'Inténtelo de nuevo');
      console.error('Error en exportación:', error);
    }
  }

  exportarPDF(tipoReporte: 'ventas' | 'productos' | 'compras', data: any[]): void {
    this.notificationService.info('Preparando exportación PDF...', 'Por favor espere');

    try {
      if (data.length === 0) {
        this.notificationService.warning('No hay datos para exportar', 'Lista vacía');
        return;
      }

      let htmlContent = `
        <!DOCTYPE html>
        <html>
        <head>
          <meta charset="utf-8">
          <title>Reporte de ${this.escapeHtml(tipoReporte.charAt(0).toUpperCase() + tipoReporte.slice(1))}</title>
          <style>
            body { font-family: Arial, sans-serif; margin: 20px; }
            h1 { color: #d97706; text-align: center; }
            table { width: 100%; border-collapse: collapse; margin-top: 20px; }
            th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
            th { background-color: #fef3c7; }
            .header { margin-bottom: 20px; }
            .date { text-align: center; color: #666; }
          </style>
        </head>
        <body>
          <div class="header">
            <h1>Reporte de ${this.escapeHtml(tipoReporte.charAt(0).toUpperCase() + tipoReporte.slice(1))}</h1>
            <p class="date">Generado el: ${this.escapeHtml(new Date().toLocaleDateString())}</p>
          </div>
          <table>
            <thead>
              <tr>
      `;

      switch (tipoReporte) {
        case 'ventas':
          htmlContent += '<th>Fecha</th><th>Total</th><th>Cantidad Productos</th>';
          break;
        case 'productos':
          htmlContent += '<th>SKU</th><th>Nombre</th><th>Descripción</th><th>Precio Compra</th><th>Precio Venta</th><th>Stock</th><th>Categoría</th>';
          break;
        case 'compras':
          htmlContent += '<th>Fecha</th><th>Total</th><th>Proveedor</th><th>Cantidad Items</th>';
          break;
      }

      htmlContent += '</tr></thead><tbody>';

      data.forEach(item => {
        htmlContent += '<tr>';

        switch (tipoReporte) {
          case 'ventas':
            htmlContent += `
              <td>${this.escapeHtml(item.fecha ? new Date(item.fecha).toLocaleDateString() : '')}</td>
              <td>$${this.escapeHtml(item.total || 0)}</td>
              <td>${this.escapeHtml(item.cantidadProductos || 0)}</td>
            `;
            break;
          case 'productos':
            htmlContent += `
              <td>${this.escapeHtml(item.sku || '')}</td>
              <td>${this.escapeHtml(item.nombre || '')}</td>
              <td>${this.escapeHtml(item.descripcion || '')}</td>
              <td>$${this.escapeHtml(item.precioCompra || 0)}</td>
              <td>$${this.escapeHtml(item.precioVenta || 0)}</td>
              <td>${this.escapeHtml(item.stock || 0)}</td>
              <td>${this.escapeHtml(item.categoriaNombre || '')}</td>
            `;
            break;
          case 'compras':
            htmlContent += `
              <td>${this.escapeHtml(item.fecha ? new Date(item.fecha).toLocaleDateString() : '')}</td>
              <td>$${this.escapeHtml(item.costoTotal || 0)}</td>
              <td>${this.escapeHtml(item.proveedor || '')}</td>
              <td>${this.escapeHtml(item.detalles?.length || 0)}</td>
            `;
            break;
        }

        htmlContent += '</tr>';
      });

      htmlContent += `
            </tbody>
          </table>
        </body>
        </html>
      `;

      const printWindow = window.open('', '_blank');
      if (printWindow) {
        printWindow.document.write(htmlContent);
        printWindow.document.close();
        printWindow.focus();

        setTimeout(() => {
          printWindow.print();
          this.notificationService.showSuccess('PDF creado correctamente');
        }, 500);
      } else {
        throw new Error('No se pudo abrir la ventana de impresión');
      }

    } catch (error) {
      this.notificationService.error('Error al exportar PDF', 'Inténtelo de nuevo');
      console.error('Error en exportación PDF:', error);
    }
  }
}
