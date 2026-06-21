import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './login.html',
  styleUrls: ['./login.css'],
})
export class Login {
  username = '';
  password = '';

  constructor(private authService: AuthService, private router: Router) {}

  login() {
    const loginData = {
      username: this.username,
      password: this.password
    };

    this.authService.login(loginData).subscribe({
      next: (res: any) => {
        localStorage.setItem('userId', res.id);
        localStorage.setItem('username', res.username);
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        console.log('Login failed', err);
        alert('Invalid username or password');
      }
    });
  }

    goToRegister() {
    this.router.navigate(['/register']);
  }
}