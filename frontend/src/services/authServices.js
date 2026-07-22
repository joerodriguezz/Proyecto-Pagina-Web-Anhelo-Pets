import { get, post, remove } from '../api/api'
import { setToken, clearToken } from '../utils/tokenStorage'

export const login = async (credentials) => {
    const { data } = await post('/api/auth/login', credentials)
    setToken(data.token)
    return data
}

export const register = async (user) => {
    const { data } = await post('/api/auth/register', user)
    setToken(data.token)
    return data
}

export const logout = () => {
    clearToken()
}

export const getCurrentUser = async () => {
    const { data } = await get('/api/auth/me')
    setToken(data.token)
    return data
}

export const resetPasswordByEmail = (email, newPassword) => {
    return post('/api/auth/password-reset', { email, newPassword })
}

export const uploadProfilePhoto = async (file) => {
    const formData = new FormData()
    formData.append('file', file)
    const { data } = await post('/api/auth/me/photo', formData)
    setToken(data.token)
    return data
}

export const deleteProfilePhoto = async () => {
    const { data } = await remove('/api/auth/me/photo')
    setToken(data.token)
    return data
}
