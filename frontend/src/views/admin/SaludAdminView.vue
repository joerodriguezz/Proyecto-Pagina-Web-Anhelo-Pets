<script setup>
import { ref, computed, watch } from 'vue'
import { usePetsStore } from '../../stores/usePetsStore'

const store = usePetsStore()

// ── Veterinarios ──

const veterinarios = ref([])

function cargarVeterinarios() {
  const usuarios =
    JSON.parse(localStorage.getItem('anhelo_usuarios')) || []

  veterinarios.value = usuarios.filter(
    u =>
      u.rol === 'Voluntario' &&
      u.solicitudVoluntario?.estado === 'Aprobada' &&
      (
        u.tipoVoluntario === 'Veterinaria' ||
        u.solicitudVoluntario?.tipo === 'Veterinaria'
      )
  )
}

cargarVeterinarios()

// ── Tabs ──
const activeTab = ref('historial')

// ── Modales ──
const showModalRegistrar = ref(false)
const showModalVer       = ref(false)
const showModalConfirm   = ref(false)

const showPetDropdown = ref(false)
const showVetDropdown = ref(false)
const showVetDropdownVacuna = ref(false)

const registroVer = ref(null)

// ── Toast ──
const toast = ref({ show: false, type: 'success', message: '' })
let toastTimer = null
function showToast(type, message) {
  clearTimeout(toastTimer)
  toast.value = { show: true, type, message }
  toastTimer = setTimeout(() => { toast.value.show = false }, 3500)
}

// ── Mascota seleccionada ──
const petSeleccionada = ref(null)

// ── Errores ──
const errores = ref({})

// ── Persistencia ──
const STORAGE_KEY = 'anhelo_salud_v3'
function cargarDatos() {
  try { return JSON.parse(localStorage.getItem(STORAGE_KEY)) || {} }
  catch { return {} }
}
function guardarDatos(d) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(d))
}
const datos = ref(cargarDatos())

watch(() => store.pets, (pets) => {
  pets.forEach(pet => {
    if (!datos.value[pet.id]) {
      datos.value[pet.id] = { medicalHistory: [], vaccines: [], treatments: [] }
    }
  })
  guardarDatos(datos.value)
}, { immediate: true, deep: true })

// ── Formularios (único formulario unificado) ──
const form = ref({
  fecha: '',
  vet: '',
  clinica: '',
  peso: '',
  diagnostico: '',
  observaciones_h: '',

  tipoVacuna: '',
  fechaAplicacion: '',
  proximaDosis: '',
  vetVacuna: '',
  clinicaVacuna: '',
  observaciones_v: '',

  tipoTratamiento: '',
  medicamento: '',
  dosis: '',
  fechaTrat: '',
  observaciones_t: ''
})

function resetForm() {
  form.value = {
    fecha: '',
    vet: '',
    clinica: '',
    peso: '',
    diagnostico: '',
    observaciones_h: '',

    tipoVacuna: '',
    fechaAplicacion: '',
    proximaDosis: '',
    vetVacuna: '',
    clinicaVacuna: '',
    observaciones_v: '',

    tipoTratamiento: '',
    medicamento: '',
    dosis: '',
    fechaTrat: '',
    observaciones_t: ''
  }

  petSeleccionada.value = null

  showPetDropdown.value = false
  showVetDropdown.value = false
  showVetDropdownVacuna.value = false

  errores.value = {}
}

// ── Filtros ──
const search     = ref('')
const filterFrom = ref('')
const filterTo   = ref('')

// ── Registros por tab ──
const registros = computed(() => {
  const todos = []
  store.pets.forEach(pet => {
    const d = datos.value[pet.id]
    if (!d) return
    const lista =
      activeTab.value === 'historial'   ? d.medicalHistory :
      activeTab.value === 'vacunas'     ? d.vaccines       :
                                          d.treatments
    lista.forEach(r => todos.push({
      ...r,
      petId:      pet.id,
      petNombre:  pet.name,
      petEspecie: pet.species || '',
      petFoto:
        pet.images?.[0]?.preview ||
        pet.foto ||
        pet.image ||
        pet.photo ||
        pet.avatar ||
  null,
      petActiva:  pet.active !== false
    }))
  })

  let result = todos.sort((a, b) => {
    const fa = a.fecha || a.fechaAplicacion || ''
    const fb = b.fecha || b.fechaAplicacion || ''
    return fb.localeCompare(fa)
  })

  // filtro búsqueda
  const q = search.value.trim().toLowerCase()
  if (q) {
    result = result.filter(r =>
      r.petNombre?.toLowerCase().includes(q) ||
      r.petId?.toString().toLowerCase().includes(q)
    )
  }

  // filtro fechas — usa el campo de fecha correcto según tab
  if (filterFrom.value || filterTo.value) {
    result = result.filter(r => {
      const fecha = r.fecha || r.fechaAplicacion || ''
      if (!fecha) return true
      if (filterFrom.value && fecha < filterFrom.value) return false
      if (filterTo.value   && fecha > filterTo.value)   return false
      return true
    })
  }

  return result
})

const hayFiltros = computed(() =>
  search.value.trim() !== '' || filterFrom.value !== '' || filterTo.value !== ''
)

// ── Generador de ID ──
function generarId(prefijo, lista) {
  return `${prefijo}-${String(lista.length + 1).padStart(3, '0')}`
}

// ── Validación ──
function validar() {
  const e = {}
  if (!petSeleccionada.value) e.pet = 'Selecciona una mascota'
  if (!form.value.fecha)                    e.fecha           = 'Obligatorio'
  if (!form.value.vet?.trim())              e.vet             = 'Obligatorio'
  if (!form.value.diagnostico?.trim())      e.diagnostico     = 'Obligatorio'
  if (!form.value.tipoVacuna?.trim())       e.tipoVacuna      = 'Obligatorio'
  if (!form.value.fechaAplicacion)          e.fechaAplicacion = 'Obligatorio'
  if (!form.value.tipoTratamiento?.trim())  e.tipoTratamiento = 'Obligatorio'
  if (!form.value.fechaTrat)                e.fechaTrat       = 'Obligatorio'
  errores.value = e
  return Object.keys(e).length === 0
}

function clearErr(campo) {
  if (errores.value[campo]) {
    const e = { ...errores.value }
    delete e[campo]
    errores.value = e
  }
}

function intentarGuardar() {
  if (!validar()) return
  showModalConfirm.value = true
}

function confirmarGuardar() {
  showModalConfirm.value = false
  const pid = petSeleccionada.value?.id
  if (!pid) return
  if (!datos.value[pid]) datos.value[pid] = { medicalHistory: [], vaccines: [], treatments: [] }

  try {
    const d = datos.value[pid]
    // historial
    d.medicalHistory.push({
      id:           generarId('SAL', d.medicalHistory),
      fecha:        form.value.fecha,
      vet:          form.value.vet,
      peso:         form.value.peso,
      diagnostico:  form.value.diagnostico,
      observaciones: form.value.observaciones_h,
      creadoEn:     new Date().toISOString()
    })
    // vacuna
    d.vaccines.push({
      id:              generarId('VAC', d.vaccines),
      tipo:            form.value.tipoVacuna,
      fechaAplicacion: form.value.fechaAplicacion,
      proximaDosis:    form.value.proximaDosis,
      vet:             form.value.vetVacuna,
      observaciones:   form.value.observaciones_v,
      creadoEn:        new Date().toISOString()
    })
    // tratamiento
    d.treatments.push({
      id:           generarId('TRA', d.treatments),
      tipo:         form.value.tipoTratamiento,
      medicamento:  form.value.medicamento,
      dosis:        form.value.dosis,
      fecha:        form.value.fechaTrat,
      observaciones: form.value.observaciones_t,
      creadoEn:     new Date().toISOString()
    })

    guardarDatos(datos.value)
    resetForm()
    showModalRegistrar.value = false
    showToast('success', 'Expediente médico guardado correctamente')
  } catch {
    showToast('error', 'Error al guardar. Intenta de nuevo.')
  }
}

function abrirModal() {
  resetForm()
  showModalRegistrar.value = true
}

function seleccionarPet(pet) {
  petSeleccionada.value = pet
  showPetDropdown.value = false
  clearErr('pet')
}

function verRegistro(r) {
  registroVer.value = r
  showModalVer.value = true
}

function formatFecha(f) {
  if (!f) return '—'
  const [y, m, d] = f.split('-')
  const meses = ['ene','feb','mar','abr','may','jun','jul','ago','sep','oct','nov','dic']
  return `${d} ${meses[parseInt(m)-1]} ${y}`
}

const mascotasActivas = computed(() => store.pets.filter(p => p.active !== false))
</script>

<template>
  <div class="sc-root">

    <!-- ── Toast ── -->
    <Teleport to="body">
      <Transition name="toast-anim">
        <div v-if="toast.show" class="sc-toast" :class="toast.type">
          <span class="sc-toast-dot"></span>
          {{ toast.message }}
        </div>
      </Transition>
    </Teleport>

    <!-- ── Header ── -->
    <header class="sc-header">
      <div class="sc-header-left">
        <h1 class="sc-title">Control de Salud</h1>
        <p class="sc-sub">Historial médico, vacunas y tratamientos</p>
      </div>
      <button class="sc-btn-new" @click="abrirModal">
        <svg xmlns="http://www.w3.org/2000/svg" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
        Nuevo expediente
      </button>
    </header>

    <!-- ── Tabs + Filtros ── -->
    <div class="sc-toolbar">
      <div class="sc-tabs">
        <button class="sc-tab" :class="{ active: activeTab === 'historial' }"    @click="activeTab = 'historial'">
          <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/><polyline points="10 9 9 9 8 9"/></svg>
          Historial
        </button>
        <button class="sc-tab" :class="{ active: activeTab === 'vacunas' }"      @click="activeTab = 'vacunas'">
          <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M22 12h-4l-3 9L9 3l-3 9H2"/></svg>
          Vacunas
        </button>
        <button class="sc-tab" :class="{ active: activeTab === 'tratamientos' }" @click="activeTab = 'tratamientos'">
          <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/></svg>
          Tratamientos
        </button>
      </div>

      <div class="sc-filters">
        <div class="sc-search-wrap">
          <svg class="sc-search-icon" xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
          <input class="sc-search" v-model="search" placeholder="Nombre o ID de mascota..." />
        </div>
        <div class="sc-date-range">
          <input type="date" class="sc-date" v-model="filterFrom" title="Desde" />
          <span class="sc-date-sep">→</span>
          <input type="date" class="sc-date" v-model="filterTo" title="Hasta" />
        </div>
        <button v-if="hayFiltros" class="sc-clear" @click="search = ''; filterFrom = ''; filterTo = ''">
          Limpiar
        </button>
      </div>
    </div>

    <!-- ── Tabla Historial ── -->
    <div v-if="activeTab === 'historial'" class="sc-table-wrap">
      <table class="sc-table">
        <thead>
          <tr>
            <th>Id Registro</th>
            <th>Mascota</th>
            <th>Fecha</th>
            <th>Veterinario</th>
            <th>Diagnóstico</th>
            <th>Peso</th>
            <th>Observaciones</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="r in registros" :key="r.id">
            <td>
  <span class="sc-id-badge">
    {{ r.id }}
  </span>
</td>

<td>
  <div class="sc-pet-cell">
    <div class="sc-avatar">
      <img v-if="r.petFoto" :src="r.petFoto" class="sc-avatar-img" />
      <span v-else class="sc-avatar-ini">{{ r.petNombre?.charAt(0) }}</span>
    </div>
    <div class="sc-pet-info">
      <span class="sc-pet-name">{{ r.petNombre }}</span>
      <span class="sc-pet-id">{{ r.petId }}</span>
    </div>
  </div>
</td>

<td>
  <span class="sc-date-badge">{{ formatFecha(r.fecha) }}</span>
</td>
            <td class="sc-td-sec">{{ r.vet || '—' }}</td>
            <td class="sc-td-main">{{ r.diagnostico }}</td>
            <td class="sc-td-sec">{{ r.peso ? r.peso + ' kg' : '—' }}</td>
            <td class="sc-td-obs">{{ r.observaciones || '—' }}</td>
            <td><button class="sc-btn-ver" @click="verRegistro(r)">Ver</button></td>
          </tr>
          <tr v-if="registros.length === 0">
            <td colspan="7" class="sc-empty">
              <div class="sc-empty-inner">
                <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
                <p>{{ hayFiltros ? 'Sin resultados para los filtros aplicados' : 'Sin registros de historial médico' }}</p>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- ── Tabla Vacunas ── -->
    <div v-if="activeTab === 'vacunas'" class="sc-table-wrap">
      <table class="sc-table">
        <thead>
          <tr>
            <th>Id Registro</th>
            <th>Mascota</th>
            <th>Vacuna</th>
            <th>Aplicación</th>
            <th>Próxima dosis</th>
            <th>Veterinario</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="r in registros" :key="r.id">
            <td>
  <span class="sc-id-badge">
    {{ r.id }}
  </span>
</td>

<td>
  <div class="sc-pet-cell">
    <div class="sc-avatar">
      <img v-if="r.petFoto" :src="r.petFoto" class="sc-avatar-img" />
      <span v-else class="sc-avatar-ini">{{ r.petNombre?.charAt(0) }}</span>
    </div>
    <div class="sc-pet-info">
      <span class="sc-pet-name">{{ r.petNombre }}</span>
      <span class="sc-pet-id">{{ r.petId }}</span>
    </div>
  </div>
</td>

<td class="sc-td-main">{{ r.tipo }}</td>
            <td><span class="sc-date-badge">{{ formatFecha(r.fechaAplicacion) }}</span></td>
            <td>
              <span v-if="r.proximaDosis" class="sc-date-badge sc-date-badge--green">{{ formatFecha(r.proximaDosis) }}</span>
              <span v-else class="sc-td-sec">—</span>
            </td>
            <td class="sc-td-sec">{{ r.vet || '—' }}</td>
            <td><button class="sc-btn-ver" @click="verRegistro(r)">Ver</button></td>
          </tr>
          <tr v-if="registros.length === 0">
            <td colspan="6" class="sc-empty">
              <div class="sc-empty-inner">
                <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M22 12h-4l-3 9L9 3l-3 9H2"/></svg>
                <p>{{ hayFiltros ? 'Sin resultados para los filtros aplicados' : 'Sin registros de vacunas' }}</p>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- ── Tabla Tratamientos ── -->
    <div v-if="activeTab === 'tratamientos'" class="sc-table-wrap">
      <table class="sc-table">
        <thead>
          <tr>
            <th>Id Registro</th>
            <th>Mascota</th>
            <th>Tratamiento</th>
            <th>Fecha</th>
            <th>Medicamento</th>
            <th>Dosis</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="r in registros" :key="r.id">
            <td>
  <span class="sc-id-badge">
    {{ r.id }}
  </span>
</td>

<td>
  <div class="sc-pet-cell">
    <div class="sc-avatar">
      <img v-if="r.petFoto" :src="r.petFoto" class="sc-avatar-img" />
      <span v-else class="sc-avatar-ini">{{ r.petNombre?.charAt(0) }}</span>
    </div>
    <div class="sc-pet-info">
      <span class="sc-pet-name">{{ r.petNombre }}</span>
      <span class="sc-pet-id">{{ r.petId }}</span>
    </div>
  </div>
</td>

<td class="sc-td-main">{{ r.tipo }}</td>
            <td><span class="sc-date-badge">{{ formatFecha(r.fecha) }}</span></td>
            <td class="sc-td-sec">{{ r.medicamento || '—' }}</td>
            <td class="sc-td-sec">{{ r.dosis || '—' }}</td>
            <td><button class="sc-btn-ver" @click="verRegistro(r)">Ver</button></td>
          </tr>
          <tr v-if="registros.length === 0">
            <td colspan="6" class="sc-empty">
              <div class="sc-empty-inner">
                <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/></svg>
                <p>{{ hayFiltros ? 'Sin resultados para los filtros aplicados' : 'Sin registros de tratamientos' }}</p>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- ══════════════════════════════════════
         Modal Registrar — formulario unificado
    ══════════════════════════════════════ -->
    <Teleport to="body">
      <Transition name="overlay-anim">
        <div v-if="showModalRegistrar" class="sc-overlay" @click.self="showModalRegistrar = false">
          <div class="sc-modal sc-modal--lg">

            <div class="sc-modal-header">
              <div>
                <p class="sc-modal-eyebrow">Expediente médico</p>
                <h2 class="sc-modal-title">Nuevo registro completo</h2>
              </div>
              <button class="sc-modal-close" @click="showModalRegistrar = false">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
              </button>
            </div>

            <div class="sc-modal-body">

              <!-- Selector mascota -->
              <div class="sc-section-label">
                <span class="sc-section-num">→</span> Mascota
              </div>
              <div class="sc-pet-selector-wrap" style="margin-bottom:28px">
                <button
                  type="button"
                  class="sc-pet-selector-btn"
                  :class="{ 'is-error': errores.pet }"
                  @click="showPetDropdown = !showPetDropdown"
                >
                  <template v-if="petSeleccionada">
                    <div class="sc-avatar sc-avatar--sm">
                      <img v-if="petSeleccionada.foto || petSeleccionada.image || petSeleccionada.photo || petSeleccionada.avatar" :src="petSeleccionada.foto || petSeleccionada.image || petSeleccionada.photo || petSeleccionada.avatar" class="sc-avatar-img" />
                      <span v-else class="sc-avatar-ini">{{ petSeleccionada.name?.charAt(0) }}</span>
                    </div>
                    <span class="sc-psel-name">{{ petSeleccionada.name }}</span>
                    <span class="sc-psel-species">{{ petSeleccionada.species }}</span>
                  </template>
                  <template v-else>
                    <span class="sc-psel-placeholder">Seleccionar mascota...</span>
                  </template>
                  <svg class="sc-chevron" :class="{ open: showPetDropdown }" xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
                </button>
                <p v-if="errores.pet" class="sc-err-msg">{{ errores.pet }}</p>
                <div v-if="showPetDropdown" class="sc-pet-dropdown">
                  <div v-if="mascotasActivas.length === 0" class="sc-dropdown-empty">No hay mascotas activas registradas</div>
                  <div
                    v-for="pet in mascotasActivas"
                    :key="pet.id"
                    class="sc-dropdown-item"
                    :class="{ selected: petSeleccionada?.id === pet.id }"
                    @click="seleccionarPet(pet)"
                  >
                    <div class="sc-avatar sc-avatar--sm">
                      <img v-if="pet.foto || pet.image || pet.photo || pet.avatar" :src="pet.foto || pet.image || pet.photo || pet.avatar" class="sc-avatar-img" />
                      <span v-else class="sc-avatar-ini">{{ pet.name?.charAt(0) }}</span>
                    </div>
                    <div class="sc-dropdown-pet-info">
                      <span class="sc-dropdown-name">{{ pet.name }}</span>
                      <span class="sc-dropdown-species">{{ pet.species }}</span>
                    </div>
                    <svg v-if="petSeleccionada?.id === pet.id" class="sc-check" xmlns="http://www.w3.org/2000/svg" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
                  </div>
                </div>
              </div>

              <!-- ── Sección Historial médico ── -->
              <div class="sc-section-label">
                <span class="sc-section-num">1</span> Historial médico
              </div>
              <div class="sc-form-grid sc-form-grid--4" style="margin-bottom:28px">
                <div class="sc-fg">
                  <label>Fecha <span class="sc-req">*</span></label>
                  <input type="date" class="sc-input" :class="{ 'is-error': errores.fecha }" v-model="form.fecha" @change="clearErr('fecha')" />
                  <p v-if="errores.fecha" class="sc-err-msg">{{ errores.fecha }}</p>
                </div>
                <div class="sc-fg">
                  <label>Peso (kg)</label>
                  <input type="number" class="sc-input" placeholder="Ej. 12.5" step="0.1" min="0" v-model="form.peso" />
                </div>
                
           <!-- Veterinario -->
<div class="sc-fg">
  <label>Veterinario responsable <span class="sc-req">*</span></label>

  <div class="sc-pet-selector-wrap">

    <button
      type="button"
      class="sc-pet-selector-btn"
      :class="{ 'is-error': errores.vet }"
      @click="showVetDropdown = !showVetDropdown"
    >
      <template v-if="form.vet">
        <div class="sc-avatar sc-avatar--sm">
          <span class="sc-avatar-ini">
            {{ form.vet.charAt(0) }}
          </span>
        </div>

        <span class="sc-psel-name">
          {{ form.vet }}
        </span>
      </template>

      <template v-else>
        <span class="sc-psel-placeholder">
          Seleccionar veterinario...
        </span>
      </template>

      <svg
        class="sc-chevron"
        :class="{ open: showVetDropdown }"
        xmlns="http://www.w3.org/2000/svg"
        width="16"
        height="16"
        viewBox="0 0 24 24"
        fill="none"
        stroke="currentColor"
        stroke-width="2.5"
      >
        <polyline points="6 9 12 15 18 9"/>
      </svg>
    </button>

    <div
      v-if="showVetDropdown"
      class="sc-pet-dropdown"
    >
      <div
        v-for="vet in veterinarios"
        :key="vet.id"
        class="sc-dropdown-item"
        @click="
          form.vet = vet.nombre;
          showVetDropdown = false;
          clearErr('vet');
        "
      >
        <div class="sc-avatar sc-avatar--sm">
          <span class="sc-avatar-ini">
            {{ vet.nombre?.charAt(0) }}
          </span>
        </div>

        <div class="sc-dropdown-pet-info">
          <span class="sc-dropdown-name">
            Dr. {{ vet.nombre }}
          </span>

          <span class="sc-dropdown-species">
            Veterinario
          </span>
        </div>
      </div>
    </div>

  </div>

  <p v-if="errores.vet" class="sc-err-msg">
    {{ errores.vet }}
  </p>
</div>

<!-- Clínica -->
<div class="sc-fg">
  <label>Clínica veterinaria</label>

  <input
    type="text"
    class="sc-input"
    placeholder="Ej. Hospital Veterinario San José"
    v-model="form.clinica"
  />
</div>     

  
                <div class="sc-fg sc-fg--full">
                  <label>Diagnóstico <span class="sc-req">*</span></label>
                  <input type="text" class="sc-input" :class="{ 'is-error': errores.diagnostico }" placeholder="Ej. Control preventivo, otitis externa..." v-model="form.diagnostico" @input="clearErr('diagnostico')" />
                  <p v-if="errores.diagnostico" class="sc-err-msg">{{ errores.diagnostico }}</p>
                </div>
                <div class="sc-fg sc-fg--full">
                  <label>Observaciones</label>
                  <textarea class="sc-textarea" placeholder="Indicaciones, seguimiento, notas clínicas..." v-model="form.observaciones_h"></textarea>
                </div>
              </div>

              <!-- ── Sección Vacuna ── -->
              <div class="sc-section-label">
                <span class="sc-section-num">2</span> Vacuna
              </div>
              <div class="sc-form-grid sc-form-grid--4" style="margin-bottom:28px">
                <div class="sc-fg sc-fg--span2">
                  <label>Tipo de vacuna <span class="sc-req">*</span></label>
                  <input type="text" class="sc-input" :class="{ 'is-error': errores.tipoVacuna }" placeholder="Ej. Antirrábica, Parvovirus..." v-model="form.tipoVacuna" @input="clearErr('tipoVacuna')" />
                  <p v-if="errores.tipoVacuna" class="sc-err-msg">{{ errores.tipoVacuna }}</p>
                </div>
                <div class="sc-fg">
                  <label>Fecha de aplicación <span class="sc-req">*</span></label>
                  <input type="date" class="sc-input" :class="{ 'is-error': errores.fechaAplicacion }" v-model="form.fechaAplicacion" @change="clearErr('fechaAplicacion')" />
                  <p v-if="errores.fechaAplicacion" class="sc-err-msg">{{ errores.fechaAplicacion }}</p>
                </div>
                <div class="sc-fg">
                  <label>Próxima dosis</label>
                  <input type="date" class="sc-input" v-model="form.proximaDosis" />
                </div>
               <div class="sc-fg sc-fg--span2">
  <label>Veterinario responsable</label>

  <div class="sc-pet-selector-wrap">

    <button
      type="button"
      class="sc-pet-selector-btn"
      @click="showVetDropdownVacuna = !showVetDropdownVacuna"
    >
      <template v-if="form.vetVacuna">
        <div class="sc-avatar sc-avatar--sm">
          <span class="sc-avatar-ini">
            {{ form.vetVacuna.charAt(0) }}
          </span>
        </div>

        <span class="sc-psel-name">
          {{ form.vetVacuna }}
        </span>
      </template>

      <template v-else>
        <span class="sc-psel-placeholder">
          Seleccionar veterinario...
        </span>
      </template>

      <svg
        class="sc-chevron"
        :class="{ open: showVetDropdownVacuna }"
        xmlns="http://www.w3.org/2000/svg"
        width="16"
        height="16"
        viewBox="0 0 24 24"
        fill="none"
        stroke="currentColor"
        stroke-width="2.5"
      >
        <polyline points="6 9 12 15 18 9"/>
      </svg>
    </button>

    <div
      v-if="showVetDropdownVacuna"
      class="sc-pet-dropdown"
    >
      <div
        v-for="vet in veterinarios"
        :key="vet.id"
        class="sc-dropdown-item"
        @click="
          form.vetVacuna = vet.nombre;
          showVetDropdownVacuna = false;
        "
      >
        <div class="sc-avatar sc-avatar--sm">
          <span class="sc-avatar-ini">
            {{ vet.nombre?.charAt(0) }}
          </span>
        </div>

        <div class="sc-dropdown-pet-info">
          <span class="sc-dropdown-name">
            Dr. {{ vet.nombre }}
          </span>

          <span class="sc-dropdown-species">
            Veterinario
          </span>
        </div>
      </div>
    </div>

  </div>
</div>
                <div class="sc-fg sc-fg--full">
                  <label>Observaciones</label>
                  <textarea class="sc-textarea" placeholder="Notas sobre la vacuna, lote, reacciones..." v-model="form.observaciones_v"></textarea>
                </div>
              </div>

              <!-- ── Sección Tratamiento ── -->
              <div class="sc-section-label">
                <span class="sc-section-num">3</span> Tratamiento
              </div>
              <div class="sc-form-grid sc-form-grid--4" style="margin-bottom:20px">
                <div class="sc-fg sc-fg--span2">
                  <label>Tipo de tratamiento <span class="sc-req">*</span></label>
                  <input type="text" class="sc-input" :class="{ 'is-error': errores.tipoTratamiento }" placeholder="Ej. Desparasitación, antibiótico..." v-model="form.tipoTratamiento" @input="clearErr('tipoTratamiento')" />
                  <p v-if="errores.tipoTratamiento" class="sc-err-msg">{{ errores.tipoTratamiento }}</p>
                </div>
                <div class="sc-fg">
                  <label>Fecha <span class="sc-req">*</span></label>
                  <input type="date" class="sc-input" :class="{ 'is-error': errores.fechaTrat }" v-model="form.fechaTrat" @change="clearErr('fechaTrat')" />
                  <p v-if="errores.fechaTrat" class="sc-err-msg">{{ errores.fechaTrat }}</p>
                </div>
                <div class="sc-fg">
                  <label>Dosis</label>
                  <input type="text" class="sc-input" placeholder="Ej. 5mg/kg" v-model="form.dosis" />
                </div>
                <div class="sc-fg sc-fg--span2">
                  <label>Medicamento</label>
                  <input type="text" class="sc-input" placeholder="Nombre del medicamento" v-model="form.medicamento" />
                </div>
                <div class="sc-fg sc-fg--full">
                  <label>Observaciones</label>
                  <textarea class="sc-textarea" placeholder="Duración, respuesta al tratamiento, seguimiento..." v-model="form.observaciones_t"></textarea>
                </div>
              </div>

              <div class="sc-immutable-note">
                <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
                Los registros médicos son permanentes y no pueden editarse ni eliminarse una vez guardados
              </div>
            </div>

            <div class="sc-modal-footer">
              <button class="sc-btn-cancel" @click="showModalRegistrar = false">Cancelar</button>
              <button class="sc-btn-save" @click="intentarGuardar">
                <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
                Guardar expediente
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- ── Modal Confirmación ── -->
    <Teleport to="body">
      <Transition name="overlay-anim">
        <div v-if="showModalConfirm" class="sc-overlay sc-overlay--top" @click.self="showModalConfirm = false">
          <div class="sc-modal sc-modal--sm">
            <div class="sc-confirm-body">
              <div class="sc-confirm-icon">
                <svg xmlns="http://www.w3.org/2000/svg" width="26" height="26" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>
              </div>
              <h3 class="sc-confirm-title">¿Guardar este expediente?</h3>
              <p class="sc-confirm-text">Se registrará el historial médico, la vacuna y el tratamiento para <strong>{{ petSeleccionada?.name }}</strong>. Esta acción es permanente y no podrá modificarse.</p>
            </div>
            <div class="sc-modal-footer">
              <button class="sc-btn-cancel" @click="showModalConfirm = false">Cancelar</button>
              <button class="sc-btn-save" @click="confirmarGuardar">Confirmar y guardar</button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- ── Modal Ver ── -->
    <Teleport to="body">
      <Transition name="overlay-anim">
        <div v-if="showModalVer && registroVer" class="sc-overlay" @click.self="showModalVer = false">
          <div class="sc-modal sc-modal--md">
            <div class="sc-modal-header">
              <div>
                <p class="sc-modal-eyebrow">Detalle</p>
                <h2 class="sc-modal-title">{{ registroVer.id }}</h2>
              </div>
              <button class="sc-modal-close" @click="showModalVer = false">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
              </button>
            </div>
            <div class="sc-modal-body">

              <!-- Mascota -->
              <div class="sc-ver-pet">
                <div class="sc-avatar sc-avatar--lg">
                  <img v-if="registroVer.petFoto" :src="registroVer.petFoto" class="sc-avatar-img" />
                  <span v-else class="sc-avatar-ini">{{ registroVer.petNombre?.charAt(0) }}</span>
                </div>
                <div>
                  <p class="sc-ver-pname">{{ registroVer.petNombre }}</p>
                  <p class="sc-ver-pspec">{{ registroVer.petEspecie }}</p>
                </div>
                <div class="sc-ver-badges">
                  <span class="sc-id-badge">{{ registroVer.petId }}</span>
                  <span v-if="!registroVer.petActiva" class="sc-inactive-badge">Inactiva</span>
                </div>
              </div>

              <!-- Contenido según tipo -->
              <div class="sc-ver-card">
                <!-- Historial -->
                <template v-if="registroVer.id?.startsWith('SAL')">
                  <div class="sc-ver-row">
                    <span class="sc-ver-label">Fecha</span>
                    <span class="sc-date-badge">{{ formatFecha(registroVer.fecha) }}</span>
                  </div>
                  <div v-if="registroVer.vet" class="sc-ver-row">
                    <span class="sc-ver-label">Veterinario</span>
                    <span class="sc-ver-val">{{ registroVer.vet }}</span>
                  </div>
                  <div class="sc-ver-row sc-ver-row--block">
                    <span class="sc-ver-label">Diagnóstico</span>
                    <span class="sc-ver-val sc-ver-val--bold">{{ registroVer.diagnostico }}</span>
                  </div>
                  <div v-if="registroVer.peso" class="sc-ver-row">
                    <span class="sc-ver-label">Peso</span>
                    <span class="sc-ver-val">{{ registroVer.peso }} kg</span>
                  </div>
                  <div v-if="registroVer.observaciones" class="sc-ver-row sc-ver-row--block">
                    <span class="sc-ver-label">Observaciones</span>
                    <span class="sc-ver-val sc-ver-val--obs">{{ registroVer.observaciones }}</span>
                  </div>
                </template>

                <!-- Vacuna -->
                <template v-if="registroVer.id?.startsWith('VAC')">
                  <div class="sc-ver-row">
                    <span class="sc-ver-label">Vacuna</span>
                    <span class="sc-ver-val sc-ver-val--bold">{{ registroVer.tipo }}</span>
                  </div>
                  <div class="sc-ver-row">
                    <span class="sc-ver-label">Aplicación</span>
                    <span class="sc-date-badge">{{ formatFecha(registroVer.fechaAplicacion) }}</span>
                  </div>
                  <div v-if="registroVer.proximaDosis" class="sc-ver-row">
                    <span class="sc-ver-label">Próxima dosis</span>
                    <span class="sc-date-badge sc-date-badge--green">{{ formatFecha(registroVer.proximaDosis) }}</span>
                  </div>
                  <div v-if="registroVer.vet" class="sc-ver-row">
                    <span class="sc-ver-label">Veterinario</span>
                    <span class="sc-ver-val">{{ registroVer.vet }}</span>
                  </div>
                  <div v-if="registroVer.observaciones" class="sc-ver-row sc-ver-row--block">
                    <span class="sc-ver-label">Observaciones</span>
                    <span class="sc-ver-val sc-ver-val--obs">{{ registroVer.observaciones }}</span>
                  </div>
                </template>

                <!-- Tratamiento -->
                <template v-if="registroVer.id?.startsWith('TRA')">
                  <div class="sc-ver-row">
                    <span class="sc-ver-label">Tratamiento</span>
                    <span class="sc-ver-val sc-ver-val--bold">{{ registroVer.tipo }}</span>
                  </div>
                  <div class="sc-ver-row">
                    <span class="sc-ver-label">Fecha</span>
                    <span class="sc-date-badge">{{ formatFecha(registroVer.fecha) }}</span>
                  </div>
                  <div v-if="registroVer.medicamento" class="sc-ver-row">
                    <span class="sc-ver-label">Medicamento</span>
                    <span class="sc-ver-val">{{ registroVer.medicamento }}</span>
                  </div>
                  <div v-if="registroVer.dosis" class="sc-ver-row">
                    <span class="sc-ver-label">Dosis</span>
                    <span class="sc-ver-val">{{ registroVer.dosis }}</span>
                  </div>
                  <div v-if="registroVer.observaciones" class="sc-ver-row sc-ver-row--block">
                    <span class="sc-ver-label">Observaciones</span>
                    <span class="sc-ver-val sc-ver-val--obs">{{ registroVer.observaciones }}</span>
                  </div>
                </template>
              </div>
            </div>
            <div class="sc-modal-footer">
              <button class="sc-btn-cancel" @click="showModalVer = false">Cerrar</button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

  </div>
</template>

<style scoped>
/* ═══════════════════════════════════════
   ROOT
═══════════════════════════════════════ */
.sc-root {
  background: transparent;
  padding-bottom: 40px;
}

/* ═══════════════════════════════════════
   TOAST
═══════════════════════════════════════ */
.sc-toast {
  position: fixed;
  bottom: 32px;
  right: 32px;
  z-index: 9999;
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 14px 20px;
  border-radius: 14px;
  font-size: 14px;
  font-weight: 600;
  box-shadow: 0 8px 32px rgba(0,0,0,0.16);
  pointer-events: none;
}
.sc-toast.success { background: #3A473C; color: #fff; }
.sc-toast.error   { background: #c0392b; color: #fff; }
.sc-toast-dot {
  width: 8px; height: 8px;
  border-radius: 50%;
  background: rgba(255,255,255,0.5);
  flex-shrink: 0;
}
.toast-anim-enter-active, .toast-anim-leave-active { transition: all 0.25s ease; }
.toast-anim-enter-from, .toast-anim-leave-to { opacity: 0; transform: translateY(10px); }

/* ═══════════════════════════════════════
   HEADER
═══════════════════════════════════════ */
.sc-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 28px;
  gap: 16px;
  flex-wrap: wrap;
}
.sc-title {
  font-size: 28px;
  font-weight: 800;
  color: #3A473C;
  letter-spacing: -0.5px;
  line-height: 1.1;
}
.sc-sub {
  font-size: 14px;
  color: #6C756D;
  margin-top: 5px;
  font-weight: 500;
}
.sc-btn-new {
  display: flex;
  align-items: center;
  gap: 7px;
  padding: 11px 20px;
  background: #3A473C;
  color: #fff;
  border: none;
  border-radius: 12px;
  font-size: 14px;
  font-weight: 700;
  cursor: pointer;
  transition: background 0.18s, transform 0.1s;
  white-space: nowrap;
  flex-shrink: 0;
}
.sc-btn-new:hover { background: #2d3730; }
.sc-btn-new:active { transform: scale(0.97); }

/* ═══════════════════════════════════════
   TOOLBAR (tabs + filtros)
═══════════════════════════════════════ */
.sc-toolbar {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 20px;
  flex-wrap: wrap;
}
.sc-tabs {
  display: flex;
  gap: 4px;
  background: #F4F6F4;
  border-radius: 12px;
  padding: 4px;
  flex-shrink: 0;
}
.sc-tab {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 16px;
  border-radius: 9px;
  border: none;
  background: transparent;
  color: #6C756D;
  font-size: 13px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.18s;
  white-space: nowrap;
}
.sc-tab:hover { color: #3A473C; background: rgba(255,255,255,0.6); }
.sc-tab.active { background: #fff; color: #3A473C; box-shadow: 0 1px 4px rgba(58,71,60,0.12); }

.sc-filters {
  display: flex;
  align-items: center;
  gap: 10px;
  flex: 1;
  flex-wrap: wrap;
}
.sc-search-wrap {
  position: relative;
  flex: 1;
  min-width: 180px;
}
.sc-search-icon {
  position: absolute;
  left: 12px;
  top: 50%;
  transform: translateY(-50%);
  color: #92A894;
  pointer-events: none;
}
.sc-search {
  width: 100%;
  box-sizing: border-box;
  padding: 9px 12px 9px 34px;
  border: 1.5px solid #E8ECE8;
  border-radius: 10px;
  font-size: 13px;
  color: #3A473C;
  background: #fff;
  outline: none;
  font-family: inherit;
  transition: border-color 0.18s;
}
.sc-search:focus { border-color: #92A894; }
.sc-date-range {
  display: flex;
  align-items: center;
  gap: 6px;
  flex-shrink: 0;
}
.sc-date-sep { font-size: 12px; color: #92A894; font-weight: 700; }
.sc-date {
  padding: 8px 10px;
  border: 1.5px solid #E8ECE8;
  border-radius: 10px;
  font-size: 12px;
  color: #3A473C;
  background: #fff;
  outline: none;
  font-family: inherit;
  transition: border-color 0.18s;
}
.sc-date:focus { border-color: #92A894; }
.sc-clear {
  padding: 8px 14px;
  border: 1.5px solid #fdd;
  border-radius: 10px;
  background: #fff5f5;
  color: #c0392b;
  font-size: 12px;
  font-weight: 700;
  cursor: pointer;
  transition: background 0.15s;
  white-space: nowrap;
}
.sc-clear:hover { background: #ffe5e5; }

/* ═══════════════════════════════════════
   TABLA
═══════════════════════════════════════ */
.sc-table-wrap {
  background: #fff;
  border-radius: 20px;
  box-shadow: 0 2px 16px rgba(58,71,60,0.06);
  overflow: hidden;
}
.sc-table {
  width: 100%;
  border-collapse: collapse;
}
.sc-table thead { background: #F9FAF9; }
.sc-table th {
  padding: 14px 20px;
  font-size: 11px;
  font-weight: 800;
  color: #92A894;
  text-transform: uppercase;
  letter-spacing: 0.6px;
  white-space: nowrap;
  border-bottom: 1.5px solid #F0F2F0;
}
.sc-table td {
  padding: 14px 20px;
  font-size: 14px;
  color: #3A473C;
  border-bottom: 1px solid #F5F7F5;
  vertical-align: middle;
}
.sc-table tbody tr:last-child td { border-bottom: none; }
.sc-table tbody tr { transition: background 0.12s; }
.sc-table tbody tr:hover { background: #FAFBFA; }

/* Avatar */
.sc-avatar {
  width: 38px;
  height: 38px;
  border-radius: 50%;
  overflow: hidden;
  flex-shrink: 0;
  background: #DDE6DE;
  display: flex;
  align-items: center;
  justify-content: center;
}
.sc-avatar--sm { width: 32px; height: 32px; }
.sc-avatar--lg { width: 48px; height: 48px; }
.sc-avatar-img { width: 100%; height: 100%; object-fit: cover; display: block; }
.sc-avatar-ini {
  font-size: 14px;
  font-weight: 800;
  color: #5A6E5C;
  text-transform: uppercase;
  line-height: 1;
}
.sc-avatar--sm .sc-avatar-ini { font-size: 12px; }
.sc-avatar--lg .sc-avatar-ini { font-size: 18px; }

.sc-pet-cell { display: flex; align-items: center; gap: 10px; }
.sc-pet-info  { display: flex; flex-direction: column; gap: 2px; }
.sc-pet-name  { font-weight: 700; font-size: 14px; color: #3A473C; }
.sc-pet-id    { font-size: 11px; color: #92A894; font-family: monospace; }

.sc-date-badge {
  display: inline-block;
  padding: 4px 10px;
  background: #F0F4F0;
  color: #4A6550;
  border-radius: 7px;
  font-size: 12px;
  font-weight: 600;
  white-space: nowrap;
}
.sc-date-badge--green { background: #E8F4EA; color: #2E6B38; }

.sc-td-main {
  font-weight: 600;
  color: #3A473C;

  max-width: 250px;
  overflow: hidden;
  text-overflow: ellipsis;

  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
}
.sc-td-sec  { color: #7A8A7C; font-size: 13px; }
.sc-td-obs {
  color: #7A8A7C;
  font-size: 13px;

  max-width: 220px;

  overflow: hidden;
  text-overflow: ellipsis;

  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
}

.sc-btn-ver {
  padding: 6px 14px;
  background: #F0F4F0;
  color: #3A473C;
  border: none;
  border-radius: 8px;
  font-size: 12px;
  font-weight: 700;
  cursor: pointer;
  transition: background 0.15s;
  white-space: nowrap;
}
.sc-btn-ver:hover { background: #DDE6DE; }

.sc-empty { padding: 0; }
.sc-empty-inner {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  padding: 56px 24px;
  color: #92A894;
}
.sc-empty-inner svg { opacity: 0.4; }
.sc-empty-inner p {
  font-size: 14px;
  font-weight: 500;
  color: #7A8A7C;
  margin: 0;
}

/* ═══════════════════════════════════════
   OVERLAY / MODAL
═══════════════════════════════════════ */
.sc-overlay {
  position: fixed;
  inset: 0;
  background: rgba(20, 30, 22, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 200;
  padding: 20px;
  backdrop-filter: blur(2px);
}
.sc-overlay--top { z-index: 400; }

.overlay-anim-enter-active, .overlay-anim-leave-active { transition: all 0.22s ease; }
.overlay-anim-enter-from, .overlay-anim-leave-to { opacity: 0; }
.overlay-anim-enter-from .sc-modal, .overlay-anim-leave-to .sc-modal {
  transform: translateY(16px) scale(0.98);
}
.sc-modal {
  background: #fff;
  border-radius: 22px;
  width: 100%;
  max-height: 88vh;
  overflow-y: auto;
  box-shadow: 0 24px 80px rgba(0,0,0,0.2);
  transition: transform 0.22s ease;
}
.sc-modal--sm { max-width: 420px; }
.sc-modal--md { max-width: 520px; }
.sc-modal--lg { max-width: 900px; }

.sc-modal-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  padding: 24px 28px 0;
  border-bottom: 1.5px solid #F0F2F0;
  padding-bottom: 18px;
}
.sc-modal-eyebrow {
  font-size: 11px;
  font-weight: 800;
  color: #92A894;
  text-transform: uppercase;
  letter-spacing: 0.7px;
  margin-bottom: 4px;
}
.sc-modal-title {
  font-size: 20px;
  font-weight: 800;
  color: #3A473C;
  letter-spacing: -0.4px;
}
.sc-modal-close {
  width: 34px;
  height: 34px;
  border-radius: 10px;
  border: 1.5px solid #E8ECE8;
  background: #fff;
  color: #6C756D;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background 0.15s, border-color 0.15s;
  flex-shrink: 0;
}
.sc-modal-close:hover { background: #F4F6F4; border-color: #ccc; }

.sc-modal-body { padding: 24px 28px 8px; }

/* Sección label */
.sc-section-label {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 13px;
  font-weight: 800;
  color: #3A473C;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin-bottom: 14px;
}
.sc-section-num {
  width: 24px;
  height: 24px;
  border-radius: 7px;
  background: #3A473C;
  color: #fff;
  font-size: 11px;
  font-weight: 800;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

/* Grid formulario */
.sc-form-grid {
  display: grid;
  gap: 14px;
}
.sc-form-grid--4 { grid-template-columns: repeat(4, 1fr); }
.sc-fg { display: flex; flex-direction: column; gap: 6px; }
.sc-fg--span2 { grid-column: span 2; }
.sc-fg--full  { grid-column: 1 / -1; }

.sc-fg label {
  font-size: 12px;
  font-weight: 700;
  color: #5A6E5C;
  letter-spacing: 0.1px;
}
.sc-req { color: #c0392b; }

.sc-input {
  padding: 10px 13px;
  border: 1.5px solid #E8ECE8;
  border-radius: 10px;
  font-size: 13px;
  color: #3A473C;
  background: #FAFBFA;
  outline: none;
  font-family: inherit;
  transition: border-color 0.18s, background 0.18s;
  width: 100%;
  box-sizing: border-box;
}
.sc-input:focus { border-color: #92A894; background: #fff; }
.sc-input.is-error { border-color: #e57373; background: #fff8f8; }

.sc-textarea {
  padding: 10px 13px;
  border: 1.5px solid #E8ECE8;
  border-radius: 10px;
  font-size: 13px;
  color: #3A473C;
  background: #FAFBFA;
  outline: none;
  font-family: inherit;
  transition: border-color 0.18s, background 0.18s;
  width: 100%;
  box-sizing: border-box;
  height: 80px;
  resize: vertical;
  line-height: 1.5;
}
.sc-textarea:focus { border-color: #92A894; background: #fff; }

.sc-err-msg {
  font-size: 11px;
  color: #c0392b;
  font-weight: 600;
  margin: 0;
}

/* Pet selector */
.sc-pet-selector-wrap { position: relative; }
.sc-pet-selector-btn {
  width: 100%;
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 13px;
  border: 1.5px solid #E8ECE8;
  border-radius: 10px;
  background: #FAFBFA;
  cursor: pointer;
  font-family: inherit;
  font-size: 13px;
  color: #3A473C;
  text-align: left;
  transition: border-color 0.18s, background 0.18s;
  box-sizing: border-box;
}
.sc-pet-selector-btn:hover,
.sc-pet-selector-btn:focus { border-color: #92A894; background: #fff; outline: none; }
.sc-pet-selector-btn.is-error { border-color: #e57373; }
.sc-psel-placeholder { color: #9CA8A0; flex: 1; font-size: 13px; }
.sc-psel-name { font-weight: 700; flex: 1; }
.sc-psel-species { font-size: 12px; color: #92A894; }
.sc-chevron { margin-left: auto; color: #92A894; transition: transform 0.18s; flex-shrink: 0; }
.sc-chevron.open { transform: rotate(180deg); }

.sc-pet-dropdown {
  position: absolute;
  top: calc(100% + 6px);
  left: 0; right: 0;
  background: #fff;
  border: 1.5px solid #E8ECE8;
  border-radius: 14px;
  box-shadow: 0 8px 32px rgba(58,71,60,0.14);
  z-index: 500;
  max-height: 240px;
  overflow-y: auto;
  padding: 6px;
}
.sc-dropdown-empty {
  padding: 16px;
  text-align: center;
  font-size: 13px;
  color: #92A894;
}
.sc-dropdown-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 9px 10px;
  border-radius: 9px;
  cursor: pointer;
  transition: background 0.12s;
}
.sc-dropdown-item:hover { background: #F4F6F4; }
.sc-dropdown-item.selected { background: #EEF2EE; }
.sc-dropdown-pet-info { display: flex; flex-direction: column; gap: 1px; flex: 1; min-width: 0; }
.sc-dropdown-name    { font-size: 14px; font-weight: 700; color: #3A473C; }
.sc-dropdown-species { font-size: 11px; color: #92A894; }
.sc-check { color: #92A894; flex-shrink: 0; }

/* Immutable note */
.sc-immutable-note {
  display: flex;
  align-items: flex-start;
  gap: 8px;
  padding: 12px 14px;
  background: #FFFBF2;
  border-radius: 10px;
  border-left: 3px solid #F9C17A;
  font-size: 12px;
  color: #996C2A;
  font-weight: 600;
  line-height: 1.4;
  margin-top: 4px;
}
.sc-immutable-note svg { flex-shrink: 0; margin-top: 1px; }

/* Modal footer */
.sc-modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  padding: 18px 28px 24px;
  border-top: 1.5px solid #F0F2F0;
  margin-top: 12px;
}
.sc-btn-cancel {
  padding: 10px 18px;
  background: #F4F6F4;
  border: none;
  border-radius: 10px;
  font-size: 13px;
  font-weight: 700;
  color: #6C756D;
  cursor: pointer;
  transition: background 0.15s;
}
.sc-btn-cancel:hover { background: #E5EAE6; }
.sc-btn-save {
  display: flex;
  align-items: center;
  gap: 7px;
  padding: 10px 20px;
  background: #3A473C;
  border: none;
  border-radius: 10px;
  font-size: 13px;
  font-weight: 700;
  color: #fff;
  cursor: pointer;
  transition: background 0.18s;
}
.sc-btn-save:hover { background: #2d3730; }

/* Modal confirmación */
.sc-confirm-body {
  padding: 32px 28px 8px;
  text-align: center;
}
.sc-confirm-icon {
  width: 60px;
  height: 60px;
  border-radius: 50%;
  background: #EEF2EE;
  color: #3A473C;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 18px;
}
.sc-confirm-title {
  font-size: 18px;
  font-weight: 800;
  color: #3A473C;
  margin-bottom: 10px;
}
.sc-confirm-text {
  font-size: 13px;
  color: #6C756D;
  line-height: 1.6;
  max-width: 320px;
  margin: 0 auto;
}

/* Modal Ver */
.sc-ver-pet {
  display: flex;
  align-items: center;
  gap: 14px;
  padding-bottom: 18px;
  margin-bottom: 18px;
  border-bottom: 1.5px solid #F0F2F0;
}
.sc-ver-pname { font-size: 17px; font-weight: 800; color: #3A473C; }
.sc-ver-pspec { font-size: 12px; color: #92A894; margin-top: 2px; }
.sc-ver-badges {
  margin-left: auto;
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 5px;
}
.sc-id-badge {
  background: #EEF2EE;
  color: #5A6E5C;
  padding: 4px 10px;
  border-radius: 7px;
  font-size: 12px;
  font-weight: 700;
  font-family: monospace;
}
.sc-inactive-badge {
  background: #FFF3CD;
  color: #856404;
  padding: 3px 8px;
  border-radius: 6px;
  font-size: 11px;
  font-weight: 700;
}

.sc-ver-card {
  background: #F9FAF9;
  border-radius: 14px;
  padding: 18px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}
.sc-ver-row {
  display: flex;
  align-items: center;
  gap: 12px;
}
.sc-ver-row--block { align-items: flex-start; }
.sc-ver-label {
  font-size: 11px;
  font-weight: 800;
  color: #92A894;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  min-width: 100px;
  flex-shrink: 0;
}
.sc-ver-val { font-size: 13px; color: #3A473C; }
.sc-ver-val--bold { font-weight: 700; font-size: 14px; }
.sc-ver-val--obs { color: #6C756D; line-height: 1.5; }

/* ═══════════════════════════════════════
   RESPONSIVE
═══════════════════════════════════════ */
@media (max-width: 900px) {
  .sc-form-grid--4 { grid-template-columns: repeat(2, 1fr); }
  .sc-fg--span2 { grid-column: span 1; }
}

@media (max-width: 640px) {
  .sc-header     { flex-direction: column; align-items: flex-start; }
  .sc-toolbar    { flex-direction: column; align-items: flex-start; }
  .sc-filters    { width: 100%; }
  .sc-search-wrap { min-width: unset; width: 100%; }
  .sc-date-range { width: 100%; }
  .sc-form-grid--4 { grid-template-columns: 1fr; }
  .sc-fg--span2,
  .sc-fg--full   { grid-column: 1; }
  .sc-table th:nth-child(4),
  .sc-table td:nth-child(4),
  .sc-table th:nth-child(5),
  .sc-table td:nth-child(5) { display: none; }
  .sc-modal-body { padding: 16px 18px 8px; }
  .sc-modal-header,
  .sc-modal-footer { padding-left: 18px; padding-right: 18px; }
}
</style>