<script setup>
import { ref, computed, watch } from 'vue'
import { ubicacionesCR } from '../../data/ubicaciones'

// ── Estado principal ──────────────────────────────────────────
const voluntarios  = ref([])
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
function cargarVoluntarios() {
  const usuarios = JSON.parse(localStorage.getItem('anhelo_usuarios')) || []
  voluntarios.value = usuarios.filter(u => u.solicitudVoluntario)
}
cargarVoluntarios()

// ── Provincias disponibles en los datos ──────────────────────
const provinciasDisponibles = computed(() =>
  ubicacionesCR ? Object.keys(ubicacionesCR) : []
)

// ── Tipos disponibles ─────────────────────────────────────────
const TIPOS = ['Casa cuna','Eventos de adopción','Transporte','Veterinaria','Redes sociales','Rescatista']
const ESTADOS = ['Pendiente','Aprobada','Rechazada','Inactivo']

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

// ── Helpers localStorage ──────────────────────────────────────
function getUsuarios() {
  return JSON.parse(localStorage.getItem('anhelo_usuarios')) || []
}
function saveUsuarios(arr) {
  localStorage.setItem('anhelo_usuarios', JSON.stringify(arr))
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
function ejecutarAprobar(usuario) {
  try {
    const usuarios = getUsuarios()
    const i = usuarios.findIndex(u => u.id === usuario.id)

    if (i === -1) throw new Error()

    const totalVoluntarios = usuarios.filter(
      u => u.codigoVoluntario
    ).length

    if (!usuarios[i].codigoVoluntario) {
      usuarios[i].codigoVoluntario =
        'VOL-' + String(totalVoluntarios + 1).padStart(3, '0')
    }

    usuarios[i].solicitudVoluntario.estado = 'Aprobada'
    usuarios[i].rol = 'Voluntario'
    usuarios[i].tipoVoluntario = usuarios[i].solicitudVoluntario.tipo

    saveUsuarios(usuarios)
    cargarVoluntarios()

    mostrarToast('Solicitud aprobada correctamente.')
  } catch {
    mostrarToast('Error al aprobar la solicitud.', 'error')
  }
}

function ejecutarRechazar(usuario) {
  try {
    const usuarios = getUsuarios()
    const i = usuarios.findIndex(u => u.id === usuario.id)
    if (i === -1) throw new Error()
    usuarios[i].solicitudVoluntario.estado = 'Rechazada'
    usuarios[i].rol = 'Usuario'
    saveUsuarios(usuarios)
    cargarVoluntarios()
    mostrarToast('Solicitud rechazada.')
  } catch { mostrarToast('Error al rechazar la solicitud.', 'error') }
}

function ejecutarInactivar(usuario) {
  try {
    const usuarios = getUsuarios()
    const i = usuarios.findIndex(u => u.id === usuario.id)
    if (i === -1) throw new Error()
    usuarios[i].solicitudVoluntario.estado = 'Inactivo'
    saveUsuarios(usuarios)
    cargarVoluntarios()
    mostrarToast('Voluntario inactivado.')
  } catch { mostrarToast('Error al inactivar el voluntario.', 'error') }
}

function ejecutarReactivar(usuario) {
  try {
    const usuarios = getUsuarios()
    const i = usuarios.findIndex(u => u.id === usuario.id)
    if (i === -1) throw new Error()
    usuarios[i].solicitudVoluntario.estado = 'Aprobada'
    usuarios[i].rol = 'Voluntario'
    saveUsuarios(usuarios)
    cargarVoluntarios()
    mostrarToast('Voluntario reactivado correctamente.')
  } catch { mostrarToast('Error al reactivar el voluntario.', 'error') }
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

function guardarEdicion() {
  try {
    const usuarios = getUsuarios()
    const i = usuarios.findIndex(u => u.id === voluntarioActivo.value.id)
    if (i === -1) throw new Error()
    const f = formEditar.value
    const deOriginal = usuarios[i].solicitudVoluntario?.datosEspecificos || {}
    const deEditado  = { ...f.datosEspecificos }
    if (f.tipo === 'Rescatista') {
      deEditado.anosExp      = deOriginal.anosExp
      deEditado.cantRescates = deOriginal.cantRescates
    }
    Object.assign(usuarios[i].solicitudVoluntario, {
      nombre:    f.nombre,
      cedula:    f.cedula,
      correo:    f.correo,
      telefono:  f.telefono,
      direccion: { provincia: f.provincia, canton: f.canton, distrito: f.distrito },
      tipo:      f.tipo,
      datosEspecificos: deEditado
    })
    usuarios[i].nombre    = f.nombre
    usuarios[i].correo    = f.correo
    usuarios[i].cedula    = f.cedula
    usuarios[i].telefono  = f.telefono
    usuarios[i].direccion = { provincia: f.provincia, canton: f.canton, distrito: f.distrito }
    saveUsuarios(usuarios)
    cargarVoluntarios()
    modalEditar.value = false
    mostrarToast('Información actualizada correctamente.')
  } catch { mostrarToast('Error al guardar los cambios.', 'error') }
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
  return {
    Aprobada:  'badge--green',
    Rechazada: 'badge--red',
    Inactivo:  'badge--orange',
    Pendiente: 'badge--yellow',
  }[estado] || 'badge--yellow'
}
function estadoIcon(estado) {
  return { Aprobada: '✓', Rechazada: '✕', Inactivo: '◉', Pendiente: '⏳' }[estado] || '⏳'
}
function tipoBadgeClass(tipo) {
  return {
    'Casa cuna':           'badge--blue',
    'Eventos de adopción': 'badge--purple',
    'Transporte':          'badge--teal',
    'Veterinaria':         'badge--crimson',
    'Redes sociales':      'badge--sky',
    'Rescatista':          'badge--gold',
  }[tipo] || 'badge--neutral'
}

function getDE(v) { return v?.solicitudVoluntario?.datosEspecificos || {} }

function iniciales(nombre) {
  if (!nombre) return '?'
  return nombre.trim().split(' ').map(p => p[0]).slice(0, 2).join('').toUpperCase()
}
</script>

<template>
  <div class="sc-root">

    <!-- ── Toast ── -->
    <Teleport to="body">
      <Transition name="toast-anim">
        <div v-if="toast.visible" class="sc-toast" :class="toast.tipo === 'error' ? 'error' : 'success'">
          <svg v-if="toast.tipo === 'exito'" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
          <svg v-else width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
          {{ toast.texto }}
        </div>
      </Transition>
    </Teleport>

    <!-- ── Header ── -->
    <header class="sc-header">
      <div class="sc-header-left">
        <h1 class="sc-title">Voluntarios</h1>
        <p class="sc-sub">Gestión de solicitudes y voluntarios activos</p>
      </div>
    </header>

    <!-- ── Toolbar: tabs estilo Salud + filtros en misma fila ── -->
    <div class="sc-toolbar">


      <!-- Filtros  -->
      <div class="sc-filters">
        <!-- Búsqueda por nombre -->
        <div class="sc-search-wrap">
          <svg class="sc-search-icon" xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
          <input class="sc-search" v-model="search" placeholder="Nombre del voluntario..." />
        </div>
        <!-- Filtro tipo -->
        <div class="sc-select-wrap">
          <select class="sc-filter-select" v-model="filtroTipo">
            <option value="Todos">Tipo: Todos</option>
            <option v-for="t in TIPOS" :key="t" :value="t">{{ t }}</option>
          </select>
          <svg class="sc-select-icon" xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
        </div>

         <!-- Tabs de estado -->
      <div class="sc-select-wrap">
  <select class="sc-filter-select" v-model="filtroEstado">
    <option value="Todos">Todos los estados</option>
    <option value="Pendiente">Pendientes</option>
    <option value="Aprobada">Aprobados</option>
    <option value="Rechazada">Rechazados</option>
    <option value="Inactivo">Inactivos</option>
  </select>

  <svg
    class="sc-select-icon"
    xmlns="http://www.w3.org/2000/svg"
    width="12"
    height="12"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    stroke-width="2.5"
    stroke-linecap="round"
    stroke-linejoin="round"
  >
    <polyline points="6 9 12 15 18 9"/>
  </svg>
</div>

        <!-- Filtro provincia -->
        <div class="sc-select-wrap">
          <select class="sc-filter-select" v-model="filtroProv">
            <option value="Todos">Provincia: Todas</option>
            <option v-for="p in provinciasDisponibles" :key="p" :value="p">{{ p }}</option>
          </select>
          <svg class="sc-select-icon" xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
        </div>
        <!-- Limpiar -->
        <button v-if="hayFiltros" class="sc-clear" @click="limpiarFiltros">Limpiar</button>
      </div>

    </div>

    <!-- ── Tabla ── -->
    <div class="sc-table-wrap">
      <table class="sc-table">
        <thead>
          <tr>
            <th style="width:220px">Voluntario</th>
            <th style="width:200px">Contacto</th>
            <th style="width:160px">Tipo</th>
            <th style="width:160px">Cantón</th>
            <th style="width:120px">Estado</th>
            <th style="width:120px">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="v in voluntariosFiltrados" :key="v.id">

            <!-- Voluntario -->
            <td>
              <div class="sc-pet-cell">
                <div class="sc-avatar">
                  <span class="sc-avatar-ini">{{ iniciales(v.solicitudVoluntario?.nombre || v.nombre) }}</span>
                </div>
                <div class="sc-pet-info">
                  <span class="sc-pet-name">{{ v.solicitudVoluntario?.nombre || v.nombre }}</span>
                  <span class="sc-pet-id">
  {{
    v.codigoVoluntario ||
    (v.solicitudVoluntario?.estado === 'Pendiente'
      ? 'Sin asignar'
      : '—')
  }}
</span>
                </div>
              </div>
            </td>

            <!-- Contacto -->
            <td>
              <div class="sc-contact-stack">
                <span class="sc-td-main">{{ v.solicitudVoluntario?.correo || v.correo || '—' }}</span>
                <span class="sc-td-sec">{{ v.solicitudVoluntario?.telefono || '—' }}</span>
              </div>
            </td>

            <!-- Tipo -->
            <td>
              <span class="sc-badge" :class="tipoBadgeClass(v.solicitudVoluntario?.tipo)">
                {{ v.solicitudVoluntario?.tipo || '—' }}
              </span>
            </td>

            <!-- Ubicación -->
            <td class="sc-td-sec">
              {{ v.solicitudVoluntario?.direccion?.canton || '—' }}
            </td>

            <!-- Estado -->
            <td>
              <span class="sc-badge" :class="estadoBadgeClass(v.solicitudVoluntario?.estado)">
                {{ estadoIcon(v.solicitudVoluntario?.estado) }}
                {{ v.solicitudVoluntario?.estado }}
              </span>
            </td>

            <!-- Acciones -->
            <td>
              <!-- PENDIENTE: Ver + Aprobar + Rechazar -->
              <div v-if="v.solicitudVoluntario?.estado === 'Pendiente'" class="sc-actions">
  <button class="sc-btn-ver sc-btn-ver--neutral" @click="abrirVer(v)">
    <img src="/img-acciones/eye.png" class="action-icon" alt="Ver">
  </button>

  <button class="sc-btn-ver sc-btn-ver--green" @click="pedirConfirmacion('aprobar', v)">
    <img src="/img-acciones/check.png" class="action-icon" alt="Aprobar">
  </button>

  <button class="sc-btn-ver sc-btn-ver--red" @click="pedirConfirmacion('rechazar', v)">
    <img src="/img-acciones/close.png" class="action-icon" alt="Rechazar">
  </button>
</div>
              <!-- APROBADA: Ver + Editar + Inactivar -->
              <div v-else-if="v.solicitudVoluntario?.estado === 'Aprobada'" class="sc-actions">
  <button class="sc-btn-ver sc-btn-ver--neutral" @click="abrirVer(v)">
    <img src="/img-acciones/eye.png" class="action-icon" alt="Ver">
  </button>

  <button class="sc-btn-ver sc-btn-ver--blue" @click="abrirEditar(v)">
    <img src="/img-acciones/edit.png" class="action-icon" alt="Editar">
  </button>

  <button class="sc-btn-ver sc-btn-ver--orange" @click="pedirConfirmacion('inactivar', v)">
    <img src="/img-acciones/close.png" class="action-icon" alt="Inactivar">
  </button>
</div>
              <!-- INACTIVO: Ver + Editar + Reactivar -->
              <div v-else-if="v.solicitudVoluntario?.estado === 'Inactivo'" class="sc-actions">
  <button class="sc-btn-ver sc-btn-ver--neutral" @click="abrirVer(v)">
    <img src="/img-acciones/eye.png" class="action-icon" alt="Ver">
  </button>

  <button class="sc-btn-ver sc-btn-ver--blue" @click="abrirEditar(v)">
    <img src="/img-acciones/edit.png" class="action-icon" alt="Editar">
  </button>

  <button class="sc-btn-ver sc-btn-ver--green" @click="pedirConfirmacion('reactivar', v)">
    <img src="/img-acciones/check.png" class="action-icon" alt="Reactivar">
  </button>
</div>
              <!-- RECHAZADA: Ver + Editar -->
              <div v-else class="sc-actions">
  <button class="sc-btn-ver sc-btn-ver--neutral" @click="abrirVer(v)">
    <img src="/img-acciones/eye.png" class="action-icon" alt="Ver">
  </button>

  <button class="sc-btn-ver sc-btn-ver--blue" @click="abrirEditar(v)">
    <img src="/img-acciones/edit.png" class="action-icon" alt="Editar">
  </button>
</div>
            </td>

          </tr>

          <tr v-if="voluntariosFiltrados.length === 0">
            <td colspan="6" class="sc-empty">
              <div class="sc-empty-inner">
                <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
                <p>{{ hayFiltros ? 'Sin resultados para los filtros aplicados' : 'No hay registros para mostrar' }}</p>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- ══════════════════════════════════════════
         MODAL VER DETALLE
    ═══════════════════════════════════════════ -->
    <Teleport to="body">
      <Transition name="overlay-anim">
        <div v-if="modalVer" class="sc-overlay" @click.self="modalVer = false">
          <div class="sc-modal sc-modal--lg">

            <div class="exp-header">
              <div class="exp-avatar">{{ iniciales(voluntarioActivo?.solicitudVoluntario?.nombre || voluntarioActivo?.nombre) }}</div>
              <div class="exp-header-info">
                <div class="exp-name">{{ voluntarioActivo?.solicitudVoluntario?.nombre || voluntarioActivo?.nombre }}</div>
                <div class="exp-meta">
                  <span class="sc-badge" :class="tipoBadgeClass(voluntarioActivo?.solicitudVoluntario?.tipo)">{{ voluntarioActivo?.solicitudVoluntario?.tipo || '—' }}</span>
                  <span class="sc-badge" :class="estadoBadgeClass(voluntarioActivo?.solicitudVoluntario?.estado)">{{ estadoIcon(voluntarioActivo?.solicitudVoluntario?.estado) }} {{ voluntarioActivo?.solicitudVoluntario?.estado }}</span>
                </div>
              </div>
              <button class="sc-modal-close" @click="modalVer = false">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
              </button>
            </div>

            <div class="sc-modal-body exp-body" v-if="voluntarioActivo">
              <!-- Información personal -->
              <div class="exp-section">
                <div class="exp-section-title"><span class="exp-section-dot"></span>Información personal</div>
                <div class="exp-grid">
                  <div class="exp-field"><span class="exp-label">Nombre completo</span><span class="exp-value fw">{{ voluntarioActivo.solicitudVoluntario?.nombre || voluntarioActivo.nombre || '—' }}</span></div>
                  <div class="exp-field"><span class="exp-label">Cédula</span><span class="exp-value">{{ voluntarioActivo.solicitudVoluntario?.cedula || voluntarioActivo.cedula || '—' }}</span></div>
                  <div class="exp-field"><span class="exp-label">Correo electrónico</span><span class="exp-value">{{ voluntarioActivo.solicitudVoluntario?.correo || voluntarioActivo.correo || '—' }}</span></div>
                  <div class="exp-field"><span class="exp-label">Teléfono</span><span class="exp-value">{{ voluntarioActivo.solicitudVoluntario?.telefono || '—' }}</span></div>
                </div>
              </div>
              <!-- Ubicación -->
              <div class="exp-section">
                <div class="exp-section-title"><span class="exp-section-dot"></span>Ubicación</div>
                <div class="exp-grid cols-3">
                  <div class="exp-field"><span class="exp-label">Provincia</span><span class="exp-value">{{ voluntarioActivo.solicitudVoluntario?.direccion?.provincia || '—' }}</span></div>
                  <div class="exp-field"><span class="exp-label">Cantón</span><span class="exp-value">{{ voluntarioActivo.solicitudVoluntario?.direccion?.canton || '—' }}</span></div>
                  <div class="exp-field"><span class="exp-label">Distrito</span><span class="exp-value">{{ voluntarioActivo.solicitudVoluntario?.direccion?.distrito || '—' }}</span></div>
                </div>
              </div>
              <!-- Casa cuna -->
              <div v-if="voluntarioActivo.solicitudVoluntario?.tipo === 'Casa cuna'" class="exp-section">
                <div class="exp-section-title accent-orange"><span class="exp-section-dot orange"></span>Casa cuna — Detalles</div>
                <div class="exp-grid">
                  <div class="exp-field"><span class="exp-label">Máximo de mascotas</span><span class="exp-value fw">{{ getDE(voluntarioActivo).maxMascotas || '—' }}</span></div>
                  <div class="exp-field"><span class="exp-label">Tipo de vivienda</span><span class="exp-value">{{ getDE(voluntarioActivo).tipoVivienda || '—' }}</span></div>
                  <div class="exp-field"><span class="exp-label">Patio cerrado</span><span class="exp-value">{{ getDE(voluntarioActivo).patioCerrado || '—' }}</span></div>
                  <div class="exp-field"><span class="exp-label">Otras mascotas</span><span class="exp-value">{{ getDE(voluntarioActivo).otrasMascotas || '—' }}</span></div>
                  <div class="exp-field"><span class="exp-label">Niños en vivienda</span><span class="exp-value">{{ getDE(voluntarioActivo).ninos || '—' }}</span></div>
                  <div class="exp-field"><span class="exp-label">Tiempo disponible</span><span class="exp-value">{{ getDE(voluntarioActivo).tiempoDisp || '—' }}</span></div>
                </div>
                <div class="exp-field mt-12" v-if="getDE(voluntarioActivo).puedeRecibir?.length"><span class="exp-label">Puede recibir</span><div class="badges-row"><span v-for="b in getDE(voluntarioActivo).puedeRecibir" :key="b" class="info-badge">{{ b }}</span></div></div>
                <div class="exp-field mt-12" v-if="getDE(voluntarioActivo).comentarios"><span class="exp-label">Comentarios adicionales</span><p class="exp-text-block">{{ getDE(voluntarioActivo).comentarios }}</p></div>
              </div>
              <!-- Eventos de adopción -->
              <div v-if="voluntarioActivo.solicitudVoluntario?.tipo === 'Eventos de adopción'" class="exp-section">
                <div class="exp-section-title accent-orange"><span class="exp-section-dot orange"></span>Eventos de adopción — Detalles</div>
                <div class="exp-grid">
                  <div class="exp-field"><span class="exp-label">Ha participado antes</span><span class="exp-value">{{ getDE(voluntarioActivo).participadoAntes || '—' }}</span></div>
                  <div class="exp-field"><span class="exp-label">Horario disponible</span><span class="exp-value">{{ getDE(voluntarioActivo).horario || '—' }}</span></div>
                  <div class="exp-field"><span class="exp-label">Transporte propio</span><span class="exp-value">{{ getDE(voluntarioActivo).transportePropio || '—' }}</span></div>
                </div>
                <div class="exp-field mt-12" v-if="getDE(voluntarioActivo).experienciaPublico"><span class="exp-label">Experiencia en atención al público</span><p class="exp-text-block">{{ getDE(voluntarioActivo).experienciaPublico }}</p></div>
                <div class="exp-field mt-12" v-if="getDE(voluntarioActivo).disponibilidad?.length"><span class="exp-label">Disponibilidad</span><div class="badges-row"><span v-for="b in getDE(voluntarioActivo).disponibilidad" :key="b" class="info-badge">{{ b }}</span></div></div>
                <div class="exp-field mt-12" v-if="getDE(voluntarioActivo).habilidades?.length"><span class="exp-label">Habilidades</span><div class="badges-row"><span v-for="b in getDE(voluntarioActivo).habilidades" :key="b" class="info-badge">{{ b }}</span></div></div>
              </div>
              <!-- Transporte -->
              <div v-if="voluntarioActivo.solicitudVoluntario?.tipo === 'Transporte'" class="exp-section">
                <div class="exp-section-title accent-orange"><span class="exp-section-dot orange"></span>Transporte — Detalles</div>
                <div class="exp-grid cols-3">
                  <div class="exp-field"><span class="exp-label">Tipo de vehículo</span><span class="exp-value">{{ getDE(voluntarioActivo).tipoVehiculo || '—' }}</span></div>
                  <div class="exp-field"><span class="exp-label">Cobertura</span><span class="exp-value">{{ getDE(voluntarioActivo).cobertura || '—' }}</span></div>
                  <div class="exp-field"><span class="exp-label">Licencia vigente</span><span class="exp-value">{{ getDE(voluntarioActivo).licencia || '—' }}</span></div>
                </div>
                <div class="exp-field mt-12" v-if="getDE(voluntarioActivo).disponibilidad?.length"><span class="exp-label">Disponibilidad</span><div class="badges-row"><span v-for="b in getDE(voluntarioActivo).disponibilidad" :key="b" class="info-badge">{{ b }}</span></div></div>
                <div class="exp-field mt-12" v-if="getDE(voluntarioActivo).puedeTransp?.length"><span class="exp-label">Puede transportar</span><div class="badges-row"><span v-for="b in getDE(voluntarioActivo).puedeTransp" :key="b" class="info-badge">{{ b }}</span></div></div>
              </div>
              <!-- Veterinaria -->
              <div v-if="voluntarioActivo.solicitudVoluntario?.tipo === 'Veterinaria'" class="exp-section">
                <div class="exp-section-title accent-orange"><span class="exp-section-dot orange"></span>Veterinaria — Detalles</div>
                <div class="exp-grid cols-3">
                  <div class="exp-field"><span class="exp-label">Profesión</span><span class="exp-value fw">{{ getDE(voluntarioActivo).profesion || '—' }}</span></div>
                  <div class="exp-field"><span class="exp-label">Número de colegiado</span><span class="exp-value">{{ getDE(voluntarioActivo).colegiado || '—' }}</span></div>
                  <div class="exp-field"><span class="exp-label">Clínica</span><span class="exp-value">{{ getDE(voluntarioActivo).clinica || '—' }}</span></div>
                </div>
                <div class="exp-field mt-12" v-if="getDE(voluntarioActivo).especialidades?.length"><span class="exp-label">Especialidades</span><div class="badges-row"><span v-for="b in getDE(voluntarioActivo).especialidades" :key="b" class="info-badge">{{ b }}</span></div></div>
                <div class="exp-field mt-12" v-if="getDE(voluntarioActivo).disponibilidad?.length"><span class="exp-label">Disponibilidad</span><div class="badges-row"><span v-for="b in getDE(voluntarioActivo).disponibilidad" :key="b" class="info-badge">{{ b }}</span></div></div>
              </div>
              <!-- Redes sociales -->
              <div v-if="voluntarioActivo.solicitudVoluntario?.tipo === 'Redes sociales'" class="exp-section">
                <div class="exp-section-title accent-orange"><span class="exp-section-dot orange"></span>Redes sociales — Detalles</div>
                <div class="exp-grid cols-3">
                  <div class="exp-field"><span class="exp-label">Red principal</span><span class="exp-value fw">{{ getDE(voluntarioActivo).red || '—' }}</span></div>
                  <div class="exp-field"><span class="exp-label">Horas semanales</span><span class="exp-value">{{ getDE(voluntarioActivo).horasSemanales || '—' }}</span></div>
                  <div class="exp-field" v-if="getDE(voluntarioActivo).portafolio"><span class="exp-label">Portafolio / perfil</span><a :href="getDE(voluntarioActivo).portafolio" target="_blank" class="exp-link">{{ getDE(voluntarioActivo).portafolio }}</a></div>
                </div>
                <div class="exp-field mt-12" v-if="getDE(voluntarioActivo).experiencia?.length"><span class="exp-label">Experiencia</span><div class="badges-row"><span v-for="b in getDE(voluntarioActivo).experiencia" :key="b" class="info-badge">{{ b }}</span></div></div>
                <div class="exp-field mt-12" v-if="getDE(voluntarioActivo).programas?.length"><span class="exp-label">Programas que maneja</span><div class="badges-row"><span v-for="b in getDE(voluntarioActivo).programas" :key="b" class="info-badge">{{ b }}</span></div></div>
              </div>
              <!-- Rescatista -->
              <div v-if="voluntarioActivo.solicitudVoluntario?.tipo === 'Rescatista'" class="exp-section">
                <div class="exp-section-title accent-orange"><span class="exp-section-dot orange"></span>Rescatista — Detalles</div>
                <div class="exp-grid">
                  <div class="exp-field"><span class="exp-label">Años de experiencia</span><span class="exp-value fw">{{ getDE(voluntarioActivo).anosExp || '—' }}</span></div>
                  <div class="exp-field"><span class="exp-label">Rescates realizados</span><span class="exp-value fw">{{ getDE(voluntarioActivo).cantRescates || '—' }}</span></div>
                  <div class="exp-field"><span class="exp-label">Disponibilidad</span><span class="exp-value">{{ getDE(voluntarioActivo).disponibilidad || '—' }}</span></div>
                  <div class="exp-field"><span class="exp-label">Capacitación en manejo animal</span><span class="exp-value">{{ getDE(voluntarioActivo).capacitacion || '—' }}</span></div>
                  <div class="exp-field"><span class="exp-label">Zona de cobertura</span><span class="exp-value">{{ [getDE(voluntarioActivo).zonaProvincia, getDE(voluntarioActivo).zonaCanton].filter(Boolean).join(', ') || '—' }}</span></div>
                </div>
                <div class="exp-field mt-12" v-if="getDE(voluntarioActivo).equipo?.length"><span class="exp-label">Equipo disponible</span><div class="badges-row"><span v-for="b in getDE(voluntarioActivo).equipo" :key="b" class="info-badge">{{ b }}</span></div></div>
              </div>
            </div>

            <div class="sc-modal-footer" :class="{ 'footer-pending': voluntarioActivo?.solicitudVoluntario?.estado === 'Pendiente' }">
              <button class="sc-btn-cancel" @click="modalVer = false">Cerrar expediente</button>
              <template v-if="voluntarioActivo?.solicitudVoluntario?.estado === 'Pendiente'">
                <div class="pending-actions">
                  <button class="sc-btn-ver sc-btn-ver--red" @click="modalVer = false; pedirConfirmacion('rechazar', voluntarioActivo)">
                    <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
                    Rechazar
                  </button>
                  <button class="sc-btn-save" @click="modalVer = false; pedirConfirmacion('aprobar', voluntarioActivo)">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
                    Aprobar solicitud
                  </button>
                </div>
              </template>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- ══════════════════════════════════════════
         MODAL EDITAR
    ═══════════════════════════════════════════ -->
    <Teleport to="body">
      <Transition name="overlay-anim">
        <div v-if="modalEditar" class="sc-overlay" @click.self="modalEditar = false">
          <div class="sc-modal sc-modal--lg">
            <div class="sc-modal-header">
              <div class="edit-header-info">
                <div class="edit-avatar-sm">{{ iniciales(formEditar.nombre) }}</div>
                <div>
                  <p class="sc-modal-eyebrow">Voluntario</p>
                  <h2 class="sc-modal-title">{{ formEditar.nombre || 'Sin nombre' }}</h2>
                </div>
              </div>
              <button class="sc-modal-close" @click="modalEditar = false">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
              </button>
            </div>
            <div class="sc-modal-body edit-body">
              <div class="sc-section-label"><span class="sc-section-num">1</span>Datos personales</div>
              <div class="sc-form-grid sc-form-grid--4">
                <div class="sc-fg"><label>Nombre completo</label><input class="sc-input" v-model="formEditar.nombre" placeholder="Nombre completo"></div>
                <div class="sc-fg"><label>Cédula</label><input class="sc-input" v-model="formEditar.cedula" placeholder="1-2345-6789"></div>
                <div class="sc-fg"><label>Correo electrónico</label><input class="sc-input" type="email" v-model="formEditar.correo" placeholder="correo@ejemplo.com"></div>
                <div class="sc-fg"><label>Teléfono</label><input class="sc-input" v-model="formEditar.telefono" placeholder="+506 88888888"></div>
              </div>
              <div class="sc-section-label" style="margin-top:24px"><span class="sc-section-num">2</span>Ubicación</div>
              <div class="sc-form-grid sc-form-grid--4">
                <div class="sc-fg"><label>Provincia</label><div class="select-wrap"><select class="sc-input" v-model="formEditar.provincia"><option value="">Seleccione</option><option v-for="p in provincias" :key="p" :value="p">{{ p }}</option></select><i class='bx bx-chevron-down'></i></div></div>
                <div class="sc-fg"><label>Cantón</label><div class="select-wrap"><select class="sc-input" v-model="formEditar.canton" :disabled="!formEditar.provincia"><option value="">Seleccione</option><option v-for="c in cantonesEdit" :key="c" :value="c">{{ c }}</option></select><i class='bx bx-chevron-down'></i></div></div>
                <div class="sc-fg"><label>Distrito</label><div class="select-wrap"><select class="sc-input" v-model="formEditar.distrito" :disabled="!formEditar.canton"><option value="">Seleccione</option><option v-for="d in distritosEdit" :key="d" :value="d">{{ d }}</option></select><i class='bx bx-chevron-down'></i></div></div>
              </div>
              <div class="sc-section-label" style="margin-top:24px"><span class="sc-section-num">3</span>Tipo de voluntariado</div>
              <div class="sc-form-grid sc-form-grid--4">
                <div class="sc-fg sc-fg--span2"><label>Tipo</label><div class="select-wrap"><select class="sc-input" v-model="formEditar.tipo"><option value="">Seleccionar tipo</option><option>Casa cuna</option><option>Eventos de adopción</option><option>Transporte</option><option>Veterinaria</option><option>Redes sociales</option><option>Rescatista</option></select><i class='bx bx-chevron-down'></i></div></div>
              </div>
              <template v-if="formEditar.tipo">
                <div class="sc-section-label accent" style="margin-top:24px"><span class="sc-section-num orange">4</span>Información específica — {{ formEditar.tipo }}</div>
                <!-- CASA CUNA -->
                <template v-if="formEditar.tipo === 'Casa cuna'">
                  <div class="sc-form-grid sc-form-grid--4">
                    <div class="sc-fg"><label>Máximo de mascotas</label><input class="sc-input" type="number" min="1" :value="formEditar.datosEspecificos.maxMascotas" @input="formEditar.datosEspecificos.maxMascotas = $event.target.value"></div>
                    <div class="sc-fg"><label>Tipo de vivienda</label><div class="select-wrap"><select class="sc-input" v-model="formEditar.datosEspecificos.tipoVivienda"><option value="">Seleccione</option><option>Casa</option><option>Apartamento</option><option>Finca</option></select><i class='bx bx-chevron-down'></i></div></div>
                    <div class="sc-fg"><label>Patio cerrado</label><div class="radio-row"><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.patioCerrado" value="Sí"><span>Sí</span></label><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.patioCerrado" value="No"><span>No</span></label></div></div>
                    <div class="sc-fg"><label>Otras mascotas</label><div class="radio-row"><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.otrasMascotas" value="Sí"><span>Sí</span></label><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.otrasMascotas" value="No"><span>No</span></label></div></div>
                    <div class="sc-fg"><label>Niños en vivienda</label><div class="radio-row"><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.ninos" value="Sí"><span>Sí</span></label><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.ninos" value="No"><span>No</span></label></div></div>
                    <div class="sc-fg"><label>Tiempo disponible</label><input class="sc-input" v-model="formEditar.datosEspecificos.tiempoDisp" placeholder="Ej. 1 mes"></div>
                  </div>
                  <div class="sc-fg" style="margin-top:14px"><label>Puede recibir</label><div class="check-wrap"><label v-for="op in ['Cachorros','Adultos','Adultos mayores','Casos médicos']" :key="op" class="c-opt" :class="{ checked: deIncludes('puedeRecibir', op) }"><input type="checkbox" :checked="deIncludes('puedeRecibir', op)" @change="toggleDE('puedeRecibir', op)">{{ op }}</label></div></div>
                  <div class="sc-fg" style="margin-top:14px"><label>Comentarios adicionales</label><textarea class="sc-textarea" v-model="formEditar.datosEspecificos.comentarios" placeholder="Comentarios..."></textarea></div>
                </template>
                <!-- EVENTOS DE ADOPCIÓN -->
                <template v-if="formEditar.tipo === 'Eventos de adopción'">
                  <div class="sc-form-grid sc-form-grid--4">
                    <div class="sc-fg"><label>Ha participado antes</label><div class="radio-row"><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.participadoAntes" value="Sí"><span>Sí</span></label><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.participadoAntes" value="No"><span>No</span></label></div></div>
                    <div class="sc-fg"><label>Horario disponible</label><input class="sc-input" v-model="formEditar.datosEspecificos.horario" placeholder="Ej. 8am – 2pm"></div>
                    <div class="sc-fg"><label>Transporte propio</label><div class="radio-row"><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.transportePropio" value="Sí"><span>Sí</span></label><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.transportePropio" value="No"><span>No</span></label></div></div>
                  </div>
                  <div class="sc-fg" style="margin-top:14px"><label>Disponibilidad</label><div class="check-wrap"><label v-for="op in ['Sábados','Domingos','Entre semana']" :key="op" class="c-opt" :class="{ checked: deIncludes('disponibilidad', op) }"><input type="checkbox" :checked="deIncludes('disponibilidad', op)" @change="toggleDE('disponibilidad', op)">{{ op }}</label></div></div>
                  <div class="sc-fg" style="margin-top:14px"><label>Habilidades</label><div class="check-wrap"><label v-for="op in ['Atención al público','Organización','Fotografía','Manejo de mascotas']" :key="op" class="c-opt" :class="{ checked: deIncludes('habilidades', op) }"><input type="checkbox" :checked="deIncludes('habilidades', op)" @change="toggleDE('habilidades', op)">{{ op }}</label></div></div>
                  <div class="sc-fg" style="margin-top:14px"><label>Experiencia en atención al público</label><textarea class="sc-textarea" v-model="formEditar.datosEspecificos.experienciaPublico" placeholder="Describe la experiencia..."></textarea></div>
                </template>
                <!-- TRANSPORTE -->
                <template v-if="formEditar.tipo === 'Transporte'">
                  <div class="sc-form-grid sc-form-grid--4">
                    <div class="sc-fg"><label>Tipo de vehículo</label><div class="select-wrap"><select class="sc-input" v-model="formEditar.datosEspecificos.tipoVehiculo"><option value="">Seleccione</option><option>Carro</option><option>Moto</option><option>Pick-up</option><option>SUV</option></select><i class='bx bx-chevron-down'></i></div></div>
                    <div class="sc-fg"><label>Cobertura</label><div class="select-wrap"><select class="sc-input" v-model="formEditar.datosEspecificos.cobertura"><option value="">Seleccione</option><option>Cantón</option><option>Provincia</option><option>Todo el país</option></select><i class='bx bx-chevron-down'></i></div></div>
                    <div class="sc-fg"><label>Licencia vigente</label><div class="radio-row"><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.licencia" value="Sí"><span>Sí</span></label><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.licencia" value="No"><span>No</span></label></div></div>
                  </div>
                  <div class="sc-fg" style="margin-top:14px"><label>Disponibilidad</label><div class="check-wrap"><label v-for="op in ['Mañanas','Tardes','Noches','Emergencias']" :key="op" class="c-opt" :class="{ checked: deIncludes('disponibilidad', op) }"><input type="checkbox" :checked="deIncludes('disponibilidad', op)" @change="toggleDE('disponibilidad', op)">{{ op }}</label></div></div>
                  <div class="sc-fg" style="margin-top:14px"><label>Puede transportar</label><div class="check-wrap"><label v-for="op in ['Mascotas pequeñas','Mascotas medianas','Mascotas grandes','Traslados veterinarios']" :key="op" class="c-opt" :class="{ checked: deIncludes('puedeTransp', op) }"><input type="checkbox" :checked="deIncludes('puedeTransp', op)" @change="toggleDE('puedeTransp', op)">{{ op }}</label></div></div>
                </template>
                <!-- VETERINARIA -->
                <template v-if="formEditar.tipo === 'Veterinaria'">
                  <div class="sc-form-grid sc-form-grid--4">
                    <div class="sc-fg"><label>Profesión</label><div class="select-wrap"><select class="sc-input" v-model="formEditar.datosEspecificos.profesion"><option value="">Seleccione</option><option>Médico veterinario</option><option>Estudiante</option><option>Asistente veterinario</option></select><i class='bx bx-chevron-down'></i></div></div>
                    <div class="sc-fg"><label>Número de colegiado</label><input class="sc-input" v-model="formEditar.datosEspecificos.colegiado" placeholder="Opcional"></div>
                    <div class="sc-fg"><label>Clínica</label><input class="sc-input" v-model="formEditar.datosEspecificos.clinica" placeholder="Opcional"></div>
                  </div>
                  <div class="sc-fg" style="margin-top:14px"><label>Especialidades</label><div class="check-wrap"><label v-for="op in ['Medicina general','Cirugía','Emergencias','Rehabilitación','Dermatología']" :key="op" class="c-opt" :class="{ checked: deIncludes('especialidades', op) }"><input type="checkbox" :checked="deIncludes('especialidades', op)" @change="toggleDE('especialidades', op)">{{ op }}</label></div></div>
                  <div class="sc-fg" style="margin-top:14px"><label>Disponibilidad</label><div class="check-wrap"><label v-for="op in ['Consultas','Esterilizaciones','Emergencias']" :key="op" class="c-opt" :class="{ checked: deIncludes('disponibilidad', op) }"><input type="checkbox" :checked="deIncludes('disponibilidad', op)" @change="toggleDE('disponibilidad', op)">{{ op }}</label></div></div>
                </template>
                <!-- REDES SOCIALES -->
                <template v-if="formEditar.tipo === 'Redes sociales'">
                  <div class="sc-form-grid sc-form-grid--4">
                    <div class="sc-fg"><label>Red principal</label><div class="select-wrap"><select class="sc-input" v-model="formEditar.datosEspecificos.red"><option value="">Seleccione</option><option>Instagram</option><option>Facebook</option><option>TikTok</option><option>X</option></select><i class='bx bx-chevron-down'></i></div></div>
                    <div class="sc-fg"><label>Horas semanales</label><input class="sc-input" type="number" min="1" :value="formEditar.datosEspecificos.horasSemanales" @input="formEditar.datosEspecificos.horasSemanales = $event.target.value"></div>
                    <div class="sc-fg sc-fg--span2"><label>Portafolio / perfil</label><input class="sc-input" type="url" v-model="formEditar.datosEspecificos.portafolio" placeholder="https://..."></div>
                  </div>
                  <div class="sc-fg" style="margin-top:14px"><label>Experiencia</label><div class="check-wrap"><label v-for="op in ['Diseño gráfico','Fotografía','Video','Copywriting','Community Manager']" :key="op" class="c-opt" :class="{ checked: deIncludes('experiencia', op) }"><input type="checkbox" :checked="deIncludes('experiencia', op)" @change="toggleDE('experiencia', op)">{{ op }}</label></div></div>
                  <div class="sc-fg" style="margin-top:14px"><label>Programas que maneja</label><div class="check-wrap"><label v-for="op in ['Canva','Photoshop','CapCut','Illustrator']" :key="op" class="c-opt" :class="{ checked: deIncludes('programas', op) }"><input type="checkbox" :checked="deIncludes('programas', op)" @change="toggleDE('programas', op)">{{ op }}</label></div></div>
                </template>
                <!-- RESCATISTA -->
                <template v-if="formEditar.tipo === 'Rescatista'">
                  <div class="sc-form-grid sc-form-grid--4">
                    <div class="sc-fg"><label>Años de experiencia <span class="label-readonly-badge"><svg width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg> Solo lectura</span></label><div class="readonly-field"><span class="readonly-value">{{ formEditar.datosEspecificos.anosExp || '—' }}</span></div></div>
                    <div class="sc-fg"><label>Cantidad de rescates <span class="label-readonly-badge"><svg width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg> Solo lectura</span></label><div class="readonly-field"><span class="readonly-value">{{ formEditar.datosEspecificos.cantRescates || '—' }}</span></div></div>
                    <div class="sc-fg"><label>Disponibilidad</label><div class="select-wrap"><select class="sc-input" v-model="formEditar.datosEspecificos.disponibilidad"><option value="">Seleccione</option><option>Emergencias 24/7</option><option>Solo fines de semana</option><option>Entre semana</option></select><i class='bx bx-chevron-down'></i></div></div>
                    <div class="sc-fg"><label>Capacitación en manejo animal</label><div class="radio-row"><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.capacitacion" value="Sí"><span>Sí</span></label><label class="r-opt"><input type="radio" v-model="formEditar.datosEspecificos.capacitacion" value="No"><span>No</span></label></div></div>
                  </div>
                  <div class="sc-form-grid sc-form-grid--4" style="margin-top:14px">
                    <div class="sc-fg sc-fg--span2"><label>Zona — Provincia</label><div class="select-wrap"><select class="sc-input" v-model="formEditar.datosEspecificos.zonaProvincia"><option value="">Seleccione</option><option v-for="p in provincias" :key="p" :value="p">{{ p }}</option></select><i class='bx bx-chevron-down'></i></div></div>
                    <div class="sc-fg sc-fg--span2"><label>Zona — Cantón</label><div class="select-wrap"><select class="sc-input" v-model="formEditar.datosEspecificos.zonaCanton" :disabled="!formEditar.datosEspecificos.zonaProvincia"><option value="">Seleccione</option><option v-for="c in cantonesZonaEdit" :key="c" :value="c">{{ c }}</option></select><i class='bx bx-chevron-down'></i></div></div>
                  </div>
                  <div class="sc-fg" style="margin-top:14px"><label>Equipo disponible</label><div class="check-wrap"><label v-for="op in ['Transportadora','Correas','Jaulas trampa','Botiquín']" :key="op" class="c-opt" :class="{ checked: deIncludes('equipo', op) }"><input type="checkbox" :checked="deIncludes('equipo', op)" @change="toggleDE('equipo', op)">{{ op }}</label></div></div>
                </template>
              </template>
            </div>
            <div class="sc-modal-footer">
              <button class="sc-btn-cancel" @click="modalEditar = false">Cancelar</button>
              <button class="sc-btn-save" @click="guardarEdicion">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
                Guardar cambios
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- ══ MODAL CONFIRMACIÓN ══ -->
    <Teleport to="body">
      <Transition name="overlay-anim">
        <div v-if="modalConfirm" class="sc-overlay sc-overlay--top" @click.self="cancelarConfirmacion">
          <div class="sc-modal sc-modal--sm">
            <div class="sc-confirm-body">
              <div class="sc-confirm-icon">
                <svg width="26" height="26" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
              </div>
              <h3 class="sc-confirm-title">Confirmar acción</h3>
              <p class="sc-confirm-text" v-html="mensajeConfirm"></p>
            </div>
            <div class="sc-modal-footer">
              <button class="sc-btn-cancel" @click="cancelarConfirmacion">Cancelar</button>
              <button class="sc-btn-save" @click="confirmarAccion">Confirmar</button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

  </div>
</template>

<style scoped>
/* ═══════════════════════════════════════
   ROOT
═══════════════════════════════════════ */
.sc-root { background: transparent; padding-bottom: 40px; }

/* ═══════════════════════════════════════
   TOAST  (idéntico Salud)
═══════════════════════════════════════ */
.sc-toast {
  position: fixed; bottom: 32px; right: 32px; z-index: 9999;
  display: flex; align-items: center; gap: 10px;
  padding: 14px 20px; border-radius: 14px;
  font-size: 14px; font-weight: 600;
  box-shadow: 0 8px 32px rgba(0,0,0,0.16); pointer-events: none;
}
.sc-toast.success { background: #3A473C; color: #fff; }
.sc-toast.error   { background: #c0392b; color: #fff; }
.toast-anim-enter-active, .toast-anim-leave-active { transition: all 0.25s ease; }
.toast-anim-enter-from, .toast-anim-leave-to { opacity: 0; transform: translateY(10px); }

/* ═══════════════════════════════════════
   HEADER  (idéntico Salud)
═══════════════════════════════════════ */
.sc-header {
  display: flex; justify-content: space-between; align-items: flex-start;
  margin-bottom: 28px; gap: 16px; flex-wrap: wrap;
}
.sc-title  { font-size: 28px; font-weight: 800; color: #3A473C; letter-spacing: -0.5px; line-height: 1.1; }
.sc-sub    { font-size: 14px; color: #6C756D; margin-top: 5px; font-weight: 500; }

/* ═══════════════════════════════════════
   TOOLBAR  (idéntico Salud — una sola fila)
═══════════════════════════════════════ */
.sc-toolbar {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 20px;
  flex-wrap: nowrap;   /* misma fila siempre */
}

/* Tabs (idéntico Salud) */
.sc-tabs {
  display: flex; gap: 4px;
  background: #F4F6F4; border-radius: 12px; padding: 4px;
  flex-shrink: 0;
}
.sc-tab {
  display: flex; align-items: center; gap: 6px;
  padding: 8px 16px; border-radius: 9px; border: none;
  background: transparent; color: #6C756D;
  font-size: 13px; font-weight: 700; cursor: pointer;
  transition: all 0.18s; white-space: nowrap; font-family: inherit;
}
.sc-tab:hover  { color: #3A473C; background: rgba(255,255,255,0.6); }
.sc-tab.active { background: #fff; color: #3A473C; box-shadow: 0 1px 4px rgba(58,71,60,0.12); }

/* Filtros (idéntico Salud) */
.sc-filters {
  display: flex; align-items: center; gap: 10px;
  flex: 1; flex-wrap: nowrap;
  min-width: 0;
}

/* Búsqueda (idéntico Salud) */
.sc-search-wrap {
  position: relative; flex: 1; min-width: 160px; max-width: 260px;
}
.sc-search-icon {
  position: absolute; left: 12px; top: 50%; transform: translateY(-50%);
  color: #92A894; pointer-events: none;
}
.sc-search {
  width: 100%; box-sizing: border-box;
  padding: 9px 12px 9px 34px;
  border: 1.5px solid #E8ECE8; border-radius: 10px;
  font-size: 13px; color: #3A473C; background: #fff;
  outline: none; font-family: inherit; transition: border-color 0.18s;
  height: 36px;
}
.sc-search:focus { border-color: #92A894; }

/* Selects de filtro (mismo alto/borde/radio que sc-search) */
.sc-select-wrap {
  position: relative; flex-shrink: 0;
}
.sc-filter-select {
  appearance: none;
  padding: 0 32px 0 12px;
  height: 36px;
  border: 1.5px solid #E8ECE8; border-radius: 10px;
  font-size: 13px; color: #3A473C; background: #fff;
  outline: none; font-family: inherit; cursor: pointer;
  transition: border-color 0.18s; white-space: nowrap;
  box-sizing: border-box;
}
.sc-filter-select:focus { border-color: #92A894; }
.sc-select-icon {
  position: absolute; right: 10px; top: 50%; transform: translateY(-50%);
  color: #92A894; pointer-events: none;
}

/* Limpiar (idéntico Salud) */
.sc-clear {
  padding: 0 14px; height: 36px;
  border: 1.5px solid #fdd; border-radius: 10px;
  background: #fff5f5; color: #c0392b;
  font-size: 12px; font-weight: 700; font-family: inherit;
  cursor: pointer; transition: background 0.15s; white-space: nowrap; flex-shrink: 0;
}
.sc-clear:hover { background: #ffe5e5; }

/* ═══════════════════════════════════════
   TABLA  (idéntico Salud)
═══════════════════════════════════════ */
.sc-table-wrap {
  background: #fff; border-radius: 20px;
  box-shadow: 0 2px 16px rgba(58,71,60,0.06); overflow: hidden;
}
.sc-table { width: 100%; border-collapse: collapse; table-layout: fixed; }
.sc-table thead { background: #F9FAF9; }
.sc-table th {
  padding: 14px 20px;
  font-size: 11px; font-weight: 800; color: #92A894;
  text-transform: uppercase; letter-spacing: 0.6px;
  white-space: nowrap; border-bottom: 1.5px solid #F0F2F0;
  text-align: left;
}
.sc-table td {
  padding: 14px 20px; font-size: 14px; color: #3A473C;
  border-bottom: 1px solid #F5F7F5; vertical-align: middle;
}
.sc-table tbody tr:last-child td { border-bottom: none; }
.sc-table tbody tr { transition: background 0.12s; }
.sc-table tbody tr:hover { background: #FAFBFA; }

/* Avatar circular (idéntico Salud) */
.sc-avatar {
  width: 38px; height: 38px; border-radius: 50%;
  overflow: hidden; flex-shrink: 0;
  background: #DDE6DE;
  display: flex; align-items: center; justify-content: center;
}
.sc-avatar-ini { font-size: 14px; font-weight: 800; color: #5A6E5C; text-transform: uppercase; line-height: 1; }

.sc-pet-cell { display: flex; align-items: center; gap: 10px; }
.sc-pet-info  { display: flex; flex-direction: column; gap: 2px; min-width: 0; }
.sc-pet-name  { font-weight: 700; font-size: 14px; color: #3A473C; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.sc-pet-id    { font-size: 11px; color: #92A894; font-family: monospace; }

.sc-contact-stack { display: flex; flex-direction: column; gap: 2px; }
.sc-td-main { font-weight: 500; color: #3A473C; font-size: 13px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.sc-td-sec  { color: #7A8A7C; font-size: 12px; }

/* ── Badges unificados (tamaño y forma idénticos a Salud sc-date-badge) ── */
.sc-badge {
  display: inline-flex; align-items: center; gap: 5px;
  padding: 4px 10px; border-radius: 7px;
  font-size: 12px; font-weight: 600; white-space: nowrap;
  /* base neutro — sobrescrito por modificadores */
  background: #F0F4F0; color: #4A6550;
}

/* Estado badges */
.badge--green  { background: rgba(76,175,80,.14);  color: #2E7D32; }
.badge--red    { background: rgba(235,119,119,.16); color: #C45252; }
.badge--orange { background: rgba(255,152,0,.14);   color: #E65100; }
.badge--yellow { background: rgba(255,193,7,.16);   color: #9A6A00; }

/* Tipo badges — mismo tamaño, solo color distinto */
.badge--blue   { background: rgba(33,150,243,.13);  color: #1565C0; }
.badge--purple { background: rgba(156,39,176,.13);  color: #7B1FA2; }
.badge--teal   { background: rgba(0,150,136,.13);   color: #00695C; }
.badge--crimson{ background: rgba(244,67,54,.13);   color: #C62828; }
.badge--sky    { background: rgba(2,185,250,.13);   color: #006E9B; }
.badge--gold   { background: rgba(255,193,7,.18);   color: #7A5200; }
.badge--neutral{ background: #F4F6F4;               color: #6C756D; }

/* ── Botones de acción (idénticos a sc-btn-ver Salud) ── */
.sc-actions {
  display: flex;
  align-items: center;
  gap: 4px;
  justify-content: center;
}
.sc-btn-ver {
  display: inline-flex; align-items: center; gap: 4px;
  padding: 5px 11px; height: 28px;
  border: none; border-radius: 8px;
  font-size: 12px; font-weight: 700; font-family: inherit;
  cursor: pointer; transition: background 0.15s, opacity 0.15s;
  white-space: nowrap; flex-shrink: 0;
}

.action-icon {
  width: 10px;
  height: 10px;
  object-fit: contain;
  display: block;
}

.sc-btn-ver--neutral { background: #F0F4F0;               color: #3A473C; }
.sc-btn-ver--neutral:hover { background: #DDE6DE; }
.sc-btn-ver--green   { background: rgba(76,175,80,.14);   color: #2E7D32; }
.sc-btn-ver--green:hover   { background: rgba(76,175,80,.26); }
.sc-btn-ver--red     { background: rgba(235,119,119,.14); color: #C45252; }
.sc-btn-ver--red:hover     { background: rgba(235,119,119,.26); }
.sc-btn-ver--blue    { background: rgba(33,150,243,.12);  color: #1565C0; }
.sc-btn-ver--blue:hover    { background: rgba(33,150,243,.22); }
.sc-btn-ver--orange  { background: rgba(255,152,0,.13);   color: #E65100; }
.sc-btn-ver--orange:hover  { background: rgba(255,152,0,.24); }

/* Empty */
.sc-empty { padding: 0; }
.sc-empty-inner {
  display: flex; flex-direction: column; align-items: center;
  justify-content: center; gap: 12px; padding: 56px 24px; color: #92A894;
}
.sc-empty-inner svg { opacity: 0.4; }
.sc-empty-inner p { font-size: 14px; font-weight: 500; color: #7A8A7C; margin: 0; }

/* ═══════════════════════════════════════
   OVERLAY / MODAL  (idéntico Salud)
═══════════════════════════════════════ */
.sc-overlay {
  position: fixed; inset: 0; background: rgba(20,30,22,0.5);
  display: flex; align-items: center; justify-content: center;
  z-index: 200; padding: 20px; backdrop-filter: blur(2px); overflow-y: auto;
}
.sc-overlay--top { z-index: 400; }
.overlay-anim-enter-active, .overlay-anim-leave-active { transition: all 0.22s ease; }
.overlay-anim-enter-from, .overlay-anim-leave-to { opacity: 0; }
.overlay-anim-enter-from .sc-modal, .overlay-anim-leave-to .sc-modal { transform: translateY(16px) scale(0.98); }
.sc-modal {
  background: #fff; border-radius: 22px; width: 100%;
  max-height: 88vh; overflow-y: auto;
  box-shadow: 0 24px 80px rgba(0,0,0,0.2);
  transition: transform 0.22s ease; margin: auto;
}
.sc-modal--sm { max-width: 420px; }
.sc-modal--lg { max-width: 900px; }
.sc-modal-header {
  display: flex; justify-content: space-between; align-items: flex-start;
  padding: 24px 28px 18px; border-bottom: 1.5px solid #F0F2F0;
}
.sc-modal-eyebrow { font-size: 11px; font-weight: 800; color: #92A894; text-transform: uppercase; letter-spacing: 0.7px; margin-bottom: 4px; }
.sc-modal-title   { font-size: 20px; font-weight: 800; color: #3A473C; letter-spacing: -0.4px; }
.sc-modal-close {
  width: 34px; height: 34px; border-radius: 10px;
  border: 1.5px solid #E8ECE8; background: #fff; color: #6C756D;
  cursor: pointer; display: flex; align-items: center; justify-content: center;
  transition: background 0.15s, border-color 0.15s; flex-shrink: 0;
}
.sc-modal-close:hover { background: #F4F6F4; border-color: #ccc; }
.sc-modal-body { padding: 24px 28px 8px; }

/* Sección label */
.sc-section-label {
  display: flex; align-items: center; gap: 10px;
  font-size: 13px; font-weight: 800; color: #3A473C;
  text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 14px;
}
.sc-section-label.accent { color: #C08030; }
.sc-section-num {
  width: 24px; height: 24px; border-radius: 7px;
  background: #3A473C; color: #fff;
  font-size: 11px; font-weight: 800;
  display: flex; align-items: center; justify-content: center; flex-shrink: 0;
}
.sc-section-num.orange { background: #F9C17A; color: #8A5A1E; }

/* Grid formulario */
.sc-form-grid { display: grid; gap: 14px; }
.sc-form-grid--4 { grid-template-columns: repeat(4, 1fr); }
.sc-fg { display: flex; flex-direction: column; gap: 6px; }
.sc-fg--span2 { grid-column: span 2; }
.sc-fg--full  { grid-column: 1 / -1; }
.sc-fg label { font-size: 12px; font-weight: 700; color: #5A6E5C; letter-spacing: 0.1px; display: flex; align-items: center; gap: 7px; }
.sc-input {
  padding: 10px 13px; border: 1.5px solid #E8ECE8; border-radius: 10px;
  font-size: 13px; color: #3A473C; background: #FAFBFA;
  outline: none; font-family: inherit;
  transition: border-color 0.18s, background 0.18s;
  width: 100%; box-sizing: border-box;
}
.sc-input:focus    { border-color: #92A894; background: #fff; }
.sc-input:disabled { background: #F4F6F4; color: #9BA99C; cursor: not-allowed; }
.sc-textarea {
  padding: 10px 13px; border: 1.5px solid #E8ECE8; border-radius: 10px;
  font-size: 13px; color: #3A473C; background: #FAFBFA;
  outline: none; font-family: inherit;
  transition: border-color 0.18s, background 0.18s;
  width: 100%; box-sizing: border-box; min-height: 88px; resize: vertical; line-height: 1.5;
}
.sc-textarea:focus { border-color: #92A894; background: #fff; }
.select-wrap { position: relative; }
.select-wrap select.sc-input { appearance: none; padding-right: 36px; cursor: pointer; }
.select-wrap i { position: absolute; right: 12px; top: 50%; transform: translateY(-50%); font-size: 18px; color: #92A894; pointer-events: none; }
.radio-row { display: flex; gap: 16px; align-items: center; padding-top: 4px; }
.r-opt { display: flex; align-items: center; gap: 7px; font-size: 13px; font-weight: 600; color: #3A473C; cursor: pointer; }
.r-opt input[type="radio"] { accent-color: #92A894; width: 15px; height: 15px; cursor: pointer; }
.check-wrap { display: flex; flex-wrap: wrap; gap: 8px; margin-top: 2px; }
.c-opt { display: inline-flex; align-items: center; gap: 7px; padding: 7px 14px; border-radius: 10px; background: #F4F7F4; border: 1.5px solid #E8EDE8; font-size: 13px; font-weight: 600; color: #6C756D; cursor: pointer; transition: all .15s; }
.c-opt input[type="checkbox"] { accent-color: #92A894; width: 14px; height: 14px; cursor: pointer; }
.c-opt.checked { background: #E7F1E8; border-color: #92A894; color: #3A473C; }
.label-readonly-badge { display: inline-flex; align-items: center; gap: 4px; background: rgba(249,193,122,.18); color: #C08030; font-size: 10px; font-weight: 700; letter-spacing: 0.04em; padding: 2px 8px; border-radius: 99px; border: 1px solid rgba(249,193,122,.35); text-transform: uppercase; }
.readonly-field { display: flex; align-items: center; gap: 10px; background: #F7F5F0; border: 1.5px solid rgba(249,193,122,.35); border-radius: 10px; padding: 10px 13px; cursor: not-allowed; min-height: 41px; box-sizing: border-box; }
.readonly-value { font-size: 14px; font-weight: 700; color: #8A7A60; }
.edit-header-info { display: flex; align-items: center; gap: 14px; flex: 1; min-width: 0; }
.edit-avatar-sm { width: 44px; height: 44px; min-width: 44px; border-radius: 14px; background: #DDE6DE; color: #5A6E5C; font-size: 16px; font-weight: 800; display: flex; align-items: center; justify-content: center; }

/* Modal footer */
.sc-modal-footer {
  display: flex; justify-content: flex-end; gap: 10px;
  padding: 18px 28px 24px; border-top: 1.5px solid #F0F2F0; margin-top: 12px;
}
.sc-btn-cancel { padding: 10px 18px; background: #F4F6F4; border: none; border-radius: 10px; font-size: 13px; font-weight: 700; color: #6C756D; cursor: pointer; transition: background 0.15s; font-family: inherit; }
.sc-btn-cancel:hover { background: #E5EAE6; }
.sc-btn-save { display: flex; align-items: center; gap: 7px; padding: 10px 20px; background: #3A473C; border: none; border-radius: 10px; font-size: 13px; font-weight: 700; color: #fff; cursor: pointer; transition: background 0.18s; font-family: inherit; }
.sc-btn-save:hover { background: #2d3730; }

/* Confirmación */
.sc-confirm-body { padding: 32px 28px 8px; text-align: center; }
.sc-confirm-icon { width: 60px; height: 60px; border-radius: 50%; background: #EEF2EE; color: #3A473C; display: flex; align-items: center; justify-content: center; margin: 0 auto 18px; }
.sc-confirm-title { font-size: 18px; font-weight: 800; color: #3A473C; margin-bottom: 10px; }
.sc-confirm-text  { font-size: 13px; color: #6C756D; line-height: 1.6; max-width: 320px; margin: 0 auto; }

/* ═══════════════════════════════════════
   EXPEDIENTE — Modal Ver  (idéntico Salud)
═══════════════════════════════════════ */
.exp-header {
  display: flex; align-items: center; gap: 18px;
  padding: 26px 28px 22px; border-bottom: 1.5px solid #F0F2F0;
  background: linear-gradient(135deg, #F7F9F7, white);
}
.exp-avatar { width: 64px; height: 64px; min-width: 64px; border-radius: 18px; background: #DDE6DE; color: #5A6E5C; font-size: 22px; font-weight: 800; display: flex; align-items: center; justify-content: center; box-shadow: 0 8px 20px rgba(58,71,60,0.12); }
.exp-header-info { flex: 1; min-width: 0; }
.exp-name { font-size: 20px; font-weight: 800; color: #3A473C; margin-bottom: 8px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.exp-meta { display: flex; gap: 8px; flex-wrap: wrap; }
.exp-body { display: flex; flex-direction: column; gap: 0; }
.exp-section { border-bottom: 1.5px solid #F4F6F4; padding: 20px 0; }
.exp-section:last-child { border-bottom: none; }
.exp-section-title { display: flex; align-items: center; gap: 9px; font-size: 11px; font-weight: 800; letter-spacing: 0.10em; text-transform: uppercase; color: #92A894; margin-bottom: 16px; }
.exp-section-title.accent-orange { color: #C08030; }
.exp-section-dot { width: 7px; height: 7px; border-radius: 50%; background: #92A894; flex-shrink: 0; }
.exp-section-dot.orange { background: #F9C17A; }
.exp-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 14px 20px; }
.exp-grid.cols-3 { grid-template-columns: 1fr 1fr 1fr; }
.exp-field { display: flex; flex-direction: column; gap: 4px; }
.exp-label { font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.06em; color: #92A894; }
.exp-value { font-size: 14px; color: #3A473C; }
.exp-value.fw { font-weight: 700; }
.exp-link { font-size: 13px; color: #3B82F6; text-decoration: none; word-break: break-all; }
.exp-link:hover { text-decoration: underline; }
.exp-text-block { font-size: 14px; color: #3A473C; line-height: 1.7; background: #F9FAF9; border-radius: 12px; padding: 12px 14px; margin: 0; }
.mt-12 { margin-top: 14px !important; }
.badges-row { display: flex; flex-wrap: wrap; gap: 6px; margin-top: 4px; }
.info-badge { display: inline-block; background: #EEF2EE; color: #3A473C; font-size: 12px; font-weight: 600; padding: 5px 12px; border-radius: 999px; border: 1px solid rgba(146,168,148,0.25); }
.footer-pending { justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 10px; }
.pending-actions { display: flex; gap: 8px; align-items: center; }

/* ═══════════════════════════════════════
   RESPONSIVE
═══════════════════════════════════════ */
@media (max-width: 1100px) {
  .sc-toolbar { flex-wrap: wrap; }
  .sc-filters  { flex-wrap: wrap; }
}
@media (max-width: 900px) {
  .sc-form-grid--4 { grid-template-columns: repeat(2, 1fr); }
  .sc-fg--span2    { grid-column: span 1; }
  .exp-grid.cols-3 { grid-template-columns: 1fr 1fr; }
}
@media (max-width: 640px) {
  .sc-header   { flex-direction: column; align-items: flex-start; }
  .sc-toolbar  { flex-direction: column; align-items: flex-start; }
  .sc-tabs     { flex-wrap: wrap; }
  .sc-filters  { width: 100%; flex-wrap: wrap; }
  .sc-search-wrap { max-width: 100%; }
  .sc-form-grid--4 { grid-template-columns: 1fr; }
  .sc-fg--span2, .sc-fg--full { grid-column: 1; }
  .sc-table th:nth-child(4),
  .sc-table td:nth-child(4) { display: none; }
  .sc-modal-body   { padding: 16px 18px 8px; }
  .sc-modal-header,
  .sc-modal-footer { padding-left: 18px; padding-right: 18px; }
  .exp-grid        { grid-template-columns: 1fr; }
  .exp-grid.cols-3 { grid-template-columns: 1fr 1fr; }
  .footer-pending  { flex-direction: column; align-items: stretch; }
  .pending-actions { justify-content: flex-end; }
}
@media (max-width: 480px) {
  .exp-grid.cols-3 { grid-template-columns: 1fr; }
}
</style>
