import { get, post, put, patch } from '../api/api'

// Envío público del formulario. El correo debe pertenecer a una cuenta ya registrada.
export const submitVolunteerApplication = (application) => {
    return post('/api/volunteers', {
        email: application.email,
        nationalId: application.nationalId,
        volunteerType: application.volunteerType,
        motivation: application.motivation || null,
        applicationDetails: application.applicationDetails
            ? JSON.stringify(application.applicationDetails)
            : null,
        phonePrimary: application.phonePrimary,
        city: application.city || null,
        town: application.town || null,
        district: application.district || null,
        createdBy: 'public',
    })
}

// Consulta pública: ¿este correo ya tiene una solicitud? Responde null si no.
export const getMyVolunteerApplication = (email) => {
    return get(`/api/volunteers/by-email/${encodeURIComponent(email)}`)
}

// Listado para el panel admin
export const getVolunteers = () => {
    return get('/api/volunteers')
}

export const updateVolunteer = (volunteerId, data) => {
    return put(`/api/volunteers/${volunteerId}`, {
        nationalId: data.nationalId ?? null,
        volunteerType: data.volunteerType ?? null,
        motivation: data.motivation ?? null,
        applicationDetails: data.applicationDetails ? JSON.stringify(data.applicationDetails) : null,
        phonePrimary: data.phonePrimary ?? null,
        city: data.city ?? null,
        town: data.town ?? null,
        district: data.district ?? null,
        modifiedBy: 'admin',
    })
}

// action: 'Aprobar' | 'Rechazar' | 'Inactivar' | 'Reactivar'
export const updateVolunteerStatus = (volunteerId, action, validationNotes = null) => {
    return patch(`/api/volunteers/${volunteerId}/status`, {
        action,
        validationNotes,
        modifiedBy: 'admin',
    })
}

// El JSON de datosEspecificos viaja como string; helper para leerlo de vuelta
export function parseApplicationDetails(raw) {
    if (!raw) return {}
    try {
        return JSON.parse(raw)
    } catch {
        return {}
    }
}
