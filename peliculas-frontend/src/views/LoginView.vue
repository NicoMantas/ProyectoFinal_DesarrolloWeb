<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { API_BASE_URL } from '../services/api';

const router = useRouter();
const email = ref('');
const password = ref('');
const showPassword = ref(false);
const error = ref('');

async function login() {
  error.value = '';

  try {
    const response = await fetch(`${API_BASE_URL}/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        email: email.value,
        password: password.value,
      }),
    });

    const data = await response.json();

    if (!response.ok) {
      throw new Error(data.message || 'Credenciales inválidas');
    }

    localStorage.setItem('jwt_token', data.token);
    localStorage.setItem('usuario', JSON.stringify(data.usuario));
    router.push('/home');
  } catch (err) {
    error.value = err?.message || 'No se pudo iniciar sesión';
  }
}
</script>

<template>
  <main class="auth-screen">
    <section class="auth-hero">
      <p class="eyebrow">Colección personal</p>
      <h1>La sala está lista.</h1>
      <p>Entra para ver, guardar y ordenar las películas de tu colección.</p>
    </section>

    <section class="auth-panel">
      <p class="eyebrow">Bienvenido</p>
      <h2>Iniciar sesión</h2>

      <form @submit.prevent="login">
        <label>
          Email
          <input v-model="email" type="email" placeholder="correo@ejemplo.com" required />
        </label>

        <label>
          Contraseña
          <span class="password-field">
            <input
              v-model="password"
              :type="showPassword ? 'text' : 'password'"
              placeholder="Tu contraseña"
              required
            />
            <button
              type="button"
              class="password-toggle"
              :aria-label="showPassword ? 'Ocultar contraseña' : 'Mostrar contraseña'"
              @click="showPassword = !showPassword"
            >
              <svg v-if="showPassword" viewBox="0 0 24 24" aria-hidden="true">
                <path d="M3 3l18 18" />
                <path d="M10.6 10.6a3 3 0 0 0 4.2 4.2" />
                <path d="M9.9 5.1A10.8 10.8 0 0 1 12 5c6.5 0 10 7 10 7a18.5 18.5 0 0 1-3.2 4.4" />
                <path d="M6.1 6.1C3.8 7.8 2 12 2 12s3.5 7 10 7a10.8 10.8 0 0 0 4.1-.8" />
              </svg>
              <svg v-else viewBox="0 0 24 24" aria-hidden="true">
                <path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12z" />
                <circle cx="12" cy="12" r="3" />
              </svg>
            </button>
          </span>
        </label>

        <button type="submit">Entrar</button>
      </form>

      <p v-if="error" class="error">{{ error }}</p>

      <p class="switch-link">
        ¿No tienes cuenta?
        <router-link to="/register">Regístrate</router-link>
      </p>
    </section>
  </main>
</template>
