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

import { register } from '../services/authServices'

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
   MOSTRAR / OCULTAR CONTRASEÑA
───────────────────────────── */

const showPassword        = ref(false)
const showConfirmPassword = ref(false)

function toggleShowPassword() {
  showPassword.value = !showPassword.value
}

function toggleShowConfirmPassword() {
  showConfirmPassword.value = !showConfirmPassword.value
}

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
const verificandoCodigo     = ref(false)
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
  verificandoCodigo.value = true

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
  } catch (err) {
    console.error(err)
    success.value = false
    errorVerificacion.value = 'No se pudo crear el usuario'
    verificandoCodigo.value = false
    return
  }

  verificandoCodigo.value = false

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

  // Autocompletar el código telefónico según el país elegido.
  // El usuario aún puede cambiarlo manualmente desde su propio selector.
  const matchingCode = phoneCodesList.find(item => item.name === country)
  if (matchingCode) {
    selectedCountryObject.value = matchingCode
  }
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
            Únete a nuestra<br />
            comunidad
          </h1>

          <div class="visual-divider"></div>

          <p class="visual-description">
            Crea tu cuenta y forma parte de
            Anhelo Pets. Juntos hacemos posible
            encontrarles un hogar a quienes más
            lo necesitan.
          </p>

        </div>

        <div class="visual-stats">
          <div class="visual-stat">
            <span class="visual-stat-value">+200</span>
            <span class="visual-stat-label">adopciones</span>
          </div>
          <div class="visual-stat">
            <span class="visual-stat-value">+80</span>
            <span class="visual-stat-label">voluntarios</span>
          </div>
          <div class="visual-stat">
            <span class="visual-stat-value">+5</span>
            <span class="visual-stat-label">años activos</span>
          </div>
        </div>

      </div>

    </div>

    <!-- FORM -->

    <div class="login-form-side">

      <div class="form-container">

        <!-- ════════════════════════════════
             VISTA 1: FORMULARIO DE REGISTRO
        ════════════════════════════════ -->
        <template v-if="!esperandoVerificacion">

          <div class="form-header">
            <h2>Crear cuenta</h2>
            <p>Completa tu información para comenzar</p>
          </div>

          <!-- ERROR -->
          <div v-if="error" class="error-box">
            {{ error }}
          </div>

          <form @submit.prevent="crearCuenta">

            <!-- NOMBRE -->
            <div class="input-group">
              <label>Nombre completo <span class="req-mark">*</span></label>
              <input
                v-model="nombre"
                class="custom-input"
                placeholder="María González"
              />
            </div>

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

            <!-- PAÍS -->
            <div class="input-group">
              <label>País <span class="req-mark">*</span></label>
              <div class="autocomplete-wrapper">
                <input
                  v-model="countrySearch"
                  class="custom-input"
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
            <div class="fields-row">

              <div class="input-group" ref="dropdownContainer">
                <label>Código país <span class="req-mark">*</span></label>
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

              <div class="input-group">
                <label>Teléfono <span class="req-mark">*</span></label>
                <input
                  v-model="telefono"
                  @input="filterPhoneNumber"
                  class="custom-input"
                  placeholder="88888888"
                />
              </div>

            </div>

            <!-- CÉDULA -->
            <div class="input-group">
              <label>Cédula <span class="req-mark">*</span></label>
              <input
                v-model="cedula"
                class="custom-input"
                placeholder="1-2345-6789"
              />
            </div>

            <!-- CONTRASEÑAS -->
            <div class="fields-row">
              <div class="input-group">
                <label>Contraseña <span class="req-mark">*</span></label>
                <div class="password-field-wrap">
                  <input
                    v-model="password"
                    :type="showPassword ? 'text' : 'password'"
                    class="custom-input"
                    placeholder="Mínimo 8 caracteres"
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
              <div class="input-group">
                <label>Confirmar contraseña <span class="req-mark">*</span></label>
                <div class="password-field-wrap">
                  <input
                    v-model="confirmarPassword"
                    :type="showConfirmPassword ? 'text' : 'password'"
                    class="custom-input"
                    placeholder="Repetir contraseña"
                  />
                  <button
                    type="button"
                    class="password-toggle-btn"
                    :aria-label="showConfirmPassword ? 'Ocultar contraseña' : 'Mostrar contraseña'"
                    @click="toggleShowConfirmPassword"
                  >
                    <svg v-if="!showConfirmPassword" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
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
            </div>

            <!-- ── TÉRMINOS Y CONDICIONES ── -->
            <div class="tc-check-area">
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
                  <span class="req-mark">*</span>
                </span>
              </label>
            </div>

            <!-- BOTON -->
            <button
              type="submit"
              class="btn-login"
              :disabled="!termsAccepted || enviandoCodigo"
            >
              <span v-if="enviandoCodigo">Enviando código...</span>
              <span v-else>Crear mi cuenta</span>
            </button>

          </form>

          <!-- FOOTER -->
          <div class="form-footer">
            <p>
              ¿Ya tienes una cuenta?
              <RouterLink to="/login" class="register-link">Inicia sesión</RouterLink>
            </p>
          </div>

        </template>

        <!-- ════════════════════════════════
             VISTA 2: VERIFICACIÓN DE CORREO
        ════════════════════════════════ -->
        <template v-else>

          <div class="verify-inline-card">

            <!-- SUCCESS -->
            <div v-if="success" class="r-alert-success">
              Cuenta creada correctamente
            </div>

            <!-- Ícono -->
            <div class="r-icon-wrap">
              <svg viewBox="0 0 48 48" fill="none" xmlns="http://www.w3.org/2000/svg">
                <rect width="48" height="48" rx="14" fill="rgba(58,71,60,0.08)"/>
                <path d="M10 16a2 2 0 0 1 2-2h24a2 2 0 0 1 2 2v16a2 2 0 0 1-2 2H12a2 2 0 0 1-2-2V16Z" stroke="#2D372F" stroke-width="1.8"/>
                <path d="M10 17l14 10 14-10" stroke="#2D372F" stroke-width="1.8" stroke-linecap="round"/>
              </svg>
            </div>

            <h3 class="r-modal-title">Verifica tu correo</h3>

            <p class="r-subtitle">
              Enviamos un código de 6 dígitos a
              <strong class="r-verify-email">{{ correo }}</strong>
            </p>
            <p class="r-hint">Revisa tu bandeja de entrada o la carpeta de spam.</p>

            <div class="r-divider-line"></div>

            <!-- Error verificación -->
            <div v-if="errorVerificacion" class="r-alert-error">
              {{ errorVerificacion }}
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
                @keyup.enter="verificarCodigo"
              />
            </div>

            <!-- Botón verificar -->
            <button
              type="button"
              class="r-btn"
              :disabled="verificandoCodigo"
              @click="verificarCodigo"
            >
              <span v-if="verificandoCodigo" class="btn-spinner"></span>
              {{ verificandoCodigo ? 'Creando cuenta...' : 'Verificar y crear cuenta' }}
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
}

.visual-inner {
  display: flex;
  flex-direction: column;
  height: 100%;
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
  position: relative;
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

.fields-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 14px;
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
   AUTOCOMPLETE PAÍS
═══════════════════════════════════ */

.autocomplete-wrapper { position: relative; }

.autocomplete-dropdown {
  position: absolute;
  top: calc(100% + 6px);
  left: 0;
  width: 100%;
  background: #fff;
  border-radius: 14px;
  border: 1.5px solid #DCE4DD;
  max-height: 220px;
  overflow-y: auto;
  z-index: 300;
  box-shadow: 0 8px 28px rgba(45,55,47,0.12);
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
  height: 52px;
  border-radius: 14px;
  border: 1.5px solid #DCE4DD;
  background: #F8FAF8;
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
  color: #1A201B;
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
  background: #fff;
  border-radius: 14px;
  border: 1.5px solid #DCE4DD;
  overflow: hidden;
  z-index: 300;
  box-shadow: 0 8px 28px rgba(45,55,47,0.12);
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
  background: #F8FAF8;
  color: #2D372F;
  box-sizing: border-box;
}

.phone-search-input:focus { border-color: #2D372F; }

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
  color: #1A201B;
  font-weight: 700;
}

/* ═══════════════════════════════════
   TÉRMINOS Y CONDICIONES
═══════════════════════════════════ */

.tc-check-area {
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
  accent-color: #2D372F;
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

/* ─────────────────────────────
   TOOLTIP TÉRMINOS Y CONDICIONES
───────────────────────────── */

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
  color: #5F6A61;
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
   TARJETA DE VERIFICACIÓN
   (mismo estilo que el modal "verificar código" del login)
═══════════════════════════════════ */

.verify-inline-card {
  position: relative;
  width: 100%;
  background: #FFFFFF;
  border-radius: 24px;
  padding: 40px 32px 32px;
  box-shadow: 0 24px 64px rgba(20,24,20,0.12);
  text-align: center;
}

.r-alert-success {
  background: rgba(58,71,60,0.08);
  color: #2D372F;
  border: 1px solid rgba(58,71,60,0.16);
  padding: 13px 16px;
  border-radius: 12px;
  font-size: 13px;
  font-weight: 600;
  margin-bottom: 18px;
  text-align: left;
}

.r-icon-wrap {
  width: 60px;
  height: 60px;
  margin: 0 auto 18px;
}

.r-icon-wrap svg {
  width: 100%;
  height: 100%;
}

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

.r-divider-line {
  width: 100%;
  height: 1px;
  background: #E5E9E5;
  margin: 22px 0;
}

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

/* ═══════════════════════════════════
   RESPONSIVE
═══════════════════════════════════ */

@media (max-width: 900px) {
  .login-visual    { display: none; }
  .login-form-side { padding: 40px 24px; }
  .fields-row      { grid-template-columns: 1fr; }
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

  .fields-row {
    grid-template-columns: 1fr;
  }

  .custom-input {
    height: 48px;
    font-size: 13px;
  }

  .code-btn {
    height: 48px;
  }

  .phone-dropdown {
    width: 100%;
    left: 0;
  }

  .btn-login {
    height: 50px;
    font-size: 14px;
  }

  .verify-inline-card {
    padding: 32px 20px 24px;
    border-radius: 22px;
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

@media (max-width: 480px) {
  .login-form-side { padding: 28px 16px; align-items: flex-start; }
  .form-header h2 { font-size: 25px; }
  .verify-inline-card { padding: 28px 16px 20px; }
}

@media (max-width: 380px) {
  .login-form-side {
    padding: 24px 14px 40px;
  }

  .form-header h2 {
    font-size: 22px;
  }
}

</style>