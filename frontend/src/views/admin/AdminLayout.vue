<script setup>
import { ref } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import {
  LayoutDashboard,
  PawPrint,
  House,
  Ambulance,
  Stethoscope,
  Users,
  HeartHandshake,
  UserPlus,
  ShieldCheck, 
  Globe,
  LogOut,
  PanelLeftClose,
  PanelLeftOpen,
  Menu,
  X
} from 'lucide-vue-next'

const route = useRoute()
const sidebarOpen = ref(true)
const mobileMenuOpen = ref(false)

const navItems = [
  { to: '/admin', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/admin/mascotas', label: 'Mascotas', icon: PawPrint },
  { to: '/admin/adopciones', label: 'Adopciones', icon: House },
  { to: '/admin/rescates', label: 'Rescates', icon: Ambulance },
  { to: '/admin/salud', label: 'Salud', icon: Stethoscope },
  { to: '/admin/usuarios', label: 'Usuarios', icon: Users },
  { to: '/admin/donaciones', label: 'Donaciones', icon: HeartHandshake },
  { to: '/admin/voluntarios', label: 'Voluntarios', icon: UserPlus },
  { to: '/admin/auditoria', label: 'Auditoría', icon: ShieldCheck }
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
        <button
          class="toggle-btn"
          @click="sidebarOpen = !sidebarOpen"
          :aria-label="sidebarOpen ? 'Contraer menú' : 'Expandir menú'"
        >
          <PanelLeftClose v-if="sidebarOpen" :size="16" :stroke-width="2" />
          <PanelLeftOpen v-else :size="16" :stroke-width="2" />
        </button>
      </div>

      <div v-if="sidebarOpen" class="sidebar-section-label">Menú principal</div>

      <nav class="sidebar-nav">
        <RouterLink
          v-for="item in navItems"
          :key="item.to"
          :to="item.to"
          class="nav-item"
          :class="{ active: isActive(item.to) }"
        >
          <component :is="item.icon" class="nav-icon" :size="19" :stroke-width="1.9" />
          <span v-if="sidebarOpen" class="nav-label">{{ item.label }}</span>
        </RouterLink>
      </nav>

      <div class="sidebar-footer">
        <RouterLink to="/" class="nav-item-footer">
          <Globe class="footer-icon" :size="17" :stroke-width="1.9" />
          <span v-if="sidebarOpen">Ver sitio web</span>
        </RouterLink>
        <RouterLink to="/login" class="nav-item-footer logout">
          <LogOut class="footer-icon" :size="17" :stroke-width="1.9" />
          <span v-if="sidebarOpen">Cerrar sesión</span>
        </RouterLink>
      </div>
    </aside>

    <!-- ── MOBILE TOPBAR ── -->
    <div class="mobile-topbar">
      <div class="mobile-topbar-left">
        <button class="hamburger-btn" @click="mobileMenuOpen = true" aria-label="Abrir menú">
          <Menu :size="20" :stroke-width="2" />
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
            <X :size="18" :stroke-width="2" />
          </button>
        </div>

        <div class="sidebar-section-label mobile">Menú principal</div>

        <nav class="mobile-drawer-nav">
          <RouterLink
            v-for="item in navItems"
            :key="item.to"
            :to="item.to"
            class="mobile-nav-item"
            :class="{ active: isActive(item.to) }"
            @click="handleMobileNavClick"
          >
            <component :is="item.icon" class="nav-icon" :size="20" :stroke-width="1.9" />
            <span class="nav-label">{{ item.label }}</span>
          </RouterLink>
        </nav>

        <div class="mobile-drawer-footer">
          <RouterLink to="/" class="mobile-nav-item-footer" @click="closeMobileMenu">
            <Globe class="footer-icon" :size="17" :stroke-width="1.9" />
            <span>Ver sitio web</span>
          </RouterLink>
          <RouterLink to="/login" class="mobile-nav-item-footer logout" @click="closeMobileMenu">
            <LogOut class="footer-icon" :size="17" :stroke-width="1.9" />
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
  width: 272px;
  background: #2E3A30;
  display: flex;
  flex-direction: column;
  flex-shrink: 0;
  transition: width 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  position: sticky;
  top: 0;
  height: 100vh;
  z-index: 101;
  padding: 28px 18px;
  box-sizing: border-box;
}

.admin-shell.collapsed .sidebar {
  width: 84px;
  padding: 28px 14px;
}

.sidebar-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 8px 28px 8px;
  margin-bottom: 20px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.06);
}

.admin-shell.collapsed .sidebar-header {
  justify-content: center;
  padding: 0 0 28px 0;
}

.sidebar-logo {
  text-decoration: none;
}

.logo-text {
  font-size: 19px;
  font-weight: 500;
  color: white;
  letter-spacing: -0.3px;
}

.logo-text-short {
  font-size: 20px;
  font-weight: 500;
  color: #F4B565;
}

.peach {
  color: #F4B565;
}

.toggle-btn {
  background: rgba(255, 255, 255, 0.05);
  border: none;
  color: rgba(255, 255, 255, 0.55);
  width: 30px;
  height: 30px;
  border-radius: 9px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background 0.2s ease, color 0.2s ease;
  flex-shrink: 0;
}

.admin-shell.collapsed .toggle-btn {
  display: none;
}

.toggle-btn:hover {
  background: rgba(255, 255, 255, 0.1);
  color: white;
}

.sidebar-section-label {
  font-size: 11px;
  font-weight: 500;
  color: rgba(255, 255, 255, 0.32);
  text-transform: uppercase;
  letter-spacing: 0.6px;
  padding: 0 12px 10px;
}

.admin-shell.collapsed .sidebar-section-label {
  display: none;
}

.sidebar-nav {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 11px 14px;
  border-radius: 11px;
  color: rgba(255, 255, 255, 0.62);
  font-size: 14px;
  font-weight: 400;
  text-decoration: none;
  transition: background 0.2s ease, color 0.2s ease;
  white-space: nowrap;
}

.nav-icon {
  flex-shrink: 0;
  color: inherit;
}

.admin-shell.collapsed .nav-item {
  justify-content: center;
  padding: 11px 0;
}

.nav-item:hover {
  background: rgba(255, 255, 255, 0.05);
  color: white;
}

.nav-item.active {
  background: rgba(244, 181, 101, 0.14);
  color: #F4B565;
  font-weight: 500;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.08);
}

.sidebar-footer {
  border-top: 1px solid rgba(255, 255, 255, 0.07);
  padding-top: 14px;
  margin-top: 14px;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.nav-item-footer {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 9px 14px;
  border-radius: 10px;
  color: rgba(255, 255, 255, 0.4);
  font-size: 13px;
  font-weight: 400;
  text-decoration: none;
  transition: background 0.2s ease, color 0.2s ease;
}

.admin-shell.collapsed .nav-item-footer {
  justify-content: center;
  padding: 9px 0;
}

.nav-item-footer:hover {
  color: white;
  background: rgba(255, 255, 255, 0.04);
}

.logout:hover {
  color: #E28080;
  background: rgba(226, 128, 128, 0.08);
}

.footer-icon {
  flex-shrink: 0;
  color: inherit;
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

  .sidebar {
    display: none;
  }

  .admin-shell {
    flex-direction: column;
  }

  /* ── Mobile Topbar ── */
  .mobile-topbar {
    display: flex;
    align-items: center;
    justify-content: space-between;
    background: #2E3A30;
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

  .hamburger-btn {
    background: rgba(255, 255, 255, 0.08);
    border: none;
    color: white;
    width: 38px;
    height: 38px;
    border-radius: 10px;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 0;
    transition: background 0.2s ease;
    flex-shrink: 0;
  }

  .hamburger-btn:hover {
    background: rgba(255, 255, 255, 0.15);
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
    width: 288px;
    background: #2E3A30;
    z-index: 400;
    overflow-y: auto;
    padding: 24px 16px;
    box-sizing: border-box;
  }

  .mobile-drawer-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 0 6px 22px 6px;
    margin-bottom: 16px;
    border-bottom: 1px solid rgba(255, 255, 255, 0.07);
    flex-shrink: 0;
  }

  .close-btn {
    background: rgba(255, 255, 255, 0.08);
    border: none;
    color: rgba(255, 255, 255, 0.65);
    width: 32px;
    height: 32px;
    border-radius: 9px;
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

  .sidebar-section-label.mobile {
    padding: 0 10px 10px;
  }

  /* Nav items del drawer */
  .mobile-drawer-nav {
    flex: 1;
    display: flex;
    flex-direction: column;
    gap: 3px;
  }

  .mobile-nav-item {
    display: flex;
    align-items: center;
    gap: 13px;
    padding: 12px 14px;
    border-radius: 11px;
    color: rgba(255, 255, 255, 0.62);
    font-size: 14.5px;
    font-weight: 400;
    text-decoration: none;
    transition: background 0.2s ease, color 0.2s ease;
  }

  .mobile-nav-item:hover {
    background: rgba(255, 255, 255, 0.05);
    color: white;
  }

  .mobile-nav-item.active {
    background: rgba(244, 181, 101, 0.14);
    color: #F4B565;
    font-weight: 500;
  }

  /* Footer del drawer */
  .mobile-drawer-footer {
    border-top: 1px solid rgba(255, 255, 255, 0.07);
    padding-top: 14px;
    margin-top: 14px;
    display: flex;
    flex-direction: column;
    gap: 2px;
    flex-shrink: 0;
  }

  .mobile-nav-item-footer {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 10px 14px;
    border-radius: 10px;
    color: rgba(255, 255, 255, 0.4);
    font-size: 13px;
    font-weight: 400;
    text-decoration: none;
    transition: background 0.2s ease, color 0.2s ease;
  }

  .mobile-nav-item-footer:hover {
    color: white;
    background: rgba(255, 255, 255, 0.04);
  }

  .mobile-nav-item-footer.logout:hover {
    color: #E28080;
    background: rgba(226, 128, 128, 0.08);
  }

  /* Ajustes de la topbar del contenido principal en móvil */
  .admin-topbar {
    height: 56px;
    padding: 0 16px;
    top: 60px;
  }

  .page-title {
    font-size: 15px;
  }

  .admin-badge {
    display: none;
  }

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