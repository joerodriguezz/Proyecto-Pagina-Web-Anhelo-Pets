<script setup>
import { ref, computed, onMounted } from 'vue'
import { getDonations, updateDonationStatus } from '../../services/donationServices'

// ─── Datos desde la API ────────────────────────────────────────
const todasDonaciones = ref([])

// Adapta el DonationDto plano de la API a los nombres en español que ya
// usa esta plantilla, para no reescribir la tabla/modal existentes.
function adaptarDonacion(dto) {
  return {
    id: dto.donationId,
    nombre: dto.donorName,
    correo: dto.email,
    telefono: dto.phone,
    metodo: dto.method,
    moneda: dto.currency,
    monto: dto.amount,
    fechaDonacion: dto.donatedAt,
    fechaRegistro: dto.createdAt,
    mensaje: dto.message,
    comprobante: dto.proofFile,
    estado: dto.validationStatus,
  }
}

async function cargarDonaciones() {
  try {
    const { data } = await getDonations()
    todasDonaciones.value = data.map(adaptarDonacion)
  } catch (e) {
    console.error(e)
    todasDonaciones.value = []
  }
}

onMounted(() => {
  cargarDonaciones()
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
async function cambiarEstado(nuevoEstado) {
  if (!donacionActual.value) return
  const action = nuevoEstado === 'Aprobada' ? 'Aprobar' : 'Rechazar'

  try {
    await updateDonationStatus(donacionActual.value.id, action)
  } catch (e) {
    console.error(e)
    return
  }

  const idx = todasDonaciones.value.findIndex(d => d.id === donacionActual.value.id)
  if (idx !== -1) todasDonaciones.value[idx].estado = nuevoEstado
  donacionActual.value.estado = nuevoEstado
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
      <div>
        <h1 class="admin-page-title">Donaciones</h1>
        <p class="admin-page-sub">Historial y control de donaciones recibidas</p>
      </div>
    </header>

    <!-- TARJETAS RESUMEN -->
    <div class="don-summary">
      <div class="don-card total-mes">
        <span class="don-label">Total aprobado este mes</span>
        <strong class="don-value">
  {{ simboloEstadisticas }} {{ formatMonto(totalMes) }}
</strong>
      </div>
      <div class="don-card total-año">
        <span class="don-label">Total aprobado este año</span>
        <strong class="don-value">
  {{ simboloEstadisticas }} {{ formatMonto(totalMes) }}
</strong>
      </div>
      <div class="don-card count">
        <span class="don-label">Total donaciones</span>
        <strong class="don-value">{{ totalDonaciones }}</strong>
      </div>
      <div class="don-card pendientes">
        <span class="don-label">Pendientes</span>
        <strong class="don-value">{{ totalPendientes }}</strong>
      </div>
      <div class="don-card aprobadas">
        <span class="don-label">Aprobadas</span>
        <strong class="don-value">{{ totalAprobadas }}</strong>
      </div>
    </div>

    <!-- FILTROS -->
    <div class="filtros-panel">

      <!-- Buscar donante -->
      <div class="filtro-group">
        <label class="filtro-label">Buscar donante</label>
        <div class="filtro-input-wrap">
          <input
            v-model="filtroNombre"
            placeholder="Nombre..."
            class="filtro-input filtro-input--icon"
          />
          <span class="filtro-icon filtro-icon--right">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
          </span>
        </div>
      </div>

      <!-- Estado -->
      <div class="filtro-group">
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
          <span class="filtro-icon filtro-icon--right filtro-icon--no-events">
            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
          </span>
        </div>
      </div>

      <!-- Moneda -->
      <div class="filtro-group">
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

      <!-- Limpiar -->
      <div class="filtro-group filtro-group--btn">
        <button
          type="button"
          class="btn-limpiar"
          :class="{ 'btn-limpiar--activo': hayFiltros }"
          @click="limpiarFiltros"
        >
          Limpiar filtros
        </button>
      </div>

    </div>

    <!-- ESTADO VACÍO -->
    <div v-if="donacionesFiltradas.length === 0" class="empty-state">
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
              <td><button class="btn-ver" @click="abrirModal(d)">Ver detalle</button></td>
            </tr>
          </tbody>
        </table>
      </div>
      <div class="table-footer">
        {{ donacionesFiltradas.length }} donación{{ donacionesFiltradas.length !== 1 ? 'es' : '' }} encontrada{{ donacionesFiltradas.length !== 1 ? 's' : '' }}
      </div>
    </div>

    <!-- ═══════════ MODAL DE DETALLE ═══════════ -->
    <transition name="modal-fade">
      <div v-if="modalAbierto && donacionActual" class="modal-overlay" @click.self="cerrarModal">
        <div class="modal-box">

          <button class="modal-close" @click="cerrarModal">✕</button>

          <div class="modal-header">
            <span class="modal-id">{{ donacionActual.id }}</span>
            <span class="estado-badge" :class="estadoClass(donacionActual.estado)">{{ donacionActual.estado }}</span>
          </div>

          <div class="modal-section">
            <h4 class="modal-section-title">Información personal</h4>
            <div class="modal-grid">
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
            <div class="modal-grid">
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
              <button class="btn-abrir-pdf" @click="abrirPDF(donacionActual.comprobante)">Abrir PDF</button>
            </div>
          </div>

          <div v-if="donacionActual.estado === 'Pendiente'" class="modal-acciones">
            <button class="btn-aprobar" @click="cambiarEstado('Aprobada')">Aprobar donación</button>
            <button class="btn-rechazar" @click="cambiarEstado('Rechazada')">Rechazar donación</button>
          </div>
          <div v-else class="modal-estado-final">
            <p v-if="donacionActual.estado === 'Aprobada'" class="estado-aprobada-msg">Esta donación ha sido aprobada.</p>
            <p v-if="donacionActual.estado === 'Rechazada'" class="estado-rechazada-msg">Esta donación ha sido rechazada.</p>
          </div>

        </div>
      </div>
    </transition>

  </div>
</template>

<style scoped>
/* ── Variables ─────────────────────────────────────── */
.view-container {
  --verde:     #3A473C;
  --verde-sec: #92A894;
  --fondo:     #F7F8F7;
  --blanco:    #FFFFFF;
  --texto:     #2F352F;
  --texto-sec: #6C756D;
  --borde:     #E8ECE8;
  --amarillo:  #F5B942;
  --verde-ok:  #4CAF6A;
  background: transparent;
}

/* ── Encabezado ────────────────────────────────────── */
.page-header       { margin-bottom: 28px; }
.admin-page-title  { font-size: 28px; font-weight: 800; color: var(--verde); letter-spacing: -0.5px; line-height: 1.1; }
.admin-page-sub    { font-size: 14px; color: var(--texto-sec); margin-top: 4px; font-weight: 500; }

/* ── Tarjetas resumen ──────────────────────────────── */
.don-summary {
  display: flex;
  gap: 14px;
  margin-bottom: 20px;
  flex-wrap: wrap;
}

.don-card {
  flex: 1;
  min-width: 150px;
  background: var(--blanco);
  border-radius: 14px;
  padding: 20px;
  border: 1px solid var(--borde);
  border-top: 3px solid var(--borde);
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.total-mes  { border-top-color: var(--amarillo); }
.total-año  { border-top-color: var(--verde-sec); }
.count      { border-top-color: var(--texto-sec); }
.pendientes { border-top-color: var(--amarillo); }
.aprobadas  { border-top-color: var(--verde-ok); }

.don-label { font-size: 11px; color: var(--texto-sec); font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; }
.don-value { font-size: 24px; font-weight: 800; color: var(--verde); line-height: 1; }

/* ── Panel de filtros ──────────────────────────────── */
.filtros-panel {
  background: var(--blanco);
  border-radius: 14px;
  padding: 20px;
  margin-bottom: 20px;
  border: 1px solid var(--borde);
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
  align-items: flex-end;
}

.filtro-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
  flex: 1;
  min-width: 130px;
}

/* El grupo del botón no crece tanto */
.filtro-group--btn {
  flex: 0 0 auto;
  min-width: unset;
}

.filtro-label {
  font-size: 11px;
  font-weight: 700;
  color: var(--verde);
  text-transform: uppercase;
  letter-spacing: 0.5px;
  /* altura fija para que todos los labels ocupen lo mismo */
  min-height: 16px;
  display: flex;
  align-items: flex-end;
}

/* Wrapper para posicionar íconos */
.filtro-input-wrap {
  position: relative;
  display: flex;
  align-items: center;
}

/* Input base — altura fija 38px para nivelar todos */
.filtro-input {
  width: 100%;
  height: 38px;
  padding: 0 36px 0 12px;
  border-radius: 8px;
  border: 1.5px solid var(--borde);
  background: var(--fondo);
  font-size: 13px;
  color: var(--texto);
  font-family: inherit;
  outline: none;
  transition: border-color 0.18s, background 0.18s;
  box-sizing: border-box;
}

.filtro-input:focus {
  border-color: var(--verde-sec);
  background: var(--blanco);
}

.filtro-input::placeholder { color: #9CA8A0; }

/* Select: quitar flecha nativa */
.filtro-select {
  appearance: none;
  -webkit-appearance: none;
  cursor: pointer;
}

/* Ícono derecho genérico */
.filtro-icon {
  position: absolute;
  display: flex;
  align-items: center;
  color: var(--texto-sec);
}

.filtro-icon--right  { right: 11px; }
.filtro-icon--no-events { pointer-events: none; }

/* Botón calendario — mismo aspecto que los inputs */
.cal-wrap { position: relative; }

.filtro-date-btn {
  display: flex;
  align-items: center;
  justify-content: space-between;
  text-align: left;
  cursor: pointer;
  background: var(--fondo);
  border: 1.5px solid var(--borde);
  padding: 0 12px;
  /* hereda height: 38px de .filtro-input */
}

.filtro-date-btn:focus,
.filtro-date-btn:hover     { border-color: var(--verde-sec); background: var(--blanco); outline: none; }
.filtro-date-btn--activa   { border-color: var(--verde-sec); color: var(--texto); }

.placeholder-color { color: #9CA8A0; }

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
  border: 1.5px solid var(--borde);
  border-radius: 12px;
  padding: 16px;
  min-width: 220px;
  box-shadow: 0 8px 24px rgba(58,71,60,0.10);
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

.cal-year {
  font-size: 14px;
  font-weight: 700;
  color: var(--verde);
}

.cal-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 6px;
}

.cal-mes-btn {
  padding: 8px 4px;
  font-size: 12px;
  font-weight: 600;
  color: var(--texto);
  background: none;
  border: 1.5px solid transparent;
  border-radius: 7px;
  cursor: pointer;
  transition: all 0.15s;
  font-family: inherit;
}

.cal-mes-btn:hover         { background: var(--fondo); border-color: var(--borde); }
.cal-mes-btn--sel          { background: var(--verde); color: var(--blanco); border-color: var(--verde); }
.cal-mes-btn--sel:hover    { background: #2D372F; border-color: #2D372F; }

.cal-clear {
  margin-top: 10px;
  padding-top: 10px;
  border-top: 1px solid var(--borde);
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

.cal-clear-btn:hover { color: #D95C5C; }

/* Animación dropdown */
.cal-drop-enter-active, .cal-drop-leave-active { transition: opacity 0.15s, transform 0.15s; }
.cal-drop-enter-from, .cal-drop-leave-to       { opacity: 0; transform: translateY(-4px); }

/* Botón limpiar — misma altura que inputs */
.btn-limpiar {
  height: 38px;
  padding: 0 16px;
  border-radius: 8px;
  border: 1.5px solid var(--borde);
  background: transparent;
  color: var(--texto-sec);
  font-size: 12px;
  font-weight: 700;
  cursor: pointer;
  white-space: nowrap;
  transition: all 0.18s;
  font-family: inherit;
}

.btn-limpiar--activo          { border-color: var(--verde); color: var(--verde); }
.btn-limpiar:hover            { background: var(--verde); color: var(--blanco); border-color: var(--verde); }

/* ── Estado vacío ──────────────────────────────────── */
.empty-state {
  text-align: center;
  padding: 72px 24px;
  background: var(--blanco);
  border-radius: 14px;
  border: 1px solid var(--borde);
}

.empty-title { font-size: 16px; font-weight: 700; color: var(--texto); margin-bottom: 6px; }
.empty-sub   { font-size: 13px; color: var(--texto-sec); }

/* ── Tabla ─────────────────────────────────────────── */
.table-wrapper {
  background: var(--blanco);
  border-radius: 14px;
  border: 1px solid var(--borde);
  overflow: hidden;
}

.table-scroll          { overflow-x: auto; -webkit-overflow-scrolling: touch; }

.don-table             { width: 100%; border-collapse: collapse; min-width: 680px; }
.don-table thead tr    { background: var(--verde); }
.don-table thead th    { padding: 13px 16px; text-align: left; color: var(--blanco); font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.6px; white-space: nowrap; }
.don-table tbody tr    { border-bottom: 1px solid var(--borde); transition: background 0.15s; }
.don-table tbody tr:last-child { border-bottom: none; }
.don-table tbody tr:hover      { background: #F4F6F4; }
.don-table tbody td    { padding: 13px 16px; vertical-align: middle; }

.id-pill    { font-size: 11px; font-family: monospace; background: var(--fondo); border: 1px solid var(--borde); padding: 3px 9px; border-radius: 6px; color: var(--verde); font-weight: 700; white-space: nowrap; }
.donor-name { display: block; font-size: 13px; font-weight: 700; color: var(--texto); line-height: 1.3; }
.donor-mail { display: block; font-size: 11px; color: var(--texto-sec); margin-top: 2px; }
.metodo-text { font-size: 13px; color: var(--texto-sec); }
.monto-text  { font-size: 13px; font-weight: 700; color: var(--verde); }
.fecha-text  { font-size: 13px; color: var(--texto-sec); white-space: nowrap; }

.table-footer { padding: 12px 16px; border-top: 1px solid var(--borde); font-size: 12px; color: var(--texto-sec); font-weight: 500; }

.btn-ver {
  padding: 6px 14px;
  border-radius: 7px;
  border: 1.5px solid var(--borde);
  background: var(--blanco);
  color: var(--verde);
  font-size: 12px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.18s;
  white-space: nowrap;
  font-family: inherit;
}

.btn-ver:hover { background: var(--verde); color: var(--blanco); border-color: var(--verde); }

/* ── Badges ────────────────────────────────────────── */
.estado-badge    { display: inline-block; font-size: 11px; font-weight: 700; padding: 4px 12px; border-radius: 20px; white-space: nowrap; }
.badge-pendiente { background: #FFF7E0; color: #96650A; }
.badge-aprobada  { background: #E8F5E9; color: #2E7D32; }
.badge-rechazada { background: #FDECEA; color: #B71C1C; }

/* ── Modal ─────────────────────────────────────────── */
.modal-overlay {
  position: fixed; inset: 0;
  background: rgba(0,0,0,0.35);
  backdrop-filter: blur(4px);
  z-index: 1000;
  display: flex; align-items: center; justify-content: center;
  padding: 24px;
}

.modal-box {
  background: #FFFFFF;
  border-radius: 20px;
  padding: 36px;
  width: 100%; max-width: 620px;
  max-height: 90vh; overflow-y: auto;
  position: relative;
}

.modal-close {
  position: absolute; top: 18px; right: 18px;
  width: 32px; height: 32px; border-radius: 50%;
  border: none; background: var(--fondo);
  color: var(--texto); font-size: 13px; font-weight: 700;
  cursor: pointer; display: flex; align-items: center; justify-content: center;
  transition: all 0.2s; font-family: inherit;
}
.modal-close:hover { background: var(--verde); color: var(--blanco); }

.modal-header { display: flex; align-items: center; gap: 10px; margin-bottom: 28px; }

.modal-id {
  font-size: 13px; font-family: monospace;
  background: var(--fondo); border: 1px solid var(--borde);
  padding: 5px 11px; border-radius: 7px;
  color: var(--verde); font-weight: 700;
}

.modal-section       { margin-bottom: 24px; }
.modal-section-title {
  font-size: 11px; font-weight: 700; color: var(--texto-sec);
  text-transform: uppercase; letter-spacing: 0.5px;
  margin-bottom: 14px; padding-bottom: 10px;
  border-bottom: 1px solid var(--borde);
}

.modal-grid   { display: grid; grid-template-columns: repeat(2,1fr); gap: 16px; }
.modal-field  { display: flex; flex-direction: column; gap: 4px; }
.modal-field-label { font-size: 10px; font-weight: 700; color: #9CA8A0; text-transform: uppercase; letter-spacing: 0.4px; }
.modal-field-value { font-size: 14px; color: var(--texto); font-weight: 600; word-break: break-word; }
.monto-highlight { font-size: 17px; color: var(--verde); font-weight: 800; }

.modal-mensaje {
  font-size: 14px; color: var(--texto); line-height: 1.7;
  background: var(--fondo); border-radius: 10px; padding: 14px 16px;
}

.sin-comprobante { font-size: 13px; color: #9CA8A0; font-style: italic; }

.comprobante-img-wrap { display: flex; flex-direction: column; align-items: flex-start; gap: 8px; }

.comprobante-thumb {
  border-radius: 10px; border: 1px solid var(--borde);
  max-width: 200px; max-height: 200px; object-fit: cover;
  cursor: zoom-in; transition: all 0.3s ease;
}
.comprobante-thumb.ampliada { max-width: 100%; max-height: 460px; cursor: zoom-out; }
.comprobante-hint { font-size: 11px; color: #9CA8A0; }

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
  display: flex; gap: 10px;
  padding-top: 24px; border-top: 1px solid var(--borde); margin-top: 8px;
}

.btn-aprobar {
  flex: 1; padding: 13px; border-radius: 10px; border: none;
  background: #E8F5E9; color: #2E7D32;
  font-size: 13px; font-weight: 700; cursor: pointer;
  transition: all 0.2s; font-family: inherit;
}
.btn-aprobar:hover { background: #2E7D32; color: var(--blanco); }

.btn-rechazar {
  flex: 1; padding: 13px; border-radius: 10px; border: none;
  background: #FDECEA; color: #B71C1C;
  font-size: 13px; font-weight: 700; cursor: pointer;
  transition: all 0.2s; font-family: inherit;
}
.btn-rechazar:hover { background: #B71C1C; color: var(--blanco); }

.modal-estado-final { padding-top: 20px; border-top: 1px solid var(--borde); text-align: center; }
.estado-aprobada-msg  { color: #2E7D32; font-weight: 700; font-size: 14px; }
.estado-rechazada-msg { color: #B71C1C; font-weight: 700; font-size: 14px; }

/* ── Animaciones ───────────────────────────────────── */
.modal-fade-enter-active, .modal-fade-leave-active { transition: opacity 0.22s ease; }
.modal-fade-enter-from, .modal-fade-leave-to       { opacity: 0; }

/* ── Responsive ────────────────────────────────────── */
@media (max-width: 900px) {
  .don-summary { display: grid; grid-template-columns: repeat(2,1fr); }
  .aprobadas   { grid-column: span 2; }
}

@media (max-width: 640px) {
  .filtros-panel      { flex-direction: column; }
  .filtro-group       { min-width: 100%; }
  .filtro-group--btn  { width: 100%; }
  .btn-limpiar        { width: 100%; justify-content: center; }
  .modal-grid         { grid-template-columns: 1fr; }
  .modal-box          { padding: 24px 20px; }
  .modal-acciones     { flex-direction: column; }
  .don-summary        { grid-template-columns: 1fr; }
  .aprobadas          { grid-column: span 1; }
}


/* ── MOBILE RESPONSIVE ── */
@media (max-width: 768px) {
  .don-summary {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 10px;
  }

  .aprobadas {
    grid-column: span 2;
  }

  .filtros-panel {
    flex-direction: column;
    gap: 10px;
    padding: 14px;
  }

  .filtro-group {
    min-width: unset;
    width: 100%;
  }

  .filtro-group--btn {
    width: 100%;
  }

  .btn-limpiar {
    width: 100%;
    justify-content: center;
  }

  .cal-dropdown {
    min-width: 200px;
    width: 100%;
    max-width: 280px;
  }

  .table-scroll {
    overflow-x: auto;
    -webkit-overflow-scrolling: touch;
  }

  .modal-box {
    padding: 24px 16px;
    max-height: 95vh;
  }

  .modal-grid {
    grid-template-columns: 1fr;
  }

  .modal-acciones {
    flex-direction: column;
  }
}

@media (max-width: 480px) {
  .don-summary {
    grid-template-columns: 1fr;
  }

  .aprobadas {
    grid-column: span 1;
  }
}


</style>
