<template>
  <ion-page>
    <ion-content class="auth-page" :fullscreen="true">
      <div class="auth-screen">
        <section class="auth-hero">
          <p class="eyebrow">{{ isRegister ? 'Nueva butaca' : 'Mi colección' }}</p>
          <h1>{{ isRegister ? 'Guarda lo que quieres volver a ver.' : 'La sala está lista.' }}</h1>
          <p>
            {{ isRegister ? 'Crea tu cuenta y arma una colección con nombre, sinopsis e imagen.' : 'Entra para ver, guardar y ordenar las películas de tu colección.' }}
          </p>
        </section>

        <section class="auth-panel">
        <div class="auth-card">
          <p class="eyebrow">{{ isRegister ? 'Registro' : 'Bienvenido' }}</p>
          <h2>{{ isRegister ? 'Crear cuenta' : 'Iniciar sesión' }}</h2>
          <ion-segment :value="mode" @ionChange="handleModeChange" class="auth-segment">
            <ion-segment-button value="login">
              <ion-label>Login</ion-label>
            </ion-segment-button>
            <ion-segment-button value="register">
              <ion-label>Registro</ion-label>
            </ion-segment-button>
          </ion-segment>

          <form v-if="isRegister" @submit.prevent="submitRegister">
            <ion-item>
              <ion-input v-model="registerForm.nombre" label="Nombre" label-placement="stacked" required />
            </ion-item>
            <ion-item>
              <ion-input v-model="registerForm.email" type="email" label="Correo" label-placement="stacked" required />
            </ion-item>
            <ion-item class="password-item">
              <ion-input
                v-model="registerForm.password"
                :type="showRegisterPassword ? 'text' : 'password'"
                label="Contraseña"
                label-placement="stacked"
                required
              />
              <ion-button type="button" fill="clear" slot="end" @click="showRegisterPassword = !showRegisterPassword" aria-label="Mostrar u ocultar contraseña">
                <ion-icon :icon="showRegisterPassword ? eyeOffOutline : eyeOutline" />
              </ion-button>
            </ion-item>

            <ion-button expand="block" type="submit" class="submit-button" :disabled="loading">
              {{ loading ? 'Registrando...' : 'Registrarme' }}
            </ion-button>
          </form>

          <form v-else @submit.prevent="submitLogin">
            <ion-item>
              <ion-input v-model="loginForm.email" type="email" label="Correo" label-placement="stacked" required />
            </ion-item>
            <ion-item class="password-item">
              <ion-input
                v-model="loginForm.password"
                :type="showLoginPassword ? 'text' : 'password'"
                label="Contraseña"
                label-placement="stacked"
                required
              />
              <ion-button type="button" fill="clear" slot="end" @click="showLoginPassword = !showLoginPassword" aria-label="Mostrar u ocultar contraseña">
                <ion-icon :icon="showLoginPassword ? eyeOffOutline : eyeOutline" />
              </ion-button>
            </ion-item>

            <ion-button expand="block" type="submit" class="submit-button" :disabled="loading">
              {{ loading ? 'Ingresando...' : 'Ingresar' }}
            </ion-button>
          </form>
        </div>
        </section>
      </div>
    </ion-content>
  </ion-page>
</template>

<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import {
  IonPage,
  IonContent,
  IonSegment,
  IonSegmentButton,
  IonLabel,
  IonItem,
  IonInput,
  IonButton,
  IonIcon,
  alertController,
} from '@ionic/vue';
import { eyeOutline, eyeOffOutline } from 'ionicons/icons';
import { loginUser, registerUser } from '@/services/auth.service';

const router = useRouter();
const mode = ref<'login' | 'register'>('login');
const loading = ref(false);
const showLoginPassword = ref(false);
const showRegisterPassword = ref(false);

const loginForm = reactive({ email: '', password: '' });
const registerForm = reactive({ nombre: '', email: '', password: '' });

const isRegister = computed(() => mode.value === 'register');

function releaseFocus() {
  const activeElement = document.activeElement as HTMLElement | null;
  if (activeElement && typeof activeElement.blur === 'function') {
    activeElement.blur();
  }
}

function handleModeChange(event: CustomEvent) {
  const value = event.detail.value as 'login' | 'register';
  mode.value = value;
}

async function submitLogin() {
  loading.value = true;

  try {
    await loginUser({
      email: loginForm.email,
      password: loginForm.password,
    });

    releaseFocus();
    await router.push('/peliculas');
  } catch (error) {
    const message = error instanceof Error ? error.message : 'No se pudo iniciar sesión.';
    const alert = await alertController.create({
      header: 'Error de sesión',
      message,
      buttons: ['Aceptar'],
    });
    await alert.present();
  } finally {
    loading.value = false;
  }
}

async function submitRegister() {
  loading.value = true;

  try {
    await registerUser({
      nombre: registerForm.nombre,
      email: registerForm.email,
      password: registerForm.password,
    });

    releaseFocus();
    await router.push('/peliculas');
  } catch (error) {
    const message = error instanceof Error ? error.message : 'No se pudo registrar el usuario.';
    const alert = await alertController.create({
      header: 'Error de registro',
      message,
      buttons: ['Aceptar'],
    });
    await alert.present();
  } finally {
    loading.value = false;
  }
}
</script>

<style scoped>
.auth-page {
  --background:
    radial-gradient(circle at 20% 0%, rgba(231, 177, 90, 0.22), transparent 34%),
    #0c1018;
}

.auth-page::part(scroll) {
  display: flex;
  flex-direction: column;
}

.auth-screen {
  flex: 1;
  min-height: 100%;
  display: grid;
  grid-template-columns: minmax(280px, 1.05fr) minmax(420px, 640px);
}

.auth-hero,
.auth-panel {
  display: flex;
  flex-direction: column;
  justify-content: center;
  padding: 48px clamp(28px, 4vw, 64px);
  box-sizing: border-box;
}

.auth-hero {
  align-items: center;
  text-align: center;
  background:
    linear-gradient(180deg, rgba(12, 16, 24, 0.15), rgba(12, 16, 24, 0.72)),
    radial-gradient(circle at 30% 30%, rgba(231, 177, 90, 0.35), transparent 42%);
}

.auth-panel {
  align-items: stretch;
}

.eyebrow {
  margin: 0;
  color: #e7b15a;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  font-size: 0.72rem;
}

h1,
h2 {
  margin: 8px 0 0;
  font-family: Palatino, Georgia, serif;
  font-weight: 600;
  color: #f8fafc;
  line-height: 1.05;
}

h1 {
  max-width: 12ch;
  font-size: clamp(2.4rem, 4.4vw, 4.2rem);
}

h2 {
  margin-bottom: 18px;
  font-size: clamp(1.8rem, 3vw, 2.4rem);
}

.auth-hero p,
.auth-panel p {
  margin: 14px 0 0;
  max-width: 36ch;
  color: #d9d0c3;
  line-height: 1.5;
  font-size: 1rem;
}

.auth-card {
  width: 100%;
  background: transparent;
  border: 0;
  border-radius: 0;
  padding: 0;
  box-shadow: none;
}

.auth-segment {
  margin-bottom: 16px;
  --background: rgba(255, 255, 255, 0.05);
  border-radius: 16px;
}

form {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

ion-item {
  --background: rgba(255, 255, 255, 0.04);
  --border-radius: 16px;
  --inner-border-width: 0;
  --highlight-color-focused: #e7b15a;
  border-radius: 16px;
  margin-bottom: 4px;
  box-shadow: inset 0 0 0 1px rgba(255, 255, 255, 0.02);
}

.password-item {
  --padding-end: 0;
}

.password-item ion-button {
  --padding-start: 8px;
  --padding-end: 8px;
  color: #e7b15a;
}

.submit-button {
  margin-top: 12px;
  --background: #e7b15a;
  --background-hover: #cb9c4f;
  --color: #1c1408;
  --box-shadow: none;
  border-radius: 14px;
  font-weight: 700;
  letter-spacing: 0.02em;
}

ion-segment-button {
  --indicator-color: rgba(231, 177, 90, 0.2);
  --color: #b7aea0;
  --color-checked: #f4efe6;
  --background-checked: rgba(231, 177, 90, 0.14);
  border-radius: 12px;
  min-width: 0;
}

@media (max-width: 860px) {
  .auth-screen {
    grid-template-columns: 1fr;
  }

  .auth-hero,
  .auth-panel {
    padding: 28px 20px;
  }

  .auth-hero {
    min-height: 220px;
  }

  h1 {
    max-width: 14ch;
    font-size: clamp(2rem, 9vw, 3rem);
  }
}

@media (max-width: 380px) {
  h1 {
    font-size: 1.7rem;
  }

  .auth-hero p,
  .auth-panel p {
    font-size: 0.92rem;
  }
}
</style>
