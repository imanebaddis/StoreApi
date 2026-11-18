const http = require('node:http');
const fs = require('node:fs');
const path = require('node:path');
const { configScript } = require('./public-config');

const port = Number(process.env.PORT || 4174);
const api = process.env.BOOKSTORE_API_URL || 'http://127.0.0.1:5187';
const dataMode = process.env.BOOKSTORE_DATA_MODE || 'api';
const root = __dirname;
const types = { '.html': 'text/html', '.css': 'text/css', '.js': 'text/javascript', '.json': 'application/json' };

http.createServer(async (req, res) => {
  const { pathname, search } = new URL(req.url, 'http://localhost');
  if (pathname === '/config.js') {
    res.writeHead(200, { 'Content-Type': 'text/javascript; charset=utf-8', 'Cache-Control': 'no-store' });
    res.end(configScript());
    return;
  }
  if (pathname === '/api/me' || /^\/api\/(books|categories)(\/\d+)?$/.test(pathname)) {
    if (req.method !== 'GET') { res.writeHead(405); res.end(); return; }
    try {
      const headers = pathname === '/api/me' && req.headers.authorization ? { Authorization: req.headers.authorization } : {};
      const query = /^\/api\/books$/.test(pathname) ? search : '';
      const response = await fetch(new URL(pathname + query, api), { headers, signal: AbortSignal.timeout(5000) });
      res.writeHead(response.status, { 'Content-Type': response.headers.get('content-type') || 'application/json', 'Cache-Control': 'no-store', 'X-BookStore-Mode': dataMode });
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
