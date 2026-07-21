import { ref, computed } from 'vue'
import { getRescues, getRescue, createRescue, updateRescue, closeRescue } from '../services/rescueServices'
import { getAnimals } from '../services/petServices'

const rescates = ref([])
const loaded = ref(false)

function mapDto(dto) {
  return {
    id:            dto.rescateId,
    animalId:      dto.animalId ?? '',
    mascota:       dto.animalName || '—',
    fechaRescate:  dto.fecha || '',
    ubicacion:     dto.ubicacion || '—',
    descripcion:   dto.descripcion || '',
    estado:        dto.status || 'Activo',
    fosterHomeId:  dto.fosterHomeId,
    fosterHomeName: dto.fosterHomeName || '',
    volunteerId:   dto.volunteerId || null,
    fechaCreacion: dto.createdAt || '',
    creadoPor:     dto.createdBy || '',
  }
}

async function ensureAnimalsCache() {
  // lazy-load animals into a map for enriching rescue data
  const { data } = await getAnimals()
  const map = {}
  for (const a of data || []) {
    map[a.animalId] = a
  }
  return map
}

export function useRescuesStore() {
  async function fetchRescues() {
    try {
      const { data } = await getRescues()
      rescates.value = (data || []).map(mapDto)
      loaded.value = true
    } catch {
      rescates.value = []
      loaded.value = true
    }
  }

  async function fetchRescueById(id) {
    try {
      const { data } = await getRescue(id)
      return data ? mapDto(data) : null
    } catch {
      return null
    }
  }

  async function addRescue(form) {
    const { data } = await createRescue({
      animalId:      form.animalId || null,
      fecha:         form.fechaRescate || null,
      ubicacion:     form.ubicacion || '',
      descripcion:   form.descripcion || '',
      status:        form.estado || 'Activo',
      fosterHomeId:  form.fosterHomeId || null,
      volunteerId:   form.volunteerId || null,
    })
    if (data) {
      rescates.value.unshift(mapDto(data))
    }
  }

  async function editRescue(id, form) {
    const { data } = await updateRescue(id, {
      animalId:      form.animalId || null,
      fecha:         form.fechaRescate || null,
      ubicacion:     form.ubicacion || '',
      descripcion:   form.descripcion || '',
      status:        form.estado || 'Activo',
      fosterHomeId:  form.fosterHomeId || null,
      volunteerId:   form.volunteerId || null,
    })
    if (data) {
      const idx = rescates.value.findIndex(r => r.id === id)
      if (idx !== -1) rescates.value[idx] = mapDto(data)
    }
  }

  async function removeRescue(id) {
    await closeRescue(id)
    const idx = rescates.value.findIndex(r => r.id === id)
    if (idx !== -1) rescates.value[idx].estado = 'Cerrado'
  }

  const rescatesActivos = computed(() =>
    rescates.value.filter(r => r.estado === 'Activo')
  )

  const totalRescates = computed(() => rescates.value.length)

  return {
    rescates,
    loaded,
    rescatesActivos,
    totalRescates,
    fetchRescues,
    fetchRescueById,
    addRescue,
    editRescue,
    removeRescue,
  }
}
