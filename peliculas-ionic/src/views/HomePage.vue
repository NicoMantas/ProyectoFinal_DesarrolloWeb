<template>
  <ion-page>
    <ion-header class="ion-no-border">
      <ion-toolbar>
        <ion-title class="catalog-title">Cartelera</ion-title>
      </ion-toolbar>
    </ion-header>

    <ion-content class="catalog-page">
      <div class="topbar">
        <div>
          <p class="eyebrow">Mi colección</p>
          <h2>Películas</h2>
        </div>
        <div class="topbar-actions">
          <ion-button class="logout-button" fill="outline" @click="cerrarSesion">
            <ion-icon slot="start" :icon="logOutOutline" />
            Cerrar sesión
          </ion-button>
          <ion-button shape="round" @click="abrirModalCrear">
            <ion-icon slot="start" :icon="add" />
            Nueva
          </ion-button>
        </div>
      </div>

      <ion-searchbar
        :value="busqueda"
        placeholder="Buscar por nombre..."
        @ionInput="onBuscar"
        class="searchbar-custom"
      />

      <p v-if="cargando && peliculas.length === 0" class="empty-state">Cargando la cartelera...</p>
      <p v-else-if="peliculas.length === 0" class="empty-state">No hay películas para mostrar.</p>

      <div v-else class="catalog">
        <article v-for="pelicula in peliculas" :key="pelicula.id" class="poster-card" @click="abrirModalEditar(pelicula)">
          <img
            v-if="pelicula.imagen && !imagenRota[pelicula.id]"
            :src="pelicula.imagen"
            :alt="pelicula.nombre"
            @error="marcarImagenRota(pelicula.id)"
          />
          <div v-else class="poster-fallback">{{ pelicula.nombre.slice(0, 1) }}</div>
          <div class="poster-copy">
            <h3>{{ pelicula.nombre }}</h3>
            <p>{{ pelicula.sinopsis }}</p>
            <ion-button fill="clear" size="small" color="danger" @click.stop="confirmarEliminar(pelicula)">
              Eliminar
            </ion-button>
          </div>
        </article>
      </div>

      <ion-infinite-scroll @ionInfinite="cargarMas" :disabled="pagina >= totalPaginas">
        <ion-infinite-scroll-content loading-text="Cargando más películas..." />
      </ion-infinite-scroll>
    </ion-content>

    <pelicula-form-modal
      :is-open="modalAbierto"
      :pelicula="peliculaSeleccionada"
      @close="modalAbierto = false"
      @save="guardar"
    />
  </ion-page>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import {
  IonPage, IonHeader, IonToolbar, IonTitle, IonContent, IonSearchbar,
  IonInfiniteScroll, IonInfiniteScrollContent,
  IonIcon, alertController, IonButton,
} from '@ionic/vue';
import { add, logOutOutline } from 'ionicons/icons';
import { useRouter } from 'vue-router';
import PeliculaFormModal from '@/components/PeliculaFormModal.vue';
import { fetchPeliculas, createPelicula, updatePelicula, deletePelicula } from '@/services/pelicula.service';
import { clearToken } from '@/services/http';
import type { CreatePeliculaInput, Pelicula } from '@/types/pelicula';

const router = useRouter();

const LIMITE = 10;

const peliculas = ref<Pelicula[]>([]);
const busqueda = ref('');
const pagina = ref(1);
const totalPaginas = ref(1);
const cargando = ref(false);
const imagenRota = ref<Record<number, boolean>>({});
let debounceTimer: ReturnType<typeof setTimeout>;

const modalAbierto = ref(false);
const peliculaSeleccionada = ref<Pelicula | null>(null);

function marcarImagenRota(id: number) {
  imagenRota.value = { ...imagenRota.value, [id]: true };
}

async function cargarPagina(nuevaPagina: number, reemplazar: boolean) {
  cargando.value = true;

  try {
    const respuesta = await fetchPeliculas(busqueda.value, nuevaPagina, LIMITE);
    peliculas.value = reemplazar ? respuesta.data : [...peliculas.value, ...respuesta.data];
    pagina.value = respuesta.meta.pagina;
    totalPaginas.value = Math.max(respuesta.meta.totalPaginas, 1);
  } catch (error) {
    const message = error instanceof Error ? error.message : 'Error al cargar películas.';
    const alert = await alertController.create({
      header: 'Error de conexión',
      message,
      buttons: ['Aceptar'],
    });
    await alert.present();
  } finally {
    cargando.value = false;
  }
}

onMounted(() => cargarPagina(1, true));

function onBuscar(event: CustomEvent) {
  busqueda.value = event.detail.value ?? '';
  clearTimeout(debounceTimer);
  debounceTimer = setTimeout(() => cargarPagina(1, true), 350);
}

async function cargarMas(event: CustomEvent) {
  if (pagina.value < totalPaginas.value) {
    await cargarPagina(pagina.value + 1, false);
  }
  (event.target as HTMLIonInfiniteScrollElement).complete();
}

function abrirModalCrear() {
  peliculaSeleccionada.value = null;
  modalAbierto.value = true;
}

function abrirModalEditar(pelicula: Pelicula) {
  peliculaSeleccionada.value = pelicula;
  modalAbierto.value = true;
}

async function recargarListaActual(elementosNuevos = 0) {
  const cantidadCargada = Math.max(peliculas.value.length + elementosNuevos, LIMITE);
  const respuesta = await fetchPeliculas(busqueda.value, 1, cantidadCargada);
  peliculas.value = respuesta.data;
  totalPaginas.value = Math.ceil(respuesta.meta.total / LIMITE);
}

async function guardar(data: CreatePeliculaInput) {
  const esNueva = !peliculaSeleccionada.value;

  try {
    if (peliculaSeleccionada.value) {
      await updatePelicula(peliculaSeleccionada.value.id, data);
    } else {
      await createPelicula(data);
    }

    modalAbierto.value = false;
    await recargarListaActual(esNueva ? 1 : 0);
  } catch (error) {
    const message = error instanceof Error ? error.message : 'No se pudo guardar la película.';
    const alert = await alertController.create({
      header: 'No se guardó',
      message,
      buttons: ['Aceptar'],
    });
    await alert.present();
  }
}

async function confirmarEliminar(pelicula: Pelicula) {
  const alert = await alertController.create({
    header: 'Eliminar película',
    message: `¿Seguro que quieres eliminar "${pelicula.nombre}"?`,
    buttons: [
      { text: 'Cancelar', role: 'cancel' },
      {
        text: 'Eliminar',
        role: 'destructive',
        handler: async () => {
          await deletePelicula(pelicula.id);
          peliculas.value = peliculas.value.filter((p) => p.id !== pelicula.id);
        },
      },
    ],
  });
  await alert.present();
}

async function cerrarSesion() {
  const activeElement = document.activeElement as HTMLElement | null;
  if (activeElement && typeof activeElement.blur === 'function') {
    activeElement.blur();
  }

  await clearToken();
  await router.push('/auth');
}
</script>

<style scoped>
.catalog-title {
  text-align: start;
  padding-inline: 16px 8px;
}

.catalog-page {
  --background:
    radial-gradient(circle at 10% 0%, rgba(231, 177, 90, 0.18), transparent 32%),
    #0c1018;
}

.topbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin: 8px 16px 12px;
}

.topbar-actions {
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: 8px;
}

.logout-button {
  --color: #f4efe6;
  --border-color: rgba(244, 239, 230, 0.45);
  --border-radius: 999px;
  --background: transparent;
  font-weight: 700;
  text-transform: none;
}

.eyebrow {
  margin: 0;
  color: #e7b15a;
  font-size: 0.72rem;
  font-weight: 700;
  letter-spacing: 0.14em;
  text-transform: uppercase;
}

h2 {
  margin: 4px 0 0;
  font-family: Palatino, Georgia, serif;
  font-size: 2rem;
  font-weight: 600;
}

.searchbar-custom {
  --background: rgba(255, 255, 255, 0.05);
  --color: #f4efe6;
  --placeholder-color: #b7aea0;
  --icon-color: #e7b15a;
  --border-radius: 16px;
  --box-shadow: none;
  padding-inline: 8px;
}

.catalog {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(min(100%, 220px), 1fr));
  gap: 14px;
  padding: 8px 16px 24px;
}

.poster-card {
  overflow: hidden;
  background: #161d2b;
  border: 1px solid rgba(244, 239, 230, 0.08);
  border-radius: 18px;
}

.poster-card img,
.poster-fallback {
  width: 100%;
  height: 190px;
  object-fit: cover;
  display: block;
  background: #1c2433;
}

.poster-fallback {
  display: grid;
  place-items: center;
  color: #e7b15a;
  font-family: Palatino, Georgia, serif;
  font-size: 2.4rem;
}

.poster-copy {
  padding: 12px;
}

.poster-copy h3,
.poster-copy p {
  margin: 0;
}

.poster-copy p {
  margin-top: 6px;
  color: #cfc6b8;
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.empty-state {
  margin: 28px 20px;
  text-align: center;
  color: #b7aea0;
}

@media (max-width: 420px) {
  .topbar {
    align-items: flex-start;
    flex-wrap: wrap;
    margin-inline: 12px;
  }

  .topbar-actions {
    width: 100%;
  }

  h2 {
    font-size: 1.55rem;
  }

  .catalog {
    padding-inline: 12px;
  }

  .poster-card img,
  .poster-fallback {
    height: 210px;
  }
}
</style>