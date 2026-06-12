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
  if (rol === 'Admin')      return 'badge--peach'
  if (rol === 'Voluntario') return 'badge--green'
  return 'badge--blue'
}

function solicitudBadgeClass(estado) {
  if (estado === 'Aprobada')  return 'badge--green'
  if (estado === 'Rechazada') return 'badge--red'
  if (estado === 'Pendiente') return 'badge--yellow'
  return 'badge--neutral'
}

function estadoBadgeClass(user) {
  return user.activo ? 'badge--green' : 'badge--orange'
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
        <h1 class="sc-title">Usuarios</h1>
        <p class="sc-sub">Control de cuentas y roles del sistema</p>
      </div>
    </header>

    <!-- ── Toolbar ── -->
    <div class="sc-toolbar">
      <div class="sc-filters">

        <!-- Búsqueda -->
        <div class="sc-search-wrap">
          <svg class="sc-search-icon" xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
          <input class="sc-search" v-model="filtroTexto" placeholder="Nombre, correo, cédula o ID..." />
        </div>

        <!-- Filtro rol -->
        <div class="sc-select-wrap">
          <select class="sc-filter-select" v-model="filtroRol">
            <option value="Todos">Rol: Todos</option>
            <option value="Admin">Admin</option>
            <option value="Voluntario">Voluntario</option>
            <option value="Usuario">Usuario</option>
          </select>
          <svg class="sc-select-icon" xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
        </div>

        <!-- Filtro estado -->
        <div class="sc-select-wrap">
          <select class="sc-filter-select" v-model="filtroEstado">
            <option value="Todos">Todos los estados</option>
            <option value="Activo">Activos</option>
            <option value="Inactivo">Inactivos</option>
            <option value="Pendiente">Solicitud pendiente</option>
            <option value="Aprobada">Solicitud aprobada</option>
            <option value="Rechazada">Solicitud rechazada</option>
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
            <th style="width:60px">ID</th>
            <th style="width:220px">Usuario</th>
            <th style="width:220px">Correo</th>
            <th style="width:120px">Rol</th>
            <th style="width:130px">Solicitud</th>
            <th style="width:110px">Estado</th>
            <th style="width:110px">Acciones</th>
          </tr>
        </thead>
        <tbody>

          <tr v-for="u in usuariosFiltrados" :key="u.id">

            <!-- ID -->
            <td>
              <span class="sc-pet-id" style="font-size:11px">{{ u.codigoVoluntario || u.id }}</span>
            </td>

            <!-- Usuario -->
            <td>
              <div class="sc-pet-cell">
                <div class="sc-avatar">
                  <span class="sc-avatar-ini">{{ iniciales(u.nombre) }}</span>
                </div>
                <div class="sc-pet-info">
                  <span class="sc-pet-name">{{ u.nombre }}</span>
                </div>
              </div>
            </td>

            <!-- Correo -->
            <td>
              <span class="sc-td-main">{{ u.correo }}</span>
            </td>

            <!-- Rol -->
            <td>
              <span class="sc-badge" :class="rolBadgeClass(u.rol)">{{ u.rol }}</span>
            </td>

            <!-- Solicitud voluntario -->
            <td>
              <span v-if="u.solicitudVoluntario?.estado" class="sc-badge" :class="solicitudBadgeClass(u.solicitudVoluntario.estado)">
                {{ u.solicitudVoluntario.estado }}
              </span>
              <span v-else class="sc-td-sec">—</span>
            </td>

            <!-- Estado cuenta -->
            <td>
              <span class="sc-badge" :class="estadoBadgeClass(u)">
                {{ estadoLabel(u) }}
              </span>
            </td>

            <!-- Acciones -->
            <td>
              <div class="sc-actions">
                <button class="sc-btn-ver sc-btn-ver--neutral" @click="verDetalle(u)" title="Ver detalle">
                  <img src="/img-acciones/eye.png" class="action-icon" alt="Ver">
                </button>
                <button
                  class="sc-btn-ver"
                  :class="u.activo ? 'sc-btn-ver--orange' : 'sc-btn-ver--green'"
                  :disabled="u.id === ADMIN_ID"
                  :title="u.activo ? 'Desactivar' : 'Activar'"
                  @click="pedirConfirmacionEstado(u)"
                >
                  <img :src="u.activo ? '/img-acciones/unlock.png' : '/img-acciones/padlock.png'" class="action-icon" :alt="u.activo ? 'Desactivar' : 'Activar'">
                </button>
              </div>
            </td>

          </tr>

          <tr v-if="usuariosFiltrados.length === 0">
            <td colspan="7" class="sc-empty">
              <div class="sc-empty-inner">
                <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
                <p>{{ hayFiltros ? 'Sin resultados para los filtros aplicados' : 'No hay registros para mostrar' }}</p>
              </div>
            </td>
          </tr>

        </tbody>
      </table>
    </div>

    <!-- ══ MODAL VER DETALLE ══ -->
    <Teleport to="body">
      <Transition name="overlay-anim">
        <div v-if="showModal" class="sc-overlay" @click.self="cerrarModal">
          <div class="sc-modal sc-modal--lg">

            <div class="exp-header">
              <div class="exp-avatar">{{ iniciales(selectedUser?.nombre) }}</div>
              <div class="exp-header-info">
                <div class="exp-name">{{ selectedUser?.nombre }}</div>
                <div class="exp-meta">
                  <span class="sc-badge" :class="rolBadgeClass(selectedUser?.rol)">{{ selectedUser?.rol }}</span>
                  <span class="sc-badge" :class="estadoBadgeClass(selectedUser)">{{ estadoLabel(selectedUser) }}</span>
                </div>
              </div>
              <button class="sc-modal-close" @click="cerrarModal">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
              </button>
            </div>

            <div class="sc-modal-body exp-body" v-if="selectedUser">

              <!-- Información personal -->
              <div class="exp-section">
                <div class="exp-section-title"><span class="exp-section-dot"></span>Información personal</div>
                <div class="exp-grid">
                  <div class="exp-field"><span class="exp-label">ID / Código</span><span class="exp-value fw">{{ selectedUser.codigoVoluntario || selectedUser.id }}</span></div>
                  <div class="exp-field"><span class="exp-label">Cédula</span><span class="exp-value">{{ selectedUser.cedula || '—' }}</span></div>
                  <div class="exp-field"><span class="exp-label">Correo electrónico</span><span class="exp-value">{{ selectedUser.correo }}</span></div>
                  <div class="exp-field"><span class="exp-label">Teléfono</span><span class="exp-value">{{ selectedUser.telefono || '—' }}</span></div>
                </div>
              </div>

              <!-- Ubicación -->
              <div class="exp-section">
                <div class="exp-section-title"><span class="exp-section-dot"></span>Ubicación</div>
                <div class="exp-grid">
                  <div class="exp-field"><span class="exp-label">País</span><span class="exp-value">{{ selectedUser.pais || '—' }}</span></div>
                  <div class="exp-field"><span class="exp-label">Dirección</span><span class="exp-value">{{ selectedUser.direccion || '—' }}</span></div>
                </div>
              </div>

              <!-- Rol y estado -->
              <div class="exp-section">
                <div class="exp-section-title"><span class="exp-section-dot"></span>Rol y estado</div>
                <div class="exp-grid">
                  <div class="exp-field"><span class="exp-label">Rol</span><span class="sc-badge" :class="rolBadgeClass(selectedUser.rol)" style="margin-top:4px">{{ selectedUser.rol }}</span></div>
                  <div class="exp-field"><span class="exp-label">Tipo de voluntario</span><span class="exp-value">{{ selectedUser.tipoVoluntario || '—' }}</span></div>
                  <div class="exp-field"><span class="exp-label">Estado de cuenta</span><span class="sc-badge" :class="estadoBadgeClass(selectedUser)" style="margin-top:4px">{{ estadoLabel(selectedUser) }}</span></div>
                  <div class="exp-field">
                    <span class="exp-label">Estado de solicitud</span>
                    <span v-if="selectedUser.solicitudVoluntario?.estado" class="sc-badge" :class="solicitudBadgeClass(selectedUser.solicitudVoluntario.estado)" style="margin-top:4px">{{ selectedUser.solicitudVoluntario.estado }}</span>
                    <span v-else class="exp-value">—</span>
                  </div>
                </div>
              </div>

            </div>

            <div class="sc-modal-footer" v-if="selectedUser?.id !== ADMIN_ID" style="justify-content:space-between;align-items:center;flex-wrap:wrap;gap:10px">
              <button class="sc-btn-cancel" @click="cerrarModal">Cerrar</button>
              <div style="display:flex;gap:8px">
                <button
                  class="sc-btn-ver sc-btn-ver--green"
                  :disabled="selectedUser?.activo"
                  style="padding:8px 16px;height:auto;font-size:13px"
                  @click="() => { selectedUser.activo = true; guardarUsuarios(); cerrarModal() }"
                >
                  <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
                  Activar
                </button>
                <button
                  class="sc-btn-save"
                  style="background: #c0392b"
                  :disabled="!selectedUser?.activo"
                  @click="() => { selectedUser.activo = false; guardarUsuarios(); cerrarModal() }"
                >
                  <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
                  Desactivar
                </button>
              </div>
            </div>

            <div v-else class="sc-modal-footer">
              <button class="sc-btn-cancel" @click="cerrarModal">Cerrar</button>
            </div>

            <!-- Nota informativa -->
            <div class="sc-info-note">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
              El rol se actualiza automáticamente desde <strong>Solicitudes de Voluntariado</strong> al aprobar o rechazar una postulación.
            </div>

          </div>
        </div>
      </Transition>
    </Teleport>

<!-- ══ MODAL CONFIRMACIÓN ══ -->
<Teleport to="body">
  <Transition name="overlay-anim">
    <div
      v-if="modalConfirm"
      class="sc-overlay"
      @click.self="cancelarConfirmacion"
    >
      <div class="sc-confirm-modal">

        <div class="sc-confirm-icon">
          <img
            :src="usuarioSeleccionado?.activo
              ? '/img-acciones/warning.png'
              : '/img-acciones/warning.png'"
            alt=""
          >
        </div>

        <h3 class="sc-confirm-title">
          {{ usuarioSeleccionado?.activo
            ? 'Desactivar usuario'
            : 'Activar usuario' }}
        </h3>

        <p class="sc-confirm-text">
          {{ usuarioSeleccionado?.nombre }}
        </p>

        <div class="sc-confirm-actions">
          <button
            class="sc-btn-cancel"
            @click="cancelarConfirmacion"
          >
            Cancelar
          </button>

          <button
            class="sc-btn-save"
            @click="confirmarCambioEstado"
          >
            Confirmar
          </button>
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
   TOAST
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
   HEADER
═══════════════════════════════════════ */
.sc-header {
  display: flex; justify-content: space-between; align-items: flex-start;
  margin-bottom: 28px; gap: 16px; flex-wrap: wrap;
}
.sc-title { font-size: 28px; font-weight: 800; color: #3A473C; letter-spacing: -0.5px; line-height: 1.1; }
.sc-sub   { font-size: 14px; color: #6C756D; margin-top: 5px; font-weight: 500; }

/* ═══════════════════════════════════════
   TOOLBAR
═══════════════════════════════════════ */
.sc-toolbar {
  display: flex; align-items: center; gap: 16px;
  margin-bottom: 20px; flex-wrap: nowrap;
}

.sc-filters {
  display: flex; align-items: center; gap: 10px;
  flex: 1; flex-wrap: nowrap; min-width: 0;
}

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

.sc-select-wrap { position: relative; flex-shrink: 0; }
.sc-filter-select {
  appearance: none;
  padding: 0 32px 0 12px; height: 36px;
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

.sc-clear {
  padding: 0 14px; height: 36px;
  border: 1.5px solid #fdd; border-radius: 10px;
  background: #fff5f5; color: #c0392b;
  font-size: 12px; font-weight: 700; font-family: inherit;
  cursor: pointer; transition: background 0.15s; white-space: nowrap; flex-shrink: 0;
}
.sc-clear:hover { background: #ffe5e5; }

/* ═══════════════════════════════════════
   TABLA
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

/* Avatar */
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

.sc-td-main { font-weight: 500; color: #3A473C; font-size: 13px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; display: block; }
.sc-td-sec  { color: #7A8A7C; font-size: 12px; }

/* ── Badges ── */
.sc-badge {
  display: inline-flex; align-items: center; gap: 5px;
  padding: 4px 10px; border-radius: 7px;
  font-size: 12px; font-weight: 600; white-space: nowrap;
  background: #F0F4F0; color: #4A6550;
}
.badge--green   { background: rgba(76,175,80,.14);   color: #2E7D32; }
.badge--red     { background: rgba(235,119,119,.16);  color: #C45252; }
.badge--orange  { background: rgba(255,152,0,.14);    color: #E65100; }
.badge--yellow  { background: rgba(255,193,7,.16);    color: #9A6A00; }
.badge--blue    { background: rgba(33,150,243,.13);   color: #1565C0; }
.badge--peach   { background: rgba(249,193,122,.18);  color: #D18C3A; }
.badge--purple  { background: rgba(156,39,176,.13);   color: #7B1FA2; }
.badge--neutral { background: #F4F6F4;                color: #6C756D; }

/* ── Botones de acción ── */
.sc-actions {
  display: flex; align-items: center; gap: 4px; justify-content: center;
}
.sc-btn-ver {
  display: inline-flex; align-items: center; gap: 4px;
  padding: 5px 11px; height: 28px;
  border: none; border-radius: 8px;
  font-size: 12px; font-weight: 700; font-family: inherit;
  cursor: pointer; transition: background 0.15s, opacity 0.15s;
  white-space: nowrap; flex-shrink: 0;
}
.sc-btn-ver:disabled { opacity: 0.35; cursor: not-allowed; }

.action-icon { width: 10px; height: 10px; object-fit: contain; display: block; }

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

/* Empty state */
.sc-empty { padding: 0; }
.sc-empty-inner {
  display: flex; flex-direction: column; align-items: center;
  justify-content: center; gap: 12px; padding: 56px 24px; color: #92A894;
}
.sc-empty-inner svg { opacity: 0.4; }
.sc-empty-inner p { font-size: 14px; font-weight: 500; color: #7A8A7C; margin: 0; }

/* ═══════════════════════════════════════
   OVERLAY / MODAL
═══════════════════════════════════════ */
.sc-overlay {
  position: fixed; inset: 0; background: rgba(20,30,22,0.5);
  display: flex; align-items: center; justify-content: center;
  z-index: 200; padding: 20px; backdrop-filter: blur(2px); overflow-y: auto;
}
.overlay-anim-enter-active, .overlay-anim-leave-active { transition: all 0.22s ease; }
.overlay-anim-enter-from, .overlay-anim-leave-to { opacity: 0; }
.overlay-anim-enter-from .sc-modal, .overlay-anim-leave-to .sc-modal { transform: translateY(16px) scale(0.98); }
.sc-modal {
  background: #fff; border-radius: 22px; width: 100%;
  max-height: 88vh; overflow-y: auto;
  box-shadow: 0 24px 80px rgba(0,0,0,0.2);
  transition: transform 0.22s ease; margin: auto;
}
.sc-modal--lg { max-width: 720px; }

.sc-confirm-modal {
  width: 340px;
  background: white;
  border-radius: 20px;
  padding: 24px;
  text-align: center;
  box-shadow: 0 20px 50px rgba(0,0,0,.18);
}

.sc-confirm-icon {
  width: 60px;
  height: 60px;
  margin: 0 auto 14px;
  border-radius: 16px;
  background: #F4F6F4;
  display: flex;
  align-items: center;
  justify-content: center;
}

.sc-confirm-icon img {
  width: 28px;
  height: 28px;
}

.sc-confirm-title {
  font-size: 18px;
  font-weight: 800;
  color: #3A473C;
  margin-bottom: 6px;
}

.sc-confirm-text {
  font-size: 14px;
  color: #6C756D;
  margin-bottom: 20px;
}

.sc-confirm-actions {
  display: flex;
  gap: 10px;
}

.sc-confirm-actions button {
  flex: 1;
}

/* Cabecera del modal (estilo expediente) */
.exp-header {
  display: flex; align-items: center; gap: 18px;
  padding: 26px 28px 22px; border-bottom: 1.5px solid #F0F2F0;
  background: linear-gradient(135deg, #F7F9F7, white);
}
.exp-avatar {
  width: 64px; height: 64px; min-width: 64px; border-radius: 18px;
  background: #DDE6DE; color: #5A6E5C;
  font-size: 22px; font-weight: 800;
  display: flex; align-items: center; justify-content: center;
  box-shadow: 0 8px 20px rgba(58,71,60,0.12);
}
.exp-header-info { flex: 1; min-width: 0; }
.exp-name { font-size: 20px; font-weight: 800; color: #3A473C; margin-bottom: 8px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.exp-meta { display: flex; gap: 8px; flex-wrap: wrap; }

.sc-modal-close {
  width: 34px; height: 34px; border-radius: 10px;
  border: 1.5px solid #E8ECE8; background: #fff; color: #6C756D;
  cursor: pointer; display: flex; align-items: center; justify-content: center;
  transition: background 0.15s, border-color 0.15s; flex-shrink: 0;
}
.sc-modal-close:hover { background: #F4F6F4; border-color: #ccc; }

.sc-modal-body { padding: 24px 28px 8px; }
.exp-body { display: flex; flex-direction: column; gap: 0; }

.exp-section { border-bottom: 1.5px solid #F4F6F4; padding: 20px 0; }
.exp-section:last-child { border-bottom: none; }
.exp-section-title {
  display: flex; align-items: center; gap: 9px;
  font-size: 11px; font-weight: 800; letter-spacing: 0.10em;
  text-transform: uppercase; color: #92A894; margin-bottom: 16px;
}
.exp-section-dot { width: 7px; height: 7px; border-radius: 50%; background: #92A894; flex-shrink: 0; }
.exp-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 14px 20px; }
.exp-field { display: flex; flex-direction: column; gap: 4px; }
.exp-label { font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.06em; color: #92A894; }
.exp-value { font-size: 14px; color: #3A473C; }
.exp-value.fw { font-weight: 700; }

/* Footer modal */
.sc-modal-footer {
  display: flex; justify-content: flex-end; gap: 10px;
  padding: 18px 28px 0; border-top: 1.5px solid #F0F2F0; margin-top: 12px;
}
.sc-btn-cancel {
  padding: 10px 18px; background: #F4F6F4; border: none; border-radius: 10px;
  font-size: 13px; font-weight: 700; color: #6C756D; cursor: pointer;
  transition: background 0.15s; font-family: inherit;
}
.sc-btn-cancel:hover { background: #E5EAE6; }
.sc-btn-save {
  display: flex; align-items: center; gap: 7px; padding: 10px 20px;
  background: #3A473C; border: none; border-radius: 10px;
  font-size: 13px; font-weight: 700; color: #fff; cursor: pointer;
  transition: background 0.18s; font-family: inherit;
}
.sc-btn-save:hover { background: #2d3730; }
.sc-btn-save:disabled { opacity: 0.4; cursor: not-allowed; }

/* Nota informativa */
.sc-info-note {
  display: flex; align-items: flex-start; gap: 10px;
  margin: 12px 28px 24px; padding: 14px 18px;
  background: rgba(146,168,148,.10);
  border-radius: 14px; font-size: 13px; color: #5A6E5C; line-height: 1.6;
}
.sc-info-note svg { flex-shrink: 0; margin-top: 1px; color: #5A6E5C; }

/* ═══════════════════════════════════════
   RESPONSIVE
═══════════════════════════════════════ */
@media (max-width: 1100px) {
  .sc-toolbar { flex-wrap: wrap; }
  .sc-filters  { flex-wrap: wrap; }
}
@media (max-width: 900px) {
  .sc-table th:nth-child(5),
  .sc-table td:nth-child(5) { display: none; }
}
@media (max-width: 640px) {
  .sc-header  { flex-direction: column; align-items: flex-start; }
  .sc-toolbar { flex-direction: column; align-items: flex-start; }
  .sc-filters { width: 100%; flex-wrap: wrap; }
  .sc-search-wrap { max-width: 100%; }
  .sc-table th:nth-child(1),
  .sc-table td:nth-child(1) { display: none; }
  .sc-table th:nth-child(4),
  .sc-table td:nth-child(4) { display: none; }
  .exp-grid { grid-template-columns: 1fr; }
  .sc-modal-body   { padding: 16px 18px 8px; }
  .sc-modal-footer,
  .exp-header      { padding-left: 18px; padding-right: 18px; }
  .sc-info-note    { margin-left: 18px; margin-right: 18px; }
}
</style>