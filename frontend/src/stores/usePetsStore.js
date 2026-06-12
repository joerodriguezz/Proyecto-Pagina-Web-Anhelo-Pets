// src/stores/usePetsStore.js

import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const usePetsStore = defineStore('pets', () => {

  // ─────────────────────────────────────────────
  // Estado global
  // ─────────────────────────────────────────────
  const nextId = ref(1)

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

    // computed
    publicPets,
    adoptedPets,
    featuredPets,

    // acciones
    addPet,
    updatePet,
    changeStatus,
    deactivatePet,
    toggleFeatured,
    addRequest,
  }
})