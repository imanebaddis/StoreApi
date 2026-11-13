(() => {
  const config = window.BOOKSTORE_CONFIG || {};
  const area = document.querySelector('#auth-area');
  const status = document.querySelector('#auth-status');
  const ready = config.googleEnabled === true && /^https:\/\//.test(config.supabaseUrl || '') && /^sb_publishable_/.test(config.supabasePublishableKey || '') && Boolean(window.supabase?.createClient);
  let client;
  let currentIdentity = null;

  function message(value) { status.textContent = value; status.hidden = !value; }
  function render() {
    if (currentIdentity) {
      const label = document.createElement('span');
      label.className = 'auth-identity';
      label.textContent = currentIdentity.email || 'Account collegato';
      const role = document.createElement('span');
      role.className = 'auth-role';
      role.textContent = currentIdentity.role === 'admin' ? 'Admin' : 'Cliente';
      const button = document.createElement('button');
      button.type = 'button';
      button.className = 'auth-button';
      button.textContent = 'Esci';
      button.addEventListener('click', signOut);
      area.replaceChildren(label, role, button);
      return;
    }
    const button = document.createElement('button');
    button.type = 'button';
    button.className = 'auth-button';
    button.textContent = 'Accedi con Google';
    button.disabled = !ready;
    if (!ready) button.title = 'Accesso Google in attesa della configurazione Supabase';
    button.addEventListener('click', signIn);
    area.replaceChildren(button);
  }
  async function refreshIdentity() {
    if (!client) return;
    const { data, error } = await client.auth.getSession();
    if (error || !data.session?.access_token) {
      currentIdentity = null;
      render();
      if (error) message('La sessione non è disponibile. Riprova ad accedere.');
      return;
    }
    try {
      const response = await fetch(`${config.apiBaseUrl || ''}/api/me`, {
        headers: { Authorization: `Bearer ${data.session.access_token}` },
        cache: 'no-store'
      });
      if (!response.ok) throw new Error(String(response.status));
      currentIdentity = await response.json();
      message('');
    } catch {
      currentIdentity = null;
      message('Accesso Google riuscito, ma il profilo BookStore non è disponibile. Riprova tra poco.');
    }
    render();
  }
  async function signIn() {
    if (!client) return;
    message('Apertura dell’accesso Google…');
    const { error } = await client.auth.signInWithOAuth({
      provider: 'google',
      options: { redirectTo: `${location.origin}/?auth=callback` }
    });
    if (error) message('Non è stato possibile avviare l’accesso Google. Riprova.');
  }
  async function signOut() {
    if (!client) return;
    const { error } = await client.auth.signOut();
    if (error) { message('Non è stato possibile uscire. Riprova.'); return; }
    currentIdentity = null;
    render();
    message('Hai effettuato l’uscita.');
  }
  render();
  if (!ready) {
    message('Accesso Google in preparazione. Il catalogo è disponibile senza account.');
    return;
  }
  const callbackError = new URLSearchParams(location.search).has('error');
  client = window.supabase.createClient(config.supabaseUrl, config.supabasePublishableKey, {
    auth: { flowType: 'pkce', detectSessionInUrl: true, persistSession: true, autoRefreshToken: true }
  });
  client.auth.onAuthStateChange(() => { setTimeout(refreshIdentity, 0); });
  refreshIdentity().finally(() => {
    const params = new URLSearchParams(location.search);
    if (params.has('auth') || params.has('code') || params.has('error')) {
      for (const key of ['auth', 'code', 'error', 'error_code', 'error_description']) params.delete(key);
      const query = params.toString();
      history.replaceState(null, '', location.pathname + (query ? `?${query}` : '') + location.hash);
    }
    if (callbackError) message('Accesso Google annullato o non riuscito. Puoi riprovare.');
  });
})();
