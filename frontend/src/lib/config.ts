const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5209/api';
const hubUrl = import.meta.env.VITE_HUB_URL ?? 'http://localhost:5209/hubs/alerts';
const mapStyleUrl =
  import.meta.env.VITE_MAP_STYLE_URL ?? 'https://tiles.openfreemap.org/styles/liberty';

export const config = {
  apiBaseUrl: apiBaseUrl.replace(/\/$/, ''),
  hubUrl,
  mapStyleUrl,
  devicesPollMs: 15_000,
};
