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

    <!-- CABECERA -->
    <header class="page-header">
      <div class="brand-row">
        <div class="brand-mark">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="9" y1="13" x2="15" y2="13"/><line x1="9" y1="17" x2="13" y2="17"/></svg>
        </div>
        <div>
          <h1 class="admin-page-title">Solicitudes de Adopción</h1>
          <p class="admin-page-sub">Gestión y seguimiento de solicitudes recibidas</p>
        </div>
      </div>
    </header>

    <!-- TARJETAS RESUMEN -->
    <div class="don-summary">
      <div class="don-card pendiente-card">
        <div class="don-icon pendiente-icon">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="9"/><polyline points="12 7 12 12 16 14"/></svg>
        </div>
        <strong class="don-value">{{ solicitudes.filter(s => s.estado === 'Pendiente').length }}</strong>
        <span class="don-label">Pendientes</span>
        <span class="don-desc">Por revisar</span>
      </div>
      <div class="don-card proceso-card">
        <div class="don-icon proceso-icon">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/></svg>
        </div>
        <strong class="don-value">{{ solicitudes.filter(s => s.estado === 'En proceso').length }}</strong>
        <span class="don-label">En proceso</span>
        <span class="don-desc">En evaluación</span>
      </div>
      <div class="don-card aprobada-card">
        <div class="don-icon aprobada-icon">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/></svg>
        </div>
        <strong class="don-value">{{ solicitudes.filter(s => s.estado === 'Aprobada').length }}</strong>
        <span class="don-label">Aprobadas</span>
        <span class="don-desc">Adopciones confirmadas</span>
      </div>
      <div class="don-card rechazada-card">
        <div class="don-icon rechazada-icon">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="9"/><line x1="15" y1="9" x2="9" y2="15"/><line x1="9" y1="9" x2="15" y2="15"/></svg>
        </div>
        <strong class="don-value">{{ solicitudes.filter(s => s.estado === 'Rechazada').length }}</strong>
        <span class="don-label">Rechazadas</span>
        <span class="don-desc">No procedieron</span>
      </div>
      <div class="don-card total-card">
        <div class="don-icon total-icon">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="7" height="7" rx="1.5"/><rect x="14" y="3" width="7" height="7" rx="1.5"/><rect x="3" y="14" width="7" height="7" rx="1.5"/><rect x="14" y="14" width="7" height="7" rx="1.5"/></svg>
        </div>
        <strong class="don-value">{{ solicitudes.length }}</strong>
        <span class="don-label">Total</span>
        <span class="don-desc">En el sistema</span>
      </div>
    </div>

    <!-- FILTROS -->
    <div class="filtros-panel">

      <div class="filtros-row">
        <!-- Estado tabs -->
        <div class="filtro-group filtro-group--tabs">
          <label class="filtro-label">Estado</label>
          <div class="tabs-wrap">
            <button
              v-for="s in ['Todos', 'Pendiente', 'En proceso', 'Aprobada', 'Rechazada']"
              :key="s"
              type="button"
              class="tab-btn"
              :class="{ active: filterStatus === s }"
              @click="filterStatus = s"
            >{{ s }}</button>
          </div>
        </div>

        <!-- Fecha tabs -->
        <div class="filtro-group filtro-group--tabs">
          <label class="filtro-label">Fecha</label>
          <div class="tabs-wrap">
            <button
              v-for="f in ['Todas', 'Hoy', 'Últimos 7 días', 'Últimos 30 días']"
              :key="f"
              type="button"
              class="tab-btn"
              :class="{ active: filterFecha === f }"
              @click="filterFecha = f"
            >{{ f }}</button>
          </div>
        </div>
      </div>

      <div class="filtros-divider"></div>

      <div class="filtros-row filtros-row--end">
        <!-- Solicitante -->
        <div class="filtro-group filtro-group--search">
          <label class="filtro-label">Solicitante</label>
          <div class="filtro-input-wrap">
            <span class="filtro-icon filtro-icon--left">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
            </span>
            <input
              v-model="filterSolicitante"
              type="text"
              class="filtro-input filtro-input--icon-left"
              placeholder="Nombre, cédula o correo"
            />
          </div>
        </div>

        <!-- Mascota -->
        <div class="filtro-group filtro-group--search">
          <label class="filtro-label">Mascota</label>
          <div class="filtro-input-wrap">
            <span class="filtro-icon filtro-icon--left">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
            </span>
            <input
              v-model="filterMascota"
              type="text"
              class="filtro-input filtro-input--icon-left"
              placeholder="Nombre de la mascota"
            />
          </div>
        </div>

        <!-- Limpiar -->
        <div class="filtro-group filtro-group--btn">
          <button
            type="button"
            class="btn-limpiar"
            :class="{ 'btn-limpiar--activo': filterStatus !== 'Todos' || filterSolicitante.trim() !== '' || filterMascota.trim() !== '' || filterFecha !== 'Todas' }"
            @click="limpiarFiltros"
          >Limpiar filtros</button>
        </div>
      </div>
    </div>

    <!-- ESTADO VACÍO -->
    <div v-if="filtered.length === 0" class="empty-state">
      <svg xmlns="http://www.w3.org/2000/svg" width="36" height="36" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
      <p class="empty-title">No hay solicitudes que coincidan con el filtro</p>
      <p class="empty-sub">Ajusta los filtros para ver más resultados.</p>
    </div>

    <!-- TABLA -->
    <div v-else class="table-wrapper">
      <div class="table-scroll">
        <table class="don-table">
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
              <td><span class="fecha-text">{{ s.fecha }}</span></td>
              <td><span class="fecha-text">{{ s.telefono }}</span></td>
              <td><span class="estado-badge" :class="statusClass(s.estado)">{{ s.estado }}</span></td>
              <td>
                <div class="action-group">
                  <button v-if="s.estado === 'Pendiente'" type="button" class="btn-accion-pill btn-revisar" @click="procesoSolicitud(s.id)">Revisar</button>
                  <button v-if="s.estado === 'En proceso'" type="button" class="btn-accion-pill btn-aprobar" @click="aprobarSolicitud(s.id)">Aprobar</button>
                  <button v-if="s.estado === 'En proceso'" type="button" class="btn-accion-pill btn-rechazar" @click="rechazarSolicitud(s.id)">Rechazar</button>
                  <button type="button" class="icon-only icon-only--ver" @click="verDetalle(s)" data-tooltip="Ver detalle">
                    <img src="/img-acciones/eye.png" alt="Ver detalle" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div class="table-footer">
        {{ filtered.length }} solicitud{{ filtered.length !== 1 ? 'es' : '' }} encontrada{{ filtered.length !== 1 ? 's' : '' }}
      </div>
    </div>

    <!-- MODAL DETALLE — mismo lenguaje visual que el expediente de Ver mascota -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showDetailModal" class="modal-overlay" @click.self="showDetailModal = false">
          <div class="modal-box modal-box--uniform">
            <button type="button" class="modal-close" @click="showDetailModal = false">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>

            <div class="modal-head">
              <div class="modal-head-avatar">{{ selectedRequest?.solicitante?.charAt(0) }}</div>
              <div class="modal-head-left">
                <div class="modal-meta">
                  <span class="id-pill">{{ selectedRequest?.id }}</span>
                  <span class="pet-chip">{{ selectedRequest?.mascota }}</span>
                  <span class="estado-badge" :class="statusClass(selectedRequest?.estado)">{{ selectedRequest?.estado }}</span>
                </div>
                <h2 class="modal-title-large">{{ selectedRequest?.solicitante }}</h2>
              </div>
            </div>

            <div class="uniform-scroll">
              <div class="modal-sections">

                <div class="modal-section">
                  <div class="modal-section-title">Datos personales</div>
                  <div class="modal-grid modal-grid-3">
                    <div class="modal-field"><span class="modal-field-label">Cédula</span><span class="modal-field-val">{{ selectedRequest?.cedula }}</span></div>
                    <div class="modal-field"><span class="modal-field-label">Edad</span><span class="modal-field-val">{{ selectedRequest?.edad }}</span></div>
                    <div class="modal-field"><span class="modal-field-label">Profesión</span><span class="modal-field-val">{{ selectedRequest?.profesion }}</span></div>
                  </div>
                </div>

                <div class="modal-section">
                  <div class="modal-section-title">Contacto</div>
                  <div class="modal-grid modal-grid-3">
                    <div class="modal-field"><span class="modal-field-label">Teléfono</span><span class="modal-field-val">{{ selectedRequest?.telefono }}</span></div>
                    <div class="modal-field"><span class="modal-field-label">WhatsApp</span><span class="modal-field-val">{{ selectedRequest?.whatsapp }}</span></div>
                    <div class="modal-field"><span class="modal-field-label">Correo</span><span class="modal-field-val">{{ selectedRequest?.email }}</span></div>
                  </div>
                </div>

                <div class="modal-section">
                  <div class="modal-section-title">Ubicación y hogar</div>
                  <div class="modal-grid modal-grid-3">
                    <div class="modal-field modal-field--full"><span class="modal-field-label">Ubicación</span><span class="modal-field-val">{{ selectedRequest?.direccion }}</span></div>
                    <div class="modal-field"><span class="modal-field-label">Personas del hogar</span><span class="modal-field-val">{{ selectedRequest?.hogar }}</span></div>
                    <div class="modal-field"><span class="modal-field-label">Otras mascotas</span><span class="modal-field-val">{{ selectedRequest?.otrasMascotas }}</span></div>
                    <div class="modal-field"><span class="modal-field-label">Horas sola</span><span class="modal-field-val">{{ selectedRequest?.horasSola }}</span></div>
                  </div>
                  <div class="modal-field modal-field-highlight" style="margin-top:10px">
                    <span class="modal-field-label">Rutina diaria</span>
                    <span class="modal-field-val modal-field-quote">{{ selectedRequest?.rutina }}</span>
                  </div>
                </div>

                <div class="modal-section">
                  <div class="modal-section-title">Estado de la solicitud</div>
                  <div class="modal-grid modal-grid-3">
                    <div class="modal-field"><span class="modal-field-label">Mascota</span><span class="modal-field-val">{{ selectedRequest?.mascota }}</span></div>
                    <div class="modal-field"><span class="modal-field-label">Fecha recibida</span><span class="modal-field-val">{{ selectedRequest?.fecha }}</span></div>
                    <div class="modal-field"><span class="modal-field-label">Estado</span><span class="estado-badge" :class="statusClass(selectedRequest?.estado)" style="margin-top:2px;display:inline-block">{{ selectedRequest?.estado }}</span></div>
                  </div>
                </div>

                <div class="modal-section">
                  <div class="modal-section-title">Motivos de adopción</div>
                  <div class="modal-field modal-field-highlight">
                    <span class="modal-field-label">¿Por qué desea adoptar esta mascota?</span>
                    <span class="modal-field-val modal-field-quote">{{ selectedRequest?.porqueMascota }}</span>
                  </div>
                  <div class="modal-field modal-field-highlight" style="margin-top:10px">
                    <span class="modal-field-label">Motivos de adopción</span>
                    <span class="modal-field-val modal-field-quote">{{ selectedRequest?.motivos }}</span>
                  </div>
                </div>

              </div>
            </div>

            <div class="modal-actions">
              <button type="button" class="btn-ghost" @click="showDetailModal = false">Cerrar expediente</button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

  </div>
</template>

<style scoped>
/* ── Variables (idénticas a Mascotas) ─────────────────────────────────── */
.view-container {
  --verde:       #3A473C;
  --verde-sec:   #92A894;
  --fondo:       #F7F8F7;
  --blanco:      #FFFFFF;
  --texto:       #2B322C;
  --texto-sec:   #7A827B;
  --texto-ter:   #A2A9A3;
  --borde:       #E9ECE9;
  --borde-suave: #EFF2EF;
  --amarillo:    #F5B942;
  --verde-ok:    #4CAF6A;
  --sombra-sm:   0 1px 2px rgba(58,71,60,.03);
  --sombra-md:   0 2px 4px rgba(58,71,60,.05), 0 14px 32px -14px rgba(58,71,60,.18);
  background:
    radial-gradient(ellipse 800px 420px at 12% 0%, rgba(146,168,148,.07), transparent),
    var(--fondo);
  padding-bottom: 40px;
}

/* ── Encabezado ────────────────────────────────────────────────────────── */
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
  background: linear-gradient(150deg, var(--verde) 0%, #6E8870 100%);
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 10px -3px rgba(58,71,60,.45);
}
.admin-page-title { font-size: 22px; font-weight: 700; color: var(--texto); letter-spacing: -0.4px; line-height: 1.15; margin: 0 0 2px; }
.admin-page-sub   { font-size: 12.5px; color: var(--texto-sec); font-weight: 500; margin: 0; }

/* ── Tarjetas resumen ──────────────────────────────────────────────────── */
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
.pendiente-icon  { background: #FDF6E8; border-color: #F2E1B8; color: #A97A0C; }
.proceso-icon    { background: #EEF1FB; border-color: #CBD5F2; color: #4F73B8; }
.aprobada-icon   { background: #EDF6EF; border-color: #C9E4CE; color: #2E7D45; }
.rechazada-icon  { background: #FBEDEC; border-color: #F1C7C3; color: #B71C1C; }
.total-icon      { background: #F2F3F2; border-color: #DFE2DF; color: #616861; }

.don-value { font-size: 21px; font-weight: 700; color: var(--texto); line-height: 1; letter-spacing: -0.4px; font-variant-numeric: tabular-nums; }
.don-label { font-size: 10.5px; color: var(--texto-ter); font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; margin-top: 7px; }
.don-desc  { font-size: 11px; color: var(--texto-sec); margin-top: 2px; }

/* ── Panel de filtros ──────────────────────────────────────────────────── */
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
.filtro-group--tabs { flex: 0 0 auto; }
.filtro-group--btn  { flex: 0 0 auto; }
.filtro-group--search { flex: 1; min-width: 220px; max-width: 340px; }

.filtro-label {
  font-size: 10.5px;
  font-weight: 700;
  color: var(--texto-ter);
  text-transform: uppercase;
  letter-spacing: 0.6px;
}

/* Tabs */
.tabs-wrap {
  display: flex;
  gap: 3px;
  background: var(--fondo);
  border: 1px solid var(--borde-suave);
  border-radius: 10px;
  padding: 3px;
  flex-wrap: wrap;
}
.tab-btn {
  padding: 7px 13px;
  border-radius: 7px;
  border: none;
  background: transparent;
  color: var(--texto-sec);
  font-size: 12px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.18s;
  white-space: nowrap;
  font-family: inherit;
}
.tab-btn:hover { color: var(--texto); }
.tab-btn.active { background: var(--blanco); color: var(--texto); box-shadow: var(--sombra-sm); border: 1px solid var(--borde); }

/* Inputs */
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
.filtro-input--icon-left { padding-left: 36px; }

.filtro-icon { position: absolute; display: flex; align-items: center; color: var(--texto-sec); }
.filtro-icon--left { left: 12px; }

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

/* ── Estado vacío ──────────────────────────────────────────────────────── */
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

/* ── Tabla ─────────────────────────────────────────────────────────────── */
.table-wrapper {
  background: var(--blanco);
  border-radius: 14px;
  border: 1px solid var(--borde);
  overflow: hidden;
  box-shadow: var(--sombra-sm);
}
.table-scroll        { overflow-x: auto; -webkit-overflow-scrolling: touch; }
.don-table           { width: 100%; border-collapse: collapse; min-width: 700px; }
.don-table thead th  { padding: 12px 16px; text-align: left; color: var(--texto-ter); font-size: 9.5px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.6px; white-space: nowrap; }
.don-table tbody tr  { border-top: 1px solid var(--borde-suave); transition: background 0.15s; }
.don-table tbody tr:hover { background: #FAFBFA; }
.don-table tbody td  { padding: 12px 16px; vertical-align: middle; }

.id-pill    { font-size: 11px; font-family: ui-monospace, Menlo, Consolas, monospace; background: var(--fondo); border: 1px solid var(--borde); padding: 3px 9px; border-radius: 6px; color: var(--texto); font-weight: 700; white-space: nowrap; }
.donor-name { display: block; font-size: 12.5px; font-weight: 700; color: var(--texto); line-height: 1.3; }
.fecha-text { font-size: 12.5px; color: var(--texto-sec); white-space: nowrap; }

.pet-chip {
  font-size: 11.5px;
  font-weight: 600;
  color: #4E6E51;
  background: #F1F5F1;
  padding: 3px 10px;
  border-radius: 7px;
  white-space: nowrap;
}

/* Badges */
.estado-badge    { display: inline-block; font-size: 10.5px; font-weight: 700; padding: 4px 11px; border-radius: 20px; white-space: nowrap; }
.badge-pendiente { background: #FDF6E8; color: #96650A; }
.badge-proceso   { background: #EEF1FB; color: #4F73B8; }
.badge-aprobada  { background: #EDF6EF; color: #2E7D32; }
.badge-rechazada { background: #FBEDEC; color: #B71C1C; }
.badge-neutral   { background: #F2F3F2; color: #7A827B; }

/* Acciones */
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
.btn-revisar  { background: #EEF1FB; border-color: #CBD5F2; color: #4F73B8; }
.btn-revisar:hover  { background: #DDE4FA; }
.btn-aprobar  { background: #EDF6EF; border-color: #C9E4CE; color: #2E7D32; }
.btn-aprobar:hover  { background: #D9EEDC; }
.btn-rechazar { background: #FBEDEC; border-color: #F1C7C3; color: #B71C1C; }
.btn-rechazar:hover { background: #F7D6D2; }

/* Botón icon-only "Ver detalle" — mismo componente que en el módulo Mascotas */
.icon-only {
  width: 38px; height: 38px; border-radius: 8px; border: 1px solid var(--borde);
  background: var(--blanco); display: flex; align-items: center; justify-content: center;
  cursor: pointer; transition: background-color .16s ease, border-color .16s ease; position: relative;
  flex-shrink: 0;
}
.icon-only img { width: 16px; height: 16px; object-fit: contain; }
.icon-only--ver { }
.icon-only--ver:hover { border-color: #C7D3C8; background: #FAFCFA; }
.icon-only::before {
  content: attr(data-tooltip); position: absolute; bottom: calc(100% + 8px); left: 50%;
  transform: translateX(-50%) translateY(4px); background: var(--verde); color: #fff;
  font-size: 11px; font-weight: 600; padding: 5px 9px; border-radius: 7px; white-space: nowrap;
  opacity: 0; visibility: hidden; pointer-events: none; transition: opacity .15s ease, transform .15s ease; z-index: 20;
}
.icon-only:hover::before { opacity: 1; visibility: visible; transform: translateX(-50%) translateY(0); }

.table-footer { padding: 12px 16px; border-top: 1px solid var(--borde-suave); font-size: 12px; color: var(--texto-sec); font-weight: 500; }

/* ══════════════════════════════════════════════
   MODAL — mismo lenguaje de diseño que el
   expediente "Ver mascota": hero + bloques + 880×660
   ══════════════════════════════════════════════ */
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
  border-radius: 22px;
  box-shadow: var(--sombra-md);
  position: relative;
}
.modal-box--uniform {
  width: 880px;
  max-width: 92vw;
  height: 660px;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}
.uniform-scroll { flex: 1; min-height: 0; overflow-y: auto; }

.modal-close {
  position: absolute; top: 18px; right: 18px; z-index: 6;
  width: 34px; height: 34px; border-radius: 9px; border: 1.5px solid #EEF3EE; background: transparent;
  color: #9AA89C; display: flex; align-items: center; justify-content: center; cursor: pointer;
  transition: all .15s;
}
.modal-close svg { width: 18px; height: 18px; }
.modal-close:hover { background: #F0F4F0; color: var(--verde); }

/* Cabecera del expediente — mismo patrón que Rescates / Ver mascota */
.modal-head {
  flex-shrink: 0;
  display: flex; align-items: flex-start; gap: 16px;
  padding: 26px 36px 20px;
  border-bottom: 1.5px solid #F0F4F0;
}
.modal-head-left { flex: 1; min-width: 0; }
.modal-head-avatar {
  width: 56px; height: 56px; min-width: 56px; border-radius: 14px; background: #F1F5F1;
  color: #4E6E51; font-size: 20px; font-weight: 800; overflow: hidden; text-transform: uppercase;
  display: flex; align-items: center; justify-content: center;
}
.modal-meta { display: flex; gap: 8px; align-items: center; flex-wrap: wrap; margin-bottom: 10px; }
.modal-title-large { font-size: 22px; font-weight: 800; color: var(--verde); margin: 0; letter-spacing: -0.3px; overflow-wrap: break-word; }

/* Cuerpo por secciones */
.modal-sections { display: flex; flex-direction: column; gap: 22px; padding: 22px 36px 8px; }
.modal-section  { display: flex; flex-direction: column; gap: 12px; }
.modal-section-title {
  font-size: 11px; font-weight: 800; color: #9AA89C; text-transform: uppercase; letter-spacing: .7px;
  display: flex; align-items: center; gap: 8px;
}
.modal-section-title::before { content: ''; display: block; width: 3px; height: 14px; background: #92A894; border-radius: 2px; }

.modal-grid   { display: grid; gap: 10px; }
.modal-grid-3 { grid-template-columns: repeat(3, 1fr); }
.modal-field  {
  background: #F7FAF7; border-radius: 12px; padding: 12px 14px; border: 1px solid #EEF3EE;
  display: flex; flex-direction: column; gap: 4px; min-width: 0;
}
.modal-field--full { grid-column: 1 / -1; }
.modal-field-highlight { background: #FAFCFA; border-left: 3px solid #92A894; }
.modal-field-label { font-size: 11px; font-weight: 700; color: #9AA89C; text-transform: uppercase; letter-spacing: .4px; }
.modal-field-val   { font-size: 14px; font-weight: 600; color: var(--verde); line-height: 1.5; overflow-wrap: break-word; word-break: break-word; }
.modal-field-quote { font-style: italic; font-weight: 500; color: #3A4C3C; }

/* Pie del expediente */
.modal-actions {
  flex-shrink: 0; display: flex; gap: 10px; justify-content: flex-end;
  padding: 16px 36px 20px; border-top: 1px solid var(--borde-suave); margin-top: 0;
}
.btn-ghost {
  padding: 11px 22px; border-radius: 12px; border: 1.5px solid #EEF3EE; background: transparent;
  color: #7A847C; font-size: 14px; font-weight: 700; cursor: pointer; transition: all .2s; font-family: inherit;
}
.btn-ghost:hover { background: #F0F4F0; color: var(--verde); }

.modal-fade-enter-active, .modal-fade-leave-active { transition: opacity 0.22s ease; }
.modal-fade-enter-from, .modal-fade-leave-to       { opacity: 0; }

/* ── Responsive ────────────────────────────────────────────────────────── */
@media (max-width: 1100px) {
  .don-summary { grid-template-columns: repeat(3, 1fr); }
}
@media (max-width: 900px) {
  .don-summary { grid-template-columns: repeat(2, 1fr); }
  .modal-box--uniform { width: 94vw; height: 88vh; }
  .modal-grid-3 { grid-template-columns: repeat(2, 1fr); }
}
@media (max-width: 768px) {
  .don-summary { grid-template-columns: repeat(2, 1fr); gap: 10px; }

  .filtros-panel { padding: 14px; gap: 14px; }
  .filtros-row { flex-direction: column; gap: 12px; }
  .filtro-group { min-width: unset; width: 100%; }
  .filtro-group--search { max-width: none; }

  .tabs-wrap { overflow-x: auto; -webkit-overflow-scrolling: touch; flex-wrap: nowrap; padding-bottom: 4px; }
  .tab-btn { white-space: nowrap; flex-shrink: 0; }

  .btn-limpiar { width: 100%; }

  .table-scroll { overflow-x: auto; -webkit-overflow-scrolling: touch; }

  .modal-box--uniform { width: 96vw; height: 92vh; border-radius: 18px; }
  .modal-head, .modal-sections, .modal-actions { padding-left: 20px; padding-right: 20px; }
  .modal-grid-3 { grid-template-columns: 1fr; }

  .page-header { flex-direction: column; align-items: flex-start; gap: 10px; }

  .action-group { flex-wrap: wrap; }
}
@media (max-width: 480px) {
  .don-summary { grid-template-columns: 1fr; }
}
</style>