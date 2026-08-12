<script setup>
import { ref } from 'vue'

import {
  RouterLink,
  useRouter
} from 'vue-router'

import { resetPasswordByEmail } from '../services/authServices'
import { useAuthStore } from '../stores/useAuthStore'
const router = useRouter()
const authStore = useAuthStore()




/* ─────────────────────────────
   LOGIN
───────────────────────────── */

const correo   = ref('')
const password = ref('')
const error    = ref('')
const loading  = ref(false)

const showPassword = ref(false)

function toggleShowPassword() {
  showPassword.value = !showPassword.value
}

/* ─────────────────────────────
   RECUPERAR — ESTADO GENERAL
───────────────────────────── */

const showRecoverModal = ref(false)

// etapas: 'buscar' | 'verificar' | 'nuevaPassword' | 'exito'
const etapaRecuperacion   = ref('buscar')

const recoverEmail        = ref('')
const recoverError        = ref('')
const recoverSuccess      = ref(false)

const codigoRecuperacion  = ref('')
const codigoIngresado     = ref('')

const enviandoCorreo      = ref(false)
const actualizandoPassword = ref(false)
const verificandoCodigo   = ref(false)
const reenvioDeshabilitado = ref(false)

const nuevaPassword        = ref('')
const confirmarNuevaPassword = ref('')

/* ─────────────────────────────
   EMAILJS CONFIG
───────────────────────────── */

const EMAILJS_SERVICE_ID = 'service_okmmsx7'

const EMAILJS_REGISTER_TEMPLATE_ID =
  'template_aztswto'

const EMAILJS_RESET_TEMPLATE_ID =
  'template_ynpp8ld'

const EMAILJS_PUBLIC_KEY =
  'fwH1f3N8oVs98GPAx'

/* ─────────────────────────────
   EMAILJS — CARGA DINÁMICA
───────────────────────────── */

async function cargarEmailJS() {
  if (window.emailjs) return
  await new Promise((resolve, reject) => {
    const script    = document.createElement('script')
    script.src      = 'https://cdn.jsdelivr.net/npm/@emailjs/browser@4/dist/email.min.js'
    script.onload   = resolve
    script.onerror  = reject
    document.head.appendChild(script)
  })
  window.emailjs.init(EMAILJS_PUBLIC_KEY)
}

/* ─────────────────────────────
   GENERAR CÓDIGO
───────────────────────────── */

function generarCodigoRecuperacion() {
  return String(Math.floor(100000 + Math.random() * 900000))
}

/* ─────────────────────────────
   ENVIAR CORREO
───────────────────────────── */

async function enviarCorreoRecuperacion(codigo) {
  await cargarEmailJS()

const templateParams = {
  user_name: recoverEmail.value,
  reset_code: codigo,
  to_email: recoverEmail.value
}

const response = await window.emailjs.send(
  EMAILJS_SERVICE_ID,
  EMAILJS_RESET_TEMPLATE_ID,
  templateParams
)

  console.log('Correo de recuperación enviado correctamente:', response)
  return response
}

/* ─────────────────────────────
   ETAPA 1 — BUSCAR CUENTA
───────────────────────────── */

async function recuperarPassword() {
  recoverError.value   = ''
  recoverSuccess.value = false

  if (!recoverEmail.value) {
    recoverError.value = 'Ingresa tu correo electrónico'
    return
  }

  enviandoCorreo.value = true

  try {
    const codigo             = generarCodigoRecuperacion()
    codigoRecuperacion.value = codigo
    codigoIngresado.value    = ''

    await enviarCorreoRecuperacion(codigo)

    etapaRecuperacion.value = 'verificar'
  } catch (err) {
    console.error('Error al enviar el correo de recuperación:', err)
    recoverError.value = 'No se pudo enviar el correo. Intenta de nuevo.'
  } finally {
    enviandoCorreo.value = false
  }
}

/* ─────────────────────────────
   ETAPA 2 — VERIFICAR CÓDIGO
───────────────────────────── */

function verificarCodigoRecuperacion() {
  recoverError.value = ''

  if (!codigoIngresado.value.trim()) {
    recoverError.value = 'Por favor ingresa el código de verificación'
    return
  }

  if (codigoIngresado.value.trim() !== codigoRecuperacion.value) {
    recoverError.value = 'El código es incorrecto. Intenta nuevamente'
    return
  }

  etapaRecuperacion.value = 'nuevaPassword'
  recoverError.value      = ''
}

/* ─────────────────────────────
   REENVIAR CÓDIGO
───────────────────────────── */

async function reenviarCodigo() {
  reenvioDeshabilitado.value = true
  recoverError.value         = ''

  try {
    const nuevoCodigo        = generarCodigoRecuperacion()
    codigoRecuperacion.value = nuevoCodigo
    codigoIngresado.value    = ''

    await enviarCorreoRecuperacion(nuevoCodigo)
    console.log('Código de recuperación reenviado correctamente')
  } catch (err) {
    console.error('Error al reenviar el código:', err)
    recoverError.value = 'No se pudo reenviar el correo. Intenta de nuevo.'
  } finally {
    setTimeout(() => {
      reenvioDeshabilitado.value = false
    }, 30000)
  }
}

/* ─────────────────────────────
   ETAPA 3 — NUEVA CONTRASEÑA
───────────────────────────── */

async function actualizarPassword() {
  recoverError.value = ''

  if (!nuevaPassword.value || !confirmarNuevaPassword.value) {
    recoverError.value = 'Completa todos los campos'
    return
  }

  if (nuevaPassword.value !== confirmarNuevaPassword.value) {
    recoverError.value = 'Las contraseñas no coinciden'
    return
  }

  if (nuevaPassword.value.length < 8) {
    recoverError.value = 'La contraseña debe tener mínimo 8 caracteres'
    return
  }

  actualizandoPassword.value = true
  try {
    await resetPasswordByEmail(recoverEmail.value, nuevaPassword.value)
  } catch (err) {
    console.error(err)
    recoverError.value = err.response?.status === 404
      ? 'No se encontró la cuenta. Intenta de nuevo.'
      : 'No se pudo actualizar la contraseña. Intenta de nuevo.'
    actualizandoPassword.value = false
    return
  }
  actualizandoPassword.value = false

  etapaRecuperacion.value = 'exito'

  // Cerrar el modal automáticamente después de 3 segundos
  setTimeout(() => {
    cerrarModal()
  }, 3000)
}

/* ─────────────────────────────
   CERRAR MODAL — RESET TOTAL
───────────────────────────── */

function cerrarModal() {
  showRecoverModal.value       = false
  etapaRecuperacion.value      = 'buscar'
  recoverEmail.value           = ''
  recoverError.value           = ''
  recoverSuccess.value         = false
  codigoRecuperacion.value     = ''
  codigoIngresado.value        = ''
  enviandoCorreo.value         = false
  verificandoCodigo.value      = false
  reenvioDeshabilitado.value   = false
  nuevaPassword.value          = ''
  confirmarNuevaPassword.value = ''
}



/* ─────────────────────────────
   LOGIN
───────────────────────────── */

async function iniciarSesion() {
  error.value = ''

  if (!correo.value || !password.value) {
    error.value = 'Completa todos los campos'
    return
  }

  loading.value = true

  try {
    await authStore.login({ email: correo.value, password: password.value })
  } catch (err) {
    loading.value = false
    error.value = err.response?.data?.message || 'Correo o contraseña incorrectos'
    return
  }

  loading.value = false

  if (authStore.isAdmin) {
    router.push('/admin')
  } else {
    router.push('/')
  }
}
</script>

<template>

  <div class="login-container">

    <!-- VISUAL -->

    <div class="login-visual">

      <div class="visual-inner">

        <RouterLink to="/" class="logo-link">
          Anhelo
          <span class="logo-green">Pets</span>
        </RouterLink>

        <div class="visual-content">

          <h1 class="visual-title">
            Bienvenido<br />
            nuevamente
          </h1>

          <div class="visual-divider"></div>

          <p class="visual-description">
            Inicia sesión para continuar
            con tu proceso de adopción
            y gestionar tus solicitudes.
          </p>

        </div>

        <div class="visual-stats">
          <div class="visual-stat">
            <span class="visual-stat-value">+120</span>
            <span class="visual-stat-label">adopciones</span>
          </div>
          <div class="visual-stat">
            <span class="visual-stat-value">98%</span>
            <span class="visual-stat-label">satisfacción</span>
          </div>
        </div>

      </div>

    </div>

    <!-- FORM -->

    <div class="login-form-side">

      <div class="form-container">

        <div class="form-header">
          <h2>Iniciar sesión</h2>
          <p>Ingresa tus credenciales para continuar</p>
      
        </div>

        <!-- ERROR -->

        <div v-if="error" class="error-box">
          {{ error }}
        </div>

        <!-- FORM -->

        <form @submit.prevent="iniciarSesion">

          <!-- CORREO -->
          <div class="input-group">
            <label>Correo electrónico <span class="req-mark">*</span></label>
            <input
              v-model="correo"
              type="email"
              class="custom-input"
              placeholder="correo@ejemplo.com"
            />
          </div>

          <!-- PASSWORD -->
          <div class="input-group">
            <label>Contraseña <span class="req-mark">*</span></label>
            <div class="password-field-wrap">
              <input
                v-model="password"
                :type="showPassword ? 'text' : 'password'"
                class="custom-input"
                placeholder="••••••••"
              />
              <button
                type="button"
                class="password-toggle-btn"
                :aria-label="showPassword ? 'Ocultar contraseña' : 'Mostrar contraseña'"
                @click="toggleShowPassword"
              >
                <svg v-if="!showPassword" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path d="M1 12s4-7 11-7 11 7 11 7-4 7-11 7-11-7-11-7Z" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/>
                  <circle cx="12" cy="12" r="3" stroke="currentColor" stroke-width="1.8"/>
                </svg>
                <svg v-else viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path d="M17.94 17.94A10.94 10.94 0 0 1 12 19c-7 0-11-7-11-7a20.6 20.6 0 0 1 5.06-5.94M9.9 4.24A10.4 10.4 0 0 1 12 4c7 0 11 7 11 7a20.6 20.6 0 0 1-3.16 4.24M14.12 14.12a3 3 0 1 1-4.24-4.24" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/>
                  <path d="M1 1l22 22" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/>
                </svg>
              </button>
            </div>
          </div>

          <!-- RECUPERAR -->
          <div class="forgot-password-wrap">
            <button
              type="button"
              class="forgot-password-btn"
              @click="showRecoverModal = true"
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
            <span v-if="!loading">Ingresar</span>
            <span v-else>Ingresando...</span>
          </button>

        </form>

        <!-- FOOTER -->
        <div class="form-footer">
          <p>
            ¿No tienes cuenta?
            <RouterLink to="/registro" class="register-link">Registrarse</RouterLink>
          </p>
        </div>

      </div>

    </div>

    <!-- ══════════════════════════════════════
         MODAL RECUPERAR CONTRASEÑA
    ══════════════════════════════════════ -->

    <div
      v-if="showRecoverModal"
      class="modal-overlay"
      @click.self="cerrarModal"
    >

      <div class="recover-modal">

        <!-- CERRAR -->
        <button class="close-modal" @click="cerrarModal" aria-label="Cerrar">
          <svg viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M18 6L6 18M6 6l12 12" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
          </svg>
        </button>

        <!-- ════════════════════
             ETAPA 1: BUSCAR
        ════════════════════ -->
        <template v-if="etapaRecuperacion === 'buscar'">

          <div class="r-icon-wrap">
            <svg viewBox="0 0 48 48" fill="none" xmlns="http://www.w3.org/2000/svg">
              <rect width="48" height="48" rx="14" fill="rgba(58,71,60,0.08)"/>
              <path d="M10 16a2 2 0 0 1 2-2h24a2 2 0 0 1 2 2v16a2 2 0 0 1-2 2H12a2 2 0 0 1-2-2V16Z" stroke="#2D372F" stroke-width="1.8"/>
              <path d="M10 17l14 10 14-10" stroke="#2D372F" stroke-width="1.8" stroke-linecap="round"/>
            </svg>
          </div>

          <h3 class="r-modal-title">Recuperar contraseña</h3>

          <p class="r-subtitle">
            Ingresa tu correo y te enviaremos un código
            para restablecer tu contraseña.
          </p>

          <div class="r-divider-line"></div>

          <!-- Error -->
          <div v-if="recoverError" class="r-alert-error">
            {{ recoverError }}
          </div>

          <!-- Campo correo -->
          <div class="r-field-group">
            <label class="r-field-label">
              Correo electrónico
            </label>
            <input
              v-model="recoverEmail"
              type="email"
              class="r-field-input"
              placeholder="correo@ejemplo.com"
              @keyup.enter="recuperarPassword"
            />
          </div>

          <!-- Info -->
          <div class="r-info-box">
            <svg viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M12 3l7 3v6c0 4.5-3 8-7 9-4-1-7-4.5-7-9V6l7-3z" stroke="#5F7A62" stroke-width="1.6" stroke-linejoin="round"/>
            </svg>
            <span>Te enviaremos un código seguro para que puedas crear una nueva contraseña.</span>
          </div>

          <!-- Botón buscar -->
          <button
            type="button"
            class="r-btn"
            :disabled="enviandoCorreo"
            @click="recuperarPassword"
          >
            <span v-if="enviandoCorreo">Enviando código...</span>
            <span v-else class="r-btn-icon">
              <svg viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M22 2L11 13" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
                <path d="M22 2l-7 20-4-9-9-4 20-7z" stroke="currentColor" stroke-width="2" stroke-linejoin="round"/>
              </svg>
              Enviar código
            </span>
          </button>

          <div class="r-footer-row">
            <span>¿Recordaste tu contraseña?</span>
            <button type="button" class="r-footer-link" @click="cerrarModal">Iniciar sesión</button>
          </div>

        </template>

        <!-- ════════════════════
             ETAPA 2: VERIFICAR
        ════════════════════ -->
        <template v-else-if="etapaRecuperacion === 'verificar'">

          <!-- Ícono -->
          <div class="r-icon-wrap">
            <svg viewBox="0 0 48 48" fill="none" xmlns="http://www.w3.org/2000/svg">
              <rect width="48" height="48" rx="14" fill="rgba(58,71,60,0.08)"/>
              <path d="M10 16a2 2 0 0 1 2-2h24a2 2 0 0 1 2 2v16a2 2 0 0 1-2 2H12a2 2 0 0 1-2-2V16Z" stroke="#2D372F" stroke-width="1.8"/>
              <path d="M10 17l14 10 14-10" stroke="#2D372F" stroke-width="1.8" stroke-linecap="round"/>
            </svg>
          </div>

          <h3 class="r-modal-title">Verifica tu código</h3>

          <p class="r-subtitle">
            Hemos enviado un código de verificación a
            <strong class="r-verify-email">{{ recoverEmail }}</strong>
          </p>
          <p class="r-hint">Revisa tu bandeja de entrada o la carpeta de spam.</p>

          <div class="r-divider-line"></div>

          <!-- Error -->
          <div v-if="recoverError" class="r-alert-error">
            {{ recoverError }}
          </div>

          <!-- Campo código -->
          <div class="r-field-group">
            <label class="r-field-label">
              Código de verificación
            </label>
            <input
              v-model="codigoIngresado"
              class="r-field-input r-code-input"
              placeholder="123456"
              maxlength="6"
              inputmode="numeric"
              @keyup.enter="verificarCodigoRecuperacion"
            />
          </div>

          <!-- Botón verificar -->
          <button
            type="button"
            class="r-btn"
            @click="verificarCodigoRecuperacion"
          >
            Verificar código
          </button>

          <!-- Reenviar -->
          <div class="r-resend-row">
            <span class="r-resend-label">¿No recibiste el correo?</span>
            <button
              type="button"
              class="r-resend-btn"
              :disabled="reenvioDeshabilitado"
              @click="reenviarCodigo"
            >
              {{ reenvioDeshabilitado ? 'Código reenviado' : 'Reenviar código' }}
            </button>
          </div>

          <!-- Volver -->
          <div class="r-back-row">
            <button
              type="button"
              class="r-back-btn"
              @click="etapaRecuperacion = 'buscar'; recoverError = ''"
            >
              ← Volver
            </button>
          </div>

        </template>

        <!-- ════════════════════════
             ETAPA 3: NUEVA PASSWORD
        ════════════════════════ -->
        <template v-else-if="etapaRecuperacion === 'nuevaPassword'">

          <div class="r-icon-wrap">
            <svg viewBox="0 0 48 48" fill="none" xmlns="http://www.w3.org/2000/svg">
              <rect width="48" height="48" rx="14" fill="rgba(58,71,60,0.08)"/>
              <path d="M14 22a2 2 0 0 1 2-2h16a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H16a2 2 0 0 1-2-2V22Z" stroke="#2D372F" stroke-width="1.8"/>
              <path d="M18 20v-4a6 6 0 0 1 12 0v4" stroke="#2D372F" stroke-width="1.8" stroke-linecap="round"/>
            </svg>
          </div>

          <h3 class="r-modal-title">Nueva contraseña</h3>

          <p class="r-subtitle">
            Crea una nueva contraseña para tu cuenta
            <strong class="r-verify-email">{{ recoverEmail }}</strong>
          </p>

          <div class="r-divider-line"></div>

          <!-- Error -->
          <div v-if="recoverError" class="r-alert-error">
            {{ recoverError }}
          </div>

          <!-- Nueva contraseña -->
          <div class="r-field-group">
            <label class="r-field-label">
              Nueva contraseña
            </label>
            <input
              v-model="nuevaPassword"
              type="password"
              class="r-field-input"
              placeholder="Mínimo 8 caracteres"
            />
          </div>

          <!-- Confirmar contraseña -->
          <div class="r-field-group">
            <label class="r-field-label">
              Confirmar contraseña
            </label>
            <input
              v-model="confirmarNuevaPassword"
              type="password"
              class="r-field-input"
              placeholder="Repetir contraseña"
              @keyup.enter="actualizarPassword"
            />
          </div>

          <!-- Botón actualizar -->
          <button
            type="button"
            class="r-btn"
            :disabled="actualizandoPassword"
            @click="actualizarPassword"
          >
            <span v-if="actualizandoPassword" class="btn-spinner"></span>
            {{ actualizandoPassword ? 'Actualizando...' : 'Actualizar contraseña' }}
          </button>

          <!-- Volver -->
          <div class="r-back-row">
            <button
              type="button"
              class="r-back-btn"
              @click="etapaRecuperacion = 'verificar'; recoverError = ''"
            >
              ← Volver
            </button>
          </div>

        </template>

        <!-- ════════════════════
             ETAPA 4: ÉXITO
        ════════════════════ -->
        <template v-else-if="etapaRecuperacion === 'exito'">

          <div class="r-success-card">

            <!-- Ícono check -->
            <div class="r-icon-wrap r-icon-wrap-success">
              <svg viewBox="0 0 48 48" fill="none" xmlns="http://www.w3.org/2000/svg">
                <rect width="48" height="48" rx="14" fill="rgba(58,71,60,0.1)"/>
                <path d="M14 24l7 7 12-14" stroke="#2D372F" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"/>
              </svg>
            </div>

            <h3 class="r-modal-title">Contraseña actualizada</h3>

            <p class="r-subtitle">
              Tu contraseña ha sido actualizada correctamente. Ya puedes iniciar sesión con tu nueva contraseña.
            </p>

            <div class="r-success-closing">
              Cerrando automáticamente...
            </div>

          </div>

        </template>

      </div>

    </div>

  </div>

</template>

<style scoped>

/* ═══════════════════════════════════
   LAYOUT BASE
═══════════════════════════════════ */

.login-container {
  min-height: 100vh;
  display: flex;
  background: #FAFAFA;
}

/* ═══════════════════════════════════
   PANEL VISUAL IZQUIERDO
═══════════════════════════════════ */

.login-visual {
  flex: 1;
  background: #2D372F;
  padding: 56px 52px;
  display: flex;
  flex-direction: column;
  position: relative;
  overflow: hidden;
}

.visual-inner {
  display: flex;
  flex-direction: column;
  height: 100%;
  position: relative;
  z-index: 1;
}

.logo-link {
  font-size: 26px;
  font-weight: 700;
  color: #FAFAFA;
  text-decoration: none;
  letter-spacing: -0.4px;
}

.logo-green {
  color: #C9A06A;
}

.visual-content {
  margin-top: auto;
  margin-bottom: auto;
  padding: 48px 0;
}

.visual-title {
  font-size: 54px;
  font-weight: 800;
  color: #FAFAFA;
  line-height: 1.12;
  letter-spacing: -1px;
  margin: 0 0 22px;
}

.visual-divider {
  width: 34px;
  height: 3px;
  background: #C9A06A;
  border-radius: 2px;
  margin-bottom: 24px;
}

.visual-description {
  font-size: 15px;
  color: #C3CDC4;
  line-height: 1.75;
  max-width: 340px;
}

.visual-stats {
  display: flex;
  gap: 40px;
  padding-top: 8px;
}

.visual-stat {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.visual-stat-value {
  font-size: 26px;
  font-weight: 800;
  color: #FAFAFA;
  letter-spacing: -0.4px;
}

.visual-stat-label {
  font-size: 12px;
  color: #9DAE9F;
}

/* ═══════════════════════════════════
   LADO FORMULARIO
═══════════════════════════════════ */

.login-form-side {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 48px 40px;
  overflow-y: auto;
}

.form-container {
  width: 100%;
  max-width: 460px;
}

.form-header {
  margin-bottom: 28px;
}

.form-header h2 {
  font-size: 36px;
  font-weight: 800;
  color: #2D372F;
  letter-spacing: -0.6px;
  margin: 0 0 6px;
}

.form-header p {
  font-size: 14px;
  color: #7A876B;
  margin: 0;
}

/* ═══════════════════════════════════
   ACCESO RÁPIDO ADMIN
═══════════════════════════════════ */

.admin-box {
  background: #F4F6F4;
  border: 1px solid rgba(45,55,47,0.08);
  padding: 20px;
  border-radius: 16px;
  margin-bottom: 24px;
}

.admin-box-title {
  margin: 0 0 6px;
  font-size: 14px;
  font-weight: 700;
  color: #2D372F;
}

.admin-box-desc {
  margin: 0 0 14px;
  color: #7A876B;
  font-size: 13px;
  line-height: 1.6;
}

.admin-box-btn {
  width: 100%;
  height: 46px;
  border: none;
  border-radius: 12px;
  background: #2D372F;
  color: white;
  font-size: 13.5px;
  font-weight: 700;
  cursor: pointer;
  transition: background 0.2s ease, transform 0.15s ease;
}

.admin-box-btn:hover {
  background: #232B25;
  transform: translateY(-1px);
}

/* ═══════════════════════════════════
   ALERTAS
═══════════════════════════════════ */

.error-box {
  background: rgba(196,82,82,0.09);
  color: #B04040;
  padding: 14px 18px;
  border-radius: 14px;
  margin-bottom: 20px;
  font-weight: 600;
  font-size: 13px;
}

/* ═══════════════════════════════════
   CAMPOS
═══════════════════════════════════ */

.input-group {
  margin-bottom: 18px;
}

.input-group label {
  display: block;
  margin-bottom: 8px;
  font-size: 13.5px;
  font-weight: 700;
  color: #2D372F;
}

.req-mark {
  color: #C9A06A;
  font-weight: 700;
}

.custom-input {
  width: 100%;
  height: 52px;
  border-radius: 14px;
  border: 1.5px solid #DCE4DD;
  background: #F8FAF8;
  padding: 0 16px;
  font-size: 14px;
  color: #2D372F;
  outline: none;
  transition: border-color 0.2s ease, box-shadow 0.2s ease, background 0.2s ease;
  box-sizing: border-box;
}

.custom-input::placeholder { color: #9AA89B; }

.custom-input:hover {
  border-color: #B0C4B2;
  background: #FDFEFE;
}

.custom-input:focus {
  border-color: #2D372F;
  background: #fff;
  box-shadow: 0 0 0 3px rgba(45,55,47,0.08);
}

/* ─────────────────────────────
   CAMPO CONTRASEÑA — mostrar/ocultar
───────────────────────────── */

.password-field-wrap {
  position: relative;
}

.password-field-wrap .custom-input {
  padding-right: 48px;
}

.password-toggle-btn {
  position: absolute;
  top: 50%;
  right: 14px;
  transform: translateY(-50%);
  width: 28px;
  height: 28px;
  border: none;
  background: none;
  padding: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #9AA89B;
  cursor: pointer;
  transition: color 0.18s ease;
}

.password-toggle-btn:hover {
  color: #2D372F;
}

.password-toggle-btn svg {
  width: 19px;
  height: 19px;
}

/* ═══════════════════════════════════
   RECUPERAR CONTRASEÑA — link
═══════════════════════════════════ */

.forgot-password-wrap {
  display: flex;
  justify-content: flex-end;
  margin-top: -4px;
  margin-bottom: 22px;
}

.forgot-password-btn {
  border: none;
  background: none;
  color: #2D372F;
  font-size: 12.5px;
  font-weight: 600;
  cursor: pointer;
  padding: 0;
  border-bottom: 1.5px solid rgba(45,55,47,0.25);
  padding-bottom: 1px;
  transition: color 0.18s, border-color 0.18s;
}

.forgot-password-btn:hover {
  color: #C9A06A;
  border-bottom-color: #C9A06A;
}

/* ═══════════════════════════════════
   BOTÓN PRINCIPAL
═══════════════════════════════════ */

.btn-login {
  width: 100%;
  height: 56px;
  border: none;
  border-radius: 16px;
  background: #2D372F;
  color: white;
  font-size: 15px;
  font-weight: 700;
  cursor: pointer;
  margin-top: 4px;
  letter-spacing: 0.2px;
  transition: background 0.2s ease, transform 0.15s ease;
}

.btn-login:hover:not(:disabled) {
  background: #232B25;
  transform: translateY(-1px);
}

.btn-login:active:not(:disabled) {
  transform: translateY(0);
}

.btn-login:disabled {
  background: #B0BAB2;
  cursor: not-allowed;
}

/* ═══════════════════════════════════
   FOOTER
═══════════════════════════════════ */

.form-footer {
  margin-top: 24px;
  text-align: center;
  color: #7A876B;
  font-size: 13px;
}

.register-link {
  color: #2D372F;
  font-weight: 700;
  text-decoration: none;
  border-bottom: 1.5px solid rgba(45,55,47,0.25);
  padding-bottom: 1px;
}

.register-link:hover {
  color: #C9A06A;
  border-bottom-color: #C9A06A;
}

/* ═══════════════════════════════════
   MODAL OVERLAY
═══════════════════════════════════ */

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(20,24,20,0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 9999;
  padding: 20px;
}

/* ═══════════════════════════════════════
   MODAL RECUPERAR
═══════════════════════════════════════ */

.recover-modal {
  position: relative;
  width: 100%;
  max-width: 440px;
  background: #FFFFFF;
  border-radius: 24px;
  padding: 40px 32px 32px;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 24px 64px rgba(20,24,20,0.22);
  text-align: center;
}

.close-modal {
  position: absolute;
  top: 18px;
  right: 18px;
  width: 34px;
  height: 34px;
  border-radius: 10px;
  border: 1px solid #DCE4DD;
  background: #F4F6F4;
  cursor: pointer;
  color: #7A876B;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background 0.18s, border-color 0.18s, color 0.18s;
}

.close-modal svg {
  width: 13px;
  height: 13px;
}

.close-modal:hover {
  background: #DCE4DD;
  border-color: #B0C4B2;
  color: #2D372F;
}

/* ─────────────────────────────
   ÍCONO SUPERIOR
───────────────────────────── */

.r-icon-wrap {
  width: 60px;
  height: 60px;
  margin: 0 auto 18px;
}

.r-icon-wrap svg {
  width: 100%;
  height: 100%;
}

/* ─────────────────────────────
   TÍTULO / SUBTÍTULO
───────────────────────────── */

.r-modal-title {
  font-size: 22px;
  font-weight: 800;
  color: #2D372F;
  letter-spacing: -0.4px;
  margin: 0 0 10px;
}

.r-subtitle {
  font-size: 14px;
  color: #5F6A61;
  margin: 0 auto 4px;
  line-height: 1.6;
  max-width: 320px;
}

.r-verify-email {
  color: #2D372F;
  font-weight: 700;
}

.r-hint {
  font-size: 12px;
  color: #9AA89B;
  margin: 6px 0 0;
}

/* ─────────────────────────────
   SEPARADOR
───────────────────────────── */

.r-divider-line {
  width: 100%;
  height: 1px;
  background: #E5E9E5;
  margin: 22px 0;
}

/* ─────────────────────────────
   ALERTA ERROR
───────────────────────────── */

.r-alert-error {
  padding: 13px 16px;
  border-radius: 12px;
  font-size: 13px;
  font-weight: 600;
  margin-bottom: 16px;
  line-height: 1.5;
  background: rgba(196,82,82,0.09);
  color: #B04040;
  border: 1px solid rgba(196,82,82,0.18);
  text-align: left;
}

/* ─────────────────────────────
   CAMPOS
───────────────────────────── */

.r-field-group {
  margin-bottom: 16px;
  text-align: left;
}

.r-field-label {
  display: block;
  margin-bottom: 7px;
  font-size: 13px;
  font-weight: 700;
  color: #2D372F;
}

.r-req {
  color: #C9A06A;
  font-weight: 700;
  margin-left: 1px;
}

.r-field-input {
  width: 100%;
  height: 50px;
  border-radius: 14px;
  border: 1.5px solid #DCE4DD;
  background: #F8FAF8;
  padding: 0 16px;
  font-size: 14px;
  color: #2D372F;
  outline: none;
  transition: border-color 0.18s ease, box-shadow 0.18s ease, background 0.18s ease;
  box-sizing: border-box;
}

.r-field-input::placeholder {
  color: #9AA89B;
}

.r-field-input:hover {
  border-color: #B0C4B2;
  background: #FDFEFE;
}

.r-field-input:focus {
  border-color: #2D372F;
  background: #fff;
  box-shadow: 0 0 0 3px rgba(45,55,47,0.08);
}

.r-code-input {
  text-align: center;
  font-size: 20px;
  font-weight: 700;
  letter-spacing: 8px;
  color: #2D372F;
}

/* ─────────────────────────────
   CAJA INFORMATIVA
───────────────────────────── */

.r-info-box {
  display: flex;
  align-items: flex-start;
  gap: 10px;
  background: #EDF1EC;
  border-radius: 12px;
  padding: 13px 14px;
  margin: 2px 0 20px;
  text-align: left;
}

.r-info-box svg {
  width: 16px;
  height: 16px;
  flex-shrink: 0;
  margin-top: 2px;
}

.r-info-box span {
  font-size: 12.5px;
  color: #5F6A61;
  line-height: 1.55;
}

/* ─────────────────────────────
   BOTÓN PRINCIPAL
───────────────────────────── */

.r-btn {
  width: 100%;
  height: 54px;
  border: none;
  border-radius: 16px;
  background: #2D372F;
  color: white;
  font-size: 15px;
  font-weight: 700;
  cursor: pointer;
  letter-spacing: 0.2px;
  transition: background 0.2s ease, transform 0.15s ease;
}

.r-btn:hover:not(:disabled) {
  background: #232B25;
  transform: translateY(-1px);
}

.r-btn:active:not(:disabled) {
  transform: translateY(0);
}

.r-btn:disabled {
  background: #B0BAB2;
  cursor: not-allowed;
}

.btn-spinner {
  display: inline-block;
  width: 14px; height: 14px;
  margin-right: 8px;
  vertical-align: -2px;
  border: 2px solid rgba(255,255,255,0.4);
  border-top-color: #fff;
  border-radius: 50%;
  animation: btn-spin 0.7s linear infinite;
}
@keyframes btn-spin { to { transform: rotate(360deg); } }

.r-btn-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 9px;
}

.r-btn-icon svg {
  width: 15px;
  height: 15px;
  flex-shrink: 0;
}

/* ─────────────────────────────
   FOOTER DEL MODAL
───────────────────────────── */

.r-footer-row {
  margin-top: 20px;
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  color: #7A876B;
}

.r-footer-link {
  border: none;
  background: none;
  padding: 0;
  cursor: pointer;
  font-size: 13px;
  font-weight: 700;
  color: #2D372F;
  border-bottom: 1.5px solid rgba(45,55,47,0.25);
  padding-bottom: 1px;
}

.r-footer-link:hover {
  color: #C9A06A;
  border-bottom-color: #C9A06A;
}

/* ─────────────────────────────
   REENVIAR
───────────────────────────── */

.r-resend-row {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  margin-top: 16px;
  font-size: 12.5px;
}

.r-resend-label {
  color: #7A876B;
}

.r-resend-btn {
  background: none;
  border: none;
  font-size: 12.5px;
  font-weight: 700;
  color: #2D372F;
  cursor: pointer;
  padding: 0;
  border-bottom: 1.5px solid rgba(45,55,47,0.25);
  padding-bottom: 1px;
  transition: color 0.18s, border-color 0.18s;
}

.r-resend-btn:hover:not(:disabled) {
  color: #C9A06A;
  border-bottom-color: #C9A06A;
}

.r-resend-btn:disabled {
  color: #9AA89B;
  border-bottom-color: transparent;
  cursor: default;
}

/* ─────────────────────────────
   VOLVER
───────────────────────────── */

.r-back-row {
  margin-top: 14px;
  text-align: center;
}

.r-back-btn {
  background: none;
  border: none;
  font-size: 11.5px;
  color: #9AA89B;
  cursor: pointer;
  padding: 0;
  transition: color 0.18s;
}

.r-back-btn:hover {
  color: #5F6A61;
}

/* ─────────────────────────────
   TARJETA ÉXITO
───────────────────────────── */

.r-success-card {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.r-success-closing {
  font-size: 11px;
  color: #9AA89B;
  font-style: italic;
  margin-top: 14px;
}

/* ═══════════════════════════════════
   RESPONSIVE
═══════════════════════════════════ */

@media (max-width: 900px) {
  .login-visual    { display: none; }
  .login-form-side { padding: 40px 24px; }
}

@media (max-width: 768px) {
  .login-container {
    flex-direction: column;
  }

  .login-visual {
    display: none;
  }

  .login-form-side {
    padding: 36px 20px 56px;
    align-items: flex-start;
  }

  .form-container {
    max-width: 100%;
  }

  .form-header h2 {
    font-size: 28px;
  }

  .custom-input {
    height: 48px;
    font-size: 13px;
  }

  .btn-login {
    height: 50px;
    font-size: 14px;
  }

  .modal-overlay {
    padding: 12px;
    align-items: flex-end;
  }

  .recover-modal {
    max-width: 100%;
    border-radius: 22px 22px 16px 16px;
    padding: 32px 20px 24px;
    max-height: 92vh;
  }

  .r-modal-title {
    font-size: 20px;
  }

  .r-field-input {
    height: 46px;
    font-size: 13px;
  }

  .r-btn {
    height: 48px;
    font-size: 14px;
  }

  .r-code-input {
    font-size: 18px;
    letter-spacing: 6px;
  }

  .r-resend-row {
    flex-direction: column;
    text-align: center;
    gap: 6px;
  }
}

@media (max-width: 480px) {
  .login-form-side { padding: 28px 16px; align-items: flex-start; }
  .form-header h2 { font-size: 25px; }
}

@media (max-width: 380px) {
  .login-form-side {
    padding: 24px 14px 40px;
  }

  .form-header h2 {
    font-size: 22px;
  }

  .admin-box {
    padding: 16px;
  }
}

</style>