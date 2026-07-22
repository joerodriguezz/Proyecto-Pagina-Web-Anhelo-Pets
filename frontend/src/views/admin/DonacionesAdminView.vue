<script setup>
import { ref, computed, onMounted } from 'vue'
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
        <strong class="don-value">{{ simboloEstadisticas }} {{ formatMonto(totalMes) }}</strong>
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
        <!-- Estado -->
        <div class="filtro-group filtro-group--select">
          <label class="filtro-label">Estado</label>
          <div class="filtro-input-wrap">
            <select v-model="filtroEstado" class="filtro-input filtro-select">
              <option value="">Todos</option>
              <option value="Pendiente">Pendiente</option>
              <option value="Aprobada">Aprobada</option>
              <option value="Rechazada">Rechazada</option>
            </select>
            <span class="filtro-icon filtro-icon--right filtro-icon--no-events">
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
            </span>
          </div>
        </div>

        <!-- Método -->
        <div class="filtro-group filtro-group--select">
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
            <span class="filtro-icon filtro-icon--right filtro-icon--no-events">
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
            </span>
          </div>
        </div>

        <!-- Moneda -->
        <div class="filtro-group filtro-group--select">
          <label class="filtro-label">Moneda</label>
          <div class="filtro-input-wrap">
            <select v-model="filtroMoneda" class="filtro-input filtro-select">
              <option value="">Todas</option>
              <option value="CRC">CRC — Colones</option>
              <option value="USD">USD — Dólares</option>
            </select>
            <span class="filtro-icon filtro-icon--right filtro-icon--no-events">
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
            </span>
          </div>
        </div>

        <!-- Fecha donación — calendario custom -->
        <div class="filtro-group filtro-group--select">
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

        <!-- Limpiar -->
        <div class="filtro-group filtro-group--btn">
          <button
            type="button"
            class="btn-limpiar"
            :class="{ 'btn-limpiar--activo': hayFiltros }"
            @click="limpiarFiltros"
          >Limpiar filtros</button>
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
              <th>Acción</th>
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
                  <button type="button" class="btn-accion-pill btn-ver" @click="abrirModal(d)">Ver detalle</button>
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

    <!-- ═══════════ MODAL DE DETALLE ═══════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="modalAbierto && donacionActual" class="modal-overlay" @click.self="cerrarModal">
          <div class="modal-box modal-box--lg">
            <button type="button" class="modal-close" @click="cerrarModal">✕</button>

            <div class="modal-header">
              <div class="modal-header-info">
                <p class="modal-eyebrow">Donación {{ donacionActual.id }}</p>
                <h2 class="modal-title">{{ donacionActual.nombre || 'Anónimo' }}</h2>
                <p class="modal-sub">{{ donacionActual.correo || '—' }}</p>
              </div>
              <div class="modal-header-badges">
                <span class="estado-badge" :class="estadoClass(donacionActual.estado)">{{ donacionActual.estado }}</span>
                <span class="monto-chip">{{ simboloMoneda(donacionActual.moneda) }} {{ formatMonto(donacionActual.monto) }}</span>
              </div>
            </div>

            <div class="modal-section">
              <h4 class="modal-section-title">Información personal</h4>
              <div class="modal-grid modal-grid--3">
                <div class="modal-field">
                  <span class="modal-field-label">Nombre</span>
                  <strong class="modal-field-value">{{ donacionActual.nombre || '—' }}</strong>
                </div>
                <div class="modal-field">
                  <span class="modal-field-label">Correo</span>
                  <strong class="modal-field-value">{{ donacionActual.correo || '—' }}</strong>
                </div>
                <div class="modal-field">
                  <span class="modal-field-label">Teléfono</span>
                  <strong class="modal-field-value">{{ donacionActual.telefono || '—' }}</strong>
                </div>
              </div>
            </div>

            <div class="modal-section">
              <h4 class="modal-section-title">Información financiera</h4>
              <div class="modal-grid modal-grid--3">
                <div class="modal-field">
                  <span class="modal-field-label">Método</span>
                  <strong class="modal-field-value">{{ donacionActual.metodo || '—' }}</strong>
                </div>
                <div class="modal-field">
                  <span class="modal-field-label">Moneda</span>
                  <strong class="modal-field-value">{{ donacionActual.moneda || '—' }}</strong>
                </div>
                <div class="modal-field">
                  <span class="modal-field-label">Monto</span>
                  <strong class="modal-field-value monto-highlight">
                    {{ simboloMoneda(donacionActual.moneda) }} {{ formatMonto(donacionActual.monto) }}
                  </strong>
                </div>
                <div class="modal-field">
                  <span class="modal-field-label">Fecha de donación</span>
                  <strong class="modal-field-value">{{ formatFecha(donacionActual.fechaDonacion) }}</strong>
                </div>
                <div class="modal-field">
                  <span class="modal-field-label">Fecha de registro</span>
                  <strong class="modal-field-value">{{ formatFecha(donacionActual.fechaRegistro) }}</strong>
                </div>
              </div>
            </div>

            <div v-if="donacionActual.mensaje" class="modal-section">
              <h4 class="modal-section-title">Mensaje</h4>
              <p class="modal-mensaje">{{ donacionActual.mensaje }}</p>
            </div>

            <div class="modal-section">
              <h4 class="modal-section-title">Comprobante</h4>
              <div v-if="!donacionActual.comprobante" class="sin-comprobante">Sin comprobante adjunto</div>
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
              <div v-else-if="esPDF(donacionActual.comprobante)" class="comprobante-pdf">
                <p class="pdf-label">Documento PDF adjunto</p>
                <button type="button" class="btn-abrir-pdf" @click="abrirPDF(donacionActual.comprobante)">Abrir PDF</button>
              </div>
            </div>

            <div v-if="donacionActual.estado === 'Pendiente'" class="modal-acciones">
              <button type="button" class="btn-accion-pill btn-aprobar" @click="cambiarEstado('Aprobada')">Aprobar donación</button>
              <button type="button" class="btn-accion-pill btn-rechazar" @click="cambiarEstado('Rechazada')">Rechazar donación</button>
            </div>
            <div v-else class="modal-estado-final">
              <p v-if="donacionActual.estado === 'Aprobada'" class="estado-aprobada-msg">Esta donación ha sido aprobada.</p>
              <p v-if="donacionActual.estado === 'Rechazada'" class="estado-rechazada-msg">Esta donación ha sido rechazada.</p>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

  </div>
</template>

<style scoped>
/* ── Variables (idénticas a Solicitudes de Adopción) ────────────────────── */
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
.total-mes-icon  { background: #EAF2F6; border-color: #C7DCE6; color: #3C6E85; }
.total-anio-icon { background: #EDF6EF; border-color: #C9E4CE; color: #2E7D45; }
.total-icon      { background: #F2F3F2; border-color: #DFE2DF; color: #616861; }
.pendiente-icon  { background: #FDF6E8; border-color: #F2E1B8; color: #A97A0C; }
.aprobada-icon   { background: #EDF6EF; border-color: #C9E4CE; color: #2E7D45; }

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
.filtro-group--select { flex: 0 0 auto; min-width: 180px; }
.filtro-group--btn  { flex: 0 0 auto; }
.filtro-group--search { flex: 1; min-width: 220px; max-width: 340px; }

.filtro-label {
  font-size: 10.5px;
  font-weight: 700;
  color: var(--texto-ter);
  text-transform: uppercase;
  letter-spacing: 0.6px;
}

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

.filtro-select {
  appearance: none;
  -webkit-appearance: none;
  cursor: pointer;
  padding-right: 34px;
}

.filtro-icon { position: absolute; display: flex; align-items: center; color: var(--texto-sec); }
.filtro-icon--right { right: 12px; }
.filtro-icon--left  { left: 12px; }
.filtro-icon--no-events { pointer-events: none; }

/* Botón calendario — mismo aspecto que los inputs */
.cal-wrap { position: relative; }
.filtro-date-btn {
  display: flex;
  align-items: center;
  justify-content: space-between;
  text-align: left;
  cursor: pointer;
}
.filtro-date-btn:hover,
.filtro-date-btn:focus { border-color: var(--verde-sec); background: var(--blanco); outline: none; }
.filtro-date-btn--activa { border-color: var(--verde-sec); color: var(--texto); }
.placeholder-color { color: var(--texto-ter); }
.filtro-icon-cal {
  color: var(--texto-sec);
  display: flex;
  align-items: center;
  flex-shrink: 0;
  margin-left: 8px;
}

/* Dropdown del calendario */
.cal-dropdown {
  position: absolute;
  top: calc(100% + 6px);
  left: 0;
  z-index: 200;
  background: var(--blanco);
  border: 1px solid var(--borde);
  border-radius: 12px;
  padding: 16px;
  min-width: 220px;
  box-shadow: var(--sombra-md);
}
.cal-nav {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 12px;
}
.cal-nav-btn {
  background: none;
  border: none;
  cursor: pointer;
  color: var(--verde);
  padding: 4px 6px;
  border-radius: 6px;
  display: flex;
  align-items: center;
  transition: background 0.15s;
  font-family: inherit;
}
.cal-nav-btn:hover { background: var(--fondo); }
.cal-year { font-size: 13px; font-weight: 700; color: var(--texto); }
.cal-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 6px; }
.cal-mes-btn {
  padding: 8px 4px;
  font-size: 12px;
  font-weight: 600;
  color: var(--texto);
  background: none;
  border: 1px solid transparent;
  border-radius: 7px;
  cursor: pointer;
  transition: all 0.15s;
  font-family: inherit;
}
.cal-mes-btn:hover      { background: var(--fondo); border-color: var(--borde); }
.cal-mes-btn--sel       { background: var(--verde); color: var(--blanco); border-color: var(--verde); }
.cal-mes-btn--sel:hover { background: var(--verde); border-color: var(--verde); }
.cal-clear {
  margin-top: 10px;
  padding-top: 10px;
  border-top: 1px solid var(--borde-suave);
  text-align: center;
}
.cal-clear-btn {
  background: none;
  border: none;
  font-size: 12px;
  color: var(--texto-sec);
  cursor: pointer;
  font-weight: 600;
  font-family: inherit;
  transition: color 0.15s;
}
.cal-clear-btn:hover { color: #B71C1C; }
.cal-drop-enter-active, .cal-drop-leave-active { transition: opacity 0.15s, transform 0.15s; }
.cal-drop-enter-from, .cal-drop-leave-to       { opacity: 0; transform: translateY(-4px); }

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
.donor-mail { display: block; font-size: 11px; color: var(--texto-sec); margin-top: 2px; }
.metodo-text { font-size: 12.5px; color: var(--texto-sec); }
.monto-text  { font-size: 12.5px; font-weight: 700; color: var(--texto); }
.fecha-text  { font-size: 12.5px; color: var(--texto-sec); white-space: nowrap; }

/* Badges */
.estado-badge    { display: inline-block; font-size: 10.5px; font-weight: 700; padding: 4px 11px; border-radius: 20px; white-space: nowrap; }
.badge-pendiente { background: #FDF6E8; color: #96650A; }
.badge-aprobada  { background: #EDF6EF; color: #2E7D32; }
.badge-rechazada { background: #FBEDEC; color: #B71C1C; }

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
.btn-ver      { color: var(--verde-sec); }
.btn-ver:hover      { background: #F1F5F1; border-color: #DCE4DC; color: var(--verde); transform: translateY(-1px); }
.btn-aprobar  { background: #EDF6EF; border-color: #C9E4CE; color: #2E7D32; }
.btn-aprobar:hover  { background: #D9EEDC; }
.btn-rechazar { background: #FBEDEC; border-color: #F1C7C3; color: #B71C1C; }
.btn-rechazar:hover { background: #F7D6D2; }

.table-footer { padding: 12px 16px; border-top: 1px solid var(--borde-suave); font-size: 12px; color: var(--texto-sec); font-weight: 500; }

/* ── Modal ─────────────────────────────────────────────────────────────── */
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
  border-radius: 20px;
  padding: 36px;
  width: 100%; max-width: 640px;
  max-height: 90vh; overflow-y: auto;
  position: relative;
  box-shadow: var(--sombra-md);
}
.modal-box--lg { max-width: 760px; }

.modal-close {
  position: absolute; top: 18px; right: 18px;
  width: 32px; height: 32px; border-radius: 50%;
  border: 1px solid var(--borde); background: var(--fondo);
  color: var(--texto); font-size: 13px; font-weight: 700;
  cursor: pointer; display: flex; align-items: center; justify-content: center;
  transition: all 0.2s; font-family: inherit;
}
.modal-close:hover { background: var(--verde); color: var(--blanco); border-color: var(--verde); }

.modal-header {
  display: flex;
  align-items: flex-start;
  gap: 14px;
  margin-bottom: 24px;
  padding-bottom: 20px;
  border-bottom: 1px solid var(--borde-suave);
}
.modal-header-info { flex: 1; min-width: 0; }
.modal-eyebrow {
  font-size: 10.5px; font-weight: 700; color: var(--verde-sec);
  text-transform: uppercase; letter-spacing: 0.7px; margin: 0 0 4px;
}
.modal-title { font-size: 19px; font-weight: 700; color: var(--texto); letter-spacing: -0.4px; margin: 0; }
.modal-sub   { font-size: 12px; color: var(--texto-sec); margin: 3px 0 0; }
.modal-header-badges {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 6px;
  flex-shrink: 0;
}
.monto-chip {
  font-size: 11px;
  font-weight: 600;
  color: #4E6E51;
  background: #F1F5F1;
  padding: 3px 10px;
  border-radius: 7px;
  white-space: nowrap;
}

.modal-section       { margin-bottom: 24px; }
.modal-section-title {
  font-size: 10.5px; font-weight: 700; color: var(--texto-ter);
  text-transform: uppercase; letter-spacing: 0.5px;
  margin-bottom: 14px; padding-bottom: 10px;
  border-bottom: 1px solid var(--borde-suave);
}

.modal-grid    { display: grid; grid-template-columns: repeat(2,1fr); gap: 14px; }
.modal-grid--1 { grid-template-columns: 1fr; }
.modal-grid--3 { display: grid; grid-template-columns: repeat(3,1fr); gap: 12px; }
.modal-field  { display: flex; flex-direction: column; gap: 4px; background: var(--fondo); border-radius: 10px; padding: 10px 12px; border: 1px solid var(--borde); transition: border-color 0.15s, box-shadow 0.15s; }
.modal-field:hover { border-color: #D5DED6; box-shadow: 0 2px 10px rgba(58,71,60,0.06); }
.modal-field-label { font-size: 10px; font-weight: 700; color: var(--texto-ter); text-transform: uppercase; letter-spacing: 0.4px; }
.modal-field-value { font-size: 13px; color: var(--texto); font-weight: 600; word-break: break-word; }
.monto-highlight { font-size: 15px; color: var(--texto); font-weight: 800; }

.modal-mensaje {
  font-size: 13.5px; color: var(--texto); line-height: 1.7;
  background: var(--fondo); border-radius: 10px; padding: 14px 16px;
  margin: 0;
}

.sin-comprobante { font-size: 13px; color: var(--texto-ter); font-style: italic; }
.comprobante-img-wrap { display: flex; flex-direction: column; align-items: flex-start; gap: 8px; }
.comprobante-thumb {
  border-radius: 10px; border: 1px solid var(--borde);
  max-width: 200px; max-height: 200px; object-fit: cover;
  cursor: zoom-in; transition: all 0.3s ease;
  box-shadow: var(--sombra-sm);
}
.comprobante-thumb.ampliada { max-width: 100%; max-height: 460px; cursor: zoom-out; }
.comprobante-hint { font-size: 11px; color: var(--texto-ter); }
.comprobante-pdf {
  background: var(--fondo); border-radius: 10px;
  border: 1px solid var(--borde); padding: 16px;
}
.pdf-label { font-size: 13px; color: var(--texto); font-weight: 600; margin-bottom: 10px; }
.btn-abrir-pdf {
  padding: 7px 16px; border-radius: 7px; border: none;
  background: var(--verde); color: var(--blanco);
  font-size: 12px; font-weight: 700; cursor: pointer;
  transition: background 0.2s; font-family: inherit;
}
.btn-abrir-pdf:hover { background: #2D372F; }

.modal-acciones {
  display: flex;
  gap: 10px;
  justify-content: flex-end;
  padding-top: 20px;
  border-top: 1px solid var(--borde-suave);
}
.modal-estado-final {
  padding-top: 20px;
  border-top: 1px solid var(--borde-suave);
  text-align: center;
}
.estado-aprobada-msg  { color: #2E7D32; font-weight: 700; font-size: 13.5px; }
.estado-rechazada-msg { color: #B71C1C; font-weight: 700; font-size: 13.5px; }

.modal-fade-enter-active, .modal-fade-leave-active { transition: opacity 0.22s ease; }
.modal-fade-enter-from, .modal-fade-leave-to       { opacity: 0; }

/* ── Responsive ────────────────────────────────────────────────────────── */
@media (max-width: 1100px) {
  .don-summary { grid-template-columns: repeat(3, 1fr); }
}
@media (max-width: 900px) {
  .don-summary { grid-template-columns: repeat(2, 1fr); }
  .modal-grid--3 { grid-template-columns: repeat(2, 1fr); }
}
@media (max-width: 768px) {
  .don-summary { grid-template-columns: repeat(2, 1fr); gap: 10px; }

  .filtros-panel { padding: 14px; gap: 14px; }
  .filtros-row { flex-direction: column; gap: 12px; }
  .filtro-group { min-width: unset; width: 100%; }
  .filtro-group--search { max-width: none; }
  .filtro-group--select { min-width: unset; width: 100%; }

  .btn-limpiar { width: 100%; }

  .table-scroll { overflow-x: auto; -webkit-overflow-scrolling: touch; }

  .modal-box { padding: 22px 16px; max-width: calc(100vw - 32px); max-height: 95vh; }
  .modal-grid { grid-template-columns: 1fr; }
  .modal-grid--3 { grid-template-columns: 1fr 1fr; }
  .modal-header { flex-wrap: wrap; gap: 10px; }
  .modal-acciones { flex-direction: column; }

  .page-header { flex-direction: column; align-items: flex-start; gap: 10px; }

  .action-group { flex-wrap: wrap; }
  .cal-dropdown { min-width: 200px; width: 100%; max-width: 280px; }
}
@media (max-width: 480px) {
  .don-summary { grid-template-columns: 1fr; }
  .modal-grid--3 { grid-template-columns: 1fr; }
}
</style>

<style>
/* ── Variables globales ─────────────────────────────
   El modal usa <Teleport to="body">, lo que lo saca del árbol DOM de
   .view-container y le impediría heredar las variables CSS definidas
   ahí (dejándolo con fondo transparente). Se declaran también en :root
   para que el modal teletransportado las reciba correctamente. */
:root {
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
}
</style>