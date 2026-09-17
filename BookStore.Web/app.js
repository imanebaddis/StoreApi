const demoBooks = [
  { id: 1, title: 'La biblioteca di mezzanotte', author: 'Matt Haig', category: 'Narrativa', year: '2020', description: 'Tra una vita e l’altra c’è una biblioteca. Ogni libro apre una possibilità diversa e invita a chiedersi che cosa rende una vita davvero propria.', tone: 'indigo', symbol: '✧' },
  { id: 2, title: 'Il barone rampante', author: 'Italo Calvino', category: 'Classici', year: '1957', description: 'Cosimo sale su un albero e decide di non scendere più. Una storia di libertà, immaginazione e sguardi nuovi sul mondo.', tone: 'moss', symbol: '♣' },
  { id: 3, title: 'Le otto montagne', author: 'Paolo Cognetti', category: 'Narrativa', year: '2016', description: 'Un’amicizia attraversa estati, distanze e montagne. Un romanzo sul tempo che passa e sui luoghi in cui ci riconosciamo.', tone: 'blue', symbol: '△' },
  { id: 4, title: 'L’arte di correre', author: 'Haruki Murakami', category: 'Saggi', year: '2007', description: 'Correre e scrivere diventano due modi di trovare ritmo, disciplina e spazio per ascoltare se stessi.', tone: 'amber', symbol: '◌' },
  { id: 5, title: 'Il piccolo principe', author: 'Antoine de Saint-Exupéry', category: 'Classici', year: '1943', description: 'Un viaggio fra pianeti e incontri singolari, raccontato con la semplicità delle storie che sanno parlare a ogni età.', tone: 'rose', symbol: '✶' },
  { id: 6, title: 'Breve storia di quasi tutto', author: 'Bill Bryson', category: 'Scienza', year: '2003', description: 'Una passeggiata curiosa tra le grandi domande della scienza: dall’universo alle particelle, passando per la Terra e la vita.', tone: 'teal', symbol: '◎' },
  { id: 7, title: 'Stoner', author: 'John Williams', category: 'Narrativa', year: '1965', description: 'La vita apparentemente ordinaria di un professore diventa un racconto intenso su desideri, scelte e resistenza quotidiana.', tone: 'plum', symbol: '◆' },
  { id: 8, title: 'Sapiens', author: 'Yuval Noah Harari', category: 'Saggi', year: '2011', description: 'Un percorso attraverso la storia della nostra specie e le idee che hanno cambiato il modo in cui viviamo insieme.', tone: 'forest', symbol: '◈' },
  { id: 9, title: 'Il nome della rosa', author: 'Umberto Eco', category: 'Narrativa', year: '1980', description: 'In un’abbazia medievale, un’indagine tra manoscritti e misteri mette alla prova conoscenza, fede e potere.', tone: 'plum', symbol: '✢' },
  { id: 10, title: 'Le città invisibili', author: 'Italo Calvino', category: 'Classici', year: '1972', description: 'Marco Polo racconta città immaginarie e, attraverso di esse, desideri, memoria e possibilità della vita urbana.', tone: 'blue', symbol: '◇' },
  { id: 11, title: 'L’amica geniale', author: 'Elena Ferrante', category: 'Narrativa', year: '2011', description: 'Due amiche crescono in un quartiere di Napoli, intrecciando ambizioni, rivalità e legami lungo gli anni.', tone: 'rose', symbol: '❋' }
];

const state = { books: demoBooks, source: 'demo', query: '', category: 'Tutti', favorites: readFavorites() };
const app = document.querySelector('#app');
const esc = (value) => String(value ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[c]);
const slug = (book) => `${state.source}:${book.id}`;
const categories = () => ['Tutti', ...new Set(state.books.map(book => book.category))];

function readFavorites() { try { return new Set(JSON.parse(localStorage.getItem('bookstore-favorites-v2') || '[]')); } catch { return new Set(); } }
function saveFavorites() { localStorage.setItem('bookstore-favorites-v2', JSON.stringify([...state.favorites])); document.querySelector('#favorite-count').textContent = [...state.favorites].filter(key => key.startsWith(`${state.source}:`)).length; }
function bookCard(book) {
  const saved = state.favorites.has(slug(book));
  return `<article class="book-card"><a class="cover-link" href="#/libro/${encodeURIComponent(book.id)}" aria-label="Apri ${esc(book.title)}"><div class="book-cover tone-${esc(book.tone)}"><span class="cover-symbol" aria-hidden="true">${esc(book.symbol)}</span><span class="cover-title">${esc(book.title)}</span><span class="cover-author">${esc(book.author)}</span></div></a><div class="card-meta"><span>${esc(book.category)}</span><button class="favorite-button ${saved ? 'is-saved' : ''}" type="button" data-favorite="${esc(book.id)}" aria-label="${saved ? 'Rimuovi dai' : 'Aggiungi ai'} preferiti: ${esc(book.title)}" aria-pressed="${saved}">${saved ? '♥' : '♡'}</button></div><h3><a href="#/libro/${encodeURIComponent(book.id)}">${esc(book.title)}</a></h3><p>${esc(book.author)}</p></article>`;
}
function sourceNotice() { const message = state.source === 'api-demo' ? 'API BookStore collegata in modalità demo: libri temporanei, senza database.' : state.source === 'api' ? 'Catalogo collegato all’API BookStore con dati persistenti. L’API fornisce titolo e autore.' : 'Modalità demo locale: libri di esempio, senza database collegato.'; return `<div class="source-notice" role="status"><span class="status-dot ${state.source !== 'demo' ? 'live' : ''}"></span><span>${message}</span></div>`; }
function home() {
  const featured = state.books[0];
  app.innerHTML = `<section class="hero"><div class="hero-copy"><p class="hero-kicker">La tua prossima lettura inizia qui</p><h1>Apri un libro.<br>Trova un mondo.</h1><p class="hero-text">Storie da scoprire, autori da incontrare, pagine da scegliere con calma.</p><div class="hero-actions"><a class="button-primary" href="#/catalogo">Esplora il catalogo <span aria-hidden="true">↗</span></a><span>${state.books.length} libri da sfogliare</span></div></div><div class="hero-art" aria-hidden="true"><div class="hero-orbit orbit-one"></div><div class="hero-orbit orbit-two"></div><div class="hero-book hero-back"></div><div class="hero-book hero-front"><span>BOOK<br>STORE</span><i>✦</i><small>STORIE DA SCEGLIERE</small></div><div class="hero-caption">Una storia per ogni momento</div></div></section>${sourceNotice()}<section class="section-intro"><div><p class="section-kicker">Da dove iniziare</p><h2>Una selezione da sfogliare</h2></div><a class="text-link" href="#/catalogo">Vedi tutto il catalogo <span aria-hidden="true">↗</span></a></section><div class="book-grid">${state.books.slice(0, 4).map(bookCard).join('')}</div><section class="closing-band"><span aria-hidden="true">✦</span><p>Ogni libro è un invito a guardare più lontano.</p><a href="#/catalogo">Trova la tua prossima lettura</a></section>`;
}
function catalog(favoritesOnly = false) {
  const title = favoritesOnly ? 'I tuoi preferiti' : 'Il catalogo';
  const filtered = state.books.filter(b => (!favoritesOnly || state.favorites.has(slug(b))) && (state.category === 'Tutti' || b.category === state.category) && `${b.title} ${b.author}`.toLocaleLowerCase('it').includes(state.query.toLocaleLowerCase('it')));
  app.innerHTML = `<section class="page-heading"><p class="section-kicker">${favoritesOnly ? 'Salvati in questo browser' : 'Scopri la collezione'}</p><h1>${title}</h1><p>${favoritesOnly ? 'Ritrova i titoli che hai segnato durante la visita.' : 'Cerca un titolo, segui un autore, lasciati incuriosire.'}</p></section>${sourceNotice()}<section class="catalog-tools" aria-label="Filtri del catalogo"><label class="search-field"><span class="sr-only">Cerca per titolo o autore</span><span aria-hidden="true">⌕</span><input id="search" type="search" placeholder="Cerca un titolo o un autore" value="${esc(state.query)}"></label><div class="filter-list" role="group" aria-label="Filtra per categoria">${categories().map(c => `<button type="button" class="filter-chip ${state.category === c ? 'selected' : ''}" data-category="${esc(c)}" aria-pressed="${state.category === c}">${esc(c)}</button>`).join('')}</div></section><div class="result-count">${filtered.length} ${filtered.length === 1 ? 'libro' : 'libri'}</div>${filtered.length ? `<div class="book-grid">${filtered.map(bookCard).join('')}</div>` : `<div class="empty-state"><span aria-hidden="true">⌁</span><h2>${favoritesOnly && !filtered.length && ![...state.favorites].some(key => key.startsWith(`${state.source}:`)) ? 'Ancora nessun preferito' : 'Nessun libro trovato'}</h2><p>${favoritesOnly && ![...state.favorites].some(key => key.startsWith(`${state.source}:`)) ? 'Apri un libro e seleziona il cuore per ritrovarlo qui.' : 'Prova un altro titolo, autore o categoria.'}</p><a href="#/catalogo">Esplora il catalogo</a></div>`}`;
  const input = document.querySelector('#search');
  input.addEventListener('input', e => { const start = e.target.selectionStart; state.query = e.target.value; catalog(favoritesOnly); const next = document.querySelector('#search'); next.focus(); next.setSelectionRange(start, start); });
}
function detail(id) {
  const book = state.books.find(b => String(b.id) === id);
  if (!book) { app.innerHTML = `<div class="empty-state"><h1>Libro non trovato</h1><p>Questo titolo non è presente nel catalogo disponibile.</p><a href="#/catalogo">Torna al catalogo</a></div>`; return; }
  const saved = state.favorites.has(slug(book));
  app.innerHTML = `<div class="breadcrumbs"><a href="#/catalogo">Catalogo</a><span aria-hidden="true">/</span><span>${esc(book.title)}</span></div>${sourceNotice()}<article class="detail-layout"><div class="detail-cover-wrap"><div class="book-cover detail-cover tone-${esc(book.tone)}"><span class="cover-symbol" aria-hidden="true">${esc(book.symbol)}</span><span class="cover-title">${esc(book.title)}</span><span class="cover-author">${esc(book.author)}</span></div><p>Copertina illustrativa del prototipo</p></div><div class="detail-copy"><span class="category-pill">${esc(book.category)}</span><h1>${esc(book.title)}</h1><p class="detail-author">di ${esc(book.author)}</p><div class="detail-rule"></div><h2>Il libro</h2><p class="description">${esc(book.description)}</p><div class="detail-facts"><div><span>Autore</span><strong>${esc(book.author)}</strong></div><div><span>Categoria</span><strong>${esc(book.category)}</strong></div>${book.year ? `<div><span>Anno</span><strong>${esc(book.year)}</strong></div>` : ''}</div><button class="button-primary detail-save" type="button" data-favorite="${esc(book.id)}" aria-pressed="${saved}">${saved ? '♥ Salvato nei preferiti' : '♡ Aggiungi ai preferiti'}</button><p class="detail-footnote">I preferiti sono salvati solo in questo browser.</p></div></article><section class="related"><div class="section-intro"><div><p class="section-kicker">Continua a esplorare</p><h2>Altre storie da scoprire</h2></div></div><div class="book-grid">${state.books.filter(b => b.id !== book.id).slice(0, 4).map(bookCard).join('')}</div></section>`;
}
function route() {
  const [_, page, id] = location.hash.match(/^#\/(?:([^/]+))?(?:\/([^/]+))?/) || [];
  document.querySelectorAll('[data-nav]').forEach(a => a.classList.toggle('active', a.dataset.nav === (page === 'catalogo' ? 'catalog' : page === 'preferiti' ? 'favorites' : !page ? 'home' : '')));
  if (page === 'catalogo') catalog(); else if (page === 'preferiti') catalog(true); else if (page === 'libro') detail(decodeURIComponent(id || '')); else home();
  saveFavorites();
}
document.addEventListener('click', e => {
  const favorite = e.target.closest('[data-favorite]');
  if (favorite) { const id = `${state.source}:${favorite.dataset.favorite}`; state.favorites.has(id) ? state.favorites.delete(id) : state.favorites.add(id); route(); }
  const category = e.target.closest('[data-category]');
  if (category) { state.category = category.dataset.category; catalog(location.hash.startsWith('#/preferiti')); }
});
window.addEventListener('hashchange', () => { state.query = ''; state.category = 'Tutti'; route(); window.scrollTo(0, 0); });
async function loadBooks() {
  try {
    const response = await fetch('/api/books');
    if (!response.ok) throw new Error('API non disponibile');
    const data = await response.json();
    if (!Array.isArray(data)) throw new Error('Risposta non valida');
    state.source = response.headers.get('X-BookStore-Mode') === 'demo' ? 'api-demo' : 'api';
    state.books = data.map((b, index) => {
      const normalized = value => String(value || '').normalize('NFKD').replace(/[’‘]/g, "'").toLocaleLowerCase('it');
      const matching = demoBooks.find(d => normalized(d.title) === normalized(b.title));
      return { id: b.id, title: b.title || 'Senza titolo', author: b.author || 'Autore non indicato', category: matching?.category || 'Altri libri', year: matching?.year || '', description: matching?.description || 'La descrizione non è ancora disponibile nel catalogo API.', tone: matching?.tone || ['indigo','moss','blue','amber','rose','teal','plum','forest'][index % 8], symbol: matching?.symbol || '✦' };
    });
  } catch { state.source = 'demo'; state.books = demoBooks; }
  route();
}
loadBooks();
