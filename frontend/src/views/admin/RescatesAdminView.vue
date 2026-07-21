<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { ubicacionesCR } from '../../data/ubicaciones'
import { useRescuesStore } from '../../stores/useRescuesStore'
import { getAnimals, createAnimals } from '../../services/petServices'
import { getFosterHomes, createFosterHome } from '../../services/fosterHomeServices'

/* ─── Store de rescates ──────────────────────────────────── */
const store = useRescuesStore()

/* ─── Lista de animales registrados ─────────────────────── */
const animalesList = ref([])
const animalesLoading = ref(false)

async function loadAnimales() {
  animalesLoading.value = true
  try {
    const { data } = await getAnimals()
    animalesList.value = (data || []).map(a => ({
      id:   a.animalId,
      name: a.animalName,
      type: a.species || '',
      sex:  a.sex === 'H' ? 'Hembra' : a.sex === 'M' ? 'Macho' : '',
      age:  [a.ageYears, a.ageMonths].filter(Boolean).join(' '),
    }))
  } catch {
    animalesList.value = []
  } finally {
    animalesLoading.value = false
  }
}

/* ─── Main state from store ──────────────────────────────── */
const rescates = store.rescates

/* ─── UI ─────────────────────────────────────────────────── */
const showForm        = ref(false)
const editMode        = ref(false)
const showEditModal   = ref(false)
const editingId       = ref(null)
const showDetailModal = ref(false)
const rescueSelected  = ref(null)
const modalConfirm    = ref(false)
const confirmId       = ref(null)

/* ─── Toast ──────────────────────────────────────────────── */
const toast = ref({ visible: false, tipo: 'exito', texto: '' })

function mostrarToast(texto, tipo = 'exito') {
  toast.value = { visible: true, tipo, texto }
  setTimeout(() => { toast.value.visible = false }, 3000)
}

/* ─── Usuario actual ─────────────────────────────────────── */
const usuarioActual = ref({ nombre: 'Shirley Valverde', rol: 'Admin' })

/* ─── Voluntarios ────────────────────────────────────────── */
const voluntarios = ref(
  JSON.parse(localStorage.getItem('anhelo_usuarios')) || []
)

/* ─── Provincias para filtro ─────────────────────────────── */
const provinciasDisponibles = computed(() => Object.keys(ubicacionesCR))

/* ─── Helper: obtener nombre del rescatista desde localStorage ── */
function getVolunteerName(id) {
  if (!id) return 'Anónimo'
  const v = voluntarios.value.find(u => u.id === id)
  return v?.nombre || v?.correo || id
}

/* ─── Casas cuna y rescatistas ───────────────────────────── */
const casasCunaDisponibles = computed(() =>
  voluntarios.value.filter(v => {
    const estado = v.solicitudVoluntario?.estado
    const tipo   = v.solicitudVoluntario?.tipo || ''
    return estado === 'Aprobada' &&
      (tipo.toLowerCase() === 'casa cuna')
  })
)

const rescatistasDisponibles = computed(() =>
  voluntarios.value.filter(v => {
    const estado = v.solicitudVoluntario?.estado
    const tipo   = v.solicitudVoluntario?.tipo || ''
    return estado === 'Aprobada' && tipo === 'Rescatista'
  })
)

/* ─── Filtros de tabla ───────────────────────────────────── */
const filtroSearch   = ref('')
const filtroProv     = ref('Todos')
const filtroEstado   = ref('Todos')

const hayFiltros = computed(() =>
  filtroSearch.value.trim() !== '' ||
  filtroProv.value   !== 'Todos'   ||
  filtroEstado.value !== 'Todos'
)

function limpiarFiltros() {
  filtroSearch.value = ''
  filtroProv.value   = 'Todos'
  filtroEstado.value = 'Todos'
}

const rescatesFiltrados = computed(() => {
  const q = filtroSearch.value.trim().toLowerCase()
  return rescates.value.filter(r => {
    const coincideSearch =
      !q ||
      String(r.id || '').toLowerCase().includes(q) ||
      (r.mascota || '').toLowerCase().includes(q) ||
      (r.descripcion || '').toLowerCase().includes(q)

    const coincideProv =
      filtroProv.value === 'Todos' ||
      (r.ubicacion || '').toLowerCase().startsWith(filtroProv.value.toLowerCase())

    const coincideEstado =
      filtroEstado.value === 'Todos' ||
      r.estado === filtroEstado.value

    return coincideSearch && coincideProv && coincideEstado
  })
})

/* ─── Carga ──────────────────────────────────────────────── */
onMounted(() => {
  if (!store.loaded) store.fetchRescues()
  loadAnimales()
  loadFosterHomes()
})

/* ─── Lista de casas cuna desde API ────────────────────── */
const fosterHomesList = ref([])
const fhLoading = ref(false)

async function loadFosterHomes() {
  fhLoading.value = true
  try {
    const { data } = await getFosterHomes()
    fosterHomesList.value = (data || []).filter(f => f.active)
  } catch {
    fosterHomesList.value = []
  } finally {
    fhLoading.value = false
  }
}

/* ─── Quick-create foster home modal ──────────────────── */
const showQuickFoster = ref(false)
const quickFoster     = ref({ name: '', address: '', phone: '', responsible: '', capacity: 1 })
const quickFosterLoading = ref(false)
const quickFosterErrors  = ref([])

function abrirQuickFoster() {
  quickFoster.value = { name: '', address: '', phone: '', responsible: '', capacity: 1 }
  quickFosterErrors.value = []
  showQuickFoster.value = true
}

async function guardarQuickFoster() {
  const e = []
  if (!quickFoster.value.name.trim())        e.push('Nombre')
  if (!quickFoster.value.address.trim())     e.push('Dirección')
  if (!quickFoster.value.phone.trim())       e.push('Teléfono')
  if (!quickFoster.value.responsible.trim()) e.push('Responsable')
  quickFosterErrors.value = e
  if (e.length) return

  quickFosterLoading.value = true
  try {
    await createFosterHome(quickFoster.value)
    await loadFosterHomes()
    showQuickFoster.value = false
    mostrarToast('Casa cuna creada correctamente.')
  } catch {
    mostrarToast('Error al crear la casa cuna.', 'error')
  } finally {
    quickFosterLoading.value = false
  }
}

/* ─── Quick-create animal modal ─────────────────────────── */
const showQuickAnimal = ref(false)
const quickAnimal     = ref({ name: '', species: '', sex: '', ageYears: 0 })
const quickAnimalLoading = ref(false)
const quickAnimalErrors  = ref([])

function abrirQuickAnimal() {
  quickAnimal.value = { name: '', species: '', sex: '', ageYears: 0 }
  quickAnimalErrors.value = []
  showQuickAnimal.value = true
}

async function guardarQuickAnimal() {
  const e = []
  if (!quickAnimal.value.name.trim())   e.push('Nombre')
  if (!quickAnimal.value.species)       e.push('Tipo')
  if (!quickAnimal.value.sex)           e.push('Sexo')
  quickAnimalErrors.value = e
  if (e.length) return

  quickAnimalLoading.value = true
  try {
    const payload = {
      name:  quickAnimal.value.name,
      type:  quickAnimal.value.species,
      age:   quickAnimal.value.ageYears ? `${quickAnimal.value.ageYears} año${quickAnimal.value.ageYears !== 1 ? 's' : ''}` : '',
      sex:   quickAnimal.value.sex === 'H' ? 'Hembra' : 'Macho',
    }
    const { data } = await createAnimals(payload)
    await loadAnimales()
    animalId.value = data.animalId
    showQuickAnimal.value = false
    mostrarToast(`Mascota "${data.animalName}" creada correctamente.`)
  } catch {
    mostrarToast('Error al crear la mascota.', 'error')
  } finally {
    quickAnimalLoading.value = false
  }
}

/* ─── Ubicación en formulario ────────────────────────────── */
const provincia = ref('')
const canton    = ref('')
const distrito  = ref('')

const cantonesDisponibles = computed(() => {
  if (!provincia.value || !ubicacionesCR[provincia.value]) return []
  return Object.keys(ubicacionesCR[provincia.value])
})
const distritosDisponibles = computed(() => {
  if (!provincia.value || !canton.value) return []
  return ubicacionesCR[provincia.value]?.[canton.value] || []
})

watch(provincia, () => { canton.value = ''; distrito.value = '' })
watch(canton,    () => { distrito.value = '' })

/* ─── Formulario ─────────────────────────────────────────── */
const currentStep   = ref(1)
const animalId      = ref('')
const fechaRescate  = ref('')
const descripcion   = ref('')
const casaCuna      = ref('')
const rescatista    = ref('')
const estado        = ref('Activo')
const formErrors    = ref([])

/* ─── Validación ─────────────────────────────────────────── */
function validar() {
  const errores = []
  if (!animalId.value)        errores.push('Animal')
  if (!fechaRescate.value)    errores.push('Fecha de rescate')
  if (!provincia.value)       errores.push('Provincia')
  if (!canton.value)          errores.push('Cantón')
  if (!distrito.value)        errores.push('Distrito')
  if (!descripcion.value.trim()) errores.push('Descripción del rescate')
  formErrors.value = errores
  return errores.length === 0
}

/* ─── Guardar rescate ────────────────────────────────────── */
async function guardarRescate() {
  if (!validar()) {
    mostrarToast('Completa todos los campos obligatorios.', 'error')
    return
  }

  const formData = {
    animalId:     animalId.value,
    fechaRescate: fechaRescate.value,
    ubicacion:    `${provincia.value} · ${canton.value} · ${distrito.value}`,
    descripcion:  descripcion.value,
    estado:       estado.value,
    fosterHomeId: casaCuna.value || null,
    volunteerId: rescatista.value || null,
  }

  try {
    if (editMode.value && editingId.value) {
      await store.editRescue(editingId.value, formData)
      mostrarToast('Rescate actualizado correctamente.')
    } else {
      await store.addRescue(formData)
      mostrarToast('Rescate registrado correctamente.')
    }

    limpiarFormulario()
    showForm.value     = false
    showEditModal.value = false
    editMode.value     = false
    editingId.value    = null
  } catch {
    mostrarToast('Error al guardar el rescate.', 'error')
  }
}

/* ─── Editar ─────────────────────────────────────────────── */
function editarRescate(index) {
  const r = rescates.value[index]
  animalId.value     = r.animalId || ''
  fechaRescate.value = r.fechaRescate
  descripcion.value  = r.descripcion
  casaCuna.value     = r.fosterHomeId || ''
  rescatista.value   = r.volunteerId || ''
  estado.value       = r.estado
  // parse ubicacion back to provincia/canton/distrito
  const parts = (r.ubicacion || '').split(' · ')
  provincia.value    = parts[0] || ''
  canton.value       = parts[1] || ''
  distrito.value     = parts[2] || ''
  editingId.value    = r.id
  editMode.value     = true
  showEditModal.value = true
}

/* ─── Cerrar rescate (con confirmación) ──────────────────── */
function pedirCerrar(index) {
  confirmId.value = rescates.value[index]?.id
  modalConfirm.value = true
}
async function confirmarCerrar() {
  if (confirmId.value != null) {
    try {
      await store.removeRescue(confirmId.value)
      mostrarToast('Rescate cerrado correctamente.')
    } catch {
      mostrarToast('Error al cerrar el rescate.', 'error')
    }
  }
  modalConfirm.value = false
  confirmId.value = null
}

/* ─── Ver detalle ────────────────────────────────────────── */
function verDetalle(rescate) {
  rescueSelected.value  = rescate
  showDetailModal.value = true
}

/* ─── Limpiar formulario ─────────────────────────────────── */
function limpiarFormulario() {
  currentStep.value  = 1
  animalId.value     = ''
  fechaRescate.value = ''
  descripcion.value  = ''
  casaCuna.value     = ''
  rescatista.value   = ''
  estado.value       = 'Activo'
  provincia.value    = ''
  canton.value       = ''
  distrito.value     = ''
  formErrors.value   = []
}

function abrirNuevo() {
  limpiarFormulario()
  editMode.value  = false
  showForm.value  = true
}

function cancelarFormulario() {
  limpiarFormulario()
  editMode.value  = false
  showForm.value  = false
}

function pasoValido(paso) {
  if (paso === 1) {
    return animalId.value && fechaRescate.value
  }
  return true
}

function siguientePaso() {
  if (!pasoValido(currentStep.value)) {
    mostrarToast('Completa los campos obligatorios del paso actual.', 'error')
    return
  }
  currentStep.value++
}

function pasoAnterior() {
  currentStep.value--
}

/* ─── KPIs ───────────────────────────────────────────────── */
const ahora     = new Date()
const mesActual = ahora.getMonth()
const añoActual = ahora.getFullYear()

const totalRescates = computed(() => rescates.value.length)
const totalActivos  = computed(() => rescates.value.filter(r => r.estado === 'Activo').length)
const totalCerrados = computed(() => rescates.value.filter(r => r.estado === 'Cerrado').length)
const totalEsteMes  = computed(() =>
  rescates.value.filter(r => {
    const f = new Date(r.fechaRescate || r.fechaCreacion)
    return !isNaN(f) && f.getMonth() === mesActual && f.getFullYear() === añoActual
  }).length
)

/* ─── Badge helpers ──────────────────────────────────────── */
function estadoBadgeClass(est) {
  return {
    'Activo':  'badge-activo',
    'Cerrado': 'badge-cerrado',
  }[est] || 'badge-cerrado'
}

function estadoIcon(est) {
  return { 'Activo': '●', 'Cerrado': '◉' }[est] || '●'
}

function iniciales(nombre) {
  if (!nombre) return '?'
  return nombre.trim().split(' ').map(p => p[0]).slice(0, 2).join('').toUpperCase()
}
</script>

<template>
  <div class="view-container">

    <!-- ══ Toast ══ -->
    <Teleport to="body">
      <Transition name="toast-anim">
        <div v-if="toast.visible" class="rc-toast" :class="toast.tipo === 'error' ? 'toast-error' : 'toast-success'">
          <svg v-if="toast.tipo === 'exito'" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
          <svg v-else width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
          {{ toast.texto }}
        </div>
      </Transition>
    </Teleport>

    <!-- ══════════════════════════════════════════
         VISTA TABLA
    ═══════════════════════════════════════════ -->
    <template v-if="!showForm">

      <!-- Cabecera -->
      <header class="page-header">
        <div>
          <h1 class="admin-page-title">Rescates</h1>
          <p class="admin-page-sub">Registro y seguimiento de animales rescatados</p>
        </div>
          <button class="btn-nuevo" @click="abrirNuevo">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
          Nuevo rescate
        </button>
        <button class="btn-nuevo btn-nuevo--secondary" @click="store.fetchRescues()" style="margin-left:8px">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="23 4 23 10 17 10"/><path d="M20.49 15a9 9 0 1 1-2.12-9.36L23 10"/></svg>
          Recargar
        </button>
      </header>

      <!-- KPIs -->
      <div class="don-summary">
        <div class="don-card kpi-mes">
          <span class="don-label">Rescates este mes</span>
          <strong class="don-value">{{ totalEsteMes }}</strong>
        </div>
        <div class="don-card kpi-total">
          <span class="don-label">Total rescates</span>
          <strong class="don-value">{{ totalRescates }}</strong>
        </div>
        <div class="don-card kpi-activos">
          <span class="don-label">Activos</span>
          <strong class="don-value">{{ totalActivos }}</strong>
        </div>
        <div class="don-card kpi-cerrados">
          <span class="don-label">Cerrados</span>
          <strong class="don-value">{{ totalCerrados }}</strong>
        </div>
      </div>

      <!-- Panel de filtros -->
      <div class="filtros-panel">

        <div class="filtro-group">
          <label class="filtro-label">Buscar</label>
          <div class="filtro-input-wrap">
            <input
              v-model="filtroSearch"
              placeholder="ID, mascota o rescatista..."
              class="filtro-input"
            />
            <span class="filtro-icon filtro-icon--right">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
            </span>
          </div>
        </div>

        <div class="filtro-group">
          <label class="filtro-label">Provincia</label>
          <div class="filtro-input-wrap">
            <select v-model="filtroProv" class="filtro-input filtro-select">
              <option value="Todos">Todas</option>
              <option v-for="p in provinciasDisponibles" :key="p" :value="p">{{ p }}</option>
            </select>
            <span class="filtro-icon filtro-icon--right filtro-icon--no-events">
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
            </span>
          </div>
        </div>

        <div class="filtro-group">
          <label class="filtro-label">Estado</label>
          <div class="filtro-input-wrap">
            <select v-model="filtroEstado" class="filtro-input filtro-select">
              <option value="Todos">Todos</option>
              <option value="Activo">Activo</option>
              <option value="Cerrado">Cerrado</option>
            </select>
            <span class="filtro-icon filtro-icon--right filtro-icon--no-events">
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
            </span>
          </div>
        </div>

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

      <!-- Estado vacío -->
      <div v-if="rescatesFiltrados.length === 0" class="empty-state">
        <p class="empty-title">{{ hayFiltros ? 'Sin resultados para los filtros aplicados' : 'No hay rescates registrados' }}</p>
        <p class="empty-sub">{{ hayFiltros ? 'Ajusta los filtros para ver resultados.' : 'Registra el primer rescate usando el botón superior.' }}</p>
      </div>

      <!-- Tabla -->
      <div v-else class="table-wrapper">
        <div class="table-scroll">
          <table class="don-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Mascota</th>
                <th>Rescatista</th>
                <th>Provincia</th>
                <th>Casa cuna</th>
                <th>Estado</th>
                <th>Acción</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="r in rescatesFiltrados" :key="r.id" class="don-row">

                <td><span class="id-pill">{{ r.id }}</span></td>

                <td>
                  <div class="pet-cell">
                    <div class="pet-avatar">
                      <span class="pet-avatar-ini">{{ iniciales(r.mascota) }}</span>
                    </div>
                    <div>
                      <span class="donor-name">{{ r.mascota }}</span>
                      <span class="donor-mail" v-if="r.animalId">{{ r.animalId }}</span>
                    </div>
                  </div>
                </td>

                <td><span class="fecha-text">{{ getVolunteerName(r.volunteerId) }}</span></td>
                <td><span class="fecha-text">{{ (r.ubicacion || '—').split(' · ')[0] || '—' }}</span></td>
                <td><span class="fecha-text">{{ r.fosterHomeName || r.fosterHomeId || '—' }}</span></td>

                <td>
                  <span class="estado-badge" :class="estadoBadgeClass(r.estado)">{{ r.estado }}</span>
                </td>

                <td>
                  <div class="acciones-cell">
                    <button class="btn-ver" title="Ver detalle" @click="verDetalle(r)">Ver</button>
                    <button class="btn-accion-edit" title="Editar" @click="editarRescate(rescates.indexOf(r))">
                      <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                    </button>
                    <button
                      class="btn-accion-close"
                      title="Cerrar rescate"
                      :disabled="r.estado === 'Cerrado'"
                      @click="pedirCerrar(rescates.indexOf(r))"
                    >
                      <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
                    </button>
                  </div>
                </td>

              </tr>
            </tbody>
          </table>
        </div>
        <div class="table-footer">
          {{ rescatesFiltrados.length }} rescate{{ rescatesFiltrados.length !== 1 ? 's' : '' }} encontrado{{ rescatesFiltrados.length !== 1 ? 's' : '' }}
        </div>
      </div>

    </template>

    <!-- ══════════════════════════════════════════
         VISTA FORMULARIO
    ═══════════════════════════════════════════ -->
    <template v-else>

      <header class="page-header">
        <div>
          <h1 class="admin-page-title">{{ editMode ? 'Editar rescate' : 'Nuevo rescate' }}</h1>
          <p class="admin-page-sub">{{ editMode ? 'Modifica la información del rescate' : 'Registra un nuevo animal rescatado' }}</p>
        </div>
        <button class="btn-cancelar-header" @click="cancelarFormulario">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
          Cancelar
        </button>
      </header>

      <div v-if="formErrors.length > 0" class="form-errors-banner">
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
        <span>Campos obligatorios incompletos: {{ formErrors.join(', ') }}.</span>
      </div>

      <div class="form-panel">

        <!-- ══ Step indicator ══ -->
        <div class="steps-indicator">
          <div class="step" :class="{ 'step--active': currentStep === 1, 'step--done': currentStep > 1 }" @click="currentStep = 1">
            <div class="step-circle">{{ currentStep > 1 ? '✓' : '1' }}</div>
            <span class="step-label">Animal</span>
          </div>
          <div class="steps-line"></div>
          <div class="step" :class="{ 'step--active': currentStep === 2, 'step--done': currentStep > 2 }">
            <div class="step-circle">{{ currentStep > 2 ? '✓' : '2' }}</div>
            <span class="step-label">Casa cuna</span>
          </div>
        </div>

        <!-- ══ Paso 1: Animal ══ -->
        <template v-if="currentStep === 1">
          <div class="form-section-title">
            <span class="form-section-num">1</span>
            Registrar o asociar el animal
          </div>

          <div class="form-grid form-grid--4">
            <div class="fg fg--span2">
              <label class="fg-label">Animal rescatado <span class="req">*</span></label>
              <div style="display:flex;gap:6px;align-items:start">
                <div class="sel-wrap" style="flex:1">
                  <select class="form-input" v-model="animalId" :disabled="animalesLoading">
                    <option value="">Seleccione un animal</option>
                    <option v-for="a in animalesList" :key="a.id" :value="a.id">
                      {{ a.id }} — {{ a.name }} ({{ a.type }})
                    </option>
                  </select>
                  <span class="sel-icon"><svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg></span>
                </div>
                <button type="button" class="btn-agregar-animal" @click="abrirQuickAnimal" title="Crear nueva mascota">
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
                  Agregar
                </button>
              </div>
            </div>
            <div class="fg">
              <label class="fg-label">Fecha de rescate <span class="req">*</span></label>
              <input type="date" class="form-input" v-model="fechaRescate">
            </div>
            <div class="fg">
              <label class="fg-label">Estado</label>
              <div class="sel-wrap">
                <select class="form-input" v-model="estado">
                  <option>Activo</option>
                  <option>Cerrado</option>
                </select>
                <span class="sel-icon"><svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg></span>
              </div>
            </div>
          </div>
          <div class="form-grid form-grid--4" style="margin-top:16px">
            <div class="fg fg--span2">
              <label class="fg-label">Rescatista</label>
              <div class="sel-wrap">
                <select class="form-input" v-model="rescatista">
                  <option value="">Anónimo / Sin asignar</option>
                  <option v-for="v in rescatistasDisponibles" :key="v.id" :value="v.id">
                    {{ v.nombre || v.correo || v.id }}
                  </option>
                </select>
                <span class="sel-icon"><svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg></span>
              </div>
            </div>
          </div>
        </template>

        <!-- ══ Paso 2: Casa cuna + ubicación + descripción ══ -->
        <template v-if="currentStep === 2">
          <div class="form-section-title">
            <span class="form-section-num">2</span>
            Registrar o asociar a una casa cuna
          </div>

          <div class="form-grid form-grid--4">
            <div class="fg fg--span2">
              <label class="fg-label">Casa cuna</label>
              <div style="display:flex;gap:6px;align-items:start">
                <div class="sel-wrap" style="flex:1">
                  <select class="form-input" v-model="casaCuna" :disabled="fhLoading">
                    <option value="">Sin asignar</option>
                    <option v-for="c in fosterHomesList" :key="c.fosterHomeId" :value="c.fosterHomeId">
                      {{ c.name }} — {{ c.responsible }}
                    </option>
                  </select>
                  <span class="sel-icon"><svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg></span>
                </div>
                <button type="button" class="btn-agregar-animal" @click="abrirQuickFoster" title="Crear nueva casa cuna">
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
                  Agregar
                </button>
              </div>
            </div>
          </div>

          <div class="form-section-title" style="margin-top:28px">
            <span class="form-section-num">—</span>
            Ubicación del rescate
          </div>
          <div class="form-grid form-grid--4">
            <div class="fg">
              <label class="fg-label">Provincia <span class="req">*</span></label>
              <div class="sel-wrap">
                <select class="form-input" v-model="provincia">
                  <option value="">Seleccione</option>
                  <option v-for="p in provinciasDisponibles" :key="p" :value="p">{{ p }}</option>
                </select>
                <span class="sel-icon"><svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg></span>
              </div>
            </div>
            <div class="fg">
              <label class="fg-label">Cantón <span class="req">*</span></label>
              <div class="sel-wrap">
                <select class="form-input" v-model="canton" :disabled="!provincia">
                  <option value="">Seleccione</option>
                  <option v-for="c in cantonesDisponibles" :key="c" :value="c">{{ c }}</option>
                </select>
                <span class="sel-icon"><svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg></span>
              </div>
            </div>
            <div class="fg">
              <label class="fg-label">Distrito <span class="req">*</span></label>
              <div class="sel-wrap">
                <select class="form-input" v-model="distrito" :disabled="!canton">
                  <option value="">Seleccione</option>
                  <option v-for="d in distritosDisponibles" :key="d" :value="d">{{ d }}</option>
                </select>
                <span class="sel-icon"><svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg></span>
              </div>
            </div>
          </div>

          <div class="form-section-title" style="margin-top:28px">
            <span class="form-section-num">—</span>
            Descripción del rescate
          </div>
          <div class="fg">
            <label class="fg-label">Descripción <span class="req">*</span></label>
            <textarea class="form-textarea" v-model="descripcion" placeholder="Describe las circunstancias del rescate, condición del animal, observaciones importantes..."></textarea>
          </div>
        </template>

        <div class="form-footer">
          <button class="btn-cancelar" @click="cancelarFormulario">Cancelar</button>
          <button v-if="currentStep === 1" class="btn-guardar" @click="siguientePaso">
            Siguiente
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="9 18 15 12 9 6"/></svg>
          </button>
          <template v-if="currentStep === 2">
            <button class="btn-cancelar" @click="pasoAnterior">Anterior</button>
            <button class="btn-guardar" @click="guardarRescate">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
              {{ editMode ? 'Guardar cambios' : 'Registrar rescate' }}
            </button>
          </template>
        </div>

      </div>
    </template>

    <!-- ══════════════════════════════════════════
         MODAL EDITAR
    ═══════════════════════════════════════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showEditModal" class="modal-overlay" @click.self="showEditModal = false">
          <div class="modal-box modal-box--lg">

            <button class="modal-close" @click="showEditModal = false">✕</button>

            <div class="modal-header">
              <div class="modal-header-avatar">{{ iniciales(mascota) }}</div>
              <div>
                <p class="modal-eyebrow">Editar rescate</p>
                <h2 class="modal-title">{{ mascota || 'Sin nombre' }}</h2>
              </div>
            </div>

            <div class="modal-body">

              <div class="modal-section">
                <h4 class="modal-section-title">Información del rescate</h4>
                <div class="modal-grid">
                  <div class="modal-field">
                    <label class="fg-label">Animal rescatado</label>
                    <div style="display:flex;gap:6px;align-items:start">
                      <div class="sel-wrap" style="flex:1">
                        <select class="form-input" v-model="animalId" :disabled="animalesLoading">
                          <option value="">Seleccione un animal</option>
                          <option v-for="a in animalesList" :key="a.id" :value="a.id">
                            {{ a.id }} — {{ a.name }} ({{ a.type }})
                          </option>
                        </select>
                        <span class="sel-icon"><svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg></span>
                      </div>
                      <button type="button" class="btn-agregar-animal" @click="abrirQuickAnimal" title="Crear nueva mascota">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
                        Agregar
                      </button>
                    </div>
                  </div>
                  <div class="modal-field">
                    <label class="fg-label">Fecha de rescate</label>
                    <input type="date" class="form-input" v-model="fechaRescate">
                  </div>
                  <div class="modal-field">
                    <label class="fg-label">Estado</label>
                    <div class="sel-wrap">
                      <select class="form-input" v-model="estado">
                        <option>Activo</option>
                        <option>Cerrado</option>
                      </select>
                      <span class="sel-icon"><svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg></span>
                    </div>
                  </div>
                  <div class="modal-field">
                    <label class="fg-label">Rescatista</label>
                    <div class="sel-wrap">
                      <select class="form-input" v-model="rescatista">
                        <option value="">Anónimo / Sin asignar</option>
                        <option v-for="v in rescatistasDisponibles" :key="v.id" :value="v.id">
                          {{ v.nombre || v.correo || v.id }}
                        </option>
                      </select>
                      <span class="sel-icon"><svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg></span>
                    </div>
                  </div>
                </div>
              </div>

              <div class="modal-section">
                <h4 class="modal-section-title">Ubicación</h4>
                <div class="modal-grid modal-grid--3">
                  <div class="modal-field">
                    <label class="fg-label">Provincia</label>
                    <div class="sel-wrap">
                      <select class="form-input" v-model="provincia">
                        <option value="">Seleccione</option>
                        <option v-for="p in provinciasDisponibles" :key="p" :value="p">{{ p }}</option>
                      </select>
                      <span class="sel-icon"><svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg></span>
                    </div>
                  </div>
                  <div class="modal-field">
                    <label class="fg-label">Cantón</label>
                    <div class="sel-wrap">
                      <select class="form-input" v-model="canton" :disabled="!provincia">
                        <option value="">Seleccione</option>
                        <option v-for="c in cantonesDisponibles" :key="c" :value="c">{{ c }}</option>
                      </select>
                      <span class="sel-icon"><svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg></span>
                    </div>
                  </div>
                  <div class="modal-field">
                    <label class="fg-label">Distrito</label>
                    <div class="sel-wrap">
                      <select class="form-input" v-model="distrito" :disabled="!canton">
                        <option value="">Seleccione</option>
                        <option v-for="d in distritosDisponibles" :key="d" :value="d">{{ d }}</option>
                      </select>
                      <span class="sel-icon"><svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg></span>
                    </div>
                  </div>
                </div>
              </div>

              <div class="modal-section">
                <h4 class="modal-section-title">Descripción del rescate</h4>
                <textarea class="form-textarea" v-model="descripcion"></textarea>
              </div>

            </div>

            <div class="modal-footer">
              <button class="btn-cancelar" @click="showEditModal = false">Cancelar</button>
              <button class="btn-guardar" @click="guardarRescate">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
                Guardar cambios
              </button>
            </div>

          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- ══════════════════════════════════════════
         MODAL VER DETALLE
    ═══════════════════════════════════════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showDetailModal" class="modal-overlay" @click.self="showDetailModal = false">
          <div class="modal-box modal-box--lg">

            <button class="modal-close" @click="showDetailModal = false">✕</button>

            <div class="modal-header">
              <div class="modal-header-avatar-lg">
                <span>{{ iniciales(rescueSelected?.mascota) }}</span>
              </div>
              <div>
                <p class="modal-eyebrow">Expediente de rescate</p>
                <h2 class="modal-title">{{ rescueSelected?.mascota }}</h2>
                <div class="modal-badges-row">
                  <span class="id-pill">{{ rescueSelected?.animalId }}</span>
                  <span class="estado-badge" :class="estadoBadgeClass(rescueSelected?.estado)">{{ rescueSelected?.estado }}</span>
                  <span class="id-pill">#{{ rescueSelected?.id }}</span>
                </div>
              </div>
            </div>

            <div class="modal-body" v-if="rescueSelected">

              <div class="modal-section">
                <h4 class="modal-section-title">Ubicación</h4>
                <div class="modal-grid">
                  <div class="modal-field">
                    <span class="modal-field-label">Lugar</span>
                    <strong class="modal-field-value">{{ rescueSelected.ubicacion || '—' }}</strong>
                  </div>
                </div>
              </div>

              <div class="modal-section">
                <h4 class="modal-section-title">Fechas</h4>
                <div class="modal-grid">
                  <div class="modal-field">
                    <span class="modal-field-label">Fecha de rescate</span>
                    <strong class="modal-field-value">{{ rescueSelected.fechaRescate || '—' }}</strong>
                  </div>
                </div>
              </div>

              <div class="modal-section">
                <h4 class="modal-section-title">Descripción del rescate</h4>
                <p class="modal-mensaje">{{ rescueSelected.descripcion || '—' }}</p>
              </div>

              <div v-if="rescueSelected?.estado === 'Activo'" class="modal-acciones">
                <button
                  class="btn-cerrar-rescate"
                  @click="showDetailModal = false; pedirCerrar(rescates.indexOf(rescueSelected))"
                >
                  Cerrar rescate
                </button>
              </div>
              <div v-else class="modal-estado-final">
                <p class="estado-cerrado-msg">Este rescate ha sido cerrado.</p>
              </div>

            </div>

            <div class="modal-footer">
              <button class="btn-cancelar" @click="showDetailModal = false">Cerrar expediente</button>
            </div>

          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- ══ MODAL CONFIRMACIÓN CERRAR ══ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="modalConfirm" class="modal-overlay modal-overlay--top" @click.self="modalConfirm = false">
          <div class="modal-box modal-box--sm">
            <div class="confirm-body">
              <div class="confirm-icon">
                <svg width="26" height="26" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
              </div>
              <h3 class="confirm-title">Cerrar rescate</h3>
              <p class="confirm-text">¿Estás seguro de que deseas marcar este rescate como <strong>Cerrado</strong>? Esta acción no se puede deshacer.</p>
            </div>
            <div class="modal-footer">
              <button class="btn-cancelar" @click="modalConfirm = false">Cancelar</button>
              <button class="btn-guardar" @click="confirmarCerrar">Confirmar</button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

  </div>

  <!-- ══════════════════════════════════════════
       MODAL CREAR MASCOTA RÁPIDO
  ═══════════════════════════════════════════ -->
  <Teleport to="body">
    <Transition name="modal-fade">
      <div v-if="showQuickAnimal" class="modal-overlay" @click.self="showQuickAnimal = false">
        <div class="modal-box modal-box--sm">
          <button class="modal-close" @click="showQuickAnimal = false">✕</button>
          <div class="modal-header">
            <div class="modal-header-avatar">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>
            </div>
            <div>
              <p class="modal-eyebrow">Nueva mascota</p>
              <h2 class="modal-title">Crear mascota</h2>
            </div>
          </div>

          <div class="modal-body">
            <div v-if="quickAnimalErrors.length" class="form-errors-banner" style="margin-bottom:16px">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
              <span>Campos obligatorios: {{ quickAnimalErrors.join(', ') }}.</span>
            </div>

            <div class="modal-grid">
              <div class="modal-field">
                <label class="fg-label">Nombre <span class="req">*</span></label>
                <input class="form-input" v-model="quickAnimal.name" placeholder="Ej. Luna">
              </div>
              <div class="modal-field">
                <label class="fg-label">Tipo <span class="req">*</span></label>
                <div class="sel-wrap">
                  <select class="form-input" v-model="quickAnimal.species">
                    <option value="">Seleccione</option>
                    <option value="Perro">Perro</option>
                    <option value="Gato">Gato</option>
                  </select>
                  <span class="sel-icon"><svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg></span>
                </div>
              </div>
              <div class="modal-field">
                <label class="fg-label">Sexo <span class="req">*</span></label>
                <div class="sel-wrap">
                  <select class="form-input" v-model="quickAnimal.sex">
                    <option value="">Seleccione</option>
                    <option value="M">Macho</option>
                    <option value="H">Hembra</option>
                  </select>
                  <span class="sel-icon"><svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg></span>
                </div>
              </div>
              <div class="modal-field">
                <label class="fg-label">Edad (años)</label>
                <input type="number" min="0" class="form-input" v-model="quickAnimal.ageYears" placeholder="0">
              </div>
            </div>
          </div>

          <div class="modal-footer">
            <button class="btn-cancelar" @click="showQuickAnimal = false">Cancelar</button>
            <button class="btn-guardar" :disabled="quickAnimalLoading" @click="guardarQuickAnimal">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
              {{ quickAnimalLoading ? 'Creando...' : 'Crear mascota' }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>

  <!-- ══════════════════════════════════════════
       MODAL CREAR CASA CUNA RÁPIDO
  ═══════════════════════════════════════════ -->
  <Teleport to="body">
    <Transition name="modal-fade">
      <div v-if="showQuickFoster" class="modal-overlay" @click.self="showQuickFoster = false">
        <div class="modal-box modal-box--sm">
          <button class="modal-close" @click="showQuickFoster = false">✕</button>
          <div class="modal-header">
            <div class="modal-header-avatar">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/></svg>
            </div>
            <div>
              <p class="modal-eyebrow">Nueva casa cuna</p>
              <h2 class="modal-title">Crear casa cuna</h2>
            </div>
          </div>

          <div class="modal-body">
            <div v-if="quickFosterErrors.length" class="form-errors-banner" style="margin-bottom:16px">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
              <span>Campos obligatorios: {{ quickFosterErrors.join(', ') }}.</span>
            </div>

            <div class="modal-grid">
              <div class="modal-field">
                <label class="fg-label">Nombre <span class="req">*</span></label>
                <input class="form-input" v-model="quickFoster.name" placeholder="Ej. Hogar temporal San José">
              </div>
              <div class="modal-field">
                <label class="fg-label">Dirección <span class="req">*</span></label>
                <input class="form-input" v-model="quickFoster.address" placeholder="Ej. Av. Central, #123">
              </div>
              <div class="modal-field">
                <label class="fg-label">Teléfono <span class="req">*</span></label>
                <input class="form-input" v-model="quickFoster.phone" placeholder="Ej. 8888-7777">
              </div>
              <div class="modal-field">
                <label class="fg-label">Responsable <span class="req">*</span></label>
                <input class="form-input" v-model="quickFoster.responsible" placeholder="Ej. María Rojas">
              </div>
              <div class="modal-field">
                <label class="fg-label">Capacidad</label>
                <input type="number" min="1" max="50" class="form-input" v-model.number="quickFoster.capacity" placeholder="1">
              </div>
            </div>
          </div>

          <div class="modal-footer">
            <button class="btn-cancelar" @click="showQuickFoster = false">Cancelar</button>
            <button class="btn-guardar" :disabled="quickFosterLoading" @click="guardarQuickFoster">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
              {{ quickFosterLoading ? 'Creando...' : 'Crear casa cuna' }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>

</template>

<style scoped>
/* ══════════════════════════════════════════════
   VARIABLES — sistema de tokens Anhelo Pets
   idéntico al módulo de Donaciones
══════════════════════════════════════════════ */
.view-container {
  --verde:      #3A473C;
  --verde-sec:  #92A894;
  --fondo:      #F7F8F7;
  --blanco:     #FFFFFF;
  --texto:      #2F352F;
  --texto-sec:  #6C756D;
  --borde:      #E8ECE8;
  --amarillo:   #F5B942;
  --verde-ok:   #4CAF6A;
  background: transparent;
}

/* ══════════════════════════════════════════════
   TOAST
══════════════════════════════════════════════ */
.rc-toast {
  position: fixed; bottom: 32px; right: 32px; z-index: 9999;
  display: flex; align-items: center; gap: 10px;
  padding: 14px 20px; border-radius: 14px;
  font-size: 14px; font-weight: 600;
  box-shadow: 0 8px 32px rgba(0,0,0,0.16); pointer-events: none;
}
.toast-success { background: var(--verde); color: #fff; }
.toast-error   { background: #c0392b; color: #fff; }
.toast-anim-enter-active, .toast-anim-leave-active { transition: all 0.25s ease; }
.toast-anim-enter-from, .toast-anim-leave-to { opacity: 0; transform: translateY(10px); }

/* ══════════════════════════════════════════════
   ENCABEZADO — idéntico a Donaciones
══════════════════════════════════════════════ */
.page-header {
  display: flex; justify-content: space-between; align-items: flex-start;
  margin-bottom: 28px; gap: 16px; flex-wrap: wrap;
}
.admin-page-title {
  font-size: 28px; font-weight: 800; color: var(--verde);
  letter-spacing: -0.5px; line-height: 1.1;
}
.admin-page-sub {
  font-size: 14px; color: var(--texto-sec); margin-top: 4px; font-weight: 500;
}

.btn-nuevo {
  display: flex; align-items: center; gap: 8px;
  padding: 10px 20px; background: var(--verde); border: none; border-radius: 8px;
  font-size: 13px; font-weight: 700; color: #fff;
  cursor: pointer; transition: background 0.18s; font-family: inherit;
  flex-shrink: 0; white-space: nowrap;
}
.btn-nuevo:hover { background: #2d3730; }

.btn-cancelar-header {
  display: flex; align-items: center; gap: 8px;
  padding: 10px 18px; background: var(--fondo);
  border: 1.5px solid var(--borde); border-radius: 8px;
  font-size: 13px; font-weight: 700; color: var(--texto-sec);
  cursor: pointer; transition: background 0.15s; font-family: inherit;
  flex-shrink: 0; white-space: nowrap;
}
.btn-cancelar-header:hover { background: #e5eae6; }

/* ══════════════════════════════════════════════
   KPI CARDS — idénticas a Donaciones
══════════════════════════════════════════════ */
.don-summary {
  display: flex; gap: 14px; margin-bottom: 20px; flex-wrap: wrap;
}
.don-card {
  flex: 1; min-width: 150px;
  background: var(--blanco); border-radius: 14px; padding: 20px;
  border: 1px solid var(--borde); border-top: 3px solid var(--borde);
  display: flex; flex-direction: column; gap: 8px;
}
.kpi-mes      { border-top-color: var(--amarillo); }
.kpi-total    { border-top-color: var(--verde-sec); }
.kpi-activos  { border-top-color: var(--amarillo); }
.kpi-cerrados { border-top-color: var(--verde-ok); }

.don-label {
  font-size: 11px; color: var(--texto-sec); font-weight: 700;
  text-transform: uppercase; letter-spacing: 0.5px;
}
.don-value {
  font-size: 24px; font-weight: 800; color: var(--verde); line-height: 1;
}

/* ══════════════════════════════════════════════
   PANEL DE FILTROS — idéntico a Donaciones
══════════════════════════════════════════════ */
.filtros-panel {
  background: var(--blanco); border-radius: 14px; padding: 20px;
  margin-bottom: 20px; border: 1px solid var(--borde);
  display: flex; gap: 12px; flex-wrap: wrap; align-items: flex-end;
}
.filtro-group {
  display: flex; flex-direction: column; gap: 6px; flex: 1; min-width: 130px;
}
.filtro-group--btn { flex: 0 0 auto; min-width: unset; }
.filtro-label {
  font-size: 11px; font-weight: 700; color: var(--verde);
  text-transform: uppercase; letter-spacing: 0.5px;
  min-height: 16px; display: flex; align-items: flex-end;
}
.filtro-input-wrap {
  position: relative; display: flex; align-items: center;
}
.filtro-input {
  width: 100%; height: 38px; padding: 0 36px 0 12px;
  border-radius: 8px; border: 1.5px solid var(--borde);
  background: var(--fondo); font-size: 13px; color: var(--texto);
  font-family: inherit; outline: none;
  transition: border-color 0.18s, background 0.18s; box-sizing: border-box;
}
.filtro-input:focus { border-color: var(--verde-sec); background: var(--blanco); }
.filtro-input::placeholder { color: #9CA8A0; }
.filtro-select { appearance: none; -webkit-appearance: none; cursor: pointer; }
.filtro-icon { position: absolute; display: flex; align-items: center; color: var(--texto-sec); }
.filtro-icon--right { right: 11px; }
.filtro-icon--no-events { pointer-events: none; }

.btn-limpiar {
  height: 38px; padding: 0 16px; border-radius: 8px;
  border: 1.5px solid var(--borde); background: transparent;
  color: var(--texto-sec); font-size: 12px; font-weight: 700;
  cursor: pointer; white-space: nowrap; transition: all 0.18s; font-family: inherit;
}
.btn-limpiar--activo { border-color: var(--verde); color: var(--verde); }
.btn-limpiar:hover   { background: var(--verde); color: var(--blanco); border-color: var(--verde); }

/* ══════════════════════════════════════════════
   ESTADO VACÍO — idéntico a Donaciones
══════════════════════════════════════════════ */
.empty-state {
  text-align: center; padding: 72px 24px;
  background: var(--blanco); border-radius: 14px; border: 1px solid var(--borde);
}
.empty-title { font-size: 16px; font-weight: 700; color: var(--texto); margin-bottom: 6px; }
.empty-sub   { font-size: 13px; color: var(--texto-sec); margin: 0; }

/* ══════════════════════════════════════════════
   TABLA — idéntica a Donaciones
══════════════════════════════════════════════ */
.table-wrapper {
  background: var(--blanco); border-radius: 14px;
  border: 1px solid var(--borde); overflow: hidden;
}
.table-scroll { overflow-x: auto; -webkit-overflow-scrolling: touch; }

.don-table { width: 100%; border-collapse: collapse; min-width: 700px; }
.don-table thead tr { background: var(--verde); }
.don-table thead th {
  padding: 13px 16px; text-align: left; color: var(--blanco);
  font-size: 11px; font-weight: 700; text-transform: uppercase;
  letter-spacing: 0.6px; white-space: nowrap;
}
.don-table tbody tr { border-bottom: 1px solid var(--borde); transition: background 0.15s; }
.don-table tbody tr:last-child { border-bottom: none; }
.don-table tbody tr:hover { background: #F4F6F4; }
.don-table tbody td { padding: 13px 16px; vertical-align: middle; }

.table-footer {
  padding: 12px 16px; border-top: 1px solid var(--borde);
  font-size: 12px; color: var(--texto-sec); font-weight: 500;
}

/* Celda mascota con avatar */
.pet-cell { display: flex; align-items: center; gap: 10px; }
.pet-avatar {
  width: 36px; height: 36px; border-radius: 50%; overflow: hidden; flex-shrink: 0;
  background: #DDE6DE; display: flex; align-items: center; justify-content: center;
}
.pet-avatar-img { width: 100%; height: 100%; object-fit: cover; }
.pet-avatar-ini { font-size: 13px; font-weight: 800; color: #5A6E5C; }

/* Tipografías de celda — idénticas a Donaciones */
.id-pill {
  font-size: 11px; font-family: monospace; background: var(--fondo);
  border: 1px solid var(--borde); padding: 3px 9px; border-radius: 6px;
  color: var(--verde); font-weight: 700; white-space: nowrap;
}
.donor-name { display: block; font-size: 13px; font-weight: 700; color: var(--texto); line-height: 1.3; }
.donor-mail { display: block; font-size: 11px; color: var(--texto-sec); margin-top: 2px; }
.metodo-text { font-size: 13px; color: var(--texto-sec); }
.fecha-text  { font-size: 13px; color: var(--texto-sec); }

/* Acciones en tabla */
.acciones-cell { display: flex; align-items: center; gap: 6px; }

.btn-ver {
  padding: 6px 14px; border-radius: 7px;
  border: 1.5px solid var(--borde); background: var(--blanco);
  color: var(--verde); font-size: 12px; font-weight: 700;
  cursor: pointer; transition: all 0.18s; white-space: nowrap; font-family: inherit;
}
.btn-ver:hover { background: var(--verde); color: var(--blanco); border-color: var(--verde); }

.btn-accion-edit,
.btn-accion-close {
  display: inline-flex; align-items: center; justify-content: center;
  width: 30px; height: 30px; border-radius: 7px; border: none;
  cursor: pointer; transition: background 0.15s, opacity 0.15s; flex-shrink: 0;
}
.btn-accion-edit  { background: rgba(33,150,243,.10); color: #1565C0; }
.btn-accion-edit:hover  { background: rgba(33,150,243,.22); }
.btn-accion-close { background: rgba(176,0,32,.10); color: #B71C1C; }
.btn-accion-close:not(:disabled):hover { background: rgba(176,0,32,.20); }
.btn-accion-close:disabled { opacity: 0.3; cursor: not-allowed; }

/* ══════════════════════════════════════════════
   BADGES — pill idéntico a Donaciones
══════════════════════════════════════════════ */
.estado-badge {
  display: inline-block; font-size: 11px; font-weight: 700;
  padding: 4px 12px; border-radius: 20px; white-space: nowrap;
}
.badge-activo  { background: #E8F5E9; color: #2E7D32; }
.badge-cerrado { background: #F4F6F4; color: #6C756D; }

/* ══════════════════════════════════════════════
   FORMULARIO
══════════════════════════════════════════════ */
.form-errors-banner {
  display: flex; align-items: flex-start; gap: 10px;
  padding: 14px 18px; background: rgba(176,0,32,.08);
  border: 1.5px solid rgba(176,0,32,.20); border-radius: 12px;
  font-size: 13px; color: #B71C1C; margin-bottom: 20px; line-height: 1.5;
}
.form-errors-banner svg { flex-shrink: 0; margin-top: 1px; }

.form-panel {
  background: var(--blanco); border-radius: 14px;
  border: 1px solid var(--borde); padding: 32px 32px 28px;
}

.form-section-title {
  display: flex; align-items: center; gap: 10px;
  font-size: 11px; font-weight: 700; color: var(--verde);
  text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 16px;
}
.form-section-num {
  width: 22px; height: 22px; border-radius: 6px;
  background: var(--verde); color: #fff;
  font-size: 11px; font-weight: 800;
  display: flex; align-items: center; justify-content: center; flex-shrink: 0;
}

.req { color: #B71C1C; margin-left: 2px; }

.form-grid { display: grid; gap: 14px; }
.form-grid--4 { grid-template-columns: repeat(4, 1fr); }
.fg { display: flex; flex-direction: column; gap: 6px; }
.fg--span2 { grid-column: span 2; }
.fg--full  { grid-column: 1 / -1; }
.fg-label {
  font-size: 11px; font-weight: 700; color: var(--verde);
  text-transform: uppercase; letter-spacing: 0.5px;
}

.form-input {
  width: 100%; height: 38px; padding: 0 36px 0 12px; box-sizing: border-box;
  border: 1.5px solid var(--borde); border-radius: 8px;
  font-size: 13px; color: var(--texto); background: var(--fondo);
  outline: none; font-family: inherit; transition: border-color 0.18s, background 0.18s;
}
.form-input:focus    { border-color: var(--verde-sec); background: var(--blanco); }
.form-input:disabled { background: #F4F6F4; color: #9BA99C; cursor: not-allowed; }

.form-textarea {
  width: 100%; padding: 10px 13px; box-sizing: border-box;
  border: 1.5px solid var(--borde); border-radius: 8px;
  font-size: 13px; color: var(--texto); background: var(--fondo);
  outline: none; font-family: inherit; min-height: 100px; resize: vertical; line-height: 1.5;
  transition: border-color 0.18s, background 0.18s;
}
.form-textarea:focus { border-color: var(--verde-sec); background: var(--blanco); }

.sel-wrap { position: relative; }
.sel-wrap .form-input { appearance: none; -webkit-appearance: none; cursor: pointer; padding-right: 36px; }
.sel-icon {
  position: absolute; right: 11px; top: 50%; transform: translateY(-50%);
  color: var(--texto-sec); pointer-events: none; display: flex; align-items: center;
}

.radio-row { display: flex; gap: 16px; align-items: center; padding-top: 4px; }
.r-opt {
  display: flex; align-items: center; gap: 7px;
  font-size: 13px; font-weight: 600; color: var(--texto); cursor: pointer;
}
.r-opt input[type="radio"] { accent-color: var(--verde); width: 15px; height: 15px; cursor: pointer; }

.foto-wrap { display: flex; align-items: center; gap: 20px; }
.foto-preview {
  width: 100px; height: 100px; border-radius: 12px;
  border: 2px dashed var(--borde); background: var(--fondo);
  display: flex; align-items: center; justify-content: center;
  overflow: hidden; flex-shrink: 0;
}
.foto-preview.has-img { border-style: solid; border-color: var(--verde-sec); }
.foto-img { width: 100%; height: 100%; object-fit: cover; }
.foto-placeholder { display: flex; flex-direction: column; align-items: center; gap: 6px; color: var(--verde-sec); }
.foto-placeholder span { font-size: 11px; font-weight: 600; }
.foto-actions { display: flex; flex-direction: column; gap: 8px; }
.btn-foto {
  display: inline-flex; align-items: center; gap: 8px;
  padding: 9px 16px; background: var(--fondo); border: 1.5px solid var(--borde);
  border-radius: 8px; font-size: 13px; font-weight: 700; color: var(--texto);
  cursor: pointer; transition: background 0.15s; font-family: inherit;
}
.btn-foto:hover { background: #e5eae6; }
.foto-hint { font-size: 11px; color: var(--verde-sec); margin: 0; }

.form-footer {
  display: flex; justify-content: flex-end; gap: 10px;
  padding-top: 24px; margin-top: 24px; border-top: 1px solid var(--borde);
}

/* ══════════════════════════════════════════════
   STEP INDICATOR
══════════════════════════════════════════════ */
.steps-indicator {
  display: flex; align-items: center; justify-content: center; gap: 0;
  margin-bottom: 28px; padding: 0 20px;
}
.step {
  display: flex; flex-direction: column; align-items: center; gap: 6px;
  cursor: pointer; user-select: none;
}
.step-circle {
  width: 34px; height: 34px; border-radius: 50%;
  display: flex; align-items: center; justify-content: center;
  font-size: 14px; font-weight: 800;
  background: var(--fondo); color: var(--verde-sec);
  border: 2px solid var(--borde); transition: all 0.2s;
}
.step--active .step-circle {
  background: var(--verde); color: #fff; border-color: var(--verde);
}
.step--done .step-circle {
  background: var(--verde); color: #fff; border-color: var(--verde);
}
.step-label {
  font-size: 11px; font-weight: 700; color: var(--texto-sec);
  text-transform: uppercase; letter-spacing: 0.3px;
}
.step--active .step-label { color: var(--verde); }
.steps-line {
  flex: 1; max-width: 80px; height: 2px;
  background: var(--borde); margin: 0 12px; margin-bottom: 22px;
}

/* ══════════════════════════════════════════════
   BOTONES COMUNES
══════════════════════════════════════════════ */
.btn-agregar-animal {
  display: inline-flex; align-items: center; gap: 4px;
  padding: 7px 12px; background: var(--verde); border: none; border-radius: 8px;
  font-size: 12px; font-weight: 700; color: #fff; cursor: pointer;
  white-space: nowrap; font-family: inherit; transition: background 0.15s; height: 38px;
}
.btn-agregar-animal:hover { background: #2d3730; }

.btn-cancelar {
  padding: 10px 18px; background: var(--fondo); border: none; border-radius: 8px;
  font-size: 13px; font-weight: 700; color: var(--texto-sec);
  cursor: pointer; transition: background 0.15s; font-family: inherit;
}
.btn-cancelar:hover { background: #e5eae6; }

.btn-guardar {
  display: flex; align-items: center; gap: 7px; padding: 10px 20px;
  background: var(--verde); border: none; border-radius: 8px;
  font-size: 13px; font-weight: 700; color: #fff;
  cursor: pointer; transition: background 0.18s; font-family: inherit;
}
.btn-guardar:hover { background: #2d3730; }

/* ══════════════════════════════════════════════
   MODALES — idénticos a Donaciones
══════════════════════════════════════════════ */
.modal-overlay {
  position: fixed; inset: 0;
  background: rgba(0,0,0,0.35); backdrop-filter: blur(4px);
  z-index: 1000; display: flex; align-items: center; justify-content: center;
  padding: 24px; overflow-y: auto;
}
.modal-overlay--top { z-index: 1100; }

.modal-box {
  background: #FFFFFF; border-radius: 20px; width: 100%;
  max-height: 90vh; overflow-y: auto; position: relative;
  box-shadow: 0 24px 80px rgba(0,0,0,0.18); margin: auto;
}
.modal-box--sm { max-width: 420px; }
.modal-box--lg { max-width: 680px; }

.modal-close {
  position: absolute; top: 18px; right: 18px;
  width: 32px; height: 32px; border-radius: 50%;
  border: none; background: var(--fondo); color: var(--texto);
  font-size: 13px; font-weight: 700; cursor: pointer;
  display: flex; align-items: center; justify-content: center;
  transition: all 0.2s; font-family: inherit; z-index: 1;
}
.modal-close:hover { background: var(--verde); color: var(--blanco); }

.modal-header {
  display: flex; align-items: center; gap: 14px;
  padding: 26px 28px 22px; border-bottom: 1px solid var(--borde);
}
.modal-header-avatar {
  width: 44px; height: 44px; min-width: 44px; border-radius: 12px;
  background: #DDE6DE; color: #5A6E5C;
  font-size: 16px; font-weight: 800;
  display: flex; align-items: center; justify-content: center;
}
.modal-header-avatar-lg {
  width: 56px; height: 56px; min-width: 56px; border-radius: 14px;
  background: #DDE6DE; overflow: hidden;
  display: flex; align-items: center; justify-content: center;
  font-size: 20px; font-weight: 800; color: #5A6E5C; flex-shrink: 0;
}
.modal-avatar-img { width: 100%; height: 100%; object-fit: cover; }
.modal-eyebrow {
  font-size: 11px; font-weight: 700; color: var(--texto-sec);
  text-transform: uppercase; letter-spacing: 0.7px; margin-bottom: 4px;
}
.modal-title {
  font-size: 20px; font-weight: 800; color: var(--verde);
  letter-spacing: -0.4px; margin-bottom: 8px;
}
.modal-badges-row { display: flex; gap: 6px; flex-wrap: wrap; align-items: center; }

.modal-body { padding: 24px 28px 8px; }

.modal-section { margin-bottom: 24px; }
.modal-section-title {
  font-size: 11px; font-weight: 700; color: var(--texto-sec);
  text-transform: uppercase; letter-spacing: 0.5px;
  margin-bottom: 14px; padding-bottom: 10px; border-bottom: 1px solid var(--borde);
}

.modal-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 16px; }
.modal-grid--3 { grid-template-columns: repeat(3, 1fr); }
.modal-field  { display: flex; flex-direction: column; gap: 4px; }
.modal-field-label {
  font-size: 10px; font-weight: 700; color: #9CA8A0;
  text-transform: uppercase; letter-spacing: 0.4px;
}
.modal-field-value { font-size: 14px; color: var(--texto); font-weight: 600; word-break: break-word; }

.modal-mensaje {
  font-size: 14px; color: var(--texto); line-height: 1.7;
  background: var(--fondo); border-radius: 10px; padding: 14px 16px; margin: 0;
}

.modal-acciones {
  display: flex; gap: 10px;
  padding-top: 20px; border-top: 1px solid var(--borde); margin-top: 8px;
}
.btn-cerrar-rescate {
  flex: 1; padding: 13px; border-radius: 10px; border: none;
  background: #FDECEA; color: #B71C1C;
  font-size: 13px; font-weight: 700; cursor: pointer;
  transition: all 0.2s; font-family: inherit;
}
.btn-cerrar-rescate:hover { background: #B71C1C; color: var(--blanco); }

.modal-estado-final {
  padding-top: 20px; border-top: 1px solid var(--borde); text-align: center;
}
.estado-cerrado-msg { color: var(--texto-sec); font-weight: 700; font-size: 14px; }

.modal-footer {
  display: flex; justify-content: flex-end; gap: 10px;
  padding: 18px 28px 24px; border-top: 1px solid var(--borde); margin-top: 12px;
}

/* ══════════════════════════════════════════════
   CONFIRMACIÓN
══════════════════════════════════════════════ */
.confirm-body { padding: 32px 28px 8px; text-align: center; }
.confirm-icon {
  width: 60px; height: 60px; border-radius: 50%;
  background: #EEF2EE; color: var(--verde);
  display: flex; align-items: center; justify-content: center; margin: 0 auto 18px;
}
.confirm-title { font-size: 18px; font-weight: 800; color: var(--verde); margin-bottom: 10px; }
.confirm-text {
  font-size: 13px; color: var(--texto-sec); line-height: 1.6;
  max-width: 320px; margin: 0 auto;
}

/* ══════════════════════════════════════════════
   ANIMACIONES
══════════════════════════════════════════════ */
.modal-fade-enter-active, .modal-fade-leave-active { transition: opacity 0.22s ease; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity: 0; }

/* ══════════════════════════════════════════════
   RESPONSIVE
══════════════════════════════════════════════ */
@media (max-width: 900px) {
  .don-summary { display: grid; grid-template-columns: repeat(2, 1fr); }
  .kpi-cerrados { grid-column: span 2; }
  .modal-grid--3 { grid-template-columns: repeat(2, 1fr); }
}

@media (max-width: 640px) {
  .page-header { flex-direction: column; align-items: flex-start; }
  .filtros-panel { flex-direction: column; }
  .filtro-group { min-width: 100%; }
  .filtro-group--btn { width: 100%; }
  .btn-limpiar { width: 100%; justify-content: center; }
  .form-grid--4 { grid-template-columns: repeat(2, 1fr); }
  .fg--span2 { grid-column: 1; }
  .fg--full  { grid-column: 1; }
  .form-panel { padding: 20px 18px; }
  .modal-grid { grid-template-columns: 1fr; }
  .modal-grid--3 { grid-template-columns: 1fr 1fr; }
  .modal-body { padding: 16px 18px 8px; }
  .modal-header { padding: 20px 18px 16px; }
  .modal-footer { padding: 14px 18px 20px; }
  .modal-acciones { flex-direction: column; }
  .don-summary { grid-template-columns: 1fr; }
  .kpi-cerrados { grid-column: span 1; }
}

@media (max-width: 480px) {
  .modal-grid--3 { grid-template-columns: 1fr; }
}


/* ── MOBILE RESPONSIVE ── */
@media (max-width: 768px) {
  .don-summary {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 10px;
  }

  .kpi-cerrados {
    grid-column: span 2;
  }

  .filtros-panel {
    flex-direction: column;
    gap: 10px;
    padding: 14px;
  }

  .filtro-group,
  .filtro-group--btn {
    min-width: unset;
    width: 100%;
  }

  .btn-limpiar {
    width: 100%;
    justify-content: center;
  }

  .table-scroll {
    overflow-x: auto;
    -webkit-overflow-scrolling: touch;
  }

  .page-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 10px;
  }

  .btn-nuevo {
    width: 100%;
    justify-content: center;
  }

  .btn-cancelar-header {
    width: 100%;
    justify-content: center;
  }

  .form-panel {
    padding: 20px 14px;
  }

  .form-grid--4 {
    grid-template-columns: repeat(2, 1fr);
  }

  .fg--span2 { grid-column: span 1; }
  .fg--full  { grid-column: span 2; }

  .foto-wrap {
    flex-direction: column;
    align-items: flex-start;
  }

  .modal-box--lg {
    max-width: calc(100vw - 24px);
    max-height: 95vh;
    padding: 22px 14px;
  }

  .modal-body { padding: 16px 0 0; }

  .modal-grid { grid-template-columns: 1fr; }
  .modal-grid--3 { grid-template-columns: 1fr 1fr; }

  .modal-header { flex-wrap: wrap; gap: 10px; }

  .modal-footer {
    padding: 14px 0 0;
    flex-direction: column;
  }

  .modal-footer .btn-cancelar,
  .modal-footer .btn-guardar {
    width: 100%;
    justify-content: center;
  }

  .acciones-cell { flex-wrap: wrap; gap: 4px; }

  .form-footer {
    flex-direction: column;
    gap: 8px;
  }

  .form-footer .btn-cancelar,
  .form-footer .btn-guardar {
    width: 100%;
    justify-content: center;
  }
}

@media (max-width: 480px) {
  .don-summary { grid-template-columns: 1fr; }
  .kpi-cerrados { grid-column: span 1; }

  .form-grid--4 { grid-template-columns: 1fr; }
  .fg--span2,
  .fg--full { grid-column: span 1; }

  .modal-grid--3 { grid-template-columns: 1fr; }
}


</style>