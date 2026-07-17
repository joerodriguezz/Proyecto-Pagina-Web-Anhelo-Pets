<script setup>

import {
  ref,
  computed,
  onMounted,
  onBeforeUnmount
} from 'vue'

import {
  RouterLink,
  useRouter
} from 'vue-router'

import {
  countryList,
  phoneCodesList
} from '../data/paises'

import { register } from '../services/userServices'

console.log("Registro cargado")
const router = useRouter()

/* ─────────────────────────────
   EMAILJS CONFIG
───────────────────────────── */

const EMAILJS_SERVICE_ID  = 'service_okmmsx7'
const EMAILJS_TEMPLATE_ID = 'template_aztswto'
const EMAILJS_PUBLIC_KEY  = 'fwH1f3N8oVs98GPAx'

/* ─────────────────────────────
   ESTADOS DEL FORMULARIO
───────────────────────────── */

const nombre           = ref('')
const correo           = ref('')
const telefono         = ref('')
const cedula           = ref('')
const password         = ref('')
const confirmarPassword = ref('')

const error   = ref('')
const success = ref(false)

/* ─────────────────────────────
   TÉRMINOS
───────────────────────────── */

const termsAccepted    = ref(false)
const showTermsExpanded = ref(false)

/* ─────────────────────────────
   TOOLTIP
───────────────────────────── */

const tooltipTrigger = ref(null)
const tooltipCard    = ref(null)

function showTooltip() {
  if (!tooltipTrigger.value || !tooltipCard.value) return
  const rect = tooltipTrigger.value.getBoundingClientRect()
  const card = tooltipCard.value
  card.style.top       = (rect.top - 10) + 'px'
  card.style.left      = (rect.left + rect.width / 2) + 'px'
  card.style.transform = 'translate(-50%, -100%)'
}

function hideTooltip() {}

/* ─────────────────────────────
   VERIFICACIÓN POR CORREO
───────────────────────────── */

const esperandoVerificacion = ref(false)
const codigoIngresado       = ref('')
const codigoGenerado        = ref('')
const errorVerificacion     = ref('')
const enviandoCodigo        = ref(false)
const reenvioDeshabilitado  = ref(false)

function generarCodigoVerificacion() {
  return String(Math.floor(100000 + Math.random() * 900000))
}

async function enviarCorreoVerificacion(codigo) {

  if (!window.emailjs) {

    await new Promise((resolve, reject) => {

      const script = document.createElement('script')

      script.src =
        'https://cdn.jsdelivr.net/npm/@emailjs/browser@4/dist/email.min.js'

      script.onload = resolve
      script.onerror = reject

      document.head.appendChild(script)

    })

    console.log(
      'PUBLIC KEY USADA:',
      EMAILJS_PUBLIC_KEY
    )

    window.emailjs.init({
      publicKey: EMAILJS_PUBLIC_KEY
    })

  }

  const templateParams = {
    user_name: nombre.value,
    verification_code: codigo,
    to_email: correo.value
  }

  const response = await window.emailjs.send(
    EMAILJS_SERVICE_ID,
    EMAILJS_TEMPLATE_ID,
    templateParams
  )

  return response

}


async function verificarCodigo() {
  errorVerificacion.value = ''

  try {
    const user = {
      firstName: nombre.value,
      email: correo.value,
      phonePrimary: `${selectedCountryObject.value.code} ${telefono.value}`,
      nationalId: cedula.value,
      nationality: countrySearch.value,
      password: password.value
    }
    await register(user)
    success.value = true
  } catch (error) {
    console.error(error)
    success.value = false
    error.value = 'No se pudo crear el usuario'
  }


  setTimeout(() => {
    router.push('/')
  }, 1000)


}

async function reenviarCodigo() {
  reenvioDeshabilitado.value = true
  errorVerificacion.value    = ''

  try {
    const nuevoCodigo    = generarCodigoVerificacion()
    codigoGenerado.value = nuevoCodigo
    codigoIngresado.value = ''

    await enviarCorreoVerificacion(nuevoCodigo)
    console.log('Código reenviado correctamente')
  } catch (err) {

  console.error('ERROR COMPLETO EMAILJS:', err)

  alert(
    JSON.stringify(err)
  )

  error.value =
    'No se pudo enviar el código de verificación.'

  } finally {
    // Esperar 30 segundos antes de permitir otro reenvío
    setTimeout(() => {
      reenvioDeshabilitado.value = false
    }, 30000)
  }
}

/* ─────────────────────────────
   PAISES
───────────────────────────── */

const countrySearch       = ref('')
const showCountryDropdown = ref(false)

const filteredCountries = computed(() => {
  if (!countrySearch.value) return countryList
  return countryList.filter(country =>
    country.toLowerCase().includes(countrySearch.value.toLowerCase())
  )
})

function selectCountry(country) {
  countrySearch.value       = country
  showCountryDropdown.value = false
}

/* ─────────────────────────────
   CODIGOS TELEFONICOS
───────────────────────────── */

const dropdownContainer      = ref(null)
const showCodeDropdown       = ref(false)
const codeSearchQuery        = ref('')
const selectedCountryObject  = ref({ name: 'Costa Rica', code: '+506' })

const filteredPhoneCodes = computed(() => {
  const query = codeSearchQuery.value.toLowerCase().trim()
  if (!query) return phoneCodesList
  return phoneCodesList.filter(item =>
    item.name.toLowerCase().includes(query) || item.code.includes(query)
  )
})

function selectPhoneCode(item) {
  selectedCountryObject.value = item
  showCodeDropdown.value      = false
  codeSearchQuery.value       = ''
}

/* ─────────────────────────────
   CLICK AFUERA
───────────────────────────── */

function handleClickOutside(event) {
  if (
    dropdownContainer.value &&
    !dropdownContainer.value.contains(event.target)
  ) {
    showCodeDropdown.value    = false
    showCountryDropdown.value = false
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside)
})

onBeforeUnmount(() => {
  document.removeEventListener('click', handleClickOutside)
})

/* ─────────────────────────────
   TELEFONO SOLO NUMEROS
───────────────────────────── */

function filterPhoneNumber() {
  telefono.value = telefono.value.replace(/\D/g, '')
}

/* ─────────────────────────────
   REGISTRO (ahora dispara verificación)
───────────────────────────── */

async function crearCuenta() {
  error.value         = ''
  enviandoCodigo.value = true

  // ── Validaciones existentes ──
  if (
    !nombre.value ||
    !correo.value ||
    !telefono.value ||
    !cedula.value ||
    !password.value ||
    !confirmarPassword.value ||
    !countrySearch.value
  ) {
    error.value          = 'Completa todos los campos'
    enviandoCodigo.value = false
    return
  }

  if (password.value !== confirmarPassword.value) {
    error.value          = 'Las contraseñas no coinciden'
    enviandoCodigo.value = false
    return
  }

  if (password.value.length < 8) {
    error.value          = 'La contraseña debe tener mínimo 8 caracteres'
    enviandoCodigo.value = false
    return
  }

  const usuarios     = JSON.parse(localStorage.getItem('anhelo_usuarios')) || []
  const correoExiste = usuarios.find(u =>
    u.correo.toLowerCase() === correo.value.toLowerCase()
  )

  if (correoExiste) {
    error.value          = 'Ya existe una cuenta con este correo'
    enviandoCodigo.value = false
    return
  }

  // ── Generar y enviar código ──
  try {
    const codigo         = generarCodigoVerificacion()
    codigoGenerado.value = codigo

    await enviarCorreoVerificacion(codigo)

    esperandoVerificacion.value = true
  } catch (err) {
    console.error('Error al enviar el correo de verificación:', err)
    error.value = 'No se pudo enviar el código de verificación. Revisa tu correo e intenta de nuevo.'
  } finally {
    enviandoCodigo.value = false
  }
}

</script>

<template>

  <div class="auth-container">

    <!-- ── PANEL VISUAL IZQUIERDO ── -->

    <div class="auth-visual">

      <div class="visual-bg-layer"></div>

      <div class="visual-inner">

        <RouterLink to="/" class="logo-link">
          Anhelo <span class="logo-accent">Pets</span>
        </RouterLink>

        <div class="visual-content">

          <h1 class="visual-title">
            Únete a nuestra comunidad
          </h1>

          <div class="visual-divider"></div>

          <p class="visual-description">
            Crea tu cuenta y forma parte de Anhelo Pets. Juntos hacemos posible encontrarles un hogar a quienes más lo necesitan.
          </p>

          <div class="visual-stats">
            <div class="stat-item">
              <span class="stat-number">+200</span>
              <span class="stat-label">Adopciones</span>
            </div>
            <div class="stat-divider"></div>
            <div class="stat-item">
              <span class="stat-number">+80</span>
              <span class="stat-label">Voluntarios</span>
            </div>
            <div class="stat-divider"></div>
            <div class="stat-item">
              <span class="stat-number">+5</span>
              <span class="stat-label">Años activos</span>
            </div>
          </div>

        </div>

      </div>

    </div>

    <!-- ── FORMULARIO ── -->

    <div class="auth-form-side">

      <div class="form-container">

        <!-- ════════════════════════════════
             VISTA 1: FORMULARIO DE REGISTRO
        ════════════════════════════════ -->
        <template v-if="!esperandoVerificacion">

          <div class="form-header">
            <span class="form-eyebrow"></span>
            <h2>Crear cuenta</h2>
            <p>Completa tu información para comenzar</p>
          </div>

          <!-- ERROR -->
          <div v-if="error" class="alert-box alert-error">
            {{ error }}
          </div>

          <form @submit.prevent="crearCuenta">

            <!-- NOMBRE -->
            <div class="field-group">
              <label class="field-label">Nombre completo <span class="req">*</span></label>
              <input
                v-model="nombre"
                class="field-input"
                placeholder="María González"
              />
            </div>

            <!-- CORREO -->
            <div class="field-group">
              <label class="field-label">Correo electrónico <span class="req">*</span></label>
              <input
                v-model="correo"
                type="email"
                class="field-input"
                placeholder="correo@ejemplo.com"
              />
            </div>

            <!-- PAÍS -->
            <div class="field-group">
              <label class="field-label">País <span class="req">*</span></label>
              <div class="autocomplete-wrapper">
                <input
                  v-model="countrySearch"
                  class="field-input"
                  placeholder="Buscar país..."
                  @focus="showCountryDropdown = true"
                />
                <div v-if="showCountryDropdown" class="autocomplete-dropdown">
                  <button
                    v-for="country in filteredCountries"
                    :key="country"
                    type="button"
                    class="dropdown-item"
                    @click="selectCountry(country)"
                  >
                    {{ country }}
                  </button>
                </div>
              </div>
            </div>

            <!-- TELÉFONO -->
            <div class="form-row">

              <div class="field-group" ref="dropdownContainer">
                <label class="field-label">Código país <span class="req">*</span></label>
                <button
                  type="button"
                  class="code-btn"
                  @click="showCodeDropdown = !showCodeDropdown"
                >
                  <span class="code-btn-name">{{ selectedCountryObject.name }}</span>
                  <strong class="code-btn-code">{{ selectedCountryObject.code }}</strong>
                  <svg class="code-btn-chevron" viewBox="0 0 10 6" fill="none">
                    <path d="M1 1l4 4 4-4" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
                  </svg>
                </button>

                <div v-if="showCodeDropdown" class="phone-dropdown">
                  <div class="phone-search-wrap">
                    <input
                      v-model="codeSearchQuery"
                      class="phone-search-input"
                      placeholder="Buscar país o código..."
                    />
                  </div>
                  <div class="phone-list">
                    <button
                      v-for="item in filteredPhoneCodes"
                      :key="item.code + item.name"
                      type="button"
                      class="phone-option"
                      @click="selectPhoneCode(item)"
                    >
                      <span>{{ item.name }}</span>
                      <strong>{{ item.code }}</strong>
                    </button>
                  </div>
                </div>
              </div>

              <div class="field-group">
                <label class="field-label">Teléfono <span class="req">*</span></label>
                <input
                  v-model="telefono"
                  @input="filterPhoneNumber"
                  class="field-input"
                  placeholder="88888888"
                />
              </div>

            </div>

            <!-- CÉDULA -->
            <div class="field-group">
              <label class="field-label">Cédula <span class="req">*</span></label>
              <input
                v-model="cedula"
                class="field-input"
                placeholder="1-2345-6789"
              />
            </div>

            <!-- CONTRASEÑAS -->
            <div class="form-row">
              <div class="field-group">
                <label class="field-label">Contraseña <span class="req">*</span></label>
                <input
                  v-model="password"
                  type="password"
                  class="field-input"
                  placeholder="Mínimo 8 caracteres"
                />
              </div>
              <div class="field-group">
                <label class="field-label">Confirmar contraseña <span class="req">*</span></label>
                <input
                  v-model="confirmarPassword"
                  type="password"
                  class="field-input"
                  placeholder="Repetir contraseña"
                />
              </div>
            </div>

            <!-- ── TÉRMINOS Y CONDICIONES ── -->
            <div class="tc-check-area-standalone">
              <label class="tc-check-row">
                <input
                  type="checkbox"
                  v-model="termsAccepted"
                  class="tc-checkbox"
                />
                <span class="tc-check-label">
                  He leído y acepto los
                  <span
                    class="tc-tooltip-trigger"
                    @mouseenter="showTooltip"
                    @mouseleave="hideTooltip"
                    ref="tooltipTrigger"
                  >
                    <span class="tc-underline">Términos y Condiciones</span>
                    <span class="tc-tooltip-card" ref="tooltipCard" role="tooltip">
                      <span class="tc-tooltip-title">Términos y Condiciones</span>
                      <ul class="tc-tooltip-list">
                        <li>La información proporcionada debe ser verídica y actualizada.</li>
                        <li>El usuario es responsable de mantener segura su cuenta.</li>
                        <li>Los datos serán utilizados únicamente para procesos relacionados con Anhelo Pets.</li>
                        <li>El usuario acepta las políticas de uso de la plataforma.</li>
                        <li>El usuario autoriza el tratamiento de sus datos para los servicios ofrecidos por Anhelo Pets.</li>
                      </ul>
                    </span>
                  </span>
                  de Anhelo Pets
                  <span class="req">*</span>
                </span>
              </label>
            </div>

            <!-- BOTÓN -->
            <button
              type="submit"
              class="btn-register"
              :disabled="!termsAccepted || enviandoCodigo"
            >
              <span v-if="enviandoCodigo">Enviando código...</span>
              <span v-else>Crear mi cuenta</span>
            </button>

            <!-- ENLACE LOGIN -->
            <div class="login-row">
              <span>¿Ya tienes una cuenta?</span>
              <RouterLink to="/login" class="login-link">Inicia sesión</RouterLink>
            </div>

          </form>

        </template>

        <!-- ════════════════════════════════
             VISTA 2: VERIFICACIÓN DE CORREO
        ════════════════════════════════ -->
        <template v-else>

          <!-- SUCCESS -->
          <div v-if="success" class="alert-box alert-success">
            Cuenta creada correctamente
          </div>

          <div class="verify-card">

            <!-- Ícono -->
            <div class="verify-icon-box">
              <svg viewBox="0 0 48 48" fill="none" xmlns="http://www.w3.org/2000/svg">
                <rect width="48" height="48" rx="14" fill="rgba(201,160,106,0.12)"/>
                <path d="M10 16a2 2 0 0 1 2-2h24a2 2 0 0 1 2 2v16a2 2 0 0 1-2 2H12a2 2 0 0 1-2-2V16Z" stroke="#C9A06A" stroke-width="1.8"/>
                <path d="M10 17l14 10 14-10" stroke="#C9A06A" stroke-width="1.8" stroke-linecap="round"/>
              </svg>
            </div>

            <!-- Textos -->
            <div class="verify-header">
              <h2 class="verify-title">Verifica tu correo</h2>
              <p class="verify-subtitle">
                Enviamos un código de 6 dígitos a
                <strong class="verify-email">{{ correo }}</strong>
              </p>
              <p class="verify-hint">Revisa tu bandeja de entrada o la carpeta de spam.</p>
            </div>

            <!-- Separador -->
            <div class="verify-divider"></div>

            <!-- Error verificación -->
            <div v-if="errorVerificacion" class="alert-box alert-error">
              {{ errorVerificacion }}
            </div>

            <!-- Campo código -->
            <div class="field-group">
              <label class="field-label">
                Código de verificación <span class="req">*</span>
              </label>
              <input
                v-model="codigoIngresado"
                class="field-input verify-code-input"
                placeholder="123456"
                maxlength="6"
                inputmode="numeric"
                @keyup.enter="verificarCodigo"
              />
            </div>

            <!-- Botón verificar -->
            <button
              type="button"
              class="btn-register"
              @click="verificarCodigo"
            >
              Verificar y crear cuenta
            </button>

            <!-- Reenviar -->
            <div class="verify-resend-row">
              <span class="verify-resend-label">¿No recibiste el correo?</span>
              <button
                type="button"
                class="verify-resend-btn"
                :disabled="reenvioDeshabilitado"
                @click="reenviarCodigo"
              >
                {{ reenvioDeshabilitado ? 'Código reenviado' : 'Reenviar código' }}
              </button>
            </div>

            <!-- Volver -->
            <div class="verify-back-row">
              <button
                type="button"
                class="verify-back-btn"
                @click="esperandoVerificacion = false; errorVerificacion = ''"
              >
                ← Volver al formulario
              </button>
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

.auth-container {
  min-height: 100vh;
  display: flex;
  background: #F4F6F4;
}

/* ═══════════════════════════════════
   PANEL VISUAL IZQUIERDO
═══════════════════════════════════ */

.auth-visual {
  flex: 1;
  position: relative;
  background: #2D372F;
  padding: 56px 52px;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.visual-bg-layer {
  position: absolute;
  inset: 0;
  background:
    radial-gradient(ellipse 80% 55% at 50% 110%, rgba(201,160,106,0.14) 0%, transparent 65%),
    radial-gradient(ellipse 50% 45% at 90% 0%, rgba(58,71,60,0.7) 0%, transparent 55%);
  pointer-events: none;
}

.visual-inner {
  position: relative;
  z-index: 2;
  display: flex;
  flex-direction: column;
  height: 100%;
}

.logo-link {
  font-size: 26px;
  font-weight: 800;
  color: #F4F6F4;
  text-decoration: none;
  letter-spacing: -0.5px;
}

.logo-accent {
  color: #C9A06A;
}

.visual-content {
  margin-top: auto;
  margin-bottom: auto;
  padding: 40px 0;
}

.visual-eyebrow {
  display: inline-block;
  font-size: 10px;
  letter-spacing: 3px;
  text-transform: uppercase;
  color: #C9A06A;
  font-weight: 700;
  margin-bottom: 20px;
  padding: 5px 14px;
  border: 1px solid rgba(201,160,106,0.35);
  border-radius: 100px;
  background: rgba(201,160,106,0.08);
}

.visual-title {
  font-size: 44px;
  font-weight: 800;
  color: #F4F6F4;
  line-height: 1.1;
  letter-spacing: -1.2px;
  margin: 0 0 20px;
}

.visual-divider {
  width: 36px;
  height: 2px;
  background: #C9A06A;
  border-radius: 2px;
  margin-bottom: 20px;
}

.visual-description {
  font-size: 14px;
  color: rgba(220,228,221,0.75);
  line-height: 1.75;
  max-width: 340px;
  margin-bottom: 40px;
}

.visual-stats {
  display: flex;
  align-items: center;
  gap: 0;
  padding: 22px 24px;
  background: rgba(255,255,255,0.05);
  border: 1px solid rgba(255,255,255,0.08);
  border-radius: 16px;
  width: fit-content;
}

.stat-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 0 24px;
}

.stat-item:first-child { padding-left: 0; }
.stat-item:last-child  { padding-right: 0; }

.stat-number {
  font-size: 22px;
  font-weight: 800;
  color: #C9A06A;
  letter-spacing: -0.5px;
  line-height: 1;
}

.stat-label {
  font-size: 11px;
  color: rgba(220,228,221,0.6);
  margin-top: 4px;
  font-weight: 500;
}

.stat-divider {
  width: 1px;
  height: 32px;
  background: rgba(255,255,255,0.12);
}

/* ═══════════════════════════════════
   LADO FORMULARIO
═══════════════════════════════════ */

.auth-form-side {
  flex: 1.1;
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 48px 40px;
  overflow-y: auto;
}

.form-container {
  width: 100%;
  max-width: 520px;
}

.form-header {
  margin-bottom: 32px;
}

.form-eyebrow {
  display: inline-block;
  font-size: 10px;
  letter-spacing: 2.5px;
  text-transform: uppercase;
  color: #C9A06A;
  font-weight: 700;
  margin-bottom: 10px;
}

.form-header h2 {
  font-size: 32px;
  font-weight: 800;
  color: #2D372F;
  letter-spacing: -0.8px;
  margin: 0 0 6px;
}

.form-header p {
  font-size: 14px;
  color: #7A876B;
  margin: 0;
}

/* ═══════════════════════════════════
   ALERTAS
═══════════════════════════════════ */

.alert-box {
  padding: 14px 18px;
  border-radius: 12px;
  font-size: 13px;
  font-weight: 600;
  margin-bottom: 24px;
  line-height: 1.5;
}

.alert-error {
  background: rgba(196,82,82,0.09);
  color: #b04040;
  border: 1px solid rgba(196,82,82,0.18);
}

.alert-success {
  background: rgba(58,71,60,0.08);
  color: #3A473C;
  border: 1px solid rgba(58,71,60,0.16);
}

/* ═══════════════════════════════════
   CAMPOS
═══════════════════════════════════ */

.field-group {
  margin-bottom: 18px;
  position: relative;
}

.field-label {
  display: block;
  margin-bottom: 7px;
  font-size: 13px;
  font-weight: 700;
  color: #2D372F;
}

.req {
  color: #C9A06A;
  font-weight: 700;
  margin-left: 1px;
}

.field-input {
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

.field-input::placeholder { color: #B0BAB2; }

.field-input:hover {
  border-color: #B0C4B2;
  background: #FDFEFE;
}

.field-input:focus {
  border-color: #3A473C;
  background: #fff;
  box-shadow: 0 0 0 3px rgba(58,71,60,0.08);
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 14px;
}

/* ═══════════════════════════════════
   AUTOCOMPLETE PAÍS
═══════════════════════════════════ */

.autocomplete-wrapper { position: relative; }

.autocomplete-dropdown {
  position: absolute;
  top: calc(100% + 6px);
  left: 0;
  width: 100%;
  background: white;
  border-radius: 14px;
  border: 1.5px solid #DCE4DD;
  max-height: 220px;
  overflow-y: auto;
  z-index: 300;
  box-shadow: 0 8px 28px rgba(0,0,0,0.09);
  padding: 4px 0;
}

.dropdown-item {
  width: 100%;
  border: none;
  background: transparent;
  padding: 11px 16px;
  text-align: left;
  cursor: pointer;
  font-size: 13px;
  color: #2D372F;
  transition: background 0.15s;
  display: block;
}

.dropdown-item:hover { background: #F4F6F4; }

/* ═══════════════════════════════════
   SELECTOR CÓDIGO TELEFÓNICO
═══════════════════════════════════ */

.code-btn {
  width: 100%;
  height: 50px;
  border-radius: 12px;
  border: 1.5px solid #DCE4DD;
  background: #FAFAFA;
  padding: 0 14px;
  display: flex;
  align-items: center;
  gap: 6px;
  cursor: pointer;
  transition: border-color 0.18s, background 0.18s;
  overflow: hidden;
}

.code-btn:hover {
  border-color: #B0C4B2;
  background: #FDFEFE;
}

.code-btn-name {
  font-size: 13px;
  color: #2D372F;
  flex: 1;
  text-align: left;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.code-btn-code {
  font-size: 13px;
  font-weight: 700;
  color: #C9A06A;
  flex-shrink: 0;
}

.code-btn-chevron {
  width: 10px;
  height: 6px;
  color: #7A876B;
  flex-shrink: 0;
}

.phone-dropdown {
  position: absolute;
  top: calc(100% + 6px);
  left: 0;
  width: 100%;
  background: white;
  border-radius: 14px;
  border: 1.5px solid #DCE4DD;
  overflow: hidden;
  z-index: 300;
  box-shadow: 0 8px 28px rgba(0,0,0,0.09);
}

.phone-search-wrap {
  padding: 10px;
  border-bottom: 1px solid #ECEFEC;
}

.phone-search-input {
  width: 100%;
  height: 38px;
  border-radius: 10px;
  border: 1.5px solid #DCE4DD;
  padding: 0 12px;
  font-size: 13px;
  outline: none;
  background: #FAFAFA;
  color: #2D372F;
  box-sizing: border-box;
}

.phone-search-input:focus { border-color: #3A473C; }

.phone-list {
  max-height: 200px;
  overflow-y: auto;
}

.phone-option {
  width: 100%;
  border: none;
  background: transparent;
  padding: 11px 14px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  cursor: pointer;
  font-size: 13px;
  color: #2D372F;
  transition: background 0.15s;
}

.phone-option:hover { background: #F4F6F4; }

.phone-option strong {
  color: #C9A06A;
  font-weight: 700;
}

/* ═══════════════════════════════════
   TÉRMINOS Y CONDICIONES
═══════════════════════════════════ */

.tc-check-area-standalone {
  margin-bottom: 22px;
}

.tc-check-row {
  display: flex;
  align-items: flex-start;
  gap: 10px;
  cursor: pointer;
}

.tc-checkbox {
  width: 16px;
  height: 16px;
  flex-shrink: 0;
  margin-top: 2px;
  cursor: pointer;
  accent-color: #3A473C;
}

.tc-check-label {
  font-size: 12px;
  font-weight: 600;
  color: #2D372F;
  line-height: 1.55;
  cursor: pointer;
  user-select: none;
}

.tc-underline {
  text-decoration: underline;
  text-decoration-color: #C9A06A;
  text-underline-offset: 2px;
  text-decoration-thickness: 1.5px;
  font-weight: 700;
}

/* ═══════════════════════════════════
   TOOLTIP TÉRMINOS Y CONDICIONES
═══════════════════════════════════ */

.tc-tooltip-trigger {
  position: relative;
  display: inline;
}

.tc-tooltip-card {
  position: fixed;
  width: 270px;
  background: #fff;
  border: 1.5px solid rgba(201,160,106,0.35);
  border-radius: 14px;
  padding: 14px 16px;
  box-shadow:
    0 8px 28px rgba(45,55,47,0.13),
    0 2px 8px rgba(45,55,47,0.07);
  pointer-events: none;
  opacity: 0;
  visibility: hidden;
  transition:
    opacity 0.22s ease,
    visibility 0.22s ease;
  z-index: 9999;
}

.tc-tooltip-trigger:hover .tc-tooltip-card {
  opacity: 1;
  visibility: visible;
}

.tc-tooltip-card::after {
  content: '';
  position: absolute;
  top: 100%;
  left: 50%;
  transform: translateX(-50%);
  border: 7px solid transparent;
  border-top-color: #fff;
}

.tc-tooltip-card::before {
  content: '';
  position: absolute;
  top: 100%;
  left: 50%;
  transform: translateX(-50%);
  border: 8px solid transparent;
  border-top-color: rgba(201,160,106,0.35);
  margin-top: 1px;
}

.tc-tooltip-title {
  display: block;
  font-size: 11px;
  font-weight: 800;
  color: #C9A06A;
  letter-spacing: 0.8px;
  text-transform: uppercase;
  margin-bottom: 10px;
  padding-bottom: 8px;
  border-bottom: 1px solid rgba(201,160,106,0.2);
}

.tc-tooltip-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 7px;
}

.tc-tooltip-list li {
  font-size: 11px;
  color: #4A5A4C;
  line-height: 1.55;
  padding-left: 13px;
  position: relative;
}

.tc-tooltip-list li::before {
  content: '';
  position: absolute;
  left: 0;
  top: 6px;
  width: 4px;
  height: 4px;
  border-radius: 50%;
  background: #C9A06A;
}

/* ═══════════════════════════════════
   TARJETA DE VERIFICACIÓN
═══════════════════════════════════ */

.verify-card {
  background: #fff;
  border: 1.5px solid rgba(201,160,106,0.28);
  border-radius: 22px;
  padding: 36px 32px;
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  box-shadow: 0 6px 32px rgba(45,55,47,0.07);
}

.verify-icon-box {
  width: 64px;
  height: 64px;
  margin-bottom: 24px;
}

.verify-icon-box svg {
  width: 100%;
  height: 100%;
}

.verify-header {
  margin-bottom: 6px;
}

.verify-title {
  font-size: 26px;
  font-weight: 800;
  color: #2D372F;
  letter-spacing: -0.6px;
  margin: 0 0 10px;
}

.verify-subtitle {
  font-size: 14px;
  color: #5F6A61;
  margin: 0 0 6px;
  line-height: 1.55;
}

.verify-email {
  color: #2D372F;
  font-weight: 700;
}

.verify-hint {
  font-size: 12px;
  color: #9AA89B;
  margin: 0;
}

.verify-divider {
  width: 40px;
  height: 2px;
  background: #C9A06A;
  border-radius: 2px;
  margin: 20px 0 24px;
  flex-shrink: 0;
}

.verify-code-input {
  text-align: center;
  font-size: 22px;
  font-weight: 700;
  letter-spacing: 8px;
  color: #2D372F;
}

.verify-resend-row {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-top: 18px;
  font-size: 13px;
}

.verify-resend-label {
  color: #7A876B;
}

.verify-resend-btn {
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

.verify-resend-btn:hover:not(:disabled) {
  color: #2D372F;
  border-bottom-color: #2D372F;
}

.verify-resend-btn:disabled {
  color: #B0BAB2;
  border-bottom-color: transparent;
  cursor: default;
}

.verify-back-row {
  margin-top: 14px;
}

.verify-back-btn {
  background: none;
  border: none;
  font-size: 12px;
  color: #9AA89B;
  cursor: pointer;
  padding: 0;
  transition: color 0.18s;
}

.verify-back-btn:hover {
  color: #5F6A61;
}

/* ═══════════════════════════════════
   BOTÓN PRINCIPAL
═══════════════════════════════════ */

.btn-register {
  width: 100%;
  height: 52px;
  border: none;
  border-radius: 14px;
  background: #3A473C;
  color: white;
  font-size: 15px;
  font-weight: 700;
  cursor: pointer;
  margin-top: 4px;
  letter-spacing: 0.2px;
  box-shadow: 0 4px 14px rgba(58,71,60,0.22);
  transition: background 0.2s ease, transform 0.15s ease, box-shadow 0.2s ease;
}

.btn-register:hover:not(:disabled) {
  background: #2D372F;
  transform: translateY(-1px);
  box-shadow: 0 6px 20px rgba(58,71,60,0.28);
}

.btn-register:active:not(:disabled) {
  transform: translateY(0);
}

.btn-register:disabled {
  background: #B0BAB2;
  box-shadow: none;
  cursor: not-allowed;
}

/* ═══════════════════════════════════
   ENLACE LOGIN
═══════════════════════════════════ */

.login-row {
  margin-top: 22px;
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  color: #7A876B;
}

.login-link {
  color: #3A473C;
  font-weight: 700;
  text-decoration: none;
  border-bottom: 1.5px solid rgba(58,71,60,0.25);
  padding-bottom: 1px;
  transition: border-color 0.18s, color 0.18s;
}

.login-link:hover {
  color: #2D372F;
  border-bottom-color: #2D372F;
}

/* ═══════════════════════════════════
   RESPONSIVE
═══════════════════════════════════ */

@media (max-width: 900px) {
  .auth-visual     { display: none; }
  .auth-form-side  { padding: 40px 24px; }
  .form-row        { grid-template-columns: 1fr; }
}

@media (max-width: 480px) {
  .auth-form-side  { padding: 28px 16px; align-items: flex-start; }
  .form-header h2  { font-size: 26px; }
  .verify-card     { padding: 28px 20px; }
}

/* ── MOBILE RESPONSIVE ── */
@media (max-width: 768px) {
  .auth-container {
    flex-direction: column;
  }

  .auth-visual {
    display: none;
  }

  .auth-form-side {
    padding: 36px 20px 56px;
    align-items: flex-start;
  }

  .form-container {
    max-width: 100%;
  }

  .form-header h2 {
    font-size: 26px;
  }

  .form-row {
    grid-template-columns: 1fr;
    gap: 0;
  }

  .field-input {
    height: 46px;
    font-size: 13px;
  }

  .code-btn {
    width: 100%;
    height: 46px;
  }

  .phone-dropdown {
    width: 100%;
    left: 0;
  }

  .btn-register {
    height: 48px;
    font-size: 14px;
  }

  /* Tarjeta de verificación */
  .verify-card {
    padding: 28px 18px;
    border-radius: 18px;
  }

  .verify-title {
    font-size: 22px;
  }

  .verify-subtitle {
    font-size: 13px;
  }

  .verify-code-input {
    font-size: 20px;
    letter-spacing: 6px;
  }

  .verify-resend-row {
    flex-direction: column;
    text-align: center;
    gap: 6px;
  }

  /* Tooltip: mostrar como modal en mobile */
  .tc-tooltip-card {
    position: fixed;
    bottom: 16px;
    left: 16px;
    right: 16px;
    top: auto;
    width: auto;
    transform: none;
  }
}

@media (max-width: 380px) {
  .auth-form-side {
    padding: 24px 14px 48px;
  }

  .form-header h2 {
    font-size: 22px;
  }

  .verify-card {
    padding: 22px 14px;
  }
}

</style>