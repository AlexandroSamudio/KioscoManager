import { Injectable } from '@angular/core';

export interface FooterLink {
  text: string;
  url: string;
  external?: boolean;
}

export interface FooterSection {
  title: string;
  links: FooterLink[];
}

export interface Feature {
  icon: string;
  title: string;
  description: string;
}

export interface Benefit {
  icon: string;
  title: string;
  description: string;
  metric?: string;
}

export interface Testimonial {
  id: number;
  name: string;
  business: string;
  rating: number;
  comment: string;
  avatar?: string;
}

@Injectable({
  providedIn: 'root'
})
export class HomeDataService {

  readonly features: readonly Feature[] = [
    {
      icon: 'package',
      title: 'Gestión de Productos',
      description: 'Administra tu inventario completo con categorías y precios dinámicos'
    },
    {
      icon: 'shopping-cart',
      title: 'Registro de Ventas',
      description: 'Realiza ventas rápidas con un sistema de carrito intuitivo'
    },
    {
      icon: 'alert-triangle',
      title: 'Alertas de Stock',
      description: 'Notificaciones automáticas cuando los productos están por agotarse'
    },
    {
      icon: 'bar-chart-3',
      title: 'Reportes Avanzados',
      description: 'Análisis detallados de ventas, productos más vendidos y tendencias de mercado'
    },
    {
      icon: 'users',
      title: 'Multiusuario',
      description: 'Permite acceso a múltiples usuarios con roles personalizados para empleados y administradores'
    },
    {
      icon: 'shield',
      title: 'Seguridad Total',
      description: 'Acceso seguro con diferentes niveles de usuario'
    }
  ] as const;

  readonly testimonials: readonly Testimonial[] = [
    {
      id: 1,
      name: 'María González',
      business: 'Kiosco El Rincón',
      rating: 5,
      comment: 'Desde que uso KioscoManager, mis ventas aumentaron 40%. Las alertas de stock me salvaron de perder muchas ventas.',
      avatar: 'https://randomuser.me/api/portraits/women/44.jpg'
    },
    {
      id: 2,
      name: 'Carlos Ruiz',
      business: 'Minimarket San José',
      rating: 5,
      comment: 'La facilidad de uso es increíble. En una semana ya tenía todo configurado y funcionando perfectamente.',
      avatar: 'https://randomuser.me/api/portraits/men/32.jpg'
    },
    {
      id: 3,
      name: 'Ana Martínez',
      business: 'Kiosco Central',
      rating: 5,
      comment: 'Los reportes me ayudan a tomar mejores decisiones. Ahora sé exactamente qué productos comprar y cuándo.',
      avatar: 'https://randomuser.me/api/portraits/women/65.jpg'
    }
  ] as const;

  readonly benefits: readonly Benefit[] = [
    {
      icon: 'clock',
      title: 'Ahorra hasta 5 horas semanales',
      description: 'Automatiza tareas repetitivas y enfócate en atender clientes',
      metric: '5h/semana'
    },
    {
      icon: 'trending-down',
      title: 'Reduce pérdidas por stock',
      description: 'Alertas inteligentes previenen productos vencidos o agotados',
      metric: '-60% pérdidas'
    },
    {
      icon: 'trending-up',
      title: 'Aumenta ventas 30%',
      description: 'Identifica productos estrella y optimiza tu estrategia de precios',
      metric: '+30% ventas'
    }
  ] as const;

  readonly footerSections: readonly FooterSection[] = [
    {
      title: 'Producto',
      links: [
        { text: 'Características', url: '#features' },
        { text: 'Precios', url: '#pricing' },
      ]
    },
    {
      title: 'Soporte',
      links: [
        { text: 'Documentación', url: '/docs' },
        { text: 'Contacto', url: '/contact' },
      ]
    },
    {
      title: 'Empresa',
      links: [
        { text: 'Acerca de Nosotros', url: '/about' },
        { text: 'Blog', url: '/blog' },

      ]
    },
    {
      title: 'Legal',
      links: [
        { text: 'Términos de Servicio', url: '/terms' },
        { text: 'Política de Privacidad', url: '/privacy' },
      ]
    }
  ] as const;

  readonly socialLinks = [
    {
      name: 'Facebook',
      url: 'https://facebook.com/kiosco-manager',
      icon: 'facebook'
    },
    {
      name: 'Twitter',
      url: 'https://twitter.com/kiosco-manager',
      icon: 'twitter'
    },
    {
      name: 'LinkedIn',
      url: 'https://linkedin.com/company/kiosco-manager',
      icon: 'linkedin'
    },
    {
      name: 'Instagram',
      url: 'https://instagram.com/kiosco-manager',
      icon: 'instagram'
    }
  ] as const;

  constructor() { }
}
