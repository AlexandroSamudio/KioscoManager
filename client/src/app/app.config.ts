import { ApplicationConfig, provideZoneChangeDetection} from '@angular/core';
import { provideRouter, withPreloading, NoPreloading, withViewTransitions } from '@angular/router';
import { provideHttpClient, withInterceptors, withFetch } from '@angular/common/http';

import { routes } from './app.routes';
import { jwtInterceptor } from './_interceptors/jwt.interceptor';
import { provideCharts, withDefaultRegisterables } from 'ng2-charts';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({
      eventCoalescing: true,
      runCoalescing: true
    }),
    provideRouter(
      routes,
      withViewTransitions(),
      withPreloading(NoPreloading) // NoPreloading para carga bajo demanda más eficiente
    ),
    provideHttpClient(
      withInterceptors([jwtInterceptor]),
      withFetch() // Fetch API en lugar de XHR para mejor rendimiento
    ),
    provideCharts(withDefaultRegisterables()),
  ],
};
