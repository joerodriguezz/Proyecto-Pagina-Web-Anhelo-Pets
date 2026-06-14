<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'

// ─── Emits ────────────────────────────────────────────────────
// Emite { tipo, desde, hasta } cada vez que se aplica un filtro.
// tipo: 'hoy' | '7dias' | '30dias' | 'mes' | 'anio' | 'rango' | 'mesAnio'
const emit = defineEmits(['cambio'])

// ─── Estado original (API conservada) ────────────────────────
const calAbierto = ref(false)
const calYear    = ref(new Date().getFullYear())
const fechaMes   = ref(new Date().getMonth()) // 0-11
const fechaAño   = ref(new Date().getFullYear())

const MESES = [
  'enero', 'febrero', 'marzo', 'abril', 'mayo', 'junio',
  'julio', 'agosto', 'septiembre', 'octubre', 'noviembre', 'diciembre'
]

const DIAS_SEMANA = ['lu', 'ma', 'mi', 'ju', 'vi', 'sa', 'do']

// ─── Etiqueta del botón cerrado (API conservada) ──────────────
const fechaLabel = computed(() => {
  if (rangoActivo.value === 'hoy')      return 'Hoy'
  if (rangoActivo.value === '7dias')    return 'Últimos 7 días'
  if (rangoActivo.value === '30dias')   return 'Últimos 30 días'
  if (rangoActivo.value === 'anio')     return `Año ${fechaAño.value}`
  if (rangoActivo.value === 'rango' && rangoInicio.value && rangoFin.value) {
    return `${formatCorta(rangoInicio.value)} – ${formatCorta(rangoFin.value)}`
  }
  return `${MESES[fechaMes.value]} ${fechaAño.value}`
})

function formatCorta(d) {
  return d.toLocaleDateString('es-CR', { day: '2-digit', month: 'short' })
}

// ─── Toggle del panel (API conservada) ────────────────────────
function toggleCal() {
  calAbierto.value = !calAbierto.value
}

function cerrarCal() {
  calAbierto.value = false
}

// ─── Selección de mes (API conservada, ahora vía dropdown) ────
function seleccionarMes(indiceMes) {
  fechaMes.value = indiceMes
  rangoActivo.value = 'mesAnio'
  mostrarSelectorMes.value = false
}

// ─── Navegación de año del calendario ─────────────────────────
function añoAnterior() {
  calYear.value -= 1
}
function añoSiguiente() {
  calYear.value += 1
}

// ─── Rango activo / accesos rápidos ───────────────────────────
const rangoActivo  = ref('mesAnio')
const rangoInicio  = ref(null)
const rangoFin     = ref(null)
const seleccionandoRango = ref(false)

const mostrarSelectorMes = ref(false)

const ACCESOS_RAPIDOS = [
  { id: 'hoy',    label: 'Hoy' },
  { id: '7dias',  label: 'Últimos 7 días' },
  { id: '30dias', label: 'Últimos 30 días' },
  { id: 'mes',    label: 'Este mes' },
  { id: 'anio',   label: 'Este año' },
]

function aplicarAcceso(id) {
  const hoy = new Date()
  let desde, hasta

  if (id === 'hoy') {
    desde = hasta = new Date(hoy)
  } else if (id === '7dias') {
    desde = new Date(hoy); desde.setDate(desde.getDate() - 6)
    hasta = new Date(hoy)
  } else if (id === '30dias') {
    desde = new Date(hoy); desde.setDate(desde.getDate() - 29)
    hasta = new Date(hoy)
  } else if (id === 'mes') {
    desde = new Date(hoy.getFullYear(), hoy.getMonth(), 1)
    hasta = new Date(hoy.getFullYear(), hoy.getMonth() + 1, 0)
    fechaMes.value = hoy.getMonth()
    fechaAño.value = hoy.getFullYear()
    calYear.value  = hoy.getFullYear()
  } else if (id === 'anio') {
    desde = new Date(hoy.getFullYear(), 0, 1)
    hasta = new Date(hoy.getFullYear(), 11, 31)
    fechaAño.value = hoy.getFullYear()
    calYear.value  = hoy.getFullYear()
  }

  rangoActivo.value = id
  rangoInicio.value = desde
  rangoFin.value    = hasta
  seleccionandoRango.value = false

  emitirCambio()
  cerrarCal()
}

function iniciarRangoPersonalizado() {
  rangoActivo.value = 'rango'
  rangoInicio.value = null
  rangoFin.value    = null
  seleccionandoRango.value = true
}

// ─── Calendario ────────────────────────────────────────────────
const diasDelMesActual = computed(() => {
  const primerDia = new Date(calYear.value, fechaMes.value, 1)
  const ultimoDia = new Date(calYear.value, fechaMes.value + 1, 0)

  // Lunes = 0 ... Domingo = 6
  let offsetInicio = primerDia.getDay() - 1
  if (offsetInicio < 0) offsetInicio = 6

  const dias = []

  // Días del mes anterior (relleno)
  const ultimoDiaMesAnterior = new Date(calYear.value, fechaMes.value, 0).getDate()
  for (let i = offsetInicio - 1; i >= 0; i--) {
    dias.push({
      numero: ultimoDiaMesAnterior - i,
      fuera: true,
      fecha: new Date(calYear.value, fechaMes.value - 1, ultimoDiaMesAnterior - i)
    })
  }

  // Días del mes actual
  for (let d = 1; d <= ultimoDia.getDate(); d++) {
    dias.push({
      numero: d,
      fuera: false,
      fecha: new Date(calYear.value, fechaMes.value, d)
    })
  }

  // Relleno final hasta completar semanas de 7
  let diaSiguiente = 1
  while (dias.length % 7 !== 0) {
    dias.push({
      numero: diaSiguiente,
      fuera: true,
      fecha: new Date(calYear.value, fechaMes.value + 1, diaSiguiente)
    })
    diaSiguiente++
  }

  return dias
})

function esHoy(fecha) {
  const hoy = new Date()
  return fecha.toDateString() === hoy.toDateString()
}

function esSeleccionado(fecha) {
  if (!rangoInicio.value) return false
  if (!rangoFin.value) return fecha.toDateString() === rangoInicio.value.toDateString()
  return fecha >= rangoInicio.value && fecha <= rangoFin.value
}

function esExtremo(fecha) {
  if (rangoInicio.value && fecha.toDateString() === rangoInicio.value.toDateString()) return 'inicio'
  if (rangoFin.value && fecha.toDateString() === rangoFin.value.toDateString()) return 'fin'
  return null
}

function clickDia(dia) {
  rangoActivo.value = 'rango'

  if (!rangoInicio.value || (rangoInicio.value && rangoFin.value)) {
    rangoInicio.value = dia.fecha
    rangoFin.value    = null
    return
  }

  if (dia.fecha < rangoInicio.value) {
    rangoFin.value    = rangoInicio.value
    rangoInicio.value = dia.fecha
  } else {
    rangoFin.value = dia.fecha
  }
}

// ─── Acciones del panel ────────────────────────────────────────
function cancelar() {
  cerrarCal()
}

function aplicar() {
  emitirCambio()
  cerrarCal()
}

function emitirCambio() {
  let desde = rangoInicio.value
  let hasta = rangoFin.value

  if (rangoActivo.value === 'mesAnio') {
    desde = new Date(fechaAño.value, fechaMes.value, 1)
    hasta = new Date(fechaAño.value, fechaMes.value + 1, 0)
  }

  emit('cambio', {
    tipo: rangoActivo.value,
    desde,
    hasta
  })
}

const resumenRango = computed(() => {
  if (rangoInicio.value && rangoFin.value) {
    return `${formatCorta(rangoInicio.value)} ${rangoInicio.value.getFullYear()} → ${formatCorta(rangoFin.value)} ${rangoFin.value.getFullYear()}`
  }
  if (rangoInicio.value) return `${formatCorta(rangoInicio.value)} ${rangoInicio.value.getFullYear()} → seleccione fecha final`
  return 'Seleccione un rango de fechas'
})

// ─── Años disponibles para el selector ────────────────────────
const añosDisponibles = computed(() => {
  const actual = new Date().getFullYear()
  const años = []
  for (let y = actual - 4; y <= actual + 1; y++) años.push(y)
  return años
})

const mostrarSelectorAño = ref(false)

function seleccionarAño(y) {
  calYear.value  = y
  fechaAño.value = y
  mostrarSelectorAño.value = false
}

// ─── Cerrar al hacer click fuera ───────────────────────────────
const root = ref(null)

function onDocClick(e) {
  if (root.value && !root.value.contains(e.target)) {
    cerrarCal()
    mostrarSelectorMes.value = false
    mostrarSelectorAño.value = false
  }
}

onMounted(() => document.addEventListener('click', onDocClick))
onUnmounted(() => document.removeEventListener('click', onDocClick))
</script>

<template>
  <div class="filtro-fecha" ref="root">

    <!-- ── BOTÓN CERRADO ── -->
    <button class="filtro-btn" @click="toggleCal" :class="{ 'filtro-btn--activo': calAbierto }">
      <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="4" width="18" height="18" rx="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>
      <span class="filtro-btn-label">{{ fechaLabel }}</span>
      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="filtro-chevron" :class="{ 'filtro-chevron--abierto': calAbierto }"><polyline points="6 9 12 15 18 9"/></svg>
    </button>

    <!-- ── PANEL DESPLEGABLE ── -->
    <transition name="cal-fade">
      <div v-if="calAbierto" class="cal-dropdown">

        <!-- Columna de accesos rápidos -->
        <div class="cal-rapidos">
          <span class="cal-rapidos-titulo">Rangos rápidos</span>
          <button
            v-for="acc in ACCESOS_RAPIDOS"
            :key="acc.id"
            class="cal-rapido-item"
            :class="{ 'cal-rapido-item--activo': rangoActivo === acc.id }"
            @click="aplicarAcceso(acc.id)"
          >
            {{ acc.label }}
          </button>
          <button
            class="cal-rapido-item cal-rapido-item--rango"
            :class="{ 'cal-rapido-item--activo': rangoActivo === 'rango' }"
            @click="iniciarRangoPersonalizado"
          >
            Rango personalizado
          </button>
        </div>

        <!-- Calendario real -->
        <div class="cal-cuerpo">

          <div class="cal-controles">
            <!-- Selector de mes -->
            <div class="cal-select-wrap">
              <button class="cal-select-btn" @click="mostrarSelectorMes = !mostrarSelectorMes">
                {{ MESES[fechaMes] }}
                <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
              </button>
              <div v-if="mostrarSelectorMes" class="cal-select-menu cal-mes-grid">
                <button
                  v-for="(m, i) in MESES"
                  :key="m"
                  class="cal-mes-btn"
                  :class="{ 'cal-mes-btn--activo': i === fechaMes }"
                  @click="seleccionarMes(i)"
                >
                  {{ m.slice(0, 3) }}
                </button>
              </div>
            </div>

            <!-- Selector de año con flechas -->
            <div class="cal-anio-nav">
              <button class="cal-nav-flecha" @click="añoAnterior" aria-label="Año anterior">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="15 18 9 12 15 6"/></svg>
              </button>

              <div class="cal-select-wrap">
                <button class="cal-select-btn" @click="mostrarSelectorAño = !mostrarSelectorAño">
                  {{ calYear }}
                  <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
                </button>
                <div v-if="mostrarSelectorAño" class="cal-select-menu cal-anio-lista">
                  <button
                    v-for="y in añosDisponibles"
                    :key="y"
                    class="cal-mes-btn"
                    :class="{ 'cal-mes-btn--activo': y === calYear }"
                    @click="seleccionarAño(y)"
                  >
                    {{ y }}
                  </button>
                </div>
              </div>

              <button class="cal-nav-flecha" @click="añoSiguiente" aria-label="Año siguiente">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="9 18 15 12 9 6"/></svg>
              </button>
            </div>
          </div>

          <!-- Cabecera días de semana -->
          <div class="cal-grid cal-grid-header">
            <span v-for="d in DIAS_SEMANA" :key="d" class="cal-dia-nombre">{{ d }}</span>
          </div>

          <!-- Días del mes -->
          <div class="cal-grid">
            <button
              v-for="(dia, i) in diasDelMesActual"
              :key="i"
              class="cal-dia"
              :class="[
                { 'cal-dia--fuera': dia.fuera },
                { 'cal-dia--hoy': esHoy(dia.fecha) && !dia.fuera },
                { 'cal-dia--seleccionado': esSeleccionado(dia.fecha) },
                esExtremo(dia.fecha) === 'inicio' ? 'cal-dia--inicio' : '',
                esExtremo(dia.fecha) === 'fin' ? 'cal-dia--fin' : ''
              ]"
              @click="clickDia(dia)"
            >
              {{ dia.numero }}
            </button>
          </div>

          <!-- Pie: resumen + acciones -->
          <div class="cal-footer">
            <span class="cal-resumen">{{ resumenRango }}</span>
            <div class="cal-acciones">
              <button class="cal-btn cal-btn--ghost" @click="cancelar">Cancelar</button>
              <button class="cal-btn cal-btn--primario" @click="aplicar">Aplicar</button>
            </div>
          </div>

        </div>
      </div>
    </transition>
  </div>
</template>

<style scoped>
.filtro-fecha {
  position: relative;
  font-family: inherit;
  --verde:       #3A473C;
  --verde-sec:   #92A894;
  --fondo:       #F7F8F7;
  --blanco:      #FFFFFF;
  --texto:       #2F352F;
  --texto-sec:   #6C756D;
  --texto-ter:   #9CA39B;
  --borde:       #E8ECE8;
  --amarillo:    #F5B942;
  --info-bg:     #EAF1EA;
  --info-text:   #3A473C;
  --info-border: #92A894;
}

/* ── Botón cerrado ── */
.filtro-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
  font-weight: 600;
  color: var(--texto);
  background: var(--blanco);
  border: 1px solid var(--borde);
  border-radius: 8px;
  padding: 8px 14px;
  cursor: pointer;
  white-space: nowrap;
  transition: border-color .15s, background .15s;
}
.filtro-btn:hover { border-color: var(--verde-sec); }
.filtro-btn--activo {
  border-color: var(--verde-sec);
  background: rgba(146,168,148,.08);
}
.filtro-btn svg:first-child { color: var(--texto-sec); flex-shrink: 0; }
.filtro-btn-label { text-transform: capitalize; }
.filtro-chevron { color: var(--texto-sec); flex-shrink: 0; transition: transform .15s; }
.filtro-chevron--abierto { transform: rotate(180deg); }

/* ── Panel ── */
.cal-dropdown {
  position: absolute;
  top: calc(100% + 8px);
  left: 0;
  z-index: 30;
  display: flex;
  background: var(--blanco);
  border: 1px solid var(--borde);
  border-radius: 12px;
  box-shadow: 0 8px 24px rgba(58,71,60,.12);
  overflow: hidden;
  min-width: 560px;
}

.cal-fade-enter-active, .cal-fade-leave-active { transition: opacity .12s, transform .12s; }
.cal-fade-enter-from, .cal-fade-leave-to { opacity: 0; transform: translateY(-4px); }

/* ── Columna accesos rápidos ── */
.cal-rapidos {
  width: 168px;
  flex-shrink: 0;
  border-right: 1px solid var(--borde);
  padding: 12px;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.cal-rapidos-titulo {
  font-size: 11px;
  font-weight: 700;
  color: var(--texto-ter);
  text-transform: uppercase;
  letter-spacing: .5px;
  padding: 6px 10px 8px;
}

.cal-rapido-item {
  display: block;
  width: 100%;
  text-align: left;
  font-size: 13px;
  font-weight: 600;
  color: var(--texto);
  background: transparent;
  border: none;
  border-radius: 8px;
  padding: 8px 10px;
  cursor: pointer;
  transition: background .12s;
}
.cal-rapido-item:hover { background: var(--fondo); }
.cal-rapido-item--activo {
  background: rgba(146,168,148,.18);
  color: var(--verde);
}
.cal-rapido-item--rango {
  margin-top: 6px;
  border: 1px solid var(--borde);
}
.cal-rapido-item--rango.cal-rapido-item--activo {
  border-color: var(--verde-sec);
}

/* ── Cuerpo calendario ── */
.cal-cuerpo {
  flex: 1;
  padding: 14px;
  min-width: 360px;
}

.cal-controles {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 12px;
  gap: 8px;
}

.cal-select-wrap { position: relative; }

.cal-select-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 12px;
  font-weight: 600;
  color: var(--texto);
  background: var(--blanco);
  border: 1px solid var(--borde);
  border-radius: 8px;
  padding: 6px 10px;
  cursor: pointer;
  text-transform: capitalize;
  transition: border-color .15s;
}
.cal-select-btn:hover { border-color: var(--verde-sec); }
.cal-select-btn svg { color: var(--texto-sec); }

.cal-select-menu {
  position: absolute;
  top: calc(100% + 6px);
  left: 0;
  z-index: 10;
  background: var(--blanco);
  border: 1px solid var(--borde);
  border-radius: 10px;
  box-shadow: 0 6px 18px rgba(58,71,60,.14);
  padding: 8px;
}

.cal-mes-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 4px;
  width: 168px;
}

.cal-anio-lista {
  display: flex;
  flex-direction: column;
  gap: 2px;
  width: 90px;
  max-height: 180px;
  overflow-y: auto;
}

.cal-mes-btn {
  font-size: 12px;
  font-weight: 600;
  color: var(--texto);
  background: transparent;
  border: none;
  border-radius: 6px;
  padding: 7px 8px;
  cursor: pointer;
  text-transform: capitalize;
  transition: background .12s;
}
.cal-mes-btn:hover { background: var(--fondo); }
.cal-mes-btn--activo {
  background: var(--verde-sec);
  color: var(--blanco);
}

.cal-anio-nav {
  display: flex;
  align-items: center;
  gap: 6px;
}

.cal-nav-flecha {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  border: 1px solid var(--borde);
  border-radius: 8px;
  background: var(--blanco);
  color: var(--texto-sec);
  cursor: pointer;
  transition: border-color .15s, color .15s;
}
.cal-nav-flecha:hover { border-color: var(--verde-sec); color: var(--verde); }

/* ── Grid de días ── */
.cal-grid {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 2px;
}

.cal-grid-header { margin-bottom: 4px; }

.cal-dia-nombre {
  text-align: center;
  font-size: 10px;
  font-weight: 700;
  color: var(--texto-ter);
  text-transform: uppercase;
  letter-spacing: .4px;
  padding: 4px 0;
}

.cal-dia {
  aspect-ratio: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: 600;
  color: var(--texto);
  background: transparent;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: background .12s;
}
.cal-dia:hover:not(.cal-dia--fuera) { background: var(--fondo); }

.cal-dia--fuera { color: var(--texto-ter); font-weight: 500; }

.cal-dia--hoy {
  border: 1.5px solid var(--verde-sec);
  border-radius: 50%;
  color: var(--verde);
}

.cal-dia--seleccionado {
  background: rgba(146,168,148,.18);
  border-radius: 0;
  color: var(--verde);
}

.cal-dia--inicio,
.cal-dia--fin {
  background: var(--verde-sec);
  color: var(--blanco);
}
.cal-dia--inicio { border-radius: 8px 0 0 8px; }
.cal-dia--fin    { border-radius: 0 8px 8px 0; }
.cal-dia--inicio.cal-dia--fin { border-radius: 8px; }

/* ── Pie del panel ── */
.cal-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-top: 14px;
  padding-top: 12px;
  border-top: 1px solid var(--borde);
}

.cal-resumen {
  font-size: 12px;
  color: var(--texto-sec);
  font-weight: 500;
}

.cal-acciones { display: flex; gap: 8px; }

.cal-btn {
  font-size: 12px;
  font-weight: 700;
  border-radius: 8px;
  padding: 7px 14px;
  cursor: pointer;
  border: 1px solid transparent;
  transition: all .15s;
}

.cal-btn--ghost {
  background: var(--blanco);
  border-color: var(--borde);
  color: var(--texto-sec);
}
.cal-btn--ghost:hover { border-color: var(--verde-sec); color: var(--verde); }

.cal-btn--primario {
  background: var(--verde);
  border-color: var(--verde);
  color: var(--blanco);
}
.cal-btn--primario:hover { background: #2F3A31; }

/* ── Responsive: bottom sheet en móvil ── */
@media (max-width: 640px) {
  .cal-dropdown {
    position: fixed;
    top: auto;
    left: 0;
    right: 0;
    bottom: 0;
    min-width: 0;
    width: 100%;
    border-radius: 16px 16px 0 0;
    flex-direction: column;
    max-height: 85vh;
    overflow-y: auto;
    box-shadow: 0 -8px 24px rgba(58,71,60,.18);
  }

  .cal-rapidos {
    width: 100%;
    border-right: none;
    border-bottom: 1px solid var(--borde);
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 6px;
  }
  .cal-rapidos-titulo { display: none; }
  .cal-rapido-item { text-align: center; }
  .cal-rapido-item--rango { grid-column: span 2; }

  .cal-cuerpo { min-width: 0; }

  .cal-footer { flex-direction: column-reverse; align-items: stretch; gap: 10px; }
  .cal-acciones { display: grid; grid-template-columns: 1fr 1fr; }
  .cal-resumen { text-align: center; }
}
</style>