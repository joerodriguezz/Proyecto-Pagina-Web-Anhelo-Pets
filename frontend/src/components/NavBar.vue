<script setup>
import { ref, onMounted } from 'vue'
import {
  RouterLink,
  useRoute,
  useRouter
} from 'vue-router'

import HamburgerMenu from './Hamburgermenu.vue'

const route = useRoute()
const router = useRouter()

const menuOpen = ref(false)

const usuarioActual = ref(null)

const navLinks = [
  { name: 'Inicio', to: '/' },
  { name: 'Mascotas', to: '/mascotas' },
  { name: 'Rescates', to: '/rescates' },
  { name: 'Voluntario', to: '/voluntarios' },
  { name: 'Donar', to: '/donar' },
  { name: 'Nosotros', to: '/nosotros' },
]

/* ─────────────────────────────
   CARGAR SESION
───────────────────────────── */

onMounted(() => {
  const usuarioGuardado = localStorage.getItem('anhelo_usuario_actual')
  if (usuarioGuardado) {
    usuarioActual.value = JSON.parse(usuarioGuardado)
  }
})

/* ─────────────────────────────
   CERRAR SESION
───────────────────────────── */

function cerrarSesion() {
  localStorage.removeItem('anhelo_usuario_actual')
  usuarioActual.value = null
  router.push('/')
  location.reload()
}
</script>

<template>

  <header class="navbar">

    <div class="nav-inner">

      <!-- LOGO -->
      <RouterLink to="/" class="logo">
        <span class="logo-text">
          Anhelo
          <span class="logo-green">Pets</span>
        </span>
      </RouterLink>

      <!-- LINKS -->
      <nav class="nav-links">
        <RouterLink
          v-for="link in navLinks"
          :key="link.to"
          :to="link.to"
          class="nav-link"
          :class="{ active: route.path === link.to }"
        >
          {{ link.name }}
        </RouterLink>
      </nav>

      <!-- AUTH (solo visible en escritorio) -->
      <div class="nav-auth">
        <template v-if="!usuarioActual">
          <RouterLink to="/login" class="btn-login">
            Iniciar sesión
          </RouterLink>
          <RouterLink to="/registro" class="btn-register">
            Registrarse
          </RouterLink>
        </template>
      </div>

      <!-- USER SLOT: siempre visible en todos los breakpoints -->
      <div class="nav-user-slot">

        <!-- CON SESION: HamburgerMenu -->
        <template v-if="usuarioActual">
          <HamburgerMenu />
        </template>

        <!-- SIN SESION: botón hamburguesa nativo para el menú mobile -->
        <button
          v-else
          class="hamburger"
          @click="menuOpen = !menuOpen"
          aria-label="Menu"
        >
          <span></span>
          <span></span>
          <span></span>
        </button>

      </div>

    </div>

    <!-- MOBILE MENU (solo para usuarios sin sesión) -->
    <div class="mobile-menu" :class="{ open: menuOpen }">

      <RouterLink
        v-for="link in navLinks"
        :key="link.to"
        :to="link.to"
        class="mobile-link"
        @click="menuOpen = false"
      >
        {{ link.name }}
      </RouterLink>

      <!-- MOBILE AUTH -->
      <div class="mobile-auth">
        <template v-if="!usuarioActual">
          <RouterLink
            to="/login"
            class="btn-login"
            @click="menuOpen = false"
          >
            Iniciar sesión
          </RouterLink>
          <RouterLink
            to="/registro"
            class="btn-register"
            @click="menuOpen = false"
          >
            Registrarse
          </RouterLink>
        </template>
      </div>

    </div>

  </header>

</template>

<style scoped>

.navbar {
  background: rgba(255, 255, 255, 0.88);
  backdrop-filter: blur(16px);
  border-bottom: 1px solid rgba(0, 0, 0, 0.04);
  position: sticky;
  top: 0;
  z-index: 1000;
  box-shadow: 0 4px 24px rgba(42, 92, 66, 0.05);
}

.nav-inner {
  max-width: 1240px;
  margin: 0 auto;
  padding: 0 30px;
  height: 76px;
  display: flex;
  align-items: center;
  gap: 30px;
}

/* LOGO */
.logo {
  display: flex;
  align-items: center;
  text-decoration: none;
  flex-shrink: 0;
}

.logo-text {
  font-size: 28px;
  font-weight: 800;
  letter-spacing: -1.5px;
  color: #3A473C;
  line-height: 1;
}

.logo-green {
  color: #C9A06A;
}

/* LINKS */
.nav-links {
  display: flex;
  align-items: center;
  gap: 6px;
  flex: 1;
}

.nav-link {
  padding: 10px 16px;
  border-radius: 999px;
  font-size: 14px;
  font-weight: 600;
  color: #667085;
  text-decoration: none;
  transition: 0.3s ease;
  white-space: nowrap;
}

.nav-link:hover,
.nav-link.active {
  background: #E7EEE7;
  color: #596B5C;
}

/* AUTH (escritorio, sin sesión) */
.nav-auth {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-shrink: 0;
}

/* USER SLOT: siempre visible */
.nav-user-slot {
  display: flex;
  align-items: center;
  flex-shrink: 0;
}

.btn-login {
  padding: 10px 18px;
  border-radius: 999px;
  font-size: 14px;
  font-weight: 700;
  color: #6F8572;
  border: 1.5px solid #6F8572;
  background: white;
  text-decoration: none;
  transition: 0.3s ease;
  cursor: pointer;
}

.btn-login:hover {
  background: #E7EEE7;
}

.btn-register {
  padding: 10px 20px;
  border-radius: 999px;
  font-size: 14px;
  font-weight: 700;
  color: white;
  background: #6F8572;
  border: 1.5px solid #6F8572;
  text-decoration: none;
  transition: 0.3s ease;
  cursor: pointer;
}

.btn-register:hover {
  background: #596B5C;
  border-color: #596B5C;
}

/* HAMBURGER NATIVO (sin sesión en móvil) */
.hamburger {
  display: none;
  flex-direction: column;
  gap: 5px;
  background: none;
  border: none;
  cursor: pointer;
  padding: 4px;
}

.hamburger span {
  width: 24px;
  height: 2px;
  border-radius: 2px;
  background: #1F2937;
  display: block;
}

/* MOBILE MENU */
.mobile-menu {
  display: none;
  flex-direction: column;
  padding: 18px 24px 24px;
  background: white;
  border-top: 1px solid rgba(0, 0, 0, 0.05);
}

.mobile-menu.open {
  display: flex;
}

.mobile-link {
  padding: 14px 0;
  font-size: 15px;
  font-weight: 600;
  color: #667085;
  text-decoration: none;
  border-bottom: 1px solid rgba(0, 0, 0, 0.05);
}

.mobile-auth {
  display: flex;
  flex-direction: column;
  gap: 12px;
  margin-top: 18px;
}

/* RESPONSIVE */
@media (max-width: 980px) {

  /* Ocultar links de nav y botones de auth en móvil */
  .nav-links,
  .nav-auth {
    display: none;
  }

  /* El slot del usuario SIEMPRE visible (HamburgerMenu o hamburger nativo) */
  .nav-user-slot {
    display: flex;
    margin-left: auto;
  }

  /* Hamburger nativo visible solo sin sesión */
  .hamburger {
    display: flex;
  }

}

@media (max-width: 640px) {

  .nav-inner {
    height: 72px;
    padding: 0 20px;
  }

  .logo-text {
    font-size: 24px;
  }

}
</style>
