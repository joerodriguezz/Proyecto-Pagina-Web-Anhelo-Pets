<script setup>
import { ref } from 'vue'
import { RouterLink, useRoute, useRouter } from 'vue-router'

const route = useRoute()
const router = useRouter()
const sidebarOpen = ref(true)
const mobileMenuOpen = ref(false)

const navItems = [
  {
    to: '/admin',
    label: 'Dashboard',
    icon: '/img-voluntarios/dashboard.svg'
  },
  {
    to: '/admin/mascotas',
    label: 'Mascotas',
    icon: '/img-voluntarios/mascotas.svg'
  },
  {
    to: '/admin/adopciones',
    label: 'Adopciones',
    icon: '/img-voluntarios/adopciones.svg'
  },
  {
    to: '/admin/rescates',
    label: 'Rescates',
    icon: '/img-voluntarios/rescates.svg'
  },
  {
    to: '/admin/salud',
    label: 'Salud',
    icon: '/img-voluntarios/salud.svg'
  },
  {
    to: '/admin/usuarios',
    label: 'Usuarios',
    icon: '/img-voluntarios/usuarios.svg'
  },
  {
    to: '/admin/donaciones',
    label: 'Donaciones',
    icon: '/img-voluntarios/donacion.svg'
  },
  {
    to: '/admin/voluntarios',
    label: 'Voluntarios',
    icon: '/img-voluntarios/voluntario.svg'
  }
]

const isActive = (to) => {
  if (to === '/admin') return route.path === '/admin'
  return route.path.startsWith(to)
}

const closeMobileMenu = () => {
  mobileMenuOpen.value = false
}

const handleMobileNavClick = () => {
  closeMobileMenu()
}
</script>

<template>
  <div class="admin-shell" :class="{ collapsed: !sidebarOpen }">

    <!-- ── DESKTOP SIDEBAR ── -->
    <aside class="sidebar">
      <div class="sidebar-header">
        <RouterLink to="/" class="sidebar-logo">
          <span v-if="sidebarOpen" class="logo-text">Anhelo<span class="peach">Pets</span></span>
          <span v-else class="logo-text-short">A</span>
        </RouterLink>
        <button class="toggle-btn" @click="sidebarOpen = !sidebarOpen" :class="{ rotated: !sidebarOpen }">
          <span class="arrow-char">‹</span>
        </button>
      </div>

      <nav class="sidebar-nav">
        <RouterLink
          v-for="item in navItems"
          :key="item.to"
          :to="item.to"
          class="nav-item"
          :class="{ active: isActive(item.to) }"
        >
          <img :src="item.icon" :alt="item.label" class="nav-icon">
          <span v-if="sidebarOpen" class="nav-label">{{ item.label }}</span>
        </RouterLink>
      </nav>

      <div class="sidebar-footer">
        <RouterLink to="/" class="nav-item-footer">
          <img src="/img-voluntarios/web.svg" alt="Web" class="footer-icon">
          <span v-if="sidebarOpen">Ver sitio web</span>
        </RouterLink>
        <RouterLink to="/login" class="nav-item-footer logout">
          <img src="/img-voluntarios/cerrar.svg" alt="Cerrar sesión" class="footer-icon">
          <span v-if="sidebarOpen">Cerrar sesión</span>
        </RouterLink>
      </div>
    </aside>

    <!-- ── MOBILE TOPBAR ── -->
    <div class="mobile-topbar">
      <div class="mobile-topbar-left">
        <button class="hamburger-btn" @click="mobileMenuOpen = true" aria-label="Abrir menú">
          <span class="hamburger-line"></span>
          <span class="hamburger-line"></span>
          <span class="hamburger-line"></span>
        </button>
        <RouterLink to="/" class="mobile-logo">
          Anhelo<span class="peach">Pets</span>
        </RouterLink>
      </div>
      <div class="mobile-topbar-right">
        <div class="admin-avatar">A</div>
      </div>
    </div>

    <!-- ── MOBILE DRAWER OVERLAY ── -->
    <Transition name="overlay-fade">
      <div
        v-if="mobileMenuOpen"
        class="mobile-overlay"
        @click="closeMobileMenu"
        aria-hidden="true"
      ></div>
    </Transition>

    <!-- ── MOBILE DRAWER ── -->
    <Transition name="drawer-slide">
      <aside v-if="mobileMenuOpen" class="mobile-drawer" role="dialog" aria-label="Menú de navegación">
        <div class="mobile-drawer-header">
          <RouterLink to="/" class="sidebar-logo" @click="closeMobileMenu">
            <span class="logo-text">Anhelo<span class="peach">Pets</span></span>
          </RouterLink>
          <button class="close-btn" @click="closeMobileMenu" aria-label="Cerrar menú">
            <svg width="20" height="20" viewBox="0 0 20 20" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M15 5L5 15M5 5L15 15" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
            </svg>
          </button>
        </div>

        <nav class="mobile-drawer-nav">
          <RouterLink
            v-for="item in navItems"
            :key="item.to"
            :to="item.to"
            class="mobile-nav-item"
            :class="{ active: isActive(item.to) }"
            @click="handleMobileNavClick"
          >
            <img :src="item.icon" :alt="item.label" class="nav-icon">
            <span class="nav-label">{{ item.label }}</span>
          </RouterLink>
        </nav>

        <div class="mobile-drawer-footer">
          <RouterLink to="/" class="mobile-nav-item-footer" @click="closeMobileMenu">
            <img src="/img-voluntarios/web.svg" alt="Web" class="footer-icon">
            <span>Ver sitio web</span>
          </RouterLink>
          <RouterLink to="/login" class="mobile-nav-item-footer logout" @click="closeMobileMenu">
            <img src="/img-voluntarios/cerrar.svg" alt="Cerrar sesión" class="footer-icon">
            <span>Cerrar sesión</span>
          </RouterLink>
        </div>
      </aside>
    </Transition>

    <!-- ── MAIN CONTENT ── -->
    <div class="admin-main">
      <header class="admin-topbar">
        <div class="topbar-left">
          <h2 class="page-title">Panel de administración</h2>
        </div>
        <div class="topbar-right">
          <span class="admin-badge">Administrador</span>
          <div class="admin-avatar">A</div>
        </div>
      </header>

      <main class="admin-content">
        <RouterView />
      </main>
    </div>
  </div>
</template>

<style scoped>
/* ══════════════════════════════════════════
   ESTRUCTURA GENERAL
══════════════════════════════════════════ */
.admin-shell {
  display: flex;
  min-height: 100vh;
  background: #FAFAFA;
  font-family: 'Inter', sans-serif;
}

/* ══════════════════════════════════════════
   DESKTOP SIDEBAR
══════════════════════════════════════════ */
.sidebar {
  width: 260px;
  background: #3A473C;
  display: flex;
  flex-direction: column;
  flex-shrink: 0;
  transition: width 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  position: sticky;
  top: 0;
  height: 100vh;
  z-index: 101;
}

.admin-shell.collapsed .sidebar {
  width: 72px;
}

.sidebar-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 24px 20px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.05);
  min-height: 76px;
  box-sizing: border-box;
}

.sidebar-logo {
  text-decoration: none;
}

.logo-text {
  font-size: 20px;
  font-weight: 800;
  color: white;
  letter-spacing: -1px;
}

.logo-text-short {
  font-size: 22px;
  font-weight: 800;
  color: #F9C17A;
  padding-left: 6px;
}

.peach {
  color: #F9C17A;
}

.toggle-btn {
  background: rgba(255, 255, 255, 0.06);
  border: none;
  color: rgba(255, 255, 255, 0.7);
  width: 28px;
  height: 28px;
  border-radius: 8px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
}

.toggle-btn:hover {
  background: rgba(255, 255, 255, 0.12);
  color: white;
}

.arrow-char {
  font-size: 20px;
  line-height: 1;
  margin-top: -2px;
  transition: transform 0.3s;
}

.toggle-btn.rotated .arrow-char {
  transform: rotate(180deg);
}

.sidebar-nav {
  flex: 1;
  padding: 24px 14px;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.admin-shell.collapsed .sidebar-nav {
  padding: 24px 10px;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 16px;
  border-radius: 12px;
  color: rgba(255, 255, 255, 0.65);
  font-size: 14px;
  font-weight: 600;
  text-decoration: none;
  transition: all 0.25s ease;
  white-space: nowrap;
}

.nav-icon {
  width: 24px;
  height: 24px;
  object-fit: contain;
  flex-shrink: 0;
}

.admin-shell.collapsed .nav-item {
  justify-content: center;
  padding: 12px 0;
}

.nav-item:hover {
  background: rgba(255, 255, 255, 0.05);
  color: white;
}

.nav-item.active {
  background: #92A894;
  color: white;
  font-weight: 700;
}

.admin-shell.collapsed .nav-item.active {
  background: transparent;
  color: #F9C17A;
  font-weight: 800;
}

.sidebar-footer {
  padding: 14px;
  border-top: 1px solid rgba(255, 255, 255, 0.05);
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.nav-item-footer {
  display: flex;
  align-items: center;
  padding: 10px 16px;
  border-radius: 10px;
  color: rgba(255, 255, 255, 0.4);
  font-size: 13px;
  font-weight: 600;
  text-decoration: none;
  transition: all 0.2s ease;
  gap: 12px;
}

.admin-shell.collapsed .nav-item-footer {
  justify-content: center;
  padding: 8px 0;
}

.nav-item-footer:hover {
  color: white;
  background: rgba(255, 255, 255, 0.03);
}

.logout:hover {
  color: #EB7777;
  background: rgba(235, 119, 119, 0.08);
}

.footer-icon {
  width: 20px;
  height: 20px;
  flex-shrink: 0;
}

/* ══════════════════════════════════════════
   MOBILE TOPBAR (oculto en desktop)
══════════════════════════════════════════ */
.mobile-topbar {
  display: none;
}

/* ══════════════════════════════════════════
   MOBILE DRAWER
══════════════════════════════════════════ */
.mobile-overlay {
  display: none;
}

.mobile-drawer {
  display: none;
}

/* ══════════════════════════════════════════
   MAIN CONTENT
══════════════════════════════════════════ */
.admin-main {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.admin-topbar {
  background: white;
  border-bottom: 2px solid #F4F6F4;
  height: 76px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 40px;
  position: sticky;
  top: 0;
  z-index: 100;
}

.page-title {
  font-size: 18px;
  font-weight: 800;
  color: #3A473C;
  letter-spacing: -0.5px;
}

.topbar-right {
  display: flex;
  align-items: center;
  gap: 16px;
}

.admin-badge {
  background: #F4F6F4;
  color: #6C756D;
  padding: 6px 14px;
  border-radius: 99px;
  font-size: 13px;
  font-weight: 700;
}

.admin-avatar {
  width: 38px;
  height: 38px;
  background: #92A894;
  color: white;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 15px;
  font-weight: 800;
}

.admin-content {
  flex: 1;
  padding: 40px;
  overflow-y: auto;
  box-sizing: border-box;
}

/* ══════════════════════════════════════════
   MOBILE RESPONSIVE (≤768px)
   Desktop permanece intacto
══════════════════════════════════════════ */
@media (max-width: 768px) {

  /* Ocultar sidebar de escritorio */
  .sidebar {
    display: none;
  }

  /* Layout en columna para móvil */
  .admin-shell {
    flex-direction: column;
  }

  /* ── Mobile Topbar ── */
  .mobile-topbar {
    display: flex;
    align-items: center;
    justify-content: space-between;
    background: #3A473C;
    height: 60px;
    padding: 0 16px;
    position: sticky;
    top: 0;
    z-index: 200;
    flex-shrink: 0;
  }

  .mobile-topbar-left {
    display: flex;
    align-items: center;
    gap: 14px;
  }

  .mobile-topbar-right {
    display: flex;
    align-items: center;
  }

  .mobile-logo {
    font-size: 18px;
    font-weight: 800;
    color: white;
    text-decoration: none;
    letter-spacing: -0.5px;
  }

  /* Botón hamburguesa */
  .hamburger-btn {
    background: rgba(255, 255, 255, 0.08);
    border: none;
    width: 38px;
    height: 38px;
    border-radius: 10px;
    cursor: pointer;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 5px;
    padding: 0;
    transition: background 0.2s ease;
    flex-shrink: 0;
  }

  .hamburger-btn:hover {
    background: rgba(255, 255, 255, 0.15);
  }

  .hamburger-line {
    display: block;
    width: 18px;
    height: 2px;
    background: white;
    border-radius: 2px;
  }

  /* ── Overlay oscuro ── */
  .mobile-overlay {
    display: block;
    position: fixed;
    inset: 0;
    background: rgba(0, 0, 0, 0.5);
    z-index: 300;
    backdrop-filter: blur(2px);
  }

  /* ── Drawer lateral ── */
  .mobile-drawer {
    display: flex;
    flex-direction: column;
    position: fixed;
    top: 0;
    left: 0;
    bottom: 0;
    width: 280px;
    background: #3A473C;
    z-index: 400;
    overflow-y: auto;
  }

  .mobile-drawer-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 20px 20px;
    border-bottom: 1px solid rgba(255, 255, 255, 0.07);
    min-height: 68px;
    box-sizing: border-box;
    flex-shrink: 0;
  }

  .close-btn {
    background: rgba(255, 255, 255, 0.08);
    border: none;
    color: rgba(255, 255, 255, 0.7);
    width: 34px;
    height: 34px;
    border-radius: 8px;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: all 0.2s ease;
    flex-shrink: 0;
  }

  .close-btn:hover {
    background: rgba(255, 255, 255, 0.15);
    color: white;
  }

  /* Nav items del drawer */
  .mobile-drawer-nav {
    flex: 1;
    padding: 20px 14px;
    display: flex;
    flex-direction: column;
    gap: 4px;
  }

  .mobile-nav-item {
    display: flex;
    align-items: center;
    gap: 14px;
    padding: 13px 16px;
    border-radius: 12px;
    color: rgba(255, 255, 255, 0.65);
    font-size: 15px;
    font-weight: 600;
    text-decoration: none;
    transition: all 0.2s ease;
  }

  .mobile-nav-item:hover {
    background: rgba(255, 255, 255, 0.06);
    color: white;
  }

  .mobile-nav-item.active {
    background: #92A894;
    color: white;
    font-weight: 700;
  }

  /* Footer del drawer */
  .mobile-drawer-footer {
    padding: 14px;
    border-top: 1px solid rgba(255, 255, 255, 0.07);
    display: flex;
    flex-direction: column;
    gap: 4px;
    flex-shrink: 0;
  }

  .mobile-nav-item-footer {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 11px 16px;
    border-radius: 10px;
    color: rgba(255, 255, 255, 0.4);
    font-size: 13px;
    font-weight: 600;
    text-decoration: none;
    transition: all 0.2s ease;
  }

  .mobile-nav-item-footer:hover {
    color: white;
    background: rgba(255, 255, 255, 0.04);
  }

  .mobile-nav-item-footer.logout:hover {
    color: #EB7777;
    background: rgba(235, 119, 119, 0.08);
  }

  /* Ajustes de la topbar del contenido principal en móvil */
  .admin-topbar {
    height: 56px;
    padding: 0 16px;
    top: 60px; /* Debajo del mobile topbar */
  }

  .page-title {
    font-size: 15px;
  }

  .admin-badge {
    display: none;
  }

  /* El avatar del topbar principal se oculta en móvil (ya está en mobile-topbar) */
  .topbar-right .admin-avatar {
    display: none;
  }

  .admin-content {
    padding: 16px;
  }
}

/* ══════════════════════════════════════════
   TRANSICIONES DEL DRAWER
══════════════════════════════════════════ */
.drawer-slide-enter-active,
.drawer-slide-leave-active {
  transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.drawer-slide-enter-from,
.drawer-slide-leave-to {
  transform: translateX(-100%);
}

.overlay-fade-enter-active,
.overlay-fade-leave-active {
  transition: opacity 0.3s ease;
}

.overlay-fade-enter-from,
.overlay-fade-leave-to {
  opacity: 0;
}
</style>