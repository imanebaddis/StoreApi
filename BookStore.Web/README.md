# BookStore — prototipo frontend

Interfaccia navigabile per il progetto d'esame PMO: pagina iniziale, catalogo, ricerca per titolo/autore, filtro per categoria, dettagli del libro e preferiti locali.

## Avvio

Richiede Node.js 18 o superiore, senza installare pacchetti.

```powershell
cd BookStore.Web
node server.js
```

Aprire <http://localhost:4173>.

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

I preferiti sono conservati nel browser locale. Account, abbonamenti, pagamenti e raccomandazioni non fanno parte di questo incremento.
