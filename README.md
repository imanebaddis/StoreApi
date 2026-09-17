# BookStore

Progetto d'esame PMO: API ASP.NET Core in `StoreApi/`, modelli e repository in `StoreApi.Infra/`, prototipo frontend in `BookStore.Web/`.

## Stato attuale

- Catalogo libri: API CRUD e persistenza PostgreSQL su Supabase tramite `store.books`.
- Interfaccia: catalogo, ricerca, dettagli e preferiti locali nel browser. Per i titoli dimostrativi arricchisce la presentazione con contenuti statici.
- Utenti, abbonamenti e carte: implementazioni ancora in memoria. Il pagamento è simulato.

La versione attuale è un prototipo locale. Le API di scrittura non hanno ancora autorizzazione; non esporre il server su Internet. La gestione password e carte esistente richiede una revisione prima di usarla con dati reali.

Il progetto F# nella radice, le viste MVC e `wwwroot/lib` appartengono al vecchio template e non sono l'interfaccia BookStore corrente. Le librerie vendorizzate restano con le rispettive licenze per non rompere quel template.

## Avvio locale

Seguire [DATABASE_SETUP.md](DATABASE_SETUP.md) per configurare la connessione server senza includere credenziali nel repository e installare la tabella libri. Avviare l'API in Development su `http://127.0.0.1:5187`, poi avviare `BookStore.Web/server.js` con Node.js 18 o superiore. L'interfaccia è su `http://localhost:4173` e usa l'API reale per impostazione predefinita.

Per provare senza Supabase, usare `BookStore__DemoMode=true` in Development e avviare l'API sulla porta 5178; il frontend può usare `BOOKSTORE_API_URL=http://127.0.0.1:5178` e `BOOKSTORE_DATA_MODE=demo`.
