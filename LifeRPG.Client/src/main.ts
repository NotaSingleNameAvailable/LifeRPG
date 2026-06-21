import { bootstrapApplication } from '@angular/platform-browser';
import { App } from './app/app';
import { provideRouter } from '@angular/router';
import { routes } from './app/app.routes';
import { provideHttpClient } from '@angular/common/http'; // ✅ new way

bootstrapApplication(App, {
  providers: [
    provideRouter(routes),   // your routes
    provideHttpClient(),     // provides HttpClient app-wide
  ]
});