import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment.development';
import { UserManagement,UserRoleAssignment, UserRoleResponse, PasswordChangeRequest, PasswordChangeResponse,ProfileUpdateRequest } from '../_models/user.model';



@Injectable({
  providedIn: 'root'
})
export class UserService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl + 'users';

  getUsers(): Observable<UserManagement[]> {
    return this.http.get<UserManagement[]>(this.baseUrl);
  }

  getUser(id: number): Observable<UserManagement> {
    return this.http.get<UserManagement>(`${this.baseUrl}/${id}`);
  }

  getUsersByKiosco(kioscoId: number): Observable<UserManagement[]> {
    return this.http.get<UserManagement[]>(`${this.baseUrl}/kiosco/${kioscoId}`);
  }

  assignRole(userId: number, roleAssignment: UserRoleAssignment): Observable<UserRoleResponse> {
    return this.http.post<UserRoleResponse>(`${this.baseUrl}/${userId}/role`, roleAssignment);
  }

  getUserRoles(userId: number): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/${userId}/roles`);
  }

  isUserAdmin(userId: number): Observable<boolean> {
    return this.http.get<boolean>(`${this.baseUrl}/${userId}/is-admin`);
  }

  updateProfile(userId: number, profileData: ProfileUpdateRequest): Observable<UserManagement> {
    return this.http.put<UserManagement>(`${this.baseUrl}/${userId}/perfil`, profileData);
  }

  changePassword(userId: number, passwordData: PasswordChangeRequest): Observable<PasswordChangeResponse> {
    return this.http.put<PasswordChangeResponse>(`${this.baseUrl}/${userId}/password`, passwordData);
  }
}
