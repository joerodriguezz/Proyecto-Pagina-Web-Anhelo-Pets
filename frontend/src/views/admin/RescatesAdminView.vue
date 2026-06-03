<script setup>
import { computed, onMounted, ref } from 'vue'
import {
  animalsApi,
  fosterHomesApi,
  fosterPlacementsApi,
  rescuesApi,
} from '../../services/api'

const rescues = ref([])
const fosterHomes = ref([])
const placements = ref([])
const animals = ref([])
const loading = ref(false)
const errorMessage = ref('')
const successMessage = ref('')

const rescueForm = ref(emptyRescueForm())
const fosterHomeForm = ref(emptyFosterHomeForm())
const placementForm = ref(emptyPlacementForm())

const activeFosterHomes = computed(() => fosterHomes.value.filter((home) => home.active))

function emptyRescueForm() {
  return {
    rescateId: null,
    animalId: '',
    fecha: '',
    ubicacion: '',
    descripcion: '',
    status: 'Activo',
    fosterHomeId: '',
  }
}

function emptyFosterHomeForm() {
  return {
    fosterHomeId: null,
    name: '',
    address: '',
    phone: '',
    responsible: '',
    capacity: 1,
    active: true,
  }
}

function emptyPlacementForm() {
  return {
    animalFosterPlacementId: null,
    animalId: '',
    fosterHomeId: '',
    startDate: new Date().toISOString().slice(0, 10),
    endDate: '',
    notes: '',
  }
}

async function loadData() {
  loading.value = true
  errorMessage.value = ''

  try {
    const [rescueRows, fosterRows, placementRows, animalRows] = await Promise.all([
      rescuesApi.getAll(),
      fosterHomesApi.getAll(),
      fosterPlacementsApi.getAll(),
      animalsApi.getAll({ status: 'Todos' }),
    ])

    rescues.value = rescueRows
    fosterHomes.value = fosterRows
    placements.value = placementRows
    animals.value = animalRows
  } catch (error) {
    errorMessage.value = error.message || 'No se pudo cargar la informacion.'
  } finally {
    loading.value = false
  }
}

function nullableNumber(value) {
  return value === '' || value === null || value === undefined ? null : Number(value)
}

async function saveRescue() {
  errorMessage.value = ''
  successMessage.value = ''

  try {
    const payload = {
      animalId: nullableNumber(rescueForm.value.animalId),
      fecha: rescueForm.value.fecha,
      ubicacion: rescueForm.value.ubicacion,
      descripcion: rescueForm.value.descripcion,
      status: rescueForm.value.status,
      fosterHomeId: nullableNumber(rescueForm.value.fosterHomeId),
      createdBy: 'frontend',
      modifiedBy: 'frontend',
    }

    if (rescueForm.value.rescateId) {
      await rescuesApi.update(rescueForm.value.rescateId, payload)
      successMessage.value = 'Rescate actualizado.'
    } else {
      await rescuesApi.create(payload)
      successMessage.value = 'Rescate registrado.'
    }

    rescueForm.value = emptyRescueForm()
    await loadData()
  } catch (error) {
    errorMessage.value = error.message || 'No se pudo guardar el rescate.'
  }
}

function editRescue(rescue) {
  rescueForm.value = {
    rescateId: rescue.rescateId,
    animalId: rescue.animalId || '',
    fecha: rescue.fecha,
    ubicacion: rescue.ubicacion,
    descripcion: rescue.descripcion,
    status: rescue.status || 'Activo',
    fosterHomeId: rescue.fosterHomeId || '',
  }
}

async function closeRescue(rescue) {
  try {
    await rescuesApi.close(rescue.rescateId)
    successMessage.value = 'Rescate cerrado.'
    await loadData()
  } catch (error) {
    errorMessage.value = error.message || 'No se pudo cerrar el rescate.'
  }
}

async function saveFosterHome() {
  errorMessage.value = ''
  successMessage.value = ''

  try {
    const payload = {
      name: fosterHomeForm.value.name,
      address: fosterHomeForm.value.address,
      phone: fosterHomeForm.value.phone,
      responsible: fosterHomeForm.value.responsible,
      capacity: Number(fosterHomeForm.value.capacity || 1),
      active: fosterHomeForm.value.active,
      createdBy: 'frontend',
      modifiedBy: 'frontend',
    }

    if (fosterHomeForm.value.fosterHomeId) {
      await fosterHomesApi.update(fosterHomeForm.value.fosterHomeId, payload)
      successMessage.value = 'Casa cuna actualizada.'
    } else {
      await fosterHomesApi.create(payload)
      successMessage.value = 'Casa cuna registrada.'
    }

    fosterHomeForm.value = emptyFosterHomeForm()
    await loadData()
  } catch (error) {
    errorMessage.value = error.message || 'No se pudo guardar la casa cuna.'
  }
}

function editFosterHome(home) {
  fosterHomeForm.value = {
    fosterHomeId: home.fosterHomeId,
    name: home.name,
    address: home.address,
    phone: home.phone,
    responsible: home.responsible,
    capacity: home.capacity,
    active: home.active,
  }
}

async function deactivateFosterHome(home) {
  try {
    await fosterHomesApi.deactivate(home.fosterHomeId)
    successMessage.value = 'Casa cuna desactivada.'
    await loadData()
  } catch (error) {
    errorMessage.value = error.message || 'No se pudo desactivar la casa cuna.'
  }
}

async function savePlacement() {
  errorMessage.value = ''
  successMessage.value = ''

  try {
    const payload = {
      animalId: Number(placementForm.value.animalId),
      fosterHomeId: Number(placementForm.value.fosterHomeId),
      startDate: placementForm.value.startDate,
      endDate: placementForm.value.endDate || null,
      notes: placementForm.value.notes,
      createdBy: 'frontend',
      modifiedBy: 'frontend',
    }

    if (placementForm.value.animalFosterPlacementId) {
      await fosterPlacementsApi.update(placementForm.value.animalFosterPlacementId, payload)
      successMessage.value = 'Asignacion actualizada.'
    } else {
      await fosterPlacementsApi.create(payload)
      successMessage.value = 'Mascota asignada a casa cuna.'
    }

    placementForm.value = emptyPlacementForm()
    await loadData()
  } catch (error) {
    errorMessage.value = error.message || 'No se pudo guardar la asignacion.'
  }
}

function editPlacement(placement) {
  placementForm.value = {
    animalFosterPlacementId: placement.animalFosterPlacementId,
    animalId: placement.animalId,
    fosterHomeId: placement.fosterHomeId,
    startDate: placement.startDate,
    endDate: placement.endDate || '',
    notes: placement.notes,
  }
}

async function closePlacement(placement) {
  try {
    await fosterPlacementsApi.close(placement.animalFosterPlacementId)
    successMessage.value = 'Asignacion cerrada.'
    await loadData()
  } catch (error) {
    errorMessage.value = error.message || 'No se pudo cerrar la asignacion.'
  }
}

onMounted(loadData)
</script>

<template>
  <div class="view-container">
    <header class="page-header">
      <div>
        <h1 class="admin-page-title">Rescates y casas cuna</h1>
        <p class="admin-page-sub">Registro, validacion y cuidado temporal de mascotas</p>
      </div>
      <button class="btn-secondary" :disabled="loading" @click="loadData">Actualizar</button>
    </header>

    <p v-if="errorMessage" class="form-error">{{ errorMessage }}</p>
    <p v-if="successMessage" class="form-success">{{ successMessage }}</p>

    <section class="work-section">
      <div class="section-header">
        <h2>Registrar rescate</h2>
      </div>

      <form class="form-grid" @submit.prevent="saveRescue">
        <div class="form-group">
          <label>Mascota asociada</label>
          <select v-model="rescueForm.animalId" class="custom-input">
            <option value="">Sin asociar</option>
            <option v-for="animal in animals" :key="animal.animalId" :value="animal.animalId">
              {{ animal.animalName }} - {{ animal.species }}
            </option>
          </select>
        </div>

        <div class="form-group">
          <label>Fecha de rescate *</label>
          <input v-model="rescueForm.fecha" type="date" class="custom-input" required />
        </div>

        <div class="form-group full-width">
          <label>Ubicacion *</label>
          <input v-model="rescueForm.ubicacion" class="custom-input" placeholder="Canton, distrito, senas exactas" required />
        </div>

        <div class="form-group full-width">
          <label>Descripcion *</label>
          <textarea v-model="rescueForm.descripcion" class="custom-input custom-textarea" required></textarea>
        </div>

        <div class="form-group">
          <label>Casa cuna asignada</label>
          <select v-model="rescueForm.fosterHomeId" class="custom-input">
            <option value="">Sin asignar</option>
            <option v-for="home in activeFosterHomes" :key="home.fosterHomeId" :value="home.fosterHomeId">
              {{ home.name }}
            </option>
          </select>
        </div>

        <div class="form-group">
          <label>Estado</label>
          <select v-model="rescueForm.status" class="custom-input">
            <option>Activo</option>
            <option>Cerrado</option>
          </select>
        </div>

        <div class="form-actions full-width">
          <button class="btn-primary" type="submit">{{ rescueForm.rescateId ? 'Actualizar rescate' : 'Guardar rescate' }}</button>
          <button class="btn-secondary" type="button" @click="rescueForm = emptyRescueForm()">Limpiar</button>
        </div>
      </form>

      <div class="table-wrapper">
        <table class="data-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Mascota</th>
              <th>Fecha</th>
              <th>Ubicacion</th>
              <th>Casa cuna</th>
              <th>Estado</th>
              <th class="text-right">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading">
              <td colspan="7">Cargando rescates...</td>
            </tr>
            <tr v-for="rescue in rescues" :key="rescue.rescateId">
              <td><span class="id-code">R-{{ rescue.rescateId }}</span></td>
              <td>{{ rescue.animalName || 'Sin asociar' }}</td>
              <td>{{ rescue.fecha }}</td>
              <td>{{ rescue.ubicacion }}</td>
              <td>{{ rescue.fosterHomeName || 'Sin asignar' }}</td>
              <td><span class="badge" :class="rescue.status === 'Activo' ? 'badge-green' : 'badge-gray'">{{ rescue.status }}</span></td>
              <td class="text-right">
                <button class="text-action" @click="editRescue(rescue)">Editar</button>
                <button class="text-action danger" @click="closeRescue(rescue)">Cerrar</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <section class="work-section">
      <div class="section-header">
        <h2>Casas cuna</h2>
      </div>

      <form class="form-grid" @submit.prevent="saveFosterHome">
        <div class="form-group">
          <label>Nombre *</label>
          <input v-model="fosterHomeForm.name" class="custom-input" placeholder="Familia Mora" required />
        </div>
        <div class="form-group">
          <label>Responsable *</label>
          <input v-model="fosterHomeForm.responsible" class="custom-input" placeholder="Maria Mora" required />
        </div>
        <div class="form-group">
          <label>Telefono *</label>
          <input v-model="fosterHomeForm.phone" class="custom-input" placeholder="+506 8888-8888" required />
        </div>
        <div class="form-group">
          <label>Capacidad *</label>
          <input v-model="fosterHomeForm.capacity" class="custom-input" type="number" min="1" required />
        </div>
        <div class="form-group full-width">
          <label>Direccion *</label>
          <input v-model="fosterHomeForm.address" class="custom-input" placeholder="Provincia, canton, distrito y senas" required />
        </div>
        <label class="checkbox-line">
          <input v-model="fosterHomeForm.active" type="checkbox" />
          Casa cuna activa
        </label>
        <div class="form-actions full-width">
          <button class="btn-primary" type="submit">{{ fosterHomeForm.fosterHomeId ? 'Actualizar casa cuna' : 'Guardar casa cuna' }}</button>
          <button class="btn-secondary" type="button" @click="fosterHomeForm = emptyFosterHomeForm()">Limpiar</button>
        </div>
      </form>

      <div class="table-wrapper">
        <table class="data-table">
          <thead>
            <tr>
              <th>Nombre</th>
              <th>Responsable</th>
              <th>Telefono</th>
              <th>Capacidad</th>
              <th>Estado</th>
              <th class="text-right">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="home in fosterHomes" :key="home.fosterHomeId">
              <td>{{ home.name }}</td>
              <td>{{ home.responsible }}</td>
              <td>{{ home.phone }}</td>
              <td>{{ home.capacity }}</td>
              <td><span class="badge" :class="home.active ? 'badge-green' : 'badge-gray'">{{ home.active ? 'Activa' : 'Inactiva' }}</span></td>
              <td class="text-right">
                <button class="text-action" @click="editFosterHome(home)">Editar</button>
                <button class="text-action danger" @click="deactivateFosterHome(home)">Desactivar</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <section class="work-section">
      <div class="section-header">
        <h2>Asignar mascota a casa cuna</h2>
      </div>

      <form class="form-grid" @submit.prevent="savePlacement">
        <div class="form-group">
          <label>Mascota *</label>
          <select v-model="placementForm.animalId" class="custom-input" required>
            <option value="" disabled>Seleccionar mascota</option>
            <option v-for="animal in animals" :key="animal.animalId" :value="animal.animalId">
              {{ animal.animalName }} - {{ animal.species }}
            </option>
          </select>
        </div>
        <div class="form-group">
          <label>Casa cuna *</label>
          <select v-model="placementForm.fosterHomeId" class="custom-input" required>
            <option value="" disabled>Seleccionar casa cuna</option>
            <option v-for="home in activeFosterHomes" :key="home.fosterHomeId" :value="home.fosterHomeId">
              {{ home.name }}
            </option>
          </select>
        </div>
        <div class="form-group">
          <label>Fecha inicio *</label>
          <input v-model="placementForm.startDate" type="date" class="custom-input" required />
        </div>
        <div class="form-group">
          <label>Fecha fin</label>
          <input v-model="placementForm.endDate" type="date" class="custom-input" />
        </div>
        <div class="form-group full-width">
          <label>Notas</label>
          <textarea v-model="placementForm.notes" class="custom-input custom-textarea"></textarea>
        </div>
        <div class="form-actions full-width">
          <button class="btn-primary" type="submit">{{ placementForm.animalFosterPlacementId ? 'Actualizar asignacion' : 'Asignar mascota' }}</button>
          <button class="btn-secondary" type="button" @click="placementForm = emptyPlacementForm()">Limpiar</button>
        </div>
      </form>

      <div class="table-wrapper">
        <table class="data-table">
          <thead>
            <tr>
              <th>Mascota</th>
              <th>Casa cuna</th>
              <th>Inicio</th>
              <th>Fin</th>
              <th>Notas</th>
              <th class="text-right">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="placement in placements" :key="placement.animalFosterPlacementId">
              <td>{{ placement.animalName }}</td>
              <td>{{ placement.fosterHomeName }}</td>
              <td>{{ placement.startDate }}</td>
              <td>{{ placement.endDate || 'Actual' }}</td>
              <td>{{ placement.notes }}</td>
              <td class="text-right">
                <button class="text-action" @click="editPlacement(placement)">Editar</button>
                <button class="text-action danger" @click="closePlacement(placement)">Cerrar</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  </div>
</template>

<style scoped>
.view-container {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.page-header,
.section-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
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

.work-section,
.table-wrapper {
  background: white;
  border-radius: 8px;
  padding: 24px;
  box-shadow: 0 4px 20px rgba(58, 71, 60, 0.03);
}

.work-section {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.section-header h2 {
  font-size: 18px;
  color: #3A473C;
  font-weight: 800;
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 16px;
}

.full-width {
  grid-column: 1 / -1;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-group label,
.checkbox-line {
  font-size: 13px;
  color: #3A473C;
  font-weight: 700;
}

.checkbox-line {
  display: flex;
  align-items: center;
  gap: 10px;
}

.custom-input {
  width: 100%;
  min-height: 44px;
  padding: 11px 13px;
  border-radius: 8px;
  border: 2px solid #F4F6F4;
  background: #F9FAF9;
  color: #3A473C;
  font: inherit;
  outline: none;
}

.custom-textarea {
  min-height: 96px;
  resize: vertical;
}

.custom-input:focus {
  background: white;
  border-color: #92A894;
}

.form-actions {
  display: flex;
  gap: 12px;
}

.btn-primary,
.btn-secondary,
.text-action {
  border: none;
  cursor: pointer;
  font-weight: 800;
}

.btn-primary,
.btn-secondary {
  min-height: 42px;
  padding: 0 18px;
  border-radius: 8px;
}

.btn-primary {
  background: #3A473C;
  color: white;
}

.btn-secondary {
  background: #F4F6F4;
  color: #3A473C;
}

.btn-secondary:disabled {
  opacity: 0.65;
  cursor: not-allowed;
}

.table-wrapper {
  overflow-x: auto;
  padding: 18px;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
}

.data-table th {
  color: #6C756D;
  font-size: 12px;
  padding: 0 12px 12px 0;
  border-bottom: 1px solid #F4F6F4;
}

.data-table td {
  color: #3A473C;
  font-size: 14px;
  padding: 14px 12px 14px 0;
  border-bottom: 1px solid #FAFAFA;
  vertical-align: top;
}

.text-right {
  text-align: right;
}

.id-code {
  font-family: monospace;
  font-size: 12px;
  background: #F4F6F4;
  padding: 4px 8px;
  border-radius: 8px;
}

.badge {
  display: inline-flex;
  padding: 5px 9px;
  border-radius: 8px;
  font-size: 12px;
  font-weight: 800;
}

.badge-green {
  background: rgba(146, 168, 148, 0.2);
  color: #5A6E5C;
}

.badge-gray {
  background: #F4F6F4;
  color: #6C756D;
}

.text-action {
  background: transparent;
  color: #5A6E5C;
  margin-left: 10px;
}

.text-action.danger {
  color: #B42318;
}

.form-error,
.form-success {
  border-radius: 8px;
  padding: 12px 14px;
  font-size: 14px;
  font-weight: 700;
}

.form-error {
  background: #FEE4E2;
  color: #B42318;
}

.form-success {
  background: #E7F1E8;
  color: #4F6F55;
}

@media (max-width: 860px) {
  .page-header,
  .section-header {
    align-items: flex-start;
    flex-direction: column;
  }

  .form-grid {
    grid-template-columns: 1fr;
  }

  .form-actions {
    flex-direction: column;
  }
}
</style>
