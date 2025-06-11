import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { environment } from '../environments/environment.development';
import { User } from '../_models/user';
import { catchError, map, Observable } from 'rxjs';
import { jwtDecode } from 'jwt-decode';
import { Login } from '../_models/login.model';
import { Register } from '../_models/register.model';
import { JoinKiosco } from '../_models/join-kiosco.model';
import { CreateKiosco } from '../_models/create-kiosco.model';

interface JwtPayload {
  role?: string | string[];
  kioscoId?: string | null;
  [key: string]: unknown;
}


@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private baseUrl = environment.apiUrl;
  currentUser = signal<User | null>(null);
  private decodedTokenCache: {
    userToken: string | null;
    payload: JwtPayload | null;
  } = { userToken: null, payload: null };

  private getDecodedToken(): JwtPayload | null {
    const user = this.currentUser();
    const token = user?.token ?? null;
    if (!token) return null;
    if (this.decodedTokenCache.userToken === token) {
      return this.decodedTokenCache.payload;
    }
    try {
      const payload = jwtDecode<JwtPayload>(token);
      this.decodedTokenCache = { userToken: token, payload };
      return payload;
    } catch (error) {
      console.error('Error al decodificar el token JWT:', error);
      this.decodedTokenCache = { userToken: token, payload: null };
      return null;
    }
  }

  roles = computed(() => {
    const payload = this.getDecodedToken();
    const role = payload?.role;
    return Array.isArray(role) ? role : role ? [role] : [];
  });
  kioscoId = computed(() => {
    const payload = this.getDecodedToken();
    return payload?.kioscoId ?? null;
  });

  login(model: Login) {
    return this.handleAuth(
      this.http.post<User>(this.baseUrl + 'account/login', model)
    );
  }

  register(model: Register) {
    return this.handleAuth(
      this.http.post<User>(this.baseUrl + 'account/register', model)
    );
  }

  joinKiosco(model: JoinKiosco) {
    return this.handleAuth(
      this.http.post<User>(this.baseUrl + 'account/join-kiosco', model)
    );
  }

  createKiosco(model: CreateKiosco) {
      return this.handleAuth(
      this.http.post<User>(this.baseUrl + 'account/create-kiosco', model)
    );
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

  logout() {
    localStorage.removeItem('user');
    this.currentUser.set(null);
    this.decodedTokenCache = { userToken: null, payload: null };
    this.router.navigate(['/']);
  }

  private handleAuth<T>(obs: Observable<User>) {
    return obs.pipe(
      map((user) => {
        if (user) this.setCurrentUser(user);
        return user;
      }),
      catchError((err) => {
        console.error(err);
        throw err;
      })
    );
  }
}

