<script setup>
import { ref, computed, watch } from 'vue'
import { ubicacionesCR } from '../../data/ubicaciones'
import { usePetsStore } from '../../stores/usePetsStore'

/* ─── Store de mascotas ─────────────────────────────────── */
const petsStore = usePetsStore()

/* ─── Estado principal ──────────────────────────────────── */
const rescates = ref([])

/* ─── UI ─────────────────────────────────────────────────── */
const showForm        = ref(false)   // oculta tabla y filtros
const editMode        = ref(false)
const showEditModal   = ref(false)
const rescueIndex     = ref(null)
const showDetailModal = ref(false)
const rescueSelected  = ref(null)
const modalConfirm    = ref(false)
const confirmIndex    = ref(null)

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
      (r.id || '').toLowerCase().includes(q) ||
      (r.mascota || '').toLowerCase().includes(q) ||
      (r.rescatista || '').toLowerCase().includes(q)

    const coincideProv =
      filtroProv.value === 'Todos' ||
      (r.provincia || '') === filtroProv.value

    const coincideEstado =
      filtroEstado.value === 'Todos' ||
      r.estado === filtroEstado.value

    return coincideSearch && coincideProv && coincideEstado
  })
})

/* ─── Carga ──────────────────────────────────────────────── */
function cargarRescates() {
  rescates.value = JSON.parse(localStorage.getItem('anhelo_rescates')) || []
}
cargarRescates()

function guardarRescatesLS() {
  localStorage.setItem('anhelo_rescates', JSON.stringify(rescates.value))
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
const mascota     = ref('')
const tipoMascota = ref('')

const fotoPreview = ref('')
const fotoFile    = ref(null)

const edad        = ref('')
const sexo        = ref('')
const tieneRaza   = ref('No')
const raza        = ref('')
const fechaRescate = ref('')
const descripcion  = ref('')
const casaCuna     = ref('')
const rescatista   = ref('')
const estado       = ref('Activo')
const formErrors   = ref([])

/* ─── Manejo de foto ─────────────────────────────────────── */
function onFotoChange(e) {
  const file = e.target.files?.[0]
  if (!file) return
  fotoFile.value = file
  const reader = new FileReader()
  reader.onload = ev => { fotoPreview.value = ev.target.result }
  reader.readAsDataURL(file)
}

/* ─── Validación ─────────────────────────────────────────── */
function validar() {
  const errores = []
  if (!mascota.value.trim())  errores.push('Nombre de la mascota')
  if (!tipoMascota.value)     errores.push('Tipo de mascota')
  if (!fotoPreview.value)     errores.push('Fotografía principal')
  if (!edad.value.trim())     errores.push('Edad')
  if (!sexo.value)            errores.push('Sexo')
  if (!fechaRescate.value)    errores.push('Fecha de rescate')
  if (!provincia.value)       errores.push('Provincia')
  if (!canton.value)          errores.push('Cantón')
  if (!distrito.value)        errores.push('Distrito')
  if (!descripcion.value.trim()) errores.push('Descripción del rescate')
  if (!rescatista.value)      errores.push('Rescatista')
  formErrors.value = errores
  return errores.length === 0
}

/* ─── Obtener fecha actual ───────────────────────────────── */
function obtenerFechaActual() {
  return new Date().toLocaleString('es-CR', {
    year:'numeric', month:'2-digit', day:'2-digit',
    hour:'2-digit', minute:'2-digit'
  })
}

/* ─── Guardar rescate + crear mascota automáticamente ────── */
function guardarRescate() {
  if (!validar()) {
    mostrarToast('Completa todos los campos obligatorios.', 'error')
    return
  }

  const razaFinal = tieneRaza.value === 'Si' ? raza.value : 'Sin raza'

  if (editMode.value) {
    // ── EDITAR ──
    const orig = rescates.value[rescueIndex.value]
    rescates.value[rescueIndex.value] = {
      ...orig,
      mascota:     mascota.value,
      tipoMascota: tipoMascota.value,
      foto:        fotoPreview.value,
      edad:        edad.value,
      sexo:        sexo.value,
      raza:        razaFinal,
      fechaRescate: fechaRescate.value,
      provincia:   provincia.value,
      canton:      canton.value,
      distrito:    distrito.value,
      ubicacion:   `${provincia.value} · ${canton.value} · ${distrito.value}`,
      descripcion: descripcion.value,
      casaCuna:    casaCuna.value || 'Sin asignar',
      rescatista:  rescatista.value,
      estado:      estado.value
    }

    // Actualizar mascota en petsStore si existe
    if (orig.mascotaId) {
      const petIndex = petsStore.pets.findIndex(p => p.id === orig.mascotaId)
      if (petIndex !== -1) {
        petsStore.pets[petIndex] = {
          ...petsStore.pets[petIndex],
          name:   mascota.value,
          type:   tipoMascota.value,
          image:  fotoPreview.value,
          age:    edad.value,
          gender: sexo.value,
          breed:  razaFinal !== 'Sin raza' ? razaFinal : '',
        }
        petsStore.savePets?.()
      }
    }

    editMode.value    = false
    rescueIndex.value = null
    showEditModal.value = false

  } else {
    // ── NUEVO ──
    // 1. Crear mascota en petsStore
const nuevaMascota = {
  id: `pet-${Date.now()}`,
  name: mascota.value,
  type: tipoMascota.value,

  images: [
    {
      preview: fotoPreview.value
    }
  ],

  image: fotoPreview.value,

  age: edad.value,
  gender: sexo.value,
  breed: razaFinal !== 'Sin raza' ? razaFinal : '',
  status: 'En rescate',
  description: descripcion.value,
  location: `${provincia.value}, ${canton.value}`,
  createdAt: obtenerFechaActual()
}

    console.log('Mascota a guardar:', nuevaMascota)
    console.log('Foto:', fotoPreview.value)

    // Soporte para distintas implementaciones del store
    if (typeof petsStore.addPet === 'function') {
      petsStore.addPet(nuevaMascota)
    } else if (Array.isArray(petsStore.pets)) {
      petsStore.pets.unshift(nuevaMascota)
      if (typeof petsStore.savePets === 'function') petsStore.savePets()
    }

    // 2. Crear rescate vinculado
    const id = `R-${String(rescates.value.length + 1).padStart(3, '0')}`
    rescates.value.unshift({
      id,
      mascotaId:    nuevaMascota.id,
      mascota:      mascota.value,
      tipoMascota:  tipoMascota.value,
      foto:         fotoPreview.value,
      edad:         edad.value,
      sexo:         sexo.value,
      raza:         razaFinal,
      fechaRescate: fechaRescate.value,
      fechaCreacion: obtenerFechaActual(),
      creadoPor:    usuarioActual.value.nombre,
      provincia:    provincia.value,
      canton:       canton.value,
      distrito:     distrito.value,
      ubicacion:    `${provincia.value} · ${canton.value} · ${distrito.value}`,
      descripcion:  descripcion.value,
      casaCuna:     casaCuna.value || 'Sin asignar',
      rescatista:   rescatista.value,
      estado:       estado.value
    })
  }

  guardarRescatesLS()
  limpiarFormulario()
  showForm.value = false
  mostrarToast(editMode.value ? 'Rescate actualizado.' : 'Rescate registrado y mascota creada correctamente.')
}

/* ─── Editar ─────────────────────────────────────────────── */
function editarRescate(index) {
  const r = rescates.value[index]
  mascota.value      = r.mascota
tipoMascota.value  = r.tipoMascota || ''
  fotoPreview.value  = r.foto || ''
  edad.value         = r.edad
  sexo.value         = r.sexo
  fechaRescate.value = r.fechaRescate
  descripcion.value  = r.descripcion
  casaCuna.value     = r.casaCuna
  rescatista.value   = r.rescatista
  estado.value       = r.estado
  tieneRaza.value    = (r.raza && r.raza !== 'Sin raza') ? 'Si' : 'No'
  raza.value         = (r.raza && r.raza !== 'Sin raza') ? r.raza : ''
  provincia.value    = r.provincia || ''
  canton.value       = r.canton    || ''
  distrito.value     = r.distrito  || ''
  rescueIndex.value  = index
  editMode.value     = true
  showEditModal.value = true
}

/* ─── Cerrar rescate (con confirmación) ──────────────────── */
function pedirCerrar(index) {
  confirmIndex.value = index
  modalConfirm.value = true
}
function confirmarCerrar() {
  rescates.value[confirmIndex.value].estado = 'Cerrado'
  guardarRescatesLS()
  modalConfirm.value = false
  confirmIndex.value = null
  mostrarToast('Rescate cerrado correctamente.')
}

/* ─── Ver detalle ────────────────────────────────────────── */
function verDetalle(rescate) {
  rescueSelected.value  = rescate
  showDetailModal.value = true
}

/* ─── Limpiar formulario ─────────────────────────────────── */
function limpiarFormulario() {
  mascota.value      = ''
tipoMascota.value  = ''
  fotoPreview.value  = ''
  fotoFile.value     = null
  edad.value         = ''
  sexo.value         = ''
  tieneRaza.value    = 'No'
  raza.value         = ''
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

/* ─── Badge helpers ──────────────────────────────────────── */
function estadoBadgeClass(estado) {
  return {
    'Activo':  'badge--green',
    'Cerrado': 'badge--neutral',
  }[estado] || 'badge--neutral'
}
function estadoIcon(estado) {
  return { 'Activo': '●', 'Cerrado': '◉' }[estado] || '●'
}

function iniciales(nombre) {
  if (!nombre) return '?'
  return nombre.trim().split(' ').map(p => p[0]).slice(0, 2).join('').toUpperCase()
}
</script>

<template>
  <div class="sc-root">

    <!-- ── Toast ── -->
    <Teleport to="body">
      <Transition name="toast-anim">
        <div v-if="toast.visible" class="sc-toast" :class="toast.tipo === 'error' ? 'error' : 'success'">
          <svg v-if="toast.tipo === 'exito'" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
          <svg v-else width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
          {{ toast.texto }}
        </div>
      </Transition>
    </Teleport>

    <!-- ══════════════════════════════════════════
         VISTA TABLA (cuando showForm = false)
    ═══════════════════════════════════════════ -->
    <template v-if="!showForm">

      <!-- ── Header ── -->
      <header class="sc-header">
        <div class="sc-header-left">
          <h1 class="sc-title">Rescates</h1>
          <p class="sc-sub">Registro y seguimiento de animales rescatados</p>
        </div>
        <button class="sc-btn-nuevo" @click="abrirNuevo">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
          Nuevo rescate
        </button>
      </header>

      <!-- ── Toolbar / Filtros ── -->
      <div class="sc-toolbar">
        <div class="sc-filters">

          <!-- Búsqueda -->
          <div class="sc-search-wrap">
            <svg class="sc-search-icon" xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
            <input class="sc-search" v-model="filtroSearch" placeholder="ID, mascota o rescatista..." />
          </div>

          <!-- Filtro provincia -->
          <div class="sc-select-wrap">
            <select class="sc-filter-select" v-model="filtroProv">
              <option value="Todos">Provincia: Todas</option>
              <option v-for="p in provinciasDisponibles" :key="p" :value="p">{{ p }}</option>
            </select>
            <svg class="sc-select-icon" xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
          </div>

          <!-- Filtro estado -->
          <div class="sc-select-wrap">
            <select class="sc-filter-select" v-model="filtroEstado">
              <option value="Todos">Todos los estados</option>
              <option value="Activo">Activos</option>
              <option value="Cerrado">Cerrados</option>
            </select>
            <svg class="sc-select-icon" xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
          </div>

          <button v-if="hayFiltros" class="sc-clear" @click="limpiarFiltros">Limpiar</button>
        </div>
      </div>

      <!-- ── Tabla ── -->
      <div class="sc-table-wrap">
        <table class="sc-table">
          <thead>
            <tr>
              <th style="width:80px">ID</th>
              <th style="width:220px">Mascota</th>
              <th style="width:180px">Rescatista</th>
              <th style="width:140px">Provincia</th>
              <th style="width:180px">Casa cuna</th>
              <th style="width:110px">Estado</th>
              <th style="width:130px">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(r, index) in rescatesFiltrados" :key="r.id">

              <!-- ID -->
              <td>
                <span class="sc-pet-id">{{ r.id }}</span>
              </td>

              <!-- Mascota -->
              <td>
                <div class="sc-pet-cell">
                  <div class="sc-avatar">
                    <img v-if="r.foto" :src="r.foto" class="sc-avatar-img" alt="foto">
                    <span v-else class="sc-avatar-ini">{{ iniciales(r.mascota) }}</span>
                  </div>
                  <div class="sc-pet-info">
                    <span class="sc-pet-name">{{ r.mascota }}</span>
                    <span class="sc-pet-sub">{{ r.edad }} · {{ r.sexo }}</span>
                  </div>
                </div>
              </td>

              <!-- Rescatista -->
              <td>
                <span class="sc-td-main">{{ r.rescatista || '—' }}</span>
              </td>

              <!-- Provincia -->
              <td class="sc-td-sec">
                {{ r.provincia || '—' }}
              </td>

              <!-- Casa cuna -->
              <td class="sc-td-sec">
                {{ r.casaCuna || 'Sin asignar' }}
              </td>

              <!-- Estado -->
              <td>
                <span class="sc-badge" :class="estadoBadgeClass(r.estado)">
                  {{ estadoIcon(r.estado) }} {{ r.estado }}
                </span>
              </td>

              <!-- Acciones -->
              <td>
                <div class="sc-actions">
                  <button class="sc-btn-ver sc-btn-ver--neutral" title="Ver detalle" @click="verDetalle(r)">
                    <img src="/img-acciones/eye.png" class="action-icon" alt="Ver">
                  </button>
                  <button class="sc-btn-ver sc-btn-ver--blue" title="Editar" @click="editarRescate(rescates.indexOf(r))">
                    <img src="/img-acciones/edit.png" class="action-icon" alt="Editar">
                  </button>
                  <button
                    class="sc-btn-ver sc-btn-ver--red"
                    title="Cerrar rescate"
                    :disabled="r.estado === 'Cerrado'"
                    @click="pedirCerrar(rescates.indexOf(r))"
                  >
                    <img src="/img-acciones/close.png" class="action-icon" alt="Cerrar">
                  </button>
                </div>
              </td>

            </tr>

            <tr v-if="rescatesFiltrados.length === 0">
              <td colspan="7" class="sc-empty">
                <div class="sc-empty-inner">
                  <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78L12 21.23l8.84-8.84a5.5 5.5 0 0 0 0-7.78z"/></svg>
                  <p>{{ hayFiltros ? 'Sin resultados para los filtros aplicados' : 'No hay rescates registrados' }}</p>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

    </template>

    <!-- ══════════════════════════════════════════
         VISTA FORMULARIO (cuando showForm = true)
    ═══════════════════════════════════════════ -->
    <template v-else>

      <!-- Header del formulario -->
      <header class="sc-header">
        <div class="sc-header-left">
          <h1 class="sc-title">{{ editMode ? 'Editar rescate' : 'Nuevo rescate' }}</h1>
          <p class="sc-sub">{{ editMode ? 'Modifica la información del rescate' : 'Registra un nuevo animal rescatado' }}</p>
        </div>
        <button class="sc-btn-cancel-header" @click="cancelarFormulario">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
          Cancelar
        </button>
      </header>

      <!-- Errores de validación -->
      <div v-if="formErrors.length > 0" class="sc-form-errors">
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
        <span>Campos obligatorios incompletos: {{ formErrors.join(', ') }}.</span>
      </div>

      <!-- Panel del formulario -->
      <div class="sc-form-panel">

        <!-- Sección 1: Datos de la mascota -->
        <div class="sc-section-label">
          <span class="sc-section-num">1</span>
          Datos de la mascota
        </div>

        <!-- Foto -->
        <div class="sc-form-grid sc-form-grid--4" style="margin-bottom:20px">
          <div class="sc-fg sc-fg--full">
            <label>Fotografía principal <span class="sc-required">*</span></label>
            <div class="sc-foto-wrap">
              <div class="sc-foto-preview" :class="{ 'has-img': fotoPreview }">
                <img v-if="fotoPreview" :src="fotoPreview" class="sc-foto-img" alt="preview">
                <div v-else class="sc-foto-placeholder">
                  <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="18" height="18" rx="2"/><circle cx="8.5" cy="8.5" r="1.5"/><polyline points="21 15 16 10 5 21"/></svg>
                  <span>Sin fotografía</span>
                </div>
              </div>
              <div class="sc-foto-actions">
                <label class="sc-btn-foto">
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="17 8 12 3 7 8"/><line x1="12" y1="3" x2="12" y2="15"/></svg>
                  Subir foto
                  <input type="file" accept="image/*" style="display:none" @change="onFotoChange">
                </label>
                <p class="sc-foto-hint">JPG, PNG o WEBP. Se usará en Animales también.</p>
              </div>
            </div>
          </div>
        </div>

        <div class="sc-form-grid sc-form-grid--4">
          <div class="sc-fg sc-fg--span2">
            <label>Nombre de la mascota <span class="sc-required">*</span></label>
            <input class="sc-input" v-model="mascota" placeholder="Ej. Luna">
          </div>
          <div class="sc-fg">
  <label>Tipo de mascota <span class="sc-required">*</span></label>
  <div class="select-wrap">
    <select class="sc-input" v-model="tipoMascota">
      <option value="">Seleccione</option>
      <option value="Perro">Perro</option>
      <option value="Gato">Gato</option>
    </select>
    <i class='bx bx-chevron-down'></i>
  </div>
</div>
          <div class="sc-fg">
            <label>Edad <span class="sc-required">*</span></label>
            <input class="sc-input" v-model="edad" placeholder="Ej. 2 años">
          </div>
          <div class="sc-fg">
            <label>Sexo <span class="sc-required">*</span></label>
            <div class="select-wrap">
              <select class="sc-input" v-model="sexo">
                <option value="">Seleccione</option>
                <option>Macho</option>
                <option>Hembra</option>
              </select>
              <i class='bx bx-chevron-down'></i>
            </div>
          </div>
          <div class="sc-fg">
            <label>¿Tiene raza?</label>
            <div class="radio-row">
              <label class="r-opt"><input type="radio" v-model="tieneRaza" value="No"><span>No</span></label>
              <label class="r-opt"><input type="radio" v-model="tieneRaza" value="Si"><span>Sí</span></label>
            </div>
          </div>
          <div v-if="tieneRaza === 'Si'" class="sc-fg sc-fg--span2">
            <label>Raza</label>
            <input class="sc-input" v-model="raza" placeholder="Ej. Labrador">
          </div>
          <div class="sc-fg">
            <label>Fecha de rescate <span class="sc-required">*</span></label>
            <input type="date" class="sc-input" v-model="fechaRescate">
          </div>
        </div>

        <!-- Sección 2: Ubicación -->
        <div class="sc-section-label" style="margin-top:28px">
          <span class="sc-section-num">2</span>
          Ubicación del rescate
        </div>
        <div class="sc-form-grid sc-form-grid--4">
          <div class="sc-fg">
            <label>Provincia <span class="sc-required">*</span></label>
            <div class="select-wrap">
              <select class="sc-input" v-model="provincia">
                <option value="">Seleccione</option>
                <option v-for="p in provinciasDisponibles" :key="p" :value="p">{{ p }}</option>
              </select>
              <i class='bx bx-chevron-down'></i>
            </div>
          </div>
          <div class="sc-fg">
            <label>Cantón <span class="sc-required">*</span></label>
            <div class="select-wrap">
              <select class="sc-input" v-model="canton" :disabled="!provincia">
                <option value="">Seleccione</option>
                <option v-for="c in cantonesDisponibles" :key="c" :value="c">{{ c }}</option>
              </select>
              <i class='bx bx-chevron-down'></i>
            </div>
          </div>
          <div class="sc-fg">
            <label>Distrito <span class="sc-required">*</span></label>
            <div class="select-wrap">
              <select class="sc-input" v-model="distrito" :disabled="!canton">
                <option value="">Seleccione</option>
                <option v-for="d in distritosDisponibles" :key="d" :value="d">{{ d }}</option>
              </select>
              <i class='bx bx-chevron-down'></i>
            </div>
          </div>
        </div>

        <!-- Sección 3: Asignaciones -->
        <div class="sc-section-label" style="margin-top:28px">
          <span class="sc-section-num">3</span>
          Asignaciones
        </div>
        <div class="sc-form-grid sc-form-grid--4">
          <div class="sc-fg sc-fg--span2">
            <label>Rescatista <span class="sc-required">*</span></label>
            <div class="select-wrap">
              <select class="sc-input" v-model="rescatista">
                <option value="">Seleccione un rescatista</option>
                <option
                  v-for="r in rescatistasDisponibles"
                  :key="r.id"
                  :value="r.solicitudVoluntario?.nombre || r.nombre"
                >{{ r.solicitudVoluntario?.nombre || r.nombre }}</option>
              </select>
              <i class='bx bx-chevron-down'></i>
            </div>
          </div>
          <div class="sc-fg sc-fg--span2">
            <label>Casa cuna</label>
            <div class="select-wrap">
              <select class="sc-input" v-model="casaCuna">
                <option value="">Sin asignar</option>
                <option
                  v-for="c in casasCunaDisponibles"
                  :key="c.id"
                  :value="c.solicitudVoluntario?.nombre || c.nombre"
                >{{ c.solicitudVoluntario?.nombre || c.nombre }}</option>
              </select>
              <i class='bx bx-chevron-down'></i>
            </div>
          </div>
          <div class="sc-fg">
            <label>Estado</label>
            <div class="select-wrap">
              <select class="sc-input" v-model="estado">
                <option>Activo</option>
                <option>Cerrado</option>
              </select>
              <i class='bx bx-chevron-down'></i>
            </div>
          </div>
        </div>

        <!-- Sección 4: Descripción -->
        <div class="sc-section-label" style="margin-top:28px">
          <span class="sc-section-num">4</span>
          Descripción del rescate
        </div>
        <div class="sc-fg">
          <label>Descripción <span class="sc-required">*</span></label>
          <textarea class="sc-textarea" v-model="descripcion" placeholder="Describe las circunstancias del rescate, condición del animal, observaciones importantes..."></textarea>
        </div>

        <!-- Acciones del formulario -->
        <div class="sc-form-footer">
          <button class="sc-btn-cancel" @click="cancelarFormulario">Cancelar</button>
          <button class="sc-btn-save" @click="guardarRescate">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
            {{ editMode ? 'Guardar cambios' : 'Registrar rescate' }}
          </button>
        </div>

      </div>
    </template>

    <!-- ══════════════════════════════════════════
         MODAL EDITAR (desde tabla)
    ═══════════════════════════════════════════ -->
    <Teleport to="body">
      <Transition name="overlay-anim">
        <div v-if="showEditModal" class="sc-overlay" @click.self="showEditModal = false">
          <div class="sc-modal sc-modal--lg">
            <div class="sc-modal-header">
              <div class="edit-header-info">
                <div class="edit-avatar-sm">{{ iniciales(mascota) }}</div>
                <div>
                  <p class="sc-modal-eyebrow">Rescate</p>
                  <h2 class="sc-modal-title">{{ mascota || 'Sin nombre' }}</h2>
                </div>
              </div>
              <button class="sc-modal-close" @click="showEditModal = false">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
              </button>
            </div>
            <div class="sc-modal-body edit-body">

              <!-- Foto -->
              <div class="sc-section-label"><span class="sc-section-num">1</span>Fotografía</div>
              <div class="sc-foto-wrap" style="margin-bottom:20px">
                <div class="sc-foto-preview" :class="{ 'has-img': fotoPreview }">
                  <img v-if="fotoPreview" :src="fotoPreview" class="sc-foto-img" alt="preview">
                  <div v-else class="sc-foto-placeholder">
                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="18" height="18" rx="2"/><circle cx="8.5" cy="8.5" r="1.5"/><polyline points="21 15 16 10 5 21"/></svg>
                    <span>Sin foto</span>
                  </div>
                </div>
                <div class="sc-foto-actions">
                  <label class="sc-btn-foto">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="17 8 12 3 7 8"/><line x1="12" y1="3" x2="12" y2="15"/></svg>
                    Cambiar foto
                    <input type="file" accept="image/*" style="display:none" @change="onFotoChange">
                  </label>
                </div>
              </div>

              <div class="sc-section-label"><span class="sc-section-num">2</span>Datos personales</div>
              <div class="sc-form-grid sc-form-grid--4">
                <div class="sc-fg sc-fg--span2"><label>Nombre</label><div class="sc-fg">

    <label>Tipo de mascota</label>

    <div class="select-wrap">

      <select class="sc-input" v-model="tipoMascota">

        <option value="Perro">Perro</option>

        <option value="Gato">Gato</option>

      </select>

      <i class='bx bx-chevron-down'></i>

    </div>

  </div><input class="sc-input" v-model="mascota"></div>
                <div class="sc-fg"><label>Edad</label><input class="sc-input" v-model="edad"></div>
                <div class="sc-fg"><label>Sexo</label><div class="select-wrap"><select class="sc-input" v-model="sexo"><option>Macho</option><option>Hembra</option></select><i class='bx bx-chevron-down'></i></div></div>
                <div class="sc-fg"><label>¿Tiene raza?</label><div class="radio-row"><label class="r-opt"><input type="radio" v-model="tieneRaza" value="No"><span>No</span></label><label class="r-opt"><input type="radio" v-model="tieneRaza" value="Si"><span>Sí</span></label></div></div>
                <div v-if="tieneRaza === 'Si'" class="sc-fg sc-fg--span2"><label>Raza</label><input class="sc-input" v-model="raza"></div>
                <div class="sc-fg"><label>Fecha de rescate</label><input type="date" class="sc-input" v-model="fechaRescate"></div>
              </div>

              <div class="sc-section-label" style="margin-top:20px"><span class="sc-section-num">3</span>Ubicación</div>
              <div class="sc-form-grid sc-form-grid--4">
                <div class="sc-fg"><label>Provincia</label><div class="select-wrap"><select class="sc-input" v-model="provincia"><option value="">Seleccione</option><option v-for="p in provinciasDisponibles" :key="p" :value="p">{{ p }}</option></select><i class='bx bx-chevron-down'></i></div></div>
                <div class="sc-fg"><label>Cantón</label><div class="select-wrap"><select class="sc-input" v-model="canton" :disabled="!provincia"><option value="">Seleccione</option><option v-for="c in cantonesDisponibles" :key="c" :value="c">{{ c }}</option></select><i class='bx bx-chevron-down'></i></div></div>
                <div class="sc-fg"><label>Distrito</label><div class="select-wrap"><select class="sc-input" v-model="distrito" :disabled="!canton"><option value="">Seleccione</option><option v-for="d in distritosDisponibles" :key="d" :value="d">{{ d }}</option></select><i class='bx bx-chevron-down'></i></div></div>
              </div>

              <div class="sc-section-label" style="margin-top:20px"><span class="sc-section-num">4</span>Asignaciones</div>
              <div class="sc-form-grid sc-form-grid--4">
                <div class="sc-fg sc-fg--span2"><label>Rescatista</label><div class="select-wrap"><select class="sc-input" v-model="rescatista"><option value="">Seleccione</option><option v-for="r in rescatistasDisponibles" :key="r.id" :value="r.solicitudVoluntario?.nombre || r.nombre">{{ r.solicitudVoluntario?.nombre || r.nombre }}</option></select><i class='bx bx-chevron-down'></i></div></div>
                <div class="sc-fg sc-fg--span2"><label>Casa cuna</label><div class="select-wrap"><select class="sc-input" v-model="casaCuna"><option value="">Sin asignar</option><option v-for="c in casasCunaDisponibles" :key="c.id" :value="c.solicitudVoluntario?.nombre || c.nombre">{{ c.solicitudVoluntario?.nombre || c.nombre }}</option></select><i class='bx bx-chevron-down'></i></div></div>
                <div class="sc-fg"><label>Estado</label><div class="select-wrap"><select class="sc-input" v-model="estado"><option>Activo</option><option>Cerrado</option></select><i class='bx bx-chevron-down'></i></div></div>
              </div>

              <div class="sc-section-label" style="margin-top:20px"><span class="sc-section-num">5</span>Descripción</div>
              <div class="sc-fg"><label>Descripción del rescate</label><textarea class="sc-textarea" v-model="descripcion"></textarea></div>

            </div>
            <div class="sc-modal-footer">
              <button class="sc-btn-cancel" @click="showEditModal = false">Cancelar</button>
              <button class="sc-btn-save" @click="guardarRescate">
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
      <Transition name="overlay-anim">
        <div v-if="showDetailModal" class="sc-overlay" @click.self="showDetailModal = false">
          <div class="sc-modal sc-modal--lg">

            <div class="exp-header">
              <div class="exp-avatar">
                <img v-if="rescueSelected?.foto" :src="rescueSelected.foto" class="exp-avatar-img" alt="foto">
                <span v-else class="exp-avatar-ini">{{ iniciales(rescueSelected?.mascota) }}</span>
              </div>
              <div class="exp-header-info">
                <div class="exp-name">{{ rescueSelected?.mascota }}</div>
                <div class="exp-meta">
                  <span class="sc-badge badge--neutral">{{ rescueSelected?.edad }} · {{ rescueSelected?.sexo }}</span>
                  <span class="sc-badge" :class="estadoBadgeClass(rescueSelected?.estado)">
                    {{ estadoIcon(rescueSelected?.estado) }} {{ rescueSelected?.estado }}
                  </span>
                  <span class="sc-badge badge--neutral">{{ rescueSelected?.id }}</span>
                </div>
              </div>
              <button class="sc-modal-close" @click="showDetailModal = false">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
              </button>
            </div>

            <div class="sc-modal-body exp-body" v-if="rescueSelected">

              <!-- Información de la mascota -->
              <div class="exp-section">
                <div class="exp-section-title"><span class="exp-section-dot"></span>Información de la mascota</div>
                <div class="exp-grid">
                  <div class="exp-field"><span class="exp-label">Nombre</span><span class="exp-value fw">{{ rescueSelected.mascota }}</span></div>
                  <div class="exp-field"><span class="exp-label">Edad</span><span class="exp-value">{{ rescueSelected.edad }}</span></div>
                  <div class="exp-field"><span class="exp-label">Sexo</span><span class="exp-value">{{ rescueSelected.sexo }}</span></div>
                  <div class="exp-field"><span class="exp-label">Raza</span><span class="exp-value">{{ rescueSelected.raza || 'Sin raza' }}</span></div>
                </div>
              </div>

              <!-- Ubicación -->
              <div class="exp-section">
                <div class="exp-section-title"><span class="exp-section-dot"></span>Ubicación</div>
                <div class="exp-grid cols-3">
                  <div class="exp-field"><span class="exp-label">Provincia</span><span class="exp-value">{{ rescueSelected.provincia || '—' }}</span></div>
                  <div class="exp-field"><span class="exp-label">Cantón</span><span class="exp-value">{{ rescueSelected.canton || '—' }}</span></div>
                  <div class="exp-field"><span class="exp-label">Distrito</span><span class="exp-value">{{ rescueSelected.distrito || '—' }}</span></div>
                </div>
              </div>

              <!-- Asignaciones y fechas -->
              <div class="exp-section">
                <div class="exp-section-title"><span class="exp-section-dot"></span>Asignaciones y fechas</div>
                <div class="exp-grid">
                  <div class="exp-field"><span class="exp-label">Rescatista</span><span class="exp-value fw">{{ rescueSelected.rescatista || '—' }}</span></div>
                  <div class="exp-field"><span class="exp-label">Casa cuna</span><span class="exp-value">{{ rescueSelected.casaCuna || 'Sin asignar' }}</span></div>
                  <div class="exp-field"><span class="exp-label">Fecha de rescate</span><span class="exp-value">{{ rescueSelected.fechaRescate || '—' }}</span></div>
                  <div class="exp-field"><span class="exp-label">Fecha de creación</span><span class="exp-value">{{ rescueSelected.fechaCreacion || '—' }}</span></div>
                  <div class="exp-field"><span class="exp-label">Registrado por</span><span class="exp-value">{{ rescueSelected.creadoPor || '—' }}</span></div>
                  <div class="exp-field"><span class="exp-label">Estado</span>
                    <span class="sc-badge" :class="estadoBadgeClass(rescueSelected.estado)" style="margin-top:4px">{{ rescueSelected.estado }}</span>
                  </div>
                </div>
              </div>

              <!-- Descripción -->
              <div class="exp-section">
                <div class="exp-section-title accent-orange"><span class="exp-section-dot orange"></span>Descripción del rescate</div>
                <p class="exp-text-block">{{ rescueSelected.descripcion || '—' }}</p>
              </div>

            </div>

            <div class="sc-modal-footer">
              <button class="sc-btn-cancel" @click="showDetailModal = false">Cerrar expediente</button>
              <button
                v-if="rescueSelected?.estado === 'Activo'"
                class="sc-btn-ver sc-btn-ver--red"
                style="padding:10px 18px;height:auto;font-size:13px;display:flex;align-items:center;gap:6px"
                @click="showDetailModal = false; pedirCerrar(rescates.indexOf(rescueSelected))"
              >
                <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
                Cerrar rescate
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- ══ MODAL CONFIRMACIÓN CERRAR ══ -->
    <Teleport to="body">
      <Transition name="overlay-anim">
        <div v-if="modalConfirm" class="sc-overlay sc-overlay--top" @click.self="modalConfirm = false">
          <div class="sc-modal sc-modal--sm">
            <div class="sc-confirm-body">
              <div class="sc-confirm-icon">
                <svg width="26" height="26" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
              </div>
              <h3 class="sc-confirm-title">Cerrar rescate</h3>
              <p class="sc-confirm-text">¿Estás seguro de que deseas marcar este rescate como <strong>Cerrado</strong>?</p>
            </div>
            <div class="sc-modal-footer">
              <button class="sc-btn-cancel" @click="modalConfirm = false">Cancelar</button>
              <button class="sc-btn-save" @click="confirmarCerrar">Confirmar</button>
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
.sc-root { background: transparent; padding-bottom: 40px; }

/* ═══════════════════════════════════════
   TOAST
═══════════════════════════════════════ */
.sc-toast {
  position: fixed; bottom: 32px; right: 32px; z-index: 9999;
  display: flex; align-items: center; gap: 10px;
  padding: 14px 20px; border-radius: 14px;
  font-size: 14px; font-weight: 600;
  box-shadow: 0 8px 32px rgba(0,0,0,0.16); pointer-events: none;
}
.sc-toast.success { background: #3A473C; color: #fff; }
.sc-toast.error   { background: #c0392b; color: #fff; }
.toast-anim-enter-active, .toast-anim-leave-active { transition: all 0.25s ease; }
.toast-anim-enter-from, .toast-anim-leave-to { opacity: 0; transform: translateY(10px); }

/* ═══════════════════════════════════════
   HEADER
═══════════════════════════════════════ */
.sc-header {
  display: flex; justify-content: space-between; align-items: flex-start;
  margin-bottom: 28px; gap: 16px; flex-wrap: wrap;
}
.sc-title { font-size: 28px; font-weight: 800; color: #3A473C; letter-spacing: -0.5px; line-height: 1.1; }
.sc-sub   { font-size: 14px; color: #6C756D; margin-top: 5px; font-weight: 500; }

/* Botón Nuevo rescate */
.sc-btn-nuevo {
  display: flex; align-items: center; gap: 8px;
  padding: 10px 20px; background: #3A473C; border: none; border-radius: 10px;
  font-size: 13px; font-weight: 700; color: #fff;
  cursor: pointer; transition: background 0.18s; font-family: inherit;
  flex-shrink: 0; white-space: nowrap;
}
.sc-btn-nuevo:hover { background: #2d3730; }

/* Botón Cancelar en header del formulario */
.sc-btn-cancel-header {
  display: flex; align-items: center; gap: 8px;
  padding: 10px 18px; background: #F4F6F4; border: none; border-radius: 10px;
  font-size: 13px; font-weight: 700; color: #6C756D;
  cursor: pointer; transition: background 0.15s; font-family: inherit;
  flex-shrink: 0; white-space: nowrap;
}
.sc-btn-cancel-header:hover { background: #E5EAE6; }

/* ═══════════════════════════════════════
   TOOLBAR / FILTROS
═══════════════════════════════════════ */
.sc-toolbar {
  display: flex; align-items: center; gap: 16px;
  margin-bottom: 20px; flex-wrap: nowrap;
}
.sc-filters {
  display: flex; align-items: center; gap: 10px;
  flex: 1; flex-wrap: nowrap; min-width: 0;
}
.sc-search-wrap {
  position: relative; flex: 1; min-width: 160px; max-width: 280px;
}
.sc-search-icon {
  position: absolute; left: 12px; top: 50%; transform: translateY(-50%);
  color: #92A894; pointer-events: none;
}
.sc-search {
  width: 100%; box-sizing: border-box;
  padding: 9px 12px 9px 34px; height: 36px;
  border: 1.5px solid #E8ECE8; border-radius: 10px;
  font-size: 13px; color: #3A473C; background: #fff;
  outline: none; font-family: inherit; transition: border-color 0.18s;
}
.sc-search:focus { border-color: #92A894; }
.sc-select-wrap { position: relative; flex-shrink: 0; }
.sc-filter-select {
  appearance: none; padding: 0 32px 0 12px; height: 36px;
  border: 1.5px solid #E8ECE8; border-radius: 10px;
  font-size: 13px; color: #3A473C; background: #fff;
  outline: none; font-family: inherit; cursor: pointer;
  transition: border-color 0.18s; white-space: nowrap; box-sizing: border-box;
}
.sc-filter-select:focus { border-color: #92A894; }
.sc-select-icon {
  position: absolute; right: 10px; top: 50%; transform: translateY(-50%);
  color: #92A894; pointer-events: none;
}
.sc-clear {
  padding: 0 14px; height: 36px; border: 1.5px solid #fdd; border-radius: 10px;
  background: #fff5f5; color: #c0392b; font-size: 12px; font-weight: 700;
  font-family: inherit; cursor: pointer; transition: background 0.15s;
  white-space: nowrap; flex-shrink: 0;
}
.sc-clear:hover { background: #ffe5e5; }

/* ═══════════════════════════════════════
   TABLA
═══════════════════════════════════════ */
.sc-table-wrap {
  background: #fff; border-radius: 20px;
  box-shadow: 0 2px 16px rgba(58,71,60,0.06); overflow: hidden;
}
.sc-table { width: 100%; border-collapse: collapse; table-layout: fixed; }
.sc-table thead { background: #F9FAF9; }
.sc-table th {
  padding: 14px 20px; font-size: 11px; font-weight: 800; color: #92A894;
  text-transform: uppercase; letter-spacing: 0.6px;
  white-space: nowrap; border-bottom: 1.5px solid #F0F2F0; text-align: left;
}
.sc-table td {
  padding: 14px 20px; font-size: 14px; color: #3A473C;
  border-bottom: 1px solid #F5F7F5; vertical-align: middle;
}
.sc-table tbody tr:last-child td { border-bottom: none; }
.sc-table tbody tr { transition: background 0.12s; }
.sc-table tbody tr:hover { background: #FAFBFA; }

/* Avatar */
.sc-avatar {
  width: 38px; height: 38px; border-radius: 50%; overflow: hidden; flex-shrink: 0;
  background: #DDE6DE; display: flex; align-items: center; justify-content: center;
}
.sc-avatar-img { width: 100%; height: 100%; object-fit: cover; }
.sc-avatar-ini { font-size: 14px; font-weight: 800; color: #5A6E5C; text-transform: uppercase; line-height: 1; }

.sc-pet-cell { display: flex; align-items: center; gap: 10px; }
.sc-pet-info  { display: flex; flex-direction: column; gap: 2px; min-width: 0; }
.sc-pet-name  { font-weight: 700; font-size: 14px; color: #3A473C; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.sc-pet-sub   { font-size: 11px; color: #92A894; }
.sc-pet-id    { font-size: 11px; color: #92A894; font-family: monospace; }

.sc-td-main { font-weight: 500; color: #3A473C; font-size: 13px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; display: block; }
.sc-td-sec  { color: #7A8A7C; font-size: 12px; }

/* ── Badges ── */
.sc-badge {
  display: inline-flex; align-items: center; gap: 5px;
  padding: 4px 10px; border-radius: 7px;
  font-size: 12px; font-weight: 600; white-space: nowrap;
  background: #F0F4F0; color: #4A6550;
}
.badge--green   { background: rgba(76,175,80,.14);   color: #2E7D32; }
.badge--red     { background: rgba(235,119,119,.16);  color: #C45252; }
.badge--orange  { background: rgba(255,152,0,.14);    color: #E65100; }
.badge--yellow  { background: rgba(255,193,7,.16);    color: #9A6A00; }
.badge--neutral { background: #F4F6F4;                color: #6C756D; }
.badge--blue    { background: rgba(33,150,243,.13);   color: #1565C0; }

/* ── Botones de acción ── */
.sc-actions { display: flex; align-items: center; gap: 4px; justify-content: center; }
.sc-btn-ver {
  display: inline-flex; align-items: center; gap: 4px;
  padding: 5px 11px; height: 28px; border: none; border-radius: 8px;
  font-size: 12px; font-weight: 700; font-family: inherit;
  cursor: pointer; transition: background 0.15s, opacity 0.15s;
  white-space: nowrap; flex-shrink: 0;
}
.sc-btn-ver:disabled { opacity: 0.35; cursor: not-allowed; }
.action-icon { width: 10px; height: 10px; object-fit: contain; display: block; }
.sc-btn-ver--neutral { background: #F0F4F0;               color: #3A473C; }
.sc-btn-ver--neutral:hover { background: #DDE6DE; }
.sc-btn-ver--green   { background: rgba(76,175,80,.14);   color: #2E7D32; }
.sc-btn-ver--green:hover   { background: rgba(76,175,80,.26); }
.sc-btn-ver--red     { background: rgba(235,119,119,.14); color: #C45252; }
.sc-btn-ver--red:hover     { background: rgba(235,119,119,.26); }
.sc-btn-ver--blue    { background: rgba(33,150,243,.12);  color: #1565C0; }
.sc-btn-ver--blue:hover    { background: rgba(33,150,243,.22); }
.sc-btn-ver--orange  { background: rgba(255,152,0,.13);   color: #E65100; }
.sc-btn-ver--orange:hover  { background: rgba(255,152,0,.24); }

/* Empty state */
.sc-empty { padding: 0; }
.sc-empty-inner {
  display: flex; flex-direction: column; align-items: center;
  justify-content: center; gap: 12px; padding: 56px 24px; color: #92A894;
}
.sc-empty-inner svg { opacity: 0.4; }
.sc-empty-inner p { font-size: 14px; font-weight: 500; color: #7A8A7C; margin: 0; }

/* ═══════════════════════════════════════
   FORMULARIO COMO PANTALLA
═══════════════════════════════════════ */
.sc-form-errors {
  display: flex; align-items: flex-start; gap: 10px;
  padding: 14px 18px; background: rgba(235,119,119,.10);
  border: 1.5px solid rgba(235,119,119,.25); border-radius: 12px;
  font-size: 13px; color: #C45252; margin-bottom: 20px; line-height: 1.5;
}
.sc-form-errors svg { flex-shrink: 0; margin-top: 1px; }

.sc-form-panel {
  background: #fff; border-radius: 20px;
  box-shadow: 0 2px 16px rgba(58,71,60,0.06);
  padding: 32px 32px 28px;
}

.sc-section-label {
  display: flex; align-items: center; gap: 10px;
  font-size: 13px; font-weight: 800; color: #3A473C;
  text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 16px;
}
.sc-section-label.accent-orange { color: #C08030; }
.sc-section-num {
  width: 24px; height: 24px; border-radius: 7px;
  background: #3A473C; color: #fff;
  font-size: 11px; font-weight: 800;
  display: flex; align-items: center; justify-content: center; flex-shrink: 0;
}
.sc-section-num.orange { background: #F9C17A; color: #8A5A1E; }

.sc-required { color: #c0392b; margin-left: 2px; }

/* Grid formulario */
.sc-form-grid { display: grid; gap: 14px; }
.sc-form-grid--4 { grid-template-columns: repeat(4, 1fr); }
.sc-fg { display: flex; flex-direction: column; gap: 6px; }
.sc-fg--span2 { grid-column: span 2; }
.sc-fg--full  { grid-column: 1 / -1; }
.sc-fg label { font-size: 12px; font-weight: 700; color: #5A6E5C; letter-spacing: 0.1px; display: flex; align-items: center; gap: 7px; }
.sc-input {
  padding: 10px 13px; border: 1.5px solid #E8ECE8; border-radius: 10px;
  font-size: 13px; color: #3A473C; background: #FAFBFA;
  outline: none; font-family: inherit; transition: border-color 0.18s, background 0.18s;
  width: 100%; box-sizing: border-box;
}
.sc-input:focus    { border-color: #92A894; background: #fff; }
.sc-input:disabled { background: #F4F6F4; color: #9BA99C; cursor: not-allowed; }
.sc-textarea {
  padding: 10px 13px; border: 1.5px solid #E8ECE8; border-radius: 10px;
  font-size: 13px; color: #3A473C; background: #FAFBFA;
  outline: none; font-family: inherit; transition: border-color 0.18s, background 0.18s;
  width: 100%; box-sizing: border-box; min-height: 100px; resize: vertical; line-height: 1.5;
}
.sc-textarea:focus { border-color: #92A894; background: #fff; }
.select-wrap { position: relative; }
.select-wrap select.sc-input { appearance: none; padding-right: 36px; cursor: pointer; }
.select-wrap i { position: absolute; right: 12px; top: 50%; transform: translateY(-50%); font-size: 18px; color: #92A894; pointer-events: none; }
.radio-row { display: flex; gap: 16px; align-items: center; padding-top: 4px; }
.r-opt { display: flex; align-items: center; gap: 7px; font-size: 13px; font-weight: 600; color: #3A473C; cursor: pointer; }
.r-opt input[type="radio"] { accent-color: #92A894; width: 15px; height: 15px; cursor: pointer; }

/* Foto */
.sc-foto-wrap { display: flex; align-items: center; gap: 20px; }
.sc-foto-preview {
  width: 100px; height: 100px; border-radius: 14px;
  border: 2px dashed #E8ECE8; background: #FAFBFA;
  display: flex; align-items: center; justify-content: center;
  overflow: hidden; flex-shrink: 0;
}
.sc-foto-preview.has-img { border-style: solid; border-color: #92A894; }
.sc-foto-img { width: 100%; height: 100%; object-fit: cover; }
.sc-foto-placeholder { display: flex; flex-direction: column; align-items: center; gap: 6px; color: #92A894; }
.sc-foto-placeholder span { font-size: 11px; font-weight: 600; }
.sc-foto-actions { display: flex; flex-direction: column; gap: 8px; }
.sc-btn-foto {
  display: inline-flex; align-items: center; gap: 8px;
  padding: 9px 16px; background: #F4F6F4; border: 1.5px solid #E8ECE8;
  border-radius: 10px; font-size: 13px; font-weight: 700; color: #3A473C;
  cursor: pointer; transition: background 0.15s; font-family: inherit;
}
.sc-btn-foto:hover { background: #E5EAE6; }
.sc-foto-hint { font-size: 11px; color: #92A894; margin: 0; }

/* Footer del formulario */
.sc-form-footer {
  display: flex; justify-content: flex-end; gap: 10px;
  padding-top: 24px; margin-top: 24px;
  border-top: 1.5px solid #F0F2F0;
}

/* ═══════════════════════════════════════
   OVERLAY / MODAL
═══════════════════════════════════════ */
.sc-overlay {
  position: fixed; inset: 0; background: rgba(20,30,22,0.5);
  display: flex; align-items: center; justify-content: center;
  z-index: 200; padding: 20px; backdrop-filter: blur(2px); overflow-y: auto;
}
.sc-overlay--top { z-index: 400; }
.overlay-anim-enter-active, .overlay-anim-leave-active { transition: all 0.22s ease; }
.overlay-anim-enter-from, .overlay-anim-leave-to { opacity: 0; }
.overlay-anim-enter-from .sc-modal, .overlay-anim-leave-to .sc-modal { transform: translateY(16px) scale(0.98); }
.sc-modal {
  background: #fff; border-radius: 22px; width: 100%;
  max-height: 88vh; overflow-y: auto;
  box-shadow: 0 24px 80px rgba(0,0,0,0.2);
  transition: transform 0.22s ease; margin: auto;
}
.sc-modal--sm { max-width: 420px; }
.sc-modal--lg { max-width: 900px; }

.sc-modal-header {
  display: flex; justify-content: space-between; align-items: flex-start;
  padding: 24px 28px 18px; border-bottom: 1.5px solid #F0F2F0;
}
.sc-modal-eyebrow { font-size: 11px; font-weight: 800; color: #92A894; text-transform: uppercase; letter-spacing: 0.7px; margin-bottom: 4px; }
.sc-modal-title   { font-size: 20px; font-weight: 800; color: #3A473C; letter-spacing: -0.4px; }
.sc-modal-close {
  width: 34px; height: 34px; border-radius: 10px;
  border: 1.5px solid #E8ECE8; background: #fff; color: #6C756D;
  cursor: pointer; display: flex; align-items: center; justify-content: center;
  transition: background 0.15s, border-color 0.15s; flex-shrink: 0;
}
.sc-modal-close:hover { background: #F4F6F4; border-color: #ccc; }
.sc-modal-body { padding: 24px 28px 8px; }

.edit-header-info { display: flex; align-items: center; gap: 14px; flex: 1; min-width: 0; }
.edit-avatar-sm { width: 44px; height: 44px; min-width: 44px; border-radius: 14px; background: #DDE6DE; color: #5A6E5C; font-size: 16px; font-weight: 800; display: flex; align-items: center; justify-content: center; }
.edit-body { display: flex; flex-direction: column; gap: 0; }

/* Expediente — Modal Ver */
.exp-header {
  display: flex; align-items: center; gap: 18px;
  padding: 26px 28px 22px; border-bottom: 1.5px solid #F0F2F0;
  background: linear-gradient(135deg, #F7F9F7, white);
}
.exp-avatar {
  width: 64px; height: 64px; min-width: 64px; border-radius: 18px;
  background: #DDE6DE; color: #5A6E5C;
  font-size: 22px; font-weight: 800;
  display: flex; align-items: center; justify-content: center;
  box-shadow: 0 8px 20px rgba(58,71,60,0.12); overflow: hidden;
}
.exp-avatar-img { width: 100%; height: 100%; object-fit: cover; }
.exp-avatar-ini { font-size: 22px; font-weight: 800; color: #5A6E5C; }
.exp-header-info { flex: 1; min-width: 0; }
.exp-name { font-size: 20px; font-weight: 800; color: #3A473C; margin-bottom: 8px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.exp-meta { display: flex; gap: 8px; flex-wrap: wrap; }

.exp-body { display: flex; flex-direction: column; gap: 0; }
.exp-section { border-bottom: 1.5px solid #F4F6F4; padding: 20px 0; }
.exp-section:last-child { border-bottom: none; }
.exp-section-title {
  display: flex; align-items: center; gap: 9px;
  font-size: 11px; font-weight: 800; letter-spacing: 0.10em;
  text-transform: uppercase; color: #92A894; margin-bottom: 16px;
}
.exp-section-title.accent-orange { color: #C08030; }
.exp-section-dot { width: 7px; height: 7px; border-radius: 50%; background: #92A894; flex-shrink: 0; }
.exp-section-dot.orange { background: #F9C17A; }
.exp-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 14px 20px; }
.exp-grid.cols-3 { grid-template-columns: 1fr 1fr 1fr; }
.exp-field { display: flex; flex-direction: column; gap: 4px; }
.exp-label { font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.06em; color: #92A894; }
.exp-value { font-size: 14px; color: #3A473C; }
.exp-value.fw { font-weight: 700; }
.exp-text-block { font-size: 14px; color: #3A473C; line-height: 1.7; background: #F9FAF9; border-radius: 12px; padding: 12px 14px; margin: 0; }

/* Modal footer */
.sc-modal-footer {
  display: flex; justify-content: flex-end; gap: 10px;
  padding: 18px 28px 24px; border-top: 1.5px solid #F0F2F0; margin-top: 12px;
}
.sc-btn-cancel {
  padding: 10px 18px; background: #F4F6F4; border: none; border-radius: 10px;
  font-size: 13px; font-weight: 700; color: #6C756D; cursor: pointer;
  transition: background 0.15s; font-family: inherit;
}
.sc-btn-cancel:hover { background: #E5EAE6; }
.sc-btn-save {
  display: flex; align-items: center; gap: 7px; padding: 10px 20px;
  background: #3A473C; border: none; border-radius: 10px;
  font-size: 13px; font-weight: 700; color: #fff; cursor: pointer;
  transition: background 0.18s; font-family: inherit;
}
.sc-btn-save:hover { background: #2d3730; }
.sc-btn-save:disabled { opacity: 0.4; cursor: not-allowed; }

/* Confirmación */
.sc-confirm-body { padding: 32px 28px 8px; text-align: center; }
.sc-confirm-icon { width: 60px; height: 60px; border-radius: 50%; background: #EEF2EE; color: #3A473C; display: flex; align-items: center; justify-content: center; margin: 0 auto 18px; }
.sc-confirm-title { font-size: 18px; font-weight: 800; color: #3A473C; margin-bottom: 10px; }
.sc-confirm-text  { font-size: 13px; color: #6C756D; line-height: 1.6; max-width: 320px; margin: 0 auto; }

/* ═══════════════════════════════════════
   RESPONSIVE
═══════════════════════════════════════ */
@media (max-width: 1100px) {
  .sc-toolbar { flex-wrap: wrap; }
  .sc-filters  { flex-wrap: wrap; }
}
@media (max-width: 900px) {
  .sc-form-grid--4 { grid-template-columns: repeat(2, 1fr); }
  .sc-fg--span2    { grid-column: span 1; }
  .exp-grid.cols-3 { grid-template-columns: 1fr 1fr; }
  .sc-table th:nth-child(5),
  .sc-table td:nth-child(5) { display: none; }
}
@media (max-width: 640px) {
  .sc-header   { flex-direction: column; align-items: flex-start; }
  .sc-toolbar  { flex-direction: column; align-items: flex-start; }
  .sc-filters  { width: 100%; flex-wrap: wrap; }
  .sc-search-wrap { max-width: 100%; }
  .sc-form-grid--4 { grid-template-columns: 1fr; }
  .sc-fg--span2, .sc-fg--full { grid-column: 1; }
  .sc-form-panel { padding: 20px 18px; }
  .sc-table th:nth-child(4),
  .sc-table td:nth-child(4) { display: none; }
  .sc-modal-body   { padding: 16px 18px 8px; }
  .sc-modal-header,
  .sc-modal-footer { padding-left: 18px; padding-right: 18px; }
  .exp-grid        { grid-template-columns: 1fr; }
  .exp-grid.cols-3 { grid-template-columns: 1fr 1fr; }
}
@media (max-width: 480px) {
  .exp-grid.cols-3 { grid-template-columns: 1fr; }
}
</style>