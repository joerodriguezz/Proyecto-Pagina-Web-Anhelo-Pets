<script setup>
import { ref } from 'vue'
import Icon from '../../components/Icon.vue'

const usuarios = ref([])
const showModal = ref(false)
const selectedUser = ref(null)

function cargarUsuarios() {
  const guardados = JSON.parse(
    localStorage.getItem('anhelo_usuarios')
  ) || [
    {
      id: 'ADMIN-001',
      nombre: 'Shirley Valverde',
      cedula: '1-0932-0528',
      correo: 'shirley@anhelopets.cr',
      telefono: '+506 8840-3334',
      password: 'Admin123',
      rol: 'Admin',
      tipoVoluntario: '',
      direccion: '',
      pais: 'Costa Rica',
      solicitudVoluntario: null,
      activo: true
    }
  ]
  usuarios.value = guardados
}

cargarUsuarios()

function guardarUsuarios() {
  localStorage.setItem(
    'anhelo_usuarios',
    JSON.stringify(usuarios.value)
  )
}

function toggleEstado(user) {
  if (user.id === 'ADMIN-001') return
  user.activo = !user.activo
  guardarUsuarios()
}

function verDetalle(user) {
  selectedUser.value = user
  showModal.value = true
}

function rolClass(rol) {
  if (rol === 'Admin')      return 'badge-peach'
  if (rol === 'Voluntario') return 'badge-green'
  return 'badge-gray'
}

function solicitudClass(estado) {
  if (estado === 'Aprobada')  return 'badge-green'
  if (estado === 'Rechazada') return 'badge-red'
  if (estado === 'Pendiente') return 'badge-peach'
  return 'badge-gray'
}
</script>

<template>
  <div class="view-container">

    <!-- HEADER -->
    <header class="page-header">
      <div>
        <h1 class="admin-page-title">Gestión de Usuarios</h1>
        <p class="admin-page-sub">Control de cuentas y roles</p>
      </div>
    </header>

    <!-- TABLA -->
    <div class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Nombre</th>
            <th>Cédula</th>
            <th>Correo</th>
            <th>Teléfono</th>
            <th>Rol</th>
            <th>Tipo</th>
            <th>Solicitud</th>
            <th>Estado</th>
            <th>Acciones</th>
          </tr>
        </thead>

        <tbody>
          <tr v-for="u in usuarios" :key="u.id">

            <td><span class="id-code">{{ u.id }}</span></td>

            <td class="font-semibold">{{ u.nombre }}</td>

            <td class="text-secondary">{{ u.cedula || '—' }}</td>

            <td class="text-secondary">{{ u.correo }}</td>

            <td>{{ u.telefono || '—' }}</td>

            <td>
              <span class="badge" :class="rolClass(u.rol)">
                {{ u.rol }}
              </span>
            </td>

            <td>{{ u.tipoVoluntario || '—' }}</td>

            <td>
              <span
                v-if="u.solicitudVoluntario?.estado"
                class="badge"
                :class="solicitudClass(u.solicitudVoluntario.estado)"
              >
                {{ u.solicitudVoluntario.estado }}
              </span>
              <span v-else class="text-secondary">—</span>
            </td>

            <td>
              <span class="badge" :class="u.activo ? 'badge-green' : 'badge-red'">
                {{ u.activo ? 'Activo' : 'Inactivo' }}
              </span>
            </td>

            <td>
              <div class="action-btns">

                <!-- VER -->
                <button
                  class="action-btn"
                  title="Ver detalle"
                  @click="verDetalle(u)"
                >
                  <Icon name="Eye" />
                </button>

                <!-- ACTIVAR / DESACTIVAR -->
                <button
                  class="action-btn"
                  :class="{ 'danger': u.activo }"
                  :disabled="u.id === 'ADMIN-001'"
                  :title="u.activo ? 'Desactivar cuenta' : 'Activar cuenta'"
                  @click="toggleEstado(u)"
                >
                  <Icon :name="u.activo ? 'Lock' : 'Unlock'" />
                </button>

              </div>
            </td>

          </tr>
        </tbody>
      </table>
    </div>

    <!-- MODAL DETALLE -->
    <div
      v-if="showModal"
      class="modal-overlay"
      @click.self="showModal = false"
    >
      <div class="modal-card">

        <div class="modal-header">
          <h2>Información del usuario</h2>
          <button class="close-btn" @click="showModal = false">×</button>
        </div>

        <div v-if="selectedUser" class="detail-grid">

          <div class="detail-item">
            <span>Nombre</span>
            <strong>{{ selectedUser.nombre }}</strong>
          </div>

          <div class="detail-item">
            <span>Cédula</span>
            <strong>{{ selectedUser.cedula || '—' }}</strong>
          </div>

          <div class="detail-item">
            <span>Correo</span>
            <strong>{{ selectedUser.correo }}</strong>
          </div>

          <div class="detail-item">
            <span>Teléfono</span>
            <strong>{{ selectedUser.telefono || '—' }}</strong>
          </div>

          <div class="detail-item">
            <span>País</span>
            <strong>{{ selectedUser.pais || '—' }}</strong>
          </div>

          <div class="detail-item">
            <span>Dirección</span>
            <strong>{{ selectedUser.direccion || '—' }}</strong>
          </div>

          <div class="detail-item">
            <span>Rol</span>
            <span class="badge" :class="rolClass(selectedUser.rol)">
              {{ selectedUser.rol }}
            </span>
          </div>

          <div class="detail-item">
            <span>Estado de cuenta</span>
            <span class="badge" :class="selectedUser.activo ? 'badge-green' : 'badge-red'">
              {{ selectedUser.activo ? 'Activo' : 'Inactivo' }}
            </span>
          </div>

          <div class="detail-item">
            <span>Tipo voluntario</span>
            <strong>{{ selectedUser.tipoVoluntario || '—' }}</strong>
          </div>

          <div class="detail-item">
            <span>Solicitud de voluntariado</span>
            <span
              v-if="selectedUser.solicitudVoluntario?.estado"
              class="badge"
              :class="solicitudClass(selectedUser.solicitudVoluntario.estado)"
            >
              {{ selectedUser.solicitudVoluntario.estado }}
            </span>
            <strong v-else>—</strong>
          </div>

        </div>

        <!-- NOTA -->
        <div class="info-note">
          <i class="bx bxs-info-circle"></i>
          El rol se actualiza automáticamente desde
          <strong>Solicitudes de Voluntariado</strong>
          al aprobar o rechazar una postulación.
        </div>

        <button class="close-full-btn" @click="showModal = false">
          Cerrar
        </button>

      </div>
    </div>

  </div>
</template>

<style scoped>
.view-container {
  padding: 10px;
}

.page-header {
  margin-bottom: 28px;
}

.admin-page-title {
  font-size: 32px;
  font-weight: 800;
  color: #2F3B31;
}

.admin-page-sub {
  color: #667085;
  margin-top: 4px;
}

.table-wrapper {
  background: white;
  border-radius: 24px;
  padding: 24px;
  overflow-x: auto;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
}

.data-table th {
  text-align: left;
  padding-bottom: 16px;
  color: #667085;
  font-size: 13px;
}

.data-table td {
  padding: 18px 0;
  border-top: 1px solid #F4F6F4;
}

.badge {
  padding: 6px 12px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 700;
  display: inline-block;
}

.badge-peach {
  background: rgba(249,193,122,0.18);
  color: #D18C3A;
}

.badge-gray {
  background: #F4F6F4;
  color: #667085;
}

.badge-green {
  background: rgba(146,168,148,0.18);
  color: #5A6E5C;
}

.badge-red {
  background: rgba(235,119,119,0.16);
  color: #C45252;
}

.id-code {
  background: #F4F6F4;
  padding: 6px 10px;
  border-radius: 10px;
  font-size: 12px;
  font-weight: 700;
  font-family: monospace;
  color: #3A473C;
}

.font-semibold { font-weight: 700; }
.text-secondary { color: #667085; }

.action-btns {
  display: flex;
  gap: 10px;
}

.action-btn {
  width: 38px;
  height: 38px;
  border: none;
  border-radius: 12px;
  background: #F4F6F4;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background 0.15s;
}

.action-btn:disabled {
  opacity: 0.35;
  cursor: not-allowed;
}

.action-btn.danger {
  background: rgba(235,119,119,0.16);
  color: #C45252;
}

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 999;
}

.modal-card {
  width: 100%;
  max-width: 700px;
  background: white;
  border-radius: 28px;
  padding: 30px;
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 28px;
}

.modal-header h2 {
  font-size: 22px;
  font-weight: 800;
  color: #2F3B31;
}

.close-btn {
  border: none;
  background: transparent;
  font-size: 28px;
  cursor: pointer;
  color: #667085;
  line-height: 1;
}

.detail-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 18px;
}

.detail-item {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.detail-item span:first-child {
  font-size: 13px;
  color: #667085;
}

.detail-item strong {
  color: #2F3B31;
  font-weight: 600;
}

.info-note {
  margin-top: 24px;
  padding: 14px 18px;
  background: rgba(146,168,148,0.10);
  border-radius: 14px;
  font-size: 13px;
  color: #5A6E5C;
  display: flex;
  align-items: flex-start;
  gap: 10px;
  line-height: 1.6;
}

.info-note i {
  font-size: 18px;
  flex-shrink: 0;
  margin-top: 1px;
}

.close-full-btn {
  width: 100%;
  height: 54px;
  border: none;
  border-radius: 16px;
  margin-top: 20px;
  background: linear-gradient(135deg, #92A894, #7C927E);
  color: white;
  font-weight: 800;
  cursor: pointer;
  font-size: 15px;
}
</style>