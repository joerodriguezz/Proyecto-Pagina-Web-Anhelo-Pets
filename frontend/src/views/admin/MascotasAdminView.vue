<script setup>
import { ref, computed, onMounted } from 'vue'
import Icon from '../../components/Icon.vue'
import { usePetsStore } from '../../stores/usePetsStore'
import { useRescuesStore } from '../../stores/useRescuesStore'
import { createAnimals, uploadAnimalPhoto, getAnimalPhotos, deleteAnimalPhoto } from '../../services/petServices.js'
import { registrarAuditoria } from '../../composables/useAuditLog'
import { getFosterHomes } from '../../services/fosterHomeServices'
import { getAdoptionRequests, mapAdoptionRequestDtoToRow } from '../../services/adoptionServices'

const store        = usePetsStore()
const rescuesStore = useRescuesStore()

onMounted(() => {
  store.fetchPets({ status: 'Todos' })
  cargarCasasCuna()
  cargarSolicitudesAdopcion()
})

// ─────────────────────────────────────────────
// Casas cuna
// ─────────────────────────────────────────────
const casasCuna = ref([])
async function cargarCasasCuna() {
  try {
    const { data } = await getFosterHomes()
    casasCuna.value = (data || []).map(fh => ({
      id: fh.fosterHomeId,
      nombre: fh.name,
    }))
  } catch {
    casasCuna.value = []
  }
}

// ─────────────────────────────────────────────
// Solicitudes por mascota (solicitudes de adopción reales)
// ─────────────────────────────────────────────
const todasLasSolicitudes = ref([])
async function cargarSolicitudesAdopcion() {
  try {
    const { data } = await getAdoptionRequests()
    todasLasSolicitudes.value = (data || []).map(mapAdoptionRequestDtoToRow)
  } catch {
    todasLasSolicitudes.value = []
  }
}
const solicitudesMascota = computed(() => {
  if (!requestsTarget.value) return []
  return todasLasSolicitudes.value.filter(
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
// Wizard paso a paso (mismo estilo que el de Salud/expedientes:
// las secciones ya existentes del formulario se recorren una por
// una y una quinta de resumen concentra el envío final).
// ─────────────────────────────────────────────
const PASOS = [
  { n: 1, titulo: 'Información básica', desc: 'Datos del animal' },
  { n: 2, titulo: 'Contenido público',  desc: 'Descripción visible en el catálogo' },
  { n: 3, titulo: 'Notas internas',     desc: 'Información solo para el equipo' },
  { n: 4, titulo: 'Fotos',              desc: 'Galería del animal' },
  { n: 5, titulo: 'Resumen',            desc: 'Revisa y guarda' },
]
const TOTAL_PASOS = PASOS.length
const pasoActual = ref(1)
const pasoMaximo  = ref(1)

const pasoInfo     = computed(() => PASOS.find(p => p.n === pasoActual.value) || PASOS[0])
const esUltimoPaso = computed(() => pasoActual.value === TOTAL_PASOS)
const progreso      = computed(() => ((pasoActual.value - 1) / (TOTAL_PASOS - 1)) * 100)

const CAMPOS_POR_PASO = {
  1: ['name', 'breed', 'age', 'healthBasic'],
  2: [],
  3: [],
  4: ['images'],
  5: [],
}

function pasoCompleto(n) {
  if (n === 1) {
    return !!(formData.value.name.trim() && formData.value.breed.trim() && formData.value.age.trim() && formData.value.healthBasic.trim())
  }
  if (n === 4) return formData.value.images.length > 0
  return true
}

function validarPaso(n) {
  const e = { ...formErrors.value }
  const campos = CAMPOS_POR_PASO[n] || []
  campos.forEach(c => delete e[c])

  if (n === 1) {
    if (!formData.value.name.trim())        e.name        = 'El nombre es obligatorio'
    if (!formData.value.breed.trim())       e.breed       = 'La raza es obligatoria'
    if (!formData.value.age.trim())         e.age         = 'La edad es obligatoria'
    if (!formData.value.healthBasic.trim()) e.healthBasic = 'El estado de salud es obligatorio'
  }
  if (n === 4) {
    if (formData.value.images.length === 0) e.images = 'Debes subir al menos una foto'
  }

  formErrors.value = e
  return campos.every(c => !e[c])
}

function irAPaso(n) {
  if (n > pasoMaximo.value) return
  pasoActual.value = n
}
function pasoSiguiente() {
  if (!validarPaso(pasoActual.value)) return
  if (pasoActual.value < TOTAL_PASOS) {
    pasoActual.value += 1
    if (pasoActual.value > pasoMaximo.value) pasoMaximo.value = pasoActual.value
  }
}
function pasoAnterior() {
  if (pasoActual.value > 1) pasoActual.value -= 1
}
function guardarDesdeResumen() {
  const ok = [1, 4].every(n => validarPaso(n))
  if (!ok) return
  savePet()
}

// ─────────────────────────────────────────────
// Modales
// ─────────────────────────────────────────────
const showDeactivateModal = ref(false)
const deactivateTarget    = ref(null)
const showRequestsModal   = ref(false)
const requestsTarget      = ref(null)
const showViewModal       = ref(false)
const viewTarget          = ref(null)
const expedienteTab       = ref('general')
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
// Lista filtrada — activas primero, inactivas siempre al final.
// Se usa un sort estable (spec ES2019 / V8) que solo reordena
// según el criterio "es Inactiva o no": el resto del orden
// (por tipo, búsqueda, fecha de creación, etc.) no se altera.
// ─────────────────────────────────────────────
const filteredPets = computed(() => {
  const filtered = store.pets.filter(p => {
    const matchStatus = filterStatus.value === 'Todos' || p.status === filterStatus.value
    const matchType   = filterType.value === 'Todos' || p.type === filterType.value
    const q = searchQuery.value.toLowerCase()
    const matchSearch = !q ||
      p.name.toLowerCase().includes(q) ||
      String(p.id).toLowerCase().includes(q) ||
      p.breed.toLowerCase().includes(q)
    return matchStatus && matchType && matchSearch
  })
  return [...filtered].sort((a, b) => {
    const aInactiva = a.status === 'Inactiva' ? 1 : 0
    const bInactiva = b.status === 'Inactiva' ? 1 : 0
    return aInactiva - bInactiva
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
async function removeImage(index) {
  const img = formData.value.images[index]
  // Si la foto ya existe en el backend (tiene animalPhotoId), se borra ahí también.
  if (img?.animalPhotoId && editingPetId.value) {
    try {
      await deleteAnimalPhoto(editingPetId.value, img.animalPhotoId)
      registrarAuditoria({
        modulo: 'Mascotas', accion: 'Eliminó una foto', tipoAccion: 'editar',
        elemento: formData.value.name || editingPetId.value, elementoId: editingPetId.value,
        descripcion: `Se eliminó una foto del expediente de "${formData.value.name}".`,
      })
    } catch (err) {
      console.error('Error al borrar la foto:', err)
      showToast('error', 'No se pudo borrar la foto')
      return
    }
  }
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
// Validación (por paso — ver validarPaso más arriba)
// ─────────────────────────────────────────────
function clearErr(campo) {
  if (formErrors.value[campo]) {
    const e = { ...formErrors.value }
    delete e[campo]
    formErrors.value = e
  }
}

// ─────────────────────────────────────────────
// Subida de fotos nuevas (las que aún no tienen animalPhotoId)
// ─────────────────────────────────────────────
async function subirFotosNuevas(animalId, petName) {
  const nuevas = formData.value.images.filter(img => img.file && !img.animalPhotoId)
  if (nuevas.length === 0) return
  try {
    await Promise.all(
      nuevas.map((img, i) =>
        uploadAnimalPhoto(animalId, img.file, i === 0 && !formData.value.images.some(x => x.animalPhotoId))
      )
    )
    registrarAuditoria({
      modulo: 'Mascotas', accion: 'Agregó fotos', tipoAccion: 'editar',
      elemento: petName, elementoId: animalId,
      descripcion: `Se agregaron ${nuevas.length} foto(s) al expediente de "${petName}".`,
    })
  } catch (err) {
    console.error('Error al subir fotos:', err)
    showToast('error', 'Mascota guardada, pero una foto no se pudo subir')
  }
}

// ─────────────────────────────────────────────
// Guardar mascota
// ─────────────────────────────────────────────
async function savePet() {
  const petData = { ...formData.value, images: [...formData.value.images] }

  if (editMode.value && editingPetId.value !== null) {
    try {
      await store.updatePet(editingPetId.value, petData)
      await subirFotosNuevas(editingPetId.value, petData.name)
      await store.fetchPets({ status: 'Todos' })
      registrarAuditoria({
        modulo: 'Mascotas', accion: 'Editó una mascota', tipoAccion: 'editar',
        elemento: petData.name, elementoId: editingPetId.value,
        descripcion: `Se actualizó la información de "${petData.name}".`,
      })
      showToast('success', 'Mascota actualizada correctamente')
    } catch (err) {
      console.error('Error al actualizar mascota:', err)
      showToast('error', 'Error al actualizar la mascota')
      return
    }
  } else {
    try {
      const { data: created } = await createAnimals(petData)
      if (created?.animalId) {
        await subirFotosNuevas(created.animalId, petData.name)
      }
      await store.fetchPets({ status: 'Todos' })
      registrarAuditoria({
        modulo: 'Mascotas', accion: 'Registró una mascota', tipoAccion: 'crear',
        elemento: petData.name, elementoId: created?.animalId,
        descripcion: `Se registró la mascota "${petData.name}" (${petData.type}, ${petData.breed}).`,
      })
      showToast('success', 'Mascota registrada correctamente')
    } catch (err) {
      const detail = err.response?.data?.message || err.response?.data || err.message
      console.error('Error al crear mascota:', detail)
      showToast('error', 'Error al registrar la mascota')
      return
    }
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
  pasoActual.value = 1
  pasoMaximo.value  = 1
  showForm.value = true
}
async function openEdit(pet) {
  editMode.value     = true
  editingPetId.value = pet.id
  pasoActual.value   = 1
  pasoMaximo.value   = TOTAL_PASOS
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

  // Trae la galería real desde el backend (no desde el objeto en memoria)
  try {
    const { data } = await getAnimalPhotos(pet.id)
    formData.value.images = (data || []).map(p => ({
      preview: p.photoUrl,
      animalPhotoId: p.animalPhotoId,
    }))
  } catch (err) {
    console.error('Error al cargar la galería de fotos:', err)
  }
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
  pasoActual.value = 1
  pasoMaximo.value  = 1
}

// ─────────────────────────────────────────────
// Ver mascota (solo lectura)
// ─────────────────────────────────────────────
function openView(pet) {
  viewTarget.value    = pet
  expedienteTab.value = 'general'
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
async function confirmStatusChange() {
  if (!statusTargetPet.value) return
  const estadoAnterior = statusTargetPet.value.status
  try {
    await store.changeStatus(statusTargetPet.value.id, pendingStatus.value)
    await store.fetchPets({ status: 'Todos' })
    registrarAuditoria({
      modulo: 'Mascotas', accion: 'Cambió el estado de una mascota', tipoAccion: 'estado',
      elemento: statusTargetPet.value.name, elementoId: statusTargetPet.value.id,
      valoresAnteriores: { estado: estadoAnterior },
      valoresNuevos: { estado: pendingStatus.value },
    })
  } catch (err) {
    console.error('Error al cambiar el estado:', err)
    showToast('error', 'No se pudo cambiar el estado')
  }
  showStatusModal.value = false
  statusTargetPet.value = null
}

// ─────────────────────────────────────────────
// Activar / Inactivar rápido
// ─────────────────────────────────────────────
async function toggleActive(pet) {
  if (pet.status === 'Inactiva') {
    try {
      await store.changeStatus(pet.id, 'Disponible')
      await store.fetchPets({ status: 'Todos' })
      registrarAuditoria({
        modulo: 'Mascotas', accion: 'Reactivó una mascota', tipoAccion: 'estado',
        elemento: pet.name, elementoId: pet.id,
        valoresAnteriores: { estado: 'Inactiva' }, valoresNuevos: { estado: 'Disponible' },
      })
    } catch (err) {
      console.error('Error al reactivar la mascota:', err)
      showToast('error', 'No se pudo reactivar la mascota')
    }
  } else {
    deactivateTarget.value    = pet
    showDeactivateModal.value = true
  }
}

// ─────────────────────────────────────────────
// Desactivar
// ─────────────────────────────────────────────
async function confirmDeactivate() {
  if (!deactivateTarget.value) return
  try {
    await store.deactivatePet(deactivateTarget.value.id)
    await store.fetchPets({ status: 'Todos' })
    registrarAuditoria({
      modulo: 'Mascotas', accion: 'Desactivó una mascota', tipoAccion: 'estado',
      elemento: deactivateTarget.value.name, elementoId: deactivateTarget.value.id,
      descripcion: `"${deactivateTarget.value.name}" pasó a estado Inactiva.`,
    })
  } catch (err) {
    console.error('Error al desactivar la mascota:', err)
    showToast('error', 'No se pudo desactivar la mascota')
  }
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

// ─────────────────────────────────────────────
// EXPEDIENTE COMPLETO
// - Mascotas, fotos y rescates: ahora leen del backend real
//   (store / servicios), reutilizando lo que ya usa la
//   versión funcional del proyecto.
// - Salud (historial, vacunas, tratamientos) y solicitudes de
//   adopción: no se identificó un endpoint equivalente, por lo
//   que se mantienen igual (localStorage) hasta que se indique
//   el servicio correspondiente.
// ─────────────────────────────────────────────
const expedienteHistorialMedico = computed(() => {
  if (!viewTarget.value) return []
  const datosSalud = JSON.parse(localStorage.getItem('anhelo_salud_v3')) || {}
  const d = datosSalud[viewTarget.value.id]
  if (!d || !Array.isArray(d.medicalHistory)) return []
  return [...d.medicalHistory].sort((a, b) =>
    String(b.fecha || '').localeCompare(String(a.fecha || ''))
  )
})
const expedienteVacunas = computed(() => {
  if (!viewTarget.value) return []
  const datosSalud = JSON.parse(localStorage.getItem('anhelo_salud_v3')) || {}
  const d = datosSalud[viewTarget.value.id]
  if (!d || !Array.isArray(d.vaccines)) return []
  return [...d.vaccines].sort((a, b) =>
    String(b.fechaAplicacion || '').localeCompare(String(a.fechaAplicacion || ''))
  )
})
const expedienteTratamientos = computed(() => {
  if (!viewTarget.value) return []
  const datosSalud = JSON.parse(localStorage.getItem('anhelo_salud_v3')) || {}
  const d = datosSalud[viewTarget.value.id]
  if (!d || !Array.isArray(d.treatments)) return []
  return [...d.treatments].sort((a, b) =>
    String(b.fecha || '').localeCompare(String(a.fecha || ''))
  )
})
const expedienteRescates = computed(() => {
  if (!viewTarget.value) return []
  if (!rescuesStore.loaded) rescuesStore.fetchRescues()
  return rescuesStore.rescates.value
    .filter(r => r.animalId === viewTarget.value.id || r.mascota === viewTarget.value.name)
    .sort((a, b) =>
      String(b.fechaRescate || '').localeCompare(String(a.fechaRescate || ''))
    )
})
const expedienteSolicitudes = computed(() => {
  if (!viewTarget.value) return []
  const solicitudes = JSON.parse(localStorage.getItem('anhelo_solicitudes')) || []
  return solicitudes
    .filter(s => s.petId === viewTarget.value.id || s.mascota === viewTarget.value.name)
    .sort((a, b) => String(b.fecha || '').localeCompare(String(a.fecha || '')))
})
const expedienteAdopciones = computed(() =>
  expedienteSolicitudes.value.filter(s => s.estado === 'Aprobada')
)
const expedienteTimeline = computed(() => {
  if (!viewTarget.value) return []
  const eventos = []
  expedienteRescates.value.forEach(r => {
    eventos.push({
      fecha:   r.fechaRescate || r.fechaCreacion || '',
      icono:   '🐾',
      titulo:  'Rescatada',
      detalle: r.ubicacion || [r.provincia, r.canton, r.distrito].filter(Boolean).join(' · ')
    })
    if (r.casaCuna && r.casaCuna !== 'Sin asignar') {
      eventos.push({
        fecha:   r.fechaCreacion || r.fechaRescate || '',
        icono:   '🏠',
        titulo:  'Asignada a casa cuna',
        detalle: r.casaCuna
      })
    }
  })
  expedienteHistorialMedico.value.forEach(h => {
    eventos.push({
      fecha:   h.fecha || '',
      icono:   '🩺',
      titulo:  'Revisión médica',
      detalle: h.diagnostico || ''
    })
  })
  expedienteVacunas.value.forEach(v => {
    eventos.push({
      fecha:   v.fechaAplicacion || '',
      icono:   '💉',
      titulo:  `Vacunación${v.tipo ? ': ' + v.tipo : ''}`,
      detalle: v.vet || ''
    })
  })
  expedienteTratamientos.value.forEach(t => {
    eventos.push({
      fecha:   t.fecha || '',
      icono:   '💊',
      titulo:  `Tratamiento${t.tipo ? ': ' + t.tipo : ''}`,
      detalle: t.medicamento || ''
    })
  })
  expedienteSolicitudes.value.forEach(s => {
    eventos.push({
      fecha:   s.fecha || '',
      icono:   '📋',
      titulo:  `Solicitud recibida${s.solicitante ? ' — ' + s.solicitante : ''}`,
      detalle: s.estado || ''
    })
    if (s.estado === 'Aprobada') {
      eventos.push({
        fecha:   s.fecha || '',
        icono:   '🎉',
        titulo:  'Adopción aprobada',
        detalle: s.solicitante || ''
      })
    }
    if (s.estado === 'Rechazada') {
      eventos.push({
        fecha:   s.fecha || '',
        icono:   '✕',
        titulo:  'Solicitud rechazada',
        detalle: s.solicitante || ''
      })
    }
  })
  return eventos
    .filter(e => e.fecha)
    .sort((a, b) => String(a.fecha).localeCompare(String(b.fecha)))
})
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
         MODAL 1/3 — EDITAR / NUEVA MASCOTA
         Mismo ancho y alto que Ver / Inactivar (.modal-box--uniform)
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
              <h2 class="form-title">{{ editMode ? 'Editar mascota' : 'Nueva mascota' }}</h2>

              <div class="wiz-steps" role="list">
                <div class="wiz-track">
                  <div class="wiz-track-fill" :style="{ width: progreso + '%' }"></div>
                </div>
                <button
                  v-for="p in PASOS"
                  :key="p.n"
                  type="button"
                  role="listitem"
                  class="wiz-step"
                  :class="{
                    'is-active': pasoActual === p.n,
                    'is-done':   p.n < pasoActual && pasoCompleto(p.n),
                    'is-locked': p.n > pasoMaximo
                  }"
                  :disabled="p.n > pasoMaximo"
                  :aria-current="pasoActual === p.n ? 'step' : undefined"
                  @click="irAPaso(p.n)"
                >
                  <span class="wiz-bullet">
                    <svg v-if="p.n < pasoActual && pasoCompleto(p.n)" xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3.2" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
                    <template v-else>{{ p.n }}</template>
                  </span>
                  <span class="wiz-step-label">{{ p.titulo }}</span>
                </button>
              </div>

              <div class="wiz-context">
                <span class="wiz-context-count">Paso {{ pasoActual }} de {{ TOTAL_PASOS }}</span>
                <span class="wiz-context-sep">·</span>
                <span class="wiz-context-desc">{{ pasoInfo.desc }}</span>
              </div>
            </div>
            <div class="uniform-scroll wiz-body">
              <div class="form-body">
                <!-- Sección 1: Información básica -->
                <div v-show="pasoActual === 1" class="form-section wiz-pane">
                  <div class="form-section-label"><span class="form-num">1</span> Información básica</div>
                  <div class="form-grid">
                    <div class="fg">
                      <label>Nombre <span class="req">*</span></label>
                      <input v-model="formData.name" placeholder="Nombre del animal" class="input" :class="{ 'is-error': formErrors.name }" @input="clearErr('name')" />
                      <p v-if="formErrors.name" class="err-msg">{{ formErrors.name }}</p>
                    </div>
                    <div class="fg">
                      <label>Tipo <span class="req">*</span></label>
                      <select v-model="formData.type" class="select">
                        <option>Perro</option>
                        <option>Gato</option>
                      </select>
                    </div>
                    <div class="fg">
                      <label>Raza <span class="req">*</span></label>
                      <input v-model="formData.breed" placeholder="Raza" class="input" :class="{ 'is-error': formErrors.breed }" @input="clearErr('breed')" />
                      <p v-if="formErrors.breed" class="err-msg">{{ formErrors.breed }}</p>
                    </div>
                    <div class="fg">
                      <label>Edad <span class="req">*</span></label>
                      <input v-model="formData.age" placeholder="Ej. 2 años" class="input" :class="{ 'is-error': formErrors.age }" @input="clearErr('age')" />
                      <p v-if="formErrors.age" class="err-msg">{{ formErrors.age }}</p>
                    </div>
                    <div class="fg">
                      <label>Sexo</label>
                      <select v-model="formData.sex" class="select">
                        <option>Macho</option>
                        <option>Hembra</option>
                      </select>
                    </div>
                    <div class="fg">
                      <label>Tamaño</label>
                      <select v-model="formData.size" class="select">
                        <option>Pequeño</option>
                        <option>Mediano</option>
                        <option>Grande</option>
                      </select>
                    </div>
                    <div class="fg">
                      <label>Estado</label>
                      <select v-model="formData.status" class="select">
                        <option v-for="s in STATUS_OPTIONS" :key="s">{{ s }}</option>
                      </select>
                    </div>
                    <div class="fg">
                      <label>Salud básica <span class="req">*</span></label>
                      <input v-model="formData.healthBasic" placeholder="Ej. Vacunado, desparasitado" class="input" :class="{ 'is-error': formErrors.healthBasic }" @input="clearErr('healthBasic')" />
                      <p v-if="formErrors.healthBasic" class="err-msg">{{ formErrors.healthBasic }}</p>
                    </div>
                    <div class="fg fg--span2">
                      <label>Personalidad</label>
                      <input v-model="formData.personality" placeholder="Ej. Juguetón, tranquilo" class="input" />
                    </div>
                    <div class="fg fg--span2">
                      <label>Casa cuna asignada</label>
                      <select class="select" :value="formData.casaCunaId" @change="onCasaCunaChange">
                        <option value="">Sin asignar</option>
                        <option v-for="cc in casasCuna" :key="cc.id" :value="cc.id">{{ cc.nombre || cc.name }}</option>
                      </select>
                    </div>
                  </div>
                </div>
                <!-- Sección 2: Contenido público -->
                <div v-show="pasoActual === 2" class="form-section wiz-pane">
                  <div class="form-section-label"><span class="form-num">2</span> Contenido público</div>
                  <div class="form-grid">
                    <div class="fg fg--full">
                      <label>Descripción pública</label>
                      <textarea v-model="formData.description" placeholder="Descripción visible en el catálogo..." class="textarea"></textarea>
                    </div>
                  </div>
                </div>
                <!-- Sección 3: Notas internas -->
                <div v-show="pasoActual === 3" class="form-section wiz-pane">
                  <div class="form-section-label"><span class="form-num">3</span> Notas internas <span class="private-badge">Solo admin</span></div>
                  <div class="form-grid">
                    <div class="fg fg--full">
                      <textarea v-model="formData.internalNotes" placeholder="Historial médico, ubicación exacta, observaciones privadas..." class="textarea textarea--private"></textarea>
                    </div>
                  </div>
                </div>
                <!-- Sección 4: Fotos -->
                <div v-show="pasoActual === 4" class="form-section wiz-pane">
                  <div class="form-section-label"><span class="form-num">4</span> Fotos <span class="req">*</span></div>
                  <div v-if="formData.images.length > 0" class="image-previews">
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
                  <div v-else class="upload-zone" :class="{ 'is-error': formErrors.images }" @click="imageInputRef.click()">
                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="17 8 12 3 7 8"/><line x1="12" y1="3" x2="12" y2="15"/></svg>
                    <p class="upload-title">Subir fotos del animal</p>
                    <p class="upload-sub">Haz clic para seleccionar · JPG, PNG, WebP</p>
                  </div>
                  <p v-if="formErrors.images" class="err-msg" style="margin-top:8px">{{ formErrors.images }}</p>
                  <input ref="imageInputRef" type="file" accept="image/*" multiple style="display:none" @change="handleImageUpload" />
                </div>

                <!-- Sección 5: Resumen -->
                <div v-show="pasoActual === 5" class="form-section wiz-pane">
                  <div class="form-section-label"><span class="form-num">5</span> Resumen</div>

                  <div class="wiz-resumen">
                    <div class="wiz-res-card">
                      <div class="wiz-res-head">
                        <span class="wiz-res-title">Información básica</span>
                        <button type="button" class="wiz-res-edit" @click="irAPaso(1)">Editar</button>
                      </div>
                      <dl class="wiz-res-list">
                        <div><dt>Nombre</dt><dd>{{ formData.name || '—' }}</dd></div>
                        <div><dt>Tipo</dt><dd>{{ formData.type }}</dd></div>
                        <div><dt>Raza</dt><dd>{{ formData.breed || '—' }}</dd></div>
                        <div><dt>Edad</dt><dd>{{ formData.age || '—' }}</dd></div>
                        <div><dt>Sexo</dt><dd>{{ formData.sex }}</dd></div>
                        <div><dt>Tamaño</dt><dd>{{ formData.size }}</dd></div>
                        <div><dt>Estado</dt><dd>{{ formData.status }}</dd></div>
                        <div><dt>Salud básica</dt><dd>{{ formData.healthBasic || '—' }}</dd></div>
                        <div class="wiz-res-full"><dt>Casa cuna</dt><dd>{{ formData.casaCunaNombre || 'Sin asignar' }}</dd></div>
                      </dl>
                    </div>

                    <div class="wiz-res-card">
                      <div class="wiz-res-head">
                        <span class="wiz-res-title">Contenido público</span>
                        <button type="button" class="wiz-res-edit" @click="irAPaso(2)">Editar</button>
                      </div>
                      <dl class="wiz-res-list">
                        <div class="wiz-res-full"><dt>Descripción</dt><dd>{{ formData.description || '—' }}</dd></div>
                      </dl>
                    </div>

                    <div class="wiz-res-card">
                      <div class="wiz-res-head">
                        <span class="wiz-res-title">Notas internas</span>
                        <button type="button" class="wiz-res-edit" @click="irAPaso(3)">Editar</button>
                      </div>
                      <dl class="wiz-res-list">
                        <div class="wiz-res-full"><dt>Notas (solo admin)</dt><dd>{{ formData.internalNotes || '—' }}</dd></div>
                      </dl>
                    </div>

                    <div class="wiz-res-card wiz-res-card--full">
                      <div class="wiz-res-head">
                        <span class="wiz-res-title">Fotos</span>
                        <button type="button" class="wiz-res-edit" @click="irAPaso(4)">Editar</button>
                      </div>
                      <div v-if="formData.images.length > 0" class="image-previews">
                        <div v-for="(img, i) in formData.images" :key="i" class="image-preview-item">
                          <img :src="img.preview" :alt="img.name" />
                          <span v-if="i === 0" class="main-photo-label">Principal</span>
                        </div>
                      </div>
                      <p v-else class="wiz-res-sub">Sin fotos agregadas todavía.</p>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <div class="form-footer wiz-footer">
              <button class="btn-cancel" @click="closeForm">Cancelar</button>
              <div class="wiz-nav">
                <button v-if="pasoActual > 1" class="btn-cancel btn-back" @click="pasoAnterior">
                  <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="15 18 9 12 15 6"/></svg>
                  Atrás
                </button>
                <button v-if="!esUltimoPaso" class="btn-save" @click="pasoSiguiente">
                  Siguiente
                  <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="9 18 15 12 9 6"/></svg>
                </button>
                <button v-else class="btn-save" @click="guardarDesdeResumen">
                  <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
                  <span>{{ editMode ? 'Guardar cambios' : 'Registrar mascota' }}</span>
                </button>
              </div>
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
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/></svg>
          </div>
          <div>
            <h1 class="admin-page-title">Gestión de mascotas</h1>
            <p class="admin-page-sub">Registro y administración de animales de la fundación</p>
          </div>
        </div>
        <button class="btn btn--primary" @click="openForm">
          <svg class="btn-ico" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
          <span>Nueva mascota</span>
        </button>
      </header>
      <div class="don-summary">
        <div class="don-card total-card">
          <div class="don-icon total-icon">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="7" height="7" rx="1.5"/><rect x="14" y="3" width="7" height="7" rx="1.5"/><rect x="3" y="14" width="7" height="7" rx="1.5"/><rect x="14" y="14" width="7" height="7" rx="1.5"/></svg>
          </div>
          <strong class="don-value">{{ stats.total }}</strong>
          <span class="don-label">Total mascotas</span>
          <span class="don-desc">En el sistema</span>
        </div>
        <div class="don-card disponible-card">
          <div class="don-icon disponible-icon">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/></svg>
          </div>
          <strong class="don-value">{{ stats.disponible }}</strong>
          <span class="don-label">Disponibles</span>
          <span class="don-desc">Visibles en catálogo</span>
        </div>
        <div class="don-card proceso-card">
          <div class="don-icon proceso-icon">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="9"/><polyline points="12 7 12 12 16 14"/></svg>
          </div>
          <strong class="don-value">{{ stats.enProceso }}</strong>
          <span class="don-label">En proceso</span>
          <span class="don-desc">Evaluando solicitudes</span>
        </div>
        <div class="don-card adoptada-card">
          <div class="don-icon adoptada-icon">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/></svg>
          </div>
          <strong class="don-value">{{ stats.adoptada }}</strong>
          <span class="don-label">Adoptadas</span>
          <span class="don-desc">Historias felices</span>
        </div>
        <div class="don-card inactiva-card">
          <div class="don-icon inactiva-icon">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M21 8v13H3V8"/><path d="M1 3h22v5H1z"/><line x1="10" y1="12" x2="14" y2="12"/></svg>
          </div>
          <strong class="don-value">{{ stats.inactiva }}</strong>
          <span class="don-label">Inactivas</span>
          <span class="don-desc">Ocultas del público</span>
        </div>
      </div>
      <div class="filtros-panel">
        <div class="filtros-row">
          <div class="filtro-group filtro-group--tabs">
            <label class="filtro-label">Tipo</label>
            <div class="tabs-wrap">
              <button v-for="t in TYPE_TABS" :key="t" class="tab-btn" :class="{ active: filterType === t }" @click="filterType = t">{{ t }}</button>
            </div>
          </div>
          <div class="filtro-group filtro-group--tabs">
            <label class="filtro-label">Estado</label>
            <div class="tabs-wrap">
              <button v-for="s in STATUS_TABS" :key="s" class="tab-btn" :class="{ active: filterStatus === s }" @click="filterStatus = s">{{ s }}</button>
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
              <input v-model="searchQuery" placeholder="ID, nombre o raza..." class="filtro-input filtro-input--icon-left" />
            </div>
          </div>
          <div class="filtro-group filtro-group--btn">
            <button class="btn btn--ghost" :class="{ 'btn--ghost-active': hayFiltros }" @click="searchQuery = ''; filterStatus = 'Todos'; filterType = 'Todos'">Limpiar filtros</button>
          </div>
        </div>
      </div>
      <div v-if="filteredPets.length === 0" class="empty-state">
        <svg xmlns="http://www.w3.org/2000/svg" width="36" height="36" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M20 7H4a2 2 0 0 0-2 2v10a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2V9a2 2 0 0 0-2-2z"/><path d="M16 3l-4 4-4-4"/></svg>
        <p class="empty-title">{{ hayFiltros ? 'Sin resultados para los filtros aplicados' : 'No hay mascotas registradas aún' }}</p>
        <p class="empty-sub">{{ hayFiltros ? 'Ajusta los filtros para ver más resultados.' : 'Registra la primera mascota con el botón superior.' }}</p>
      </div>
      <div v-else class="table-wrapper">
        <div class="table-scroll">
          <table class="don-table">
            <thead>
              <tr>
                <th>ID</th><th>Foto</th><th>Nombre</th><th>Tipo</th><th>Estado</th><th>Casa cuna</th><th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="p in filteredPets" :key="p.id" class="don-row" :class="{ 'row-inactive': p.status === 'Inactiva' }">
                <td><span class="id-pill">{{ p.id }}</span></td>
                <td>
                  <div class="pet-avatar">
                    <img v-if="p.images?.length > 0" :src="p.images[0].preview" class="pet-avatar-img" :alt="p.name" />
                    <span v-else class="pet-avatar-ini">{{ p.name?.charAt(0) }}</span>
                  </div>
                </td>
                <td>
                  <span class="donor-name">{{ p.name }}</span>
                  <span class="donor-mail">{{ p.breed }}</span>
                </td>
                <td><span class="type-chip">{{ p.type }}</span></td>
                <td><span class="estado-badge" :class="statusBadgeClass(p.status)">{{ p.status }}</span></td>
                <td><span class="fecha-text">{{ getNombreCasaCuna(p) }}</span></td>
                <td>
                  <!-- Botones de acción — mismo tamaño, mismo componente base (icon-only) -->
                  <div class="action-group">
                    <button class="icon-only icon-only--ver" @click="openView(p)" data-tooltip="Ver mascota">
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
                    </button>
                    <button class="icon-only icon-only--editar" @click="openEdit(p)" data-tooltip="Editar">
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.12 2.12 0 0 1 3 3L12 15l-4 1 1-4z"/></svg>
                    </button>
                    <button
                      class="icon-only"
                      :class="p.status === 'Inactiva' ? 'icon-only--activar' : 'icon-only--inactivar'"
                      @click="toggleActive(p)"
                      :data-tooltip="p.status === 'Inactiva' ? 'Activar mascota' : 'Desactivar mascota'"
                    >
                      <svg v-if="p.status === 'Inactiva'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
                      <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 8v13H3V8"/><path d="M1 3h22v5H1z"/><line x1="10" y1="12" x2="14" y2="12"/></svg>
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
         MODAL 2/3 — VER MASCOTA (expediente)
         Mismo ancho y alto que Editar / Inactivar (.modal-box--uniform)
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
                <img v-if="viewTarget.images?.length > 0" :src="viewTarget.images[0].preview" :alt="viewTarget.name" />
                <span v-else class="hero-photo-ini">{{ viewTarget.name?.charAt(0) }}</span>
              </div>
              <div class="hero-info">
                <div class="hero-name-row">
                  <h2 class="hero-name">{{ viewTarget.name }}</h2>
                  <span class="estado-badge badge-status-hero" :class="statusBadgeClass(viewTarget.status)">{{ viewTarget.status }}</span>
                </div>
                <div class="hero-meta">
                  <span class="hero-meta-chip">
                    <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/></svg>
                    {{ viewTarget.type }}
                  </span>
                  <span class="hero-meta-chip">{{ viewTarget.breed }}</span>
                  <span class="hero-meta-chip">{{ viewTarget.age }}</span>
                  <span class="hero-meta-chip">{{ viewTarget.sex }}</span>
                </div>
              </div>
            </div>
            <div class="tabs">
              <button class="tab" :class="{ active: expedienteTab === 'general' }" @click="expedienteTab = 'general'">General</button>
              <button class="tab" :class="{ active: expedienteTab === 'medico' }" @click="expedienteTab = 'medico'">
                Médico
                <span v-if="expedienteHistorialMedico.length + expedienteVacunas.length + expedienteTratamientos.length" class="tab-count">{{ expedienteHistorialMedico.length + expedienteVacunas.length + expedienteTratamientos.length }}</span>
              </button>
              <button class="tab" :class="{ active: expedienteTab === 'rescate' }" @click="expedienteTab = 'rescate'">
                Rescate
                <span v-if="expedienteRescates.length" class="tab-count">{{ expedienteRescates.length }}</span>
              </button>
              <button class="tab" :class="{ active: expedienteTab === 'adopcion' }" @click="expedienteTab = 'adopcion'">
                Adopción
                <span v-if="expedienteSolicitudes.length" class="tab-count">{{ expedienteSolicitudes.length }}</span>
              </button>
              <button class="tab" :class="{ active: expedienteTab === 'linea' }" @click="expedienteTab = 'linea'">Línea de tiempo</button>
            </div>
            <div class="uniform-scroll">
              <div class="body">
                <!-- TAB: General -->
                <template v-if="expedienteTab === 'general'">
                  <div class="grid-2col">
                    <div>
                      <div class="block">
                        <h4 class="block-title">
                          <span class="block-title-icon">
                            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/></svg>
                          </span>
                          Información de la mascota
                        </h4>
                        <div class="fields-row">
                          <div class="field-col"><span class="field-label-row">Tipo</span><span class="field-value">{{ viewTarget.type }}</span></div>
                          <div class="field-col"><span class="field-label-row">Tamaño</span><span class="field-value">{{ viewTarget.size }}</span></div>
                          <div class="field-col"><span class="field-label-row">Salud básica</span><span class="field-value">{{ viewTarget.healthBasic }}</span></div>
                        </div>
                        <div class="info-subsection" v-if="viewTarget.personality">
                          <span class="field-label-row">Personalidad</span>
                          <p class="info-subsection-text">{{ viewTarget.personality }}</p>
                        </div>
                        <div class="info-subsection" v-if="viewTarget.description">
                          <span class="field-label-row">Descripción pública</span>
                          <p class="info-subsection-text">{{ viewTarget.description }}</p>
                        </div>
                      </div>
                      <div class="block" v-if="viewTarget.images?.length > 1">
                        <h4 class="block-title">
                          <span class="block-title-icon">
                            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="18" height="18" rx="2"/><circle cx="8.5" cy="8.5" r="1.5"/><path d="M21 15l-5-5L5 21"/></svg>
                          </span>
                          Galería
                        </h4>
                        <div class="modal-gallery">
                          <img v-for="(img, i) in viewTarget.images.slice(1)" :key="i" :src="img.preview" :alt="viewTarget.name" class="gallery-img" />
                        </div>
                      </div>
                    </div>
                    <div class="block" style="margin-bottom:0;">
                      <h4 class="block-title">
                        <span class="block-title-icon">
                          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="7" width="20" height="14" rx="2"/><path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"/></svg>
                        </span>
                        Asignaciones
                      </h4>
                      <div class="list-col">
                        <div class="list-item">
                          <div class="list-icon"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/></svg></div>
                          <div class="list-text"><span class="list-label">Casa cuna</span><span class="list-value">{{ getNombreCasaCuna(viewTarget) }}</span></div>
                        </div>
                        <div class="list-item">
                          <div class="list-icon"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/></svg></div>
                          <div class="list-text"><span class="list-label">Estado</span><span class="list-value">{{ viewTarget.status }}</span></div>
                        </div>
                        <div class="list-item">
                          <div class="list-icon"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="9"/></svg></div>
                          <div class="list-text"><span class="list-label">Sexo</span><span class="list-value">{{ viewTarget.sex }}</span></div>
                        </div>
                        <div class="list-item">
                          <div class="list-icon"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="9"/><polyline points="12 7 12 12 16 14"/></svg></div>
                          <div class="list-text"><span class="list-label">Edad</span><span class="list-value">{{ viewTarget.age }}</span></div>
                        </div>
                      </div>
                    </div>
                  </div>
                  <div class="block block-wide" v-if="viewTarget.internalNotes">
                    <h4 class="block-title">
                      <span class="block-title-icon block-title-icon--warn">🔒</span>
                      Notas internas
                    </h4>
                    <div class="tint-box tint-box--warn tint-box--desc">
                      <span>{{ viewTarget.internalNotes }}</span>
                    </div>
                  </div>
                </template>
                <!-- TAB: Médico -->
                <template v-if="expedienteTab === 'medico'">
                  <div class="block">
                    <h4 class="block-title">
                      <span class="block-title-icon">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
                      </span>
                      Historial médico
                    </h4>
                    <div v-if="expedienteHistorialMedico.length" class="expediente-list">
                      <div v-for="h in expedienteHistorialMedico" :key="h.id" class="expediente-item expediente-item--medico">
                        <div class="expediente-item-header">
                          <span class="expediente-fecha">{{ h.fecha || '—' }}</span>
                          <span class="id-pill">{{ h.id }}</span>
                        </div>
                        <p class="expediente-diag"><strong>Diagnóstico:</strong> {{ h.diagnostico || '—' }}</p>
                        <p class="expediente-detalle" v-if="h.vet">Veterinario: {{ h.vet }}</p>
                        <p class="expediente-detalle" v-if="h.peso">Peso registrado: {{ h.peso }} kg</p>
                        <p class="expediente-detalle" v-if="h.observaciones">{{ h.observaciones }}</p>
                      </div>
                    </div>
                    <p v-else class="modal-empty-text">No existen registros médicos para esta mascota.</p>
                  </div>
                  <div v-if="expedienteVacunas.length" class="block">
                    <h4 class="block-title">
                      <span class="block-title-icon">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 12h-4l-3 9L9 3l-3 9H2"/></svg>
                      </span>
                      Vacunas aplicadas
                    </h4>
                    <div class="expediente-list">
                      <div v-for="v in expedienteVacunas" :key="v.id" class="expediente-item expediente-item--vacuna">
                        <div class="expediente-item-header">
                          <span class="expediente-fecha">{{ v.fechaAplicacion || '—' }}</span>
                          <span class="id-pill">{{ v.id }}</span>
                        </div>
                        <p class="expediente-diag"><strong>{{ v.tipo }}</strong></p>
                        <p class="expediente-detalle" v-if="v.vet">Veterinario: {{ v.vet }}</p>
                        <p class="expediente-detalle" v-if="v.proximaDosis">Próxima dosis: {{ v.proximaDosis }}</p>
                        <p class="expediente-detalle" v-if="v.observaciones">{{ v.observaciones }}</p>
                      </div>
                    </div>
                  </div>
                  <div v-if="expedienteTratamientos.length" class="block">
                    <h4 class="block-title">
                      <span class="block-title-icon">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="7" height="7" rx="1.5"/><rect x="14" y="3" width="7" height="7" rx="1.5"/><rect x="14" y="14" width="7" height="7" rx="1.5"/><rect x="3" y="14" width="7" height="7" rx="1.5"/></svg>
                      </span>
                      Tratamientos
                    </h4>
                    <div class="expediente-list">
                      <div v-for="t in expedienteTratamientos" :key="t.id" class="expediente-item expediente-item--tratamiento">
                        <div class="expediente-item-header">
                          <span class="expediente-fecha">{{ t.fecha || '—' }}</span>
                          <span class="id-pill">{{ t.id }}</span>
                        </div>
                        <p class="expediente-diag"><strong>{{ t.tipo }}</strong></p>
                        <p class="expediente-detalle" v-if="t.medicamento">Medicamento: {{ t.medicamento }} {{ t.dosis ? '· ' + t.dosis : '' }}</p>
                        <p class="expediente-detalle" v-if="t.observaciones">{{ t.observaciones }}</p>
                      </div>
                    </div>
                  </div>
                </template>
                <!-- TAB: Rescate -->
                <template v-if="expedienteTab === 'rescate'">
                  <div class="block">
                    <h4 class="block-title">
                      <span class="block-title-icon">
  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
    <path d="M3 9l9-7 9 7"/>
    <path d="M5 10v10h14V10"/>
  </svg>
</span>
                      Historial de rescates
                    </h4>
                    <div v-if="expedienteRescates.length" class="expediente-list">
                      <div v-for="r in expedienteRescates" :key="r.id" class="expediente-item expediente-item--rescate">
                        <div class="expediente-item-header">
                          <span class="expediente-fecha">{{ r.fechaRescate || '—' }}</span>
                          <span class="id-pill">{{ r.id }}</span>
                        </div>
                        <p class="expediente-diag">{{ r.ubicacion || [r.provincia, r.canton, r.distrito].filter(Boolean).join(', ') || '—' }}</p>
                        <p class="expediente-detalle" v-if="r.rescatista">Rescatista: {{ r.rescatista }}</p>
                        <p class="expediente-detalle" v-if="r.descripcion">{{ r.descripcion }}</p>
                      </div>
                    </div>
                    <p v-else class="modal-empty-text">Esta mascota no posee registros de rescate.</p>
                  </div>
                </template>
                <!-- TAB: Adopción -->
                <template v-if="expedienteTab === 'adopcion'">
                  <div class="block">
                    <h4 class="block-title">
                      <span class="block-title-icon">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="7" width="20" height="14" rx="2"/><path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"/></svg>
                      </span>
                      Solicitudes de adopción
                    </h4>
                    <div v-if="expedienteSolicitudes.length" class="expediente-list">
                      <div v-for="s in expedienteSolicitudes" :key="s.id" class="expediente-item expediente-item--solicitud">
                        <div class="expediente-item-header">
                          <span class="expediente-fecha">{{ s.fecha || '—' }}</span>
                          <span class="estado-badge" :class="{
                            'badge-aprobada': s.estado === 'Aprobada',
                            'badge-pendiente': s.estado === 'Pendiente',
                            'badge-proceso': s.estado === 'En proceso',
                            'badge-rechazada': s.estado === 'Rechazada',
                          }">{{ s.estado }}</span>
                        </div>
                        <p class="expediente-diag">{{ s.solicitante || '—' }}</p>
                        <p class="expediente-detalle" v-if="s.observaciones">{{ s.observaciones }}</p>
                      </div>
                    </div>
                    <p v-else class="modal-empty-text">No hay solicitudes registradas para esta mascota.</p>
                  </div>
                </template>
                <!-- TAB: Línea de tiempo -->
                <template v-if="expedienteTab === 'linea'">
                  <div class="block">
                    <h4 class="block-title">
                      <span class="block-title-icon">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="9"/><polyline points="12 7 12 12 16 14"/></svg>
                      </span>
                      Línea de tiempo
                    </h4>
                    <div v-if="expedienteTimeline.length" class="timeline-list">
                      <div v-for="(e, i) in expedienteTimeline" :key="i" class="timeline-item">
                        <span class="timeline-icon">
                          <svg v-if="e.icono === '🐾'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="8" cy="8" r="2"/><circle cx="16" cy="8" r="2"/><circle cx="5" cy="14" r="2"/><circle cx="19" cy="14" r="2"/><path d="M12 14c-3 0-5 2.2-5 4.8 0 1.8 1.5 3.2 3.4 3.2.9 0 1.6-.4 1.6-.4s.7.4 1.6.4c1.9 0 3.4-1.4 3.4-3.2 0-2.6-2-4.8-5-4.8z"/></svg>
                          <svg v-else-if="e.icono === '🏠'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/></svg>
                          <svg v-else-if="e.icono === '🩺'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
                          <svg v-else-if="e.icono === '💉'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M22 12h-4l-3 9L9 3l-3 9H2"/></svg>
                          <svg v-else-if="e.icono === '💊'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="7" height="7" rx="1.5"/><rect x="14" y="3" width="7" height="7" rx="1.5"/><rect x="14" y="14" width="7" height="7" rx="1.5"/><rect x="3" y="14" width="7" height="7" rx="1.5"/></svg>
                          <svg v-else-if="e.icono === '📋'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="2" y="7" width="20" height="14" rx="2"/><path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"/></svg>
                          <svg v-else-if="e.icono === '🎉'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/></svg>
                          <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="9"/><line x1="9" y1="9" x2="15" y2="15"/><line x1="15" y1="9" x2="9" y2="15"/></svg>
                        </span>
                        <div class="timeline-content">
                          <span class="timeline-fecha">{{ e.fecha }}</span>
                          <strong class="timeline-titulo">{{ e.titulo }}</strong>
                          <span v-if="e.detalle" class="timeline-detalle">{{ e.detalle }}</span>
                        </div>
                      </div>
                    </div>
                    <p v-else class="modal-empty-text">Aún no hay eventos suficientes para construir una línea de tiempo.</p>
                  </div>
                </template>
              </div>
            </div>
            <div class="footer">
              <button class="btn-ghost-red" @click="showViewModal = false">
                Cerrar expediente
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
    <!-- ══════════════════════════════════════
         Modal: Cambiar estado (sin cambios de diseño)
    ══════════════════════════════════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showStatusModal" class="modal-overlay" @click.self="showStatusModal = false">
          <div class="modal-box modal-box--sm">
            <button class="btn btn--icon btn--icon-close" @click="showStatusModal = false">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>
            <div class="modal-header">
              <div class="modal-header-info">
                <p class="modal-eyebrow">Cambiar estado</p>
                <h2 class="modal-title">{{ statusTargetPet?.name }}</h2>
              </div>
            </div>
            <div class="modal-section">
              <div class="status-options">
                <label v-for="s in STATUS_OPTIONS" :key="s" class="status-option" :class="{ selected: pendingStatus === s }">
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
              <button class="btn btn--ghost" @click="showStatusModal = false">Cancelar</button>
              <button class="btn btn--primary" @click="confirmStatusChange">
                <svg class="btn-ico" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
                <span>Confirmar</span>
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
    <!-- ══════════════════════════════════════
         MODAL 3/3 — INACTIVAR MASCOTA
         Mismo ancho y alto que Ver / Editar (.modal-box--uniform)
    ══════════════════════════════════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showDeactivateModal" class="modal-overlay" @click.self="showDeactivateModal = false">
          <div class="modal-box modal-box--confirm">
            <button class="close-btn" @click="showDeactivateModal = false">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>
            <div class="confirm-header">
              <div class="confirm-icon">
                <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 8v13H3V8"/><path d="M1 3h22v5H1z"/><line x1="10" y1="12" x2="14" y2="12"/></svg>
              </div>
              <div>
                <p class="confirm-eyebrow">Desactivar mascota</p>
                <h2 class="confirm-title">{{ deactivateTarget?.name }}</h2>
              </div>
            </div>
            <div class="confirm-body">
              <div class="warn-box">
                <p>🔒 <strong>No se eliminará</strong> del sistema. Se conservará todo su historial, solicitudes y registros.</p>
                <p>La mascota pasará a estado <strong>Inactiva</strong> y dejará de ser visible en el catálogo público.</p>
              </div>
            </div>
            <div class="confirm-footer">
              <button class="btn-cancel" @click="showDeactivateModal = false">Cancelar</button>
              <button class="btn-danger" @click="confirmDeactivate">Desactivar</button>
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
  --btn-height:      33px;
  --btn-radius:      9px;
  --btn-pad-x:       13px;
  --btn-icon-size:   14px;
  --btn-icon-gap:    6px;
  --btn-font-size:   12.5px;
  --btn-font-weight: 600;
  --btn-transition:  0.16s ease;
  --select-arrow: url('data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="%237A827B" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>');
  --select-arrow-focus: url('data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="%233A473C" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>');
  background:
    radial-gradient(ellipse 800px 420px at 12% 0%, rgba(146,168,148,.07), transparent),
    var(--fondo);
  padding-bottom: 40px;
}
/* ── Sistema de botones — compacto, moderno, con sombra sutil ── */
.btn { display:inline-flex; align-items:center; justify-content:center; gap:var(--btn-icon-gap); height:var(--btn-height); padding:0 var(--btn-pad-x); border-radius:var(--btn-radius); border:1px solid transparent; font-family:inherit; font-size:var(--btn-font-size); font-weight:var(--btn-font-weight); line-height:1; white-space:nowrap; cursor:pointer; user-select:none; transition:background-color var(--btn-transition), border-color var(--btn-transition), color var(--btn-transition), box-shadow var(--btn-transition); }
.btn-ico, .btn :deep(svg) { width:var(--btn-icon-size); height:var(--btn-icon-size); flex-shrink:0; }
.btn:active:not(:disabled) { transform:translateY(1px); }
.btn:focus-visible { outline:none; box-shadow:0 0 0 3px rgba(58,71,60,.16); }
.btn--primary { background:var(--verde); color:#fff; box-shadow:0 1px 2px rgba(58,71,60,.12), 0 4px 10px -4px rgba(58,71,60,.35); }
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
.don-summary { display:grid; grid-template-columns:repeat(5, 1fr); gap:12px; margin-bottom:20px; }
.don-card { background:var(--blanco); border-radius:16px; padding:16px 15px; border:1px solid var(--borde); box-shadow:var(--sombra-sm); display:flex; flex-direction:column; transition:box-shadow .18s ease, border-color .18s ease; }
.don-card:hover { border-color:#D7DED8; box-shadow:var(--sombra-md); }
.don-icon { width:32px; height:32px; border-radius:50%; display:flex; align-items:center; justify-content:center; margin-bottom:12px; border:1px solid transparent; }
.total-icon { background:#F2F3F2; border-color:#DFE2DF; color:#616861; }
.disponible-icon { background:#EDF6EF; border-color:#C9E4CE; color:#2E7D45; }
.proceso-icon { background:#FDF6E8; border-color:#F2E1B8; color:#A97A0C; }
.adoptada-icon { background:#EAF2F6; border-color:#C7DCE6; color:#3C6E85; }
.inactiva-icon { background:#F2F3F2; border-color:#DFE2DF; color:#7A827B; }
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
.type-chip { font-size:11.5px; font-weight:600; color:#4E6E51; background:#F1F5F1; padding:3px 10px; border-radius:7px; white-space:nowrap; }
.estado-badge { display:inline-block; font-size:10.5px; font-weight:700; padding:4px 11px; border-radius:20px; white-space:nowrap; }
.badge-pendiente { background:#FDF6E8; color:#96650A; }
.badge-aprobada { background:#EDF6EF; color:#2E7D32; }
.badge-rechazada { background:#FBEDEC; color:#B71C1C; }
.badge-adoptada { background:#EAF2F6; color:#3C6E85; }
.badge-inactiva { background:#F2F3F2; color:#7A827B; }
.badge-rescate { background:#FBF0E6; color:#9A5420; }
.badge-proceso { background:#EEF1FB; color:#4F73B8; }
.table-footer { padding:12px 16px; border-top:1px solid var(--borde-suave); font-size:12px; color:var(--texto-sec); font-weight:500; }
/* Botones de acción de la tabla — mismo componente base, distinto acento */
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
.icon-only--activar { color:#2E7D45; border-color:#CFE8D6; }
.icon-only--activar:hover { background:#F3FAF5; border-color:#2E7D45; }
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
.modal-box--sm { max-width:480px; width:100%; padding:32px; max-height:90vh; overflow-y:auto; }
/* ── Modal de confirmación compacto (Desactivar) ── */
.modal-box--confirm { width:420px; max-width:90vw; max-height:90vh; display:flex; flex-direction:column; overflow:hidden; border:1px solid var(--borde-suave); }
/* ══════════════════════════════════════════════
   .modal-box--uniform — Ver / Editar / Inactivar
   MISMO ANCHO Y MISMO ALTO EXACTOS para las 3 vistas
   (sin cambios de dimensión). Header/tabs/footer fijos;
   el contenido central (.uniform-scroll) hace scroll.
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
.close-btn--hero { background:var(--fondo); }
.close-btn--hero:hover { background:var(--verde); color:#fff; }
/* ── HERO (Ver mascota) — compacto, premium, foto + chips ── */
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
.hero-photo img { width:100%; height:100%; object-fit:cover; display:block; }
.hero-photo-ini { font-size:20px; font-weight:700; color:#3E7A45; letter-spacing:-.3px; }
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
.tab-count { font-size:10px; font-weight:700; background:var(--fondo); color:var(--texto); border:1px solid var(--borde); border-radius:20px; padding:1px 6px; }
.tab.active .tab-count { background:#EDF6EF; border-color:#C9E4CE; color:#2E7D45; }
/* ── BODY (Ver mascota) ── */
.body { padding:18px 40px 10px; }
.grid-2col { display:grid; grid-template-columns:1.6fr 1fr; gap:14px; align-items:start; margin-bottom:0; }
.block { background:var(--blanco); border:1px solid var(--borde-suave); border-radius:14px; padding:18px 20px; margin-bottom:14px; box-shadow:var(--sombra-sm); }
.block:last-child { margin-bottom:0; }
.block-title { display:flex; align-items:center; gap:10px; font-size:12.5px; font-weight:700; color:var(--texto); text-transform:uppercase; letter-spacing:.4px; margin:0 0 14px; }
.block-title-icon { width:24px; height:24px; border-radius:50%; background:#F0F5F0; color:#4E7A54; display:flex; align-items:center; justify-content:center; flex-shrink:0; font-size:12px; }
.block-title-icon--warn { background:#FFFBF3; color:#A97A0C; }
.fields-row { display:grid; grid-template-columns:repeat(3, 1fr); gap:14px 16px; }
.field-col { display:flex; flex-direction:column; gap:5px; }
.field-label-row { font-size:10px; font-weight:700; color:var(--texto-ter); text-transform:uppercase; letter-spacing:.4px; }
.field-value { font-size:14px; font-weight:600; color:var(--texto); }
.info-subsection { margin-top:16px; padding-top:16px; border-top:1px solid var(--borde-suave); }
.info-subsection .field-label-row { display:block; margin-bottom:7px; }
.info-subsection-text { font-size:13px; font-weight:500; color:#4B534A; line-height:1.6; margin:0; }
.block-wide { margin-top:14px; margin-bottom:0 !important; }
.tint-box { background:var(--fondo); border-radius:10px; padding:13px 15px; }
.tint-box span { font-size:13px; font-weight:600; color:var(--texto); line-height:1.55; }
.tint-box--desc span { font-weight:500; color:#4B534A; }
.tint-box--warn { background:#FFFBF3; }
.list-col { display:grid; grid-template-columns:1fr; gap:8px; }
.list-item { border:1px solid var(--borde-suave); border-radius:10px; padding:10px 12px; display:flex; align-items:center; gap:10px; }
.list-icon { width:30px; height:30px; border-radius:8px; flex-shrink:0; background:#EDF3EE; color:#3E7A45; display:flex; align-items:center; justify-content:center; }
.list-text { display:flex; flex-direction:column; gap:2px; min-width:0; }
.list-label { font-size:9.5px; font-weight:700; color:var(--texto-ter); text-transform:uppercase; letter-spacing:.4px; }
.list-value { font-size:12.5px; font-weight:700; color:var(--texto); white-space:nowrap; overflow:hidden; text-overflow:ellipsis; }
.modal-gallery { display:flex; gap:8px; flex-wrap:wrap; }
.gallery-img { width:60px; height:60px; border-radius:8px; object-fit:cover; border:1px solid var(--borde); }
.modal-empty-text { font-size:13px; color:var(--texto-ter); background:var(--fondo); border:1px dashed var(--borde); border-radius:10px; padding:18px 16px; margin:0; text-align:center; }
/* Listas del expediente (médico / rescate / adopción) — 2 columnas cuando el ancho lo permite */
.expediente-list { display:flex; flex-direction:column; gap:10px; }
.expediente-item { background:var(--blanco); border:1px solid var(--borde-suave); border-left:3px solid #92A894; border-radius:0 12px 12px 0; padding:14px 20px; box-shadow:var(--sombra-sm); }
.expediente-item--medico { border-left-color:#4F8A6F; }
.expediente-item--vacuna { border-left-color:#3E7CB1; }
.expediente-item--tratamiento { border-left-color:#C98A35; }
.expediente-item--rescate { border-left-color:#A85C2E; }
.expediente-item--solicitud { border-left-color:#6B5B95; }
.expediente-item-header { display:flex; align-items:center; justify-content:space-between; gap:10px; margin-bottom:7px; }
.expediente-fecha { font-size:10.5px; font-weight:700; letter-spacing:.3px; color:var(--verde-sec); text-transform:uppercase; }
.expediente-diag { font-size:13.5px; color:var(--texto); margin:0 0 4px; line-height:1.55; }
.expediente-detalle { display:inline-block; font-size:12px; color:var(--texto-sec); margin:0 18px 0 0; line-height:1.6; }
/* Línea de tiempo */
.timeline-list { display:flex; flex-direction:column; }
.timeline-item { display:flex; gap:13px; padding:9px 0; position:relative; }
.timeline-item:not(:last-child)::before { content:''; position:absolute; left:15px; top:34px; bottom:-5px; width:2px; background:var(--borde); }
.timeline-icon { width:30px; height:30px; flex-shrink:0; border-radius:50%; background:var(--blanco); border:1px solid var(--borde); display:flex; align-items:center; justify-content:center; color:#4E7A54; z-index:1; }
.timeline-icon svg { width:15px; height:15px; }
.timeline-content { display:flex; flex-direction:column; gap:2px; padding-top:2px; }
.timeline-fecha { font-size:10px; font-weight:700; color:var(--verde-sec); text-transform:uppercase; letter-spacing:.5px; }
.timeline-titulo { font-size:13px; color:var(--texto); font-weight:700; }
.timeline-detalle { font-size:11.5px; color:var(--texto-sec); }
/* ── FOOTER (Ver mascota) ── */
.footer { flex-shrink:0; display:flex; justify-content:flex-end; padding:14px 40px 18px; border-top:1px solid var(--borde-suave); }
.btn-ghost-red { display:flex; align-items:center; gap:6px; height:29px; padding:0 12px; border-radius:8px; background:var(--blanco); border:1px solid var(--borde); color:var(--texto-sec); font-size:11.5px; font-weight:600; cursor:pointer; transition:background-color .16s ease, border-color .16s ease, color .16s ease; }
.btn-ghost-red:hover { background:#FDF4F3; border-color:#E8B9B2; color:var(--rojo); }
/* ══════════════════════════════════════════════
   FORMULARIO (Editar / Nueva mascota)
   ══════════════════════════════════════════════ */
.form-header { flex-shrink:0; background:linear-gradient(165deg, #FFFFFF 0%, #F7FAF7 100%); padding:26px 40px 18px; border-bottom:1px solid var(--borde-suave); }
.form-eyebrow { font-size:11px; font-weight:700; color:#3E8B54; text-transform:uppercase; letter-spacing:.6px; margin:0 0 4px; }
.form-title { font-size:20px; font-weight:700; color:var(--texto); margin:0 0 4px; letter-spacing:-.3px; }
.form-sub { font-size:12.5px; color:var(--texto-sec); margin:0; }
.form-body { padding:20px 40px 8px; }
.form-section { margin-bottom:20px; }
.form-section-label { display:flex; align-items:center; gap:9px; font-size:12px; font-weight:700; color:var(--texto); text-transform:uppercase; letter-spacing:.5px; margin-bottom:12px; padding-bottom:9px; border-bottom:1px solid var(--borde-suave); }
.form-num { width:20px; height:20px; border-radius:7px; background:var(--verde); color:#fff; font-size:10px; font-weight:700; display:flex; align-items:center; justify-content:center; flex-shrink:0; }
.private-badge { font-size:10.5px; font-weight:600; color:#A97A0C; background:#FDF6E8; padding:2px 8px; border-radius:6px; text-transform:none; letter-spacing:0; margin-left:auto; }
.req { color:#c0392b; }
.form-grid { display:grid; grid-template-columns:repeat(4,1fr); gap:13px 16px; }
.fg { display:flex; flex-direction:column; gap:6px; }
.fg--span2 { grid-column:span 2; }
.fg--full { grid-column:1 / -1; }
.fg label { font-size:11.5px; font-weight:700; color:var(--texto-sec); }
.err-msg { font-size:11px; color:#c0392b; font-weight:600; margin:0; }
.input, .select {
  height:38px; padding:0 12px; border-radius:9px; border:1px solid var(--borde);
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
.input:hover, .select:hover { border-color:#D3D8D3; }
.input:focus, .select:focus { border-color:var(--verde-sec); box-shadow:0 0 0 3px rgba(146,168,148,.2); }
.select:focus { background-image:var(--select-arrow-focus); }
.input.is-error { border-color:#e57373; background:#fff8f8; }
.textarea { padding:10px 12px; border-radius:9px; border:1px solid var(--borde); background:var(--blanco); font-size:13px; color:var(--texto); font-family:inherit; outline:none; width:100%; box-sizing:border-box; height:72px; resize:vertical; line-height:1.5; transition:border-color .16s ease, box-shadow .16s ease; }
.textarea:hover { border-color:#D3D8D3; }
.textarea:focus { border-color:var(--verde-sec); box-shadow:0 0 0 3px rgba(146,168,148,.2); }
.textarea--private { background:#FFFBF3; border-color:rgba(249,193,122,.3); }
.upload-zone { border:1.5px dashed #D0D9D1; border-radius:9px; padding:20px; text-align:center; cursor:pointer; background:#FAFCFA; transition:border-color .16s ease, background-color .16s ease; }
.upload-zone:hover { border-color:var(--verde-sec); background:#F2F7F2; }
.upload-zone.is-error { border-color:#e57373; background:#fff8f8; }
.upload-zone svg { color:var(--verde-sec); margin-bottom:8px; }
.upload-title { font-size:13px; font-weight:700; color:var(--texto); margin:0 0 3px; }
.upload-sub { font-size:12px; color:var(--texto-ter); margin:0; }
.image-previews { display:flex; flex-wrap:wrap; gap:10px; }
.image-preview-item { position:relative; width:74px; height:74px; border-radius:9px; overflow:hidden; border:1px solid var(--borde); }
.image-preview-item img { width:100%; height:100%; object-fit:cover; display:block; }
.remove-image-btn { position:absolute; top:4px; right:4px; width:20px; height:20px; border-radius:50%; background:rgba(0,0,0,0.5); color:white; font-size:12px; border:none; cursor:pointer; display:flex; align-items:center; justify-content:center; }
.main-photo-label { position:absolute; bottom:0; left:0; right:0; background:rgba(45,58,46,.75); color:white; font-size:9px; font-weight:700; text-align:center; padding:3px 0; }
.add-more-btn { width:74px; height:74px; border-radius:9px; border:1.5px dashed #D0D9D1; background:#FAFCFA; color:var(--texto-ter); font-size:11px; font-weight:600; cursor:pointer; display:flex; flex-direction:column; align-items:center; justify-content:center; gap:4px; font-family:inherit; transition:border-color .16s ease, background-color .16s ease, color .16s ease; }
.add-more-btn:hover { border-color:var(--verde-sec); background:#F2F7F2; color:var(--verde); }
.add-more-icon { font-size:20px; line-height:1; }
.form-footer { flex-shrink:0; display:flex; justify-content:flex-end; gap:10px; padding:14px 40px 18px; border-top:1px solid var(--borde-suave); }
.btn-cancel { height:38px; padding:0 16px; border-radius:9px; background:var(--blanco); border:1px solid var(--borde); color:var(--texto-sec); font-size:13px; font-weight:600; cursor:pointer; transition:background-color .16s ease, border-color .16s ease, color .16s ease; }
.btn-cancel:hover { background:#FAFBFA; color:var(--texto); border-color:#D3D8D3; }
.btn-save { display:flex; align-items:center; gap:7px; height:38px; padding:0 17px; border-radius:9px; background:var(--verde); border:none; color:#fff; font-size:13px; font-weight:600; cursor:pointer; box-shadow:0 1px 2px rgba(58,71,60,.12), 0 4px 10px -4px rgba(58,71,60,.35); transition:background-color .16s ease; }
.btn-save svg { width:14px; height:14px; }
.btn-save:hover { background:#465747; }
/* ══════════════════════════════════════════════
   CONFIRMAR DESACTIVAR
   ══════════════════════════════════════════════ */
.confirm-header { flex-shrink:0; padding:24px 32px 16px; display:flex; align-items:center; gap:14px; border-bottom:1px solid var(--borde); background:linear-gradient(165deg, #FFFFFF 0%, #FDF7F6 100%); }
.confirm-icon { width:42px; height:42px; border-radius:11px; flex-shrink:0; background:var(--rojo-bg); color:var(--rojo); display:flex; align-items:center; justify-content:center; }
.confirm-eyebrow { font-size:11px; font-weight:700; color:var(--rojo); text-transform:uppercase; letter-spacing:.6px; margin:0 0 4px; }
.confirm-title { font-size:17px; font-weight:700; color:var(--texto); margin:0; letter-spacing:-.3px; }
.confirm-body { padding:20px 32px; }
.warn-box { background:#FFFBF3; border-left:3px solid var(--amarillo); border-radius:0 10px 10px 0; padding:14px 16px; font-size:13px; color:var(--texto); line-height:1.7; }
.warn-box p { margin:0 0 6px; }
.warn-box p:last-child { margin:0; }
.confirm-footer { flex-shrink:0; display:flex; justify-content:flex-end; gap:10px; padding:14px 32px 18px; border-top:1px solid var(--borde-suave); }
.btn-danger { height:38px; padding:0 16px; border-radius:9px; background:var(--rojo-bg); border:none; color:var(--rojo); font-size:13px; font-weight:600; cursor:pointer; transition:background-color .16s ease, color .16s ease; }
.btn-danger:hover { background:var(--rojo); color:#fff; }
/* Modal cambiar estado (sin cambios de diseño) */
.modal-header { display:flex; align-items:center; gap:14px; margin-bottom:24px; padding-bottom:20px; border-bottom:1px solid var(--borde-suave); }
.modal-header-info { flex:1; min-width:0; }
.modal-eyebrow { font-size:10.5px; font-weight:700; color:var(--verde-sec); text-transform:uppercase; letter-spacing:0.7px; margin:0 0 4px; }
.modal-title { font-size:19px; font-weight:700; color:var(--texto); letter-spacing:-0.4px; margin:0; }
.modal-section { margin-bottom:24px; }
.modal-acciones { display:flex; gap:10px; justify-content:flex-end; padding-top:20px; border-top:1px solid var(--borde-suave); }
.status-options { display:flex; flex-direction:column; gap:8px; margin-bottom:8px; }
.status-option { display:flex; align-items:center; gap:12px; padding:11px 14px; border-radius:10px; border:1px solid var(--borde); cursor:pointer; transition:border-color .16s ease, background-color .16s ease; }
.status-option input[type="radio"] { display:none; }
.status-option:hover { border-color:var(--verde-sec); background:#FAFCFA; }
.status-option.selected { border-color:var(--verde-sec); background:rgba(146,168,148,.07); }
.status-desc { font-size:13px; color:var(--texto-ter); }
/* Animaciones modal */
.modal-fade-enter-active, .modal-fade-leave-active { transition:opacity 0.22s ease; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity:0; }
/* ── Responsive ── */
@media (max-width:1100px) { .don-summary { grid-template-columns:repeat(3, 1fr); } }
@media (max-width:900px) {
  .don-summary { grid-template-columns:repeat(2, 1fr); }
  .form-grid { grid-template-columns:repeat(2, 1fr); }
  .fg--span2 { grid-column:span 1; }
  .modal-box--uniform { width:94vw; height:88vh; }
  .grid-2col { grid-template-columns:1fr; }
  .fields-row { grid-template-columns:repeat(2, 1fr); }
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
  .hero, .form-header, .form-body, .tabs, .body, .footer, .form-footer, .confirm-header, .confirm-body, .confirm-footer { padding-left:20px; padding-right:20px; }
  .fields-row { grid-template-columns:1fr; }
}
@media (max-width:480px) { .don-summary { grid-template-columns:1fr; } }

/* ══════════════════════════════════════════════
   WIZARD PASO A PASO (mismo patrón que SaludAdminView.vue)
   ══════════════════════════════════════════════ */
.wiz-steps { position:relative; display:flex; justify-content:space-between; gap:6px; margin-top:18px; }
.wiz-track { position:absolute; top:15px; left:6%; right:6%; height:2px; background:var(--borde-suave); border-radius:2px; }
.wiz-track-fill { height:100%; background:var(--verde); border-radius:2px; transition:width .32s cubic-bezier(.4,0,.2,1); }
.wiz-step { position:relative; z-index:1; flex:1; display:flex; flex-direction:column; align-items:center; gap:7px; background:transparent; border:none; padding:0; cursor:pointer; font-family:inherit; min-width:0; }
.wiz-step.is-locked { cursor:default; }
.wiz-bullet { width:32px; height:32px; border-radius:50%; display:flex; align-items:center; justify-content:center; font-size:12px; font-weight:800; background:var(--blanco); border:2px solid var(--borde-suave); color:var(--texto-sec); transition:all .22s ease; flex-shrink:0; }
.wiz-step-label { font-size:11px; font-weight:700; color:var(--texto-sec); text-align:center; letter-spacing:.2px; transition:color .22s ease; max-width:100%; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; }
.wiz-step.is-done .wiz-bullet { background:var(--verde); border-color:var(--verde); color:#FFFFFF; }
.wiz-step.is-done .wiz-step-label { color:var(--verde); }
.wiz-step.is-active .wiz-bullet { background:var(--verde); border-color:var(--verde); color:#FFFFFF; box-shadow:0 0 0 4px rgba(58,71,60,.12); }
.wiz-step.is-active .wiz-step-label { color:var(--verde); font-weight:800; }
.wiz-step:not(.is-locked):not(.is-active):hover .wiz-bullet { border-color:var(--verde-sec); }

.wiz-context { display:flex; align-items:center; gap:7px; margin-top:16px; font-size:12px; color:var(--texto-sec); flex-wrap:wrap; }
.wiz-context-count { font-weight:800; color:var(--verde); text-transform:uppercase; letter-spacing:.5px; font-size:11px; }
.wiz-context-sep { opacity:.5; }
.wiz-context-desc { font-weight:500; }

.wiz-body { min-height:260px; }
.wiz-pane { animation:wiz-in .26s ease; }
@keyframes wiz-in { from { opacity:0; transform:translateX(10px); } to { opacity:1; transform:translateX(0); } }

.wiz-resumen { display:grid; grid-template-columns:repeat(2,1fr); gap:14px; }
.wiz-res-card { border:1.5px solid var(--borde-suave); border-radius:12px; padding:16px; background:var(--blanco); }
.wiz-res-card--full { grid-column:1 / -1; }
.wiz-res-head { display:flex; align-items:center; justify-content:space-between; gap:10px; margin-bottom:12px; padding-bottom:10px; border-bottom:1px solid var(--borde-suave); }
.wiz-res-title { font-size:11px; font-weight:800; color:var(--verde); text-transform:uppercase; letter-spacing:.5px; }
.wiz-res-edit { border:none; background:transparent; color:var(--texto-sec); font-size:11px; font-weight:700; cursor:pointer; font-family:inherit; text-decoration:underline; padding:0; }
.wiz-res-edit:hover { color:var(--verde); }
.wiz-res-sub { display:block; font-size:12px; color:var(--texto-sec); }
.wiz-res-list { display:grid; grid-template-columns:repeat(2,1fr); gap:12px; margin:0; }
.wiz-res-list > div { min-width:0; }
.wiz-res-full { grid-column:1 / -1; }
.wiz-res-list dt { font-size:10px; font-weight:700; color:var(--texto-sec); text-transform:uppercase; letter-spacing:.4px; }
.wiz-res-list dd { font-size:13px; font-weight:600; color:var(--texto); margin:3px 0 0; word-break:break-word; }

.wiz-footer { justify-content:space-between; align-items:center; }
.wiz-nav { display:flex; gap:10px; }
.btn-back { display:flex; align-items:center; gap:6px; }

@media (max-width:768px) {
  .wiz-step-label { display:none; }
  .wiz-steps { justify-content:center; gap:0; }
  .wiz-track { top:15px; left:10%; right:10%; }
  .wiz-resumen { grid-template-columns:1fr; }
  .wiz-footer { flex-direction:column-reverse; align-items:stretch; gap:8px; }
  .wiz-nav { width:100%; }
  .wiz-nav .btn-cancel, .wiz-nav .btn-save { flex:1; justify-content:center; }
}
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