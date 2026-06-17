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
 
  const q = search.value.trim().toLowerCase()
  if (q) {
    result = result.filter(r =>
      r.petNombre?.toLowerCase().includes(q) ||
      r.petId?.toString().toLowerCase().includes(q)
    )
  }
 
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
    d.medicalHistory.push({
      id:           generarId('SAL', d.medicalHistory),
      fecha:        form.value.fecha,
      vet:          form.value.vet,
      peso:         form.value.peso,
      diagnostico:  form.value.diagnostico,
      observaciones: form.value.observaciones_h,
      creadoEn:     new Date().toISOString()
    })
    d.vaccines.push({
      id:              generarId('VAC', d.vaccines),
      tipo:            form.value.tipoVacuna,
      fechaAplicacion: form.value.fechaAplicacion,
      proximaDosis:    form.value.proximaDosis,
      vet:             form.value.vetVacuna,
      observaciones:   form.value.observaciones_v,
      creadoEn:        new Date().toISOString()
    })
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
 
// ── KPIs ──
const totalHistorial = computed(() => {
  let count = 0
  store.pets.forEach(pet => {
    count += datos.value[pet.id]?.medicalHistory?.length || 0
  })
  return count
})
const totalVacunas = computed(() => {
  let count = 0
  store.pets.forEach(pet => {
    count += datos.value[pet.id]?.vaccines?.length || 0
  })
  return count
})
const totalTratamientos = computed(() => {
  let count = 0
  store.pets.forEach(pet => {
    count += datos.value[pet.id]?.treatments?.length || 0
  })
  return count
})
const totalMascotas = computed(() => store.pets.filter(p => p.active !== false).length)
</script>
 
<template>
  <div class="view-container">
 
    <!-- ── Toast ── -->
    <Teleport to="body">
      <Transition name="toast-anim">
        <div v-if="toast.show" class="sal-toast" :class="toast.type">
          {{ toast.message }}
        </div>
      </Transition>
    </Teleport>
 
    <!-- ── ENCABEZADO ── -->
    <header class="page-header">
      <div>
        <h1 class="admin-page-title">Control de Salud</h1>
        <p class="admin-page-sub">Historial médico, vacunas y tratamientos</p>
      </div>
      <button class="btn-primary" @click="abrirModal">
        <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
        Nuevo expediente
      </button>
    </header>
 
    <!-- ── KPI CARDS ── -->
    <div class="don-summary">
      <div class="don-card kpi-historial">
        <span class="don-label">Registros de historial</span>
        <strong class="don-value">{{ totalHistorial }}</strong>
      </div>
      <div class="don-card kpi-vacunas">
        <span class="don-label">Vacunas aplicadas</span>
        <strong class="don-value">{{ totalVacunas }}</strong>
      </div>
      <div class="don-card kpi-tratamientos">
        <span class="don-label">Tratamientos</span>
        <strong class="don-value">{{ totalTratamientos }}</strong>
      </div>
      <div class="don-card kpi-mascotas">
        <span class="don-label">Mascotas activas</span>
        <strong class="don-value">{{ totalMascotas }}</strong>
      </div>
    </div>
 
    <!-- ── PANEL FILTROS ── -->
    <div class="filtros-panel">
 
      <!-- Tabs dentro del panel -->
      <div class="filtro-group filtro-group--tabs">
        <label class="filtro-label">Sección</label>
        <div class="tabs-group">
          <button class="tab-btn" :class="{ 'tab-btn--active': activeTab === 'historial' }" @click="activeTab = 'historial'">
            <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
            Historial
          </button>
          <button class="tab-btn" :class="{ 'tab-btn--active': activeTab === 'vacunas' }" @click="activeTab = 'vacunas'">
            <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M22 12h-4l-3 9L9 3l-3 9H2"/></svg>
            Vacunas
          </button>
          <button class="tab-btn" :class="{ 'tab-btn--active': activeTab === 'tratamientos' }" @click="activeTab = 'tratamientos'">
            <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/></svg>
            Tratamientos
          </button>
        </div>
      </div>
 
      <!-- Buscar -->
      <div class="filtro-group">
        <label class="filtro-label">Buscar mascota</label>
        <div class="filtro-input-wrap">
          <input v-model="search" placeholder="Nombre o ID..." class="filtro-input filtro-input--icon" />
          <span class="filtro-icon filtro-icon--right">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
          </span>
        </div>
      </div>
 
      <!-- Desde -->
      <div class="filtro-group">
        <label class="filtro-label">Desde</label>
        <div class="filtro-input-wrap">
          <input type="date" class="filtro-input" v-model="filterFrom" />
        </div>
      </div>
 
      <!-- Hasta -->
      <div class="filtro-group">
        <label class="filtro-label">Hasta</label>
        <div class="filtro-input-wrap">
          <input type="date" class="filtro-input" v-model="filterTo" />
        </div>
      </div>
 
      <!-- Limpiar -->
      <div class="filtro-group filtro-group--btn">
        <button
          type="button"
          class="btn-limpiar"
          :class="{ 'btn-limpiar--activo': hayFiltros }"
          @click="search = ''; filterFrom = ''; filterTo = ''"
        >
          Limpiar filtros
        </button>
      </div>
 
    </div>
 
    <!-- ── TABLA HISTORIAL ── -->
    <div v-if="activeTab === 'historial'" class="table-wrapper">
      <div class="table-scroll">
        <table class="don-table">
          <thead>
            <tr>
              <th>ID Registro</th>
              <th>Mascota</th>
              <th>Fecha</th>
              <th>Veterinario</th>
              <th>Diagnóstico</th>
              <th>Peso</th>
              <th>Acción</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="r in registros" :key="r.id" class="don-row">
              <td><span class="id-pill">{{ r.id }}</span></td>
              <td>
                <div class="pet-cell">
                  <div class="pet-avatar">
                    <img v-if="r.petFoto" :src="r.petFoto" class="pet-avatar-img" />
                    <span v-else class="pet-avatar-ini">{{ r.petNombre?.charAt(0) }}</span>
                  </div>
                  <div>
                    <span class="donor-name">{{ r.petNombre }}</span>
                    <span class="donor-mail">{{ r.petId }}</span>
                  </div>
                </div>
              </td>
              <td><span class="fecha-text">{{ formatFecha(r.fecha) }}</span></td>
              <td><span class="metodo-text">{{ r.vet || '—' }}</span></td>
              <td><span class="monto-text">{{ r.diagnostico }}</span></td>
              <td><span class="metodo-text">{{ r.peso ? r.peso + ' kg' : '—' }}</span></td>
              <td><button class="btn-ver" @click="verRegistro(r)">Ver detalle</button></td>
            </tr>
            <tr v-if="registros.length === 0">
              <td colspan="7" class="empty-cell">
                <div class="empty-state-inner">
                  <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
                  <p>{{ hayFiltros ? 'Sin resultados para los filtros aplicados' : 'Sin registros de historial médico' }}</p>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div class="table-footer">
        {{ registros.length }} registro{{ registros.length !== 1 ? 's' : '' }} encontrado{{ registros.length !== 1 ? 's' : '' }}
      </div>
    </div>
 
    <!-- ── TABLA VACUNAS ── -->
    <div v-if="activeTab === 'vacunas'" class="table-wrapper">
      <div class="table-scroll">
        <table class="don-table">
          <thead>
            <tr>
              <th>ID Registro</th>
              <th>Mascota</th>
              <th>Vacuna</th>
              <th>Aplicación</th>
              <th>Próxima dosis</th>
              <th>Veterinario</th>
              <th>Acción</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="r in registros" :key="r.id" class="don-row">
              <td><span class="id-pill">{{ r.id }}</span></td>
              <td>
                <div class="pet-cell">
                  <div class="pet-avatar">
                    <img v-if="r.petFoto" :src="r.petFoto" class="pet-avatar-img" />
                    <span v-else class="pet-avatar-ini">{{ r.petNombre?.charAt(0) }}</span>
                  </div>
                  <div>
                    <span class="donor-name">{{ r.petNombre }}</span>
                    <span class="donor-mail">{{ r.petId }}</span>
                  </div>
                </div>
              </td>
              <td><span class="monto-text">{{ r.tipo }}</span></td>
              <td><span class="fecha-text">{{ formatFecha(r.fechaAplicacion) }}</span></td>
              <td>
                <span v-if="r.proximaDosis" class="estado-badge badge-aprobada">{{ formatFecha(r.proximaDosis) }}</span>
                <span v-else class="metodo-text">—</span>
              </td>
              <td><span class="metodo-text">{{ r.vet || '—' }}</span></td>
              <td><button class="btn-ver" @click="verRegistro(r)">Ver detalle</button></td>
            </tr>
            <tr v-if="registros.length === 0">
              <td colspan="7" class="empty-cell">
                <div class="empty-state-inner">
                  <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M22 12h-4l-3 9L9 3l-3 9H2"/></svg>
                  <p>{{ hayFiltros ? 'Sin resultados para los filtros aplicados' : 'Sin registros de vacunas' }}</p>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div class="table-footer">
        {{ registros.length }} registro{{ registros.length !== 1 ? 's' : '' }} encontrado{{ registros.length !== 1 ? 's' : '' }}
      </div>
    </div>
 
    <!-- ── TABLA TRATAMIENTOS ── -->
    <div v-if="activeTab === 'tratamientos'" class="table-wrapper">
      <div class="table-scroll">
        <table class="don-table">
          <thead>
            <tr>
              <th>ID Registro</th>
              <th>Mascota</th>
              <th>Tratamiento</th>
              <th>Fecha</th>
              <th>Medicamento</th>
              <th>Dosis</th>
              <th>Acción</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="r in registros" :key="r.id" class="don-row">
              <td><span class="id-pill">{{ r.id }}</span></td>
              <td>
                <div class="pet-cell">
                  <div class="pet-avatar">
                    <img v-if="r.petFoto" :src="r.petFoto" class="pet-avatar-img" />
                    <span v-else class="pet-avatar-ini">{{ r.petNombre?.charAt(0) }}</span>
                  </div>
                  <div>
                    <span class="donor-name">{{ r.petNombre }}</span>
                    <span class="donor-mail">{{ r.petId }}</span>
                  </div>
                </div>
              </td>
              <td><span class="monto-text">{{ r.tipo }}</span></td>
              <td><span class="fecha-text">{{ formatFecha(r.fecha) }}</span></td>
              <td><span class="metodo-text">{{ r.medicamento || '—' }}</span></td>
              <td><span class="metodo-text">{{ r.dosis || '—' }}</span></td>
              <td><button class="btn-ver" @click="verRegistro(r)">Ver detalle</button></td>
            </tr>
            <tr v-if="registros.length === 0">
              <td colspan="7" class="empty-cell">
                <div class="empty-state-inner">
                  <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/></svg>
                  <p>{{ hayFiltros ? 'Sin resultados para los filtros aplicados' : 'Sin registros de tratamientos' }}</p>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div class="table-footer">
        {{ registros.length }} registro{{ registros.length !== 1 ? 's' : '' }} encontrado{{ registros.length !== 1 ? 's' : '' }}
      </div>
    </div>
 
    <!-- ══════════════════════════════════════
         MODAL REGISTRAR — formulario unificado
    ══════════════════════════════════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showModalRegistrar" class="modal-overlay" @click.self="showModalRegistrar = false">
          <div class="modal-box modal-box--lg">
 
            <button class="modal-close" @click="showModalRegistrar = false">✕</button>
 
            <div class="modal-header">
              <div>
                <p class="modal-eyebrow">Expediente médico</p>
                <h2 class="modal-title">Nuevo registro completo</h2>
              </div>
            </div>
 
            <div class="modal-body">
 
              <!-- Selector mascota -->
              <div class="form-section">
                <h4 class="modal-section-title">Mascota</h4>
                <div class="pet-selector-wrap">
                  <button
                    type="button"
                    class="pet-selector-btn"
                    :class="{ 'is-error': errores.pet }"
                    @click="showPetDropdown = !showPetDropdown"
                  >
                    <template v-if="petSeleccionada">
                      <div class="pet-avatar pet-avatar--sm">
                        <img v-if="petSeleccionada.foto || petSeleccionada.image || petSeleccionada.photo || petSeleccionada.avatar" :src="petSeleccionada.foto || petSeleccionada.image || petSeleccionada.photo || petSeleccionada.avatar" class="pet-avatar-img" />
                        <span v-else class="pet-avatar-ini">{{ petSeleccionada.name?.charAt(0) }}</span>
                      </div>
                      <span class="psel-name">{{ petSeleccionada.name }}</span>
                      <span class="psel-species">{{ petSeleccionada.species }}</span>
                    </template>
                    <template v-else>
                      <span class="psel-placeholder">Seleccionar mascota...</span>
                    </template>
                    <svg class="psel-chevron" :class="{ open: showPetDropdown }" xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
                  </button>
                  <p v-if="errores.pet" class="field-error">{{ errores.pet }}</p>
                  <div v-if="showPetDropdown" class="pet-dropdown">
                    <div v-if="mascotasActivas.length === 0" class="dropdown-empty">No hay mascotas activas registradas</div>
                    <div
                      v-for="pet in mascotasActivas"
                      :key="pet.id"
                      class="dropdown-item"
                      :class="{ selected: petSeleccionada?.id === pet.id }"
                      @click="seleccionarPet(pet)"
                    >
                      <div class="pet-avatar pet-avatar--sm">
                        <img v-if="pet.foto || pet.image || pet.photo || pet.avatar" :src="pet.foto || pet.image || pet.photo || pet.avatar" class="pet-avatar-img" />
                        <span v-else class="pet-avatar-ini">{{ pet.name?.charAt(0) }}</span>
                      </div>
                      <div class="dropdown-info">
                        <span class="dropdown-name">{{ pet.name }}</span>
                        <span class="dropdown-sub">{{ pet.species }}</span>
                      </div>
                      <svg v-if="petSeleccionada?.id === pet.id" xmlns="http://www.w3.org/2000/svg" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" style="color:#92A894;flex-shrink:0"><polyline points="20 6 9 17 4 12"/></svg>
                    </div>
                  </div>
                </div>
              </div>
 
              <!-- ── Sección 1: Historial médico ── -->
              <div class="form-section">
                <h4 class="modal-section-title">1 — Historial médico</h4>
                <div class="form-grid form-grid--4">
                  <div class="fg">
                    <label class="fg-label">Fecha <span class="req">*</span></label>
                    <input type="date" class="fg-input" :class="{ 'is-error': errores.fecha }" v-model="form.fecha" @change="clearErr('fecha')" />
                    <p v-if="errores.fecha" class="field-error">{{ errores.fecha }}</p>
                  </div>
                  <div class="fg">
                    <label class="fg-label">Peso (kg)</label>
                    <input type="number" class="fg-input" placeholder="Ej. 12.5" step="0.1" min="0" v-model="form.peso" />
                  </div>
 
                  <!-- Veterinario historial -->
                  <div class="fg">
                    <label class="fg-label">Veterinario responsable <span class="req">*</span></label>
                    <div class="pet-selector-wrap">
                      <button type="button" class="pet-selector-btn" :class="{ 'is-error': errores.vet }" @click="showVetDropdown = !showVetDropdown">
                        <template v-if="form.vet">
                          <div class="pet-avatar pet-avatar--sm"><span class="pet-avatar-ini">{{ form.vet.charAt(0) }}</span></div>
                          <span class="psel-name">{{ form.vet }}</span>
                        </template>
                        <template v-else><span class="psel-placeholder">Seleccionar veterinario...</span></template>
                        <svg class="psel-chevron" :class="{ open: showVetDropdown }" xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
                      </button>
                      <div v-if="showVetDropdown" class="pet-dropdown">
                        <div v-for="vet in veterinarios" :key="vet.id" class="dropdown-item" @click="form.vet = vet.nombre; showVetDropdown = false; clearErr('vet')">
                          <div class="pet-avatar pet-avatar--sm"><span class="pet-avatar-ini">{{ vet.nombre?.charAt(0) }}</span></div>
                          <div class="dropdown-info">
                            <span class="dropdown-name">Dr. {{ vet.nombre }}</span>
                            <span class="dropdown-sub">Veterinario</span>
                          </div>
                        </div>
                      </div>
                    </div>
                    <p v-if="errores.vet" class="field-error">{{ errores.vet }}</p>
                  </div>
 
                  <!-- Clínica -->
                  <div class="fg">
                    <label class="fg-label">Clínica veterinaria</label>
                    <input type="text" class="fg-input" placeholder="Ej. Hospital Veterinario San José" v-model="form.clinica" />
                  </div>
 
                  <div class="fg fg--full">
                    <label class="fg-label">Diagnóstico <span class="req">*</span></label>
                    <input type="text" class="fg-input" :class="{ 'is-error': errores.diagnostico }" placeholder="Ej. Control preventivo, otitis externa..." v-model="form.diagnostico" @input="clearErr('diagnostico')" />
                    <p v-if="errores.diagnostico" class="field-error">{{ errores.diagnostico }}</p>
                  </div>
                  <div class="fg fg--full">
                    <label class="fg-label">Observaciones</label>
                    <textarea class="fg-textarea" placeholder="Indicaciones, seguimiento, notas clínicas..." v-model="form.observaciones_h"></textarea>
                  </div>
                </div>
              </div>
 
              <!-- ── Sección 2: Vacuna ── -->
              <div class="form-section">
                <h4 class="modal-section-title">2 — Vacuna</h4>
                <div class="form-grid form-grid--4">
                  <div class="fg fg--span2">
                    <label class="fg-label">Tipo de vacuna <span class="req">*</span></label>
                    <input type="text" class="fg-input" :class="{ 'is-error': errores.tipoVacuna }" placeholder="Ej. Antirrábica, Parvovirus..." v-model="form.tipoVacuna" @input="clearErr('tipoVacuna')" />
                    <p v-if="errores.tipoVacuna" class="field-error">{{ errores.tipoVacuna }}</p>
                  </div>
                  <div class="fg">
                    <label class="fg-label">Fecha de aplicación <span class="req">*</span></label>
                    <input type="date" class="fg-input" :class="{ 'is-error': errores.fechaAplicacion }" v-model="form.fechaAplicacion" @change="clearErr('fechaAplicacion')" />
                    <p v-if="errores.fechaAplicacion" class="field-error">{{ errores.fechaAplicacion }}</p>
                  </div>
                  <div class="fg">
                    <label class="fg-label">Próxima dosis</label>
                    <input type="date" class="fg-input" v-model="form.proximaDosis" />
                  </div>
 
                  <!-- Veterinario vacuna -->
                  <div class="fg fg--span2">
                    <label class="fg-label">Veterinario responsable</label>
                    <div class="pet-selector-wrap">
                      <button type="button" class="pet-selector-btn" @click="showVetDropdownVacuna = !showVetDropdownVacuna">
                        <template v-if="form.vetVacuna">
                          <div class="pet-avatar pet-avatar--sm"><span class="pet-avatar-ini">{{ form.vetVacuna.charAt(0) }}</span></div>
                          <span class="psel-name">{{ form.vetVacuna }}</span>
                        </template>
                        <template v-else><span class="psel-placeholder">Seleccionar veterinario...</span></template>
                        <svg class="psel-chevron" :class="{ open: showVetDropdownVacuna }" xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
                      </button>
                      <div v-if="showVetDropdownVacuna" class="pet-dropdown">
                        <div v-for="vet in veterinarios" :key="vet.id" class="dropdown-item" @click="form.vetVacuna = vet.nombre; showVetDropdownVacuna = false">
                          <div class="pet-avatar pet-avatar--sm"><span class="pet-avatar-ini">{{ vet.nombre?.charAt(0) }}</span></div>
                          <div class="dropdown-info">
                            <span class="dropdown-name">Dr. {{ vet.nombre }}</span>
                            <span class="dropdown-sub">Veterinario</span>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
 
                  <div class="fg fg--full">
                    <label class="fg-label">Observaciones</label>
                    <textarea class="fg-textarea" placeholder="Notas sobre la vacuna, lote, reacciones..." v-model="form.observaciones_v"></textarea>
                  </div>
                </div>
              </div>
 
              <!-- ── Sección 3: Tratamiento ── -->
              <div class="form-section">
                <h4 class="modal-section-title">3 — Tratamiento</h4>
                <div class="form-grid form-grid--4">
                  <div class="fg fg--span2">
                    <label class="fg-label">Tipo de tratamiento <span class="req">*</span></label>
                    <input type="text" class="fg-input" :class="{ 'is-error': errores.tipoTratamiento }" placeholder="Ej. Desparasitación, antibiótico..." v-model="form.tipoTratamiento" @input="clearErr('tipoTratamiento')" />
                    <p v-if="errores.tipoTratamiento" class="field-error">{{ errores.tipoTratamiento }}</p>
                  </div>
                  <div class="fg">
                    <label class="fg-label">Fecha <span class="req">*</span></label>
                    <input type="date" class="fg-input" :class="{ 'is-error': errores.fechaTrat }" v-model="form.fechaTrat" @change="clearErr('fechaTrat')" />
                    <p v-if="errores.fechaTrat" class="field-error">{{ errores.fechaTrat }}</p>
                  </div>
                  <div class="fg">
                    <label class="fg-label">Dosis</label>
                    <input type="text" class="fg-input" placeholder="Ej. 5mg/kg" v-model="form.dosis" />
                  </div>
                  <div class="fg fg--span2">
                    <label class="fg-label">Medicamento</label>
                    <input type="text" class="fg-input" placeholder="Nombre del medicamento" v-model="form.medicamento" />
                  </div>
                  <div class="fg fg--full">
                    <label class="fg-label">Observaciones</label>
                    <textarea class="fg-textarea" placeholder="Duración, respuesta al tratamiento, seguimiento..." v-model="form.observaciones_t"></textarea>
                  </div>
                </div>
              </div>
 
              <!-- Nota inmutable -->
              <div class="immutable-note">
                <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
                Los registros médicos son permanentes y no pueden editarse ni eliminarse una vez guardados
              </div>
 
            </div>
 
            <div class="modal-footer">
              <button class="btn-cancel" @click="showModalRegistrar = false">Cancelar</button>
              <button class="btn-save" @click="intentarGuardar">
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
      <Transition name="modal-fade">
        <div v-if="showModalConfirm" class="modal-overlay modal-overlay--top" @click.self="showModalConfirm = false">
          <div class="modal-box modal-box--sm">
            <button class="modal-close" @click="showModalConfirm = false">✕</button>
            <div class="confirm-body">
              <div class="confirm-icon">
                <svg xmlns="http://www.w3.org/2000/svg" width="26" height="26" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>
              </div>
              <h3 class="confirm-title">¿Guardar este expediente?</h3>
              <p class="confirm-text">Se registrará el historial médico, la vacuna y el tratamiento para <strong>{{ petSeleccionada?.name }}</strong>. Esta acción es permanente y no podrá modificarse.</p>
            </div>
            <div class="modal-footer">
              <button class="btn-cancel" @click="showModalConfirm = false">Cancelar</button>
              <button class="btn-save" @click="confirmarGuardar">Confirmar y guardar</button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
 
    <!-- ── Modal Ver ── -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showModalVer && registroVer" class="modal-overlay" @click.self="showModalVer = false">
          <div class="modal-box modal-box--md">
 
            <button class="modal-close" @click="showModalVer = false">✕</button>
 
            <div class="modal-header">
              <span class="id-pill">{{ registroVer.id }}</span>
              <span v-if="!registroVer.petActiva" class="estado-badge badge-pendiente">Inactiva</span>
            </div>
 
            <div class="modal-body">
              <!-- Mascota -->
              <div class="modal-section">
                <h4 class="modal-section-title">Mascota</h4>
                <div class="modal-grid">
                  <div class="modal-field">
                    <span class="modal-field-label">Nombre</span>
                    <div style="display:flex;align-items:center;gap:10px;margin-top:4px">
                      <div class="pet-avatar pet-avatar--sm">
                        <img v-if="registroVer.petFoto" :src="registroVer.petFoto" class="pet-avatar-img" />
                        <span v-else class="pet-avatar-ini">{{ registroVer.petNombre?.charAt(0) }}</span>
                      </div>
                      <strong class="modal-field-value">{{ registroVer.petNombre }}</strong>
                    </div>
                  </div>
                  <div class="modal-field">
                    <span class="modal-field-label">Especie</span>
                    <strong class="modal-field-value">{{ registroVer.petEspecie || '—' }}</strong>
                  </div>
                  <div class="modal-field">
                    <span class="modal-field-label">ID</span>
                    <strong class="modal-field-value"><span class="id-pill">{{ registroVer.petId }}</span></strong>
                  </div>
                </div>
              </div>
 
              <!-- Historial -->
              <template v-if="registroVer.id?.startsWith('SAL')">
                <div class="modal-section">
                  <h4 class="modal-section-title">Historial médico</h4>
                  <div class="modal-grid">
                    <div class="modal-field">
                      <span class="modal-field-label">Fecha</span>
                      <strong class="modal-field-value">{{ formatFecha(registroVer.fecha) }}</strong>
                    </div>
                    <div class="modal-field">
                      <span class="modal-field-label">Veterinario</span>
                      <strong class="modal-field-value">{{ registroVer.vet || '—' }}</strong>
                    </div>
                    <div class="modal-field">
                      <span class="modal-field-label">Peso</span>
                      <strong class="modal-field-value">{{ registroVer.peso ? registroVer.peso + ' kg' : '—' }}</strong>
                    </div>
                    <div class="modal-field modal-field--full">
                      <span class="modal-field-label">Diagnóstico</span>
                      <strong class="modal-field-value monto-highlight">{{ registroVer.diagnostico }}</strong>
                    </div>
                    <div v-if="registroVer.observaciones" class="modal-field modal-field--full">
                      <span class="modal-field-label">Observaciones</span>
                      <p class="modal-mensaje">{{ registroVer.observaciones }}</p>
                    </div>
                  </div>
                </div>
              </template>
 
              <!-- Vacuna -->
              <template v-if="registroVer.id?.startsWith('VAC')">
                <div class="modal-section">
                  <h4 class="modal-section-title">Vacuna</h4>
                  <div class="modal-grid">
                    <div class="modal-field modal-field--full">
                      <span class="modal-field-label">Tipo</span>
                      <strong class="modal-field-value monto-highlight">{{ registroVer.tipo }}</strong>
                    </div>
                    <div class="modal-field">
                      <span class="modal-field-label">Aplicación</span>
                      <strong class="modal-field-value">{{ formatFecha(registroVer.fechaAplicacion) }}</strong>
                    </div>
                    <div class="modal-field">
                      <span class="modal-field-label">Próxima dosis</span>
                      <strong class="modal-field-value">{{ registroVer.proximaDosis ? formatFecha(registroVer.proximaDosis) : '—' }}</strong>
                    </div>
                    <div class="modal-field">
                      <span class="modal-field-label">Veterinario</span>
                      <strong class="modal-field-value">{{ registroVer.vet || '—' }}</strong>
                    </div>
                    <div v-if="registroVer.observaciones" class="modal-field modal-field--full">
                      <span class="modal-field-label">Observaciones</span>
                      <p class="modal-mensaje">{{ registroVer.observaciones }}</p>
                    </div>
                  </div>
                </div>
              </template>
 
              <!-- Tratamiento -->
              <template v-if="registroVer.id?.startsWith('TRA')">
                <div class="modal-section">
                  <h4 class="modal-section-title">Tratamiento</h4>
                  <div class="modal-grid">
                    <div class="modal-field modal-field--full">
                      <span class="modal-field-label">Tipo</span>
                      <strong class="modal-field-value monto-highlight">{{ registroVer.tipo }}</strong>
                    </div>
                    <div class="modal-field">
                      <span class="modal-field-label">Fecha</span>
                      <strong class="modal-field-value">{{ formatFecha(registroVer.fecha) }}</strong>
                    </div>
                    <div class="modal-field">
                      <span class="modal-field-label">Medicamento</span>
                      <strong class="modal-field-value">{{ registroVer.medicamento || '—' }}</strong>
                    </div>
                    <div class="modal-field">
                      <span class="modal-field-label">Dosis</span>
                      <strong class="modal-field-value">{{ registroVer.dosis || '—' }}</strong>
                    </div>
                    <div v-if="registroVer.observaciones" class="modal-field modal-field--full">
                      <span class="modal-field-label">Observaciones</span>
                      <p class="modal-mensaje">{{ registroVer.observaciones }}</p>
                    </div>
                  </div>
                </div>
              </template>
            </div>
 
            <div class="modal-footer">
              <button class="btn-cancel" style="flex:1" @click="showModalVer = false">Cerrar</button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
 
  </div>
</template>
 
<style scoped>
/* ══════════════════════════════════════════
   VARIABLES — definidas en :root para que
   los Teleport (modales) también las hereden
══════════════════════════════════════════ */
:root {
  --sal-verde:     #3A473C;
  --sal-verde-sec: #92A894;
  --sal-fondo:     #F7F8F7;
  --sal-blanco:    #FFFFFF;
  --sal-texto:     #2F352F;
  --sal-texto-sec: #6C756D;
  --sal-borde:     #E8ECE8;
  --sal-amarillo:  #F5B942;
  --sal-verde-ok:  #4CAF6A;
}
 
.view-container {
  /* alias locales para compatibilidad con el resto del CSS */
  --verde:     var(--sal-verde);
  --verde-sec: var(--sal-verde-sec);
  --fondo:     var(--sal-fondo);
  --blanco:    var(--sal-blanco);
  --texto:     var(--sal-texto);
  --texto-sec: var(--sal-texto-sec);
  --borde:     var(--sal-borde);
  --amarillo:  var(--sal-amarillo);
  --verde-ok:  var(--sal-verde-ok);
  background: transparent;
  padding-bottom: 40px;
}
 
/* Las clases de modal usan directamente los tokens :root
   para garantizar visibilidad aunque estén en Teleport */
.modal-overlay,
.modal-box,
.modal-body,
.form-section,
.form-grid,
.fg,
.fg-input,
.fg-textarea,
.fg-label,
.pet-selector-btn,
.pet-dropdown,
.modal-section-title,
.modal-field-label,
.modal-field-value,
.modal-close,
.modal-title,
.modal-eyebrow,
.immutable-note,
.confirm-body,
.confirm-title,
.confirm-text,
.btn-cancel,
.btn-save {
  --verde:     var(--sal-verde);
  --verde-sec: var(--sal-verde-sec);
  --fondo:     var(--sal-fondo);
  --blanco:    var(--sal-blanco);
  --texto:     var(--sal-texto);
  --texto-sec: var(--sal-texto-sec);
  --borde:     var(--sal-borde);
  --amarillo:  var(--sal-amarillo);
  --verde-ok:  var(--sal-verde-ok);
}
 
/* ── Toast ─────────────────────────────── */
.sal-toast {
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
.sal-toast.success { background: #3A473C; color: #fff; }
.sal-toast.error   { background: #c0392b; color: #fff; }
.toast-anim-enter-active, .toast-anim-leave-active { transition: all 0.25s ease; }
.toast-anim-enter-from, .toast-anim-leave-to { opacity: 0; transform: translateY(10px); }
 
/* ── Encabezado ────────────────────────── */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 28px;
  gap: 16px;
  flex-wrap: wrap;
}
.admin-page-title {
  font-size: 28px;
  font-weight: 800;
  color: var(--verde);
  letter-spacing: -0.5px;
  line-height: 1.1;
}
.admin-page-sub {
  font-size: 14px;
  color: var(--texto-sec);
  margin-top: 4px;
  font-weight: 500;
}
.btn-primary {
  display: flex;
  align-items: center;
  gap: 7px;
  height: 38px;
  padding: 0 18px;
  background: #3A473C;
  color: #ffffff;
  border: none;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 700;
  cursor: pointer;
  transition: background 0.18s;
  white-space: nowrap;
  flex-shrink: 0;
  font-family: inherit;
}
.btn-primary:hover { background: #2d3730; }
 
/* ── KPI Cards ─────────────────────────── */
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
.kpi-historial {
  border-top: 3px solid #5C7C5F;
}

.kpi-vacunas {
  border-top: 3px solid #6FAE7A;
}

.kpi-tratamientos {
  border-top: 3px solid #E8B04B;
}

.kpi-mascotas {
  border-top: 3px solid #3A473C;
}
 
.don-label {
  font-size: 11px;
  color: var(--texto-sec);
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}
.don-value {
  font-size: 24px;
  font-weight: 800;
  color: var(--verde);
  line-height: 1;
}
 
/* ── Panel filtros ─────────────────────── */
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
.filtro-group--tabs { flex: 2; min-width: 220px; }
.filtro-group--btn  { flex: 0 0 auto; min-width: unset; }
 
.filtro-label {
  font-size: 11px;
  font-weight: 700;
  color: var(--verde);
  text-transform: uppercase;
  letter-spacing: 0.5px;
  min-height: 16px;
  display: flex;
  align-items: flex-end;
}
.filtro-input-wrap {
  position: relative;
  display: flex;
  align-items: center;
}
.filtro-input {
  width: 100%; height: 38px; padding: 0 36px 0 12px;
  border-radius: 8px; border: 1.5px solid var(--borde);
  background: var(--fondo); font-size: 13px; color: var(--texto);
  font-family: inherit; outline: none;
  transition: border-color 0.18s, background 0.18s; box-sizing: border-box;
}
.filtro-input--icon { padding-left: 34px; }
.filtro-input:focus { border-color: var(--verde-sec); background: var(--blanco); }
.filtro-input::placeholder { color: #9CA8A0; }
.filtro-icon { position: absolute; display: flex; align-items: center; color: var(--texto-sec); }
.filtro-icon--right { right: 11px; }
 
/* Tabs dentro del filtro */
.tabs-group {
  display: flex;
  gap: 4px;
  background: var(--fondo);
  border-radius: 8px;
  padding: 3px;
  border: 1.5px solid var(--borde);
  height: 38px;
  box-sizing: border-box;
  align-items: center;
}
.tab-btn {
  display: flex;
  align-items: center;
  gap: 5px;
  padding: 5px 12px;
  border-radius: 6px;
  border: none;
  background: transparent;
  color: var(--texto-sec);
  font-size: 12px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.18s;
  white-space: nowrap;
  font-family: inherit;
  height: 28px;
}
.tab-btn:hover { color: var(--verde); background: rgba(255,255,255,0.7); }
.tab-btn--active { background: var(--blanco); color: var(--verde); box-shadow: 0 1px 3px rgba(58,71,60,0.12); }
 
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
.btn-limpiar--activo { border-color: var(--verde); color: var(--verde); }
.btn-limpiar:hover   { background: var(--verde); color: var(--blanco); border-color: var(--verde); }
 
/* ── Tabla ─────────────────────────────── */
.table-wrapper {
  background: var(--blanco);
  border-radius: 14px;
  border: 1px solid var(--borde);
  overflow: hidden;
}
.table-scroll { overflow-x: auto; -webkit-overflow-scrolling: touch; }
.don-table { width: 100%; border-collapse: collapse; min-width: 680px; }
.don-table thead th {
  background: #3A473C !important;
  color: white !important;
}
.don-table thead th {
  padding: 13px 16px;
  text-align: left;
  color: var(--blanco);
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.6px;
  white-space: nowrap;
}
.don-table tbody tr { border-bottom: 1px solid var(--borde); transition: background 0.15s; }
.don-table tbody tr:last-child { border-bottom: none; }
.don-table tbody tr:hover { background: #F4F6F4; }
.don-table tbody td { padding: 13px 16px; vertical-align: middle; }
 
/* Pet cell */
.pet-cell { display: flex; align-items: center; gap: 10px; }
.pet-avatar {
  width: 38px; height: 38px;
  border-radius: 50%;
  background: #DDE6DE;
  display: flex; align-items: center; justify-content: center;
  flex-shrink: 0; overflow: hidden;
}
.pet-avatar--sm { width: 32px; height: 32px; }
.pet-avatar-img { width: 100%; height: 100%; object-fit: cover; display: block; }
.pet-avatar-ini { font-size: 13px; font-weight: 800; color: #5A6E5C; text-transform: uppercase; line-height: 1; }
.pet-avatar--sm .pet-avatar-ini { font-size: 11px; }
 
.donor-name { display: block; font-size: 13px; font-weight: 700; color: var(--texto); line-height: 1.3; }
.donor-mail { display: block; font-size: 11px; color: var(--texto-sec); margin-top: 2px; font-family: monospace; }
.metodo-text { font-size: 13px; color: var(--texto-sec); }
.monto-text  { font-size: 13px; font-weight: 700; color: var(--verde); }
.fecha-text  { font-size: 13px; color: var(--texto-sec); white-space: nowrap; }
 
.id-pill {
  font-size: 11px; font-family: monospace;
  background: var(--fondo); border: 1px solid var(--borde);
  padding: 3px 9px; border-radius: 6px;
  color: var(--verde); font-weight: 700; white-space: nowrap;
}
 
.estado-badge { display: inline-block; font-size: 11px; font-weight: 700; padding: 4px 12px; border-radius: 20px; white-space: nowrap; }
.badge-aprobada  { background: #E8F5E9; color: #2E7D32; }
.badge-pendiente { background: #FFF7E0; color: #96650A; }
.badge-rechazada { background: #FDECEA; color: #B71C1C; }
 
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
 
.table-footer {
  padding: 12px 16px;
  border-top: 1px solid var(--borde);
  font-size: 12px;
  color: var(--texto-sec);
  font-weight: 500;
}
 
/* Empty */
.empty-cell { padding: 0; }
.empty-state-inner {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  padding: 56px 24px;
  color: var(--verde-sec);
}
.empty-state-inner svg { opacity: 0.4; }
.empty-state-inner p { font-size: 14px; font-weight: 500; color: var(--texto-sec); margin: 0; }
 
/* ── Modales ───────────────────────────── */
.modal-overlay {
  position: fixed; inset: 0;
  background: rgba(0,0,0,0.35);
  backdrop-filter: blur(4px);
  z-index: 200;
  display: flex; align-items: center; justify-content: center;
  padding: 20px;
}
.modal-overlay--top { z-index: 400; }
 
.modal-fade-enter-active, .modal-fade-leave-active { transition: opacity 0.22s ease; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity: 0; }
 
.modal-box {
  background: #FFFFFF;
  background: var(--blanco, #FFFFFF);
  border-radius: 20px;
  padding: 36px;
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
  position: relative;
  box-shadow: 0 24px 80px rgba(0,0,0,0.2);
  color: #2F352F;
  color: var(--texto, #2F352F);
}
.modal-box--sm { max-width: 420px; }
.modal-box--md { max-width: 560px; }
.modal-box--lg { max-width: 900px; }
 
.modal-close {
  position: absolute; top: 18px; right: 18px;
  width: 32px; height: 32px; border-radius: 50%;
  border: none; background: var(--fondo);
  color: var(--texto); font-size: 13px; font-weight: 700;
  cursor: pointer; display: flex; align-items: center; justify-content: center;
  transition: all 0.2s; font-family: inherit;
}
.modal-close:hover { background: var(--verde); color: var(--blanco); }
 
.modal-header {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 24px;
}
.modal-eyebrow {
  font-size: 11px; font-weight: 700; color: var(--texto-sec);
  text-transform: uppercase; letter-spacing: 0.7px; margin-bottom: 4px;
}
.modal-title {
  font-size: 20px; font-weight: 800; color: var(--verde); letter-spacing: -0.4px;
}
 
.modal-body { /* no extra padding since modal-box has padding */ }
 
.form-section { margin-bottom: 24px; }
 
.modal-section { margin-bottom: 24px; }
.modal-section-title {
  font-size: 11px; font-weight: 700; color: var(--texto-sec);
  text-transform: uppercase; letter-spacing: 0.5px;
  margin-bottom: 14px; padding-bottom: 10px;
  border-bottom: 1px solid var(--borde);
}
.modal-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 16px; }
.modal-field { display: flex; flex-direction: column; gap: 4px; }
.modal-field--full { grid-column: 1 / -1; }
.modal-field-label { font-size: 10px; font-weight: 700; color: #9CA8A0; text-transform: uppercase; letter-spacing: 0.4px; }
.modal-field-value { font-size: 14px; color: var(--texto); font-weight: 600; word-break: break-word; }
.monto-highlight { font-size: 17px; color: var(--verde); font-weight: 800; }
.modal-mensaje {
  font-size: 14px; color: var(--texto); line-height: 1.7;
  background: var(--fondo); border-radius: 10px; padding: 14px 16px; margin: 4px 0 0;
}
 
/* Formulario */
.form-grid { display: grid; gap: 14px; }
.form-grid--4 { grid-template-columns: repeat(4, 1fr); }
.fg { display: flex; flex-direction: column; gap: 6px; }
.fg--span2 { grid-column: span 2; }
.fg--full { grid-column: 1 / -1; }
.fg-label { font-size: 11px; font-weight: 700; color: var(--verde); text-transform: uppercase; letter-spacing: 0.4px; }
.req { color: #c0392b; }
.fg-input {
  height: 38px;
  padding: 0 13px;
  border: 1.5px solid #E8ECE8;
  border: 1.5px solid var(--borde, #E8ECE8);
  border-radius: 8px;
  font-size: 13px;
  color: #2F352F;
  color: var(--texto, #2F352F);
  background: #F7F8F7;
  background: var(--fondo, #F7F8F7);
  outline: none;
  font-family: inherit;
  transition: border-color 0.18s, background 0.18s;
  width: 100%;
  box-sizing: border-box;
}
.fg-input:focus {
  border-color: #92A894;
  border-color: var(--verde-sec, #92A894);
  background: #FFFFFF;
  background: var(--blanco, #FFFFFF);
}
.fg-input.is-error { border-color: #e57373; background: #fff8f8; }
.fg-textarea {
  padding: 10px 13px;
  border: 1.5px solid #E8ECE8;
  border: 1.5px solid var(--borde, #E8ECE8);
  border-radius: 8px;
  font-size: 13px;
  color: #2F352F;
  color: var(--texto, #2F352F);
  background: #F7F8F7;
  background: var(--fondo, #F7F8F7);
  outline: none;
  font-family: inherit;
  transition: border-color 0.18s, background 0.18s;
  width: 100%;
  box-sizing: border-box;
  height: 80px;
  resize: vertical;
  line-height: 1.5;
}
.fg-textarea:focus {
  border-color: #92A894;
  border-color: var(--verde-sec, #92A894);
  background: #FFFFFF;
  background: var(--blanco, #FFFFFF);
}
.field-error { font-size: 11px; color: #c0392b; font-weight: 600; margin: 0; }
 
/* Pet selector */
.pet-selector-wrap { position: relative; }
.pet-selector-btn {
  width: 100%;
  display: flex;
  align-items: center;
  gap: 10px;
  height: 38px;
  padding: 0 13px;
  border: 1.5px solid #E8ECE8;
  border: 1.5px solid var(--borde, #E8ECE8);
  border-radius: 8px;
  background: #F7F8F7;
  background: var(--fondo, #F7F8F7);
  cursor: pointer;
  font-family: inherit;
  font-size: 13px;
  color: #2F352F;
  color: var(--texto, #2F352F);
  text-align: left;
  transition: border-color 0.18s, background 0.18s;
  box-sizing: border-box;
}
.pet-selector-btn:hover,
.pet-selector-btn:focus {
  border-color: #92A894;
  border-color: var(--verde-sec, #92A894);
  background: #FFFFFF;
  background: var(--blanco, #FFFFFF);
  outline: none;
}
.pet-selector-btn.is-error { border-color: #e57373; }
.psel-placeholder { color: #9CA8A0; flex: 1; font-size: 13px; }
.psel-name { font-weight: 700; flex: 1; color: #2F352F; color: var(--texto, #2F352F); }
.psel-species { font-size: 12px; color: #92A894; color: var(--verde-sec, #92A894); }
.psel-chevron { margin-left: auto; color: #92A894; color: var(--verde-sec, #92A894); transition: transform 0.18s; flex-shrink: 0; }
.psel-chevron.open { transform: rotate(180deg); }
 
.pet-dropdown {
  position: absolute;
  top: calc(100% + 6px);
  left: 0; right: 0;
  background: #FFFFFF;
  background: var(--blanco, #FFFFFF);
  border: 1.5px solid #E8ECE8;
  border: 1.5px solid var(--borde, #E8ECE8);
  border-radius: 12px;
  box-shadow: 0 8px 24px rgba(58,71,60,0.12);
  z-index: 600;
  max-height: 220px;
  overflow-y: auto;
  padding: 6px;
}
.dropdown-empty { padding: 16px; text-align: center; font-size: 13px; color: #92A894; color: var(--verde-sec, #92A894); }
.dropdown-item {
  display: flex; align-items: center; gap: 10px;
  padding: 9px 10px; border-radius: 8px;
  cursor: pointer; transition: background 0.12s;
}
.dropdown-item:hover { background: #F7F8F7; background: var(--fondo, #F7F8F7); }
.dropdown-item.selected { background: #EEF2EE; }
.dropdown-info { display: flex; flex-direction: column; gap: 1px; flex: 1; min-width: 0; }
.dropdown-name { font-size: 13px; font-weight: 700; color: #2F352F; color: var(--texto, #2F352F); }
.dropdown-sub  { font-size: 11px; color: #92A894; color: var(--verde-sec, #92A894); }
 
/* Immutable note */
.immutable-note {
  display: flex;
  align-items: flex-start;
  gap: 8px;
  padding: 12px 14px;
  background: #FFFBF2;
  border-radius: 8px;
  border-left: 3px solid #F9C17A;
  font-size: 12px;
  color: #996C2A;
  font-weight: 600;
  line-height: 1.4;
  margin-top: 4px;
}
.immutable-note svg { flex-shrink: 0; margin-top: 1px; }
 
/* Modal footer */
.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  padding-top: 24px;
  border-top: 1px solid var(--borde);
  margin-top: 24px;
}
.btn-cancel {
  height: 40px;
  padding: 0 18px;
  background: var(--fondo);
  border: none;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 700;
  color: var(--texto-sec);
  cursor: pointer;
  transition: background 0.15s;
  font-family: inherit;
}
.btn-cancel:hover { background: #E5EAE6; }
.btn-save {
  display: flex;
  align-items: center;
  gap: 7px;
  height: 40px;
  padding: 0 20px;
  background: var(--verde);
  border: none;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 700;
  color: var(--blanco);
  cursor: pointer;
  transition: background 0.18s;
  font-family: inherit;
}
.btn-save:hover { background: #2d3730; }
 
/* Confirmación */
.confirm-body { text-align: center; padding-bottom: 8px; }
.confirm-icon {
  width: 60px; height: 60px; border-radius: 50%;
  background: #EEF2EE; color: var(--verde);
  display: flex; align-items: center; justify-content: center;
  margin: 0 auto 18px;
}
.confirm-title { font-size: 18px; font-weight: 800; color: var(--verde); margin-bottom: 10px; }
.confirm-text { font-size: 13px; color: var(--texto-sec); line-height: 1.6; max-width: 320px; margin: 0 auto; }
 
/* ── Responsive ────────────────────────── */
@media (max-width: 900px) {
  .don-summary { display: grid; grid-template-columns: repeat(2, 1fr); }
  .form-grid--4 { grid-template-columns: repeat(2, 1fr); }
  .fg--span2 { grid-column: span 1; }
}
 
@media (max-width: 640px) {
  .page-header { flex-direction: column; align-items: flex-start; }
  .btn-primary { width: 100%; justify-content: center; }
  .filtros-panel { flex-direction: column; }
  .filtro-group, .filtro-group--tabs { min-width: 100%; flex: none; }
  .filtro-group--btn { width: 100%; }
  .btn-limpiar { width: 100%; }
  .don-summary { grid-template-columns: 1fr; }
  .form-grid--4 { grid-template-columns: 1fr; }
  .fg--span2, .fg--full { grid-column: 1; }
  .modal-box { padding: 24px 18px; }
  .modal-grid { grid-template-columns: 1fr; }
  .don-table thead th:nth-child(4),
  .don-table tbody td:nth-child(4),
  .don-table thead th:nth-child(5),
  .don-table tbody td:nth-child(5) { display: none; }
}

/* ── MOBILE RESPONSIVE ── */
@media (max-width: 768px) {
  .don-summary {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 10px;
  }

  .filtros-panel {
    flex-direction: column;
    gap: 10px;
    padding: 14px;
  }

  .filtro-group,
  .filtro-group--tabs,
  .filtro-group--btn {
    min-width: unset;
    width: 100%;
    flex: none;
  }

  .tabs-group {
    overflow-x: auto;
    -webkit-overflow-scrolling: touch;
    height: auto;
    padding: 4px;
    flex-wrap: nowrap;
  }

  .tab-btn {
    white-space: nowrap;
    flex-shrink: 0;
    font-size: 11px;
    padding: 5px 10px;
    height: auto;
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

  .btn-primary {
    width: 100%;
    justify-content: center;
  }

  .modal-box--lg {
    max-width: calc(100vw - 24px);
    padding: 22px 14px;
    max-height: 95vh;
  }

  .form-grid--4 {
    grid-template-columns: repeat(2, 1fr);
  }

  .fg--span2 { grid-column: span 1; }
  .fg--full { grid-column: span 2; }

  .modal-grid { grid-template-columns: 1fr; }

  .modal-footer {
    padding-top: 16px;
    flex-direction: column;
    gap: 8px;
  }

  .modal-footer .btn-cancel,
  .modal-footer .btn-save {
    width: 100%;
    justify-content: center;
  }

  .pet-selector-btn { font-size: 12px; }
}

@media (max-width: 480px) {
  .don-summary { grid-template-columns: 1fr; }

  .form-grid--4 { grid-template-columns: 1fr; }
  .fg--span2,
  .fg--full { grid-column: span 1; }
}


</style>