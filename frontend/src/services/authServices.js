import { get, post } from '../api/api'
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
    return data
}

export const resetPasswordByEmail = (email, newPassword) => {
    return post('/api/auth/password-reset', { email, newPassword })
}
