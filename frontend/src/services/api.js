const DEFAULT_API_BASE_URL = 'http://localhost:5272'
const configuredApiBaseUrl = import.meta.env.VITE_API_BASE_URL

export const API_BASE_URL = (
  configuredApiBaseUrl === undefined
    ? DEFAULT_API_BASE_URL
    : configuredApiBaseUrl
).replace(/\/$/, '')

async function request(path, options = {}) {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    headers: {
      'Content-Type': 'application/json',
      ...(options.headers || {}),
    },
    ...options,
  })

  if (response.status === 204) {
    return null
  }

  const contentType = response.headers.get('content-type') || ''
  const body = contentType.includes('application/json')
    ? await response.json()
    : await response.text()

  if (!response.ok) {
    const message = typeof body === 'string'
      ? body
      : body?.message || body?.title || 'No se pudo completar la solicitud.'

    throw new Error(message)
  }

  return body
}

export const animalsApi = {
  getAll(params = {}) {
    const query = new URLSearchParams()

    Object.entries(params).forEach(([key, value]) => {
      if (key === 'status' && value === 'Todos') {
        query.set(key, value)
        return
      }

      if (value !== undefined && value !== null && value !== '' && value !== 'Todos') {
        query.set(key, value)
      }
    })

    const suffix = query.toString() ? `?${query}` : ''
    return request(`/api/animals${suffix}`)
  },

  getById(id) {
    return request(`/api/animals/${id}`)
  },

  create(payload) {
    return request('/api/animals', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  update(id, payload) {
    return request(`/api/animals/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
  },
}

export const authApi = {
  login(payload) {
    return request('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  register(payload) {
    return request('/api/auth/register', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },
}

export const volunteersApi = {
  getAll() {
    return request('/api/volunteers')
  },

  create(payload) {
    return request('/api/volunteers', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  update(id, payload) {
    return request(`/api/volunteers/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
  },

  deactivate(id) {
    return request(`/api/volunteers/${id}`, {
      method: 'DELETE',
    })
  },
}

export const rescuesApi = {
  getAll() {
    return request('/api/rescates')
  },

  create(payload) {
    return request('/api/rescates', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  update(id, payload) {
    return request(`/api/rescates/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
  },

  close(id) {
    return request(`/api/rescates/${id}`, {
      method: 'DELETE',
    })
  },
}

export const fosterHomesApi = {
  getAll() {
    return request('/api/foster-homes')
  },

  create(payload) {
    return request('/api/foster-homes', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  update(id, payload) {
    return request(`/api/foster-homes/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
  },

  deactivate(id) {
    return request(`/api/foster-homes/${id}`, {
      method: 'DELETE',
    })
  },
}

export const fosterPlacementsApi = {
  getAll() {
    return request('/api/foster-placements')
  },

  create(payload) {
    return request('/api/foster-placements', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  update(id, payload) {
    return request(`/api/foster-placements/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
  },

  close(id) {
    return request(`/api/foster-placements/${id}`, {
      method: 'DELETE',
    })
  },
}
