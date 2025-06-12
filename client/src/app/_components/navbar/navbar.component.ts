import { Component, input, output, computed, signal, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

import { NavigationItem, UserProfile } from '../../_models/navigation.interface';
import { AccountService } from '../../_services/account.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class NavbarComponent {
  private readonly defaultNavigationItems: NavigationItem[] = [
    { label: 'Inventario', path: '/inventario' },
    { label: 'Ventas', path: '/ventas' },
    { label: 'Reportes', path: '/reportes' },
    { label: 'Compras', path: '/compras' },
    { label: 'Configuración', path: '/configuracion' }
  ];

  accountService = inject(AccountService);

  navigationItems = input<NavigationItem[]>();

  navigationItemClick = output<NavigationItem>();
  userMenuClick = output<void>();

  protected readonly isUserMenuOpen = signal<boolean>(false);

  protected readonly userProfile = computed<UserProfile>(() => {
    const user = this.accountService.currentUser();
    if (!user) {
      return { name: 'Usuario', initials: 'U' };
    }

    const name = this.toTitleCase(user.username) || 'Usuario';
    const initials = this.generateInitials(name);

    return { name, initials };
  });

  protected readonly userInitials = computed(() =>
    this.userProfile().initials || this.generateInitials(this.userProfile().name)
  );

  protected readonly effectiveNavigationItems = computed(() =>
    this.navigationItems() ?? this.defaultNavigationItems
  );

  protected onNavigationItemClick(item: NavigationItem): void {
    this.navigationItemClick.emit(item);
  }

  protected onUserMenuClick(): void {
    this.isUserMenuOpen.update(isOpen => !isOpen);
    this.userMenuClick.emit();
  }

  private generateInitials(name: string): string {
    if (!name) return 'U';
    return name
      .split(' ')
      .map(word => word.charAt(0).toUpperCase())
      .slice(0, 2)
      .join('');
  }

  private toTitleCase(str: string): string {
    return str.toLowerCase().replace(/\b\w/g, l => l.toUpperCase());
  }
}
