<script setup>
import { onMounted, ref } from 'vue'
import Icon from '../../components/Icon.vue'
import { volunteersApi } from '../../services/api'

const volunteers = ref([])
const loading = ref(false)
const errorMessage = ref('')

async function loadVolunteers() {
  loading.value = true
  errorMessage.value = ''

  try {
    volunteers.value = await volunteersApi.getAll()
  } catch (error) {
    errorMessage.value = error.message || 'No se pudieron cargar los voluntarios.'
    volunteers.value = []
  } finally {
    loading.value = false
  }
}

async function setActive(volunteer, active) {
  try {
    await volunteersApi.update(volunteer.volunteerId, {
      active,
      modifiedBy: 'frontend',
    })
    await loadVolunteers()
  } catch (error) {
    errorMessage.value = error.message || 'No se pudo actualizar el estado.'
  }
}

async function validateVolunteer(volunteer, validationStatus) {
  const storedUser = localStorage.getItem('authUser')
  const authUser = storedUser ? JSON.parse(storedUser) : null

  if (!authUser?.userId) {
    errorMessage.value = 'Debes iniciar sesion para validar voluntarios.'
    return
  }

  try {
    await volunteersApi.update(volunteer.volunteerId, {
      validationStatus,
      validatedByUserId: authUser.userId,
      validationNotes: validationStatus === 'Aprobado' ? 'Aprobado desde admin' : 'Rechazado desde admin',
      modifiedBy: 'frontend',
    })
    await loadVolunteers()
  } catch (error) {
    errorMessage.value = error.message || 'No se pudo validar el voluntario.'
  }
}

function activeLabel(value) {
  return value ? 'Activo' : 'Inactivo'
}

onMounted(loadVolunteers)
</script>

<template>
  <div class="view-container">
    <header class="page-header">
      <div>
        <h1 class="admin-page-title">Voluntarios</h1>
        <p class="admin-page-sub">Gestion y seguimiento de voluntarios registrados</p>
      </div>
    </header>

    <p v-if="errorMessage" class="form-error">{{ errorMessage }}</p>

    <div class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Nombre</th>
            <th>Cedula</th>
            <th>Telefono</th>
            <th>Correo</th>
            <th>Tipo</th>
            <th>Validacion</th>
            <th>Estado</th>
            <th class="text-right">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading">
            <td colspan="9">Cargando voluntarios...</td>
          </tr>

          <tr v-for="v in volunteers" :key="v.volunteerId">
            <td><span class="id-code">V-{{ v.volunteerId }}</span></td>
            <td class="font-semibold">{{ v.fullName }}</td>
            <td class="text-secondary">{{ v.nationalId }}</td>
            <td>{{ v.phone }}</td>
            <td class="text-email">{{ v.email }}</td>
            <td>
              <span class="badge badge-type">
                {{ v.volunteerType }}
              </span>
            </td>
            <td>
              <span class="badge badge-gray">
                {{ v.validationStatus || 'Pendiente' }}
              </span>
            </td>
            <td>
              <span class="badge" :class="v.active ? 'badge-green' : 'badge-gray'">
                {{ activeLabel(v.active) }}
              </span>
            </td>
            <td>
              <div class="action-btns">
                <button class="action-btn" title="Aprobar" @click="validateVolunteer(v, 'Aprobado')">
                  <Icon name="Check" />
                </button>
                <button class="action-btn" title="Rechazar" @click="validateVolunteer(v, 'Rechazado')">
                  <Icon name="XCircle" />
                </button>
                <button class="action-btn disable-vol" :title="v.active ? 'Desactivar' : 'Activar'" @click="setActive(v, !v.active)">
                  <Icon name="Lock" />
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<style scoped>
.view-container {
  background-color: transparent;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 32px;
}

.admin-page-title {
  font-size: 28px;
  font-weight: 800;
  color: #3A473C;
  letter-spacing: 0;
}

.admin-page-sub {
  font-size: 14px;
  color: #6C756D;
  margin-top: 4px;
  font-weight: 500;
}

.table-wrapper {
  background: white;
  border-radius: 24px;
  padding: 24px;
  box-shadow: 0 4px 20px rgba(58, 71, 60, 0.02);
  overflow-x: auto;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
}

.data-table th {
  font-size: 13px;
  font-weight: 700;
  color: #6C756D;
  padding-bottom: 16px;
  border-bottom: 1px solid #F4F6F4;
}

.data-table td {
  padding: 16px 0;
  font-size: 14px;
  color: #3A473C;
  border-bottom: 1px solid #FAFAFA;
  vertical-align: middle;
}

.id-code {
  font-size: 12px;
  font-family: monospace;
  background: #F4F6F4;
  padding: 4px 8px;
  border-radius: 8px;
  color: #3A473C;
  font-weight: 600;
}

.font-semibold { font-weight: 600; }
.text-secondary { font-size: 13px; color: #6C756D; }
.text-email { font-size: 13px; color: #3A473C; }
.text-right { text-align: right; }

.badge {
  padding: 6px 12px;
  border-radius: 10px;
  font-size: 12px;
  font-weight: 700;
  display: inline-block;
}

.badge-type {
  background: rgba(249, 193, 122, 0.2);
  color: #D18C3A;
  font-size: 11px;
}

.badge-green { background: rgba(146, 168, 148, 0.2); color: #5A6E5C; }
.badge-gray { background: #F4F6F4; color: #6C756D; }

.action-btns {
  display: flex;
  gap: 8px;
  justify-content: flex-end;
}

.action-btn {
  width: 34px;
  height: 34px;
  border: 2px solid #F4F6F4;
  border-radius: 10px;
  background: white;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
  color: #6C756D;
}

.action-btn:hover {
  background: #F4F6F4;
  border-color: #6C756D;
  color: #3A473C;
  transform: translateY(-1px);
}

.action-btn.disable-vol:hover {
  background: rgba(235, 119, 119, 0.1);
  border-color: #EB7777;
  color: #C45252;
}

.form-error {
  color: #B42318;
  font-size: 14px;
  font-weight: 700;
  margin-bottom: 16px;
}

@media (max-width: 768px) {
  .page-header { flex-direction: column; align-items: flex-start; gap: 8px; }
}
</style>
