-- Apply after 001_books.sql. Keep the store schema outside Supabase Data API.
alter table store.books add column if not exists open_library_key text;
alter table store.books add column if not exists first_publish_year integer;
alter table store.books add column if not exists subject text;
alter table store.books add constraint books_open_library_key_format
    check (open_library_key is null or open_library_key ~ '^/works/OL[0-9]+W$');
create unique index if not exists books_open_library_key_unique on store.books (open_library_key)
    where open_library_key is not null;
create unique index if not exists books_title_author_unique
    on store.books (lower(trim(title)), lower(trim(author)));
