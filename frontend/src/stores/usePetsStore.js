import { defineStore } from 'pinia'
import { ref, watch } from 'vue'

const STORAGE_KEY = 'anhelo_pets'

function loadPets() {
  try {
    return JSON.parse(localStorage.getItem(STORAGE_KEY)) || []
  } catch {
    return []
  }
}

export const usePetsStore = defineStore('pets', () => {
  const pets = ref(loadPets())

  watch(pets, (val) => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(val))
  }, { deep: true })

  function addPet(pet) {
    const id = pets.value.length ? Math.max(...pets.value.map(p => p.id)) + 1 : 1
    pets.value.push({ ...pet, id })
  }

  function updatePet(id, data) {
    const idx = pets.value.findIndex(p => p.id === id)
    if (idx !== -1) {
      pets.value[idx] = { ...pets.value[idx], ...data }
    }
  }

  function changeStatus(id, status) {
    const pet = pets.value.find(p => p.id === id)
    if (pet) pet.status = status
  }

  function deactivatePet(id) {
    const pet = pets.value.find(p => p.id === id)
    if (pet) {
      pet.status = 'Inactiva'
      pet.active = false
    }
  }

  function removePet(id) {
    pets.value = pets.value.filter(p => p.id !== id)
  }

  return { pets, addPet, updatePet, changeStatus, deactivatePet, removePet }
})
