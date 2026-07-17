import {get, post} from '../api/api'

export const getAnimals = (params = {}) => {
    return get("/api/animals/GetAll", {
        params
    })
}

export const createAnimals = (animal) => {
    return get("api/animals", animal)
}