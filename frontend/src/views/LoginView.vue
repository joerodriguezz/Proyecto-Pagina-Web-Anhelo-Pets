<script setup>
import { ref } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import { authApi } from '../services/api'

const router = useRouter()
const usernameOrEmail = ref('')
const password = ref('')
const loading = ref(false)
const errorMessage = ref('')

async function login() {
  loading.value = true
  errorMessage.value = ''

  try {
    const user = await authApi.login({
      usernameOrEmail: usernameOrEmail.value,
      password: password.value,
    })

    localStorage.setItem('authUser', JSON.stringify(user))
    router.push('/admin')
  } catch (error) {
    errorMessage.value = 'Credenciales invalidas o servicio no disponible.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="auth-container">
    <div class="auth-visual">
      <div class="brand-wrapper">
        <RouterLink to="/" class="logo-link">
          Anhelo<span class="peach">Pets</span>
        </RouterLink>
      </div>
      
      <div class="visual-content">
        <h1 class="visual-title">Tu próxima <br>historia de <span class="accent-text">amor</span> empieza aquí.</h1>
        <p class="visual-description">
          Ingresa para gestionar tus solicitudes de adopción, revisar el estado de tus procesos y continuar cambiando vidas.
        </p>
      </div>

      <div class="visual-footer">
        <p>© Anhelo Pets. Dedicados al bienestar animal.</p>
      </div>
    </div>

    <div class="auth-form-side">
      <div class="form-container">
        <header class="form-header">
          <h2>Bienvenido</h2>
          <p>Ingresa tus credenciales para continuar</p>
        </header>

        <form class="main-form" @submit.prevent="login">
          <div class="input-group">
            <label>Correo electrónico</label>
            <input v-model="usernameOrEmail" type="email" placeholder="ejemplo@correo.com" class="custom-input" required />
          </div>

          <div class="input-group">
            <label>Contraseña</label>
            <input v-model="password" type="password" placeholder="************" class="custom-input" required />
          </div>

          <div class="form-utils">
            <label class="custom-checkbox">
              <input type="checkbox" />
              <span class="label-text">Recordarme</span>
            </label>
            <a href="#" class="forgot-password">¿Olvidaste tu contraseña?</a>
          </div>

          <p v-if="errorMessage" class="form-error">{{ errorMessage }}</p>

          <button type="submit" class="btn-login" :disabled="loading">
            {{ loading ? 'Ingresando...' : 'Iniciar sesion' }}
          </button>
        </form>

        <div class="divider">
          <span>o accede como</span>
        </div>

        <RouterLink to="/admin" class="btn-admin-demo">
          🔧 Administrador (Demo)
        </RouterLink>

        <footer class="form-footer">
          <p>¿Aún no eres parte de la familia? 
            <RouterLink to="/registro" class="register-link">Regístrate</RouterLink>
          </p>
        </footer>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* ── Estructura Principal ── */
.auth-container {
  min-height: 100vh;
  display: flex;
  background-color: #FAFAFA;
}

/* ── Lado Visual (Izquierdo) ── */
.auth-visual {
  flex: 1.2;
  position: relative;
  background: linear-gradient(135deg, #92A894 0%, #7C927E 100%);
  padding: 60px;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  overflow: hidden;
  color: white;
}

.logo-link {
  font-size: 28px;
  font-weight: 800;
  color: white;
  text-decoration: none;
  letter-spacing: -1px;
}

.peach { color: #F9C17A; }

.visual-content {
  max-width: 480px;
  margin-auto: 0; /* Centra verticalmente el contenido principal */
  z-index: 2;
}

.visual-title {
  font-size: 52px;
  line-height: 1.1;
  font-weight: 800;
  margin-bottom: 20px;
  letter-spacing: -2px;
}

.accent-text { color: #F9C17A; }

.visual-description {
  font-size: 18px;
  line-height: 1.6;
  opacity: 0.9;
}

.visual-footer {
  z-index: 2;
  font-size: 13px;
  opacity: 0.6;
}

/* ── Lado Formulario (Derecho) ── */
.auth-form-side {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 40px;
}

.form-container {
  width: 100%;
  max-width: 400px;
}

.form-header h2 {
  font-size: 32px;
  font-weight: 800;
  color: #3A473C;
  margin-bottom: 8px;
}

.form-header p {
  color: #6C756D;
  font-size: 15px;
  margin-bottom: 40px;
}

/* ── Inputs y Campos ── */
.input-group {
  margin-bottom: 20px;
}

.input-group label {
  display: block;
  font-size: 14px;
  font-weight: 700;
  color: #3A473C;
  margin-bottom: 8px;
}

.custom-input {
  width: 100%;
  padding: 14px 18px;
  border-radius: 15px;
  border: 2px solid #F4F6F4;
  background-color: #F4F6F4;
  font-size: 15px;
  color: #3A473C;
  transition: all 0.3s ease;
  outline: none;
}

.custom-input:focus {
  background-color: white;
  border-color: #92A894;
  box-shadow: 0 8px 20px rgba(146, 168, 148, 0.1);
}

/* ── Checkbox y Links ── */
.form-utils {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 30px;
}

.custom-checkbox {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  font-size: 14px;
  color: #6C756D;
}

.custom-checkbox input {
  accent-color: #92A894;
}

.forgot-password {
  color: #92A894;
  text-decoration: none;
  font-size: 14px;
  font-weight: 700;
}

/* ── Botones ── */
.btn-login {
  width: 100%;
  padding: 16px;
  border-radius: 18px;
  border: none;
  background-color: #92A894;
  color: white;
  font-size: 16px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.3s ease;
  margin-bottom: 20px;
}

.btn-login:hover {
  background-color: #7C927E;
  transform: translateY(-2px);
  box-shadow: 0 10px 25px rgba(146, 168, 148, 0.2);
}

.form-error {
  color: #B42318;
  font-size: 14px;
  font-weight: 700;
  margin: -10px 0 16px;
}

.btn-admin-demo {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100%;
  padding: 14px;
  border-radius: 18px;
  border: 2px solid #92A894;
  color: #92A894;
  text-decoration: none;
  font-weight: 700;
  font-size: 14px;
  transition: all 0.3s ease;
}

.btn-admin-demo:hover {
  background-color: #92A894;
  color: white;
}

/* ── Separador ── */
.divider {
  text-align: center;
  position: relative;
  margin: 25px 0;
}

.divider::before {
  content: "";
  position: absolute;
  top: 50%;
  left: 0;
  right: 0;
  height: 1px;
  background-color: #E7ECE7;
}

.divider span {
  position: relative;
  background-color: white;
  padding: 0 15px;
  color: #6C756D;
  font-size: 13px;
}

/* ── Footer ── */
.form-footer {
  text-align: center;
  margin-top: 40px;
  font-size: 15px;
  color: #6C756D;
}

.register-link {
  color: #92A894;
  font-weight: 800;
  text-decoration: none;
}

/* ── Responsivo ── */
@media (max-width: 1100px) {
  .visual-title { font-size: 40px; }
  .auth-visual { padding: 40px; }
}

@media (max-width: 900px) {
  .auth-visual { display: none; }
  .auth-form-side { flex: 1; background-color: white; }
}
</style>
