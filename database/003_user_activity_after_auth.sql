-- Proposed follow-up migration. Apply only after backend validation of Supabase Auth JWTs is integrated.
-- The API must derive user_id from the verified token, never from a request body.
-- Keep store outside schemas exposed through Supabase Data API.
create table if not exists store.book_activity (
    id bigint generated always as identity primary key,
    user_id uuid not null references auth.users(id) on delete cascade,
    book_id integer not null references store.books(id) on delete cascade,
    event_type text not null check (event_type in ('view', 'favorite', 'purchase')),
    created_at timestamptz not null default now()
);
create index if not exists book_activity_user_recent on store.book_activity (user_id, created_at desc);
create index if not exists book_activity_user_book on store.book_activity (user_id, book_id);
alter table store.book_activity enable row level security;
-- No direct Data API grants or policies: activity is accessed by the backend after token validation.
