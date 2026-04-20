import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'

const routes = [
  { path: '/', name: 'home', component: HomeView },
  { path: '/mascotas', name: 'mascotas', component: () => import('../views/MascotasView.vue') },
  { path: '/adoptar/:id', name: 'adoptar', component: () => import('../views/AdoptarView.vue') },
  { path: '/rescates', name: 'rescates', component: () => import('../views/RescatesView.vue') },
  { path: '/voluntarios', name: 'voluntarios', component: () => import('../views/VoluntariosView.vue') },
  { path: '/nosotros', name: 'nosotros', component: () => import('../views/NosotrosView.vue') },
  { path: '/login', name: 'login', component: () => import('../views/LoginView.vue') },
  { path: '/registro', name: 'registro', component: () => import('../views/RegistroView.vue') },
  {
    path: '/admin',
    component: () => import('../views/admin/AdminLayout.vue'),
    children: [
      { path: '', name: 'admin-dashboard', component: () => import('../views/admin/DashboardView.vue') },
      { path: 'mascotas', name: 'admin-mascotas', component: () => import('../views/admin/MascotasAdminView.vue') },
      { path: 'adopciones', name: 'admin-adopciones', component: () => import('../views/admin/AdopcionesAdminView.vue') },
      { path: 'rescates', name: 'admin-rescates', component: () => import('../views/admin/RescatesAdminView.vue') },
      { path: 'salud', name: 'admin-salud', component: () => import('../views/admin/SaludAdminView.vue') },
      { path: 'usuarios', name: 'admin-usuarios', component: () => import('../views/admin/UsuariosAdminView.vue') },
      { path: 'donaciones', name: 'admin-donaciones', component: () => import('../views/admin/DonacionesAdminView.vue') },
      { path: 'voluntarios', name: 'admin-voluntarios', component: () => import('../views/admin/VoluntariosAdminView.vue') },
    ]
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior() { return { top: 0 } }
})

export default router
