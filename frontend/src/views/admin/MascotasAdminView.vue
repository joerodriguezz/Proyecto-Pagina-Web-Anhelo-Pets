<script setup>
import { ref, computed } from 'vue'
import Icon from '../../components/Icon.vue'
import { usePetsStore } from '../../stores/usePetsStore'


const store = usePetsStore()

// ─────────────────────────────────────────────
// Estado del formulario
// ─────────────────────────────────────────────
const showForm      = ref(false)
const editMode      = ref(false)
const editingPetId  = ref(null)

const formData = ref({
  name:        '',
  type:        'Perro',
  breed:       '',
  age:         '',
  sex:         'Macho',
  size:        'Mediano',
  personality: '',
  healthBasic: '',
  status:      'Disponible',
  description: '',
  internalNotes: '',
  images:      [],        // { url: string, file: File, preview: string }[]
  featured:    false,
})

const formErrors = ref({})


// ─────────────────────────────────────────────
// Modal de cambio de estado
// ─────────────────────────────────────────────
const showStatusModal  = ref(false)
const statusTargetPet  = ref(null)
const pendingStatus    = ref('')

// Modal de confirmación de desactivar
const showDeactivateModal = ref(false)
const deactivateTarget    = ref(null)

// Modal de solicitudes (vista previa)
const showRequestsModal = ref(false)
const requestsTarget    = ref(null)

// ─────────────────────────────────────────────
// Filtros de la tabla admin
// ─────────────────────────────────────────────
const filterStatus = ref('Todos')
const filterType   = ref('Todos')
const searchQuery  = ref('')

const STATUS_OPTIONS = ['Disponible', 'En proceso', 'Adoptada', 'Inactiva']

// ─────────────────────────────────────────────
// Colores de badge según estado
// ─────────────────────────────────────────────
const statusBadgeClass = s => ({
  'Disponible': 'badge-green',
  'En proceso': 'badge-peach',
  'Adoptada':   'badge-blue',
  'Inactiva':   'badge-gray',
}[s] || 'badge-gray')



// ─────────────────────────────────────────────
// Lista filtrada para la tabla admin
// ─────────────────────────────────────────────
const filteredPets = computed(() => {
  return store.pets.filter(p => {
    const matchStatus = filterStatus.value === 'Todos' || p.status === filterStatus.value
    const matchType   = filterType.value   === 'Todos' || p.type   === filterType.value
    const matchSearch = !searchQuery.value ||
      p.name.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      p.breed.toLowerCase().includes(searchQuery.value.toLowerCase())
    return matchStatus && matchType && matchSearch
  })
})

// ─────────────────────────────────────────────
// Manejo de imágenes
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
  // limpiar el input para permitir re-subir el mismo archivo
  e.target.value = ''
}

function removeImage(index) {
  formData.value.images.splice(index, 1)
}

// ─────────────────────────────────────────────
// Validación del formulario
// ─────────────────────────────────────────────
function validateForm() {
  const errors = {}
  if (!formData.value.name.trim())        errors.name    = 'El nombre es obligatorio'
  if (!formData.value.breed.trim())       errors.breed   = 'La raza es obligatoria'
  if (!formData.value.age.trim())         errors.age     = 'La edad es obligatoria'
  if (!formData.value.healthBasic.trim()) errors.healthBasic = 'El estado de salud es obligatorio'
  if (formData.value.images.length === 0) errors.images  = 'Debes subir al menos una foto'
  formErrors.value = errors
  return Object.keys(errors).length === 0
}

// ─────────────────────────────────────────────
// Guardar mascota (nueva o edición)
// ─────────────────────────────────────────────
function savePet() {

  if (!validateForm()) return

  const petData = {

    name: formData.value.name,
    type: formData.value.type,
    breed: formData.value.breed,
    age: formData.value.age,
    sex: formData.value.sex,
    size: formData.value.size,

    personality: formData.value.personality,
    healthBasic: formData.value.healthBasic,

    status: formData.value.status,

    description: formData.value.description,
    internalNotes: formData.value.internalNotes,

    images: [...formData.value.images],

    featured: formData.value.featured,
  }

  if (editMode.value && editingPetId.value !== null) {

    store.updatePet(
      editingPetId.value,
      petData
    )

  } else {

    store.addPet(petData)

  }

  closeForm()
}

// ─────────────────────────────────────────────
// Abrir form en modo edición
// ─────────────────────────────────────────────
function openEdit(pet) {
  editMode.value       = true
  editingPetId.value   = pet.id
  formData.value       = {
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
    images:        [...pet.images],
    featured:      pet.featured,
  }
  formErrors.value     = {}
  showForm.value       = true
  // scroll hacia el form
  setTimeout(() => document.querySelector('.form-panel')?.scrollIntoView({ behavior: 'smooth', block: 'start' }), 50)
}

function closeForm() {
  showForm.value      = false
  editMode.value      = false
  editingPetId.value  = null
  formErrors.value    = {}
  formData.value      = {
    name: '', type: 'Perro', breed: '', age: '', sex: 'Macho',
    size: 'Mediano', personality: '', healthBasic: '', status: 'Disponible',
    description: '', internalNotes: '', images: [], featured: false,
  }
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

  store.changeStatus(
    statusTargetPet.value.id,
    pendingStatus.value
  )

  showStatusModal.value = false
  statusTargetPet.value = null
}

// ─────────────────────────────────────────────
// Marcar / desmarcar destacada
// ─────────────────────────────────────────────
function toggleFeatured(pet) {
  store.toggleFeatured(pet.id)
}

// ─────────────────────────────────────────────
// Desactivar (cambio a Inactiva — sin eliminar)
// ─────────────────────────────────────────────
function openDeactivate(pet) {
  deactivateTarget.value    = pet
  showDeactivateModal.value = true
}

function confirmDeactivate() {

  if (!deactivateTarget.value) return

  store.deactivatePet(
    deactivateTarget.value.id
  )

  showDeactivateModal.value = false
  deactivateTarget.value = null
}

// ─────────────────────────────────────────────
// Ver solicitudes
// ─────────────────────────────────────────────
function openRequests(pet) {
  requestsTarget.value    = pet
  showRequestsModal.value = true
}
</script>

<template>
  <div class="view-container">

    <!-- ══════════════════════════════════════
         CABECERA
    ══════════════════════════════════════ -->
    <header class="page-header">
      <div>
        <h1 class="admin-page-title">Gestión de Mascotas</h1>
        <p class="admin-page-sub">Registro y control de animales de la fundación</p>
      </div>
      <button
        class="btn-toggle-form"
        :class="{ 'btn-cancel': showForm }"
        @click="showForm ? closeForm() : (showForm = true)"
      >
        {{ showForm ? 'Cancelar' : 'Nueva mascota' }}
      </button>
    </header>

    <!-- ══════════════════════════════════════
         FORMULARIO
    ══════════════════════════════════════ -->
    <Transition name="slide-down">
      <div v-if="showForm" class="form-panel">
        <h3>{{ editMode ? 'Editar mascota' : 'Registrar nueva mascota' }}</h3>

        <div class="form-grid">
          <!-- Nombre -->
          <div class="form-group">
            <label>Nombre <span class="required">*</span></label>
            <input v-model="formData.name" placeholder="Nombre de la mascota" class="custom-input" :class="{ 'input-error': formErrors.name }" />
            <span v-if="formErrors.name" class="error-msg">{{ formErrors.name }}</span>
          </div>
          <!-- Tipo -->
          <div class="form-group">
            <label>Tipo <span class="required">*</span></label>
            <select v-model="formData.type" class="custom-select">
              <option>Perro</option>
              <option>Gato</option>
            </select>
          </div>
          <!-- Raza -->
          <div class="form-group">
            <label>Raza <span class="required">*</span></label>
            <input v-model="formData.breed" placeholder="Raza" class="custom-input" :class="{ 'input-error': formErrors.breed }" />
            <span v-if="formErrors.breed" class="error-msg">{{ formErrors.breed }}</span>
          </div>
          <!-- Edad -->
          <div class="form-group">
            <label>Edad <span class="required">*</span></label>
            <input v-model="formData.age" placeholder="Ej. 2 años" class="custom-input" :class="{ 'input-error': formErrors.age }" />
            <span v-if="formErrors.age" class="error-msg">{{ formErrors.age }}</span>
          </div>
          <!-- Sexo -->
          <div class="form-group">
            <label>Sexo <span class="required">*</span></label>
            <select v-model="formData.sex" class="custom-select">
              <option>Macho</option>
              <option>Hembra</option>
            </select>
          </div>
          <!-- Tamaño -->
          <div class="form-group">
            <label>Tamaño</label>
            <select v-model="formData.size" class="custom-select">
              <option>Pequeño</option>
              <option>Mediano</option>
              <option>Grande</option>
            </select>
          </div>
          <!-- Estado -->
          <div class="form-group">
            <label>Estado <span class="required">*</span></label>
            <select v-model="formData.status" class="custom-select">
              <option v-for="s in STATUS_OPTIONS" :key="s">{{ s }}</option>
            </select>
          </div>
          <!-- Salud básica (pública) -->
          <div class="form-group">
            <label>Salud básica <span class="required">*</span></label>
            <input v-model="formData.healthBasic" placeholder="Ej. Vacunado, desparasitado" class="custom-input" :class="{ 'input-error': formErrors.healthBasic }" />
            <span v-if="formErrors.healthBasic" class="error-msg">{{ formErrors.healthBasic }}</span>
          </div>
          <!-- Personalidad -->
          <div class="form-group">
            <label>Personalidad</label>
            <input v-model="formData.personality" placeholder="Ej. Juguetón, tranquilo" class="custom-input" />
          </div>
          <!-- Descripción pública -->
          <div class="form-group full-width">
            <label>Descripción pública</label>
            <textarea v-model="formData.description" placeholder="Descripción visible en el catálogo..." class="custom-textarea"></textarea>
          </div>
          <!-- Notas internas (PRIVADO) -->
          <div class="form-group full-width">
            <label>
              Notas internas
              <span class="private-label">🔒 Solo admin</span>
            </label>
            <textarea v-model="formData.internalNotes" placeholder="Historial médico completo, ubicación exacta, observaciones privadas..." class="custom-textarea textarea-private"></textarea>
          </div>

          <!-- ── Galería de imágenes ── -->
          <div class="form-group full-width">
            <label>
              Fotos de la mascota <span class="required">*</span>
              <span class="label-hint">(mínimo 1 imagen)</span>
            </label>

            <!-- Preview de imágenes cargadas -->
            <div v-if="formData.images.length > 0" class="image-previews">
              <div
                v-for="(img, i) in formData.images"
                :key="i"
                class="image-preview-item"
              >
                <img :src="img.preview" :alt="img.name" />
                <button class="remove-image-btn" @click="removeImage(i)" title="Eliminar foto">×</button>
                <span v-if="i === 0" class="main-photo-label">Principal</span>
              </div>

              <!-- Botón agregar más -->
              <button class="add-more-btn" @click="imageInputRef.click()" title="Agregar más fotos">
                <span class="add-more-icon">+</span>
                <span>Agregar</span>
              </button>
            </div>

            <!-- Zona de drop / upload inicial -->
            <div
              v-else
              class="image-upload-zone"
              :class="{ 'upload-error': formErrors.images }"
              @click="imageInputRef.click()"
            >
              <div class="upload-icon">📷</div>
              <p class="upload-title">Subir fotos de la mascota</p>
              <p class="upload-sub">Haz clic para seleccionar imágenes (JPG, PNG, WebP)</p>
            </div>

            <span v-if="formErrors.images" class="error-msg">{{ formErrors.images }}</span>

            <input
              ref="imageInputRef"
              type="file"
              accept="image/*"
              multiple
              style="display:none"
              @change="handleImageUpload"
            />
          </div>
        </div>

        <div class="form-actions">
          <button class="btn-save" @click="savePet">
            {{ editMode ? 'Guardar cambios' : 'Registrar mascota' }}
          </button>
          <button class="btn-discard" @click="closeForm">Cancelar</button>
        </div>
      </div>
    </Transition>

    <!-- ══════════════════════════════════════
         BARRA DE FILTROS
    ══════════════════════════════════════ -->
    <div class="filters-bar">
      <input v-model="searchQuery" class="filter-input" placeholder="🔍  Buscar por nombre o raza…" />
      <select v-model="filterStatus" class="filter-select">
        <option value="Todos">Todos los estados</option>
        <option v-for="s in STATUS_OPTIONS" :key="s" :value="s">{{ s }}</option>
      </select>
      <select v-model="filterType" class="filter-select">
        <option value="Todos">Todos los tipos</option>
        <option value="Perro">Perros</option>
        <option value="Gato">Gatos</option>
      </select>
      <div class="filters-legend">
        <span class="badge badge-green">Disponible</span>
        <span class="badge badge-peach">En proceso</span>
        <span class="badge badge-blue">Adoptada</span>
        <span class="badge badge-gray">Inactiva</span>
      </div>
    </div>

    <!-- ══════════════════════════════════════
         TABLA DE MASCOTAS
    ══════════════════════════════════════ -->
    <div class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Foto</th>
            <th>Nombre</th>
            <th>Tipo</th>
            <th>Raza</th>
            <th>Edad</th>
            <th>Sexo</th>
            <th>Salud</th>
            <th>Estado</th>
            <th>Destacada</th>
            <th class="text-right">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="filteredPets.length === 0">
            <td colspan="11" class="empty-row">No hay mascotas que coincidan con los filtros</td>
          </tr>
          <tr
            v-for="p in filteredPets"
            :key="p.id"
            :class="{ 'row-inactive': p.status === 'Inactiva' }"
          >
            <td><span class="id-code">{{ p.id }}</span></td>
            <td>
              <div class="pet-thumb-wrap">
                <img
                  v-if="p.images.length > 0"
                  :src="p.images[0].preview"
                  class="pet-thumb"
                  :alt="p.name"
                />
                <div v-else class="pet-thumb-placeholder">🐾</div>
                <span v-if="p.images.length > 1" class="img-count">+{{ p.images.length - 1 }}</span>
              </div>
            </td>
            <td class="font-semibold">{{ p.name }}</td>
            <td>{{ p.type }}</td>
            <td class="text-secondary">{{ p.breed }}</td>
            <td>{{ p.age }}</td>
            <td>{{ p.sex }}</td>
            <td><span class="badge badge-health">{{ p.healthBasic }}</span></td>
            <td>
              <span class="badge" :class="statusBadgeClass(p.status)">{{ p.status }}</span>
            </td>
            <td class="text-center">
              <button
                class="featured-btn"
                :class="{ 'featured-on': p.featured }"
                :disabled="p.status === 'Adoptada' || p.status === 'Inactiva'"
                @click="toggleFeatured(p)"
                :title="p.featured ? 'Quitar de destacadas' : 'Marcar como destacada'"
              >
                ★
              </button>
            </td>
            <td>
              <div class="action-btns">
                <!-- Editar -->
                <button class="action-btn" @click="openEdit(p)" title="Editar mascota">
                  <Icon name="Edit" />
                </button>
                <!-- Cambiar estado -->
                <button class="action-btn status-btn" @click="openStatusModal(p)" title="Cambiar estado">
                  <Icon name="RefreshCw" />
                </button>
                <!-- Ver solicitudes -->
                <button class="action-btn" @click="openRequests(p)" title="Ver solicitudes">
                  <Icon name="Clipboard" />
                </button>
                <!-- Desactivar (no elimina) -->
                <button
                  class="action-btn archive-btn"
                  @click="openDeactivate(p)"
                  :disabled="p.status === 'Inactiva'"
                  title="Desactivar mascota (no se elimina)"
                >
                  <Icon name="Archive" />
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- ══════════════════════════════════════
         INFO: Vista pública (referencia)
    ══════════════════════════════════════ -->
    <section class="visibility-info">
      <h4>Visibilidad en el catálogo público</h4>
      <div class="visibility-grid">
        <div class="vis-card vis-public">
          <strong>Catálogo principal</strong>
          <p>Muestra solo mascotas con estado <em>Disponible</em> o <em>En proceso</em>.</p>
          <p class="vis-count">{{ store.publicPets.length }} mascota{{ store.publicPets.length !== 1 ? 's' : '' }} visible{{ store.publicPets.length !== 1 ? 's' : '' }}</p>
        </div>
        <div class="vis-card vis-featured">
          <strong>Destacadas</strong>
          <p>Máximo 3. Se auto-actualiza si una cambia a Adoptada/Inactiva.</p>
          <p class="vis-count">{{ store.featuredPets.length }} destacada{{ store.featuredPets.length !== 1 ? 's' : '' }}</p>
        </div>
        <div class="vis-card vis-adopted">
          <strong>Historias felices</strong>
          <p>Sección separada para mascotas <em>Adoptadas</em>.</p>
          <p class="vis-count">{{ store.adoptedPets.length }} adoptada{{ store.adoptedPets.length !== 1 ? 's' : '' }}</p>
        </div>
        <div class="vis-card vis-hidden">
          <strong>Ocultas del público</strong>
          <p>Mascotas <em>Inactivas</em> no aparecen en ninguna sección pública, pero conservan todo su historial.</p>
        </div>
      </div>
    </section>

    <!-- ══════════════════════════════════════
         MODAL: Cambiar estado
    ══════════════════════════════════════ -->
    <Transition name="modal-fade">
      <div v-if="showStatusModal" class="modal-overlay" @click.self="showStatusModal = false">
        <div class="modal-box">
          <h3 class="modal-title">Cambiar estado</h3>
          <p class="modal-sub">
            Mascota: <strong>{{ statusTargetPet?.name }}</strong><br/>
            Estado actual: <span class="badge" :class="statusBadgeClass(statusTargetPet?.status)">{{ statusTargetPet?.status }}</span>
          </p>
          <div class="modal-status-options">
            <label
              v-for="s in STATUS_OPTIONS"
              :key="s"
              class="status-option"
              :class="{ 'status-selected': pendingStatus === s }"
            >
              <input type="radio" :value="s" v-model="pendingStatus" />
              <span class="badge" :class="statusBadgeClass(s)">{{ s }}</span>
              <span class="status-desc">
                <template v-if="s === 'Disponible'">Visible en catálogo, acepta solicitudes</template>
                <template v-else-if="s === 'En proceso'">Visible, evaluando solicitudes</template>
                <template v-else-if="s === 'Adoptada'">Se mueve a «Historias felices»</template>
                <template v-else-if="s === 'Inactiva'">Oculta del público, historial conservado</template>
              </span>
            </label>
          </div>
          <div class="modal-warning" v-if="pendingStatus === 'Adoptada' || pendingStatus === 'Inactiva'">
            ⚠️ La mascota se eliminará de destacadas y no aparecerá en el catálogo principal.
          </div>
          <div class="modal-actions">
            <button class="btn-save" @click="confirmStatusChange">Confirmar cambio</button>
            <button class="btn-discard" @click="showStatusModal = false">Cancelar</button>
          </div>
        </div>
      </div>
    </Transition>

    <!-- ══════════════════════════════════════
         MODAL: Confirmar desactivar
    ══════════════════════════════════════ -->
    <Transition name="modal-fade">
      <div v-if="showDeactivateModal" class="modal-overlay" @click.self="showDeactivateModal = false">
        <div class="modal-box">
          <h3 class="modal-title">Desactivar mascota</h3>
          <p class="modal-sub">
            ¿Deseas desactivar a <strong>{{ deactivateTarget?.name }}</strong>?
          </p>
          <div class="modal-info-box">
            <p>🔒 <strong>No se eliminará</strong> del sistema. Se conservará todo su historial, solicitudes y registros médicos.</p>
            <p>La mascota pasará a estado <strong>Inactiva</strong> y dejará de ser visible en el catálogo público.</p>
          </div>
          <div class="modal-actions">
            <button class="btn-save btn-danger" @click="confirmDeactivate">Desactivar</button>
            <button class="btn-discard" @click="showDeactivateModal = false">Cancelar</button>
          </div>
        </div>
      </div>
    </Transition>

    <!-- ══════════════════════════════════════
         MODAL: Solicitudes
    ══════════════════════════════════════ -->
    <Transition name="modal-fade">
      <div v-if="showRequestsModal" class="modal-overlay" @click.self="showRequestsModal = false">
        <div class="modal-box">
          <h3 class="modal-title">Solicitudes de adopción</h3>
          <p class="modal-sub">Mascota: <strong>{{ requestsTarget?.name }}</strong></p>
          <div class="requests-empty">
            <div class="requests-empty-icon">📋</div>
            <p>No hay solicitudes registradas todavía.</p>
            <p class="text-secondary">Las solicitudes enviadas desde el catálogo aparecerán aquí.</p>
          </div>
          <div class="modal-actions">
            <button class="btn-discard" @click="showRequestsModal = false">Cerrar</button>
          </div>
        </div>
      </div>
    </Transition>

  </div>
</template>

<style scoped>
/* ── Estructura general ── */
.view-container { background-color: transparent; }

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 32px;
}

.admin-page-title { font-size: 28px; font-weight: 800; color: #3A473C; letter-spacing: -0.5px; }
.admin-page-sub   { font-size: 14px; color: #6C756D; margin-top: 4px; font-weight: 500; }

/* Botón disparador del formulario */
.btn-toggle-form {
  padding: 12px 24px;
  border-radius: 14px;
  border: none;
  background-color: #92A894;
  color: white;
  font-size: 14px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.25s ease;
}
.btn-toggle-form:hover { background-color: #7C927E; transform: translateY(-1px); box-shadow: 0 4px 12px rgba(146,168,148,.15); }
.btn-toggle-form.btn-cancel { background-color: #F4F6F4; color: #6C756D; }
.btn-toggle-form.btn-cancel:hover { background-color: #EBEFEA; color: #3A473C; box-shadow: none; }

/* ── Formulario ── */
.form-panel {
  background: white;
  border-radius: 24px;
  padding: 28px;
  box-shadow: 0 4px 20px rgba(58,71,60,.04);
  margin-bottom: 32px;
}
.form-panel h3 { font-size: 18px; font-weight: 800; color: #3A473C; margin-bottom: 24px; letter-spacing: -0.5px; }

.form-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 16px; }
.full-width { grid-column: 1 / -1; }
.form-group { display: flex; flex-direction: column; gap: 8px; }
.form-group label { font-size: 14px; font-weight: 700; color: #3A473C; display: flex; align-items: center; gap: 6px; }

.required { color: #E07070; }
.label-hint { font-size: 12px; font-weight: 500; color: #6C756D; }
.private-label { font-size: 12px; font-weight: 600; color: #D18C3A; background: rgba(249,193,122,.15); padding: 2px 8px; border-radius: 6px; }

.custom-input, .custom-select, .custom-textarea {
  width: 100%;
  padding: 13px 16px;
  border-radius: 14px;
  border: 2px solid #F4F6F4;
  background-color: #F4F6F4;
  font-size: 14px;
  color: #3A473C;
  transition: all .3s ease;
  outline: none;
  box-sizing: border-box;
}
.custom-textarea { height: 100px; resize: vertical; font-family: inherit; }
.textarea-private { background-color: #FFFBF3; border-color: rgba(249,193,122,.3); }
.textarea-private:focus { border-color: #F9C17A; box-shadow: 0 6px 15px rgba(249,193,122,.1); }

.custom-select {
  appearance: none;
  background-image: url("data:image/svg+xml;utf8,<svg xmlns='http://www.w3.org/2000/svg' width='24' height='24' viewBox='0 0 24 24' fill='none' stroke='%236C756D' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'><polyline points='6 9 12 15 18 9'></polyline></svg>");
  background-repeat: no-repeat;
  background-position: right 14px center;
  background-size: 16px;
  padding-right: 40px;
}

.custom-input:focus, .custom-select:focus, .custom-textarea:focus {
  background-color: white;
  border-color: #92A894;
  box-shadow: 0 6px 15px rgba(146,168,148,.07);
}

.input-error { border-color: #E07070 !important; background-color: #FEF8F8 !important; }
.error-msg   { font-size: 12px; color: #E07070; font-weight: 600; }

/* ── Imágenes ── */
.image-upload-zone {
  border: 2px dashed #D0D9D1;
  border-radius: 16px;
  padding: 32px 24px;
  text-align: center;
  cursor: pointer;
  transition: all .25s ease;
  background: #FAFCFA;
}
.image-upload-zone:hover { border-color: #92A894; background: #F0F5F0; }
.image-upload-zone.upload-error { border-color: #E07070; background: #FEF8F8; }
.upload-icon  { font-size: 36px; margin-bottom: 8px; }
.upload-title { font-weight: 700; color: #3A473C; margin: 0 0 4px; font-size: 15px; }
.upload-sub   { font-size: 13px; color: #6C756D; margin: 0; }

.image-previews {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  padding: 4px 0;
}
.image-preview-item {
  position: relative;
  width: 100px;
  height: 100px;
  border-radius: 12px;
  overflow: hidden;
  border: 2px solid #F0F4F0;
}
.image-preview-item img { width: 100%; height: 100%; object-fit: cover; display: block; }
.remove-image-btn {
  position: absolute;
  top: 4px; right: 4px;
  width: 22px; height: 22px;
  border-radius: 50%;
  border: none;
  background: rgba(0,0,0,.55);
  color: white;
  font-size: 14px;
  cursor: pointer;
  display: flex; align-items: center; justify-content: center;
  line-height: 1;
}
.remove-image-btn:hover { background: rgba(200,60,60,.8); }
.main-photo-label {
  position: absolute;
  bottom: 0; left: 0; right: 0;
  background: rgba(58,71,60,.7);
  color: white;
  font-size: 10px;
  font-weight: 700;
  text-align: center;
  padding: 3px 0;
}
.add-more-btn {
  width: 100px; height: 100px;
  border-radius: 12px;
  border: 2px dashed #D0D9D1;
  background: #FAFCFA;
  color: #6C756D;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  display: flex; flex-direction: column; align-items: center; justify-content: center;
  gap: 4px;
  transition: all .2s ease;
}
.add-more-btn:hover { border-color: #92A894; background: #F0F5F0; color: #3A473C; }
.add-more-icon { font-size: 24px; line-height: 1; }

.form-actions { display: flex; gap: 12px; margin-top: 24px; }

.btn-save {
  padding: 13px 24px;
  border-radius: 12px;
  border: none;
  background-color: #3A473C;
  color: white;
  font-size: 14px;
  font-weight: 700;
  cursor: pointer;
  transition: all .2s ease;
}
.btn-save:hover    { background-color: #2D372F; }
.btn-save.btn-danger { background-color: #C05050; }
.btn-save.btn-danger:hover { background-color: #A03030; }

.btn-discard {
  padding: 13px 24px;
  border-radius: 12px;
  border: 2px solid #F4F6F4;
  background-color: transparent;
  color: #6C756D;
  font-size: 14px;
  font-weight: 700;
  cursor: pointer;
  transition: all .2s ease;
}
.btn-discard:hover { background-color: #F4F6F4; color: #3A473C; }

/* ── Barra de filtros ── */
.filters-bar {
  display: flex;
  gap: 12px;
  align-items: center;
  margin-bottom: 20px;
  flex-wrap: wrap;
}
.filter-input, .filter-select {
  padding: 10px 16px;
  border-radius: 12px;
  border: 2px solid #F4F6F4;
  background: white;
  font-size: 14px;
  color: #3A473C;
  outline: none;
  transition: border-color .2s;
}
.filter-input  { flex: 1; min-width: 200px; }
.filter-select { appearance: none; padding-right: 32px; background-image: url("data:image/svg+xml;utf8,<svg xmlns='http://www.w3.org/2000/svg' width='24' height='24' viewBox='0 0 24 24' fill='none' stroke='%236C756D' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'><polyline points='6 9 12 15 18 9'></polyline></svg>"); background-repeat: no-repeat; background-position: right 10px center; background-size: 14px; }
.filter-input:focus, .filter-select:focus { border-color: #92A894; }
.filters-legend { display: flex; gap: 6px; flex-wrap: wrap; margin-left: auto; }

/* ── Tabla ── */
.table-wrapper {
  background: white;
  border-radius: 24px;
  padding: 24px;
  box-shadow: 0 4px 20px rgba(58,71,60,.02);
  overflow-x: auto;
  margin-bottom: 32px;
}

.data-table { width: 100%; border-collapse: collapse; text-align: left; }

.data-table th {
  font-size: 13px;
  font-weight: 700;
  color: #6C756D;
  padding-bottom: 16px;
  border-bottom: 1px solid #F4F6F4;
  white-space: nowrap;
}

.data-table td {
  padding: 14px 0;
  font-size: 14px;
  color: #3A473C;
  border-bottom: 1px solid #FAFAFA;
  vertical-align: middle;
  padding-right: 12px;
}

.row-inactive td { opacity: .55; }

.empty-row { text-align: center; color: #6C756D; padding: 32px !important; }

.id-code { font-size: 12px; font-family: monospace; background: #F4F6F4; padding: 4px 8px; border-radius: 8px; color: #3A473C; font-weight: 600; }

/* Thumbnail */
.pet-thumb-wrap { position: relative; width: 48px; }
.pet-thumb { width: 48px; height: 48px; border-radius: 12px; object-fit: cover; border: 2px solid #F0F4F0; display: block; }
.pet-thumb-placeholder { width: 48px; height: 48px; border-radius: 12px; background: #F4F6F4; display: flex; align-items: center; justify-content: center; font-size: 22px; }
.img-count { position: absolute; bottom: -4px; right: -4px; background: #3A473C; color: white; font-size: 10px; font-weight: 700; border-radius: 6px; padding: 1px 5px; }

.font-semibold { font-weight: 600; }
.text-secondary { font-size: 13px; color: #6C756D; }
.text-right  { text-align: right; }
.text-center { text-align: center; }

/* Badges */
.badge { padding: 6px 12px; border-radius: 10px; font-size: 12px; font-weight: 700; display: inline-block; }
.badge-green  { background: rgba(146,168,148,.2);  color: #5A6E5C; }
.badge-peach  { background: rgba(249,193,122,.2);  color: #D18C3A; }
.badge-blue   { background: rgba(130,160,180,.15); color: #4A6070; }
.badge-gray   { background: #F4F6F4;               color: #6C756D; }
.badge-health { background: #EDF1EE; color: #5A6E5C; font-size: 11px; }

/* Destacada */
.featured-btn {
  background: none;
  border: none;
  font-size: 20px;
  cursor: pointer;
  color: #D0D9D1;
  transition: all .2s;
  padding: 2px;
  line-height: 1;
}
.featured-btn:hover:not(:disabled) { color: #F9C17A; transform: scale(1.2); }
.featured-btn.featured-on  { color: #F9C17A; text-shadow: 0 0 6px rgba(249,193,122,.5); }
.featured-btn:disabled { cursor: not-allowed; opacity: .35; }

/* Botones de acción */
.action-btns { display: flex; gap: 6px; justify-content: flex-end; }
.action-btn {
  width: 34px; height: 34px;
  border: 2px solid #F4F6F4;
  border-radius: 10px;
  background: white;
  cursor: pointer;
  display: flex; align-items: center; justify-content: center;
  transition: all .2s ease;
  color: #6C756D;
}
.action-btn:hover:not(:disabled) { background: #F4F6F4; border-color: #6C756D; color: #3A473C; transform: translateY(-1px); }
.action-btn.status-btn:hover:not(:disabled) { background: rgba(146,168,148,.15); border-color: #92A894; color: #5A6E5C; }
.action-btn.archive-btn:hover:not(:disabled) { background: rgba(249,193,122,.15); border-color: #F9C17A; color: #D18C3A; }
.action-btn:disabled { cursor: not-allowed; opacity: .35; }

/* ── Info de visibilidad ── */
.visibility-info { margin-top: 8px; margin-bottom: 32px; }
.visibility-info h4 { font-size: 16px; font-weight: 800; color: #3A473C; margin-bottom: 16px; }
.visibility-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 12px; }

.vis-card {
  border-radius: 16px;
  padding: 16px 18px;
  font-size: 13px;
  line-height: 1.5;
}
.vis-card strong { display: block; font-size: 14px; margin-bottom: 6px; }
.vis-card p { margin: 0 0 4px; color: #6C756D; }
.vis-card em { font-style: normal; font-weight: 700; }
.vis-count { font-size: 20px; font-weight: 800; color: #3A473C !important; margin-top: 8px !important; }

.vis-public   { background: rgba(146,168,148,.12); border: 1.5px solid rgba(146,168,148,.3); }
.vis-featured { background: rgba(249,193,122,.1);  border: 1.5px solid rgba(249,193,122,.3); }
.vis-adopted  { background: rgba(130,160,180,.1);  border: 1.5px solid rgba(130,160,180,.25); }
.vis-hidden   { background: #F4F6F4; border: 1.5px solid #E8ECE8; }

/* ── Modales ── */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(30,38,32,.45);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 16px;
}
.modal-box {
  background: white;
  border-radius: 24px;
  padding: 32px;
  max-width: 480px;
  width: 100%;
  box-shadow: 0 20px 60px rgba(30,38,32,.18);
}
.modal-title { font-size: 20px; font-weight: 800; color: #3A473C; margin-bottom: 12px; }
.modal-sub   { font-size: 14px; color: #6C756D; margin-bottom: 20px; line-height: 1.6; }
.modal-sub strong { color: #3A473C; }

.modal-status-options { display: flex; flex-direction: column; gap: 8px; margin-bottom: 16px; }
.status-option {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 16px;
  border-radius: 12px;
  border: 2px solid #F4F6F4;
  cursor: pointer;
  transition: all .2s;
}
.status-option input[type="radio"] { display: none; }
.status-option:hover      { border-color: #92A894; background: #FAFCFA; }
.status-option.status-selected { border-color: #92A894; background: rgba(146,168,148,.08); }
.status-desc { font-size: 13px; color: #6C756D; }

.modal-warning {
  background: rgba(249,193,122,.15);
  border: 1.5px solid rgba(249,193,122,.4);
  border-radius: 10px;
  padding: 12px 16px;
  font-size: 13px;
  color: #8C5E1A;
  margin-bottom: 16px;
}

.modal-info-box {
  background: #F4F6F4;
  border-radius: 12px;
  padding: 16px;
  margin-bottom: 20px;
  font-size: 14px;
  color: #3A473C;
  line-height: 1.7;
}
.modal-info-box p { margin: 0 0 6px; }
.modal-info-box p:last-child { margin-bottom: 0; }

.modal-actions { display: flex; gap: 12px; margin-top: 20px; }

.requests-empty { text-align: center; padding: 24px 0; color: #6C756D; }
.requests-empty-icon { font-size: 40px; margin-bottom: 12px; }
.requests-empty p { margin: 0 0 6px; font-size: 14px; }

/* ── Animaciones ── */
.slide-down-enter-active { transition: all .3s cubic-bezier(.4,0,.2,1); }
.slide-down-leave-active { transition: all .2s ease; }
.slide-down-enter-from   { opacity: 0; transform: translateY(-12px); }
.slide-down-leave-to     { opacity: 0; transform: translateY(-6px); }

.modal-fade-enter-active { transition: all .25s ease; }
.modal-fade-leave-active { transition: all .15s ease; }
.modal-fade-enter-from   { opacity: 0; }
.modal-fade-leave-to     { opacity: 0; }
.modal-fade-enter-from .modal-box { transform: scale(.96) translateY(10px); }

/* ── Responsivo ── */
@media (max-width: 1200px) { .visibility-grid { grid-template-columns: repeat(2,1fr); } }
@media (max-width: 992px)  { .form-grid { grid-template-columns: 1fr 1fr; } }
@media (max-width: 768px)  {
  .page-header   { flex-direction: column; align-items: flex-start; gap: 14px; }
  .btn-toggle-form { width: 100%; }
  .form-grid     { grid-template-columns: 1fr; gap: 0; }
  .filters-bar   { flex-direction: column; align-items: stretch; }
  .filters-legend { margin-left: 0; }
  .visibility-grid { grid-template-columns: 1fr; }
}
</style>