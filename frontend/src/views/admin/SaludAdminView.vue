<script setup>
import { ref, computed, watch } from 'vue'
import { usePetsStore } from '../../stores/usePetsStore'

const store = usePetsStore()

/* ─── Veterinarios ────────────────────────────────────────── */
const veterinarios = ref([])
function cargarVeterinarios() {
  const usuarios = JSON.parse(localStorage.getItem('anhelo_usuarios')) || []
  veterinarios.value = usuarios.filter(
    u =>
      u.rol === 'Voluntario' &&
      u.solicitudVoluntario?.estado === 'Aprobada' &&
      (u.tipoVoluntario === 'Veterinaria' || u.solicitudVoluntario?.tipo === 'Veterinaria')
  )
}
cargarVeterinarios()

/* ─── Tabs ────────────────────────────────────────────────── */
const activeTab = ref('historial')
const SECCION_TABS = [
  { v: 'historial', l: 'Historial' },
  { v: 'vacunas', l: 'Vacunas' },
  { v: 'tratamientos', l: 'Tratamientos' },
]

/* ─── Modales ─────────────────────────────────────────────── */
const showModalRegistrar    = ref(false)
const showModalVer          = ref(false)
const showModalConfirm      = ref(false)
const showPetDropdown       = ref(false)
const showVetDropdown       = ref(false)
const showVetDropdownVacuna = ref(false)
const registroVer = ref(null)

/* ─── Toast ───────────────────────────────────────────────── */
const toast = ref({ show: false, type: 'success', message: '' })
let toastTimer = null
function showToast(type, message) {
  clearTimeout(toastTimer)
  toast.value = { show: true, type, message }
  toastTimer = setTimeout(() => { toast.value.show = false }, 3500)
}

/* ─── Mascota seleccionada / errores ─────────────────────── */
const petSeleccionada = ref(null)
const errores = ref({})

/* ─── Persistencia ────────────────────────────────────────── */
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

/* ─── Formulario unificado ────────────────────────────────── */
function formDataInicial() {
  return {
    fecha: '', vet: '', clinica: '', peso: '', diagnostico: '', observaciones_h: '',
    tipoVacuna: '', fechaAplicacion: '', proximaDosis: '', vetVacuna: '', clinicaVacuna: '', observaciones_v: '',
    tipoTratamiento: '', medicamento: '', dosis: '', fechaTrat: '', observaciones_t: '',
  }
}
const form = ref(formDataInicial())

function resetForm() {
  form.value = formDataInicial()
  petSeleccionada.value = null
  showPetDropdown.value = false
  showVetDropdown.value = false
  showVetDropdownVacuna.value = false
  errores.value = {}
}

/* ─── Filtros ─────────────────────────────────────────────── */
const search     = ref('')
const filterFrom = ref('')
const filterTo   = ref('')

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
      petEspecie: pet.species || pet.especie || pet.tipo || pet.type || '',
      petRaza:    pet.raza || pet.breed || pet.raza_mascota || '',
      petFoto:
        pet.images?.[0]?.preview ||
        pet.foto || pet.image || pet.photo || pet.avatar || null,
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
function limpiarFiltros() {
  search.value = ''
  filterFrom.value = ''
  filterTo.value = ''
}

/* ─── Generador de ID ─────────────────────────────────────── */
function generarId(prefijo, lista) {
  return `${prefijo}-${String(lista.length + 1).padStart(3, '0')}`
}

/* ─── Validación ──────────────────────────────────────────── */
function validar() {
  const e = {}
  if (!petSeleccionada.value)               e.pet             = 'Selecciona una mascota'
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
      id: generarId('SAL', d.medicalHistory),
      fecha: form.value.fecha, vet: form.value.vet, peso: form.value.peso,
      diagnostico: form.value.diagnostico, observaciones: form.value.observaciones_h,
      creadoEn: new Date().toISOString()
    })
    d.vaccines.push({
      id: generarId('VAC', d.vaccines),
      tipo: form.value.tipoVacuna, fechaAplicacion: form.value.fechaAplicacion,
      proximaDosis: form.value.proximaDosis, vet: form.value.vetVacuna,
      observaciones: form.value.observaciones_v, creadoEn: new Date().toISOString()
    })
    d.treatments.push({
      id: generarId('TRA', d.treatments),
      tipo: form.value.tipoTratamiento, medicamento: form.value.medicamento,
      dosis: form.value.dosis, fecha: form.value.fechaTrat,
      observaciones: form.value.observaciones_t, creadoEn: new Date().toISOString()
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
function cerrarModalRegistrar() {
  showModalRegistrar.value = false
  resetForm()
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

/* ─── KPIs ────────────────────────────────────────────────── */
const stats = computed(() => {
  let historial = 0, vacunas = 0, tratamientos = 0
  store.pets.forEach(pet => {
    historial    += datos.value[pet.id]?.medicalHistory?.length || 0
    vacunas      += datos.value[pet.id]?.vaccines?.length || 0
    tratamientos += datos.value[pet.id]?.treatments?.length || 0
  })
  return {
    historial, vacunas, tratamientos,
    mascotas: store.pets.filter(p => p.active !== false).length,
  }
})

function iniciales(nombre) {
  if (!nombre) return '?'
  return nombre.trim().split(' ').map(p => p[0]).slice(0, 2).join('').toUpperCase()
}
</script>

<template>
  <div class="view-container">

    <!-- ── Toast ── -->
    <Teleport to="body">
      <Transition name="toast-fade">
        <div v-if="toast.show" class="don-toast" :class="toast.type">
          <span class="don-toast-dot"></span>
          {{ toast.message }}
        </div>
      </Transition>
    </Teleport>

    <!-- ══════════════════════════════════════
         MODAL 1/2 — NUEVO EXPEDIENTE
         Mismo ancho y alto que Ver (.modal-box--uniform)
    ══════════════════════════════════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showModalRegistrar" class="modal-overlay" @click.self="cerrarModalRegistrar">
          <div class="modal-box modal-box--uniform">
            <button class="close-btn" @click="cerrarModalRegistrar">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>

            <div class="form-header">
              <p class="form-eyebrow">Expediente médico</p>
              <h2 class="form-title">Nuevo registro completo</h2>
              <p class="form-sub">Registra historial, vacuna y tratamiento en un mismo expediente</p>
            </div>

            <div class="uniform-scroll">
              <div class="form-body">

                <!-- Selector de mascota -->
                <div class="form-section">
                  <div class="form-section-label"><span class="form-num">1</span> Mascota</div>
                  <div class="form-grid">
                    <div class="fg fg--full">
                      <div class="pet-select-wrap">
                        <button type="button" class="pet-select-btn" :class="{ 'is-error': errores.pet }" @click="showPetDropdown = !showPetDropdown">
                          <template v-if="petSeleccionada">
                            <div class="pet-avatar pet-avatar--sm">
                              <img v-if="petSeleccionada.foto || petSeleccionada.image || petSeleccionada.photo || petSeleccionada.avatar" :src="petSeleccionada.foto || petSeleccionada.image || petSeleccionada.photo || petSeleccionada.avatar" class="pet-avatar-img" />
                              <span v-else class="pet-avatar-ini">{{ iniciales(petSeleccionada.name) }}</span>
                            </div>
                            <span class="psel-name">{{ petSeleccionada.name }}</span>
                            <span class="psel-species">{{ petSeleccionada.species }}</span>
                          </template>
                          <template v-else>
                            <span class="psel-placeholder">Seleccionar mascota...</span>
                          </template>
                          <svg class="psel-chevron" :class="{ open: showPetDropdown }" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
                        </button>
                        <p v-if="errores.pet" class="err-msg">{{ errores.pet }}</p>
                        <div v-if="showPetDropdown" class="pet-dropdown">
                          <div v-if="mascotasActivas.length === 0" class="dropdown-empty">No hay mascotas activas registradas</div>
                          <div v-for="pet in mascotasActivas" :key="pet.id" class="dropdown-item" :class="{ selected: petSeleccionada?.id === pet.id }" @click="seleccionarPet(pet)">
                            <div class="pet-avatar pet-avatar--sm">
                              <img v-if="pet.foto || pet.image || pet.photo || pet.avatar" :src="pet.foto || pet.image || pet.photo || pet.avatar" class="pet-avatar-img" />
                              <span v-else class="pet-avatar-ini">{{ iniciales(pet.name) }}</span>
                            </div>
                            <div class="dropdown-info">
                              <span class="dropdown-name">{{ pet.name }}</span>
                              <span class="dropdown-sub">{{ pet.species }}</span>
                            </div>
                            <svg v-if="petSeleccionada?.id === pet.id" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" style="color:#92A894;flex-shrink:0"><polyline points="20 6 9 17 4 12"/></svg>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>

                <!-- Sección: Historial médico -->
                <div class="form-section">
                  <div class="form-section-label"><span class="form-num">2</span> Historial médico</div>
                  <div class="form-grid">
                    <div class="fg">
                      <label>Fecha <span class="req">*</span></label>
                      <input type="date" class="input" :class="{ 'is-error': errores.fecha }" v-model="form.fecha" @change="clearErr('fecha')" />
                      <p v-if="errores.fecha" class="err-msg">{{ errores.fecha }}</p>
                    </div>
                    <div class="fg">
                      <label>Peso (kg)</label>
                      <input type="number" class="input" placeholder="Ej. 12.5" step="0.1" min="0" v-model="form.peso" />
                    </div>
                    <div class="fg">
                      <label>Veterinario responsable <span class="req">*</span></label>
                      <div class="pet-select-wrap">
                        <button type="button" class="pet-select-btn" :class="{ 'is-error': errores.vet }" @click="showVetDropdown = !showVetDropdown">
                          <template v-if="form.vet">
                            <div class="pet-avatar pet-avatar--sm"><span class="pet-avatar-ini">{{ form.vet.charAt(0) }}</span></div>
                            <span class="psel-name">{{ form.vet }}</span>
                          </template>
                          <template v-else><span class="psel-placeholder">Seleccionar veterinario...</span></template>
                          <svg class="psel-chevron" :class="{ open: showVetDropdown }" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
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
                      <p v-if="errores.vet" class="err-msg">{{ errores.vet }}</p>
                    </div>
                    <div class="fg">
                      <label>Clínica veterinaria</label>
                      <input type="text" class="input" placeholder="Ej. Hospital Veterinario San José" v-model="form.clinica" />
                    </div>
                    <div class="fg fg--full">
                      <label>Diagnóstico <span class="req">*</span></label>
                      <input type="text" class="input" :class="{ 'is-error': errores.diagnostico }" placeholder="Ej. Control preventivo, otitis externa..." v-model="form.diagnostico" @input="clearErr('diagnostico')" />
                      <p v-if="errores.diagnostico" class="err-msg">{{ errores.diagnostico }}</p>
                    </div>
                    <div class="fg fg--full">
                      <label>Observaciones</label>
                      <textarea class="textarea" placeholder="Indicaciones, seguimiento, notas clínicas..." v-model="form.observaciones_h"></textarea>
                    </div>
                  </div>
                </div>

                <!-- Sección: Vacuna -->
                <div class="form-section">
                  <div class="form-section-label"><span class="form-num">3</span> Vacuna</div>
                  <div class="form-grid">
                    <div class="fg fg--span2">
                      <label>Tipo de vacuna <span class="req">*</span></label>
                      <input type="text" class="input" :class="{ 'is-error': errores.tipoVacuna }" placeholder="Ej. Antirrábica, Parvovirus..." v-model="form.tipoVacuna" @input="clearErr('tipoVacuna')" />
                      <p v-if="errores.tipoVacuna" class="err-msg">{{ errores.tipoVacuna }}</p>
                    </div>
                    <div class="fg">
                      <label>Fecha de aplicación <span class="req">*</span></label>
                      <input type="date" class="input" :class="{ 'is-error': errores.fechaAplicacion }" v-model="form.fechaAplicacion" @change="clearErr('fechaAplicacion')" />
                      <p v-if="errores.fechaAplicacion" class="err-msg">{{ errores.fechaAplicacion }}</p>
                    </div>
                    <div class="fg">
                      <label>Próxima dosis</label>
                      <input type="date" class="input" v-model="form.proximaDosis" />
                    </div>
                    <div class="fg fg--span2">
                      <label>Veterinario responsable</label>
                      <div class="pet-select-wrap">
                        <button type="button" class="pet-select-btn" @click="showVetDropdownVacuna = !showVetDropdownVacuna">
                          <template v-if="form.vetVacuna">
                            <div class="pet-avatar pet-avatar--sm"><span class="pet-avatar-ini">{{ form.vetVacuna.charAt(0) }}</span></div>
                            <span class="psel-name">{{ form.vetVacuna }}</span>
                          </template>
                          <template v-else><span class="psel-placeholder">Seleccionar veterinario...</span></template>
                          <svg class="psel-chevron" :class="{ open: showVetDropdownVacuna }" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
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
                      <label>Observaciones</label>
                      <textarea class="textarea" placeholder="Notas sobre la vacuna, lote, reacciones..." v-model="form.observaciones_v"></textarea>
                    </div>
                  </div>
                </div>

                <!-- Sección: Tratamiento -->
                <div class="form-section">
                  <div class="form-section-label"><span class="form-num">4</span> Tratamiento</div>
                  <div class="form-grid">
                    <div class="fg fg--span2">
                      <label>Tipo de tratamiento <span class="req">*</span></label>
                      <input type="text" class="input" :class="{ 'is-error': errores.tipoTratamiento }" placeholder="Ej. Desparasitación, antibiótico..." v-model="form.tipoTratamiento" @input="clearErr('tipoTratamiento')" />
                      <p v-if="errores.tipoTratamiento" class="err-msg">{{ errores.tipoTratamiento }}</p>
                    </div>
                    <div class="fg">
                      <label>Fecha <span class="req">*</span></label>
                      <input type="date" class="input" :class="{ 'is-error': errores.fechaTrat }" v-model="form.fechaTrat" @change="clearErr('fechaTrat')" />
                      <p v-if="errores.fechaTrat" class="err-msg">{{ errores.fechaTrat }}</p>
                    </div>
                    <div class="fg">
                      <label>Dosis</label>
                      <input type="text" class="input" placeholder="Ej. 5mg/kg" v-model="form.dosis" />
                    </div>
                    <div class="fg fg--span2">
                      <label>Medicamento</label>
                      <input type="text" class="input" placeholder="Nombre del medicamento" v-model="form.medicamento" />
                    </div>
                    <div class="fg fg--full">
                      <label>Observaciones</label>
                      <textarea class="textarea" placeholder="Duración, respuesta al tratamiento, seguimiento..." v-model="form.observaciones_t"></textarea>
                    </div>
                  </div>
                </div>

                <div class="immutable-note">
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
                  Los registros médicos son permanentes y no pueden editarse ni eliminarse una vez guardados.
                </div>

              </div>
            </div>

            <div class="form-footer">
              <button class="btn-cancel" @click="cerrarModalRegistrar">Cancelar</button>
              <button class="btn-save" @click="intentarGuardar">
                <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
                <span>Guardar expediente</span>
              </button>
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
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
          </div>
          <div>
            <h1 class="admin-page-title">Control de salud</h1>
            <p class="admin-page-sub">Historial médico, vacunas y tratamientos</p>
          </div>
        </div>
        <button class="btn btn--primary" @click="abrirModal">
          <svg class="btn-ico" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
          <span>Nuevo expediente</span>
        </button>
      </header>

      <div class="don-summary">
        <div class="don-card">
          <div class="don-icon historial-icon">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
          </div>
          <strong class="don-value">{{ stats.historial }}</strong>
          <span class="don-label">Historial</span>
          <span class="don-desc">Consultas registradas</span>
        </div>
        <div class="don-card">
          <div class="don-icon vacunas-icon">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M22 12h-4l-3 9L9 3l-3 9H2"/></svg>
          </div>
          <strong class="don-value">{{ stats.vacunas }}</strong>
          <span class="don-label">Vacunas</span>
          <span class="don-desc">Dosis administradas</span>
        </div>
        <div class="don-card">
          <div class="don-icon tratamientos-icon">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="7" height="7" rx="1.5"/><rect x="14" y="3" width="7" height="7" rx="1.5"/><rect x="14" y="14" width="7" height="7" rx="1.5"/><rect x="3" y="14" width="7" height="7" rx="1.5"/></svg>
          </div>
          <strong class="don-value">{{ stats.tratamientos }}</strong>
          <span class="don-label">Tratamientos</span>
          <span class="don-desc">En seguimiento</span>
        </div>
        <div class="don-card">
          <div class="don-icon mascotas-icon">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/></svg>
          </div>
          <strong class="don-value">{{ stats.mascotas }}</strong>
          <span class="don-label">Mascotas activas</span>
          <span class="don-desc">Con expediente</span>
        </div>
      </div>

      <div class="filtros-panel">
        <div class="filtros-row">
          <div class="filtro-group filtro-group--tabs">
            <label class="filtro-label">Sección</label>
            <div class="tabs-wrap">
              <button v-for="t in SECCION_TABS" :key="t.v" class="tab-btn" :class="{ active: activeTab === t.v }" @click="activeTab = t.v">{{ t.l }}</button>
            </div>
          </div>
        </div>
        <div class="filtros-divider"></div>
        <div class="filtros-row filtros-row--end">
          <div class="filtro-group filtro-group--search">
            <label class="filtro-label">Buscar mascota</label>
            <div class="filtro-input-wrap">
              <span class="filtro-icon filtro-icon--left">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
              </span>
              <input v-model="search" placeholder="Nombre o ID..." class="filtro-input filtro-input--icon-left" />
            </div>
          </div>
          <div class="filtro-group">
            <label class="filtro-label">Desde</label>
            <input type="date" class="filtro-input" v-model="filterFrom" />
          </div>
          <div class="filtro-group">
            <label class="filtro-label">Hasta</label>
            <input type="date" class="filtro-input" v-model="filterTo" />
          </div>
          <div class="filtro-group filtro-group--btn">
            <button class="btn btn--ghost" :class="{ 'btn--ghost-active': hayFiltros }" @click="limpiarFiltros">Limpiar filtros</button>
          </div>
        </div>
      </div>

      <!-- TABLA HISTORIAL -->
      <div v-if="activeTab === 'historial'" class="table-wrapper">
        <div class="table-scroll">
          <table class="don-table">
            <thead>
              <tr><th>ID</th><th>Mascota</th><th>Fecha</th><th>Veterinario</th><th>Diagnóstico</th><th>Peso</th><th>Acciones</th></tr>
            </thead>
            <tbody>
              <tr v-for="r in registros" :key="r.id" class="don-row">
                <td><span class="id-pill">{{ r.id }}</span></td>
                <td>
                  <div class="pet-cell">
                    <div class="pet-avatar">
                      <img v-if="r.petFoto" :src="r.petFoto" class="pet-avatar-img" />
                      <span v-else class="pet-avatar-ini">{{ iniciales(r.petNombre) }}</span>
                    </div>
                    <div>
                      <span class="donor-name">{{ r.petNombre }}</span>
                      <span class="donor-mail">{{ r.petId }}</span>
                    </div>
                  </div>
                </td>
                <td><span class="fecha-text">{{ formatFecha(r.fecha) }}</span></td>
                <td><span class="fecha-text">{{ r.vet || '—' }}</span></td>
                <td><span class="donor-name">{{ r.diagnostico }}</span></td>
                <td><span class="fecha-text">{{ r.peso ? r.peso + ' kg' : '—' }}</span></td>
                <td>
                  <div class="action-group">
                    <button class="icon-only icon-only--ver" @click="verRegistro(r)" data-tooltip="Ver detalle">
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <div v-if="registros.length === 0" class="empty-state">
          <svg width="36" height="36" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
          <p class="empty-title">{{ hayFiltros ? 'Sin resultados para los filtros aplicados' : 'Sin registros de historial médico' }}</p>
          <p class="empty-sub">{{ hayFiltros ? 'Ajusta los filtros para ver más resultados.' : 'Registra el primer expediente con el botón superior.' }}</p>
        </div>
        <div v-else class="table-footer">
          {{ registros.length }} registro{{ registros.length !== 1 ? 's' : '' }} encontrado{{ registros.length !== 1 ? 's' : '' }}
        </div>
      </div>

      <!-- TABLA VACUNAS -->
      <div v-if="activeTab === 'vacunas'" class="table-wrapper">
        <div class="table-scroll">
          <table class="don-table">
            <thead>
              <tr><th>ID</th><th>Mascota</th><th>Vacuna</th><th>Aplicación</th><th>Próxima dosis</th><th>Veterinario</th><th>Acciones</th></tr>
            </thead>
            <tbody>
              <tr v-for="r in registros" :key="r.id" class="don-row">
                <td><span class="id-pill">{{ r.id }}</span></td>
                <td>
                  <div class="pet-cell">
                    <div class="pet-avatar">
                      <img v-if="r.petFoto" :src="r.petFoto" class="pet-avatar-img" />
                      <span v-else class="pet-avatar-ini">{{ iniciales(r.petNombre) }}</span>
                    </div>
                    <div>
                      <span class="donor-name">{{ r.petNombre }}</span>
                      <span class="donor-mail">{{ r.petId }}</span>
                    </div>
                  </div>
                </td>
                <td><span class="donor-name">{{ r.tipo }}</span></td>
                <td><span class="fecha-text">{{ formatFecha(r.fechaAplicacion) }}</span></td>
                <td>
                  <span v-if="r.proximaDosis" class="estado-badge badge-aprobada">{{ formatFecha(r.proximaDosis) }}</span>
                  <span v-else class="fecha-text">—</span>
                </td>
                <td><span class="fecha-text">{{ r.vet || '—' }}</span></td>
                <td>
                  <div class="action-group">
                    <button class="icon-only icon-only--ver" @click="verRegistro(r)" data-tooltip="Ver detalle">
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <div v-if="registros.length === 0" class="empty-state">
          <svg width="36" height="36" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M22 12h-4l-3 9L9 3l-3 9H2"/></svg>
          <p class="empty-title">{{ hayFiltros ? 'Sin resultados para los filtros aplicados' : 'Sin registros de vacunas' }}</p>
          <p class="empty-sub">{{ hayFiltros ? 'Ajusta los filtros para ver más resultados.' : 'Registra el primer expediente con el botón superior.' }}</p>
        </div>
        <div v-else class="table-footer">
          {{ registros.length }} registro{{ registros.length !== 1 ? 's' : '' }} encontrado{{ registros.length !== 1 ? 's' : '' }}
        </div>
      </div>

      <!-- TABLA TRATAMIENTOS -->
      <div v-if="activeTab === 'tratamientos'" class="table-wrapper">
        <div class="table-scroll">
          <table class="don-table">
            <thead>
              <tr><th>ID</th><th>Mascota</th><th>Tratamiento</th><th>Fecha</th><th>Medicamento</th><th>Dosis</th><th>Acciones</th></tr>
            </thead>
            <tbody>
              <tr v-for="r in registros" :key="r.id" class="don-row">
                <td><span class="id-pill">{{ r.id }}</span></td>
                <td>
                  <div class="pet-cell">
                    <div class="pet-avatar">
                      <img v-if="r.petFoto" :src="r.petFoto" class="pet-avatar-img" />
                      <span v-else class="pet-avatar-ini">{{ iniciales(r.petNombre) }}</span>
                    </div>
                    <div>
                      <span class="donor-name">{{ r.petNombre }}</span>
                      <span class="donor-mail">{{ r.petId }}</span>
                    </div>
                  </div>
                </td>
                <td><span class="donor-name">{{ r.tipo }}</span></td>
                <td><span class="fecha-text">{{ formatFecha(r.fecha) }}</span></td>
                <td><span class="fecha-text">{{ r.medicamento || '—' }}</span></td>
                <td><span class="fecha-text">{{ r.dosis || '—' }}</span></td>
                <td>
                  <div class="action-group">
                    <button class="icon-only icon-only--ver" @click="verRegistro(r)" data-tooltip="Ver detalle">
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <div v-if="registros.length === 0" class="empty-state">
          <svg width="36" height="36" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/></svg>
          <p class="empty-title">{{ hayFiltros ? 'Sin resultados para los filtros aplicados' : 'Sin registros de tratamientos' }}</p>
          <p class="empty-sub">{{ hayFiltros ? 'Ajusta los filtros para ver más resultados.' : 'Registra el primer expediente con el botón superior.' }}</p>
        </div>
        <div v-else class="table-footer">
          {{ registros.length }} registro{{ registros.length !== 1 ? 's' : '' }} encontrado{{ registros.length !== 1 ? 's' : '' }}
        </div>
      </div>
    </div>

    <!-- ══════════════════════════════════════
         MODAL 2/2 — VER DETALLE
         Mismo ancho y alto que Nuevo expediente (.modal-box--uniform)
    ══════════════════════════════════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showModalVer && registroVer" class="modal-overlay" @click.self="showModalVer = false">
          <div class="modal-box modal-box--uniform">
            <button class="close-btn close-btn--hero" @click="showModalVer = false">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>

            <div class="hero">
              <div class="hero-photo">
                <img v-if="registroVer.petFoto" :src="registroVer.petFoto" :alt="registroVer.petNombre" />
                <span v-else class="hero-photo-ini">{{ iniciales(registroVer.petNombre) }}</span>
              </div>
              <div class="hero-info">
                <div class="hero-name-row">
                  <h2 class="hero-name">{{ registroVer.petNombre }}</h2>
                  <span class="estado-badge badge-status-hero" :class="registroVer.petActiva ? 'badge-aprobada' : 'badge-inactiva'">{{ registroVer.petActiva ? 'Activa' : 'Inactiva' }}</span>
                </div>
                <div class="hero-meta">
                  <span class="hero-meta-chip">
                    <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/></svg>
                    {{ registroVer.petEspecie || 'Especie no indicada' }}
                  </span>
                  <span v-if="registroVer.petRaza" class="hero-meta-chip">{{ registroVer.petRaza }}</span>
                  <span class="hero-meta-id">{{ registroVer.petId }}</span>
                </div>
              </div>
            </div>

            <div class="uniform-scroll">
              <div class="body">
                <!-- Historial -->
                <template v-if="registroVer.id?.startsWith('SAL')">
                  <div class="block">
                    <h4 class="block-title">
                      <span class="block-title-icon">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
                      </span>
                      Historial médico
                    </h4>
                    <div class="fields-row">
                      <div class="field-col"><span class="field-label-row">ID</span><span class="field-value">{{ registroVer.id }}</span></div>
                      <div class="field-col"><span class="field-label-row">Fecha</span><span class="field-value">{{ formatFecha(registroVer.fecha) }}</span></div>
                      <div class="field-col"><span class="field-label-row">Veterinario</span><span class="field-value">{{ registroVer.vet || '—' }}</span></div>
                      <div class="field-col"><span class="field-label-row">Peso</span><span class="field-value">{{ registroVer.peso ? registroVer.peso + ' kg' : '—' }}</span></div>
                    </div>
                  </div>
                  <div class="block">
                    <h4 class="block-title">
                      <span class="block-title-icon">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="9"/><polyline points="12 7 12 12 16 14"/></svg>
                      </span>
                      Diagnóstico
                    </h4>
                    <div class="tint-box">
                      <span>{{ registroVer.diagnostico }}</span>
                    </div>
                  </div>
                  <div class="block" v-if="registroVer.observaciones">
                    <h4 class="block-title">
                      <span class="block-title-icon">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
                      </span>
                      Observaciones
                    </h4>
                    <div class="tint-box tint-box--desc">
                      <span>{{ registroVer.observaciones }}</span>
                    </div>
                  </div>
                </template>

                <!-- Vacuna -->
                <template v-if="registroVer.id?.startsWith('VAC')">
                  <div class="block">
                    <h4 class="block-title">
                      <span class="block-title-icon">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 12h-4l-3 9L9 3l-3 9H2"/></svg>
                      </span>
                      Vacuna
                    </h4>
                    <div class="fields-row">
                      <div class="field-col"><span class="field-label-row">ID</span><span class="field-value">{{ registroVer.id }}</span></div>
                      <div class="field-col"><span class="field-label-row">Tipo</span><span class="field-value">{{ registroVer.tipo }}</span></div>
                      <div class="field-col"><span class="field-label-row">Aplicación</span><span class="field-value">{{ formatFecha(registroVer.fechaAplicacion) }}</span></div>
                      <div class="field-col"><span class="field-label-row">Próxima dosis</span><span class="field-value">{{ registroVer.proximaDosis ? formatFecha(registroVer.proximaDosis) : '—' }}</span></div>
                      <div class="field-col"><span class="field-label-row">Veterinario</span><span class="field-value">{{ registroVer.vet || '—' }}</span></div>
                    </div>
                  </div>
                  <div class="block" v-if="registroVer.observaciones">
                    <h4 class="block-title">
                      <span class="block-title-icon">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
                      </span>
                      Observaciones
                    </h4>
                    <div class="tint-box tint-box--desc">
                      <span>{{ registroVer.observaciones }}</span>
                    </div>
                  </div>
                </template>

                <!-- Tratamiento -->
                <template v-if="registroVer.id?.startsWith('TRA')">
                  <div class="block">
                    <h4 class="block-title">
                      <span class="block-title-icon">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="7" height="7" rx="1.5"/><rect x="14" y="3" width="7" height="7" rx="1.5"/><rect x="14" y="14" width="7" height="7" rx="1.5"/><rect x="3" y="14" width="7" height="7" rx="1.5"/></svg>
                      </span>
                      Tratamiento
                    </h4>
                    <div class="fields-row">
                      <div class="field-col"><span class="field-label-row">ID</span><span class="field-value">{{ registroVer.id }}</span></div>
                      <div class="field-col"><span class="field-label-row">Tipo</span><span class="field-value">{{ registroVer.tipo }}</span></div>
                      <div class="field-col"><span class="field-label-row">Fecha</span><span class="field-value">{{ formatFecha(registroVer.fecha) }}</span></div>
                      <div class="field-col"><span class="field-label-row">Medicamento</span><span class="field-value">{{ registroVer.medicamento || '—' }}</span></div>
                      <div class="field-col"><span class="field-label-row">Dosis</span><span class="field-value">{{ registroVer.dosis || '—' }}</span></div>
                    </div>
                  </div>
                  <div class="block" v-if="registroVer.observaciones">
                    <h4 class="block-title">
                      <span class="block-title-icon">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
                      </span>
                      Observaciones
                    </h4>
                    <div class="tint-box tint-box--desc">
                      <span>{{ registroVer.observaciones }}</span>
                    </div>
                  </div>
                </template>
              </div>
            </div>

            <div class="footer">
              <button class="btn-ghost-red" @click="showModalVer = false">
                Cerrar expediente
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- ══════════════════════════════════════
         Modal: Confirmar guardado (sin cambios de diseño)
    ══════════════════════════════════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showModalConfirm" class="modal-overlay modal-overlay--top" @click.self="showModalConfirm = false">
          <div class="modal-box modal-box--sm">
            <button class="btn btn--icon btn--icon-close" @click="showModalConfirm = false">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>
            <div class="modal-header">
              <div class="modal-header-info">
                <p class="modal-eyebrow">Confirmar guardado</p>
                <h2 class="modal-title">{{ petSeleccionada?.name }}</h2>
              </div>
            </div>
            <div class="modal-section">
              <p class="confirm-desc">Se registrará el historial médico, la vacuna y el tratamiento para <strong>{{ petSeleccionada?.name }}</strong>. Esta acción es permanente y no podrá modificarse.</p>
            </div>
            <div class="modal-acciones">
              <button class="btn btn--ghost" @click="showModalConfirm = false">Cancelar</button>
              <button class="btn btn--primary" @click="confirmarGuardar">
                <svg class="btn-ico" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
                <span>Confirmar y guardar</span>
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

  </div>
</template>

<style scoped>
/* ── Variables ─────────────────────────────────────── */
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
  --btn-height:      38px;
  --btn-radius:      9px;
  --btn-pad-x:       15px;
  --btn-icon-size:   15px;
  --btn-icon-gap:    7px;
  --btn-font-size:   13px;
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
.btn { display:inline-flex; align-items:center; justify-content:center; gap:var(--btn-icon-gap); height:var(--btn-height); padding:0 var(--btn-pad-x); border-radius:var(--btn-radius); border:1px solid transparent; font-family:inherit; font-size:var(--btn-font-size); font-weight:var(--btn-font-weight); line-height:1; white-space:nowrap; cursor:pointer; user-select:none; transition:background-color var(--btn-transition), border-color var(--btn-transition), color var(--btn-transition), box-shadow var(--btn-transition), transform var(--btn-transition); }
.btn-ico, .btn :deep(svg) { width:var(--btn-icon-size); height:var(--btn-icon-size); flex-shrink:0; }
.btn:active:not(:disabled) { transform:translateY(1px); }
.btn:focus-visible { outline:none; box-shadow:0 0 0 3px rgba(58,71,60,.16); }
.btn--primary { background:var(--verde); color:#fff; box-shadow:0 1px 2px rgba(58,71,60,.12), 0 4px 10px -4px rgba(58,71,60,.35); letter-spacing:.1px; }
.btn--primary:hover:not(:disabled) { background:#465747; box-shadow:0 1px 2px rgba(58,71,60,.14), 0 6px 14px -4px rgba(58,71,60,.4); }
.btn--ghost { background:var(--blanco); color:var(--texto-sec); border-color:var(--borde); }
.btn--ghost:hover:not(:disabled) { background:#FAFBFA; color:var(--texto); border-color:#D3D8D3; }
.btn--ghost-active { border-color:var(--verde-sec); color:var(--verde); }
.btn--ghost-active:hover:not(:disabled) { background:#F3F6F3; color:var(--verde); border-color:var(--verde-sec); }
.btn--icon { width:34px; height:34px; padding:0; border-radius:9px; background:var(--blanco); color:var(--texto-sec); border-color:var(--borde); position:relative; border-width:1px; border-style:solid; }
.btn--icon :deep(svg) { width:15px; height:15px; }
.btn--icon-close { position:absolute; top:18px; right:18px; width:30px; height:30px; border-radius:8px; background:var(--fondo); border-color:var(--borde); color:var(--texto); }
.btn--icon-close :deep(svg) { width:14px; height:14px; stroke-width:2.5; }
.btn--icon-close:hover:not(:disabled) { background:var(--verde); color:var(--blanco); border-color:var(--verde); }

/* ── Toast ── */
.don-toast { position:fixed; bottom:32px; right:32px; z-index:9999; display:flex; align-items:center; gap:10px; padding:14px 20px; border-radius:14px; font-size:14px; font-weight:600; box-shadow:0 8px 32px rgba(0,0,0,0.16); pointer-events:none; }
.don-toast.success { background:var(--verde); color:#fff; }
.don-toast.error { background:#c0392b; color:#fff; }
.don-toast-dot { width:8px; height:8px; border-radius:50%; background:rgba(255,255,255,0.5); flex-shrink:0; }
.toast-fade-enter-active, .toast-fade-leave-active { transition:all 0.25s ease; }
.toast-fade-enter-from, .toast-fade-leave-to { opacity:0; transform:translateY(10px); }

/* ── Encabezado ── */
.page-header { display:flex; justify-content:space-between; align-items:center; margin-bottom:24px; gap:16px; flex-wrap:wrap; }
.brand-row { display:flex; align-items:center; gap:12px; }
.brand-mark { width:38px; height:38px; min-width:38px; border-radius:11px; background:linear-gradient(150deg, var(--verde) 0%, #6E8870 100%); color:#fff; display:flex; align-items:center; justify-content:center; box-shadow:0 4px 10px -3px rgba(58,71,60,.45); }
.admin-page-title { font-size:22px; font-weight:700; color:var(--texto); letter-spacing:-0.4px; line-height:1.15; margin:0 0 2px; }
.admin-page-sub { font-size:12.5px; color:var(--texto-sec); font-weight:500; margin:0; }

/* ── Tarjetas resumen ── */
.don-summary { display:grid; grid-template-columns:repeat(4, 1fr); gap:12px; margin-bottom:20px; }
.don-card { background:var(--blanco); border-radius:16px; padding:16px 15px; border:1px solid var(--borde); box-shadow:var(--sombra-sm); display:flex; flex-direction:column; transition:box-shadow .18s ease, border-color .18s ease; }
.don-card:hover { border-color:#D7DED8; box-shadow:var(--sombra-md); }
.don-icon { width:32px; height:32px; border-radius:50%; display:flex; align-items:center; justify-content:center; margin-bottom:12px; border:1px solid transparent; }
.historial-icon    { background:#F2F3F2; border-color:#DFE2DF; color:#616861; }
.vacunas-icon       { background:#EAF2F6; border-color:#C7DCE6; color:#3C6E85; }
.tratamientos-icon  { background:#FDF6E8; border-color:#F2E1B8; color:#A97A0C; }
.mascotas-icon      { background:#EDF6EF; border-color:#C9E4CE; color:#2E7D45; }
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
.filtro-input--icon-left { padding-left:36px; }
.filtro-icon { position:absolute; display:flex; align-items:center; color:var(--texto-sec); }
.filtro-icon--left { left:12px; }

/* ── Estado vacío ── */
.empty-state { text-align:center; padding:72px 24px; background:var(--blanco); color:var(--verde-sec); display:flex; flex-direction:column; align-items:center; gap:10px; }
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
.pet-cell { display:flex; align-items:center; gap:10px; }
.pet-avatar { width:38px; height:38px; border-radius:50%; overflow:hidden; flex-shrink:0; background:#F1F5F1; display:flex; align-items:center; justify-content:center; border:1px solid var(--borde); }
.pet-avatar--sm { width:32px; height:32px; }
.pet-avatar-img { width:100%; height:100%; object-fit:cover; display:block; }
.pet-avatar-ini { font-size:14px; font-weight:700; color:#4E6E51; text-transform:uppercase; line-height:1; }
.pet-avatar--sm .pet-avatar-ini { font-size:11px; }
.id-pill { font-size:11px; font-family:ui-monospace, Menlo, Consolas, monospace; background:var(--fondo); border:1px solid var(--borde); padding:3px 9px; border-radius:6px; color:var(--texto); font-weight:700; white-space:nowrap; }
.donor-name { display:block; font-size:12.5px; font-weight:700; color:var(--texto); line-height:1.3; }
.donor-mail { display:block; font-size:11px; color:var(--texto-sec); margin-top:2px; }
.fecha-text { font-size:12.5px; color:var(--texto-sec); white-space:nowrap; }
.estado-badge { display:inline-block; font-size:10.5px; font-weight:700; padding:4px 11px; border-radius:20px; white-space:nowrap; }
.badge-aprobada { background:#EDF6EF; color:#2E7D32; }
.badge-inactiva { background:#F2F3F2; color:#7A827B; }
.table-footer { padding:12px 16px; border-top:1px solid var(--borde-suave); font-size:12px; color:var(--texto-sec); font-weight:500; }

/* Botones de acción de la tabla */
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
.modal-overlay--top { z-index:1100; }
.modal-box { background:var(--blanco); border-radius:22px; box-shadow:var(--sombra-md); position:relative; }
.modal-box--sm { max-width:480px; width:100%; padding:32px; max-height:90vh; overflow-y:auto; }

/* ══════════════════════════════════════════════
   .modal-box--uniform — Nuevo expediente / Ver
   MISMO ANCHO Y MISMO ALTO EXACTOS (sin cambios de dimensión).
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
.close-btn:hover { background:var(--verde); color:#fff; border-color:var(--verde); }
.close-btn--hero { color:#8B928A; background:var(--fondo); }
.close-btn--hero:hover { background:var(--verde); color:#fff; }

/* ── HERO (Ver detalle) — rediseño premium, sin foto de mascota ── */
.hero {
  flex-shrink:0;
  background:linear-gradient(165deg, #FFFFFF 0%, #F7FAF7 55%, #F1F7F2 100%);
  border-bottom:1px solid var(--borde-suave);
  padding:32px 40px 26px;
  display:flex; align-items:center; gap:18px;
}
.hero-photo {
  width:64px; height:64px; border-radius:18px; flex-shrink:0; overflow:hidden;
  background:linear-gradient(150deg,#E7F0E8 0%,#DCEBDE 100%);
  border:1px solid var(--borde-suave);
  display:flex; align-items:center; justify-content:center;
  box-shadow:0 1px 2px rgba(58,71,60,.04), 0 10px 22px -12px rgba(58,71,60,.28);
}
.hero-photo img { width:100%; height:100%; object-fit:cover; display:block; }
.hero-photo-ini { font-size:22px; font-weight:700; color:#3E7A45; letter-spacing:-.3px; }
.hero-info { flex:1; min-width:0; display:flex; flex-direction:column; gap:9px; }
.hero-name-row { display:flex; align-items:center; gap:12px; flex-wrap:wrap; }
.hero-name { font-size:24px; font-weight:700; color:var(--texto); margin:0; letter-spacing:-.5px; }
.hero-meta { display:flex; align-items:center; gap:8px; flex-wrap:wrap; }
.hero-meta-chip {
  display:inline-flex; align-items:center; gap:6px; font-size:12px; font-weight:600; color:#4B5A4C;
  background:var(--blanco); border:1px solid var(--borde-suave); padding:5px 11px 5px 9px; border-radius:20px;
}
.hero-meta-chip svg { color:var(--verde-sec); flex-shrink:0; }
.hero-meta-id { font-size:11.5px; color:var(--texto-ter); font-weight:600; font-family:ui-monospace, Menlo, Consolas, monospace; }
.badge-status-hero { padding:5px 12px !important; font-size:10.5px !important; }

/* ── BODY (Ver detalle) ── */
.body { padding:26px 40px 10px; display:flex; flex-direction:column; gap:14px; }
.block { background:var(--blanco); border:1px solid var(--borde-suave); border-radius:14px; padding:20px 22px; box-shadow:var(--sombra-sm); }
.block-title { display:flex; align-items:center; gap:10px; font-size:12.5px; font-weight:700; color:var(--texto); text-transform:uppercase; letter-spacing:.4px; margin:0 0 16px; }
.block-title-icon { width:26px; height:26px; border-radius:50%; background:#F0F5F0; color:#4E7A54; display:flex; align-items:center; justify-content:center; flex-shrink:0; }
.fields-row { display:grid; grid-template-columns:repeat(auto-fit, minmax(120px, 1fr)); gap:18px; }
.field-col { display:flex; flex-direction:column; gap:6px; }
.field-label-row { font-size:10px; font-weight:700; color:var(--texto-ter); text-transform:uppercase; letter-spacing:.5px; }
.field-value { font-size:14.5px; font-weight:600; color:var(--texto); }
.tint-box { background:var(--fondo); border-radius:10px; padding:14px 16px; }
.tint-box span { font-size:13.5px; font-weight:600; color:var(--texto); line-height:1.5; }
.tint-box--desc span { font-weight:500; color:#4B534A; }

/* ── FOOTER (Ver detalle) ── */
.footer { flex-shrink:0; display:flex; justify-content:flex-end; padding:16px 40px 20px; border-top:1px solid var(--borde-suave); }
.btn-ghost-red { display:flex; align-items:center; gap:8px; height:38px; padding:0 16px; border-radius:9px; background:var(--blanco); border:1px solid var(--borde); color:var(--texto-sec); font-size:13px; font-weight:600; cursor:pointer; transition:background-color .16s ease, border-color .16s ease, color .16s ease; }
.btn-ghost-red:hover { background:#FDF4F3; border-color:#E8B9B2; color:var(--rojo); }

/* ══════════════════════════════════════════════
   FORMULARIO (Nuevo expediente)
   ══════════════════════════════════════════════ */
.form-header { flex-shrink:0; background:linear-gradient(165deg, #FFFFFF 0%, #F7FAF7 100%); padding:28px 40px 20px; border-bottom:1px solid var(--borde-suave); }
.form-eyebrow { font-size:11px; font-weight:700; color:#3E8B54; text-transform:uppercase; letter-spacing:.6px; margin:0 0 4px; }
.form-title { font-size:20px; font-weight:700; color:var(--texto); margin:0 0 4px; letter-spacing:-.3px; }
.form-sub { font-size:12.5px; color:var(--texto-sec); margin:0; }
.form-body { padding:22px 40px 8px; }
.form-section { margin-bottom:22px; }
.form-section-label { display:flex; align-items:center; gap:9px; font-size:12px; font-weight:700; color:var(--texto); text-transform:uppercase; letter-spacing:.5px; margin-bottom:14px; padding-bottom:10px; border-bottom:1px solid var(--borde-suave); }
.form-num { width:20px; height:20px; border-radius:7px; background:var(--verde); color:#fff; font-size:10px; font-weight:700; display:flex; align-items:center; justify-content:center; flex-shrink:0; }
.req { color:#c0392b; }
.form-grid { display:grid; grid-template-columns:repeat(4,1fr); gap:13px 16px; }
.fg { display:flex; flex-direction:column; gap:6px; position:relative; }
.fg--span2 { grid-column:span 2; }
.fg--full { grid-column:1 / -1; }
.fg label { font-size:11.5px; font-weight:700; color:var(--texto-sec); }
.err-msg { font-size:11px; color:#c0392b; font-weight:600; margin:0; }
.input, .select {
  height:40px; padding:0 12px; border-radius:9px; border:1px solid var(--borde);
  background:var(--blanco); font-size:13px; color:var(--texto); font-family:inherit; outline:none; width:100%; box-sizing:border-box;
  transition:border-color .16s ease, box-shadow .16s ease;
}
.input:hover, .select:hover { border-color:#D3D8D3; }
.input:focus, .select:focus { border-color:var(--verde-sec); box-shadow:0 0 0 3px rgba(146,168,148,.2); }
.input.is-error { border-color:#e57373; background:#fff8f8; }
.textarea { padding:10px 12px; border-radius:9px; border:1px solid var(--borde); background:var(--blanco); font-size:13px; color:var(--texto); font-family:inherit; outline:none; width:100%; box-sizing:border-box; height:76px; resize:vertical; line-height:1.5; transition:border-color .16s ease, box-shadow .16s ease; }
.textarea:hover { border-color:#D3D8D3; }
.textarea:focus { border-color:var(--verde-sec); box-shadow:0 0 0 3px rgba(146,168,148,.2); }
.form-footer { flex-shrink:0; display:flex; justify-content:flex-end; gap:10px; padding:16px 40px 20px; border-top:1px solid var(--borde-suave); }
.btn-cancel { height:38px; padding:0 16px; border-radius:9px; background:var(--blanco); border:1px solid var(--borde); color:var(--texto-sec); font-size:13px; font-weight:600; cursor:pointer; transition:background-color .16s ease, border-color .16s ease, color .16s ease; }
.btn-cancel:hover { background:#FAFBFA; color:var(--texto); border-color:#D3D8D3; }
.btn-save { display:flex; align-items:center; gap:7px; height:38px; padding:0 17px; border-radius:9px; background:var(--verde); border:none; color:#fff; font-size:13px; font-weight:600; cursor:pointer; box-shadow:0 1px 2px rgba(58,71,60,.12), 0 4px 10px -4px rgba(58,71,60,.35); transition:background-color .16s ease, box-shadow .16s ease; }
.btn-save svg { width:14px; height:14px; }
.btn-save:hover { background:#465747; }

/* Selector de mascota / veterinario */
.pet-select-wrap { position:relative; }
.pet-select-btn {
  width:100%; display:flex; align-items:center; gap:10px; height:40px; padding:0 12px;
  border:1px solid var(--borde); border-radius:9px; background:var(--blanco); cursor:pointer;
  font-family:inherit; font-size:13px; color:var(--texto); text-align:left;
  transition:border-color .16s ease, box-shadow .16s ease; box-sizing:border-box;
}
.pet-select-btn:hover { border-color:#D3D8D3; }
.pet-select-btn:focus { outline:none; border-color:var(--verde-sec); box-shadow:0 0 0 3px rgba(146,168,148,.2); }
.pet-select-btn.is-error { border-color:#e57373; background:#fff8f8; }
.psel-placeholder { color:var(--texto-ter); flex:1; font-size:13px; }
.psel-name { font-weight:700; flex:1; color:var(--texto); }
.psel-species { font-size:12px; color:var(--verde-sec); }
.psel-chevron { margin-left:auto; color:var(--verde-sec); transition:transform .18s; flex-shrink:0; width:14px; height:14px; }
.psel-chevron.open { transform:rotate(180deg); }
.pet-dropdown {
  position:absolute; top:calc(100% + 6px); left:0; right:0; background:var(--blanco);
  border:1px solid var(--borde); border-radius:12px; box-shadow:0 8px 24px rgba(58,71,60,0.12);
  z-index:30; max-height:220px; overflow-y:auto; padding:6px;
}
.dropdown-empty { padding:16px; text-align:center; font-size:13px; color:var(--verde-sec); }
.dropdown-item { display:flex; align-items:center; gap:10px; padding:9px 10px; border-radius:9px; cursor:pointer; transition:background .12s; }
.dropdown-item:hover { background:var(--fondo); }
.dropdown-item.selected { background:#EEF2EE; }
.dropdown-info { display:flex; flex-direction:column; gap:1px; flex:1; min-width:0; }
.dropdown-name { font-size:13px; font-weight:700; color:var(--texto); }
.dropdown-sub { font-size:11px; color:var(--verde-sec); }

.immutable-note {
  display:flex; align-items:flex-start; gap:8px; padding:12px 14px;
  background:#FFFBF3; border-radius:10px; border-left:3px solid var(--amarillo);
  font-size:12px; color:#996C2A; font-weight:600; line-height:1.5; margin-top:4px;
}
.immutable-note svg { flex-shrink:0; margin-top:1px; }

/* Modal confirmar (sin cambios de diseño) */
.modal-header { display:flex; align-items:center; gap:14px; margin-bottom:24px; padding-bottom:20px; border-bottom:1px solid var(--borde-suave); }
.modal-header-info { flex:1; min-width:0; }
.modal-eyebrow { font-size:10.5px; font-weight:700; color:var(--verde-sec); text-transform:uppercase; letter-spacing:0.7px; margin:0 0 4px; }
.modal-title { font-size:19px; font-weight:700; color:var(--texto); letter-spacing:-0.4px; margin:0; }
.modal-section { margin-bottom:24px; }
.confirm-desc { font-size:13.5px; color:var(--texto-sec); line-height:1.6; margin:0; }
.modal-acciones { display:flex; gap:10px; justify-content:flex-end; padding-top:20px; border-top:1px solid var(--borde-suave); }

/* Animaciones modal */
.modal-fade-enter-active, .modal-fade-leave-active { transition:opacity 0.22s ease; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity:0; }

/* ── Responsive ── */
@media (max-width:1100px) { .don-summary { grid-template-columns:repeat(2, 1fr); } }
@media (max-width:900px) {
  .form-grid { grid-template-columns:repeat(2, 1fr); }
  .fg--span2 { grid-column:span 1; }
  .modal-box--uniform { width:94vw; height:88vh; }
}
@media (max-width:640px) {
  .page-header { flex-direction:column; align-items:flex-start; }
  .filtros-row { flex-direction:column; gap:14px; }
  .filtros-row--end { align-items:stretch; }
  .filtro-group { min-width:100%; }
  .filtro-group--search { max-width:none; }
  .don-summary { grid-template-columns:1fr 1fr; }
  .form-grid { grid-template-columns:1fr; }
  .fg--span2, .fg--full { grid-column:1; }
  .don-table th:nth-child(4), .don-table td:nth-child(4), .don-table th:nth-child(5), .don-table td:nth-child(5) { display:none; }
  .modal-box--uniform { width:96vw; height:92vh; border-radius:18px; }
  .hero, .form-header, .form-body, .body, .footer, .form-footer { padding-left:20px; padding-right:20px; }
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