-- Private application roles. The API creates customer rows after verified sign-in.
-- Admin assignments require a deliberate database update by the project owner.
create table store.profiles (
    user_id uuid primary key references auth.users(id) on delete cascade,
    role text not null default 'customer' check (role in ('customer', 'admin')),
    created_at timestamptz not null default now()
);

alter table store.profiles enable row level security;
revoke all on store.profiles from anon, authenticated;
