<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { ubicacionesCR } from '../../data/ubicaciones'
import {
  getVolunteers,
  updateVolunteer,
  updateVolunteerStatus,
  parseApplicationDetails
} from '../../services/volunteerServices'

// ── Estado principal ──────────────────────────────────────────
const voluntarios  = ref([])
const cargando     = ref(false)
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

// ── Carga ─────────────────────────────────────────────────────
// El estado real usa 'Aprobado'/'Rechazado' (concuerda con "voluntario");
// esta vista ya usa las formas femeninas ('Aprobada'/'Rechazada') en toda
// la plantilla, así que se traduce una sola vez al adaptar cada fila.
const ESTADO_DB_A_VISTA = {
  Aprobado: 'Aprobada',
  Rechazado: 'Rechazada',
  Pendiente: 'Pendiente'
}

// Adapta el VolunteerDto plano de la API a la forma anidada que ya espera
// esta plantilla (solicitudVoluntario.*), para no tener que reescribir las
// ~1300 líneas de tabla/modales que ya funcionan bien visualmente.
function adaptarVoluntario(dto) {
  const estadoBase = ESTADO_DB_A_VISTA[dto.validationStatus] || dto.validationStatus
  // "Inactivar" en la BD real no borra el historial de aprobación (mantiene
  // validationStatus='Aprobado' y solo apaga active); aquí se deriva el mismo
  // 4to estado visual ('Inactivo') que usaba el mock para no tocar la plantilla.
  const estado = (!dto.active && dto.validationStatus === 'Aprobado') ? 'Inactivo' : estadoBase

  const direccion = { provincia: dto.city || '', canton: dto.town || '', distrito: dto.district || '' }

  return {
    id: dto.volunteerId,
    codigoVoluntario: dto.volunteerId,
    nombre: dto.fullName,
    correo: dto.email,
    cedula: dto.nationalId,
    telefono: dto.phonePrimary,
    direccion,
    solicitudVoluntario: {
      nombre: dto.fullName,
      cedula: dto.nationalId,
      correo: dto.email,
      telefono: dto.phonePrimary,
      direccion,
      tipo: dto.volunteerType,
      datosEspecificos: parseApplicationDetails(dto.applicationDetails),
      estado
    }
  }
}

async function cargarVoluntarios() {
  cargando.value = true
  try {
    const { data } = await getVolunteers()
    voluntarios.value = (data || []).map(adaptarVoluntario)
  } catch {
    voluntarios.value = []
    mostrarToast('No se pudieron cargar los voluntarios', 'error')
  } finally {
    cargando.value = false
  }
}

onMounted(cargarVoluntarios)

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

function confirmarAccion() {
  modalConfirm.value = false
  const { tipo, voluntario } = accionPendiente.value
  const acciones = {
    aprobar:   () => ejecutarAprobar(voluntario),
    rechazar:  () => ejecutarRechazar(voluntario),
    inactivar: () => ejecutarInactivar(voluntario),
    reactivar: () => ejecutarReactivar(voluntario),
  }
  acciones[tipo]?.()
  accionPendiente.value = null
}

function cancelarConfirmacion() {
  modalConfirm.value    = false
  accionPendiente.value = null
}

// ── Acciones de estado ────────────────────────────────────────
async function ejecutarAccion(usuario, accion, mensajeExito, mensajeError) {
  try {
    await updateVolunteerStatus(usuario.id, accion)
    await cargarVoluntarios()
    mostrarToast(mensajeExito)
  } catch {
    mostrarToast(mensajeError, 'error')
  }
}

function ejecutarAprobar(usuario) {
  return ejecutarAccion(usuario, 'Aprobar', 'Solicitud aprobada correctamente.', 'Error al aprobar la solicitud.')
}
function ejecutarRechazar(usuario) {
  return ejecutarAccion(usuario, 'Rechazar', 'Solicitud rechazada.', 'Error al rechazar la solicitud.')
}
function ejecutarInactivar(usuario) {
  return ejecutarAccion(usuario, 'Inactivar', 'Voluntario inactivado.', 'Error al inactivar el voluntario.')
}
function ejecutarReactivar(usuario) {
  return ejecutarAccion(usuario, 'Reactivar', 'Voluntario reactivado correctamente.', 'Error al reactivar el voluntario.')
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
  const f = voluntarioActivo.value ? formEditar.value : null
  if (!f) return

  const deOriginal = voluntarioActivo.value.solicitudVoluntario?.datosEspecificos || {}
  const deEditado  = { ...f.datosEspecificos }
  if (f.tipo === 'Rescatista') {
    deEditado.anosExp      = deOriginal.anosExp
    deEditado.cantRescates = deOriginal.cantRescates
  }

  try {
    // Nombre y correo viven en la cuenta de usuario (users/user_profiles), no en
    // la solicitud de voluntariado: la edición administrativa solo toca los
    // campos que realmente pertenecen a volunteers/user_contacts.
    await updateVolunteer(voluntarioActivo.value.id, {
      nationalId: f.cedula,
      volunteerType: f.tipo,
      applicationDetails: deEditado,
      phonePrimary: f.telefono,
      city: f.provincia,
      town: f.canton,
      district: f.distrito
    })
    await cargarVoluntarios()
    modalEditar.value = false
    mostrarToast('Información actualizada correctamente.')
  } catch {
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
  if (estado === 'Inactivo')  return 'badge-inactivo'
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
  }[tipo] || 'badge-neutral'
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

    <!-- ── Toast ── -->
    <Teleport to="body">
      <Transition name="toast-anim">
        <div v-if="toast.visible" class="vol-toast" :class="toast.tipo === 'error' ? 'vol-toast--error' : 'vol-toast--exito'">
          <svg v-if="toast.tipo === 'exito'" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
          <svg v-else width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
          {{ toast.texto }}
        </div>
      </Transition>
    </Teleport>

    <!-- CABECERA -->
    <header class="page-header">
      <div>
        <h1 class="admin-page-title">Voluntarios</h1>
        <p class="admin-page-sub">Gestión de solicitudes y voluntarios activos</p>
      </div>
    </header>

    <!-- TARJETAS RESUMEN -->
    <div class="don-summary">
      <div class="don-card kpi-total">
        <span class="don-label">Total solicitudes</span>
        <strong class="don-value">{{ totalVoluntarios }}</strong>
      </div>
      <div class="don-card kpi-pendientes">
        <span class="don-label">Pendientes</span>
        <strong class="don-value">{{ totalPendientes }}</strong>
      </div>
      <div class="don-card kpi-aprobados">
        <span class="don-label">Aprobados</span>
        <strong class="don-value">{{ totalAprobados }}</strong>
      </div>
      <div class="don-card kpi-rechazados">
        <span class="don-label">Rechazados</span>
        <strong class="don-value">{{ totalRechazados }}</strong>
      </div>
      <div class="don-card kpi-inactivos">
        <span class="don-label">Inactivos</span>
        <strong class="don-value">{{ totalInactivos }}</strong>
      </div>
    </div>

    <!-- FILTROS -->
    <div class="filtros-panel">

      <!-- Buscar voluntario -->
      <div class="filtro-group">
        <label class="filtro-label">Buscar voluntario</label>
        <div class="filtro-input-wrap">
          <input
            v-model="search"
            placeholder="Nombre del voluntario..."
            class="filtro-input"
          />
          <span class="filtro-icon filtro-icon--right">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
          </span>
        </div>
      </div>

      <!-- Tipo -->
      <div class="filtro-group">
        <label class="filtro-label">Tipo</label>
        <div class="filtro-input-wrap">
          <select v-model="filtroTipo" class="filtro-input filtro-select">
            <option value="Todos">Todos</option>
            <option v-for="t in TIPOS" :key="t" :value="t">{{ t }}</option>
          </select>
          <span class="filtro-icon filtro-icon--right filtro-icon--no-events">
            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
          </span>
        </div>
      </div>

      <!-- Estado -->
      <div class="filtro-group">
        <label class="filtro-label">Estado</label>
        <div class="filtro-input-wrap">
          <select v-model="filtroEstado" class="filtro-input filtro-select">
            <option value="Todos">Todos los estados</option>
            <option value="Pendiente">Pendientes</option>
            <option value="Aprobada">Aprobados</option>
            <option value="Rechazada">Rechazados</option>
            <option value="Inactivo">Inactivos</option>
          </select>
          <span class="filtro-icon filtro-icon--right filtro-icon--no-events">
            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
          </span>
        </div>
      </div>

      <!-- Provincia -->
      <div class="filtro-group">
        <label class="filtro-label">Provincia</label>
        <div class="filtro-input-wrap">
          <select v-model="filtroProv" class="filtro-input filtro-select">
            <option value="Todos">Todas</option>
            <option v-for="p in provinciasDisponibles" :key="p" :value="p">{{ p }}</option>
          </select>
          <span class="filtro-icon filtro-icon--right filtro-icon--no-events">
            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
          </span>
        </div>
      </div>

      <!-- Limpiar -->
      <div class="filtro-group filtro-group--btn">
        <button
          type="button"
          class="btn-limpiar"
          :class="{ 'btn-limpiar--activo': hayFiltros }"
          @click="limpiarFiltros"
        >
          Limpiar filtros
        </button>
      </div>

    </div>

    <!-- ESTADO VACÍO -->
    <div v-if="voluntariosFiltrados.length === 0" class="empty-state">
      <p class="empty-title">No hay voluntarios registrados</p>
      <p class="empty-sub">{{ hayFiltros ? 'Ajusta los filtros para ver resultados.' : 'Aún no hay solicitudes de voluntariado.' }}</p>
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
              <th>Cantón</th>
              <th>Estado</th>
              <th>Acción</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="v in voluntariosFiltrados" :key="v.id" class="don-row">

              <!-- Voluntario -->
              <td>
                <div class="vol-cell">
                  <div class="vol-avatar">
                    <span class="vol-avatar-ini">{{ iniciales(v.solicitudVoluntario?.nombre || v.nombre) }}</span>
                  </div>
                  <div class="vol-info">
                    <span class="donor-name">{{ v.solicitudVoluntario?.nombre || v.nombre }}</span>
                    <span class="vol-codigo">
                      {{ v.codigoVoluntario || (v.solicitudVoluntario?.estado === 'Pendiente' ? 'Sin asignar' : '—') }}
                    </span>
                  </div>
                </div>
              </td>

              <!-- Contacto -->
              <td>
                <span class="donor-name" style="font-weight:500">{{ v.solicitudVoluntario?.correo || v.correo || '—' }}</span>
                <span class="donor-mail-td">{{ v.solicitudVoluntario?.telefono || '—' }}</span>
              </td>

              <!-- Tipo -->
              <td>
                <span class="estado-badge" :class="tipoBadgeClass(v.solicitudVoluntario?.tipo)">
                  {{ v.solicitudVoluntario?.tipo || '—' }}
                </span>
              </td>

              <!-- Ubicación -->
              <td>
                <span class="fecha-text">{{ v.solicitudVoluntario?.direccion?.canton || '—' }}</span>
              </td>

              <!-- Estado -->
              <td>
                <span class="estado-badge" :class="estadoBadgeClass(v.solicitudVoluntario?.estado)">
                  {{ v.solicitudVoluntario?.estado }}
                </span>
              </td>

              <!-- Acciones -->
              <td>
                <!-- PENDIENTE -->
                <div v-if="v.solicitudVoluntario?.estado === 'Pendiente'" class="acciones-cell">
                  <button class="btn-ver" @click="abrirVer(v)">Ver</button>
                  <button class="btn-accion btn-accion--aprobar" @click="pedirConfirmacion('aprobar', v)">Aprobar</button>
                  <button class="btn-accion btn-accion--rechazar" @click="pedirConfirmacion('rechazar', v)">Rechazar</button>
                </div>
                <!-- APROBADA -->
                <div v-else-if="v.solicitudVoluntario?.estado === 'Aprobada'" class="acciones-cell">
                  <button class="btn-ver" @click="abrirVer(v)">Ver</button>
                  <button class="btn-accion btn-accion--editar" @click="abrirEditar(v)">Editar</button>
                  <button class="btn-accion btn-accion--inactivar" @click="pedirConfirmacion('inactivar', v)">Inactivar</button>
                </div>
                <!-- INACTIVO -->
                <div v-else-if="v.solicitudVoluntario?.estado === 'Inactivo'" class="acciones-cell">
                  <button class="btn-ver" @click="abrirVer(v)">Ver</button>
                  <button class="btn-accion btn-accion--editar" @click="abrirEditar(v)">Editar</button>
                  <button class="btn-accion btn-accion--aprobar" @click="pedirConfirmacion('reactivar', v)">Reactivar</button>
                </div>
                <!-- RECHAZADA -->
                <div v-else class="acciones-cell">
                  <button class="btn-ver" @click="abrirVer(v)">Ver</button>
                  <button class="btn-accion btn-accion--editar" @click="abrirEditar(v)">Editar</button>
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

    <!-- ═══════════ MODAL VER DETALLE ═══════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="modalVer && voluntarioActivo" class="modal-overlay" @click.self="modalVer = false">
          <div class="modal-box modal-box--lg">

            <button class="modal-close" @click="modalVer = false">✕</button>

            <div class="modal-header">
              <span class="modal-id">{{ voluntarioActivo.codigoVoluntario || (voluntarioActivo.solicitudVoluntario?.estado === 'Pendiente' ? 'Sin código' : '—') }}</span>
              <span class="estado-badge" :class="estadoBadgeClass(voluntarioActivo.solicitudVoluntario?.estado)">{{ voluntarioActivo.solicitudVoluntario?.estado }}</span>
              <span class="estado-badge" :class="tipoBadgeClass(voluntarioActivo.solicitudVoluntario?.tipo)">{{ voluntarioActivo.solicitudVoluntario?.tipo || '—' }}</span>
            </div>

            <!-- Hero voluntario -->
            <div class="modal-usuario-hero">
              <div class="modal-avatar">
                <span class="modal-avatar-ini">{{ iniciales(voluntarioActivo.solicitudVoluntario?.nombre || voluntarioActivo.nombre) }}</span>
              </div>
              <div>
                <p class="modal-usuario-nombre">{{ voluntarioActivo.solicitudVoluntario?.nombre || voluntarioActivo.nombre }}</p>
                <p class="modal-usuario-correo">{{ voluntarioActivo.solicitudVoluntario?.correo || voluntarioActivo.correo || '—' }}</p>
              </div>
            </div>

            <div class="sc-modal-body" v-if="voluntarioActivo">

              <!-- Información personal -->
              <div class="modal-section">
                <h4 class="modal-section-title">Información personal</h4>
                <div class="modal-grid">
                  <div class="modal-field"><span class="modal-field-label">Cédula</span><strong class="modal-field-value">{{ voluntarioActivo.solicitudVoluntario?.cedula || voluntarioActivo.cedula || '—' }}</strong></div>
                  <div class="modal-field"><span class="modal-field-label">Teléfono</span><strong class="modal-field-value">{{ voluntarioActivo.solicitudVoluntario?.telefono || '—' }}</strong></div>
                </div>
              </div>

              <!-- Ubicación -->
              <div class="modal-section">
                <h4 class="modal-section-title">Ubicación</h4>
                <div class="modal-grid modal-grid--3">
                  <div class="modal-field"><span class="modal-field-label">Provincia</span><strong class="modal-field-value">{{ voluntarioActivo.solicitudVoluntario?.direccion?.provincia || '—' }}</strong></div>
                  <div class="modal-field"><span class="modal-field-label">Cantón</span><strong class="modal-field-value">{{ voluntarioActivo.solicitudVoluntario?.direccion?.canton || '—' }}</strong></div>
                  <div class="modal-field"><span class="modal-field-label">Distrito</span><strong class="modal-field-value">{{ voluntarioActivo.solicitudVoluntario?.direccion?.distrito || '—' }}</strong></div>
                </div>
              </div>

              <!-- Casa cuna -->
              <div v-if="voluntarioActivo.solicitudVoluntario?.tipo === 'Casa cuna'" class="modal-section">
                <h4 class="modal-section-title modal-section-title--accent">Casa cuna — Detalles</h4>
                <div class="modal-grid">
                  <div class="modal-field"><span class="modal-field-label">Máx. mascotas</span><strong class="modal-field-value">{{ getDE(voluntarioActivo).maxMascotas || '—' }}</strong></div>
                  <div class="modal-field"><span class="modal-field-label">Tipo de vivienda</span><strong class="modal-field-value">{{ getDE(voluntarioActivo).tipoVivienda || '—' }}</strong></div>
                  <div class="modal-field"><span class="modal-field-label">Patio cerrado</span><strong class="modal-field-value">{{ getDE(voluntarioActivo).patioCerrado || '—' }}</strong></div>
                  <div class="modal-field"><span class="modal-field-label">Otras mascotas</span><strong class="modal-field-value">{{ getDE(voluntarioActivo).otrasMascotas || '—' }}</strong></div>
                  <div class="modal-field"><span class="modal-field-label">Niños en vivienda</span><strong class="modal-field-value">{{ getDE(voluntarioActivo).ninos || '—' }}</strong></div>
                  <div class="modal-field"><span class="modal-field-label">Tiempo disponible</span><strong class="modal-field-value">{{ getDE(voluntarioActivo).tiempoDisp || '—' }}</strong></div>
                </div>
                <div v-if="getDE(voluntarioActivo).puedeRecibir?.length" class="modal-field" style="margin-top:14px">
                  <span class="modal-field-label">Puede recibir</span>
                  <div class="info-badges-row"><span v-for="b in getDE(voluntarioActivo).puedeRecibir" :key="b" class="info-badge">{{ b }}</span></div>
                </div>
                <div v-if="getDE(voluntarioActivo).comentarios" class="modal-field" style="margin-top:14px">
                  <span class="modal-field-label">Comentarios</span>
                  <p class="modal-texto-bloque">{{ getDE(voluntarioActivo).comentarios }}</p>
                </div>
              </div>

              <!-- Eventos de adopción -->
              <div v-if="voluntarioActivo.solicitudVoluntario?.tipo === 'Eventos de adopción'" class="modal-section">
                <h4 class="modal-section-title modal-section-title--accent">Eventos de adopción — Detalles</h4>
                <div class="modal-grid">
                  <div class="modal-field"><span class="modal-field-label">Ha participado antes</span><strong class="modal-field-value">{{ getDE(voluntarioActivo).participadoAntes || '—' }}</strong></div>
                  <div class="modal-field"><span class="modal-field-label">Horario disponible</span><strong class="modal-field-value">{{ getDE(voluntarioActivo).horario || '—' }}</strong></div>
                  <div class="modal-field"><span class="modal-field-label">Transporte propio</span><strong class="modal-field-value">{{ getDE(voluntarioActivo).transportePropio || '—' }}</strong></div>
                </div>
                <div v-if="getDE(voluntarioActivo).disponibilidad?.length" class="modal-field" style="margin-top:14px">
                  <span class="modal-field-label">Disponibilidad</span>
                  <div class="info-badges-row"><span v-for="b in getDE(voluntarioActivo).disponibilidad" :key="b" class="info-badge">{{ b }}</span></div>
                </div>
                <div v-if="getDE(voluntarioActivo).habilidades?.length" class="modal-field" style="margin-top:14px">
                  <span class="modal-field-label">Habilidades</span>
                  <div class="info-badges-row"><span v-for="b in getDE(voluntarioActivo).habilidades" :key="b" class="info-badge">{{ b }}</span></div>
                </div>
                <div v-if="getDE(voluntarioActivo).experienciaPublico" class="modal-field" style="margin-top:14px">
                  <span class="modal-field-label">Experiencia en atención al público</span>
                  <p class="modal-texto-bloque">{{ getDE(voluntarioActivo).experienciaPublico }}</p>
                </div>
              </div>

              <!-- Transporte -->
              <div v-if="voluntarioActivo.solicitudVoluntario?.tipo === 'Transporte'" class="modal-section">
                <h4 class="modal-section-title modal-section-title--accent">Transporte — Detalles</h4>
                <div class="modal-grid modal-grid--3">
                  <div class="modal-field"><span class="modal-field-label">Tipo de vehículo</span><strong class="modal-field-value">{{ getDE(voluntarioActivo).tipoVehiculo || '—' }}</strong></div>
                  <div class="modal-field"><span class="modal-field-label">Cobertura</span><strong class="modal-field-value">{{ getDE(voluntarioActivo).cobertura || '—' }}</strong></div>
                  <div class="modal-field"><span class="modal-field-label">Licencia vigente</span><strong class="modal-field-value">{{ getDE(voluntarioActivo).licencia || '—' }}</strong></div>
                </div>
                <div v-if="getDE(voluntarioActivo).disponibilidad?.length" class="modal-field" style="margin-top:14px">
                  <span class="modal-field-label">Disponibilidad</span>
                  <div class="info-badges-row"><span v-for="b in getDE(voluntarioActivo).disponibilidad" :key="b" class="info-badge">{{ b }}</span></div>
                </div>
                <div v-if="getDE(voluntarioActivo).puedeTransp?.length" class="modal-field" style="margin-top:14px">
                  <span class="modal-field-label">Puede transportar</span>
                  <div class="info-badges-row"><span v-for="b in getDE(voluntarioActivo).puedeTransp" :key="b" class="info-badge">{{ b }}</span></div>
                </div>
              </div>

              <!-- Veterinaria -->
              <div v-if="voluntarioActivo.solicitudVoluntario?.tipo === 'Veterinaria'" class="modal-section">
                <h4 class="modal-section-title modal-section-title--accent">Veterinaria — Detalles</h4>
                <div class="modal-grid modal-grid--3">
                  <div class="modal-field"><span class="modal-field-label">Profesión</span><strong class="modal-field-value">{{ getDE(voluntarioActivo).profesion || '—' }}</strong></div>
                  <div class="modal-field"><span class="modal-field-label">Nº colegiado</span><strong class="modal-field-value">{{ getDE(voluntarioActivo).colegiado || '—' }}</strong></div>
                  <div class="modal-field"><span class="modal-field-label">Clínica</span><strong class="modal-field-value">{{ getDE(voluntarioActivo).clinica || '—' }}</strong></div>
                </div>
                <div v-if="getDE(voluntarioActivo).especialidades?.length" class="modal-field" style="margin-top:14px">
                  <span class="modal-field-label">Especialidades</span>
                  <div class="info-badges-row"><span v-for="b in getDE(voluntarioActivo).especialidades" :key="b" class="info-badge">{{ b }}</span></div>
                </div>
                <div v-if="getDE(voluntarioActivo).disponibilidad?.length" class="modal-field" style="margin-top:14px">
                  <span class="modal-field-label">Disponibilidad</span>
                  <div class="info-badges-row"><span v-for="b in getDE(voluntarioActivo).disponibilidad" :key="b" class="info-badge">{{ b }}</span></div>
                </div>
              </div>

              <!-- Redes sociales -->
              <div v-if="voluntarioActivo.solicitudVoluntario?.tipo === 'Redes sociales'" class="modal-section">
                <h4 class="modal-section-title modal-section-title--accent">Redes sociales — Detalles</h4>
                <div class="modal-grid modal-grid--3">
                  <div class="modal-field"><span class="modal-field-label">Red principal</span><strong class="modal-field-value">{{ getDE(voluntarioActivo).red || '—' }}</strong></div>
                  <div class="modal-field"><span class="modal-field-label">Horas semanales</span><strong class="modal-field-value">{{ getDE(voluntarioActivo).horasSemanales || '—' }}</strong></div>
                  <div class="modal-field" v-if="getDE(voluntarioActivo).portafolio"><span class="modal-field-label">Portafolio</span><a :href="getDE(voluntarioActivo).portafolio" target="_blank" class="exp-link">{{ getDE(voluntarioActivo).portafolio }}</a></div>
                </div>
                <div v-if="getDE(voluntarioActivo).experiencia?.length" class="modal-field" style="margin-top:14px">
                  <span class="modal-field-label">Experiencia</span>
                  <div class="info-badges-row"><span v-for="b in getDE(voluntarioActivo).experiencia" :key="b" class="info-badge">{{ b }}</span></div>
                </div>
                <div v-if="getDE(voluntarioActivo).programas?.length" class="modal-field" style="margin-top:14px">
                  <span class="modal-field-label">Programas</span>
                  <div class="info-badges-row"><span v-for="b in getDE(voluntarioActivo).programas" :key="b" class="info-badge">{{ b }}</span></div>
                </div>
              </div>

              <!-- Rescatista -->
              <div v-if="voluntarioActivo.solicitudVoluntario?.tipo === 'Rescatista'" class="modal-section">
                <h4 class="modal-section-title modal-section-title--accent">Rescatista — Detalles</h4>
                <div class="modal-grid">
                  <div class="modal-field"><span class="modal-field-label">Años de experiencia</span><strong class="modal-field-value">{{ getDE(voluntarioActivo).anosExp || '—' }}</strong></div>
                  <div class="modal-field"><span class="modal-field-label">Rescates realizados</span><strong class="modal-field-value">{{ getDE(voluntarioActivo).cantRescates || '—' }}</strong></div>
                  <div class="modal-field"><span class="modal-field-label">Disponibilidad</span><strong class="modal-field-value">{{ getDE(voluntarioActivo).disponibilidad || '—' }}</strong></div>
                  <div class="modal-field"><span class="modal-field-label">Capacitación animal</span><strong class="modal-field-value">{{ getDE(voluntarioActivo).capacitacion || '—' }}</strong></div>
                  <div class="modal-field"><span class="modal-field-label">Zona de cobertura</span><strong class="modal-field-value">{{ [getDE(voluntarioActivo).zonaProvincia, getDE(voluntarioActivo).zonaCanton].filter(Boolean).join(', ') || '—' }}</strong></div>
                </div>
                <div v-if="getDE(voluntarioActivo).equipo?.length" class="modal-field" style="margin-top:14px">
                  <span class="modal-field-label">Equipo disponible</span>
                  <div class="info-badges-row"><span v-for="b in getDE(voluntarioActivo).equipo" :key="b" class="info-badge">{{ b }}</span></div>
                </div>
              </div>

            </div>

            <!-- Footer: acciones según estado -->
            <div v-if="voluntarioActivo.solicitudVoluntario?.estado === 'Pendiente'" class="modal-acciones modal-acciones--pendiente">
              <button class="btn-cerrar-modal" @click="modalVer = false">Cerrar expediente</button>
              <div style="display:flex;gap:10px">
                <button class="btn-rechazar" style="flex:none;padding:13px 22px" @click="modalVer = false; pedirConfirmacion('rechazar', voluntarioActivo)">Rechazar</button>
                <button class="btn-aprobar" style="flex:none;padding:13px 22px" @click="modalVer = false; pedirConfirmacion('aprobar', voluntarioActivo)">Aprobar solicitud</button>
              </div>
            </div>
            <div v-else class="modal-acciones" style="justify-content:flex-end">
              <button class="btn-cerrar-modal" @click="modalVer = false">Cerrar expediente</button>
            </div>

          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- ═══════════ MODAL EDITAR ═══════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="modalEditar" class="modal-overlay" @click.self="modalEditar = false">
          <div class="modal-box modal-box--lg">

            <button class="modal-close" @click="modalEditar = false">✕</button>

            <div class="modal-header">
              <div class="modal-usuario-hero" style="margin-bottom:0;border:none;padding:0;background:transparent">
                <div class="modal-avatar">
                  <span class="modal-avatar-ini">{{ iniciales(formEditar.nombre) }}</span>
                </div>
                <div>
                  <p class="modal-usuario-nombre">{{ formEditar.nombre || 'Sin nombre' }}</p>
                  <p class="modal-usuario-correo">Editar información del voluntario</p>
                </div>
              </div>
            </div>

            <div class="sc-modal-body edit-body">

              <!-- Sección 1: Datos personales -->
              <div class="edit-section-label">
                <span class="edit-section-num">1</span>
                Datos personales
              </div>
              <div class="edit-grid edit-grid--4">
                <div class="edit-fg"><label class="edit-label">Nombre completo</label><input class="filtro-input" style="height:40px;padding:0 13px" v-model="formEditar.nombre" placeholder="Nombre completo" disabled title="El nombre pertenece a la cuenta del usuario; se edita desde Usuarios"></div>
                <div class="edit-fg"><label class="edit-label">Cédula</label><input class="filtro-input" style="height:40px;padding:0 13px" v-model="formEditar.cedula" placeholder="1-2345-6789"></div>
                <div class="edit-fg"><label class="edit-label">Correo electrónico</label><input class="filtro-input" style="height:40px;padding:0 13px" type="email" v-model="formEditar.correo" placeholder="correo@ejemplo.com" disabled title="El correo pertenece a la cuenta del usuario; se edita desde Usuarios"></div>
                <div class="edit-fg"><label class="edit-label">Teléfono</label><input class="filtro-input" style="height:40px;padding:0 13px" v-model="formEditar.telefono" placeholder="+506 88888888"></div>
              </div>

              <!-- Sección 2: Ubicación -->
              <div class="edit-section-label" style="margin-top:24px">
                <span class="edit-section-num">2</span>
                Ubicación
              </div>
              <div class="edit-grid edit-grid--3">
                <div class="edit-fg">
                  <label class="edit-label">Provincia</label>
                  <div class="filtro-input-wrap">
                    <select class="filtro-input filtro-select" v-model="formEditar.provincia">
                      <option value="">Seleccione</option>
                      <option v-for="p in provincias" :key="p" :value="p">{{ p }}</option>
                    </select>
                    <span class="filtro-icon filtro-icon--right filtro-icon--no-events"><svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg></span>
                  </div>
                </div>
                <div class="edit-fg">
                  <label class="edit-label">Cantón</label>
                  <div class="filtro-input-wrap">
                    <select class="filtro-input filtro-select" v-model="formEditar.canton" :disabled="!formEditar.provincia">
                      <option value="">Seleccione</option>
                      <option v-for="c in cantonesEdit" :key="c" :value="c">{{ c }}</option>
                    </select>
                    <span class="filtro-icon filtro-icon--right filtro-icon--no-events"><svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg></span>
                  </div>
                </div>
                <div class="edit-fg">
                  <label class="edit-label">Distrito</label>
                  <div class="filtro-input-wrap">
                    <select class="filtro-input filtro-select" v-model="formEditar.distrito" :disabled="!formEditar.canton">
                      <option value="">Seleccione</option>
                      <option v-for="d in distritosEdit" :key="d" :value="d">{{ d }}</option>
                    </select>
                    <span class="filtro-icon filtro-icon--right filtro-icon--no-events"><svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg></span>
                  </div>
                </div>
              </div>

              <!-- Sección 3: Tipo -->
              <div class="edit-section-label" style="margin-top:24px">
                <span class="edit-section-num">3</span>
                Tipo de voluntariado
              </div>
              <div class="edit-grid edit-grid--4">
                <div class="edit-fg" style="grid-column:span 2">
                  <label class="edit-label">Tipo</label>
                  <div class="filtro-input-wrap">
                    <select class="filtro-input filtro-select" v-model="formEditar.tipo">
                      <option value="">Seleccionar tipo</option>
                      <option>Casa cuna</option>
                      <option>Eventos de adopción</option>
                      <option>Transporte</option>
                      <option>Veterinaria</option>
                      <option>Redes sociales</option>
                      <option>Rescatista</option>
                    </select>
                    <span class="filtro-icon filtro-icon--right filtro-icon--no-events"><svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg></span>
                  </div>
                </div>
              </div>

              <!-- Sección 4: Información específica -->
              <template v-if="formEditar.tipo">
                <div class="edit-section-label edit-section-label--accent" style="margin-top:24px">
                  <span class="edit-section-num edit-section-num--amber">4</span>
                  Información específica — {{ formEditar.tipo }}
                </div>

                <!-- CASA CUNA -->
                <template v-if="formEditar.tipo === 'Casa cuna'">
                  <div class="edit-grid edit-grid--4">
                    <div class="edit-fg"><label class="edit-label">Máximo de mascotas</label><input class="filtro-input" style="height:40px;padding:0 13px" type="number" min="1" :value="formEditar.datosEspecificos.maxMascotas" @input="formEditar.datosEspecificos.maxMascotas = $event.target.value"></div>
                    <div class="edit-fg">
                      <label class="edit-label">Tipo de vivienda</label>
                      <div class="filtro-input-wrap"><select class="filtro-input filtro-select" v-model="formEditar.datosEspecificos.tipoVivienda"><option value="">Seleccione</option><option>Casa</option><option>Apartamento</option><option>Finca</option></select><span class="filtro-icon filtro-icon--right filtro-icon--no-events"><svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg></span></div>
                    </div>
                    <div class="edit-fg"><label class="edit-label">Patio cerrado</label><div class="radio-row"><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.patioCerrado" value="Sí"><span>Sí</span></label><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.patioCerrado" value="No"><span>No</span></label></div></div>
                    <div class="edit-fg"><label class="edit-label">Otras mascotas</label><div class="radio-row"><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.otrasMascotas" value="Sí"><span>Sí</span></label><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.otrasMascotas" value="No"><span>No</span></label></div></div>
                    <div class="edit-fg"><label class="edit-label">Niños en vivienda</label><div class="radio-row"><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.ninos" value="Sí"><span>Sí</span></label><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.ninos" value="No"><span>No</span></label></div></div>
                    <div class="edit-fg"><label class="edit-label">Tiempo disponible</label><input class="filtro-input" style="height:40px;padding:0 13px" v-model="formEditar.datosEspecificos.tiempoDisp" placeholder="Ej. 1 mes"></div>
                  </div>
                  <div class="edit-fg" style="margin-top:14px"><label class="edit-label">Puede recibir</label><div class="check-wrap"><label v-for="op in ['Cachorros','Adultos','Adultos mayores','Casos médicos']" :key="op" class="c-opt" :class="{ checked: deIncludes('puedeRecibir', op) }"><input type="checkbox" :checked="deIncludes('puedeRecibir', op)" @change="toggleDE('puedeRecibir', op)">{{ op }}</label></div></div>
                  <div class="edit-fg" style="margin-top:14px"><label class="edit-label">Comentarios</label><textarea class="edit-textarea" v-model="formEditar.datosEspecificos.comentarios" placeholder="Comentarios..."></textarea></div>
                </template>

                <!-- EVENTOS DE ADOPCIÓN -->
                <template v-if="formEditar.tipo === 'Eventos de adopción'">
                  <div class="edit-grid edit-grid--4">
                    <div class="edit-fg"><label class="edit-label">Ha participado antes</label><div class="radio-row"><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.participadoAntes" value="Sí"><span>Sí</span></label><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.participadoAntes" value="No"><span>No</span></label></div></div>
                    <div class="edit-fg"><label class="edit-label">Horario disponible</label><input class="filtro-input" style="height:40px;padding:0 13px" v-model="formEditar.datosEspecificos.horario" placeholder="Ej. 8am – 2pm"></div>
                    <div class="edit-fg"><label class="edit-label">Transporte propio</label><div class="radio-row"><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.transportePropio" value="Sí"><span>Sí</span></label><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.transportePropio" value="No"><span>No</span></label></div></div>
                  </div>
                  <div class="edit-fg" style="margin-top:14px"><label class="edit-label">Disponibilidad</label><div class="check-wrap"><label v-for="op in ['Sábados','Domingos','Entre semana']" :key="op" class="c-opt" :class="{ checked: deIncludes('disponibilidad', op) }"><input type="checkbox" :checked="deIncludes('disponibilidad', op)" @change="toggleDE('disponibilidad', op)">{{ op }}</label></div></div>
                  <div class="edit-fg" style="margin-top:14px"><label class="edit-label">Habilidades</label><div class="check-wrap"><label v-for="op in ['Atención al público','Organización','Fotografía','Manejo de mascotas']" :key="op" class="c-opt" :class="{ checked: deIncludes('habilidades', op) }"><input type="checkbox" :checked="deIncludes('habilidades', op)" @change="toggleDE('habilidades', op)">{{ op }}</label></div></div>
                  <div class="edit-fg" style="margin-top:14px"><label class="edit-label">Experiencia en atención al público</label><textarea class="edit-textarea" v-model="formEditar.datosEspecificos.experienciaPublico" placeholder="Describe la experiencia..."></textarea></div>
                </template>

                <!-- TRANSPORTE -->
                <template v-if="formEditar.tipo === 'Transporte'">
                  <div class="edit-grid edit-grid--4">
                    <div class="edit-fg">
                      <label class="edit-label">Tipo de vehículo</label>
                      <div class="filtro-input-wrap"><select class="filtro-input filtro-select" v-model="formEditar.datosEspecificos.tipoVehiculo"><option value="">Seleccione</option><option>Carro</option><option>Moto</option><option>Pick-up</option><option>SUV</option></select><span class="filtro-icon filtro-icon--right filtro-icon--no-events"><svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg></span></div>
                    </div>
                    <div class="edit-fg">
                      <label class="edit-label">Cobertura</label>
                      <div class="filtro-input-wrap"><select class="filtro-input filtro-select" v-model="formEditar.datosEspecificos.cobertura"><option value="">Seleccione</option><option>Cantón</option><option>Provincia</option><option>Todo el país</option></select><span class="filtro-icon filtro-icon--right filtro-icon--no-events"><svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg></span></div>
                    </div>
                    <div class="edit-fg"><label class="edit-label">Licencia vigente</label><div class="radio-row"><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.licencia" value="Sí"><span>Sí</span></label><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.licencia" value="No"><span>No</span></label></div></div>
                  </div>
                  <div class="edit-fg" style="margin-top:14px"><label class="edit-label">Disponibilidad</label><div class="check-wrap"><label v-for="op in ['Mañanas','Tardes','Noches','Emergencias']" :key="op" class="c-opt" :class="{ checked: deIncludes('disponibilidad', op) }"><input type="checkbox" :checked="deIncludes('disponibilidad', op)" @change="toggleDE('disponibilidad', op)">{{ op }}</label></div></div>
                  <div class="edit-fg" style="margin-top:14px"><label class="edit-label">Puede transportar</label><div class="check-wrap"><label v-for="op in ['Mascotas pequeñas','Mascotas medianas','Mascotas grandes','Traslados veterinarios']" :key="op" class="c-opt" :class="{ checked: deIncludes('puedeTransp', op) }"><input type="checkbox" :checked="deIncludes('puedeTransp', op)" @change="toggleDE('puedeTransp', op)">{{ op }}</label></div></div>
                </template>

                <!-- VETERINARIA -->
                <template v-if="formEditar.tipo === 'Veterinaria'">
                  <div class="edit-grid edit-grid--4">
                    <div class="edit-fg">
                      <label class="edit-label">Profesión</label>
                      <div class="filtro-input-wrap"><select class="filtro-input filtro-select" v-model="formEditar.datosEspecificos.profesion"><option value="">Seleccione</option><option>Médico veterinario</option><option>Estudiante</option><option>Asistente veterinario</option></select><span class="filtro-icon filtro-icon--right filtro-icon--no-events"><svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg></span></div>
                    </div>
                    <div class="edit-fg"><label class="edit-label">Nº colegiado</label><input class="filtro-input" style="height:40px;padding:0 13px" v-model="formEditar.datosEspecificos.colegiado" placeholder="Opcional"></div>
                    <div class="edit-fg"><label class="edit-label">Clínica</label><input class="filtro-input" style="height:40px;padding:0 13px" v-model="formEditar.datosEspecificos.clinica" placeholder="Opcional"></div>
                  </div>
                  <div class="edit-fg" style="margin-top:14px"><label class="edit-label">Especialidades</label><div class="check-wrap"><label v-for="op in ['Medicina general','Cirugía','Emergencias','Rehabilitación','Dermatología']" :key="op" class="c-opt" :class="{ checked: deIncludes('especialidades', op) }"><input type="checkbox" :checked="deIncludes('especialidades', op)" @change="toggleDE('especialidades', op)">{{ op }}</label></div></div>
                  <div class="edit-fg" style="margin-top:14px"><label class="edit-label">Disponibilidad</label><div class="check-wrap"><label v-for="op in ['Consultas','Esterilizaciones','Emergencias']" :key="op" class="c-opt" :class="{ checked: deIncludes('disponibilidad', op) }"><input type="checkbox" :checked="deIncludes('disponibilidad', op)" @change="toggleDE('disponibilidad', op)">{{ op }}</label></div></div>
                </template>

                <!-- REDES SOCIALES -->
                <template v-if="formEditar.tipo === 'Redes sociales'">
                  <div class="edit-grid edit-grid--4">
                    <div class="edit-fg">
                      <label class="edit-label">Red principal</label>
                      <div class="filtro-input-wrap"><select class="filtro-input filtro-select" v-model="formEditar.datosEspecificos.red"><option value="">Seleccione</option><option>Instagram</option><option>Facebook</option><option>TikTok</option><option>X</option></select><span class="filtro-icon filtro-icon--right filtro-icon--no-events"><svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg></span></div>
                    </div>
                    <div class="edit-fg"><label class="edit-label">Horas semanales</label><input class="filtro-input" style="height:40px;padding:0 13px" type="number" min="1" :value="formEditar.datosEspecificos.horasSemanales" @input="formEditar.datosEspecificos.horasSemanales = $event.target.value"></div>
                    <div class="edit-fg" style="grid-column:span 2"><label class="edit-label">Portafolio / perfil</label><input class="filtro-input" style="height:40px;padding:0 13px" type="url" v-model="formEditar.datosEspecificos.portafolio" placeholder="https://..."></div>
                  </div>
                  <div class="edit-fg" style="margin-top:14px"><label class="edit-label">Experiencia</label><div class="check-wrap"><label v-for="op in ['Diseño gráfico','Fotografía','Video','Copywriting','Community Manager']" :key="op" class="c-opt" :class="{ checked: deIncludes('experiencia', op) }"><input type="checkbox" :checked="deIncludes('experiencia', op)" @change="toggleDE('experiencia', op)">{{ op }}</label></div></div>
                  <div class="edit-fg" style="margin-top:14px"><label class="edit-label">Programas</label><div class="check-wrap"><label v-for="op in ['Canva','Photoshop','CapCut','Illustrator']" :key="op" class="c-opt" :class="{ checked: deIncludes('programas', op) }"><input type="checkbox" :checked="deIncludes('programas', op)" @change="toggleDE('programas', op)">{{ op }}</label></div></div>
                </template>

                <!-- RESCATISTA -->
                <template v-if="formEditar.tipo === 'Rescatista'">
                  <div class="edit-grid edit-grid--4">
                    <div class="edit-fg">
                      <label class="edit-label">Años de experiencia <span class="label-readonly-badge">Solo lectura</span></label>
                      <div class="readonly-field"><span class="readonly-value">{{ formEditar.datosEspecificos.anosExp || '—' }}</span></div>
                    </div>
                    <div class="edit-fg">
                      <label class="edit-label">Rescates realizados <span class="label-readonly-badge">Solo lectura</span></label>
                      <div class="readonly-field"><span class="readonly-value">{{ formEditar.datosEspecificos.cantRescates || '—' }}</span></div>
                    </div>
                    <div class="edit-fg">
                      <label class="edit-label">Disponibilidad</label>
                      <div class="filtro-input-wrap"><select class="filtro-input filtro-select" v-model="formEditar.datosEspecificos.disponibilidad"><option value="">Seleccione</option><option>Emergencias 24/7</option><option>Solo fines de semana</option><option>Entre semana</option></select><span class="filtro-icon filtro-icon--right filtro-icon--no-events"><svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg></span></div>
                    </div>
                    <div class="edit-fg"><label class="edit-label">Capacitación animal</label><div class="radio-row"><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.capacitacion" value="Sí"><span>Sí</span></label><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.capacitacion" value="No"><span>No</span></label></div></div>
                  </div>
                  <div class="edit-grid edit-grid--4" style="margin-top:14px">
                    <div class="edit-fg" style="grid-column:span 2">
                      <label class="edit-label">Zona — Provincia</label>
                      <div class="filtro-input-wrap"><select class="filtro-input filtro-select" v-model="formEditar.datosEspecificos.zonaProvincia"><option value="">Seleccione</option><option v-for="p in provincias" :key="p" :value="p">{{ p }}</option></select><span class="filtro-icon filtro-icon--right filtro-icon--no-events"><svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg></span></div>
                    </div>
                    <div class="edit-fg" style="grid-column:span 2">
                      <label class="edit-label">Zona — Cantón</label>
                      <div class="filtro-input-wrap"><select class="filtro-input filtro-select" v-model="formEditar.datosEspecificos.zonaCanton" :disabled="!formEditar.datosEspecificos.zonaProvincia"><option value="">Seleccione</option><option v-for="c in cantonesZonaEdit" :key="c" :value="c">{{ c }}</option></select><span class="filtro-icon filtro-icon--right filtro-icon--no-events"><svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg></span></div>
                    </div>
                  </div>
                  <div class="edit-fg" style="margin-top:14px"><label class="edit-label">Equipo disponible</label><div class="check-wrap"><label v-for="op in ['Transportadora','Correas','Jaulas trampa','Botiquín']" :key="op" class="c-opt" :class="{ checked: deIncludes('equipo', op) }"><input type="checkbox" :checked="deIncludes('equipo', op)" @change="toggleDE('equipo', op)">{{ op }}</label></div></div>
                </template>
              </template>

            </div>

            <div class="modal-acciones">
              <button class="btn-cerrar-modal" @click="modalEditar = false">Cancelar</button>
              <button class="btn-aprobar" style="flex:none;padding:13px 24px" @click="guardarEdicion">Guardar cambios</button>
            </div>

          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- ═══════════ MODAL CONFIRMACIÓN ═══════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="modalConfirm" class="modal-overlay" style="z-index:1100" @click.self="cancelarConfirmacion">
          <div class="modal-box modal-box--sm">

            <button class="modal-close" @click="cancelarConfirmacion">✕</button>

            <div class="confirm-icon-wrap">
              <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
            </div>

            <h3 class="confirm-title">Confirmar acción</h3>
            <p class="confirm-text" v-html="mensajeConfirm"></p>

            <div class="modal-acciones">
              <button class="btn-cerrar-modal" style="flex:none;padding:13px 20px" @click="cancelarConfirmacion">Cancelar</button>
              <button class="btn-aprobar" style="flex:1" @click="confirmarAccion">Confirmar</button>
            </div>

          </div>
        </div>
      </Transition>
    </Teleport>

  </div>
</template>

<style scoped>
/* ── Variables — en :global(:root) para que los modales
   Teleported a <body> también las hereden ─────────────────── */
:global(:root) {
  --verde:     #3A473C;
  --verde-sec: #92A894;
  --fondo:     #F7F8F7;
  --blanco:    #FFFFFF;
  --texto:     #2F352F;
  --texto-sec: #6C756D;
  --borde:     #E8ECE8;
  --amarillo:  #F5B942;
  --verde-ok:  #4CAF6A;
}

.view-container { background: transparent; }

/* ── Encabezado ─────────────────────────────────────────────── */
.page-header       { margin-bottom: 28px; }
.admin-page-title  { font-size: 28px; font-weight: 800; color: var(--verde); letter-spacing: -0.5px; line-height: 1.1; }
.admin-page-sub    { font-size: 14px; color: var(--texto-sec); margin-top: 4px; font-weight: 500; }

/* ── Tarjetas resumen ───────────────────────────────────────── */
.don-summary {
  display: flex;
  gap: 14px;
  margin-bottom: 20px;
  flex-wrap: wrap;
}

.don-card {
  flex: 1;
  min-width: 150px;
  background: var(--blanco);
  border-radius: 14px;
  padding: 20px;
  border: 1px solid var(--borde);
  border-top: 3px solid var(--borde);
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.kpi-total      { border-top-color: var(--verde); }
.kpi-pendientes { border-top-color: var(--amarillo); }
.kpi-aprobados  { border-top-color: var(--verde-ok); }
.kpi-rechazados { border-top-color: #E57373; }
.kpi-inactivos  { border-top-color: var(--texto-sec); }

.don-label { font-size: 11px; color: var(--texto-sec); font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; }
.don-value { font-size: 24px; font-weight: 800; color: var(--verde); line-height: 1; }

/* ── Panel de filtros ───────────────────────────────────────── */
.filtros-panel {
  background: var(--blanco);
  border-radius: 14px;
  padding: 20px;
  margin-bottom: 20px;
  border: 1px solid var(--borde);
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
  align-items: flex-end;
}

.filtro-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
  flex: 1;
  min-width: 130px;
}

.filtro-group--btn {
  flex: 0 0 auto;
  min-width: unset;
}

.filtro-label {
  font-size: 11px;
  font-weight: 700;
  color: var(--verde);
  text-transform: uppercase;
  letter-spacing: 0.5px;
  min-height: 16px;
  display: flex;
  align-items: flex-end;
}

.filtro-input-wrap {
  position: relative;
  display: flex;
  align-items: center;
}

.filtro-input {
  width: 100%;
  height: 38px;
  padding: 0 36px 0 12px;
  border-radius: 8px;
  border: 1.5px solid var(--borde);
  background: #FFFFFF;
  font-size: 13px;
  color: var(--texto);
  font-family: inherit;
  outline: none;
  transition: border-color 0.18s, background 0.18s;
  box-sizing: border-box;
}

.filtro-input:focus     { border-color: var(--verde-sec); background: var(--blanco); }
.filtro-input::placeholder { color: #9CA8A0; }
.filtro-input:disabled  { background: #F4F6F4; color: #9CA8A0; cursor: not-allowed; }

.filtro-select {
  appearance: none;
  -webkit-appearance: none;
  cursor: pointer;
}

.filtro-icon {
  position: absolute;
  display: flex;
  align-items: center;
  color: var(--texto-sec);
}

.filtro-icon--right     { right: 11px; }
.filtro-icon--no-events { pointer-events: none; }

.btn-limpiar {
  height: 38px;
  padding: 0 16px;
  border-radius: 8px;
  border: 1.5px solid var(--borde);
  background: transparent;
  color: var(--texto-sec);
  font-size: 12px;
  font-weight: 700;
  cursor: pointer;
  white-space: nowrap;
  transition: all 0.18s;
  font-family: inherit;
}

.btn-limpiar--activo { border-color: var(--verde); color: var(--verde); }
.btn-limpiar:hover   { background: var(--verde); color: var(--blanco); border-color: var(--verde); }

/* ── Estado vacío ───────────────────────────────────────────── */
.empty-state {
  text-align: center;
  padding: 72px 24px;
  background: var(--blanco);
  border-radius: 14px;
  border: 1px solid var(--borde);
}

.empty-title { font-size: 16px; font-weight: 700; color: var(--texto); margin-bottom: 6px; }
.empty-sub   { font-size: 13px; color: var(--texto-sec); }

/* ── Tabla ──────────────────────────────────────────────────── */
.table-wrapper {
  background: var(--blanco);
  border-radius: 14px;
  border: 1px solid var(--borde);
  overflow: hidden;
}

.table-scroll          { overflow-x: auto; -webkit-overflow-scrolling: touch; }

.don-table             { width: 100%; border-collapse: collapse; min-width: 760px; }
.don-table thead tr    { background: var(--verde); }
.don-table thead th    { padding: 13px 16px; text-align: left; color: var(--blanco); font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.6px; white-space: nowrap; }
.don-table tbody tr    { border-bottom: 1px solid var(--borde); transition: background 0.15s; }
.don-table tbody tr:last-child { border-bottom: none; }
.don-table tbody tr:hover      { background: #F4F6F4; }
.don-table tbody td    { padding: 13px 16px; vertical-align: middle; }

/* Celda voluntario */
.vol-cell {
  display: flex;
  align-items: center;
  gap: 10px;
}

.vol-avatar {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: #DDE6DE;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.vol-avatar-ini {
  font-size: 13px;
  font-weight: 800;
  color: #5A6E5C;
  text-transform: uppercase;
  line-height: 1;
}

.vol-info { display: flex; flex-direction: column; gap: 2px; min-width: 0; }
.vol-codigo { font-size: 11px; color: var(--texto-sec); font-family: monospace; }

.donor-name    { display: block; font-size: 13px; font-weight: 700; color: var(--texto); }
.donor-mail-td { display: block; font-size: 11px; color: var(--texto-sec); margin-top: 2px; }
.fecha-text    { font-size: 13px; color: var(--texto-sec); white-space: nowrap; }

/* Acciones en tabla */
.acciones-cell {
  display: flex;
  align-items: center;
  gap: 6px;
  flex-wrap: nowrap;
}

.table-footer { padding: 12px 16px; border-top: 1px solid var(--borde); font-size: 12px; color: var(--texto-sec); font-weight: 500; }

.btn-ver {
  padding: 6px 12px;
  border-radius: 7px;
  border: 1.5px solid var(--borde);
  background: var(--blanco);
  color: var(--verde);
  font-size: 12px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.18s;
  white-space: nowrap;
  font-family: inherit;
}
.btn-ver:hover { background: var(--verde); color: var(--blanco); border-color: var(--verde); }

.btn-accion {
  padding: 6px 12px;
  border-radius: 7px;
  border: none;
  font-size: 12px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.18s;
  white-space: nowrap;
  font-family: inherit;
}

.btn-accion--aprobar   { background: #E8F5E9; color: #2E7D32; }
.btn-accion--aprobar:hover   { background: #2E7D32; color: var(--blanco); }
.btn-accion--rechazar  { background: #FDECEA; color: #B71C1C; }
.btn-accion--rechazar:hover  { background: #B71C1C; color: var(--blanco); }
.btn-accion--editar    { background: rgba(33,150,243,.12); color: #1565C0; }
.btn-accion--editar:hover    { background: #1565C0; color: var(--blanco); }
.btn-accion--inactivar { background: #FFF3E0; color: #E65100; }
.btn-accion--inactivar:hover { background: #E65100; color: var(--blanco); }

/* ── Badges ─────────────────────────────────────────────────── */
.estado-badge    { display: inline-block; font-size: 11px; font-weight: 700; padding: 4px 12px; border-radius: 20px; white-space: nowrap; }
.badge-pendiente { background: #FFF7E0; color: #96650A; }
.badge-aprobada  { background: #E8F5E9; color: #2E7D32; }
.badge-rechazada { background: #FDECEA; color: #B71C1C; }
.badge-inactivo  { background: #FFF3E0; color: #E65100; }
.badge-blue      { background: rgba(33,150,243,.13);  color: #1565C0; }
.badge-purple    { background: rgba(156,39,176,.13);  color: #7B1FA2; }
.badge-teal      { background: rgba(0,150,136,.13);   color: #00695C; }
.badge-crimson   { background: rgba(244,67,54,.13);   color: #C62828; }
.badge-sky       { background: rgba(2,185,250,.13);   color: #006E9B; }
.badge-gold      { background: rgba(255,193,7,.18);   color: #7A5200; }
.badge-neutral   { background: #F4F6F4;               color: #6C756D; }

/* ── Toast ──────────────────────────────────────────────────── */
.vol-toast {
  position: fixed; bottom: 32px; right: 32px; z-index: 9999;
  display: flex; align-items: center; gap: 10px;
  padding: 14px 20px; border-radius: 14px;
  font-size: 14px; font-weight: 600;
  box-shadow: 0 8px 32px rgba(0,0,0,0.16); pointer-events: none;
}
.vol-toast--exito { background: var(--verde); color: var(--blanco); }
.vol-toast--error { background: #B71C1C; color: var(--blanco); }
.toast-anim-enter-active, .toast-anim-leave-active { transition: all 0.25s ease; }
.toast-anim-enter-from, .toast-anim-leave-to { opacity: 0; transform: translateY(10px); }

/* ── Modal base ─────────────────────────────────────────────── */
.modal-overlay {
  position: fixed; inset: 0;
  background: rgba(0,0,0,0.35);
  backdrop-filter: blur(4px);
  z-index: 1000;
  display: flex; align-items: center; justify-content: center;
  padding: 24px;
  overflow-y: auto;
}

.modal-box {
  background: #FFFFFF;
  border-radius: 20px;
  padding: 36px;
  width: 100%; max-width: 620px;
  max-height: 90vh; overflow-y: auto;
  position: relative;
  margin: auto;
}

.modal-box--lg { max-width: 780px; }
.modal-box--sm { max-width: 420px; text-align: center; }

.modal-close {
  position: absolute; top: 18px; right: 18px;
  width: 32px; height: 32px; border-radius: 50%;
  border: none; background: var(--fondo);
  color: var(--texto); font-size: 13px; font-weight: 700;
  cursor: pointer; display: flex; align-items: center; justify-content: center;
  transition: all 0.2s; font-family: inherit;
}
.modal-close:hover { background: var(--verde); color: var(--blanco); }

.modal-header { display: flex; align-items: center; gap: 10px; margin-bottom: 20px; flex-wrap: wrap; }

.modal-id {
  font-size: 13px; font-family: monospace;
  background: var(--fondo); border: 1px solid var(--borde);
  padding: 5px 11px; border-radius: 7px;
  color: var(--verde); font-weight: 700;
}

/* Hero voluntario en modal */
.modal-usuario-hero {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px 20px;
  background: var(--fondo);
  border-radius: 12px;
  margin-bottom: 24px;
  border: 1px solid var(--borde);
}

.modal-avatar {
  width: 52px;
  height: 52px;
  border-radius: 14px;
  background: #DDE6DE;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.modal-avatar-ini {
  font-size: 20px;
  font-weight: 800;
  color: #5A6E5C;
  text-transform: uppercase;
  line-height: 1;
}

.modal-usuario-nombre {
  font-size: 16px;
  font-weight: 800;
  color: var(--texto);
  margin: 0 0 2px;
}

.modal-usuario-correo {
  font-size: 13px;
  color: var(--texto-sec);
  margin: 0;
}

/* Secciones del modal */
.sc-modal-body { padding: 0; }

.modal-section       { margin-bottom: 24px; }
.modal-section-title {
  font-size: 11px; font-weight: 700; color: var(--texto-sec);
  text-transform: uppercase; letter-spacing: 0.5px;
  margin-bottom: 14px; padding-bottom: 10px;
  border-bottom: 1px solid var(--borde);
}
.modal-section-title--accent { color: #C08030; border-bottom-color: rgba(249,193,122,.4); }

.modal-grid   { display: grid; grid-template-columns: repeat(2,1fr); gap: 16px; }
.modal-grid--3 { grid-template-columns: repeat(3,1fr); }
.modal-field  { display: flex; flex-direction: column; gap: 4px; }
.modal-field-label { font-size: 10px; font-weight: 700; color: #9CA8A0; text-transform: uppercase; letter-spacing: 0.4px; }
.modal-field-value { font-size: 14px; color: var(--texto); font-weight: 600; word-break: break-word; }

.modal-texto-bloque {
  font-size: 14px; color: var(--texto); line-height: 1.7;
  background: var(--fondo); border-radius: 10px; padding: 12px 14px;
  margin: 4px 0 0;
}

.info-badges-row { display: flex; flex-wrap: wrap; gap: 6px; margin-top: 4px; }
.info-badge {
  display: inline-block; background: #EEF2EE; color: var(--verde);
  font-size: 12px; font-weight: 600; padding: 4px 12px;
  border-radius: 20px; border: 1px solid rgba(146,168,148,.25);
}

.exp-link { font-size: 13px; color: #3B82F6; text-decoration: none; word-break: break-all; }
.exp-link:hover { text-decoration: underline; }

/* Acciones del modal */
.modal-acciones {
  display: flex;
  align-items: center;
  gap: 10px;
  padding-top: 24px;
  border-top: 1px solid var(--borde);
  margin-top: 8px;
}

.modal-acciones--pendiente { justify-content: space-between; flex-wrap: wrap; }

.btn-aprobar {
  flex: 1; padding: 13px; border-radius: 10px; border: none;
  background: #E8F5E9; color: #2E7D32;
  font-size: 13px; font-weight: 700; cursor: pointer;
  transition: all 0.2s; font-family: inherit;
}
.btn-aprobar:hover { background: #2E7D32; color: var(--blanco); }

.btn-rechazar {
  flex: 1; padding: 13px; border-radius: 10px; border: none;
  background: #FDECEA; color: #B71C1C;
  font-size: 13px; font-weight: 700; cursor: pointer;
  transition: all 0.2s; font-family: inherit;
}
.btn-rechazar:hover { background: #B71C1C; color: var(--blanco); }

.btn-cerrar-modal {
  padding: 13px 20px; border-radius: 10px;
  border: 1.5px solid var(--borde);
  background: var(--blanco);
  color: var(--texto-sec);
  font-size: 13px; font-weight: 700; cursor: pointer;
  transition: all 0.18s; font-family: inherit;
}
.btn-cerrar-modal:hover { background: var(--fondo); }

/* Confirmación */
.confirm-icon-wrap {
  width: 60px; height: 60px;
  border-radius: 16px;
  background: #FFF7E0;
  display: flex; align-items: center; justify-content: center;
  margin: 0 auto 18px;
  color: #96650A;
}

.confirm-title {
  font-size: 18px; font-weight: 800; color: var(--texto);
  margin: 0 0 8px;
}

.confirm-text {
  font-size: 14px; color: var(--texto-sec);
  margin: 0 0 24px; line-height: 1.6;
}

/* ── Formulario de edición ───────────────────────────────────── */
.edit-body { padding: 0; }

.edit-section-label {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 13px;
  font-weight: 800;
  color: var(--verde);
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin-bottom: 14px;
}

.edit-section-label--accent { color: #C08030; }

.edit-section-num {
  width: 24px; height: 24px; border-radius: 7px;
  background: var(--verde); color: var(--blanco);
  font-size: 11px; font-weight: 800;
  display: flex; align-items: center; justify-content: center;
  flex-shrink: 0;
}

.edit-section-num--amber { background: #F9C17A; color: #8A5A1E; }

.edit-grid { display: grid; gap: 14px; }
.edit-grid--4 { grid-template-columns: repeat(4, 1fr); }
.edit-grid--3 { grid-template-columns: repeat(3, 1fr); }

.edit-fg { display: flex; flex-direction: column; gap: 6px; }
.edit-label {
  font-size: 11px;
  font-weight: 700;
  color: var(--verde);
  text-transform: uppercase;
  letter-spacing: 0.5px;
  display: flex;
  align-items: center;
  gap: 7px;
}

.edit-textarea {
  padding: 10px 13px;
  border: 1.5px solid var(--borde);
  border-radius: 8px;
  font-size: 13px;
  color: var(--texto);
  background: var(--fondo);
  outline: none;
  font-family: inherit;
  transition: border-color 0.18s, background 0.18s;
  width: 100%;
  box-sizing: border-box;
  min-height: 80px;
  resize: vertical;
  line-height: 1.5;
}
.edit-textarea:focus { border-color: var(--verde-sec); background: var(--blanco); }

/* Checkboxes y radios */
.radio-row { display: flex; gap: 16px; align-items: center; padding-top: 4px; }
.r-opt { display: flex; align-items: center; gap: 7px; font-size: 13px; font-weight: 600; color: var(--texto); cursor: pointer; }
.r-opt input[type="radio"] { accent-color: var(--verde-sec); width: 15px; height: 15px; cursor: pointer; }

.check-wrap { display: flex; flex-wrap: wrap; gap: 8px; margin-top: 2px; }
.c-opt {
  display: inline-flex; align-items: center; gap: 7px;
  padding: 7px 14px; border-radius: 10px;
  background: var(--fondo); border: 1.5px solid var(--borde);
  font-size: 13px; font-weight: 600; color: var(--texto-sec);
  cursor: pointer; transition: all .15s;
}
.c-opt input[type="checkbox"] { accent-color: var(--verde-sec); width: 14px; height: 14px; cursor: pointer; }
.c-opt.checked { background: #E7F1E8; border-color: var(--verde-sec); color: var(--verde); }

/* Campos de solo lectura */
.label-readonly-badge {
  display: inline-flex; align-items: center; gap: 4px;
  background: rgba(249,193,122,.18); color: #C08030;
  font-size: 10px; font-weight: 700; letter-spacing: 0.04em;
  padding: 2px 8px; border-radius: 99px;
  border: 1px solid rgba(249,193,122,.35); text-transform: uppercase;
}
.readonly-field {
  display: flex; align-items: center;
  background: #F7F5F0; border: 1.5px solid rgba(249,193,122,.35);
  border-radius: 8px; padding: 0 13px;
  min-height: 38px; cursor: not-allowed; box-sizing: border-box;
}
.readonly-value { font-size: 14px; font-weight: 700; color: #8A7A60; }

/* ── Animaciones ────────────────────────────────────────────── */
.modal-fade-enter-active, .modal-fade-leave-active { transition: opacity 0.22s ease; }
.modal-fade-enter-from, .modal-fade-leave-to       { opacity: 0; }

/* ── Responsive ─────────────────────────────────────────────── */
@media (max-width: 900px) {
  .don-summary   { display: grid; grid-template-columns: repeat(2,1fr); }
  .kpi-inactivos { grid-column: span 2; }
  .edit-grid--4  { grid-template-columns: repeat(2, 1fr); }
  .modal-grid--3 { grid-template-columns: repeat(2, 1fr); }
}

@media (max-width: 640px) {
  .filtros-panel     { flex-direction: column; }
  .filtro-group      { min-width: 100%; }
  .filtro-group--btn { width: 100%; }
  .btn-limpiar       { width: 100%; }
  .don-summary       { grid-template-columns: 1fr; }
  .kpi-inactivos     { grid-column: span 1; }
  .modal-box         { padding: 24px 20px; }
  .modal-grid        { grid-template-columns: 1fr; }
  .modal-grid--3     { grid-template-columns: 1fr; }
  .modal-acciones    { flex-direction: column; }
  .modal-acciones--pendiente > div { flex-direction: column; width: 100%; }
  .edit-grid--4      { grid-template-columns: 1fr; }
  .edit-grid--3      { grid-template-columns: 1fr; }
  .acciones-cell     { flex-direction: column; align-items: flex-start; }
}


/* ── MOBILE RESPONSIVE ── */
@media (max-width: 768px) {
  .don-summary {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 10px;
  }

  .kpi-inactivos { grid-column: span 2; }

  .filtros-panel {
    flex-direction: column;
    gap: 10px;
    padding: 14px;
  }

  .filtro-group,
  .filtro-group--btn {
    min-width: unset;
    width: 100%;
  }

  .btn-limpiar {
    width: 100%;
    justify-content: center;
  }

  .table-scroll {
    overflow-x: auto;
    -webkit-overflow-scrolling: touch;
  }

  .acciones-cell {
    flex-direction: column;
    align-items: flex-start;
    gap: 4px;
  }

  .btn-ver,
  .btn-accion {
    width: 100%;
    text-align: center;
    justify-content: center;
  }

  .modal-box--lg {
    max-width: calc(100vw - 24px);
    padding: 22px 14px;
    max-height: 95vh;
  }

  .modal-grid { grid-template-columns: 1fr; }
  .modal-grid--3 { grid-template-columns: 1fr 1fr; }

  .modal-acciones {
    flex-direction: column;
    gap: 8px;
  }

  .modal-acciones--pendiente {
    flex-direction: column;
  }

  .modal-acciones--pendiente > div {
    width: 100%;
    flex-direction: column;
  }

  .edit-grid--4 { grid-template-columns: repeat(2, 1fr); }
  .edit-grid--3 { grid-template-columns: 1fr 1fr; }

  .modal-usuario-hero {
    flex-direction: column;
    align-items: flex-start;
    gap: 10px;
  }

  .sc-modal-body { padding: 0; }

  .check-wrap { flex-wrap: wrap; }
}

@media (max-width: 480px) {
  .don-summary { grid-template-columns: 1fr; }
  .kpi-inactivos { grid-column: span 1; }

  .edit-grid--4 { grid-template-columns: 1fr; }
  .edit-grid--3 { grid-template-columns: 1fr; }

  .modal-grid--3 { grid-template-columns: 1fr; }

  .don-table th:nth-child(4),
  .don-table td:nth-child(4),
  .don-table th:nth-child(5),
  .don-table td:nth-child(5) { display: none; }
}


</style>