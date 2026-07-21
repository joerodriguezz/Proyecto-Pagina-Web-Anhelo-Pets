<script setup>
import { ref, computed, onMounted } from 'vue'
import { getAnimals, createAnimals } from '../../services/petServices'
import { getVeterinarians, createVeterinarian } from '../../services/veterinarianServices'
import { getHealthRecords, createHealthRecord } from '../../services/healthServices'
 
const animales = ref([])
const animalesLoading = ref(false)

async function loadAnimales() {
  animalesLoading.value = true
  try {
    const { data } = await getAnimals()
    animales.value = (data || []).map(a => ({
      id: a.animalId,
      name: a.animalName,
      species: a.species || '',
      active: a.animalStatus !== 'Inactivo',
      image: null,
    }))
  } catch {
    animales.value = []
  } finally {
    animalesLoading.value = false
  }
}

// ── Veterinarios (API) ──

const veterinarios = ref([])
const veterinariosLoading = ref(false)

async function cargarVeterinarios() {
  veterinariosLoading.value = true
  try {
    const { data } = await getVeterinarians()
    veterinarios.value = (data || [])
      .filter(v => v.active !== false)
      .map(v => ({
        id: v.veterinarianId,
        nombre: v.fullName || `${v.firstName || ''} ${v.lastName || ''}`.trim(),
        especialidad: v.specialty || '',
      }))
  } catch {
    veterinarios.value = []
  } finally {
    veterinariosLoading.value = false
  }
}

onMounted(() => {
  loadAnimales()
  cargarVeterinarios()
  cargarExpedientes()
})
 
// ── Tabs ──
const TABS = [
  { id: 'historial',    titulo: 'Historial' },
  { id: 'vacunas',      titulo: 'Vacunas' },
  { id: 'tratamientos', titulo: 'Tratamientos' }
]

const activeTab = ref('historial')

// Filtro de refuerzos, solo aplicable en la pestaña de vacunas
const filtroRefuerzo = ref('todas')   // 'todas' | 'vencidas' | 'porVencer'

function cambiarTab(id) {
  activeTab.value = id
  if (id !== 'vacunas') filtroRefuerzo.value = 'todas'
}

// Atajo desde los KPI: salta a vacunas ya filtrado
function verVacunas(filtro) {
  activeTab.value = 'vacunas'
  filtroRefuerzo.value = filtro
}
 
// ── Modales ──
const showModalRegistrar = ref(false)
const showModalVer       = ref(false)
const showModalConfirm   = ref(false)
 
const showPetDropdown = ref(false)
const showVetDropdown = ref(false)
 
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

// ── Wizard paso a paso ──
// Cada expediente es de un solo tipo: historial, vacuna o tratamiento.
const TIPOS_REGISTRO = [
  { id: 'historial',    titulo: 'Historial médico', desc: 'Consulta, diagnóstico y peso' },
  { id: 'vacunas',      titulo: 'Vacuna',           desc: 'Dosis aplicada y refuerzo' },
  { id: 'tratamientos', titulo: 'Tratamiento',      desc: 'Medicación y dosis' }
]

const tipoRegistro = ref('')

const tipoInfo = computed(() =>
  TIPOS_REGISTRO.find(t => t.id === tipoRegistro.value) || null
)

const PASOS = [
  { n: 1, titulo: 'Tipo',     desc: 'Qué vas a registrar' },
  { n: 2, titulo: 'Mascota',  desc: 'Selecciona el paciente' },
  { n: 3, titulo: 'Datos',    desc: 'Completa el registro' },
  { n: 4, titulo: 'Resumen',  desc: 'Revisa y confirma' }
]
const TOTAL_PASOS = PASOS.length

const pasoActual = ref(1)
const pasoMaximo = ref(1)

const pasoInfo = computed(() => PASOS[pasoActual.value - 1])
const esUltimoPaso = computed(() => pasoActual.value === TOTAL_PASOS)
const progreso = computed(() => ((pasoActual.value - 1) / (TOTAL_PASOS - 1)) * 100)

// Campos obligatorios del paso de datos, según el tipo elegido.
// El veterinario es obligatorio siempre: veterinarian_id es NOT NULL.
const CAMPOS_POR_TIPO = {
  historial: [
    { key: 'vet',             test: () => !!form.value.vetId },
    { key: 'fecha',           test: () => !!form.value.fecha },
    { key: 'diagnostico',     test: () => !!form.value.diagnostico?.trim() }
  ],
  vacunas: [
    { key: 'vet',             test: () => !!form.value.vetId },
    { key: 'tipoVacuna',      test: () => !!form.value.tipoVacuna?.trim() },
    { key: 'fechaAplicacion', test: () => !!form.value.fechaAplicacion }
  ],
  tratamientos: [
    { key: 'vet',             test: () => !!form.value.vetId },
    { key: 'tipoTratamiento', test: () => !!form.value.tipoTratamiento?.trim() },
    { key: 'fechaTrat',       test: () => !!form.value.fechaTrat }
  ]
}

function camposDePaso(n) {
  if (n === 1) return [{ key: 'tipo', test: () => !!tipoRegistro.value }]
  if (n === 2) return [{ key: 'pet',  test: () => !!petSeleccionada.value }]
  if (n === 3) return CAMPOS_POR_TIPO[tipoRegistro.value] || []
  return []
}

function pasoCompleto(n) {
  return camposDePaso(n).every(c => c.test())
}

const MENSAJES_ERROR = {
  tipo: 'Elige qué vas a registrar',
  pet:  'Selecciona una mascota',
  vet:  'Selecciona un veterinario'
}

function validarPaso(n) {
  const e = { ...errores.value }
  let ok = true

  camposDePaso(n).forEach(c => {
    if (c.test()) {
      delete e[c.key]
      return
    }
    e[c.key] = MENSAJES_ERROR[c.key] || 'Obligatorio'
    ok = false
  })

  errores.value = e
  return ok
}

function irAPaso(n) {
  if (n < 1 || n > TOTAL_PASOS) return
  // Solo se permite saltar a pasos ya visitados
  if (n > pasoMaximo.value) return
  pasoActual.value = n
}

function pasoSiguiente() {
  if (!validarPaso(pasoActual.value)) return
  if (esUltimoPaso.value) return
  pasoActual.value += 1
  pasoMaximo.value = Math.max(pasoMaximo.value, pasoActual.value)
}

function pasoAnterior() {
  if (pasoActual.value > 1) pasoActual.value -= 1
}

// ── Expedientes (API) ──
//
// animal_medical_records no tiene columna de tipo de registro, así que el tipo
// viaja como prefijo [H]/[V]/[T] al inicio de `treatment`, seguido de los campos
// que tampoco tienen columna propia, separados por '|'. Toda esa (frágil)
// convención vive aquí y en `serializarTratamiento`, en ningún otro sitio.
const expedientes = ref([])
const expedientesLoading = ref(false)

const PREFIJO_POR_TAB    = { historial: 'H', vacunas: 'V', tratamientos: 'T' }
const ID_POR_PREFIJO     = { H: 'SAL', V: 'VAC', T: 'TRA' }
const TITULO_POR_PREFIJO = { H: 'Historial', V: 'Vacuna', T: 'Tratamiento' }

// '|' es el separador: se neutraliza en los valores para no romper el parseo
function limpiarCampo(valor) {
  return String(valor ?? '').replace(/\|/g, '/')
}

function serializarTratamiento(prefijo, extras) {
  return [`[${prefijo}]`, ...extras.map(limpiarCampo)].join('|')
}

function parseExpediente(dto) {
  const bruto = dto.treatment || ''
  const match = bruto.match(/^\[([HVT])\]/)
  const prefijo = match ? match[1] : 'H'
  const [a = '', b = ''] = bruto.split('|').slice(1)

  const base = {
    recordId:      dto.animalMedicalRecordId,
    id:            `${ID_POR_PREFIJO[prefijo]}-${dto.animalMedicalRecordId}`,
    prefijo,
    petId:         dto.animalId,
    vet:           dto.veterinarianName || '',
    vetId:         dto.veterinarianId,
    observaciones: dto.notes || '',
  }

  if (prefijo === 'V') {
    return { ...base, tipo: dto.diagnosis, fechaAplicacion: dto.visitDate, clinica: a, proximaDosis: b }
  }
  if (prefijo === 'T') {
    return { ...base, tipo: dto.diagnosis, fecha: dto.visitDate, medicamento: a, dosis: b }
  }
  return { ...base, diagnostico: dto.diagnosis, fecha: dto.visitDate, clinica: a, peso: b }
}

async function cargarExpedientes() {
  expedientesLoading.value = true
  try {
    const { data } = await getHealthRecords()
    expedientes.value = (data || []).map(parseExpediente)
  } catch {
    expedientes.value = []
    showToast('error', 'No se pudieron cargar los expedientes')
  } finally {
    expedientesLoading.value = false
  }
}
 
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
    vetId: '',
    clinica: '',
    peso: '',
    diagnostico: '',
    observaciones_h: '',

    tipoVacuna: '',
    fechaAplicacion: '',
    proximaDosis: '',
    clinicaVacuna: '',
    observaciones_v: '',
 
    tipoTratamiento: '',
    medicamento: '',
    dosis: '',
    fechaTrat: '',
    observaciones_t: ''
  }
 
  petSeleccionada.value = null
  tipoRegistro.value = ''

  showPetDropdown.value = false
  showVetDropdown.value = false
  showInlineAddPet.value = false
  showInlineAddVet.value = false

  errores.value = {}

  pasoActual.value = 1
  pasoMaximo.value = 1
}

// El paso 1 es de selección única: al elegir el tipo se avanza solo.
function seleccionarTipo(id) {
  tipoRegistro.value = id
  clearErr('tipo')

  if (pasoActual.value !== 1) return
  setTimeout(() => {
    if (pasoActual.value === 1 && tipoRegistro.value) pasoSiguiente()
  }, 280)
}
 
// ── Filtros ──
const search     = ref('')
const filterFrom = ref('')
const filterTo   = ref('')
 
// ── Registros por tab ──
const petsPorId = computed(() =>
  Object.fromEntries(animales.value.map(p => [p.id, p]))
)

const registros = computed(() => {
  const prefijo = PREFIJO_POR_TAB[activeTab.value]

  const todos = expedientes.value
    .filter(r => r.prefijo === prefijo)
    .map(r => {
      const pet = petsPorId.value[r.petId]
      return {
        ...r,
        petNombre:  pet?.name || r.petId,
        petEspecie: pet?.species || '',
        petFoto:
          pet?.images?.[0]?.preview ||
          pet?.foto ||
          pet?.image ||
          pet?.photo ||
          pet?.avatar ||
          null,
        petActiva:  pet ? pet.active !== false : true
      }
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

  if (activeTab.value === 'vacunas' && filtroRefuerzo.value !== 'todas') {
    const clase = filtroRefuerzo.value === 'vencidas' ? 'badge-rechazada' : 'badge-pendiente'
    result = result.filter(r => estadoRefuerzo(r.proximaDosis)?.clase === clase)
  }

  return result
})

const ETIQUETA_REFUERZO = {
  vencidas:  'Refuerzos vencidos',
  porVencer: 'Refuerzos por vencer'
}

function limpiarFiltros() {
  search.value = ''
  filterFrom.value = ''
  filterTo.value = ''
  filtroRefuerzo.value = 'todas'
}
 
const hayFiltros = computed(() =>
  search.value.trim() !== '' ||
  filterFrom.value !== '' ||
  filterTo.value !== '' ||
  filtroRefuerzo.value !== 'todas'
)

const ETIQUETA_TAB = {
  historial:    'Sin registros de historial médico',
  vacunas:      'Sin registros de vacunas',
  tratamientos: 'Sin registros de tratamientos'
}

// ── Columnas de la tabla, por pestaña ──
// `valor` devuelve el texto ya formateado; `tipo` decide cómo se pinta.
const COLUMNAS_POR_TAB = {
  historial: [
    { key: 'fecha',       titulo: 'Fecha',       tipo: 'fecha',    valor: r => r.fecha },
    { key: 'diagnostico', titulo: 'Diagnóstico', tipo: 'destacado', valor: r => r.diagnostico },
    { key: 'vet',         titulo: 'Veterinario', tipo: 'texto',    valor: r => r.vet },
    { key: 'peso',        titulo: 'Peso',        tipo: 'texto',    valor: r => r.peso ? `${r.peso} kg` : '' },
  ],
  vacunas: [
    { key: 'tipo',            titulo: 'Vacuna',        tipo: 'destacado', valor: r => r.tipo },
    { key: 'fechaAplicacion', titulo: 'Aplicación',    tipo: 'fecha',     valor: r => r.fechaAplicacion },
    { key: 'proximaDosis',    titulo: 'Próxima dosis', tipo: 'refuerzo',  valor: r => r.proximaDosis },
    { key: 'vet',             titulo: 'Veterinario',   tipo: 'texto',     valor: r => r.vet },
  ],
  tratamientos: [
    { key: 'tipo',        titulo: 'Tratamiento', tipo: 'destacado', valor: r => r.tipo },
    { key: 'fecha',       titulo: 'Fecha',       tipo: 'fecha',     valor: r => r.fecha },
    { key: 'medicamento', titulo: 'Medicamento', tipo: 'texto',     valor: r => r.medicamento },
    { key: 'dosis',       titulo: 'Dosis',       tipo: 'texto',     valor: r => r.dosis },
  ]
}

const columnas = computed(() => COLUMNAS_POR_TAB[activeTab.value] || [])

// Estado del refuerzo: vencido / próximo (30 días) / al día
function estadoRefuerzo(fecha) {
  if (!fecha) return null

  const hoy = new Date()
  hoy.setHours(0, 0, 0, 0)
  const objetivo = new Date(`${fecha}T00:00:00`)
  const dias = Math.round((objetivo - hoy) / 86400000)

  if (dias < 0)  return { clase: 'badge-rechazada', titulo: `Vencida hace ${Math.abs(dias)} día${Math.abs(dias) !== 1 ? 's' : ''}` }
  if (dias <= 30) return { clase: 'badge-pendiente', titulo: dias === 0 ? 'Vence hoy' : `Faltan ${dias} día${dias !== 1 ? 's' : ''}` }
  return { clase: 'badge-aprobada', titulo: `Faltan ${dias} días` }
}

const mensajeVacio = computed(() => {
  if (expedientesLoading.value) return 'Cargando expedientes...'
  if (hayFiltros.value) return 'Sin resultados para los filtros aplicados'
  return ETIQUETA_TAB[activeTab.value]
})
 
// ── Validación ──
// Revalida todos los pasos y, si alguno falla, lleva al usuario al primero incompleto.
function validar() {
  for (let n = 1; n <= TOTAL_PASOS; n++) {
    if (validarPaso(n)) continue
    pasoActual.value = n
    pasoMaximo.value = Math.max(pasoMaximo.value, n)
    return false
  }
  return true
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
 
const guardandoExpediente = ref(false)

// Traduce el formulario del tipo elegido a las columnas de animal_medical_records
function cuerpoSegunTipo() {
  if (tipoRegistro.value === 'vacunas') {
    return {
      diagnosis: form.value.tipoVacuna,
      treatment: serializarTratamiento('V', [form.value.clinicaVacuna, form.value.proximaDosis]),
      notes: form.value.observaciones_v || null,
      visitDate: form.value.fechaAplicacion,
    }
  }

  if (tipoRegistro.value === 'tratamientos') {
    return {
      diagnosis: form.value.tipoTratamiento,
      treatment: serializarTratamiento('T', [form.value.medicamento, form.value.dosis]),
      notes: form.value.observaciones_t || null,
      visitDate: form.value.fechaTrat,
    }
  }

  return {
    diagnosis: form.value.diagnostico,
    treatment: serializarTratamiento('H', [form.value.clinica, form.value.peso]),
    notes: form.value.observaciones_h || null,
    visitDate: form.value.fecha,
  }
}

async function confirmarGuardar() {
  showModalConfirm.value = false

  const animalId = petSeleccionada.value?.id
  if (!animalId) return

  guardandoExpediente.value = true
  try {
    await createHealthRecord({
      animalId,
      veterinarianId: form.value.vetId,
      createdBy: 'admin',
      ...cuerpoSegunTipo(),
    })

    await cargarExpedientes()

    resetForm()
    showModalRegistrar.value = false
    showToast('success', 'Expediente médico guardado correctamente')
  } catch (e) {
    const msg = e?.response?.data?.message
    showToast('error', msg || 'Error al guardar. Intenta de nuevo.')
  } finally {
    guardandoExpediente.value = false
  }
}
 
function abrirModal() {
  resetForm()
  showModalRegistrar.value = true
}

// Abre el wizard en el paso 1 con el formulario de alta de mascota desplegado
function abrirModalAgregarMascota() {
  abrirModal()
  abrirInlineAddPet()
}

function cerrarModalRegistrar() {
  showModalRegistrar.value = false
}
 
function seleccionarPet(pet, avanzar = false) {
  petSeleccionada.value = pet
  showPetDropdown.value = false
  clearErr('pet')

  // El paso de mascota es de selección única: al elegir se avanza solo.
  // El retardo deja ver la selección antes de cambiar de paso.
  if (!avanzar || pasoActual.value !== 2) return
  setTimeout(() => {
    if (pasoActual.value === 2 && petSeleccionada.value) pasoSiguiente()
  }, 280)
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
 
const mascotasActivas = computed(() => animales.value.filter(p => p.active !== false))
 
// ── Contadores por pestaña ──
const totalHistorial     = computed(() => expedientes.value.filter(r => r.prefijo === 'H').length)
const totalVacunas       = computed(() => expedientes.value.filter(r => r.prefijo === 'V').length)
const totalTratamientos  = computed(() => expedientes.value.filter(r => r.prefijo === 'T').length)

const conteoPorTab = computed(() => ({
  historial:    totalHistorial.value,
  vacunas:      totalVacunas.value,
  tratamientos: totalTratamientos.value
}))

// ── KPIs accionables ──
// Los contadores por tipo ya viven en las pestañas; aquí interesa lo que
// requiere acción: refuerzos vencidos y los que vencen dentro de 30 días.
const totalExpedientes = computed(() => expedientes.value.length)

const vacunasVencidas = computed(() =>
  expedientes.value.filter(r =>
    r.prefijo === 'V' && estadoRefuerzo(r.proximaDosis)?.clase === 'badge-rechazada'
  ).length
)

const vacunasPorVencer = computed(() =>
  expedientes.value.filter(r =>
    r.prefijo === 'V' && estadoRefuerzo(r.proximaDosis)?.clase === 'badge-pendiente'
  ).length
)
const totalMascotas = computed(() => animales.value.filter(p => p.active !== false).length)

const showInlineAddPet = ref(false)
const nuevaMascota = ref({ name: '', type: '', sex: '' })
const agregandoMascota = ref(false)
const errorMascota = ref('')


async function agregarMascota() {
  errorMascota.value = ''
  if (!nuevaMascota.value.name.trim()) { errorMascota.value = 'El nombre es obligatorio'; return }
  if (!nuevaMascota.value.type.trim()) { errorMascota.value = 'La especie es obligatoria'; return }
  if (!nuevaMascota.value.sex) { errorMascota.value = 'El sexo es obligatorio'; return }
  agregandoMascota.value = true
  try {
    const { data } = await createAnimals({ ...nuevaMascota.value })
    await loadAnimales()
    const added = animales.value.find(p => p.name.toLowerCase() === nuevaMascota.value.name.trim().toLowerCase())
    if (added) seleccionarPet(added)
    showInlineAddPet.value = false
    nuevaMascota.value = { name: '', type: '', sex: '' }
    showToast('success', 'Mascota agregada correctamente')
  } catch {
    errorMascota.value = 'Error al crear la mascota'
  } finally {
    agregandoMascota.value = false
  }
}

function abrirInlineAddPet() {
  errorMascota.value = ''
  nuevaMascota.value = { name: '', type: '', sex: '' }
  showPetDropdown.value = false
  showInlineAddPet.value = true
}

// ── Alta rápida de veterinario ──
// Los veterinarios viven en `anhelo_usuarios` (voluntarios de tipo
// Veterinaria aprobados), igual que en VoluntariosAdminView.
const showInlineAddVet = ref(false)
const nuevoVet = ref({ nombre: '', apellido: '', especialidad: '', correo: '', cedula: '' })
const guardandoVet = ref(false)
const errorVet = ref('')

function abrirInlineAddVet() {
  errorVet.value = ''
  nuevoVet.value = { nombre: '', apellido: '', especialidad: '', correo: '', cedula: '' }
  showVetDropdown.value = false
  showInlineAddVet.value = true
}

function seleccionarVet(vet) {
  form.value.vet = vet.nombre
  form.value.vetId = vet.id
  showVetDropdown.value = false
  clearErr('vet')
}

async function guardarVeterinario() {
  errorVet.value = ''

  const nombre = nuevoVet.value.nombre.trim()
  const apellido = nuevoVet.value.apellido.trim()
  const especialidad = nuevoVet.value.especialidad.trim()

  if (!nombre)       { errorVet.value = 'El nombre es obligatorio'; return }
  if (!apellido)     { errorVet.value = 'El apellido es obligatorio'; return }
  if (!especialidad) { errorVet.value = 'La especialidad es obligatoria'; return }

  guardandoVet.value = true
  try {
    const { data } = await createVeterinarian({
      firstName: nombre,
      lastName: apellido,
      specialty: especialidad,
      email: nuevoVet.value.correo.trim() || null,
      nationalId: nuevoVet.value.cedula.trim() || null,
      createdBy: 'admin',
    })

    await cargarVeterinarios()

    const creado = {
      id: data.veterinarianId,
      nombre: data.fullName || `${nombre} ${apellido}`,
      especialidad: data.specialty || especialidad,
    }

    seleccionarVet(creado)

    showInlineAddVet.value = false
    showToast('success', 'Veterinario agregado correctamente')
  } catch (e) {
    errorVet.value = e?.response?.data?.message || 'Error al guardar el veterinario'
  } finally {
    guardandoVet.value = false
  }
}
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
      <div class="page-actions">
        <button class="btn-secondary" @click="abrirModalAgregarMascota">
          <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M12 5v14M5 12h14"/></svg>
          Agregar mascota
        </button>
        <button class="btn-primary" @click="abrirModal">
          <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
          Nuevo expediente
        </button>
      </div>
    </header>

    <!-- ── KPIs ── -->
    <div class="don-summary">
      <div class="don-card kpi-historial">
        <span class="don-label">Expedientes</span>
        <strong class="don-value">{{ totalExpedientes }}</strong>
        <span class="don-nota">registros clínicos</span>
      </div>

      <button
        type="button"
        class="don-card kpi-vencidas"
        :class="{ 'don-card--alerta': vacunasVencidas > 0 }"
        @click="verVacunas('vencidas')"
      >
        <span class="don-label">Refuerzos vencidos</span>
        <strong class="don-value">{{ vacunasVencidas }}</strong>
        <span class="don-nota">{{ vacunasVencidas ? 'Requieren atención' : 'Nada pendiente' }}</span>
      </button>

      <button
        type="button"
        class="don-card kpi-porvencer"
        @click="verVacunas('porVencer')"
      >
        <span class="don-label">Vencen en 30 días</span>
        <strong class="don-value">{{ vacunasPorVencer }}</strong>
        <span class="don-nota">Próximos refuerzos</span>
      </button>

      <div class="don-card kpi-mascotas">
        <span class="don-label">Mascotas activas</span>
        <strong class="don-value">{{ totalMascotas }}</strong>
        <span class="don-nota">en el refugio</span>
      </div>
    </div>

    <!-- ── PANEL DE REGISTROS ── -->
    <div class="table-wrapper">

      <!-- Pestañas: navegación principal, pegada a la tabla -->
      <nav class="panel-tabs">
        <button
          v-for="t in TABS"
          :key="t.id"
          class="panel-tab"
          :class="{ 'panel-tab--active': activeTab === t.id }"
          @click="cambiarTab(t.id)"
        >
          <svg v-if="t.id === 'historial'" xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
          <svg v-else-if="t.id === 'vacunas'" xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M22 12h-4l-3 9L9 3l-3 9H2"/></svg>
          <svg v-else xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/></svg>
          {{ t.titulo }}
          <span class="panel-tab-count">{{ conteoPorTab[t.id] }}</span>
        </button>
      </nav>

      <!-- Filtros, dentro del mismo panel -->
      <div class="panel-filtros">
        <div class="filtro-input-wrap panel-buscar">
          <input v-model="search" placeholder="Buscar por nombre o ID..." class="filtro-input filtro-input--icon" />
          <span class="filtro-icon filtro-icon--right">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
          </span>
        </div>

        <div class="panel-fechas">
          <label class="panel-fechas-label">Del</label>
          <input type="date" class="filtro-input" v-model="filterFrom" />
          <label class="panel-fechas-label">al</label>
          <input type="date" class="filtro-input" v-model="filterTo" />
        </div>

        <span v-if="filtroRefuerzo !== 'todas'" class="filtro-chip">
          {{ ETIQUETA_REFUERZO[filtroRefuerzo] }}
          <button type="button" @click="filtroRefuerzo = 'todas'" aria-label="Quitar filtro">✕</button>
        </span>

        <button
          v-if="hayFiltros"
          type="button"
          class="btn-limpiar btn-limpiar--activo"
          @click="limpiarFiltros"
        >
          Limpiar filtros
        </button>
      </div>

      <div class="table-scroll">
        <table class="don-table">
          <thead>
            <tr>
              <th>ID Registro</th>
              <th>Mascota</th>
              <th v-for="c in columnas" :key="c.key">{{ c.titulo }}</th>
              <th>Acción</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="r in registros"
              :key="r.id"
              class="don-row don-row--click"
              @click="verRegistro(r)"
            >
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

              <td v-for="c in columnas" :key="c.key">
                <template v-if="c.tipo === 'refuerzo'">
                  <span
                    v-if="c.valor(r)"
                    class="estado-badge"
                    :class="estadoRefuerzo(c.valor(r))?.clase"
                    :title="estadoRefuerzo(c.valor(r))?.titulo"
                  >{{ formatFecha(c.valor(r)) }}</span>
                  <span v-else class="metodo-text">—</span>
                </template>
                <span v-else-if="c.tipo === 'fecha'" class="fecha-text">{{ formatFecha(c.valor(r)) }}</span>
                <span v-else-if="c.tipo === 'destacado'" class="monto-text">{{ c.valor(r) || '—' }}</span>
                <span v-else class="metodo-text">{{ c.valor(r) || '—' }}</span>
              </td>

              <td>
                <button class="btn-ver" @click.stop="verRegistro(r)">Ver detalle</button>
              </td>
            </tr>

            <tr v-if="registros.length === 0">
              <td :colspan="columnas.length + 3" class="empty-cell">
                <div class="empty-state-inner">
                  <svg v-if="activeTab === 'historial'" xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
                  <svg v-else-if="activeTab === 'vacunas'" xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M22 12h-4l-3 9L9 3l-3 9H2"/></svg>
                  <svg v-else xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/></svg>
                  <p>{{ mensajeVacio }}</p>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Tarjetas: sustituyen a la tabla en móvil -->
      <div class="rec-cards">
        <button
          v-for="r in registros"
          :key="r.id"
          type="button"
          class="rec-card"
          @click="verRegistro(r)"
        >
          <div class="rec-card-head">
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
            <span class="id-pill">{{ r.id }}</span>
          </div>

          <dl class="rec-card-list">
            <div v-for="c in columnas" :key="c.key">
              <dt>{{ c.titulo }}</dt>
              <dd>
                <span
                  v-if="c.tipo === 'refuerzo' && c.valor(r)"
                  class="estado-badge"
                  :class="estadoRefuerzo(c.valor(r))?.clase"
                >{{ formatFecha(c.valor(r)) }}</span>
                <template v-else-if="c.tipo === 'fecha' || c.tipo === 'refuerzo'">{{ formatFecha(c.valor(r)) }}</template>
                <template v-else>{{ c.valor(r) || '—' }}</template>
              </dd>
            </div>
          </dl>
        </button>

        <div v-if="registros.length === 0" class="empty-state-inner">
          <p>{{ mensajeVacio }}</p>
        </div>
      </div>

      <div class="table-footer">
        {{ registros.length }} registro{{ registros.length !== 1 ? 's' : '' }} encontrado{{ registros.length !== 1 ? 's' : '' }}
      </div>
    </div>

    <!-- ══════════════════════════════════════
         MODAL REGISTRAR — wizard paso a paso
    ══════════════════════════════════════ -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showModalRegistrar" class="modal-overlay" @click.self="cerrarModalRegistrar">
          <div class="modal-box modal-box--lg modal-box--wizard">

            <button class="modal-close" @click="cerrarModalRegistrar">✕</button>

            <!-- ── Cabecera del wizard ── -->
            <div class="wiz-header">
              <p class="modal-eyebrow">Expediente médico</p>
              <h2 class="modal-title">Nuevo registro completo</h2>

              <!-- Stepper -->
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
                <span v-if="petSeleccionada && pasoActual > 1" class="wiz-context-pet">
                  <span class="pet-avatar pet-avatar--xs">
                    <span class="pet-avatar-ini">{{ petSeleccionada.name?.charAt(0) }}</span>
                  </span>
                  {{ petSeleccionada.name }}
                </span>
              </div>
            </div>

            <div class="modal-body wiz-body">

              <!-- ══ PASO 1 — Tipo de registro ══ -->
              <section v-show="pasoActual === 1" class="wiz-pane">
                <h4 class="modal-section-title">¿Qué vas a registrar?</h4>

                <div class="tipo-grid">
                  <button
                    v-for="t in TIPOS_REGISTRO"
                    :key="t.id"
                    type="button"
                    class="tipo-card"
                    :class="{ 'is-selected': tipoRegistro === t.id }"
                    @click="seleccionarTipo(t.id)"
                  >
                    <span class="tipo-card-icon">
                      <svg v-if="t.id === 'historial'" xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
                      <svg v-else-if="t.id === 'vacunas'" xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M22 12h-4l-3 9L9 3l-3 9H2"/></svg>
                      <svg v-else xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/></svg>
                    </span>
                    <span class="tipo-card-title">{{ t.titulo }}</span>
                    <span class="tipo-card-desc">{{ t.desc }}</span>
                    <svg v-if="tipoRegistro === t.id" class="tipo-card-check" xmlns="http://www.w3.org/2000/svg" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
                  </button>
                </div>

                <p v-if="errores.tipo" class="field-error" style="margin-top:10px">{{ errores.tipo }}</p>
              </section>

              <!-- ══ PASO 2 — Mascota ══ -->
              <section v-show="pasoActual === 2" class="wiz-pane">
                <h4 class="modal-section-title">Selecciona la mascota</h4>

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
                      @click="seleccionarPet(pet, true)"
                    >
                      <div class="pet-avatar pet-avatar--sm">
                        <img v-if="pet.foto || pet.image || pet.photo || pet.avatar" :src="pet.foto || pet.image || pet.photo || pet.avatar" class="pet-avatar-img" />
                        <span v-else class="pet-avatar-ini">{{ pet.name?.charAt(0) }}</span>
                      </div>
                      <div class="dropdown-info">
                        <span class="dropdown-name">{{ pet.name }}</span>
                        <span class="dropdown-sub">{{ pet.species }}</span>
                      </div>
                      <svg v-if="petSeleccionada?.id === pet.id" xmlns="http://www.w3.org/2000/svg" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round" style="color:#6C756D;flex-shrink:0"><polyline points="20 6 9 17 4 12"/></svg>
                    </div>
                  </div>
                </div>

                <!-- Alta rápida de mascota -->
                <button
                  v-if="!showInlineAddPet"
                  type="button"
                  class="wiz-add-pet-toggle"
                  @click="abrirInlineAddPet"
                >
                  <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M12 5v14M5 12h14"/></svg>
                  ¿No está en la lista? Agregar mascota
                </button>

                <div v-if="showInlineAddPet" class="wiz-add-pet">
                  <div class="wiz-add-pet-head">
                    <h5 class="wiz-add-pet-title">Nueva mascota</h5>
                    <button type="button" class="wiz-add-pet-close" @click="showInlineAddPet = false">✕</button>
                  </div>
                  <div class="form-grid form-grid--3">
                    <div class="fg">
                      <label class="fg-label">Nombre <span class="req">*</span></label>
                      <input type="text" class="fg-input" placeholder="Ej. Luna" v-model="nuevaMascota.name" />
                    </div>
                    <div class="fg">
                      <label class="fg-label">Especie <span class="req">*</span></label>
                      <select class="fg-input" v-model="nuevaMascota.type">
                        <option value="">Seleccionar...</option>
                        <option value="Perro">Perro</option>
                        <option value="Gato">Gato</option>
                        <option value="Otro">Otro</option>
                      </select>
                    </div>
                    <div class="fg">
                      <label class="fg-label">Sexo <span class="req">*</span></label>
                      <select class="fg-input" v-model="nuevaMascota.sex">
                        <option value="">Seleccionar...</option>
                        <option value="Macho">Macho</option>
                        <option value="Hembra">Hembra</option>
                      </select>
                    </div>
                  </div>
                  <p v-if="errorMascota" class="field-error">{{ errorMascota }}</p>
                  <div class="wiz-add-pet-actions">
                    <button type="button" class="btn-cancel" @click="showInlineAddPet = false">Cancelar</button>
                    <button type="button" class="btn-save" :disabled="agregandoMascota" @click="agregarMascota">
                      {{ agregandoMascota ? 'Guardando...' : 'Agregar mascota' }}
                    </button>
                  </div>
                </div>

                <p v-if="animalesLoading" class="wiz-hint">Cargando mascotas...</p>
              </section>

              <!-- ══ PASO 3 — Datos del registro (según el tipo) ══ -->
              <section v-show="pasoActual === 3" class="wiz-pane">
                <h4 class="modal-section-title">{{ tipoInfo?.titulo || 'Datos del registro' }}</h4>

                <!-- Veterinario: común a los tres tipos (veterinarian_id es NOT NULL) -->
                <div class="form-grid form-grid--4">
                  <div class="fg fg--span2">
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
                        <div v-if="veterinariosLoading" class="dropdown-empty">Cargando veterinarios...</div>
                        <div v-else-if="veterinarios.length === 0" class="dropdown-empty">No hay veterinarios registrados</div>
                        <div v-for="vet in veterinarios" :key="vet.id" class="dropdown-item" :class="{ selected: form.vetId === vet.id }" @click="seleccionarVet(vet)">
                          <div class="pet-avatar pet-avatar--sm"><span class="pet-avatar-ini">{{ vet.nombre?.charAt(0) }}</span></div>
                          <div class="dropdown-info">
                            <span class="dropdown-name">Dr. {{ vet.nombre }}</span>
                            <span class="dropdown-sub">{{ vet.especialidad || 'Veterinario' }}</span>
                          </div>
                        </div>
                        <button type="button" class="dropdown-add" @click="abrirInlineAddVet()">
                          <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M12 5v14M5 12h14"/></svg>
                          Agregar veterinario
                        </button>
                      </div>
                    </div>
                    <p v-if="errores.vet" class="field-error">{{ errores.vet }}</p>
                  </div>

                  <!-- Alta rápida de veterinario -->
                  <div v-if="showInlineAddVet" class="fg fg--full">
                    <div class="wiz-add-pet">
                      <div class="wiz-add-pet-head">
                        <h5 class="wiz-add-pet-title">Nuevo veterinario</h5>
                        <button type="button" class="wiz-add-pet-close" @click="showInlineAddVet = false">✕</button>
                      </div>
                      <div class="form-grid form-grid--3">
                        <div class="fg">
                          <label class="fg-label">Nombre <span class="req">*</span></label>
                          <input type="text" class="fg-input" placeholder="Ej. Ana" v-model="nuevoVet.nombre" />
                        </div>
                        <div class="fg">
                          <label class="fg-label">Apellido <span class="req">*</span></label>
                          <input type="text" class="fg-input" placeholder="Ej. Rojas" v-model="nuevoVet.apellido" />
                        </div>
                        <div class="fg">
                          <label class="fg-label">Especialidad <span class="req">*</span></label>
                          <input type="text" class="fg-input" placeholder="Ej. Cirugía" v-model="nuevoVet.especialidad" />
                        </div>
                        <div class="fg">
                          <label class="fg-label">Cédula</label>
                          <input type="text" class="fg-input" placeholder="Opcional" v-model="nuevoVet.cedula" />
                        </div>
                        <div class="fg">
                          <label class="fg-label">Correo</label>
                          <input type="email" class="fg-input" placeholder="Opcional" v-model="nuevoVet.correo" />
                        </div>
                      </div>
                      <p v-if="errorVet" class="field-error">{{ errorVet }}</p>
                      <div class="wiz-add-pet-actions">
                        <button type="button" class="btn-cancel" @click="showInlineAddVet = false">Cancelar</button>
                        <button type="button" class="btn-save" :disabled="guardandoVet" @click="guardarVeterinario">
                          {{ guardandoVet ? 'Guardando...' : 'Agregar veterinario' }}
                        </button>
                      </div>
                    </div>
                  </div>

                </div>

                <!-- ── Campos de Historial médico ── -->
                <div v-if="tipoRegistro === 'historial'" class="form-grid form-grid--4">
                  <div class="fg">
                    <label class="fg-label">Fecha <span class="req">*</span></label>
                    <input type="date" class="fg-input" :class="{ 'is-error': errores.fecha }" v-model="form.fecha" @change="clearErr('fecha')" />
                    <p v-if="errores.fecha" class="field-error">{{ errores.fecha }}</p>
                  </div>
                  <div class="fg">
                    <label class="fg-label">Peso (kg)</label>
                    <input type="number" class="fg-input" placeholder="Ej. 12.5" step="0.1" min="0" v-model="form.peso" />
                  </div>
                  <div class="fg fg--span2">
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

                <!-- ── Campos de Vacuna ── -->
                <div v-else-if="tipoRegistro === 'vacunas'" class="form-grid form-grid--4">
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
                  <div class="fg fg--span2">
                    <label class="fg-label">Clínica veterinaria</label>
                    <input type="text" class="fg-input" placeholder="Ej. Hospital Veterinario San José" v-model="form.clinicaVacuna" />
                  </div>

                  <div class="fg fg--full">
                    <label class="fg-label">Observaciones</label>
                    <textarea class="fg-textarea" placeholder="Notas sobre la vacuna, lote, reacciones..." v-model="form.observaciones_v"></textarea>
                  </div>
                </div>

                <!-- ── Campos de Tratamiento ── -->
                <div v-else class="form-grid form-grid--4">
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
              </section>

              <!-- ══ PASO 4 — Resumen ══ -->
              <section v-show="pasoActual === 4" class="wiz-pane">
                <h4 class="modal-section-title">Revisa antes de guardar</h4>

                <div class="wiz-resumen">
                  <!-- Tipo -->
                  <article class="wiz-res-card">
                    <header class="wiz-res-head">
                      <span class="wiz-res-title">Tipo de registro</span>
                      <button type="button" class="wiz-res-edit" @click="irAPaso(1)">Editar</button>
                    </header>
                    <strong class="wiz-res-value">{{ tipoInfo?.titulo || '—' }}</strong>
                    <span class="wiz-res-sub">{{ tipoInfo?.desc }}</span>
                  </article>

                  <!-- Mascota -->
                  <article class="wiz-res-card">
                    <header class="wiz-res-head">
                      <span class="wiz-res-title">Mascota</span>
                      <button type="button" class="wiz-res-edit" @click="irAPaso(2)">Editar</button>
                    </header>
                    <div class="wiz-res-pet">
                      <div class="pet-avatar pet-avatar--sm">
                        <span class="pet-avatar-ini">{{ petSeleccionada?.name?.charAt(0) }}</span>
                      </div>
                      <div>
                        <strong class="wiz-res-value">{{ petSeleccionada?.name || '—' }}</strong>
                        <span class="wiz-res-sub">{{ petSeleccionada?.species || 'Sin especie' }} · {{ petSeleccionada?.id }}</span>
                      </div>
                    </div>
                  </article>

                  <!-- Datos del tipo elegido -->
                  <article class="wiz-res-card wiz-res-card--full">
                    <header class="wiz-res-head">
                      <span class="wiz-res-title">{{ tipoInfo?.titulo }}</span>
                      <button type="button" class="wiz-res-edit" @click="irAPaso(3)">Editar</button>
                    </header>

                    <dl v-if="tipoRegistro === 'historial'" class="wiz-res-list">
                      <div><dt>Veterinario</dt><dd>{{ form.vet || '—' }}</dd></div>
                      <div><dt>Fecha</dt><dd>{{ formatFecha(form.fecha) }}</dd></div>
                      <div><dt>Clínica</dt><dd>{{ form.clinica || '—' }}</dd></div>
                      <div><dt>Peso</dt><dd>{{ form.peso ? form.peso + ' kg' : '—' }}</dd></div>
                      <div class="wiz-res-full"><dt>Diagnóstico</dt><dd>{{ form.diagnostico || '—' }}</dd></div>
                      <div v-if="form.observaciones_h" class="wiz-res-full"><dt>Observaciones</dt><dd>{{ form.observaciones_h }}</dd></div>
                    </dl>

                    <dl v-else-if="tipoRegistro === 'vacunas'" class="wiz-res-list">
                      <div><dt>Veterinario</dt><dd>{{ form.vet || '—' }}</dd></div>
                      <div class="wiz-res-full"><dt>Tipo de vacuna</dt><dd>{{ form.tipoVacuna || '—' }}</dd></div>
                      <div><dt>Aplicación</dt><dd>{{ formatFecha(form.fechaAplicacion) }}</dd></div>
                      <div><dt>Próxima dosis</dt><dd>{{ form.proximaDosis ? formatFecha(form.proximaDosis) : '—' }}</dd></div>
                      <div><dt>Clínica</dt><dd>{{ form.clinicaVacuna || '—' }}</dd></div>
                      <div v-if="form.observaciones_v" class="wiz-res-full"><dt>Observaciones</dt><dd>{{ form.observaciones_v }}</dd></div>
                    </dl>

                    <dl v-else class="wiz-res-list">
                      <div><dt>Veterinario</dt><dd>{{ form.vet || '—' }}</dd></div>
                      <div class="wiz-res-full"><dt>Tipo de tratamiento</dt><dd>{{ form.tipoTratamiento || '—' }}</dd></div>
                      <div><dt>Fecha</dt><dd>{{ formatFecha(form.fechaTrat) }}</dd></div>
                      <div><dt>Medicamento</dt><dd>{{ form.medicamento || '—' }}</dd></div>
                      <div><dt>Dosis</dt><dd>{{ form.dosis || '—' }}</dd></div>
                      <div v-if="form.observaciones_t" class="wiz-res-full"><dt>Observaciones</dt><dd>{{ form.observaciones_t }}</dd></div>
                    </dl>
                  </article>
                </div>

                <!-- Nota inmutable -->
                <div class="immutable-note">
                  <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
                  Los registros médicos son permanentes y no pueden editarse ni eliminarse una vez guardados
                </div>
              </section>

            </div>

            <!-- ── Navegación del wizard ── -->
            <div class="modal-footer wiz-footer">
              <button class="btn-cancel" @click="cerrarModalRegistrar">Cancelar</button>

              <div class="wiz-nav">
                <button
                  v-if="pasoActual > 1"
                  class="btn-cancel btn-back"
                  @click="pasoAnterior"
                >
                  <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="15 18 9 12 15 6"/></svg>
                  Atrás
                </button>

                <button v-if="!esUltimoPaso" class="btn-save" @click="pasoSiguiente">
                  Siguiente
                  <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="9 18 15 12 9 6"/></svg>
                </button>

                <button v-else class="btn-save" @click="intentarGuardar">
                  <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
                  Guardar expediente
                </button>
              </div>
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
              <h3 class="confirm-title">¿Guardar este registro?</h3>
              <p class="confirm-text">Se registrará <strong>{{ tipoInfo?.titulo?.toLowerCase() }}</strong> para <strong>{{ petSeleccionada?.name }}</strong>. Esta acción es permanente y no podrá modificarse.</p>
            </div>
            <div class="modal-footer">
              <button class="btn-cancel" @click="showModalConfirm = false">Cancelar</button>
              <button class="btn-save" :disabled="guardandoExpediente" @click="confirmarGuardar">
                {{ guardandoExpediente ? 'Guardando...' : 'Confirmar y guardar' }}
              </button>
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
              <span class="rec-tipo-badge" :class="`rec-tipo-badge--${registroVer.prefijo}`">
                {{ TITULO_POR_PREFIJO[registroVer.prefijo] }}
              </span>
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
              <div v-if="registroVer.prefijo === 'H'" class="modal-section">
                <h4 class="modal-section-title">Historial médico</h4>
                <div class="modal-grid">
                  <div class="modal-field modal-field--full">
                    <span class="modal-field-label">Diagnóstico</span>
                    <strong class="modal-field-value monto-highlight">{{ registroVer.diagnostico }}</strong>
                  </div>
                  <div class="modal-field">
                    <span class="modal-field-label">Fecha</span>
                    <strong class="modal-field-value">{{ formatFecha(registroVer.fecha) }}</strong>
                  </div>
                  <div class="modal-field">
                    <span class="modal-field-label">Peso</span>
                    <strong class="modal-field-value">{{ registroVer.peso ? registroVer.peso + ' kg' : '—' }}</strong>
                  </div>
                  <div class="modal-field">
                    <span class="modal-field-label">Veterinario</span>
                    <strong class="modal-field-value">{{ registroVer.vet || '—' }}</strong>
                  </div>
                  <div class="modal-field">
                    <span class="modal-field-label">Clínica</span>
                    <strong class="modal-field-value">{{ registroVer.clinica || '—' }}</strong>
                  </div>
                  <div v-if="registroVer.observaciones" class="modal-field modal-field--full">
                    <span class="modal-field-label">Observaciones</span>
                    <p class="modal-mensaje">{{ registroVer.observaciones }}</p>
                  </div>
                </div>
              </div>

              <!-- Vacuna -->
              <div v-else-if="registroVer.prefijo === 'V'" class="modal-section">
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
                    <strong v-if="registroVer.proximaDosis" class="modal-field-value">
                      {{ formatFecha(registroVer.proximaDosis) }}
                      <span class="estado-badge" :class="estadoRefuerzo(registroVer.proximaDosis)?.clase" style="margin-left:6px">
                        {{ estadoRefuerzo(registroVer.proximaDosis)?.titulo }}
                      </span>
                    </strong>
                    <strong v-else class="modal-field-value">—</strong>
                  </div>
                  <div class="modal-field">
                    <span class="modal-field-label">Veterinario</span>
                    <strong class="modal-field-value">{{ registroVer.vet || '—' }}</strong>
                  </div>
                  <div class="modal-field">
                    <span class="modal-field-label">Clínica</span>
                    <strong class="modal-field-value">{{ registroVer.clinica || '—' }}</strong>
                  </div>
                  <div v-if="registroVer.observaciones" class="modal-field modal-field--full">
                    <span class="modal-field-label">Observaciones</span>
                    <p class="modal-mensaje">{{ registroVer.observaciones }}</p>
                  </div>
                </div>
              </div>

              <!-- Tratamiento -->
              <div v-else class="modal-section">
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
                    <span class="modal-field-label">Veterinario</span>
                    <strong class="modal-field-value">{{ registroVer.vet || '—' }}</strong>
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
   los Teleport (modales) también las hereden.
   :global() es obligatorio: en <style scoped> Vue le añade
   el atributo de scope a ":root", que nunca coincide con <html>
   y deja estas variables sin definir en todo el componente.
══════════════════════════════════════════ */
:global(:root) {
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
.sal-toast.success { background: var(--verde); color: #fff; }
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
  background: var(--verde);
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

.page-actions { display: flex; gap: 8px; align-items: center; }

/* Secundario real: hasta ahora se reusaba btn-cancel (gris de "cancelar") */
.btn-secondary {
  display: flex;
  align-items: center;
  gap: 7px;
  height: 38px;
  padding: 0 16px;
  background: transparent;
  color: var(--verde);
  border: 1.5px solid var(--borde);
  border-radius: 8px;
  font-size: 13px;
  font-weight: 700;
  cursor: pointer;
  transition: border-color 0.18s, background 0.18s;
  white-space: nowrap;
  flex-shrink: 0;
  font-family: inherit;
}
.btn-secondary:hover {
  border-color: var(--verde);
  background: var(--fondo);
}
 
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
.kpi-historial  { border-top: 3px solid var(--verde-sec); }
.kpi-vencidas   { border-top: 3px solid #C0392B; }
.kpi-porvencer  { border-top: 3px solid var(--amarillo); }
.kpi-mascotas   { border-top: 3px solid var(--verde); }

/* Los KPI de refuerzos son botones: saltan a vacunas ya filtrado */
button.don-card {
  text-align: left;
  font-family: inherit;
  cursor: pointer;
  transition: border-color 0.18s, box-shadow 0.18s, transform 0.12s;
}
button.don-card:hover {
  border-color: var(--verde-sec);
  box-shadow: 0 4px 14px rgba(58,71,60,0.10);
  transform: translateY(-1px);
}
.don-card--alerta .don-value { color: #C0392B; }

.don-nota {
  font-size: 11px;
  color: var(--texto-sec);
  font-weight: 500;
  margin-top: -2px;
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
 
/* ── Controles de filtro ───────────────── */
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
 
/* ── Panel de registros ────────────────── */
.table-wrapper {
  background: var(--blanco);
  border-radius: 14px;
  border: 1px solid var(--borde);
  overflow: hidden;
}

/* Pestañas: navegación principal del panel */
.panel-tabs {
  display: flex;
  gap: 2px;
  padding: 0 8px;
  border-bottom: 1px solid var(--borde);
  overflow-x: auto;
  -webkit-overflow-scrolling: touch;
}
.panel-tab {
  display: flex;
  align-items: center;
  gap: 7px;
  padding: 15px 16px 13px;
  border: none;
  border-bottom: 2.5px solid transparent;
  background: transparent;
  color: var(--texto-sec);
  font-size: 13px;
  font-weight: 700;
  cursor: pointer;
  font-family: inherit;
  white-space: nowrap;
  transition: color 0.18s, border-color 0.18s;
  margin-bottom: -1px;
}
.panel-tab:hover { color: var(--verde); }
.panel-tab--active {
  color: var(--verde);
  border-bottom-color: var(--verde);
}
.panel-tab-count {
  min-width: 20px;
  padding: 2px 7px;
  border-radius: 20px;
  background: var(--fondo);
  color: var(--texto-sec);
  font-size: 11px;
  font-weight: 800;
  text-align: center;
}
.panel-tab--active .panel-tab-count {
  background: var(--verde);
  color: var(--blanco);
}

/* Filtros dentro del panel */
.panel-filtros {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 14px 16px;
  border-bottom: 1px solid var(--borde);
  flex-wrap: wrap;
}
.panel-buscar { flex: 1; min-width: 200px; max-width: 360px; }

.panel-fechas {
  display: flex;
  align-items: center;
  gap: 7px;
}
.panel-fechas .filtro-input { width: auto; min-width: 132px; }
.panel-fechas-label {
  font-size: 12px;
  font-weight: 600;
  color: var(--texto-sec);
}

.filtro-chip {
  display: inline-flex;
  align-items: center;
  gap: 7px;
  height: 30px;
  padding: 0 6px 0 12px;
  border-radius: 20px;
  background: var(--verde);
  color: var(--blanco);
  font-size: 12px;
  font-weight: 700;
  white-space: nowrap;
}
.filtro-chip button {
  border: none;
  background: rgba(255,255,255,0.2);
  color: inherit;
  width: 18px;
  height: 18px;
  border-radius: 50%;
  cursor: pointer;
  font-size: 10px;
  font-family: inherit;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0;
}
.filtro-chip button:hover { background: rgba(255,255,255,0.35); }
.table-scroll { overflow-x: auto; -webkit-overflow-scrolling: touch; }
.don-table { width: 100%; border-collapse: collapse; min-width: 680px; }
.don-table thead th {
  background: var(--verde);
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
  border: 1.5px solid #EEF3EE;
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

/* Fila completa clicable */
.don-row--click { cursor: pointer; }

/* Distintivo de tipo de registro */
.rec-tipo-badge {
  display: inline-block;
  font-size: 11px;
  font-weight: 800;
  padding: 4px 12px;
  border-radius: 20px;
  text-transform: uppercase;
  letter-spacing: 0.4px;
  white-space: nowrap;
}
.rec-tipo-badge--H { background: #EEF2EE; color: #3A473C; }
.rec-tipo-badge--V { background: #E8F5E9; color: #2E7D32; }
.rec-tipo-badge--T { background: #FFF7E0; color: #96650A; }

/* ── Tarjetas de registro (solo móvil) ── */
.rec-cards { display: none; }

.rec-card {
  display: block;
  width: 100%;
  text-align: left;
  padding: 14px;
  border: none;
  border-bottom: 1px solid var(--borde);
  background: transparent;
  cursor: pointer;
  font-family: inherit;
}
.rec-card:last-of-type { border-bottom: none; }
.rec-card:active { background: #F4F6F4; }

.rec-card-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  margin-bottom: 12px;
}

.rec-card-list {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 10px 12px;
  margin: 0;
  padding-top: 12px;
  border-top: 1px solid var(--borde);
}
.rec-card-list > div { min-width: 0; }
.rec-card-list dt {
  font-size: 10px;
  font-weight: 700;
  color: var(--texto-sec);
  text-transform: uppercase;
  letter-spacing: 0.4px;
}
.rec-card-list dd {
  font-size: 13px;
  font-weight: 600;
  color: var(--texto);
  margin: 3px 0 0;
  word-break: break-word;
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
  z-index: 1000;
  display: flex; align-items: center; justify-content: center;
  padding: 20px;
}
.modal-overlay--top { z-index: 1100; }
 
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
.modal-field-label { font-size: 10px; font-weight: 700; color: var(--texto-sec); text-transform: uppercase; letter-spacing: 0.4px; }
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
.fg-input.is-error { border-color: #c0392b; background: #fff8f8; }
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
.pet-selector-btn.is-error { border-color: #c0392b; }
.psel-placeholder { color: var(--texto-sec); flex: 1; font-size: 13px; }
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
.dropdown-item.selected { background: var(--fondo); }
.dropdown-info { display: flex; flex-direction: column; gap: 1px; flex: 1; min-width: 0; }
.dropdown-name { font-size: 13px; font-weight: 700; color: #2F352F; color: var(--texto, #2F352F); }
.dropdown-sub  { font-size: 11px; color: #92A894; color: var(--verde-sec, #92A894); }
 
/* ══════════════════════════════════════════
   WIZARD — registro paso a paso
══════════════════════════════════════════ */
.modal-box--wizard {
  display: flex;
  flex-direction: column;
  padding: 0;
  overflow: hidden;
  --verde:     var(--sal-verde);
  --verde-sec: var(--sal-verde-sec);
  --fondo:     var(--sal-fondo);
  --blanco:    var(--sal-blanco);
  --texto:     var(--sal-texto);
  --texto-sec: var(--sal-texto-sec);
  --borde:     var(--sal-borde);
  --verde-ok:  var(--sal-verde-ok);
}

.wiz-header {
  padding: 24px 36px 18px;
  border-bottom: 1px solid var(--borde, #E8ECE8);
  background: var(--blanco, #FFFFFF);
  flex-shrink: 0;
}

/* Stepper */
.wiz-steps {
  position: relative;
  display: flex;
  justify-content: space-between;
  gap: 6px;
  margin-top: 18px;
}
.wiz-track {
  position: absolute;
  top: 15px;
  left: 6%;
  right: 6%;
  height: 2px;
  background: var(--borde, #E8ECE8);
  border-radius: 2px;
}
.wiz-track-fill {
  height: 100%;
  background: var(--verde, #3A473C);
  border-radius: 2px;
  transition: width 0.32s cubic-bezier(0.4, 0, 0.2, 1);
}

.wiz-step {
  position: relative;
  z-index: 1;
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 7px;
  background: transparent;
  border: none;
  padding: 0;
  cursor: pointer;
  font-family: inherit;
  min-width: 0;
}
.wiz-step.is-locked { cursor: default; }

.wiz-bullet {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: 800;
  background: var(--blanco, #FFFFFF);
  border: 2px solid var(--borde, #E8ECE8);
  color: var(--texto-sec, #6C756D);
  transition: all 0.22s ease;
  flex-shrink: 0;
}
.wiz-step-label {
  font-size: 11px;
  font-weight: 700;
  color: var(--texto-sec, #6C756D);
  text-align: center;
  letter-spacing: 0.2px;
  transition: color 0.22s ease;
  max-width: 100%;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.wiz-step.is-done .wiz-bullet {
  background: var(--verde, #3A473C);
  border-color: var(--verde, #3A473C);
  color: #FFFFFF;
}
.wiz-step.is-done .wiz-step-label { color: var(--verde, #3A473C); }

.wiz-step.is-active .wiz-bullet {
  background: var(--verde, #3A473C);
  border-color: var(--verde, #3A473C);
  color: #FFFFFF;
  box-shadow: 0 0 0 4px rgba(58, 71, 60, 0.12);
}
.wiz-step.is-active .wiz-step-label {
  color: var(--verde, #3A473C);
  font-weight: 800;
}

.wiz-step:not(.is-locked):not(.is-active):hover .wiz-bullet {
  border-color: var(--verde-sec, #92A894);
}

/* Contexto bajo el stepper */
.wiz-context {
  display: flex;
  align-items: center;
  gap: 7px;
  margin-top: 16px;
  font-size: 12px;
  color: var(--texto-sec, #6C756D);
  flex-wrap: wrap;
}
.wiz-context-count {
  font-weight: 800;
  color: var(--verde, #3A473C);
  text-transform: uppercase;
  letter-spacing: 0.5px;
  font-size: 11px;
}
.wiz-context-sep { opacity: 0.5; }
.wiz-context-desc { font-weight: 500; }
.wiz-context-pet {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  margin-left: auto;
  padding: 4px 10px 4px 4px;
  background: var(--fondo, #F7F8F7);
  border: 1px solid var(--borde, #E8ECE8);
  border-radius: 20px;
  font-size: 12px;
  font-weight: 700;
  color: var(--verde, #3A473C);
}
.pet-avatar--xs {
  width: 22px;
  height: 22px;
  border-radius: 50%;
  background: #DDE6DE;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  overflow: hidden;
}
.pet-avatar--xs .pet-avatar-ini { font-size: 10px; }

/* Cuerpo con scroll propio */
.wiz-body {
  flex: 1;
  overflow-y: auto;
  padding: 26px 36px;
  min-height: 260px;
}
.wiz-pane { animation: wiz-in 0.26s ease; }
@keyframes wiz-in {
  from { opacity: 0; transform: translateX(10px); }
  to   { opacity: 1; transform: translateX(0); }
}
.wiz-hint {
  font-size: 12px;
  color: var(--texto-sec, #6C756D);
  margin-top: 10px;
}

/* Paso 1 — tarjetas de tipo de registro */
.tipo-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 12px;
}
.tipo-card {
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 6px;
  padding: 18px 16px;
  border: 1.5px solid var(--borde, #E8ECE8);
  border-radius: 12px;
  background: var(--blanco, #FFFFFF);
  cursor: pointer;
  font-family: inherit;
  text-align: left;
  transition: border-color 0.18s, background 0.18s, box-shadow 0.18s;
}
.tipo-card:hover {
  border-color: var(--verde-sec, #92A894);
  background: var(--fondo, #F7F8F7);
}
.tipo-card.is-selected {
  border-color: var(--verde, #3A473C);
  background: var(--fondo, #F7F8F7);
  box-shadow: 0 0 0 3px rgba(58, 71, 60, 0.10);
}
.tipo-card-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 38px;
  height: 38px;
  border-radius: 10px;
  background: #EEF2EE;
  color: var(--verde, #3A473C);
  margin-bottom: 2px;
}
.tipo-card.is-selected .tipo-card-icon {
  background: var(--verde, #3A473C);
  color: #FFFFFF;
}
.tipo-card-title {
  font-size: 14px;
  font-weight: 800;
  color: var(--verde, #3A473C);
}
.tipo-card-desc {
  font-size: 11.5px;
  color: var(--texto-sec, #6C756D);
  line-height: 1.4;
  font-weight: 500;
}
.tipo-card-check {
  position: absolute;
  top: 14px;
  right: 14px;
  color: var(--verde, #3A473C);
}

.wiz-res-card--full { grid-column: 1 / -1; }

/* Acción "Agregar..." fija al pie de un desplegable */
.dropdown-add {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  width: 100%;
  margin-top: 4px;
  padding: 9px 10px;
  border: none;
  border-top: 1px solid var(--borde, #E8ECE8);
  border-radius: 0 0 8px 8px;
  background: transparent;
  color: var(--verde, #3A473C);
  font-size: 12px;
  font-weight: 700;
  cursor: pointer;
  font-family: inherit;
  transition: background 0.15s;
}
.dropdown-add:hover { background: var(--fondo, #F7F8F7); }

/* Alta rápida de mascota */
.wiz-add-pet-toggle {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  margin-top: 14px;
  padding: 8px 14px;
  border-radius: 8px;
  border: 1.5px dashed var(--borde, #E8ECE8);
  background: transparent;
  color: var(--texto-sec, #6C756D);
  font-size: 12px;
  font-weight: 700;
  cursor: pointer;
  font-family: inherit;
  transition: all 0.18s;
}
.wiz-add-pet-toggle:hover {
  border-color: var(--verde-sec, #92A894);
  color: var(--verde, #3A473C);
}

.wiz-add-pet {
  margin-top: 16px;
  padding: 16px;
  border-radius: 12px;
  border: 1.5px solid var(--borde, #E8ECE8);
  background: var(--fondo, #F7F8F7);
}
.wiz-add-pet-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 12px;
}
.wiz-add-pet-title {
  font-size: 12px;
  font-weight: 800;
  color: var(--verde, #3A473C);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}
.wiz-add-pet-close {
  border: none;
  background: transparent;
  color: var(--texto-sec, #6C756D);
  font-size: 12px;
  cursor: pointer;
  font-family: inherit;
  padding: 2px 6px;
}
.wiz-add-pet-actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
  margin-top: 14px;
}
.wiz-add-pet-actions .btn-save:disabled { opacity: 0.6; cursor: default; }
.form-grid--3 { grid-template-columns: repeat(3, 1fr); }

/* Resumen final */
.wiz-resumen {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 14px;
}
.wiz-res-card {
  border: 1.5px solid var(--borde, #E8ECE8);
  border-radius: 12px;
  padding: 16px;
  background: var(--blanco, #FFFFFF);
}
.wiz-res-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  margin-bottom: 12px;
  padding-bottom: 10px;
  border-bottom: 1px solid var(--borde, #E8ECE8);
}
.wiz-res-title {
  font-size: 11px;
  font-weight: 800;
  color: var(--verde, #3A473C);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}
.wiz-res-edit {
  border: none;
  background: transparent;
  color: var(--texto-sec, #6C756D);
  font-size: 11px;
  font-weight: 700;
  cursor: pointer;
  font-family: inherit;
  text-decoration: underline;
  padding: 0;
}
.wiz-res-edit:hover { color: var(--verde, #3A473C); }

.wiz-res-pet { display: flex; align-items: center; gap: 10px; }
.wiz-res-value {
  display: block;
  font-size: 14px;
  font-weight: 700;
  color: var(--texto, #2F352F);
}
.wiz-res-sub {
  display: block;
  font-size: 11px;
  color: var(--texto-sec, #6C756D);
  margin-top: 2px;
}

.wiz-res-list {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 12px;
  margin: 0;
}
.wiz-res-list > div { min-width: 0; }
.wiz-res-full { grid-column: 1 / -1; }
.wiz-res-list dt {
  font-size: 10px;
  font-weight: 700;
  color: var(--texto-sec, #6C756D);
  text-transform: uppercase;
  letter-spacing: 0.4px;
}
.wiz-res-list dd {
  font-size: 13px;
  font-weight: 600;
  color: var(--texto, #2F352F);
  margin: 3px 0 0;
  word-break: break-word;
}

/* Footer de navegación */
.wiz-footer {
  justify-content: space-between;
  align-items: center;
  padding: 18px 36px;
  margin-top: 0;
  flex-shrink: 0;
  background: var(--blanco, #FFFFFF);
}
.wiz-nav { display: flex; gap: 10px; }
.btn-back {
  display: flex;
  align-items: center;
  gap: 6px;
}

@media (max-width: 768px) {
  .wiz-header { padding: 24px 18px 16px; }
  .wiz-body   { padding: 20px 18px; }
  .wiz-footer { padding: 14px 18px; }

  .wiz-step-label { display: none; }
  .wiz-steps { justify-content: center; gap: 0; }
  .wiz-track { top: 15px; left: 10%; right: 10%; }

  .wiz-context-pet { margin-left: 0; }

  .wiz-resumen { grid-template-columns: 1fr; }
  .form-grid--3 { grid-template-columns: 1fr; }
  .tipo-grid { grid-template-columns: 1fr; }

  .wiz-footer { flex-direction: column-reverse; align-items: stretch; gap: 8px; }
  .wiz-nav { width: 100%; }
  .wiz-nav .btn-cancel,
  .wiz-nav .btn-save { flex: 1; justify-content: center; }
}

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
  .page-actions { width: 100%; }
  .btn-primary, .btn-secondary { flex: 1; justify-content: center; }
  .btn-limpiar { width: 100%; }
  .don-summary { grid-template-columns: 1fr; }
  .form-grid--4 { grid-template-columns: 1fr; }
  .fg--span2, .fg--full { grid-column: 1; }
  .modal-box { padding: 24px 18px; }
  .modal-grid { grid-template-columns: 1fr; }

  /* La tabla cede el paso a las tarjetas: nada de scroll horizontal
     ni de ocultar columnas (antes se perdían Aplicación y Próxima dosis) */
  .table-scroll { display: none; }
  .rec-cards    { display: block; }
  .rec-card-list { grid-template-columns: 1fr 1fr; }
}

/* ── MOBILE RESPONSIVE ── */
@media (max-width: 768px) {
  .don-summary {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 10px;
  }

  .panel-filtros {
    flex-direction: column;
    align-items: stretch;
    gap: 10px;
    padding: 14px;
  }
  .panel-buscar { max-width: none; }
  .panel-fechas { width: 100%; }
  .panel-fechas .filtro-input { flex: 1; min-width: 0; }

  .panel-tab { padding: 13px 12px 11px; font-size: 12px; }

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

/* El wizard maneja su propio padding y scroll: anula las reglas
   genéricas de .modal-box--lg / .modal-footer de arriba */
@media (max-width: 768px) {
  .modal-box--lg.modal-box--wizard { padding: 0; }
  .modal-footer.wiz-footer {
    flex-direction: column-reverse;
    padding: 14px 18px;
  }
}
@media (max-width: 640px) {
  .modal-box.modal-box--wizard { padding: 0; }
}


</style>