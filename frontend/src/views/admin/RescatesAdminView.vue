<script setup>
import { ref, computed, watch } from 'vue'
import { ubicacionesCR } from '../../data/ubicaciones'
import { usePetsStore } from '../../stores/usePetsStore'

/* ─── Store de mascotas ─────────────────────────────────── */
const petsStore = usePetsStore()

/* ─── Estado principal ──────────────────────────────────── */
const rescates = ref([])

/* ─── UI: modales ────────────────────────────────────────── */
const showForm       = ref(false)
const editMode       = ref(false)
const editingIndex   = ref(null)

const showViewModal  = ref(false)
const viewTarget     = ref(null)

const showCloseModal = ref(false)
const closeTarget    = ref(null)

/* ─── Toast ──────────────────────────────────────────────── */
const toast = ref({ show: false, type: 'success', message: '' })
let toastTimer = null
function showToast(type, message) {
  clearTimeout(toastTimer)
  toast.value = { show: true, type, message }
  toastTimer = setTimeout(() => { toast.value.show = false }, 3500)
}

/* ─── Usuario actual ─────────────────────────────────────── */
const usuarioActual = ref({ nombre: 'Shirley Valverde', rol: 'Admin' })

/* ─── Voluntarios ────────────────────────────────────────── */
const voluntarios = ref(
  JSON.parse(localStorage.getItem('anhelo_usuarios')) || []
)

/* ─── Provincias / casas cuna / rescatistas ──────────────── */
const provinciasDisponibles = computed(() => Object.keys(ubicacionesCR))

const casasCunaDisponibles = computed(() =>
  voluntarios.value.filter(v => {
    const estado = v.solicitudVoluntario?.estado
    const tipo   = v.solicitudVoluntario?.tipo || ''
    return estado === 'Aprobada' && tipo.toLowerCase() === 'casa cuna'
  })
)
const rescatistasDisponibles = computed(() =>
  voluntarios.value.filter(v => {
    const estado = v.solicitudVoluntario?.estado
    const tipo   = v.solicitudVoluntario?.tipo || ''
    return estado === 'Aprobada' && tipo === 'Rescatista'
  })
)

/* ─── Filtros ─────────────────────────────────────────────── */
const filtroSearch = ref('')
const filtroProv   = ref('Todos')
const filtroEstado = ref('Todos')
const ESTADO_TABS  = ['Todos', 'Activo', 'Cerrado']

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
      (r.id || '').toLowerCase().includes(q) ||
      (r.mascota || '').toLowerCase().includes(q) ||
      (r.rescatista || '').toLowerCase().includes(q)
    const coincideProv =
      filtroProv.value === 'Todos' || (r.provincia || '') === filtroProv.value
    const coincideEstado =
      filtroEstado.value === 'Todos' || r.estado === filtroEstado.value
    return coincideSearch && coincideProv && coincideEstado
  })
})

/* ─── Carga / guardado ───────────────────────────────────── */
function cargarRescates() {
  rescates.value = JSON.parse(localStorage.getItem('anhelo_rescates')) || []
}
cargarRescates()
function guardarRescatesLS() {
  localStorage.setItem('anhelo_rescates', JSON.stringify(rescates.value))
}

/* ─── Estadísticas ───────────────────────────────────────── */
const ahora     = new Date()
const mesActual = ahora.getMonth()
const añoActual = ahora.getFullYear()
const stats = computed(() => ({
  esteMes: rescates.value.filter(r => {
    const f = new Date(r.fechaCreacion || r.fechaRescate)
    return f.getMonth() === mesActual && f.getFullYear() === añoActual
  }).length,
  total:   rescates.value.length,
  activos: rescates.value.filter(r => r.estado === 'Activo').length,
  cerrados: rescates.value.filter(r => r.estado === 'Cerrado').length,
}))

/* ─── Formulario (Nuevo / Editar) ────────────────────────── */
const formData = ref(formDataInicial())
const formErrors = ref({})
const imageInputRef = ref(null)

function formDataInicial() {
  return {
    mascota: '', tipoMascota: 'Perro', edad: '', sexo: 'Macho',
    tieneRaza: 'No', raza: '', fechaRescate: '',
    provincia: '', canton: '', distrito: '',
    rescatista: '', casaCuna: '', estado: 'Activo',
    descripcion: '', foto: '',
  }
}

const cantonesDisponibles = computed(() => {
  if (!formData.value.provincia || !ubicacionesCR[formData.value.provincia]) return []
  return Object.keys(ubicacionesCR[formData.value.provincia])
})
const distritosDisponibles = computed(() => {
  if (!formData.value.provincia || !formData.value.canton) return []
  return ubicacionesCR[formData.value.provincia]?.[formData.value.canton] || []
})
watch(() => formData.value.provincia, () => { formData.value.canton = ''; formData.value.distrito = '' })
watch(() => formData.value.canton,    () => { formData.value.distrito = '' })

function handleImageUpload(e) {
  const file = e.target.files?.[0]
  if (!file || !file.type.startsWith('image/')) return
  const reader = new FileReader()
  reader.onload = ev => { formData.value.foto = ev.target.result }
  reader.readAsDataURL(file)
  e.target.value = ''
}
function removeImage() {
  formData.value.foto = ''
}

function clearErr(campo) {
  if (formErrors.value[campo]) {
    const e = { ...formErrors.value }
    delete e[campo]
    formErrors.value = e
  }
}
function validateForm() {
  const errors = {}
  if (!formData.value.mascota.trim())      errors.mascota      = 'El nombre es obligatorio'
  if (!formData.value.edad.trim())         errors.edad         = 'La edad es obligatoria'
  if (!formData.value.fechaRescate)        errors.fechaRescate = 'La fecha de rescate es obligatoria'
  if (!formData.value.provincia)           errors.provincia    = 'Selecciona la provincia'
  if (!formData.value.canton)              errors.canton       = 'Selecciona el cantón'
  if (!formData.value.distrito)            errors.distrito     = 'Selecciona el distrito'
  if (!formData.value.rescatista)          errors.rescatista   = 'Selecciona un rescatista'
  if (!formData.value.descripcion.trim())  errors.descripcion  = 'La descripción es obligatoria'
  if (!formData.value.foto)                errors.foto         = 'Debes subir una fotografía'
  formErrors.value = errors
  return Object.keys(errors).length === 0
}

function obtenerFechaActual() {
  return new Date().toLocaleString('es-CR', {
    year: 'numeric', month: '2-digit', day: '2-digit',
    hour: '2-digit', minute: '2-digit'
  })
}
function ubicacionTexto(f) {
  return [f.provincia, f.canton, f.distrito].filter(Boolean).join(', ')
}

/* ─── Abrir / cerrar formulario ──────────────────────────── */
function openForm() {
  editMode.value     = false
  editingIndex.value = null
  formErrors.value   = {}
  formData.value     = formDataInicial()
  showForm.value     = true
}
function openEdit(rescate) {
  const index = rescates.value.indexOf(rescate)
  editMode.value     = true
  editingIndex.value = index
  formData.value = {
    mascota:      rescate.mascota,
    tipoMascota:  rescate.tipoMascota || 'Perro',
    edad:         rescate.edad,
    sexo:         rescate.sexo,
    tieneRaza:    (rescate.raza && rescate.raza !== 'Sin raza') ? 'Si' : 'No',
    raza:         (rescate.raza && rescate.raza !== 'Sin raza') ? rescate.raza : '',
    fechaRescate: rescate.fechaRescate,
    provincia:    rescate.provincia || '',
    canton:       rescate.canton    || '',
    distrito:     rescate.distrito  || '',
    rescatista:   rescate.rescatista || '',
    casaCuna:     rescate.casaCuna === 'Sin asignar' ? '' : (rescate.casaCuna || ''),
    estado:       rescate.estado,
    descripcion:  rescate.descripcion,
    foto:         rescate.foto || '',
  }
  formErrors.value = {}
  showForm.value   = true
  showViewModal.value = false
}
function closeForm() {
  showForm.value     = false
  editMode.value     = false
  editingIndex.value = null
  formErrors.value   = {}
  formData.value     = formDataInicial()
}

/* ─── Guardar rescate (+ crear/actualizar mascota) ───────── */
function guardarRescate() {
  if (!validateForm()) {
    showToast('error', 'Completa todos los campos obligatorios.')
    return
  }
  const razaFinal = formData.value.tieneRaza === 'Si' ? formData.value.raza : 'Sin raza'

  if (editMode.value && editingIndex.value !== null) {
    const orig = rescates.value[editingIndex.value]
    rescates.value[editingIndex.value] = {
      ...orig,
      mascota:      formData.value.mascota,
      tipoMascota:  formData.value.tipoMascota,
      foto:         formData.value.foto,
      edad:         formData.value.edad,
      sexo:         formData.value.sexo,
      raza:         razaFinal,
      fechaRescate: formData.value.fechaRescate,
      provincia:    formData.value.provincia,
      canton:       formData.value.canton,
      distrito:     formData.value.distrito,
      ubicacion:    ubicacionTexto(formData.value),
      descripcion:  formData.value.descripcion,
      casaCuna:     formData.value.casaCuna || 'Sin asignar',
      rescatista:   formData.value.rescatista,
      estado:       formData.value.estado,
    }
    if (orig.mascotaId) {
      const petIndex = petsStore.pets.findIndex(p => p.id === orig.mascotaId)
      if (petIndex !== -1) {
        petsStore.pets[petIndex] = {
          ...petsStore.pets[petIndex],
          name:   formData.value.mascota,
          type:   formData.value.tipoMascota,
          image:  formData.value.foto,
          age:    formData.value.edad,
          gender: formData.value.sexo,
          breed:  razaFinal !== 'Sin raza' ? razaFinal : '',
        }
        petsStore.savePets?.()
      }
    }
    showToast('success', 'Rescate actualizado correctamente.')
  } else {
    const nuevaMascota = {
      id: `pet-${Date.now()}`,
      name: formData.value.mascota,
      type: formData.value.tipoMascota,
      images: [{ preview: formData.value.foto }],
      image: formData.value.foto,
      age: formData.value.edad,
      gender: formData.value.sexo,
      breed: razaFinal !== 'Sin raza' ? razaFinal : '',
      status: 'En rescate',
      description: formData.value.descripcion,
      location: `${formData.value.provincia}, ${formData.value.canton}`,
      createdAt: obtenerFechaActual(),
    }
    if (typeof petsStore.addPet === 'function') {
      petsStore.addPet(nuevaMascota)
    } else if (Array.isArray(petsStore.pets)) {
      petsStore.pets.unshift(nuevaMascota)
      if (typeof petsStore.savePets === 'function') petsStore.savePets()
    }
    const id = `R-${String(rescates.value.length + 1).padStart(3, '0')}`
    rescates.value.unshift({
      id,
      mascotaId:     nuevaMascota.id,
      mascota:       formData.value.mascota,
      tipoMascota:   formData.value.tipoMascota,
      foto:          formData.value.foto,
      edad:          formData.value.edad,
      sexo:          formData.value.sexo,
      raza:          razaFinal,
      fechaRescate:  formData.value.fechaRescate,
      fechaCreacion: obtenerFechaActual(),
      creadoPor:     usuarioActual.value.nombre,
      provincia:     formData.value.provincia,
      canton:        formData.value.canton,
      distrito:      formData.value.distrito,
      ubicacion:     ubicacionTexto(formData.value),
      descripcion:   formData.value.descripcion,
      casaCuna:      formData.value.casaCuna || 'Sin asignar',
      rescatista:    formData.value.rescatista,
      estado:        formData.value.estado,
    })
    showToast('success', 'Rescate registrado y mascota creada correctamente.')
  }

  guardarRescatesLS()
  closeForm()
}

/* ─── Ver rescate ─────────────────────────────────────────── */
function openView(rescate) {
  viewTarget.value    = rescate
  showViewModal.value = true
}

/* ─── Cerrar rescate ──────────────────────────────────────── */
function openCloseModal(rescate) {
  closeTarget.value    = rescate
  showCloseModal.value = true
}
function confirmClose() {
  if (!closeTarget.value) return
  const index = rescates.value.indexOf(closeTarget.value)
  if (index !== -1) rescates.value[index].estado = 'Cerrado'
  guardarRescatesLS()
  showCloseModal.value = false
  closeTarget.value    = null
  showViewModal.value  = false
  showToast('success', 'Rescate cerrado correctamente.')
}

/* ─── Helpers de vista ────────────────────────────────────── */
function estadoBadgeClass(est) {
  return { 'Activo': 'badge-aprobada', 'Cerrado': 'badge-inactiva' }[est] || 'badge-inactiva'
}
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
         MODAL 1/3 — NUEVO / EDITAR RESCATE
         Mismo ancho y alto que Ver / Cerrar (.modal-box--uniform)
    ══════════════════════════════════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showForm" class="modal-overlay" @click.self="closeForm">
          <div class="modal-box modal-box--uniform">
            <button class="close-btn" @click="closeForm">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>

            <div class="form-header">
              <p class="form-eyebrow">{{ editMode ? 'Editar registro' : 'Nuevo registro' }}</p>
              <h2 class="form-title">{{ editMode ? 'Editar rescate' : 'Nuevo rescate' }}</h2>
              <p class="form-sub">{{ editMode ? 'Modifica los datos del rescate' : 'Registra un animal rescatado y su expediente' }}</p>
            </div>

            <div class="uniform-scroll">
              <div class="form-body">

                <!-- Sección 1: Datos de la mascota -->
                <div class="form-section">
                  <div class="form-section-label"><span class="form-num">1</span> Datos de la mascota</div>
                  <div class="form-grid">
                    <div class="fg">
                      <label>Nombre <span class="req">*</span></label>
                      <input v-model="formData.mascota" placeholder="Ej. Luna" class="input" :class="{ 'is-error': formErrors.mascota }" @input="clearErr('mascota')" />
                      <p v-if="formErrors.mascota" class="err-msg">{{ formErrors.mascota }}</p>
                    </div>
                    <div class="fg">
                      <label>Tipo <span class="req">*</span></label>
                      <select v-model="formData.tipoMascota" class="select">
                        <option>Perro</option>
                        <option>Gato</option>
                      </select>
                    </div>
                    <div class="fg">
                      <label>Edad <span class="req">*</span></label>
                      <input v-model="formData.edad" placeholder="Ej. 2 años" class="input" :class="{ 'is-error': formErrors.edad }" @input="clearErr('edad')" />
                      <p v-if="formErrors.edad" class="err-msg">{{ formErrors.edad }}</p>
                    </div>
                    <div class="fg">
                      <label>Sexo</label>
                      <select v-model="formData.sexo" class="select">
                        <option>Macho</option>
                        <option>Hembra</option>
                      </select>
                    </div>
                    <div class="fg">
                      <label>¿Tiene raza?</label>
                      <select v-model="formData.tieneRaza" class="select">
                        <option value="No">No</option>
                        <option value="Si">Sí</option>
                      </select>
                    </div>
                    <div class="fg" v-if="formData.tieneRaza === 'Si'">
                      <label>Raza</label>
                      <input v-model="formData.raza" placeholder="Ej. Labrador" class="input" />
                    </div>
                    <div class="fg">
                      <label>Fecha de rescate <span class="req">*</span></label>
                      <input type="date" v-model="formData.fechaRescate" class="input" :class="{ 'is-error': formErrors.fechaRescate }" @input="clearErr('fechaRescate')" />
                      <p v-if="formErrors.fechaRescate" class="err-msg">{{ formErrors.fechaRescate }}</p>
                    </div>
                  </div>
                </div>

                <!-- Sección 2: Ubicación del rescate -->
                <div class="form-section">
                  <div class="form-section-label"><span class="form-num">2</span> Ubicación del rescate</div>
                  <div class="form-grid">
                    <div class="fg">
                      <label>Provincia <span class="req">*</span></label>
                      <select v-model="formData.provincia" class="select" :class="{ 'is-error': formErrors.provincia }" @change="clearErr('provincia')">
                        <option value="">Seleccione</option>
                        <option v-for="p in provinciasDisponibles" :key="p" :value="p">{{ p }}</option>
                      </select>
                      <p v-if="formErrors.provincia" class="err-msg">{{ formErrors.provincia }}</p>
                    </div>
                    <div class="fg">
                      <label>Cantón <span class="req">*</span></label>
                      <select v-model="formData.canton" class="select" :disabled="!formData.provincia" :class="{ 'is-error': formErrors.canton }" @change="clearErr('canton')">
                        <option value="">Seleccione</option>
                        <option v-for="c in cantonesDisponibles" :key="c" :value="c">{{ c }}</option>
                      </select>
                      <p v-if="formErrors.canton" class="err-msg">{{ formErrors.canton }}</p>
                    </div>
                    <div class="fg">
                      <label>Distrito <span class="req">*</span></label>
                      <select v-model="formData.distrito" class="select" :disabled="!formData.canton" :class="{ 'is-error': formErrors.distrito }" @change="clearErr('distrito')">
                        <option value="">Seleccione</option>
                        <option v-for="d in distritosDisponibles" :key="d" :value="d">{{ d }}</option>
                      </select>
                      <p v-if="formErrors.distrito" class="err-msg">{{ formErrors.distrito }}</p>
                    </div>
                  </div>
                </div>

                <!-- Sección 3: Asignaciones -->
                <div class="form-section">
                  <div class="form-section-label"><span class="form-num">3</span> Asignaciones</div>
                  <div class="form-grid">
                    <div class="fg fg--span2">
                      <label>Rescatista <span class="req">*</span></label>
                      <select v-model="formData.rescatista" class="select" :class="{ 'is-error': formErrors.rescatista }" @change="clearErr('rescatista')">
                        <option value="">Seleccione un rescatista</option>
                        <option v-for="r in rescatistasDisponibles" :key="r.id" :value="r.solicitudVoluntario?.nombre || r.nombre">{{ r.solicitudVoluntario?.nombre || r.nombre }}</option>
                      </select>
                      <p v-if="formErrors.rescatista" class="err-msg">{{ formErrors.rescatista }}</p>
                    </div>
                    <div class="fg fg--span2">
                      <label>Casa cuna</label>
                      <select v-model="formData.casaCuna" class="select">
                        <option value="">Sin asignar</option>
                        <option v-for="c in casasCunaDisponibles" :key="c.id" :value="c.solicitudVoluntario?.nombre || c.nombre">{{ c.solicitudVoluntario?.nombre || c.nombre }}</option>
                      </select>
                    </div>
                    <div class="fg">
                      <label>Estado</label>
                      <select v-model="formData.estado" class="select">
                        <option>Activo</option>
                        <option>Cerrado</option>
                      </select>
                    </div>
                  </div>
                </div>

                <!-- Sección 4: Descripción -->
                <div class="form-section">
                  <div class="form-section-label"><span class="form-num">4</span> Descripción del rescate</div>
                  <div class="form-grid">
                    <div class="fg fg--full">
                      <textarea v-model="formData.descripcion" placeholder="Describe las circunstancias del rescate, condición del animal, observaciones importantes..." class="textarea" :class="{ 'is-error': formErrors.descripcion }" @input="clearErr('descripcion')"></textarea>
                      <p v-if="formErrors.descripcion" class="err-msg">{{ formErrors.descripcion }}</p>
                    </div>
                  </div>
                </div>

                <!-- Sección 5: Fotografía -->
                <div class="form-section">
                  <div class="form-section-label"><span class="form-num">5</span> Fotografía <span class="req">*</span></div>
                  <div v-if="formData.foto" class="image-previews">
                    <div class="image-preview-item">
                      <img :src="formData.foto" alt="foto rescate" />
                      <button class="remove-image-btn" @click="removeImage" title="Eliminar">×</button>
                      <span class="main-photo-label">Principal</span>
                    </div>
                  </div>
                  <div v-else class="upload-zone" :class="{ 'is-error': formErrors.foto }" @click="imageInputRef.click()">
                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="17 8 12 3 7 8"/><line x1="12" y1="3" x2="12" y2="15"/></svg>
                    <p class="upload-title">Subir fotografía del animal</p>
                    <p class="upload-sub">Haz clic para seleccionar · JPG, PNG, WebP</p>
                  </div>
                  <p v-if="formErrors.foto" class="err-msg" style="margin-top:8px">{{ formErrors.foto }}</p>
                  <input ref="imageInputRef" type="file" accept="image/*" style="display:none" @change="handleImageUpload" />
                </div>

              </div>
            </div>

            <div class="form-footer">
              <button class="btn-cancel" @click="closeForm">Cancelar</button>
              <button class="btn-save" @click="guardarRescate">
                <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
                <span>{{ editMode ? 'Guardar cambios' : 'Registrar rescate' }}</span>
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
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M10.5 20.5 21 10a2.12 2.12 0 0 0-3-3L7.5 17.5"/><path d="M3 21l3-3"/><circle cx="6.5" cy="6.5" r="2.5"/></svg>
          </div>
          <div>
            <h1 class="admin-page-title">Rescates</h1>
            <p class="admin-page-sub">Registro y seguimiento de animales rescatados</p>
          </div>
        </div>
        <button class="btn btn--primary" @click="openForm">
          <svg class="btn-ico" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
          <span>Nuevo rescate</span>
        </button>
      </header>

      <div class="don-summary">
        <div class="don-card mes-card">
          <div class="don-icon mes-icon">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="4" width="18" height="18" rx="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>
          </div>
          <strong class="don-value">{{ stats.esteMes }}</strong>
          <span class="don-label">Este mes</span>
          <span class="don-desc">Rescates registrados</span>
        </div>
        <div class="don-card total-card">
          <div class="don-icon total-icon">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="7" height="7" rx="1.5"/><rect x="14" y="3" width="7" height="7" rx="1.5"/><rect x="3" y="14" width="7" height="7" rx="1.5"/><rect x="14" y="14" width="7" height="7" rx="1.5"/></svg>
          </div>
          <strong class="don-value">{{ stats.total }}</strong>
          <span class="don-label">Total</span>
          <span class="don-desc">En el sistema</span>
        </div>
        <div class="don-card activo-card">
          <div class="don-icon activo-icon">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="9"/><polyline points="12 7 12 12 16 14"/></svg>
          </div>
          <strong class="don-value">{{ stats.activos }}</strong>
          <span class="don-label">Activos</span>
          <span class="don-desc">En seguimiento</span>
        </div>
        <div class="don-card cerrado-card">
          <div class="don-icon cerrado-icon">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/></svg>
          </div>
          <strong class="don-value">{{ stats.cerrados }}</strong>
          <span class="don-label">Cerrados</span>
          <span class="don-desc">Casos finalizados</span>
        </div>
      </div>

      <div class="filtros-panel">
        <div class="filtros-row">
          <div class="filtro-group filtro-group--tabs">
            <label class="filtro-label">Estado</label>
            <div class="tabs-wrap">
              <button v-for="e in ESTADO_TABS" :key="e" class="tab-btn" :class="{ active: filtroEstado === e }" @click="filtroEstado = e">{{ e }}</button>
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
        </div>
        <div class="filtros-divider"></div>
        <div class="filtros-row filtros-row--end">
          <div class="filtro-group filtro-group--search">
            <label class="filtro-label">Buscar</label>
            <div class="filtro-input-wrap">
              <span class="filtro-icon filtro-icon--left">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
              </span>
              <input v-model="filtroSearch" placeholder="ID, mascota o rescatista..." class="filtro-input filtro-input--icon-left" />
            </div>
          </div>
          <div class="filtro-group filtro-group--btn">
            <button class="btn btn--ghost" :class="{ 'btn--ghost-active': hayFiltros }" @click="limpiarFiltros">Limpiar filtros</button>
          </div>
        </div>
      </div>

      <div v-if="rescatesFiltrados.length === 0" class="empty-state">
        <svg xmlns="http://www.w3.org/2000/svg" width="36" height="36" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M10.5 20.5 21 10a2.12 2.12 0 0 0-3-3L7.5 17.5"/><path d="M3 21l3-3"/><circle cx="6.5" cy="6.5" r="2.5"/></svg>
        <p class="empty-title">{{ hayFiltros ? 'Sin resultados para los filtros aplicados' : 'No hay rescates registrados' }}</p>
        <p class="empty-sub">{{ hayFiltros ? 'Ajusta los filtros para ver más resultados.' : 'Registra el primer rescate con el botón superior.' }}</p>
      </div>

      <div v-else class="table-wrapper">
        <div class="table-scroll">
          <table class="don-table">
            <thead>
              <tr>
                <th>ID</th><th>Foto</th><th>Mascota</th><th>Rescatista</th><th>Estado</th><th>Ubicación</th><th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="r in rescatesFiltrados" :key="r.id" class="don-row" :class="{ 'row-inactive': r.estado === 'Cerrado' }">
                <td><span class="id-pill">{{ r.id }}</span></td>
                <td>
                  <div class="pet-avatar">
                    <img v-if="r.foto" :src="r.foto" class="pet-avatar-img" :alt="r.mascota" />
                    <span v-else class="pet-avatar-ini">{{ iniciales(r.mascota) }}</span>
                  </div>
                </td>
                <td>
                  <span class="donor-name">{{ r.mascota }}</span>
                  <span class="donor-mail">{{ r.edad }} · {{ r.sexo }}</span>
                </td>
                <td><span class="fecha-text">{{ r.rescatista || '—' }}</span></td>
                <td><span class="estado-badge" :class="estadoBadgeClass(r.estado)">{{ r.estado }}</span></td>
                <td><span class="fecha-text">{{ r.ubicacion || '—' }}</span></td>
                <td>
                  <div class="action-group">
                    <button class="icon-only icon-only--ver" @click="openView(r)" data-tooltip="Ver rescate">
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
                    </button>
                    <button class="icon-only icon-only--editar" @click="openEdit(r)" data-tooltip="Editar">
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.12 2.12 0 0 1 3 3L12 15l-4 1 1-4z"/></svg>
                    </button>
                    <button
                      class="icon-only icon-only--inactivar"
                      :disabled="r.estado === 'Cerrado'"
                      @click="openCloseModal(r)"
                      data-tooltip="Cerrar rescate"
                    >
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
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
    </div>

    <!-- ══════════════════════════════════════
         MODAL 2/3 — VER RESCATE (expediente)
         Mismo ancho y alto que Editar / Cerrar (.modal-box--uniform)
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
                <img v-if="viewTarget.foto" :src="viewTarget.foto" :alt="viewTarget.mascota" />
                <span v-else class="hero-photo-ini">{{ iniciales(viewTarget.mascota) }}</span>
              </div>
              <div class="hero-info">
                <h2 class="hero-name">{{ viewTarget.mascota }}</h2>
                <div class="hero-row">
                  <div class="hero-item">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/></svg>
                    {{ viewTarget.raza || 'Sin raza' }} · {{ viewTarget.tipoMascota || '—' }}
                  </div>
                </div>
                <div class="hero-row">
                  <div class="hero-item">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="9"/><polyline points="12 7 12 12 16 14"/></svg>
                    {{ viewTarget.edad }}
                  </div>
                  <div class="hero-item">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="9"/></svg>
                    {{ viewTarget.sexo }}
                  </div>
                </div>
              </div>
              <span class="estado-badge badge-status-hero" :class="estadoBadgeClass(viewTarget.estado)">{{ viewTarget.estado }}</span>
            </div>

            <div class="uniform-scroll">
              <div class="body">
                <div class="grid-2col">
                  <div>
                    <div class="block">
                      <h4 class="block-title">
                        <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M10.5 20.5 21 10a2.12 2.12 0 0 0-3-3L7.5 17.5"/><path d="M3 21l3-3"/><circle cx="6.5" cy="6.5" r="2.5"/></svg>
                        Información del rescate
                      </h4>
                      <div class="fields-row">
                        <div class="field-col"><span class="field-label-row">ID</span><span class="field-value">{{ viewTarget.id }}</span></div>
                        <div class="field-col"><span class="field-label-row">Fecha de rescate</span><span class="field-value">{{ viewTarget.fechaRescate || '—' }}</span></div>
                        <div class="field-col"><span class="field-label-row">Fecha de registro</span><span class="field-value">{{ viewTarget.fechaCreacion || '—' }}</span></div>
                        <div class="field-col"><span class="field-label-row">Registrado por</span><span class="field-value">{{ viewTarget.creadoPor || '—' }}</span></div>
                      </div>
                    </div>

                    <div class="block">
                      <h4 class="block-title">
                        <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/></svg>
                        Ubicación
                      </h4>
                      <div class="tint-box">
                        <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="9"/></svg>
                        <span>{{ viewTarget.ubicacion || [viewTarget.provincia, viewTarget.canton, viewTarget.distrito].filter(Boolean).join(', ') || '—' }}</span>
                      </div>
                    </div>

                    <div class="block" v-if="viewTarget.descripcion">
                      <h4 class="block-title">
                        <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
                        Descripción del rescate
                      </h4>
                      <div class="tint-box tint-box--desc">
                        <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
                        <span>{{ viewTarget.descripcion }}</span>
                      </div>
                    </div>
                  </div>

                  <div class="block" style="margin-bottom:0;">
                    <h4 class="block-title">
                      <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="7" width="20" height="14" rx="2"/><path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"/></svg>
                      Asignaciones
                    </h4>
                    <div class="list-col">
                      <div class="list-item">
                        <div class="list-icon"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/></svg></div>
                        <div class="list-text"><span class="list-label">Rescatista</span><span class="list-value">{{ viewTarget.rescatista || '—' }}</span></div>
                      </div>
                      <div class="list-item">
                        <div class="list-icon"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/></svg></div>
                        <div class="list-text"><span class="list-label">Casa cuna</span><span class="list-value">{{ viewTarget.casaCuna || 'Sin asignar' }}</span></div>
                      </div>
                      <div class="list-item">
                        <div class="list-icon"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/></svg></div>
                        <div class="list-text"><span class="list-label">Estado</span><span class="list-value">{{ viewTarget.estado }}</span></div>
                      </div>
                      <div class="list-item">
                        <div class="list-icon"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="9"/><polyline points="12 7 12 12 16 14"/></svg></div>
                        <div class="list-text"><span class="list-label">Edad</span><span class="list-value">{{ viewTarget.edad }}</span></div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <div class="footer">
              <button v-if="viewTarget.estado === 'Activo'" class="btn-danger" @click="openCloseModal(viewTarget)">Cerrar rescate</button>
              <button class="btn-ghost-red" @click="showViewModal = false">
                <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><line x1="9" y1="9" x2="15" y2="15"/><line x1="15" y1="9" x2="9" y2="15"/></svg>
                Cerrar expediente
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- ══════════════════════════════════════
         MODAL 3/3 — CERRAR RESCATE
         Mismo ancho y alto que Ver / Editar (.modal-box--uniform)
    ══════════════════════════════════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showCloseModal" class="modal-overlay" @click.self="showCloseModal = false">
          <div class="modal-box modal-box--uniform">
            <button class="close-btn" @click="showCloseModal = false">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>

            <div class="confirm-header">
              <div class="confirm-icon">
                <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
              </div>
              <div>
                <p class="confirm-eyebrow">Cerrar rescate</p>
                <h2 class="confirm-title">{{ closeTarget?.mascota }}</h2>
              </div>
            </div>

            <div class="uniform-scroll">
              <div class="confirm-body">
                <div class="warn-box">
                  <p><strong>No se eliminará</strong> del sistema. Se conservará todo su historial, expediente y registros asociados.</p>
                  <p>El rescate pasará a estado <strong>Cerrado</strong> y dejará de aparecer como caso activo en seguimiento.</p>
                </div>
              </div>
            </div>

            <div class="confirm-footer">
              <button class="btn-cancel" @click="showCloseModal = false">Cancelar</button>
              <button class="btn-danger" @click="confirmClose">Confirmar cierre</button>
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
  --btn-height:      48px;
  --btn-radius:      8px;
  --btn-pad-x:       20px;
  --btn-icon-size:   20px;
  --btn-icon-gap:    10px;
  --btn-font-size:   15px;
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
.btn-ico, .btn :deep(svg) { width:var(--btn-icon-size); height:var(--btn-icon-size); flex-shrink:0; }
.btn:active:not(:disabled) { transform:translateY(1px); }
.btn:focus-visible { outline:none; box-shadow:0 0 0 3px rgba(58,71,60,.22); }
.btn--primary { background:var(--verde); color:#fff; }
.btn--primary:hover:not(:disabled) { background:#485749; }
.btn--ghost { background:var(--blanco); color:var(--texto-sec); border-color:var(--borde); }
.btn--ghost:hover:not(:disabled) { background:#FAFBFA; color:var(--texto); border-color:#D3D8D3; }
.btn--ghost-active { border-color:var(--verde-sec); color:var(--verde); }
.btn--ghost-active:hover:not(:disabled) { background:#F3F6F3; color:var(--verde); border-color:var(--verde-sec); }

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
.mes-icon     { background:#FDF6E8; border-color:#F2E1B8; color:#A97A0C; }
.total-icon   { background:#F2F3F2; border-color:#DFE2DF; color:#616861; }
.activo-icon  { background:#EEF1FB; border-color:#CBD5F2; color:#4F73B8; }
.cerrado-icon { background:#EDF6EF; border-color:#C9E4CE; color:#2E7D45; }
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
.filtro-select { appearance:none; -webkit-appearance:none; cursor:pointer; padding-right:34px; background-image:var(--select-arrow); background-repeat:no-repeat; background-position:right 12px center; }
.filtro-select:focus { background-image:var(--select-arrow-focus); }
.filtro-icon { position:absolute; display:flex; align-items:center; color:var(--texto-sec); }
.filtro-icon--left { left:12px; }
.filtro-icon--right { right:12px; }
.filtro-icon--no-events { pointer-events:none; }

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
.row-inactive { opacity:0.5; }
.pet-avatar { width:38px; height:38px; border-radius:50%; overflow:hidden; flex-shrink:0; background:#F1F5F1; display:flex; align-items:center; justify-content:center; border:1px solid var(--borde); }
.pet-avatar-img { width:100%; height:100%; object-fit:cover; display:block; }
.pet-avatar-ini { font-size:14px; font-weight:700; color:#4E6E51; text-transform:uppercase; line-height:1; }
.id-pill { font-size:11px; font-family:ui-monospace, Menlo, Consolas, monospace; background:var(--fondo); border:1px solid var(--borde); padding:3px 9px; border-radius:6px; color:var(--texto); font-weight:700; white-space:nowrap; }
.donor-name { display:block; font-size:12.5px; font-weight:700; color:var(--texto); line-height:1.3; }
.donor-mail { display:block; font-size:11px; color:var(--texto-sec); margin-top:2px; }
.fecha-text { font-size:12.5px; color:var(--texto-sec); }
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
.icon-only--editar { color:#2E7D45; border-color:#CFE8D6; }
.icon-only--editar:hover { background:#F3FAF5; border-color:#2E7D45; }
.icon-only--inactivar { color:#C0392B; border-color:#F0CFC9; }
.icon-only--inactivar:hover { background:#FDF4F3; border-color:#C0392B; }
.icon-only:disabled { opacity:0.35; cursor:not-allowed; }
.icon-only:disabled:hover { background:var(--blanco); border-color:var(--borde); }
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
   .modal-box--uniform — Ver / Editar / Cerrar
   MISMO ANCHO Y MISMO ALTO EXACTOS para las 3 vistas.
   ══════════════════════════════════════════════ */
.modal-box--uniform {
  width:880px;
  max-width:92vw;
  height:660px;
  max-height:90vh;
  display:flex;
  flex-direction:column;
  overflow:hidden;
}
.uniform-scroll { flex:1; min-height:0; overflow-y:auto; }
.close-btn {
  position:absolute; top:18px; right:18px; z-index:6;
  width:30px; height:30px; border-radius:8px; background:transparent; border:none;
  color:#8B928A; display:flex; align-items:center; justify-content:center; cursor:pointer;
}
.close-btn svg { width:18px; height:18px; }
.close-btn--hero { color:#fff; background:rgba(255,255,255,.18); }
.close-btn--hero:hover { background:rgba(255,255,255,.32); }

/* ── HERO (Ver rescate) ── */
.hero {
  flex-shrink:0;
  background:#F4FAF5;
  padding:26px 36px 22px;
  display:flex; align-items:flex-start; gap:18px;
}
.hero-photo { width:74px; height:74px; border-radius:50%; flex-shrink:0; overflow:hidden; background:#DDF0E1; display:flex; align-items:center; justify-content:center; }
.hero-photo img { width:100%; height:100%; object-fit:cover; display:block; }
.hero-photo-ini { font-size:26px; font-weight:800; color:#3E8B54; }
.hero-info { flex:1; min-width:0; padding-top:2px; }
.hero-name { font-size:23px; font-weight:800; color:#20261F; margin:0 0 8px; letter-spacing:-.3px; }
.hero-row { display:flex; align-items:center; gap:18px; flex-wrap:wrap; font-size:13px; color:#4B534A; font-weight:500; margin-bottom:6px; }
.hero-row:last-child { margin-bottom:0; }
.hero-item { display:flex; align-items:center; gap:6px; }
.hero-item svg { color:#6B756A; flex-shrink:0; }
.badge-status-hero { margin-left:auto; flex-shrink:0; padding:7px 14px !important; font-size:11.5px !important; }

/* ── BODY (Ver rescate) ── */
.body { padding:22px 36px 8px; }
.grid-2col { display:grid; grid-template-columns:1.7fr 1fr; gap:16px; align-items:start; }
.block { background:var(--blanco); border:1px solid var(--borde); border-radius:12px; padding:18px 20px; margin-bottom:16px; }
.block:last-child { margin-bottom:0; }
.block-title { display:flex; align-items:center; gap:8px; font-size:13px; font-weight:700; color:var(--texto); margin:0 0 13px; padding-bottom:12px; border-bottom:1px solid var(--borde); }
.block-title svg { color:#5C8F62; flex-shrink:0; }
.fields-row { display:grid; grid-template-columns:repeat(auto-fit, minmax(120px, 1fr)); gap:16px; }
.field-col { display:flex; flex-direction:column; gap:6px; }
.field-label-row { font-size:10.5px; font-weight:700; color:var(--texto-ter); text-transform:uppercase; letter-spacing:.4px; }
.field-value { font-size:15px; font-weight:700; color:var(--texto); }
.tint-box { background:#F4FAF5; border-radius:10px; padding:13px 15px; display:flex; align-items:center; gap:9px; }
.tint-box svg { color:#3E8B54; flex-shrink:0; }
.tint-box span { font-size:13.5px; font-weight:700; color:var(--texto); }
.tint-box--desc { align-items:flex-start; }
.tint-box--desc span { font-weight:500; line-height:1.6; color:#3D453B; }
.list-col { display:flex; flex-direction:column; gap:10px; }
.list-item { border:1px solid var(--borde); border-radius:10px; padding:12px 14px; display:flex; align-items:center; gap:12px; }
.list-icon { width:34px; height:34px; border-radius:8px; flex-shrink:0; background:#DDF0E1; color:#3E8B54; display:flex; align-items:center; justify-content:center; }
.list-text { display:flex; flex-direction:column; gap:2px; min-width:0; }
.list-label { font-size:10px; font-weight:700; color:var(--texto-ter); text-transform:uppercase; letter-spacing:.4px; }
.list-value { font-size:13px; font-weight:700; color:var(--texto); white-space:nowrap; overflow:hidden; text-overflow:ellipsis; }

/* ── FOOTER (Ver rescate) ── */
.footer { flex-shrink:0; display:flex; justify-content:flex-end; gap:10px; padding:16px 36px 20px; border-top:1px solid var(--borde-suave); }
.btn-ghost-red { display:flex; align-items:center; gap:8px; height:44px; padding:0 20px; border-radius:8px; background:var(--blanco); border:1px solid #E8B9B2; color:var(--rojo); font-size:13.5px; font-weight:600; cursor:pointer; transition:background-color .16s ease; }
.btn-ghost-red:hover { background:#FDF4F3; }

/* ══════════════════════════════════════════════
   FORMULARIO (Nuevo / Editar rescate)
   ══════════════════════════════════════════════ */
.form-header { flex-shrink:0; padding:26px 36px 18px; border-bottom:1px solid var(--borde); }
.form-eyebrow { font-size:11px; font-weight:700; color:#3E8B54; text-transform:uppercase; letter-spacing:.6px; margin:0 0 4px; }
.form-title { font-size:20px; font-weight:800; color:var(--texto); margin:0 0 4px; }
.form-sub { font-size:12.5px; color:var(--texto-sec); margin:0; }
.form-body { padding:20px 36px 8px; }
.form-section { margin-bottom:22px; }
.form-section-label { display:flex; align-items:center; gap:9px; font-size:12px; font-weight:700; color:var(--texto); text-transform:uppercase; letter-spacing:.5px; margin-bottom:12px; padding-bottom:8px; border-bottom:1px solid var(--borde-suave); }
.form-num { width:20px; height:20px; border-radius:6px; background:var(--verde); color:#fff; font-size:10px; font-weight:700; display:flex; align-items:center; justify-content:center; flex-shrink:0; }
.req { color:#c0392b; }
.form-grid { display:grid; grid-template-columns:repeat(4,1fr); gap:13px 16px; }
.fg { display:flex; flex-direction:column; gap:6px; }
.fg--span2 { grid-column:span 2; }
.fg--full { grid-column:1 / -1; }
.fg label { font-size:11.5px; font-weight:700; color:var(--texto-sec); }
.err-msg { font-size:11px; color:#c0392b; font-weight:600; margin:0; }
.input, .select {
  height:40px; padding:0 12px; border-radius:8px; border:1px solid var(--borde);
  background:var(--blanco); font-size:13px; color:var(--texto); font-family:inherit; outline:none; width:100%; box-sizing:border-box;
  transition:border-color .16s ease, box-shadow .16s ease;
}
.select {
  padding-right:32px;
  background-image:var(--select-arrow);
  background-repeat:no-repeat;
  background-position:right 12px center;
  appearance:none;
  -webkit-appearance:none;
  -moz-appearance:none;
}
.select:disabled { background-color:#F4F6F4; color:#B4BCB5; cursor:not-allowed; }
.input:hover, .select:hover { border-color:#D3D8D3; }
.input:focus, .select:focus { border-color:var(--verde-sec); box-shadow:0 0 0 3px rgba(146,168,148,.2); }
.select:focus { background-image:var(--select-arrow-focus); }
.input.is-error, .select.is-error { border-color:#e57373; background-color:#fff8f8; }
.textarea { padding:10px 12px; border-radius:8px; border:1px solid var(--borde); background:var(--blanco); font-size:13px; color:var(--texto); font-family:inherit; outline:none; width:100%; box-sizing:border-box; height:76px; resize:vertical; line-height:1.5; transition:border-color .16s ease, box-shadow .16s ease; }
.textarea:hover { border-color:#D3D8D3; }
.textarea:focus { border-color:var(--verde-sec); box-shadow:0 0 0 3px rgba(146,168,148,.2); }
.textarea.is-error { border-color:#e57373; background:#fff8f8; }
.upload-zone { border:1.5px dashed #D0D9D1; border-radius:8px; padding:22px; text-align:center; cursor:pointer; background:#FAFCFA; transition:border-color .16s ease, background-color .16s ease; }
.upload-zone:hover { border-color:var(--verde-sec); background:#F2F7F2; }
.upload-zone.is-error { border-color:#e57373; background:#fff8f8; }
.upload-zone svg { color:var(--verde-sec); margin-bottom:8px; }
.upload-title { font-size:13px; font-weight:700; color:var(--texto); margin:0 0 3px; }
.upload-sub { font-size:12px; color:var(--texto-ter); margin:0; }
.image-previews { display:flex; flex-wrap:wrap; gap:10px; }
.image-preview-item { position:relative; width:80px; height:80px; border-radius:8px; overflow:hidden; border:1px solid var(--borde); }
.image-preview-item img { width:100%; height:100%; object-fit:cover; display:block; }
.remove-image-btn { position:absolute; top:4px; right:4px; width:20px; height:20px; border-radius:50%; background:rgba(0,0,0,0.5); color:white; font-size:12px; border:none; cursor:pointer; display:flex; align-items:center; justify-content:center; }
.main-photo-label { position:absolute; bottom:0; left:0; right:0; background:rgba(45,58,46,.75); color:white; font-size:9px; font-weight:700; text-align:center; padding:3px 0; }
.form-footer { flex-shrink:0; display:flex; justify-content:flex-end; gap:10px; padding:16px 36px 20px; border-top:1px solid var(--borde-suave); }
.btn-cancel { height:44px; padding:0 20px; border-radius:8px; background:var(--blanco); border:1px solid var(--borde); color:var(--texto-sec); font-size:13.5px; font-weight:600; cursor:pointer; transition:background-color .16s ease, border-color .16s ease, color .16s ease; }
.btn-cancel:hover { background:#FAFBFA; color:var(--texto); border-color:#D3D8D3; }
.btn-save { display:flex; align-items:center; gap:8px; height:44px; padding:0 22px; border-radius:8px; background:var(--verde); border:none; color:#fff; font-size:13.5px; font-weight:600; cursor:pointer; transition:background-color .16s ease; }
.btn-save:hover { background:#485749; }

/* ══════════════════════════════════════════════
   CONFIRMAR CIERRE
   ══════════════════════════════════════════════ */
.confirm-header { flex-shrink:0; padding:28px 32px 18px; display:flex; align-items:center; gap:15px; border-bottom:1px solid var(--borde); }
.confirm-icon { width:48px; height:48px; border-radius:10px; flex-shrink:0; background:var(--rojo-bg); color:var(--rojo); display:flex; align-items:center; justify-content:center; }
.confirm-eyebrow { font-size:11px; font-weight:700; color:var(--rojo); text-transform:uppercase; letter-spacing:.6px; margin:0 0 4px; }
.confirm-title { font-size:18px; font-weight:800; color:var(--texto); margin:0; }
.confirm-body { padding:22px 32px; }
.warn-box { background:#FFFBF3; border-left:3px solid var(--amarillo); border-radius:0 8px 8px 0; padding:14px 16px; font-size:13px; color:var(--texto); line-height:1.7; }
.warn-box p { margin:0 0 6px; }
.warn-box p:last-child { margin:0; }
.confirm-footer { flex-shrink:0; display:flex; justify-content:flex-end; gap:10px; padding:16px 32px 22px; border-top:1px solid var(--borde-suave); }
.btn-danger { height:44px; padding:0 20px; border-radius:8px; background:var(--rojo-bg); border:none; color:var(--rojo); font-size:13.5px; font-weight:600; cursor:pointer; transition:background-color .16s ease, color .16s ease; }
.btn-danger:hover { background:var(--rojo); color:#fff; }

/* Animaciones modal */
.modal-fade-enter-active, .modal-fade-leave-active { transition:opacity 0.22s ease; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity:0; }

/* ── Responsive ── */
@media (max-width:1100px) { .don-summary { grid-template-columns:repeat(2, 1fr); } }
@media (max-width:900px) {
  .form-grid { grid-template-columns:repeat(2, 1fr); }
  .fg--span2 { grid-column:span 1; }
  .modal-box--uniform { width:94vw; height:88vh; }
  .grid-2col { grid-template-columns:1fr; }
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
  .don-table th:nth-child(4), .don-table td:nth-child(4), .don-table th:nth-child(6), .don-table td:nth-child(6) { display:none; }
  .modal-box--uniform { width:96vw; height:92vh; border-radius:18px; }
  .hero, .form-header, .form-body, .body, .footer, .form-footer, .confirm-header, .confirm-body, .confirm-footer { padding-left:20px; padding-right:20px; }
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