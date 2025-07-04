export interface KioscoConfig {
  id: number;
  kioscoId: number;
  nombreKiosco: string;
  direccion?: string;
  telefono?: string;
  email?: string;
  logoUrl?: string;
  configuracionComercial: ConfiguracionComercial;
  configuracionInventario: ConfiguracionInventario;
  configuracionReportes: ConfiguracionReportes;
}

export interface ConfiguracionComercial {
  ivaHabilitado: boolean;
  porcentajeIva: number;
  prefijoSku: string;
}

export interface ConfiguracionInventario {
  nivelMinimoStockGeneral: number;
  alertasHabilitadas: boolean;
  notificacionesEmail: boolean;
}

export interface ConfiguracionReportes {
  formatoExportacion: 'CSV' | 'PDF' | 'EXCEL';
  backupAutomatico: boolean;
  frecuenciaBackup: 'DIARIO' | 'SEMANAL' | 'MENSUAL';
}

export interface UserPreferences {
  id: number;
  userId: number;
  tema: 'claro' | 'oscuro';
  idioma: string;
  notificaciones: NotificacionesPreferences;
  dashboard: DashboardPreferences;
}

export interface NotificacionesPreferences {
  stockBajo: boolean;
  ventasImportantes: boolean;
  reportesDiarios: boolean;
  email: boolean;
  push: boolean;
}

export interface DashboardPreferences {
  componentesVisibles: string[];
  ordenComponentes: string[];
  intervalosActualizacion: number;
}

export interface KioscoBasicInfo {
  id?: number;
  nombre: string;
  direccion?: string;
  telefono?: string;
}
