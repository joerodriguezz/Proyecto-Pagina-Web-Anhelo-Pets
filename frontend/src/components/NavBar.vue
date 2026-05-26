<script setup>
import { ref } from 'vue'
import { RouterLink, useRoute } from 'vue-router'

const route = useRoute()
const menuOpen = ref(false)

const navLinks = [
  { name: 'Inicio', to: '/' },
  { name: 'Mascotas', to: '/mascotas' },
  { name: 'Rescates', to: '/rescates' },
  { name: 'Ayuda / Voluntario', to: '/voluntarios' },
  { name: 'Nosotros', to: '/nosotros' },
]
</script>

<template>

  <header class="navbar">

    <div class="nav-inner">

      <!-- LOGO -->

      <RouterLink
        to="/"
        class="logo"
      >

        <span class="logo-text">

          Anhelo

          <span class="logo-green">
            Pets
          </span>

        </span>

      </RouterLink>

      <!-- DESKTOP LINKS -->

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

      <!-- AUTH -->

      <div class="nav-auth">

        <RouterLink
          to="/login"
          class="btn-login"
        >
          Iniciar sesión
        </RouterLink>

        <RouterLink
          to="/registro"
          class="btn-register"
        >
          Registrarse
        </RouterLink>

      </div>

      <!-- HAMBURGER -->

      <button
        class="hamburger"
        @click="menuOpen = !menuOpen"
        aria-label="Menu"
      >

        <span></span>
        <span></span>
        <span></span>

      </button>

    </div>

    <!-- MOBILE MENU -->

    <div
      class="mobile-menu"
      :class="{ open: menuOpen }"
    >

      <RouterLink
        v-for="link in navLinks"
        :key="link.to"
        :to="link.to"
        class="mobile-link"
        @click="menuOpen = false"
      >

        {{ link.name }}

      </RouterLink>

      <div class="mobile-auth">

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

      </div>

    </div>

  </header>

</template>

<style scoped>

.navbar {

  background: rgba(255,255,255,0.88);

  backdrop-filter: blur(16px);

  border-bottom:
    1px solid rgba(0,0,0,0.04);

  position: sticky;

  top: 0;

  z-index: 1000;

  box-shadow:
    0 4px 24px rgba(42,92,66,0.05);
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

  color: #1F2937;

  line-height: 1;
}

.logo-green {

  color: #6F8572;
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

/* AUTH */

.nav-auth {

  display: flex;

  align-items: center;

  gap: 12px;

  flex-shrink: 0;
}

.btn-login {

  padding: 10px 18px;

  border-radius: 999px;

  font-size: 14px;

  font-weight: 700;

  color: #6F8572;

  border:
    1.5px solid #6F8572;

  text-decoration: none;

  transition: 0.3s ease;
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

  border:
    1.5px solid #6F8572;

  text-decoration: none;

  transition: 0.3s ease;
}

.btn-register:hover {

  background: #596B5C;

  border-color: #596B5C;

  transform: translateY(-1px);
}

/* HAMBURGER */

.hamburger {

  display: none;

  flex-direction: column;

  gap: 5px;

  background: none;

  border: none;

  cursor: pointer;

  margin-left: auto;
}

.hamburger span {

  width: 24px;

  height: 2px;

  border-radius: 2px;

  background: #1F2937;
}

/* MOBILE */

.mobile-menu {

  display: none;

  flex-direction: column;

  padding:
    18px 24px
    24px;

  background: white;

  border-top:
    1px solid rgba(0,0,0,0.05);
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

  border-bottom:
    1px solid rgba(0,0,0,0.05);
}

.mobile-auth {

  display: flex;

  gap: 12px;

  margin-top: 18px;
}

/* RESPONSIVE */

@media (max-width: 980px) {

  .nav-links,
  .nav-auth {

    display: none;
  }

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