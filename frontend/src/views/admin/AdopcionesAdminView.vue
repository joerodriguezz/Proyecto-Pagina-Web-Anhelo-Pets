<script setup>
import { ref, computed, onMounted } from 'vue'
import { usePetsStore } from '../../stores/usePetsStore'
import { registrarAuditoria } from '../../composables/useAuditLog'
import {
  getAdoptionRequests,
  updateAdoptionRequestStatus,
  mapAdoptionRequestDtoToRow,
} from '../../services/adoptionServices'


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

// ─── Solicitudes (conectadas a la BD vía adoptionServices) ──────────────────
const solicitudes = ref([])
const cargando    = ref(false)
const errorCarga  = ref('')

async function cargarSolicitudes() {
  cargando.value = true
  errorCarga.value = ''
  try {
    const { data } = await getAdoptionRequests()
    solicitudes.value = (data || []).map(mapAdoptionRequestDtoToRow)
  } catch (e) {
    console.error('No se pudieron cargar las solicitudes de adopción:', e)
    errorCarga.value = 'No se pudieron cargar las solicitudes de adopción.'
    solicitudes.value = []
  } finally {
    cargando.value = false
  }
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

// ─── Acciones (ahora contra la BD, con auditoría de éxito y de fallo) ───────
function sincronizarMascota(solicitud, nuevoEstadoMascota) {
  const mascota = solicitud.petId
    ? store.pets.find(p => p.id === solicitud.petId)
    : store.pets.find(p => p.name === solicitud.mascota)
  if (mascota) store.changeStatus(mascota.id, nuevoEstadoMascota)
}

// accionEnCurso = { id, tipo } de la solicitud/acción que está en vuelo,
// para poder mostrar el spinner en el ícono correcto de la fila correcta.
const accionEnCurso = ref(null)

async function procesoSolicitud(id) {
  const s = solicitudes.value.find(s => s.id === id)
  if (!s) return
  const estadoAnterior = s.estado
  accionEnCurso.value = { id, tipo: 'proceso' }
  try {
    await updateAdoptionRequestStatus(id, 'Proceso')
    s.estado = 'En proceso'
    sincronizarMascota(s, 'En proceso')
    registrarAuditoria({
      modulo: 'Adopciones', accion: 'Puso en proceso una solicitud de adopción', tipoAccion: 'estado',
      elemento: s.mascota, elementoId: s.id,
      descripcion: `Solicitud de ${s.solicitante} para "${s.mascota}" pasó de "${estadoAnterior}" a "En proceso".`,
      valoresAnteriores: { estado: estadoAnterior },
      valoresNuevos: { estado: 'En proceso' },
    })
  } catch (e) {
    console.error('No se pudo poner en proceso la solicitud:', e)
    registrarAuditoria({
      modulo: 'Adopciones', accion: 'Puso en proceso una solicitud de adopción', tipoAccion: 'estado',
      elemento: s.mascota, elementoId: s.id,
      descripcion: `Falló el intento de pasar a "En proceso" la solicitud de ${s.solicitante} para "${s.mascota}".`,
      estado: 'Fallido',
    })
  }
  accionEnCurso.value = null
}

async function aprobarSolicitud(id) {
  const s = solicitudes.value.find(s => s.id === id)
  if (!s) return
  const estadoAnterior = s.estado
  accionEnCurso.value = { id, tipo: 'aprobar' }
  try {
    await updateAdoptionRequestStatus(id, 'Aprobar')
    s.estado = 'Aprobada'
    sincronizarMascota(s, 'Adoptada')
    registrarAuditoria({
      modulo: 'Adopciones', accion: 'Aprobó una solicitud de adopción', tipoAccion: 'aprobar',
      elemento: s.mascota, elementoId: s.id,
      descripcion: `Se aprobó la adopción de "${s.mascota}" para ${s.solicitante}.`,
      valoresAnteriores: { estado: estadoAnterior },
      valoresNuevos: { estado: 'Aprobada' },
    })
  } catch (e) {
    console.error('No se pudo aprobar la solicitud:', e)
    registrarAuditoria({
      modulo: 'Adopciones', accion: 'Aprobó una solicitud de adopción', tipoAccion: 'aprobar',
      elemento: s.mascota, elementoId: s.id,
      descripcion: `Falló el intento de aprobar la adopción de "${s.mascota}" para ${s.solicitante}.`,
      estado: 'Fallido',
    })
  }
  accionEnCurso.value = null
}

async function rechazarSolicitud(id) {
  const s = solicitudes.value.find(s => s.id === id)
  if (!s) return
  const estadoAnterior = s.estado
  accionEnCurso.value = { id, tipo: 'rechazar' }
  try {
    await updateAdoptionRequestStatus(id, 'Rechazar')
    s.estado = 'Rechazada'
    sincronizarMascota(s, 'Disponible')
    registrarAuditoria({
      modulo: 'Adopciones', accion: 'Rechazó una solicitud de adopción', tipoAccion: 'rechazar',
      elemento: s.mascota, elementoId: s.id,
      descripcion: `Se rechazó la solicitud de ${s.solicitante} para "${s.mascota}".`,
      valoresAnteriores: { estado: estadoAnterior },
      valoresNuevos: { estado: 'Rechazada' },
    })
  } catch (e) {
    console.error('No se pudo rechazar la solicitud:', e)
    registrarAuditoria({
      modulo: 'Adopciones', accion: 'Rechazó una solicitud de adopción', tipoAccion: 'rechazar',
      elemento: s.mascota, elementoId: s.id,
      descripcion: `Falló el intento de rechazar la solicitud de ${s.solicitante} para "${s.mascota}".`,
      estado: 'Fallido',
    })
  }
  accionEnCurso.value = null
}

// ─── Modal ───────────────────────────────────────────────────────────────────
const showDetailModal = ref(false)
const selectedRequest = ref(null)
// Pestaña activa del expediente — únicamente controla qué bloque se muestra,
// mismo patrón presentacional que "expedienteTab" en el módulo de Mascotas.
const expedienteTab = ref('general')

function verDetalle(solicitud) {
  selectedRequest.value = solicitud
  expedienteTab.value = 'general'
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
            class="btn btn--ghost"
            :class="{ 'btn--ghost-active': filterStatus !== 'Todos' || filterSolicitante.trim() !== '' || filterMascota.trim() !== '' || filterFecha !== 'Todas' }"
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
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="s in filtered" :key="s.id" class="don-row">
              <td><span class="id-pill">{{ s.id }}</span></td>
              <td><span class="donor-name">{{ s.solicitante }}</span></td>
              <td><span class="type-chip">{{ s.mascota }}</span></td>
              <td><span class="fecha-text">{{ s.fecha }}</span></td>
              <td><span class="fecha-text">{{ s.telefono }}</span></td>
              <td><span class="estado-badge" :class="statusClass(s.estado)">{{ s.estado }}</span></td>
              <td>
                <!-- Acciones — mismo componente icon-only que Mascotas / Voluntarios -->
                <div class="action-group">
                  <button type="button" class="icon-only icon-only--ver" @click="verDetalle(s)" data-tooltip="Ver detalle">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
                  </button>

                  <button v-if="s.estado === 'Pendiente'" type="button" class="icon-only icon-only--revisar" :disabled="!!accionEnCurso" @click="procesoSolicitud(s.id)" data-tooltip="Poner en proceso">
                    <span v-if="accionEnCurso?.id === s.id && accionEnCurso?.tipo === 'proceso'" class="btn-spinner btn-spinner--dark"></span>
                    <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="9" y="2" width="6" height="4" rx="1"/><path d="M9 4H5a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6a2 2 0 0 0-2-2h-4"/><path d="M9 12l2 2 4-4"/></svg>
                  </button>

                  <template v-if="s.estado === 'En proceso'">
                    <button type="button" class="icon-only icon-only--activar" :disabled="!!accionEnCurso" @click="aprobarSolicitud(s.id)" data-tooltip="Aprobar">
                      <span v-if="accionEnCurso?.id === s.id && accionEnCurso?.tipo === 'aprobar'" class="btn-spinner btn-spinner--dark"></span>
                      <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
                    </button>
                    <button type="button" class="icon-only icon-only--inactivar" :disabled="!!accionEnCurso" @click="rechazarSolicitud(s.id)" data-tooltip="Rechazar">
                      <span v-if="accionEnCurso?.id === s.id && accionEnCurso?.tipo === 'rechazar'" class="btn-spinner btn-spinner--dark"></span>
                      <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
                    </button>
                  </template>
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

    <!-- ══════════════════════════════════════
         MODAL — VER EXPEDIENTE
         Mismo componente .modal-box--uniform (hero + tabs + bloques) que Mascotas / Voluntarios
    ══════════════════════════════════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showDetailModal && selectedRequest" class="modal-overlay" @click.self="showDetailModal = false">
          <div class="modal-box modal-box--uniform">
            <button type="button" class="close-btn close-btn--hero" @click="showDetailModal = false">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>

            <div class="hero">
              <div class="hero-photo">
                <span class="hero-photo-ini">{{ selectedRequest.solicitante?.charAt(0) }}</span>
              </div>
              <div class="hero-info">
                <div class="hero-name-row">
                  <h2 class="hero-name">{{ selectedRequest.solicitante }}</h2>
                  <span class="estado-badge badge-status-hero" :class="statusClass(selectedRequest.estado)">{{ selectedRequest.estado }}</span>
                </div>
                <div class="hero-meta">
                  <span class="hero-meta-chip">
                    <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/></svg>
                    {{ selectedRequest.mascota }}
                  </span>
                  <span class="hero-meta-chip">{{ selectedRequest.id }}</span>
                  <span class="hero-meta-chip">{{ selectedRequest.fecha }}</span>
                </div>
              </div>
            </div>

            <div class="tabs">
              <button type="button" class="tab" :class="{ active: expedienteTab === 'general' }" @click="expedienteTab = 'general'">General</button>
              <button type="button" class="tab" :class="{ active: expedienteTab === 'hogar' }" @click="expedienteTab = 'hogar'">Ubicación y hogar</button>
              <button type="button" class="tab" :class="{ active: expedienteTab === 'solicitud' }" @click="expedienteTab = 'solicitud'">Solicitud</button>
              <button type="button" class="tab" :class="{ active: expedienteTab === 'motivos' }" @click="expedienteTab = 'motivos'">Motivos</button>
            </div>

            <div class="uniform-scroll">
              <div class="body">

                <!-- TAB: General -->
                <template v-if="expedienteTab === 'general'">
                  <div class="block">
                    <h4 class="block-title">
                      <span class="block-title-icon">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
                      </span>
                      Datos personales
                    </h4>
                    <div class="fields-row">
                      <div class="field-col"><span class="field-label-row">Cédula</span><span class="field-value">{{ selectedRequest.cedula || '—' }}</span></div>
                      <div class="field-col"><span class="field-label-row">Edad</span><span class="field-value">{{ selectedRequest.edad || '—' }}</span></div>
                      <div class="field-col"><span class="field-label-row">Profesión</span><span class="field-value">{{ selectedRequest.profesion || '—' }}</span></div>
                    </div>
                  </div>
                  <div class="block">
                    <h4 class="block-title">
                      <span class="block-title-icon">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72c.127.96.361 1.903.7 2.81a2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45c.907.339 1.85.573 2.81.7A2 2 0 0 1 22 16.92z"/></svg>
                      </span>
                      Contacto
                    </h4>
                    <div class="fields-row">
                      <div class="field-col"><span class="field-label-row">Teléfono</span><span class="field-value">{{ selectedRequest.telefono || '—' }}</span></div>
                      <div class="field-col"><span class="field-label-row">WhatsApp</span><span class="field-value">{{ selectedRequest.whatsapp || '—' }}</span></div>
                      <div class="field-col"><span class="field-label-row">Correo</span><span class="field-value">{{ selectedRequest.email || '—' }}</span></div>
                    </div>
                  </div>
                </template>

                <!-- TAB: Ubicación y hogar -->
                <template v-if="expedienteTab === 'hogar'">
                  <div class="block">
                    <h4 class="block-title">
                      <span class="block-title-icon">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"/><circle cx="12" cy="10" r="3"/></svg>
                      </span>
                      Ubicación y hogar
                    </h4>
                    <div class="info-subsection" style="margin-top:0;padding-top:0;border-top:none;">
                      <span class="field-label-row">Ubicación</span>
                      <p class="info-subsection-text">{{ selectedRequest.direccion || '—' }}</p>
                    </div>
                    <div class="fields-row" style="margin-top:14px">
                      <div class="field-col"><span class="field-label-row">Personas del hogar</span><span class="field-value">{{ selectedRequest.hogar || '—' }}</span></div>
                      <div class="field-col"><span class="field-label-row">Otras mascotas</span><span class="field-value">{{ selectedRequest.otrasMascotas || '—' }}</span></div>
                      <div class="field-col"><span class="field-label-row">Horas sola</span><span class="field-value">{{ selectedRequest.horasSola || '—' }}</span></div>
                    </div>
                  </div>
                  <div class="block" v-if="selectedRequest.rutina">
                    <h4 class="block-title">
                      <span class="block-title-icon">
  <i class='bx bx-calendar'></i>
</span>
                      Rutina diaria
                    </h4>
                    <div class="tint-box tint-box--desc">
                      <span>{{ selectedRequest.rutina }}</span>
                    </div>
                  </div>
                </template>

                <!-- TAB: Solicitud -->
                <template v-if="expedienteTab === 'solicitud'">
                  <div class="block">
                    <h4 class="block-title">
                      <span class="block-title-icon">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="7" width="20" height="14" rx="2"/><path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"/></svg>
                      </span>
                      Estado de la solicitud
                    </h4>
                    <div class="fields-row">
                      <div class="field-col"><span class="field-label-row">Mascota</span><span class="field-value">{{ selectedRequest.mascota || '—' }}</span></div>
                      <div class="field-col"><span class="field-label-row">Fecha recibida</span><span class="field-value">{{ selectedRequest.fecha || '—' }}</span></div>
                      <div class="field-col"><span class="field-label-row">Estado</span><span class="estado-badge" :class="statusClass(selectedRequest.estado)">{{ selectedRequest.estado }}</span></div>
                    </div>
                  </div>
                </template>

                <!-- TAB: Motivos -->
                <template v-if="expedienteTab === 'motivos'">
                  <div class="block">
                    <h4 class="block-title">
                      <span class="block-title-icon">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/></svg>
                      </span>
                      ¿Por qué desea adoptar esta mascota?
                    </h4>
                    <div class="tint-box tint-box--desc">
                      <span>{{ selectedRequest.porqueMascota || '—' }}</span>
                    </div>
                  </div>
                  <div class="block">
                    <h4 class="block-title">
                      <span class="block-title-icon">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="9" y1="13" x2="15" y2="13"/><line x1="9" y1="17" x2="13" y2="17"/></svg>
                      </span>
                      Motivos de adopción
                    </h4>
                    <div class="tint-box tint-box--desc">
                      <span>{{ selectedRequest.motivos || '—' }}</span>
                    </div>
                  </div>
                </template>

              </div>
            </div>

            <div class="footer">
              <button type="button" class="btn-ghost-red" @click="showDetailModal = false">Cerrar expediente</button>
            </div>

          </div>
        </div>
      </Transition>
    </Teleport>

  </div>
</template>

<style scoped>
/* ── Variables (idénticas a Mascotas / Voluntarios) ─────────────────────── */
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
  background:
    radial-gradient(ellipse 800px 420px at 12% 0%, rgba(146,168,148,.07), transparent),
    var(--fondo);
  padding-bottom: 40px;
}

/* ── Sistema de botones (idéntico a Mascotas / Voluntarios) ── */
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
.pendiente-icon { background:#FDF6E8; border-color:#F2E1B8; color:#A97A0C; }
.proceso-icon   { background:#EEF1FB; border-color:#CBD5F2; color:#4F73B8; }
.aprobada-icon  { background:#EDF6EF; border-color:#C9E4CE; color:#2E7D45; }
.rechazada-icon { background:#FBEDEC; border-color:#F1C7C3; color:#B71C1C; }
.total-icon     { background:#F2F3F2; border-color:#DFE2DF; color:#616861; }
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
.tabs-wrap { display:flex; gap:3px; background:var(--fondo); border:1px solid var(--borde-suave); border-radius:10px; padding:3px; flex-wrap:wrap; }
.tab-btn { padding:7px 13px; border-radius:7px; border:none; background:transparent; color:var(--texto-sec); font-size:12px; font-weight:700; cursor:pointer; transition:all 0.18s; white-space:nowrap; font-family:inherit; }
.tab-btn:hover { color:var(--texto); }
.tab-btn.active { background:var(--blanco); color:var(--texto); box-shadow:var(--sombra-sm); border:1px solid var(--borde); }
.filtro-input-wrap { position:relative; display:flex; align-items:center; }
.filtro-input { width:100%; height:36px; padding:0 14px; border-radius:8px; border:1px solid var(--borde); background:var(--fondo); font-size:13px; color:var(--texto); font-family:inherit; outline:none; transition:border-color 0.18s, background 0.18s; box-sizing:border-box; }
.filtro-input:focus { border-color:var(--verde-sec); background:var(--blanco); }
.filtro-input::placeholder { color:var(--texto-ter); }
.filtro-input--icon-left { padding-left:36px; }
.filtro-icon { position:absolute; display:flex; align-items:center; color:var(--texto-sec); }
.filtro-icon--left { left:12px; }

/* ── Estado vacío ── */
.empty-state { text-align:center; padding:72px 24px; background:var(--blanco); border-radius:16px; border:1px solid var(--borde); color:var(--verde-sec); display:flex; flex-direction:column; align-items:center; gap:10px; }
.empty-state svg { opacity:0.4; }
.empty-title { font-size:16px; font-weight:700; color:var(--texto); margin:0; }
.empty-sub { font-size:13px; color:var(--texto-sec); margin:0; }

/* ── Tabla ── */
.table-wrapper { background:var(--blanco); border-radius:16px; border:1px solid var(--borde); overflow:hidden; box-shadow:var(--sombra-sm); }
.table-scroll { overflow-x:auto; -webkit-overflow-scrolling:touch; }
.don-table { width:100%; border-collapse:collapse; min-width:760px; }
.don-table thead th { padding:12px 16px; text-align:left; color:var(--texto-ter); font-size:9.5px; font-weight:700; text-transform:uppercase; letter-spacing:0.6px; white-space:nowrap; }
.don-table tbody tr { border-top:1px solid var(--borde-suave); transition:background 0.15s; }
.don-table tbody tr:hover { background:#FAFBFA; }
.don-table tbody td { padding:12px 16px; vertical-align:middle; }
.id-pill { font-size:11px; font-family:ui-monospace, Menlo, Consolas, monospace; background:var(--fondo); border:1px solid var(--borde); padding:3px 9px; border-radius:6px; color:var(--texto); font-weight:700; white-space:nowrap; }
.donor-name { display:block; font-size:12.5px; font-weight:700; color:var(--texto); line-height:1.3; }
.fecha-text { font-size:12.5px; color:var(--texto-sec); white-space:nowrap; }
.type-chip { display:inline-block; font-size:11.5px; font-weight:600; color:#4E6E51; background:#F1F5F1; padding:3px 10px; border-radius:7px; white-space:nowrap; }
.estado-badge { display:inline-block; font-size:10.5px; font-weight:700; padding:4px 11px; border-radius:20px; white-space:nowrap; }
.badge-pendiente { background:#FDF6E8; color:#96650A; }
.badge-proceso   { background:#EEF1FB; color:#4F73B8; }
.badge-aprobada  { background:#EDF6EF; color:#2E7D32; }
.badge-rechazada { background:#FBEDEC; color:#B71C1C; }
.badge-neutral   { background:#F2F3F2; color:#7A827B; }
.table-footer { padding:12px 16px; border-top:1px solid var(--borde-suave); font-size:12px; color:var(--texto-sec); font-weight:500; }

/* Botones de acción — mismo componente icon-only que Mascotas / Voluntarios */
.action-group { display:flex; gap:8px; align-items:center; }
.icon-only {
  width:38px; height:38px; border-radius:8px; border:1px solid var(--borde);
  background:var(--blanco); display:flex; align-items:center; justify-content:center;
  cursor:pointer; transition:background-color .16s ease, border-color .16s ease; position:relative;
  flex-shrink:0;
}
.icon-only svg { width:16px; height:16px; }
.icon-only:disabled { opacity:.5; cursor:not-allowed; }
.btn-spinner { display:inline-block; width:14px; height:14px; border:2px solid rgba(255,255,255,.4); border-top-color:#fff; border-radius:50%; animation:btn-spin .7s linear infinite; }
.btn-spinner--dark { border-color:var(--borde); border-top-color:var(--texto-sec); }
@keyframes btn-spin { to { transform:rotate(360deg); } }
.icon-only--ver { color:#3D453B; }
.icon-only--ver:hover { border-color:#C7D3C8; background:#FAFCFA; }
.icon-only--revisar { color:#4F73B8; border-color:#CBD5F2; }
.icon-only--revisar:hover { background:#EEF1FB; border-color:#4F73B8; }
.icon-only--activar { color:#2E7D45; border-color:#CFE8D6; }
.icon-only--activar:hover { background:#F3FAF5; border-color:#2E7D45; }
.icon-only--inactivar { color:#C0392B; border-color:#F0CFC9; }
.icon-only--inactivar:hover { background:#FDF4F3; border-color:#C0392B; }
.icon-only::before {
  content:attr(data-tooltip); position:absolute; bottom:calc(100% + 8px); left:50%;
  transform:translateX(-50%) translateY(4px); background:var(--verde); color:#fff;
  font-size:11px; font-weight:600; padding:5px 9px; border-radius:7px; white-space:nowrap;
  opacity:0; visibility:hidden; pointer-events:none; transition:opacity .15s ease, transform .15s ease; z-index:20;
}
.icon-only:hover::before { opacity:1; visibility:visible; transform:translateX(-50%) translateY(0); }

/* ══════════════════════════════════════════════
   MODAL BASE (idéntico a Mascotas / Voluntarios)
   ══════════════════════════════════════════════ */
.modal-overlay { position:fixed; inset:0; background:rgba(0,0,0,0.35); backdrop-filter:blur(4px); z-index:1000; display:flex; align-items:center; justify-content:center; padding:24px; }
.modal-box { background:var(--blanco); border-radius:22px; box-shadow:var(--sombra-md); position:relative; }
.modal-box--uniform {
  width:880px; max-width:92vw; height:660px; max-height:90vh;
  display:flex; flex-direction:column; overflow:hidden; border:1px solid var(--borde-suave);
}
.uniform-scroll { flex:1; min-height:0; overflow-y:auto; }
.close-btn {
  position:absolute; top:18px; right:18px; z-index:6;
  width:30px; height:30px; border-radius:9px; background:var(--fondo); border:1px solid var(--borde-suave);
  color:#8B928A; display:flex; align-items:center; justify-content:center; cursor:pointer;
  transition:background-color .16s ease, color .16s ease, border-color .16s ease;
}
.close-btn svg { width:16px; height:16px; }
.close-btn:hover { background:var(--verde); color:#fff; border-color:var(--verde); }
.close-btn--hero { background:var(--fondo); }
.close-btn--hero:hover { background:var(--verde); color:#fff; }

/* ── HERO (Ver expediente) ── */
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
.hero-photo-ini { font-size:20px; font-weight:700; color:#3E7A45; letter-spacing:-.3px; text-transform:uppercase; }
.hero-info { flex:1; min-width:0; display:flex; flex-direction:column; gap:8px; }
.hero-name-row { display:flex; align-items:center; gap:12px; flex-wrap:wrap; }
.hero-name { font-size:21px; font-weight:700; color:var(--texto); margin:0; letter-spacing:-.4px; }
.hero-meta { display:flex; align-items:center; gap:7px; flex-wrap:wrap; }
.hero-meta-chip {
  display:inline-flex; align-items:center; gap:6px; font-size:11.5px; font-weight:600; color:#4B5A4C;
  background:var(--blanco); border:1px solid var(--borde-suave); padding:4px 10px 4px 9px; border-radius:20px;
}
.hero-meta-chip svg { color:var(--verde-sec); flex-shrink:0; }
.badge-status-hero { padding:5px 12px !important; font-size:10.5px !important; }

/* ── TABS ── */
.tabs { flex-shrink:0; display:flex; gap:2px; padding:0 40px; border-bottom:1px solid var(--borde); overflow-x:auto; }
.tab { padding:11px 13px 9px; font-size:12px; font-weight:700; color:var(--texto-sec); border:none; background:transparent; cursor:pointer; border-bottom:2.5px solid transparent; margin-bottom:-1px; display:flex; align-items:center; gap:6px; white-space:nowrap; font-family:inherit; transition:color .15s ease; }
.tab:hover { color:var(--texto); }
.tab.active { color:var(--texto); border-bottom-color:var(--verde); }

/* ── BODY ── */
.body { padding:18px 40px 10px; }
.block { background:var(--blanco); border:1px solid var(--borde-suave); border-radius:14px; padding:18px 20px; margin-bottom:14px; box-shadow:var(--sombra-sm); }
.block:last-child { margin-bottom:0; }
.block-title { display:flex; align-items:center; gap:10px; font-size:12.5px; font-weight:700; color:var(--texto); text-transform:uppercase; letter-spacing:.4px; margin:0 0 14px; }
.block-title-icon { width:24px; height:24px; border-radius:50%; background:#F0F5F0; color:#4E7A54; display:flex; align-items:center; justify-content:center; flex-shrink:0; font-size:12px; }
.fields-row { display:grid; grid-template-columns:repeat(3, 1fr); gap:14px 16px; }
.field-col { display:flex; flex-direction:column; gap:5px; }
.field-label-row { font-size:10px; font-weight:700; color:var(--texto-ter); text-transform:uppercase; letter-spacing:.4px; }
.field-value { font-size:14px; font-weight:600; color:var(--texto); }
.info-subsection { margin-top:16px; padding-top:16px; border-top:1px solid var(--borde-suave); }
.info-subsection .field-label-row { display:block; margin-bottom:7px; }
.info-subsection-text { font-size:13px; font-weight:500; color:#4B534A; line-height:1.6; margin:0; }
.tint-box { background:var(--fondo); border-radius:10px; padding:13px 15px; }
.tint-box span { font-size:13px; font-weight:600; color:var(--texto); line-height:1.55; }
.tint-box--desc span { font-weight:500; color:#4B534A; font-style:italic; }

/* ── FOOTER ── */
.footer { flex-shrink:0; display:flex; justify-content:flex-end; padding:14px 40px 18px; border-top:1px solid var(--borde-suave); }
.btn-ghost-red { display:flex; align-items:center; gap:6px; height:29px; padding:0 12px; border-radius:8px; background:var(--blanco); border:1px solid var(--borde); color:var(--texto-sec); font-size:11.5px; font-weight:600; cursor:pointer; transition:background-color .16s ease, border-color .16s ease, color .16s ease; }
.btn-ghost-red:hover { background:#FDF4F3; border-color:#E8B9B2; color:var(--rojo); }

/* Animaciones */
.modal-fade-enter-active, .modal-fade-leave-active { transition:opacity 0.22s ease; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity:0; }

/* ── Responsive (mismos breakpoints que Mascotas / Voluntarios) ── */
@media (max-width:1100px) { .don-summary { grid-template-columns:repeat(3, 1fr); } }
@media (max-width:900px) {
  .don-summary { grid-template-columns:repeat(2, 1fr); }
  .modal-box--uniform { width:94vw; height:88vh; }
  .fields-row { grid-template-columns:repeat(2, 1fr); }
}
@media (max-width:640px) {
  .page-header { flex-direction:column; align-items:flex-start; }
  .filtros-row { flex-direction:column; gap:14px; }
  .filtros-row--end { align-items:stretch; }
  .filtro-group { min-width:100%; }
  .filtro-group--search { max-width:none; }
  .don-summary { grid-template-columns:1fr 1fr; }
  .don-table th:nth-child(4), .don-table td:nth-child(4) { display:none; }
  .modal-box--uniform { width:96vw; height:92vh; border-radius:18px; }
  .hero, .body, .footer { padding-left:20px; padding-right:20px; }
  .tabs { padding:0 20px; }
  .fields-row { grid-template-columns:1fr; }
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