<script setup>
import { ref } from 'vue'
import { RouterLink, useRoute } from 'vue-router'

const route = useRoute()
const menuOpen = ref(false)

const navLinks = [
  { name: 'Inicio',            to: '/' },
  { name: 'Mascotas',          to: '/mascotas' },
  { name: 'Rescates',          to: '/rescates' },
  { name: 'Ayuda / Voluntario',to: '/voluntarios' },
  { name: 'Nosotros',          to: '/nosotros' },
]
</script>

<template>
  <header class="navbar">
    <div class="nav-inner">
      <!-- Logo -->
      <RouterLink to="/" class="logo">
        <span class="logo-icon">🐾</span>
        <span class="logo-text">Anhelo<strong>Pets</strong></span>
      </RouterLink>

      <!-- Desktop Links -->
      <nav class="nav-links">
        <RouterLink
          v-for="link in navLinks"
          :key="link.to"
          :to="link.to"
          class="nav-link"
          :class="{ active: route.path === link.to }"
        >{{ link.name }}</RouterLink>
      </nav>

      <!-- Auth Buttons -->
      <div class="nav-auth">
        <RouterLink to="/login" class="btn-login">Iniciar sesión</RouterLink>
        <RouterLink to="/registro" class="btn-register">Registrarse</RouterLink>
      </div>

      <!-- Hamburger -->
      <button class="hamburger" @click="menuOpen = !menuOpen" aria-label="Menu">
        <span></span><span></span><span></span>
      </button>
    </div>

    <!-- Mobile Menu -->
    <div class="mobile-menu" :class="{ open: menuOpen }">
      <RouterLink
        v-for="link in navLinks"
        :key="link.to"
        :to="link.to"
        class="mobile-link"
        @click="menuOpen = false"
      >{{ link.name }}</RouterLink>
      <div class="mobile-auth">
        <RouterLink to="/login"   class="btn-login"    @click="menuOpen = false">Iniciar sesión</RouterLink>
        <RouterLink to="/registro" class="btn-register" @click="menuOpen = false">Registrarse</RouterLink>
      </div>
    </div>
  </header>
</template>

<style scoped>
.navbar {
  background: var(--white);
  border-bottom: 1px solid var(--border);
  position: sticky;
  top: 0;
  z-index: 1000;
  box-shadow: 0 2px 12px rgba(42,92,66,0.07);
}

.nav-inner {
  max-width: 1180px;
  margin: 0 auto;
  padding: 0 24px;
  height: 68px;
  display: flex;
  align-items: center;
  gap: 32px;
}

/* Logo */
.logo {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 20px;
  color: var(--text-dark);
  flex-shrink: 0;
}
.logo-icon { font-size: 22px; }
.logo-text  { font-size: 18px; letter-spacing: -0.3px; }
.logo-text strong { color: var(--green); }

/* Nav Links */
.nav-links {
  display: flex;
  gap: 4px;
  flex: 1;
}
.nav-link {
  padding: 6px 14px;
  border-radius: var(--radius-full);
  font-size: 14px;
  font-weight: 500;
  color: var(--text-mid);
  transition: color 0.2s, background 0.2s;
  white-space: nowrap;
}
.nav-link:hover,
.nav-link.active {
  color: var(--green-dark);
  background: var(--green-pale);
}

/* Auth */
.nav-auth {
  display: flex;
  gap: 10px;
  flex-shrink: 0;
}
.btn-login {
  padding: 8px 18px;
  border-radius: var(--radius-full);
  font-size: 14px;
  font-weight: 600;
  color: var(--green);
  border: 1.5px solid var(--green);
  background: transparent;
  cursor: pointer;
  transition: background 0.2s, color 0.2s;
}
.btn-login:hover {
  background: var(--green-pale);
}
.btn-register {
  padding: 8px 18px;
  border-radius: var(--radius-full);
  font-size: 14px;
  font-weight: 600;
  color: var(--white);
  background: var(--green);
  border: 1.5px solid var(--green);
  cursor: pointer;
  transition: background 0.2s;
}
.btn-register:hover {
  background: var(--green-dark);
  border-color: var(--green-dark);
}

/* Hamburger */
.hamburger {
  display: none;
  flex-direction: column;
  gap: 5px;
  background: none;
  border: none;
  cursor: pointer;
  padding: 4px;
  margin-left: auto;
}
.hamburger span {
  display: block;
  width: 24px;
  height: 2px;
  background: var(--text-dark);
  border-radius: 2px;
  transition: 0.2s;
}

/* Mobile Menu */
.mobile-menu {
  display: none;
  flex-direction: column;
  padding: 16px 24px 20px;
  background: var(--white);
  border-top: 1px solid var(--border);
}
.mobile-menu.open { display: flex; }
.mobile-link {
  padding: 12px 0;
  font-size: 15px;
  font-weight: 500;
  color: var(--text-mid);
  border-bottom: 1px solid var(--border);
}
.mobile-link:last-of-type { border-bottom: none; }
.mobile-auth {
  display: flex;
  gap: 12px;
  margin-top: 16px;
}

@media (max-width: 900px) {
  .nav-links, .nav-auth { display: none; }
  .hamburger { display: flex; }
}
</style>
