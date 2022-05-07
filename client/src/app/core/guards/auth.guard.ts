import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AccountService } from 'src/app/account/account.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(private accountService: AccountService, private router: Router){}

  canActivate(
    route: ActivatedRouteSnapshot, // ruta koja pokusava da se aktivira
    state: RouterStateSnapshot): Observable<boolean> { // mozemo da iskoristimo da saznamo odakle korisnik potice
    return this.accountService.currentUser$.pipe(
      map(auth => {
        if(auth){
          return true;
        }
        this.router.navigate(['account/login'], {queryParams: {returnUrl: state.url}});// state.url ce biti nas checkout
      })
    );
  }

}
