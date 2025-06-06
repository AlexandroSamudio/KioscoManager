import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AccountService } from './_services/account.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent implements OnInit {
  accountService = inject(AccountService);

  ngOnInit() {
    this.setCurrentUser();
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
