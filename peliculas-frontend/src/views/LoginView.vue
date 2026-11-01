<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter();
const email = ref('');
const password = ref('');
const error = ref('');

async function login() {
  error.value = '';

  try {
    const response = await fetch('http://localhost:3000/auth/login', {
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
  <main class="auth-container">
    <div class="auth-card">
      <h2>Iniciar sesión</h2>

      <form @submit.prevent="login">
        <label>
          Email
          <input v-model="email" type="email" placeholder="correo@ejemplo.com" required />
        </label>

        <label>
          Contraseña
          <input v-model="password" type="password" placeholder="********" required />
        </label>

        <button type="submit">Entrar</button>
      </form>

      <p v-if="error" class="error">{{ error }}</p>

      <p class="switch-link">
        ¿No tienes cuenta?
        <router-link to="/register">Regístrate</router-link>
      </p>
    </div>
  </main>
</template>

<style scoped>
.auth-container {
  min-height: 100vh;
  display: grid;
  place-items: center;
  background: #f4f6fb;
}

.auth-card {
  width: min(420px, 90vw);
  background: white;
  border-radius: 16px;
  padding: 24px;
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.08);
}

form {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

label {
  display: flex;
  flex-direction: column;
  gap: 6px;
  font-weight: 600;
}

input,
button {
  padding: 10px 12px;
  border-radius: 10px;
  border: 1px solid #d0d7e6;
}

button {
  background: #2563eb;
  color: white;
  border: none;
  cursor: pointer;
  font-weight: 600;
}

.error {
  color: #b91c1c;
  margin-top: 12px;
}

.switch-link {
  text-align: center;
  margin-top: 14px;
}
</style>
