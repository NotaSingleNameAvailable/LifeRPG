import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { NgIf } from '@angular/common';
import { Router } from '@angular/router';
import { UserService } from '../../core/services/user.service';  

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule, HttpClientModule, NgIf],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class Register {
  user = {
    username: '',
    email: '',
    password: ''  
  };

  message = '';

  //  Inject UserService instead of HttpClient
  constructor(private userService: UserService, private router: Router) {}

  onSubmit() {
    //  Use UserService instead of direct HTTP call
    this.userService.register(this.user).subscribe({
      next: (response) => {
        console.log('User registered:', response);
        this.message = 'Registration successful!';
        localStorage.setItem('userId', (response as any).id);  // Save user ID
        this.router.navigate(['/dashboard']);  //  Redirect to dashboard
      },
      error: (error) => {
        console.error('Registration failed:', error);
        this.message = error.error?.errors?.['$']?.[0] || 'Registration failed.';
      }
    });
  }

  goToLogin() {
    this.router.navigate(['/login']);
  }
}