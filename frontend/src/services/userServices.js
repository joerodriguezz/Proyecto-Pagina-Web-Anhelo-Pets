import {get, post} from '../api/api';

export const register = (user) => {
    return post("/api/auth/register", user)
}

export const login = (user) => {
    return post("/api/auth/login", user)
}
