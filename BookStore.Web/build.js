const fs = require('node:fs');
const path = require('node:path');
const { configScript } = require('./public-config');

const destination = path.join(__dirname, 'dist');
fs.mkdirSync(path.join(destination, 'vendor'), { recursive: true });
for (const file of ['index.html', 'styles.css', 'app.js', 'auth.js']) {
  fs.copyFileSync(path.join(__dirname, file), path.join(destination, file));
}
fs.copyFileSync(path.join(__dirname, 'vendor', 'supabase-2.116.0.js'), path.join(destination, 'vendor', 'supabase-2.116.0.js'));
fs.copyFileSync(path.join(__dirname, 'vendor', 'LICENSE'), path.join(destination, 'vendor', 'LICENSE'));
fs.writeFileSync(path.join(destination, 'config.js'), configScript({ forBuild: true }));
console.log('BookStore static site built in dist.');
