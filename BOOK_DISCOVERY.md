# Book discovery and recommendations

The BookStore web app has a **Cerca libri** page. A user explicitly searches Open Library; the API requests at most 10 works, caches a query for 15 minutes and sends requests at no more than one per second per API process. It asks only for the fields it displays. Results link back to Open Library and are attributed in the page.

Selecting **Aggiungi al catalogo** sends only the Open Library work key. The API imports metadata from its recent search cache, never from browser supplied title/author text. `database/002_book_discovery.sql` adds source key and metadata columns to `store.books`, with unique constraints on source key and normalized title plus author. Apply this SQL to the existing private `store` schema before starting the updated API. Check existing title/author duplicates before applying the unique index. The migration has **not** been applied to Supabase by this branch.

Import is permitted only in the Development environment until backend authorization is integrated. The normal production route returns 403. The catalog remains a local StoreApi database feature; Open Library is used only for user requested discovery, not as the catalog backend. Set an application contact email in the Open Library User-Agent before higher volume use.

The **Consigli** page currently ranks local catalog books by author and category overlap with browser favorites and viewed books. Each card states the reason. It does not use a paid AI service or send activity to Supabase. It has no purchase history yet.

`database/003_user_activity_after_auth.sql` is a proposed follow-up migration for per-user events, deliberately separate from the Auth work. Apply it only once backend Supabase Auth JWT validation is integrated. The API must derive `user_id` from a verified token. Store view/favorite/purchase events there, then move recommendation ranking to a backend service using those events. Do not accept a client-supplied user ID. The table is in the private `store` schema with RLS enabled and no Data API grants.
