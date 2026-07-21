import {get, post, put, patch, remove} from '../api/api'

function mapPetToDto(pet) {
  let ageYears = 0, ageMonths = 0
  const ageStr = pet.age || ''
  const yearMatch = ageStr.match(/(\d+)\s*año/i)
  const monthMatch = ageStr.match(/(\d+)\s*mes/i)
  if (yearMatch) ageYears = parseInt(yearMatch[1])
  if (monthMatch) ageMonths = parseInt(monthMatch[1])

  return {
    animalName:   pet.name || '',
    species:      pet.type || '',
    breed:        pet.breed || '',
    ageYears,
    ageMonths,
    sex:          pet.sex === 'Hembra' ? 'H' : 'M',
    animalStatus: pet.status || 'Disponible',
    healthStatus: pet.healthBasic || '',
    description:  pet.description || ''
  }
}

function mapDtoToPet(dto) {
  const ageParts = []
  if (dto.ageYears) ageParts.push(`${dto.ageYears} año${dto.ageYears !== 1 ? 's' : ''}`)
  if (dto.ageMonths) ageParts.push(`${dto.ageMonths} mes${dto.ageMonths !== 1 ? 'es' : ''}`)

  return {
    id:          dto.animalId,
    name:        dto.animalName,
    type:        dto.species,
    breed:       dto.breed,
    age:         ageParts.join(' ') || 'Desconocida',
    sex:         dto.sex === 'H' ? 'Hembra' : dto.sex === 'M' ? 'Macho' : dto.sex || '',
    status:      dto.animalStatus,
    healthBasic: dto.healthStatus,
    description: dto.description,
    images:      dto.photoUrl ? [{ preview: dto.photoUrl }] : [],
    image:       dto.photoUrl || '',
  }
}

export const getAnimals = (params = {}) => {
    return get("/api/Animals", { params })
}

export const createAnimals = (animal) => {
    return post("/api/animals", mapPetToDto(animal))
}

export const updateAnimal = (id, animal) => {
    return put(`/api/Animals/${id}`, mapPetToDto(animal))
}

export const changeAnimalStatus = (id, status) => {
    return patch(`/api/Animals/${id}/status`, JSON.stringify(status), {
        headers: { "Content-Type": "application/json" }
    })
}

export { mapDtoToPet }