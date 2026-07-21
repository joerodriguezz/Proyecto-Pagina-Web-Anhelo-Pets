import { get, post, put, remove } from '../api/api'

export const getRoles = () => {
    return get('/api/roles')
}

export const createRole = (role) => {
    return post('/api/roles', {
        roleName: role.roleName || '',
        roleAccess: role.roleAccess || '',
        description: role.description || null,
        createdBy: role.createdBy || 'admin',
    })
}

export const updateRole = (id, role) => {
    return put(`/api/roles/${id}`, {
        roleName: role.roleName || '',
        roleAccess: role.roleAccess || '',
        description: role.description || null,
        modifiedBy: role.modifiedBy || 'admin',
    })
}

export const deleteRole = (id) => {
    return remove(`/api/roles/${id}`)
}
