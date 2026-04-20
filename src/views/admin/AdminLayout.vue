<script setup>
import { ref } from 'vue'
import { RouterLink, useRoute } from 'vue-router'

const route = useRoute()
const sidebarOpen = ref(true)

const navItems = [
  { to:'/admin',             icon:'📊', label:'Dashboard' },
  { to:'/admin/mascotas',    icon:'🐾', label:'Mascotas' },
  { to:'/admin/adopciones',  icon:'💛', label:'Adopciones' },
  { to:'/admin/rescates',    icon:'🚨', label:'Rescates' },
  { to:'/admin/salud',       icon:'🏥', label:'Salud' },
  { to:'/admin/usuarios',    icon:'👥', label:'Usuarios' },
  { to:'/admin/donaciones',  icon:'💰', label:'Donaciones' },
  { to:'/admin/voluntarios', icon:'🤝', label:'Voluntarios' },
]

const isActive = (to) => {
  if (to === '/admin') return route.path === '/admin'
  return route.path.startsWith(to)
}
</script>

<template>
  <div class="admin-shell" :class="{ collapsed: !sidebarOpen }">
    <!-- Sidebar -->
    <aside class="sidebar">
      <div class="sidebar-header">
        <RouterLink to="/" class="sidebar-logo">
          <span>🐾</span>
          <span v-if="sidebarOpen" class="logo-text">Anhelo<strong>Pets</strong></span>
        </RouterLink>
        <button class="toggle-btn" @click="sidebarOpen = !sidebarOpen">{{ sidebarOpen ? '◀' : '▶' }}</button>
      </div>

      <nav class="sidebar-nav">
        <RouterLink
          v-for="item in navItems"
          :key="item.to"
          :to="item.to"
          class="nav-item"
          :class="{ active: isActive(item.to) }"
        >
          <span class="nav-icon">{{ item.icon }}</span>
          <span v-if="sidebarOpen" class="nav-label">{{ item.label }}</span>
        </RouterLink>
      </nav>

      <div class="sidebar-footer" v-if="sidebarOpen">
        <RouterLink to="/" class="nav-item">
          <span class="nav-icon">🌐</span>
          <span class="nav-label">Ver sitio</span>
        </RouterLink>
        <RouterLink to="/login" class="nav-item logout">
          <span class="nav-icon">🚪</span>
          <span class="nav-label">Salir</span>
        </RouterLink>
      </div>
    </aside>

    <!-- Main Content -->
    <div class="admin-main">
      <!-- Top bar -->
      <header class="admin-topbar">
        <div class="topbar-left">
          <h2 class="page-title">Panel de Administración</h2>
        </div>
        <div class="topbar-right">
          <span class="admin-badge">🔧 Administrador</span>
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
.admin-shell {
  display: flex;
  min-height: 100vh;
  background: #F0F4F1;
}

/* ── SIDEBAR ── */
.sidebar {
  width: 240px;
  background: var(--green-dark);
  display: flex;
  flex-direction: column;
  flex-shrink: 0;
  transition: width 0.25s;
  position: sticky;
  top: 0;
  height: 100vh;
  overflow-y: auto;
}
.admin-shell.collapsed .sidebar { width: 68px; }

.sidebar-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 18px 14px;
  border-bottom: 1px solid rgba(255,255,255,0.1);
  min-height: 68px;
}
.sidebar-logo {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 20px;
  color: var(--white);
  text-decoration: none;
}
.logo-text { font-size: 17px; font-weight: 700; white-space: nowrap; }
.logo-text strong { color: var(--yellow); }
.toggle-btn {
  background: rgba(255,255,255,0.12);
  border: none;
  color: var(--white);
  width: 28px; height: 28px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 12px;
  flex-shrink: 0;
}

.sidebar-nav {
  flex: 1;
  padding: 12px 8px;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 10px 12px;
  border-radius: var(--radius-sm);
  color: rgba(255,255,255,0.75);
  font-size: 14px;
  font-weight: 500;
  text-decoration: none;
  transition: background 0.2s, color 0.2s;
  white-space: nowrap;
  overflow: hidden;
}
.nav-item:hover {
  background: rgba(255,255,255,0.1);
  color: var(--white);
}
.nav-item.active {
  background: rgba(255,255,255,0.18);
  color: var(--white);
  font-weight: 700;
  border-left: 3px solid var(--yellow);
}
.nav-icon { font-size: 18px; flex-shrink: 0; }
.logout { color: rgba(255,200,200,0.8) !important; }

.sidebar-footer {
  padding: 8px;
  border-top: 1px solid rgba(255,255,255,0.1);
}

/* ── MAIN ── */
.admin-main {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.admin-topbar {
  background: var(--white);
  border-bottom: 1px solid var(--border);
  height: 68px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 28px;
  position: sticky;
  top: 0;
  z-index: 100;
}
.page-title { font-size: 18px; font-weight: 700; color: var(--text-dark); }
.topbar-right { display: flex; align-items: center; gap: 14px; }
.admin-badge {
  background: var(--green-pale);
  color: var(--green-dark);
  padding: 5px 12px;
  border-radius: var(--radius-full);
  font-size: 13px;
  font-weight: 600;
}
.admin-avatar {
  width: 36px; height: 36px;
  background: var(--green);
  color: var(--white);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 15px;
  font-weight: 700;
}

.admin-content {
  flex: 1;
  padding: 28px;
  overflow-y: auto;
}
</style>
