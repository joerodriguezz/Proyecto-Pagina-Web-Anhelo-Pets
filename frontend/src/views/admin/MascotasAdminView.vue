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
  'Disponible':  'mc-badge-green',
  'En proceso':  'mc-badge-peach',
  'Adoptada':    'mc-badge-blue',
  'Inactiva':    'mc-badge-gray',
  'En rescate':  'mc-badge-orange',
}[s] || 'mc-badge-gray')

// ─────────────────────────────────────────────
// Estadísticas
// ─────────────────────────────────────────────
const stats = computed(() => ({
  total:     store.pets.length,
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
  <div class="mc-root">

    <!-- ── Toast ── -->
    <Teleport to="body">
      <Transition name="mc-toast-anim">
        <div v-if="toast.show" class="mc-toast" :class="toast.type">
          <span class="mc-toast-dot"></span>
          {{ toast.message }}
        </div>
      </Transition>
    </Teleport>

    <!-- ══════════════════════════════════════
         FORMULARIO — pantalla independiente
    ══════════════════════════════════════ -->
    <Transition name="mc-slide">
      <div v-if="showForm" class="mc-form-screen">

        <!-- Header del formulario -->
        <header class="mc-header">
          <div class="mc-header-left">
            <h1 class="mc-title">{{ editMode ? 'Editar mascota' : 'Nueva mascota' }}</h1>
            <p class="mc-sub">{{ editMode ? 'Modifica los datos del animal' : 'Completa la información del animal para el catálogo' }}</p>
          </div>
          <button class="mc-btn-cancel" @click="closeForm">
            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            Cancelar
          </button>
        </header>

        <div class="mc-form-panel">

          <!-- Sección 1: Información básica -->
          <div class="mc-section-label">
            <span class="mc-section-num">1</span> Información básica
          </div>
          <div class="mc-form-grid mc-form-grid--4" style="margin-bottom:28px">
            <div class="mc-fg">
              <label>Nombre <span class="mc-req">*</span></label>
              <input
                v-model="formData.name"
                placeholder="Nombre del animal"
                class="mc-input"
                :class="{ 'is-error': formErrors.name }"
                @input="clearErr('name')"
              />
              <p v-if="formErrors.name" class="mc-err-msg">{{ formErrors.name }}</p>
            </div>
            <div class="mc-fg">
              <label>Tipo <span class="mc-req">*</span></label>
              <select v-model="formData.type" class="mc-input">
                <option>Perro</option>
                <option>Gato</option>
              </select>
            </div>
            <div class="mc-fg">
              <label>Raza <span class="mc-req">*</span></label>
              <input
                v-model="formData.breed"
                placeholder="Raza"
                class="mc-input"
                :class="{ 'is-error': formErrors.breed }"
                @input="clearErr('breed')"
              />
              <p v-if="formErrors.breed" class="mc-err-msg">{{ formErrors.breed }}</p>
            </div>
            <div class="mc-fg">
              <label>Edad <span class="mc-req">*</span></label>
              <input
                v-model="formData.age"
                placeholder="Ej. 2 años"
                class="mc-input"
                :class="{ 'is-error': formErrors.age }"
                @input="clearErr('age')"
              />
              <p v-if="formErrors.age" class="mc-err-msg">{{ formErrors.age }}</p>
            </div>
            <div class="mc-fg">
              <label>Sexo</label>
              <select v-model="formData.sex" class="mc-input">
                <option>Macho</option>
                <option>Hembra</option>
              </select>
            </div>
            <div class="mc-fg">
              <label>Tamaño</label>
              <select v-model="formData.size" class="mc-input">
                <option>Pequeño</option>
                <option>Mediano</option>
                <option>Grande</option>
              </select>
            </div>
            <div class="mc-fg">
              <label>Estado</label>
              <select v-model="formData.status" class="mc-input">
                <option v-for="s in STATUS_OPTIONS" :key="s">{{ s }}</option>
              </select>
            </div>
            <div class="mc-fg">
              <label>Salud básica <span class="mc-req">*</span></label>
              <input
                v-model="formData.healthBasic"
                placeholder="Ej. Vacunado, desparasitado"
                class="mc-input"
                :class="{ 'is-error': formErrors.healthBasic }"
                @input="clearErr('healthBasic')"
              />
              <p v-if="formErrors.healthBasic" class="mc-err-msg">{{ formErrors.healthBasic }}</p>
            </div>
            <div class="mc-fg">
              <label>Personalidad</label>
              <input
                v-model="formData.personality"
                placeholder="Ej. Juguetón, tranquilo"
                class="mc-input"
              />
            </div>
            <div class="mc-fg mc-fg--span2">
              <label>Casa cuna asignada</label>
              <select
                class="mc-input"
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
            </div>
          </div>

          <!-- Sección 2: Contenido público -->
          <div class="mc-section-label">
            <span class="mc-section-num">2</span> Contenido público
          </div>
          <div class="mc-form-grid mc-form-grid--4" style="margin-bottom:28px">
            <div class="mc-fg mc-fg--full">
              <label>Descripción pública</label>
              <textarea
                v-model="formData.description"
                placeholder="Descripción visible en el catálogo..."
                class="mc-textarea"
              ></textarea>
            </div>
          </div>

          <!-- Sección 3: Notas internas -->
          <div class="mc-section-label">
            <span class="mc-section-num">3</span> Notas internas
            <span class="mc-private-badge">🔒 Solo admin</span>
          </div>
          <div class="mc-form-grid mc-form-grid--4" style="margin-bottom:28px">
            <div class="mc-fg mc-fg--full">
              <textarea
                v-model="formData.internalNotes"
                placeholder="Historial médico, ubicación exacta, observaciones privadas..."
                class="mc-textarea mc-textarea--private"
              ></textarea>
            </div>
          </div>

          <!-- Sección 4: Fotos -->
          <div class="mc-section-label">
            <span class="mc-section-num">4</span> Fotos <span class="mc-req">*</span>
          </div>

          <div v-if="formData.images.length > 0" class="mc-image-previews" style="margin-bottom:28px">
            <div v-for="(img, i) in formData.images" :key="i" class="mc-image-preview-item">
              <img :src="img.preview" :alt="img.name" />
              <button class="mc-remove-image-btn" @click="removeImage(i)" title="Eliminar">×</button>
              <span v-if="i === 0" class="mc-main-photo-label">Principal</span>
            </div>
            <button class="mc-add-more-btn" @click="imageInputRef.click()">
              <span class="mc-add-more-icon">+</span>
              <span>Agregar</span>
            </button>
          </div>

          <div
            v-else
            class="mc-upload-zone"
            :class="{ 'is-error': formErrors.images }"
            @click="imageInputRef.click()"
            style="margin-bottom:8px"
          >
            <div class="mc-upload-icon">📷</div>
            <p class="mc-upload-title">Subir fotos del animal</p>
            <p class="mc-upload-sub">Haz clic para seleccionar · JPG, PNG, WebP</p>
          </div>
          <p v-if="formErrors.images" class="mc-err-msg" style="margin-bottom:20px">{{ formErrors.images }}</p>

          <input
            ref="imageInputRef"
            type="file"
            accept="image/*"
            multiple
            style="display:none"
            @change="handleImageUpload"
          />

          <div class="mc-immutable-note">
            <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
            Todos los campos marcados con * son obligatorios para publicar la mascota en el catálogo
          </div>
        </div>

        <!-- Footer del formulario -->
        <div class="mc-form-footer">
          <button class="mc-btn-cancel-ghost" @click="closeForm">Cancelar</button>
          <button class="mc-btn-save" @click="savePet">
            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
            {{ editMode ? 'Guardar cambios' : 'Registrar mascota' }}
          </button>
        </div>
      </div>
    </Transition>

    <!-- ══════════════════════════════════════
         VISTA PRINCIPAL (tabla + filtros + stats)
    ══════════════════════════════════════ -->
    <div v-if="!showForm">

      <!-- ── Header ── -->
      <header class="mc-header">
        <div class="mc-header-left">
          <h1 class="mc-title">Gestión de Mascotas</h1>
          <p class="mc-sub">Registro y administración de animales de la fundación</p>
        </div>
        <button class="mc-btn-new" @click="openForm">
          <svg xmlns="http://www.w3.org/2000/svg" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
          Nueva mascota
        </button>
      </header>

      <!-- ── Estadísticas ── -->
      <div class="mc-stats-grid">
        <div class="mc-stat-card mc-stat-total">
          <div class="mc-stat-body">
            <div class="mc-stat-value">{{ stats.total }}</div>
            <div class="mc-stat-label">Total mascotas</div>
            <div class="mc-stat-desc">En el sistema</div>
          </div>
        </div>
        <div class="mc-stat-card mc-stat-disponible">
          <div class="mc-stat-body">
            <div class="mc-stat-value">{{ stats.disponible }}</div>
            <div class="mc-stat-label">Disponibles</div>
            <div class="mc-stat-desc">Visibles en catálogo</div>
          </div>
        </div>
        <div class="mc-stat-card mc-stat-proceso">
          <div class="mc-stat-body">
            <div class="mc-stat-value">{{ stats.enProceso }}</div>
            <div class="mc-stat-label">En proceso</div>
            <div class="mc-stat-desc">Evaluando solicitudes</div>
          </div>
        </div>
        <div class="mc-stat-card mc-stat-adoptada">
          <div class="mc-stat-body">
            <div class="mc-stat-value">{{ stats.adoptada }}</div>
            <div class="mc-stat-label">Adoptadas</div>
            <div class="mc-stat-desc">Historias felices</div>
          </div>
        </div>
        <div class="mc-stat-card mc-stat-inactiva">
          <div class="mc-stat-body">
            <div class="mc-stat-value">{{ stats.inactiva }}</div>
            <div class="mc-stat-label">Inactivas</div>
            <div class="mc-stat-desc">Ocultas del público</div>
          </div>
        </div>
      </div>

      <!-- ── Toolbar (filtros) ── -->
      <div class="mc-toolbar">
        <div class="mc-tabs-row">
          <!-- Tipo -->
          <div class="mc-filter-group">
            <span class="mc-filter-label">Tipo</span>
            <div class="mc-tabs">
              <button
                v-for="t in TYPE_TABS"
                :key="t"
                class="mc-tab"
                :class="{ active: filterType === t }"
                @click="filterType = t"
              >{{ t }}</button>
            </div>
          </div>
          <!-- Estado -->
          <div class="mc-filter-group">
            <span class="mc-filter-label">Estado</span>
            <div class="mc-tabs">
              <button
                v-for="s in STATUS_TABS"
                :key="s"
                class="mc-tab"
                :class="{ active: filterStatus === s }"
                @click="filterStatus = s"
              >{{ s }}</button>
            </div>
          </div>
        </div>

        <div class="mc-filters">
          <div class="mc-search-wrap">
            <svg class="mc-search-icon" xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
            <input
              class="mc-search"
              v-model="searchQuery"
              placeholder="Buscar por ID o nombre..."
            />
          </div>
          <button
            v-if="hayFiltros"
            class="mc-clear"
            @click="searchQuery = ''; filterStatus = 'Todos'; filterType = 'Todos'"
          >Limpiar</button>
        </div>
      </div>

      <!-- ── Tabla ── -->
      <div class="mc-table-wrap">
        <table class="mc-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Foto</th>
              <th>Nombre</th>
              <th>Tipo</th>
              <th>Estado</th>
              <th>Casa cuna asignada</th>
              <th>Acciones</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="p in filteredPets"
              :key="p.id"
              :class="{ 'mc-row-inactive': p.status === 'Inactiva' }"
            >
              <!-- ID -->
              <td>
                <span class="mc-id-badge">{{ p.id }}</span>
              </td>

              <!-- Foto -->
              <td>
                <div class="mc-avatar">
                  <img
                    v-if="p.images?.length > 0"
                    :src="p.images[0].preview"
                    class="mc-avatar-img"
                    :alt="p.name"
                  />
                  <span v-else class="mc-avatar-ini">{{ p.name?.charAt(0) }}</span>
                </div>
              </td>

              <!-- Nombre -->
              <td>
                <div class="mc-pet-info">
                  <span class="mc-pet-name">{{ p.name }}</span>
                  <span class="mc-pet-sub">{{ p.breed }}</span>
                </div>
              </td>

              <!-- Tipo -->
              <td>
                <span class="mc-type-chip">{{ p.type }}</span>
              </td>

              <!-- Estado -->
              <td>
                <span class="mc-badge" :class="statusBadgeClass(p.status)">{{ p.status }}</span>
              </td>

              <!-- Casa cuna -->
              <td class="mc-td-sec">{{ getNombreCasaCuna(p) }}</td>

              <!-- Acciones -->
              <td>
                <div class="mc-action-group">
                  <button class="mc-act-btn" @click="openView(p)" title="Ver mascota">
                    <Icon name="Eye" />
                  </button>
                  <button class="mc-act-btn" @click="openEdit(p)" title="Editar">
                    <Icon name="Edit" />
                  </button>
                  <button class="mc-act-btn" @click="openRequests(p)" title="Ver solicitudes">
                    <Icon name="Clipboard" />
                  </button>
                  <button
                    class="mc-act-btn"
                    @click="toggleActive(p)"
                    :title="p.status === 'Inactiva' ? 'Activar mascota' : 'Desactivar mascota'"
                  >
                    <Icon :name="p.status === 'Inactiva' ? 'CheckCircle' : 'Archive'" />
                  </button>
                </div>
              </td>
            </tr>

            <tr v-if="filteredPets.length === 0">
              <td colspan="7" class="mc-empty">
                <div class="mc-empty-inner">
                  <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M20 7H4a2 2 0 0 0-2 2v10a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2V9a2 2 0 0 0-2-2z"/><path d="M16 3l-4 4-4-4"/></svg>
                  <p>{{ hayFiltros ? 'Sin resultados para los filtros aplicados' : 'No hay mascotas registradas aún' }}</p>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

    </div><!-- /v-if="!showForm" -->


    <!-- ══════════════════════════════════════
         MODAL: Ver mascota
    ══════════════════════════════════════ -->
    <Teleport to="body">
      <Transition name="mc-overlay-anim">
        <div v-if="showViewModal && viewTarget" class="mc-overlay" @click.self="showViewModal = false">
          <div class="mc-modal mc-modal--lg">

            <div class="mc-modal-header">
              <div>
                <p class="mc-modal-eyebrow">Detalle de mascota</p>
                <h2 class="mc-modal-title">{{ viewTarget.name }}</h2>
              </div>
              <button class="mc-modal-close" @click="showViewModal = false">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
              </button>
            </div>

            <div class="mc-modal-body">

              <!-- Pet header -->
              <div class="mc-ver-pet">
                <div class="mc-avatar mc-avatar--lg">
                  <img
                    v-if="viewTarget.images?.length > 0"
                    :src="viewTarget.images[0].preview"
                    class="mc-avatar-img"
                    :alt="viewTarget.name"
                  />
                  <span v-else class="mc-avatar-ini">{{ viewTarget.name?.charAt(0) }}</span>
                </div>
                <div>
                  <p class="mc-ver-pname">{{ viewTarget.name }}</p>
                  <p class="mc-ver-pspec">{{ viewTarget.breed }}</p>
                </div>
                <div class="mc-ver-badges">
                  <span class="mc-id-badge">{{ viewTarget.id }}</span>
                  <span class="mc-badge" :class="statusBadgeClass(viewTarget.status)">{{ viewTarget.status }}</span>
                </div>
              </div>

              <!-- Info grid -->
              <div class="mc-ver-card">
                <div class="mc-ver-grid">
                  <div class="mc-ver-item">
                    <span class="mc-ver-label">Tipo</span>
                    <span class="mc-ver-val">{{ viewTarget.type }}</span>
                  </div>
                  <div class="mc-ver-item">
                    <span class="mc-ver-label">Edad</span>
                    <span class="mc-ver-val">{{ viewTarget.age }}</span>
                  </div>
                  <div class="mc-ver-item">
                    <span class="mc-ver-label">Sexo</span>
                    <span class="mc-ver-val">{{ viewTarget.sex }}</span>
                  </div>
                  <div class="mc-ver-item">
                    <span class="mc-ver-label">Tamaño</span>
                    <span class="mc-ver-val">{{ viewTarget.size }}</span>
                  </div>
                  <div class="mc-ver-item">
                    <span class="mc-ver-label">Salud básica</span>
                    <span class="mc-ver-val">{{ viewTarget.healthBasic }}</span>
                  </div>
                  <div class="mc-ver-item">
                    <span class="mc-ver-label">Casa cuna</span>
                    <span class="mc-ver-val">{{ getNombreCasaCuna(viewTarget) }}</span>
                  </div>
                  <div v-if="viewTarget.personality" class="mc-ver-item">
                    <span class="mc-ver-label">Personalidad</span>
                    <span class="mc-ver-val">{{ viewTarget.personality }}</span>
                  </div>
                </div>

                <div v-if="viewTarget.description" class="mc-ver-row mc-ver-row--block" style="margin-top:12px">
                  <span class="mc-ver-label">Descripción pública</span>
                  <span class="mc-ver-val" style="line-height:1.5">{{ viewTarget.description }}</span>
                </div>

                <div v-if="viewTarget.internalNotes" class="mc-ver-private" style="margin-top:12px">
                  <span class="mc-ver-label">🔒 Notas internas</span>
                  <span class="mc-ver-val" style="line-height:1.5">{{ viewTarget.internalNotes }}</span>
                </div>
              </div>

              <!-- Galería adicional -->
              <div v-if="viewTarget.images?.length > 1" class="mc-gallery" style="margin-top:16px">
                <img
                  v-for="(img, i) in viewTarget.images.slice(1)"
                  :key="i"
                  :src="img.preview"
                  :alt="viewTarget.name"
                  class="mc-gallery-img"
                />
              </div>

            </div>

            <div class="mc-modal-footer">
              <button class="mc-btn-cancel-modal" @click="showViewModal = false">Cerrar</button>
              <button class="mc-btn-save" @click="openEdit(viewTarget)">
                <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                Editar
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
      <Transition name="mc-overlay-anim">
        <div v-if="showStatusModal" class="mc-overlay" @click.self="showStatusModal = false">
          <div class="mc-modal mc-modal--sm">
            <div class="mc-modal-header">
              <div>
                <p class="mc-modal-eyebrow">Cambiar estado</p>
                <h2 class="mc-modal-title">{{ statusTargetPet?.name }}</h2>
              </div>
              <button class="mc-modal-close" @click="showStatusModal = false">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
              </button>
            </div>
            <div class="mc-modal-body">
              <div class="mc-status-options">
                <label
                  v-for="s in STATUS_OPTIONS"
                  :key="s"
                  class="mc-status-option"
                  :class="{ selected: pendingStatus === s }"
                >
                  <input type="radio" :value="s" v-model="pendingStatus" />
                  <span class="mc-badge" :class="statusBadgeClass(s)">{{ s }}</span>
                  <span class="mc-status-desc">
                    <template v-if="s === 'Disponible'">Visible en catálogo, acepta solicitudes</template>
                    <template v-else-if="s === 'En proceso'">Visible, evaluando solicitudes</template>
                    <template v-else-if="s === 'Adoptada'">Se mueve a «Historias felices»</template>
                    <template v-else-if="s === 'Inactiva'">Oculta del público, historial conservado</template>
                  </span>
                </label>
              </div>
            </div>
            <div class="mc-modal-footer">
              <button class="mc-btn-cancel-modal" @click="showStatusModal = false">Cancelar</button>
              <button class="mc-btn-save" @click="confirmStatusChange">
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
      <Transition name="mc-overlay-anim">
        <div v-if="showDeactivateModal" class="mc-overlay" @click.self="showDeactivateModal = false">
          <div class="mc-modal mc-modal--sm">
            <div class="mc-modal-header">
              <div>
                <p class="mc-modal-eyebrow">Desactivar mascota</p>
                <h2 class="mc-modal-title">{{ deactivateTarget?.name }}</h2>
              </div>
              <button class="mc-modal-close" @click="showDeactivateModal = false">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
              </button>
            </div>
            <div class="mc-modal-body">
              <div class="mc-info-box">
                <p>🔒 <strong>No se eliminará</strong> del sistema. Se conservará todo su historial, solicitudes y registros.</p>
                <p>La mascota pasará a estado <strong>Inactiva</strong> y dejará de ser visible en el catálogo público.</p>
              </div>
            </div>
            <div class="mc-modal-footer">
              <button class="mc-btn-cancel-modal" @click="showDeactivateModal = false">Cancelar</button>
              <button class="mc-btn-danger" @click="confirmDeactivate">Desactivar</button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>


    <!-- ══════════════════════════════════════
         MODAL: Solicitudes de adopción
    ══════════════════════════════════════ -->
    <Teleport to="body">
      <Transition name="mc-overlay-anim">
        <div v-if="showRequestsModal" class="mc-overlay" @click.self="showRequestsModal = false">
          <div class="mc-modal mc-modal--lg">
            <div class="mc-modal-header">
              <div>
                <p class="mc-modal-eyebrow">Solicitudes de adopción</p>
                <h2 class="mc-modal-title">{{ requestsTarget?.name }}</h2>
              </div>
              <button class="mc-modal-close" @click="showRequestsModal = false">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
              </button>
            </div>

            <div class="mc-modal-body">
              <div v-if="solicitudesMascota.length" class="mc-requests-list">
                <div v-for="s in solicitudesMascota" :key="s.id" class="mc-request-card">
                  <div class="mc-request-header">
                    <div class="mc-avatar mc-avatar--md">
                      <span class="mc-avatar-ini">{{ s.solicitante?.substring(0,1).toUpperCase() }}</span>
                    </div>
                    <div class="mc-request-user">
                      <h4>{{ s.solicitante }}</h4>
                      <span class="mc-id-badge">{{ s.id }}</span>
                    </div>
                    <span
                      class="mc-req-status"
                      :class="{
                        'rs-approved': s.estado === 'Aprobada',
                        'rs-pending':  s.estado === 'Pendiente',
                        'rs-process':  s.estado === 'En proceso',
                        'rs-rejected': s.estado === 'Rechazada',
                      }"
                    >{{ s.estado }}</span>
                  </div>
                  <div class="mc-ver-grid" style="margin-top:12px">
                    <div class="mc-ver-item">
                      <span class="mc-ver-label">Teléfono</span>
                      <span class="mc-ver-val">{{ s.telefono }}</span>
                    </div>
                    <div class="mc-ver-item">
                      <span class="mc-ver-label">Correo</span>
                      <span class="mc-ver-val">{{ s.email }}</span>
                    </div>
                    <div class="mc-ver-item">
                      <span class="mc-ver-label">Fecha</span>
                      <span class="mc-date-badge">{{ s.fecha }}</span>
                    </div>
                  </div>
                </div>
              </div>

              <div v-else class="mc-empty-inner" style="padding: 40px 20px">
                <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
                <p>Sin solicitudes aún. Las solicitudes del catálogo público aparecerán aquí.</p>
              </div>
            </div>

            <div class="mc-modal-footer">
              <button class="mc-btn-cancel-modal" @click="showRequestsModal = false">Cerrar</button>
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
.mc-root {
  background: transparent;
  padding-bottom: 40px;
}

/* ═══════════════════════════════════════
   TOAST
═══════════════════════════════════════ */
.mc-toast {
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
.mc-toast.success { background: #3A473C; color: #fff; }
.mc-toast.error   { background: #c0392b; color: #fff; }
.mc-toast-dot {
  width: 8px; height: 8px;
  border-radius: 50%;
  background: rgba(255,255,255,0.5);
  flex-shrink: 0;
}
.mc-toast-anim-enter-active, .mc-toast-anim-leave-active { transition: all 0.25s ease; }
.mc-toast-anim-enter-from, .mc-toast-anim-leave-to { opacity: 0; transform: translateY(10px); }

/* ═══════════════════════════════════════
   HEADER
═══════════════════════════════════════ */
.mc-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 28px;
  gap: 16px;
  flex-wrap: wrap;
}
.mc-title {
  font-size: 28px;
  font-weight: 800;
  color: #3A473C;
  letter-spacing: -0.5px;
  line-height: 1.1;
}
.mc-sub {
  font-size: 14px;
  color: #6C756D;
  margin-top: 5px;
  font-weight: 500;
}
.mc-btn-new {
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
.mc-btn-new:hover { background: #2d3730; }
.mc-btn-new:active { transform: scale(0.97); }

.mc-btn-cancel {
  display: flex;
  align-items: center;
  gap: 7px;
  padding: 11px 20px;
  background: #F4F6F4;
  color: #6C756D;
  border: none;
  border-radius: 12px;
  font-size: 14px;
  font-weight: 700;
  cursor: pointer;
  transition: background 0.18s;
  white-space: nowrap;
  flex-shrink: 0;
}
.mc-btn-cancel:hover { background: #E5EAE6; color: #3A473C; }

/* ═══════════════════════════════════════
   ESTADÍSTICAS
═══════════════════════════════════════ */
.mc-stats-grid {
  display: grid;
  grid-template-columns: repeat(5, 1fr);
  gap: 14px;
  margin-bottom: 24px;
}
.mc-stat-card {
  background: white;
  border-radius: 18px;
  padding: 20px;
  border: 1.5px solid transparent;
  transition: box-shadow 0.2s;
}
.mc-stat-card:hover { box-shadow: 0 4px 20px rgba(0,0,0,0.06); }
.mc-stat-value {
  font-size: 26px;
  font-weight: 800;
  color: #2D3A2E;
  line-height: 1;
  margin-bottom: 4px;
}
.mc-stat-label {
  font-size: 13px;
  font-weight: 700;
  color: #2D3A2E;
  margin-bottom: 2px;
}
.mc-stat-desc { font-size: 12px; color: #9AA89C; }

.mc-stat-total     { border-color: rgba(146,168,148,.3); background: rgba(146,168,148,.06); }
.mc-stat-disponible{ border-color: rgba(146,168,148,.3); background: rgba(146,168,148,.06); }
.mc-stat-proceso   { border-color: rgba(249,193,122,.3); background: rgba(249,193,122,.06); }
.mc-stat-adoptada  { border-color: rgba(130,160,180,.25); background: rgba(130,160,180,.06); }
.mc-stat-inactiva  { border-color: #EAEEEA; }

/* ═══════════════════════════════════════
   TOOLBAR (tabs + filtros)
═══════════════════════════════════════ */
.mc-toolbar {
  background: #fff;
  border-radius: 18px;
  padding: 18px 20px;
  margin-bottom: 20px;
  border: 1px solid #E8ECE8;
  display: flex;
  flex-direction: column;
  gap: 14px;
}
.mc-tabs-row {
  display: flex;
  align-items: center;
  gap: 20px;
  flex-wrap: wrap;
}
.mc-filter-group {
  display: flex;
  align-items: center;
  gap: 10px;
}
.mc-filter-label {
  font-size: 12px;
  font-weight: 700;
  color: #3A473C;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  white-space: nowrap;
}
.mc-tabs {
  display: flex;
  gap: 4px;
  background: #F4F6F4;
  border-radius: 12px;
  padding: 4px;
}
.mc-tab {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 14px;
  border-radius: 9px;
  border: none;
  background: transparent;
  color: #6C756D;
  font-size: 13px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.18s;
  white-space: nowrap;
  font-family: inherit;
}
.mc-tab:hover { color: #3A473C; background: rgba(255,255,255,0.6); }
.mc-tab.active { background: #fff; color: #3A473C; box-shadow: 0 1px 4px rgba(58,71,60,0.12); }

.mc-filters {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
}
.mc-search-wrap {
  position: relative;
  flex: 1;
  min-width: 200px;
}
.mc-search-icon {
  position: absolute;
  left: 12px;
  top: 50%;
  transform: translateY(-50%);
  color: #92A894;
  pointer-events: none;
}
.mc-search {
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
.mc-search:focus { border-color: #92A894; }
.mc-clear {
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
  font-family: inherit;
}
.mc-clear:hover { background: #ffe5e5; }

/* ═══════════════════════════════════════
   TABLA
═══════════════════════════════════════ */
.mc-table-wrap {
  background: #fff;
  border-radius: 20px;
  box-shadow: 0 2px 16px rgba(58,71,60,0.06);
  overflow: hidden;
}
.mc-table {
  width: 100%;
  border-collapse: collapse;
}
.mc-table thead { background: #F9FAF9; }
.mc-table th {
  padding: 14px 20px;
  font-size: 11px;
  font-weight: 800;
  color: #92A894;
  text-transform: uppercase;
  letter-spacing: 0.6px;
  white-space: nowrap;
  border-bottom: 1.5px solid #F0F2F0;
  text-align: center;
}
.mc-table td {
  padding: 14px 20px;
  font-size: 14px;
  color: #3A473C;
  border-bottom: 1px solid #F5F7F5;
  vertical-align: middle;
  text-align: center;
}
.mc-table tbody tr:last-child td { border-bottom: none; }
.mc-table tbody tr { transition: background 0.12s; }
.mc-table tbody tr:hover { background: #FAFBFA; }
.mc-row-inactive { opacity: 0.5; }

/* ── Celda foto / avatar ── */
.mc-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  overflow: hidden;
  flex-shrink: 0;
  background: #DDE6DE;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 1.5px solid #EEF3EE;
}
.mc-avatar--md { width: 40px; height: 40px; }
.mc-avatar--lg { width: 56px; height: 56px; }
.mc-avatar-img { width: 100%; height: 100%; object-fit: cover; display: block; }
.mc-avatar-ini {
  font-size: 15px;
  font-weight: 800;
  color: #5A6E5C;
  text-transform: uppercase;
  line-height: 1;
}
.mc-avatar--lg .mc-avatar-ini { font-size: 20px; }

/* ── Nombre + raza ── */
.mc-pet-info   { display: flex; flex-direction: column; gap: 2px; }
.mc-pet-name   { font-weight: 700; font-size: 14px; color: #3A473C; }
.mc-pet-sub    { font-size: 11px; color: #92A894; }

/* ── Chips y badges ── */
.mc-type-chip {
  font-size: 12px;
  font-weight: 600;
  color: #5A6E5C;
  background: rgba(146,168,148,.12);
  padding: 3px 10px;
  border-radius: 7px;
  white-space: nowrap;
}

.mc-badge {
  padding: 5px 11px;
  border-radius: 9px;
  font-size: 12px;
  font-weight: 700;
  display: inline-block;
  white-space: nowrap;
}
.mc-badge-green  { background: rgba(146,168,148,.18); color: #4A6E4C; }
.mc-badge-peach  { background: rgba(249,193,122,.2);  color: #C88A30; }
.mc-badge-blue   { background: rgba(130,160,180,.15); color: #4A6070; }
.mc-badge-gray   { background: #F0F4F0;               color: #7A847C; }
.mc-badge-orange { background: rgba(230,150,80,.15);  color: #9A5420; }

.mc-id-badge {
  background: #EEF2EE;
  color: #5A6E5C;
  padding: 4px 10px;
  border-radius: 7px;
  font-size: 12px;
  font-weight: 700;
  font-family: monospace;
  white-space: nowrap;
}

.mc-date-badge {
  display: inline-block;
  padding: 4px 10px;
  background: #F0F4F0;
  color: #4A6550;
  border-radius: 7px;
  font-size: 12px;
  font-weight: 600;
  white-space: nowrap;
}

.mc-td-sec { color: #7A8A7C; font-size: 13px; }

/* ── Acciones ── */
.mc-action-group {
  display: flex;
  gap: 5px;
  justify-content: center;
}
.mc-act-btn {
  width: 32px; height: 32px;
  border-radius: 9px;
  border: 1.5px solid #EAEEEA;
  background: transparent;
  color: #7A847C;
  cursor: pointer;
  display: inline-flex; align-items: center; justify-content: center;
  transition: all 0.15s;
  flex-shrink: 0;
}
.mc-act-btn:hover {
  background: #F5F8F5;
  border-color: #C8D4C8;
  color: #2D3A2E;
  transform: translateY(-1px);
}
.mc-act-btn:active { transform: translateY(0); }

/* ── Empty ── */
.mc-empty { padding: 0; }
.mc-empty-inner {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  padding: 56px 24px;
  color: #92A894;
}
.mc-empty-inner svg { opacity: 0.4; }
.mc-empty-inner p {
  font-size: 14px;
  font-weight: 500;
  color: #7A8A7C;
  margin: 0;
  text-align: center;
}

/* ═══════════════════════════════════════
   FORMULARIO — pantalla independiente
═══════════════════════════════════════ */
.mc-form-screen {
  display: flex;
  flex-direction: column;
}
.mc-form-panel {
  background: #fff;
  border-radius: 20px;
  padding: 28px;
  box-shadow: 0 2px 16px rgba(58,71,60,0.06);
  border: 1px solid #E8ECE8;
  margin-bottom: 16px;
}
.mc-form-footer {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  padding: 18px 28px 4px;
}

/* Sección label (igual que Salud) */
.mc-section-label {
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
.mc-section-num {
  width: 24px; height: 24px;
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
.mc-private-badge {
  font-size: 11px;
  font-weight: 600;
  color: #C88A37;
  background: rgba(249,193,122,.15);
  padding: 2px 8px;
  border-radius: 6px;
  text-transform: none;
  letter-spacing: 0;
}

/* Grid del formulario */
.mc-form-grid {
  display: grid;
  gap: 14px;
}
.mc-form-grid--4 { grid-template-columns: repeat(4, 1fr); }
.mc-fg { display: flex; flex-direction: column; gap: 6px; }
.mc-fg--span2 { grid-column: span 2; }
.mc-fg--full  { grid-column: 1 / -1; }

.mc-fg label {
  font-size: 12px;
  font-weight: 700;
  color: #5A6E5C;
  letter-spacing: 0.1px;
}
.mc-req { color: #c0392b; }

.mc-input {
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
  appearance: none;
  -webkit-appearance: none;
}
.mc-input:focus { border-color: #92A894; background: #fff; }
.mc-input.is-error { border-color: #e57373; background: #fff8f8; }

.mc-textarea {
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
  height: 88px;
  resize: vertical;
  line-height: 1.5;
}
.mc-textarea:focus { border-color: #92A894; background: #fff; }
.mc-textarea--private {
  background: #FFFBF3;
  border-color: rgba(249,193,122,.3);
}
.mc-textarea--private:focus { border-color: #F9C17A; }

.mc-err-msg {
  font-size: 11px;
  color: #c0392b;
  font-weight: 600;
  margin: 0;
}

/* Imágenes */
.mc-upload-zone {
  border: 2px dashed #D0D9D1;
  border-radius: 14px;
  padding: 28px;
  text-align: center;
  cursor: pointer;
  transition: all 0.2s;
  background: #FAFCFA;
  margin-bottom: 8px;
}
.mc-upload-zone:hover { border-color: #92A894; background: #F2F7F2; }
.mc-upload-zone.is-error { border-color: #e57373; background: #fff8f8; }
.mc-upload-icon   { font-size: 32px; margin-bottom: 8px; }
.mc-upload-title  { font-size: 14px; font-weight: 700; color: #2D3A2E; margin: 0 0 4px; }
.mc-upload-sub    { font-size: 13px; color: #9AA89C; margin: 0; }

.mc-image-previews {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}
.mc-image-preview-item {
  position: relative;
  width: 88px; height: 88px;
  border-radius: 12px;
  overflow: hidden;
  border: 1.5px solid #EEF3EE;
}
.mc-image-preview-item img { width: 100%; height: 100%; object-fit: cover; display: block; }
.mc-remove-image-btn {
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
.mc-remove-image-btn:hover { background: rgba(180,40,40,.8); }
.mc-main-photo-label {
  position: absolute;
  bottom: 0; left: 0; right: 0;
  background: rgba(45,58,46,.75);
  color: white;
  font-size: 10px;
  font-weight: 700;
  text-align: center;
  padding: 3px 0;
}
.mc-add-more-btn {
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
.mc-add-more-btn:hover { border-color: #92A894; background: #F2F7F2; color: #3A473C; }
.mc-add-more-icon { font-size: 22px; line-height: 1; }

/* Nota inmutable */
.mc-immutable-note {
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
.mc-immutable-note svg { flex-shrink: 0; margin-top: 1px; }

/* Botones de acción del form */
.mc-btn-save {
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
  font-family: inherit;
}
.mc-btn-save:hover { background: #2d3730; }

.mc-btn-cancel-ghost {
  padding: 10px 18px;
  background: #F4F6F4;
  border: none;
  border-radius: 10px;
  font-size: 13px;
  font-weight: 700;
  color: #6C756D;
  cursor: pointer;
  transition: background 0.15s;
  font-family: inherit;
}
.mc-btn-cancel-ghost:hover { background: #E5EAE6; color: #3A473C; }

/* Transición formulario */
.mc-slide-enter-active { transition: all 0.28s cubic-bezier(.4,0,.2,1); }
.mc-slide-leave-active { transition: all 0.18s ease; }
.mc-slide-enter-from   { opacity: 0; transform: translateY(-10px); }
.mc-slide-leave-to     { opacity: 0; transform: translateY(-6px); }

/* ═══════════════════════════════════════
   OVERLAY / MODAL
═══════════════════════════════════════ */
.mc-overlay {
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
.mc-overlay-anim-enter-active, .mc-overlay-anim-leave-active { transition: all 0.22s ease; }
.mc-overlay-anim-enter-from, .mc-overlay-anim-leave-to { opacity: 0; }
.mc-overlay-anim-enter-from .mc-modal,
.mc-overlay-anim-leave-to   .mc-modal { transform: translateY(16px) scale(0.98); }

.mc-modal {
  background: #fff;
  border-radius: 22px;
  width: 100%;
  max-height: 88vh;
  overflow-y: auto;
  box-shadow: 0 24px 80px rgba(0,0,0,0.2);
  transition: transform 0.22s ease;
}
.mc-modal--sm { max-width: 420px; }
.mc-modal--md { max-width: 520px; }
.mc-modal--lg { max-width: 680px; }

.mc-modal-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  padding: 24px 28px 18px;
  border-bottom: 1.5px solid #F0F2F0;
}
.mc-modal-eyebrow {
  font-size: 11px;
  font-weight: 800;
  color: #92A894;
  text-transform: uppercase;
  letter-spacing: 0.7px;
  margin-bottom: 4px;
}
.mc-modal-title {
  font-size: 20px;
  font-weight: 800;
  color: #3A473C;
  letter-spacing: -0.4px;
}
.mc-modal-close {
  width: 34px; height: 34px;
  border-radius: 10px;
  border: 1.5px solid #E8ECE8;
  background: #fff;
  color: #6C756D;
  cursor: pointer;
  display: flex; align-items: center; justify-content: center;
  transition: background 0.15s, border-color 0.15s;
  flex-shrink: 0;
  font-family: inherit;
}
.mc-modal-close:hover { background: #F4F6F4; border-color: #ccc; }

.mc-modal-body { padding: 24px 28px 8px; }
.mc-modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  padding: 18px 28px 24px;
  border-top: 1.5px solid #F0F2F0;
  margin-top: 12px;
}
.mc-btn-cancel-modal {
  padding: 10px 18px;
  background: #F4F6F4;
  border: none;
  border-radius: 10px;
  font-size: 13px;
  font-weight: 700;
  color: #6C756D;
  cursor: pointer;
  transition: background 0.15s;
  font-family: inherit;
}
.mc-btn-cancel-modal:hover { background: #E5EAE6; }

.mc-btn-danger {
  padding: 10px 18px;
  background: #C05050;
  border: none;
  border-radius: 10px;
  font-size: 13px;
  font-weight: 700;
  color: #fff;
  cursor: pointer;
  transition: background 0.18s;
  font-family: inherit;
}
.mc-btn-danger:hover { background: #A03030; }

/* ── Modal Ver ── */
.mc-ver-pet {
  display: flex;
  align-items: center;
  gap: 14px;
  padding-bottom: 18px;
  margin-bottom: 18px;
  border-bottom: 1.5px solid #F0F2F0;
}
.mc-ver-pname { font-size: 17px; font-weight: 800; color: #3A473C; }
.mc-ver-pspec { font-size: 12px; color: #92A894; margin-top: 2px; }
.mc-ver-badges {
  margin-left: auto;
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 6px;
}
.mc-ver-card {
  background: #F9FAF9;
  border-radius: 14px;
  padding: 18px;
}
.mc-ver-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 10px;
}
.mc-ver-item {
  background: #fff;
  border-radius: 10px;
  padding: 10px 12px;
  border: 1px solid #EEF3EE;
  display: flex;
  flex-direction: column;
  gap: 3px;
}
.mc-ver-label {
  font-size: 11px;
  font-weight: 800;
  color: #92A894;
  text-transform: uppercase;
  letter-spacing: 0.4px;
}
.mc-ver-val {
  font-size: 13px;
  font-weight: 600;
  color: #3A473C;
}
.mc-ver-row {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-top: 12px;
}
.mc-ver-row--block { align-items: flex-start; flex-direction: column; }

.mc-ver-private {
  background: #FFFBF3;
  border-radius: 10px;
  padding: 12px 14px;
  border: 1px solid rgba(249,193,122,.3);
  display: flex;
  flex-direction: column;
  gap: 6px;
}

/* Galería */
.mc-gallery {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}
.mc-gallery-img {
  width: 72px; height: 72px;
  border-radius: 10px;
  object-fit: cover;
  border: 1.5px solid #EEF3EE;
}

/* ── Modal Estado ── */
.mc-status-options {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 8px;
}
.mc-status-option {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 11px 14px;
  border-radius: 12px;
  border: 1.5px solid #EEF3EE;
  cursor: pointer;
  transition: all 0.15s;
}
.mc-status-option input[type="radio"] { display: none; }
.mc-status-option:hover { border-color: #92A894; background: #FAFCFA; }
.mc-status-option.selected { border-color: #92A894; background: rgba(146,168,148,.07); }
.mc-status-desc { font-size: 13px; color: #9AA89C; }

/* ── Modal Info box ── */
.mc-info-box {
  background: #F7FAF7;
  border-radius: 12px;
  padding: 16px;
  font-size: 13px;
  color: #2D3A2E;
  line-height: 1.7;
  border: 1px solid #EEF3EE;
}
.mc-info-box p { margin: 0 0 6px; }
.mc-info-box p:last-child { margin: 0; }

/* ── Modal Solicitudes ── */
.mc-requests-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}
.mc-request-card {
  border: 1.5px solid #EEF3EE;
  border-radius: 14px;
  padding: 16px;
  transition: box-shadow 0.15s;
}
.mc-request-card:hover { box-shadow: 0 4px 16px rgba(0,0,0,0.06); }
.mc-request-header {
  display: flex;
  align-items: center;
  gap: 12px;
}
.mc-request-user { flex: 1; display: flex; flex-direction: column; gap: 3px; }
.mc-request-user h4 { margin: 0; font-size: 14px; font-weight: 700; color: #2D3A2E; }
.mc-req-status {
  padding: 4px 11px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 700;
  flex-shrink: 0;
}
.rs-pending  { background: rgba(249,193,122,.15); color: #C88A37; }
.rs-process  { background: rgba(110,155,255,.12); color: #4F73B8; }
.rs-approved { background: rgba(146,168,148,.15); color: #4A6E4C; }
.rs-rejected { background: rgba(208,96,96,.1);   color: #B04040; }

/* ═══════════════════════════════════════
   RESPONSIVE
═══════════════════════════════════════ */
@media (max-width: 1200px) {
  .mc-stats-grid { grid-template-columns: repeat(3, 1fr); }
}
@media (max-width: 900px) {
  .mc-stats-grid { grid-template-columns: repeat(2, 1fr); }
  .mc-form-grid--4 { grid-template-columns: repeat(2, 1fr); }
  .mc-fg--span2 { grid-column: span 1; }
  .mc-ver-grid { grid-template-columns: repeat(2, 1fr); }
}
@media (max-width: 640px) {
  .mc-header     { flex-direction: column; align-items: flex-start; }
  .mc-stats-grid { grid-template-columns: 1fr 1fr; gap: 10px; }
  .mc-toolbar    { padding: 14px 16px; }
  .mc-tabs-row   { flex-direction: column; align-items: flex-start; gap: 12px; }
  .mc-form-grid--4 { grid-template-columns: 1fr; }
  .mc-fg--span2,
  .mc-fg--full   { grid-column: 1; }
  .mc-table th:nth-child(5),
  .mc-table td:nth-child(5),
  .mc-table th:nth-child(6),
  .mc-table td:nth-child(6) { display: none; }
  .mc-modal-body  { padding: 16px 18px 8px; }
  .mc-modal-header,
  .mc-modal-footer { padding-left: 18px; padding-right: 18px; }
  .mc-ver-grid { grid-template-columns: 1fr 1fr; }
}
@media (max-width: 480px) {
  .mc-stats-grid { grid-template-columns: 1fr; }
  .mc-tabs { flex-wrap: wrap; }
  .mc-tab  { padding: 6px 10px; font-size: 12px; }
}
</style>