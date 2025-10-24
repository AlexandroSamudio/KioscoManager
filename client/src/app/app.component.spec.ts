import { TestBed } from '@angular/core/testing';
import { AppComponent } from './app.component';
import { provideRouter } from '@angular/router';
import { Title, Meta } from '@angular/platform-browser';
import { AccountService } from './_services/account.service';


describe('AppComponent', () => {
  const accountStub: jest.Mocked<Partial<AccountService>> = {
    setCurrentUser: jest.fn(),
  };

  beforeEach(async () => {
    jest.clearAllMocks();
    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [provideRouter([]), Title, Meta, { provide: AccountService, useValue: accountStub }],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it(`should have the 'Kiosco Manager' title`, () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.componentInstance;
    expect(app.title).toEqual('Kiosco Manager');
  });

  it('sets document title on init', () => {
    const fixture = TestBed.createComponent(AppComponent);
    const title = TestBed.inject(Title);
    fixture.detectChanges();
    expect(title.getTitle()).toBe('Kiosco Manager');
  });

  it('calls setCurrentUser when user exists in localStorage', () => {
    localStorage.setItem('user', JSON.stringify({ id: 1, username: 'alex' }));
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();
    expect(accountStub.setCurrentUser).toHaveBeenCalled();
    localStorage.removeItem('user');
  });

  it('removes invalid user from localStorage on parse error', () => {
    localStorage.setItem('user', 'not-json');
    const warnSpy = jest.spyOn(console, 'warn').mockImplementation(() => {});
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();
    expect(localStorage.getItem('user')).toBeNull();
    expect(warnSpy).toHaveBeenCalled();
    warnSpy.mockRestore();
  });
});
