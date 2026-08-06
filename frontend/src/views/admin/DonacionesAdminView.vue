<script setup>
import { ref, computed, onMounted } from 'vue'

import { registrarAuditoria } from '../../composables/useAuditLog'


// ─── Datos desde localStorage ─────────────────────────────────
const todasDonaciones = ref([])
function cargarDonaciones() {
  try {
    const raw = localStorage.getItem('anhelo_donaciones')
    todasDonaciones.value = raw ? JSON.parse(raw) : []
  } catch {
    todasDonaciones.value = []
  }
}
function guardarDonaciones() {
  localStorage.setItem('anhelo_donaciones', JSON.stringify(todasDonaciones.value))
}
onMounted(() => {
  cargarDonaciones()
  window.addEventListener('storage', (e) => {
    if (e.key === 'anhelo_donaciones') cargarDonaciones()
  })
})
const TIPO_CAMBIO = 485
function montoEnCRC(donacion) {
  const monto = Number(donacion.monto || 0)
  if (donacion.moneda === 'USD') {
    return monto * TIPO_CAMBIO
  }
  return monto
}
function montoFiltrado(donacion) {
  const monto = Number(donacion.monto || 0)
  if (filtroMoneda.value === 'CRC') {
    return donacion.moneda === 'CRC' ? monto : 0
  }
  if (filtroMoneda.value === 'USD') {
    return donacion.moneda === 'USD' ? monto : 0
  }
  // Sin filtro → convierte USD a CRC
  return montoEnCRC(donacion)
}
// ─── Estadísticas calculadas ──────────────────────────────────
const ahora = new Date()
const mesActual = ahora.getMonth()
const añoActual = ahora.getFullYear()
const totalMes = computed(() => {
  return todasDonaciones.value
    .filter(d => {
      const f = new Date(d.fechaDonacion || d.fechaRegistro)
      if (d.estado !== 'Aprobada') return false
      if (fechaMes.value !== null) {
        return (
          f.getMonth() === fechaMes.value &&
          f.getFullYear() === fechaAño.value
        )
      }
      return (
        f.getMonth() === mesActual &&
        f.getFullYear() === añoActual
      )
    })
    .reduce((s, d) => s + montoFiltrado(d), 0)
})
const totalAño = computed(() => {
  return todasDonaciones.value
    .filter(d => {
      const f = new Date(d.fechaDonacion || d.fechaRegistro)
      if (d.estado !== 'Aprobada') return false
      if (fechaAño.value !== null) {
        return f.getFullYear() === fechaAño.value
      }
      return f.getFullYear() === añoActual
    })
    .reduce((s, d) => s + montoFiltrado(d), 0)
})
const totalCRC = computed(() => {
  return todasDonaciones.value
    .filter(d => d.estado === 'Aprobada' && d.moneda === 'CRC')
    .reduce((s, d) => s + Number(d.monto || 0), 0)
})
const totalUSD = computed(() => {
  return todasDonaciones.value
    .filter(d => d.estado === 'Aprobada' && d.moneda === 'USD')
    .reduce((s, d) => s + Number(d.monto || 0), 0)
})
const simboloEstadisticas = computed(() => {
  if (filtroMoneda.value === 'USD') {
    return '$'
  }
  return '₡'
})
const totalDonaciones = computed(() => todasDonaciones.value.length)
const totalPendientes = computed(() => todasDonaciones.value.filter(d => d.estado === 'Pendiente').length)
const totalAprobadas  = computed(() => todasDonaciones.value.filter(d => d.estado === 'Aprobada').length)
// ─── Filtros ──────────────────────────────────────────────────
const filtroEstado = ref('')
const filtroMetodo = ref('')
const filtroNombre = ref('')
const filtroMoneda = ref('')
// Fecha: manejada por el calendario custom
const MESES_LABEL = ['Ene','Feb','Mar','Abr','May','Jun','Jul','Ago','Sep','Oct','Nov','Dic']
const calAbierto    = ref(false)
const calYear       = ref(new Date().getFullYear())
const fechaMes      = ref(null)   // 0-11
const fechaAño      = ref(null)
const fechaLabel = computed(() => {
  if (fechaMes.value === null) return null
  return MESES_LABEL[fechaMes.value] + ' ' + fechaAño.value
})
function toggleCal() { calAbierto.value = !calAbierto.value }
function closeCal()  { calAbierto.value = false }
function seleccionarMes(i) {
  fechaMes.value  = i
  fechaAño.value  = calYear.value
  calAbierto.value = false
}
// Filtro de fecha: comparar con fechaDonacion que empieza con "YYYY-MM"
const filtroFechaStr = computed(() => {
  if (fechaMes.value === null) return ''
  const mm = String(fechaMes.value + 1).padStart(2, '0')
  return `${fechaAño.value}-${mm}`
})
const donacionesFiltradas = computed(() => {
  let lista = [...todasDonaciones.value]
  if (filtroNombre.value.trim()) {
    const q = filtroNombre.value.toLowerCase()
    lista = lista.filter(d => d.nombre?.toLowerCase().includes(q))
  }
  if (filtroEstado.value) lista = lista.filter(d => d.estado === filtroEstado.value)
  if (filtroMetodo.value) lista = lista.filter(d => d.metodo === filtroMetodo.value)
  if (filtroMoneda.value) lista = lista.filter(d => d.moneda === filtroMoneda.value)
  if (filtroFechaStr.value) lista = lista.filter(d => d.fechaDonacion?.startsWith(filtroFechaStr.value))
  lista.sort((a, b) => new Date(b.fechaRegistro || 0) - new Date(a.fechaRegistro || 0))
  return lista
})
const hayFiltros = computed(() =>
  filtroNombre.value || filtroEstado.value || filtroMetodo.value ||
  filtroMoneda.value || fechaMes.value !== null
)
function limpiarFiltros() {
  filtroNombre.value = ''
  filtroEstado.value = ''
  filtroMetodo.value = ''
  filtroMoneda.value = ''
  fechaMes.value  = null
  fechaAño.value  = null
  calYear.value   = new Date().getFullYear()
}
// ─── Modal de detalle ─────────────────────────────────────────
const modalAbierto   = ref(false)
const donacionActual = ref(null)
const imagenAmpliada = ref(false)
function abrirModal(d) {
  donacionActual.value = { ...d }
  modalAbierto.value   = true
  imagenAmpliada.value = false
}
function cerrarModal() {
  modalAbierto.value   = false
  donacionActual.value = null
}
// ─── Aprobar / Rechazar ───────────────────────────────────────
function cambiarEstado(nuevoEstado) {
  if (!donacionActual.value) return
  const idx = todasDonaciones.value.findIndex(d => d.id === donacionActual.value.id)
  if (idx === -1) return
  todasDonaciones.value[idx].estado = nuevoEstado
  donacionActual.value.estado = nuevoEstado
  guardarDonaciones()
  registrarAuditoria({
    modulo: 'Donaciones',
    accion: nuevoEstado === 'Aprobada' ? 'Aprobó una donación' : 'Rechazó una donación',
    tipoAccion: nuevoEstado === 'Aprobada' ? 'aprobar' : 'rechazar',
    elemento: donacionActual.value.nombre || 'Anónimo',
    elementoId: donacionActual.value.id,
    descripcion: `Donación de ${simboloMoneda(donacionActual.value.moneda)} ${formatMonto(donacionActual.value.monto)} marcada como ${nuevoEstado}.`,
  })
}
// ─── Helpers de formato ───────────────────────────────────────
function formatMonto(n) {
  return Number(n || 0).toLocaleString('es-CR')
}
function formatFecha(f) {
  if (!f) return '—'
  const d = new Date(f)
  return isNaN(d) ? f : d.toLocaleDateString('es-CR', { day: '2-digit', month: 'short', year: 'numeric' })
}
function esImagen(comprobante) {
  return comprobante?.startsWith('data:image')
}
function esPDF(comprobante) {
  return comprobante?.startsWith('data:application/pdf')
}
function abrirPDF(comprobante) {
  const win = window.open()
  win.document.write(`<iframe src="${comprobante}" style="width:100%;height:100vh;border:none;"></iframe>`)
}
function estadoClass(estado) {
  if (estado === 'Aprobada')  return 'badge-aprobada'
  if (estado === 'Rechazada') return 'badge-rechazada'
  return 'badge-pendiente'
}
function simboloMoneda(moneda) {
  return moneda === 'USD' ? '$' : '₡'
}
// ─── Iniciales del donante (avatar del expediente) ─────────────
function inicialesDonante(nombre) {
  if (!nombre) return '?'
  return nombre.trim().split(' ').map(p => p[0]).slice(0, 2).join('').toUpperCase()
}
</script>

<template>
  <div class="view-container" @click.self="closeCal">

    <!-- CABECERA -->
    <header class="page-header">
      <div class="brand-row">
        <div class="brand-mark">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="8" width="18" height="4" rx="1"/><path d="M12 8v13"/><path d="M19 12v7a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2v-7"/><path d="M7.5 8a2.5 2.5 0 0 1 0-5C11 3 12 8 12 8"/><path d="M16.5 8a2.5 2.5 0 0 0 0-5C13 3 12 8 12 8"/></svg>
        </div>
        <div>
          <h1 class="admin-page-title">Donaciones</h1>
          <p class="admin-page-sub">Historial y control de donaciones recibidas</p>
        </div>
      </div>
    </header>

    <!-- TARJETAS RESUMEN -->
    <div class="don-summary">
      <div class="don-card">
        <div class="don-icon total-mes-icon">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="4" width="18" height="18" rx="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>
        </div>
        <strong class="don-value">{{ simboloEstadisticas }} {{ formatMonto(totalMes) }}</strong>
        <span class="don-label">Total aprobado este mes</span>
        <span class="don-desc">Ingresos aprobados</span>
      </div>
      <div class="don-card">
        <div class="don-icon total-anio-icon">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="20" x2="12" y2="10"/><line x1="18" y1="20" x2="18" y2="4"/><line x1="6" y1="20" x2="6" y2="16"/></svg>
        </div>
        <!-- CORRECCIÓN: antes mostraba formatMonto(totalMes) — duplicaba
             el valor del mes. Ahora usa totalAño, el computed correcto
             que ya existía pero nunca se estaba usando aquí. -->
        <strong class="don-value">{{ simboloEstadisticas }} {{ formatMonto(totalAño) }}</strong>
        <span class="don-label">Total aprobado este año</span>
        <span class="don-desc">Acumulado del año</span>
      </div>
      <div class="don-card">
        <div class="don-icon total-icon">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="7" height="7" rx="1.5"/><rect x="14" y="3" width="7" height="7" rx="1.5"/><rect x="3" y="14" width="7" height="7" rx="1.5"/><rect x="14" y="14" width="7" height="7" rx="1.5"/></svg>
        </div>
        <strong class="don-value">{{ totalDonaciones }}</strong>
        <span class="don-label">Total donaciones</span>
        <span class="don-desc">En el sistema</span>
      </div>
      <div class="don-card">
        <div class="don-icon pendiente-icon">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="9"/><polyline points="12 7 12 12 16 14"/></svg>
        </div>
        <strong class="don-value">{{ totalPendientes }}</strong>
        <span class="don-label">Pendientes</span>
        <span class="don-desc">Por revisar</span>
      </div>
      <div class="don-card">
        <div class="don-icon aprobada-icon">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/></svg>
        </div>
        <strong class="don-value">{{ totalAprobadas }}</strong>
        <span class="don-label">Aprobadas</span>
        <span class="don-desc">Donaciones confirmadas</span>
      </div>
    </div>

    <!-- FILTROS -->
    <div class="filtros-panel">
      <div class="filtros-row">
        <!-- Estado — tabs, como en Mascotas -->
        <div class="filtro-group filtro-group--tabs">
          <label class="filtro-label">Estado</label>
          <div class="tabs-wrap">
            <button type="button" class="tab-btn" :class="{ active: filtroEstado === '' }" @click="filtroEstado = ''">Todos</button>
            <button type="button" class="tab-btn" :class="{ active: filtroEstado === 'Pendiente' }" @click="filtroEstado = 'Pendiente'">Pendiente</button>
            <button type="button" class="tab-btn" :class="{ active: filtroEstado === 'Aprobada' }" @click="filtroEstado = 'Aprobada'">Aprobada</button>
            <button type="button" class="tab-btn" :class="{ active: filtroEstado === 'Rechazada' }" @click="filtroEstado = 'Rechazada'">Rechazada</button>
          </div>
        </div>

        <!-- Moneda — tabs, como en Mascotas -->
        <div class="filtro-group filtro-group--tabs">
          <label class="filtro-label">Moneda</label>
          <div class="tabs-wrap">
            <button type="button" class="tab-btn" :class="{ active: filtroMoneda === '' }" @click="filtroMoneda = ''">Todas</button>
            <button type="button" class="tab-btn" :class="{ active: filtroMoneda === 'CRC' }" @click="filtroMoneda = 'CRC'">CRC</button>
            <button type="button" class="tab-btn" :class="{ active: filtroMoneda === 'USD' }" @click="filtroMoneda = 'USD'">USD</button>
          </div>
        </div>

        <!-- Método — select (5 valores, no cabe cómodo en tabs) -->
        <div class="filtro-group">
          <label class="filtro-label">Método</label>
          <div class="filtro-input-wrap">
            <select v-model="filtroMetodo" class="filtro-input filtro-select">
              <option value="">Todos</option>
              <option value="PayPal">PayPal</option>
              <option value="SINPE Móvil">SINPE Móvil</option>
              <option value="BCR">BCR</option>
              <option value="Cuenta USD">Cuenta USD</option>
              <option value="Coopealianza">Coopealianza</option>
            </select>
          </div>
        </div>

        <!-- Fecha donación — calendario custom -->
        <div class="filtro-group">
          <label class="filtro-label">Fecha donación</label>
          <div class="filtro-input-wrap cal-wrap">
            <button
              type="button"
              class="filtro-input filtro-date-btn"
              :class="{ 'filtro-date-btn--activa': fechaLabel }"
              @click.stop="toggleCal"
            >
              <span :class="fechaLabel ? '' : 'placeholder-color'">{{ fechaLabel || 'Mes y año' }}</span>
              <span class="filtro-icon-cal">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="4" width="18" height="18" rx="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>
              </span>
            </button>
            <!-- Dropdown calendario -->
            <transition name="cal-drop">
              <div v-if="calAbierto" class="cal-dropdown" @click.stop>
                <div class="cal-nav">
                  <button type="button" class="cal-nav-btn" @click="calYear--">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="15 18 9 12 15 6"/></svg>
                  </button>
                  <span class="cal-year">{{ calYear }}</span>
                  <button type="button" class="cal-nav-btn" @click="calYear++">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="9 18 15 12 9 6"/></svg>
                  </button>
                </div>
                <div class="cal-grid">
                  <button
                    v-for="(mes, i) in MESES_LABEL"
                    :key="i"
                    type="button"
                    class="cal-mes-btn"
                    :class="{ 'cal-mes-btn--sel': fechaMes === i && fechaAño === calYear }"
                    @click="seleccionarMes(i)"
                  >{{ mes }}</button>
                </div>
                <div v-if="fechaLabel" class="cal-clear">
                  <button type="button" class="cal-clear-btn" @click="fechaMes = null; fechaAño = null; calAbierto = false">
                    Quitar fecha
                  </button>
                </div>
              </div>
            </transition>
          </div>
        </div>
      </div>

      <div class="filtros-divider"></div>

      <div class="filtros-row filtros-row--end">
        <!-- Buscar donante -->
        <div class="filtro-group filtro-group--search">
          <label class="filtro-label">Buscar donante</label>
          <div class="filtro-input-wrap">
            <span class="filtro-icon filtro-icon--left">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
            </span>
            <input
              v-model="filtroNombre"
              type="text"
              class="filtro-input filtro-input--icon-left"
              placeholder="Nombre..."
            />
          </div>
        </div>

        <!-- Limpiar — mismo botón exacto que Mascotas -->
        <div class="filtro-group filtro-group--btn">
          <button type="button" class="btn btn--ghost" :class="{ 'btn--ghost-active': hayFiltros }" @click="limpiarFiltros">Limpiar filtros</button>
        </div>
      </div>
    </div>

    <!-- ESTADO VACÍO -->
    <div v-if="donacionesFiltradas.length === 0" class="empty-state">
      <svg xmlns="http://www.w3.org/2000/svg" width="36" height="36" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="8" width="18" height="4" rx="1"/><path d="M12 8v13"/><path d="M19 12v7a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2v-7"/><path d="M7.5 8a2.5 2.5 0 0 1 0-5C11 3 12 8 12 8"/><path d="M16.5 8a2.5 2.5 0 0 0 0-5C13 3 12 8 12 8"/></svg>
      <p class="empty-title">No hay donaciones registradas</p>
      <p class="empty-sub">Ajusta los filtros o espera nuevas donaciones.</p>
    </div>

    <!-- TABLA PRINCIPAL -->
    <div v-else class="table-wrapper">
      <div class="table-scroll">
        <table class="don-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Donante</th>
              <th>Método</th>
              <th>Monto</th>
              <th>Fecha donación</th>
              <th>Estado</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="d in donacionesFiltradas" :key="d.id" class="don-row">
              <td><span class="id-pill">{{ d.id }}</span></td>
              <td>
                <span class="donor-name">{{ d.nombre || 'Anónimo' }}</span>
                <span class="donor-mail">{{ d.correo || '—' }}</span>
              </td>
              <td><span class="metodo-text">{{ d.metodo }}</span></td>
              <td><span class="monto-text">{{ simboloMoneda(d.moneda) }} {{ formatMonto(d.monto) }}</span></td>
              <td><span class="fecha-text">{{ formatFecha(d.fechaDonacion) }}</span></td>
              <td><span class="estado-badge" :class="estadoClass(d.estado)">{{ d.estado }}</span></td>
              <td>
                <div class="action-group">
                  <button type="button" class="icon-only icon-only--ver" @click="abrirModal(d)" data-tooltip="Ver detalle">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div class="table-footer">
        {{ donacionesFiltradas.length }} donación{{ donacionesFiltradas.length !== 1 ? 'es' : '' }} encontrada{{ donacionesFiltradas.length !== 1 ? 's' : '' }}
      </div>
    </div>

    <!-- ═══════════ MODAL DE DETALLE (expediente, mismo sistema que Mascotas) ═══════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="modalAbierto && donacionActual" class="modal-overlay" @click.self="cerrarModal">
          <div class="modal-box modal-box--uniform">
            <button type="button" class="close-btn close-btn--hero" @click="cerrarModal">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>

            <div class="hero">
              <div class="hero-photo">
                <span class="hero-photo-ini">{{ inicialesDonante(donacionActual.nombre) }}</span>
              </div>
              <div class="hero-info">
                <div class="hero-name-row">
                  <h2 class="hero-name">{{ donacionActual.nombre || 'Anónimo' }}</h2>
                  <span class="estado-badge badge-status-hero" :class="estadoClass(donacionActual.estado)">{{ donacionActual.estado }}</span>
                </div>
                <div class="hero-meta">
                  <span class="hero-meta-chip">
                    <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="7" height="7" rx="1.5"/><rect x="14" y="3" width="7" height="7" rx="1.5"/><rect x="14" y="14" width="7" height="7" rx="1.5"/><rect x="3" y="14" width="7" height="7" rx="1.5"/></svg>
                    {{ donacionActual.id }}
                  </span>
                  <span class="hero-meta-chip">{{ donacionActual.correo || '—' }}</span>
                  <span class="hero-meta-chip hero-meta-chip--monto">{{ simboloMoneda(donacionActual.moneda) }} {{ formatMonto(donacionActual.monto) }}</span>
                </div>
              </div>
            </div>

            <div class="uniform-scroll">
              <div class="body">
                <div class="grid-2col">
                  <div>
                    <div class="block">
                      <h4 class="block-title">
                        <span class="block-title-icon">
                          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/></svg>
                        </span>
                        Información personal
                      </h4>
                      <div class="fields-row">
                        <div class="field-col"><span class="field-label-row">Nombre</span><span class="field-value">{{ donacionActual.nombre || '—' }}</span></div>
                        <div class="field-col"><span class="field-label-row">Correo</span><span class="field-value">{{ donacionActual.correo || '—' }}</span></div>
                        <div class="field-col"><span class="field-label-row">Teléfono</span><span class="field-value">{{ donacionActual.telefono || '—' }}</span></div>
                      </div>
                    </div>

                    <div class="block">
                      <h4 class="block-title">
                        <span class="block-title-icon">
                          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="7" width="20" height="14" rx="2"/><path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"/></svg>
                        </span>
                        Información financiera
                      </h4>
                      <div class="fields-row">
                        <div class="field-col"><span class="field-label-row">Método</span><span class="field-value">{{ donacionActual.metodo || '—' }}</span></div>
                        <div class="field-col"><span class="field-label-row">Moneda</span><span class="field-value">{{ donacionActual.moneda || '—' }}</span></div>
                        <div class="field-col"><span class="field-label-row">Monto</span><span class="field-value field-value--highlight">{{ simboloMoneda(donacionActual.moneda) }} {{ formatMonto(donacionActual.monto) }}</span></div>
                        <div class="field-col"><span class="field-label-row">Fecha de donación</span><span class="field-value">{{ formatFecha(donacionActual.fechaDonacion) }}</span></div>
                        <div class="field-col"><span class="field-label-row">Fecha de registro</span><span class="field-value">{{ formatFecha(donacionActual.fechaRegistro) }}</span></div>
                      </div>
                    </div>

                    <div class="block block-wide" v-if="donacionActual.mensaje">
                      <h4 class="block-title">
                        <span class="block-title-icon">
                          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
                        </span>
                        Mensaje
                      </h4>
                      <div class="tint-box tint-box--desc">
                        <span>{{ donacionActual.mensaje }}</span>
                      </div>
                    </div>
                  </div>

                  <div class="block" style="margin-bottom:0;">
                    <h4 class="block-title">
                      <span class="block-title-icon">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="18" height="18" rx="2"/><circle cx="8.5" cy="8.5" r="1.5"/><path d="M21 15l-5-5L5 21"/></svg>
                      </span>
                      Comprobante
                    </h4>
                    <p v-if="!donacionActual.comprobante" class="modal-empty-text">Sin comprobante adjunto</p>
                    <div v-else-if="esImagen(donacionActual.comprobante)" class="comprobante-img-wrap">
                      <img
                        :src="donacionActual.comprobante"
                        class="comprobante-thumb"
                        :class="{ ampliada: imagenAmpliada }"
                        alt="Comprobante"
                        @click="imagenAmpliada = !imagenAmpliada"
                        :title="imagenAmpliada ? 'Clic para reducir' : 'Clic para ampliar'"
                      />
                      <p class="comprobante-hint">{{ imagenAmpliada ? 'Clic para reducir' : 'Clic para ampliar' }}</p>
                    </div>
                    <div v-else-if="esPDF(donacionActual.comprobante)" class="tint-box">
                      <p class="pdf-label">Documento PDF adjunto</p>
                      <button type="button" class="btn-abrir-pdf" @click="abrirPDF(donacionActual.comprobante)">Abrir PDF</button>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <div class="footer">
              <template v-if="donacionActual.estado === 'Pendiente'">
                <button type="button" class="btn-footer-success" @click="cambiarEstado('Aprobada')">Aprobar donación</button>
                <button type="button" class="btn-footer-danger" @click="cambiarEstado('Rechazada')">Rechazar donación</button>
              </template>
              <p v-else-if="donacionActual.estado === 'Aprobada'" class="estado-final-msg estado-final-msg--ok">Esta donación ha sido aprobada.</p>
              <p v-else-if="donacionActual.estado === 'Rechazada'" class="estado-final-msg estado-final-msg--bad">Esta donación ha sido rechazada.</p>
              <button type="button" class="btn-ghost-red" @click="cerrarModal">Cerrar expediente</button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

  </div>
</template>

<style scoped>
/* ── Variables — idénticas al sistema de diseño de Mascotas ── */
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
  --rojo:        #C0392B;
  --rojo-bg:     #FBEDEC;
  --sombra-sm:   0 1px 2px rgba(58,71,60,.03);
  --sombra-md:   0 2px 4px rgba(58,71,60,.05), 0 14px 32px -14px rgba(58,71,60,.18);
  --btn-height:      33px;
  --btn-radius:      9px;
  --btn-pad-x:       13px;
  --btn-icon-size:   14px;
  --btn-icon-gap:    6px;
  --btn-font-size:   12.5px;
  --btn-font-weight: 600;
  --btn-transition:  0.16s ease;
  --select-arrow: url('data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="%237A827B" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>');
  --select-arrow-focus: url('data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="%233A473C" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>');

  background:
    radial-gradient(ellipse 800px 420px at 12% 0%, rgba(146,168,148,.07), transparent),
    var(--fondo);
  padding-bottom: 40px;
}

/* ── Sistema de botones ── */
.btn { display:inline-flex; align-items:center; justify-content:center; gap:var(--btn-icon-gap); height:var(--btn-height); padding:0 var(--btn-pad-x); border-radius:var(--btn-radius); border:1px solid transparent; font-family:inherit; font-size:var(--btn-font-size); font-weight:var(--btn-font-weight); line-height:1; white-space:nowrap; cursor:pointer; user-select:none; transition:background-color var(--btn-transition), border-color var(--btn-transition), color var(--btn-transition), box-shadow var(--btn-transition); }
.btn:active:not(:disabled) { transform:translateY(1px); }
.btn:focus-visible { outline:none; box-shadow:0 0 0 3px rgba(58,71,60,.16); }
.btn--ghost { background:var(--blanco); color:var(--texto-sec); border-color:var(--borde); }
.btn--ghost:hover:not(:disabled) { background:#FAFBFA; color:var(--texto); border-color:#D3D8D3; }
.btn--ghost-active { border-color:var(--verde-sec); color:var(--verde); }
.btn--ghost-active:hover:not(:disabled) { background:#F3F6F3; color:var(--verde); border-color:var(--verde-sec); }

/* ── Encabezado ── */
.page-header { display:flex; justify-content:space-between; align-items:center; margin-bottom:24px; gap:16px; flex-wrap:wrap; }
.brand-row { display:flex; align-items:center; gap:12px; }
.brand-mark { width:38px; height:38px; min-width:38px; border-radius:11px; background:linear-gradient(150deg, var(--verde) 0%, #6E8870 100%); color:#fff; display:flex; align-items:center; justify-content:center; box-shadow:0 4px 10px -3px rgba(58,71,60,.45); }
.admin-page-title { font-size:22px; font-weight:700; color:var(--texto); letter-spacing:-0.4px; line-height:1.15; margin:0 0 2px; }
.admin-page-sub { font-size:12.5px; color:var(--texto-sec); font-weight:500; margin:0; }

/* ── Tarjetas resumen ── */
.don-summary { display:grid; grid-template-columns:repeat(5, 1fr); gap:12px; margin-bottom:20px; }
.don-card { background:var(--blanco); border-radius:16px; padding:16px 15px; border:1px solid var(--borde); box-shadow:var(--sombra-sm); display:flex; flex-direction:column; transition:box-shadow .18s ease, border-color .18s ease; }
.don-card:hover { border-color:#D7DED8; box-shadow:var(--sombra-md); }
.don-icon { width:32px; height:32px; border-radius:50%; display:flex; align-items:center; justify-content:center; margin-bottom:12px; border:1px solid transparent; }
.total-mes-icon  { background:#EAF2F6; border-color:#C7DCE6; color:#3C6E85; }
.total-anio-icon { background:#EDF6EF; border-color:#C9E4CE; color:#2E7D45; }
.total-icon      { background:#F2F3F2; border-color:#DFE2DF; color:#616861; }
.pendiente-icon  { background:#FDF6E8; border-color:#F2E1B8; color:#A97A0C; }
.aprobada-icon   { background:#EDF6EF; border-color:#C9E4CE; color:#2E7D45; }
.don-value { font-size:21px; font-weight:700; color:var(--texto); line-height:1; letter-spacing:-0.4px; font-variant-numeric:tabular-nums; }
.don-label { font-size:10.5px; color:var(--texto-ter); font-weight:700; text-transform:uppercase; letter-spacing:0.5px; margin-top:7px; }
.don-desc { font-size:11px; color:var(--texto-sec); margin-top:2px; }

/* ── Panel de filtros ── */
.filtros-panel { background:var(--blanco); border-radius:16px; padding:18px 20px; margin-bottom:20px; border:1px solid var(--borde); box-shadow:var(--sombra-sm); display:flex; flex-direction:column; gap:16px; }
.filtros-row { display:flex; gap:24px; flex-wrap:wrap; }
.filtros-row--end { align-items:flex-end; justify-content:space-between; }
.filtros-divider { height:1px; background:var(--borde-suave); }
.filtro-group { display:flex; flex-direction:column; gap:7px; }
.filtro-group--tabs { flex:0 0 auto; }
.filtro-group--btn { flex:0 0 auto; }
.filtro-group--search { flex:1; min-width:220px; max-width:340px; }
.filtro-label { font-size:10.5px; font-weight:700; color:var(--texto-ter); text-transform:uppercase; letter-spacing:0.6px; }
.tabs-wrap { display:flex; gap:3px; background:var(--fondo); border:1px solid var(--borde-suave); border-radius:10px; padding:3px; }
.tab-btn { padding:7px 13px; border-radius:7px; border:none; background:transparent; color:var(--texto-sec); font-size:12px; font-weight:700; cursor:pointer; transition:all 0.18s; white-space:nowrap; font-family:inherit; }
.tab-btn:hover { color:var(--texto); }
.tab-btn.active { background:var(--blanco); color:var(--texto); box-shadow:var(--sombra-sm); border:1px solid var(--borde); }
.filtro-input-wrap { position:relative; display:flex; align-items:center; }
.filtro-input { width:100%; height:36px; padding:0 14px; border-radius:8px; border:1px solid var(--borde); background:var(--fondo); font-size:13px; color:var(--texto); font-family:inherit; outline:none; transition:border-color 0.18s, background 0.18s; box-sizing:border-box; }
.filtro-input:focus { border-color:var(--verde-sec); background:var(--blanco); }
.filtro-input::placeholder { color:var(--texto-ter); }
.filtro-input--icon-left { padding-left:36px; }
/* Select — una sola flecha (antes había un ícono SVG duplicado
   superpuesto en el HTML; se retiró y se dejó únicamente esta). */
.filtro-select { appearance:none; -webkit-appearance:none; cursor:pointer; padding-right:34px; background-image:var(--select-arrow); background-repeat:no-repeat; background-position:right 12px center; }
.filtro-select:focus { background-image:var(--select-arrow-focus); }
.filtro-icon { position:absolute; display:flex; align-items:center; color:var(--texto-sec); }
.filtro-icon--left { left:12px; }
.filtro-icon--no-events { pointer-events:none; }

/* Botón calendario — mismo aspecto que los inputs */
.cal-wrap { position:relative; }
.filtro-date-btn { display:flex; align-items:center; justify-content:space-between; text-align:left; cursor:pointer; }
.filtro-date-btn:hover, .filtro-date-btn:focus { border-color:var(--verde-sec); background:var(--blanco); outline:none; }
.filtro-date-btn--activa { border-color:var(--verde-sec); color:var(--texto); }
.placeholder-color { color:var(--texto-ter); }
.filtro-icon-cal { color:var(--texto-sec); display:flex; align-items:center; flex-shrink:0; margin-left:8px; }

/* Dropdown del calendario */
.cal-dropdown { position:absolute; top:calc(100% + 6px); left:0; z-index:200; background:var(--blanco); border:1px solid var(--borde); border-radius:12px; padding:16px; min-width:220px; box-shadow:var(--sombra-md); }
.cal-nav { display:flex; align-items:center; justify-content:space-between; margin-bottom:12px; }
.cal-nav-btn { background:none; border:none; cursor:pointer; color:var(--verde); padding:4px 6px; border-radius:6px; display:flex; align-items:center; transition:background 0.15s; font-family:inherit; }
.cal-nav-btn:hover { background:var(--fondo); }
.cal-year { font-size:13px; font-weight:700; color:var(--texto); }
.cal-grid { display:grid; grid-template-columns:repeat(3, 1fr); gap:6px; }
.cal-mes-btn { padding:8px 4px; font-size:12px; font-weight:600; color:var(--texto); background:none; border:1px solid transparent; border-radius:7px; cursor:pointer; transition:all 0.15s; font-family:inherit; }
.cal-mes-btn:hover { background:var(--fondo); border-color:var(--borde); }
.cal-mes-btn--sel { background:var(--verde); color:var(--blanco); border-color:var(--verde); }
.cal-mes-btn--sel:hover { background:var(--verde); border-color:var(--verde); }
.cal-clear { margin-top:10px; padding-top:10px; border-top:1px solid var(--borde-suave); text-align:center; }
.cal-clear-btn { background:none; border:none; font-size:12px; color:var(--texto-sec); cursor:pointer; font-weight:600; font-family:inherit; transition:color 0.15s; }
.cal-clear-btn:hover { color:#B71C1C; }
.cal-drop-enter-active, .cal-drop-leave-active { transition:opacity 0.15s, transform 0.15s; }
.cal-drop-enter-from, .cal-drop-leave-to { opacity:0; transform:translateY(-4px); }

/* ── Estado vacío ── */
.empty-state { text-align:center; padding:72px 24px; background:var(--blanco); border-radius:16px; border:1px solid var(--borde); color:var(--verde-sec); display:flex; flex-direction:column; align-items:center; gap:10px; }
.empty-state svg { opacity:0.4; }
.empty-title { font-size:16px; font-weight:700; color:var(--texto); margin:0; }
.empty-sub { font-size:13px; color:var(--texto-sec); margin:0; }

/* ── Tabla ── */
.table-wrapper { background:var(--blanco); border-radius:16px; border:1px solid var(--borde); overflow:hidden; box-shadow:var(--sombra-sm); }
.table-scroll { overflow-x:auto; -webkit-overflow-scrolling:touch; }
.don-table { width:100%; border-collapse:collapse; min-width:700px; }
.don-table thead th { padding:12px 16px; text-align:left; color:var(--texto-ter); font-size:9.5px; font-weight:700; text-transform:uppercase; letter-spacing:0.6px; white-space:nowrap; }
.don-table tbody tr { border-top:1px solid var(--borde-suave); transition:background 0.15s; }
.don-table tbody tr:hover { background:#FAFBFA; }
.don-table tbody td { padding:12px 16px; vertical-align:middle; }
.id-pill { font-size:11px; font-family:ui-monospace, Menlo, Consolas, monospace; background:var(--fondo); border:1px solid var(--borde); padding:3px 9px; border-radius:6px; color:var(--texto); font-weight:700; white-space:nowrap; }
.donor-name { display:block; font-size:12.5px; font-weight:700; color:var(--texto); line-height:1.3; }
.donor-mail { display:block; font-size:11px; color:var(--texto-sec); margin-top:2px; }
.metodo-text { font-size:12.5px; color:var(--texto-sec); }
.monto-text { font-size:12.5px; font-weight:700; color:var(--texto); }
.fecha-text { font-size:12.5px; color:var(--texto-sec); white-space:nowrap; }
.estado-badge { display:inline-block; font-size:10.5px; font-weight:700; padding:4px 11px; border-radius:20px; white-space:nowrap; }
.badge-pendiente { background:#FDF6E8; color:#96650A; }
.badge-aprobada { background:#EDF6EF; color:#2E7D32; }
.badge-rechazada { background:#FBEDEC; color:#B71C1C; }
.table-footer { padding:12px 16px; border-top:1px solid var(--borde-suave); font-size:12px; color:var(--texto-sec); font-weight:500; }

/* Botones de acción de la tabla — mismo componente icon-only de Mascotas */
.action-group { display:flex; gap:8px; align-items:center; }
.icon-only {
  width:38px; height:38px; border-radius:8px; border:1px solid var(--borde);
  background:var(--blanco); display:flex; align-items:center; justify-content:center;
  cursor:pointer; transition:background-color .16s ease, border-color .16s ease; position:relative;
}
.icon-only svg { width:16px; height:16px; }
.icon-only--ver { color:#3D453B; }
.icon-only--ver:hover { border-color:#C7D3C8; background:#FAFCFA; }
.icon-only::before {
  content:attr(data-tooltip); position:absolute; bottom:calc(100% + 8px); left:50%;
  transform:translateX(-50%) translateY(4px); background:var(--verde); color:#fff;
  font-size:11px; font-weight:600; padding:5px 9px; border-radius:7px; white-space:nowrap;
  opacity:0; visibility:hidden; pointer-events:none; transition:opacity .15s ease, transform .15s ease; z-index:20;
}
.icon-only:hover::before { opacity:1; visibility:visible; transform:translateX(-50%) translateY(0); }

/* ══════════════════════════════════════════════
   MODAL BASE — overlay y contenedor
   ══════════════════════════════════════════════ */
.modal-overlay { position:fixed; inset:0; background:rgba(0,0,0,0.35); backdrop-filter:blur(4px); z-index:1000; display:flex; align-items:center; justify-content:center; padding:24px; }
.modal-box { background:var(--blanco); border-radius:22px; box-shadow:var(--sombra-md); position:relative; }

/* ══════════════════════════════════════════════
   .modal-box--uniform — mismo tamaño exacto que el
   expediente de "Ver mascota" en Mascotas
   ══════════════════════════════════════════════ */
.modal-box--uniform {
  width:880px;
  max-width:92vw;
  height:660px;
  max-height:90vh;
  display:flex;
  flex-direction:column;
  overflow:hidden;
  border:1px solid var(--borde-suave);
}
.uniform-scroll { flex:1; min-height:0; overflow-y:auto; }
.close-btn {
  position:absolute; top:18px; right:18px; z-index:6;
  width:30px; height:30px; border-radius:9px; background:var(--fondo); border:1px solid var(--borde-suave);
  color:#8B928A; display:flex; align-items:center; justify-content:center; cursor:pointer;
  transition:background-color .16s ease, color .16s ease, border-color .16s ease;
}
.close-btn svg { width:16px; height:16px; }
.close-btn--hero { background:var(--fondo); }
.close-btn--hero:hover { background:var(--verde); color:#fff; }

/* ── HERO (Ver donación) — mismo estilo que el hero de Mascotas ── */
.hero {
  flex-shrink:0;
  background:linear-gradient(165deg, #FFFFFF 0%, #F7FAF7 55%, #F1F7F2 100%);
  border-bottom:1px solid var(--borde-suave);
  padding:28px 40px 24px;
  display:flex; align-items:center; gap:20px;
}
.hero-photo {
  width:60px; height:60px; border-radius:16px; flex-shrink:0; overflow:hidden;
  background:linear-gradient(150deg,#E7F0E8 0%,#DCEBDE 100%);
  border:1px solid var(--borde-suave);
  display:flex; align-items:center; justify-content:center;
  box-shadow:0 1px 2px rgba(58,71,60,.04), 0 10px 22px -12px rgba(58,71,60,.28);
}
.hero-photo-ini { font-size:20px; font-weight:700; color:#3E7A45; letter-spacing:-.3px; }
.hero-info { flex:1; min-width:0; display:flex; flex-direction:column; gap:8px; }
.hero-name-row { display:flex; align-items:center; gap:12px; flex-wrap:wrap; }
.hero-name { font-size:21px; font-weight:700; color:var(--texto); margin:0; letter-spacing:-.4px; }
.hero-meta { display:flex; align-items:center; gap:7px; flex-wrap:wrap; }
.hero-meta-chip {
  display:inline-flex; align-items:center; gap:6px; font-size:11.5px; font-weight:600; color:#4B5A4C;
  background:var(--blanco); border:1px solid var(--borde-suave); padding:4px 10px 4px 9px; border-radius:20px;
}
.hero-meta-chip svg { color:var(--verde-sec); flex-shrink:0; }
.hero-meta-chip--monto { font-weight:700; color:#2E7D45; background:#EDF6EF; border-color:#C9E4CE; }
.badge-status-hero { padding:5px 12px !important; font-size:10.5px !important; }

/* ── BODY (Ver donación) ── */
.body { padding:18px 40px 10px; }
.grid-2col { display:grid; grid-template-columns:1.6fr 1fr; gap:14px; align-items:start; margin-bottom:0; }
.block { background:var(--blanco); border:1px solid var(--borde-suave); border-radius:14px; padding:18px 20px; margin-bottom:14px; box-shadow:var(--sombra-sm); }
.block:last-child { margin-bottom:0; }
.block-title { display:flex; align-items:center; gap:10px; font-size:12.5px; font-weight:700; color:var(--texto); text-transform:uppercase; letter-spacing:.4px; margin:0 0 14px; }
.block-title-icon { width:24px; height:24px; border-radius:50%; background:#F0F5F0; color:#4E7A54; display:flex; align-items:center; justify-content:center; flex-shrink:0; }
.fields-row { display:grid; grid-template-columns:repeat(3, 1fr); gap:14px 16px; }
.field-col { display:flex; flex-direction:column; gap:5px; }
.field-label-row { font-size:10px; font-weight:700; color:var(--texto-ter); text-transform:uppercase; letter-spacing:.4px; }
.field-value { font-size:14px; font-weight:600; color:var(--texto); word-break:break-word; }
.field-value--highlight { font-size:15px; font-weight:800; }
.block-wide { margin-top:0; }
.tint-box { background:var(--fondo); border-radius:10px; padding:13px 15px; }
.tint-box span { font-size:13px; font-weight:600; color:var(--texto); line-height:1.55; }
.tint-box--desc span { font-weight:500; color:#4B534A; }
.modal-empty-text { font-size:13px; color:var(--texto-ter); background:var(--fondo); border:1px dashed var(--borde); border-radius:10px; padding:18px 16px; margin:0; text-align:center; }
.comprobante-img-wrap { display:flex; flex-direction:column; align-items:flex-start; gap:8px; }
.comprobante-thumb { border-radius:10px; border:1px solid var(--borde); max-width:100%; max-height:200px; object-fit:cover; cursor:zoom-in; transition:all 0.3s ease; box-shadow:var(--sombra-sm); }
.comprobante-thumb.ampliada { max-height:420px; cursor:zoom-out; }
.comprobante-hint { font-size:11px; color:var(--texto-ter); }
.pdf-label { font-size:13px; color:var(--texto); font-weight:600; margin-bottom:10px; }
.btn-abrir-pdf { padding:7px 16px; border-radius:8px; border:none; background:var(--verde); color:var(--blanco); font-size:12px; font-weight:700; cursor:pointer; transition:background 0.2s; font-family:inherit; }
.btn-abrir-pdf:hover { background:#2D372F; }

/* ── FOOTER (Ver donación) ── */
.footer { flex-shrink:0; display:flex; align-items:center; justify-content:flex-end; gap:8px; padding:14px 40px 18px; border-top:1px solid var(--borde-suave); }
.estado-final-msg { flex:1; margin:0; font-size:12.5px; font-weight:700; }
.estado-final-msg--ok { color:#2E7D32; }
.estado-final-msg--bad { color:#B71C1C; }
.btn-ghost-red { display:flex; align-items:center; gap:6px; height:29px; padding:0 12px; border-radius:8px; background:var(--blanco); border:1px solid var(--borde); color:var(--texto-sec); font-size:11.5px; font-weight:600; cursor:pointer; transition:background-color .16s ease, border-color .16s ease, color .16s ease; }
.btn-ghost-red:hover { background:#FDF4F3; border-color:#E8B9B2; color:var(--rojo); }
.btn-footer-danger { display:flex; align-items:center; height:29px; padding:0 12px; border-radius:8px; background:var(--rojo-bg); border:none; color:var(--rojo); font-size:11.5px; font-weight:600; cursor:pointer; transition:background-color .16s ease, color .16s ease; }
.btn-footer-danger:hover { background:var(--rojo); color:#fff; }
.btn-footer-success { display:flex; align-items:center; height:29px; padding:0 12px; border-radius:8px; background:#EDF6EF; border:none; color:#2E7D32; font-size:11.5px; font-weight:600; cursor:pointer; transition:background-color .16s ease, color .16s ease; }
.btn-footer-success:hover { background:#2E7D32; color:#fff; }

/* Animaciones modal */
.modal-fade-enter-active, .modal-fade-leave-active { transition:opacity 0.22s ease; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity:0; }

/* ── Responsive ── */
@media (max-width:1100px) { .don-summary { grid-template-columns:repeat(3, 1fr); } }
@media (max-width:900px) {
  .don-summary { grid-template-columns:repeat(2, 1fr); }
  .modal-box--uniform { width:94vw; height:88vh; }
  .grid-2col { grid-template-columns:1fr; }
  .fields-row { grid-template-columns:repeat(2, 1fr); }
}
@media (max-width:640px) {
  .page-header { flex-direction:column; align-items:flex-start; }
  .filtros-row { flex-direction:column; gap:14px; }
  .filtros-row--end { align-items:stretch; }
  .filtro-group { min-width:100%; }
  .filtro-group--search { max-width:none; }
  .don-summary { grid-template-columns:1fr 1fr; }
  .modal-box--uniform { width:96vw; height:92vh; border-radius:18px; }
  .hero, .body, .footer { padding-left:20px; padding-right:20px; }
  .fields-row { grid-template-columns:1fr; }
  .cal-dropdown { min-width:200px; width:100%; max-width:280px; }
}
@media (max-width:480px) { .don-summary { grid-template-columns:1fr; } }
</style>
<style>
/* ── Variables globales (para contenido teletransportado) ── */
:root {
  --verde: #3A473C; --verde-sec:#92A894; --fondo:#F7F8F7; --blanco:#FFFFFF;
  --texto:#2B322C; --texto-sec:#7A827B; --texto-ter:#A2A9A3;
  --borde:#E9ECE9; --borde-suave:#EFF2EF; --amarillo:#F5B942;
  --verde-ok:#4CAF6A; --rojo:#C0392B; --rojo-bg:#FBEDEC;
  --sombra-sm:0 1px 2px rgba(58,71,60,.03);
  --sombra-md:0 2px 4px rgba(58,71,60,.05), 0 14px 32px -14px rgba(58,71,60,.18);
}
</style>
