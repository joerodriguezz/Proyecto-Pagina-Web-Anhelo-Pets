// src/stores/usePetsStore.js

import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { animalsApi } from '../services/api'

export const usePetsStore = defineStore('pets', () => {

  // ─────────────────────────────────────────────
  // Estado global
  // ─────────────────────────────────────────────
  const nextId = ref(1)
  const isLoading = ref(false)
  const loadError = ref('')

  const pets = ref(

  JSON.parse(
    localStorage.getItem('anhelo_pets')
  ) || []

)

function savePets() {

  localStorage.setItem(
    'anhelo_pets',
    JSON.stringify(pets.value)
  )

}

function formatAge(animal) {
  const years = Number(animal.ageYears || 0)
  const months = Number(animal.ageMonths || 0)

  if (years > 0 && months > 0) {
    return `${years} año${years !== 1 ? 's' : ''} y ${months} mes${months !== 1 ? 'es' : ''}`
  }

  if (years > 0) {
    return `${years} año${years !== 1 ? 's' : ''}`
  }

  if (months > 0) {
    return `${months} mes${months !== 1 ? 'es' : ''}`
  }

  return 'Edad por confirmar'
}

function normalizePhotoUrl(photoUrl) {
  if (!photoUrl || typeof photoUrl !== 'string') {
    return '/img-mascotas/mascotas.jpg'
  }

  const trimmed = photoUrl.trim()

  if (trimmed.startsWith('/')) {
    return trimmed
  }

  try {
    const url = new URL(trimmed)
    if (!['http:', 'https:'].includes(url.protocol)) {
      return '/img-mascotas/mascotas.jpg'
    }

    if (!url.hostname.includes('.')) {
      return '/img-mascotas/mascotas.jpg'
    }

    return trimmed
  } catch {
    return '/img-mascotas/mascotas.jpg'
  }
}

function normalizeAnimal(animal) {
  const image = normalizePhotoUrl(animal.photoUrl)

  return {
    id: animal.animalId,
    name: animal.animalName,
    type: animal.species,
    breed: animal.breed,
    age: formatAge(animal),
    sex: animal.sex,
    size: '',
    personality: animal.photoDescription || '',
    healthBasic: animal.healthStatus,
    status: animal.animalStatus,
    description: animal.description,
    internalNotes: '',
    image,
    images: [
      {
        preview: image,
      },
    ],
    featured: false,
    requests: [],
  }
}

async function loadPets() {
  isLoading.value = true
  loadError.value = ''

  try {
    const animals = await animalsApi.getAll({
      status: 'Todos',
    })

    pets.value = Array.isArray(animals)
      ? animals.map(normalizeAnimal)
      : []

    savePets()
  } catch (error) {
    loadError.value = error?.message || 'No se pudieron cargar las mascotas.'
  } finally {
    isLoading.value = false
  }
}

  // ─────────────────────────────────────────────
  // Catálogo público
  // Solo Disponible y En proceso
  // ─────────────────────────────────────────────
  const publicPets = computed(() =>
    pets.value.filter(p =>
      p.status === 'Disponible' ||
      p.status === 'En proceso'
    )
  )

  // ─────────────────────────────────────────────
  // Historias felices
  // ─────────────────────────────────────────────
  const adoptedPets = computed(() =>
    pets.value.filter(p => p.status === 'Adoptada')
  )

  // ─────────────────────────────────────────────
  // Mascotas destacadas
  // Máximo 3 visibles
  // ─────────────────────────────────────────────
  const featuredPets = computed(() => {

    const manualFeatured = pets.value.filter(p =>
      p.featured &&
      (
        p.status === 'Disponible' ||
        p.status === 'En proceso'
      )
    )

    const automaticFeatured = pets.value.filter(p =>
      !p.featured &&
      (
        p.status === 'Disponible' ||
        p.status === 'En proceso'
      )
    )

    const combined = [
      ...manualFeatured,
      ...automaticFeatured
    ]

    return combined.slice(0, 3)
  })

  // ─────────────────────────────────────────────
  // Registrar mascota
  // ─────────────────────────────────────────────
  function addPet(data) {

    pets.value.unshift({
      id: `M-${String(nextId.value).padStart(3, '0')}`,

      name: data.name,
      type: data.type,
      breed: data.breed,
      age: data.age,
      sex: data.sex,
      size: data.size,

      personality: data.personality,
      healthBasic: data.healthBasic,

      status: data.status || 'Disponible',

      description: data.description,
      internalNotes: data.internalNotes || '',

      images: data.images || [],

      featured: false,

      requests: [],

      createdAt: new Date().toLocaleDateString('es-CR'),
    })

    nextId.value++

savePets()
  }

  // ─────────────────────────────────────────────
  // Editar mascota
  // ─────────────────────────────────────────────
  function updatePet(id, data) {

    const index = pets.value.findIndex(p => p.id === id)

    if (index === -1) return

    pets.value[index] = {
      ...pets.value[index],

      name: data.name,
      type: data.type,
      breed: data.breed,
      age: data.age,
      sex: data.sex,
      size: data.size,

      personality: data.personality,
      healthBasic: data.healthBasic,

      status: data.status,

      description: data.description,
      internalNotes: data.internalNotes,

      images: data.images,

      featured: data.featured,
    }

    validateFeatured(pets.value[index])

savePets()
  }

  // ─────────────────────────────────────────────
  // Cambiar estado
  // ─────────────────────────────────────────────
  function changeStatus(id, newStatus) {

    const pet = pets.value.find(p => p.id === id)

    if (!pet) return

    pet.status = newStatus

    validateFeatured(pet)

savePets()
  }

  // ─────────────────────────────────────────────
  // Desactivar mascota
  // ─────────────────────────────────────────────
  function deactivatePet(id) {

    const pet = pets.value.find(p => p.id === id)

    if (!pet) return

    pet.status = 'Inactiva'
    pet.featured = false

savePets()
  }

  // ─────────────────────────────────────────────
  // Destacadas
  // ─────────────────────────────────────────────
  function toggleFeatured(id) {

    const pet = pets.value.find(p => p.id === id)

    if (!pet) return

    if (
      pet.status === 'Adoptada' ||
      pet.status === 'Inactiva'
    ) {
      return
    }

    const currentFeatured = pets.value.filter(p =>
      p.featured &&
      p.id !== id
    ).length

    if (!pet.featured && currentFeatured >= 3) {
      alert('Ya hay 3 mascotas destacadas.')
      return
    }

    pet.featured = !pet.featured

savePets()
  }

  // ─────────────────────────────────────────────
  // Validar destacadas
  // ─────────────────────────────────────────────
  function validateFeatured(pet) {

    if (
      pet.status === 'Adoptada' ||
      pet.status === 'Inactiva'
    ) {
      pet.featured = false
    }
  }

  // ─────────────────────────────────────────────
  // Solicitudes de adopción
  // ─────────────────────────────────────────────
  function addRequest(petId, requestData) {

    const pet = pets.value.find(p => p.id === petId)

    if (!pet) return

    if (!pet.requests) {
      pet.requests = []
    }

    pet.requests.unshift({
      id: Date.now(),

      applicantName: requestData.applicantName,
      email: requestData.email,
      phone: requestData.phone,

      reason: requestData.reason,

      status: 'Pendiente',

      createdAt: new Date().toLocaleDateString('es-CR'),
    })

    savePets()
    
  }

  // ─────────────────────────────────────────────
  // Retorno
  // ─────────────────────────────────────────────
  return {

    // estado
    pets,
    nextId,
    isLoading,
    loadError,

    // computed
    publicPets,
    adoptedPets,
    featuredPets,

    // acciones
    addPet,
    loadPets,
    updatePet,
    changeStatus,
    deactivatePet,
    toggleFeatured,
    addRequest,
  }
})
