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
      <div class="don-card total-usuarios">
        <div class="don-icon total-icon">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
        </div>
        <strong class="don-value">{{ totalUsuarios }}</strong>
        <span class="don-label">Total usuarios</span>
        <span class="don-desc">En el sistema</span>
      </div>
      <div class="don-card total-activos">
        <div class="don-icon activos-icon">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/></svg>
        </div>
        <strong class="don-value">{{ totalActivos }}</strong>
        <span class="don-label">Activos</span>
        <span class="don-desc">Con acceso al sistema</span>
      </div>
      <div class="don-card total-inactivos">
        <div class="don-icon inactivos-icon">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="9"/><line x1="15" y1="9" x2="9" y2="15"/><line x1="9" y1="9" x2="15" y2="15"/></svg>
        </div>
        <strong class="don-value">{{ totalInactivos }}</strong>
        <span class="don-label">Inactivos</span>
        <span class="don-desc">Sin acceso al sistema</span>
      </div>
      <div class="don-card total-voluntarios">
        <div class="don-icon voluntarios-icon">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/></svg>
        </div>
        <strong class="don-value">{{ totalVoluntarios }}</strong>
        <span class="don-label">Voluntarios</span>
        <span class="don-desc">Rol asignado</span>
      </div>
      <div class="don-card total-pendientes">
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
        <!-- Rol -->
        <div class="filtro-group filtro-group--tabs">
          <label class="filtro-label">Rol</label>
          <div class="filtro-input-wrap">
            <select v-model="filtroRol" class="filtro-input filtro-select">
              <option value="Todos">Todos</option>
              <option value="Admin">Admin</option>
              <option value="Voluntario">Voluntario</option>
              <option value="Usuario">Usuario</option>
            </select>
          </div>
        </div>

        <!-- Estado -->
        <div class="filtro-group filtro-group--tabs">
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
          </div>
        </div>
      </div>

      <div class="filtros-divider"></div>

      <div class="filtros-row filtros-row--end">
        <!-- Buscar usuario -->
        <div class="filtro-group filtro-group--search">
          <label class="filtro-label">Buscar usuario</label>
          <div class="filtro-input-wrap">
            <input
              v-model="filtroTexto"
              placeholder="Nombre, correo, cédula o ID..."
              class="filtro-input"
            />
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
                <div class="action-group">
                  <button type="button" class="btn-accion-pill btn-ver" @click="verDetalle(u)">Ver detalle</button>
                  <button
                    type="button"
                    class="btn-accion-pill"
                    :class="u.activo ? 'btn-desactivar-pill' : 'btn-activar-pill'"
                    :disabled="u.id === ADMIN_ID"
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
          <div class="modal-box modal-box--lg">

            <button type="button" class="modal-close" @click="cerrarModal">✕</button>

            <div class="modal-header">
              <div class="modal-header-info">
                <p class="modal-eyebrow">Usuario {{ selectedUser.codigoVoluntario || selectedUser.id }}</p>
                <h2 class="modal-title">{{ selectedUser.nombre }}</h2>
                <p class="modal-sub">{{ selectedUser.correo }}</p>
              </div>
              <div class="modal-header-badges">
                <span class="estado-badge" :class="estadoBadgeClass(selectedUser)">{{ estadoLabel(selectedUser) }}</span>
                <span class="estado-badge" :class="rolBadgeClass(selectedUser.rol)">{{ selectedUser.rol }}</span>
              </div>
            </div>

            <div class="modal-body">
              <div class="modal-section">
                <h4 class="modal-section-title">Información personal</h4>
                <div class="modal-grid modal-grid--3">
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
                  <div class="modal-field" style="grid-column: span 3">
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
              <p class="modal-mensaje modal-mensaje--private">
                El rol se actualiza automáticamente desde <strong>Solicitudes de Voluntariado</strong> al aprobar o rechazar una postulación.
              </p>
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

            <button type="button" class="modal-close" @click="cancelarConfirmacion">✕</button>

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
/* ══════════════════════════════════════════
   VARIABLES — mismos tokens visuales que
   Solicitudes de Adopción (definidos con
   :global(:root) para que los Teleport /
   modales también los hereden correctamente)
══════════════════════════════════════════ */
:global(:root) {
  --usr-verde:       #3A473C;
  --usr-verde-sec:   #92A894;
  --usr-fondo:       #F7F8F7;
  --usr-blanco:      #FFFFFF;
  --usr-texto:       #2B322C;
  --usr-texto-sec:   #7A827B;
  --usr-texto-ter:   #A2A9A3;
  --usr-borde:       #E9ECE9;
  --usr-borde-suave: #EFF2EF;
  --usr-amarillo:    #F5B942;
  --usr-verde-ok:    #4CAF6A;
  --usr-sombra-sm:   0 1px 2px rgba(58,71,60,.03);
  --usr-sombra-md:   0 2px 4px rgba(58,71,60,.05), 0 14px 32px -14px rgba(58,71,60,.18);
}

.view-container {
  --verde:       var(--usr-verde);
  --verde-sec:   var(--usr-verde-sec);
  --fondo:       var(--usr-fondo);
  --blanco:      var(--usr-blanco);
  --texto:       var(--usr-texto);
  --texto-sec:   var(--usr-texto-sec);
  --texto-ter:   var(--usr-texto-ter);
  --borde:       var(--usr-borde);
  --borde-suave: var(--usr-borde-suave);
  --amarillo:    var(--usr-amarillo);
  --verde-ok:    var(--usr-verde-ok);
  --sombra-sm:   var(--usr-sombra-sm);
  --sombra-md:   var(--usr-sombra-md);
  background:
    radial-gradient(ellipse 800px 420px at 12% 0%, rgba(146,168,148,.07), transparent),
    var(--fondo);
  padding-bottom: 40px;
}

/* Las clases de modal usan directamente los tokens :root
   para garantizar visibilidad aunque estén en Teleport */
.modal-overlay,
.modal-box,
.modal-body,
.modal-section-title,
.modal-field-label,
.modal-field-value,
.modal-close,
.modal-title,
.modal-eyebrow,
.modal-sub,
.modal-mensaje,
.confirm-title,
.confirm-text,
.btn-aprobar,
.btn-rechazar {
  --verde:       var(--usr-verde);
  --verde-sec:   var(--usr-verde-sec);
  --fondo:       var(--usr-fondo);
  --blanco:      var(--usr-blanco);
  --texto:       var(--usr-texto);
  --texto-sec:   var(--usr-texto-sec);
  --texto-ter:   var(--usr-texto-ter);
  --borde:       var(--usr-borde);
  --borde-suave: var(--usr-borde-suave);
  --amarillo:    var(--usr-amarillo);
  --verde-ok:    var(--usr-verde-ok);
  --sombra-sm:   var(--usr-sombra-sm);
  --sombra-md:   var(--usr-sombra-md);
}

/* ── Encabezado ────────────────────────── */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
  gap: 16px;
  flex-wrap: wrap;
}
.brand-row {
  display: flex;
  align-items: center;
  gap: 12px;
}
.brand-mark {
  width: 38px;
  height: 38px;
  min-width: 38px;
  border-radius: 11px;
  background: #3A473C;
  background: linear-gradient(150deg, var(--verde, #3A473C) 0%, #6E8870 100%);
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 10px -3px rgba(58,71,60,.45);
}
.admin-page-title {
  font-size: 22px;
  font-weight: 700;
  color: var(--texto);
  letter-spacing: -0.4px;
  line-height: 1.15;
  margin: 0 0 2px;
}
.admin-page-sub {
  font-size: 12.5px;
  color: var(--texto-sec);
  font-weight: 500;
  margin: 0;
}

/* ── Tarjetas resumen ──────────────────── */
.don-summary {
  display: grid;
  grid-template-columns: repeat(5, 1fr);
  gap: 12px;
  margin-bottom: 20px;
}
.don-card {
  background: var(--blanco);
  border-radius: 14px;
  padding: 16px 15px;
  border: 1px solid var(--borde);
  box-shadow: var(--sombra-sm);
  display: flex;
  flex-direction: column;
  transition: box-shadow .18s ease, border-color .18s ease;
}
.don-card:hover { border-color: #D7DED8; box-shadow: var(--sombra-md); }

.don-icon {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 12px;
  border: 1px solid transparent;
}
.total-icon        { background: #F2F3F2; border-color: #DFE2DF; color: #616861; }
.activos-icon      { background: #EDF6EF; border-color: #C9E4CE; color: #2E7D45; }
.inactivos-icon    { background: #FBEDEC; border-color: #F1C7C3; color: #B71C1C; }
.voluntarios-icon  { background: #EAF2F6; border-color: #C7DCE6; color: #3C6E85; }
.pendientes-icon   { background: #FDF6E8; border-color: #F2E1B8; color: #A97A0C; }

.don-value { font-size: 21px; font-weight: 700; color: var(--texto); line-height: 1; letter-spacing: -0.4px; font-variant-numeric: tabular-nums; }
.don-label { font-size: 10.5px; color: var(--texto-ter); font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; margin-top: 7px; }
.don-desc  { font-size: 11px; color: var(--texto-sec); margin-top: 2px; }

/* ── Panel de filtros ─────────────────── */
.filtros-panel {
  background: var(--blanco);
  border-radius: 14px;
  padding: 18px 20px;
  margin-bottom: 20px;
  border: 1px solid var(--borde);
  box-shadow: var(--sombra-sm);
  display: flex;
  flex-direction: column;
  gap: 16px;
}
.filtros-row { display: flex; gap: 24px; flex-wrap: wrap; }
.filtros-row--end { align-items: flex-end; justify-content: space-between; }
.filtros-divider { height: 1px; background: var(--borde-suave); }

.filtro-group { display: flex; flex-direction: column; gap: 7px; }
.filtro-group--tabs { flex: 0 0 auto; min-width: 200px; }
.filtro-group--btn  { flex: 0 0 auto; }
.filtro-group--search { flex: 1; min-width: 220px; max-width: 340px; }

.filtro-label {
  font-size: 10.5px;
  font-weight: 700;
  color: var(--texto-ter);
  text-transform: uppercase;
  letter-spacing: 0.6px;
}

.filtro-input-wrap { position: relative; display: flex; align-items: center; }
.filtro-input {
  width: 100%;
  height: 36px;
  padding: 0 14px;
  border-radius: 9px;
  border: 1px solid var(--borde);
  background: var(--fondo);
  font-size: 13px;
  color: var(--texto);
  font-family: inherit;
  outline: none;
  transition: border-color 0.18s, background 0.18s;
  box-sizing: border-box;
}
.filtro-input:focus { border-color: var(--verde-sec); background: var(--blanco); }
.filtro-input::placeholder { color: var(--texto-ter); }
.filtro-select { cursor: pointer; }

.btn-limpiar {
  height: 36px;
  padding: 0 16px;
  border-radius: 9px;
  border: 1px solid var(--borde);
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

/* ── Estado vacío ─────────────────────── */
.empty-state {
  text-align: center;
  padding: 72px 24px;
  background: var(--blanco);
  border-radius: 14px;
  border: 1px solid var(--borde);
  color: var(--verde-sec);
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
}
.empty-state svg { opacity: 0.4; }
.empty-title { font-size: 16px; font-weight: 700; color: var(--texto); margin: 0; }
.empty-sub   { font-size: 13px; color: var(--texto-sec); margin: 0; }

/* ── Tabla ────────────────────────────── */
.table-wrapper {
  background: var(--blanco);
  border-radius: 14px;
  border: 1px solid var(--borde);
  overflow: hidden;
  box-shadow: var(--sombra-sm);
}
.table-scroll        { overflow-x: auto; -webkit-overflow-scrolling: touch; }
.don-table           { width: 100%; border-collapse: collapse; min-width: 720px; }
.don-table thead th  { padding: 12px 16px; text-align: left; color: var(--texto-ter); font-size: 9.5px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.6px; white-space: nowrap; }
.don-table tbody tr  { border-top: 1px solid var(--borde-suave); transition: background 0.15s; }
.don-table tbody tr:hover { background: #FAFBFA; }
.don-table tbody td  { padding: 12px 16px; vertical-align: middle; }

.id-pill       { font-size: 11px; font-family: ui-monospace, Menlo, Consolas, monospace; background: var(--fondo); border: 1px solid var(--borde); padding: 3px 9px; border-radius: 6px; color: var(--texto); font-weight: 700; white-space: nowrap; }
.fecha-text    { font-size: 12.5px; color: var(--texto-sec); white-space: nowrap; }
.donor-name    { font-size: 12.5px; font-weight: 700; color: var(--texto); }
.donor-mail-td { font-size: 12.5px; color: var(--texto-sec); }

/* Avatar en tabla */
.usr-cell { display: flex; align-items: center; gap: 10px; }
.usr-avatar {
  width: 34px;
  height: 34px;
  border-radius: 50%;
  background: #F1F5F1;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  border: 1px solid var(--borde);
}
.usr-avatar-ini { font-size: 13px; font-weight: 700; color: #4E6E51; text-transform: uppercase; line-height: 1; }

/* Acciones (idénticas a Solicitudes de Adopción) */
.action-group { display: flex; gap: 6px; flex-wrap: wrap; align-items: center; }
.btn-accion-pill {
  padding: 6px 13px;
  border-radius: 9px;
  border: 1px solid var(--borde);
  background: transparent;
  color: var(--texto-sec);
  font-size: 11.5px;
  font-weight: 700;
  cursor: pointer;
  white-space: nowrap;
  transition: all 0.15s;
  font-family: inherit;
}
.btn-accion-pill:disabled { opacity: 0.35; cursor: not-allowed; }
.btn-ver { color: var(--verde-sec); }
.btn-ver:hover:not(:disabled) { background: #F1F5F1; border-color: #DCE4DC; color: var(--verde); transform: translateY(-1px); }
.btn-activar-pill  { background: #EDF6EF; border-color: #C9E4CE; color: #2E7D32; }
.btn-activar-pill:hover:not(:disabled)  { background: #D9EEDC; }
.btn-desactivar-pill { background: #FBEDEC; border-color: #F1C7C3; color: #B71C1C; }
.btn-desactivar-pill:hover:not(:disabled) { background: #F7D6D2; }

.table-footer { padding: 12px 16px; border-top: 1px solid var(--borde-suave); font-size: 12px; color: var(--texto-sec); font-weight: 500; }

/* ── Badges ───────────────────────────── */
.estado-badge    { display: inline-block; font-size: 10.5px; font-weight: 700; padding: 4px 11px; border-radius: 20px; white-space: nowrap; }
.badge-pendiente { background: #FDF6E8; color: #96650A; }
.badge-aprobada  { background: #EDF6EF; color: #2E7D32; }
.badge-rechazada { background: #FBEDEC; color: #B71C1C; }
.badge-inactivo  { background: #FBEDEC; color: #B71C1C; }
.badge-admin     { background: #FDF6E8; color: #A97A0C; }
.badge-blue      { background: #EEF1FB; color: #4F73B8; }
.badge-neutral   { background: #F2F3F2; color: #7A827B; }

/* ── Toast ────────────────────────────── */
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

/* ── Modal ────────────────────────────── */
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
  background: var(--blanco, #FFFFFF);
  border-radius: 20px;
  padding: 36px;
  width: 100%; max-width: 640px;
  max-height: 90vh; overflow-y: auto;
  position: relative;
  box-shadow: 0 2px 4px rgba(58,71,60,.05), 0 24px 60px -14px rgba(58,71,60,.28);
}
.modal-box--sm { max-width: 440px; text-align: center; }
.modal-box--lg { max-width: 760px; }

.modal-close {
  position: absolute; top: 18px; right: 18px;
  width: 32px; height: 32px; border-radius: 50%;
  border: 1px solid var(--borde); background: var(--fondo);
  color: var(--texto); font-size: 13px; font-weight: 700;
  cursor: pointer; display: flex; align-items: center; justify-content: center;
  transition: all 0.2s; font-family: inherit;
}
.modal-close:hover { background: var(--verde); color: var(--blanco); border-color: var(--verde); }

.modal-header {
  display: flex;
  align-items: flex-start;
  gap: 14px;
  margin-bottom: 24px;
  padding-bottom: 20px;
  border-bottom: 1px solid var(--borde-suave);
}
.modal-header-info { flex: 1; min-width: 0; }
.modal-eyebrow {
  font-size: 10.5px; font-weight: 700; color: var(--verde-sec);
  text-transform: uppercase; letter-spacing: 0.7px; margin: 0 0 4px;
}
.modal-title { font-size: 19px; font-weight: 700; color: var(--texto); letter-spacing: -0.4px; margin: 0; }
.modal-sub   { font-size: 12px; color: var(--texto-sec); margin: 3px 0 0; }
.modal-header-badges {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 6px;
  flex-shrink: 0;
}

.modal-body { /* sin padding extra: el modal-box ya trae padding */ }

.modal-section       { margin-bottom: 24px; }
.modal-section-title {
  font-size: 10.5px; font-weight: 700; color: var(--texto-ter);
  text-transform: uppercase; letter-spacing: 0.5px;
  margin-bottom: 14px; padding-bottom: 10px;
  border-bottom: 1px solid var(--borde-suave);
}
.modal-grid    { display: grid; grid-template-columns: repeat(2,1fr); gap: 14px; }
.modal-grid--3 { display: grid; grid-template-columns: repeat(3,1fr); gap: 12px; }
.modal-field  { display: flex; flex-direction: column; gap: 4px; background: var(--fondo); border-radius: 10px; padding: 10px 12px; border: 1px solid var(--borde); transition: border-color 0.15s, box-shadow 0.15s; }
.modal-field:hover { border-color: #D5DED6; box-shadow: 0 2px 10px rgba(58,71,60,0.06); }
.modal-field-label { font-size: 10px; font-weight: 700; color: var(--texto-ter); text-transform: uppercase; letter-spacing: 0.4px; }
.modal-field-value { font-size: 13px; color: var(--texto); font-weight: 600; word-break: break-word; }

.modal-mensaje {
  font-size: 13.5px; color: var(--texto); line-height: 1.7;
  background: var(--fondo); border-radius: 10px; padding: 14px 16px;
  margin: 0;
}
.modal-mensaje--private {
  background: #FFFBF3;
  border: 1px solid rgba(249,193,122,.3);
}

.modal-acciones {
  display: flex;
  gap: 10px;
  justify-content: flex-end;
  padding-top: 20px;
  border-top: 1px solid var(--borde-suave);
  margin-top: 8px;
}

.btn-aprobar {
  flex: 1; padding: 13px; border-radius: 10px; border: none;
  background: #EDF6EF; color: #2E7D32;
  font-size: 13px; font-weight: 700; cursor: pointer;
  transition: all 0.2s; font-family: inherit;
}
.btn-aprobar:hover:not(:disabled) { background: #2E7D32; color: var(--blanco); }
.btn-aprobar:disabled { opacity: 0.4; cursor: not-allowed; }

.btn-rechazar {
  flex: 1; padding: 13px; border-radius: 10px; border: none;
  background: #FBEDEC; color: #B71C1C;
  font-size: 13px; font-weight: 700; cursor: pointer;
  transition: all 0.2s; font-family: inherit;
}
.btn-rechazar:hover:not(:disabled) { background: #B71C1C; color: var(--blanco); }
.btn-rechazar:disabled { opacity: 0.4; cursor: not-allowed; }

.modal-estado-final { padding-top: 20px; border-top: 1px solid var(--borde-suave); text-align: center; }
.estado-aprobada-msg  { color: #2E7D32; font-weight: 700; font-size: 14px; }
.estado-rechazada-msg { color: #B71C1C; font-weight: 700; font-size: 14px; }

/* Confirmación */
.confirm-icon-wrap {
  width: 60px; height: 60px;
  border-radius: 16px;
  background: #FDF6E8;
  display: flex; align-items: center; justify-content: center;
  margin: 0 auto 18px;
  color: #A97A0C;
}
.confirm-title {
  font-size: 18px; font-weight: 700; color: var(--texto);
  margin: 0 0 8px; text-align: center;
}
.confirm-text {
  font-size: 14px; color: var(--texto-sec);
  margin: 0 0 24px; text-align: center;
}

/* ── Animaciones ──────────────────────── */
.modal-fade-enter-active, .modal-fade-leave-active { transition: opacity 0.22s ease; }
.modal-fade-enter-from, .modal-fade-leave-to       { opacity: 0; }

/* ── Responsive (idéntico a Solicitudes de Adopción) ── */
@media (max-width: 1100px) {
  .don-summary { grid-template-columns: repeat(3, 1fr); }
}
@media (max-width: 900px) {
  .don-summary { grid-template-columns: repeat(2, 1fr); }
  .modal-grid--3 { grid-template-columns: repeat(2, 1fr); }
}
@media (max-width: 768px) {
  .don-summary { grid-template-columns: repeat(2, 1fr); gap: 10px; }

  .filtros-panel { padding: 14px; gap: 14px; }
  .filtros-row { flex-direction: column; gap: 12px; }
  .filtro-group, .filtro-group--tabs, .filtro-group--search, .filtro-group--btn {
    min-width: unset; max-width: none; width: 100%; flex: none;
  }

  .btn-limpiar { width: 100%; }

  .table-scroll { overflow-x: auto; -webkit-overflow-scrolling: touch; }

  .action-group { flex-wrap: wrap; }
  .btn-accion-pill { flex: 1; text-align: center; }

  .modal-box { padding: 22px 16px; max-width: calc(100vw - 32px); max-height: 95vh; }
  .modal-grid { grid-template-columns: 1fr; }
  .modal-grid--3 { grid-template-columns: 1fr 1fr; }
  .modal-header { flex-wrap: wrap; gap: 10px; }
  .modal-acciones { flex-direction: column; }

  .page-header { flex-direction: column; align-items: flex-start; gap: 10px; }
}
@media (max-width: 480px) {
  .don-summary { grid-template-columns: 1fr; }
  .modal-grid--3 { grid-template-columns: 1fr; }

  .don-table th:nth-child(3),
  .don-table td:nth-child(3),
  .don-table th:nth-child(6),
  .don-table td:nth-child(6) { display: none; }
}
</style>