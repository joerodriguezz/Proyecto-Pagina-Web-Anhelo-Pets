<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { RouterLink } from 'vue-router'
import { usePetsStore } from '../../stores/usePetsStore'
import { useRescuesStore } from '../../stores/useRescuesStore'
import { getAdoptionRequests, mapAdoptionRequestDtoToRow } from '../../services/adoptionServices'

// ─── Stores ───────────────────────────────────────────────────
const petsStore   = usePetsStore()
const rescuesStore = useRescuesStore()

// ─── localStorage (usuarios, donaciones aún en LS) ─
const usuarios    = ref([])
const donaciones  = ref([])

// ─── Solicitudes de adopción (API real) ─────────────────────────
const solicitudesAdopcion = ref([])

async function cargarSolicitudesAdopcion() {
  try {
    const { data } = await getAdoptionRequests()
    solicitudesAdopcion.value = (data || []).map(mapAdoptionRequestDtoToRow)
  } catch {
    solicitudesAdopcion.value = []
  }
}

function cargar() {
  try { usuarios.value    = JSON.parse(localStorage.getItem('anhelo_usuarios')    || '[]') } catch { usuarios.value    = [] }
  try { donaciones.value  = JSON.parse(localStorage.getItem('anhelo_donaciones')  || '[]') } catch { donaciones.value  = [] }
}

function onStorage(e) {
  const claves = ['anhelo_usuarios', 'anhelo_donaciones']
  if (claves.includes(e.key)) cargar()
}

onMounted(() => {
  cargar()
  cargarSolicitudesAdopcion()
  rescuesStore.fetchRescues()
  window.addEventListener('storage', onStorage)
})
onUnmounted(() => window.removeEventListener('storage', onStorage))

// ─── KPI 1 · Mascotas registradas ────────────────────────────
const totalMascotas = computed(() => petsStore.pets.length)

// ─── KPI 2 · Adopciones activas ──────────────────────────────
const totalAdopciones = computed(() =>
  solicitudesAdopcion.value.filter(s => s.estado !== 'Rechazada').length
)

// ─── KPI 3 · Rescates activos ─────────────────────────────────
const totalRescates = computed(() =>
  rescuesStore.rescatesActivos.value.length
)

// ─── KPI 4 · Usuarios registrados ────────────────────────────
const totalUsuarios = computed(() => usuarios.value.length)

// ─── KPI 5 · Donaciones del mes (aprobadas, mes actual) ───────
const ahora     = new Date()
const mesActual = ahora.getMonth()
const añoActual = ahora.getFullYear()

const donacionesMes = computed(() =>
  donaciones.value
    .filter(d => {
      if (d.estado !== 'Aprobada' && d.estado !== 'Completada') return false
      const f = new Date(d.fechaDonacion || d.fechaRegistro)
      return f.getMonth() === mesActual && f.getFullYear() === añoActual
    })
    .reduce((sum, d) => sum + Number(d.monto || 0), 0)
)

// ─── KPI 6 · Voluntarios activos ─────────────────────────────
const totalVoluntarios = computed(() =>
  usuarios.value.filter(u => u.solicitudVoluntario?.estado === 'Aprobada').length
)

// ─── Solicitudes recientes ────────────────────────────────────
const solicitudesRecientes = computed(() =>
  [...solicitudesAdopcion.value]
    .sort((a, b) => new Date(b.fecha || 0) - new Date(a.fecha || 0))
    .slice(0, 5)
)

// ─── Estado de mascotas ───────────────────────────────────────
const mascotasDisponibles = computed(() => petsStore.pets.filter(p => p.status === 'Disponible').length)
const mascotasEnProceso   = computed(() => petsStore.pets.filter(p => p.status === 'En proceso').length)
const mascotasAdoptadas   = computed(() => petsStore.pets.filter(p => p.status === 'Adoptada').length)

function pct(n) {
  const total = totalMascotas.value
  return total ? Math.round((n / total) * 100) : 0
}

const estadoMascotas = computed(() => [
  { label: 'Disponibles', count: mascotasDisponibles.value, pct: pct(mascotasDisponibles.value), color: '#92A894' },
  { label: 'En proceso',  count: mascotasEnProceso.value,   pct: pct(mascotasEnProceso.value),   color: '#F5B942' },
  { label: 'Adoptadas',   count: mascotasAdoptadas.value,   pct: pct(mascotasAdoptadas.value),   color: '#4CAF6A' },
])

// ─── Rescates activos (lista) ─────────────────────────────────
const rescatesActivos = rescuesStore.rescatesActivos

// ─── Helpers ──────────────────────────────────────────────────
function formatMonto(n) {
  return Number(n || 0).toLocaleString('es-CR')
}

function formatFecha(f) {
  if (!f) return '—'
  const d = new Date(f)
  return isNaN(d.getTime()) ? f : d.toLocaleDateString('es-CR', { day: '2-digit', month: 'short', year: 'numeric' })
}

function badgeClass(status) {
  if (!status) return 'badge-pendiente'
  const s = status.toLowerCase()
  if (s === 'aprobada' || s === 'completada') return 'badge-aprobada'
  if (s === 'rechazada')                       return 'badge-rechazada'
  return 'badge-pendiente'
}

const fechaHeader = computed(() =>
  ahora.toLocaleDateString('es-CR', { month: 'long', year: 'numeric' })
)

// ─── SVG donut ────────────────────────────────────────────────
const DONUT_R  = 54
const DONUT_CX = 70
const DONUT_CY = 70
const CIRC     = 2 * Math.PI * DONUT_R

const donutSegments = computed(() => {
  const items = estadoMascotas.value
  const total = totalMascotas.value || 1
  let offset  = 0
  return items.map(s => {
    const dash   = (s.count / total) * CIRC
    const gap    = CIRC - dash
    const seg    = { color: s.color, dash, gap, offset: -offset + CIRC * 0.25 }
    offset += dash
    return seg
  })
})
</script>

<template>
  <div class="view-container">

    <!-- ── ENCABEZADO ── -->
    <header class="page-header">
      <div>
        <h1 class="page-title">Panel de control</h1>
        <p class="page-sub">Resumen general del sistema · Fundación Anhelo Pets</p>
      </div>
      <span class="header-date">
        <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="margin-right:5px;vertical-align:-2px"><rect x="3" y="4" width="18" height="18" rx="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>
        {{ fechaHeader }}
      </span>
    </header>

    <!-- ── KPIs ── -->
    <div class="kpi-grid">

      <div class="kpi-card k1">
        <div class="kpi-icon-wrap k1-icon">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/></svg>
        </div>
        <div class="kpi-body">
          <span class="kpi-label">Mascotas registradas</span>
          <div class="kpi-value">{{ totalMascotas }}</div>
          <span class="kpi-sub">Total en el sistema</span>
        </div>
      </div>

      <div class="kpi-card k2">
        <div class="kpi-icon-wrap k2-icon">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/></svg>
        </div>
        <div class="kpi-body">
          <span class="kpi-label">Adopciones activas</span>
          <div class="kpi-value">{{ totalAdopciones }}</div>
          <span class="kpi-sub">En proceso</span>
        </div>
      </div>

      <div class="kpi-card k3">
        <div class="kpi-icon-wrap k3-icon">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>
        </div>
        <div class="kpi-body">
          <span class="kpi-label">Rescates activos</span>
          <div class="kpi-value">{{ totalRescates }}</div>
          <span class="kpi-sub">En progreso</span>
        </div>
      </div>

      <div class="kpi-card k4">
        <div class="kpi-icon-wrap k4-icon">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
        </div>
        <div class="kpi-body">
          <span class="kpi-label">Usuarios registrados</span>
          <div class="kpi-value">{{ totalUsuarios }}</div>
          <span class="kpi-sub">Total usuarios</span>
        </div>
      </div>

      <div class="kpi-card k5">
        <div class="kpi-icon-wrap k5-icon">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="1" x2="12" y2="23"/><path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/></svg>
        </div>
        <div class="kpi-body">
          <span class="kpi-label">Donaciones del mes</span>
          <div class="kpi-value kpi-value--money">₡ {{ formatMonto(donacionesMes) }}</div>
          <span class="kpi-sub">Total recaudado</span>
        </div>
      </div>

      <div class="kpi-card k6">
        <div class="kpi-icon-wrap k6-icon">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="8" r="4"/><path d="M6 20v-2a6 6 0 0 1 12 0v2"/><polyline points="16 11 18 13 22 9"/></svg>
        </div>
        <div class="kpi-body">
          <span class="kpi-label">Voluntarios activos</span>
          <div class="kpi-value">{{ totalVoluntarios }}</div>
          <span class="kpi-sub">Colaboradores</span>
        </div>
      </div>

    </div>

    <!-- ── CUERPO PRINCIPAL ── -->
    <div class="dash-grid">

      <!-- COLUMNA IZQUIERDA -->
      <div class="col-left">

        <!-- SOLICITUDES RECIENTES -->
        <div class="dash-card">
          <div class="card-head">
            <div class="card-head-left">
              <h3 class="card-title">Solicitudes recientes</h3>
              <p class="card-sub">Últimas solicitudes de adopción recibidas</p>
            </div>
            <RouterLink to="/admin/adopciones" class="btn-ver-todas">Ver todas →</RouterLink>
          </div>

          <div v-if="solicitudesRecientes.length === 0" class="empty-state">
            <div class="empty-icon">📋</div>
            <p class="empty-title">Sin solicitudes aún</p>
            <p class="empty-sub">Las solicitudes de adopción aparecerán aquí.</p>
          </div>

          <template v-else>
            <div class="table-scroll">
              <table class="data-table">
                <thead>
                  <tr>
                    <th>Solicitante</th>
                    <th>Mascota</th>
                    <th>Fecha</th>
                    <th>Estado</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="r in solicitudesRecientes" :key="r.id">
                    <td><span class="cell-name">{{ r.solicitante || '—' }}</span></td>
                    <td><span class="cell-pet">{{ r.mascota || '—' }}</span></td>
                    <td><span class="cell-date">{{ formatFecha(r.fecha) }}</span></td>
                    <td><span class="estado-badge" :class="badgeClass(r.estado)">{{ r.estado || 'Pendiente' }}</span></td>
                  </tr>
                </tbody>
              </table>
            </div>
            <div class="table-foot">
              {{ solicitudesRecientes.length }} solicitud{{ solicitudesRecientes.length !== 1 ? 'es' : '' }} más reciente{{ solicitudesRecientes.length !== 1 ? 's' : '' }} · de nueva a antigua
            </div>
          </template>
        </div>

        <!-- ESTADO DE MASCOTAS -->
        <div class="dash-card">
          <div class="card-head">
            <div class="card-head-left">
              <h3 class="card-title">Estado de mascotas</h3>
              <p class="card-sub">Distribución por estado actual</p>
            </div>
            <RouterLink to="/admin/mascotas" class="btn-ver-todas">Gestionar →</RouterLink>
          </div>

          <div v-if="totalMascotas === 0" class="empty-state">
            <div class="empty-icon">🐾</div>
            <p class="empty-title">Sin mascotas registradas</p>
            <p class="empty-sub">Registra la primera mascota para ver las estadísticas.</p>
          </div>

          <div v-else class="status-body">
            <!-- Donut + leyenda -->
            <div class="status-layout">
              <div class="donut-wrap">
                <svg width="140" height="140" viewBox="0 0 140 140">
                  <!-- Fondo -->
                  <circle cx="70" cy="70" r="54" fill="none" stroke="#E8ECE8" stroke-width="14"/>
                  <!-- Segmentos -->
                  <circle
                    v-for="(seg, i) in donutSegments"
                    :key="i"
                    cx="70" cy="70" r="54"
                    fill="none"
                    :stroke="seg.color"
                    stroke-width="14"
                    :stroke-dasharray="`${seg.dash} ${seg.gap}`"
                    :stroke-dashoffset="seg.offset"
                    stroke-linecap="round"
                  />
                  <!-- Centro -->
                  <text x="70" y="65" text-anchor="middle" font-size="22" font-weight="800" fill="#3A473C">{{ totalMascotas }}</text>
                  <text x="70" y="81" text-anchor="middle" font-size="9" font-weight="600" fill="#6C756D" letter-spacing="0.5">TOTAL</text>
                </svg>
              </div>

              <div class="status-bars">
                <div v-for="s in estadoMascotas" :key="s.label" class="sb-row">
                  <div class="sb-top">
                    <div class="sb-label-wrap">
                      <span class="sb-dot" :style="{ background: s.color }"></span>
                      <span class="sb-label">{{ s.label }}</span>
                    </div>
                    <div class="sb-right">
                      <span class="sb-count">{{ s.count }}</span>
                      <span class="sb-pct">{{ s.pct }}%</span>
                    </div>
                  </div>
                  <div class="sb-track">
                    <div class="sb-fill" :style="{ width: s.pct + '%', background: s.color }"></div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

      </div>

      <!-- COLUMNA DERECHA -->
      <div class="col-right">

        <!-- RESCATES ACTIVOS -->
        <div class="dash-card">
          <div class="card-head">
            <div class="card-head-left">
              <h3 class="card-title">Rescates activos</h3>
              <p class="card-sub">Casos en atención actualmente</p>
            </div>
            <RouterLink to="/admin/rescates" class="btn-ver-todas">Ver todos →</RouterLink>
          </div>

          <div v-if="rescatesActivos.length === 0" class="empty-state">
            <div class="empty-icon">🛡️</div>
            <p class="empty-title">Sin rescates activos</p>
            <p class="empty-sub">Los rescates en curso aparecerán aquí.</p>
          </div>

          <div v-else class="rescue-body">
            <div v-for="r in rescatesActivos" :key="r.id" class="rescue-item">
              <div class="rescue-avatar">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>
              </div>
              <div class="rescue-info">
                <span class="rescue-name">{{ r.mascota || r.nombre || r.nombreMascota || r.pet || '—' }}</span>
                <span class="rescue-loc">
                  <svg width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" style="margin-right:3px;vertical-align:-1px"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"/><circle cx="12" cy="10" r="3"/></svg>
                  {{ r.ubicacion || r.lugar || r.location || '—' }}
                </span>
              </div>
              <span class="estado-badge badge-activo">Activo</span>
            </div>
          </div>
        </div>

        <!-- RESUMEN RÁPIDO -->
        <div class="dash-card">
          <div class="card-head">
            <div class="card-head-left">
              <h3 class="card-title">Resumen rápido</h3>
              <p class="card-sub">Indicadores clave del sistema</p>
            </div>
          </div>
          <div class="summary-body">
            <div class="summary-item">
              <span class="summary-label">Tasa de adopción</span>
              <div class="summary-val-wrap">
                <span class="summary-val">{{ totalMascotas ? Math.round((mascotasAdoptadas / totalMascotas) * 100) : 0 }}%</span>
                <div class="mini-bar-track">
                  <div class="mini-bar-fill" :style="{ width: (totalMascotas ? Math.round((mascotasAdoptadas / totalMascotas) * 100) : 0) + '%', background: '#4CAF6A' }"></div>
                </div>
              </div>
            </div>
            <div class="summary-item">
              <span class="summary-label">Mascotas disponibles</span>
              <div class="summary-val-wrap">
                <span class="summary-val">{{ totalMascotas ? Math.round((mascotasDisponibles / totalMascotas) * 100) : 0 }}%</span>
                <div class="mini-bar-track">
                  <div class="mini-bar-fill" :style="{ width: (totalMascotas ? Math.round((mascotasDisponibles / totalMascotas) * 100) : 0) + '%', background: '#92A894' }"></div>
                </div>
              </div>
            </div>
            <div class="summary-item">
              <span class="summary-label">Solicitudes pendientes</span>
              <div class="summary-val-wrap">
                <span class="summary-val summary-val--num">{{ solicitudesAdopcion.filter(s => s.estado === 'Pendiente').length }}</span>
              </div>
            </div>
            <div class="summary-item">
              <span class="summary-label">Donaciones este mes</span>
              <div class="summary-val-wrap">
                <span class="summary-val summary-val--money">₡ {{ formatMonto(donacionesMes) }}</span>
              </div>
            </div>
          </div>
        </div>

      </div>

    </div>

  </div>
</template>

<style scoped>
.view-container {
  --verde:          #3A473C;
  --verde-sec:      #92A894;
  --fondo:          #F7F8F7;
  --blanco:         #FFFFFF;
  --texto:          #2F352F;
  --texto-sec:      #6C756D;
  --borde:          #E8ECE8;
  --amarillo:       #F5B942;
  --verde-ok:       #4CAF6A;
  --rojo:           #C45252;
  --verde-claro:    #E7EEE7;
  --amarillo-claro: #FFF7E0;
  --rojo-claro:     #FDECEA;
  background: transparent;
}

/* ── Encabezado ── */
.page-header {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  margin-bottom: 28px;
}

.page-title {
  font-size: 28px;
  font-weight: 800;
  color: var(--verde);
  letter-spacing: -0.5px;
  line-height: 1.1;
  margin: 0 0 4px;
}

.page-sub {
  font-size: 14px;
  color: var(--texto-sec);
  font-weight: 500;
  margin: 0;
}

.header-date {
  font-size: 12px;
  color: var(--texto-sec);
  font-weight: 600;
  background: var(--blanco);
  border: 1px solid var(--borde);
  padding: 7px 14px;
  border-radius: 8px;
  white-space: nowrap;
  text-transform: capitalize;
  display: flex;
  align-items: center;
}

/* ── KPIs ── */
.kpi-grid {
  display: grid;
  grid-template-columns: repeat(6, 1fr);
  gap: 14px;
  margin-bottom: 24px;
}

.kpi-card {
  background: var(--blanco);
  border-radius: 14px;
  padding: 18px 16px;
  border: 1px solid var(--borde);
  border-top: 3px solid transparent;
  display: flex;
  flex-direction: row;
  align-items: flex-start;
  gap: 14px;
  box-shadow: 0 1px 4px rgba(0,0,0,.04);
}

.kpi-card.k1 { border-top-color: var(--verde-sec); }
.kpi-card.k2 { border-top-color: var(--amarillo); }
.kpi-card.k3 { border-top-color: var(--verde-sec); }
.kpi-card.k4 { border-top-color: #9CA8A0; }
.kpi-card.k5 { border-top-color: var(--amarillo); }
.kpi-card.k6 { border-top-color: var(--verde-ok); }

.kpi-icon-wrap {
  width: 38px;
  height: 38px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}
.k1-icon { background: rgba(146,168,148,.15); color: var(--verde-sec); }
.k2-icon { background: rgba(245,185,66,.15);  color: #C9870A; }
.k3-icon { background: rgba(146,168,148,.15); color: var(--verde-sec); }
.k4-icon { background: rgba(156,168,160,.15); color: #6C756D; }
.k5-icon { background: rgba(245,185,66,.15);  color: #C9870A; }
.k6-icon { background: rgba(76,175,106,.15);  color: var(--verde-ok); }

.kpi-body {
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 0;
}

.kpi-label {
  font-size: 10px;
  color: var(--texto-sec);
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  line-height: 1.3;
}

.kpi-value {
  font-size: 24px;
  font-weight: 800;
  color: var(--verde);
  line-height: 1.1;
  letter-spacing: -0.5px;
}

.kpi-value--money {
  font-size: 16px;
  letter-spacing: -0.3px;
}

.kpi-sub {
  font-size: 10px;
  color: var(--texto-sec);
  font-weight: 500;
}

/* ── Layout ── */
.dash-grid {
  display: grid;
  grid-template-columns: 1fr 360px;
  gap: 20px;
}

.col-left,
.col-right {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

/* ── Cards ── */
.dash-card {
  background: var(--blanco);
  border-radius: 14px;
  border: 1px solid var(--borde);
  overflow: hidden;
  box-shadow: 0 1px 4px rgba(0,0,0,.04);
}

.card-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 20px 22px 16px;
  border-bottom: 1px solid var(--borde);
}

.card-head-left { display: flex; flex-direction: column; gap: 2px; }

.card-title {
  font-size: 14px;
  font-weight: 800;
  color: var(--verde);
  letter-spacing: -0.2px;
  margin: 0;
}

.card-sub {
  font-size: 12px;
  color: var(--texto-sec);
  font-weight: 500;
  margin: 0;
}

.btn-ver-todas {
  font-size: 12px;
  color: var(--verde-sec);
  font-weight: 700;
  text-decoration: none;
  background: rgba(146,168,148,.1);
  border: 1px solid rgba(146,168,148,.3);
  padding: 6px 14px;
  border-radius: 8px;
  white-space: nowrap;
  transition: all .15s;
}
.btn-ver-todas:hover {
  background: var(--verde);
  color: var(--blanco);
  border-color: var(--verde);
}

/* ── Tabla ── */
.table-scroll { overflow-x: auto; -webkit-overflow-scrolling: touch; }

.data-table {
  width: 100%;
  border-collapse: collapse;
  min-width: 440px;
}

.data-table thead tr { background: var(--verde); }
.data-table thead th {
  padding: 11px 18px;
  text-align: left;
  color: var(--blanco);
  font-size: 10px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.6px;
  white-space: nowrap;
}

.data-table tbody tr { border-bottom: 1px solid var(--borde); transition: background .12s; }
.data-table tbody tr:last-child { border-bottom: none; }
.data-table tbody tr:hover { background: var(--fondo); }
.data-table tbody td { padding: 13px 18px; vertical-align: middle; }

.cell-name { font-size: 13px; font-weight: 700; color: var(--texto); }
.cell-pet  {
  font-size: 12px;
  font-weight: 600;
  color: #4A6E4C;
  background: rgba(146,168,148,.12);
  padding: 2px 9px;
  border-radius: 6px;
}
.cell-date { font-size: 11px; color: var(--texto-sec); white-space: nowrap; }

.table-foot {
  padding: 11px 18px;
  border-top: 1px solid var(--borde);
  font-size: 11px;
  color: var(--texto-sec);
  font-weight: 500;
}

/* ── Badges ── */
.estado-badge {
  display: inline-block;
  font-size: 10px;
  font-weight: 700;
  padding: 3px 10px;
  border-radius: 20px;
  white-space: nowrap;
}

.badge-pendiente { background: var(--amarillo-claro); color: #96650A; }
.badge-aprobada  { background: var(--verde-claro);    color: #2E7D32; }
.badge-rechazada { background: var(--rojo-claro);     color: var(--rojo); }
.badge-activo    { background: var(--verde-claro);    color: var(--verde); }

/* ── Estado mascotas ── */
.status-body { padding: 20px 22px 22px; }

.status-layout {
  display: flex;
  align-items: center;
  gap: 28px;
}

.donut-wrap {
  flex-shrink: 0;
  width: 140px;
  height: 140px;
}

.status-bars {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.sb-row { display: flex; flex-direction: column; gap: 6px; }

.sb-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.sb-label-wrap { display: flex; align-items: center; gap: 7px; }
.sb-dot { width: 8px; height: 8px; border-radius: 50%; flex-shrink: 0; }
.sb-label { font-size: 12px; font-weight: 600; color: var(--texto); }

.sb-right { display: flex; align-items: baseline; gap: 5px; }
.sb-count { font-size: 14px; font-weight: 800; color: var(--verde); }
.sb-pct   { font-size: 10px; color: var(--texto-sec); font-weight: 600; }

.sb-track {
  height: 5px;
  background: var(--fondo);
  border-radius: 99px;
  overflow: hidden;
  border: 1px solid var(--borde);
}

.sb-fill {
  height: 100%;
  border-radius: 99px;
  transition: width 0.6s cubic-bezier(0.4, 0, 0.2, 1);
}

/* ── Rescates ── */
.rescue-body {
  padding: 14px 16px 16px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.rescue-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 14px;
  background: var(--fondo);
  border-radius: 10px;
  border: 1px solid var(--borde);
  transition: border-color .15s;
}
.rescue-item:hover { border-color: var(--verde-sec); }

.rescue-avatar {
  width: 34px;
  height: 34px;
  border-radius: 9px;
  background: var(--verde-claro);
  color: var(--verde);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.rescue-info { display: flex; flex-direction: column; gap: 2px; flex: 1; min-width: 0; }
.rescue-name { font-size: 13px; font-weight: 700; color: var(--verde); }
.rescue-loc  { font-size: 11px; color: var(--texto-sec); display: flex; align-items: center; }

/* ── Resumen rápido ── */
.summary-body {
  padding: 16px 20px 20px;
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.summary-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.summary-label {
  font-size: 12px;
  font-weight: 600;
  color: var(--texto-sec);
  white-space: nowrap;
}

.summary-val-wrap {
  display: flex;
  align-items: center;
  gap: 10px;
  flex: 1;
  justify-content: flex-end;
}

.summary-val {
  font-size: 13px;
  font-weight: 800;
  color: var(--verde);
  white-space: nowrap;
  min-width: 36px;
  text-align: right;
}

.summary-val--num   { font-size: 16px; }
.summary-val--money { font-size: 12px; }

.mini-bar-track {
  width: 80px;
  height: 5px;
  background: var(--fondo);
  border-radius: 99px;
  overflow: hidden;
  border: 1px solid var(--borde);
  flex-shrink: 0;
}

.mini-bar-fill {
  height: 100%;
  border-radius: 99px;
  transition: width 0.5s ease;
}

/* ── Empty state ── */
.empty-state { padding: 36px 20px; text-align: center; }
.empty-icon  { font-size: 32px; margin-bottom: 10px; }
.empty-title { font-size: 13px; font-weight: 700; color: var(--texto); margin: 0 0 4px; }
.empty-sub   { font-size: 12px; color: var(--texto-sec); margin: 0; }

/* ── Responsive ── */
@media (max-width: 1200px) {
  .kpi-grid  { grid-template-columns: repeat(3, 1fr); }
  .dash-grid { grid-template-columns: 1fr; }
  .col-right { flex-direction: row; flex-wrap: wrap; }
  .col-right > * { flex: 1; min-width: 280px; }
}

@media (max-width: 900px) {
  .kpi-grid { grid-template-columns: repeat(2, 1fr); }
  .status-layout { flex-direction: column; align-items: flex-start; }
  .donut-wrap { width: 120px; height: 120px; }
}

@media (max-width: 640px) {
  .kpi-grid { grid-template-columns: 1fr 1fr; gap: 10px; }
  .kpi-card { flex-direction: column; gap: 8px; }
  .page-header { flex-direction: column; align-items: flex-start; gap: 12px; }
  .col-right { flex-direction: column; }
}

@media (max-width: 420px) {
  .kpi-value       { font-size: 20px; }
  .kpi-value--money { font-size: 14px; }
  .kpi-grid { grid-template-columns: 1fr; }
}


/* ── MOBILE RESPONSIVE ── */
@media (max-width: 1024px) {
  .kpi-grid {
    grid-template-columns: repeat(3, 1fr);
  }
}

@media (max-width: 768px) {
  .page-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 8px;
    margin-bottom: 16px;
  }

  .header-date { font-size: 11px; }

  .kpi-grid {
    grid-template-columns: repeat(2, 1fr);
    gap: 10px;
    margin-bottom: 16px;
  }

  .kpi-card {
    flex-direction: column;
    gap: 8px;
    padding: 14px 12px;
  }

  .kpi-value { font-size: 20px; }

  .kpi-value--money { font-size: 14px; }

  .dash-grid {
    grid-template-columns: 1fr;
    gap: 14px;
  }

  .col-right {
    flex-direction: column;
  }

  .table-scroll {
    overflow-x: auto;
    -webkit-overflow-scrolling: touch;
  }

  .data-table { min-width: 440px; }

  .status-layout {
    flex-direction: column;
    align-items: flex-start;
    gap: 16px;
  }

  .status-bars { width: 100%; }

  .card-head {
    flex-wrap: wrap;
    gap: 8px;
    padding: 14px 16px 12px;
  }
}

@media (max-width: 480px) {
  .kpi-grid {
    grid-template-columns: 1fr;
  }
}


</style>