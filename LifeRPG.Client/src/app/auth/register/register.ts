import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgIf } from '@angular/common';
import { Router } from '@angular/router';
import { UserService } from '../../core/services/user.service';  

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule, NgIf],
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
  isError = false;

  //  Inject UserService instead of HttpClient
  constructor(private userService: UserService, private router: Router) {}

  onSubmit() {
    this.userService.register(this.user).subscribe({
      next: (response) => {
        console.log('User registered:', response);
        this.message = 'Registration successful!';
        this.isError = false; //  green for success
        localStorage.setItem('userId', (response as any).id);
        localStorage.setItem('token', (response as any).token);//  save token
        this.router.navigate(['/dashboard']);
      },
      error: (error) => {
        console.error('Registration failed:', error);
        this.message = typeof error.error === 'string'
          ? error.error
          : 'Registration failed. Please try again.';
        this.isError = true; 
      }
    });
  }

  goToLogin() {
    this.router.navigate(['/login']);
  }
}