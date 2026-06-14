<script setup>
import { ref, computed } from 'vue'
import Icon from '../../components/Icon.vue'
import { usePetsStore } from '../../stores/usePetsStore'

const store = usePetsStore()

// ─────────────────────────────────────────────
// Casas cuna — cargadas desde voluntarios registrados
// ─────────────────────────────────────────────
const casasCuna = computed(() => {
  const usuarios = JSON.parse(localStorage.getItem('anhelo_usuarios')) || []
  return usuarios.filter(u =>
    (u.rol === 'Voluntario' || u.tipoVoluntario === 'Casa cuna' || u.solicitudVoluntario?.tipo === 'Casa cuna') &&
    (u.activo === true || u.activo === 'true' || u.estado === 'Activo' || u.solicitudVoluntario?.estado === 'Aprobada') &&
    (u.tipoVoluntario === 'Casa cuna' || u.solicitudVoluntario?.tipo === 'Casa cuna')
  )
})

// ─────────────────────────────────────────────
// Solicitudes por mascota
// ─────────────────────────────────────────────
const solicitudesMascota = computed(() => {
  if (!requestsTarget.value) return []
  const solicitudes = JSON.parse(localStorage.getItem('anhelo_solicitudes')) || []
  return solicitudes.filter(
    s => s.petId === requestsTarget.value.id || s.mascota === requestsTarget.value.name
  )
})

// ─────────────────────────────────────────────
// Estado del formulario
// ─────────────────────────────────────────────
const showForm     = ref(false)
const editMode     = ref(false)
const editingPetId = ref(null)

const formData = ref({
  name: '', type: 'Perro', breed: '', age: '', sex: 'Macho',
  size: 'Mediano', personality: '', healthBasic: '', status: 'Disponible',
  description: '', internalNotes: '', images: [],
  casaCunaId: '', casaCunaNombre: '',
})
const formErrors = ref({})

// ─────────────────────────────────────────────
// Modales
// ─────────────────────────────────────────────
const showDeactivateModal = ref(false)
const deactivateTarget    = ref(null)

const showRequestsModal   = ref(false)
const requestsTarget      = ref(null)

const showViewModal       = ref(false)
const viewTarget          = ref(null)

const showStatusModal     = ref(false)
const statusTargetPet     = ref(null)
const pendingStatus       = ref('')

// ─────────────────────────────────────────────
// Toast
// ─────────────────────────────────────────────
const toast = ref({ show: false, type: 'success', message: '' })
let toastTimer = null
function showToast(type, message) {
  clearTimeout(toastTimer)
  toast.value = { show: true, type, message }
  toastTimer = setTimeout(() => { toast.value.show = false }, 3500)
}

// ─────────────────────────────────────────────
// Filtros
// ─────────────────────────────────────────────
const filterStatus = ref('Todos')
const filterType   = ref('Todos')
const searchQuery  = ref('')

const STATUS_OPTIONS = ['Disponible', 'En proceso', 'Adoptada', 'Inactiva']
const STATUS_TABS    = ['Todos', 'En rescate', 'Disponible', 'En proceso', 'Adoptada', 'Inactiva']
const TYPE_TABS      = ['Todos', 'Perro', 'Gato']

// ─────────────────────────────────────────────
// Badges
// ─────────────────────────────────────────────
const statusBadgeClass = s => ({
  'Disponible':  'badge-aprobada',
  'En proceso':  'badge-pendiente',
  'Adoptada':    'badge-adoptada',
  'Inactiva':    'badge-inactiva',
  'En rescate':  'badge-rescate',
}[s] || 'badge-inactiva')

// ─────────────────────────────────────────────
// Estadísticas
// ─────────────────────────────────────────────
const stats = computed(() => ({
  total:      store.pets.length,
  disponible: store.pets.filter(p => p.status === 'Disponible').length,
  enProceso:  store.pets.filter(p => p.status === 'En proceso').length,
  adoptada:   store.pets.filter(p => p.status === 'Adoptada').length,
  inactiva:   store.pets.filter(p => p.status === 'Inactiva').length,
}))

// ─────────────────────────────────────────────
// Lista filtrada
// ─────────────────────────────────────────────
const filteredPets = computed(() => {
  return store.pets.filter(p => {
    const matchStatus = filterStatus.value === 'Todos' || p.status === filterStatus.value
    const matchType   = filterType.value === 'Todos' || p.type === filterType.value
    const q = searchQuery.value.toLowerCase()
    const matchSearch = !q ||
      p.name.toLowerCase().includes(q) ||
      String(p.id).toLowerCase().includes(q) ||
      p.breed.toLowerCase().includes(q)
    return matchStatus && matchType && matchSearch
  })
})

const hayFiltros = computed(() =>
  searchQuery.value.trim() !== '' ||
  filterStatus.value !== 'Todos' ||
  filterType.value !== 'Todos'
)

// ─────────────────────────────────────────────
// Imágenes
// ─────────────────────────────────────────────
const imageInputRef = ref(null)

function handleImageUpload(e) {
  const files = Array.from(e.target.files)
  files.forEach(file => {
    if (!file.type.startsWith('image/')) return
    const reader = new FileReader()
    reader.onload = ev => {
      formData.value.images.push({ preview: ev.target.result, file, name: file.name })
    }
    reader.readAsDataURL(file)
  })
  e.target.value = ''
}

function removeImage(index) {
  formData.value.images.splice(index, 1)
}

// ─────────────────────────────────────────────
// Casa cuna — cambio de select
// ─────────────────────────────────────────────
function onCasaCunaChange(e) {
  const id = e.target.value
  formData.value.casaCunaId = id
  if (!id) {
    formData.value.casaCunaNombre = ''
    return
  }
  const cc = casasCuna.value.find(u => String(u.id) === String(id))
  formData.value.casaCunaNombre = cc ? (cc.nombre || cc.name || '') : ''
}

// ─────────────────────────────────────────────
// Validación
// ─────────────────────────────────────────────
function validateForm() {
  const errors = {}
  if (!formData.value.name.trim())        errors.name        = 'El nombre es obligatorio'
  if (!formData.value.breed.trim())       errors.breed       = 'La raza es obligatoria'
  if (!formData.value.age.trim())         errors.age         = 'La edad es obligatoria'
  if (!formData.value.healthBasic.trim()) errors.healthBasic = 'El estado de salud es obligatorio'
  if (formData.value.images.length === 0) errors.images      = 'Debes subir al menos una foto'
  formErrors.value = errors
  return Object.keys(errors).length === 0
}

function clearErr(campo) {
  if (formErrors.value[campo]) {
    const e = { ...formErrors.value }
    delete e[campo]
    formErrors.value = e
  }
}

// ─────────────────────────────────────────────
// Guardar mascota
// ─────────────────────────────────────────────
function savePet() {
  if (!validateForm()) return
  const petData = { ...formData.value, images: [...formData.value.images] }
  if (editMode.value && editingPetId.value !== null) {
    store.updatePet(editingPetId.value, petData)
    showToast('success', 'Mascota actualizada correctamente')
  } else {
    store.addPet(petData)
    showToast('success', 'Mascota registrada correctamente')
  }
  closeForm()
}

// ─────────────────────────────────────────────
// Formulario edit / close
// ─────────────────────────────────────────────
function openForm() {
  editMode.value     = false
  editingPetId.value = null
  formErrors.value   = {}
  formData.value     = {
    name: '', type: 'Perro', breed: '', age: '', sex: 'Macho',
    size: 'Mediano', personality: '', healthBasic: '', status: 'Disponible',
    description: '', internalNotes: '', images: [],
    casaCunaId: '', casaCunaNombre: '',
  }
  showForm.value = true
}

function openEdit(pet) {
  editMode.value     = true
  editingPetId.value = pet.id
  formData.value     = {
    name:          pet.name,
    type:          pet.type,
    breed:         pet.breed,
    age:           pet.age,
    sex:           pet.sex,
    size:          pet.size,
    personality:   pet.personality,
    healthBasic:   pet.healthBasic,
    status:        pet.status,
    description:   pet.description,
    internalNotes: pet.internalNotes || '',
    images:        [...(pet.images || [])],
    casaCunaId:    pet.casaCunaId    || '',
    casaCunaNombre: pet.casaCunaNombre || '',
  }
  formErrors.value = {}
  showForm.value   = true
  showViewModal.value = false
}

function closeForm() {
  showForm.value     = false
  editMode.value     = false
  editingPetId.value = null
  formErrors.value   = {}
  formData.value     = {
    name: '', type: 'Perro', breed: '', age: '', sex: 'Macho',
    size: 'Mediano', personality: '', healthBasic: '', status: 'Disponible',
    description: '', internalNotes: '', images: [],
    casaCunaId: '', casaCunaNombre: '',
  }
}

// ─────────────────────────────────────────────
// Ver mascota (solo lectura)
// ─────────────────────────────────────────────
function openView(pet) {
  viewTarget.value    = pet
  showViewModal.value = true
}

// ─────────────────────────────────────────────
// Cambio de estado
// ─────────────────────────────────────────────
function openStatusModal(pet) {
  statusTargetPet.value = pet
  pendingStatus.value   = pet.status
  showStatusModal.value = true
}

function confirmStatusChange() {
  if (!statusTargetPet.value) return
  store.changeStatus(statusTargetPet.value.id, pendingStatus.value)
  showStatusModal.value = false
  statusTargetPet.value = null
}

// ─────────────────────────────────────────────
// Activar / Inactivar rápido
// ─────────────────────────────────────────────
function toggleActive(pet) {
  if (pet.status === 'Inactiva') {
    store.changeStatus(pet.id, 'Disponible')
  } else {
    deactivateTarget.value    = pet
    showDeactivateModal.value = true
  }
}

// ─────────────────────────────────────────────
// Desactivar
// ─────────────────────────────────────────────
function confirmDeactivate() {
  if (!deactivateTarget.value) return
  store.deactivatePet(deactivateTarget.value.id)
  showDeactivateModal.value = false
  deactivateTarget.value    = null
}

// ─────────────────────────────────────────────
// Ver solicitudes
// ─────────────────────────────────────────────
function openRequests(pet) {
  requestsTarget.value    = pet
  showRequestsModal.value = true
}

// ─────────────────────────────────────────────
// Helpers
// ─────────────────────────────────────────────
function getNombreCasaCuna(pet) {
  if (pet.casaCunaNombre) return pet.casaCunaNombre
  if (pet.casaCunaId) {
    const cc = casasCuna.value.find(u => String(u.id) === String(pet.casaCunaId))
    return cc ? (cc.nombre || cc.name || '—') : '—'
  }
  return 'Sin asignar'
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
         FORMULARIO — pantalla independiente
    ══════════════════════════════════════ -->
    <Transition name="form-slide">
      <div v-if="showForm" class="form-screen">

        <!-- Header del formulario -->
        <header class="page-header">
          <div>
            <h1 class="admin-page-title">{{ editMode ? 'Editar mascota' : 'Nueva mascota' }}</h1>
            <p class="admin-page-sub">{{ editMode ? 'Modifica los datos del animal' : 'Completa la información del animal para el catálogo' }}</p>
          </div>
          <button class="btn-cancelar" @click="closeForm">
            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            Cancelar
          </button>
        </header>

        <div class="form-panel">

          <!-- Sección 1: Información básica -->
          <div class="section-label">
            <span class="section-num">1</span> Información básica
          </div>
          <div class="form-grid form-grid--4" style="margin-bottom:28px">
            <div class="fg">
              <label>Nombre <span class="req">*</span></label>
              <input
                v-model="formData.name"
                placeholder="Nombre del animal"
                class="filtro-input"
                :class="{ 'is-error': formErrors.name }"
                @input="clearErr('name')"
              />
              <p v-if="formErrors.name" class="err-msg">{{ formErrors.name }}</p>
            </div>
            <div class="fg">
              <label>Tipo <span class="req">*</span></label>
              <div class="filtro-input-wrap">
                <select v-model="formData.type" class="filtro-input filtro-select">
                  <option>Perro</option>
                  <option>Gato</option>
                </select>
                <span class="filtro-icon filtro-icon--right filtro-icon--no-events">
                  <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
                </span>
              </div>
            </div>
            <div class="fg">
              <label>Raza <span class="req">*</span></label>
              <input
                v-model="formData.breed"
                placeholder="Raza"
                class="filtro-input"
                :class="{ 'is-error': formErrors.breed }"
                @input="clearErr('breed')"
              />
              <p v-if="formErrors.breed" class="err-msg">{{ formErrors.breed }}</p>
            </div>
            <div class="fg">
              <label>Edad <span class="req">*</span></label>
              <input
                v-model="formData.age"
                placeholder="Ej. 2 años"
                class="filtro-input"
                :class="{ 'is-error': formErrors.age }"
                @input="clearErr('age')"
              />
              <p v-if="formErrors.age" class="err-msg">{{ formErrors.age }}</p>
            </div>
            <div class="fg">
              <label>Sexo</label>
              <div class="filtro-input-wrap">
                <select v-model="formData.sex" class="filtro-input filtro-select">
                  <option>Macho</option>
                  <option>Hembra</option>
                </select>
                <span class="filtro-icon filtro-icon--right filtro-icon--no-events">
                  <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
                </span>
              </div>
            </div>
            <div class="fg">
              <label>Tamaño</label>
              <div class="filtro-input-wrap">
                <select v-model="formData.size" class="filtro-input filtro-select">
                  <option>Pequeño</option>
                  <option>Mediano</option>
                  <option>Grande</option>
                </select>
                <span class="filtro-icon filtro-icon--right filtro-icon--no-events">
                  <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
                </span>
              </div>
            </div>
            <div class="fg">
              <label>Estado</label>
              <div class="filtro-input-wrap">
                <select v-model="formData.status" class="filtro-input filtro-select">
                  <option v-for="s in STATUS_OPTIONS" :key="s">{{ s }}</option>
                </select>
                <span class="filtro-icon filtro-icon--right filtro-icon--no-events">
                  <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
                </span>
              </div>
            </div>
            <div class="fg">
              <label>Salud básica <span class="req">*</span></label>
              <input
                v-model="formData.healthBasic"
                placeholder="Ej. Vacunado, desparasitado"
                class="filtro-input"
                :class="{ 'is-error': formErrors.healthBasic }"
                @input="clearErr('healthBasic')"
              />
              <p v-if="formErrors.healthBasic" class="err-msg">{{ formErrors.healthBasic }}</p>
            </div>
            <div class="fg">
              <label>Personalidad</label>
              <input
                v-model="formData.personality"
                placeholder="Ej. Juguetón, tranquilo"
                class="filtro-input"
              />
            </div>
            <div class="fg fg--span2">
              <label>Casa cuna asignada</label>
              <div class="filtro-input-wrap">
                <select
                  class="filtro-input filtro-select"
                  :value="formData.casaCunaId"
                  @change="onCasaCunaChange"
                >
                  <option value="">Sin asignar</option>
                  <option
                    v-for="cc in casasCuna"
                    :key="cc.id"
                    :value="cc.id"
                  >{{ cc.nombre || cc.name }}</option>
                </select>
                <span class="filtro-icon filtro-icon--right filtro-icon--no-events">
                  <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
                </span>
              </div>
            </div>
          </div>

          <!-- Sección 2: Contenido público -->
          <div class="section-label">
            <span class="section-num">2</span> Contenido público
          </div>
          <div class="form-grid form-grid--4" style="margin-bottom:28px">
            <div class="fg fg--full">
              <label>Descripción pública</label>
              <textarea
                v-model="formData.description"
                placeholder="Descripción visible en el catálogo..."
                class="form-textarea"
              ></textarea>
            </div>
          </div>

          <!-- Sección 3: Notas internas -->
          <div class="section-label">
            <span class="section-num">3</span> Notas internas
            <span class="private-badge">🔒 Solo admin</span>
          </div>
          <div class="form-grid form-grid--4" style="margin-bottom:28px">
            <div class="fg fg--full">
              <textarea
                v-model="formData.internalNotes"
                placeholder="Historial médico, ubicación exacta, observaciones privadas..."
                class="form-textarea form-textarea--private"
              ></textarea>
            </div>
          </div>

          <!-- Sección 4: Fotos -->
          <div class="section-label">
            <span class="section-num">4</span> Fotos <span class="req">*</span>
          </div>

          <div v-if="formData.images.length > 0" class="image-previews" style="margin-bottom:28px">
            <div v-for="(img, i) in formData.images" :key="i" class="image-preview-item">
              <img :src="img.preview" :alt="img.name" />
              <button class="remove-image-btn" @click="removeImage(i)" title="Eliminar">×</button>
              <span v-if="i === 0" class="main-photo-label">Principal</span>
            </div>
            <button class="add-more-btn" @click="imageInputRef.click()">
              <span class="add-more-icon">+</span>
              <span>Agregar</span>
            </button>
          </div>

          <div
            v-else
            class="upload-zone"
            :class="{ 'is-error': formErrors.images }"
            @click="imageInputRef.click()"
            style="margin-bottom:8px"
          >
            <div class="upload-icon">📷</div>
            <p class="upload-title">Subir fotos del animal</p>
            <p class="upload-sub">Haz clic para seleccionar · JPG, PNG, WebP</p>
          </div>
          <p v-if="formErrors.images" class="err-msg" style="margin-bottom:20px">{{ formErrors.images }}</p>

          <input
            ref="imageInputRef"
            type="file"
            accept="image/*"
            multiple
            style="display:none"
            @change="handleImageUpload"
          />

          <div class="immutable-note">
            <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
            Todos los campos marcados con * son obligatorios para publicar la mascota en el catálogo
          </div>
        </div>

        <!-- Footer del formulario -->
        <div class="form-footer">
          <button class="btn-limpiar btn-limpiar--activo" @click="closeForm">Cancelar</button>
          <button class="btn-guardar" @click="savePet">
            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
            {{ editMode ? 'Guardar cambios' : 'Registrar mascota' }}
          </button>
        </div>
      </div>
    </Transition>

    <!-- ══════════════════════════════════════
         VISTA PRINCIPAL
    ══════════════════════════════════════ -->
    <div v-if="!showForm">

      <!-- CABECERA -->
      <header class="page-header">
        <div>
          <h1 class="admin-page-title">Gestión de Mascotas</h1>
          <p class="admin-page-sub">Registro y administración de animales de la fundación</p>
        </div>
        <button class="btn-nuevo" @click="openForm">
          <svg xmlns="http://www.w3.org/2000/svg" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
          Nueva mascota
        </button>
      </header>

      <!-- TARJETAS RESUMEN -->
      <div class="don-summary">
        <div class="don-card total-card">
          <span class="don-label">Total mascotas</span>
          <strong class="don-value">{{ stats.total }}</strong>
          <span class="don-desc">En el sistema</span>
        </div>
        <div class="don-card disponible-card">
          <span class="don-label">Disponibles</span>
          <strong class="don-value">{{ stats.disponible }}</strong>
          <span class="don-desc">Visibles en catálogo</span>
        </div>
        <div class="don-card proceso-card">
          <span class="don-label">En proceso</span>
          <strong class="don-value">{{ stats.enProceso }}</strong>
          <span class="don-desc">Evaluando solicitudes</span>
        </div>
        <div class="don-card adoptada-card">
          <span class="don-label">Adoptadas</span>
          <strong class="don-value">{{ stats.adoptada }}</strong>
          <span class="don-desc">Historias felices</span>
        </div>
        <div class="don-card inactiva-card">
          <span class="don-label">Inactivas</span>
          <strong class="don-value">{{ stats.inactiva }}</strong>
          <span class="don-desc">Ocultas del público</span>
        </div>
      </div>

      <!-- FILTROS -->
      <div class="filtros-panel">

        <!-- Tipo tabs -->
        <div class="filtro-group filtro-group--tabs">
          <label class="filtro-label">Tipo</label>
          <div class="tabs-wrap">
            <button
              v-for="t in TYPE_TABS"
              :key="t"
              class="tab-btn"
              :class="{ active: filterType === t }"
              @click="filterType = t"
            >{{ t }}</button>
          </div>
        </div>

        <!-- Estado tabs -->
        <div class="filtro-group filtro-group--tabs">
          <label class="filtro-label">Estado</label>
          <div class="tabs-wrap">
            <button
              v-for="s in STATUS_TABS"
              :key="s"
              class="tab-btn"
              :class="{ active: filterStatus === s }"
              @click="filterStatus = s"
            >{{ s }}</button>
          </div>
        </div>

        <!-- Buscar -->
        <div class="filtro-group">
          <label class="filtro-label">Buscar</label>
          <div class="filtro-input-wrap">
            <input
              v-model="searchQuery"
              placeholder="ID, nombre o raza..."
              class="filtro-input filtro-input--icon"
            />
            <span class="filtro-icon filtro-icon--right">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
            </span>
          </div>
        </div>

        <!-- Limpiar -->
        <div class="filtro-group filtro-group--btn">
          <button
            class="btn-limpiar"
            :class="{ 'btn-limpiar--activo': hayFiltros }"
            @click="searchQuery = ''; filterStatus = 'Todos'; filterType = 'Todos'"
          >Limpiar filtros</button>
        </div>
      </div>

      <!-- ESTADO VACÍO -->
      <div v-if="filteredPets.length === 0" class="empty-state">
        <svg xmlns="http://www.w3.org/2000/svg" width="36" height="36" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M20 7H4a2 2 0 0 0-2 2v10a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2V9a2 2 0 0 0-2-2z"/><path d="M16 3l-4 4-4-4"/></svg>
        <p class="empty-title">{{ hayFiltros ? 'Sin resultados para los filtros aplicados' : 'No hay mascotas registradas aún' }}</p>
        <p class="empty-sub">{{ hayFiltros ? 'Ajusta los filtros para ver más resultados.' : 'Registra la primera mascota con el botón superior.' }}</p>
      </div>

      <!-- TABLA -->
      <div v-else class="table-wrapper">
        <div class="table-scroll">
          <table class="don-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Foto</th>
                <th>Nombre</th>
                <th>Tipo</th>
                <th>Estado</th>
                <th>Casa cuna</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="p in filteredPets"
                :key="p.id"
                class="don-row"
                :class="{ 'row-inactive': p.status === 'Inactiva' }"
              >
                <!-- ID -->
                <td><span class="id-pill">{{ p.id }}</span></td>

                <!-- Foto -->
                <td>
                  <div class="pet-avatar">
                    <img
                      v-if="p.images?.length > 0"
                      :src="p.images[0].preview"
                      class="pet-avatar-img"
                      :alt="p.name"
                    />
                    <span v-else class="pet-avatar-ini">{{ p.name?.charAt(0) }}</span>
                  </div>
                </td>

                <!-- Nombre -->
                <td>
                  <span class="donor-name">{{ p.name }}</span>
                  <span class="donor-mail">{{ p.breed }}</span>
                </td>

                <!-- Tipo -->
                <td><span class="type-chip">{{ p.type }}</span></td>

                <!-- Estado -->
                <td><span class="estado-badge" :class="statusBadgeClass(p.status)">{{ p.status }}</span></td>

                <!-- Casa cuna -->
                <td><span class="fecha-text">{{ getNombreCasaCuna(p) }}</span></td>

                <!-- Acciones -->
                <td>
                  <div class="action-group">
                    <button class="btn-accion" @click="openView(p)" title="Ver mascota">
                      <Icon name="Eye" />
                    </button>
                    <button class="btn-accion" @click="openEdit(p)" title="Editar">
                      <Icon name="Edit" />
                    </button>
                    <button class="btn-accion" @click="openRequests(p)" title="Ver solicitudes">
                      <Icon name="Clipboard" />
                    </button>
                    <button
                      class="btn-accion"
                      @click="toggleActive(p)"
                      :title="p.status === 'Inactiva' ? 'Activar mascota' : 'Desactivar mascota'"
                    >
                      <Icon :name="p.status === 'Inactiva' ? 'CheckCircle' : 'Archive'" />
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <div class="table-footer">
          {{ filteredPets.length }} mascota{{ filteredPets.length !== 1 ? 's' : '' }} encontrada{{ filteredPets.length !== 1 ? 's' : '' }}
        </div>
      </div>

    </div>


    <!-- ══════════════════════════════════════
         MODAL: Ver mascota
    ══════════════════════════════════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showViewModal && viewTarget" class="modal-overlay" @click.self="showViewModal = false">
          <div class="modal-box">
            <button class="modal-close" @click="showViewModal = false">✕</button>

            <div class="modal-header">
              <div class="pet-avatar pet-avatar--lg">
                <img
                  v-if="viewTarget.images?.length > 0"
                  :src="viewTarget.images[0].preview"
                  class="pet-avatar-img"
                  :alt="viewTarget.name"
                />
                <span v-else class="pet-avatar-ini pet-avatar-ini--lg">{{ viewTarget.name?.charAt(0) }}</span>
              </div>
              <div class="modal-header-info">
                <p class="modal-eyebrow">Detalle de mascota</p>
                <h2 class="modal-title">{{ viewTarget.name }}</h2>
                <p class="modal-sub">{{ viewTarget.breed }}</p>
              </div>
              <div class="modal-header-badges">
                <span class="id-pill">{{ viewTarget.id }}</span>
                <span class="estado-badge" :class="statusBadgeClass(viewTarget.status)">{{ viewTarget.status }}</span>
              </div>
            </div>

            <div class="modal-section">
              <h4 class="modal-section-title">Información general</h4>
              <div class="modal-grid modal-grid--3">
                <div class="modal-field">
                  <span class="modal-field-label">Tipo</span>
                  <strong class="modal-field-value">{{ viewTarget.type }}</strong>
                </div>
                <div class="modal-field">
                  <span class="modal-field-label">Edad</span>
                  <strong class="modal-field-value">{{ viewTarget.age }}</strong>
                </div>
                <div class="modal-field">
                  <span class="modal-field-label">Sexo</span>
                  <strong class="modal-field-value">{{ viewTarget.sex }}</strong>
                </div>
                <div class="modal-field">
                  <span class="modal-field-label">Tamaño</span>
                  <strong class="modal-field-value">{{ viewTarget.size }}</strong>
                </div>
                <div class="modal-field">
                  <span class="modal-field-label">Salud básica</span>
                  <strong class="modal-field-value">{{ viewTarget.healthBasic }}</strong>
                </div>
                <div class="modal-field">
                  <span class="modal-field-label">Casa cuna</span>
                  <strong class="modal-field-value">{{ getNombreCasaCuna(viewTarget) }}</strong>
                </div>
                <div v-if="viewTarget.personality" class="modal-field">
                  <span class="modal-field-label">Personalidad</span>
                  <strong class="modal-field-value">{{ viewTarget.personality }}</strong>
                </div>
              </div>
            </div>

            <div v-if="viewTarget.description" class="modal-section">
              <h4 class="modal-section-title">Descripción pública</h4>
              <p class="modal-mensaje">{{ viewTarget.description }}</p>
            </div>

            <div v-if="viewTarget.internalNotes" class="modal-section">
              <h4 class="modal-section-title">🔒 Notas internas</h4>
              <p class="modal-mensaje modal-mensaje--private">{{ viewTarget.internalNotes }}</p>
            </div>

            <div v-if="viewTarget.images?.length > 1" class="modal-section">
              <h4 class="modal-section-title">Galería</h4>
              <div class="modal-gallery">
                <img
                  v-for="(img, i) in viewTarget.images.slice(1)"
                  :key="i"
                  :src="img.preview"
                  :alt="viewTarget.name"
                  class="gallery-img"
                />
              </div>
            </div>

            <div class="modal-acciones">
              <button class="btn-limpiar btn-limpiar--activo" @click="showViewModal = false">Cerrar</button>
              <button class="btn-guardar" @click="openEdit(viewTarget)">
                <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                Editar mascota
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>


    <!-- ══════════════════════════════════════
         MODAL: Cambiar estado
    ══════════════════════════════════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showStatusModal" class="modal-overlay" @click.self="showStatusModal = false">
          <div class="modal-box modal-box--sm">
            <button class="modal-close" @click="showStatusModal = false">✕</button>
            <div class="modal-header">
              <div class="modal-header-info">
                <p class="modal-eyebrow">Cambiar estado</p>
                <h2 class="modal-title">{{ statusTargetPet?.name }}</h2>
              </div>
            </div>
            <div class="modal-section">
              <div class="status-options">
                <label
                  v-for="s in STATUS_OPTIONS"
                  :key="s"
                  class="status-option"
                  :class="{ selected: pendingStatus === s }"
                >
                  <input type="radio" :value="s" v-model="pendingStatus" />
                  <span class="estado-badge" :class="statusBadgeClass(s)">{{ s }}</span>
                  <span class="status-desc">
                    <template v-if="s === 'Disponible'">Visible en catálogo, acepta solicitudes</template>
                    <template v-else-if="s === 'En proceso'">Visible, evaluando solicitudes</template>
                    <template v-else-if="s === 'Adoptada'">Se mueve a «Historias felices»</template>
                    <template v-else-if="s === 'Inactiva'">Oculta del público, historial conservado</template>
                  </span>
                </label>
              </div>
            </div>
            <div class="modal-acciones">
              <button class="btn-limpiar btn-limpiar--activo" @click="showStatusModal = false">Cancelar</button>
              <button class="btn-guardar" @click="confirmStatusChange">
                <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
                Confirmar
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>


    <!-- ══════════════════════════════════════
         MODAL: Confirmar desactivar
    ══════════════════════════════════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showDeactivateModal" class="modal-overlay" @click.self="showDeactivateModal = false">
          <div class="modal-box modal-box--sm">
            <button class="modal-close" @click="showDeactivateModal = false">✕</button>
            <div class="modal-header">
              <div class="modal-header-info">
                <p class="modal-eyebrow">Desactivar mascota</p>
                <h2 class="modal-title">{{ deactivateTarget?.name }}</h2>
              </div>
            </div>
            <div class="modal-section">
              <div class="info-box">
                <p>🔒 <strong>No se eliminará</strong> del sistema. Se conservará todo su historial, solicitudes y registros.</p>
                <p>La mascota pasará a estado <strong>Inactiva</strong> y dejará de ser visible en el catálogo público.</p>
              </div>
            </div>
            <div class="modal-acciones">
              <button class="btn-limpiar btn-limpiar--activo" @click="showDeactivateModal = false">Cancelar</button>
              <button class="btn-rechazar" @click="confirmDeactivate">Desactivar</button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>


    <!-- ══════════════════════════════════════
         MODAL: Solicitudes de adopción
    ══════════════════════════════════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showRequestsModal" class="modal-overlay" @click.self="showRequestsModal = false">
          <div class="modal-box">
            <button class="modal-close" @click="showRequestsModal = false">✕</button>
            <div class="modal-header">
              <div class="modal-header-info">
                <p class="modal-eyebrow">Solicitudes de adopción</p>
                <h2 class="modal-title">{{ requestsTarget?.name }}</h2>
              </div>
            </div>

            <div class="modal-section">
              <div v-if="solicitudesMascota.length" class="requests-list">
                <div v-for="s in solicitudesMascota" :key="s.id" class="request-card">
                  <div class="request-header">
                    <div class="pet-avatar">
                      <span class="pet-avatar-ini">{{ s.solicitante?.substring(0,1).toUpperCase() }}</span>
                    </div>
                    <div class="request-user">
                      <h4>{{ s.solicitante }}</h4>
                      <span class="id-pill">{{ s.id }}</span>
                    </div>
                    <span
                      class="estado-badge"
                      :class="{
                        'badge-aprobada': s.estado === 'Aprobada',
                        'badge-pendiente': s.estado === 'Pendiente',
                        'badge-proceso': s.estado === 'En proceso',
                        'badge-rechazada': s.estado === 'Rechazada',
                      }"
                    >{{ s.estado }}</span>
                  </div>
                  <div class="modal-grid" style="margin-top:12px">
                    <div class="modal-field">
                      <span class="modal-field-label">Teléfono</span>
                      <strong class="modal-field-value">{{ s.telefono }}</strong>
                    </div>
                    <div class="modal-field">
                      <span class="modal-field-label">Correo</span>
                      <strong class="modal-field-value">{{ s.email }}</strong>
                    </div>
                    <div class="modal-field">
                      <span class="modal-field-label">Fecha</span>
                      <strong class="modal-field-value">{{ s.fecha }}</strong>
                    </div>
                  </div>
                </div>
              </div>

              <div v-else class="empty-state" style="padding: 40px 20px; border:none; background:transparent;">
                <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
                <p class="empty-title">Sin solicitudes aún</p>
                <p class="empty-sub">Las solicitudes del catálogo público aparecerán aquí.</p>
              </div>
            </div>

            <div class="modal-acciones">
              <button class="btn-limpiar btn-limpiar--activo" @click="showRequestsModal = false">Cerrar</button>
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
  padding-bottom: 40px;
}

/* ── Toast ─────────────────────────────────────────── */
.don-toast {
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
.don-toast.success { background: #3A473C; color: #fff; }
.don-toast.error   { background: #c0392b; color: #fff; }
.don-toast-dot {
  width: 8px; height: 8px;
  border-radius: 50%;
  background: rgba(255,255,255,0.5);
  flex-shrink: 0;
}
.toast-fade-enter-active, .toast-fade-leave-active { transition: all 0.25s ease; }
.toast-fade-enter-from, .toast-fade-leave-to { opacity: 0; transform: translateY(10px); }

/* ── Encabezado ────────────────────────────────────── */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 28px;
  gap: 16px;
  flex-wrap: wrap;
}
.admin-page-title  { font-size: 28px; font-weight: 800; color: var(--verde); letter-spacing: -0.5px; line-height: 1.1; }
.admin-page-sub    { font-size: 14px; color: var(--texto-sec); margin-top: 4px; font-weight: 500; }

.btn-nuevo {
  display: flex;
  align-items: center;
  gap: 7px;
  padding: 11px 20px;
  background: var(--verde);
  color: #fff;
  border: none;
  border-radius: 12px;
  font-size: 14px;
  font-weight: 700;
  cursor: pointer;
  transition: background 0.18s, transform 0.1s;
  white-space: nowrap;
  flex-shrink: 0;
  font-family: inherit;
}
.btn-nuevo:hover { background: #2d3730; }
.btn-nuevo:active { transform: scale(0.97); }

.btn-cancelar {
  display: flex;
  align-items: center;
  gap: 7px;
  padding: 11px 20px;
  background: var(--fondo);
  color: var(--texto-sec);
  border: 1.5px solid var(--borde);
  border-radius: 12px;
  font-size: 14px;
  font-weight: 700;
  cursor: pointer;
  transition: background 0.18s;
  white-space: nowrap;
  flex-shrink: 0;
  font-family: inherit;
}
.btn-cancelar:hover { background: #E5EAE6; color: var(--verde); }

/* ── Tarjetas resumen ──────────────────────────────── */
.don-summary {
  display: flex;
  gap: 14px;
  margin-bottom: 20px;
  flex-wrap: wrap;
}
.don-card {
  flex: 1;
  min-width: 140px;
  background: var(--blanco);
  border-radius: 14px;
  padding: 20px;
  border: 1px solid var(--borde);
  border-top: 3px solid var(--borde);
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.total-card      { border-top-color: var(--verde-sec); }
.disponible-card { border-top-color: var(--verde-ok); }
.proceso-card    { border-top-color: var(--amarillo); }
.adoptada-card   { border-top-color: #6BA8C4; }
.inactiva-card   { border-top-color: var(--texto-sec); }

.don-label { font-size: 11px; color: var(--texto-sec); font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; }
.don-value { font-size: 28px; font-weight: 800; color: var(--verde); line-height: 1; margin: 2px 0; }
.don-desc  { font-size: 12px; color: #9AA89C; }

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
.filtro-group--tabs {
  flex: 0 0 auto;
  min-width: unset;
}
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
  min-height: 16px;
  display: flex;
  align-items: flex-end;
}

/* Tabs */
.tabs-wrap {
  display: flex;
  gap: 4px;
  background: var(--fondo);
  border-radius: 10px;
  padding: 4px;
}
.tab-btn {
  padding: 7px 12px;
  border-radius: 7px;
  border: none;
  background: transparent;
  color: var(--texto-sec);
  font-size: 12px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.18s;
  white-space: nowrap;
  font-family: inherit;
}
.tab-btn:hover { color: var(--verde); background: rgba(255,255,255,0.7); }
.tab-btn.active { background: var(--blanco); color: var(--verde); box-shadow: 0 1px 4px rgba(58,71,60,0.12); }

/* Inputs */
.filtro-input-wrap {
  position: relative;
  display: flex;
  align-items: center;
}
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
.filtro-input:focus { border-color: var(--verde-sec); background: var(--blanco); }
.filtro-input::placeholder { color: #9CA8A0; }
.filtro-input.is-error { border-color: #e57373; background: #fff8f8; }

.filtro-select {
  appearance: none;
  -webkit-appearance: none;
  cursor: pointer;
}
.filtro-input--icon { padding-left: 12px; }

.filtro-icon {
  position: absolute;
  display: flex;
  align-items: center;
  color: var(--texto-sec);
}
.filtro-icon--right  { right: 11px; }
.filtro-icon--no-events { pointer-events: none; }

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

/* ── Estado vacío ──────────────────────────────────── */
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

/* ── Tabla ─────────────────────────────────────────── */
.table-wrapper {
  background: var(--blanco);
  border-radius: 14px;
  border: 1px solid var(--borde);
  overflow: hidden;
}
.table-scroll        { overflow-x: auto; -webkit-overflow-scrolling: touch; }
.don-table           { width: 100%; border-collapse: collapse; min-width: 700px; }
.don-table thead tr  { background: var(--verde); }
.don-table thead th  { padding: 13px 16px; text-align: left; color: var(--blanco); font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.6px; white-space: nowrap; }
.don-table tbody tr  { border-bottom: 1px solid var(--borde); transition: background 0.15s; }
.don-table tbody tr:last-child { border-bottom: none; }
.don-table tbody tr:hover      { background: #F4F6F4; }
.don-table tbody td  { padding: 13px 16px; vertical-align: middle; }

.row-inactive { opacity: 0.5; }

/* Avatar mascota */
.pet-avatar {
  width: 38px; height: 38px;
  border-radius: 50%;
  overflow: hidden;
  flex-shrink: 0;
  background: #DDE6DE;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 1.5px solid #EEF3EE;
}
.pet-avatar--lg { width: 56px; height: 56px; flex-shrink: 0; }
.pet-avatar-img { width: 100%; height: 100%; object-fit: cover; display: block; }
.pet-avatar-ini {
  font-size: 14px;
  font-weight: 800;
  color: #5A6E5C;
  text-transform: uppercase;
  line-height: 1;
}
.pet-avatar-ini--lg { font-size: 20px; }

.id-pill    { font-size: 11px; font-family: monospace; background: var(--fondo); border: 1px solid var(--borde); padding: 3px 9px; border-radius: 6px; color: var(--verde); font-weight: 700; white-space: nowrap; }
.donor-name { display: block; font-size: 13px; font-weight: 700; color: var(--texto); line-height: 1.3; }
.donor-mail { display: block; font-size: 11px; color: var(--texto-sec); margin-top: 2px; }
.fecha-text { font-size: 13px; color: var(--texto-sec); }

.type-chip {
  font-size: 12px;
  font-weight: 600;
  color: #5A6E5C;
  background: rgba(146,168,148,.12);
  padding: 3px 10px;
  border-radius: 7px;
  white-space: nowrap;
}

/* Badges */
.estado-badge    { display: inline-block; font-size: 11px; font-weight: 700; padding: 4px 12px; border-radius: 20px; white-space: nowrap; }
.badge-pendiente { background: #FFF7E0; color: #96650A; }
.badge-aprobada  { background: #E8F5E9; color: #2E7D32; }
.badge-rechazada { background: #FDECEA; color: #B71C1C; }
.badge-adoptada  { background: rgba(130,160,180,.15); color: #4A6070; }
.badge-inactiva  { background: #F0F4F0; color: #7A847C; }
.badge-rescate   { background: rgba(230,150,80,.15); color: #9A5420; }
.badge-proceso   { background: rgba(110,155,255,.12); color: #4F73B8; }

/* Acciones */
.action-group {
  display: flex;
  gap: 5px;
  align-items: center;
}
.btn-accion {
  width: 30px; height: 30px;
  border-radius: 8px;
  border: 1.5px solid var(--borde);
  background: transparent;
  color: var(--texto-sec);
  cursor: pointer;
  display: inline-flex; align-items: center; justify-content: center;
  transition: all 0.15s;
  flex-shrink: 0;
}
.btn-accion:hover {
  background: var(--fondo);
  border-color: #C8D4C8;
  color: var(--verde);
  transform: translateY(-1px);
}

.table-footer { padding: 12px 16px; border-top: 1px solid var(--borde); font-size: 12px; color: var(--texto-sec); font-weight: 500; }

/* ── Formulario pantalla ───────────────────────────── */
.form-screen { display: flex; flex-direction: column; }
.form-panel {
  background: var(--blanco);
  border-radius: 14px;
  padding: 28px;
  border: 1px solid var(--borde);
  margin-bottom: 16px;
}
.form-footer {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  padding: 18px 0 4px;
}

.section-label {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 13px;
  font-weight: 800;
  color: var(--verde);
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin-bottom: 14px;
}
.section-num {
  width: 24px; height: 24px;
  border-radius: 7px;
  background: var(--verde);
  color: #fff;
  font-size: 11px;
  font-weight: 800;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}
.private-badge {
  font-size: 11px;
  font-weight: 600;
  color: #C88A37;
  background: rgba(249,193,122,.15);
  padding: 2px 8px;
  border-radius: 6px;
  text-transform: none;
  letter-spacing: 0;
}
.req { color: #c0392b; }

.form-grid { display: grid; gap: 14px; }
.form-grid--4 { grid-template-columns: repeat(4, 1fr); }
.fg { display: flex; flex-direction: column; gap: 6px; }
.fg--span2 { grid-column: span 2; }
.fg--full  { grid-column: 1 / -1; }
.fg label { font-size: 12px; font-weight: 700; color: #5A6E5C; letter-spacing: 0.1px; }

.err-msg { font-size: 11px; color: #c0392b; font-weight: 600; margin: 0; }

.form-textarea {
  padding: 10px 13px;
  border: 1.5px solid var(--borde);
  border-radius: 10px;
  font-size: 13px;
  color: var(--texto);
  background: var(--fondo);
  outline: none;
  font-family: inherit;
  transition: border-color 0.18s, background 0.18s;
  width: 100%;
  box-sizing: border-box;
  height: 88px;
  resize: vertical;
  line-height: 1.5;
}
.form-textarea:focus { border-color: var(--verde-sec); background: var(--blanco); }
.form-textarea--private {
  background: #FFFBF3;
  border-color: rgba(249,193,122,.3);
}
.form-textarea--private:focus { border-color: #F9C17A; }

/* Imágenes */
.upload-zone {
  border: 2px dashed #D0D9D1;
  border-radius: 14px;
  padding: 28px;
  text-align: center;
  cursor: pointer;
  transition: all 0.2s;
  background: #FAFCFA;
  margin-bottom: 8px;
}
.upload-zone:hover { border-color: var(--verde-sec); background: #F2F7F2; }
.upload-zone.is-error { border-color: #e57373; background: #fff8f8; }
.upload-icon  { font-size: 32px; margin-bottom: 8px; }
.upload-title { font-size: 14px; font-weight: 700; color: var(--texto); margin: 0 0 4px; }
.upload-sub   { font-size: 13px; color: #9AA89C; margin: 0; }

.image-previews { display: flex; flex-wrap: wrap; gap: 10px; }
.image-preview-item {
  position: relative;
  width: 88px; height: 88px;
  border-radius: 12px;
  overflow: hidden;
  border: 1.5px solid #EEF3EE;
}
.image-preview-item img { width: 100%; height: 100%; object-fit: cover; display: block; }
.remove-image-btn {
  position: absolute;
  top: 4px; right: 4px;
  width: 22px; height: 22px;
  border-radius: 50%;
  background: rgba(0,0,0,0.5);
  color: white;
  font-size: 13px;
  border: none;
  cursor: pointer;
  display: flex; align-items: center; justify-content: center;
}
.remove-image-btn:hover { background: rgba(180,40,40,.8); }
.main-photo-label {
  position: absolute;
  bottom: 0; left: 0; right: 0;
  background: rgba(45,58,46,.75);
  color: white;
  font-size: 10px;
  font-weight: 700;
  text-align: center;
  padding: 3px 0;
}
.add-more-btn {
  width: 88px; height: 88px;
  border-radius: 12px;
  border: 2px dashed #D0D9D1;
  background: #FAFCFA;
  color: #9AA89C;
  font-size: 12px;
  font-weight: 600;
  cursor: pointer;
  display: flex; flex-direction: column; align-items: center; justify-content: center;
  gap: 4px;
  transition: all 0.2s;
  font-family: inherit;
}
.add-more-btn:hover { border-color: var(--verde-sec); background: #F2F7F2; color: var(--verde); }
.add-more-icon { font-size: 22px; line-height: 1; }

.immutable-note {
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
.immutable-note svg { flex-shrink: 0; margin-top: 1px; }

.btn-guardar {
  display: flex;
  align-items: center;
  gap: 7px;
  padding: 10px 20px;
  background: var(--verde);
  border: none;
  border-radius: 10px;
  font-size: 13px;
  font-weight: 700;
  color: #fff;
  cursor: pointer;
  transition: background 0.18s;
  font-family: inherit;
}
.btn-guardar:hover { background: #2d3730; }

/* Transición formulario */
.form-slide-enter-active { transition: all 0.28s cubic-bezier(.4,0,.2,1); }
.form-slide-leave-active { transition: all 0.18s ease; }
.form-slide-enter-from   { opacity: 0; transform: translateY(-10px); }
.form-slide-leave-to     { opacity: 0; transform: translateY(-6px); }

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
  width: 100%; max-width: 640px;
  max-height: 90vh; overflow-y: auto;
  position: relative;
}
.modal-box--sm { max-width: 440px; }

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
  gap: 14px;
  margin-bottom: 24px;
  padding-bottom: 20px;
  border-bottom: 1px solid var(--borde);
}
.modal-header-info { flex: 1; min-width: 0; }
.modal-eyebrow {
  font-size: 11px; font-weight: 800; color: var(--verde-sec);
  text-transform: uppercase; letter-spacing: 0.7px; margin: 0 0 4px;
}
.modal-title { font-size: 20px; font-weight: 800; color: var(--verde); letter-spacing: -0.4px; margin: 0; }
.modal-sub   { font-size: 12px; color: var(--texto-sec); margin: 3px 0 0; }
.modal-header-badges {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 6px;
  flex-shrink: 0;
}

.modal-section       { margin-bottom: 24px; }
.modal-section-title {
  font-size: 11px; font-weight: 700; color: var(--texto-sec);
  text-transform: uppercase; letter-spacing: 0.5px;
  margin-bottom: 14px; padding-bottom: 10px;
  border-bottom: 1px solid var(--borde);
}

.modal-grid   { display: grid; grid-template-columns: repeat(2,1fr); gap: 14px; }
.modal-grid--3{ display: grid; grid-template-columns: repeat(3,1fr); gap: 12px; }
.modal-field  { display: flex; flex-direction: column; gap: 4px; background: var(--fondo); border-radius: 10px; padding: 10px 12px; border: 1px solid var(--borde); }
.modal-field-label { font-size: 10px; font-weight: 700; color: #9CA8A0; text-transform: uppercase; letter-spacing: 0.4px; }
.modal-field-value { font-size: 13px; color: var(--texto); font-weight: 600; word-break: break-word; }

.modal-mensaje {
  font-size: 14px; color: var(--texto); line-height: 1.7;
  background: var(--fondo); border-radius: 10px; padding: 14px 16px;
  margin: 0;
}
.modal-mensaje--private {
  background: #FFFBF3;
  border: 1px solid rgba(249,193,122,.3);
}

.modal-gallery { display: flex; gap: 8px; flex-wrap: wrap; }
.gallery-img   { width: 72px; height: 72px; border-radius: 10px; object-fit: cover; border: 1.5px solid var(--borde); }

.modal-acciones {
  display: flex;
  gap: 10px;
  justify-content: flex-end;
  padding-top: 20px;
  border-top: 1px solid var(--borde);
}

.btn-rechazar {
  padding: 10px 18px;
  border-radius: 10px;
  border: none;
  background: #FDECEA;
  color: #B71C1C;
  font-size: 13px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s;
  font-family: inherit;
}
.btn-rechazar:hover { background: #B71C1C; color: var(--blanco); }

/* Status options */
.status-options {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 8px;
}
.status-option {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 11px 14px;
  border-radius: 12px;
  border: 1.5px solid var(--borde);
  cursor: pointer;
  transition: all 0.15s;
}
.status-option input[type="radio"] { display: none; }
.status-option:hover   { border-color: var(--verde-sec); background: #FAFCFA; }
.status-option.selected{ border-color: var(--verde-sec); background: rgba(146,168,148,.07); }
.status-desc { font-size: 13px; color: #9AA89C; }

/* Info box */
.info-box {
  background: #F7FAF7;
  border-radius: 12px;
  padding: 16px;
  font-size: 13px;
  color: var(--texto);
  line-height: 1.7;
  border: 1px solid #EEF3EE;
}
.info-box p { margin: 0 0 6px; }
.info-box p:last-child { margin: 0; }

/* Requests list */
.requests-list { display: flex; flex-direction: column; gap: 12px; }
.request-card {
  border: 1.5px solid var(--borde);
  border-radius: 14px;
  padding: 16px;
  transition: box-shadow 0.15s;
}
.request-card:hover { box-shadow: 0 4px 16px rgba(0,0,0,0.06); }
.request-header { display: flex; align-items: center; gap: 12px; }
.request-user { flex: 1; display: flex; flex-direction: column; gap: 3px; }
.request-user h4 { margin: 0; font-size: 14px; font-weight: 700; color: var(--texto); }

/* Animaciones modal */
.modal-fade-enter-active, .modal-fade-leave-active { transition: opacity 0.22s ease; }
.modal-fade-enter-from, .modal-fade-leave-to       { opacity: 0; }

/* ── Responsive ────────────────────────────────────── */
@media (max-width: 1100px) {
  .don-summary { display: grid; grid-template-columns: repeat(3, 1fr); }
}
@media (max-width: 900px) {
  .don-summary { grid-template-columns: repeat(2, 1fr); }
  .form-grid--4 { grid-template-columns: repeat(2, 1fr); }
  .fg--span2 { grid-column: span 1; }
  .modal-grid--3 { grid-template-columns: repeat(2, 1fr); }
}
@media (max-width: 640px) {
  .page-header    { flex-direction: column; align-items: flex-start; }
  .filtros-panel  { flex-direction: column; }
  .filtro-group   { min-width: 100%; }
  .filtro-group--btn { width: 100%; }
  .btn-limpiar    { width: 100%; }
  .tabs-wrap      { flex-wrap: wrap; }
  .modal-grid     { grid-template-columns: 1fr; }
  .modal-grid--3  { grid-template-columns: 1fr 1fr; }
  .modal-box      { padding: 24px 20px; }
  .modal-acciones { flex-direction: column; }
  .don-summary    { grid-template-columns: 1fr 1fr; }
  .form-grid--4   { grid-template-columns: 1fr; }
  .fg--span2, .fg--full { grid-column: 1; }
  .don-table th:nth-child(4),
  .don-table td:nth-child(4),
  .don-table th:nth-child(6),
  .don-table td:nth-child(6) { display: none; }
}
@media (max-width: 480px) {
  .don-summary { grid-template-columns: 1fr; }
  .modal-grid--3 { grid-template-columns: 1fr; }
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

  .filtro-group {
    min-width: unset;
    width: 100%;
  }

  .filtro-group--tabs {
    min-width: unset;
    width: 100%;
  }

  .filtro-group--btn {
    width: 100%;
  }

  .tabs-wrap {
    overflow-x: auto;
    -webkit-overflow-scrolling: touch;
    flex-wrap: nowrap;
    padding-bottom: 4px;
  }

  .tab-btn {
    white-space: nowrap;
    flex-shrink: 0;
  }

  .btn-limpiar {
    width: 100%;
  }

  .table-scroll {
    overflow-x: auto;
    -webkit-overflow-scrolling: touch;
  }

  .form-panel {
    padding: 18px 14px;
  }

  .form-grid--4 {
    grid-template-columns: repeat(2, 1fr);
  }

  .fg--span2 {
    grid-column: span 1;
  }

  .modal-box {
    padding: 22px 16px;
    max-width: calc(100vw - 32px);
    max-height: 95vh;
  }

  .modal-grid { grid-template-columns: 1fr; }
  .modal-grid--3 { grid-template-columns: 1fr 1fr; }

  .modal-header {
    flex-wrap: wrap;
    gap: 10px;
  }

  .modal-acciones {
    flex-direction: column;
  }

  .modal-gallery { flex-wrap: wrap; }

  .status-options { gap: 6px; }

  .page-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 10px;
  }

  .btn-nuevo {
    width: 100%;
    justify-content: center;
  }

  .image-previews { flex-wrap: wrap; }
}

@media (max-width: 480px) {
  .don-summary { grid-template-columns: 1fr; }

  .form-grid--4 { grid-template-columns: 1fr; }

  .fg--span2,
  .fg--full { grid-column: span 1; }

  .modal-grid--3 { grid-template-columns: 1fr; }

  .don-table th:nth-child(5),
  .don-table td:nth-child(5) { display: none; }
}


</style>