const http = require('node:http');
const fs = require('node:fs');
const path = require('node:path');

const port = Number(process.env.PORT || 4173);
const api = process.env.BOOKSTORE_API_URL || 'http://127.0.0.1:5187';
const dataMode = process.env.BOOKSTORE_DATA_MODE || 'api';
const root = __dirname;
const types = { '.html': 'text/html', '.css': 'text/css', '.js': 'text/javascript', '.json': 'application/json' };

http.createServer(async (req, res) => {
  const pathname = new URL(req.url, 'http://localhost').pathname;
  if (pathname === '/api/books' || /^\/api\/books\/\d+$/.test(pathname)) {
    try {
      const response = await fetch(new URL(pathname, api), { signal: AbortSignal.timeout(2500) });
      res.writeHead(response.status, { 'Content-Type': response.headers.get('content-type') || 'application/json', 'X-BookStore-Mode': dataMode });
      res.end(Buffer.from(await response.arrayBuffer()));
    } catch {
      res.writeHead(503, { 'Content-Type': 'application/json' });
      res.end(JSON.stringify({ error: 'API BookStore non disponibile' }));
    }
    return;
  }
  const file = path.join(root, pathname === '/' ? 'index.html' : pathname);
  if (!file.startsWith(root + path.sep) && file !== path.join(root, 'index.html')) {
    res.writeHead(403); res.end(); return;
  }
  fs.readFile(file, (error, data) => {
    if (error) { res.writeHead(404); res.end('Non trovato'); return; }
    res.writeHead(200, { 'Content-Type': (types[path.extname(file)] || 'application/octet-stream') + '; charset=utf-8' });
    res.end(data);
  });
}).listen(port, () => console.log(`BookStore demo: http://localhost:${port} (API: ${api})`));
