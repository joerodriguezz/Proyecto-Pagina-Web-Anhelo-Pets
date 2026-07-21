import { defineStore } from 'pinia'
import { ref } from 'vue'
import { getAnimals, updateAnimal, changeAnimalStatus, mapDtoToPet } from '../services/petServices.js'

export const usePetsStore = defineStore('pets', () => {
  const pets = ref([])
  const isLoading = ref(false)

  async function fetchPets(params = {}) {
    isLoading.value = true
    try {
      const response = await getAnimals(params)
      pets.value = (response.data || []).map(mapDtoToPet)
    } catch {
      pets.value = []
    } finally {
      isLoading.value = false
    }
  }

  function addPet(pet) {
    pets.value.push(pet)
  }

  async function updatePet(id, data) {
    try {
      await updateAnimal(id, data)
      await fetchPets({ status: "Todos" })
    } catch {
      throw new Error("Error al actualizar la mascota")
    }
  }

  async function changeStatus(id, status) {
    try {
      await changeAnimalStatus(id, status)
      await fetchPets({ status: 'Todos' })
    } catch {
      throw new Error('Error al cambiar el estado')
    }
  }

  async function deactivatePet(id) {
    await changeStatus(id, 'Inactiva')
  }

  function removePet(id) {
    pets.value = pets.value.filter(p => p.id !== id)
  }

  return { pets, isLoading, fetchPets, addPet, updatePet, changeStatus, deactivatePet, removePet }
})

