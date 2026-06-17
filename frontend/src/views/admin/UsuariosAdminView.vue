<script setup>
import { ref, computed } from 'vue'
import Icon from '../../components/Icon.vue'

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
      u.correo.toLowerCase().includes(t) ||
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
      <div>
        <h1 class="admin-page-title">Usuarios</h1>
        <p class="admin-page-sub">Control de cuentas y roles del sistema</p>
      </div>
    </header>

    <!-- TARJETAS RESUMEN -->
    <div class="don-summary">
      <div class="don-card total-usuarios">
        <span class="don-label">Total usuarios</span>
        <strong class="don-value">{{ totalUsuarios }}</strong>
      </div>
      <div class="don-card total-activos">
        <span class="don-label">Activos</span>
        <strong class="don-value">{{ totalActivos }}</strong>
      </div>
      <div class="don-card total-inactivos">
        <span class="don-label">Inactivos</span>
        <strong class="don-value">{{ totalInactivos }}</strong>
      </div>
      <div class="don-card total-voluntarios">
        <span class="don-label">Voluntarios</span>
        <strong class="don-value">{{ totalVoluntarios }}</strong>
      </div>
      <div class="don-card total-pendientes">
        <span class="don-label">Solicitudes pendientes</span>
        <strong class="don-value">{{ totalPendientes }}</strong>
      </div>
    </div>

    <!-- FILTROS -->
    <div class="filtros-panel">

      <!-- Buscar usuario -->
      <div class="filtro-group">
        <label class="filtro-label">Buscar usuario</label>
        <div class="filtro-input-wrap">
          <input
            v-model="filtroTexto"
            placeholder="Nombre, correo, cédula o ID..."
            class="filtro-input filtro-input--icon"
          />
          <span class="filtro-icon filtro-icon--right">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
          </span>
        </div>
      </div>

      <!-- Rol -->
      <div class="filtro-group">
        <label class="filtro-label">Rol</label>
        <div class="filtro-input-wrap">
          <select v-model="filtroRol" class="filtro-input filtro-select">
            <option value="Todos">Todos</option>
            <option value="Admin">Admin</option>
            <option value="Voluntario">Voluntario</option>
            <option value="Usuario">Usuario</option>
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
            <option value="Activo">Activos</option>
            <option value="Inactivo">Inactivos</option>
            <option value="Pendiente">Solicitud pendiente</option>
            <option value="Aprobada">Solicitud aprobada</option>
            <option value="Rechazada">Solicitud rechazada</option>
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
    <div v-if="usuariosFiltrados.length === 0" class="empty-state">
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
              <th>Acción</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="u in usuariosFiltrados" :key="u.id" class="don-row">

              <!-- ID -->
              <td><span class="id-pill">{{ u.codigoVoluntario || u.id }}</span></td>

              <!-- Usuario -->
              <td>
                <div class="usr-cell">
                  <div class="usr-avatar">
                    <span class="usr-avatar-ini">{{ iniciales(u.nombre) }}</span>
                  </div>
                  <span class="donor-name">{{ u.nombre }}</span>
                </div>
              </td>

              <!-- Correo -->
              <td><span class="donor-mail-td">{{ u.correo }}</span></td>

              <!-- Rol -->
              <td><span class="estado-badge" :class="rolBadgeClass(u.rol)">{{ u.rol }}</span></td>

              <!-- Solicitud voluntario -->
              <td>
                <span v-if="u.solicitudVoluntario?.estado" class="estado-badge" :class="solicitudBadgeClass(u.solicitudVoluntario.estado)">
                  {{ u.solicitudVoluntario.estado }}
                </span>
                <span v-else class="fecha-text">—</span>
              </td>

              <!-- Estado cuenta -->
              <td><span class="estado-badge" :class="estadoBadgeClass(u)">{{ estadoLabel(u) }}</span></td>

              <!-- Acciones -->
              <td>
                <div class="acciones-cell">
                  <button class="btn-ver" @click="verDetalle(u)" title="Ver detalle">Ver detalle</button>
                  <button
                    class="btn-toggle"
                    :class="u.activo ? 'btn-toggle--desactivar' : 'btn-toggle--activar'"
                    :disabled="u.id === ADMIN_ID"
                    :title="u.activo ? 'Desactivar' : 'Activar'"
                    @click="pedirConfirmacionEstado(u)"
                  >
                    {{ u.activo ? 'Desactivar' : 'Activar' }}
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

    <!-- ═══════════ MODAL DE DETALLE ═══════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showModal && selectedUser" class="modal-overlay" @click.self="cerrarModal">
          <div class="modal-box">

            <button class="modal-close" @click="cerrarModal">✕</button>

            <div class="modal-header">
              <span class="modal-id">{{ selectedUser.codigoVoluntario || selectedUser.id }}</span>
              <span class="estado-badge" :class="estadoBadgeClass(selectedUser)">{{ estadoLabel(selectedUser) }}</span>
              <span class="estado-badge" :class="rolBadgeClass(selectedUser.rol)">{{ selectedUser.rol }}</span>
            </div>

            <!-- Avatar + nombre -->
            <div class="modal-usuario-hero">
              <div class="modal-avatar">
                <span class="modal-avatar-ini">{{ iniciales(selectedUser.nombre) }}</span>
              </div>
              <div>
                <p class="modal-usuario-nombre">{{ selectedUser.nombre }}</p>
                <p class="modal-usuario-correo">{{ selectedUser.correo }}</p>
              </div>
            </div>

            <div class="modal-section">
              <h4 class="modal-section-title">Información personal</h4>
              <div class="modal-grid">
                <div class="modal-field">
                  <span class="modal-field-label">Cédula</span>
                  <strong class="modal-field-value">{{ selectedUser.cedula || '—' }}</strong>
                </div>
                <div class="modal-field">
                  <span class="modal-field-label">Teléfono</span>
                  <strong class="modal-field-value">{{ selectedUser.telefono || '—' }}</strong>
                </div>
                <div class="modal-field">
                  <span class="modal-field-label">País</span>
                  <strong class="modal-field-value">{{ selectedUser.pais || '—' }}</strong>
                </div>
                <div class="modal-field">
                  <span class="modal-field-label">Dirección</span>
                  <strong class="modal-field-value">{{ selectedUser.direccion || '—' }}</strong>
                </div>
              </div>
            </div>

            <div class="modal-section">
              <h4 class="modal-section-title">Rol y estado</h4>
              <div class="modal-grid">
                <div class="modal-field">
                  <span class="modal-field-label">Rol</span>
                  <span class="estado-badge" :class="rolBadgeClass(selectedUser.rol)" style="margin-top:4px;display:inline-block">{{ selectedUser.rol }}</span>
                </div>
                <div class="modal-field">
                  <span class="modal-field-label">Tipo de voluntario</span>
                  <strong class="modal-field-value">{{ selectedUser.tipoVoluntario || '—' }}</strong>
                </div>
                <div class="modal-field">
                  <span class="modal-field-label">Estado de cuenta</span>
                  <span class="estado-badge" :class="estadoBadgeClass(selectedUser)" style="margin-top:4px;display:inline-block">{{ estadoLabel(selectedUser) }}</span>
                </div>
                <div class="modal-field">
                  <span class="modal-field-label">Solicitud voluntariado</span>
                  <span
                    v-if="selectedUser.solicitudVoluntario?.estado"
                    class="estado-badge"
                    :class="solicitudBadgeClass(selectedUser.solicitudVoluntario.estado)"
                    style="margin-top:4px;display:inline-block"
                  >{{ selectedUser.solicitudVoluntario.estado }}</span>
                  <strong v-else class="modal-field-value">—</strong>
                </div>
              </div>
            </div>

            <!-- Nota informativa -->
            <div class="modal-info-note">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
              El rol se actualiza automáticamente desde <strong>Solicitudes de Voluntariado</strong> al aprobar o rechazar una postulación.
            </div>

            <div v-if="selectedUser.id !== ADMIN_ID" class="modal-acciones">
              <button
                class="btn-aprobar"
                :disabled="selectedUser.activo"
                @click="() => { selectedUser.activo = true; guardarUsuarios(); cerrarModal() }"
              >
                Activar usuario
              </button>
              <button
                class="btn-rechazar"
                :disabled="!selectedUser.activo"
                @click="() => { selectedUser.activo = false; guardarUsuarios(); cerrarModal() }"
              >
                Desactivar usuario
              </button>
            </div>
            <div v-else class="modal-estado-final">
              <p class="estado-aprobada-msg">Esta es la cuenta de administrador principal.</p>
            </div>

          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- ═══════════ MODAL DE CONFIRMACIÓN ═══════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="modalConfirm" class="modal-overlay" @click.self="cancelarConfirmacion">
          <div class="modal-box modal-box--sm">

            <button class="modal-close" @click="cancelarConfirmacion">✕</button>

            <div class="confirm-icon-wrap">
              <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>
            </div>

            <h3 class="confirm-title">
              {{ usuarioSeleccionado?.activo ? 'Desactivar usuario' : 'Activar usuario' }}
            </h3>
            <p class="confirm-text">{{ usuarioSeleccionado?.nombre }}</p>

            <div class="modal-acciones">
              <button class="btn-rechazar" style="flex:none;padding:13px 24px" @click="cancelarConfirmacion">Cancelar</button>
              <button class="btn-aprobar" style="flex:1" @click="confirmarCambioEstado">Confirmar</button>
            </div>

          </div>
        </div>
      </Transition>
    </Teleport>

  </div>
</template>

<style scoped>
/* ── Variables (idénticas a Donaciones) ─────────────────── */
.view-container {
  --verde:     #3A473C;
  --verde-sec: #92A894;
  --fondo:     #F7F8F7;
  --blanco:    #FFFFFF;
  --texto:     #2F352F;
  --texto-sec: #6C756D;
  --borde:     #E8ECE8;
  --amarillo:  #F5B942;
  --verde-ok:  #4CAF6A;
  background: transparent;
}

/* ── Encabezado ─────────────────────────────────────────── */
.page-header       { margin-bottom: 28px; }
.admin-page-title  { font-size: 28px; font-weight: 800; color: var(--verde); letter-spacing: -0.5px; line-height: 1.1; }
.admin-page-sub    { font-size: 14px; color: var(--texto-sec); margin-top: 4px; font-weight: 500; }

/* ── Tarjetas resumen ───────────────────────────────────── */
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

.total-usuarios  { border-top-color: var(--verde); }
.total-activos   { border-top-color: var(--verde-ok); }
.total-inactivos { border-top-color: #E57373; }
.total-voluntarios { border-top-color: var(--verde-sec); }
.total-pendientes  { border-top-color: var(--amarillo); }

.don-label { font-size: 11px; color: var(--texto-sec); font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; }
.don-value { font-size: 24px; font-weight: 800; color: var(--verde); line-height: 1; }

/* ── Panel de filtros ───────────────────────────────────── */
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
  background: var(--fondo);
  font-size: 13px;
  color: var(--texto);
  font-family: inherit;
  outline: none;
  transition: border-color 0.18s, background 0.18s;
  box-sizing: border-box;
}

.filtro-input:focus {
  border-color: var(--verde-sec);
  background: var(--blanco);
}

.filtro-input::placeholder { color: #9CA8A0; }

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

.filtro-icon--right       { right: 11px; }
.filtro-icon--no-events   { pointer-events: none; }

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

.btn-limpiar--activo  { border-color: var(--verde); color: var(--verde); }
.btn-limpiar:hover    { background: var(--verde); color: var(--blanco); border-color: var(--verde); }

/* ── Estado vacío ───────────────────────────────────────── */
.empty-state {
  text-align: center;
  padding: 72px 24px;
  background: var(--blanco);
  border-radius: 14px;
  border: 1px solid var(--borde);
}

.empty-title { font-size: 16px; font-weight: 700; color: var(--texto); margin-bottom: 6px; }
.empty-sub   { font-size: 13px; color: var(--texto-sec); }

/* ── Tabla ──────────────────────────────────────────────── */
.table-wrapper {
  background: var(--blanco);
  border-radius: 14px;
  border: 1px solid var(--borde);
  overflow: hidden;
}

.table-scroll          { overflow-x: auto; -webkit-overflow-scrolling: touch; }

.don-table             { width: 100%; border-collapse: collapse; min-width: 720px; }
.don-table thead tr    { background: var(--verde); }
.don-table thead th    { padding: 13px 16px; text-align: left; color: var(--blanco); font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.6px; white-space: nowrap; }
.don-table tbody tr    { border-bottom: 1px solid var(--borde); transition: background 0.15s; }
.don-table tbody tr:last-child { border-bottom: none; }
.don-table tbody tr:hover      { background: #F4F6F4; }
.don-table tbody td    { padding: 13px 16px; vertical-align: middle; }

.id-pill     { font-size: 11px; font-family: monospace; background: var(--fondo); border: 1px solid var(--borde); padding: 3px 9px; border-radius: 6px; color: var(--verde); font-weight: 700; white-space: nowrap; }
.fecha-text  { font-size: 13px; color: var(--texto-sec); white-space: nowrap; }
.donor-name  { font-size: 13px; font-weight: 700; color: var(--texto); }
.donor-mail-td { font-size: 13px; color: var(--texto-sec); }

/* Avatar en tabla */
.usr-cell {
  display: flex;
  align-items: center;
  gap: 10px;
}

.usr-avatar {
  width: 34px;
  height: 34px;
  border-radius: 50%;
  background: #DDE6DE;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.usr-avatar-ini {
  font-size: 13px;
  font-weight: 800;
  color: #5A6E5C;
  text-transform: uppercase;
  line-height: 1;
}

/* Acciones en tabla */
.acciones-cell {
  display: flex;
  align-items: center;
  gap: 6px;
  flex-wrap: nowrap;
}

.table-footer { padding: 12px 16px; border-top: 1px solid var(--borde); font-size: 12px; color: var(--texto-sec); font-weight: 500; }

.btn-ver {
  padding: 6px 14px;
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

.btn-toggle {
  padding: 6px 14px;
  border-radius: 7px;
  border: 1.5px solid var(--borde);
  background: transparent;
  font-size: 12px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.18s;
  white-space: nowrap;
  font-family: inherit;
}

.btn-toggle:disabled { opacity: 0.35; cursor: not-allowed; }

.btn-toggle--activar {
  border-color: #C8E6C9;
  color: #2E7D32;
  background: #E8F5E9;
}
.btn-toggle--activar:hover:not(:disabled) { background: #2E7D32; color: var(--blanco); border-color: #2E7D32; }

.btn-toggle--desactivar {
  border-color: #FFCDD2;
  color: #B71C1C;
  background: #FDECEA;
}
.btn-toggle--desactivar:hover:not(:disabled) { background: #B71C1C; color: var(--blanco); border-color: #B71C1C; }

/* ── Badges ─────────────────────────────────────────────── */
.estado-badge    { display: inline-block; font-size: 11px; font-weight: 700; padding: 4px 12px; border-radius: 20px; white-space: nowrap; }
.badge-pendiente { background: #FFF7E0; color: #96650A; }
.badge-aprobada  { background: #E8F5E9; color: #2E7D32; }
.badge-rechazada { background: #FDECEA; color: #B71C1C; }
.badge-inactivo  { background: #FFF3E0; color: #E65100; }
.badge-admin     { background: rgba(249,193,122,.18); color: #D18C3A; }
.badge-blue      { background: rgba(33,150,243,.13); color: #1565C0; }
.badge-neutral   { background: #F4F6F4; color: #6C756D; }

/* ── Toast ──────────────────────────────────────────────── */
.usr-toast {
  position: fixed; bottom: 32px; right: 32px; z-index: 9999;
  display: flex; align-items: center; gap: 10px;
  padding: 14px 20px; border-radius: 14px;
  font-size: 14px; font-weight: 600;
  box-shadow: 0 8px 32px rgba(0,0,0,0.16); pointer-events: none;
}
.usr-toast--exito { background: var(--verde); color: var(--blanco); }
.usr-toast--error { background: #B71C1C; color: var(--blanco); }
.toast-anim-enter-active, .toast-anim-leave-active { transition: all 0.25s ease; }
.toast-anim-enter-from, .toast-anim-leave-to { opacity: 0; transform: translateY(10px); }

/* ── Modal ──────────────────────────────────────────────── */
.modal-overlay {
  position: fixed; inset: 0;
  background: rgba(0,0,0,0.35);
  backdrop-filter: blur(4px);
  z-index: 1000;
  display: flex; align-items: center; justify-content: center;
  padding: 24px;
}

.modal-box {
  background: #FFFFFF;
  border-radius: 20px;
  padding: 36px;
  width: 100%; max-width: 620px;
  max-height: 90vh; overflow-y: auto;
  position: relative;
}

.modal-box--sm {
  max-width: 400px;
  text-align: center;
}

.modal-close {
  position: absolute; top: 18px; right: 18px;
  width: 32px; height: 32px; border-radius: 50%;
  border: none; background: var(--fondo);
  color: var(--texto); font-size: 13px; font-weight: 700;
  cursor: pointer; display: flex; align-items: center; justify-content: center;
  transition: all 0.2s; font-family: inherit;
}
.modal-close:hover { background: var(--verde); color: var(--blanco); }

.modal-header { display: flex; align-items: center; gap: 10px; margin-bottom: 20px; }

.modal-id {
  font-size: 13px; font-family: monospace;
  background: var(--fondo); border: 1px solid var(--borde);
  padding: 5px 11px; border-radius: 7px;
  color: var(--verde); font-weight: 700;
}

/* Avatar hero en modal */
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

.modal-section       { margin-bottom: 24px; }
.modal-section-title {
  font-size: 11px; font-weight: 700; color: var(--texto-sec);
  text-transform: uppercase; letter-spacing: 0.5px;
  margin-bottom: 14px; padding-bottom: 10px;
  border-bottom: 1px solid var(--borde);
}

.modal-grid   { display: grid; grid-template-columns: repeat(2,1fr); gap: 16px; }
.modal-field  { display: flex; flex-direction: column; gap: 4px; }
.modal-field-label { font-size: 10px; font-weight: 700; color: #9CA8A0; text-transform: uppercase; letter-spacing: 0.4px; }
.modal-field-value { font-size: 14px; color: var(--texto); font-weight: 600; word-break: break-word; }

/* Nota informativa en modal */
.modal-info-note {
  display: flex;
  align-items: flex-start;
  gap: 10px;
  padding: 12px 16px;
  background: rgba(146,168,148,.10);
  border-radius: 10px;
  font-size: 12px;
  color: #5A6E5C;
  line-height: 1.6;
  margin-bottom: 20px;
}

.modal-info-note svg { flex-shrink: 0; margin-top: 1px; color: #5A6E5C; }

.modal-acciones {
  display: flex; gap: 10px;
  padding-top: 24px; border-top: 1px solid var(--borde); margin-top: 8px;
}

.btn-aprobar {
  flex: 1; padding: 13px; border-radius: 10px; border: none;
  background: #E8F5E9; color: #2E7D32;
  font-size: 13px; font-weight: 700; cursor: pointer;
  transition: all 0.2s; font-family: inherit;
}
.btn-aprobar:hover:not(:disabled) { background: #2E7D32; color: var(--blanco); }
.btn-aprobar:disabled { opacity: 0.4; cursor: not-allowed; }

.btn-rechazar {
  flex: 1; padding: 13px; border-radius: 10px; border: none;
  background: #FDECEA; color: #B71C1C;
  font-size: 13px; font-weight: 700; cursor: pointer;
  transition: all 0.2s; font-family: inherit;
}
.btn-rechazar:hover:not(:disabled) { background: #B71C1C; color: var(--blanco); }
.btn-rechazar:disabled { opacity: 0.4; cursor: not-allowed; }

.modal-estado-final { padding-top: 20px; border-top: 1px solid var(--borde); text-align: center; }
.estado-aprobada-msg  { color: #2E7D32; font-weight: 700; font-size: 14px; }
.estado-rechazada-msg { color: #B71C1C; font-weight: 700; font-size: 14px; }

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
  margin: 0 0 8px; text-align: center;
}

.confirm-text {
  font-size: 14px; color: var(--texto-sec);
  margin: 0 0 24px; text-align: center;
}

/* ── Animaciones ────────────────────────────────────────── */
.modal-fade-enter-active, .modal-fade-leave-active { transition: opacity 0.22s ease; }
.modal-fade-enter-from, .modal-fade-leave-to       { opacity: 0; }

/* ── Responsive ─────────────────────────────────────────── */
@media (max-width: 900px) {
  .don-summary { display: grid; grid-template-columns: repeat(2,1fr); }
  .total-pendientes { grid-column: span 2; }
}

@media (max-width: 640px) {
  .filtros-panel     { flex-direction: column; }
  .filtro-group      { min-width: 100%; }
  .filtro-group--btn { width: 100%; }
  .btn-limpiar       { width: 100%; }
  .modal-grid        { grid-template-columns: 1fr; }
  .modal-box         { padding: 24px 20px; }
  .modal-acciones    { flex-direction: column; }
  .don-summary       { grid-template-columns: 1fr; }
  .total-pendientes  { grid-column: span 1; }
  .acciones-cell     { flex-direction: column; align-items: flex-start; }
}

/* ── MOBILE RESPONSIVE ── */
@media (max-width: 768px) {
  .don-summary {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 10px;
  }

  .total-pendientes { grid-column: span 2; }

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
  .btn-toggle {
    width: 100%;
    text-align: center;
    justify-content: center;
  }

  .modal-box {
    padding: 22px 16px;
    max-height: 95vh;
  }

  .modal-grid { grid-template-columns: 1fr; }

  .modal-acciones {
    flex-direction: column;
  }

  .btn-aprobar,
  .btn-rechazar {
    width: 100%;
  }

  .modal-usuario-hero {
    flex-direction: column;
    align-items: flex-start;
    gap: 10px;
    padding: 12px 14px;
  }
}

@media (max-width: 480px) {
  .don-summary { grid-template-columns: 1fr; }
  .total-pendientes { grid-column: span 1; }

  .don-table th:nth-child(3),
  .don-table td:nth-child(3),
  .don-table th:nth-child(6),
  .don-table td:nth-child(6) { display: none; }
}

</style>