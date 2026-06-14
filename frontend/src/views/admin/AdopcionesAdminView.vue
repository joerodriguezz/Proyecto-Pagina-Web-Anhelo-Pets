<script setup>
import { ref, computed, onMounted } from 'vue'
import { usePetsStore } from '../../stores/usePetsStore'

const store = usePetsStore()

// ─── Filtros ─────────────────────────────────────────────────────────────────
const filterStatus      = ref('Todos')
const filterSolicitante = ref('')
const filterMascota     = ref('')
const filterFecha       = ref('Todas')

function limpiarFiltros() {
  filterStatus.value      = 'Todos'
  filterSolicitante.value = ''
  filterMascota.value     = ''
  filterFecha.value       = 'Todas'
}

// ─── Solicitudes ─────────────────────────────────────────────────────────────
const solicitudes = ref([])

function cargarSolicitudes() {
  const guardadas = JSON.parse(localStorage.getItem('anhelo_solicitudes')) || []
  guardadas.forEach(item => {
    if (!item.estado && item.status) item.estado = item.status
  })
  solicitudes.value = guardadas
}

onMounted(() => { cargarSolicitudes() })

// ─── Helpers de fecha ────────────────────────────────────────────────────────
function parseDate(fechaStr) {
  if (!fechaStr) return null
  if (/^\d{2}\/\d{2}\/\d{4}$/.test(fechaStr)) {
    const [d, m, y] = fechaStr.split('/')
    return new Date(Number(y), Number(m) - 1, Number(d))
  }
  const d = new Date(fechaStr)
  return isNaN(d.getTime()) ? null : d
}

function startOfDay(date) {
  const d = new Date(date)
  d.setHours(0, 0, 0, 0)
  return d
}

// ─── Computed filtrado ────────────────────────────────────────────────────────
const filtered = computed(() => {
  const hoy    = startOfDay(new Date())
  const hace7  = new Date(hoy); hace7.setDate(hoy.getDate() - 6)
  const hace30 = new Date(hoy); hace30.setDate(hoy.getDate() - 29)
  const busqSol  = filterSolicitante.value.trim().toLowerCase()
  const busqMasc = filterMascota.value.trim().toLowerCase()

  return solicitudes.value.filter(s => {
    // 1. Estado
    if (filterStatus.value !== 'Todos' && s.estado !== filterStatus.value) return false

    // 2. Solicitante: nombre, cédula o correo
    if (busqSol) {
      const campos = [s.solicitante, s.cedula, s.email].filter(Boolean).map(v => v.toLowerCase())
      if (!campos.some(c => c.includes(busqSol))) return false
    }

    // 3. Mascota
    if (busqMasc) {
      if (!(s.mascota || '').toLowerCase().includes(busqMasc)) return false
    }

    // 4. Fecha
    if (filterFecha.value !== 'Todas') {
      const fecha = parseDate(s.fecha)
      if (!fecha) return false
      const d = startOfDay(fecha)
      if (filterFecha.value === 'Hoy'            && d.getTime() !== hoy.getTime()) return false
      if (filterFecha.value === 'Últimos 7 días'  && d < hace7)                    return false
      if (filterFecha.value === 'Últimos 30 días' && d < hace30)                   return false
    }

    return true
  })
})

// ─── Acciones ────────────────────────────────────────────────────────────────
function guardarSolicitudes() {
  localStorage.setItem('anhelo_solicitudes', JSON.stringify(solicitudes.value))
}

function sincronizarMascota(solicitud, nuevoEstadoMascota) {
  const mascota = solicitud.petId
    ? store.pets.find(p => p.id === solicitud.petId)
    : store.pets.find(p => p.name === solicitud.mascota)
  if (mascota) store.changeStatus(mascota.id, nuevoEstadoMascota)
}

function procesoSolicitud(id) {
  const s = solicitudes.value.find(s => s.id === id)
  if (!s) return
  s.estado = 'En proceso'
  guardarSolicitudes()
  sincronizarMascota(s, 'En proceso')
}

function aprobarSolicitud(id) {
  const s = solicitudes.value.find(s => s.id === id)
  if (!s) return
  s.estado = 'Aprobada'
  guardarSolicitudes()
  sincronizarMascota(s, 'Adoptada')
}

function rechazarSolicitud(id) {
  const s = solicitudes.value.find(s => s.id === id)
  if (!s) return
  s.estado = 'Rechazada'
  guardarSolicitudes()
  sincronizarMascota(s, 'Disponible')
}

// ─── Modal ───────────────────────────────────────────────────────────────────
const showDetailModal = ref(false)
const selectedRequest = ref(null)

function verDetalle(solicitud) {
  selectedRequest.value = solicitud
  showDetailModal.value = true
}

const statusClass = (estado) => ({
  'Pendiente':  'badge-pendiente',
  'En proceso': 'badge-proceso',
  'Aprobada':   'badge-aprobada',
  'Rechazada':  'badge-rechazada',
}[estado] || 'badge-neutral')
</script>

<template>
  <div class="view-container">

    <header class="page-header">
      <div>
        <h1 class="page-title">Solicitudes de Adopción</h1>
        <p class="page-sub">Gestión y seguimiento de solicitudes recibidas</p>
      </div>
    </header>

    <!-- KPIs -->
    <div class="don-summary">
      <div class="don-card kpi-yellow">
        <span class="don-label">Pendientes</span>
        <strong class="don-value">{{ solicitudes.filter(s => s.estado === 'Pendiente').length }}</strong>
      </div>
      <div class="don-card kpi-blue">
        <span class="don-label">En proceso</span>
        <strong class="don-value">{{ solicitudes.filter(s => s.estado === 'En proceso').length }}</strong>
      </div>
      <div class="don-card kpi-green">
        <span class="don-label">Aprobadas</span>
        <strong class="don-value">{{ solicitudes.filter(s => s.estado === 'Aprobada').length }}</strong>
      </div>
      <div class="don-card kpi-red">
        <span class="don-label">Rechazadas</span>
        <strong class="don-value">{{ solicitudes.filter(s => s.estado === 'Rechazada').length }}</strong>
      </div>
      <div class="don-card kpi-gray">
        <span class="don-label">Total</span>
        <strong class="don-value">{{ solicitudes.length }}</strong>
      </div>
    </div>

    <!-- Filtros -->
    <div class="filtros-panel">

      <!-- 1. Buscar solicitante -->
      <div class="filtro-group">
        <label class="filtro-label">Solicitante</label>
        <div class="filtro-input-wrap">
          <input
            v-model="filterSolicitante"
            type="text"
            class="filtro-input filtro-text"
            placeholder="Nombre, cédula o correo"
          />
        </div>
      </div>

      <!-- 2. Buscar mascota -->
      <div class="filtro-group">
        <label class="filtro-label">Mascota</label>
        <div class="filtro-input-wrap">
          <input
            v-model="filterMascota"
            type="text"
            class="filtro-input filtro-text"
            placeholder="Nombre de la mascota"
          />
        </div>
      </div>

      <!-- 3. Estado -->
      <div class="filtro-group">
        <label class="filtro-label">Estado</label>
        <div class="filtro-input-wrap">
          <select v-model="filterStatus" class="filtro-input filtro-select">
            <option>Todos</option>
            <option>Pendiente</option>
            <option>En proceso</option>
            <option>Aprobada</option>
            <option>Rechazada</option>
          </select>
          <span class="filtro-icon-right no-events">
            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
          </span>
        </div>
      </div>

      <!-- 4. Fecha -->
      <div class="filtro-group">
        <label class="filtro-label">Fecha</label>
        <div class="filtro-input-wrap">
          <select v-model="filterFecha" class="filtro-input filtro-select">
            <option>Todas</option>
            <option>Hoy</option>
            <option>Últimos 7 días</option>
            <option>Últimos 30 días</option>
          </select>
          <span class="filtro-icon-right no-events">
            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
          </span>
        </div>
      </div>

      <!-- 5. Limpiar -->
      <div class="filtro-group filtro-group-btn">
        <label class="filtro-label">&nbsp;</label>
        <button type="button" class="btn-limpiar" @click="limpiarFiltros">Limpiar filtros</button>
      </div>

    </div>

    <!-- Tabla -->
    <div class="table-wrapper">
      <div class="table-scroll">
        <table class="don-table" v-if="filtered.length > 0">
          <thead>
            <tr>
              <th>ID</th>
              <th>Solicitante</th>
              <th>Mascota</th>
              <th>Fecha</th>
              <th>Teléfono</th>
              <th>Estado</th>
              <th>Acción</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="s in filtered" :key="s.id" class="don-row">
              <td><span class="id-pill">{{ s.id }}</span></td>
              <td><span class="donor-name">{{ s.solicitante }}</span></td>
              <td><span class="pet-chip">{{ s.mascota }}</span></td>
              <td><span class="muted-text">{{ s.fecha }}</span></td>
              <td><span class="muted-text">{{ s.telefono }}</span></td>
              <td><span class="estado-badge" :class="statusClass(s.estado)">{{ s.estado }}</span></td>
              <td>
                <div class="action-group">
                  <button v-if="s.estado === 'Pendiente'" type="button" class="btn-accion btn-revisar" @click="procesoSolicitud(s.id)">Revisar</button>
                  <button v-if="s.estado === 'En proceso'" type="button" class="btn-accion btn-aprobar" @click="aprobarSolicitud(s.id)">Aprobar</button>
                  <button v-if="s.estado === 'En proceso'" type="button" class="btn-accion btn-rechazar" @click="rechazarSolicitud(s.id)">Rechazar</button>
                  <button type="button" class="btn-ver" @click="verDetalle(s)">Ver detalle</button>
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
      <div v-if="filtered.length > 0" class="table-footer">
        {{ filtered.length }} solicitud{{ filtered.length !== 1 ? 'es' : '' }} encontrada{{ filtered.length !== 1 ? 's' : '' }}
      </div>
    </div>

    <!-- Modal detalle -->
    <Transition name="modal-fade">
      <div v-if="showDetailModal" class="modal-overlay" @click.self="showDetailModal = false">
        <div class="modal-box modal-large">
          <div class="modal-head">
            <div class="modal-head-left">
              <div class="modal-meta">
                <span class="id-pill">{{ selectedRequest?.id }}</span>
                <span class="pet-chip">{{ selectedRequest?.mascota }}</span>
                <span class="estado-badge" :class="statusClass(selectedRequest?.estado)">{{ selectedRequest?.estado }}</span>
              </div>
              <h2 class="modal-title-large">{{ selectedRequest?.solicitante }}</h2>
            </div>
            <button type="button" class="modal-close" @click="showDetailModal = false">✕</button>
          </div>
          <div class="modal-sections">
            <div class="modal-section">
              <div class="modal-section-title">Datos Personales & Contacto</div>
              <div class="modal-grid modal-grid-3">
                <div class="modal-field"><span class="modal-field-label">Cédula</span><span class="modal-field-val">{{ selectedRequest?.cedula }}</span></div>
                <div class="modal-field"><span class="modal-field-label">Edad</span><span class="modal-field-val">{{ selectedRequest?.edad }}</span></div>
                <div class="modal-field"><span class="modal-field-label">Profesión</span><span class="modal-field-val">{{ selectedRequest?.profesion }}</span></div>
                <div class="modal-field"><span class="modal-field-label">Teléfono</span><span class="modal-field-val">{{ selectedRequest?.telefono }}</span></div>
                <div class="modal-field"><span class="modal-field-label">WhatsApp</span><span class="modal-field-val">{{ selectedRequest?.whatsapp }}</span></div>
                <div class="modal-field"><span class="modal-field-label">Correo</span><span class="modal-field-val">{{ selectedRequest?.email }}</span></div>
              </div>
            </div>
            <div class="modal-section">
              <div class="modal-section-title">Hogar & Estilo de Vida</div>
              <div class="modal-grid modal-grid-1">
                <div class="modal-field"><span class="modal-field-label">Dirección</span><span class="modal-field-val">{{ selectedRequest?.direccion }}</span></div>
                <div class="modal-field"><span class="modal-field-label">Personas del hogar</span><span class="modal-field-val">{{ selectedRequest?.hogar }}</span></div>
                <div class="modal-field"><span class="modal-field-label">Otras mascotas</span><span class="modal-field-val">{{ selectedRequest?.otrasMascotas }}</span></div>
              </div>
            </div>
            <div class="modal-section">
              <div class="modal-section-title">Evaluación</div>
              <div class="modal-grid modal-grid-1">
                <div class="modal-field modal-field-highlight"><span class="modal-field-label">¿Por qué desea adoptar esta mascota?</span><span class="modal-field-val modal-field-quote">{{ selectedRequest?.porqueMascota }}</span></div>
                <div class="modal-field modal-field-highlight"><span class="modal-field-label">Motivos de adopción</span><span class="modal-field-val modal-field-quote">{{ selectedRequest?.motivos }}</span></div>
              </div>
              <div class="modal-grid modal-grid-3" style="margin-top:10px">
                <div class="modal-field"><span class="modal-field-label">Horas sola</span><span class="modal-field-val">{{ selectedRequest?.horasSola }}</span></div>
                <div class="modal-field modal-span2"><span class="modal-field-label">Rutina diaria</span><span class="modal-field-val">{{ selectedRequest?.rutina }}</span></div>
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
.view-container {
  --verde: #3A473C; --verde-sec: #92A894; --fondo: #F7F8F7;
  --blanco: #FFFFFF; --texto: #2F352F; --texto-sec: #6C756D;
  --borde: #E8ECE8; --amarillo: #F5B942; --verde-ok: #4CAF6A;
  background: transparent;
}

/* Header */
.page-header { margin-bottom: 28px; }
.page-title  { font-size: 28px; font-weight: 800; color: var(--verde); letter-spacing: -0.5px; line-height: 1.1; margin: 0 0 4px; }
.page-sub    { font-size: 14px; color: var(--texto-sec); font-weight: 500; margin: 0; }

/* KPIs */
.don-summary { display: flex; gap: 14px; margin-bottom: 20px; flex-wrap: wrap; }
.don-card    { flex: 1; min-width: 140px; background: var(--blanco); border-radius: 14px; padding: 20px; border: 1px solid var(--borde); border-top: 3px solid var(--borde); display: flex; flex-direction: column; gap: 6px; }
.kpi-yellow  { border-top-color: var(--amarillo); }
.kpi-blue    { border-top-color: #6E9BFF; }
.kpi-green   { border-top-color: var(--verde-ok); }
.kpi-red     { border-top-color: #D06060; }
.kpi-gray    { border-top-color: var(--texto-sec); }
.don-label   { font-size: 11px; color: var(--texto-sec); font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; }
.don-value   { font-size: 26px; font-weight: 800; color: var(--verde); line-height: 1; }

/* Filtros */
.filtros-panel    { background: var(--blanco); border-radius: 14px; padding: 20px; margin-bottom: 20px; border: 1px solid var(--borde); display: flex; gap: 12px; flex-wrap: wrap; align-items: flex-end; }
.filtro-group     { display: flex; flex-direction: column; gap: 6px; flex: 0 0 auto; min-width: 160px; }
.filtro-group-btn { justify-content: flex-end; }
.filtro-label     { font-size: 11px; font-weight: 700; color: var(--verde); text-transform: uppercase; letter-spacing: 0.5px; }
.filtro-input-wrap { position: relative; display: flex; align-items: center; }
.filtro-input     { width: 100%; height: 38px; padding: 0 36px 0 12px; border-radius: 8px; border: 1.5px solid var(--borde); background: var(--fondo); font-size: 13px; color: var(--texto); font-family: inherit; outline: none; transition: border-color .18s; box-sizing: border-box; }
.filtro-input:focus { border-color: var(--verde-sec); background: var(--blanco); }
.filtro-text      { padding-right: 12px; }
.filtro-select    { appearance: none; -webkit-appearance: none; cursor: pointer; }
.filtro-icon-right { position: absolute; right: 11px; display: flex; align-items: center; color: var(--texto-sec); }
.no-events        { pointer-events: none; }
.btn-limpiar      { height: 38px; padding: 0 16px; border-radius: 8px; border: 1.5px solid var(--borde); background: var(--blanco); color: var(--texto-sec); font-size: 12px; font-weight: 700; cursor: pointer; font-family: inherit; white-space: nowrap; transition: all .18s; }
.btn-limpiar:hover { background: var(--verde); color: var(--blanco); border-color: var(--verde); }

/* Tabla */
.table-wrapper { background: var(--blanco); border-radius: 14px; border: 1px solid var(--borde); overflow: hidden; margin-bottom: 32px; }
.table-scroll  { overflow-x: auto; -webkit-overflow-scrolling: touch; }
.don-table     { width: 100%; border-collapse: collapse; min-width: 680px; }
.don-table thead tr { background: var(--verde); }
.don-table thead th { padding: 13px 16px; text-align: left; color: var(--blanco); font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.6px; white-space: nowrap; }
.don-table tbody tr { border-bottom: 1px solid var(--borde); transition: background .15s; }
.don-table tbody tr:last-child { border-bottom: none; }
.don-table tbody tr:hover { background: #F4F6F4; }
.don-table tbody td { padding: 13px 16px; vertical-align: middle; }
.table-footer  { padding: 12px 16px; border-top: 1px solid var(--borde); font-size: 12px; color: var(--texto-sec); font-weight: 500; }

/* Chips / badges */
.id-pill     { font-size: 11px; font-family: monospace; background: var(--fondo); border: 1px solid var(--borde); padding: 3px 9px; border-radius: 6px; color: var(--verde); font-weight: 700; white-space: nowrap; }
.pet-chip    { font-size: 12px; font-weight: 600; color: #4A6E4C; background: rgba(146,168,148,.12); padding: 3px 10px; border-radius: 7px; white-space: nowrap; }
.donor-name  { font-size: 13px; font-weight: 700; color: var(--texto); display: block; }
.muted-text  { font-size: 13px; color: var(--texto-sec); white-space: nowrap; }
.estado-badge    { display: inline-block; font-size: 11px; font-weight: 700; padding: 4px 12px; border-radius: 20px; white-space: nowrap; }
.badge-pendiente { background: #FFF7E0; color: #96650A; }
.badge-proceso   { background: rgba(110,155,255,.14); color: #4F73B8; }
.badge-aprobada  { background: #E8F5E9; color: #2E7D32; }
.badge-rechazada { background: #FDECEA; color: #B71C1C; }
.badge-neutral   { background: #F0F4F0; color: #7A847C; }

/* Acciones */
.action-group { display: flex; gap: 6px; flex-wrap: wrap; align-items: center; }
.btn-ver      { padding: 6px 14px; border-radius: 7px; border: 1.5px solid var(--borde); background: var(--blanco); color: var(--verde); font-size: 12px; font-weight: 700; cursor: pointer; transition: all .18s; white-space: nowrap; font-family: inherit; }
.btn-ver:hover { background: var(--verde); color: var(--blanco); border-color: var(--verde); }
.btn-accion   { padding: 5px 12px; border-radius: 7px; border: 1.5px solid transparent; font-size: 12px; font-weight: 700; cursor: pointer; white-space: nowrap; font-family: inherit; transition: all .15s; }
.btn-revisar  { background: rgba(110,155,255,.1); border-color: rgba(110,155,255,.3); color: #4F73B8; }
.btn-revisar:hover  { background: rgba(110,155,255,.2); }
.btn-aprobar  { background: #E8F5E9; border-color: rgba(76,175,80,.3); color: #2E7D32; }
.btn-aprobar:hover  { background: #C8E6C9; }
.btn-rechazar { background: #FDECEA; border-color: rgba(208,96,96,.25); color: #B71C1C; }
.btn-rechazar:hover { background: #FFCDD2; }

/* Empty */
.empty-state { text-align: center; padding: 52px 20px; }
.empty-icon  { font-size: 36px; margin-bottom: 12px; }
.empty-state p { color: #9AA89C; font-size: 14px; margin: 0; }

/* Modal */
.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,.35); backdrop-filter: blur(4px); z-index: 1000; display: flex; align-items: center; justify-content: center; padding: 24px; }
.modal-box     { background: var(--blanco); border-radius: 20px; padding: 28px; width: 100%; max-width: 480px; max-height: 88vh; overflow-y: auto; box-shadow: 0 20px 60px rgba(0,0,0,.18); }
.modal-large   { max-width: 760px; }
.modal-head    { display: flex; justify-content: space-between; align-items: flex-start; gap: 16px; margin-bottom: 24px; padding-bottom: 20px; border-bottom: 1.5px solid #F0F4F0; }
.modal-meta    { display: flex; gap: 8px; align-items: center; flex-wrap: wrap; margin-bottom: 10px; }
.modal-title-large { font-size: 24px; font-weight: 800; color: var(--verde); margin: 0; letter-spacing: -0.3px; }
.modal-close   { width: 34px; height: 34px; border-radius: 9px; border: 1.5px solid #EEF3EE; background: transparent; color: #9AA89C; font-size: 14px; cursor: pointer; display: flex; align-items: center; justify-content: center; transition: all .15s; }
.modal-close:hover { background: #F0F4F0; color: var(--verde); }
.modal-sections { display: flex; flex-direction: column; gap: 24px; margin-bottom: 4px; }
.modal-section  { display: flex; flex-direction: column; gap: 12px; }
.modal-section-title { font-size: 11px; font-weight: 800; color: #9AA89C; text-transform: uppercase; letter-spacing: .7px; display: flex; align-items: center; gap: 8px; }
.modal-section-title::before { content: ''; display: block; width: 3px; height: 14px; background: #92A894; border-radius: 2px; }
.modal-grid   { display: grid; gap: 10px; }
.modal-grid-3 { grid-template-columns: repeat(3, 1fr); }
.modal-grid-1 { grid-template-columns: 1fr; }
.modal-field  { background: #F7FAF7; border-radius: 12px; padding: 12px 14px; border: 1px solid #EEF3EE; display: flex; flex-direction: column; gap: 4px; }
.modal-field-highlight { background: #FAFCFA; border-left: 3px solid #92A894; }
.modal-span2  { grid-column: span 2; }
.modal-field-label { font-size: 11px; font-weight: 700; color: #9AA89C; text-transform: uppercase; letter-spacing: .4px; }
.modal-field-val   { font-size: 14px; font-weight: 600; color: var(--verde); line-height: 1.5; }
.modal-field-quote { font-style: italic; font-weight: 500; color: #3A4C3C; }
.modal-actions { display: flex; gap: 10px; padding-top: 16px; border-top: 1px solid #F0F4F0; margin-top: 20px; }
.btn-ghost     { padding: 11px 22px; border-radius: 12px; border: 1.5px solid #EEF3EE; background: transparent; color: #7A847C; font-size: 14px; font-weight: 700; cursor: pointer; transition: all .2s; }
.btn-ghost:hover { background: #F0F4F0; color: var(--verde); }

.modal-fade-enter-active { transition: all .22s ease; }
.modal-fade-leave-active { transition: all .16s ease; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity: 0; }
.modal-fade-enter-from .modal-box { transform: scale(.97) translateY(8px); }

@media (max-width: 900px) { .don-summary { display: grid; grid-template-columns: repeat(2,1fr); } }
@media (max-width: 640px) { .filtros-panel { flex-direction: column; } .modal-grid-3 { grid-template-columns: 1fr 1fr; } .modal-box { padding: 20px; } .don-summary { grid-template-columns: 1fr; } }
@media (max-width: 500px) { .modal-grid-3 { grid-template-columns: 1fr; } }


/* ── MOBILE RESPONSIVE ── */
@media (max-width: 768px) {
  .don-summary {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 10px;
  }

  .don-card:last-child {
    grid-column: span 2;
  }

  .filtros-panel {
    flex-direction: column;
    gap: 10px;
    padding: 16px;
  }

  .filtro-group {
    min-width: unset;
    width: 100%;
  }

  .filtro-group-btn {
    width: 100%;
  }

  .btn-limpiar {
    width: 100%;
    justify-content: center;
  }

  .table-wrapper {
    overflow: hidden;
  }

  .table-scroll {
    overflow-x: auto;
    -webkit-overflow-scrolling: touch;
  }

  .modal-box {
    padding: 20px 16px;
    margin: 0 8px;
    max-height: 95vh;
  }

  .modal-grid-3 {
    grid-template-columns: 1fr 1fr;
  }

  .modal-acciones {
    flex-direction: column;
  }

  .action-group {
    flex-wrap: wrap;
  }
}

@media (max-width: 480px) {
  .don-summary {
    grid-template-columns: 1fr;
  }

  .don-card:last-child {
    grid-column: span 1;
  }

  .modal-grid-3 {
    grid-template-columns: 1fr;
  }

  .modal-span2 {
    grid-column: span 1;
  }
}


</style>