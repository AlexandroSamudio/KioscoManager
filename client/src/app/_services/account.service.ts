import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { environment } from '../environments/environment.development';
import { User } from '../_models/user';
import { catchError, map } from 'rxjs';
import { jwtDecode } from 'jwt-decode';
import { Login } from '../_models/login.model';
import { Register } from '../_models/register.model';

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;
  currentUser = signal<User | null>(null);
  roles = computed(() => {
      const user = this.currentUser();
      if (user?.token) {
      try {
        const payload = jwtDecode<{ role: string | string[] }>(user.token);
        const role = payload.role;
        return Array.isArray(role) ? role : [role];
      } catch (error) {
        console.error('Error al parsear el JWT token:', error);
        return [];
      }
      }
      return [];
  });

  login(model: Login) {
    return this.http.post<User>(this.baseUrl + 'account/login', model).pipe(
      map(user => {
        if (user) {
          this.setCurrentUser(user);
        }
        return user;
      }),
      catchError(error => {
        console.error('Error en login:', error);
        throw error;
      })
    )
  }

  register(model: Register) {
    return this.http.post<User>(this.baseUrl + 'account/register', model).pipe(
      map(user => {
        if (user) {
          this.setCurrentUser(user);
        }
        return user;
      }),
      catchError(error => {
        console.error('Error al registrarse:', error);
        throw error;
      })
    )
  }

  setCurrentUser(user: User) {
    if (!user?.username || !user?.email || !user?.token) {
      console.error('Usuario inválido, no se puede guardar en localStorage');
      return;
    }
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUser.set(user);
  }

  isLoggedIn(): boolean {
    return this.currentUser() !== null;
  }


}
