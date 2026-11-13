# BookStore — prototipo frontend

Interfaccia navigabile per il progetto d'esame PMO: pagina iniziale, catalogo, ricerca per titolo/autore, filtro per categoria, dettagli del libro e preferiti locali.

## Avvio

Richiede Node.js 18 o superiore, senza installare pacchetti.

```powershell
cd BookStore.Web
node server.js
```

Aprire <http://localhost:4174>.

Il server tenta di leggere `GET /api/books` dall'API con database su `http://127.0.0.1:5187`. L'API deve essere avviata e configurata secondo `../DATABASE_SETUP.md`. Per usare un altro indirizzo:

```powershell
$env:BOOKSTORE_API_URL = 'http://127.0.0.1:5187'
$env:BOOKSTORE_DATA_MODE = 'api'
node server.js
```

Per usare invece la modalità demo dell'API StoreApi in un altro terminale:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:BookStore__DemoMode = 'true'
dotnet run --project ..\StoreApi\StoreApi.csproj --no-launch-profile --urls http://127.0.0.1:5178
```

Poi avviare il frontend con `BOOKSTORE_API_URL=http://127.0.0.1:5178` e `BOOKSTORE_DATA_MODE=demo`.

Quando l'API non risponde, il catalogo mostra libri di esempio e una chiara indicazione di modalità demo locale. Quando l'API risponde, titolo e autore arrivano dall'API. L'API attuale non fornisce categoria, descrizione, anno o immagini: per i titoli di esempio la UI aggiunge contenuti dimostrativi; per altri titoli usa una categoria generica e segnala che la descrizione non è disponibile. Le copertine sono composizioni grafiche illustrative. Impostare `BOOKSTORE_DATA_MODE=api` solo quando l'API usa il database persistente.

I preferiti sono conservati nel browser locale. Abbonamenti, pagamenti e raccomandazioni non fanno parte di questo incremento.

## Accesso Google

Il pulsante usa Supabase Auth con flusso PKCE. L'utente torna a `/?auth=callback`; il frontend invia l'access token Supabase a `GET /api/me`, che restituisce `userId`, `email` e `role`. Il catalogo resta pubblico. Il ruolo viene letto soltanto dall'API, non dai metadati modificabili dell'utente.

Il progetto Supabase usa l'URL pubblico `https://qpwjzzmnqxtexelhpfqm.supabase.co`. Impostare la chiave **publishable** come variabile `SUPABASE_PUBLISHABLE_KEY` nel server locale o nell'ambiente di build Render. Non inserire `sb_secret_`, `service_role`, password database o Google Client Secret nel frontend. La configurazione rifiuta chiavi diverse da `sb_publishable_...`.

Il provider Google è attivo nel progetto Supabase. Impostare `SUPABASE_GOOGLE_ENABLED=true`; senza il flag il pulsante resta disabilitato con una spiegazione. La allow list dei redirect di Supabase deve comprendere `http://localhost:4174/?auth=callback` e, dopo il deploy, `https://<dominio-frontend>/?auth=callback`. In Google Cloud configurare l'origine del frontend e l'URI callback indicato nella pagina del provider Google di Supabase. Per il sito distribuito su un'origine diversa dall'API, configurare anche l'origine frontend in `BookStore__AllowedOrigins` del backend.

Per una prova locale con chiave pubblicabile già presente nell'ambiente:

```powershell
$env:SUPABASE_PUBLISHABLE_KEY = '<chiave sb_publishable_...>'
$env:SUPABASE_GOOGLE_ENABLED = 'true'
node server.js
```

## Render Static Site

Creare un Render Static Site dalla cartella `BookStore.Web`: build command `node build.js`, publish directory `dist`. Impostare `BOOKSTORE_API_URL` sull'origine HTTPS del backend Render, `BOOKSTORE_DATA_MODE=api`, `SUPABASE_PUBLISHABLE_KEY` sulla chiave pubblicabile e `SUPABASE_GOOGLE_ENABLED=true` dopo aver attivato Google Auth. `build.js` genera `config.js` con soli valori pubblici. Per il test locale della build si può usare un'API su localhost, ma per Render serve l'URL HTTPS effettivo.

La libreria Supabase JS v2.116.0 è inclusa in `vendor/` con licenza MIT, fissata al file distribuito su `https://unpkg.com/@supabase/supabase-js@2.116.0/dist/umd/supabase.js` (SHA-256 `84EE9BF45695C1DD3BA1595B6BCFB0F09672434631351FFC8EBE9140545D5FF6`).

Il 17 settembre 2026 il flusso locale su `http://localhost:4174` è stato verificato end-to-end: scelta dell'account Google, ritorno al sito, profilo Cliente da `/api/me` e uscita. Il test non copre ancora il dominio Render definitivo.
