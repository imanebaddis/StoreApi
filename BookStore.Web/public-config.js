const projectUrl = 'https://qpwjzzmnqxtexelhpfqm.supabase.co';

function publicConfig({ forBuild = false } = {}) {
  const key = process.env.SUPABASE_PUBLISHABLE_KEY || '';
  if (key && !/^sb_publishable_[A-Za-z0-9_-]+$/.test(key)) {
    throw new Error('SUPABASE_PUBLISHABLE_KEY must be a Supabase publishable key. Secret and service_role keys are not accepted.');
  }
  if (forBuild && !process.env.BOOKSTORE_API_URL) {
    throw new Error('Set BOOKSTORE_API_URL to the deployed HTTPS API origin before building the static site.');
  }
  const apiBaseUrl = forBuild ? process.env.BOOKSTORE_API_URL : '';
  if (apiBaseUrl && !/^https:\/\//.test(apiBaseUrl) && !/^http:\/\/(localhost|127\.0\.0\.1)(:\d+)?$/.test(apiBaseUrl)) {
    throw new Error('BOOKSTORE_API_URL must use HTTPS outside localhost.');
  }
  return {
    apiBaseUrl: apiBaseUrl?.replace(/\/$/, '') || '',
    dataMode: process.env.BOOKSTORE_DATA_MODE || 'api',
    supabaseUrl: process.env.SUPABASE_URL || projectUrl,
    supabasePublishableKey: key,
    googleEnabled: process.env.SUPABASE_GOOGLE_ENABLED === 'true'
  };
}

function configScript(options) {
  return `window.BOOKSTORE_CONFIG = ${JSON.stringify(publicConfig(options)).replace(/</g, '\\u003c')};\n`;
}

module.exports = { publicConfig, configScript };
