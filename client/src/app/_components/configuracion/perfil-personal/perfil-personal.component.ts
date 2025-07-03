import { Component, signal, OnInit, inject, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { AccountService } from '../../../_services/account.service';
import { UserService } from '../../../_services/user.service';
import { NotificationService } from '../../../_services/notification.service';
import {
  UserManagement,
  PasswordChangeRequest,
  ProfileUpdateRequest,
  PasswordChangeResponse,
} from '../../../_models/user.model';

enum PasswordChangeErrorCode {
  None = 0,
  UserNotFound = 1,
  InvalidCurrentPassword = 2,
  PasswordValidationFailed = 3,
  UnknownError = 99,
}

@Component({
  selector: 'app-perfil-personal',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './perfil-personal.component.html',
})
export class PerfilPersonalComponent implements OnInit {
  private fb = inject(FormBuilder);
  private accountService = inject(AccountService);
  private userService = inject(UserService);
  private notificationService = inject(NotificationService);
  private destroyRef = inject(DestroyRef);

  isLoading = signal(true);
  isSubmittingProfile = signal(false);
  isSubmittingPassword = signal(false);
  initialProfileData = signal<{ username: string; email: string } | null>(null);
  showCurrentPassword = signal(false);
  showNewPassword = signal(false);
  showConfirmPassword = signal(false);

  profileForm: FormGroup;
  passwordForm: FormGroup;

  constructor() {
    this.profileForm = this.fb.group({
      username: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.pattern(/^[a-zA-Z0-9_]+$/),
        ],
      ],
      email: ['', [Validators.required, Validators.email]],
    });

    this.passwordForm = this.fb.group(
      {
        currentPassword: ['', [Validators.required]],
        newPassword: [
          '',
          [
            Validators.required,
            Validators.minLength(8),
            this.passwordStrengthValidator,
          ],
        ],
        confirmNewPassword: ['', [Validators.required]],
      },
      {
        validators: this.passwordMatchValidator,
      }
    );
  }

  ngOnInit(): void {
    this.loadUserData();
  }

  private loadUserData(): void {
    const currentUser = this.accountService.currentUser();
    if (!currentUser?.id) {
      this.notificationService.error(
        'Error',
        'No se pudo obtener la información del usuario'
      );
      this.isLoading.set(false);
      return;
    }

    const profileData = {
      username: currentUser.username,
      email: currentUser.email,
    };

    this.profileForm.patchValue(profileData);
    this.initialProfileData.set(profileData);
    this.isLoading.set(false);
  }

  private passwordMatchValidator(form: FormGroup) {
    const newPassword = form.get('newPassword');
    const confirmNewPassword = form.get('confirmNewPassword');

    if (
      newPassword &&
      confirmNewPassword &&
      newPassword.value !== confirmNewPassword.value
    ) {
      return { passwordMismatch: true };
    }
    return null;
  }

  private passwordStrengthValidator(control: any) {
    const password = control.value || '';

    const hasValidLength = password.length >= 8 && password.length <= 128;
    const hasUppercase = /[A-Z]/.test(password);
    const hasLowercase = /[a-z]/.test(password);
    const hasNumber = /\d/.test(password);
    const hasSpecialChar = /[\W_]/.test(password);

    const isValid =
      hasValidLength &&
      hasUppercase &&
      hasLowercase &&
      hasNumber &&
      hasSpecialChar;

    if (!isValid) {
      return {
        passwordStrength: {
          hasValidLength,
          hasUppercase,
          hasLowercase,
          hasNumber,
          hasSpecialChar,
        },
      };
    }

    return null;
  }

  hasProfileChanges(): boolean {
    const initial = this.initialProfileData();
    if (!initial) return false;

    const current = this.profileForm.value;
    return (
      initial.username !== current.username || initial.email !== current.email
    );
  }

  hasUnsavedChanges(): boolean {
    return this.hasProfileChanges() || this.passwordForm.dirty;
  }

  onSubmitProfile(): void {
    if (this.profileForm.invalid || this.isSubmittingProfile()) return;

    const currentUser = this.accountService.currentUser();
    if (!currentUser?.id) return;

    this.isSubmittingProfile.set(true);

    const profileData: ProfileUpdateRequest = {
      username: this.profileForm.value.username,
      email: this.profileForm.value.email,
    };

    this.userService
      .updateProfile(currentUser.id, profileData)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (updatedUser: UserManagement) => {
          const currentUser = this.accountService.currentUser();
          if (currentUser) {
            const updatedCurrentUser = {
              ...currentUser,
              username: updatedUser.username,
              email: updatedUser.email,
            };
            this.accountService.setCurrentUser(updatedCurrentUser);
          }

          this.initialProfileData.set({
            username: updatedUser.username,
            email: updatedUser.email,
          });
          this.notificationService.success(
            'Éxito',
            'Perfil actualizado correctamente'
          );
          this.isSubmittingProfile.set(false);
        },
        error: (error) => {
          console.error('Error updating profile:', error);
          this.notificationService.error(
            'Error',
            'Error al actualizar el perfil'
          );
          this.isSubmittingProfile.set(false);
        },
      });
  }

  onSubmitPassword(): void {
    if (!this.isPasswordFormValid() || this.isSubmittingPassword()) return;

    const currentUser = this.accountService.currentUser();
    if (!currentUser?.id) return;

    this.isSubmittingPassword.set(true);

    const passwordData: PasswordChangeRequest = {
      currentPassword: this.passwordForm.value.currentPassword,
      newPassword: this.passwordForm.value.newPassword,
      confirmPassword: this.passwordForm.value.confirmNewPassword,
    };

    this.userService
      .changePassword(currentUser.id, passwordData)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (_: PasswordChangeResponse) => {
          this.notificationService.success(
            'Éxito',
            'Contraseña cambiada correctamente'
          );
          this.resetPasswordForm();
          this.isSubmittingPassword.set(false);
        },
        error: (error) => {
          this.isSubmittingPassword.set(false);

          const errorResponse = error?.error;

          if (errorResponse?.errorCode !== undefined) {
            this.handlePasswordChangeError(
              errorResponse.errorCode,
              errorResponse.message || error.message
            );
          } else {
            console.error('Error inesperado al cambiar la contraseña:', error);
            this.notificationService.error(
              'Error',
              'Error inesperado al cambiar la contraseña'
            );
          }
        },
      });
  }

  resetProfileForm(): void {
    const initial = this.initialProfileData();
    if (initial) {
      this.profileForm.patchValue(initial);
      this.profileForm.markAsPristine();
    }
  }

  resetPasswordForm(): void {
    this.passwordForm.reset();
    this.passwordForm.markAsPristine();
  }

  private getNewPassword(): string {
    return this.passwordForm.get('newPassword')?.value || '';
  }

  checkPasswordLength(): boolean {
    const password = this.getNewPassword();
    return password.length >= 8 && password.length <= 128;
  }
  checkPasswordUppercase(): boolean {
    const password = this.getNewPassword();
    return /[A-Z]/.test(password);
  }
  checkPasswordLowercase(): boolean {
    const password = this.getNewPassword();
    return /[a-z]/.test(password);
  }
  checkPasswordNumber(): boolean {
    const password = this.getNewPassword();
    return /\d/.test(password);
  }
  checkPasswordSpecialChar(): boolean {
    const password = this.getNewPassword();
    return /[\W_]/.test(password);
  }

  passwordsMatch(): boolean {
    const newPassword = this.passwordForm.get('newPassword')?.value;
    const confirmPassword = this.passwordForm.get('confirmNewPassword')?.value;
    return newPassword === confirmPassword && newPassword !== '';
  }

  isPasswordFormValid(): boolean {
    return (
      this.passwordForm.valid &&
      this.checkPasswordLength() &&
      this.checkPasswordUppercase() &&
      this.checkPasswordLowercase() &&
      this.checkPasswordNumber() &&
      this.checkPasswordSpecialChar() &&
      this.passwordsMatch()
    );
  }

  private handlePasswordChangeError(
    errorCode: number | undefined,
    message: string | undefined
  ): void {
    switch (errorCode) {
      case PasswordChangeErrorCode.InvalidCurrentPassword:
        this.notificationService.error(
          'Contraseña incorrecta',
          message || 'La contraseña actual ingresada no es correcta.'
        );
        break;
      case PasswordChangeErrorCode.UserNotFound:
        this.notificationService.error(
          'Usuario no encontrado',
          message || 'No se pudo encontrar el usuario.'
        );
        break;
      case PasswordChangeErrorCode.PasswordValidationFailed:
        this.notificationService.error(
          'Validación de contraseña',
          message || 'La nueva contraseña no cumple con los requisitos.'
        );
        break;
      default:
        this.notificationService.error(
          'Error',
          message || 'Error al cambiar la contraseña'
        );
    }
  }
}
