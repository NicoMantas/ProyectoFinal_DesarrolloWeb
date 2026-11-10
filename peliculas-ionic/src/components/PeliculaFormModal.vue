<template>
  <ion-modal :is-open="isOpen" @did-dismiss="cerrarModal">
    <ion-header>
      <ion-toolbar>
        <ion-title>{{ titulo }}</ion-title>
        <ion-buttons slot="end">
          <ion-button @click="cerrarModal">Cerrar</ion-button>
        </ion-buttons>
      </ion-toolbar>
    </ion-header>

    <ion-content class="ion-padding">
      <div v-if="!modoEdicion && pelicula" class="movie-detail">
        <img :src="pelicula.imagen || 'https://placehold.co/600x400/1f1f2d/ffffff?text=Sin+imagen'" :alt="pelicula.nombre" />
        <h2>{{ pelicula.nombre }}</h2>
        <p>{{ pelicula.sinopsis }}</p>

        <ion-button expand="block" class="ion-margin-top" @click="modoEdicion = true">
          Editar película
        </ion-button>
      </div>

      <template v-else>
        <img v-if="form.imagen" class="form-preview" :src="form.imagen" alt="Vista previa" />
        <ion-item>
          <ion-input label="Nombre" label-placement="stacked" v-model="form.nombre" />
        </ion-item>
        <ion-item>
        <ion-textarea label="Sinopsis" label-placement="stacked" v-model="form.sinopsis" :auto-grow="true" :rows="1" />      
        </ion-item>
        <ion-item>
          <ion-input label="URL de la imagen" label-placement="stacked" v-model="form.imagen" />
        </ion-item>

        <ion-button expand="block" class="ion-margin-top save-button" :disabled="!esValido" @click="guardar">
          Guardar
        </ion-button>
      </template>
    </ion-content>
  </ion-modal>
</template>

<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue';
import {
  IonModal, IonHeader, IonToolbar, IonTitle, IonButtons, IonButton,
  IonContent, IonItem, IonInput, IonTextarea,
} from '@ionic/vue';
import type { CreatePeliculaInput, Pelicula } from '@/types/pelicula';

const props = defineProps<{
  isOpen: boolean;
  pelicula: Pelicula | null;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'save', data: CreatePeliculaInput): void;
}>();

const form = reactive<CreatePeliculaInput>({ nombre: '', sinopsis: '', imagen: '' });
const modoEdicion = ref(false);

const titulo = computed(() => {
  if (!props.pelicula) return 'Nueva película';
  return modoEdicion.value ? 'Editar película' : 'Detalle de la película';
});

watch(
  () => [props.isOpen, props.pelicula],
  () => {
    if (props.isOpen) {
      form.nombre = props.pelicula?.nombre ?? '';
      form.sinopsis = props.pelicula?.sinopsis ?? '';
      form.imagen = props.pelicula?.imagen ?? '';
      modoEdicion.value = !props.pelicula;
    }
  },
);

const esValido = computed(() =>
  form.nombre.trim() !== '' && form.sinopsis.trim() !== '' && form.imagen.trim() !== '',
);

function cerrarModal() {
  emit('close');
  modoEdicion.value = false;
}

function guardar() {
  emit('save', { ...form });
}
</script>

<style scoped>
/* Recuadro grande (modal): esquinas más redondeadas y más largo hacia abajo */
ion-modal {
  --width: min(560px, 100%);
  --height: min(840px, 100%);   /* antes 760px */
  --border-radius: 30px;        /* antes 18px */
}

@media (max-width: 576px) {
  ion-modal {
    --width: 100%;
    --height: 100%;
    --border-radius: 0;
  }
}

/* Recuadros de cada campo (Nombre, Sinopsis, URL) */
ion-item {
  --border-radius: 16px;
  --min-height: 68px;
  --padding-bottom: 20px;
  --border-width: 0;
  --inner-border-width: 0;
  position: relative;
  margin-bottom: 14px;
}

ion-input,
ion-textarea {
  --padding-top: 0;
  --padding-bottom: 0;
  min-height: 44px;
}

ion-item::part(native) {
  background-image: linear-gradient(#fcfcfc, #fcfcfc);  /* color de la línea */
  background-repeat: no-repeat;
  background-size: calc(100% - 32px) 1px;               /* 32px = 16px de margen a cada lado; 1px = grosor */
  background-position: 16px calc(100% - 10px);          /* 16px = desde la izquierda; 12px = altura desde abajo */
}

/* Etiquetas con la misma tipografía de "Mi colección" (.eyebrow del HomePage) */
:deep(.label-text-wrapper) {
  transform: none !important;   /* evita que Ionic reduzca el tamaño de la etiqueta */
  max-width: 100%;
}

:deep(.label-text) {
  color: #e7b15a;
  font-size: 0.72rem;
  font-weight: 700;
  letter-spacing: 0.14em;
  text-transform: uppercase;
}

.movie-detail {
  display: flex;
  flex-direction: column;
  gap: 16px;
  padding-bottom: 12px;
}

.movie-detail img {
  width: 100%;
  height: min(42vh, 320px);
  object-fit: cover;
  border-radius: 22px;
  box-shadow: 0 16px 32px rgba(0, 0, 0, 0.22);
  background: rgba(255, 255, 255, 0.06);
}

.movie-detail h2 {
  margin: 0;
  font-size: clamp(1.4rem, 5vw, 1.9rem);
  font-weight: 800;
  line-height: 1.2;
  overflow-wrap: anywhere;
}

.movie-detail p {
  margin: 0;
  color: #cfc6b8;
  line-height: 1.7;
  font-size: 0.98rem;
}

.form-preview {
  width: 100%;
  height: 180px;
  object-fit: cover;
  border-radius: 18px;
  margin-bottom: 14px;
  background: #1c2433;
}

.save-button {
  --background: #e7b15a;
  --background-hover: #cb9c4f;
  --color: #1c1408;
  font-weight: 700;
}
</style>