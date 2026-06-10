<script setup>
import { ref } from 'vue'
import { RouterLink, useRoute } from 'vue-router'

const route = useRoute()
const sidebarOpen = ref(true)

const navItems = [
  { to:'/admin',             label:'Dashboard' },
  { to:'/admin/mascotas',    label:'Mascotas' },
  { to:'/admin/adopciones',  label:'Adopciones' },
  { to:'/admin/rescates',    label:'Rescates' },
  { to:'/admin/salud',       label:'Salud' },
  { to:'/admin/usuarios',    label:'Usuarios' },
  { to:'/admin/donaciones',  label:'Donaciones' },
  { to:'/admin/voluntarios',  label:'Voluntarios' },



]

const isActive = (to) => {
  if (to === '/admin') return route.path === '/admin'
  return route.path.startsWith(to)
}
</script>

<template>
  <div class="admin-shell" :class="{ collapsed: !sidebarOpen }">
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
          <span class="nav-label">{{ item.label }}</span>
        </RouterLink>
      </nav>

      <div class="sidebar-footer">
        <RouterLink to="/" class="nav-item-footer">
          <span v-if="sidebarOpen">Ver sitio web</span>
          <span v-else class="footer-dot">·</span>
        </RouterLink>
        <RouterLink to="/login" class="nav-item-footer logout">
          <span v-if="sidebarOpen">Cerrar sesión</span>
          <span v-else class="footer-dot">×</span>
        </RouterLink>
      </div>
    </aside>

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
/* ── Estructura General ── */
.admin-shell {
  display: flex;
  min-height: 100vh;
  background: #FAFAFA;
  font-family: 'Inter', sans-serif;
}

/* ── SIDEBAR (Izquierdo) ── */
.sidebar {
  width: 260px;
  background: #3A473C; /* Verde oscuro del tema principal */
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

/* Botón colapsable minimalista */
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

/* Navegación Interna */
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
  padding: 12px 16px;
  border-radius: 12px;
  color: rgba(255, 255, 255, 0.65);
  font-size: 14px;
  font-weight: 600;
  text-decoration: none;
  transition: all 0.25s ease;
  white-space: nowrap;
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
  background: #92A894; /* Color verde claro identitario */
  color: white;
  font-weight: 700;
}

.admin-shell.collapsed .nav-item.active {
  background: transparent;
  color: #F9C17A;
  font-weight: 800;
}

/* Footer del Sidebar */
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

.footer-dot {
  font-size: 18px;
  font-weight: 800;
}

/* ── MAIN CONTENT (Derecho) ── */
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

/* Ajustes finos para pantallas móviles */
@media (max-width: 768px) {
  .admin-topbar { padding: 0 20px; }
  .admin-content { padding: 20px; }
}
</style>