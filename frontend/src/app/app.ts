import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize, Observable } from 'rxjs';
import { AccountService, AccountSummary, TransactionItem } from './account.service';

interface Toast {
  id: number;
  kind: 'ok' | 'err';
  text: string;
}

@Component({
  imports: [FormsModule, DecimalPipe, DatePipe],
  selector: 'app-root',
  styleUrl: './app.scss',
  templateUrl: './app.html',
})
export class App implements OnInit {
  private readonly accounts = inject(AccountService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly accountId = signal('');
  protected readonly accountList = signal<AccountSummary[]>([]);
  protected readonly transactions = signal<TransactionItem[]>([]);
  protected readonly summary = signal<AccountSummary | null>(null);
  protected readonly toasts = signal<Toast[]>([]);
  protected readonly pending = signal(0);
  private nextToastId = 0;

  protected ownerName = '';
  protected currency = 'BAM';
  protected readonly currencies = [
    { code: 'BAM', label: 'BAM (KM)' },
    { code: 'EUR', label: 'EUR (€)' },
    { code: 'USD', label: 'USD ($)' },
    { code: 'GBP', label: 'GBP (£)' },
    { code: 'CHF', label: 'CHF' },
  ];
  protected amount: number | null = null;
  protected reference = '';

  ngOnInit(): void {
    this.loadAccounts();
    // The URL (?account=<id>) is the source of truth for the active account.
    this.route.queryParamMap.subscribe(params => this.activate(params.get('account') ?? ''));
  }

  protected loadAccounts(): void {
    this.run(this.accounts.list(), list => this.accountList.set(list));
  }

  protected select(id: string): void {
    this.router.navigate([], { queryParams: { account: id || null }, queryParamsHandling: 'merge' });
  }

  private activate(id: string): void {
    this.accountId.set(id);
    this.summary.set(null);
    this.transactions.set([]);
    if (id) {
      this.refresh();
    }
  }

  protected canOpen(): boolean {
    return this.ownerName.trim().length > 0 && !!this.currency && this.pending() === 0;
  }

  protected canTransact(): boolean {
    return !!this.accountId() && (this.amount ?? 0) > 0 && this.pending() === 0;
  }

  protected open(): void {
    this.run(this.accounts.open(this.ownerName, this.currency), res => {
      this.show('ok', 'Account opened.');
      this.loadAccounts();
      this.select(res.id);
    });
  }

  protected deposit(): void {
    this.run(this.accounts.deposit(this.accountId(), this.amount ?? 0, this.reference), res => {
      this.show('ok', `Deposited. New balance: ${res.balance}`);
      this.refresh();
    });
  }

  protected withdraw(): void {
    this.run(this.accounts.withdraw(this.accountId(), this.amount ?? 0, this.reference), res => {
      this.show('ok', `Withdrawn. New balance: ${res.balance}`);
      this.refresh();
    });
  }

  protected refresh(): void {
    this.run(this.accounts.getSummary(this.accountId()), s => {
      this.summary.set(s);
      this.loadAccounts();
      this.run(this.accounts.getTransactions(this.accountId()), t => this.transactions.set(t));
    });
  }

  protected dismiss(id: number): void {
    this.toasts.update(list => list.filter(t => t.id !== id));
  }

  private show(kind: Toast['kind'], text: string): void {
    const id = this.nextToastId++;
    this.toasts.update(list => [...list, { id, kind, text }]);
    setTimeout(() => this.dismiss(id), 4000);
  }

  private run<T>(request: Observable<T>, onSuccess: (value: T) => void): void {
    this.pending.update(n => n + 1);
    request.pipe(finalize(() => this.pending.update(n => n - 1))).subscribe({
      next: onSuccess,
      error: (e: HttpErrorResponse) =>
        this.show('err', e.error?.error ?? (e.status === 404 ? 'Account not found' : e.message)),
    });
  }
}
