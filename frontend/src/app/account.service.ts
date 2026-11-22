import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export interface AccountSummary {
  id: string;
  ownerName: string;
  currency: string;
  balance: number;
  totalDeposited: number;
  totalWithdrawn: number;
  transactionCount: number;
}

export interface BalanceResponse {
  id: string;
  balance: number;
}

export interface TransactionItem {
  type: 'Deposit' | 'Withdrawal';
  amount: number;
  reference: string;
  timestamp: string;
}

@Injectable({ providedIn: 'root' })
export class AccountService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/accounts';

  open(ownerName: string, currency: string): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.baseUrl, { ownerName, currency });
  }

  deposit(id: string, amount: number, reference: string): Observable<BalanceResponse> {
    return this.http.post<BalanceResponse>(`${this.baseUrl}/${id}/deposit`, { amount, reference });
  }

  withdraw(id: string, amount: number, reference: string): Observable<BalanceResponse> {
    return this.http.post<BalanceResponse>(`${this.baseUrl}/${id}/withdraw`, { amount, reference });
  }

  getTransactions(id: string): Observable<TransactionItem[]> {
    return this.http.get<TransactionItem[]>(`${this.baseUrl}/${id}/transactions`);
  }

  list(): Observable<AccountSummary[]> {
    return this.http.get<AccountSummary[]>(this.baseUrl);
  }

  getSummary(id: string): Observable<AccountSummary> {
    return this.http.get<AccountSummary>(`${this.baseUrl}/${id}`);
  }
}
