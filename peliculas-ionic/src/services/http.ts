import { Preferences } from '@capacitor/preferences';

const API_PORT = '5080';

export function getApiBaseUrl(): string {
  const configuredUrl = import.meta.env.VITE_API_URL;
  if (configuredUrl) return configuredUrl.replace(/\/$/, '');

  const hostname = typeof window !== 'undefined' ? window.location.hostname : 'localhost';
  const nativePlatform = typeof window !== 'undefined'
    && Boolean((window as Window & { Capacitor?: { isNativePlatform?: () => boolean } }).Capacitor?.isNativePlatform?.());

  if (nativePlatform && (hostname === 'localhost' || hostname === '127.0.0.1')) {
    return `http://10.0.2.2:${API_PORT}`;
  }

  if (
    hostname.startsWith('192.168.') ||
    hostname.startsWith('10.') ||
    hostname.startsWith('172.') ||
    hostname === '10.0.2.2'
  ) {
    return `http://${hostname}:${API_PORT}`;
  }

  return `http://localhost:${API_PORT}`;
}

export const BASE_URL = getApiBaseUrl();
const TOKEN_KEY = 'auth_token';

export async function getToken(): Promise<string | null> {
  const { value } = await Preferences.get({ key: TOKEN_KEY });
  return value ?? null;
}

export async function setToken(token: string): Promise<void> {
  await Preferences.set({ key: TOKEN_KEY, value: token });
}

export async function clearToken(): Promise<void> {
  await Preferences.remove({ key: TOKEN_KEY });
}

export async function apiFetch<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = await getToken();
  const baseUrl = getApiBaseUrl();
  const fullUrl = `${baseUrl}${path}`;

  const headers: HeadersInit = {
    'Content-Type': 'application/json',
    ...(options.headers ?? {}),
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };

  try {
    const response = await fetch(fullUrl, { ...options, headers });

    if (!response.ok) {
      const body = await response.json().catch(() => null);
      if (response.status === 401) await clearToken();

      throw new Error(body?.message ?? `Error ${response.status} al solicitar ${path}`);
    }

    if (response.status === 204) return undefined as T;
    return (await response.json()) as T;
  } catch (err: unknown) {
    if (err instanceof TypeError && err.message.includes('Failed to fetch')) {
      throw new Error(`No se pudo conectar con el backend (${baseUrl}). Verifica que la API de ASP.NET Core esté corriendo en http://localhost:5080.`);
    }
    throw err;
  }
}