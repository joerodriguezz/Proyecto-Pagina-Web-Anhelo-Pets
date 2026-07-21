import { get, patch, put } from '../api/api'

export const getUsers = () => {
    return get('/api/users')
}

export const updateUserStatus = (userId, active) => {
    return patch(`/api/users/${userId}/status`, {
        active,
        modifiedBy: 'admin',
    })
}

// Reemplaza el conjunto completo de roles del usuario
export const updateUserRoles = (userId, roleIds) => {
    return put(`/api/users/${userId}/roles`, {
        roleIds,
        modifiedBy: 'admin',
    })
}
