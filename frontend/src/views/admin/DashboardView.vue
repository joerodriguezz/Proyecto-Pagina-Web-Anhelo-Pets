<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { RouterLink } from 'vue-router'
import { usePetsStore } from '../../stores/usePetsStore'
import { useRescuesStore } from '../../stores/useRescuesStore'
import { getAdoptionRequests, mapAdoptionRequestDtoToRow } from '../../services/adoptionServices'

// ─── Stores ───────────────────────────────────────────────────
const petsStore    = usePetsStore()
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

// ─── Selector de periodo (encabezado) ─────────────────────────
// Componente puramente de navegación/visualización: reemplaza el
// indicador estático de mes/año. No altera el cálculo de ningún KPI.
const meses = [
  'Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
  'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'
]

const ahora     = new Date()
const mesActual = ahora.getMonth()
const añoActual = ahora.getFullYear()

const selectedMonthIndex = ref(mesActual)
const selectedYear       = ref(añoActual)
const showPeriodPanel    = ref(false)
const periodSelectorRef  = ref(null)

const periodoLabel = computed(() => `${meses[selectedMonthIndex.value]} ${selectedYear.value}`)

function prevMonth() {
  if (selectedMonthIndex.value === 0) {
    selectedMonthIndex.value = 11
    selectedYear.value -= 1
  } else {
    selectedMonthIndex.value -= 1
  }
}

function nextMonth() {
  if (selectedMonthIndex.value === 11) {
    selectedMonthIndex.value = 0
    selectedYear.value += 1
  } else {
    selectedMonthIndex.value += 1
  }
}

function prevYear() { selectedYear.value -= 1 }
function nextYear() { selectedYear.value += 1 }

function selectMonth(i) {
  selectedMonthIndex.value = i
  showPeriodPanel.value = false
}

function togglePeriodPanel() {
  showPeriodPanel.value = !showPeriodPanel.value
}

function handleClickOutsidePeriod(e) {
  if (periodSelectorRef.value && !periodSelectorRef.value.contains(e.target)) {
    showPeriodPanel.value = false
  }
}

onMounted(() => {
  cargar()
  cargarSolicitudesAdopcion()
  rescuesStore.fetchRescues()
  window.addEventListener('storage', onStorage)
  document.addEventListener('click', handleClickOutsidePeriod)
})
onUnmounted(() => {
  window.removeEventListener('storage', onStorage)
  document.removeEventListener('click', handleClickOutsidePeriod)
})

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

// Iniciales para el avatar del solicitante en la tabla.
function initials(name) {
  if (!name) return '—'
  const partes = name.trim().split(/\s+/)
  const a = partes[0]?.[0] || ''
  const b = partes[1]?.[0] || ''
  return (a + b).toUpperCase()
}

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
      <div class="brand-row">
        <div class="brand-mark">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/></svg>
        </div>
        <div>
          <h1 class="page-title">Panel de control</h1>
          <p class="page-sub">Fundación Anhelo Pets &middot; resumen general del sistema</p>
        </div>
      </div>

      <div class="period-selector" ref="periodSelectorRef">
        <div class="period-bar">
          <button type="button" class="period-chip period-arrow" aria-label="Mes anterior" @click="prevMonth">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.3" stroke-linecap="round" stroke-linejoin="round"><polyline points="15 18 9 12 15 6"/></svg>
          </button>
          <button type="button" class="period-chip period-label" @click="togglePeriodPanel">
            <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="4" width="18" height="18" rx="3"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>
            <span>{{ periodoLabel }}</span>
            <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" class="period-chevron" :class="{ open: showPeriodPanel }"><polyline points="6 9 12 15 18 9"/></svg>
          </button>
          <button type="button" class="period-chip period-arrow" aria-label="Mes siguiente" @click="nextMonth">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.3" stroke-linecap="round" stroke-linejoin="round"><polyline points="9 18 15 12 9 6"/></svg>
          </button>
        </div>

        <Transition name="panel-fade">
          <div v-if="showPeriodPanel" class="period-panel">
            <div class="period-panel-head">
              <button type="button" class="year-nav" aria-label="Año anterior" @click="prevYear">
                <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.3" stroke-linecap="round" stroke-linejoin="round"><polyline points="15 18 9 12 15 6"/></svg>
              </button>
              <span class="year-label">{{ selectedYear }}</span>
              <button type="button" class="year-nav" aria-label="Año siguiente" @click="nextYear">
                <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.3" stroke-linecap="round" stroke-linejoin="round"><polyline points="9 18 15 12 9 6"/></svg>
              </button>
            </div>
            <div class="month-grid">
              <button
                v-for="(m, i) in meses"
                :key="m"
                type="button"
                class="month-btn"
                :class="{ active: i === selectedMonthIndex }"
                @click="selectMonth(i)"
              >{{ m.slice(0, 3) }}</button>
            </div>
          </div>
        </Transition>
      </div>
    </header>

    <!-- ── KPIs ── -->
    <div class="kpi-grid">

      <div class="kpi-card k1">
        <div class="kpi-icon k1-icon">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/></svg>
        </div>
        <div class="kpi-value">{{ totalMascotas }}</div>
        <div class="kpi-label">Mascotas registradas</div>
      </div>

      <div class="kpi-card k2">
        <div class="kpi-icon k2-icon">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/></svg>
        </div>
        <div class="kpi-value">{{ totalAdopciones }}</div>
        <div class="kpi-label">Adopciones activas</div>
      </div>

      <div class="kpi-card k3">
        <div class="kpi-icon k3-icon">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>
        </div>
        <div class="kpi-value">{{ totalRescates }}</div>
        <div class="kpi-label">Rescates activos</div>
      </div>

      <div class="kpi-card k4">
        <div class="kpi-icon k4-icon">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
        </div>
        <div class="kpi-value">{{ totalUsuarios }}</div>
        <div class="kpi-label">Usuarios registrados</div>
      </div>

      <div class="kpi-card k5">
        <div class="kpi-icon k5-icon">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="1" x2="12" y2="23"/><path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/></svg>
        </div>
        <div class="kpi-value kpi-value--money">₡ {{ formatMonto(donacionesMes) }}</div>
        <div class="kpi-label">Donaciones del mes</div>
      </div>

      <div class="kpi-card k6">
        <div class="kpi-icon k6-icon">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="8" r="4"/><path d="M6 20v-2a6 6 0 0 1 12 0v2"/><polyline points="16 11 18 13 22 9"/></svg>
        </div>
        <div class="kpi-value">{{ totalVoluntarios }}</div>
        <div class="kpi-label">Voluntarios activos</div>
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
            <div class="empty-icon">
              <svg width="26" height="26" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round"><path d="M9 11l3 3L22 4"/><path d="M21 12v7a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11"/></svg>
            </div>
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
                    <td>
                      <div class="applicant-cell">
                        <span class="applicant-avatar">{{ initials(r.solicitante) }}</span>
                        <span class="cell-name">{{ r.solicitante || '—' }}</span>
                      </div>
                    </td>
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
            <div class="empty-icon">
              <svg width="26" height="26" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/></svg>
            </div>
            <p class="empty-title">Sin mascotas registradas</p>
            <p class="empty-sub">Registra la primera mascota para ver las estadísticas.</p>
          </div>

          <div v-else class="status-body">
            <!-- Donut + leyenda -->
            <div class="status-layout">
              <div class="donut-wrap">
                <svg width="140" height="140" viewBox="0 0 140 140">
                  <!-- Fondo -->
                  <circle cx="70" cy="70" r="54" fill="none" stroke="#F0F2F0" stroke-width="12"/>
                  <!-- Segmentos -->
                  <circle
                    v-for="(seg, i) in donutSegments"
                    :key="i"
                    cx="70" cy="70" r="54"
                    fill="none"
                    :stroke="seg.color"
                    stroke-width="12"
                    :stroke-dasharray="`${seg.dash} ${seg.gap}`"
                    :stroke-dashoffset="seg.offset"
                    stroke-linecap="round"
                  />
                  <!-- Centro -->
                  <text x="70" y="65" text-anchor="middle" font-size="22" font-weight="700" fill="#2B322C" font-variant-numeric="tabular-nums">{{ totalMascotas }}</text>
                  <text x="70" y="81" text-anchor="middle" font-size="8" font-weight="700" fill="#A2A9A3" letter-spacing="0.6">TOTAL</text>
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
            <div class="empty-icon">
              <svg width="26" height="26" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>
            </div>
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
  --texto:          #2B322C;
  --texto-sec:      #7A827B;
  --texto-ter:      #A2A9A3;
  --borde:          #E9ECE9;
  --borde-suave:    #EFF2EF;
  --amarillo:       #F5B942;
  --verde-ok:       #4CAF6A;
  --rojo:           #C45252;
  --verde-claro:    #E7EEE7;
  --amarillo-claro: #FFF7E0;
  --rojo-claro:     #FDECEA;
  --sombra-sm:      0 1px 2px rgba(58,71,60,.03);
  --sombra-md:      0 2px 4px rgba(58,71,60,.05), 0 14px 32px -14px rgba(58,71,60,.18);
  background:
    radial-gradient(ellipse 800px 420px at 12% 0%, rgba(146,168,148,.08), transparent),
    var(--fondo);
}

/* ── Encabezado ── */
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 14px;
  flex-wrap: wrap;
  margin-bottom: 24px;
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
  color: var(--blanco);
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 10px -3px rgba(58,71,60,.45);
}

.page-title {
  font-size: 22px;
  font-weight: 700;
  color: var(--texto);
  letter-spacing: -0.4px;
  line-height: 1.15;
  margin: 0 0 2px;
}

.page-sub {
  font-size: 12.5px;
  color: var(--texto-sec);
  font-weight: 500;
  margin: 0;
}

/* ── Selector de periodo ── */
.period-selector { position: relative; }

.period-bar {
  display: flex;
  align-items: center;
  gap: 1px;
  background: var(--blanco);
  border: 1px solid var(--borde);
  border-radius: 11px;
  padding: 3px;
  box-shadow: var(--sombra-sm), 0 8px 18px -12px rgba(58,71,60,.16);
}

.period-chip {
  border: none;
  background: transparent;
  border-radius: 8px;
  cursor: pointer;
  font-family: inherit;
  transition: background .15s;
}
.period-chip:hover { background: #F1F4F1; }

.period-arrow {
  width: 29px;
  height: 29px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--texto-sec);
}

.period-label {
  display: flex;
  align-items: center;
  gap: 8px;
  height: 29px;
  padding: 0 12px;
  font-size: 12.5px;
  font-weight: 700;
  color: var(--texto);
  white-space: nowrap;
  letter-spacing: .1px;
}

.period-label svg:first-child { color: #8FA391; }

.period-chevron { transition: transform .18s ease; }
.period-chevron.open { transform: rotate(180deg); }

.period-panel {
  position: absolute;
  top: 40px;
  right: 0;
  width: 258px;
  background: var(--blanco);
  border: 1px solid var(--borde);
  border-radius: 13px;
  box-shadow: 0 4px 10px rgba(58,71,60,.06), 0 20px 40px -14px rgba(58,71,60,.22);
  padding: 13px;
  z-index: 20;
}

.period-panel-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 10px;
}

.year-nav {
  border: none;
  background: transparent;
  cursor: pointer;
  color: var(--texto-sec);
  width: 24px;
  height: 24px;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background .15s;
}
.year-nav:hover { background: #F1F4F1; }

.year-label {
  font-size: 12.5px;
  font-weight: 700;
  color: var(--texto);
}

.month-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 5px;
}

.month-btn {
  border: 1px solid var(--borde);
  background: #FAFBFA;
  color: var(--texto-sec);
  border-radius: 8px;
  padding: 6px 0;
  font-size: 11px;
  font-weight: 600;
  cursor: pointer;
  font-family: inherit;
  transition: all .15s;
}
.month-btn:hover { border-color: #C9D4CA; }
.month-btn.active {
  border-color: var(--verde);
  background: var(--verde-claro);
  color: var(--texto);
}

.panel-fade-enter-active,
.panel-fade-leave-active { transition: opacity .15s ease, transform .15s ease; }
.panel-fade-enter-from,
.panel-fade-leave-to { opacity: 0; transform: translateY(-4px); }

/* ── KPIs ── */
.kpi-grid {
  display: grid;
  grid-template-columns: repeat(6, 1fr);
  gap: 12px;
  margin-bottom: 20px;
}

.kpi-card {
  background: var(--blanco);
  border-radius: 14px;
  padding: 16px 15px;
  border: 1px solid var(--borde);
  box-shadow: var(--sombra-sm);
  transition: box-shadow .18s ease, border-color .18s ease;
}
.kpi-card:hover {
  border-color: #D7DED8;
  box-shadow: var(--sombra-md);
}

.kpi-icon {
  width: 34px;
  height: 34px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 14px;
  border: 1px solid transparent;
}
.k1-icon { background: #F1F5F1; border-color: #DCE4DC; color: #4E6E51; }
.k2-icon { background: #FDF6E8; border-color: #F2E1B8; color: #A97A0C; }
.k3-icon { background: #F1F5F1; border-color: #DCE4DC; color: #4E6E51; }
.k4-icon { background: #F2F3F2; border-color: #DFE2DF; color: #616861; }
.k5-icon { background: #FDF6E8; border-color: #F2E1B8; color: #A97A0C; }
.k6-icon { background: #EDF6EF; border-color: #C9E4CE; color: #2E7D45; }

.kpi-value {
  font-size: 21px;
  font-weight: 700;
  color: var(--texto);
  line-height: 1;
  letter-spacing: -0.4px;
  font-variant-numeric: tabular-nums;
}

.kpi-value--money {
  font-size: 17px;
  letter-spacing: -0.3px;
  line-height: 1.15;
}

.kpi-label {
  font-size: 10.5px;
  color: var(--texto-ter);
  font-weight: 600;
  letter-spacing: .1px;
  margin-top: 6px;
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
  box-shadow: var(--sombra-sm);
  transition: box-shadow .18s ease, border-color .18s ease;
}
.dash-card:hover {
  border-color: #D7DED8;
  box-shadow: var(--sombra-md);
}

.card-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 17px 19px 14px;
  border-bottom: 1px solid var(--borde-suave);
}

.card-head-left { display: flex; flex-direction: column; gap: 2px; }

.card-title {
  font-size: 13.5px;
  font-weight: 700;
  color: var(--texto);
  letter-spacing: -0.1px;
  margin: 0;
}

.card-sub {
  font-size: 11.5px;
  color: var(--texto-ter);
  font-weight: 500;
  margin: 0;
}

.btn-ver-todas {
  font-size: 11px;
  color: #4E6E51;
  font-weight: 700;
  text-decoration: none;
  background: #F1F5F1;
  border: 1px solid #DCE4DC;
  padding: 6px 12px;
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

.data-table thead th {
  padding: 11px 19px 9px;
  text-align: left;
  color: var(--texto-ter);
  font-size: 9.5px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.6px;
  white-space: nowrap;
}

.data-table tbody tr { border-top: 1px solid var(--borde-suave); transition: background .12s; }
.data-table tbody tr:hover { background: #FAFBFA; }
.data-table tbody td { padding: 11px 19px; vertical-align: middle; }

.applicant-cell { display: flex; align-items: center; gap: 9px; }

.applicant-avatar {
  width: 26px;
  height: 26px;
  min-width: 26px;
  border-radius: 50%;
  background: #F1F5F1;
  color: #4E6E51;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10.5px;
  font-weight: 700;
}

.cell-name { font-size: 12.5px; font-weight: 600; color: var(--texto); }
.cell-pet  {
  font-size: 11.5px;
  font-weight: 600;
  color: #4E6E51;
}
.cell-date { font-size: 11px; color: var(--texto-sec); white-space: nowrap; }

.table-foot {
  padding: 11px 19px;
  border-top: 1px solid var(--borde-suave);
  font-size: 11px;
  color: var(--texto-sec);
  font-weight: 500;
}

/* ── Badges (punto + texto) ── */
.estado-badge {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  font-size: 10.5px;
  font-weight: 700;
  white-space: nowrap;
}

.estado-badge::before {
  content: '';
  width: 6px;
  height: 6px;
  min-width: 6px;
  border-radius: 50%;
  background: currentColor;
}

.badge-pendiente { color: #96650A; }
.badge-aprobada  { color: #2E7D32; }
.badge-rechazada { color: var(--rojo); }
.badge-activo    { color: var(--verde); }

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
.sb-count { font-size: 14px; font-weight: 700; color: var(--texto); font-variant-numeric: tabular-nums; }
.sb-pct   { font-size: 10px; color: var(--texto-ter); font-weight: 600; }

.sb-track {
  height: 4px;
  background: #F0F2F0;
  border-radius: 99px;
  overflow: hidden;
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
  background: #FAFBFA;
  border-radius: 11px;
  border: 1px solid var(--borde-suave);
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
  font-weight: 700;
  color: var(--texto);
  white-space: nowrap;
  min-width: 36px;
  text-align: right;
  font-variant-numeric: tabular-nums;
}

.summary-val--num   { font-size: 16px; }
.summary-val--money { font-size: 12px; }

.mini-bar-track {
  width: 80px;
  height: 4px;
  background: #F0F2F0;
  border-radius: 99px;
  overflow: hidden;
  flex-shrink: 0;
}

.mini-bar-fill {
  height: 100%;
  border-radius: 99px;
  transition: width 0.5s ease;
}

/* ── Empty state ── */
.empty-state { padding: 36px 20px; text-align: center; }
.empty-icon  {
  width: 52px;
  height: 52px;
  margin: 0 auto 10px;
  border-radius: 50%;
  background: #F1F5F1;
  color: #4E6E51;
  display: flex;
  align-items: center;
  justify-content: center;
}
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
  .page-header { flex-direction: column; align-items: flex-start; gap: 12px; }
  .col-right { flex-direction: column; }
}

@media (max-width: 420px) {
  .kpi-value        { font-size: 19px; }
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
    gap: 12px;
    margin-bottom: 16px;
  }

  .period-selector { align-self: stretch; }
  .period-bar { justify-content: space-between; }

  .kpi-grid {
    grid-template-columns: repeat(2, 1fr);
    gap: 10px;
    margin-bottom: 16px;
  }

  .kpi-card { padding: 14px 12px; }

  .kpi-value { font-size: 19px; }

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

  .period-panel { width: 100%; left: 0; right: 0; }
}

</style>