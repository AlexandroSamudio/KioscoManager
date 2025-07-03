export interface User {
  id: number;
  username: string;
  email: string;
  token: string;
}

export interface UserManagement {
  id: number;
  userName: string;
  email: string;
  role?: string;
  kioscoId?: number;
  nombreKiosco?: string;
}

export interface UserRoleAssignment {
  role: string;
}

export interface UserRoleResponse {
  userId: number;
  userName: string;
  email: string;
  role: string;
  success: boolean;
  message: string;
}

export interface PasswordChangeResponse {
  errorCode?: number;
}

export interface PasswordChangeRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

export interface ProfileUpdateRequest {
  userName?: string;
  email?: string;
}
