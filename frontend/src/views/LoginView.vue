<script setup>
import { ref } from 'vue'

import {
  RouterLink,
  useRouter
} from 'vue-router'

const router = useRouter()

/* ─────────────────────────────
   LOGIN
───────────────────────────── */

const correo = ref('')
const password = ref('')

const error = ref('')

const loading = ref(false)

/* ─────────────────────────────
   RECUPERAR
───────────────────────────── */

const showRecoverModal = ref(false)

const recoverEmail = ref('')

const recoverError = ref('')

const recoverSuccess = ref(false)

/* ─────────────────────────────
   ENTRAR COMO ADMIN
───────────────────────────── */

function entrarComoAdmin() {

  const adminDemo = {

    id: 'ADMIN-001',

    nombre:
      'Administrador',

    correo:
      'admin@anhelopets.cr',

    rol:
      'Admin',

    activo:
      true

  }

  localStorage.setItem(

    'anhelo_usuario_actual',

    JSON.stringify(
      adminDemo
    )

  )

  router.push('/admin')

}

/* ─────────────────────────────
   LOGIN
───────────────────────────── */

function iniciarSesion() {

  error.value = ''

  if (

    !correo.value ||
    !password.value

  ) {

    error.value =
      'Completa todos los campos'

    return
  }

  loading.value = true

  const usuarios = JSON.parse(

    localStorage.getItem(
      'anhelo_usuarios'
    )

  ) || []

  const usuario = usuarios.find(u =>

    u.correo.toLowerCase() ===
      correo.value.toLowerCase()

    &&

    u.password === password.value

  )

  if (!usuario) {

    loading.value = false

    error.value =
      'Correo o contraseña incorrectos'

    return
  }

  if (!usuario.activo) {

    loading.value = false

    error.value =
      'Tu cuenta está inactiva'

    return
  }

  localStorage.setItem(

    'anhelo_usuario_actual',

    JSON.stringify(
      usuario
    )

  )

  setTimeout(() => {

    if (

      usuario.rol === 'Admin'

    ) {

      router.push('/admin')

    } else {

      router.push('/')

    }

  }, 700)

}

/* ─────────────────────────────
   RECUPERAR
───────────────────────────── */

function recuperarPassword() {

  recoverError.value = ''

  recoverSuccess.value = false

  if (!recoverEmail.value) {

    recoverError.value =
      'Ingresa tu correo'

    return
  }

  const usuarios = JSON.parse(

    localStorage.getItem(
      'anhelo_usuarios'
    )

  ) || []

  const usuario = usuarios.find(u =>

    u.correo.toLowerCase() ===

    recoverEmail.value.toLowerCase()

  )

  if (!usuario) {

    recoverError.value =
      'No existe una cuenta con este correo'

    return
  }

  recoverSuccess.value = true

}
</script>

<template>

  <div class="login-container">

    <!-- VISUAL -->

    <div class="login-visual">

      <RouterLink
        to="/"
        class="logo-link"
      >

        Anhelo

        <span class="logo-green">
          Pets
        </span>

      </RouterLink>

      <div class="visual-content">

        <h1 class="visual-title">

          Bienvenido
          nuevamente

        </h1>

        <p class="visual-description">

          Inicia sesión para continuar
          con tu proceso de adopción
          y gestionar tus solicitudes.

        </p>

      </div>

    </div>

    <!-- FORM -->

    <div class="login-form-side">

      <div class="form-container">

        <div class="form-header">

          <h2>
            Iniciar sesión
          </h2>

          <p>
            Ingresa tus credenciales
          </p>

        </div>

        <!-- DEMO ADMIN -->

        <div class="demo-box">

          <strong>
            Acceso rápido administrador
          </strong>

          <p>
            Ingresa automáticamente al panel admin.
          </p>

          <button
            type="button"
            class="demo-admin-btn"
            @click="entrarComoAdmin"
          >

            Entrar como administrador

          </button>

        </div>

        <!-- ERROR -->

        <div
          v-if="error"
          class="error-box"
        >

          {{ error }}

        </div>

        <!-- FORM -->

        <form
          @submit.prevent="
            iniciarSesion
          "
        >

          <!-- CORREO -->

          <div class="input-group">

            <label>
              Correo electrónico
            </label>

            <input
              v-model="correo"
              type="email"
              class="custom-input"
              placeholder="correo@ejemplo.com"
            />

          </div>

          <!-- PASSWORD -->

          <div class="input-group">

            <label>
              Contraseña
            </label>

            <input
              v-model="password"
              type="password"
              class="custom-input"
              placeholder="••••••••"
            />

          </div>

          <!-- RECUPERAR -->

          <div class="forgot-password-wrap">

            <button
              type="button"
              class="forgot-password-btn"
              @click="
                showRecoverModal = true
              "
            >

              ¿Olvidaste tu contraseña?

            </button>

          </div>

          <!-- BOTON -->

          <button
            type="submit"
            class="btn-login"
            :disabled="loading"
          >

            <span v-if="!loading">

              Ingresar

            </span>

            <span v-else>

              Ingresando...

            </span>

          </button>

        </form>

        <!-- FOOTER -->

        <div class="form-footer">

          <p>

            ¿No tienes cuenta?

            <RouterLink
              to="/registro"
              class="register-link"
            >

              Registrarse

            </RouterLink>

          </p>

        </div>

      </div>

    </div>

    <!-- MODAL RECUPERAR -->

    <div
      v-if="showRecoverModal"
      class="modal-overlay"
      @click.self="
        showRecoverModal = false
      "
    >

      <div class="recover-modal">

        <div class="modal-header">

          <h3>
            Recuperar contraseña
          </h3>

          <button
            class="close-modal"
            @click="
              showRecoverModal = false
            "
          >
            ×
          </button>

        </div>

        <p class="recover-text">

          Ingresa tu correo electrónico
          para buscar tu cuenta.

        </p>

        <input
          v-model="recoverEmail"
          type="email"
          class="custom-input"
          placeholder="correo@ejemplo.com"
        />

        <!-- ERROR -->

        <div
          v-if="recoverError"
          class="recover-error"
        >

          {{ recoverError }}

        </div>

        <!-- SUCCESS -->

        <div
          v-if="recoverSuccess"
          class="recover-success"
        >

          Si existe una cuenta asociada,
          podrás restablecer tu contraseña
          próximamente.

        </div>

        <button
          class="recover-btn"
          @click="
            recuperarPassword
          "
        >

          Buscar cuenta

        </button>

      </div>

    </div>

  </div>

</template>

<style scoped>

.login-container {

  min-height: 100vh;

  display: flex;

  background: #FAFAFA;
}

/* VISUAL */

.login-visual {

  flex: 1;

  background:
    linear-gradient(
      135deg,
      #92A894,
      #7C927E
    );

  padding: 60px;

  color: white;
}

.logo-link {

  font-size: 30px;

  font-weight: 800;

  color: white;

  text-decoration: none;
}

.logo-green {

  color: #F9C17A;
}

.visual-content {

  margin-top: 140px;
}

.visual-title {

  font-size: 58px;

  font-weight: 800;

  line-height: 1.1;
}

.visual-description {

  margin-top: 22px;

  font-size: 18px;

  line-height: 1.7;

  max-width: 420px;
}

/* FORM */

.login-form-side {

  flex: 1;

  display: flex;

  align-items: center;

  justify-content: center;

  padding: 40px;
}

.form-container {

  width: 100%;

  max-width: 460px;
}

.form-header h2 {

  font-size: 38px;

  font-weight: 800;

  color: #2F3B31;

  margin-bottom: 6px;
}

.form-header p {

  color: #667085;

  margin-bottom: 26px;
}

/* DEMO */

.demo-box {

  background:
    rgba(146,168,148,0.12);

  border:
    1px solid rgba(146,168,148,0.18);

  padding: 18px;

  border-radius: 18px;

  margin-bottom: 24px;
}

.demo-box strong {

  display: block;

  margin-bottom: 10px;

  color: #2F3B31;
}

.demo-box p {

  margin: 0;

  color: #667085;

  font-size: 14px;

  line-height: 1.7;
}

.demo-admin-btn {

  width: 100%;

  height: 48px;

  border: none;

  border-radius: 14px;

  margin-top: 14px;

  background:
    linear-gradient(
      135deg,
      #3A473C,
      #556857
    );

  color: white;

  font-size: 14px;

  font-weight: 800;

  cursor: pointer;

  transition: 0.25s ease;
}

.demo-admin-btn:hover {

  transform: translateY(-2px);

  opacity: 0.95;
}

/* INPUTS */

.input-group {

  margin-bottom: 20px;
}

.input-group label {

  display: block;

  margin-bottom: 8px;

  font-size: 14px;

  font-weight: 700;

  color: #2F3B31;
}

.custom-input {

  width: 100%;

  height: 56px;

  border-radius: 16px;

  border: 2px solid #EEF2EE;

  background: #F8FAF8;

  padding: 0 18px;

  font-size: 14px;

  outline: none;

  transition: 0.25s ease;

  box-sizing: border-box;
}

.custom-input:focus {

  border-color: #92A894;

  background: white;
}

/* BOTON */

.btn-login {

  width: 100%;

  height: 58px;

  border: none;

  border-radius: 18px;

  background:
    linear-gradient(
      135deg,
      #92A894,
      #7C927E
    );

  color: white;

  font-size: 15px;

  font-weight: 800;

  cursor: pointer;

  transition: 0.25s ease;
}

.btn-login:hover {

  transform: translateY(-2px);
}

.btn-login:disabled {

  opacity: 0.7;

  cursor: not-allowed;
}

/* ERROR */

.error-box {

  background:
    rgba(235,119,119,0.12);

  color: #C45252;

  padding: 16px;

  border-radius: 16px;

  margin-bottom: 20px;

  font-weight: 700;
}

/* FOOTER */

.form-footer {

  margin-top: 28px;

  text-align: center;

  color: #667085;
}

.register-link {

  color: #6F8572;

  font-weight: 800;

  text-decoration: none;
}

/* RECOVER */

.forgot-password-wrap {

  display: flex;

  justify-content: flex-end;

  margin-top: -4px;

  margin-bottom: 22px;
}

.forgot-password-btn {

  border: none;

  background: none;

  color: #6F8572;

  font-size: 13px;

  font-weight: 700;

  cursor: pointer;
}

.forgot-password-btn:hover {

  text-decoration: underline;
}

/* MODAL */

.modal-overlay {

  position: fixed;

  inset: 0;

  background:
    rgba(0,0,0,0.45);

  display: flex;

  align-items: center;

  justify-content: center;

  z-index: 9999;

  padding: 20px;
}

.recover-modal {

  width: 100%;

  max-width: 460px;

  background: white;

  border-radius: 28px;

  padding: 30px;
}

.modal-header {

  display: flex;

  justify-content: space-between;

  align-items: center;

  margin-bottom: 14px;
}

.modal-header h3 {

  font-size: 24px;

  color: #2F3B31;
}

.close-modal {

  width: 38px;

  height: 38px;

  border-radius: 12px;

  border: none;

  background: #F4F6F4;

  font-size: 24px;

  cursor: pointer;
}

.recover-text {

  color: #667085;

  margin-bottom: 18px;

  line-height: 1.6;
}

.recover-btn {

  width: 100%;

  height: 56px;

  border: none;

  border-radius: 16px;

  margin-top: 18px;

  background:
    linear-gradient(
      135deg,
      #92A894,
      #7C927E
    );

  color: white;

  font-size: 14px;

  font-weight: 800;

  cursor: pointer;
}

.recover-error {

  background:
    rgba(235,119,119,0.12);

  color: #C45252;

  padding: 14px;

  border-radius: 14px;

  margin-top: 16px;

  font-weight: 700;
}

.recover-success {

  background:
    rgba(146,168,148,0.14);

  color: #5C715E;

  padding: 14px;

  border-radius: 14px;

  margin-top: 16px;

  line-height: 1.6;
}

/* RESPONSIVE */

@media (max-width: 900px) {

  .login-visual {

    display: none;
  }

  .login-form-side {

    flex: 1;
  }

}
</style>