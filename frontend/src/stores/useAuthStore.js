import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { login as apiLogin, logout as apiLogout, getCurrentUser } from '../services/authServices'
import { isLoggedIn } from '../utils/tokenStorage'

export const useAuthStore = defineStore('auth', () => {
  const user = ref(null)
  const isReady = ref(false)

  async function init() {
    if (!isLoggedIn()) {
      user.value = null
      isReady.value = true
      return
    }

    try {
      user.value = await getCurrentUser()
    } catch {
      user.value = null
      apiLogout()
    } finally {
      isReady.value = true
    }
  }

  async function login(credentials) {
    const data = await apiLogin(credentials)
    user.value = data
    return data
  }

  function logout() {
    apiLogout()
    user.value = null
  }

  const isAdmin = computed(() => user.value?.roles?.includes('Admin') ?? false)
  const isVolunteer = computed(() => user.value?.roles?.includes('Voluntario') ?? false)

  return { user, isReady, init, login, logout, isAdmin, isVolunteer }
})
