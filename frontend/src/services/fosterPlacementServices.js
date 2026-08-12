import { get, post, put } from '../api/api'

// Asignación vigente de un animal a una casa cuna (null si no tiene ninguna asignada)
export const getActivePlacementByAnimal = (animalId) => {
    return get(`/api/foster-placements/by-animal/${animalId}`)
}

export const createFosterPlacement = (data) => {
    return post('/api/foster-placements', {
        animalId: data.animalId,
        fosterHomeId: data.fosterHomeId,
        startDate: data.startDate,
        endDate: data.endDate || null,
        notes: data.notes || null,
        createdBy: data.createdBy || 'admin',
    })
}

// Cierra una asignación (se usa para "reasignar": termina la vieja antes de crear la nueva)
export const endFosterPlacement = (id, placement) => {
    return put(`/api/foster-placements/${id}`, {
        animalId: placement.animalId,
        fosterHomeId: placement.fosterHomeId,
        startDate: placement.startDate,
        endDate: placement.endDate,
        notes: placement.notes || null,
        modifiedBy: 'admin',
    })
}

export const getAllFosterPlacements = () => {
    return get('/api/foster-placements')
}
