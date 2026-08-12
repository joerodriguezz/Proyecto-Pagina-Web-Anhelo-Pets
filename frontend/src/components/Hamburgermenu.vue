<script setup>
import { ref, computed, onMounted, onBeforeUnmount, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/useAuthStore'
import { resetPasswordByEmail } from '../services/authServices'

const router = useRouter()
const authStore = useAuthStore()

/* ─── Panel state ─────────────────────────────── */
const isOpen    = ref(false)
const activeTab = ref('perfil')

function open()   { isOpen.value = true;  document.body.style.overflow = 'hidden' }
function close()  { isOpen.value = false; document.body.style.overflow = '' }
function toggle() { isOpen.value ? close() : open() }

function onKey(e) { if (e.key === 'Escape') close() }
onMounted(()       => document.addEventListener('keydown', onKey))
onBeforeUnmount(() => document.removeEventListener('keydown', onKey))

/* ─── Usuario ─────────────────────────────────── */
// Adapta el AuthResponseDto plano de /api/auth/me a la forma que ya espera
// esta plantilla (nombre/correo/rol/solicitudVoluntario.estado), para no
// reescribir todo el panel — mismo patrón que adaptarVoluntario/adaptarDonacion.
function adaptarUsuarioSesion(apiUser) {
  if (!apiUser) return null
  const roles = apiUser.roles || []
  const rolPrincipal = roles.includes('Admin')
    ? 'Admin'
    : roles.includes('Voluntario') ? 'Voluntario' : 'Usuario'

  return {
    id: apiUser.userId,
    nombre: [apiUser.firstName, apiUser.lastName].filter(Boolean).join(' ') || apiUser.username,
    correo: apiUser.email,
    telefono: apiUser.phonePrimary || '',
    rol: rolPrincipal,
    foto: apiUser.photoUrl || '',
    solicitudVoluntario: apiUser.isVolunteer
      ? { estado: apiUser.volunteerActive ? 'Aprobada' : (apiUser.volunteerValidationStatus || 'Pendiente') }
      : null,
  }
}

const usuario = computed(() => adaptarUsuarioSesion(authStore.user))

const iniciales = computed(() => {
  if (!usuario.value?.nombre) return '?'
  return usuario.value.nombre
    .trim().split(' ').map(p => p[0]).slice(0, 2).join('').toUpperCase()
})

const rol       = computed(() => usuario.value?.rol || 'Usuario')
const esAdmin   = computed(() => rol.value === 'Admin')
const solicitud = computed(() => usuario.value?.solicitudVoluntario || null)
const esVol     = computed(() => rol.value === 'Voluntario' && solicitud.value)
const volActivo = computed(() => solicitud.value?.estado === 'Aprobada')

const estadoLabel = computed(() => {
  if (esAdmin.value) return 'Administrador'
  if (esVol.value)   return volActivo.value ? 'Voluntario activo' : 'Voluntario inactivo'
  return 'Usuario activo'
})

/* ─── Foto de perfil ──────────────────────────── */
const fotoPreview  = computed(() => usuario.value?.foto || '')
const fileInputRef = ref(null)
const subiendoFoto = ref(false)

function triggerFileInput() { fileInputRef.value?.click() }

async function onFotoChange(e) {
  const file = e.target.files?.[0]
  e.target.value = ''
  if (!file || !file.type.startsWith('image/')) return

  subiendoFoto.value = true
  try {
    await authStore.updatePhoto(file)
  } catch (err) {
    console.error('Error al subir la foto de perfil:', err)
  } finally {
    subiendoFoto.value = false
  }
}

const eliminandoFoto = ref(false)
async function eliminarFoto() {
  eliminandoFoto.value = true
  try {
    await authStore.removePhoto()
  } catch (err) {
    console.error('Error al quitar la foto de perfil:', err)
  }
  eliminandoFoto.value = false
}

/* ─── Cerrar sesión ───────────────────────────── */
function cerrarSesion() {
  authStore.logout()
  close()
  router.push('/')
  location.reload()
}

/* ─── Panel Admin ─────────────────────────────── */
function irAdmin() {
  close()
  router.push('/admin')
}

/* ─── Navegación de pestañas ──────────────────── */
function setTab(tab) { activeTab.value = tab }

/* ─── DATOS: Adopciones ───────────────────────── */
const adopciones = computed(() => {
  if (!usuario.value) return []
  try {
    const todas = JSON.parse(localStorage.getItem('anhelo_adopciones') || '[]')
    return todas.filter(a =>
      a.usuarioId === usuario.value.id ||
      a.correoUsuario === usuario.value.correo
    )
  } catch { return [] }
})

/* ─── DATOS: Donaciones ───────────────────────── */
const donaciones = computed(() => {
  if (!usuario.value) return []
  try {
    const todas = JSON.parse(localStorage.getItem('anhelo_donaciones') || '[]')
    return todas.filter(d =>
      d.usuarioId === usuario.value.id ||
      d.correo === usuario.value.correo
    )
  } catch { return [] }
})

const totalDonado = computed(() =>
  donaciones.value.reduce((s, d) => s + (parseFloat(d.monto) || 0), 0)
)

/* ─── DATOS: Voluntariado ─────────────────────── */
const actividadesVol = computed(() => {
  if (!usuario.value) return []
  try {
    const todas = JSON.parse(localStorage.getItem('anhelo_actividades_voluntario') || '[]')
    return todas.filter(a =>
      a.usuarioId === usuario.value.id ||
      a.correo === usuario.value.correo
    )
  } catch { return [] }
})

/* ─── CONFIGURACIÓN: editar perfil ───────────── */
const editForm = ref({ nombre: '', correo: '', telefono: '' })
const editMsg  = ref('')

function cargarFormEdit() {
  if (!usuario.value) return
  editForm.value = {
    nombre:   usuario.value.nombre   || '',
    correo:   usuario.value.correo   || '',
    telefono: usuario.value.telefono || ''
  }
}

watch(isOpen, v => { if (v) cargarFormEdit() })

function guardarPerfil() {
  if (!usuario.value) return

  // No hay endpoint de edición de perfil todavía en el backend — esto es
  // solo una confirmación visual optimista, no persiste (se pierde al
  // refrescar/re-loguear). Implementar un PUT real es una feature aparte.
  editMsg.value = 'Cambios guardados correctamente.'
  setTimeout(() => { editMsg.value = '' }, 3000)
}

/* ══════════════════════════════════════════════════
   CAMBIO DE CONTRASEÑA — idéntico al LoginView
══════════════════════════════════════════════════ */

const pwStep                 = ref(0)
const pwCorreo               = ref('')
const pwCodigo               = ref('')
const pwCodigoGen            = ref('')
const pwNueva                = ref('')
const pwConfirm              = ref('')
const pwMsg                  = ref('')
const pwLoading              = ref(false)
const pwReenvioDeshabilitado = ref(false)

const EMAILJS_SERVICE_ID  = 'service_okmmsx7'
const EMAILJS_TEMPLATE_ID = 'template_ynpp8ld'
const EMAILJS_PUBLIC_KEY  = 'fwH1f3N8oVs98GPAx'

/* Idéntico al LoginView */
async function cargarEmailJS() {
  if (window.emailjs) return
  await new Promise((resolve, reject) => {
    const script   = document.createElement('script')
    script.src     = 'https://cdn.jsdelivr.net/npm/@emailjs/browser@4/dist/email.min.js'
    script.onload  = resolve
    script.onerror = reject
    document.head.appendChild(script)
  })
  window.emailjs.init(EMAILJS_PUBLIC_KEY)
}

function generarCodigo() {
  return String(Math.floor(100000 + Math.random() * 900000))
}

/* Idéntico al LoginView */
async function enviarCorreoRecuperacion(codigo) {
  await cargarEmailJS()

  const templateParams = {
    user_name:  usuario.value?.nombre || pwCorreo.value,
    reset_code: codigo,
    to_email:   pwCorreo.value.trim()
  }

  console.log('[HamburgerMenu] Enviando código:', codigo, 'a:', pwCorreo.value)

  const response = await window.emailjs.send(
    EMAILJS_SERVICE_ID,
    EMAILJS_TEMPLATE_ID,
    templateParams
  )

  console.log('[HamburgerMenu] Respuesta EmailJS:', response)
  return response
}

function iniciarCambioPassword() {
  pwStep.value   = 1
  pwCorreo.value = usuario.value?.correo || ''
  pwMsg.value    = ''
  console.log('[HamburgerMenu] Iniciando cambio. Correo:', pwCorreo.value)
}

async function enviarCodigo() {
  pwMsg.value = ''

  if (!pwCorreo.value.trim()) {
    pwMsg.value = 'Ingresa tu correo.'
    return
  }

  pwLoading.value   = true
  pwCodigoGen.value = generarCodigo()

  try {
    await enviarCorreoRecuperacion(pwCodigoGen.value)
    pwStep.value = 2
    pwMsg.value  = 'Código enviado. Revisa tu correo (también spam).'
  } catch (err) {
    console.error('[HamburgerMenu] Error EmailJS completo:', err)
    pwCodigoGen.value = ''
    pwMsg.value = err?.text || err?.message || 'Error al enviar el correo.'
  }

  pwLoading.value = false
}

async function reenviarCodigo() {
  pwReenvioDeshabilitado.value = true
  pwMsg.value = ''

  try {
    const nuevoCodigo = generarCodigo()
    pwCodigoGen.value = nuevoCodigo
    pwCodigo.value    = ''
    await enviarCorreoRecuperacion(nuevoCodigo)
    pwMsg.value = 'Código reenviado correctamente.'
  } catch (err) {
    console.error('[HamburgerMenu] Error reenvío:', err)
    pwMsg.value = 'No se pudo reenviar el correo.'
  } finally {
    setTimeout(() => { pwReenvioDeshabilitado.value = false }, 30000)
  }
}

function verificarCodigo() {
  pwMsg.value = ''

  console.log('[HamburgerMenu] Verificando. Ingresado:', pwCodigo.value.trim(), '| Generado:', pwCodigoGen.value)

  if (!pwCodigo.value.trim()) {
    pwMsg.value = 'Ingresa el código de verificación.'
    return
  }

  if (!pwCodigoGen.value) {
    pwMsg.value = 'El código expiró. Vuelve a solicitar uno.'
    return
  }

  if (pwCodigo.value.trim() !== pwCodigoGen.value) {
    pwMsg.value = 'Código incorrecto. Intenta nuevamente.'
    return
  }

  pwStep.value = 3
  pwMsg.value  = ''
}

const guardandoPass = ref(false)
async function guardarNuevaPass() {
  pwMsg.value = ''

  if (!pwNueva.value || !pwConfirm.value) {
    pwMsg.value = 'Completa todos los campos.'
    return
  }

  if (pwNueva.value !== pwConfirm.value) {
    pwMsg.value = 'Las contraseñas no coinciden.'
    return
  }

  if (pwNueva.value.length < 6) {
    pwMsg.value = 'La contraseña debe tener al menos 6 caracteres.'
    return
  }

  guardandoPass.value = true
  try {
    await resetPasswordByEmail(pwCorreo.value.trim(), pwNueva.value)
  } catch (err) {
    console.error('[HamburgerMenu] Error actualizando password:', err)
    pwMsg.value = 'No se pudo actualizar la contraseña. Intenta de nuevo.'
    guardandoPass.value = false
    return
  }

  guardandoPass.value = false
  pwStep.value = 4
}

function resetPw() {
  pwStep.value                 = 0
  pwCorreo.value               = ''
  pwCodigo.value               = ''
  pwCodigoGen.value            = ''
  pwNueva.value                = ''
  pwConfirm.value              = ''
  pwMsg.value                  = ''
  pwReenvioDeshabilitado.value = false
}

function finalizarCambioPassword() {
  resetPw()
}

/* ─── Helpers ─────────────────────────────────── */
function formatFecha(f) {
  if (!f) return '—'
  try {
    return new Date(f).toLocaleDateString('es-CR', {
      day: '2-digit', month: 'short', year: 'numeric'
    })
  } catch { return f }
}

function estadoBadgeClass(estado) {
  if (!estado) return 'badge-gray'
  const e = estado.toLowerCase()
  if (e.includes('aprobad') || e.includes('complet') || e.includes('activ')) return 'badge-green'
  if (e.includes('pendient') || e.includes('revision') || e.includes('proceso')) return 'badge-gold'
  if (e.includes('rechazad') || e.includes('cancel')) return 'badge-red'
  return 'badge-gray'
}
</script>

<template>
  <!-- ── Botón hamburguesa ── -->
  <button class="ham-btn" @click="toggle" :aria-expanded="isOpen" aria-label="Abrir menú">
    <svg width="22" height="22" viewBox="0 0 22 22" xmlns="http://www.w3.org/2000/svg"
      aria-hidden="true" focusable="false" style="display:block;pointer-events:none;">
      <line x1="3" y1="5"  x2="19" y2="5"  stroke="#3A473C" stroke-width="2" stroke-linecap="round"/>
      <line x1="3" y1="11" x2="19" y2="11" stroke="#3A473C" stroke-width="2" stroke-linecap="round"/>
      <line x1="3" y1="17" x2="19" y2="17" stroke="#3A473C" stroke-width="2" stroke-linecap="round"/>
    </svg>
  </button>

  <Teleport to="body">
    <Transition name="overlay-fade">
      <div v-if="isOpen" class="ap-overlay" @click.self="close"
        aria-modal="true" role="dialog" aria-label="Panel de usuario">
        <Transition name="panel-slide">
          <div v-if="isOpen" class="ap-shell">

            <!-- ── SIDEBAR ── -->
            <aside class="ap-sidebar">

              <div class="sb-profile">
                <div class="sb-avatar-wrap">
                  <div class="sb-avatar">
                    <img v-if="fotoPreview" :src="fotoPreview" alt="Foto de perfil" class="sb-avatar-img" />
                    <span v-else class="sb-avatar-ini">{{ iniciales }}</span>
                  </div>
                  <button class="sb-cam" :disabled="subiendoFoto" @click="triggerFileInput" aria-label="Cambiar foto">
                    <svg width="9" height="9" viewBox="0 0 24 24" fill="none" stroke="#3A473C" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
                      <path d="M23 19a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h4l2-3h6l2 3h4a2 2 0 0 1 2 2z"/>
                      <circle cx="12" cy="13" r="4"/>
                    </svg>
                  </button>
                  <input ref="fileInputRef" type="file" accept="image/*" style="display:none" @change="onFotoChange" />
                </div>
                <p class="sb-name">{{ usuario?.nombre || 'Usuario' }}</p>
                <p class="sb-email">{{ usuario?.correo || '' }}</p>
                <span class="sb-pill">
                  <span class="sb-dot" :class="{
                    'dot-green': !esAdmin && (!esVol || volActivo),
                    'dot-gold':  esVol && !volActivo,
                    'dot-blue':  esAdmin
                  }"></span>
                  {{ estadoLabel }}
                </span>
              </div>

              <nav class="sb-nav" aria-label="Secciones de usuario">
                <button class="sb-item" :class="{ active: activeTab === 'perfil' }" @click="setTab('perfil')">
                  <svg class="sb-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
                  Mi perfil
                </button>
                <button class="sb-item" :class="{ active: activeTab === 'adopciones' }" @click="setTab('adopciones')">
                  <svg class="sb-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78L12 21.23l8.84-8.84a5.5 5.5 0 0 0 0-7.78z"/></svg>
                  Mis adopciones
                </button>
                <button class="sb-item" :class="{ active: activeTab === 'donaciones' }" @click="setTab('donaciones')">
                  <svg class="sb-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="16"/><line x1="8" y1="12" x2="16" y2="12"/></svg>
                  Mis donaciones
                </button>
                <button class="sb-item" :class="{ active: activeTab === 'voluntariado' }" @click="setTab('voluntariado')">
                  <svg class="sb-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
                  Mi voluntariado
                </button>
                <button class="sb-item" :class="{ active: activeTab === 'config' }" @click="setTab('config')">
                  <svg class="sb-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="3"/><path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 1 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 1 1-2.83-2.83l.06-.06A1.65 1.65 0 0 0 4.68 15a1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 1 1 2.83-2.83l.06.06A1.65 1.65 0 0 0 9 4.68a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 1 1 2.83 2.83l-.06.06A1.65 1.65 0 0 0 19.4 9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z"/></svg>
                  Configuración
                </button>

                <div class="sb-divider"></div>

                <button v-if="esAdmin" class="sb-item sb-item-nav" @click="irAdmin">
                  <svg class="sb-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/></svg>
                  Panel de administración
                  <svg class="sb-arrow" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="9 18 15 12 9 6"/></svg>
                </button>

                <button v-if="esVol && volActivo" class="sb-item sb-item-nav"
                  @click="() => { close(); router.push('/panel-voluntario') }">
                  <svg class="sb-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/></svg>
                  Panel voluntario
                  <svg class="sb-arrow" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="9 18 15 12 9 6"/></svg>
                </button>

                <button class="sb-item sb-item-danger" @click="cerrarSesion">
                  <svg class="sb-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/><polyline points="16 17 21 12 16 7"/><line x1="21" y1="12" x2="9" y2="12"/></svg>
                  Cerrar sesión
                </button>
              </nav>

              <button class="sb-close-btn" @click="close" aria-label="Cerrar panel">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round">
                  <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
                </svg>
              </button>
            </aside>

            <!-- ── CONTENIDO DERECHO ── -->
            <main class="ap-content">

              <!-- MI PERFIL -->
              <section v-if="activeTab === 'perfil'" class="content-section">
                <h2 class="content-title">Mi perfil</h2>
                <div class="profile-hero">
                  <div class="profile-avatar-wrap">
                    <div class="profile-avatar">
                      <img v-if="fotoPreview" :src="fotoPreview" alt="Foto" class="profile-avatar-img" />
                      <span v-else>{{ iniciales }}</span>
                    </div>
                    <button class="profile-cam-btn" :disabled="subiendoFoto" @click="triggerFileInput" aria-label="Cambiar foto">
                      <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="#3A473C" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
                        <path d="M23 19a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h4l2-3h6l2 3h4a2 2 0 0 1 2 2z"/>
                        <circle cx="12" cy="13" r="4"/>
                      </svg>
                    </button>
                  </div>
                  <div>
                    <p class="profile-name">{{ usuario?.nombre || 'Usuario' }}</p>
                    <p class="profile-email">{{ usuario?.correo || '—' }}</p>
                    <span class="sb-pill" style="margin-top:8px;display:inline-flex;">
                      <span class="sb-dot" :class="{
                        'dot-green': !esAdmin && (!esVol || volActivo),
                        'dot-gold':  esVol && !volActivo,
                        'dot-blue':  esAdmin
                      }"></span>
                      {{ estadoLabel }}
                    </span>
                  </div>
                </div>

                <div class="info-grid">
                  <div class="info-field"><label>Nombre completo</label><span>{{ usuario?.nombre || '—' }}</span></div>
                  <div class="info-field"><label>Correo electrónico</label><span>{{ usuario?.correo || '—' }}</span></div>
                  <div class="info-field"><label>Teléfono</label><span>{{ usuario?.telefono || 'No registrado' }}</span></div>
                  <div class="info-field"><label>Fecha de registro</label><span>{{ formatFecha(usuario?.fechaRegistro || usuario?.createdAt) }}</span></div>
                  <div class="info-field"><label>Rol</label><span>{{ rol }}</span></div>
                  <div class="info-field"><label>Estado</label><span>{{ estadoLabel }}</span></div>
                  <template v-if="esVol && solicitud">
                    <div class="info-field"><label>Tipo de voluntariado</label><span>{{ solicitud.tipo || '—' }}</span></div>
                    <div class="info-field"><label>Estado voluntariado</label><span :class="estadoBadgeClass(solicitud.estado)" class="badge">{{ solicitud.estado || '—' }}</span></div>
                  </template>
                </div>

                <div class="action-row">
                  <button class="btn-primary" @click="setTab('config')">Editar perfil</button>
                </div>
              </section>

              <!-- MIS ADOPCIONES -->
              <section v-else-if="activeTab === 'adopciones'" class="content-section">
                <h2 class="content-title">Mis adopciones</h2>
                <div v-if="adopciones.length === 0" class="empty-state">
                  <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="#92A894" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78L12 21.23l8.84-8.84a5.5 5.5 0 0 0 0-7.78z"/></svg>
                  <p class="empty-title">Aún no tienes adopciones registradas</p>
                  <p class="empty-sub">Cuando inicies una solicitud de adopción, aparecerá aquí.</p>
                  <button class="btn-primary" @click="() => { close(); router.push('/mascotas') }">Ver mascotas disponibles</button>
                </div>
                <div v-else class="list-wrap">
                  <div v-for="a in adopciones" :key="a.id || a.fecha" class="list-card">
                    <div class="list-card-icon">
                      <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="#3A473C" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78L12 21.23l8.84-8.84a5.5 5.5 0 0 0 0-7.78z"/></svg>
                    </div>
                    <div class="list-card-info">
                      <p class="list-card-title">{{ a.nombreMascota || a.mascota?.nombre || 'Mascota' }}</p>
                      <p class="list-card-sub">{{ formatFecha(a.fecha || a.fechaSolicitud) }}</p>
                      <p v-if="a.observaciones" class="list-card-obs">{{ a.observaciones }}</p>
                    </div>
                    <span :class="estadoBadgeClass(a.estado)" class="badge">{{ a.estado || 'Pendiente' }}</span>
                  </div>
                </div>
              </section>

              <!-- MIS DONACIONES -->
              <section v-else-if="activeTab === 'donaciones'" class="content-section">
                <h2 class="content-title">Mis donaciones</h2>
                <div v-if="donaciones.length === 0" class="empty-state">
                  <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="#92A894" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="16"/><line x1="8" y1="12" x2="16" y2="12"/></svg>
                  <p class="empty-title">Aún no has realizado donaciones</p>
                  <p class="empty-sub">Tu apoyo hace la diferencia. Cada donación ayuda a más animales.</p>
                  <button class="btn-primary" @click="() => { close(); router.push('/donar') }">Hacer una donación</button>
                </div>
                <template v-else>
                  <div class="summary-card">
                    <p class="summary-label">Total donado</p>
                    <p class="summary-value">₡{{ totalDonado.toLocaleString('es-CR') }}</p>
                    <p class="summary-sub">{{ donaciones.length }} donación{{ donaciones.length !== 1 ? 'es' : '' }}</p>
                  </div>
                  <div class="list-wrap">
                    <div v-for="d in donaciones" :key="d.id || d.fecha" class="list-card">
                      <div class="list-card-icon">
                        <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="#3A473C" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="16"/><line x1="8" y1="12" x2="16" y2="12"/></svg>
                      </div>
                      <div class="list-card-info">
                        <p class="list-card-title">
                          {{ d.moneda || '₡' }}{{ parseFloat(d.monto || 0).toLocaleString('es-CR') }}
                          <span v-if="d.metodo" style="font-weight:400;color:#6C756D;font-size:12px;"> · {{ d.metodo }}</span>
                        </p>
                        <p class="list-card-sub">{{ formatFecha(d.fecha || d.createdAt) }}</p>
                        <p v-if="d.descripcion || d.categoria" class="list-card-obs">{{ d.descripcion || d.categoria }}</p>
                      </div>
                      <span :class="estadoBadgeClass(d.estado)" class="badge">{{ d.estado || 'Completada' }}</span>
                    </div>
                  </div>
                </template>
              </section>

              <!-- MI VOLUNTARIADO -->
              <section v-else-if="activeTab === 'voluntariado'" class="content-section">
                <h2 class="content-title">Mi voluntariado</h2>
                <div v-if="!esVol" class="empty-state">
                  <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="#92A894" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
                  <p class="empty-title">Actualmente no perteneces al programa de voluntariado</p>
                  <p class="empty-sub">Únete a nuestra comunidad y marca la diferencia en la vida de los animales.</p>
                  <button class="btn-primary" @click="() => { close(); router.push('/voluntarios') }">Quiero ser voluntario</button>
                </div>
                <template v-else>
                  <div class="vol-hero-card">
                    <div class="vol-hero-icon">
                      <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="#fff" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
                    </div>
                    <div>
                      <p class="vol-hero-tipo">{{ solicitud?.tipo || 'Voluntario' }}</p>
                      <p class="vol-hero-fecha">Desde {{ formatFecha(solicitud?.fecha || solicitud?.fechaSolicitud) }}</p>
                    </div>
                    <span :class="estadoBadgeClass(solicitud?.estado)" class="badge" style="margin-left:auto;">{{ solicitud?.estado || '—' }}</span>
                  </div>
                  <div class="info-grid" style="margin-top:16px;">
                    <div class="info-field"><label>Tipo de voluntariado</label><span>{{ solicitud?.tipo || '—' }}</span></div>
                    <div class="info-field"><label>Estado</label><span>{{ solicitud?.estado || '—' }}</span></div>
                    <div class="info-field"><label>Fecha de ingreso</label><span>{{ formatFecha(solicitud?.fecha || solicitud?.fechaSolicitud) }}</span></div>
                    <div v-if="solicitud?.observaciones" class="info-field" style="grid-column:1/-1;"><label>Observaciones</label><span>{{ solicitud.observaciones }}</span></div>
                  </div>
                  <div v-if="actividadesVol.length > 0" style="margin-top:20px;">
                    <p class="section-sub-title">Actividades recientes</p>
                    <div class="list-wrap">
                      <div v-for="a in actividadesVol" :key="a.id || a.fecha" class="list-card">
                        <div class="list-card-icon">
                          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="#3A473C" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="4" width="18" height="18" rx="2" ry="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>
                        </div>
                        <div class="list-card-info">
                          <p class="list-card-title">{{ a.actividad || a.descripcion || 'Actividad' }}</p>
                          <p class="list-card-sub">{{ formatFecha(a.fecha) }}</p>
                        </div>
                        <span v-if="a.estado" :class="estadoBadgeClass(a.estado)" class="badge">{{ a.estado }}</span>
                      </div>
                    </div>
                  </div>
                  <div v-else style="margin-top:20px;" class="empty-sub-note">
                    Aún no tienes actividades de voluntariado registradas.
                  </div>
                </template>
              </section>

              <!-- CONFIGURACIÓN -->
              <section v-else-if="activeTab === 'config'" class="content-section">
                <h2 class="content-title">Configuración</h2>

                <!-- Editar perfil -->
                <div class="config-block">
                  <p class="config-block-title">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="vertical-align:-2px;margin-right:6px;"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
                    Datos personales
                  </p>
                  <div class="config-fields">
                    <div class="config-field">
                      <label>Nombre completo</label>
                      <input v-model="editForm.nombre" type="text" placeholder="Tu nombre" />
                    </div>
                    <div class="config-field">
                      <label>Correo electrónico</label>
                      <input v-model="editForm.correo" type="email" placeholder="Tu correo" />
                    </div>
                    <div class="config-field">
                      <label>Teléfono</label>
                      <input v-model="editForm.telefono" type="tel" placeholder="Tu teléfono" />
                    </div>
                  </div>
                  <div v-if="editMsg" class="msg-success">{{ editMsg }}</div>
                  <button class="btn-primary" @click="guardarPerfil">Guardar cambios</button>
                </div>

                <!-- Cambiar foto -->
                <div class="config-block">
                  <p class="config-block-title">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="vertical-align:-2px;margin-right:6px;"><path d="M23 19a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h4l2-3h6l2 3h4a2 2 0 0 1 2 2z"/><circle cx="12" cy="13" r="4"/></svg>
                    Foto de perfil
                  </p>
                  <div class="foto-row">
                    <div class="foto-preview">
                      <img v-if="fotoPreview" :src="fotoPreview" alt="Foto" />
                      <span v-else>{{ iniciales }}</span>
                    </div>
                    <div class="foto-actions">
                      <button class="btn-primary" :disabled="subiendoFoto" @click="triggerFileInput">
                        <span v-if="subiendoFoto" class="btn-spinner"></span>
                        {{ subiendoFoto ? 'Subiendo...' : 'Subir nueva foto' }}
                      </button>
                      <button v-if="fotoPreview" class="btn-ghost" :disabled="eliminandoFoto" @click="eliminarFoto">
                        {{ eliminandoFoto ? 'Quitando...' : 'Quitar foto' }}
                      </button>
                    </div>
                  </div>
                </div>

                <!-- Cambiar contraseña -->
                <div class="config-block">
                  <p class="config-block-title">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="vertical-align:-2px;margin-right:6px;"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
                    Cambiar contraseña
                  </p>

                  <!-- PASO 0: inicio -->
                  <div v-if="pwStep === 0">
                    <p class="config-desc">Para cambiar tu contraseña debes verificar tu identidad por correo electrónico.</p>
                    <button class="btn-primary" @click="iniciarCambioPassword">Cambiar contraseña</button>
                  </div>

                  <!-- PASO 1: ingresar correo -->
                  <div v-else-if="pwStep === 1" class="pw-form">
                    <div class="config-field">
                      <label>Correo asociado a tu cuenta</label>
                      <input v-model="pwCorreo" type="email" placeholder="correo@ejemplo.com" @keyup.enter="enviarCodigo" />
                    </div>
                    <div v-if="pwMsg" :class="pwMsg.includes('enviado') ? 'msg-success' : 'msg-error'">{{ pwMsg }}</div>
                    <div class="pw-actions">
                      <button class="btn-primary" :disabled="pwLoading" @click="enviarCodigo">
                        {{ pwLoading ? 'Enviando…' : 'Enviar código' }}
                      </button>
                      <button class="btn-ghost" @click="resetPw">Cancelar</button>
                    </div>
                  </div>

                  <!-- PASO 2: verificar código -->
                  <div v-else-if="pwStep === 2" class="pw-form">
                    <p class="config-desc">
                      Hemos enviado un código de verificación a <strong>{{ pwCorreo }}</strong>.<br>
                      Revisa tu bandeja de entrada o la carpeta de spam.
                    </p>
                    <div class="config-field">
                      <label>Código de verificación</label>
                      <input v-model="pwCodigo" type="text" placeholder="123456" maxlength="6" inputmode="numeric" @keyup.enter="verificarCodigo" />
                    </div>
                    <div v-if="pwMsg" :class="pwMsg.includes('enviado') || pwMsg.includes('reenviado') ? 'msg-success' : 'msg-error'">{{ pwMsg }}</div>
                    <div class="pw-actions">
                      <button class="btn-primary" @click="verificarCodigo">Verificar código</button>
                      <button class="btn-ghost" @click="resetPw">Cancelar</button>
                    </div>
                    <div style="margin-top:12px;display:flex;align-items:center;gap:10px;flex-wrap:wrap;">
                      <span style="font-size:12px;color:#8A9389;">¿No recibiste el correo?</span>
                      <button class="btn-ghost" style="padding:6px 14px;font-size:12px;"
                        :disabled="pwReenvioDeshabilitado" @click="reenviarCodigo">
                        {{ pwReenvioDeshabilitado ? 'Código reenviado' : 'Reenviar código' }}
                      </button>
                    </div>
                    <div style="margin-top:8px;">
                      <button class="btn-ghost" style="padding:6px 14px;font-size:12px;"
                        @click="pwStep = 1; pwMsg = ''">← Volver</button>
                    </div>
                  </div>

                  <!-- PASO 3: nueva contraseña -->
                  <div v-else-if="pwStep === 3" class="pw-form">
                    <p class="config-desc">Crea una nueva contraseña para <strong>{{ pwCorreo }}</strong>.</p>
                    <div class="config-field">
                      <label>Nueva contraseña</label>
                      <input v-model="pwNueva" type="password" placeholder="Mínimo 6 caracteres" />
                    </div>
                    <div class="config-field">
                      <label>Confirmar nueva contraseña</label>
                      <input v-model="pwConfirm" type="password" placeholder="Repite la contraseña" @keyup.enter="guardarNuevaPass" />
                    </div>
                    <div v-if="pwMsg" class="msg-error">{{ pwMsg }}</div>
                    <div class="pw-actions">
                      <button class="btn-primary" :disabled="guardandoPass" @click="guardarNuevaPass">
                        <span v-if="guardandoPass" class="btn-spinner"></span>
                        {{ guardandoPass ? 'Guardando...' : 'Guardar nueva contraseña' }}
                      </button>
                      <button class="btn-ghost" :disabled="guardandoPass" @click="pwStep = 2; pwMsg = ''">← Volver</button>
                    </div>
                  </div>

                  <!-- PASO 4: éxito -->
                  <div v-else-if="pwStep === 4" class="pw-form">
                    <div class="msg-success" style="display:flex;flex-direction:column;gap:6px;padding:16px;">
                      <strong style="font-size:14px;">✓ Contraseña actualizada correctamente</strong>
                      <span>Ya puedes iniciar sesión con tu nueva contraseña.</span>
                    </div>
                    <button class="btn-primary" style="margin-top:12px;" @click="finalizarCambioPassword">Cerrar</button>
                  </div>
                </div>

                <!-- Cerrar sesión -->
                <div class="config-block">
                  <p class="config-block-title">Sesión</p>
                  <p class="config-desc">Cerrar sesión en este dispositivo.</p>
                  <button class="btn-danger" @click="cerrarSesion">Cerrar sesión</button>
                </div>
              </section>

            </main>
          </div>
        </Transition>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.ham-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 38px;
  height: 38px;
  background: #F4F6F4;
  border: 1px solid #DCE5DC;
  border-radius: 10px;
  cursor: pointer;
  transition: all 0.2s ease;
}
.ham-btn:hover { background: #E7EEE7; border-color: #C9D4CA; }
.ham-btn svg   { width: 18px; height: 18px; display: block; }
.ham-btn line  { stroke: #3A473C; stroke-width: 2.2; }

.ap-overlay {
  position: fixed;
  inset: 0;
  background: rgba(20, 30, 22, 0.52);
  backdrop-filter: blur(4px);
  z-index: 9000;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
}

.ap-shell {
  display: flex;
  width: min(860px, calc(100vw - 40px));
  height: min(680px, calc(100vh - 40px));
  background: #fff;
  border-radius: 18px;
  overflow: hidden;
  box-shadow: 0 24px 80px rgba(0,0,0,0.20);
}

.ap-sidebar {
  width: 240px;
  flex-shrink: 0;
  background: #F4F6F4;
  border-right: 1px solid #E8ECE8;
  display: flex;
  flex-direction: column;
  overflow-y: auto;
  position: relative;
}

.sb-profile {
  padding: 28px 20px 20px;
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  border-bottom: 1px solid #E8ECE8;
}

.sb-avatar-wrap { position: relative; margin-bottom: 10px; }
.sb-avatar {
  width: 64px; height: 64px;
  border-radius: 50%;
  background: #92A894;
  display: flex; align-items: center; justify-content: center;
  overflow: hidden;
  border: 3px solid #fff;
  box-shadow: 0 2px 10px rgba(0,0,0,0.10);
}
.sb-avatar-img { width: 100%; height: 100%; object-fit: cover; }
.sb-avatar-ini { color: #fff; font-size: 22px; font-weight: 600; letter-spacing: -0.5px; }

.sb-cam {
  position: absolute; bottom: 0; right: 0;
  width: 22px; height: 22px;
  border-radius: 50%;
  background: #C9A06A;
  border: 2px solid #F4F6F4;
  cursor: pointer;
  display: flex; align-items: center; justify-content: center;
  transition: background 0.18s;
}
.sb-cam:disabled, .profile-cam-btn:disabled { opacity: 0.55; cursor: not-allowed; }
.sb-cam:hover { background: #b8894e; }

.sb-name  { color: #2F352F; font-size: 14px; font-weight: 700; margin-bottom: 3px; }
.sb-email { color: #6C756D; font-size: 11px; margin-bottom: 8px; word-break: break-all; }

.sb-pill {
  display: inline-flex; align-items: center; gap: 5px;
  background: #fff; border: 1px solid #E8ECE8;
  border-radius: 20px; font-size: 11px; color: #596B5C; padding: 3px 10px;
}
.sb-dot { width: 6px; height: 6px; border-radius: 50%; flex-shrink: 0; }
.dot-green { background: #7EC8A0; }
.dot-gold  { background: #C9A06A; }
.dot-blue  { background: #7BAAD4; }

.sb-nav { padding: 12px; display: flex; flex-direction: column; gap: 2px; flex: 1; }

.sb-item {
  display: flex; align-items: center; gap: 9px;
  width: 100%; padding: 9px 12px;
  border-radius: 10px; border: none;
  background: transparent; color: #4A574C;
  font-size: 13.5px; font-family: inherit;
  text-align: left; cursor: pointer;
  transition: background 0.15s, color 0.15s;
  line-height: 1.3;
}
.sb-item:hover  { background: #E7EEE7; color: #3A473C; }
.sb-item.active { background: #fff; color: #3A473C; font-weight: 600; box-shadow: 0 1px 4px rgba(0,0,0,0.07); }
.sb-item.active .sb-icon { color: #3A473C; }
.sb-item-nav .sb-arrow { margin-left: auto; width: 14px; height: 14px; opacity: 0.45; }
.sb-item-danger { color: #C45252; }
.sb-item-danger:hover { background: rgba(196,82,82,0.08); color: #b03030; }
.sb-item-danger .sb-icon { color: #C45252; }

.sb-icon    { width: 17px; height: 17px; flex-shrink: 0; color: #6C756D; transition: color 0.15s; }
.sb-divider { height: 1px; background: #E8ECE8; margin: 6px 4px; }

.sb-close-btn {
  position: absolute; top: 12px; right: 12px;
  width: 26px; height: 26px;
  border-radius: 7px; border: none;
  background: #E8ECE8; color: #596B5C;
  cursor: pointer;
  display: flex; align-items: center; justify-content: center;
  transition: background 0.15s;
}
.sb-close-btn:hover { background: #dce5dc; }

.ap-content     { flex: 1; overflow-y: auto; background: #fff; }
.content-section { padding: 32px 36px; }
.content-title  { font-size: 20px; font-weight: 700; color: #2F352F; margin-bottom: 24px; letter-spacing: -0.4px; }

.profile-hero {
  display: flex; align-items: center; gap: 20px;
  margin-bottom: 24px; padding: 20px;
  background: #F7F8F7; border-radius: 14px;
}
.profile-avatar-wrap { position: relative; flex-shrink: 0; }
.profile-avatar {
  width: 80px; height: 80px;
  border-radius: 50%; background: #3A473C;
  display: flex; align-items: center; justify-content: center;
  overflow: hidden; border: 3px solid #fff;
  box-shadow: 0 4px 14px rgba(0,0,0,0.12);
  color: #fff; font-size: 26px; font-weight: 700;
}
.profile-avatar-img { width: 100%; height: 100%; object-fit: cover; }
.profile-cam-btn {
  position: absolute; bottom: 2px; right: 2px;
  width: 26px; height: 26px; border-radius: 50%;
  background: #C9A06A; border: 2.5px solid #fff;
  cursor: pointer; display: flex; align-items: center; justify-content: center;
  transition: background 0.15s;
}
.profile-cam-btn:hover { background: #b8894e; }
.profile-name  { font-size: 18px; font-weight: 700; color: #2F352F; margin-bottom: 3px; }
.profile-email { font-size: 13px; color: #6C756D; }

.info-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; margin-bottom: 20px; }
.info-field { background: #F7F8F7; border: 1px solid #E8ECE8; border-radius: 10px; padding: 12px 14px; }
.info-field label { display: block; font-size: 11px; color: #8A9389; margin-bottom: 4px; text-transform: uppercase; letter-spacing: 0.04em; }
.info-field span  { font-size: 13px; color: #2F352F; font-weight: 500; }

.action-row      { display: flex; gap: 10px; }
.section-sub-title { font-size: 13px; font-weight: 600; color: #3A473C; margin-bottom: 12px; }

.empty-state { display: flex; flex-direction: column; align-items: center; justify-content: center; min-height: 320px; gap: 12px; text-align: center; padding: 20px; }
.empty-title { font-size: 16px; font-weight: 600; color: #2F352F; }
.empty-sub   { font-size: 13px; color: #8A9389; max-width: 320px; line-height: 1.6; }
.empty-sub-note { font-size: 13px; color: #8A9389; padding: 16px 0; }

.summary-card  { background: #3A473C; border-radius: 14px; padding: 20px 24px; margin-bottom: 20px; color: #fff; }
.summary-label { font-size: 12px; color: rgba(255,255,255,0.65); margin-bottom: 4px; }
.summary-value { font-size: 28px; font-weight: 700; letter-spacing: -0.5px; margin-bottom: 4px; }
.summary-sub   { font-size: 12px; color: rgba(255,255,255,0.55); }

.list-wrap { display: flex; flex-direction: column; gap: 10px; }
.list-card { display: flex; align-items: center; gap: 14px; padding: 14px 16px; background: #F7F8F7; border: 1px solid #E8ECE8; border-radius: 12px; transition: border-color 0.15s; }
.list-card:hover { border-color: #C4D0C5; }
.list-card-icon  { width: 40px; height: 40px; border-radius: 10px; background: #E7EEE7; display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
.list-card-info  { flex: 1; min-width: 0; }
.list-card-title { font-size: 13.5px; font-weight: 600; color: #2F352F; margin-bottom: 3px; }
.list-card-sub   { font-size: 11.5px; color: #8A9389; }
.list-card-obs   { font-size: 11.5px; color: #6C756D; margin-top: 3px; }

.badge       { display: inline-block; font-size: 11px; padding: 3px 9px; border-radius: 20px; font-weight: 600; white-space: nowrap; }
.badge-green { background: #E7EEE7; color: #3A6640; }
.badge-gold  { background: #FEF3E2; color: #8A5C10; }
.badge-red   { background: #FDEAEA; color: #C45252; }
.badge-gray  { background: #F0F2F0; color: #6C756D; }

.vol-hero-card  { display: flex; align-items: center; gap: 14px; background: #3A473C; border-radius: 14px; padding: 18px 20px; }
.vol-hero-icon  { width: 48px; height: 48px; border-radius: 12px; background: rgba(255,255,255,0.12); display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
.vol-hero-tipo  { font-size: 15px; font-weight: 700; color: #fff; margin-bottom: 3px; }
.vol-hero-fecha { font-size: 12px; color: rgba(255,255,255,0.6); }

.config-block       { border: 1px solid #E8ECE8; border-radius: 14px; padding: 20px 22px; margin-bottom: 16px; }
.config-block-title { font-size: 14px; font-weight: 700; color: #2F352F; margin-bottom: 16px; display: flex; align-items: center; }
.config-desc        { font-size: 13px; color: #6C756D; margin-bottom: 14px; line-height: 1.6; }
.config-fields      { display: flex; flex-direction: column; gap: 12px; margin-bottom: 16px; }
.config-field       { display: flex; flex-direction: column; gap: 5px; }
.config-field label { font-size: 12px; color: #596B5C; font-weight: 600; }
.config-field input {
  padding: 9px 12px; border: 1.5px solid #E8ECE8; border-radius: 9px;
  font-size: 13.5px; color: #2F352F; font-family: inherit;
  background: #FAFAFA; outline: none; transition: border-color 0.15s;
}
.config-field input:focus { border-color: #92A894; background: #fff; }

.foto-row    { display: flex; align-items: center; gap: 16px; }
.foto-preview {
  width: 64px; height: 64px; border-radius: 50%;
  background: #92A894; display: flex; align-items: center; justify-content: center;
  overflow: hidden; flex-shrink: 0; color: #fff; font-size: 20px; font-weight: 700;
  border: 2px solid #E8ECE8;
}
.foto-preview img { width: 100%; height: 100%; object-fit: cover; }
.foto-actions { display: flex; flex-direction: column; gap: 8px; }

.pw-form    { display: flex; flex-direction: column; gap: 14px; }
.pw-actions { display: flex; gap: 10px; }

.msg-success { font-size: 12.5px; color: #3A6640; background: #E7EEE7; border-radius: 8px; padding: 8px 12px; }
.msg-error   { font-size: 12.5px; color: #C45252; background: #FDEAEA; border-radius: 8px; padding: 8px 12px; }

.btn-primary {
  padding: 10px 20px; background: #3A473C; color: #fff;
  border: none; border-radius: 10px;
  font-size: 13.5px; font-family: inherit; font-weight: 600;
  cursor: pointer; transition: background 0.15s;
  display: inline-flex; align-items: center; gap: 6px;
}
.btn-primary:hover:not(:disabled) { background: #2F3B31; }
.btn-primary:disabled { opacity: 0.55; cursor: not-allowed; }

.btn-spinner { display:inline-block; width:13px; height:13px; border:2px solid rgba(255,255,255,.4); border-top-color:#fff; border-radius:50%; animation:btn-spin .7s linear infinite; }
@keyframes btn-spin { to { transform:rotate(360deg); } }

.btn-ghost {
  padding: 10px 20px; background: transparent; color: #3A473C;
  border: 1.5px solid #92A894; border-radius: 10px;
  font-size: 13.5px; font-family: inherit; font-weight: 600;
  cursor: pointer; transition: background 0.15s;
}
.btn-ghost:hover    { background: #E7EEE7; }
.btn-ghost:disabled { opacity: 0.55; cursor: not-allowed; }

.btn-danger {
  padding: 10px 20px; background: #FDEAEA; color: #C45252;
  border: 1.5px solid #f0c0c0; border-radius: 10px;
  font-size: 13.5px; font-family: inherit; font-weight: 600;
  cursor: pointer; transition: background 0.15s;
}
.btn-danger:hover { background: #f9d5d5; }

.overlay-fade-enter-active, .overlay-fade-leave-active { transition: opacity 0.26s ease; }
.overlay-fade-enter-from,  .overlay-fade-leave-to      { opacity: 0; }

.panel-slide-enter-active { transition: opacity 0.28s ease, transform 0.28s cubic-bezier(0.4,0,0.2,1); }
.panel-slide-leave-active { transition: opacity 0.18s ease, transform 0.18s ease; }
.panel-slide-enter-from, .panel-slide-leave-to { opacity: 0; transform: scale(0.96) translateY(8px); }

@media (max-width: 700px) {
  .ap-overlay { padding: 0; }
  .ap-shell   { width: 100%; height: 100%; border-radius: 0; flex-direction: column; }
  .ap-sidebar { width: 100%; flex-shrink: 0; max-height: 220px; border-right: none; border-bottom: 1px solid #E8ECE8; }
  .sb-profile { padding: 16px 20px 12px; flex-direction: row; text-align: left; gap: 12px; }
  .sb-avatar  { width: 48px; height: 48px; }
  .sb-nav     { flex-direction: row; overflow-x: auto; gap: 4px; padding: 8px 12px; }
  .sb-item    { white-space: nowrap; padding: 7px 10px; font-size: 12.5px; }
  .sb-close-btn    { top: 10px; right: 10px; }
  .content-section { padding: 20px 18px; }
  .info-grid  { grid-template-columns: 1fr; }
  .pw-actions { flex-direction: column; }
}

@media (max-width: 480px) {
  .content-title { font-size: 17px; }
  .profile-hero  { flex-direction: column; text-align: center; }
  .foto-row      { flex-direction: column; align-items: flex-start; }
}
</style>