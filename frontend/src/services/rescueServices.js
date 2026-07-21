import {get, post, put, remove} from '../api/api'

export const getRescues = () => {
    return get("/api/Rescates")
}

export const getRescue = (id) => {
    return get(`/api/Rescates/${id}`)
}

export const createRescue = (rescue) => {
    return post("/api/Rescates", {
        animalId: rescue.animalId || null,
        fecha: rescue.fecha || null,
        ubicacion: rescue.ubicacion || '',
        descripcion: rescue.descripcion || '',
        status: rescue.status || 'Activo',
        fosterHomeId: rescue.fosterHomeId || null,
        volunteerId: rescue.volunteerId || null,
    })
}

export const updateRescue = (id, rescue) => {
    return put(`/api/Rescates/${id}`, {
        animalId: rescue.animalId || null,
        fecha: rescue.fecha || null,
        ubicacion: rescue.ubicacion || '',
        descripcion: rescue.descripcion || '',
        status: rescue.status || 'Activo',
        fosterHomeId: rescue.fosterHomeId || null,
        volunteerId: rescue.volunteerId || null,
    })
}

export const closeRescue = (id) => {
    return remove(`/api/Rescates/${id}`)
}
