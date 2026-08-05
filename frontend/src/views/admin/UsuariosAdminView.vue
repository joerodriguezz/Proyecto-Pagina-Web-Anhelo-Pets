<script setup>
import { ref, computed } from 'vue'

const ADMIN_ID = 'ADMIN-001'

const usuarios = ref([])
const showModal = ref(false)
const selectedUser = ref(null)

const filtroTexto = ref('')
const filtroRol = ref('Todos')
const filtroEstado = ref('Todos')

const modalConfirm = ref(false)
const usuarioSeleccionado = ref(null)
const mensajeConfirm = ref('')

const toast = ref({ visible: false, tipo: 'exito', texto: '' })

function adminPorDefecto() {
  return {
    id: 'ADMIN-001',
    nombre: 'Shirley Valverde',
    cedula: '1-0932-0528',
    correo: 'shirley@anhelopets.cr',
    telefono: '+506 8840-3334',
    password: 'Anhelo123',
    rol: 'Admin',
    tipoVoluntario: '',
    direccion: 'Quepos',
    pais: 'Costa Rica',
    solicitudVoluntario: null,
    activo: true
  }
}

function cargarUsuarios() {
  try {
    const raw = localStorage.getItem('anhelo_usuarios')
    const guardados = raw ? JSON.parse(raw) : null
    const admin = adminPorDefecto()
    if (Array.isArray(guardados) && guardados.length > 0) {
      const idx = guardados.findIndex(u => u.id === ADMIN_ID)
      if (idx >= 0) guardados[idx] = admin
      else guardados.unshift(admin)
      usuarios.value = guardados
    } else {
      usuarios.value = [admin]
    }
    guardarUsuarios()
  } catch {
    localStorage.removeItem('anhelo_usuarios')
    usuarios.value = [adminPorDefecto()]
  }
}

function guardarUsuarios() {
  localStorage.setItem('anhelo_usuarios', JSON.stringify(usuarios.value))
}

cargarUsuarios()

function toggleEstado(user) {
  if (user.id === ADMIN_ID) return
  const todos = JSON.parse(localStorage.getItem('anhelo_usuarios')) || []
  const i = todos.findIndex(u => u.id === user.id)
  if (i !== -1) {
    todos[i].activo = !todos[i].activo
    localStorage.setItem('anhelo_usuarios', JSON.stringify(todos))
    user.activo = todos[i].activo
    cargarUsuarios()
    mostrarToast(user.activo ? 'Usuario activado.' : 'Usuario desactivado.')
  }
}

function pedirConfirmacionEstado(user) {
  usuarioSeleccionado.value = user

  mensajeConfirm.value = user.activo
  ? `
    Estás a punto de desactivar la cuenta de
    <strong>${user.nombre}</strong>.<br><br>
    El usuario perderá acceso al sistema hasta que sea activado nuevamente.
  `
  : `
    Estás a punto de activar la cuenta de
    <strong>${user.nombre}</strong>.<br><br>
    El usuario recuperará el acceso al sistema inmediatamente.
  `

  modalConfirm.value = true
}

function confirmarCambioEstado() {
  if (usuarioSeleccionado.value) {
    toggleEstado(usuarioSeleccionado.value)
  }

  modalConfirm.value = false
  usuarioSeleccionado.value = null
}

function cancelarConfirmacion() {
  modalConfirm.value = false
  usuarioSeleccionado.value = null
}


function verDetalle(user) {
  selectedUser.value = { ...user }
  showModal.value = true
}

function cerrarModal() {
  showModal.value = false
  selectedUser.value = null
}

/* ── CORRECCIÓN DE GUARDADO ──────────────────────────────────
   selectedUser es una COPIA (verDetalle hace `{ ...user }`), no
   una referencia al objeto dentro de usuarios.value. Antes, los
   botones "Activar/Desactivar usuario" del modal de detalle
   mutaban esa copia directamente (`selectedUser.activo = true`)
   y luego llamaban guardarUsuarios() — pero guardarUsuarios()
   serializa usuarios.value, que nunca fue modificado. El cambio
   se veía en el modal un instante y se perdía por completo al
   guardar: no se escribía en localStorage.
   Esta función localiza al usuario REAL dentro de usuarios.value
   por id, lo modifica ahí, y recién entonces guarda. */
function setEstadoUsuario(activo) {
  if (!selectedUser.value) return
  const idx = usuarios.value.findIndex(u => u.id === selectedUser.value.id)
  if (idx !== -1) {
    usuarios.value[idx].activo = activo
    guardarUsuarios()
    mostrarToast(activo ? 'Usuario activado.' : 'Usuario desactivado.')
  }
  cerrarModal()
}

function mostrarToast(texto, tipo = 'exito') {
  toast.value = { visible: true, tipo, texto }
  setTimeout(() => { toast.value.visible = false }, 3000)
}

function iniciales(nombre = '') {
  return nombre.trim().split(' ').map(p => p[0]).slice(0, 2).join('').toUpperCase()
}

function rolBadgeClass(rol) {
  if (rol === 'Admin')      return 'badge-admin'
  if (rol === 'Voluntario') return 'badge-aprobada'
  return 'badge-blue'
}

function solicitudBadgeClass(estado) {
  if (estado === 'Aprobada')  return 'badge-aprobada'
  if (estado === 'Rechazada') return 'badge-rechazada'
  if (estado === 'Pendiente') return 'badge-pendiente'
  return 'badge-neutral'
}

function estadoBadgeClass(user) {
  return user.activo ? 'badge-aprobada' : 'badge-inactivo'
}

function estadoLabel(user) {
  return user.activo ? 'Activo' : 'Inactivo'
}

const hayFiltros = computed(() =>
  filtroTexto.value.trim() !== '' ||
  filtroRol.value !== 'Todos' ||
  filtroEstado.value !== 'Todos'
)

function limpiarFiltros() {
  filtroTexto.value  = ''
  filtroRol.value    = 'Todos'
  filtroEstado.value = 'Todos'
}

const usuariosFiltrados = computed(() => {
  return usuarios.value.filter(u => {
    const t = filtroTexto.value.trim().toLowerCase()
    const coincideTexto =
      !t ||
      u.nombre.toLowerCase().includes(t) ||
      /* CORRECCIÓN: antes era u.correo.toLowerCase() sin resguardo —
         si un usuario no tenía correo definido, esto lanzaba un
         TypeError y rompía la búsqueda (y con ella toda la tabla,
         ya que usuariosFiltrados es un computed). */
      (u.correo || '').toLowerCase().includes(t) ||
      (u.cedula || '').toLowerCase().includes(t) ||
      (u.codigoVoluntario || u.id).toLowerCase().includes(t)

    const coincideRol =
      filtroRol.value === 'Todos' || u.rol === filtroRol.value

    const coincideEstado =
      filtroEstado.value === 'Todos' ||
      (filtroEstado.value === 'Activo'    &&  u.activo) ||
      (filtroEstado.value === 'Inactivo'  && !u.activo) ||
      (filtroEstado.value === 'Pendiente' && u.solicitudVoluntario?.estado === 'Pendiente') ||
      (filtroEstado.value === 'Aprobada'  && u.solicitudVoluntario?.estado === 'Aprobada')  ||
      (filtroEstado.value === 'Rechazada' && u.solicitudVoluntario?.estado === 'Rechazada')

    return coincideTexto && coincideRol && coincideEstado
  })
})

const totalUsuarios   = computed(() => usuarios.value.length)
const totalActivos    = computed(() => usuarios.value.filter(u => u.activo).length)
const totalInactivos  = computed(() => usuarios.value.filter(u => !u.activo).length)
const totalVoluntarios = computed(() => usuarios.value.filter(u => u.rol === 'Voluntario').length)
const totalPendientes = computed(() => usuarios.value.filter(u => u.solicitudVoluntario?.estado === 'Pendiente').length)

const ROL_TABS = ['Todos', 'Admin', 'Voluntario', 'Usuario']
const ESTADO_TABS = ['Todos', 'Activo', 'Inactivo', 'Pendiente', 'Aprobada', 'Rechazada']
const ESTADO_TAB_LABEL = {
  Todos: 'Todos', Activo: 'Activos', Inactivo: 'Inactivos',
  Pendiente: 'Solicitud pendiente', Aprobada: 'Solicitud aprobada', Rechazada: 'Solicitud rechazada',
}
</script>

<template>
  <div class="view-container">

    <!-- ── Toast ── -->
    <Teleport to="body">
      <Transition name="toast-anim">
        <div v-if="toast.visible" class="usr-toast" :class="toast.tipo === 'error' ? 'usr-toast--error' : 'usr-toast--exito'">
          <svg v-if="toast.tipo === 'exito'" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
          <svg v-else width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
          {{ toast.texto }}
        </div>
      </Transition>
    </Teleport>

    <!-- CABECERA -->
    <header class="page-header">
      <div class="brand-row">
        <div class="brand-mark">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
        </div>
        <div>
          <h1 class="admin-page-title">Usuarios</h1>
          <p class="admin-page-sub">Control de cuentas y roles del sistema</p>
        </div>
      </div>
    </header>

    <!-- TARJETAS RESUMEN -->
    <div class="don-summary">
      <div class="don-card">
        <div class="don-icon total-icon">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
        </div>
        <strong class="don-value">{{ totalUsuarios }}</strong>
        <span class="don-label">Total usuarios</span>
        <span class="don-desc">En el sistema</span>
      </div>
      <div class="don-card">
        <div class="don-icon activos-icon">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/></svg>
        </div>
        <strong class="don-value">{{ totalActivos }}</strong>
        <span class="don-label">Activos</span>
        <span class="don-desc">Con acceso al sistema</span>
      </div>
      <div class="don-card">
        <div class="don-icon inactivos-icon">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M21 8v13H3V8"/><path d="M1 3h22v5H1z"/><line x1="10" y1="12" x2="14" y2="12"/></svg>
        </div>
        <strong class="don-value">{{ totalInactivos }}</strong>
        <span class="don-label">Inactivos</span>
        <span class="don-desc">Sin acceso al sistema</span>
      </div>
      <div class="don-card">
        <div class="don-icon voluntarios-icon">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/></svg>
        </div>
        <strong class="don-value">{{ totalVoluntarios }}</strong>
        <span class="don-label">Voluntarios</span>
        <span class="don-desc">Rol asignado</span>
      </div>
      <div class="don-card">
        <div class="don-icon pendientes-icon">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="9"/><polyline points="12 7 12 12 16 14"/></svg>
        </div>
        <strong class="don-value">{{ totalPendientes }}</strong>
        <span class="don-label">Solicitudes pendientes</span>
        <span class="don-desc">Por revisar</span>
      </div>
    </div>

    <!-- FILTROS -->
    <div class="filtros-panel">

      <div class="filtros-row">
        <!-- Rol — tabs, como en Mascotas -->
        <div class="filtro-group filtro-group--tabs">
          <label class="filtro-label">Rol</label>
          <div class="tabs-wrap">
            <button v-for="r in ROL_TABS" :key="r" type="button" class="tab-btn" :class="{ active: filtroRol === r }" @click="filtroRol = r">{{ r }}</button>
          </div>
        </div>

        <!-- Estado — tabs, como en Mascotas -->
        <div class="filtro-group filtro-group--tabs">
          <label class="filtro-label">Estado</label>
          <div class="tabs-wrap">
            <button v-for="e in ESTADO_TABS" :key="e" type="button" class="tab-btn" :class="{ active: filtroEstado === e }" @click="filtroEstado = e">{{ ESTADO_TAB_LABEL[e] }}</button>
          </div>
        </div>
      </div>

      <div class="filtros-divider"></div>

      <div class="filtros-row filtros-row--end">
        <!-- Buscar usuario -->
        <div class="filtro-group filtro-group--search">
          <label class="filtro-label">Buscar usuario</label>
          <div class="filtro-input-wrap">
            <span class="filtro-icon filtro-icon--left">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
            </span>
            <input
              v-model="filtroTexto"
              placeholder="Nombre, correo, cédula o ID..."
              class="filtro-input filtro-input--icon-left"
            />
          </div>
        </div>

        <!-- Limpiar — mismo botón exacto que Mascotas -->
        <div class="filtro-group filtro-group--btn">
          <button type="button" class="btn btn--ghost" :class="{ 'btn--ghost-active': hayFiltros }" @click="limpiarFiltros">Limpiar filtros</button>
        </div>
      </div>

    </div>

    <!-- ESTADO VACÍO -->
    <div v-if="usuariosFiltrados.length === 0" class="empty-state">
      <svg xmlns="http://www.w3.org/2000/svg" width="36" height="36" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
      <p class="empty-title">No hay usuarios registrados</p>
      <p class="empty-sub">Ajusta los filtros o espera nuevos registros.</p>
    </div>

    <!-- TABLA PRINCIPAL -->
    <div v-else class="table-wrapper">
      <div class="table-scroll">
        <table class="don-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Usuario</th>
              <th>Correo</th>
              <th>Rol</th>
              <th>Solicitud</th>
              <th>Estado</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="u in usuariosFiltrados" :key="u.id" class="don-row">

              <td><span class="id-pill">{{ u.codigoVoluntario || u.id }}</span></td>

              <td>
                <div class="usr-cell">
                  <div class="usr-avatar">
                    <span class="usr-avatar-ini">{{ iniciales(u.nombre) }}</span>
                  </div>
                  <span class="donor-name">{{ u.nombre }}</span>
                </div>
              </td>

              <td><span class="donor-mail-td">{{ u.correo }}</span></td>

              <td><span class="estado-badge" :class="rolBadgeClass(u.rol)">{{ u.rol }}</span></td>

              <td>
                <span v-if="u.solicitudVoluntario?.estado" class="estado-badge" :class="solicitudBadgeClass(u.solicitudVoluntario.estado)">
                  {{ u.solicitudVoluntario.estado }}
                </span>
                <span v-else class="fecha-text">—</span>
              </td>

              <td><span class="estado-badge" :class="estadoBadgeClass(u)">{{ estadoLabel(u) }}</span></td>

              <td>
                <div class="action-group">
                  <button type="button" class="icon-only icon-only--ver" @click="verDetalle(u)" data-tooltip="Ver detalle">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
                  </button>
                  <button
                    type="button"
                    class="icon-only"
                    :class="u.activo ? 'icon-only--inactivar' : 'icon-only--activar'"
                    :disabled="u.id === ADMIN_ID"
                    @click="pedirConfirmacionEstado(u)"
                    :data-tooltip="u.activo ? 'Desactivar usuario' : 'Activar usuario'"
                  >
                    <svg v-if="u.activo" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 8v13H3V8"/><path d="M1 3h22v5H1z"/><line x1="10" y1="12" x2="14" y2="12"/></svg>
                    <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
                  </button>
                </div>
              </td>

            </tr>
          </tbody>
        </table>
      </div>
      <div class="table-footer">
        {{ usuariosFiltrados.length }} usuario{{ usuariosFiltrados.length !== 1 ? 's' : '' }} encontrado{{ usuariosFiltrados.length !== 1 ? 's' : '' }}
      </div>
    </div>

    <!-- ═══════════ MODAL DE DETALLE (expediente, mismo sistema que Mascotas) ═══════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showModal && selectedUser" class="modal-overlay" @click.self="cerrarModal">
          <div class="modal-box modal-box--uniform">
            <button type="button" class="close-btn close-btn--hero" @click="cerrarModal">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>

            <div class="hero">
              <div class="hero-photo">
                <span class="hero-photo-ini">{{ iniciales(selectedUser.nombre) }}</span>
              </div>
              <div class="hero-info">
                <div class="hero-name-row">
                  <h2 class="hero-name">{{ selectedUser.nombre }}</h2>
                  <span class="estado-badge badge-status-hero" :class="estadoBadgeClass(selectedUser)">{{ estadoLabel(selectedUser) }}</span>
                </div>
                <div class="hero-meta">
                  <span class="hero-meta-chip">
                    <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="7" height="7" rx="1.5"/><rect x="14" y="3" width="7" height="7" rx="1.5"/><rect x="14" y="14" width="7" height="7" rx="1.5"/><rect x="3" y="14" width="7" height="7" rx="1.5"/></svg>
                    {{ selectedUser.codigoVoluntario || selectedUser.id }}
                  </span>
                  <span class="hero-meta-chip">{{ selectedUser.correo }}</span>
                  <span class="hero-meta-chip" :class="'badge-chip-' + rolBadgeClass(selectedUser.rol)">{{ selectedUser.rol }}</span>
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
                          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/></svg>
                        </span>
                        Información personal
                      </h4>
                      <div class="fields-row">
                        <div class="field-col"><span class="field-label-row">Cédula</span><span class="field-value">{{ selectedUser.cedula || '—' }}</span></div>
                        <div class="field-col"><span class="field-label-row">Teléfono</span><span class="field-value">{{ selectedUser.telefono || '—' }}</span></div>
                        <div class="field-col"><span class="field-label-row">País</span><span class="field-value">{{ selectedUser.pais || '—' }}</span></div>
                      </div>
                      <div class="info-subsection">
                        <span class="field-label-row">Dirección</span>
                        <p class="info-subsection-text">{{ selectedUser.direccion || '—' }}</p>
                      </div>
                    </div>

                    <div class="block block-wide">
                      <h4 class="block-title">
                        <span class="block-title-icon">
                          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
                        </span>
                        Nota
                      </h4>
                      <div class="tint-box tint-box--warn tint-box--desc">
                        <span>El rol se actualiza automáticamente desde <strong>Solicitudes de Voluntariado</strong> al aprobar o rechazar una postulación.</span>
                      </div>
                    </div>
                  </div>

                  <div class="block" style="margin-bottom:0;">
                    <h4 class="block-title">
                      <span class="block-title-icon">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="7" width="20" height="14" rx="2"/><path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"/></svg>
                      </span>
                      Rol y estado
                    </h4>
                    <div class="list-col">
                      <div class="list-item">
                        <div class="list-icon"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/></svg></div>
                        <div class="list-text"><span class="list-label">Rol</span><span class="estado-badge list-badge" :class="rolBadgeClass(selectedUser.rol)">{{ selectedUser.rol }}</span></div>
                      </div>
                      <div class="list-item">
                        <div class="list-icon"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/></svg></div>
                        <div class="list-text"><span class="list-label">Tipo de voluntario</span><span class="list-value">{{ selectedUser.tipoVoluntario || '—' }}</span></div>
                      </div>
                      <div class="list-item">
                        <div class="list-icon"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/></svg></div>
                        <div class="list-text"><span class="list-label">Estado de cuenta</span><span class="estado-badge list-badge" :class="estadoBadgeClass(selectedUser)">{{ estadoLabel(selectedUser) }}</span></div>
                      </div>
                      <div class="list-item">
                        <div class="list-icon"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="7" width="20" height="14" rx="2"/></svg></div>
                        <div class="list-text">
                          <span class="list-label">Solicitud voluntariado</span>
                          <span v-if="selectedUser.solicitudVoluntario?.estado" class="estado-badge list-badge" :class="solicitudBadgeClass(selectedUser.solicitudVoluntario.estado)">{{ selectedUser.solicitudVoluntario.estado }}</span>
                          <span v-else class="list-value">—</span>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <div class="footer">
              <template v-if="selectedUser.id !== ADMIN_ID">
                <button type="button" class="btn-footer-success" :disabled="selectedUser.activo" @click="setEstadoUsuario(true)">Activar usuario</button>
                <button type="button" class="btn-footer-danger" :disabled="!selectedUser.activo" @click="setEstadoUsuario(false)">Desactivar usuario</button>
              </template>
              <p v-else class="estado-final-msg estado-final-msg--ok">Esta es la cuenta de administrador principal.</p>
              <button type="button" class="btn-ghost-red" @click="cerrarModal">Cerrar expediente</button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- ═══════════ MODAL DE CONFIRMACIÓN — mismo diseño que "Desactivar mascota" en Mascotas ═══════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="modalConfirm" class="modal-overlay" @click.self="cancelarConfirmacion">
          <div class="modal-box modal-box--confirm">
            <button type="button" class="close-btn" @click="cancelarConfirmacion">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>

            <div class="confirm-header">
              <div class="confirm-icon">
                <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>
              </div>
              <div>
                <p class="confirm-eyebrow">{{ usuarioSeleccionado?.activo ? 'Desactivar usuario' : 'Activar usuario' }}</p>
                <h2 class="confirm-title">{{ usuarioSeleccionado?.nombre }}</h2>
              </div>
            </div>

            <div class="confirm-body">
              <!-- CORRECCIÓN: mensajeConfirm se armaba en pedirConfirmacionEstado()
                   pero nunca estaba enlazado a la plantilla; el admin nunca veía
                   la advertencia detallada. Ahora sí se muestra. -->
              <div class="warn-box" v-html="mensajeConfirm"></div>
            </div>

            <div class="confirm-footer">
              <button type="button" class="btn-cancel" @click="cancelarConfirmacion">Cancelar</button>
              <button type="button" class="btn-danger" @click="confirmarCambioEstado">Confirmar</button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

  </div>
</template>

<style scoped>
/* ── Variables — idénticas al sistema de diseño de Mascotas
   (se retira la capa --usr-* / :global(:root) / lista de
   remapeo por selector: era frágil, ya que cualquier clase
   nueva usada en el Teleport que no estuviera en esa lista
   se quedaba sin temas. El bloque <style> global de abajo,
   igual que en Mascotas, ya alcanza a todo el contenido
   teletransportado sin necesitar listar cada clase.) ── */
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
  background:
    radial-gradient(ellipse 800px 420px at 12% 0%, rgba(146,168,148,.07), transparent),
    var(--fondo);
  padding-bottom: 40px;
}

/* ── Sistema de botones ── */
.btn { display:inline-flex; align-items:center; justify-content:center; gap:var(--btn-icon-gap); height:var(--btn-height); padding:0 var(--btn-pad-x); border-radius:var(--btn-radius); border:1px solid transparent; font-family:inherit; font-size:var(--btn-font-size); font-weight:var(--btn-font-weight); line-height:1; white-space:nowrap; cursor:pointer; user-select:none; transition:background-color var(--btn-transition), border-color var(--btn-transition), color var(--btn-transition), box-shadow var(--btn-transition); }
.btn:active:not(:disabled) { transform:translateY(1px); }
.btn:focus-visible { outline:none; box-shadow:0 0 0 3px rgba(58,71,60,.16); }
.btn--ghost { background:var(--blanco); color:var(--texto-sec); border-color:var(--borde); }
.btn--ghost:hover:not(:disabled) { background:#FAFBFA; color:var(--texto); border-color:#D3D8D3; }
.btn--ghost-active { border-color:var(--verde-sec); color:var(--verde); }
.btn--ghost-active:hover:not(:disabled) { background:#F3F6F3; color:var(--verde); border-color:var(--verde-sec); }

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
.total-icon        { background:#F2F3F2; border-color:#DFE2DF; color:#616861; }
.activos-icon      { background:#EDF6EF; border-color:#C9E4CE; color:#2E7D45; }
/* CORRECCIÓN: antes era rojo (#FBEDEC/#B71C1C); Mascotas usa gris para
   su tarjeta "Inactivas" (misma familia que total-icon), no rojo. */
.inactivos-icon    { background:#F2F3F2; border-color:#DFE2DF; color:#7A827B; }
.voluntarios-icon  { background:#EAF2F6; border-color:#C7DCE6; color:#3C6E85; }
.pendientes-icon   { background:#FDF6E8; border-color:#F2E1B8; color:#A97A0C; }
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
.filtro-label { font-size:10.5px; font-weight:700; color:var(--texto-ter); text-transform:uppercase; letter-spacing:0.6px; }
.tabs-wrap { display:flex; gap:3px; background:var(--fondo); border:1px solid var(--borde-suave); border-radius:10px; padding:3px; flex-wrap:wrap; }
.tab-btn { padding:7px 13px; border-radius:7px; border:none; background:transparent; color:var(--texto-sec); font-size:12px; font-weight:700; cursor:pointer; transition:all 0.18s; white-space:nowrap; font-family:inherit; }
.tab-btn:hover { color:var(--texto); }
.tab-btn.active { background:var(--blanco); color:var(--texto); box-shadow:var(--sombra-sm); border:1px solid var(--borde); }
.filtro-input-wrap { position:relative; display:flex; align-items:center; }
.filtro-input { width:100%; height:36px; padding:0 14px; border-radius:8px; border:1px solid var(--borde); background:var(--fondo); font-size:13px; color:var(--texto); font-family:inherit; outline:none; transition:border-color 0.18s, background 0.18s; box-sizing:border-box; }
.filtro-input:focus { border-color:var(--verde-sec); background:var(--blanco); }
.filtro-input::placeholder { color:var(--texto-ter); }
.filtro-input--icon-left { padding-left:36px; }
.filtro-icon { position:absolute; display:flex; align-items:center; color:var(--texto-sec); }
.filtro-icon--left { left:12px; }

/* ── Estado vacío ── */
.empty-state { text-align:center; padding:72px 24px; background:var(--blanco); border-radius:16px; border:1px solid var(--borde); color:var(--verde-sec); display:flex; flex-direction:column; align-items:center; gap:10px; }
.empty-state svg { opacity:0.4; }
.empty-title { font-size:16px; font-weight:700; color:var(--texto); margin:0; }
.empty-sub { font-size:13px; color:var(--texto-sec); margin:0; }

/* ── Tabla ── */
.table-wrapper { background:var(--blanco); border-radius:16px; border:1px solid var(--borde); overflow:hidden; box-shadow:var(--sombra-sm); }
.table-scroll { overflow-x:auto; -webkit-overflow-scrolling:touch; }
.don-table { width:100%; border-collapse:collapse; min-width:720px; }
.don-table thead th { padding:12px 16px; text-align:left; color:var(--texto-ter); font-size:9.5px; font-weight:700; text-transform:uppercase; letter-spacing:0.6px; white-space:nowrap; }
.don-table tbody tr { border-top:1px solid var(--borde-suave); transition:background 0.15s; }
.don-table tbody tr:hover { background:#FAFBFA; }
.don-table tbody td { padding:12px 16px; vertical-align:middle; }
.id-pill { font-size:11px; font-family:ui-monospace, Menlo, Consolas, monospace; background:var(--fondo); border:1px solid var(--borde); padding:3px 9px; border-radius:6px; color:var(--texto); font-weight:700; white-space:nowrap; }
.fecha-text { font-size:12.5px; color:var(--texto-sec); white-space:nowrap; }
.donor-name { font-size:12.5px; font-weight:700; color:var(--texto); }
.donor-mail-td { font-size:12.5px; color:var(--texto-sec); }
.usr-cell { display:flex; align-items:center; gap:10px; }
.usr-avatar { width:34px; height:34px; border-radius:50%; background:#F1F5F1; display:flex; align-items:center; justify-content:center; flex-shrink:0; border:1px solid var(--borde); }
.usr-avatar-ini { font-size:13px; font-weight:700; color:#4E6E51; text-transform:uppercase; line-height:1; }
.estado-badge { display:inline-block; font-size:10.5px; font-weight:700; padding:4px 11px; border-radius:20px; white-space:nowrap; }
.badge-pendiente { background:#FDF6E8; color:#96650A; }
.badge-aprobada { background:#EDF6EF; color:#2E7D32; }
.badge-rechazada { background:#FBEDEC; color:#B71C1C; }
.badge-inactivo { background:#F2F3F2; color:#7A827B; }
.badge-admin { background:#FDF6E8; color:#A97A0C; }
.badge-blue { background:#EEF1FB; color:#4F73B8; }
.badge-neutral { background:#F2F3F2; color:#7A827B; }
.table-footer { padding:12px 16px; border-top:1px solid var(--borde-suave); font-size:12px; color:var(--texto-sec); font-weight:500; }

/* Botones de acción de la tabla — mismo componente icon-only de Mascotas */
.action-group { display:flex; gap:8px; align-items:center; }
.icon-only {
  width:38px; height:38px; border-radius:8px; border:1px solid var(--borde);
  background:var(--blanco); display:flex; align-items:center; justify-content:center;
  cursor:pointer; transition:background-color .16s ease, border-color .16s ease; position:relative;
}
.icon-only svg { width:16px; height:16px; }
.icon-only--ver { color:#3D453B; }
.icon-only--ver:hover { border-color:#C7D3C8; background:#FAFCFA; }
.icon-only--activar { color:#2E7D45; border-color:#CFE8D6; }
.icon-only--activar:hover { background:#F3FAF5; border-color:#2E7D45; }
.icon-only--inactivar { color:#C0392B; border-color:#F0CFC9; }
.icon-only--inactivar:hover { background:#FDF4F3; border-color:#C0392B; }
.icon-only:disabled { opacity:0.35; cursor:not-allowed; }
.icon-only:disabled:hover { background:var(--blanco); border-color:var(--borde); }
.icon-only::before {
  content:attr(data-tooltip); position:absolute; bottom:calc(100% + 8px); left:50%;
  transform:translateX(-50%) translateY(4px); background:var(--verde); color:#fff;
  font-size:11px; font-weight:600; padding:5px 9px; border-radius:7px; white-space:nowrap;
  opacity:0; visibility:hidden; pointer-events:none; transition:opacity .15s ease, transform .15s ease; z-index:20;
}
.icon-only:hover::before { opacity:1; visibility:visible; transform:translateX(-50%) translateY(0); }

/* ── Toast ── */
.usr-toast { position:fixed; bottom:32px; right:32px; z-index:9999; display:flex; align-items:center; gap:10px; padding:14px 20px; border-radius:14px; font-size:14px; font-weight:600; box-shadow:0 8px 32px rgba(0,0,0,0.16); pointer-events:none; }
.usr-toast--exito { background:var(--verde); color:var(--blanco); }
.usr-toast--error { background:#B71C1C; color:var(--blanco); }
.toast-anim-enter-active, .toast-anim-leave-active { transition:all 0.25s ease; }
.toast-anim-enter-from, .toast-anim-leave-to { opacity:0; transform:translateY(10px); }

/* ══════════════════════════════════════════════
   MODAL BASE
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
.close-btn--hero { background:var(--fondo); }
.close-btn--hero:hover { background:var(--verde); color:#fff; }

/* ── HERO (Ver usuario) ── */
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

/* ── BODY (Ver usuario) ── */
.body { padding:18px 40px 10px; }
.grid-2col { display:grid; grid-template-columns:1.6fr 1fr; gap:14px; align-items:start; margin-bottom:0; }
.block { background:var(--blanco); border:1px solid var(--borde-suave); border-radius:14px; padding:18px 20px; margin-bottom:14px; box-shadow:var(--sombra-sm); }
.block:last-child { margin-bottom:0; }
.block-title { display:flex; align-items:center; gap:10px; font-size:12.5px; font-weight:700; color:var(--texto); text-transform:uppercase; letter-spacing:.4px; margin:0 0 14px; }
.block-title-icon { width:24px; height:24px; border-radius:50%; background:#F0F5F0; color:#4E7A54; display:flex; align-items:center; justify-content:center; flex-shrink:0; }
.fields-row { display:grid; grid-template-columns:repeat(3, 1fr); gap:14px 16px; }
.field-col { display:flex; flex-direction:column; gap:5px; }
.field-label-row { font-size:10px; font-weight:700; color:var(--texto-ter); text-transform:uppercase; letter-spacing:.4px; }
.field-value { font-size:14px; font-weight:600; color:var(--texto); word-break:break-word; }
.info-subsection { margin-top:16px; padding-top:16px; border-top:1px solid var(--borde-suave); }
.info-subsection .field-label-row { display:block; margin-bottom:7px; }
.info-subsection-text { font-size:13px; font-weight:500; color:#4B534A; line-height:1.6; margin:0; }
.block-wide { margin-top:0; }
.tint-box { background:var(--fondo); border-radius:10px; padding:13px 15px; }
.tint-box span { font-size:13px; font-weight:600; color:var(--texto); line-height:1.55; }
.tint-box--desc span { font-weight:500; color:#4B534A; }
.tint-box--warn { background:#FFFBF3; }
.list-col { display:grid; grid-template-columns:1fr; gap:8px; }
.list-item { border:1px solid var(--borde-suave); border-radius:10px; padding:10px 12px; display:flex; align-items:center; gap:10px; }
.list-icon { width:30px; height:30px; border-radius:8px; flex-shrink:0; background:#EDF3EE; color:#3E7A45; display:flex; align-items:center; justify-content:center; }
.list-text { display:flex; flex-direction:column; gap:4px; min-width:0; }
.list-label { font-size:9.5px; font-weight:700; color:var(--texto-ter); text-transform:uppercase; letter-spacing:.4px; }
.list-value { font-size:12.5px; font-weight:700; color:var(--texto); white-space:nowrap; overflow:hidden; text-overflow:ellipsis; }
.list-badge { align-self:flex-start; }

/* ── FOOTER (Ver usuario) ── */
.footer { flex-shrink:0; display:flex; align-items:center; justify-content:flex-end; gap:8px; padding:14px 40px 18px; border-top:1px solid var(--borde-suave); }
.estado-final-msg { flex:1; margin:0; font-size:12.5px; font-weight:700; }
.estado-final-msg--ok { color:#2E7D32; }
.btn-ghost-red { display:flex; align-items:center; gap:6px; height:29px; padding:0 12px; border-radius:8px; background:var(--blanco); border:1px solid var(--borde); color:var(--texto-sec); font-size:11.5px; font-weight:600; cursor:pointer; transition:background-color .16s ease, border-color .16s ease, color .16s ease; }
.btn-ghost-red:hover { background:#FDF4F3; border-color:#E8B9B2; color:var(--rojo); }
.btn-footer-danger { display:flex; align-items:center; height:29px; padding:0 12px; border-radius:8px; background:var(--rojo-bg); border:none; color:var(--rojo); font-size:11.5px; font-weight:600; cursor:pointer; transition:background-color .16s ease, color .16s ease; }
.btn-footer-danger:hover:not(:disabled) { background:var(--rojo); color:#fff; }
.btn-footer-danger:disabled { opacity:0.4; cursor:not-allowed; }
.btn-footer-success { display:flex; align-items:center; height:29px; padding:0 12px; border-radius:8px; background:#EDF6EF; border:none; color:#2E7D32; font-size:11.5px; font-weight:600; cursor:pointer; transition:background-color .16s ease, color .16s ease; }
.btn-footer-success:hover:not(:disabled) { background:#2E7D32; color:#fff; }
.btn-footer-success:disabled { opacity:0.4; cursor:not-allowed; }

/* ══════════════════════════════════════════════
   CONFIRMAR ACTIVAR / DESACTIVAR
   ══════════════════════════════════════════════ */
.confirm-header { flex-shrink:0; padding:24px 32px 16px; display:flex; align-items:center; gap:14px; border-bottom:1px solid var(--borde); background:linear-gradient(165deg, #FFFFFF 0%, #FFFBF3 100%); }
.confirm-icon { width:42px; height:42px; border-radius:11px; flex-shrink:0; background:#FDF6E8; color:#A97A0C; display:flex; align-items:center; justify-content:center; }
.confirm-eyebrow { font-size:11px; font-weight:700; color:#A97A0C; text-transform:uppercase; letter-spacing:.6px; margin:0 0 4px; }
.confirm-title { font-size:17px; font-weight:700; color:var(--texto); margin:0; letter-spacing:-.3px; }
.confirm-body { padding:20px 32px; }
.warn-box { background:#FFFBF3; border-left:3px solid var(--amarillo); border-radius:0 10px 10px 0; padding:14px 16px; font-size:13px; color:var(--texto); line-height:1.7; }
.confirm-footer { flex-shrink:0; display:flex; justify-content:flex-end; gap:10px; padding:14px 32px 18px; border-top:1px solid var(--borde-suave); }
.btn-cancel { height:38px; padding:0 16px; border-radius:9px; background:var(--blanco); border:1px solid var(--borde); color:var(--texto-sec); font-size:13px; font-weight:600; cursor:pointer; transition:background-color .16s ease, border-color .16s ease, color .16s ease; }
.btn-cancel:hover { background:#FAFBFA; color:var(--texto); border-color:#D3D8D3; }
.btn-danger { height:38px; padding:0 16px; border-radius:9px; background:var(--rojo-bg); border:none; color:var(--rojo); font-size:13px; font-weight:600; cursor:pointer; transition:background-color .16s ease, color .16s ease; }
.btn-danger:hover { background:var(--rojo); color:#fff; }

/* Animaciones modal */
.modal-fade-enter-active, .modal-fade-leave-active { transition:opacity 0.22s ease; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity:0; }

/* ── Responsive ── */
@media (max-width:1100px) { .don-summary { grid-template-columns:repeat(3, 1fr); } }
@media (max-width:900px) {
  .don-summary { grid-template-columns:repeat(2, 1fr); }
  .modal-box--uniform { width:94vw; height:88vh; }
  .grid-2col { grid-template-columns:1fr; }
  .fields-row { grid-template-columns:repeat(2, 1fr); }
}
@media (max-width:640px) {
  .page-header { flex-direction:column; align-items:flex-start; }
  .filtros-row { flex-direction:column; gap:14px; }
  .filtros-row--end { align-items:stretch; }
  .filtro-group { min-width:100%; }
  .filtro-group--search { max-width:none; }
  .don-summary { grid-template-columns:1fr 1fr; }
  .modal-box--uniform { width:96vw; height:92vh; border-radius:18px; }
  .hero, .body, .footer { padding-left:20px; padding-right:20px; }
  .fields-row { grid-template-columns:1fr; }
  .don-table th:nth-child(3), .don-table td:nth-child(3), .don-table th:nth-child(5), .don-table td:nth-child(5) { display:none; }
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
