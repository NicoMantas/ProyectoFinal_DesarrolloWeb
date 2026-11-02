<script setup>
import { computed, onMounted, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import {
  ObtenerPeliculas,
  CrearPelicula,
  ActualizarPelicula,
  EliminarPelicula,
} from '../services/peliculaService';

const router = useRouter();

const usuario = computed(() => {
  const raw = localStorage.getItem('usuario');
  if (!raw) return null;

  try {
    return JSON.parse(raw);
  } catch {
    return null;
  }
});

const peliculas = ref([]);
const search = ref('');
const pagina = ref(1);
const limite = ref(5);
const totalPaginas = ref(1);
const total = ref(0);
const loading = ref(false);
const error = ref('');
const success = ref('');
const form = ref({
  id: null,
  nombre: '',
  sinopsis: '',
  imagen: '',
});
const editando = ref(false);
const imagenRota = ref({});
let requestId = 0;
let searchTimer;

function resetFormulario() {
  form.value = {
    id: null,
    nombre: '',
    sinopsis: '',
    imagen: '',
  };
  editando.value = false;
}

function marcarImagenRota(id) {
  imagenRota.value = { ...imagenRota.value, [id]: true };
}

async function cargarPeliculas() {
  const id = ++requestId;
  loading.value = true;
  error.value = '';

  try {
    const respuesta = await ObtenerPeliculas(search.value, pagina.value, limite.value);
    if (id !== requestId) return;

    const totalRegistros = respuesta.meta?.total ?? 0;
    const paginas = respuesta.meta?.totalPaginas ?? 1;

    if (totalRegistros > 0 && paginas > 0 && pagina.value > paginas) {
      pagina.value = paginas;
      return cargarPeliculas();
    }

    peliculas.value = respuesta.data ?? [];
    totalPaginas.value = Math.max(paginas, 1);
    total.value = totalRegistros;
  } catch (err) {
    if (id !== requestId) return;
    error.value = err?.message || 'No se pudieron cargar las películas';
  } finally {
    if (id === requestId) loading.value = false;
  }
}

watch(search, () => {
  clearTimeout(searchTimer);
  searchTimer = setTimeout(() => {
    pagina.value = 1;
    cargarPeliculas();
  }, 300);
});

watch(limite, () => {
  pagina.value = 1;
  cargarPeliculas();
});

function cambiarPagina(nuevaPagina) {
  if (nuevaPagina < 1 || nuevaPagina > totalPaginas.value) {
    return;
  }

  pagina.value = nuevaPagina;
  cargarPeliculas();
}

async function guardarPelicula() {
  const payload = {
    nombre: form.value.nombre.trim(),
    sinopsis: form.value.sinopsis.trim(),
    imagen: form.value.imagen.trim(),
  };

  if (!payload.nombre || !payload.sinopsis || !payload.imagen) {
    error.value = 'Todos los campos son obligatorios';
    return;
  }

  success.value = '';
  error.value = '';

  try {
    if (editando.value && form.value.id) {
      await ActualizarPelicula(form.value.id, payload);
      success.value = 'Película actualizada correctamente';
    } else {
      await CrearPelicula(payload);
      success.value = 'Película creada correctamente';
    }

    resetFormulario();
    await cargarPeliculas();
  } catch (err) {
    error.value = err?.message || 'No se pudo guardar la película';
  }
}

function editarPelicula(pelicula) {
  form.value = {
    id: pelicula.id,
    nombre: pelicula.nombre,
    sinopsis: pelicula.sinopsis,
    imagen: pelicula.imagen,
  };
  editando.value = true;
  window.scrollTo({ top: 0, behavior: 'smooth' });
}

async function borrarPelicula(id) {
  const confirmar = window.confirm('¿Seguro que deseas eliminar esta película?');
  if (!confirmar) return;

  try {
    await EliminarPelicula(id);
    if (form.value.id === id) {
      resetFormulario();
    }
    await cargarPeliculas();
  } catch (err) {
    error.value = err?.message || 'No se pudo eliminar la película';
  }
}

function logout() {
  localStorage.removeItem('jwt_token');
  localStorage.removeItem('usuario');
  router.push('/login');
}

onMounted(() => {
  if (!localStorage.getItem('jwt_token')) {
    router.push('/login');
    return;
  }

  cargarPeliculas();
});
</script>

<template>
  <main class="home-screen">
    <header class="topbar">
      <div class="brand">
        <span class="mark">P</span>
        <div>
          <p class="eyebrow">Colección</p>
          <h1>Películas</h1>
          <p v-if="usuario">Hola, {{ usuario.nombre }}</p>
        </div>
      </div>
      <div class="userbox">
        <button class="ghost-btn" @click="logout">Cerrar sesión</button>
      </div>
    </header>

    <div class="layout">
      <section class="panel">
        <h2>{{ editando ? 'Editar película' : 'Nueva película' }}</h2>

        <form class="movie-form" @submit.prevent="guardarPelicula">
          <input v-model="form.nombre" type="text" placeholder="Nombre" />
          <textarea v-model="form.sinopsis" placeholder="Sinopsis" rows="4"></textarea>
          <input v-model="form.imagen" type="url" placeholder="URL de la imagen" />

          <div class="form-actions">
            <button type="submit" class="primary-btn">
              {{ editando ? 'Guardar cambios' : 'Crear película' }}
            </button>
            <button v-if="editando" type="button" class="ghost-btn" @click="resetFormulario">
              Cancelar
            </button>
          </div>
        </form>

        <p v-if="error" class="error">{{ error }}</p>
        <p v-if="success" class="success">{{ success }}</p>
      </section>

      <section class="panel">
        <div class="toolbar">
          <input v-model="search" type="search" placeholder="Buscar por nombre" />
          <select v-model.number="limite">
            <option :value="5">5 por página</option>
            <option :value="10">10 por página</option>
            <option :value="15">15 por página</option>
          </select>
        </div>

        <p class="summary">Mostrando {{ peliculas.length }} de {{ total }} registros</p>

        <div v-if="loading" class="empty-state">Cargando la cartelera...</div>
        <div v-else-if="peliculas.length === 0" class="empty-state">No hay películas para mostrar.</div>

        <div v-else class="catalog">
          <article v-for="pelicula in peliculas" :key="pelicula.id" class="poster">
            <img
              v-if="pelicula.imagen && !imagenRota[pelicula.id]"
              class="poster-media"
              :src="pelicula.imagen"
              :alt="pelicula.nombre"
              @error="marcarImagenRota(pelicula.id)"
            />
            <div v-else class="poster-fallback">{{ pelicula.nombre.slice(0, 1) }}</div>
            <div class="poster-body">
              <h3>{{ pelicula.nombre }}</h3>
              <p>{{ pelicula.sinopsis }}</p>
              <div class="card-actions">
                <button class="ghost-btn" @click="editarPelicula(pelicula)">Editar</button>
                <button class="danger-btn" @click="borrarPelicula(pelicula.id)">Eliminar</button>
              </div>
            </div>
          </article>
        </div>

        <div v-if="totalPaginas > 1" class="pagination">
          <button class="ghost-btn" :disabled="pagina === 1" @click="cambiarPagina(pagina - 1)">Anterior</button>
          <span>Página {{ pagina }} / {{ totalPaginas }}</span>
          <button class="ghost-btn" :disabled="pagina === totalPaginas" @click="cambiarPagina(pagina + 1)">Siguiente</button>
        </div>
      </section>
    </div>
  </main>
</template>
