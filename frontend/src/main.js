import { createApp } from 'vue'
import App from './App.vue'
import router from './router'

import { createPinia } from 'pinia'

import './style.css'
import 'boxicons/css/boxicons.min.css'

import { useAuthStore } from './stores/useAuthStore'

const app = createApp(App)

const pinia = createPinia()

app.use(pinia)
app.use(router)

useAuthStore().init()

app.mount('#app')