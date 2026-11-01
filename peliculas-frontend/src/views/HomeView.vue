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
  return raw ? JSON.parse(raw) : null;
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

function resetFormulario() {
  form.value = {
    id: null,
    nombre: '',
    sinopsis: '',
    imagen: '',
  };
  editando.value = false;
}

async function cargarPeliculas() {
  loading.value = true;
  error.value = '';

  try {
    const respuesta = await ObtenerPeliculas(search.value, pagina.value, limite.value);
    peliculas.value = respuesta.data ?? [];
    totalPaginas.value = respuesta.meta?.totalPaginas ?? 1;
    total.value = respuesta.meta?.total ?? 0;
  } catch (err) {
    error.value = err?.message || 'No se pudieron cargar las películas';
  } finally {
    loading.value = false;
  }
}

watch(search, () => {
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
  <main class="home-page">
    <header class="topbar">
      <div>
        <h1>Películas</h1>
        <p v-if="usuario">Bienvenido, {{ usuario.nombre }}</p>
      </div>
      <button class="logout-btn" @click="logout">Cerrar sesión</button>
    </header>

    <section class="panel">
      <h2>{{ editando ? 'Editar película' : 'Nueva película' }}</h2>

      <form class="movie-form" @submit.prevent="guardarPelicula">
        <input v-model="form.nombre" type="text" placeholder="Nombre" />
        <textarea v-model="form.sinopsis" placeholder="Sinopsis" rows="3"></textarea>
        <input v-model="form.imagen" type="text" placeholder="URL de la imagen" />

        <div class="form-actions">
          <button type="submit" class="primary-btn">
            {{ editando ? 'Guardar cambios' : 'Crear película' }}
          </button>
          <button v-if="editando" type="button" class="secondary-btn" @click="resetFormulario">
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
        <select v-model="limite" @change="pagina = 1; cargarPeliculas()">
          <option :value="5">5 por página</option>
          <option :value="10">10 por página</option>
          <option :value="15">15 por página</option>
        </select>
      </div>

      <p class="summary">Mostrando {{ peliculas.length }} de {{ total }} registros</p>

      <div v-if="loading" class="empty-state">Cargando...</div>
      <div v-else-if="peliculas.length === 0" class="empty-state">No hay películas para mostrar.</div>

      <div v-else class="movies-grid">
        <article v-for="pelicula in peliculas" :key="pelicula.id" class="movie-card">
          <img v-if="pelicula.imagen" :src="pelicula.imagen" :alt="pelicula.nombre" />
          <div class="movie-content">
            <h3>{{ pelicula.nombre }}</h3>
            <p>{{ pelicula.sinopsis }}</p>
            <div class="card-actions">
              <button class="secondary-btn" @click="editarPelicula(pelicula)">Editar</button>
              <button class="danger-btn" @click="borrarPelicula(pelicula.id)">Eliminar</button>
            </div>
          </div>
        </article>
      </div>

      <div v-if="totalPaginas > 1" class="pagination">
        <button :disabled="pagina === 1" @click="cambiarPagina(pagina - 1)">Anterior</button>
        <span>Página {{ pagina }} / {{ totalPaginas }}</span>
        <button :disabled="pagina === totalPaginas" @click="cambiarPagina(pagina + 1)">Siguiente</button>
      </div>
    </section>
  </main>
</template>

<style scoped>
.home-page {
  min-height: 100vh;
  padding: 32px 20px;
  background: linear-gradient(135deg, #eef4ff, #f8fafc);
  color: #0f172a;
}

.topbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
  gap: 16px;
}

.topbar h1 {
  margin: 0 0 6px;
}

.panel {
  background: white;
  border-radius: 18px;
  box-shadow: 0 12px 26px rgba(15, 23, 42, 0.08);
  padding: 20px;
  margin-bottom: 24px;
}

.movie-form {
  display: grid;
  gap: 12px;
}

input,
textarea,
select,
button {
  border-radius: 10px;
  border: 1px solid #dbe3f0;
  font: inherit;
}

input,
textarea,
select {
  width: 100%;
  padding: 10px 12px;
  box-sizing: border-box;
}

.form-actions,
.card-actions,
.pagination,
.toolbar {
  display: flex;
  gap: 12px;
  align-items: center;
}

.toolbar {
  display: flex;
  flex-wrap: wrap;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 12px;
}

.toolbar input,
.toolbar select {
  flex: 1 1 180px;
  min-width: 0;
}

button {
  padding: 10px 14px;
  cursor: pointer;
  border: none;
  transition: 0.2s ease;
}

.primary-btn {
  background: #2563eb;
  color: white;
}

.secondary-btn {
  background: #e2e8f0;
  color: #0f172a;
}

.danger-btn,
.logout-btn {
  background: #ef4444;
  color: white;
}

.error {
  color: #b91c1c;
  margin-top: 10px;
  font-weight: 600;
}

.success {
  color: #15803d;
  margin-top: 10px;
  font-weight: 600;
}

.summary {
  margin: 12px 0 18px;
  color: #475569;
}

.movies-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
  gap: 18px;
}

.movie-card {
  border: 1px solid #e2e8f0;
  border-radius: 16px;
  overflow: hidden;
  background: #fff;
}

.movie-card img {
  width: 100%;
  height: 180px;
  object-fit: cover;
  display: block;
  background: #e2e8f0;
}

.movie-content {
  padding: 16px;
}

.movie-content h3 {
  margin-top: 0;
}

.card-actions {
  justify-content: flex-end;
  margin-top: 14px;
}

.pagination {
  justify-content: center;
  margin-top: 18px;
}

.empty-state {
  text-align: center;
  padding: 20px;
  color: #64748b;
}
</style>
