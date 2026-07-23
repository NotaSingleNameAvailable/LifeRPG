import { HttpInterceptorFn } from '@angular/common/http';

// Why an interceptor? Without it, you'd have to manually add the token
// to every single HTTP call in every service. The interceptor runs automatically
// on EVERY outgoing request and adds the header for you.
//
// Mental model:
// Any HTTP call → Interceptor grabs token from localStorage
//               → Adds "Authorization: Bearer ***..." header
//               → Request continues to backend
//               → Backend JWT middleware validates it

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('token');
  
  if (token) {
    const authReq = req.clone({
      headers: req.headers.set('Authorization', `Bearer ${token}`)
    });
    return next(authReq);
  }

  return next(req);
};