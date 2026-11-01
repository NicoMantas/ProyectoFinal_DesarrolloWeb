const API_URL = 'http://localhost:3000/pelicula';

function getAuthHeaders(extraHeaders = {}) {
  const token = localStorage.getItem('jwt_token');
  const headers = {
    'Content-Type': 'application/json',
    ...extraHeaders,
  };

  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  return headers;
}

async function apiFetch(url, options = {}) {
  const token = localStorage.getItem('jwt_token');
  const method = options.method || 'GET';

  console.log('[Frontend API] Request', {
    url,
    method,
    hasToken: Boolean(token),
    tokenPreview: token ? `${token.slice(0, 12)}...` : null,
    body: options.body || null,
  });

  const respuesta = await fetch(url, options);

  console.log('[Frontend API] Response', {
    url,
    method,
    status: respuesta.status,
    statusText: respuesta.statusText,
    ok: respuesta.ok,
  });

  if (!respuesta.ok) {
    const text = await respuesta.text();
    console.error('[Frontend API] Error HTTP', {
      url,
      method,
      status: respuesta.status,
      statusText: respuesta.statusText,
      responseBody: text,
    });

    throw new Error(`Error al tratar de obtener las peliculas (${respuesta.status}: ${respuesta.statusText})`);
  }

  return respuesta.json();
}

export async function ObtenerPeliculas(nombre = '', pagina = 1, limite = 5) {
  const parametros = new URLSearchParams({
    pagina: pagina.toString(),
    limite: limite.toString(),
  });

  if (nombre.trim()) {
    parametros.set('nombre', nombre.trim());
  }

  return apiFetch(`${API_URL}?${parametros.toString()}`, {
    headers: getAuthHeaders(),
  });
}

export async function CrearPelicula(pelicula) {
  return apiFetch(API_URL, {
    method: 'POST',
    headers: getAuthHeaders(),
    body: JSON.stringify(pelicula),
  });
}

export async function ActualizarPelicula(id, pelicula) {
  return apiFetch(`${API_URL}/${id}`, {
    method: 'PATCH',
    headers: getAuthHeaders(),
    body: JSON.stringify(pelicula),
  });
}

export async function EliminarPelicula(id) {
  return apiFetch(`${API_URL}/${id}`, {
    method: 'DELETE',
    headers: getAuthHeaders(),
  });
}