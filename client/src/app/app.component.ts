import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AccountService } from './_services/account.service';
import { Meta, Title } from '@angular/platform-browser';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent implements OnInit {
  accountService = inject(AccountService);
  metaService = inject(Meta);
  titleService = inject(Title);
  title = 'Kiosco Manager';


  ngOnInit() {
    this.setCurrentUser();
    this.titleService.setTitle(this.title);

     this.metaService.addTag({
      name: 'description',
      content: 'Gestiona tu kiosco de forma eficiente. Controla el stock, ventas, productos, y más, con estadísticas en tiempo real.'
    });
  }

  setCurrentUser() {
    const userString = localStorage.getItem('user');
    if (!userString) return;
    try {
      const user = JSON.parse(userString);
      this.accountService.setCurrentUser(user);
    } catch (error) {
      console.warn(
        'Error al parsear datos de usuario desde localStorage:',
        error
      );
      localStorage.removeItem('user');
    }
  }
}
