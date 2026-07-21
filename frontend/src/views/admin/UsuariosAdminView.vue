<script setup>
import { ref, computed, onMounted } from 'vue'
import { getUsers, updateUserStatus, updateUserRoles } from '../../services/usersAdminServices'
import { getRoles, createRole, updateRole, deleteRole } from '../../services/rolesServices'

// ── Datos reales ──
const usuarios = ref([])
const usuariosLoading = ref(false)
const roles = ref([])
const rolesLoading = ref(false)

async function cargarUsuarios() {
  usuariosLoading.value = true
  try {
    const { data } = await getUsers()
    usuarios.value = data || []
  } catch {
    usuarios.value = []
    mostrarToast('No se pudieron cargar los usuarios', 'error')
  } finally {
    usuariosLoading.value = false
  }
}

async function cargarRoles() {
  rolesLoading.value = true
  try {
    const { data } = await getRoles()
    roles.value = data || []
  } catch {
    roles.value = []
    mostrarToast('No se pudieron cargar los roles', 'error')
  } finally {
    rolesLoading.value = false
  }
}

onMounted(() => {
  cargarUsuarios()
  cargarRoles()
})

// ── Pestañas del panel ──
const TABS = [
  { id: 'usuarios', titulo: 'Usuarios' },
  { id: 'roles', titulo: 'Roles' }
]
const activeTab = ref('usuarios')

// ── Toast ──
const toast = ref({ visible: false, tipo: 'exito', texto: '' })
function mostrarToast(texto, tipo = 'exito') {
  toast.value = { visible: true, tipo, texto }
  setTimeout(() => { toast.value.visible = false }, 3000)
}

// ── Helpers de presentación ──
function iniciales(nombre = '') {
  return nombre.trim().split(/\s+/).map(p => p[0]).filter(Boolean).slice(0, 2).join('').toUpperCase()
}

function estadoBadgeClass(user) {
  return user.active ? 'badge-aprobada' : 'badge-inactivo'
}
function estadoLabel(user) {
  return user.active ? 'Activo' : 'Inactivo'
}

// Los tres roles semilla tienen su propio color; cualquier rol nuevo cae en badge-blue
function rolBadgeClass(roleName) {
  if (roleName === 'Admin') return 'badge-admin'
  if (roleName === 'Voluntario') return 'badge-aprobada'
  if (roleName === 'Usuario') return 'badge-neutral'
  return 'badge-blue'
}

// ═══════════════════════════════════════════════
// PESTAÑA USUARIOS
// ═══════════════════════════════════════════════

const filtroTexto = ref('')
const filtroRol = ref('Todos')
const filtroEstado = ref('Todos')

const hayFiltrosUsuarios = computed(() =>
  filtroTexto.value.trim() !== '' || filtroRol.value !== 'Todos' || filtroEstado.value !== 'Todos'
)

function limpiarFiltrosUsuarios() {
  filtroTexto.value = ''
  filtroRol.value = 'Todos'
  filtroEstado.value = 'Todos'
}

const usuariosFiltrados = computed(() => {
  return usuarios.value.filter(u => {
    const t = filtroTexto.value.trim().toLowerCase()
    const coincideTexto =
      !t ||
      u.fullName.toLowerCase().includes(t) ||
      (u.email || '').toLowerCase().includes(t) ||
      (u.nationalId || '').toLowerCase().includes(t) ||
      u.userId.toLowerCase().includes(t)

    const coincideRol =
      filtroRol.value === 'Todos' || u.roles.some(r => r.roleName === filtroRol.value)

    const coincideEstado =
      filtroEstado.value === 'Todos' ||
      (filtroEstado.value === 'Activo' && u.active) ||
      (filtroEstado.value === 'Inactivo' && !u.active) ||
      (filtroEstado.value === 'Voluntario' && u.isVolunteer)

    return coincideTexto && coincideRol && coincideEstado
  })
})

const totalUsuarios     = computed(() => usuarios.value.length)
const totalActivos      = computed(() => usuarios.value.filter(u => u.active).length)
const totalInactivos    = computed(() => usuarios.value.filter(u => !u.active).length)
const totalVoluntarios  = computed(() => usuarios.value.filter(u => u.isVolunteer).length)
const totalSinRol       = computed(() => usuarios.value.filter(u => u.roles.length === 0).length)

// ── Modal de detalle + editor de roles ──
const showModal = ref(false)
const selectedUser = ref(null)
const editandoRoles = ref(false)
const rolesSeleccionados = ref([])
const guardandoRoles = ref(false)

function verDetalle(user) {
  selectedUser.value = user
  editandoRoles.value = false
  showModal.value = true
}

function cerrarModal() {
  showModal.value = false
  selectedUser.value = null
  editandoRoles.value = false
}

function abrirEditorRoles() {
  rolesSeleccionados.value = selectedUser.value.roles.map(r => r.roleId)
  editandoRoles.value = true
}

function toggleRolSeleccionado(roleId) {
  const i = rolesSeleccionados.value.indexOf(roleId)
  if (i === -1) rolesSeleccionados.value.push(roleId)
  else rolesSeleccionados.value.splice(i, 1)
}

function reemplazarUsuarioEnLista(actualizado) {
  const idx = usuarios.value.findIndex(u => u.userId === actualizado.userId)
  if (idx !== -1) usuarios.value[idx] = actualizado
  if (selectedUser.value?.userId === actualizado.userId) selectedUser.value = actualizado
}

async function guardarRolesUsuario() {
  guardandoRoles.value = true
  try {
    const { data } = await updateUserRoles(selectedUser.value.userId, rolesSeleccionados.value)
    reemplazarUsuarioEnLista(data)
    editandoRoles.value = false
    // El conteo de usuarios por rol (pestaña Roles) depende de esta asignación
    await cargarRoles()
    mostrarToast('Roles actualizados correctamente')
  } catch {
    mostrarToast('Error al actualizar los roles', 'error')
  } finally {
    guardandoRoles.value = false
  }
}

// ── Confirmación activar/desactivar ──
const modalConfirm = ref(false)
const usuarioSeleccionado = ref(null)
const mensajeConfirm = ref('')
const cambiandoEstado = ref(false)

function pedirConfirmacionEstado(user) {
  usuarioSeleccionado.value = user
  mensajeConfirm.value = user.active
    ? `Estás a punto de desactivar la cuenta de <strong>${user.fullName}</strong>.<br><br>El usuario perderá acceso al sistema hasta que sea activado nuevamente.`
    : `Estás a punto de activar la cuenta de <strong>${user.fullName}</strong>.<br><br>El usuario recuperará el acceso al sistema inmediatamente.`
  modalConfirm.value = true
}

async function confirmarCambioEstado() {
  if (!usuarioSeleccionado.value) return
  cambiandoEstado.value = true
  try {
    const nuevoEstado = !usuarioSeleccionado.value.active
    const { data } = await updateUserStatus(usuarioSeleccionado.value.userId, nuevoEstado)
    reemplazarUsuarioEnLista(data)
    mostrarToast(nuevoEstado ? 'Usuario activado.' : 'Usuario desactivado.')
  } catch {
    mostrarToast('Error al cambiar el estado', 'error')
  } finally {
    cambiandoEstado.value = false
    modalConfirm.value = false
    usuarioSeleccionado.value = null
  }
}

function cancelarConfirmacion() {
  modalConfirm.value = false
  usuarioSeleccionado.value = null
}

// ═══════════════════════════════════════════════
// PESTAÑA ROLES
// ═══════════════════════════════════════════════

const showModalRol = ref(false)
const rolEditando = ref(null)
const formRol = ref({ roleName: '', roleAccess: '', description: '' })
const errorRol = ref('')
const guardandoRol = ref(false)

function abrirNuevoRol() {
  rolEditando.value = null
  formRol.value = { roleName: '', roleAccess: '', description: '' }
  errorRol.value = ''
  showModalRol.value = true
}

function abrirEditarRol(rol) {
  rolEditando.value = rol
  formRol.value = { roleName: rol.roleName, roleAccess: rol.roleAccess, description: rol.description || '' }
  errorRol.value = ''
  showModalRol.value = true
}

function cerrarModalRol() {
  showModalRol.value = false
  rolEditando.value = null
}

async function guardarRol() {
  errorRol.value = ''
  if (!formRol.value.roleName.trim()) { errorRol.value = 'El nombre es obligatorio'; return }
  if (!formRol.value.roleAccess.trim()) { errorRol.value = 'El acceso es obligatorio'; return }

  guardandoRol.value = true
  try {
    if (rolEditando.value) {
      await updateRole(rolEditando.value.roleId, formRol.value)
      mostrarToast('Rol actualizado correctamente')
    } else {
      await createRole(formRol.value)
      mostrarToast('Rol creado correctamente')
    }
    await cargarRoles()
    cerrarModalRol()
  } catch (e) {
    errorRol.value = e?.response?.data?.message || 'Error al guardar el rol'
  } finally {
    guardandoRol.value = false
  }
}

// ── Confirmación de borrado ──
const modalConfirmRol = ref(false)
const rolAEliminar = ref(null)
const eliminandoRol = ref(false)

function pedirConfirmacionBorrarRol(rol) {
  rolAEliminar.value = rol
  modalConfirmRol.value = true
}

function cancelarBorrarRol() {
  modalConfirmRol.value = false
  rolAEliminar.value = null
}

async function confirmarBorrarRol() {
  if (!rolAEliminar.value) return
  eliminandoRol.value = true
  try {
    await deleteRole(rolAEliminar.value.roleId)
    mostrarToast('Rol eliminado correctamente')
    await cargarRoles()
  } catch (e) {
    mostrarToast(e?.response?.data?.message || 'No se pudo eliminar el rol', 'error')
  } finally {
    eliminandoRol.value = false
    modalConfirmRol.value = false
    rolAEliminar.value = null
  }
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
      <div>
        <h1 class="admin-page-title">Usuarios</h1>
        <p class="admin-page-sub">Cuentas del sistema y gestión de roles</p>
      </div>
      <button v-if="activeTab === 'roles'" class="btn-primary" @click="abrirNuevoRol">
        <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
        Nuevo rol
      </button>
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
      <div class="don-card total-sinrol">
        <span class="don-label">Sin rol asignado</span>
        <strong class="don-value">{{ totalSinRol }}</strong>
      </div>
    </div>

    <!-- PANEL: pestañas + filtros + contenido -->
    <div class="table-wrapper">

      <nav class="panel-tabs">
        <button
          v-for="t in TABS"
          :key="t.id"
          class="panel-tab"
          :class="{ 'panel-tab--active': activeTab === t.id }"
          @click="activeTab = t.id"
        >
          {{ t.titulo }}
          <span class="panel-tab-count">{{ t.id === 'usuarios' ? totalUsuarios : roles.length }}</span>
        </button>
      </nav>

      <!-- ══════════════ PESTAÑA USUARIOS ══════════════ -->
      <template v-if="activeTab === 'usuarios'">

        <div class="panel-filtros">
          <div class="filtro-input-wrap panel-buscar">
            <input v-model="filtroTexto" placeholder="Nombre, correo, cédula o ID..." class="filtro-input filtro-input--icon" />
            <span class="filtro-icon filtro-icon--right">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
            </span>
          </div>

          <div class="filtro-input-wrap" style="width:auto;min-width:160px">
            <select v-model="filtroRol" class="filtro-input filtro-select">
              <option value="Todos">Todos los roles</option>
              <option v-for="r in roles" :key="r.roleId" :value="r.roleName">{{ r.roleName }}</option>
            </select>
            <span class="filtro-icon filtro-icon--right filtro-icon--no-events">
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
            </span>
          </div>

          <div class="filtro-input-wrap" style="width:auto;min-width:160px">
            <select v-model="filtroEstado" class="filtro-input filtro-select">
              <option value="Todos">Todos los estados</option>
              <option value="Activo">Activos</option>
              <option value="Inactivo">Inactivos</option>
              <option value="Voluntario">Voluntarios</option>
            </select>
            <span class="filtro-icon filtro-icon--right filtro-icon--no-events">
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
            </span>
          </div>

          <button
            v-if="hayFiltrosUsuarios"
            type="button"
            class="btn-limpiar btn-limpiar--activo"
            @click="limpiarFiltrosUsuarios"
          >
            Limpiar filtros
          </button>
        </div>

        <div v-if="usuariosLoading" class="empty-state">
          <p class="empty-title">Cargando usuarios...</p>
        </div>

        <div v-else-if="usuariosFiltrados.length === 0" class="empty-state">
          <p class="empty-title">No hay usuarios registrados</p>
          <p class="empty-sub">{{ hayFiltrosUsuarios ? 'Ajusta los filtros para ver más resultados.' : 'Aún no hay cuentas creadas en el sistema.' }}</p>
        </div>

        <template v-else>
          <div class="table-scroll">
            <table class="don-table">
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Usuario</th>
                  <th>Correo</th>
                  <th>Roles</th>
                  <th>Estado</th>
                  <th>Acción</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="u in usuariosFiltrados" :key="u.userId" class="don-row">

                  <td><span class="id-pill">{{ u.userId }}</span></td>

                  <td>
                    <div class="usr-cell">
                      <div class="usr-avatar"><span class="usr-avatar-ini">{{ iniciales(u.fullName) }}</span></div>
                      <span class="donor-name">{{ u.fullName }}</span>
                    </div>
                  </td>

                  <td><span class="donor-mail-td">{{ u.email || '—' }}</span></td>

                  <td>
                    <div class="roles-cell">
                      <span v-for="r in u.roles" :key="r.roleId" class="estado-badge" :class="rolBadgeClass(r.roleName)">{{ r.roleName }}</span>
                      <span v-if="u.roles.length === 0" class="fecha-text">Sin rol</span>
                    </div>
                  </td>

                  <td><span class="estado-badge" :class="estadoBadgeClass(u)">{{ estadoLabel(u) }}</span></td>

                  <td>
                    <div class="acciones-cell">
                      <button class="btn-ver" @click="verDetalle(u)" title="Ver detalle">Ver detalle</button>
                      <button
                        class="btn-toggle"
                        :class="u.active ? 'btn-toggle--desactivar' : 'btn-toggle--activar'"
                        :title="u.active ? 'Desactivar' : 'Activar'"
                        @click="pedirConfirmacionEstado(u)"
                      >
                        {{ u.active ? 'Desactivar' : 'Activar' }}
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
        </template>
      </template>

      <!-- ══════════════ PESTAÑA ROLES ══════════════ -->
      <template v-else>

        <div v-if="rolesLoading" class="empty-state">
          <p class="empty-title">Cargando roles...</p>
        </div>

        <div v-else-if="roles.length === 0" class="empty-state">
          <p class="empty-title">No hay roles definidos</p>
          <p class="empty-sub">Crea el primero con el botón "Nuevo rol".</p>
        </div>

        <template v-else>
          <div class="table-scroll">
            <table class="don-table">
              <thead>
                <tr>
                  <th>Rol</th>
                  <th>Acceso</th>
                  <th>Descripción</th>
                  <th>Usuarios</th>
                  <th>Acción</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="r in roles" :key="r.roleId" class="don-row">
                  <td><span class="estado-badge" :class="rolBadgeClass(r.roleName)">{{ r.roleName }}</span></td>
                  <td><span class="metodo-text">{{ r.roleAccess }}</span></td>
                  <td><span class="donor-mail-td">{{ r.description || '—' }}</span></td>
                  <td><span class="id-pill">{{ r.userCount }}</span></td>
                  <td>
                    <div class="acciones-cell">
                      <button class="btn-ver" @click="abrirEditarRol(r)">Editar</button>
                      <button class="btn-toggle btn-toggle--desactivar" @click="pedirConfirmacionBorrarRol(r)">Eliminar</button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
          <div class="table-footer">
            {{ roles.length }} rol{{ roles.length !== 1 ? 'es' : '' }} definido{{ roles.length !== 1 ? 's' : '' }}
          </div>
        </template>
      </template>

    </div>

    <!-- ═══════════ MODAL DE DETALLE DE USUARIO ═══════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showModal && selectedUser" class="modal-overlay" @click.self="cerrarModal">
          <div class="modal-box">

            <button class="modal-close" @click="cerrarModal">✕</button>

            <div class="modal-header">
              <span class="modal-id">{{ selectedUser.userId }}</span>
              <span class="estado-badge" :class="estadoBadgeClass(selectedUser)">{{ estadoLabel(selectedUser) }}</span>
              <span v-if="selectedUser.isVolunteer" class="estado-badge badge-aprobada">Voluntario</span>
            </div>

            <!-- Avatar + nombre -->
            <div class="modal-usuario-hero">
              <div class="modal-avatar">
                <span class="modal-avatar-ini">{{ iniciales(selectedUser.fullName) }}</span>
              </div>
              <div>
                <p class="modal-usuario-nombre">{{ selectedUser.fullName }}</p>
                <p class="modal-usuario-correo">{{ selectedUser.email || 'Sin correo registrado' }}</p>
              </div>
            </div>

            <div class="modal-section">
              <h4 class="modal-section-title">Información personal</h4>
              <div class="modal-grid">
                <div class="modal-field">
                  <span class="modal-field-label">Cédula</span>
                  <strong class="modal-field-value">{{ selectedUser.nationalId || '—' }}</strong>
                </div>
                <div class="modal-field">
                  <span class="modal-field-label">Teléfono</span>
                  <strong class="modal-field-value">{{ selectedUser.phonePrimary || '—' }}</strong>
                </div>
                <div class="modal-field">
                  <span class="modal-field-label">Nacionalidad</span>
                  <strong class="modal-field-value">{{ selectedUser.nationality || '—' }}</strong>
                </div>
                <div class="modal-field">
                  <span class="modal-field-label">Ciudad / Cantón</span>
                  <strong class="modal-field-value">{{ [selectedUser.city, selectedUser.town].filter(Boolean).join(' / ') || '—' }}</strong>
                </div>
                <div class="modal-field modal-field--full">
                  <span class="modal-field-label">Dirección</span>
                  <strong class="modal-field-value">{{ selectedUser.addressLine || '—' }}</strong>
                </div>
              </div>
            </div>

            <div v-if="selectedUser.isVolunteer" class="modal-section">
              <h4 class="modal-section-title">Voluntariado</h4>
              <div class="modal-grid">
                <div class="modal-field">
                  <span class="modal-field-label">Tipo</span>
                  <strong class="modal-field-value">{{ selectedUser.volunteerType || '—' }}</strong>
                </div>
                <div class="modal-field">
                  <span class="modal-field-label">Validación</span>
                  <strong class="modal-field-value">{{ selectedUser.volunteerValidationStatus || '—' }}</strong>
                </div>
              </div>
            </div>

            <!-- Roles -->
            <div class="modal-section">
              <div class="modal-section-head">
                <h4 class="modal-section-title modal-section-title--flat">Roles asignados</h4>
                <button v-if="!editandoRoles" type="button" class="rol-editar-btn" @click="abrirEditorRoles">Editar roles</button>
              </div>

              <template v-if="!editandoRoles">
                <div class="roles-cell" style="margin-top:10px">
                  <span v-for="r in selectedUser.roles" :key="r.roleId" class="estado-badge" :class="rolBadgeClass(r.roleName)">{{ r.roleName }}</span>
                  <span v-if="selectedUser.roles.length === 0" class="fecha-text">Este usuario no tiene roles asignados</span>
                </div>
              </template>

              <template v-else>
                <div class="rol-checklist">
                  <label v-for="r in roles" :key="r.roleId" class="rol-check-item">
                    <input type="checkbox" :checked="rolesSeleccionados.includes(r.roleId)" @change="toggleRolSeleccionado(r.roleId)" />
                    <span>{{ r.roleName }}</span>
                  </label>
                  <p v-if="roles.length === 0" class="fecha-text">No hay roles definidos. Créalos en la pestaña Roles.</p>
                </div>
                <div class="rol-editar-actions">
                  <button type="button" class="btn-limpiar" @click="editandoRoles = false">Cancelar</button>
                  <button type="button" class="btn-aprobar" style="flex:1" :disabled="guardandoRoles" @click="guardarRolesUsuario">
                    {{ guardandoRoles ? 'Guardando...' : 'Guardar roles' }}
                  </button>
                </div>
              </template>
            </div>

            <div class="modal-acciones">
              <button
                class="btn-aprobar"
                :disabled="selectedUser.active"
                @click="pedirConfirmacionEstado(selectedUser)"
              >
                Activar usuario
              </button>
              <button
                class="btn-rechazar"
                :disabled="!selectedUser.active"
                @click="pedirConfirmacionEstado(selectedUser)"
              >
                Desactivar usuario
              </button>
            </div>

          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- ═══════════ MODAL CONFIRMAR ACTIVAR/DESACTIVAR ═══════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="modalConfirm" class="modal-overlay" @click.self="cancelarConfirmacion">
          <div class="modal-box modal-box--sm">

            <button class="modal-close" @click="cancelarConfirmacion">✕</button>

            <div class="confirm-icon-wrap">
              <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>
            </div>

            <h3 class="confirm-title">
              {{ usuarioSeleccionado?.active ? 'Desactivar usuario' : 'Activar usuario' }}
            </h3>
            <p class="confirm-text">{{ usuarioSeleccionado?.fullName }}</p>

            <div class="modal-acciones">
              <button class="btn-rechazar" style="flex:none;padding:13px 24px" @click="cancelarConfirmacion">Cancelar</button>
              <button class="btn-aprobar" style="flex:1" :disabled="cambiandoEstado" @click="confirmarCambioEstado">
                {{ cambiandoEstado ? 'Guardando...' : 'Confirmar' }}
              </button>
            </div>

          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- ═══════════ MODAL CREAR/EDITAR ROL ═══════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showModalRol" class="modal-overlay" @click.self="cerrarModalRol">
          <div class="modal-box modal-box--sm">

            <button class="modal-close" @click="cerrarModalRol">✕</button>

            <h3 class="confirm-title rol-form-title">{{ rolEditando ? 'Editar rol' : 'Nuevo rol' }}</h3>

            <div class="rol-form">
              <div class="fg">
                <label class="fg-label">Nombre <span class="req">*</span></label>
                <input type="text" class="filtro-input" placeholder="Ej. Coordinador" v-model="formRol.roleName" />
              </div>
              <div class="fg">
                <label class="fg-label">Acceso <span class="req">*</span></label>
                <input type="text" class="filtro-input" placeholder="Ej. coordinador" v-model="formRol.roleAccess" />
              </div>
              <div class="fg">
                <label class="fg-label">Descripción</label>
                <textarea class="filtro-input rol-textarea" placeholder="Para qué se usa este rol..." v-model="formRol.description"></textarea>
              </div>
            </div>

            <p v-if="errorRol" class="field-error">{{ errorRol }}</p>

            <div class="modal-acciones">
              <button class="btn-rechazar" style="flex:none;padding:13px 24px" @click="cerrarModalRol">Cancelar</button>
              <button class="btn-aprobar" style="flex:1" :disabled="guardandoRol" @click="guardarRol">
                {{ guardandoRol ? 'Guardando...' : (rolEditando ? 'Guardar cambios' : 'Crear rol') }}
              </button>
            </div>

          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- ═══════════ MODAL CONFIRMAR BORRADO DE ROL ═══════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="modalConfirmRol" class="modal-overlay" @click.self="cancelarBorrarRol">
          <div class="modal-box modal-box--sm">

            <button class="modal-close" @click="cancelarBorrarRol">✕</button>

            <div class="confirm-icon-wrap">
              <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/></svg>
            </div>

            <h3 class="confirm-title">Eliminar rol</h3>
            <p class="confirm-text">
              ¿Eliminar <strong>{{ rolAEliminar?.roleName }}</strong>?
              <template v-if="rolAEliminar?.userCount > 0">
                <br><br>Tiene {{ rolAEliminar.userCount }} usuario{{ rolAEliminar.userCount !== 1 ? 's' : '' }} asignado{{ rolAEliminar.userCount !== 1 ? 's' : '' }}; no podrá eliminarse hasta quitarles el rol.
              </template>
            </p>

            <div class="modal-acciones">
              <button class="btn-rechazar" style="flex:none;padding:13px 24px" @click="cancelarBorrarRol">Cancelar</button>
              <button class="btn-aprobar" style="flex:1" :disabled="eliminandoRol" @click="confirmarBorrarRol">
                {{ eliminandoRol ? 'Eliminando...' : 'Eliminar' }}
              </button>
            </div>

          </div>
        </div>
      </Transition>
    </Teleport>

  </div>
</template>

<style scoped>
/* ── Variables — en :global(:root) para que los modales
   Teleported a <body> también las hereden ─────────────── */
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

/* ── Encabezado ─────────────────────────────────────────── */
.page-header       { display: flex; justify-content: space-between; align-items: flex-start; gap: 16px; flex-wrap: wrap; margin-bottom: 28px; }
.admin-page-title  { font-size: 28px; font-weight: 800; color: var(--verde); letter-spacing: -0.5px; line-height: 1.1; }
.admin-page-sub    { font-size: 14px; color: var(--texto-sec); margin-top: 4px; font-weight: 500; }

.btn-primary {
  display: flex; align-items: center; gap: 7px;
  height: 38px; padding: 0 18px;
  background: var(--verde); color: #ffffff;
  border: none; border-radius: 8px;
  font-size: 13px; font-weight: 700; cursor: pointer;
  transition: background 0.18s; white-space: nowrap; flex-shrink: 0;
  font-family: inherit;
}
.btn-primary:hover { background: #2d3730; }

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

.total-usuarios    { border-top-color: var(--verde); }
.total-activos     { border-top-color: var(--verde-ok); }
.total-inactivos   { border-top-color: #E57373; }
.total-voluntarios { border-top-color: var(--verde-sec); }
.total-sinrol      { border-top-color: var(--amarillo); }

.don-label { font-size: 11px; color: var(--texto-sec); font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; }
.don-value { font-size: 24px; font-weight: 800; color: var(--verde); line-height: 1; }

/* ── Panel: pestañas + filtros + tabla ─────────────────── */
.table-wrapper {
  background: var(--blanco);
  border-radius: 14px;
  border: 1px solid var(--borde);
  overflow: hidden;
}

.panel-tabs {
  display: flex;
  gap: 2px;
  padding: 0 8px;
  border-bottom: 1px solid var(--borde);
  overflow-x: auto;
  -webkit-overflow-scrolling: touch;
}
.panel-tab {
  display: flex;
  align-items: center;
  gap: 7px;
  padding: 15px 16px 13px;
  border: none;
  border-bottom: 2.5px solid transparent;
  background: transparent;
  color: var(--texto-sec);
  font-size: 13px;
  font-weight: 700;
  cursor: pointer;
  font-family: inherit;
  white-space: nowrap;
  transition: color 0.18s, border-color 0.18s;
  margin-bottom: -1px;
}
.panel-tab:hover { color: var(--verde); }
.panel-tab--active {
  color: var(--verde);
  border-bottom-color: var(--verde);
}
.panel-tab-count {
  min-width: 20px;
  padding: 2px 7px;
  border-radius: 20px;
  background: var(--fondo);
  color: var(--texto-sec);
  font-size: 11px;
  font-weight: 800;
  text-align: center;
}
.panel-tab--active .panel-tab-count {
  background: var(--verde);
  color: var(--blanco);
}

.panel-filtros {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 14px 16px;
  border-bottom: 1px solid var(--borde);
  flex-wrap: wrap;
}
.panel-buscar { flex: 1; min-width: 200px; max-width: 360px; }

/* ── Controles de filtro ───────────────────────────────── */
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
  padding: 56px 24px;
}

.empty-title { font-size: 15px; font-weight: 700; color: var(--texto); margin-bottom: 6px; }
.empty-sub   { font-size: 13px; color: var(--texto-sec); }

/* ── Tabla ──────────────────────────────────────────────── */
.table-scroll          { overflow-x: auto; -webkit-overflow-scrolling: touch; }

.don-table             { width: 100%; border-collapse: collapse; min-width: 720px; }
.don-table thead tr    { background: var(--verde); }
.don-table thead th    { padding: 13px 16px; text-align: left; color: var(--blanco); font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.6px; white-space: nowrap; }
.don-table tbody tr    { border-bottom: 1px solid var(--borde); transition: background 0.15s; }
.don-table tbody tr:last-child { border-bottom: none; }
.don-table tbody tr:hover      { background: #F4F6F4; }
.don-table tbody td    { padding: 13px 16px; vertical-align: middle; }

.id-pill      { font-size: 11px; font-family: monospace; background: var(--fondo); border: 1px solid var(--borde); padding: 3px 9px; border-radius: 6px; color: var(--verde); font-weight: 700; white-space: nowrap; }
.fecha-text   { font-size: 13px; color: var(--texto-sec); white-space: nowrap; }
.metodo-text  { font-size: 13px; color: var(--texto-sec); }
.donor-name   { font-size: 13px; font-weight: 700; color: var(--texto); }
.donor-mail-td { font-size: 13px; color: var(--texto-sec); }

/* Varios badges de rol en una celda */
.roles-cell { display: flex; flex-wrap: wrap; gap: 6px; align-items: center; }

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
  background: var(--blanco);
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
.modal-section-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  border-bottom: 1px solid var(--borde);
  padding-bottom: 10px;
  margin-bottom: 0;
}
.modal-section-title--flat { border: none; margin: 0; padding: 0; }

.modal-grid   { display: grid; grid-template-columns: repeat(2,1fr); gap: 16px; }
.modal-field  { display: flex; flex-direction: column; gap: 4px; }
.modal-field--full { grid-column: 1 / -1; }
.modal-field-label { font-size: 10px; font-weight: 700; color: #9CA8A0; text-transform: uppercase; letter-spacing: 0.4px; }
.modal-field-value { font-size: 14px; color: var(--texto); font-weight: 600; word-break: break-word; }

/* Edición de roles del usuario, dentro del modal de detalle */
.rol-editar-btn {
  border: none;
  background: transparent;
  color: var(--verde);
  font-size: 12px;
  font-weight: 700;
  cursor: pointer;
  font-family: inherit;
  text-decoration: underline;
  padding: 0;
}
.rol-editar-btn:hover { color: #2d3730; }

.rol-checklist {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 8px;
  margin-top: 12px;
}
.rol-check-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 9px 12px;
  border: 1.5px solid var(--borde);
  border-radius: 8px;
  font-size: 13px;
  font-weight: 600;
  color: var(--texto);
  cursor: pointer;
  transition: border-color 0.15s, background 0.15s;
}
.rol-check-item:hover { border-color: var(--verde-sec); background: var(--fondo); }
.rol-check-item input { accent-color: var(--verde); width: 15px; height: 15px; flex-shrink: 0; }

.rol-editar-actions {
  display: flex;
  gap: 10px;
  margin-top: 16px;
}

/* Formulario de rol (modal crear/editar) */
.rol-form-title { text-align: left; margin-bottom: 20px; }
.rol-form { display: flex; flex-direction: column; gap: 14px; text-align: left; margin-bottom: 16px; }
.fg { display: flex; flex-direction: column; gap: 6px; }
.fg-label { font-size: 11px; font-weight: 700; color: var(--verde); text-transform: uppercase; letter-spacing: 0.4px; }
.req { color: #c0392b; }
.rol-textarea { height: 80px; padding-top: 10px; resize: vertical; line-height: 1.5; }
.field-error { font-size: 11px; color: #c0392b; font-weight: 600; margin: 0 0 12px; text-align: left; }

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
  .total-sinrol { grid-column: span 2; }
}

@media (max-width: 768px) {
  .don-summary {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 10px;
  }
  .total-sinrol { grid-column: span 2; }

  .panel-filtros {
    flex-direction: column;
    align-items: stretch;
    gap: 10px;
    padding: 14px;
  }
  .panel-buscar { max-width: none; }
  .panel-tab { padding: 13px 12px 11px; font-size: 12px; }

  .btn-limpiar { width: 100%; justify-content: center; }

  .table-scroll { overflow-x: auto; -webkit-overflow-scrolling: touch; }

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
  .rol-checklist { grid-template-columns: 1fr; }

  .modal-acciones,
  .rol-editar-actions {
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

@media (max-width: 640px) {
  .page-header { flex-direction: column; align-items: flex-start; }
  .btn-primary { width: 100%; justify-content: center; }
}

@media (max-width: 480px) {
  .don-summary { grid-template-columns: 1fr; }
  .total-sinrol { grid-column: span 1; }

  .don-table th:nth-child(3),
  .don-table td:nth-child(3) { display: none; }
}
</style>
