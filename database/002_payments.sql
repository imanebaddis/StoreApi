-- Apply with a privileged Postgres connection after reviewing the target database.
create schema if not exists store;
create table if not exists store.payments (
    id uuid primary key,
    user_id uuid not null,
    plan text not null check (plan in ('Basic', 'Silver', 'Gold')),
    amount numeric(10,2) not null check (amount > 0),
    currency char(3) not null default 'EUR',
    paypal_order_id text unique,
    paypal_capture_id text unique,
    status text not null check (status in ('pending', 'approved', 'completed', 'failed', 'refunded')),
    created_at timestamptz not null default now(),
    completed_at timestamptz,
    access_until timestamptz
);
create index if not exists payments_user_completed_idx on store.payments (user_id, access_until desc) where status = 'completed';
alter table store.payments enable row level security;
revoke all on store.payments from public, anon, authenticated;
-- No Data API policy is created. Use a server-side Postgres connection with a
-- role allowed to bypass RLS (e.g. the existing trusted postgres connection).
-- Never expose that connection string to the browser.
