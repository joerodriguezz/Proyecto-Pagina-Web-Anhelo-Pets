import { get, post, patch } from '../api/api'

export const submitAdoptionRequest = (request) => {
    return post('/api/adoption-requests', {
        animalId: request.animalId,
        applicantName: request.applicantName,
        nationalId: request.nationalId,
        email: request.email,
        phone: request.phone,
        age: Number(request.age),
        hasWhatsapp: request.hasWhatsapp,
        livesInCostaRica: request.livesInCostaRica,
        foreignCountry: request.foreignCountry || null,
        address: request.address,
        petNameSnapshot: request.petNameSnapshot,
        reasonForPet: request.reasonForPet || null,
        adoptionReasons: request.adoptionReasons,
        householdMembers: request.householdMembers,
        otherPets: request.otherPets || null,
        profession: request.profession,
        dailyRoutine: request.dailyRoutine,
        hoursAlone: request.hoursAlone,
    })
}

export const getAdoptionRequests = () => get('/api/adoption-requests')

// action: 'Proceso' | 'Aprobar' | 'Rechazar'
export const updateAdoptionRequestStatus = (id, action, validationNotes = null) => {
    return patch(`/api/adoption-requests/${id}/status`, {
        action,
        validationNotes,
        modifiedBy: 'admin',
    })
}

// Traduce el DTO del backend (camelCase) al shape en español que ya usa la vista admin
export function mapAdoptionRequestDtoToRow(dto) {
    return {
        id: dto.adoptionRequestId,
        petId: dto.animalId,
        usuarioId: dto.userId,
        solicitante: dto.applicantName,
        cedula: dto.nationalId,
        email: dto.email,
        telefono: dto.phone,
        edad: dto.age,
        whatsapp: dto.hasWhatsapp ? 'Sí' : 'No',
        viveEnCR: dto.livesInCostaRica ? 'Sí' : 'No',
        paisExtranjero: dto.foreignCountry || '',
        direccion: dto.address,
        mascota: dto.petNameSnapshot,
        porqueMascota: dto.reasonForPet || '',
        motivos: dto.adoptionReasons,
        hogar: dto.householdMembers,
        otrasMascotas: dto.otherPets || '',
        profesion: dto.profession,
        rutina: dto.dailyRoutine,
        horasSola: dto.hoursAlone,
        fecha: dto.createdAt ? dto.createdAt.split('T')[0] : '',
        estado: dto.validationStatus,
    }
}
