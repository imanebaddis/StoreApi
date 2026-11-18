# StoreApi book persistence on Supabase Free

This first increment persists only books. Users, subscriptions and bank cards still use their existing in-memory repositories. No paid Supabase feature is required.

1. Verify the intended Supabase project and inspect existing schemas before applying `database/001_books.sql`.
2. Run the SQL in the Supabase SQL editor. The `store` schema is private and is not intended for the Data API.
3. Rotate the database password and secret API key present in older local PMO setup files before using this integration. Do not copy those files into this repository.
4. On this Windows machine, run `Configure-Supabase.ps1`. Paste the **Session pooler** URI from the Supabase **Connect** dialog with `[YOUR-PASSWORD]` still in it, then enter the new password at the hidden prompt. The script checks the project ref and stores an Npgsql connection string in .NET user-secrets for this Windows account. It never writes the password into this repository or command history. For another deployment environment, set the server-side environment variable `ConnectionStrings__StoreDb` in Npgsql format; never put it in `appsettings.json`.
5. Start the C# `StoreApi/StoreApi.csproj` project. Its existing `/api/books` endpoints now read and write `store.books`.

After saving user-secrets, check the remote schema with `dotnet run --project StoreApi/StoreApi.csproj -- --check-books-schema`. If `store.books` is absent in the confirmed BookStore project, apply the prepared migration with `dotnet run --project StoreApi/StoreApi.csproj -- --apply-books-schema`. This command refuses to apply when the table already exists. Neither command prints the password.

This does not create or change a remote database automatically. Applying the SQL and a connection smoke test are still required before calling the deployment complete.

## Local demo without Supabase

For a local prototype, run in `Development` with `BookStore__DemoMode=true` and bind to `http://127.0.0.1:5178`. This uses three sample books in memory and needs no database credentials. Data resets when the process restarts. Demo mode refuses to start outside Development.

## Categorie dei libri (`003_categories.sql`)

Lo script `database/003_categories.sql` crea `store.categories` (nome obbligatorio di 1–80 caratteri e unico senza distinzione tra maiuscole e minuscole, descrizione facoltativa fino a 500 caratteri, `created_at`), abilita Row Level Security e revoca l'accesso ad `anon` e `authenticated` come per `store.profiles`. Aggiunge a `store.books` la colonna facoltativa `category_id` con chiave esterna `on delete set null` e relativo indice: eliminando una categoria i libri restano nel catalogo senza categoria. Inserisce infine le categorie iniziali Narrativa, Classici, Saggi e Scienza (ignorate se già presenti).

Lo script richiede che `001_books.sql` sia già applicato. Verificare lo stato con:

```powershell
dotnet run --project StoreApi/StoreApi.csproj -- --check-categories-schema
```

Il comando mostra le colonne di `store.categories` e di `store.books`. Se `store.categories` è assente nel progetto Supabase confermato, applicare lo script una sola volta con `--apply-categories-schema` (rifiuta di procedere se la tabella esiste già) oppure dall'editor SQL di Supabase.

**Ordine obbligatorio:** applicare `003_categories.sql` **prima** di avviare o distribuire la versione dell'API che include le categorie. Le letture dei libri usano un `left join` su `store.categories`: senza lo script anche `GET /api/books` restituirebbe errore.

I libri già presenti restano senza categoria finché un amministratore non li aggiorna con `PUT /api/books/{id}` indicando `categoryId`.
