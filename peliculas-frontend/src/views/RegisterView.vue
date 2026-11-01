<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter();
const nombre = ref('');
const email = ref('');
const password = ref('');
const error = ref('');

async function register() {
  error.value = '';

  try {
    const response = await fetch('http://localhost:3000/auth/register', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        nombre: nombre.value,
        email: email.value,
        password: password.value,
      }),
    });

    const data = await response.json();

    if (!response.ok) {
      throw new Error(data.message || 'No se pudo registrar');
    }

    localStorage.setItem('jwt_token', data.token);
    localStorage.setItem('usuario', JSON.stringify(data.usuario));
    router.push('/home');
  } catch (err) {
    error.value = err?.message || 'No se pudo registrar';
  }
}
</script>

<template>
  <main class="auth-container">
    <div class="auth-card">
      <h2>Registro</h2>

      <form @submit.prevent="register">
        <label>
          Nombre
          <input v-model="nombre" type="text" placeholder="Tu nombre" required />
        </label>

        <label>
          Email
          <input v-model="email" type="email" placeholder="correo@ejemplo.com" required />
        </label>

        <label>
          Contraseña
          <input v-model="password" type="password" placeholder="********" required />
        </label>

        <button type="submit">Registrarme</button>
      </form>

      <p v-if="error" class="error">{{ error }}</p>

      <p class="switch-link">
        ¿Ya tienes cuenta?
        <router-link to="/login">Inicia sesión</router-link>
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
  background: #10b981;
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
