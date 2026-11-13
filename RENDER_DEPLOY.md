# BookStore on Render Free

The repository contains the backend Dockerfile and `render.yaml` for a **Free** API web service plus a Free static frontend site. Supabase remains the persistent database and Auth provider. The frontend build receives the resulting backend URL and its public Supabase publishable key.

## Before creating the Render service

1. Publish the verified repository changes to GitHub after reviewing the combined frontend and backend increment. Render builds the repository branch, not this local checkout.
2. Open Render and connect the GitHub repository. Choose the Blueprint from `render.yaml`. It creates `bookstore-api` (Docker Web Service, Free plan, Frankfurt, health check `/healthz`) and `bookstore-web` (Static Site, root `BookStore.Web`, build `node build.js`, publish `dist`).
3. In Render's environment settings, enter `ConnectionStrings__StoreDb` as the Npgsql connection string for the existing Supabase database. Copy it directly from your secret store or Supabase dashboard; never put it in GitHub, chat, or frontend configuration. SSL must remain enabled.
4. Set `BookStore__AllowedOrigins` on the API to the exact HTTPS origin of the deployed frontend, without a trailing slash (for example, `https://bookstore.techspace.tech`). For local testing, a comma-separated list may additionally include `http://localhost:4173`. Do not use `*`.
5. On `bookstore-web`, set `BOOKSTORE_API_URL` to the backend's actual HTTPS `onrender.com` URL and `SUPABASE_PUBLISHABLE_KEY` to the project's `sb_publishable_...` key. Both are public browser configuration, but the key should still be copied directly into Render and kept out of this document. The Blueprint sets `SUPABASE_GOOGLE_ENABLED=true` because the provider is now verified active. Confirm `ASPNETCORE_ENVIRONMENT=Production`, `BookStore__DemoMode=false`, and `Supabase__Url=https://qpwjzzmnqxtexelhpfqm.supabase.co` for the API. Render provides `PORT`; the API listens on it automatically.
6. Deploy. If the Blueprint needs the backend URL before its first build, deploy the API first or retry the static site build after its URL is available. Check `/healthz` returns 200, `/api/Books` returns the persisted catalog, and unauthenticated book writes return 401. Google OAuth, `/api/me` as a customer, and logout have passed locally on `localhost:4174`; repeat these checks on Render HTTPS. An administrator flow has not yet been verified.

The database migrations `001_books.sql` and `002_profiles.sql` have already been applied to the existing Supabase project. Do not run them automatically at every deploy. Legacy in-memory user and subscription endpoints are not compiled into the public API. Payments are not enabled.

## Domain and OAuth

After both Render services are live, add a frontend custom domain such as `bookstore.techspace.tech` and optionally a backend custom domain such as `api-bookstore.techspace.tech`, **only if** the workspace's included custom-domain allowance covers both. Get the exact DNS target values from Render before adding records in Hostinger. Render manages HTTPS certificates for verified custom domains. Update `BookStore__AllowedOrigins`, the frontend's API URL, Supabase Auth Site URL and redirect allow list, and Google/Facebook app settings with the final HTTPS addresses. OAuth provider redirect URI points to Supabase Auth's callback, as shown in its dashboard.

Render Free sleeps after 15 minutes without traffic and can take around a minute to wake. Its filesystem is temporary; use Supabase for persistence. Render's Hobby workspace includes two custom domains, while extra domains may incur a fee. Free services also share usage limits; if a payment method is on file, overages can be billed. Check the account's included usage and spend controls before deployment. Sources: [Render Free](https://render.com/docs/free), [custom domains](https://render.com/docs/custom-domains), [web services](https://render.com/docs/web-services).
