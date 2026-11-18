

# BookStore

Progetto d'esame PMO: API ASP.NET Core in `StoreApi/`, modelli e repository in `StoreApi.Infra/`, prototipo frontend in `BookStore.Web/`.
Motivazione e Obiettivi:

L'obiettivo del progetto è sviluppare una piattaforma BookStore che permetta agli utenti di gestire il proprio account, inclusa la registrazione, l'abbonamento e i pagamenti. Il sistema offrirà una gestione completa dei libri, delle categorie, degli abbonamenti e dei metodi di pagamento. Inoltre, basandosi sulla cronologia degli acquisti e delle interazioni, verrà implementata una funzionalità di raccomandazione personalizzata, che suggerirà libri in base alle preferenze e alle attività passate dell'utente. Per l'autenticazione, si offrirà la possibilità di registrarsi e accedere tramite metodi tradizionali o attraverso account di terze parti, come Google e Facebook, per una maggiore flessibilità e comodità

Gestione Utenti Attività: Creare modello utente Implementare la creazione dell'utente Implementare l'aggiornamento dell'utente Implementare l'eliminazione dell'utente Implementare la gestione dei ruoli (admin, cliente)

Gestione Categorie di Libri Attività: Creare modello categoria Implementare la creazione della categoria Implementare l'aggiornamento della categoria Implementare l'eliminazione della categoria

Gestione Libri Attività: Creare modello libro Implementare la creazione del libro Implementare l'aggiornamento del libro Implementare l'eliminazione del libro Visualizzare elenco di libri per i clienti Visualizzare i dettagli del libro

Gestione Abbonamenti Attività: Creare modello abbonamento Implementare i livelli di abbonamento Implementare la creazione dell'abbonamento Implementare l'aggiornamento dell'abbonamento Implementare l'eliminazione dell'abbonamento Gestire lo stato dell'abbonamento (attivo, inattivo, cancellato)

Gestione Pagamenti Attività: Creare modello pagamento Integrare il gateway di pagamento (es. Satispay, PayPal) Implementare la creazione del pagamento Implementare l'aggiornamento del pagamento

Gestire i metodi di pagamento Gestione Blacklist Attività: Creare modello blacklist Implementare il blacklisting degli utenti

Gestire gli utenti in blacklist Gestione Cronologia e Preferiti Attività: Creare modello cronologia attività Implementare il tracciamento delle attività degli utenti Creare modello preferiti Implementare l'aggiunta di libri ai preferiti Implementare la rimozione di libri dai preferiti Visualizzare la cronologia delle attività dell'utente Visualizzare l'elenco dei preferiti

Interfaccia Utente (Frontend) Epic: Interfaccia Utente (Frontend) Attività: Progettare il layout generale del sito Implementare la homepage Implementare la pagina del catalogo libri Implementare la pagina dei dettagli del libro Implementare la pagina degli abbonamenti Implementare la pagina del profilo utente Implementare la gestione dei preferiti nell'interfaccia utente Implementare notifiche e messaggi per gli utenti

Sicurezza e Autenticazione Attività: Implementare la registrazione utente Implementare il login utente Implementare il reset della password Implementare la validazione dei dati Gestire permessi e controllo accessi

Tracciamento Attività Utente e Raccomandazioni Attività: Implementare il tracciamento delle visualizzazioni delle categorie Implementare il tracciamento delle visualizzazioni dei libri Analizzare i dati sulle attività degli utenti Sviluppare un sistema di raccomandazioni basato sulle attività Visualizzare raccomandazioni personalizzate agli utenti

## Stato attuale

- Catalogo libri: API CRUD e persistenza PostgreSQL su Supabase tramite `store.books`, con categoria facoltativa per ogni libro.
- Categorie: API CRUD su `store.categories` (script `database/003_categories.sql`). Lettura pubblica, scritture riservate al ruolo `admin` come per i libri.
- Interfaccia: catalogo, ricerca, filtro per le categorie fornite dall'API, dettagli e preferiti locali nel browser. Per i titoli dimostrativi arricchisce la presentazione con contenuti statici.
- Utenti, abbonamenti e carte: implementazioni ancora in memoria. Il pagamento è simulato.

La versione attuale è un prototipo locale. Le API di scrittura su libri e categorie richiedono un token Supabase Auth con ruolo `admin` (vedi [AUTH_SETUP.md](AUTH_SETUP.md)). La gestione password e carte esistente richiede una revisione prima di usarla con dati reali.

Il progetto F# nella radice, le viste MVC e `wwwroot/lib` appartengono al vecchio template e non sono l'interfaccia BookStore corrente. Le librerie vendorizzate restano con le rispettive licenze per non rompere quel template.

## Avvio locale

Seguire [DATABASE_SETUP.md](DATABASE_SETUP.md) per configurare la connessione server senza includere credenziali nel repository e installare la tabella libri. Avviare l'API in Development su `http://127.0.0.1:5187`, poi avviare `BookStore.Web/server.js` con Node.js 18 o superiore. L'interfaccia è su `http://localhost:4173` e usa l'API reale per impostazione predefinita.

Per provare senza Supabase, usare `BookStore__DemoMode=true` in Development e avviare l'API sulla porta 5178; il frontend può usare `BOOKSTORE_API_URL=http://127.0.0.1:5178` e `BOOKSTORE_DATA_MODE=demo`.

## Endpoint del catalogo

| Metodo e rotta | Accesso | Note |
| --- | --- | --- |
| `GET /api/books` | pubblico | Facoltativo `?categoryId=<id>` per filtrare per categoria. Ogni libro include `categoryId` e `categoryName`. |
| `GET /api/books/{id}` | pubblico | |
| `POST /api/books`, `PUT /api/books/{id}`, `DELETE /api/books/{id}` | `admin` | Corpo: `title`, `author`, `categoryId` facoltativo. Una categoria inesistente restituisce 400. `PUT` sostituisce il libro: senza `categoryId` il libro resta senza categoria. |
| `GET /api/categories` | pubblico | Elenco ordinato per nome: `id`, `name`, `description`. |
| `GET /api/categories/{id}` | pubblico | 404 se assente. |
| `POST /api/categories`, `PUT /api/categories/{id}` | `admin` | Corpo: `name` (1–80 caratteri), `description` facoltativa (max 500). Nome duplicato, anche con maiuscole diverse: 409. |
| `DELETE /api/categories/{id}` | `admin` | I libri della categoria restano nel catalogo senza categoria. |

Senza token le scritture restituiscono 401; con un utente non amministratore 403; con dati non validi 400.
