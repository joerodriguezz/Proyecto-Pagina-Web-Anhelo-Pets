import { get, post, put, remove } from '../api/api'

export const getVeterinarians = () => {
    return get("/api/veterinarians")
}

export const getVeterinarian = (id) => {
    return get(`/api/veterinarians/${id}`)
}

// Alta en cascada: el backend crea user + user_profile + volunteer + veterinarian
export const createVeterinarian = (vet) => {
    return post("/api/veterinarians", {
        firstName: vet.firstName || '',
        lastName: vet.lastName || '',
        specialty: vet.specialty || '',
        nationalId: vet.nationalId || null,
        email: vet.email || null,
        nationality: vet.nationality || null,
        createdBy: vet.createdBy || 'admin',
    })
}

export const updateVeterinarian = (id, vet) => {
    return put(`/api/veterinarians/${id}`, {
        specialty: vet.specialty || '',
        modifiedBy: vet.modifiedBy || 'admin',
    })
}

export const deactivateVeterinarian = (id) => {
    return remove(`/api/veterinarians/${id}`)
}
