import { get, post, patch } from '../api/api'

// Envío público del formulario de donaciones (con comprobante en base64)
export const submitDonation = (donation) => {
    return post('/api/donations', {
        donorName: donation.donorName,
        email: donation.email,
        phone: donation.phone,
        method: donation.method,
        currency: donation.currency,
        amount: donation.amount,
        donatedAt: donation.donatedAt,
        message: donation.message || null,
        proofFile: donation.proofFile,
        createdBy: 'public',
    })
}

// Listado para el panel admin
export const getDonations = () => {
    return get('/api/donations')
}

// action: 'Aprobar' | 'Rechazar'
export const updateDonationStatus = (donationId, action, validationNotes = null) => {
    return patch(`/api/donations/${donationId}/status`, {
        action,
        validationNotes,
        modifiedBy: 'admin',
    })
}
