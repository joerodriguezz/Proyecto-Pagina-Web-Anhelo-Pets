<script setup>
import { ref, computed, onMounted } from 'vue'
import { usePetsStore } from '../../stores/usePetsStore'

const store = usePetsStore()

const filterStatus = ref('Todos')
const solicitudes = ref([])
const showDetailModal = ref(false)
const selectedRequest = ref(null)

/* ─────────────────────────────
   CARGAR SOLICITUDES
───────────────────────────── */
function cargarSolicitudes() {

  const guardadas =
    JSON.parse(
      localStorage.getItem('anhelo_solicitudes')
    ) || []

  // Normalizar: unificar status → estado por si hay registros viejos
  guardadas.forEach(item => {
    if (!item.estado && item.status) {
      item.estado = item.status
    }
  })

  solicitudes.value = guardadas

}

onMounted(() => {

  cargarSolicitudes()

})

/* ─────────────────────────────
   FILTROS
───────────────────────────── */
const filtered = computed(() => {

  if (filterStatus.value === 'Todos') {

    return solicitudes.value

  }

  return solicitudes.value.filter(

    s => s.estado === filterStatus.value

  )

})

/* ─────────────────────────────
   GUARDAR SOLICITUDES
───────────────────────────── */
function guardarSolicitudes() {

  localStorage.setItem(
    'anhelo_solicitudes',
    JSON.stringify(solicitudes.value)
  )

}

/* ─────────────────────────────
   SINCRONIZAR MASCOTA
   Actualiza el estado de la
   mascota en el store y persiste
───────────────────────────── */
function sincronizarMascota(solicitud, nuevoEstadoMascota) {

  // Buscar por petId primero; si no existe, buscar por nombre (compatibilidad)
  const petId = solicitud.petId
  const nombreMascota = solicitud.mascota

  const mascota = petId
    ? store.pets.find(p => p.id === petId)
    : store.pets.find(p => p.name === nombreMascota)

  if (mascota) {
    store.changeStatus(mascota.id, nuevoEstadoMascota)
  }

}

/* ─────────────────────────────
   EN PROCESO
   solicitud → "En proceso"
   mascota   → "En proceso"
───────────────────────────── */
function procesoSolicitud(id) {

  const solicitud =
    solicitudes.value.find(s => s.id === id)

  if (!solicitud) return

  solicitud.estado = 'En proceso'

  guardarSolicitudes()

  sincronizarMascota(solicitud, 'En proceso')

}

/* ─────────────────────────────
   APROBAR
   solicitud → "Aprobada"
   mascota   → "Adoptada"
───────────────────────────── */
function aprobarSolicitud(id) {

  const solicitud =
    solicitudes.value.find(s => s.id === id)

  if (!solicitud) return

  solicitud.estado = 'Aprobada'

  guardarSolicitudes()

  sincronizarMascota(solicitud, 'Adoptada')

}

/* ─────────────────────────────
   RECHAZAR
   solicitud → "Rechazada"
   mascota   → "Disponible"
───────────────────────────── */
function rechazarSolicitud(id) {

  const solicitud =
    solicitudes.value.find(s => s.id === id)

  if (!solicitud) return

  solicitud.estado = 'Rechazada'

  guardarSolicitudes()

  sincronizarMascota(solicitud, 'Disponible')

}

/* ─────────────────────────────
   VER DETALLE
───────────────────────────── */
function verDetalle(solicitud) {

  selectedRequest.value = solicitud

  showDetailModal.value = true

}

/* ─────────────────────────────
   BADGES
───────────────────────────── */
const statusClass = (estado) => {

  return {
    'Pendiente':  'badge-yellow',
    'En proceso': 'badge-blue',
    'Aprobada':   'badge-green',
    'Rechazada':  'badge-red'
  }[estado] || 'badge-gray'

}
</script>

<template>

  <div class="view-container">

    <!-- HEADER -->
    <header class="page-header">

      <h1 class="admin-page-title">
        Solicitudes de Adopción
      </h1>

      <div class="filter-chips">

        <button
          v-for="s in [
            'Todos',
            'Pendiente',
            'En proceso',
            'Aprobada',
            'Rechazada'
          ]"
          :key="s"
          class="chip"
          :class="{ active: filterStatus === s }"
          @click="filterStatus = s"
        >
          {{ s }}
        </button>

      </div>

    </header>

    <!-- RESUMEN -->
    <div class="summary-row">

      <div class="sum-card pending">
        <span class="sum-label">Pendientes</span>
        <strong class="sum-value">
          {{ solicitudes.filter(s => s.estado === 'Pendiente').length }}
        </strong>
      </div>

      <div class="sum-card process">
        <span class="sum-label">En proceso</span>
        <strong class="sum-value">
          {{ solicitudes.filter(s => s.estado === 'En proceso').length }}
        </strong>
      </div>

      <div class="sum-card approved">
        <span class="sum-label">Aprobadas</span>
        <strong class="sum-value">
          {{ solicitudes.filter(s => s.estado === 'Aprobada').length }}
        </strong>
      </div>

      <div class="sum-card rejected">
        <span class="sum-label">Rechazadas</span>
        <strong class="sum-value">
          {{ solicitudes.filter(s => s.estado === 'Rechazada').length }}
        </strong>
      </div>

      <div class="sum-card total">
        <span class="sum-label">Total</span>
        <strong class="sum-value">
          {{ solicitudes.length }}
        </strong>
      </div>

    </div>

    <!-- TABLA -->
    <div class="table-container">

      <table
        class="admin-table"
        v-if="filtered.length > 0"
      >

        <thead>
          <tr>
            <th>ID</th>
            <th>Solicitante</th>
            <th>Mascota</th>
            <th>Fecha</th>
            <th>Teléfono</th>
            <th>Estado</th>
            <th class="th-actions">Acciones</th>
          </tr>
        </thead>

        <tbody>

          <tr
            v-for="s in filtered"
            :key="s.id"
          >

            <td class="td-id text-nowrap">
              {{ s.id }}
            </td>

            <td class="bold-text text-nowrap">
              {{ s.solicitante }}
            </td>

            <td class="bold-text text-nowrap">
              {{ s.mascota }}
            </td>

            <td class="subtle-text text-nowrap">
              {{ s.fecha }}
            </td>

            <td class="phone-text text-nowrap">
              {{ s.telefono }}
            </td>

            <td>
              <span
                class="badge"
                :class="statusClass(s.estado)"
              >
                {{ s.estado }}
              </span>
            </td>

            <td class="td-actions">

              <div class="action-btns">

                <!-- REVISAR: Pendiente → En proceso -->
                <button
                  v-if="s.estado === 'Pendiente'"
                  type="button"
                  class="action-btn process-btn"
                  @click="procesoSolicitud(s.id)"
                >
                  Revisar
                </button>

                <!-- APROBAR: En proceso → Aprobada + mascota Adoptada -->
                <button
                  v-if="s.estado === 'En proceso'"
                  type="button"
                  class="action-btn approve"
                  @click="aprobarSolicitud(s.id)"
                >
                  Aprobar
                </button>

                <!-- RECHAZAR: En proceso → Rechazada + mascota Disponible -->
                <button
                  v-if="s.estado === 'En proceso'"
                  type="button"
                  class="action-btn reject"
                  @click="rechazarSolicitud(s.id)"
                >
                  Rechazar
                </button>

                <!-- VER DETALLE -->
                <button
                  type="button"
                  class="action-btn view"
                  @click="verDetalle(s)"
                >
                  Ver solicitud
                </button>

              </div>

            </td>

          </tr>

        </tbody>

      </table>

      <div
        v-else
        class="empty-state"
      >
        No hay solicitudes registradas
      </div>

    </div>

    <!-- MODAL DETALLE -->
    <div
      v-if="showDetailModal"
      class="modal-overlay"
      @click.self="showDetailModal = false"
    >

      <div class="detail-modal">

        <!-- HEADER MODAL -->
        <div class="detail-header">

          <div>

            <div class="modal-badge-container">

              <span class="detail-tag">
                SOLICITUD {{ selectedRequest?.id }}
              </span>

              <span class="pet-interest-badge">
                Interés: {{ selectedRequest?.mascota }}
              </span>

              <!-- Estado actual de la solicitud -->
              <span
                class="badge"
                :class="statusClass(selectedRequest?.estado)"
              >
                {{ selectedRequest?.estado }}
              </span>

            </div>

            <h2>
              {{ selectedRequest?.solicitante }}
            </h2>

          </div>

          <button
            type="button"
            class="close-btn"
            @click="showDetailModal = false"
          >
            ×
          </button>

        </div>

        <!-- CONTENIDO -->
        <div class="detail-content">

          <!-- DATOS PERSONALES -->
          <div class="section-block">

            <h3 class="section-title">
              Datos Personales & Contacto
            </h3>

            <div class="grid-3-cols">

              <div class="detail-card">
                <span class="detail-label">Cédula</span>
                <p class="data-text">{{ selectedRequest?.cedula }}</p>
              </div>

              <div class="detail-card">
                <span class="detail-label">Edad</span>
                <p class="data-text">{{ selectedRequest?.edad }}</p>
              </div>

              <div class="detail-card">
                <span class="detail-label">Profesión</span>
                <p class="data-text">{{ selectedRequest?.profesion }}</p>
              </div>

              <div class="detail-card">
                <span class="detail-label">Teléfono</span>
                <p class="data-text">{{ selectedRequest?.telefono }}</p>
              </div>

              <div class="detail-card">
                <span class="detail-label">WhatsApp</span>
                <p class="data-text">{{ selectedRequest?.whatsapp }}</p>
              </div>

              <div class="detail-card">
                <span class="detail-label">Correo</span>
                <p class="data-text">{{ selectedRequest?.email }}</p>
              </div>

            </div>

          </div>

          <!-- HOGAR -->
          <div class="section-block">

            <h3 class="section-title">
              Hogar & Estilo de Vida
            </h3>

            <div class="grid-mixed">

              <div class="detail-card full">
                <span class="detail-label">Dirección</span>
                <p class="data-text long-text">{{ selectedRequest?.direccion }}</p>
              </div>

              <div class="detail-card full">
                <span class="detail-label">Personas del hogar</span>
                <p class="data-text long-text">{{ selectedRequest?.hogar }}</p>
              </div>

              <div class="detail-card full">
                <span class="detail-label">Otras mascotas</span>
                <p class="data-text long-text">{{ selectedRequest?.otrasMascotas }}</p>
              </div>

            </div>

          </div>

          <!-- EVALUACIÓN -->
          <div class="section-block">

            <h3 class="section-title">
              Evaluación
            </h3>

            <div class="grid-mixed">

              <div class="detail-card full question-highlight">
                <span class="detail-label">¿Por qué desea adoptar esta mascota?</span>
                <p class="data-text long-text italic-quote">{{ selectedRequest?.porqueMascota }}</p>
              </div>

              <div class="detail-card full question-highlight">
                <span class="detail-label">Motivos de adopción</span>
                <p class="data-text long-text italic-quote">{{ selectedRequest?.motivos }}</p>
              </div>

              <div class="detail-card">
                <span class="detail-label">Horas sola</span>
                <p class="data-text">{{ selectedRequest?.horasSola }}</p>
              </div>

              <div class="detail-card full-from-two">
                <span class="detail-label">Rutina diaria</span>
                <p class="data-text long-text">{{ selectedRequest?.rutina }}</p>
              </div>

            </div>

          </div>

        </div>

      </div>

    </div>

  </div>

</template>

<style scoped>
.process {
  border-top: 5px solid #6E9BFF;
}

.badge-blue {
  background: rgba(110,155,255,0.14);
  color: #4F73B8;
}

.process-btn {
  background: #E8F0FF;
  color: #4F73B8;
}
</style>
<style scoped>
.view-container {
  padding: 10px;
}

.page-header {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  margin-bottom: 34px;
  gap: 20px;
}

.admin-page-title {
  font-size: 42px;
  font-weight: 800;
  color: #2F3B31;
}

.filter-chips {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
}

.chip {
  height: 54px;
  padding: 0 32px;
  border-radius: 999px;
  border: none;
  background: white;
  color: #5E665F;
  font-weight: 700;
  font-size: 16px;
  cursor: pointer;
  transition: 0.2s;
  box-shadow: 0 2px 8px rgba(0,0,0,0.04);
}

.chip.active {
  background: #92A894;
  color: white;
  box-shadow: 0 4px 12px rgba(146, 168, 148, 0.3);
}

.summary-row {
  display: grid;
  grid-template-columns: repeat(5, 1fr);
  gap: 14px;
  margin-bottom: 24px;
}

.sum-card {
  background: white;
  border-radius: 18px;
  padding: 18px;
}

.sum-label {
  display: block;
  color: #6C756D;
  margin-bottom: 8px;
  font-size: 13px;
}

.sum-value {
  font-size: 38px;
  color: #2F3B31;
  font-weight: 800;
}

.pending  { border-top: 5px solid #F9C17A; }
.approved { border-top: 5px solid #92A894; }
.rejected { border-top: 5px solid #EB7777; }
.total    { border-top: 5px solid #6C756D; }

.table-container {
  background: white;
  border-radius: 28px;
  padding: 10px;
  overflow-x: auto;
}

.admin-table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
}

.admin-table th,
.admin-table td {
  padding: 20px 24px;
  font-size: 15px;
  color: #2F3B31;
  vertical-align: middle;
}

.admin-table th {
  font-weight: 700;
  color: #6C756D;
  border-bottom: 1px solid #EEF3EE;
}

.admin-table tbody tr {
  border-bottom: 1px solid #FAFAFA;
}

.admin-table tbody tr:last-child {
  border-bottom: none;
}

.td-id {
  font-weight: 700;
  color: #92A894;
  font-family: monospace;
}

.text-nowrap  { white-space: nowrap; }
.bold-text    { font-weight: 700; color: #2F3B31; }
.subtle-text  { color: #6C756D; }
.phone-text   { font-weight: 600; color: #4A544C; }

.th-actions, .td-actions {
  width: 320px;
  text-align: left !important;
}

.badge {
  padding: 8px 14px;
  border-radius: 999px;
  font-size: 13px;
  font-weight: 700;
  display: inline-block;
}

.badge-yellow { background: rgba(249,193,122,0.15); color: #C88A37; }
.badge-green  { background: rgba(146,168,148,0.15); color: #5A705C; }
.badge-red    { background: rgba(235,119,119,0.12); color: #C45252; }
.badge-gray   { background: #EEF3EE; color: #6C756D; }

.action-btns {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.action-btn {
  min-width: 95px;
  height: 36px;
  padding: 0 14px;
  border-radius: 10px;
  border: none;
  font-weight: 700;
  font-size: 12px;
  cursor: pointer;
  transition: 0.2s ease;
  white-space: nowrap;
}

.approve { background: #E7F1E8; color: #567058; }
.reject  { background: #FCE8E8; color: #C45252; }
.view    { background: #3A473C; color: white; }

.action-btn:hover {
  transform: translateY(-2px);
  opacity: 0.92;
}

/* ── MODAL ── */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 17, 0.4);
  backdrop-filter: blur(4px);
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 24px;
  z-index: 999;
}

.detail-modal {
  width: 100%;
  max-width: 920px;
  max-height: 85vh;
  overflow-y: auto;
  background: white;
  border-radius: 36px;
  padding: 40px;
  box-shadow: 0 20px 50px rgba(0,0,0,0.08);
}

.detail-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 36px;
  border-bottom: 1px solid #EEF3EE;
  padding-bottom: 24px;
}

.modal-badge-container {
  display: flex;
  gap: 8px;
  align-items: center;
  margin-bottom: 8px;
  flex-wrap: wrap;
}

.detail-tag {
  color: #92A894;
  font-size: 11px;
  font-weight: 800;
  letter-spacing: 1px;
  background: #F4F6F4;
  padding: 4px 10px;
  border-radius: 6px;
}

.pet-interest-badge {
  background: rgba(146, 168, 148, 0.12);
  color: #4A574C;
  font-size: 12px;
  font-weight: 700;
  padding: 4px 12px;
  border-radius: 6px;
}

.detail-header h2 {
  font-size: 38px;
  font-weight: 800;
  color: #2F3B31;
  margin: 4px 0 0 0;
}

.detail-content {
  display: flex;
  flex-direction: column;
  gap: 32px;
}

.section-block {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.section-title {
  font-size: 14px;
  font-weight: 800;
  color: #92A894;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin: 0;
  border-left: 3px solid #92A894;
  padding-left: 10px;
}

.grid-3-cols {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 14px;
}

.grid-mixed {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 14px;
}

.detail-card {
  background: #FAFAFA;
  border-radius: 16px;
  padding: 16px 20px;
  border: 1px solid #F4F6F4;
  transition: all 0.2s ease;
}

.detail-card:hover {
  background: #FDFDFD;
  border-color: #E2EAE3;
}

.detail-card.full          { grid-column: span 3; }
.detail-card.full-from-two { grid-column: span 2; }

.question-highlight {
  background: #FCFDFD;
  border-left: 4px solid #92A894;
}

.detail-label {
  display: block;
  margin-bottom: 6px;
  color: #7E8880;
  font-weight: 700;
  font-size: 12px;
}

.data-text {
  color: #2F3B31;
  font-weight: 600;
  font-size: 15px;
  margin: 0;
}

.long-text {
  line-height: 1.6;
  font-weight: 500;
  color: #38453A;
}

.italic-quote {
  font-style: italic;
  color: #435245;
  font-size: 15px;
}

.close-btn {
  width: 44px;
  height: 44px;
  border: none;
  border-radius: 14px;
  background: #F4F6F4;
  font-size: 24px;
  color: #6C756D;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: 0.2s;
}

.close-btn:hover {
  background: #2F3B31;
  color: white;
}

.empty-state {
  background: white;
  border-radius: 24px;
  padding: 50px;
  text-align: center;
  color: #6C756D;
  font-weight: 600;
}

@media (max-width: 850px) {
  .summary-row { grid-template-columns: 1fr 1fr; }
  .grid-3-cols { grid-template-columns: 1fr 1fr; }
  .grid-mixed  { grid-template-columns: 1fr; }
  .detail-card.full, .detail-card.full-from-two { grid-column: span 1; }
}

@media (max-width: 550px) {
  .grid-3-cols { grid-template-columns: 1fr; }
}
</style>