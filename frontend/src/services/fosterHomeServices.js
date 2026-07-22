import {get, post, remove} from '../api/api'

export const getFosterHomes = () => {
    return get("/api/foster-homes")
}

export const createFosterHome = (fh) => {
    return post("/api/foster-homes", {
        volunteerId: fh.volunteerId || null,
        name: fh.name || '',
        address: fh.address || '',
        phone: fh.phone || '',
        responsible: fh.responsible || '',
        capacity: fh.capacity || 1,
    })
}

export const deactivateFosterHome = (id) => {
    return remove(`/api/foster-homes/${id}`)
}

export const uploadFosterHomePhoto = (id, file) => {
    const formData = new FormData()
    formData.append('file', file)
    return post(`/api/foster-homes/${id}/photo`, formData)
}
