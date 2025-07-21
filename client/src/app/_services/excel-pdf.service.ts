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

  private getColumnsConfig(tipoReporte: 'ventas' | 'productos' | 'compras') {
    const configs = {
      ventas: {
        headers: ['Fecha', 'Total', 'Cantidad Productos'],
        extractData: (item: any) => [
          item.fecha ? new Date(item.fecha).toLocaleDateString() : '',
          (item.total || 0).toString(),
          (item.cantidadProductos || 0).toString()
        ]
      },
      productos: {
        headers: ['SKU', 'Nombre', 'Descripción', 'Precio Compra', 'Precio Venta', 'Stock', 'Categoría'],
        extractData: (item: any) => [
          item.sku || '',
          item.nombre || '',
          item.descripcion || '',
          item.precioCompra?.toString() || '0',
          item.precioVenta?.toString() || '0',
          item.stock?.toString() || '0',
          item.categoriaNombre || ''
        ]
      },
      compras: {
        headers: ['Fecha', 'Costo Total', 'Proveedor', 'Nota', 'Cantidad Items'],
        extractData: (item: any) => [
          item.fecha ? new Date(item.fecha).toLocaleDateString() : '',
          item.costoTotal?.toString() || '0',
          item.proveedor || '',
          item.nota || '',
          item.detalles?.length?.toString() || '0'
        ]
      }
    };

    return configs[tipoReporte];
  }

  private validateExportData(data: any[], tipoReporte: string): boolean {
    if (data.length === 0) {
      this.notificationService.warning('No hay datos para exportar', 'Lista vacía');
      return false;
    }
    return true;
  }

  private generateHtmlContent(tipoReporte: 'ventas' | 'productos' | 'compras', data: any[]): string {
    const config = this.getColumnsConfig(tipoReporte);
    const reportTitle = tipoReporte.charAt(0).toUpperCase() + tipoReporte.slice(1);

    let htmlContent = `
      <!DOCTYPE html>
      <html>
      <head>
        <meta charset="utf-8">
        <title>Reporte de ${this.escapeHtml(reportTitle)}</title>
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
          <h1>Reporte de ${this.escapeHtml(reportTitle)}</h1>
          <p class="date">Generado el: ${this.escapeHtml(new Date().toLocaleDateString())}</p>
        </div>
        <table>
          <thead>
            <tr>
    `;

    config.headers.forEach(header => {
      htmlContent += `<th>${this.escapeHtml(header)}</th>`;
    });

    htmlContent += '</tr></thead><tbody>';

    data.forEach(item => {
      htmlContent += '<tr>';
      const rowData = config.extractData(item);

      rowData.forEach((cell, index) => {
        let cellContent = cell;
        if ((tipoReporte === 'ventas' && index === 1) ||
            (tipoReporte === 'productos' && (index === 3 || index === 4)) ||
            (tipoReporte === 'compras' && index === 1)) {
          cellContent = `$${cell}`;
        }
        htmlContent += `<td>${this.escapeHtml(cellContent)}</td>`;
      });

      htmlContent += '</tr>';
    });

    htmlContent += `
          </tbody>
        </table>
      </body>
      </html>
    `;

    return htmlContent;
  }

  exportarCsv(tipoReporte: 'ventas' | 'productos' | 'compras', data: any[]): void {
    this.notificationService.info('Preparando exportación...', 'Por favor espere');

    try {
      if (!this.validateExportData(data, tipoReporte)) {
        return;
      }

      const config = this.getColumnsConfig(tipoReporte);
      let csvContent = config.headers.join(',') + '\n';

      data.forEach(item => {
        const row = config.extractData(item);
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
      if (!this.validateExportData(data, tipoReporte)) {
        return;
      }

      const htmlContent = this.generateHtmlContent(tipoReporte, data);

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
