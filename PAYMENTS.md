# Pagamenti BookStore: stato e attivazione

Questa integrazione vende **un mese di accesso, con pagamento singolo e senza rinnovo automatico**. I prezzi sono fissati dal server: Basic €4, Silver €7, Gold €10. Non è una sottoscrizione PayPal ricorrente. Il checkout è ospitato da PayPal: BookStore non riceve né conserva numero carta o CVV.

## Cosa è pronto

- `POST /api/payments/orders` richiede un bearer token Supabase Auth valido e `{ "requestId": "UUID", "plan": "Basic|Silver|Gold" }`. Restituisce `paymentId` e `approvalUrl`. Riutilizzare lo stesso `requestId` se la richiesta va in timeout.
- Dopo il ritorno da PayPal, il browser richiama `POST /api/payments/{paymentId}/capture` con lo stesso bearer token. Il `paymentId` va conservato in `sessionStorage` prima di aprire `approvalUrl`; il ritorno PayPal include anche il token dell'ordine nella query. Non fidarsi del solo ritorno nel browser: l'accesso si attiva soltanto dopo capture `COMPLETED` con importo e valuta attesi.
- `GET /api/payments/access` restituisce `{ "active": true|false }` per l'utente autenticato. La stessa verifica va applicata **sul server** a ogni contenuto riservato, non solo alla UI. Il catalogo attuale è pubblico e non serve ancora il testo dei libri.
- `POST /api/payments/webhooks/paypal` verifica la firma presso PayPal; conferma pagamenti completati e revoca l'accesso su rimborso/reversal. Il pagamento e l'identità Supabase sono salvati in `store.payments`. ID PayPal univoci e stato transazionale evitano doppia attivazione. La creazione e la capture usano `PayPal-Request-Id` stabile.
- Gli endpoint legacy `/api/subscriptions/*` che attivavano abbonamenti con pagamenti simulati restituiscono `410`, salvo `/plans`. In produzione anche `/api/users/*` legacy è disabilitato in attesa dell'integrazione Supabase Auth.

## Prima prova sandbox

1. Applicare `database/002_payments.sql` al database Postgres **dopo una revisione**. La tabella ha RLS attivo e nessuna policy Data API: usare la connessione Postgres privilegiata già presente solo nel server. Non inserire la connection string in Git.
2. Preparare un'app PayPal REST **Sandbox** e un account acquirente sandbox. Configurare il webhook sandbox su un endpoint HTTPS pubblico `https://<host>/api/payments/webhooks/paypal` e sottoscrivere almeno `PAYMENT.CAPTURE.COMPLETED`, `PAYMENT.CAPTURE.REFUNDED`, `PAYMENT.CAPTURE.REVERSED`. Salvare il relativo Webhook ID.
3. Impostare in segreti dell'ambiente server: `ConnectionStrings__StoreDb`, `Supabase__Url`, `Supabase__PublishableKey`, `PayPal__Environment=Sandbox`, `PayPal__ClientId`, `PayPal__ClientSecret`, `PayPal__WebhookId`, `PayPal__ReturnUrl`, `PayPal__CancelUrl`. Return e cancel devono essere URL HTTPS assoluti. Solo la publishable key può raggiungere il client; ClientSecret, WebhookId e connection string restano sul server.
4. Collegare la UI al contratto sopra dopo il login Google tramite Supabase. Mostrare esplicitamente "un mese, pagamento singolo, nessun rinnovo automatico" prima della conferma. Il browser deve chiamare l'API dallo stesso sito o con CORS configurato per l'origine esatta.
5. Provare con account sandbox: annullamento, pagamento riuscito, doppio click/riprova con stesso `requestId`, webhook duplicato, utente diverso che tenta la capture, importo inatteso, rimborso e accesso dopo la scadenza. Confrontare `store.payments` con la cronologia PayPal. Non usare il Webhooks Simulator per concludere che la verifica firma funziona: serve una consegna reale sandbox.

## Passaggio live

L'utente deve [aprire e verificare un conto PayPal Business](https://developer.paypal.com/api/rest/production/) e creare l'app REST **Live** in PayPal Developer. La verifica del conto e l'eventuale revisione PayPal dipendono da PayPal: avviarle subito per la scadenza di metà ottobre. Il dominio `techspace.tech` è già disponibile su Hostinger, ma occorre scegliere l'hosting dell'API prima di configurare DNS e URL definitivi. Serve un endpoint raggiungibile su [HTTPS porta 443](https://developer.paypal.com/api/rest/webhooks/rest/), per ritorno checkout e webhook. Configurare **nuovi** Client ID, Client Secret e Webhook ID live; quelli sandbox non funzionano in live. Solo dopo i test sandbox, impostare `PayPal__Environment=Live`, i URL HTTPS definitivi e i segreti live nel gestore segreti dell'hosting. Verificare una transazione reale di importo minimo solo con autorizzazione esplicita dell'utente.

## Limiti aperti

- Mancano conto Business verificato, credenziali sandbox/live, dominio HTTPS pubblico e collegamento UI/auth dell'altra attività: nessun pagamento reale è stato eseguito.
- La revoca da webhook cerca il pagamento tramite `supplementary_data.related_ids` (capture o ordine), il link `rel=up` verso la capture e, per i reversal, l'ID della risorsa. Se l'evento non contiene alcun riferimento risponde 422 e lo registra nei log; se il pagamento non è ancora registrato risponde 503 perché PayPal ritenti. Un pagamento rimborsato non può più essere completato. Queste forme dell'evento sono verificate solo con dati simulati: controllare gli eventi effettivi in sandbox, anche per i rimborsi parziali; in caso di rimborso parziale l'implementazione revoca l'intero mese per prudenza.
- Non sono implementati rinnovi automatici, upgrade di piano, prorata o piani ricorrenti PayPal. Se è richiesto un addebito mensile automatico, serve una seconda integrazione con PayPal Subscriptions API e la relativa gestione del ciclo di vita.
- La migrazione non è stata applicata a Supabase e la prova end-to-end con PayPal non è possibile senza accesso agli account. Il progetto compila e gli endpoint legacy sono stati verificati localmente in modalità demo.

Riferimenti: [PayPal Orders v2](https://developer.paypal.com/api/orders/v2/), [idempotenza PayPal](https://developer.paypal.com/reference/guidelines/idempotency/), [verifica webhook](https://developer.paypal.com/api/rest/webhooks/rest/), [verifica utente Supabase](https://supabase.com/docs/guides/auth/jwts).
