# BookStore authentication setup

The API accepts signed Supabase Auth access tokens from this project's Auth service. `Supabase:Url` is public configuration; the Postgres connection string stays in local user-secrets or deployment secrets. No Google or Facebook client secret belongs in this repository.

## Local setup

1. Configure the Supabase Postgres connection as described in `DATABASE_SETUP.md`.
2. Check `store.profiles` with `dotnet run --project StoreApi/StoreApi.csproj -- --check-profiles-schema`. If absent, apply `database/002_profiles.sql` once with `--apply-profiles-schema` from the same project command.
3. In Supabase Authentication, configure the local site URL and redirect allow list for the frontend's callback URL. Configure Google under Authentication > Providers with the Google Cloud OAuth web client ID and secret. Google's authorized redirect URI is the Supabase Auth callback shown by the provider settings, not the API URL.
4. The frontend signs in through Supabase Auth, then sends `Authorization: Bearer <access_token>` to the API. `GET /api/me` returns the verified user ID, email claim, and BookStore role. The first successful request creates a `customer` profile.

`GET /api/Books` is public. Book creation, update, and deletion require the `admin` profile role; unauthenticated requests return 401 and authenticated customers return 403. Promote only a known user after independently verifying their `auth.users.id`, using a privileged database session:

```sql
update store.profiles set role = 'admin' where user_id = '<verified-user-uuid>';
```

The old in-memory user and subscription controllers are excluded from the API because they do not use Supabase Auth or persistent account data. Payment handling is not enabled.

## Public HTTPS address later

The owner has `techspace.tech` at Hostinger. Choose the web host and a final HTTPS subdomain before changing DNS. Then add the exact frontend callback URL to Supabase's redirect allow list and to the OAuth application's authorized origins where required. The provider callback remains the Supabase Auth URL displayed in the provider dashboard. Configure the backend's allowed frontend origin and production database secret in the chosen host; never place server secrets in the browser bundle.

Google login needs Google Cloud OAuth consent configuration and a web client. Facebook login needs a Meta developer app, its OAuth settings, and a provider client secret in Supabase. Provider review, domain checks, and policy requirements may take time; set these up well before the mid-October deadline. Do not activate paid services or collect client secrets in chat.

Official references: [Supabase Google login](https://supabase.com/docs/guides/auth/social-login/auth-google), [Supabase Facebook login](https://supabase.com/docs/guides/auth/social-login/auth-facebook), [Supabase redirect URLs](https://supabase.com/docs/guides/auth/redirect-urls), [Supabase JWTs](https://supabase.com/docs/guides/auth/jwts).
