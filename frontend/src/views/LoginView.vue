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

const correo   = ref('')
const password = ref('')
const error    = ref('')
const loading  = ref(false)

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

  const usuario = obtenerUsuarioPorCorreo(recoverEmail.value)

const templateParams = {
  user_name: usuario ? usuario.nombre : recoverEmail.value,
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
   UTIL — BUSCAR USUARIO
───────────────────────────── */

function obtenerUsuarioPorCorreo(email) {
  const usuarios = JSON.parse(localStorage.getItem('anhelo_usuarios')) || []
  return usuarios.find(u => u.correo.toLowerCase() === email.toLowerCase())
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

  const usuario = obtenerUsuarioPorCorreo(recoverEmail.value)

  if (!usuario) {
    recoverError.value = 'No existe una cuenta con este correo'
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

function actualizarPassword() {
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

  const usuarios = JSON.parse(localStorage.getItem('anhelo_usuarios')) || []

  const index = usuarios.findIndex(
    u => u.correo.toLowerCase() === recoverEmail.value.toLowerCase()
  )

  if (index === -1) {
    recoverError.value = 'No se encontró la cuenta. Intenta de nuevo.'
    return
  }

  // Actualizar solo el campo password, mantener todo lo demás intacto
  usuarios[index].password = nuevaPassword.value
  localStorage.setItem('anhelo_usuarios', JSON.stringify(usuarios))

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
   ENTRAR COMO ADMIN
───────────────────────────── */

function entrarComoAdmin() {
  const adminDemo = {
    id:      'ADMIN-001',
    nombre:  'Administrador',
    correo:  'admin@anhelopets.cr',
    rol:     'Admin',
    activo:  true
  }
  localStorage.setItem('anhelo_usuario_actual', JSON.stringify(adminDemo))
  router.push('/admin')
}

/* ─────────────────────────────
   LOGIN
───────────────────────────── */

function iniciarSesion() {
  error.value = ''

  if (!correo.value || !password.value) {
    error.value = 'Completa todos los campos'
    return
  }

  loading.value = true

  const usuarios = JSON.parse(localStorage.getItem('anhelo_usuarios')) || []

  const usuario = usuarios.find(u =>
    u.correo.toLowerCase() === correo.value.toLowerCase() &&
    u.password === password.value
  )

  if (!usuario) {
    loading.value = false
    error.value   = 'Correo o contraseña incorrectos'
    return
  }

  if (!usuario.activo) {
    loading.value = false
    error.value   = 'Tu cuenta está inactiva'
    return
  }

  localStorage.setItem('anhelo_usuario_actual', JSON.stringify(usuario))

  setTimeout(() => {
    if (usuario.rol === 'Admin') {
      router.push('/admin')
    } else {
      router.push('/')
    }
  }, 700)
}
</script>

<template>

  <div class="login-container">

    <!-- VISUAL -->

    <div class="login-visual">

      <RouterLink to="/" class="logo-link">
        Anhelo
        <span class="logo-green">Pets</span>
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
          <h2>Iniciar sesión</h2>
          <p>Ingresa tus credenciales</p>
        </div>

        <!-- DEMO ADMIN -->

        <div class="demo-box">
          <strong>Acceso rápido administrador</strong>
          <p>Ingresa automáticamente al panel admin.</p>
          <button
            type="button"
            class="demo-admin-btn"
            @click="entrarComoAdmin"
          >
            Entrar como administrador
          </button>
        </div>

        <!-- ERROR -->

        <div v-if="error" class="error-box">
          {{ error }}
        </div>

        <!-- FORM -->

        <form @submit.prevent="iniciarSesion">

          <!-- CORREO -->
          <div class="input-group">
            <label>Correo electrónico</label>
            <input
              v-model="correo"
              type="email"
              class="custom-input"
              placeholder="correo@ejemplo.com"
            />
          </div>

          <!-- PASSWORD -->
          <div class="input-group">
            <label>Contraseña</label>
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

        <!-- ── ENCABEZADO MODAL ── -->
        <div class="modal-header">
          <h3>Recuperar contraseña</h3>
          <button class="close-modal" @click="cerrarModal">×</button>
        </div>

        <!-- ════════════════════
             ETAPA 1: BUSCAR
        ════════════════════ -->
        <template v-if="etapaRecuperacion === 'buscar'">

          <p class="recover-text">
            Ingresa tu correo electrónico y te enviaremos un código para restablecer tu contraseña.
          </p>

          <!-- Error -->
          <div v-if="recoverError" class="r-alert-error">
            {{ recoverError }}
          </div>

          <!-- Campo correo -->
          <div class="r-field-group">
            <label class="r-field-label">
              Correo electrónico <span class="r-req">*</span>
            </label>
            <input
              v-model="recoverEmail"
              type="email"
              class="r-field-input"
              placeholder="correo@ejemplo.com"
              @keyup.enter="recuperarPassword"
            />
          </div>

          <!-- Botón buscar -->
          <button
            type="button"
            class="r-btn"
            :disabled="enviandoCorreo"
            @click="recuperarPassword"
          >
            <span v-if="enviandoCorreo">Enviando código...</span>
            <span v-else>Buscar cuenta</span>
          </button>

        </template>

        <!-- ════════════════════
             ETAPA 2: VERIFICAR
        ════════════════════ -->
        <template v-else-if="etapaRecuperacion === 'verificar'">

          <!-- Ícono -->
          <div class="r-verify-icon-box">
            <svg viewBox="0 0 48 48" fill="none" xmlns="http://www.w3.org/2000/svg">
              <rect width="48" height="48" rx="14" fill="rgba(201,160,106,0.12)"/>
              <path d="M10 16a2 2 0 0 1 2-2h24a2 2 0 0 1 2 2v16a2 2 0 0 1-2 2H12a2 2 0 0 1-2-2V16Z" stroke="#C9A06A" stroke-width="1.8"/>
              <path d="M10 17l14 10 14-10" stroke="#C9A06A" stroke-width="1.8" stroke-linecap="round"/>
            </svg>
          </div>

          <p class="r-verify-subtitle">
            Hemos enviado un código de verificación a
            <strong class="r-verify-email">{{ recoverEmail }}</strong>
          </p>
          <p class="r-verify-hint">Revisa tu bandeja de entrada o la carpeta de spam.</p>

          <div class="r-divider"></div>

          <!-- Error -->
          <div v-if="recoverError" class="r-alert-error">
            {{ recoverError }}
          </div>

          <!-- Campo código -->
          <div class="r-field-group">
            <label class="r-field-label">
              Código de verificación <span class="r-req">*</span>
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

          <p class="recover-text">
            Crea una nueva contraseña para tu cuenta
            <strong class="r-verify-email">{{ recoverEmail }}</strong>
          </p>

          <div class="r-divider"></div>

          <!-- Error -->
          <div v-if="recoverError" class="r-alert-error">
            {{ recoverError }}
          </div>

          <!-- Nueva contraseña -->
          <div class="r-field-group">
            <label class="r-field-label">
              Nueva contraseña <span class="r-req">*</span>
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
              Confirmar contraseña <span class="r-req">*</span>
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
            @click="actualizarPassword"
          >
            Actualizar contraseña
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
            <div class="r-success-icon">
              <svg viewBox="0 0 52 52" fill="none" xmlns="http://www.w3.org/2000/svg">
                <rect width="52" height="52" rx="16" fill="rgba(58,71,60,0.10)"/>
                <path d="M16 26l8 8 12-14" stroke="#3A473C" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"/>
              </svg>
            </div>

            <h4 class="r-success-title">Contraseña actualizada</h4>

            <p class="r-success-body">
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

/* ═══════════════════════════════
   LOGIN BASE — sin cambios
═══════════════════════════════ */

.login-container {
  min-height: 100vh;
  display: flex;
  background: #FAFAFA;
}

.login-visual {
  flex: 1;
  background: linear-gradient(135deg, #3A473C, #7C927E);
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

.demo-box {
  background: rgba(146,168,148,0.12);
  border: 1px solid rgba(146,168,148,0.18);
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
  background: linear-gradient(135deg, #3A473C, #556857);
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
  border-color: #3A473C;
  background: white;
}

.btn-login {
  width: 100%;
  height: 58px;
  border: none;
  border-radius: 18px;
  background: linear-gradient(135deg, #3A473C, #7C927E);
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

.error-box {
  background: rgba(235,119,119,0.12);
  color: #C45252;
  padding: 16px;
  border-radius: 16px;
  margin-bottom: 20px;
  font-weight: 700;
}

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

/* ═══════════════════════════════
   MODAL OVERLAY — sin cambios
═══════════════════════════════ */

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 9999;
  padding: 20px;
}

/* ═══════════════════════════════════════
   MODAL RECUPERAR — estilo RegistroView
═══════════════════════════════════════ */

.recover-modal {
  width: 100%;
  max-width: 480px;
  background: #fff;
  border: 1.5px solid rgba(201,160,106,0.28);
  border-radius: 22px;
  padding: 32px;
  box-shadow: 0 12px 48px rgba(45,55,47,0.14);
  max-height: 90vh;
  overflow-y: auto;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.modal-header h3 {
  font-size: 22px;
  font-weight: 800;
  color: #2D372F;
  letter-spacing: -0.4px;
  margin: 0;
}

.close-modal {
  width: 36px;
  height: 36px;
  border-radius: 10px;
  border: 1.5px solid #DCE4DD;
  background: #F4F6F4;
  font-size: 20px;
  cursor: pointer;
  color: #7A876B;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background 0.18s, border-color 0.18s;
  line-height: 1;
}

.close-modal:hover {
  background: #DCE4DD;
  border-color: #B0C4B2;
}

.recover-text {
  font-size: 14px;
  color: #7A876B;
  margin: 0 0 20px;
  line-height: 1.65;
}

/* ─────────────────────────────
   ALERTA ERROR — igual a RegistroView
───────────────────────────── */

.r-alert-error {
  padding: 14px 18px;
  border-radius: 12px;
  font-size: 13px;
  font-weight: 600;
  margin-bottom: 18px;
  line-height: 1.5;
  background: rgba(196,82,82,0.09);
  color: #b04040;
  border: 1px solid rgba(196,82,82,0.18);
}

/* ─────────────────────────────
   CAMPOS — igual a RegistroView
───────────────────────────── */

.r-field-group {
  margin-bottom: 18px;
  position: relative;
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
  border-radius: 12px;
  border: 1.5px solid #DCE4DD;
  background: #FAFAFA;
  padding: 0 16px;
  font-size: 14px;
  color: #2D372F;
  outline: none;
  transition: border-color 0.18s ease, box-shadow 0.18s ease, background 0.18s ease;
  box-sizing: border-box;
}

.r-field-input::placeholder {
  color: #B0BAB2;
}

.r-field-input:hover {
  border-color: #B0C4B2;
  background: #FDFEFE;
}

.r-field-input:focus {
  border-color: #3A473C;
  background: #fff;
  box-shadow: 0 0 0 3px rgba(58,71,60,0.08);
}

.r-code-input {
  text-align: center;
  font-size: 22px;
  font-weight: 700;
  letter-spacing: 8px;
  color: #2D372F;
}

/* ─────────────────────────────
   BOTÓN PRINCIPAL — igual a RegistroView
───────────────────────────── */

.r-btn {
  width: 100%;
  height: 52px;
  border: none;
  border-radius: 14px;
  background: #3A473C;
  color: white;
  font-size: 15px;
  font-weight: 700;
  cursor: pointer;
  letter-spacing: 0.2px;
  box-shadow: 0 4px 14px rgba(58,71,60,0.22);
  transition: background 0.2s ease, transform 0.15s ease, box-shadow 0.2s ease;
  margin-top: 4px;
}

.r-btn:hover:not(:disabled) {
  background: #2D372F;
  transform: translateY(-1px);
  box-shadow: 0 6px 20px rgba(58,71,60,0.28);
}

.r-btn:active:not(:disabled) {
  transform: translateY(0);
}

.r-btn:disabled {
  background: #B0BAB2;
  box-shadow: none;
  cursor: not-allowed;
}

/* ─────────────────────────────
   SEPARADOR DORADO
───────────────────────────── */

.r-divider {
  width: 36px;
  height: 2px;
  background: #C9A06A;
  border-radius: 2px;
  margin: 0 auto 20px;
}

/* ─────────────────────────────
   VERIFICACIÓN — ícono y textos
───────────────────────────── */

.r-verify-icon-box {
  width: 56px;
  height: 56px;
  margin: 0 auto 16px;
  display: block;
}

.r-verify-icon-box svg {
  width: 100%;
  height: 100%;
}

.r-verify-subtitle {
  font-size: 14px;
  color: #5F6A61;
  text-align: center;
  margin: 0 0 6px;
  line-height: 1.55;
}

.r-verify-email {
  color: #2D372F;
  font-weight: 700;
}

.r-verify-hint {
  font-size: 12px;
  color: #9AA89B;
  text-align: center;
  margin: 0 0 16px;
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
  font-size: 13px;
}

.r-resend-label {
  color: #7A876B;
}

.r-resend-btn {
  background: none;
  border: none;
  font-size: 13px;
  font-weight: 700;
  color: #3A473C;
  cursor: pointer;
  padding: 0;
  border-bottom: 1.5px solid rgba(58,71,60,0.25);
  padding-bottom: 1px;
  transition: color 0.18s, border-color 0.18s;
}

.r-resend-btn:hover:not(:disabled) {
  color: #2D372F;
  border-bottom-color: #2D372F;
}

.r-resend-btn:disabled {
  color: #B0BAB2;
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
  font-size: 12px;
  color: #9AA89B;
  cursor: pointer;
  padding: 0;
  transition: color 0.18s;
}

.r-back-btn:hover {
  color: #5F6A61;
}

/* ─────────────────────────────
   TARJETA ÉXITO — igual a RegistroView verify-card
───────────────────────────── */

.r-success-card {
  background: #F4F6F4;
  border: 1.5px solid rgba(58,71,60,0.12);
  border-radius: 18px;
  padding: 28px 24px;
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
}

.r-success-icon {
  width: 56px;
  height: 56px;
  margin-bottom: 18px;
}

.r-success-icon svg {
  width: 100%;
  height: 100%;
}

.r-success-title {
  font-size: 20px;
  font-weight: 800;
  color: #2D372F;
  letter-spacing: -0.4px;
  margin: 0 0 10px;
}

.r-success-body {
  font-size: 13px;
  color: #5F6A61;
  line-height: 1.65;
  margin: 0 0 16px;
}

.r-success-closing {
  font-size: 11px;
  color: #9AA89B;
  font-style: italic;
}

/* ═══════════════════════════════
   RESPONSIVE — sin cambios
═══════════════════════════════ */

@media (max-width: 900px) {
  .login-visual    { display: none; }
  .login-form-side { flex: 1; }
}


/* ── MOBILE RESPONSIVE ── */
@media (max-width: 768px) {
  .login-container {
    flex-direction: column;
  }

  .login-visual {
    display: none;
  }

  .login-form-side {
    flex: 1;
    padding: 36px 20px 48px;
    align-items: flex-start;
  }

  .form-container {
    max-width: 100%;
  }

  .form-header h2 {
    font-size: 30px;
  }

  .custom-input {
    height: 52px;
    font-size: 14px;
  }

  .btn-login {
    height: 52px;
    font-size: 14px;
  }

  /* Modal recuperar */
  .modal-overlay {
    padding: 12px;
    align-items: flex-end;
  }

  .recover-modal {
    max-width: 100%;
    border-radius: 20px 20px 16px 16px;
    padding: 24px 20px;
    max-height: 92vh;
  }

  .modal-header h3 {
    font-size: 18px;
  }

  .r-field-input {
    height: 46px;
    font-size: 13px;
  }

  .r-btn {
    height: 48px;
    font-size: 14px;
  }

  .r-verify-subtitle {
    font-size: 13px;
  }

  .r-code-input {
    font-size: 20px;
    letter-spacing: 6px;
    text-align: center;
  }

  .r-resend-row {
    flex-direction: column;
    text-align: center;
    gap: 6px;
  }
}

@media (max-width: 380px) {
  .login-form-side {
    padding: 24px 14px 40px;
  }

  .form-header h2 {
    font-size: 26px;
  }

  .demo-box {
    padding: 14px;
  }
}


</style>
