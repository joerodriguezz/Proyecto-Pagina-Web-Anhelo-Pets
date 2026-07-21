import {
  createRouter,
  createWebHistory
} from 'vue-router'

import HomeView from '../views/HomeView.vue'

const routes = [

  {
    path: '/',
    name: 'home',
    component: HomeView
  },

  {
    path: '/mascotas',
    name: 'mascotas',
    component: () =>
      import('../views/MascotasView.vue')
  },

  {
    path: '/adoptar/:id',
    name: 'adoptar',
    component: () =>
      import('../views/AdoptarView.vue')
  },

  {
    path: '/rescates',
    name: 'rescates',
    component: () =>
      import('../views/RescatesView.vue')
  },

  {
    path: '/voluntarios',
    name: 'voluntarios',
    component: () =>
      import('../views/VoluntariosView.vue')
  },

  {
    path: '/nosotros',
    name: 'nosotros',
    component: () =>
      import('../views/NosotrosView.vue')
  },

  {
    path: '/login',
    name: 'login',
    component: () =>
      import('../views/LoginView.vue')
  },

  {
    path: '/registro',
    name: 'registro',
    component: () =>
      import('../views/RegistroView.vue')
  },

  {
    path: '/donar',
    name: 'donar',
    component: () =>
      import('../views/DonarView.vue')
  },

  /* ADMIN */

  {
    path: '/admin',
    component: () =>
      import('../views/admin/AdminLayout.vue'),

    children: [

      {
        path: '',
        name: 'admin-dashboard',
        component: () =>
          import('../views/admin/DashboardView.vue')
      },

      {
        path: 'mascotas',
        name: 'admin-mascotas',
        component: () =>
          import('../views/admin/MascotasAdminView.vue')
      },

      {
        path: 'adopciones',
        name: 'admin-adopciones',
        component: () =>
          import('../views/admin/AdopcionesAdminView.vue')
      },

      {
        path: 'rescates',
        name: 'admin-rescates',
        component: () =>
          import('../views/admin/RescatesAdminView.vue')
      },

      {
        path: 'salud',
        name: 'admin-salud',
        component: () =>
          import('../views/admin/SaludAdminView.vue')
      },

      {
        path: 'usuarios',
        name: 'admin-usuarios',
        component: () =>
          import('../views/admin/UsuariosAdminView.vue')
      },

      {
        path: 'donaciones',
        name: 'admin-donaciones',
        component: () =>
          import('../views/admin/DonacionesAdminView.vue')
      },

      

{
  path: 'voluntarios',
  name: 'admin-voluntarios',
  component: () =>
    import('../views/admin/VoluntariosAdminView.vue')
}

]

    
  }

]

const router = createRouter({

  history: createWebHistory(),

  routes,

  scrollBehavior() {

    return { top: 0 }

  }

})

const DYNAMIC_IMPORT_RELOAD_KEY =
  'anhelo_dynamic_import_reload'

function isDynamicImportError(error) {
  const message = String(
    error?.message || error || ''
  )

  return (
    message.includes(
      'Failed to fetch dynamically imported module'
    ) ||
    message.includes(
      'Importing a module script failed'
    ) ||
    message.includes(
      'error loading dynamically imported module'
    )
  )
}

router.onError((error, to) => {
  if (!isDynamicImportError(error)) return

  if (
    sessionStorage.getItem(
      DYNAMIC_IMPORT_RELOAD_KEY
    ) === '1'
  ) {
    sessionStorage.removeItem(
      DYNAMIC_IMPORT_RELOAD_KEY
    )
    return
  }

  sessionStorage.setItem(
    DYNAMIC_IMPORT_RELOAD_KEY,
    '1'
  )

  window.location.assign(
    to.fullPath || window.location.pathname
  )
})

router.afterEach(() => {
  sessionStorage.removeItem(
    DYNAMIC_IMPORT_RELOAD_KEY
  )
})

/* ─────────────────────────────
   PROTEGER RUTAS
───────────────────────────── */

router.beforeEach((to, from, next) => {

  const usuario = JSON.parse(

    localStorage.getItem(
      'anhelo_usuario_actual'
    )

  )

  /* RUTAS QUE NECESITAN LOGIN */

  const rutasProtegidas = [

    '/perfil',
    '/mis-adopciones'

  ]

  /* PROTEGER ADOPTAR */

  if (

    to.path.startsWith('/adoptar')

  ) {

    if (!usuario) {

      return next('/login')

    }

  }

  

  /* PROTEGER PERFIL */

  if (

    rutasProtegidas.includes(
      to.path
    )

  ) {

    if (!usuario) {

      return next('/login')

    }

  }

  next()

})

export default router

