<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { ubicacionesCR } from '../../data/ubicaciones'
import { registrarAuditoria } from '../../composables/useAuditLog'
import {
  getVolunteers,
  updateVolunteer,
  updateVolunteerStatus,
  parseApplicationDetails,
} from '../../services/volunteerServices'

// ── Estado principal ──────────────────────────────────────────
const voluntarios  = ref([])
const cargando     = ref(false)
const errorCarga   = ref('')
const filtroEstado = ref('Todos')
const filtroTipo   = ref('Todos')
const filtroProv   = ref('Todos')
const search       = ref('')

// ── Modales ───────────────────────────────────────────────────
const modalVer     = ref(false)
const modalEditar  = ref(false)
const modalConfirm = ref(false)
const voluntarioActivo = ref(null)

const accionPendiente = ref(null)
const mensajeConfirm  = ref('')

const toast = ref({ visible: false, tipo: 'exito', texto: '' })

// ── Formulario de edición ─────────────────────────────────────
const formEditar = ref({
  nombre: '', cedula: '', correo: '', telefono: '',
  provincia: '', canton: '', distrito: '',
  tipo: '',
  datosEspecificos: {}
})
// La motivación ("motivation") no se edita desde este formulario, pero
// updateVolunteer() reemplaza el registro completo, así que se conserva
// el valor original aquí para reenviarlo intacto al guardar.
const motivoActual = ref('')

// ── Ubicación en edición ──────────────────────────────────────
const provincias = computed(() =>
  ubicacionesCR ? Object.keys(ubicacionesCR) : []
)
const cantonesEdit = computed(() => {
  if (!formEditar.value.provincia || !ubicacionesCR) return []
  return Object.keys(ubicacionesCR[formEditar.value.provincia] || {})
})
const distritosEdit = computed(() => {
  if (!formEditar.value.provincia || !formEditar.value.canton || !ubicacionesCR) return []
  return ubicacionesCR[formEditar.value.provincia]?.[formEditar.value.canton] || []
})
watch(() => formEditar.value.provincia, () => {
  formEditar.value.canton   = ''
  formEditar.value.distrito = ''
})
watch(() => formEditar.value.canton, () => {
  formEditar.value.distrito = ''
})

const cantonesZonaEdit = computed(() => {
  const p = formEditar.value.datosEspecificos?.zonaProvincia
  if (!p || !ubicacionesCR) return []
  return Object.keys(ubicacionesCR[p] || {})
})
watch(() => formEditar.value.datosEspecificos?.zonaProvincia, () => {
  if (formEditar.value.datosEspecificos)
    formEditar.value.datosEspecificos.zonaCanton = ''
})

// ── Carga desde el backend ─────────────────────────────────────
// NOTA / SUPUESTO DE INTEGRACIÓN:
// No se pudo verificar el contrato exacto de GET /api/volunteers. Este
// mapeo asume que cada registro trae los mismos nombres de campo que
// vuelven a enviarse en submitVolunteerApplication()/updateVolunteer()
// (email, nationalId, volunteerType, motivation, applicationDetails,
// phonePrimary, city, town, district) más status, _id/id y, si el
// backend ya asigna un código de voluntario al aprobar, volunteerCode.
// El nombre del voluntario no forma parte del modelo de la solicitud
// (la solicitud se vincula por correo a una cuenta ya existente), así
// que se intenta leerlo de un posible objeto de usuario anidado y, si
// no existe, se usa el correo como respaldo. Ajusta mapVoluntario()
// si el backend expone estos datos de otra forma.
function mapVoluntario(d) {
  const id      = d._id || d.id
  const nombre  = d.nombre || d.name || d.user?.nombre || d.user?.name || d.usuario?.nombre || d.email
  const detalles = parseApplicationDetails(d.applicationDetails)
  const direccion = {
    provincia: d.city ?? '',
    canton:    d.town ?? '',
    distrito:  d.district ?? '',
  }
  const solicitudVoluntario = {
    nombre,
    cedula:    d.nationalId ?? '',
    correo:    d.email ?? '',
    telefono:  d.phonePrimary ?? '',
    direccion,
    tipo:      d.volunteerType ?? '',
    datosEspecificos: detalles,
    motivo:    d.motivation ?? '',
    // Se asume que el backend devuelve el estado ya en español
    // ('Pendiente' | 'Aprobada' | 'Rechazada' | 'Inactivo'), igual que
    // las acciones que recibe updateVolunteerStatus(). Ajusta aquí si
    // el backend usa otros valores.
    estado: d.status ?? d.estado ?? 'Pendiente',
  }
  return {
    id,
    codigoVoluntario: d.volunteerCode ?? d.codigoVoluntario ?? null,
    rol: d.rol ?? d.role ?? null,
    nombre,
    correo: d.email ?? '',
    cedula: d.nationalId ?? '',
    telefono: d.phonePrimary ?? '',
    direccion,
    solicitudVoluntario,
  }
}

async function cargarVoluntarios() {
  cargando.value   = true
  errorCarga.value = ''
  try {
    const data  = await getVolunteers()
    const lista = Array.isArray(data) ? data : (data?.volunteers || data?.data || [])
    voluntarios.value = lista.map(mapVoluntario)

    registrarAuditoria({
      modulo: 'Voluntarios',
      accion: 'Consultó el listado de voluntarios',
      // AUDIT_TIPOS_ACCION no incluye un tipo "consultar"; se usa 'editar'
      // como el más cercano disponible sin modificar useAuditLog.js.
      tipoAccion: 'editar',
      elemento: 'Listado de voluntarios',
      descripcion: `Se cargaron ${voluntarios.value.length} solicitudes de voluntariado desde el servidor.`,
      estado: 'Exitoso',
    })
  } catch (e) {
    console.error('Error al cargar voluntarios:', e)
    errorCarga.value = 'No se pudieron cargar los voluntarios. Intenta de nuevo más tarde.'
    voluntarios.value = []

    registrarAuditoria({
      modulo: 'Voluntarios',
      accion: 'Consultó el listado de voluntarios',
      tipoAccion: 'editar',
      elemento: 'Listado de voluntarios',
      descripcion: `Falló la carga de voluntarios: ${e?.message || 'error desconocido'}.`,
      estado: 'Fallido',
    })
  } finally {
    cargando.value = false
  }
}

onMounted(() => {
  cargarVoluntarios()
})

// ── Provincias disponibles en los datos ──────────────────────
const provinciasDisponibles = computed(() =>
  ubicacionesCR ? Object.keys(ubicacionesCR) : []
)

// ── Tipos disponibles ─────────────────────────────────────────
const TIPOS = ['Casa cuna','Eventos de adopción','Transporte','Veterinaria','Redes sociales','Rescatista']

// ── Filtrado ──────────────────────────────────────────────────
const voluntariosFiltrados = computed(() => {
  let result = voluntarios.value

  const q = search.value.trim().toLowerCase()
  if (q) {
    result = result.filter(v =>
      (v.solicitudVoluntario?.nombre || v.nombre || '').toLowerCase().includes(q)
    )
  }

  if (filtroEstado.value !== 'Todos') {
    result = result.filter(v => v.solicitudVoluntario?.estado === filtroEstado.value)
  }

  if (filtroTipo.value !== 'Todos') {
    result = result.filter(v => v.solicitudVoluntario?.tipo === filtroTipo.value)
  }

  if (filtroProv.value !== 'Todos') {
    result = result.filter(v => v.solicitudVoluntario?.direccion?.provincia === filtroProv.value)
  }

  return result
})

const hayFiltros = computed(() =>
  search.value.trim() !== '' ||
  filtroEstado.value !== 'Todos' ||
  filtroTipo.value !== 'Todos' ||
  filtroProv.value !== 'Todos'
)

function limpiarFiltros() {
  search.value       = ''
  filtroEstado.value = 'Todos'
  filtroTipo.value   = 'Todos'
  filtroProv.value   = 'Todos'
}

// ── Toast ─────────────────────────────────────────────────────
function mostrarToast(texto, tipo = 'exito') {
  toast.value = { visible: true, tipo, texto }
  setTimeout(() => { toast.value.visible = false }, 3000)
}

// ── Confirmación ──────────────────────────────────────────────
const mensajesPorAccion = {
  aprobar:   v => `¿Aprobar la solicitud de <strong>${v.solicitudVoluntario?.nombre || v.nombre}</strong>?`,
  rechazar:  v => `¿Rechazar la solicitud de <strong>${v.solicitudVoluntario?.nombre || v.nombre}</strong>?`,
  inactivar: v => `¿Inactivar al voluntario <strong>${v.solicitudVoluntario?.nombre || v.nombre}</strong>?`,
  reactivar: v => `¿Reactivar al voluntario <strong>${v.solicitudVoluntario?.nombre || v.nombre}</strong>?`,
}

function pedirConfirmacion(tipo, voluntario) {
  accionPendiente.value = { tipo, voluntario }
  mensajeConfirm.value  = mensajesPorAccion[tipo](voluntario)
  modalConfirm.value    = true
}

async function confirmarAccion() {
  modalConfirm.value = false
  const { tipo, voluntario } = accionPendiente.value
  const acciones = {
    aprobar:   () => ejecutarAprobar(voluntario),
    rechazar:  () => ejecutarRechazar(voluntario),
    inactivar: () => ejecutarInactivar(voluntario),
    reactivar: () => ejecutarReactivar(voluntario),
  }
  await acciones[tipo]?.()
  accionPendiente.value = null
}

function cancelarConfirmacion() {
  modalConfirm.value    = false
  accionPendiente.value = null
}

// ── Acciones de estado (contra el backend) ─────────────────────
// Nota: antes esta vista generaba el código "VOL-XXX" y asignaba el rol
// 'Voluntario' en el propio cliente al aprobar. Eso vivía en
// localStorage; ahora que el estado lo controla el backend a través de
// updateVolunteerStatus(), se asume que es el servidor quien asigna el
// código de voluntario y el rol al procesar la acción 'Aprobar'. Si no
// es así, falta un endpoint/lógica en el backend para hacerlo.
async function ejecutarAprobar(usuario) {
  const estadoAnterior = usuario.solicitudVoluntario?.estado
  try {
    await updateVolunteerStatus(usuario.id, 'Aprobar')
    await cargarVoluntarios()
    registrarAuditoria({
      modulo: 'Voluntarios',
      accion: 'Aprobó una solicitud de voluntariado',
      tipoAccion: 'aprobar',
      elemento: usuario.solicitudVoluntario?.nombre || usuario.nombre,
      elementoId: usuario.id,
      descripcion: `Solicitud de voluntariado de "${usuario.solicitudVoluntario?.nombre || usuario.nombre}" aprobada.`,
      estado: 'Exitoso',
      valoresAnteriores: { estado: estadoAnterior },
      valoresNuevos: { estado: 'Aprobada' },
    })
    mostrarToast('Solicitud aprobada correctamente.')
  } catch (e) {
    console.error(e)
    registrarAuditoria({
      modulo: 'Voluntarios',
      accion: 'Aprobó una solicitud de voluntariado',
      tipoAccion: 'aprobar',
      elemento: usuario.solicitudVoluntario?.nombre || usuario.nombre,
      elementoId: usuario.id,
      descripcion: `Falló la aprobación: ${e?.message || 'error desconocido'}.`,
      estado: 'Fallido',
      valoresAnteriores: { estado: estadoAnterior },
    })
    mostrarToast('Error al aprobar la solicitud.', 'error')
  }
}

async function ejecutarRechazar(usuario) {
  const estadoAnterior = usuario.solicitudVoluntario?.estado
  try {
    await updateVolunteerStatus(usuario.id, 'Rechazar')
    await cargarVoluntarios()
    registrarAuditoria({
      modulo: 'Voluntarios',
      accion: 'Rechazó una solicitud de voluntariado',
      tipoAccion: 'rechazar',
      elemento: usuario.solicitudVoluntario?.nombre || usuario.nombre,
      elementoId: usuario.id,
      descripcion: `Solicitud de voluntariado de "${usuario.solicitudVoluntario?.nombre || usuario.nombre}" rechazada.`,
      estado: 'Exitoso',
      valoresAnteriores: { estado: estadoAnterior },
      valoresNuevos: { estado: 'Rechazada' },
    })
    mostrarToast('Solicitud rechazada.')
  } catch (e) {
    console.error(e)
    registrarAuditoria({
      modulo: 'Voluntarios',
      accion: 'Rechazó una solicitud de voluntariado',
      tipoAccion: 'rechazar',
      elemento: usuario.solicitudVoluntario?.nombre || usuario.nombre,
      elementoId: usuario.id,
      descripcion: `Falló el rechazo: ${e?.message || 'error desconocido'}.`,
      estado: 'Fallido',
      valoresAnteriores: { estado: estadoAnterior },
    })
    mostrarToast('Error al rechazar la solicitud.', 'error')
  }
}

async function ejecutarInactivar(usuario) {
  const estadoAnterior = usuario.solicitudVoluntario?.estado
  try {
    await updateVolunteerStatus(usuario.id, 'Inactivar')
    await cargarVoluntarios()
    registrarAuditoria({
      modulo: 'Voluntarios',
      accion: 'Inactivó un voluntario',
      tipoAccion: 'estado',
      elemento: usuario.solicitudVoluntario?.nombre || usuario.nombre,
      elementoId: usuario.id,
      descripcion: `El voluntario "${usuario.solicitudVoluntario?.nombre || usuario.nombre}" fue inactivado.`,
      estado: 'Exitoso',
      valoresAnteriores: { estado: estadoAnterior },
      valoresNuevos: { estado: 'Inactivo' },
    })
    mostrarToast('Voluntario inactivado.')
  } catch (e) {
    registrarAuditoria({
      modulo: 'Voluntarios',
      accion: 'Inactivó un voluntario',
      tipoAccion: 'estado',
      elemento: usuario.solicitudVoluntario?.nombre || usuario.nombre,
      elementoId: usuario.id,
      descripcion: `Falló la inactivación: ${e?.message || 'error desconocido'}.`,
      estado: 'Fallido',
      valoresAnteriores: { estado: estadoAnterior },
    })
    mostrarToast('Error al inactivar el voluntario.', 'error')
  }
}

async function ejecutarReactivar(usuario) {
  const estadoAnterior = usuario.solicitudVoluntario?.estado
  try {
    await updateVolunteerStatus(usuario.id, 'Reactivar')
    await cargarVoluntarios()
    registrarAuditoria({
      modulo: 'Voluntarios',
      accion: 'Reactivó un voluntario',
      tipoAccion: 'estado',
      elemento: usuario.solicitudVoluntario?.nombre || usuario.nombre,
      elementoId: usuario.id,
      descripcion: `El voluntario "${usuario.solicitudVoluntario?.nombre || usuario.nombre}" fue reactivado.`,
      estado: 'Exitoso',
      valoresAnteriores: { estado: estadoAnterior },
      valoresNuevos: { estado: 'Aprobada' },
    })
    mostrarToast('Voluntario reactivado correctamente.')
  } catch (e) {
    registrarAuditoria({
      modulo: 'Voluntarios',
      accion: 'Reactivó un voluntario',
      tipoAccion: 'estado',
      elemento: usuario.solicitudVoluntario?.nombre || usuario.nombre,
      elementoId: usuario.id,
      descripcion: `Falló la reactivación: ${e?.message || 'error desconocido'}.`,
      estado: 'Fallido',
      valoresAnteriores: { estado: estadoAnterior },
    })
    mostrarToast('Error al reactivar el voluntario.', 'error')
  }
}

// ── Modales ───────────────────────────────────────────────────
function abrirVer(v) {
  voluntarioActivo.value = v
  modalVer.value = true
}

function abrirEditar(v) {
  voluntarioActivo.value = v
  const s   = v.solicitudVoluntario || {}
  const dir = s.direccion || {}
  motivoActual.value = s.motivo || ''
  formEditar.value = {
    nombre:    s.nombre   || v.nombre   || '',
    cedula:    s.cedula   || v.cedula   || '',
    correo:    s.correo   || v.correo   || '',
    telefono:  s.telefono || v.telefono || '',
    provincia: dir.provincia || '',
    canton:    dir.canton    || '',
    distrito:  dir.distrito  || '',
    tipo:      s.tipo || '',
    datosEspecificos: s.datosEspecificos
      ? JSON.parse(JSON.stringify(s.datosEspecificos))
      : {}
  }
  modalEditar.value = true
}

async function guardarEdicion() {
  const objetivo = voluntarioActivo.value
  const f = formEditar.value
  const deOriginal = objetivo?.solicitudVoluntario?.datosEspecificos || {}
  const deEditado  = { ...f.datosEspecificos }
  if (f.tipo === 'Rescatista') {
    deEditado.anosExp      = deOriginal.anosExp
    deEditado.cantRescates = deOriginal.cantRescates
  }

  // updateVolunteer() reemplaza el registro completo, así que se
  // reenvían también los campos que este formulario no edita
  // (motivation) para no perderlos.
  const payload = {
    nationalId: f.cedula,
    volunteerType: f.tipo,
    motivation: motivoActual.value || null,
    applicationDetails: deEditado,
    phonePrimary: f.telefono,
    city: f.provincia,
    town: f.canton,
    district: f.distrito,
  }

  try {
    await updateVolunteer(objetivo.id, payload)
    await cargarVoluntarios()
    registrarAuditoria({
      modulo: 'Voluntarios',
      accion: 'Editó un voluntario',
      tipoAccion: 'editar',
      elemento: f.nombre || objetivo?.nombre,
      elementoId: objetivo?.id,
      descripcion: `Se actualizó la información del voluntario "${f.nombre}".`,
      estado: 'Exitoso',
      valoresAnteriores: { ...objetivo?.solicitudVoluntario },
      valoresNuevos: { ...payload },
    })
    modalEditar.value = false
    mostrarToast('Información actualizada correctamente.')
  } catch (e) {
    console.error(e)
    registrarAuditoria({
      modulo: 'Voluntarios',
      accion: 'Editó un voluntario',
      tipoAccion: 'editar',
      elemento: f.nombre || objetivo?.nombre,
      elementoId: objetivo?.id,
      descripcion: `Falló la edición: ${e?.message || 'error desconocido'}.`,
      estado: 'Fallido',
    })
    mostrarToast('Error al guardar los cambios.', 'error')
  }
}

function toggleDE(key, val) {
  const arr = formEditar.value.datosEspecificos[key]
  if (!Array.isArray(arr)) { formEditar.value.datosEspecificos[key] = [val]; return }
  const idx = arr.indexOf(val)
  if (idx === -1) arr.push(val)
  else arr.splice(idx, 1)
}
function deIncludes(key, val) {
  const arr = formEditar.value.datosEspecificos[key]
  return Array.isArray(arr) && arr.includes(val)
}

// ── Badge helpers ─────────────────────────────────────────────
function estadoBadgeClass(estado) {
  if (estado === 'Aprobada')  return 'badge-aprobada'
  if (estado === 'Rechazada') return 'badge-rechazada'
  if (estado === 'Inactivo')  return 'badge-inactiva'
  return 'badge-pendiente'
}

function tipoBadgeClass(tipo) {
  return {
    'Casa cuna':           'badge-blue',
    'Eventos de adopción': 'badge-purple',
    'Transporte':          'badge-teal',
    'Veterinaria':         'badge-crimson',
    'Redes sociales':      'badge-sky',
    'Rescatista':          'badge-gold',
  }[tipo] || 'badge-inactiva'
}

function getDE(v) { return v?.solicitudVoluntario?.datosEspecificos || {} }

function iniciales(nombre) {
  if (!nombre) return '?'
  return nombre.trim().split(' ').map(p => p[0]).slice(0, 2).join('').toUpperCase()
}

// ── KPIs ─────────────────────────────────────────────────────
const totalVoluntarios = computed(() => voluntarios.value.length)
const totalPendientes  = computed(() => voluntarios.value.filter(v => v.solicitudVoluntario?.estado === 'Pendiente').length)
const totalAprobados   = computed(() => voluntarios.value.filter(v => v.solicitudVoluntario?.estado === 'Aprobada').length)
const totalInactivos   = computed(() => voluntarios.value.filter(v => v.solicitudVoluntario?.estado === 'Inactivo').length)
const totalRechazados  = computed(() => voluntarios.value.filter(v => v.solicitudVoluntario?.estado === 'Rechazada').length)
</script>

<template>
  <div class="view-container">

    <!-- ── Toast (mismo componente que Mascotas) ── -->
    <Teleport to="body">
      <Transition name="toast-fade">
        <div v-if="toast.visible" class="don-toast" :class="toast.tipo === 'error' ? 'error' : 'success'">
          <span class="don-toast-dot"></span>
          {{ toast.texto }}
        </div>
      </Transition>
    </Teleport>

    <div>
      <!-- CABECERA -->
      <header class="page-header">
        <div class="brand-row">
          <div class="brand-mark">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
          </div>
          <div>
            <h1 class="admin-page-title">Voluntarios</h1>
            <p class="admin-page-sub">Gestión de solicitudes y voluntarios activos</p>
          </div>
        </div>
      </header>

      <!-- TARJETAS RESUMEN -->
      <div class="don-summary">
        <div class="don-card total-card">
          <div class="don-icon total-icon">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="7" height="7" rx="1.5"/><rect x="14" y="3" width="7" height="7" rx="1.5"/><rect x="3" y="14" width="7" height="7" rx="1.5"/><rect x="14" y="14" width="7" height="7" rx="1.5"/></svg>
          </div>
          <strong class="don-value">{{ totalVoluntarios }}</strong>
          <span class="don-label">Total solicitudes</span>
          <span class="don-desc">En el sistema</span>
        </div>
        <div class="don-card proceso-card">
          <div class="don-icon proceso-icon">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="9"/><polyline points="12 7 12 12 16 14"/></svg>
          </div>
          <strong class="don-value">{{ totalPendientes }}</strong>
          <span class="don-label">Pendientes</span>
          <span class="don-desc">Por revisar</span>
        </div>
        <div class="don-card disponible-card">
          <div class="don-icon disponible-icon">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/></svg>
          </div>
          <strong class="don-value">{{ totalAprobados }}</strong>
          <span class="don-label">Aprobados</span>
          <span class="don-desc">Voluntarios activos</span>
        </div>
        <div class="don-card rechazada-card">
          <div class="don-icon rechazada-icon">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="9"/><line x1="9" y1="9" x2="15" y2="15"/><line x1="15" y1="9" x2="9" y2="15"/></svg>
          </div>
          <strong class="don-value">{{ totalRechazados }}</strong>
          <span class="don-label">Rechazados</span>
          <span class="don-desc">Solicitudes cerradas</span>
        </div>
        <div class="don-card inactiva-card">
          <div class="don-icon inactiva-icon">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M21 8v13H3V8"/><path d="M1 3h22v5H1z"/><line x1="10" y1="12" x2="14" y2="12"/></svg>
          </div>
          <strong class="don-value">{{ totalInactivos }}</strong>
          <span class="don-label">Inactivos</span>
          <span class="don-desc">Fuera de servicio</span>
        </div>
      </div>

      <!-- FILTROS -->
      <div class="filtros-panel">
        <div class="filtros-row">
          <div class="filtro-group filtro-group--tabs">
            <label class="filtro-label">Tipo</label>
            <div class="tabs-wrap">
              <button class="tab-btn" :class="{ active: filtroTipo === 'Todos' }" @click="filtroTipo = 'Todos'">Todos</button>
              <button v-for="t in TIPOS" :key="t" class="tab-btn" :class="{ active: filtroTipo === t }" @click="filtroTipo = t">{{ t }}</button>
            </div>
          </div>
          <div class="filtro-group filtro-group--tabs">
            <label class="filtro-label">Estado</label>
            <div class="tabs-wrap">
              <button class="tab-btn" :class="{ active: filtroEstado === 'Todos' }" @click="filtroEstado = 'Todos'">Todos</button>
              <button class="tab-btn" :class="{ active: filtroEstado === 'Pendiente' }" @click="filtroEstado = 'Pendiente'">Pendientes</button>
              <button class="tab-btn" :class="{ active: filtroEstado === 'Aprobada' }" @click="filtroEstado = 'Aprobada'">Aprobados</button>
              <button class="tab-btn" :class="{ active: filtroEstado === 'Rechazada' }" @click="filtroEstado = 'Rechazada'">Rechazados</button>
              <button class="tab-btn" :class="{ active: filtroEstado === 'Inactivo' }" @click="filtroEstado = 'Inactivo'">Inactivos</button>
            </div>
          </div>
        </div>

        <div class="filtros-divider"></div>

        <div class="filtros-row filtros-row--end">
          <div class="filtro-group filtro-group--search">
            <label class="filtro-label">Buscar voluntario</label>
            <div class="filtro-input-wrap">
              <span class="filtro-icon filtro-icon--left">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
              </span>
              <input v-model="search" placeholder="Nombre del voluntario..." class="filtro-input filtro-input--icon-left" />
            </div>
          </div>

          <div class="filtro-group filtro-group--prov">
            <label class="filtro-label">Provincia</label>
            <select v-model="filtroProv" class="select">
              <option value="Todos">Todas</option>
              <option v-for="p in provinciasDisponibles" :key="p" :value="p">{{ p }}</option>
            </select>
          </div>

          <div class="filtro-group filtro-group--btn">
            <button class="btn btn--ghost" :class="{ 'btn--ghost-active': hayFiltros }" @click="limpiarFiltros">Limpiar filtros</button>
          </div>
        </div>
      </div>

      <!-- ESTADO VACÍO -->
      <div v-if="voluntariosFiltrados.length === 0" class="empty-state">
        <svg xmlns="http://www.w3.org/2000/svg" width="36" height="36" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
        <!-- Texto ajustado mínimamente para reflejar carga/error del backend,
             sin tocar la estructura ni los estilos del bloque. -->
        <p class="empty-title">{{ errorCarga ? 'No se pudo cargar la información' : (cargando ? 'Cargando voluntarios...' : 'No hay voluntarios registrados') }}</p>
        <p class="empty-sub">{{ errorCarga || (hayFiltros ? 'Ajusta los filtros para ver resultados.' : 'Aún no hay solicitudes de voluntariado.') }}</p>
      </div>

      <!-- TABLA PRINCIPAL -->
      <div v-else class="table-wrapper">
        <div class="table-scroll">
          <table class="don-table">
            <thead>
              <tr>
                <th>Voluntario</th>
                <th>Contacto</th>
                <th>Tipo</th>
                <th>Dirección</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="v in voluntariosFiltrados" :key="v.id" class="don-row" :class="{ 'row-inactive': v.solicitudVoluntario?.estado === 'Inactivo' }">

                <!-- Voluntario -->
                <td>
                  <div class="vol-cell">
                    <div class="pet-avatar">
                      <span class="pet-avatar-ini">{{ iniciales(v.solicitudVoluntario?.nombre || v.nombre) }}</span>
                    </div>
                    <div class="vol-info">
                      <span class="donor-name">{{ v.solicitudVoluntario?.nombre || v.nombre }}</span>
                      <span class="donor-mail">{{ v.codigoVoluntario || (v.solicitudVoluntario?.estado === 'Pendiente' ? 'Sin asignar' : '—') }}</span>
                    </div>
                  </div>
                </td>

                <!-- Contacto -->
                <td>
                  <span class="donor-name">{{ v.solicitudVoluntario?.correo || v.correo || '—' }}</span>
                  <span class="donor-mail">{{ v.solicitudVoluntario?.telefono || '—' }}</span>
                </td>

                <!-- Tipo -->
                <td>
                  <span class="estado-badge" :class="tipoBadgeClass(v.solicitudVoluntario?.tipo)">{{ v.solicitudVoluntario?.tipo || '—' }}</span>
                </td>

                <!-- Dirección -->
                <td><span class="fecha-text">{{ [v.solicitudVoluntario?.direccion?.provincia, v.solicitudVoluntario?.direccion?.canton, v.solicitudVoluntario?.direccion?.distrito].filter(Boolean).join(', ') || '—' }}</span></td>

                <!-- Estado -->
                <td>
                  <span class="estado-badge" :class="estadoBadgeClass(v.solicitudVoluntario?.estado)">{{ v.solicitudVoluntario?.estado }}</span>
                </td>

                <!-- Acciones — mismo componente icon-only que Mascotas -->
                <td>
                  <div class="action-group">
                    <button class="icon-only icon-only--ver" @click="abrirVer(v)" data-tooltip="Ver expediente">
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
                    </button>

                    <template v-if="v.solicitudVoluntario?.estado === 'Pendiente'">
                      <button class="icon-only icon-only--activar" @click="pedirConfirmacion('aprobar', v)" data-tooltip="Aprobar">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
                      </button>
                      <button class="icon-only icon-only--inactivar" @click="pedirConfirmacion('rechazar', v)" data-tooltip="Rechazar">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
                      </button>
                    </template>

                    <template v-else-if="v.solicitudVoluntario?.estado === 'Aprobada'">
                      <button class="icon-only icon-only--editar" @click="abrirEditar(v)" data-tooltip="Editar">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.12 2.12 0 0 1 3 3L12 15l-4 1 1-4z"/></svg>
                      </button>
                      <button class="icon-only icon-only--inactivar" @click="pedirConfirmacion('inactivar', v)" data-tooltip="Inactivar">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 8v13H3V8"/><path d="M1 3h22v5H1z"/><line x1="10" y1="12" x2="14" y2="12"/></svg>
                      </button>
                    </template>

                    <template v-else-if="v.solicitudVoluntario?.estado === 'Inactivo'">
                      <button class="icon-only icon-only--editar" @click="abrirEditar(v)" data-tooltip="Editar">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.12 2.12 0 0 1 3 3L12 15l-4 1 1-4z"/></svg>
                      </button>
                      <button class="icon-only icon-only--activar" @click="pedirConfirmacion('reactivar', v)" data-tooltip="Reactivar">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
                      </button>
                    </template>

                    <template v-else>
                      <button class="icon-only icon-only--editar" @click="abrirEditar(v)" data-tooltip="Editar">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.12 2.12 0 0 1 3 3L12 15l-4 1 1-4z"/></svg>
                      </button>
                    </template>
                  </div>
                </td>

              </tr>
            </tbody>
          </table>
        </div>
        <div class="table-footer">
          {{ voluntariosFiltrados.length }} voluntario{{ voluntariosFiltrados.length !== 1 ? 's' : '' }} encontrado{{ voluntariosFiltrados.length !== 1 ? 's' : '' }}
        </div>
      </div>
    </div>

    <!-- ══════════════════════════════════════
         MODAL — VER EXPEDIENTE
         Mismo componente .modal-box--uniform que Mascotas
    ══════════════════════════════════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="modalVer && voluntarioActivo" class="modal-overlay" @click.self="modalVer = false">
          <div class="modal-box modal-box--uniform">
            <button class="close-btn close-btn--hero" @click="modalVer = false">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>

            <div class="hero">
              <div class="hero-photo">
                <span class="hero-photo-ini">{{ iniciales(voluntarioActivo.solicitudVoluntario?.nombre || voluntarioActivo.nombre) }}</span>
              </div>
              <div class="hero-info">
                <div class="hero-name-row">
                  <h2 class="hero-name">{{ voluntarioActivo.solicitudVoluntario?.nombre || voluntarioActivo.nombre }}</h2>
                  <span class="estado-badge badge-status-hero" :class="estadoBadgeClass(voluntarioActivo.solicitudVoluntario?.estado)">{{ voluntarioActivo.solicitudVoluntario?.estado }}</span>
                </div>
                <div class="hero-meta">
                  <span class="hero-meta-chip">
                    <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/></svg>
                    {{ voluntarioActivo.solicitudVoluntario?.tipo || '—' }}
                  </span>
                  <span class="hero-meta-chip">{{ voluntarioActivo.codigoVoluntario || (voluntarioActivo.solicitudVoluntario?.estado === 'Pendiente' ? 'Sin código' : '—') }}</span>
                  <span class="hero-meta-chip">{{ voluntarioActivo.solicitudVoluntario?.correo || voluntarioActivo.correo || '—' }}</span>
                </div>
              </div>
            </div>

            <div class="uniform-scroll">
              <div class="body">
                <div class="grid-2col">
                  <div>
                    <div class="block">
                      <h4 class="block-title">
                        <span class="block-title-icon">
                          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
                        </span>
                        Información personal
                      </h4>
                      <div class="fields-row">
                        <div class="field-col"><span class="field-label-row">Cédula</span><span class="field-value">{{ voluntarioActivo.solicitudVoluntario?.cedula || voluntarioActivo.cedula || '—' }}</span></div>
                        <div class="field-col"><span class="field-label-row">Teléfono</span><span class="field-value">{{ voluntarioActivo.solicitudVoluntario?.telefono || '—' }}</span></div>
                        <div class="field-col"><span class="field-label-row">Correo</span><span class="field-value">{{ voluntarioActivo.solicitudVoluntario?.correo || voluntarioActivo.correo || '—' }}</span></div>
                      </div>
                    </div>

                    <!-- Bloques específicos por tipo -->
                    <div v-if="voluntarioActivo.solicitudVoluntario?.tipo === 'Casa cuna'" class="block">
                      <h4 class="block-title">
  <span class="block-title-icon">
    <i class='bx bx-home'></i>
  </span>
  Casa cuna — Detalles
</h4>
                      <div class="fields-row">
                        <div class="field-col"><span class="field-label-row">Máx. mascotas</span><span class="field-value">{{ getDE(voluntarioActivo).maxMascotas || '—' }}</span></div>
                        <div class="field-col"><span class="field-label-row">Tipo de vivienda</span><span class="field-value">{{ getDE(voluntarioActivo).tipoVivienda || '—' }}</span></div>
                        <div class="field-col"><span class="field-label-row">Patio cerrado</span><span class="field-value">{{ getDE(voluntarioActivo).patioCerrado || '—' }}</span></div>
                      </div>
                      <div class="fields-row" style="margin-top:14px">
                        <div class="field-col"><span class="field-label-row">Otras mascotas</span><span class="field-value">{{ getDE(voluntarioActivo).otrasMascotas || '—' }}</span></div>
                        <div class="field-col"><span class="field-label-row">Niños en vivienda</span><span class="field-value">{{ getDE(voluntarioActivo).ninos || '—' }}</span></div>
                        <div class="field-col"><span class="field-label-row">Tiempo disponible</span><span class="field-value">{{ getDE(voluntarioActivo).tiempoDisp || '—' }}</span></div>
                      </div>
                      <div class="info-subsection" v-if="getDE(voluntarioActivo).puedeRecibir?.length">
                        <span class="field-label-row">Puede recibir</span>
                        <div class="chips-row"><span v-for="b in getDE(voluntarioActivo).puedeRecibir" :key="b" class="type-chip">{{ b }}</span></div>
                      </div>
                      <div class="info-subsection" v-if="getDE(voluntarioActivo).comentarios">
                        <span class="field-label-row">Comentarios</span>
                        <p class="info-subsection-text">{{ getDE(voluntarioActivo).comentarios }}</p>
                      </div>
                    </div>

                    <div v-if="voluntarioActivo.solicitudVoluntario?.tipo === 'Eventos de adopción'" class="block">
                      <h4 class="block-title">
  <span class="block-title-icon">
    <i class='bx bx-calendar-event'></i>
  </span>
  Eventos de adopción — Detalles
</h4>
                      <div class="fields-row">
                        <div class="field-col"><span class="field-label-row">Ha participado antes</span><span class="field-value">{{ getDE(voluntarioActivo).participadoAntes || '—' }}</span></div>
                        <div class="field-col"><span class="field-label-row">Horario disponible</span><span class="field-value">{{ getDE(voluntarioActivo).horario || '—' }}</span></div>
                        <div class="field-col"><span class="field-label-row">Transporte propio</span><span class="field-value">{{ getDE(voluntarioActivo).transportePropio || '—' }}</span></div>
                      </div>
                      <div class="info-subsection" v-if="getDE(voluntarioActivo).disponibilidad?.length">
                        <span class="field-label-row">Disponibilidad</span>
                        <div class="chips-row"><span v-for="b in getDE(voluntarioActivo).disponibilidad" :key="b" class="type-chip">{{ b }}</span></div>
                      </div>
                      <div class="info-subsection" v-if="getDE(voluntarioActivo).habilidades?.length">
                        <span class="field-label-row">Habilidades</span>
                        <div class="chips-row"><span v-for="b in getDE(voluntarioActivo).habilidades" :key="b" class="type-chip">{{ b }}</span></div>
                      </div>
                      <div class="info-subsection" v-if="getDE(voluntarioActivo).experienciaPublico">
                        <span class="field-label-row">Experiencia en atención al público</span>
                        <p class="info-subsection-text">{{ getDE(voluntarioActivo).experienciaPublico }}</p>
                      </div>
                    </div>

                    <div v-if="voluntarioActivo.solicitudVoluntario?.tipo === 'Transporte'" class="block">
                      <h4 class="block-title">
  <span class="block-title-icon">
    <i class='bx bx-car'></i>
  </span>
  Transporte — Detalles
</h4>
                      <div class="fields-row">
                        <div class="field-col"><span class="field-label-row">Tipo de vehículo</span><span class="field-value">{{ getDE(voluntarioActivo).tipoVehiculo || '—' }}</span></div>
                        <div class="field-col"><span class="field-label-row">Cobertura</span><span class="field-value">{{ getDE(voluntarioActivo).cobertura || '—' }}</span></div>
                        <div class="field-col"><span class="field-label-row">Licencia vigente</span><span class="field-value">{{ getDE(voluntarioActivo).licencia || '—' }}</span></div>
                      </div>
                      <div class="info-subsection" v-if="getDE(voluntarioActivo).disponibilidad?.length">
                        <span class="field-label-row">Disponibilidad</span>
                        <div class="chips-row"><span v-for="b in getDE(voluntarioActivo).disponibilidad" :key="b" class="type-chip">{{ b }}</span></div>
                      </div>
                      <div class="info-subsection" v-if="getDE(voluntarioActivo).puedeTransp?.length">
                        <span class="field-label-row">Puede transportar</span>
                        <div class="chips-row"><span v-for="b in getDE(voluntarioActivo).puedeTransp" :key="b" class="type-chip">{{ b }}</span></div>
                      </div>
                    </div>

                    <div v-if="voluntarioActivo.solicitudVoluntario?.tipo === 'Veterinaria'" class="block">
                      <h4 class="block-title">
  <span class="block-title-icon">
    <i class='bx bx-plus-medical'></i>
  </span>
  Veterinaria — Detalles
</h4>
                      <div class="fields-row">
                        <div class="field-col"><span class="field-label-row">Profesión</span><span class="field-value">{{ getDE(voluntarioActivo).profesion || '—' }}</span></div>
                        <div class="field-col"><span class="field-label-row">Nº colegiado</span><span class="field-value">{{ getDE(voluntarioActivo).colegiado || '—' }}</span></div>
                        <div class="field-col"><span class="field-label-row">Clínica</span><span class="field-value">{{ getDE(voluntarioActivo).clinica || '—' }}</span></div>
                      </div>
                      <div class="info-subsection" v-if="getDE(voluntarioActivo).especialidades?.length">
                        <span class="field-label-row">Especialidades</span>
                        <div class="chips-row"><span v-for="b in getDE(voluntarioActivo).especialidades" :key="b" class="type-chip">{{ b }}</span></div>
                      </div>
                      <div class="info-subsection" v-if="getDE(voluntarioActivo).disponibilidad?.length">
                        <span class="field-label-row">Disponibilidad</span>
                        <div class="chips-row"><span v-for="b in getDE(voluntarioActivo).disponibilidad" :key="b" class="type-chip">{{ b }}</span></div>
                      </div>
                    </div>

                    <div v-if="voluntarioActivo.solicitudVoluntario?.tipo === 'Redes sociales'" class="block">
                      <h4 class="block-title">
  <span class="block-title-icon">
    <i class='bx bx-share-alt'></i>
  </span>
  Redes sociales — Detalles
</h4>
                      <div class="fields-row">
                        <div class="field-col"><span class="field-label-row">Red principal</span><span class="field-value">{{ getDE(voluntarioActivo).red || '—' }}</span></div>
                        <div class="field-col"><span class="field-label-row">Horas semanales</span><span class="field-value">{{ getDE(voluntarioActivo).horasSemanales || '—' }}</span></div>
                        <div class="field-col" v-if="getDE(voluntarioActivo).portafolio"><span class="field-label-row">Portafolio</span><a :href="getDE(voluntarioActivo).portafolio" target="_blank" class="exp-link">{{ getDE(voluntarioActivo).portafolio }}</a></div>
                      </div>
                      <div class="info-subsection" v-if="getDE(voluntarioActivo).experiencia?.length">
                        <span class="field-label-row">Experiencia</span>
                        <div class="chips-row"><span v-for="b in getDE(voluntarioActivo).experiencia" :key="b" class="type-chip">{{ b }}</span></div>
                      </div>
                      <div class="info-subsection" v-if="getDE(voluntarioActivo).programas?.length">
                        <span class="field-label-row">Programas</span>
                        <div class="chips-row"><span v-for="b in getDE(voluntarioActivo).programas" :key="b" class="type-chip">{{ b }}</span></div>
                      </div>
                    </div>

                    <div v-if="voluntarioActivo.solicitudVoluntario?.tipo === 'Rescatista'" class="block">
                      <h4 class="block-title">
  <span class="block-title-icon">
    <i class='bx bx-user'></i>
  </span>
  Rescatista — Detalles
</h4>
                      <div class="fields-row">
                        <div class="field-col"><span class="field-label-row">Años de experiencia</span><span class="field-value">{{ getDE(voluntarioActivo).anosExp || '—' }}</span></div>
                        <div class="field-col"><span class="field-label-row">Rescates realizados</span><span class="field-value">{{ getDE(voluntarioActivo).cantRescates || '—' }}</span></div>
                        <div class="field-col"><span class="field-label-row">Disponibilidad</span><span class="field-value">{{ getDE(voluntarioActivo).disponibilidad || '—' }}</span></div>
                      </div>
                      <div class="fields-row" style="margin-top:14px">
                        <div class="field-col"><span class="field-label-row">Capacitación animal</span><span class="field-value">{{ getDE(voluntarioActivo).capacitacion || '—' }}</span></div>
                        <div class="field-col"><span class="field-label-row">Zona de cobertura</span><span class="field-value">{{ [getDE(voluntarioActivo).zonaProvincia, getDE(voluntarioActivo).zonaCanton].filter(Boolean).join(', ') || '—' }}</span></div>
                      </div>
                      <div class="info-subsection" v-if="getDE(voluntarioActivo).equipo?.length">
                        <span class="field-label-row">Equipo disponible</span>
                        <div class="chips-row"><span v-for="b in getDE(voluntarioActivo).equipo" :key="b" class="type-chip">{{ b }}</span></div>
                      </div>
                    </div>
                  </div>

                  <div class="block" style="margin-bottom:0;">
                    <h4 class="block-title">
                      <span class="block-title-icon">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"/><circle cx="12" cy="10" r="3"/></svg>
                      </span>
                      Ubicación
                    </h4>
                    <div class="list-col">
                      <div class="list-item">
                        <div class="list-icon"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/></svg></div>
                        <div class="list-text"><span class="list-label">Provincia</span><span class="list-value">{{ voluntarioActivo.solicitudVoluntario?.direccion?.provincia || '—' }}</span></div>
                      </div>
                      <div class="list-item">
                        <div class="list-icon"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="9"/></svg></div>
                        <div class="list-text"><span class="list-label">Cantón</span><span class="list-value">{{ voluntarioActivo.solicitudVoluntario?.direccion?.canton || '—' }}</span></div>
                      </div>
                      <div class="list-item">
                        <div class="list-icon"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="9"/><polyline points="12 7 12 12 16 14"/></svg></div>
                        <div class="list-text"><span class="list-label">Distrito</span><span class="list-value">{{ voluntarioActivo.solicitudVoluntario?.direccion?.distrito || '—' }}</span></div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Footer: acciones según estado -->
            <div v-if="voluntarioActivo.solicitudVoluntario?.estado === 'Pendiente'" class="footer footer--pendiente">
              <button class="btn-ghost-red" @click="modalVer = false">Cerrar expediente</button>
              <div class="footer-actions">
                <button class="btn btn--danger" @click="modalVer = false; pedirConfirmacion('rechazar', voluntarioActivo)">Rechazar</button>
                <button class="btn btn--primary" @click="modalVer = false; pedirConfirmacion('aprobar', voluntarioActivo)">Aprobar solicitud</button>
              </div>
            </div>
            <div v-else class="footer">
              <button class="btn-ghost-red" @click="modalVer = false">Cerrar expediente</button>
            </div>

          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- ══════════════════════════════════════
         MODAL — EDITAR VOLUNTARIO
         Mismo componente que el formulario de Mascotas
    ══════════════════════════════════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="modalEditar" class="modal-overlay" @click.self="modalEditar = false">
          <div class="modal-box modal-box--uniform">
            <button class="close-btn" @click="modalEditar = false">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>

            <div class="form-header">
              <p class="form-eyebrow">Editar registro</p>
              <h2 class="form-title">{{ formEditar.nombre || 'Editar voluntario' }}</h2>
              <p class="form-sub">Modifica los datos del voluntario</p>
            </div>

            <div class="uniform-scroll">
              <div class="form-body">

                <!-- Sección 1: Datos personales -->
                <div class="form-section">
                  <div class="form-section-label"><span class="form-num">1</span> Datos personales</div>
                  <div class="form-grid">
                    <div class="fg"><label>Nombre completo</label><input class="input" v-model="formEditar.nombre" placeholder="Nombre completo"></div>
                    <div class="fg"><label>Cédula</label><input class="input" v-model="formEditar.cedula" placeholder="1-2345-6789"></div>
                    <div class="fg"><label>Correo electrónico</label><input class="input" type="email" v-model="formEditar.correo" placeholder="correo@ejemplo.com"></div>
                    <div class="fg"><label>Teléfono</label><input class="input" v-model="formEditar.telefono" placeholder="+506 88888888"></div>
                  </div>
                </div>

                <!-- Sección 2: Ubicación -->
                <div class="form-section">
                  <div class="form-section-label"><span class="form-num">2</span> Ubicación</div>
                  <div class="form-grid">
                    <div class="fg">
                      <label>Provincia</label>
                      <select class="select" v-model="formEditar.provincia">
                        <option value="">Seleccione</option>
                        <option v-for="p in provincias" :key="p" :value="p">{{ p }}</option>
                      </select>
                    </div>
                    <div class="fg">
                      <label>Cantón</label>
                      <select class="select" v-model="formEditar.canton" :disabled="!formEditar.provincia">
                        <option value="">Seleccione</option>
                        <option v-for="c in cantonesEdit" :key="c" :value="c">{{ c }}</option>
                      </select>
                    </div>
                    <div class="fg">
                      <label>Distrito</label>
                      <select class="select" v-model="formEditar.distrito" :disabled="!formEditar.canton">
                        <option value="">Seleccione</option>
                        <option v-for="d in distritosEdit" :key="d" :value="d">{{ d }}</option>
                      </select>
                    </div>
                  </div>
                </div>

                <!-- Sección 3: Tipo -->
                <div class="form-section">
                  <div class="form-section-label"><span class="form-num">3</span> Tipo de voluntariado</div>
                  <div class="form-grid">
                    <div class="fg fg--span2">
                      <label>Tipo</label>
                      <select class="select" v-model="formEditar.tipo">
                        <option value="">Seleccionar tipo</option>
                        <option v-for="t in TIPOS" :key="t" :value="t">{{ t }}</option>
                      </select>
                    </div>
                  </div>
                </div>

                <!-- Sección 4: Información específica -->
                <template v-if="formEditar.tipo">
                  <div class="form-section">
                    <div class="form-section-label form-section-label--accent"><span class="form-num form-num--amber">4</span> Información específica — {{ formEditar.tipo }}</div>

                    <!-- CASA CUNA -->
                    <template v-if="formEditar.tipo === 'Casa cuna'">
                      <div class="form-grid">
                        <div class="fg"><label>Máximo de mascotas</label><input class="input" type="number" min="1" :value="formEditar.datosEspecificos.maxMascotas" @input="formEditar.datosEspecificos.maxMascotas = $event.target.value"></div>
                        <div class="fg">
                          <label>Tipo de vivienda</label>
                          <select class="select" v-model="formEditar.datosEspecificos.tipoVivienda"><option value="">Seleccione</option><option>Casa</option><option>Apartamento</option><option>Finca</option></select>
                        </div>
                        <div class="fg"><label>Patio cerrado</label><div class="radio-row"><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.patioCerrado" value="Sí"><span>Sí</span></label><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.patioCerrado" value="No"><span>No</span></label></div></div>
                        <div class="fg"><label>Otras mascotas</label><div class="radio-row"><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.otrasMascotas" value="Sí"><span>Sí</span></label><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.otrasMascotas" value="No"><span>No</span></label></div></div>
                        <div class="fg"><label>Niños en vivienda</label><div class="radio-row"><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.ninos" value="Sí"><span>Sí</span></label><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.ninos" value="No"><span>No</span></label></div></div>
                        <div class="fg"><label>Tiempo disponible</label><input class="input" v-model="formEditar.datosEspecificos.tiempoDisp" placeholder="Ej. 1 mes"></div>
                      </div>
                      <div class="fg" style="margin-top:14px"><label>Puede recibir</label><div class="check-wrap"><label v-for="op in ['Cachorros','Adultos','Adultos mayores','Casos médicos']" :key="op" class="c-opt" :class="{ checked: deIncludes('puedeRecibir', op) }"><input type="checkbox" :checked="deIncludes('puedeRecibir', op)" @change="toggleDE('puedeRecibir', op)">{{ op }}</label></div></div>
                      <div class="fg" style="margin-top:14px"><label>Comentarios</label><textarea class="textarea" v-model="formEditar.datosEspecificos.comentarios" placeholder="Comentarios..."></textarea></div>
                    </template>

                    <!-- EVENTOS DE ADOPCIÓN -->
                    <template v-if="formEditar.tipo === 'Eventos de adopción'">
                      <div class="form-grid">
                        <div class="fg"><label>Ha participado antes</label><div class="radio-row"><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.participadoAntes" value="Sí"><span>Sí</span></label><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.participadoAntes" value="No"><span>No</span></label></div></div>
                        <div class="fg"><label>Horario disponible</label><input class="input" v-model="formEditar.datosEspecificos.horario" placeholder="Ej. 8am – 2pm"></div>
                        <div class="fg"><label>Transporte propio</label><div class="radio-row"><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.transportePropio" value="Sí"><span>Sí</span></label><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.transportePropio" value="No"><span>No</span></label></div></div>
                      </div>
                      <div class="fg" style="margin-top:14px"><label>Disponibilidad</label><div class="check-wrap"><label v-for="op in ['Sábados','Domingos','Entre semana']" :key="op" class="c-opt" :class="{ checked: deIncludes('disponibilidad', op) }"><input type="checkbox" :checked="deIncludes('disponibilidad', op)" @change="toggleDE('disponibilidad', op)">{{ op }}</label></div></div>
                      <div class="fg" style="margin-top:14px"><label>Habilidades</label><div class="check-wrap"><label v-for="op in ['Atención al público','Organización','Fotografía','Manejo de mascotas']" :key="op" class="c-opt" :class="{ checked: deIncludes('habilidades', op) }"><input type="checkbox" :checked="deIncludes('habilidades', op)" @change="toggleDE('habilidades', op)">{{ op }}</label></div></div>
                      <div class="fg" style="margin-top:14px"><label>Experiencia en atención al público</label><textarea class="textarea" v-model="formEditar.datosEspecificos.experienciaPublico" placeholder="Describe la experiencia..."></textarea></div>
                    </template>

                    <!-- TRANSPORTE -->
                    <template v-if="formEditar.tipo === 'Transporte'">
                      <div class="form-grid">
                        <div class="fg">
                          <label>Tipo de vehículo</label>
                          <select class="select" v-model="formEditar.datosEspecificos.tipoVehiculo"><option value="">Seleccione</option><option>Carro</option><option>Moto</option><option>Pick-up</option><option>SUV</option></select>
                        </div>
                        <div class="fg">
                          <label>Cobertura</label>
                          <select class="select" v-model="formEditar.datosEspecificos.cobertura"><option value="">Seleccione</option><option>Cantón</option><option>Provincia</option><option>Todo el país</option></select>
                        </div>
                        <div class="fg"><label>Licencia vigente</label><div class="radio-row"><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.licencia" value="Sí"><span>Sí</span></label><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.licencia" value="No"><span>No</span></label></div></div>
                      </div>
                      <div class="fg" style="margin-top:14px"><label>Disponibilidad</label><div class="check-wrap"><label v-for="op in ['Mañanas','Tardes','Noches','Emergencias']" :key="op" class="c-opt" :class="{ checked: deIncludes('disponibilidad', op) }"><input type="checkbox" :checked="deIncludes('disponibilidad', op)" @change="toggleDE('disponibilidad', op)">{{ op }}</label></div></div>
                      <div class="fg" style="margin-top:14px"><label>Puede transportar</label><div class="check-wrap"><label v-for="op in ['Mascotas pequeñas','Mascotas medianas','Mascotas grandes','Traslados veterinarios']" :key="op" class="c-opt" :class="{ checked: deIncludes('puedeTransp', op) }"><input type="checkbox" :checked="deIncludes('puedeTransp', op)" @change="toggleDE('puedeTransp', op)">{{ op }}</label></div></div>
                    </template>

                    <!-- VETERINARIA -->
                    <template v-if="formEditar.tipo === 'Veterinaria'">
                      <div class="form-grid">
                        <div class="fg">
                          <label>Profesión</label>
                          <select class="select" v-model="formEditar.datosEspecificos.profesion"><option value="">Seleccione</option><option>Médico veterinario</option><option>Estudiante</option><option>Asistente veterinario</option></select>
                        </div>
                        <div class="fg"><label>Nº colegiado</label><input class="input" v-model="formEditar.datosEspecificos.colegiado" placeholder="Opcional"></div>
                        <div class="fg"><label>Clínica</label><input class="input" v-model="formEditar.datosEspecificos.clinica" placeholder="Opcional"></div>
                      </div>
                      <div class="fg" style="margin-top:14px"><label>Especialidades</label><div class="check-wrap"><label v-for="op in ['Medicina general','Cirugía','Emergencias','Rehabilitación','Dermatología']" :key="op" class="c-opt" :class="{ checked: deIncludes('especialidades', op) }"><input type="checkbox" :checked="deIncludes('especialidades', op)" @change="toggleDE('especialidades', op)">{{ op }}</label></div></div>
                      <div class="fg" style="margin-top:14px"><label>Disponibilidad</label><div class="check-wrap"><label v-for="op in ['Consultas','Esterilizaciones','Emergencias']" :key="op" class="c-opt" :class="{ checked: deIncludes('disponibilidad', op) }"><input type="checkbox" :checked="deIncludes('disponibilidad', op)" @change="toggleDE('disponibilidad', op)">{{ op }}</label></div></div>
                    </template>

                    <!-- REDES SOCIALES -->
                    <template v-if="formEditar.tipo === 'Redes sociales'">
                      <div class="form-grid">
                        <div class="fg">
                          <label>Red principal</label>
                          <select class="select" v-model="formEditar.datosEspecificos.red"><option value="">Seleccione</option><option>Instagram</option><option>Facebook</option><option>TikTok</option><option>X</option></select>
                        </div>
                        <div class="fg"><label>Horas semanales</label><input class="input" type="number" min="1" :value="formEditar.datosEspecificos.horasSemanales" @input="formEditar.datosEspecificos.horasSemanales = $event.target.value"></div>
                        <div class="fg fg--span2"><label>Portafolio / perfil</label><input class="input" type="url" v-model="formEditar.datosEspecificos.portafolio" placeholder="https://..."></div>
                      </div>
                      <div class="fg" style="margin-top:14px"><label>Experiencia</label><div class="check-wrap"><label v-for="op in ['Diseño gráfico','Fotografía','Video','Copywriting','Community Manager']" :key="op" class="c-opt" :class="{ checked: deIncludes('experiencia', op) }"><input type="checkbox" :checked="deIncludes('experiencia', op)" @change="toggleDE('experiencia', op)">{{ op }}</label></div></div>
                      <div class="fg" style="margin-top:14px"><label>Programas</label><div class="check-wrap"><label v-for="op in ['Canva','Photoshop','CapCut','Illustrator']" :key="op" class="c-opt" :class="{ checked: deIncludes('programas', op) }"><input type="checkbox" :checked="deIncludes('programas', op)" @change="toggleDE('programas', op)">{{ op }}</label></div></div>
                    </template>

                    <!-- RESCATISTA -->
                    <template v-if="formEditar.tipo === 'Rescatista'">
                      <div class="form-grid">
                        <div class="fg">
                          <label>Años de experiencia <span class="private-badge">Solo lectura</span></label>
                          <div class="readonly-field"><span class="readonly-value">{{ formEditar.datosEspecificos.anosExp || '—' }}</span></div>
                        </div>
                        <div class="fg">
                          <label>Rescates realizados <span class="private-badge">Solo lectura</span></label>
                          <div class="readonly-field"><span class="readonly-value">{{ formEditar.datosEspecificos.cantRescates || '—' }}</span></div>
                        </div>
                        <div class="fg">
                          <label>Disponibilidad</label>
                          <select class="select" v-model="formEditar.datosEspecificos.disponibilidad"><option value="">Seleccione</option><option>Emergencias 24/7</option><option>Solo fines de semana</option><option>Entre semana</option></select>
                        </div>
                        <div class="fg"><label>Capacitación animal</label><div class="radio-row"><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.capacitacion" value="Sí"><span>Sí</span></label><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.capacitacion" value="No"><span>No</span></label></div></div>
                      </div>
                      <div class="form-grid" style="margin-top:14px">
                        <div class="fg fg--span2">
                          <label>Zona — Provincia</label>
                          <select class="select" v-model="formEditar.datosEspecificos.zonaProvincia"><option value="">Seleccione</option><option v-for="p in provincias" :key="p" :value="p">{{ p }}</option></select>
                        </div>
                        <div class="fg fg--span2">
                          <label>Zona — Cantón</label>
                          <select class="select" v-model="formEditar.datosEspecificos.zonaCanton" :disabled="!formEditar.datosEspecificos.zonaProvincia"><option value="">Seleccione</option><option v-for="c in cantonesZonaEdit" :key="c" :value="c">{{ c }}</option></select>
                        </div>
                      </div>
                      <div class="fg" style="margin-top:14px"><label>Equipo disponible</label><div class="check-wrap"><label v-for="op in ['Transportadora','Correas','Jaulas trampa','Botiquín']" :key="op" class="c-opt" :class="{ checked: deIncludes('equipo', op) }"><input type="checkbox" :checked="deIncludes('equipo', op)" @change="toggleDE('equipo', op)">{{ op }}</label></div></div>
                    </template>
                  </div>
                </template>

              </div>
            </div>

            <div class="form-footer">
              <button class="btn-cancel" @click="modalEditar = false">Cancelar</button>
              <button class="btn-save" @click="guardarEdicion">
                <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
                <span>Guardar cambios</span>
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- ══════════════════════════════════════
         MODAL — CONFIRMACIÓN
         Mismo componente que Desactivar mascota
    ══════════════════════════════════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="modalConfirm" class="modal-overlay" @click.self="cancelarConfirmacion">
          <div class="modal-box modal-box--confirm">
            <button class="close-btn" @click="cancelarConfirmacion">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>
            <div class="confirm-header">
              <div class="confirm-icon">
                <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
              </div>
              <div>
                <p class="confirm-eyebrow">Confirmar acción</p>
                <h2 class="confirm-title">Voluntarios</h2>
              </div>
            </div>
            <div class="confirm-body">
              <div class="warn-box">
                <p v-html="mensajeConfirm"></p>
              </div>
            </div>
            <div class="confirm-footer">
              <button class="btn-cancel" @click="cancelarConfirmacion">Cancelar</button>
              <button class="btn-save" @click="confirmarAccion">Confirmar</button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

  </div>
</template>

<style scoped>
/* ── Variables (idénticas a Mascotas) ─────────────────────── */
.view-container {
  --verde:       #3A473C;
  --verde-sec:   #92A894;
  --fondo:       #F7F8F7;
  --blanco:      #FFFFFF;
  --texto:       #2B322C;
  --texto-sec:   #7A827B;
  --texto-ter:   #A2A9A3;
  --borde:       #E9ECE9;
  --borde-suave: #EFF2EF;
  --amarillo:    #F5B942;
  --verde-ok:    #4CAF6A;
  --rojo:        #C0392B;
  --rojo-bg:     #FBEDEC;
  --sombra-sm:   0 1px 2px rgba(58,71,60,.03);
  --sombra-md:   0 2px 4px rgba(58,71,60,.05), 0 14px 32px -14px rgba(58,71,60,.18);
  --btn-height:      33px;
  --btn-radius:      9px;
  --btn-pad-x:       13px;
  --btn-icon-size:   14px;
  --btn-icon-gap:    6px;
  --btn-font-size:   12.5px;
  --btn-font-weight: 600;
  --btn-transition:  0.16s ease;
  --select-arrow: url('data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="%237A827B" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>');
  --select-arrow-focus: url('data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="%233A473C" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>');
  background:
    radial-gradient(ellipse 800px 420px at 12% 0%, rgba(146,168,148,.07), transparent),
    var(--fondo);
  padding-bottom: 40px;
}

/* ── Sistema de botones (idéntico a Mascotas) ── */
.btn { display:inline-flex; align-items:center; justify-content:center; gap:var(--btn-icon-gap); height:var(--btn-height); padding:0 var(--btn-pad-x); border-radius:var(--btn-radius); border:1px solid transparent; font-family:inherit; font-size:var(--btn-font-size); font-weight:var(--btn-font-weight); line-height:1; white-space:nowrap; cursor:pointer; user-select:none; transition:background-color var(--btn-transition), border-color var(--btn-transition), color var(--btn-transition), box-shadow var(--btn-transition); }
.btn:active:not(:disabled) { transform:translateY(1px); }
.btn:focus-visible { outline:none; box-shadow:0 0 0 3px rgba(58,71,60,.16); }
.btn--primary { background:var(--verde); color:#fff; box-shadow:0 1px 2px rgba(58,71,60,.12), 0 4px 10px -4px rgba(58,71,60,.35); }
.btn--primary:hover:not(:disabled) { background:#465747; box-shadow:0 1px 2px rgba(58,71,60,.14), 0 6px 14px -4px rgba(58,71,60,.4); }
.btn--danger { background:var(--rojo-bg); color:var(--rojo); }
.btn--danger:hover:not(:disabled) { background:var(--rojo); color:#fff; }
.btn--ghost { background:var(--blanco); color:var(--texto-sec); border-color:var(--borde); }
.btn--ghost:hover:not(:disabled) { background:#FAFBFA; color:var(--texto); border-color:#D3D8D3; }
.btn--ghost-active { border-color:var(--verde-sec); color:var(--verde); }
.btn--ghost-active:hover:not(:disabled) { background:#F3F6F3; color:var(--verde); border-color:var(--verde-sec); }

/* ── Toast ── */
.don-toast { position:fixed; bottom:32px; right:32px; z-index:9999; display:flex; align-items:center; gap:10px; padding:14px 20px; border-radius:14px; font-size:14px; font-weight:600; box-shadow:0 8px 32px rgba(0,0,0,0.16); pointer-events:none; }
.don-toast.success { background:var(--verde); color:#fff; }
.don-toast.error { background:#c0392b; color:#fff; }
.don-toast-dot { width:8px; height:8px; border-radius:50%; background:rgba(255,255,255,0.5); flex-shrink:0; }
.toast-fade-enter-active, .toast-fade-leave-active { transition:all 0.25s ease; }
.toast-fade-enter-from, .toast-fade-leave-to { opacity:0; transform:translateY(10px); }

/* ── Encabezado ── */
.page-header { display:flex; justify-content:space-between; align-items:center; margin-bottom:24px; gap:16px; flex-wrap:wrap; }
.brand-row { display:flex; align-items:center; gap:12px; }
.brand-mark { width:38px; height:38px; min-width:38px; border-radius:11px; background:linear-gradient(150deg, var(--verde) 0%, #6E8870 100%); color:#fff; display:flex; align-items:center; justify-content:center; box-shadow:0 4px 10px -3px rgba(58,71,60,.45); }
.admin-page-title { font-size:22px; font-weight:700; color:var(--texto); letter-spacing:-0.4px; line-height:1.15; margin:0 0 2px; }
.admin-page-sub { font-size:12.5px; color:var(--texto-sec); font-weight:500; margin:0; }

/* ── Tarjetas resumen ── */
.don-summary { display:grid; grid-template-columns:repeat(5, 1fr); gap:12px; margin-bottom:20px; }
.don-card { background:var(--blanco); border-radius:16px; padding:16px 15px; border:1px solid var(--borde); box-shadow:var(--sombra-sm); display:flex; flex-direction:column; transition:box-shadow .18s ease, border-color .18s ease; }
.don-card:hover { border-color:#D7DED8; box-shadow:var(--sombra-md); }
.don-icon { width:32px; height:32px; border-radius:50%; display:flex; align-items:center; justify-content:center; margin-bottom:12px; border:1px solid transparent; }
.total-icon { background:#F2F3F2; border-color:#DFE2DF; color:#616861; }
.disponible-icon { background:#EDF6EF; border-color:#C9E4CE; color:#2E7D45; }
.proceso-icon { background:#FDF6E8; border-color:#F2E1B8; color:#A97A0C; }
.rechazada-icon { background:#FBEDEC; border-color:#F0CFC9; color:#B71C1C; }
.inactiva-icon { background:#F2F3F2; border-color:#DFE2DF; color:#7A827B; }
.don-value { font-size:21px; font-weight:700; color:var(--texto); line-height:1; letter-spacing:-0.4px; font-variant-numeric:tabular-nums; }
.don-label { font-size:10.5px; color:var(--texto-ter); font-weight:700; text-transform:uppercase; letter-spacing:0.5px; margin-top:7px; }
.don-desc { font-size:11px; color:var(--texto-sec); margin-top:2px; }

/* ── Panel de filtros ── */
.filtros-panel { background:var(--blanco); border-radius:16px; padding:18px 20px; margin-bottom:20px; border:1px solid var(--borde); box-shadow:var(--sombra-sm); display:flex; flex-direction:column; gap:16px; }
.filtros-row { display:flex; gap:24px; flex-wrap:wrap; }
.filtros-row--end { align-items:flex-end; justify-content:space-between; }
.filtros-divider { height:1px; background:var(--borde-suave); }
.filtro-group { display:flex; flex-direction:column; gap:7px; }
.filtro-group--tabs { flex:0 0 auto; }
.filtro-group--btn { flex:0 0 auto; }
.filtro-group--search { flex:1; min-width:220px; max-width:340px; }
.filtro-group--prov { flex:0 0 auto; min-width:170px; }
.filtro-label { font-size:10.5px; font-weight:700; color:var(--texto-ter); text-transform:uppercase; letter-spacing:0.6px; }
.tabs-wrap { display:flex; gap:3px; background:var(--fondo); border:1px solid var(--borde-suave); border-radius:10px; padding:3px; flex-wrap:wrap; }
.tab-btn { padding:7px 13px; border-radius:7px; border:none; background:transparent; color:var(--texto-sec); font-size:12px; font-weight:700; cursor:pointer; transition:all 0.18s; white-space:nowrap; font-family:inherit; }
.tab-btn:hover { color:var(--texto); }
.tab-btn.active { background:var(--blanco); color:var(--texto); box-shadow:var(--sombra-sm); border:1px solid var(--borde); }
.filtro-input-wrap { position:relative; display:flex; align-items:center; }
.filtro-input { width:100%; height:36px; padding:0 14px; border-radius:8px; border:1px solid var(--borde); background:var(--fondo); font-size:13px; color:var(--texto); font-family:inherit; outline:none; transition:border-color 0.18s, background 0.18s; box-sizing:border-box; }
.filtro-input:focus { border-color:var(--verde-sec); background:var(--blanco); }
.filtro-input--icon-left { padding-left:36px; }
.filtro-icon { position:absolute; display:flex; align-items:center; color:var(--texto-sec); }
.filtro-icon--left { left:12px; }
.select {
  height:36px; padding:0 32px 0 12px; border-radius:8px; border:1px solid var(--borde); background:var(--fondo);
  font-size:13px; color:var(--texto); font-family:inherit; outline:none; width:100%; box-sizing:border-box;
  background-image:var(--select-arrow); background-repeat:no-repeat; background-position:right 12px center;
  appearance:none; -webkit-appearance:none; -moz-appearance:none; cursor:pointer;
  transition:border-color .16s ease, box-shadow .16s ease, background .16s ease;
}
.select:hover { border-color:#D3D8D3; }
.select:focus { border-color:var(--verde-sec); background-color:var(--blanco); background-image:var(--select-arrow-focus); box-shadow:0 0 0 3px rgba(146,168,148,.2); }
.select:disabled { background-color:#F4F6F4; color:#9CA8A0; cursor:not-allowed; }

/* ── Estado vacío ── */
.empty-state { text-align:center; padding:72px 24px; background:var(--blanco); border-radius:16px; border:1px solid var(--borde); color:var(--verde-sec); display:flex; flex-direction:column; align-items:center; gap:10px; }
.empty-state svg { opacity:0.4; }
.empty-title { font-size:16px; font-weight:700; color:var(--texto); margin:0; }
.empty-sub { font-size:13px; color:var(--texto-sec); margin:0; }

/* ── Tabla ── */
.table-wrapper { background:var(--blanco); border-radius:16px; border:1px solid var(--borde); overflow:hidden; box-shadow:var(--sombra-sm); }
.table-scroll { overflow-x:auto; -webkit-overflow-scrolling:touch; }
.don-table { width:100%; border-collapse:collapse; min-width:760px; }
.don-table thead th { padding:12px 16px; text-align:left; color:var(--texto-ter); font-size:9.5px; font-weight:700; text-transform:uppercase; letter-spacing:0.6px; white-space:nowrap; }
.don-table tbody tr { border-top:1px solid var(--borde-suave); transition:background 0.15s; }
.don-table tbody tr:hover { background:#FAFBFA; }
.don-table tbody td { padding:12px 16px; vertical-align:middle; }
.row-inactive { opacity:0.5; }

.vol-cell { display:flex; align-items:center; gap:10px; }
.pet-avatar { width:38px; height:38px; border-radius:50%; overflow:hidden; flex-shrink:0; background:#F1F5F1; display:flex; align-items:center; justify-content:center; border:1px solid var(--borde); }
.pet-avatar-ini { font-size:14px; font-weight:700; color:#4E6E51; text-transform:uppercase; line-height:1; }
.vol-info { display:flex; flex-direction:column; gap:2px; min-width:0; }
.donor-name { display:block; font-size:12.5px; font-weight:700; color:var(--texto); line-height:1.3; }
.donor-mail { display:block; font-size:11px; color:var(--texto-sec); margin-top:2px; }
.fecha-text { font-size:12.5px; color:var(--texto-sec); }
.type-chip { display:inline-block; font-size:11.5px; font-weight:600; color:#4E6E51; background:#F1F5F1; padding:3px 10px; border-radius:7px; white-space:nowrap; }
.estado-badge { display:inline-block; font-size:10.5px; font-weight:700; padding:4px 11px; border-radius:20px; white-space:nowrap; }
.badge-pendiente { background:#FDF6E8; color:#96650A; }
.badge-aprobada { background:#EDF6EF; color:#2E7D32; }
.badge-rechazada { background:#FBEDEC; color:#B71C1C; }
.badge-inactiva { background:#F2F3F2; color:#7A827B; }
.badge-blue      { background:rgba(33,150,243,.13);  color:#1565C0; }
.badge-purple    { background:rgba(156,39,176,.13);  color:#7B1FA2; }
.badge-teal      { background:rgba(0,150,136,.13);   color:#00695C; }
.badge-crimson   { background:rgba(244,67,54,.13);   color:#C62828; }
.badge-sky       { background:rgba(2,185,250,.13);   color:#006E9B; }
.badge-gold      { background:rgba(255,193,7,.18);   color:#7A5200; }
.table-footer { padding:12px 16px; border-top:1px solid var(--borde-suave); font-size:12px; color:var(--texto-sec); font-weight:500; }

/* Botones de acción de la tabla — mismo componente icon-only que Mascotas */
.action-group { display:flex; gap:8px; align-items:center; }
.icon-only {
  width:38px; height:38px; border-radius:8px; border:1px solid var(--borde);
  background:var(--blanco); display:flex; align-items:center; justify-content:center;
  cursor:pointer; transition:background-color .16s ease, border-color .16s ease; position:relative;
}
.icon-only svg { width:16px; height:16px; }
.icon-only--ver { color:#3D453B; }
.icon-only--ver:hover { border-color:#C7D3C8; background:#FAFCFA; }
.icon-only--editar { color:#2E7D45; border-color:#CFE8D6; }
.icon-only--editar:hover { background:#F3FAF5; border-color:#2E7D45; }
.icon-only--inactivar { color:#C0392B; border-color:#F0CFC9; }
.icon-only--inactivar:hover { background:#FDF4F3; border-color:#C0392B; }
.icon-only--activar { color:#2E7D45; border-color:#CFE8D6; }
.icon-only--activar:hover { background:#F3FAF5; border-color:#2E7D45; }
.icon-only::before {
  content:attr(data-tooltip); position:absolute; bottom:calc(100% + 8px); left:50%;
  transform:translateX(-50%) translateY(4px); background:var(--verde); color:#fff;
  font-size:11px; font-weight:600; padding:5px 9px; border-radius:7px; white-space:nowrap;
  opacity:0; visibility:hidden; pointer-events:none; transition:opacity .15s ease, transform .15s ease; z-index:20;
}
.icon-only:hover::before { opacity:1; visibility:visible; transform:translateX(-50%) translateY(0); }

/* ══════════════════════════════════════════════
   MODAL BASE (idéntico a Mascotas)
   ══════════════════════════════════════════════ */
.modal-overlay { position:fixed; inset:0; background:rgba(0,0,0,0.35); backdrop-filter:blur(4px); z-index:1000; display:flex; align-items:center; justify-content:center; padding:24px; }
.modal-box { background:var(--blanco); border-radius:22px; box-shadow:var(--sombra-md); position:relative; }
.modal-box--confirm { width:420px; max-width:90vw; max-height:90vh; display:flex; flex-direction:column; overflow:hidden; border:1px solid var(--borde-suave); }
.modal-box--uniform {
  width:880px; max-width:92vw; height:660px; max-height:90vh;
  display:flex; flex-direction:column; overflow:hidden; border:1px solid var(--borde-suave);
}
.uniform-scroll { flex:1; min-height:0; overflow-y:auto; }
.close-btn {
  position:absolute; top:18px; right:18px; z-index:6;
  width:30px; height:30px; border-radius:9px; background:var(--fondo); border:1px solid var(--borde-suave);
  color:#8B928A; display:flex; align-items:center; justify-content:center; cursor:pointer;
  transition:background-color .16s ease, color .16s ease, border-color .16s ease;
}
.close-btn svg { width:16px; height:16px; }
.close-btn:hover { background:var(--verde); color:#fff; border-color:var(--verde); }
.close-btn--hero { background:var(--fondo); }
.close-btn--hero:hover { background:var(--verde); color:#fff; }

/* ── HERO (Ver voluntario) ── */
.hero {
  flex-shrink:0;
  background:linear-gradient(165deg, #FFFFFF 0%, #F7FAF7 55%, #F1F7F2 100%);
  border-bottom:1px solid var(--borde-suave);
  padding:28px 40px 24px;
  display:flex; align-items:center; gap:20px;
}
.hero-photo {
  width:60px; height:60px; border-radius:16px; flex-shrink:0; overflow:hidden;
  background:linear-gradient(150deg,#E7F0E8 0%,#DCEBDE 100%);
  border:1px solid var(--borde-suave);
  display:flex; align-items:center; justify-content:center;
  box-shadow:0 1px 2px rgba(58,71,60,.04), 0 10px 22px -12px rgba(58,71,60,.28);
}
.hero-photo-ini { font-size:20px; font-weight:700; color:#3E7A45; letter-spacing:-.3px; }
.hero-info { flex:1; min-width:0; display:flex; flex-direction:column; gap:8px; }
.hero-name-row { display:flex; align-items:center; gap:12px; flex-wrap:wrap; }
.hero-name { font-size:21px; font-weight:700; color:var(--texto); margin:0; letter-spacing:-.4px; }
.hero-meta { display:flex; align-items:center; gap:7px; flex-wrap:wrap; }
.hero-meta-chip {
  display:inline-flex; align-items:center; gap:6px; font-size:11.5px; font-weight:600; color:#4B5A4C;
  background:var(--blanco); border:1px solid var(--borde-suave); padding:4px 10px 4px 9px; border-radius:20px;
}
.hero-meta-chip svg { color:var(--verde-sec); flex-shrink:0; }
.badge-status-hero { padding:5px 12px !important; font-size:10.5px !important; }

/* ── BODY (Ver voluntario) ── */
.body { padding:18px 40px 10px; }
.grid-2col { display:grid; grid-template-columns:1.6fr 1fr; gap:14px; align-items:start; margin-bottom:0; }
.block { background:var(--blanco); border:1px solid var(--borde-suave); border-radius:14px; padding:18px 20px; margin-bottom:14px; box-shadow:var(--sombra-sm); }
.block:last-child { margin-bottom:0; }
.block-title { display:flex; align-items:center; gap:10px; font-size:12.5px; font-weight:700; color:var(--texto); text-transform:uppercase; letter-spacing:.4px; margin:0 0 14px; }
.block-title-icon { width:24px; height:24px; border-radius:50%; background:#F0F5F0; color:#4E7A54; display:flex; align-items:center; justify-content:center; flex-shrink:0; font-size:12px; }
.fields-row { display:grid; grid-template-columns:repeat(3, 1fr); gap:14px 16px; }
.field-col { display:flex; flex-direction:column; gap:5px; }
.field-label-row { font-size:10px; font-weight:700; color:var(--texto-ter); text-transform:uppercase; letter-spacing:.4px; }
.field-value { font-size:14px; font-weight:600; color:var(--texto); }
.info-subsection { margin-top:16px; padding-top:16px; border-top:1px solid var(--borde-suave); }
.info-subsection .field-label-row { display:block; margin-bottom:7px; }
.info-subsection-text { font-size:13px; font-weight:500; color:#4B534A; line-height:1.6; margin:0; }
.chips-row { display:flex; flex-wrap:wrap; gap:6px; margin-top:4px; }
.exp-link { font-size:13px; color:#3B82F6; text-decoration:none; word-break:break-all; }
.exp-link:hover { text-decoration:underline; }
.list-col { display:grid; grid-template-columns:1fr; gap:8px; }
.list-item { border:1px solid var(--borde-suave); border-radius:10px; padding:10px 12px; display:flex; align-items:center; gap:10px; }
.list-icon { width:30px; height:30px; border-radius:8px; flex-shrink:0; background:#EDF3EE; color:#3E7A45; display:flex; align-items:center; justify-content:center; }
.list-text { display:flex; flex-direction:column; gap:2px; min-width:0; }
.list-label { font-size:9.5px; font-weight:700; color:var(--texto-ter); text-transform:uppercase; letter-spacing:.4px; }
.list-value { font-size:12.5px; font-weight:700; color:var(--texto); white-space:nowrap; overflow:hidden; text-overflow:ellipsis; }

/* ── FOOTER (Ver voluntario) ── */
.footer { flex-shrink:0; display:flex; justify-content:flex-end; padding:14px 40px 18px; border-top:1px solid var(--borde-suave); }
.footer--pendiente { justify-content:space-between; flex-wrap:wrap; gap:10px; }
.footer-actions { display:flex; gap:10px; }
.btn-ghost-red { display:flex; align-items:center; gap:6px; height:29px; padding:0 12px; border-radius:8px; background:var(--blanco); border:1px solid var(--borde); color:var(--texto-sec); font-size:11.5px; font-weight:600; cursor:pointer; transition:background-color .16s ease, border-color .16s ease, color .16s ease; }
.btn-ghost-red:hover { background:#FDF4F3; border-color:#E8B9B2; color:var(--rojo); }

/* ══════════════════════════════════════════════
   FORMULARIO (Editar voluntario) — idéntico a Mascotas
   ══════════════════════════════════════════════ */
.form-header { flex-shrink:0; background:linear-gradient(165deg, #FFFFFF 0%, #F7FAF7 100%); padding:26px 40px 18px; border-bottom:1px solid var(--borde-suave); }
.form-eyebrow { font-size:11px; font-weight:700; color:#3E8B54; text-transform:uppercase; letter-spacing:.6px; margin:0 0 4px; }
.form-title { font-size:20px; font-weight:700; color:var(--texto); margin:0 0 4px; letter-spacing:-.3px; }
.form-sub { font-size:12.5px; color:var(--texto-sec); margin:0; }
.form-body { padding:20px 40px 8px; }
.form-section { margin-bottom:20px; }
.form-section-label { display:flex; align-items:center; gap:9px; font-size:12px; font-weight:700; color:var(--texto); text-transform:uppercase; letter-spacing:.5px; margin-bottom:12px; padding-bottom:9px; border-bottom:1px solid var(--borde-suave); }
.form-section-label--accent { color:#A97A0C; }
.form-num { width:20px; height:20px; border-radius:7px; background:var(--verde); color:#fff; font-size:10px; font-weight:700; display:flex; align-items:center; justify-content:center; flex-shrink:0; }
.form-num--amber { background:#F9C17A; color:#8A5A1E; }
.private-badge { font-size:10.5px; font-weight:600; color:#A97A0C; background:#FDF6E8; padding:2px 8px; border-radius:6px; text-transform:none; letter-spacing:0; margin-left:6px; }
.form-grid { display:grid; grid-template-columns:repeat(4,1fr); gap:13px 16px; }
.fg { display:flex; flex-direction:column; gap:6px; }
.fg--span2 { grid-column:span 2; }
.fg label { font-size:11.5px; font-weight:700; color:var(--texto-sec); }
.input {
  height:38px; padding:0 12px; border-radius:9px; border:1px solid var(--borde);
  background:var(--blanco); font-size:13px; color:var(--texto); font-family:inherit; outline:none; width:100%; box-sizing:border-box;
  transition:border-color .16s ease, box-shadow .16s ease;
}
.input:hover { border-color:#D3D8D3; }
.input:focus { border-color:var(--verde-sec); box-shadow:0 0 0 3px rgba(146,168,148,.2); }
.textarea { padding:10px 12px; border-radius:9px; border:1px solid var(--borde); background:var(--blanco); font-size:13px; color:var(--texto); font-family:inherit; outline:none; width:100%; box-sizing:border-box; min-height:72px; resize:vertical; line-height:1.5; transition:border-color .16s ease, box-shadow .16s ease; }
.textarea:hover { border-color:#D3D8D3; }
.textarea:focus { border-color:var(--verde-sec); box-shadow:0 0 0 3px rgba(146,168,148,.2); }
.form-footer { flex-shrink:0; display:flex; justify-content:flex-end; gap:10px; padding:14px 40px 18px; border-top:1px solid var(--borde-suave); }
.btn-cancel { height:38px; padding:0 16px; border-radius:9px; background:var(--blanco); border:1px solid var(--borde); color:var(--texto-sec); font-size:13px; font-weight:600; cursor:pointer; transition:background-color .16s ease, border-color .16s ease, color .16s ease; }
.btn-cancel:hover { background:#FAFBFA; color:var(--texto); border-color:#D3D8D3; }
.btn-save { display:flex; align-items:center; gap:7px; height:38px; padding:0 17px; border-radius:9px; background:var(--verde); border:none; color:#fff; font-size:13px; font-weight:600; cursor:pointer; box-shadow:0 1px 2px rgba(58,71,60,.12), 0 4px 10px -4px rgba(58,71,60,.35); transition:background-color .16s ease; }
.btn-save svg { width:14px; height:14px; }
.btn-save:hover { background:#465747; }

/* Checkboxes y radios — mismos tokens visuales que el resto del sistema */
.radio-row { display:flex; gap:16px; align-items:center; padding-top:4px; }
.r-opt { display:flex; align-items:center; gap:7px; font-size:13px; font-weight:600; color:var(--texto); cursor:pointer; }
.r-opt input[type="radio"] { accent-color:var(--verde-sec); width:15px; height:15px; cursor:pointer; }
.check-wrap { display:flex; flex-wrap:wrap; gap:8px; margin-top:2px; }
.c-opt {
  display:inline-flex; align-items:center; gap:7px;
  padding:7px 14px; border-radius:10px;
  background:var(--fondo); border:1px solid var(--borde);
  font-size:13px; font-weight:600; color:var(--texto-sec);
  cursor:pointer; transition:all .15s;
}
.c-opt input[type="checkbox"] { accent-color:var(--verde-sec); width:14px; height:14px; cursor:pointer; }
.c-opt.checked { background:#EDF6EF; border-color:var(--verde-sec); color:#2E7D45; }
.readonly-field {
  display:flex; align-items:center;
  background:#FDF6E8; border:1px solid rgba(169,122,12,.25);
  border-radius:9px; padding:0 12px; min-height:38px; cursor:not-allowed; box-sizing:border-box;
}
.readonly-value { font-size:14px; font-weight:700; color:#8A7A60; }

/* ══════════════════════════════════════════════
   CONFIRMACIÓN (idéntico a Desactivar mascota)
   ══════════════════════════════════════════════ */
.confirm-header { flex-shrink:0; padding:24px 32px 16px; display:flex; align-items:center; gap:14px; border-bottom:1px solid var(--borde); background:linear-gradient(165deg, #FFFFFF 0%, #FDF9F0 100%); }
.confirm-icon { width:42px; height:42px; border-radius:11px; flex-shrink:0; background:#FDF6E8; color:#96650A; display:flex; align-items:center; justify-content:center; }
.confirm-eyebrow { font-size:11px; font-weight:700; color:#96650A; text-transform:uppercase; letter-spacing:.6px; margin:0 0 4px; }
.confirm-title { font-size:17px; font-weight:700; color:var(--texto); margin:0; letter-spacing:-.3px; }
.confirm-body { padding:20px 32px; }
.warn-box { background:#FFFBF3; border-left:3px solid var(--amarillo); border-radius:0 10px 10px 0; padding:14px 16px; font-size:13px; color:var(--texto); line-height:1.7; }
.warn-box p { margin:0; }
.confirm-footer { flex-shrink:0; display:flex; justify-content:flex-end; gap:10px; padding:14px 32px 18px; border-top:1px solid var(--borde-suave); }

/* Animaciones */
.modal-fade-enter-active, .modal-fade-leave-active { transition:opacity 0.22s ease; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity:0; }

/* ── Responsive (mismos breakpoints que Mascotas) ── */
@media (max-width:1100px) { .don-summary { grid-template-columns:repeat(3, 1fr); } }
@media (max-width:900px) {
  .don-summary { grid-template-columns:repeat(2, 1fr); }
  .form-grid { grid-template-columns:repeat(2, 1fr); }
  .fg--span2 { grid-column:span 1; }
  .modal-box--uniform { width:94vw; height:88vh; }
  .grid-2col { grid-template-columns:1fr; }
  .fields-row { grid-template-columns:repeat(2, 1fr); }
}
@media (max-width:640px) {
  .page-header { flex-direction:column; align-items:flex-start; }
  .filtros-row { flex-direction:column; gap:14px; }
  .filtros-row--end { align-items:stretch; }
  .filtro-group { min-width:100%; }
  .filtro-group--search, .filtro-group--prov { max-width:none; }
  .don-summary { grid-template-columns:1fr 1fr; }
  .form-grid { grid-template-columns:1fr; }
  .fg--span2 { grid-column:1; }
  .don-table th:nth-child(4), .don-table td:nth-child(4) { display:none; }
  .modal-box--uniform { width:96vw; height:92vh; border-radius:18px; }
  .hero, .form-header, .form-body, .body, .footer, .form-footer, .confirm-header, .confirm-body, .confirm-footer { padding-left:20px; padding-right:20px; }
  .fields-row { grid-template-columns:1fr; }
  .footer--pendiente { flex-direction:column; align-items:stretch; }
  .footer-actions { flex-direction:column; }
}
@media (max-width:480px) { .don-summary { grid-template-columns:1fr; } }
</style>
<style>
/* ── Variables globales (para contenido teletransportado) ── */
:root {
  --verde: #3A473C; --verde-sec:#92A894; --fondo:#F7F8F7; --blanco:#FFFFFF;
  --texto:#2B322C; --texto-sec:#7A827B; --texto-ter:#A2A9A3;
  --borde:#E9ECE9; --borde-suave:#EFF2EF; --amarillo:#F5B942;
  --verde-ok:#4CAF6A; --rojo:#C0392B; --rojo-bg:#FBEDEC;
  --sombra-sm:0 1px 2px rgba(58,71,60,.03);
  --sombra-md:0 2px 4px rgba(58,71,60,.05), 0 14px 32px -14px rgba(58,71,60,.18);
}
</style>