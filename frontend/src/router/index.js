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

  /* PROTEGER ADMIN */

  if (

    to.path.startsWith('/admin')

  ) {

    if (

      !usuario ||

      usuario.rol !== 'Admin'

    ) {

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