<script setup>
import { computed, onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import {
  animalsApi,
  fosterHomesApi,
  fosterPlacementsApi,
  rescuesApi,
  volunteersApi,
} from '../../services/api'

const animals = ref([])
const rescues = ref([])
const volunteers = ref([])
const fosterHomes = ref([])
const placements = ref([])
const loading = ref(false)
const errorMessage = ref('')

const activeRescues = computed(() => rescues.value.filter((rescue) => rescue.status === 'Activo'))
const activeVolunteers = computed(() => volunteers.value.filter((volunteer) => volunteer.active))
const activeFosterHomes = computed(() => fosterHomes.value.filter((home) => home.active))

const petStatus = computed(() => {
  const total = animals.value.length || 1
  return ['Disponible', 'En proceso', 'Adoptada'].map((status) => {
    const count = animals.value.filter((animal) => animal.animalStatus === status).length
    return {
      label: status,
      count,
      pct: Math.round((count / total) * 100),
      color: status === 'Disponible' ? '#92A894' : status === 'En proceso' ? '#F9C17A' : '#7C927E',
    }
  })
})

const kpis = computed(() => [
  { label: 'Mascotas registradas', value: animals.value.length, color: '#92A894' },
  { label: 'Rescates activos', value: activeRescues.value.length, color: '#7C927E' },
  { label: 'Voluntarios activos', value: activeVolunteers.value.length, color: '#7C927E' },
  { label: 'Casas cuna activas', value: activeFosterHomes.value.length, color: '#F9C17A' },
  { label: 'Asignaciones actuales', value: placements.value.filter((p) => !p.endDate).length, color: '#92A894' },
  { label: 'Adopciones registradas', value: 0, color: '#6C756D' },
])

async function loadDashboard() {
  loading.value = true
  errorMessage.value = ''

  try {
    const [animalRows, rescueRows, volunteerRows, fosterRows, placementRows] = await Promise.all([
      animalsApi.getAll({ status: 'Todos' }),
      rescuesApi.getAll(),
      volunteersApi.getAll(),
      fosterHomesApi.getAll(),
      fosterPlacementsApi.getAll(),
    ])

    animals.value = animalRows
    rescues.value = rescueRows
    volunteers.value = volunteerRows
    fosterHomes.value = fosterRows
    placements.value = placementRows
  } catch (error) {
    errorMessage.value = error.message || 'No se pudo cargar el dashboard.'
  } finally {
    loading.value = false
  }
}

onMounted(loadDashboard)
</script>

<template>
  <div class="dashboard">
    <header class="dash-header">
      <div>
        <h1 class="dash-title">Panel de control</h1>
        <p class="dash-sub">Resumen general con datos reales de base de datos</p>
      </div>
      <button class="refresh-btn" :disabled="loading" @click="loadDashboard">Actualizar</button>
    </header>

    <p v-if="errorMessage" class="form-error">{{ errorMessage }}</p>

    <div class="kpi-grid">
      <div v-for="kpi in kpis" :key="kpi.label" class="kpi-card" :style="{ borderTop: `4px solid ${kpi.color}` }">
        <span class="kpi-label">{{ kpi.label }}</span>
        <div class="kpi-value">{{ kpi.value }}</div>
      </div>
    </div>

    <div class="dash-grid">
      <div class="dash-card">
        <div class="dash-card-head">
          <h3>Estado de mascotas</h3>
          <RouterLink to="/admin/mascotas" class="view-all">Gestionar</RouterLink>
        </div>
        <div class="status-bars">
          <div v-for="status in petStatus" :key="status.label" class="status-bar-item">
            <div class="sb-header">
              <span class="sb-label">{{ status.label }}</span>
              <span class="sb-count">{{ status.count }}</span>
            </div>
            <div class="sb-track">
              <div class="sb-fill" :style="{ width: status.pct + '%', background: status.color }"></div>
            </div>
          </div>
        </div>
      </div>

      <div class="dash-card">
        <div class="dash-card-head">
          <h3>Rescates activos</h3>
          <RouterLink to="/admin/rescates" class="view-all">Ver todos</RouterLink>
        </div>
        <div v-if="activeRescues.length" class="rescue-list">
          <div v-for="rescue in activeRescues.slice(0, 5)" :key="rescue.rescateId" class="rescue-item">
            <div class="rescue-info">
              <span class="rescue-name">{{ rescue.animalName || 'Sin mascota asociada' }}</span>
              <span class="rescue-loc">{{ rescue.ubicacion }}</span>
            </div>
            <span class="badge badge-active">{{ rescue.status }}</span>
          </div>
        </div>
        <p v-else class="empty-text">No hay rescates activos registrados.</p>
      </div>
    </div>

    <div class="quick-actions">
      <h3>Acciones rapidas</h3>
      <div class="qa-grid">
        <RouterLink to="/admin/mascotas" class="qa-btn">Nueva mascota</RouterLink>
        <RouterLink to="/admin/rescates" class="qa-btn">Registrar rescate</RouterLink>
        <RouterLink to="/admin/voluntarios" class="qa-btn">Ver voluntarios</RouterLink>
      </div>
    </div>
  </div>
</template>

<style scoped>
.dashboard {
  padding: 40px;
  background-color: #FAFAFA;
  min-height: 100vh;
  font-family: 'Inter', sans-serif;
}

.dash-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 32px;
}

.dash-title {
  font-size: 32px;
  font-weight: 800;
  color: #3A473C;
  letter-spacing: 0;
  margin-bottom: 6px;
}

.dash-sub {
  font-size: 14px;
  color: #6C756D;
  font-weight: 500;
}

.refresh-btn {
  min-height: 42px;
  padding: 0 18px;
  border-radius: 8px;
  border: none;
  background: #3A473C;
  color: white;
  font-weight: 800;
  cursor: pointer;
}

.refresh-btn:disabled {
  cursor: not-allowed;
  opacity: 0.7;
}

.kpi-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 20px;
  margin-bottom: 32px;
}

.kpi-card {
  background: white;
  border-radius: 8px;
  padding: 24px;
  box-shadow: 0 4px 20px rgba(58, 71, 60, 0.02);
  min-height: 110px;
}

.kpi-label {
  font-size: 13px;
  color: #6C756D;
  font-weight: 600;
}

.kpi-value {
  font-size: 30px;
  font-weight: 800;
  color: #3A473C;
  line-height: 1;
  margin-top: 8px;
}

.dash-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 24px;
  margin-bottom: 32px;
}

.dash-card,
.quick-actions {
  background: white;
  border-radius: 8px;
  padding: 28px;
  box-shadow: 0 4px 20px rgba(58, 71, 60, 0.02);
}

.dash-card-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 24px;
}

.dash-card-head h3,
.quick-actions h3 {
  font-size: 18px;
  font-weight: 800;
  color: #3A473C;
  letter-spacing: 0;
}

.view-all {
  font-size: 13px;
  color: #5A6E5C;
  font-weight: 700;
  text-decoration: none;
}

.status-bars,
.rescue-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.sb-header {
  display: flex;
  justify-content: space-between;
  font-size: 14px;
  margin-bottom: 8px;
}

.sb-label {
  font-weight: 600;
  color: #6C756D;
}

.sb-count {
  font-weight: 700;
  color: #3A473C;
}

.sb-track {
  height: 8px;
  background: #F4F6F4;
  border-radius: 99px;
  overflow: hidden;
}

.sb-fill {
  height: 100%;
  border-radius: 99px;
}

.rescue-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 14px 18px;
  background: #F4F6F4;
  border-radius: 8px;
}

.rescue-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.rescue-name {
  font-size: 14px;
  font-weight: 700;
  color: #3A473C;
}

.rescue-loc,
.empty-text {
  font-size: 13px;
  color: #6C756D;
}

.badge {
  padding: 6px 12px;
  border-radius: 8px;
  font-size: 12px;
  font-weight: 700;
  display: inline-block;
}

.badge-active {
  background: #E7F1E8;
  color: #4F6F55;
}

.qa-grid {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
  margin-top: 20px;
}

.qa-btn {
  padding: 14px 24px;
  background: #F4F6F4;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 700;
  color: #3A473C;
  text-decoration: none;
}

.form-error {
  background: #FEE4E2;
  color: #B42318;
  border-radius: 8px;
  padding: 12px 14px;
  font-size: 14px;
  font-weight: 700;
  margin-bottom: 20px;
}

@media (max-width: 1100px) {
  .kpi-grid,
  .dash-grid {
    grid-template-columns: 1fr 1fr;
  }
}

@media (max-width: 650px) {
  .dashboard { padding: 20px; }
  .dash-header { align-items: flex-start; flex-direction: column; }
  .kpi-grid,
  .dash-grid {
    grid-template-columns: 1fr;
  }
}
</style>
