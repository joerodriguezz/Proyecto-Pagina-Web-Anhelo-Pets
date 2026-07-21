import { get, post, remove } from '../api/api'

export const getHealthRecords = () => {
    return get("/api/MedicalRecords")
}

export const getHealthRecordsByAnimal = (animalId) => {
    return get(`/api/MedicalRecords/animal/${animalId}`)
}

export const getHealthRecord = (id) => {
    return get(`/api/MedicalRecords/${id}`)
}

// veterinarianId es un text tipo 'VET-000001' y es NOT NULL en la tabla
export const createHealthRecord = (record) => {
    return post("/api/MedicalRecords", {
        animalId: record.animalId,
        veterinarianId: record.veterinarianId,
        diagnosis: record.diagnosis || '',
        treatment: record.treatment || '',
        notes: record.notes || null,
        visitDate: record.visitDate || null,
        createdBy: record.createdBy || null,
    })
}

export const deleteHealthRecord = (id) => {
    return remove(`/api/MedicalRecords/${id}`)
}
