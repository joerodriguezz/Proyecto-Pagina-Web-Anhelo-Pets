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
───────────────────────────── */
function sincronizarMascota(solicitud, nuevoEstadoMascota) {

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
───────────────────────────── */
function procesoSolicitud(id) {

  const solicitud = solicitudes.value.find(s => s.id === id)
  if (!solicitud) return
  solicitud.estado = 'En proceso'
  guardarSolicitudes()
  sincronizarMascota(solicitud, 'En proceso')

}

/* ─────────────────────────────
   APROBAR
───────────────────────────── */
function aprobarSolicitud(id) {

  const solicitud = solicitudes.value.find(s => s.id === id)
  if (!solicitud) return
  solicitud.estado = 'Aprobada'
  guardarSolicitudes()
  sincronizarMascota(solicitud, 'Adoptada')

}

/* ─────────────────────────────
   RECHAZAR
───────────────────────────── */
function rechazarSolicitud(id) {

  const solicitud = solicitudes.value.find(s => s.id === id)
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
    'Pendiente':  'badge-peach',
    'En proceso': 'badge-blue',
    'Aprobada':   'badge-green',
    'Rechazada':  'badge-red',
  }[estado] || 'badge-gray'

}
</script>

<template>
  <div class="view-container">

    <!-- ══════════════════════════════════════
         CABECERA
    ══════════════════════════════════════ -->
    <header class="page-header">
      <div class="header-text">
        <h1 class="page-title">Solicitudes de Adopción</h1>
        <p class="page-sub">Gestión y seguimiento de solicitudes recibidas</p>
      </div>
    </header>

    <!-- ══════════════════════════════════════
         TARJETAS DE ESTADÍSTICAS
    ══════════════════════════════════════ -->
    <div class="stats-grid">

      <div class="stat-card stat-pending">
        <div class="stat-icon"></div>
        <div class="stat-body">
          <div class="stat-value">{{ solicitudes.filter(s => s.estado === 'Pendiente').length }}</div>
          <div class="stat-label">Pendientes</div>
          <div class="stat-desc">En espera de revisión</div>
        </div>
      </div>

      <div class="stat-card stat-process">
        <div class="stat-icon"></div>
        <div class="stat-body">
          <div class="stat-value">{{ solicitudes.filter(s => s.estado === 'En proceso').length }}</div>
          <div class="stat-label">En proceso</div>
          <div class="stat-desc">Actualmente evaluadas</div>
        </div>
      </div>

      <div class="stat-card stat-approved">
        <div class="stat-icon"></div>
        <div class="stat-body">
          <div class="stat-value">{{ solicitudes.filter(s => s.estado === 'Aprobada').length }}</div>
          <div class="stat-label">Aprobadas</div>
          <div class="stat-desc">Adopciones confirmadas</div>
        </div>
      </div>

      <div class="stat-card stat-rejected">
        <div class="stat-icon"></div>
        <div class="stat-body">
          <div class="stat-value">{{ solicitudes.filter(s => s.estado === 'Rechazada').length }}</div>
          <div class="stat-label">Rechazadas</div>
          <div class="stat-desc">No aprobadas</div>
        </div>
      </div>

      <div class="stat-card stat-total">
        <div class="stat-icon"></div>
        <div class="stat-body">
          <div class="stat-value">{{ solicitudes.length }}</div>
          <div class="stat-label">Total</div>
          <div class="stat-desc">Solicitudes registradas</div>
        </div>
      </div>

    </div>

    <!-- ══════════════════════════════════════
         FILTROS
    ══════════════════════════════════════ -->
    <div class="filters-panel">
      <div class="filter-group">
        <span class="filter-group-label">Estado</span>
        <div class="tab-group">
          <button
            v-for="s in ['Todos', 'Pendiente', 'En proceso', 'Aprobada', 'Rechazada']"
            :key="s"
            class="tab-btn"
            :class="{ 'tab-active': filterStatus === s }"
            @click="filterStatus = s"
          >{{ s }}</button>
        </div>
      </div>
    </div>

    <!-- ══════════════════════════════════════
         TABLA
    ══════════════════════════════════════ -->
    <div class="table-card">

      <div class="table-header-row">
        <span class="table-count">
          {{ filtered.length }} solicitud{{ filtered.length !== 1 ? 'es' : '' }}
        </span>
      </div>

      <div class="table-scroll">
        <table class="data-table" v-if="filtered.length > 0">
          <thead>
            <tr>
              <th>ID</th>
              <th>Solicitante</th>
              <th>Mascota</th>
              <th>Fecha</th>
              <th>Teléfono</th>
              <th>Estado</th>
              <th class="th-right">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="s in filtered"
              :key="s.id"
              class="table-row"
            >

              <!-- ID -->
              <td>
                <span class="id-chip">{{ s.id }}</span>
              </td>

              <!-- Solicitante -->
              <td class="name-cell">{{ s.solicitante }}</td>

              <!-- Mascota -->
              <td>
                <span class="pet-chip">{{ s.mascota }}</span>
              </td>

              <!-- Fecha -->
              <td class="muted-cell">{{ s.fecha }}</td>

              <!-- Teléfono -->
              <td class="phone-cell">{{ s.telefono }}</td>

              <!-- Estado -->
              <td>
                <span class="badge" :class="statusClass(s.estado)">{{ s.estado }}</span>
              </td>

              <!-- Acciones -->
              <td class="td-right">
                <div class="action-group">

                  <button
                    v-if="s.estado === 'Pendiente'"
                    type="button"
                    class="act-pill act-process"
                    @click="procesoSolicitud(s.id)"
                  >Revisar</button>

                  <button
                    v-if="s.estado === 'En proceso'"
                    type="button"
                    class="act-pill act-approve"
                    @click="aprobarSolicitud(s.id)"
                  >Aprobar</button>

                  <button
                    v-if="s.estado === 'En proceso'"
                    type="button"
                    class="act-pill act-reject"
                    @click="rechazarSolicitud(s.id)"
                  >Rechazar</button>

                  <button
                    type="button"
                    class="act-pill act-view"
                    @click="verDetalle(s)"
                  >Ver</button>

                </div>
              </td>

            </tr>
          </tbody>
        </table>

        <div v-else class="empty-state">
          <div class="empty-icon">📋</div>
          <p>No hay solicitudes que coincidan con el filtro</p>
        </div>
      </div>
    </div>

    <!-- ══════════════════════════════════════
         MODAL: Ver detalle
    ══════════════════════════════════════ -->
    <Transition name="modal-fade">
      <div
        v-if="showDetailModal"
        class="modal-overlay"
        @click.self="showDetailModal = false"
      >
        <div class="modal-box modal-large">

          <!-- Head -->
          <div class="modal-head">
            <div class="modal-head-left">
              <div class="modal-meta">
                <span class="id-chip">{{ selectedRequest?.id }}</span>
                <span class="pet-chip">{{ selectedRequest?.mascota }}</span>
                <span class="badge" :class="statusClass(selectedRequest?.estado)">{{ selectedRequest?.estado }}</span>
              </div>
              <h2 class="modal-title-large">{{ selectedRequest?.solicitante }}</h2>
            </div>
            <button type="button" class="modal-close" @click="showDetailModal = false">✕</button>
          </div>

          <!-- Secciones -->
          <div class="modal-sections">

            <!-- Datos personales -->
            <div class="modal-section">
              <div class="modal-section-title">Datos Personales & Contacto</div>
              <div class="detail-grid detail-grid-3">
                <div class="detail-item">
                  <span class="detail-label">Cédula</span>
                  <span class="detail-val">{{ selectedRequest?.cedula }}</span>
                </div>
                <div class="detail-item">
                  <span class="detail-label">Edad</span>
                  <span class="detail-val">{{ selectedRequest?.edad }}</span>
                </div>
                <div class="detail-item">
                  <span class="detail-label">Profesión</span>
                  <span class="detail-val">{{ selectedRequest?.profesion }}</span>
                </div>
                <div class="detail-item">
                  <span class="detail-label">Teléfono</span>
                  <span class="detail-val">{{ selectedRequest?.telefono }}</span>
                </div>
                <div class="detail-item">
                  <span class="detail-label">WhatsApp</span>
                  <span class="detail-val">{{ selectedRequest?.whatsapp }}</span>
                </div>
                <div class="detail-item">
                  <span class="detail-label">Correo</span>
                  <span class="detail-val">{{ selectedRequest?.email }}</span>
                </div>
              </div>
            </div>

            <!-- Hogar -->
            <div class="modal-section">
              <div class="modal-section-title">Hogar & Estilo de Vida</div>
              <div class="detail-grid detail-grid-1">
                <div class="detail-item">
                  <span class="detail-label">Dirección</span>
                  <span class="detail-val">{{ selectedRequest?.direccion }}</span>
                </div>
                <div class="detail-item">
                  <span class="detail-label">Personas del hogar</span>
                  <span class="detail-val">{{ selectedRequest?.hogar }}</span>
                </div>
                <div class="detail-item">
                  <span class="detail-label">Otras mascotas</span>
                  <span class="detail-val">{{ selectedRequest?.otrasMascotas }}</span>
                </div>
              </div>
            </div>

            <!-- Evaluación -->
            <div class="modal-section">
              <div class="modal-section-title">Evaluación</div>
              <div class="detail-grid detail-grid-1">
                <div class="detail-item detail-item-highlight">
                  <span class="detail-label">¿Por qué desea adoptar esta mascota?</span>
                  <span class="detail-val detail-val-quote">{{ selectedRequest?.porqueMascota }}</span>
                </div>
                <div class="detail-item detail-item-highlight">
                  <span class="detail-label">Motivos de adopción</span>
                  <span class="detail-val detail-val-quote">{{ selectedRequest?.motivos }}</span>
                </div>
              </div>
              <div class="detail-grid detail-grid-3" style="margin-top:10px">
                <div class="detail-item">
                  <span class="detail-label">Horas sola</span>
                  <span class="detail-val">{{ selectedRequest?.horasSola }}</span>
                </div>
                <div class="detail-item detail-item-span2">
                  <span class="detail-label">Rutina diaria</span>
                  <span class="detail-val">{{ selectedRequest?.rutina }}</span>
                </div>
              </div>
            </div>

          </div>

          <div class="modal-actions">
            <button type="button" class="btn-ghost" @click="showDetailModal = false">Cerrar</button>
          </div>

        </div>
      </div>
    </Transition>

  </div>
</template>

<style scoped>

/* ═══════════════════════════════════════
   BASE
═══════════════════════════════════════ */
.view-container {
  background: transparent;
}

/* ═══════════════════════════════════════
   CABECERA
═══════════════════════════════════════ */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 28px;
  gap: 16px;
}

.page-title {
  font-size: 26px;
  font-weight: 800;
  color: #2D3A2E;
  letter-spacing: -0.5px;
  margin: 0 0 4px;
}

.page-sub {
  font-size: 14px;
  color: #7A847C;
  font-weight: 500;
  margin: 0;
}

/* ═══════════════════════════════════════
   STATS
═══════════════════════════════════════ */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(5, 1fr);
  gap: 14px;
  margin-bottom: 24px;
}

.stat-card {
  background: white;
  border-radius: 18px;
  padding: 20px;
  display: flex;
  align-items: center;
  gap: 14px;
  border: 1.5px solid transparent;
  transition: box-shadow .2s;
}
.stat-card:hover {
  box-shadow: 0 4px 20px rgba(0,0,0,.06);
}

.stat-icon {
  font-size: 26px;
  line-height: 1;
  flex-shrink: 0;
}

.stat-value {
  font-size: 26px;
  font-weight: 800;
  color: #2D3A2E;
  line-height: 1;
  margin-bottom: 3px;
}
.stat-label {
  font-size: 13px;
  font-weight: 700;
  color: #2D3A2E;
  margin-bottom: 2px;
}
.stat-desc {
  font-size: 12px;
  color: #9AA89C;
}

.stat-pending  { border-color: rgba(249,193,122,.3);  background: rgba(249,193,122,.05); }
.stat-process  { border-color: rgba(110,155,255,.25); background: rgba(110,155,255,.04); }
.stat-approved { border-color: rgba(146,168,148,.3);  background: rgba(146,168,148,.06); }
.stat-rejected { border-color: rgba(208,96,96,.2);    background: rgba(208,96,96,.04); }
.stat-total    { border-color: #EAEEEA; }

/* ═══════════════════════════════════════
   FILTROS
═══════════════════════════════════════ */
.filters-panel {
  background: white;
  border-radius: 18px;
  padding: 16px 20px;
  margin-bottom: 20px;
  border: 1px solid #EEF3EE;
  display: flex;
  align-items: center;
  gap: 20px;
  flex-wrap: wrap;
}

.filter-group {
  display: flex;
  align-items: center;
  gap: 10px;
}
.filter-group-label {
  font-size: 12px;
  font-weight: 700;
  color: #9AA89C;
  text-transform: uppercase;
  letter-spacing: .5px;
  white-space: nowrap;
}

.tab-group {
  display: flex;
  gap: 4px;
  background: #F0F4F0;
  padding: 3px;
  border-radius: 12px;
}
.tab-btn {
  padding: 6px 14px;
  border-radius: 9px;
  border: none;
  background: transparent;
  font-size: 13px;
  font-weight: 600;
  color: #7A847C;
  cursor: pointer;
  transition: all .18s;
  white-space: nowrap;
}
.tab-btn:hover { color: #2D3A2E; background: rgba(255,255,255,.6); }
.tab-btn.tab-active {
  background: white;
  color: #2D3A2E;
  box-shadow: 0 1px 6px rgba(0,0,0,.08);
}

/* ═══════════════════════════════════════
   TABLA
═══════════════════════════════════════ */
.table-card {
  background: white;
  border-radius: 20px;
  border: 1px solid #EEF3EE;
  overflow: hidden;
  margin-bottom: 32px;
}

.table-header-row {
  padding: 16px 20px;
  border-bottom: 1px solid #F0F4F0;
}
.table-count {
  font-size: 13px;
  font-weight: 700;
  color: #9AA89C;
}

.table-scroll {
  overflow-x: auto;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
  min-width: 680px;
}

.data-table th {
  padding: 12px 16px;
  font-size: 11px;
  font-weight: 800;
  color: #9AA89C;
  text-transform: uppercase;
  letter-spacing: .6px;
  background: #FAFCFA;
  text-align: center;
  white-space: nowrap;
}
.th-right { text-align: right; }

.data-table td {
  padding: 13px 16px;
  font-size: 14px;
  border-bottom: 1px solid #F5F8F5;
  vertical-align: middle;
}
.td-right { text-align: right; }

.table-row { transition: background .15s; }
.table-row:hover { background: #FAFCFA; }
.table-row:last-child td { border-bottom: none; }

/* Celdas */
.id-chip {
  font-size: 11px;
  font-family: monospace;
  background: #F0F4F0;
  color: #3A473C;
  padding: 3px 8px;
  border-radius: 7px;
  font-weight: 600;
  white-space: nowrap;
}

.pet-chip {
  font-size: 12px;
  font-weight: 600;
  color: #4A6E4C;
  background: rgba(146,168,148,.12);
  padding: 3px 10px;
  border-radius: 7px;
  white-space: nowrap;
}

.name-cell  { font-weight: 600; color: #2D3A2E; white-space: nowrap; }
.muted-cell { color: #9AA89C; white-space: nowrap; font-size: 13px; }
.phone-cell { font-weight: 600; color: #4A544C; white-space: nowrap; }

/* Badges */
.badge {
  padding: 5px 11px;
  border-radius: 9px;
  font-size: 12px;
  font-weight: 700;
  display: inline-block;
  white-space: nowrap;
}
.badge-peach { background: rgba(249,193,122,.2);  color: #C88A30; }
.badge-blue  { background: rgba(110,155,255,.14); color: #4F73B8; }
.badge-green { background: rgba(146,168,148,.18); color: #4A6E4C; }
.badge-red   { background: rgba(208,96,96,.1);    color: #B04040; }
.badge-gray  { background: #F0F4F0;              color: #7A847C; }

/* Botones de acción */
.action-group {
  display: flex;
  gap: 6px;
  justify-content: flex-end;
  flex-wrap: wrap;
}

.act-pill {
  height: 30px;
  padding: 0 13px;
  border-radius: 9px;
  border: 1.5px solid transparent;
  font-size: 12px;
  font-weight: 700;
  cursor: pointer;
  transition: all .15s;
  white-space: nowrap;
}
.act-pill:hover { transform: translateY(-1px); }

.act-process {
  background: rgba(110,155,255,.1);
  border-color: rgba(110,155,255,.3);
  color: #4F73B8;
}
.act-process:hover { background: rgba(110,155,255,.18); }

.act-approve {
  background: rgba(146,168,148,.12);
  border-color: rgba(146,168,148,.35);
  color: #4A6E4C;
}
.act-approve:hover { background: rgba(146,168,148,.2); }

.act-reject {
  background: rgba(208,96,96,.08);
  border-color: rgba(208,96,96,.25);
  color: #B04040;
}
.act-reject:hover { background: rgba(208,96,96,.14); }

.act-view {
  background: #3A473C;
  border-color: #3A473C;
  color: white;
}
.act-view:hover { background: #2D372F; }

/* Empty */
.empty-state {
  text-align: center;
  padding: 52px 20px;
}
.empty-icon { font-size: 36px; margin-bottom: 12px; }
.empty-state p { color: #9AA89C; font-size: 14px; margin: 0; }

/* ═══════════════════════════════════════
   MODAL
═══════════════════════════════════════ */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(20,28,22,.45);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 20px;
}

.modal-box {
  background: white;
  border-radius: 20px;
  padding: 28px;
  width: 100%;
  max-width: 480px;
  max-height: 88vh;
  overflow-y: auto;
  box-shadow: 0 20px 60px rgba(0,0,0,.18);
}
.modal-large { max-width: 760px; }

.modal-head {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 16px;
  margin-bottom: 24px;
  padding-bottom: 20px;
  border-bottom: 1.5px solid #F0F4F0;
}

.modal-meta {
  display: flex;
  gap: 8px;
  align-items: center;
  flex-wrap: wrap;
  margin-bottom: 10px;
}

.modal-title-large {
  font-size: 24px;
  font-weight: 800;
  color: #2D3A2E;
  margin: 0;
  letter-spacing: -0.3px;
}

.modal-close {
  width: 34px; height: 34px;
  border-radius: 9px;
  border: 1.5px solid #EEF3EE;
  background: transparent;
  color: #9AA89C;
  font-size: 14px;
  cursor: pointer;
  display: flex; align-items: center; justify-content: center;
  flex-shrink: 0;
  transition: all .15s;
}
.modal-close:hover { background: #F0F4F0; color: #2D3A2E; }

/* Secciones del modal */
.modal-sections {
  display: flex;
  flex-direction: column;
  gap: 24px;
  margin-bottom: 4px;
}

.modal-section {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.modal-section-title {
  font-size: 11px;
  font-weight: 800;
  color: #9AA89C;
  text-transform: uppercase;
  letter-spacing: .7px;
  display: flex;
  align-items: center;
  gap: 8px;
}
.modal-section-title::before {
  content: '';
  display: block;
  width: 3px;
  height: 14px;
  background: #92A894;
  border-radius: 2px;
}

.detail-grid {
  display: grid;
  gap: 10px;
}
.detail-grid-3 { grid-template-columns: repeat(3, 1fr); }
.detail-grid-1 { grid-template-columns: 1fr; }

.detail-item {
  background: #F7FAF7;
  border-radius: 12px;
  padding: 12px 14px;
  border: 1px solid #EEF3EE;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.detail-item-highlight {
  background: #FAFCFA;
  border-left: 3px solid #92A894;
}

.detail-item-span2 {
  grid-column: span 2;
}

.detail-label {
  font-size: 11px;
  font-weight: 700;
  color: #9AA89C;
  text-transform: uppercase;
  letter-spacing: .4px;
}

.detail-val {
  font-size: 14px;
  font-weight: 600;
  color: #2D3A2E;
  line-height: 1.5;
}
.detail-val-quote {
  font-style: italic;
  font-weight: 500;
  color: #3A4C3C;
  font-size: 14px;
}

/* Actions bar */
.modal-actions {
  display: flex;
  gap: 10px;
  padding-top: 16px;
  border-top: 1px solid #F0F4F0;
  margin-top: 20px;
}

.btn-ghost {
  padding: 11px 22px;
  border-radius: 12px;
  border: 1.5px solid #EEF3EE;
  background: transparent;
  color: #7A847C;
  font-size: 14px;
  font-weight: 700;
  cursor: pointer;
  transition: all .2s;
}
.btn-ghost:hover { background: #F0F4F0; color: #2D3A2E; }

/* Animaciones */
.modal-fade-enter-active { transition: all .22s ease; }
.modal-fade-leave-active { transition: all .16s ease; }
.modal-fade-enter-from   { opacity: 0; }
.modal-fade-leave-to     { opacity: 0; }
.modal-fade-enter-from .modal-box { transform: scale(.97) translateY(8px); }

/* ═══════════════════════════════════════
   RESPONSIVO
═══════════════════════════════════════ */
@media (max-width: 1100px) {
  .stats-grid { grid-template-columns: repeat(3, 1fr); }
}
@media (max-width: 750px) {
  .stats-grid { grid-template-columns: repeat(2, 1fr); }
  .detail-grid-3 { grid-template-columns: 1fr 1fr; }
  .detail-item-span2 { grid-column: span 1; }
}
@media (max-width: 500px) {
  .stats-grid { grid-template-columns: 1fr; }
  .page-header { flex-direction: column; align-items: flex-start; }
  .tab-btn { padding: 5px 10px; font-size: 12px; }
  .detail-grid-3 { grid-template-columns: 1fr; }
  .modal-box { padding: 20px; }
  .modal-title-large { font-size: 20px; }
}
</style>