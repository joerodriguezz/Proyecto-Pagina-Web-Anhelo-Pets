<script setup>
import { ref, computed, onMounted } from 'vue'
import { getAuditLog, AUDIT_MODULOS, AUDIT_TIPOS_ACCION } from '../../composables/useAuditLog'

// ─────────────────────────────────────────────
// Datos
// ─────────────────────────────────────────────
const registros = ref([])
function cargarRegistros() {
  registros.value = getAuditLog()
}
onMounted(cargarRegistros)

// ─────────────────────────────────────────────
// Modal ver detalle
// ─────────────────────────────────────────────
const showViewModal = ref(false)
const viewTarget     = ref(null)
function openView(r) {
  viewTarget.value = r
  showViewModal.value = true
}

// ─────────────────────────────────────────────
// Filtros
// ─────────────────────────────────────────────
const filterModulo     = ref('Todos')
const filterEstado     = ref('Todos')
const filterTipoAccion = ref('Todos')
const filterRol        = ref('Todos')
const filterUsuario    = ref('Todos')
const fechaInicio      = ref('')
const fechaFin         = ref('')
const searchQuery      = ref('')

const MODULO_TABS = ['Todos', ...AUDIT_MODULOS]
const ESTADO_TABS = ['Todos', 'Exitoso', 'Fallido']

const rolesDisponibles = computed(() => {
  const set = new Set(registros.value.map(r => r.rol).filter(Boolean))
  return ['Todos', ...Array.from(set)]
})
const usuariosDisponibles = computed(() => {
  const set = new Set(registros.value.map(r => r.usuario).filter(Boolean))
  return ['Todos', ...Array.from(set)]
})

const hayFiltros = computed(() =>
  searchQuery.value.trim() !== '' ||
  filterModulo.value !== 'Todos' ||
  filterEstado.value !== 'Todos' ||
  filterTipoAccion.value !== 'Todos' ||
  filterRol.value !== 'Todos' ||
  filterUsuario.value !== 'Todos' ||
  fechaInicio.value !== '' ||
  fechaFin.value !== ''
)

function limpiarFiltros() {
  searchQuery.value      = ''
  filterModulo.value     = 'Todos'
  filterEstado.value     = 'Todos'
  filterTipoAccion.value = 'Todos'
  filterRol.value        = 'Todos'
  filterUsuario.value    = 'Todos'
  fechaInicio.value      = ''
  fechaFin.value         = ''
}

// ─────────────────────────────────────────────
// Ordenamiento
// ─────────────────────────────────────────────
const sortBy  = ref('fecha')
const sortDir = ref('desc')
function toggleSort(campo) {
  if (sortBy.value === campo) {
    sortDir.value = sortDir.value === 'desc' ? 'asc' : 'desc'
  } else {
    sortBy.value  = campo
    sortDir.value = 'desc'
  }
}

// ─────────────────────────────────────────────
// Lista filtrada + ordenada
// ─────────────────────────────────────────────
const filteredRegistros = computed(() => {
  const q = searchQuery.value.trim().toLowerCase()

  let lista = registros.value.filter(r => {
    const matchModulo  = filterModulo.value === 'Todos' || r.modulo === filterModulo.value
    const matchEstado  = filterEstado.value === 'Todos' || r.estado === filterEstado.value
    const matchTipo    = filterTipoAccion.value === 'Todos' || r.tipoAccion === filterTipoAccion.value
    const matchRol      = filterRol.value === 'Todos' || r.rol === filterRol.value
    const matchUsuario  = filterUsuario.value === 'Todos' || r.usuario === filterUsuario.value
    const matchFechaIni = !fechaInicio.value || r.fecha >= fechaInicio.value
    const matchFechaFin = !fechaFin.value || r.fecha <= fechaFin.value
    const matchSearch = !q ||
      r.usuario.toLowerCase().includes(q) ||
      r.elemento.toLowerCase().includes(q) ||
      r.accion.toLowerCase().includes(q) ||
      r.modulo.toLowerCase().includes(q) ||
      (r.descripcion || '').toLowerCase().includes(q)
    return matchModulo && matchEstado && matchTipo && matchRol && matchUsuario && matchFechaIni && matchFechaFin && matchSearch
  })

  const dir = sortDir.value === 'asc' ? 1 : -1
  lista = [...lista].sort((a, b) => {
    let av, bv
    if (sortBy.value === 'fecha')   { av = a.timestamp || 0; bv = b.timestamp || 0 }
    if (sortBy.value === 'usuario') { av = a.usuario || ''; bv = b.usuario || '' }
    if (sortBy.value === 'modulo')  { av = a.modulo || '';  bv = b.modulo || '' }
    if (sortBy.value === 'accion')  { av = a.accion || '';  bv = b.accion || '' }
    if (av < bv) return -1 * dir
    if (av > bv) return  1 * dir
    return 0
  })

  return lista
})

// ─────────────────────────────────────────────
// Paginación
// ─────────────────────────────────────────────
const currentPage = ref(1)
const pageSize     = 10
const totalPages = computed(() => Math.max(1, Math.ceil(filteredRegistros.value.length / pageSize)))
const paginatedRegistros = computed(() => {
  const start = (currentPage.value - 1) * pageSize
  return filteredRegistros.value.slice(start, start + pageSize)
})
function irPagina(p) {
  if (p < 1 || p > totalPages.value) return
  currentPage.value = p
}
// Si un filtro reduce los resultados, evita quedar en una página vacía
function resetPagina() { currentPage.value = 1 }

// ─────────────────────────────────────────────
// KPIs
// ─────────────────────────────────────────────
function hoyStr() { return new Date().toISOString().slice(0, 10) }
function haceNDias(n) {
  const d = new Date()
  d.setDate(d.getDate() - n)
  return d.toISOString().slice(0, 10)
}

const stats = computed(() => {
  const hoy       = hoyStr()
  const hace7dias = haceNDias(7)
  const accionesHoy    = registros.value.filter(r => r.fecha === hoy).length
  const accionesSemana = registros.value.filter(r => r.fecha >= hace7dias).length
  const usuariosActivos = new Set(
    registros.value.filter(r => r.fecha >= hace7dias).map(r => r.usuario)
  ).size
  const cambiosRealizados = registros.value.filter(r => r.valoresNuevos).length
  return { accionesHoy, accionesSemana, usuariosActivos, cambiosRealizados }
})

// ─────────────────────────────────────────────
// Badges / helpers
// ─────────────────────────────────────────────
const estadoBadgeClass = e => e === 'Exitoso' ? 'badge-aprobada' : 'badge-rechazada'

const tipoAccionLabel = {
  crear: 'Creación', editar: 'Edición', eliminar: 'Eliminación',
  aprobar: 'Aprobación', rechazar: 'Rechazo', estado: 'Cambio de estado',
  asignar: 'Asignación', sesion: 'Sesión', password: 'Contraseña',
}

function formatCampo(v) {
  if (v === null || v === undefined || v === '') return '—'
  if (typeof v === 'object') return JSON.stringify(v)
  return String(v)
}
</script>

<template>
  <div class="view-container">

    <!-- ══════════════════════════════════════
         MODAL — VER DETALLE DE REGISTRO
    ══════════════════════════════════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showViewModal && viewTarget" class="modal-overlay" @click.self="showViewModal = false">
          <div class="modal-box modal-box--uniform">
            <button class="close-btn close-btn--hero" @click="showViewModal = false">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>

            <div class="hero">
              <div class="hero-photo">
                <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path d="M9 12l2 2 4-4"/><circle cx="12" cy="12" r="9"/></svg>
              </div>
              <div class="hero-info">
                <div class="hero-name-row">
                  <h2 class="hero-name">{{ viewTarget.accion }}</h2>
                  <span class="estado-badge badge-status-hero" :class="estadoBadgeClass(viewTarget.estado)">{{ viewTarget.estado }}</span>
                </div>
                <div class="hero-meta">
                  <span class="hero-meta-chip">{{ viewTarget.modulo }}</span>
                  <span class="hero-meta-chip">{{ viewTarget.fecha }} · {{ viewTarget.hora }}</span>
                  <span class="hero-meta-chip">{{ viewTarget.usuario }}</span>
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
                          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="4" width="18" height="18" rx="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>
                        </span>
                        Información general
                      </h4>
                      <div class="fields-row">
                        <div class="field-col"><span class="field-label-row">Usuario</span><span class="field-value">{{ viewTarget.usuario }}</span></div>
                        <div class="field-col"><span class="field-label-row">Rol</span><span class="field-value">{{ viewTarget.rol }}</span></div>
                        <div class="field-col"><span class="field-label-row">Módulo</span><span class="field-value">{{ viewTarget.modulo }}</span></div>
                        <div class="field-col"><span class="field-label-row">Tipo de acción</span><span class="field-value">{{ tipoAccionLabel[viewTarget.tipoAccion] || viewTarget.tipoAccion }}</span></div>
                        <div class="field-col"><span class="field-label-row">Fecha</span><span class="field-value">{{ viewTarget.fecha }}</span></div>
                        <div class="field-col"><span class="field-label-row">Hora</span><span class="field-value">{{ viewTarget.hora }}</span></div>
                      </div>
                      <div class="info-subsection" v-if="viewTarget.descripcion">
                        <span class="field-label-row">Descripción</span>
                        <p class="info-subsection-text">{{ viewTarget.descripcion }}</p>
                      </div>
                    </div>
                  </div>

                  <div class="block" style="margin-bottom:0;">
                    <h4 class="block-title">
                      <span class="block-title-icon">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/></svg>
                      </span>
                      Elemento afectado
                    </h4>
                    <div class="list-col">
                      <div class="list-item">
                        <div class="list-icon"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="9"/></svg></div>
                        <div class="list-text"><span class="list-label">Nombre</span><span class="list-value">{{ viewTarget.elemento }}</span></div>
                      </div>
                      <div class="list-item">
                        <div class="list-icon"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="7" height="7" rx="1.5"/><rect x="14" y="3" width="7" height="7" rx="1.5"/><rect x="14" y="14" width="7" height="7" rx="1.5"/><rect x="3" y="14" width="7" height="7" rx="1.5"/></svg></div>
                        <div class="list-text"><span class="list-label">ID</span><span class="list-value">{{ viewTarget.elementoId || '—' }}</span></div>
                      </div>
                    </div>
                  </div>
                </div>

                <!-- Cambios realizados: antes / después -->
                <div class="block block-wide" v-if="viewTarget.valoresAnteriores || viewTarget.valoresNuevos">
                  <h4 class="block-title">
                    <span class="block-title-icon">
                      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 1l4 4-4 4"/><path d="M3 11V9a4 4 0 0 1 4-4h14"/><path d="M7 23l-4-4 4-4"/><path d="M21 13v2a4 4 0 0 1-4 4H3"/></svg>
                    </span>
                    Cambios realizados
                  </h4>
                  <div class="diff-list">
                    <div v-for="campo in Object.keys(viewTarget.valoresNuevos || {})" :key="campo" class="diff-row">
                      <span class="diff-campo">{{ campo }}</span>
                      <div class="diff-values">
                        <span class="diff-antes">{{ formatCampo(viewTarget.valoresAnteriores?.[campo]) }}</span>
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="diff-arrow"><line x1="5" y1="12" x2="19" y2="12"/><polyline points="12 5 19 12 12 19"/></svg>
                        <span class="diff-despues">{{ formatCampo(viewTarget.valoresNuevos?.[campo]) }}</span>
                      </div>
                    </div>
                  </div>
                </div>
                <p v-else class="modal-empty-text" style="margin-top:14px;">Esta acción no modificó valores existentes.</p>
              </div>
            </div>

            <div class="footer">
              <button class="btn-ghost-red" @click="showViewModal = false">Cerrar</button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- ══════════════════════════════════════
         VISTA PRINCIPAL
    ══════════════════════════════════════ -->
    <div>
      <header class="page-header">
        <div class="brand-row">
          <div class="brand-mark">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="4" width="18" height="18" rx="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>
          </div>
          <div>
            <h1 class="admin-page-title">Auditoría del sistema</h1>
            <p class="admin-page-sub">Trazabilidad completa de las acciones realizadas en el panel</p>
          </div>
        </div>
      </header>

      <div class="don-summary">
        <div class="don-card total-card">
          <div class="don-icon total-icon">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="9"/><polyline points="12 7 12 12 16 14"/></svg>
          </div>
          <strong class="don-value">{{ stats.accionesHoy }}</strong>
          <span class="don-label">Acciones hoy</span>
          <span class="don-desc">Registradas en el día</span>
        </div>
        <div class="don-card disponible-card">
          <div class="don-icon disponible-icon">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="4" width="18" height="18" rx="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>
          </div>
          <strong class="don-value">{{ stats.accionesSemana }}</strong>
          <span class="don-label">Últimos 7 días</span>
          <span class="don-desc">Actividad reciente</span>
        </div>
        <div class="don-card proceso-card">
          <div class="don-icon proceso-icon">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/></svg>
          </div>
          <strong class="don-value">{{ stats.usuariosActivos }}</strong>
          <span class="don-label">Usuarios activos</span>
          <span class="don-desc">Con acciones esta semana</span>
        </div>
        <div class="don-card adoptada-card">
          <div class="don-icon adoptada-icon">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M17 1l4 4-4 4"/><path d="M3 11V9a4 4 0 0 1 4-4h14"/><path d="M7 23l-4-4 4-4"/><path d="M21 13v2a4 4 0 0 1-4 4H3"/></svg>
          </div>
          <strong class="don-value">{{ stats.cambiosRealizados }}</strong>
          <span class="don-label">Cambios realizados</span>
          <span class="don-desc">Con valores modificados</span>
        </div>
      </div>

      <div class="filtros-panel">
        <div class="filtros-row">
          <div class="filtro-group filtro-group--tabs">
            <label class="filtro-label">Módulo</label>
            <div class="tabs-wrap">
              <button v-for="m in MODULO_TABS" :key="m" class="tab-btn" :class="{ active: filterModulo === m }" @click="filterModulo = m; resetPagina()">{{ m }}</button>
            </div>
          </div>
          <div class="filtro-group filtro-group--tabs">
            <label class="filtro-label">Estado</label>
            <div class="tabs-wrap">
              <button v-for="e in ESTADO_TABS" :key="e" class="tab-btn" :class="{ active: filterEstado === e }" @click="filterEstado = e; resetPagina()">{{ e }}</button>
            </div>
          </div>
        </div>

        <div class="filtros-divider"></div>

        <div class="filtros-row">
          <div class="filtro-group">
            <label class="filtro-label">Desde</label>
            <input type="date" v-model="fechaInicio" class="filtro-input filtro-date" @change="resetPagina()" />
          </div>
          <div class="filtro-group">
            <label class="filtro-label">Hasta</label>
            <input type="date" v-model="fechaFin" class="filtro-input filtro-date" @change="resetPagina()" />
          </div>
          <div class="filtro-group">
            <label class="filtro-label">Usuario</label>
            <select v-model="filterUsuario" class="filtro-input filtro-select" @change="resetPagina()">
              <option v-for="u in usuariosDisponibles" :key="u">{{ u }}</option>
            </select>
          </div>
          <div class="filtro-group">
            <label class="filtro-label">Rol</label>
            <select v-model="filterRol" class="filtro-input filtro-select" @change="resetPagina()">
              <option v-for="r in rolesDisponibles" :key="r">{{ r }}</option>
            </select>
          </div>
          <div class="filtro-group">
            <label class="filtro-label">Tipo de acción</label>
            <select v-model="filterTipoAccion" class="filtro-input filtro-select" @change="resetPagina()">
              <option value="Todos">Todos</option>
              <option v-for="t in AUDIT_TIPOS_ACCION" :key="t" :value="t">{{ tipoAccionLabel[t] }}</option>
            </select>
          </div>
        </div>

        <div class="filtros-divider"></div>

        <div class="filtros-row filtros-row--end">
          <div class="filtro-group filtro-group--search">
            <label class="filtro-label">Buscar</label>
            <div class="filtro-input-wrap">
              <span class="filtro-icon filtro-icon--left">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
              </span>
              <input v-model="searchQuery" @input="resetPagina()" placeholder="Usuario, mascota, donación, rescate, adopción..." class="filtro-input filtro-input--icon-left" />
            </div>
          </div>
          <div class="filtro-group filtro-group--btn">
            <button class="btn btn--ghost" :class="{ 'btn--ghost-active': hayFiltros }" @click="limpiarFiltros(); resetPagina()">Limpiar filtros</button>
          </div>
        </div>
      </div>

      <div v-if="filteredRegistros.length === 0" class="empty-state">
        <svg xmlns="http://www.w3.org/2000/svg" width="36" height="36" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="4" width="18" height="18" rx="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>
        <p class="empty-title">{{ hayFiltros ? 'Sin resultados para los filtros aplicados' : 'Aún no hay registros de auditoría' }}</p>
        <p class="empty-sub">{{ hayFiltros ? 'Ajusta los filtros para ver más resultados.' : 'Las acciones del sistema aparecerán aquí automáticamente.' }}</p>
      </div>

      <div v-else class="table-wrapper">
        <div class="table-scroll">
          <table class="don-table">
            <thead>
              <tr>
                <th class="th-sort" @click="toggleSort('fecha')">Fecha / hora</th>
                <th class="th-sort" @click="toggleSort('usuario')">Usuario</th>
                <th>Rol</th>
                <th class="th-sort" @click="toggleSort('modulo')">Módulo</th>
                <th class="th-sort" @click="toggleSort('accion')">Acción</th>
                <th>Elemento afectado</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="r in paginatedRegistros" :key="r.id" class="don-row">
                <td>
                  <span class="donor-name">{{ r.fecha }}</span>
                  <span class="donor-mail">{{ r.hora }}</span>
                </td>
                <td><span class="donor-name">{{ r.usuario }}</span></td>
                <td><span class="type-chip">{{ r.rol }}</span></td>
                <td><span class="fecha-text">{{ r.modulo }}</span></td>
                <td>
                  <span class="donor-name">{{ r.accion }}</span>
                  <span class="donor-mail">{{ tipoAccionLabel[r.tipoAccion] || r.tipoAccion }}</span>
                </td>
                <td><span class="fecha-text">{{ r.elemento }}</span></td>
                <td><span class="estado-badge" :class="estadoBadgeClass(r.estado)">{{ r.estado }}</span></td>
                <td>
                  <div class="action-group">
                    <button class="icon-only icon-only--ver" @click="openView(r)" data-tooltip="Ver detalle">
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div class="table-footer table-footer--paginated">
          <span>{{ filteredRegistros.length }} registro{{ filteredRegistros.length !== 1 ? 's' : '' }} encontrado{{ filteredRegistros.length !== 1 ? 's' : '' }}</span>
          <div class="pagination" v-if="totalPages > 1">
            <button class="page-btn" :disabled="currentPage === 1" @click="irPagina(currentPage - 1)">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"><polyline points="15 18 9 12 15 6"/></svg>
            </button>
            <span class="page-info">Página {{ currentPage }} de {{ totalPages }}</span>
            <button class="page-btn" :disabled="currentPage === totalPages" @click="irPagina(currentPage + 1)">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"><polyline points="9 18 15 12 9 6"/></svg>
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* ── Variables (idénticas a Mascotas.vue) ─────────────────────────── */
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
  --select-arrow: url('data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="%237A827B" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>');
  background:
    radial-gradient(ellipse 800px 420px at 12% 0%, rgba(146,168,148,.07), transparent),
    var(--fondo);
  padding-bottom: 40px;
}

/* ── Botones ── */
.btn { display:inline-flex; align-items:center; justify-content:center; gap:6px; height:33px; padding:0 13px; border-radius:9px; border:1px solid transparent; font-family:inherit; font-size:12.5px; font-weight:600; line-height:1; white-space:nowrap; cursor:pointer; user-select:none; transition:background-color .16s ease, border-color .16s ease, color .16s ease; }
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

/* ── KPIs ── */
.don-summary { display:grid; grid-template-columns:repeat(4, 1fr); gap:12px; margin-bottom:20px; }
.don-card { background:var(--blanco); border-radius:16px; padding:16px 15px; border:1px solid var(--borde); box-shadow:var(--sombra-sm); display:flex; flex-direction:column; transition:box-shadow .18s ease, border-color .18s ease; }
.don-card:hover { border-color:#D7DED8; box-shadow:var(--sombra-md); }
.don-icon { width:32px; height:32px; border-radius:50%; display:flex; align-items:center; justify-content:center; margin-bottom:12px; border:1px solid transparent; }
.total-icon { background:#F2F3F2; border-color:#DFE2DF; color:#616861; }
.disponible-icon { background:#EDF6EF; border-color:#C9E4CE; color:#2E7D45; }
.proceso-icon { background:#FDF6E8; border-color:#F2E1B8; color:#A97A0C; }
.adoptada-icon { background:#EAF2F6; border-color:#C7DCE6; color:#3C6E85; }
.don-value { font-size:21px; font-weight:700; color:var(--texto); line-height:1; letter-spacing:-0.4px; font-variant-numeric:tabular-nums; }
.don-label { font-size:10.5px; color:var(--texto-ter); font-weight:700; text-transform:uppercase; letter-spacing:0.5px; margin-top:7px; }
.don-desc { font-size:11px; color:var(--texto-sec); margin-top:2px; }

/* ── Filtros ── */
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
.filtro-input { height:36px; padding:0 14px; border-radius:8px; border:1px solid var(--borde); background:var(--fondo); font-size:13px; color:var(--texto); font-family:inherit; outline:none; transition:border-color 0.18s, background 0.18s; box-sizing:border-box; }
.filtro-input:focus { border-color:var(--verde-sec); background:var(--blanco); }
.filtro-input--icon-left { padding-left:36px; width:100%; }
.filtro-icon { position:absolute; display:flex; align-items:center; color:var(--texto-sec); }
.filtro-icon--left { left:12px; }
.filtro-date { width:150px; }
.filtro-select {
  width:170px; padding-right:30px;
  background-image:var(--select-arrow); background-repeat:no-repeat; background-position:right 10px center;
  appearance:none; -webkit-appearance:none; -moz-appearance:none;
}

/* ── Estado vacío ── */
.empty-state { text-align:center; padding:72px 24px; background:var(--blanco); border-radius:16px; border:1px solid var(--borde); color:var(--verde-sec); display:flex; flex-direction:column; align-items:center; gap:10px; }
.empty-state svg { opacity:0.4; }
.empty-title { font-size:16px; font-weight:700; color:var(--texto); margin:0; }
.empty-sub { font-size:13px; color:var(--texto-sec); margin:0; }

/* ── Tabla ── */
.table-wrapper { background:var(--blanco); border-radius:16px; border:1px solid var(--borde); overflow:hidden; box-shadow:var(--sombra-sm); }
.table-scroll { overflow-x:auto; -webkit-overflow-scrolling:touch; }
.don-table { width:100%; border-collapse:collapse; min-width:860px; }
.don-table thead th { padding:12px 16px; text-align:left; color:var(--texto-ter); font-size:9.5px; font-weight:700; text-transform:uppercase; letter-spacing:0.6px; white-space:nowrap; }
.th-sort { cursor:pointer; user-select:none; }
.th-sort:hover { color:var(--texto); }
.don-table tbody tr { border-top:1px solid var(--borde-suave); transition:background 0.15s; }
.don-table tbody tr:hover { background:#FAFBFA; }
.don-table tbody td { padding:12px 16px; vertical-align:middle; }
.donor-name { display:block; font-size:12.5px; font-weight:700; color:var(--texto); line-height:1.3; }
.donor-mail { display:block; font-size:11px; color:var(--texto-sec); margin-top:2px; }
.fecha-text { font-size:12.5px; color:var(--texto-sec); }
.type-chip { font-size:11.5px; font-weight:600; color:#4E6E51; background:#F1F5F1; padding:3px 10px; border-radius:7px; white-space:nowrap; }
.estado-badge { display:inline-block; font-size:10.5px; font-weight:700; padding:4px 11px; border-radius:20px; white-space:nowrap; }
.badge-aprobada { background:#EDF6EF; color:#2E7D32; }
.badge-rechazada { background:#FBEDEC; color:#B71C1C; }
.table-footer { padding:12px 16px; border-top:1px solid var(--borde-suave); font-size:12px; color:var(--texto-sec); font-weight:500; }
.table-footer--paginated { display:flex; align-items:center; justify-content:space-between; gap:12px; flex-wrap:wrap; }
.pagination { display:flex; align-items:center; gap:10px; }
.page-btn { width:28px; height:28px; border-radius:7px; border:1px solid var(--borde); background:var(--blanco); color:var(--texto-sec); display:flex; align-items:center; justify-content:center; cursor:pointer; transition:background-color .16s ease, border-color .16s ease; }
.page-btn:hover:not(:disabled) { background:#FAFBFA; border-color:#D3D8D3; color:var(--texto); }
.page-btn:disabled { opacity:0.4; cursor:not-allowed; }
.page-info { font-size:12px; color:var(--texto-sec); font-weight:600; white-space:nowrap; }

.action-group { display:flex; gap:8px; align-items:center; }
.icon-only { width:38px; height:38px; border-radius:8px; border:1px solid var(--borde); background:var(--blanco); display:flex; align-items:center; justify-content:center; cursor:pointer; transition:background-color .16s ease, border-color .16s ease; position:relative; }
.icon-only svg { width:16px; height:16px; }
.icon-only--ver { color:#3D453B; }
.icon-only--ver:hover { border-color:#C7D3C8; background:#FAFCFA; }
.icon-only::before { content:attr(data-tooltip); position:absolute; bottom:calc(100% + 8px); left:50%; transform:translateX(-50%) translateY(4px); background:var(--verde); color:#fff; font-size:11px; font-weight:600; padding:5px 9px; border-radius:7px; white-space:nowrap; opacity:0; visibility:hidden; pointer-events:none; transition:opacity .15s ease, transform .15s ease; z-index:20; }
.icon-only:hover::before { opacity:1; visibility:visible; transform:translateX(-50%) translateY(0); }

/* ── Modal base ── */
.modal-overlay { position:fixed; inset:0; background:rgba(0,0,0,0.35); backdrop-filter:blur(4px); z-index:1000; display:flex; align-items:center; justify-content:center; padding:24px; }
.modal-box { background:var(--blanco); border-radius:22px; box-shadow:var(--sombra-md); position:relative; }
.modal-box--uniform { width:880px; max-width:92vw; height:660px; max-height:90vh; display:flex; flex-direction:column; overflow:hidden; border:1px solid var(--borde-suave); }
.uniform-scroll { flex:1; min-height:0; overflow-y:auto; }
.close-btn { position:absolute; top:18px; right:18px; z-index:6; width:30px; height:30px; border-radius:9px; background:var(--fondo); border:1px solid var(--borde-suave); color:#8B928A; display:flex; align-items:center; justify-content:center; cursor:pointer; transition:background-color .16s ease, color .16s ease, border-color .16s ease; }
.close-btn svg { width:16px; height:16px; }
.close-btn--hero { background:var(--fondo); }
.close-btn--hero:hover { background:var(--verde); color:#fff; }

.hero { flex-shrink:0; background:linear-gradient(165deg, #FFFFFF 0%, #F7FAF7 55%, #F1F7F2 100%); border-bottom:1px solid var(--borde-suave); padding:28px 40px 24px; display:flex; align-items:center; gap:20px; }
.hero-photo { width:60px; height:60px; border-radius:16px; flex-shrink:0; overflow:hidden; background:linear-gradient(150deg,#E7F0E8 0%,#DCEBDE 100%); border:1px solid var(--borde-suave); display:flex; align-items:center; justify-content:center; color:#3E7A45; box-shadow:0 1px 2px rgba(58,71,60,.04), 0 10px 22px -12px rgba(58,71,60,.28); }
.hero-info { flex:1; min-width:0; display:flex; flex-direction:column; gap:8px; }
.hero-name-row { display:flex; align-items:center; gap:12px; flex-wrap:wrap; }
.hero-name { font-size:19px; font-weight:700; color:var(--texto); margin:0; letter-spacing:-.4px; }
.hero-meta { display:flex; align-items:center; gap:7px; flex-wrap:wrap; }
.hero-meta-chip { display:inline-flex; align-items:center; gap:6px; font-size:11.5px; font-weight:600; color:#4B5A4C; background:var(--blanco); border:1px solid var(--borde-suave); padding:4px 10px 4px 9px; border-radius:20px; }
.badge-status-hero { padding:5px 12px !important; font-size:10.5px !important; }

.body { padding:18px 40px 10px; }
.grid-2col { display:grid; grid-template-columns:1.6fr 1fr; gap:14px; align-items:start; margin-bottom:14px; }
.block { background:var(--blanco); border:1px solid var(--borde-suave); border-radius:14px; padding:18px 20px; margin-bottom:14px; box-shadow:var(--sombra-sm); }
.block:last-child { margin-bottom:0; }
.block-wide { margin-top:0; }
.block-title { display:flex; align-items:center; gap:10px; font-size:12.5px; font-weight:700; color:var(--texto); text-transform:uppercase; letter-spacing:.4px; margin:0 0 14px; }
.block-title-icon { width:24px; height:24px; border-radius:50%; background:#F0F5F0; color:#4E7A54; display:flex; align-items:center; justify-content:center; flex-shrink:0; }
.fields-row { display:grid; grid-template-columns:repeat(3, 1fr); gap:14px 16px; }
.field-col { display:flex; flex-direction:column; gap:5px; }
.field-label-row { font-size:10px; font-weight:700; color:var(--texto-ter); text-transform:uppercase; letter-spacing:.4px; }
.field-value { font-size:14px; font-weight:600; color:var(--texto); }
.info-subsection { margin-top:16px; padding-top:16px; border-top:1px solid var(--borde-suave); }
.info-subsection .field-label-row { display:block; margin-bottom:7px; }
.info-subsection-text { font-size:13px; font-weight:500; color:#4B534A; line-height:1.6; margin:0; }
.list-col { display:grid; grid-template-columns:1fr; gap:8px; }
.list-item { border:1px solid var(--borde-suave); border-radius:10px; padding:10px 12px; display:flex; align-items:center; gap:10px; }
.list-icon { width:30px; height:30px; border-radius:8px; flex-shrink:0; background:#EDF3EE; color:#3E7A45; display:flex; align-items:center; justify-content:center; }
.list-text { display:flex; flex-direction:column; gap:2px; min-width:0; }
.list-label { font-size:9.5px; font-weight:700; color:var(--texto-ter); text-transform:uppercase; letter-spacing:.4px; }
.list-value { font-size:12.5px; font-weight:700; color:var(--texto); white-space:nowrap; overflow:hidden; text-overflow:ellipsis; }
.modal-empty-text { font-size:13px; color:var(--texto-ter); background:var(--fondo); border:1px dashed var(--borde); border-radius:10px; padding:18px 16px; margin:0; text-align:center; }

/* ── Antes / después ── */
.diff-list { display:flex; flex-direction:column; gap:8px; }
.diff-row { display:flex; align-items:center; gap:16px; padding:12px 14px; background:var(--fondo); border-radius:10px; flex-wrap:wrap; }
.diff-campo { font-size:11px; font-weight:700; color:var(--texto-ter); text-transform:uppercase; letter-spacing:.4px; width:130px; flex-shrink:0; }
.diff-values { display:flex; align-items:center; gap:10px; flex:1; min-width:200px; }
.diff-antes { font-size:13px; color:#B71C1C; background:#FBEDEC; padding:5px 10px; border-radius:7px; text-decoration:line-through; opacity:0.85; }
.diff-despues { font-size:13px; color:#2E7D32; background:#EDF6EF; padding:5px 10px; border-radius:7px; font-weight:600; }
.diff-arrow { color:var(--texto-ter); flex-shrink:0; }

.footer { flex-shrink:0; display:flex; justify-content:flex-end; padding:14px 40px 18px; border-top:1px solid var(--borde-suave); }
.btn-ghost-red { display:flex; align-items:center; gap:6px; height:29px; padding:0 12px; border-radius:8px; background:var(--blanco); border:1px solid var(--borde); color:var(--texto-sec); font-size:11.5px; font-weight:600; cursor:pointer; transition:background-color .16s ease, border-color .16s ease, color .16s ease; }
.btn-ghost-red:hover { background:#FDF4F3; border-color:#E8B9B2; color:var(--rojo); }

.modal-fade-enter-active, .modal-fade-leave-active { transition:opacity 0.22s ease; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity:0; }

/* ── Responsive ── */
@media (max-width:1100px) { .don-summary { grid-template-columns:repeat(2, 1fr); } }
@media (max-width:900px) {
  .modal-box--uniform { width:94vw; height:88vh; }
  .grid-2col { grid-template-columns:1fr; }
  .fields-row { grid-template-columns:repeat(2, 1fr); }
}
@media (max-width:640px) {
  .filtros-row { flex-direction:column; gap:14px; }
  .filtros-row--end { align-items:stretch; }
  .filtro-group { min-width:100%; }
  .filtro-group--search { max-width:none; }
  .filtro-date, .filtro-select { width:100%; }
  .don-summary { grid-template-columns:1fr 1fr; }
  .don-table th:nth-child(3), .don-table td:nth-child(3) { display:none; }
  .modal-box--uniform { width:96vw; height:92vh; border-radius:18px; }
  .hero, .body, .footer { padding-left:20px; padding-right:20px; }
  .fields-row { grid-template-columns:1fr; }
  .diff-campo { width:100%; }
}
@media (max-width:480px) { .don-summary { grid-template-columns:1fr; } }
</style>