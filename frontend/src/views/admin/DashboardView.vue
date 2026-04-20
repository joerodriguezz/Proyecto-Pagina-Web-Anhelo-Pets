<template>
  <div class="dashboard">
    <h1 class="dash-title">Panel de control</h1>
    <p class="dash-sub">Resumen general del sistema · Fundación Anhelo Pets</p>

    <!-- KPI Cards -->
    <div class="kpi-grid">
      <div v-for="kpi in kpis" :key="kpi.label" class="kpi-card" :style="{ borderTop: `4px solid ${kpi.color}` }">
        <div class="kpi-icon">{{ kpi.icon }}</div>
        <div class="kpi-info">
          <div class="kpi-value">{{ kpi.value }}</div>
          <div class="kpi-label">{{ kpi.label }}</div>
        </div>
        <span class="kpi-trend" :class="kpi.up ? 'up' : 'down'">
          {{ kpi.up ? '↑' : '↓' }} {{ kpi.trend }}
        </span>
      </div>
    </div>

    <!-- Two column layout -->
    <div class="dash-grid">
      <!-- Recent Adoptions -->
      <div class="dash-card">
        <div class="dash-card-head">
          <h3>💛 Solicitudes recientes</h3>
          <RouterLink to="/admin/adopciones" class="view-all">Ver todas →</RouterLink>
        </div>
        <div class="table-wrapper">
          <table class="data-table">
            <thead>
              <tr><th>Solicitante</th><th>Mascota</th><th>Fecha</th><th>Estado</th></tr>
            </thead>
            <tbody>
              <tr v-for="s in recentAdoptions" :key="s.id">
                <td>{{ s.name }}</td>
                <td>{{ s.pet }}</td>
                <td style="font-size:12px;color:var(--text-light)">{{ s.date }}</td>
                <td><span class="badge" :class="s.statusClass">{{ s.status }}</span></td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Pet Status Breakdown -->
      <div class="dash-card">
        <div class="dash-card-head">
          <h3>🐾 Estado de mascotas</h3>
          <RouterLink to="/admin/mascotas" class="view-all">Gestionar →</RouterLink>
        </div>
        <div class="status-bars">
          <div v-for="s in petStatus" :key="s.label" class="status-bar-item">
            <div class="sb-header">
              <span>{{ s.icon }} {{ s.label }}</span>
              <span class="sb-count">{{ s.count }}</span>
            </div>
            <div class="sb-track">
              <div class="sb-fill" :style="{ width: s.pct + '%', background: s.color }"></div>
            </div>
          </div>
        </div>

        <div class="dash-card-head" style="margin-top:24px">
          <h3>📋 Rescates activos</h3>
          <RouterLink to="/admin/rescates" class="view-all">Ver →</RouterLink>
        </div>
        <div class="rescue-list">
          <div v-for="r in activeRescues" :key="r.id" class="rescue-item">
            <span class="rescue-icon">🚨</span>
            <div class="rescue-info">
              <span class="rescue-name">{{ r.pet }}</span>
              <span class="rescue-loc">{{ r.location }}</span>
            </div>
            <span class="badge badge-green">{{ r.status }}</span>
          </div>
        </div>
      </div>
    </div>

    <!-- Quick actions -->
    <div class="quick-actions">
      <h3>⚡ Acciones rápidas</h3>
      <div class="qa-grid">
        <RouterLink v-for="qa in quickActions" :key="qa.label" :to="qa.to" class="qa-btn">
          <span>{{ qa.icon }}</span>
          <span>{{ qa.label }}</span>
        </RouterLink>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  data() {
    return {
      kpis: [
        { icon:'🐾', label:'Mascotas registradas', value:42,  color:'#3A7A56', trend:'5 este mes',  up:true  },
        { icon:'💛', label:'Adopciones activas',   value:8,   color:'#F5C840', trend:'2 esta semana', up:true },
        { icon:'🚨', label:'Rescates activos',     value:12,  color:'#E8A838', trend:'3 nuevos',    up:true  },
        { icon:'👥', label:'Usuarios registrados', value:156, color:'#5B9BD5', trend:'12 nuevos',   up:true  },
        { icon:'💰', label:'Donaciones (mes)',      value:'₡124.000', color:'#6DB96D', trend:'20%', up:true },
        { icon:'🤝', label:'Voluntarios activos',  value:40,  color:'#9E6BC4', trend:'stables',     up:false },
      ],
      recentAdoptions: [
        { id:1, name:'Ana Rodríguez',  pet:'Luna',   date:'2026-04-09', status:'Pendiente',  statusClass:'badge-yellow' },
        { id:2, name:'Carlos Mora',    pet:'Rocky',  date:'2026-04-08', status:'Aprobada',   statusClass:'badge-green'  },
        { id:3, name:'Sofía Vega',     pet:'Mochi',  date:'2026-04-07', status:'Pendiente',  statusClass:'badge-yellow' },
        { id:4, name:'Diego Salas',    pet:'Bella',  date:'2026-04-05', status:'Rechazada',  statusClass:'badge-red'    },
        { id:5, name:'Laura Jiménez',  pet:'Nube',   date:'2026-04-03', status:'Aprobada',   statusClass:'badge-green'  },
      ],
      petStatus: [
        { icon:'✅', label:'Disponible',  count:26, pct:62, color:'#3A7A56' },
        { icon:'⏳', label:'En proceso',  count:8,  pct:19, color:'#F5C840' },
        { icon:'🏠', label:'Adoptada',    count:8,  pct:19, color:'#5B9BD5' },
      ],
      activeRescues: [
        { id:1, pet:'Luna',   location:'Desamparados, SJ', status:'Activo' },
        { id:2, pet:'Mochi',  location:'Cartago Centro',   status:'Activo' },
        { id:3, pet:'Rocky',  location:'Heredia, Barreal', status:'Activo' },
      ],
      quickActions: [
        { icon:'➕', label:'Nueva mascota',     to:'/admin/mascotas'   },
        { icon:'📋', label:'Nueva adopción',    to:'/admin/adopciones' },
        { icon:'🚨', label:'Registrar rescate', to:'/admin/rescates'   },
        { icon:'🏥', label:'Control de salud',  to:'/admin/salud'      },
        { icon:'👤', label:'Nuevo usuario',     to:'/admin/usuarios'   },
        { icon:'💰', label:'Ver donaciones',    to:'/admin/donaciones' },
      ]
    }
  }
}
</script>

<style scoped>
.dash-title { font-size: 26px; font-weight: 800; color: var(--text-dark); margin-bottom: 4px; }
.dash-sub   { font-size: 14px; color: var(--text-light); margin-bottom: 28px; }

.kpi-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 16px;
  margin-bottom: 28px;
}
.kpi-card {
  background: var(--white);
  border-radius: var(--radius-md);
  padding: 18px 20px;
  box-shadow: var(--shadow-sm);
  display: flex;
  align-items: center;
  gap: 14px;
}
.kpi-icon  { font-size: 30px; }
.kpi-info  { flex: 1; }
.kpi-value { font-size: 24px; font-weight: 800; color: var(--text-dark); line-height: 1.1; }
.kpi-label { font-size: 12px; color: var(--text-light); font-weight: 500; }
.kpi-trend { font-size: 12px; font-weight: 600; white-space: nowrap; }
.kpi-trend.up   { color: var(--green); }
.kpi-trend.down { color: var(--text-light); }

.dash-grid { display: grid; grid-template-columns: 1.5fr 1fr; gap: 20px; margin-bottom: 24px; }
.dash-card {
  background: var(--white);
  border-radius: var(--radius-md);
  padding: 22px;
  box-shadow: var(--shadow-sm);
}
.dash-card-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;
}
.dash-card-head h3 { font-size: 15px; font-weight: 700; color: var(--text-dark); }
.view-all { font-size: 13px; color: var(--green); font-weight: 600; }

.status-bars { display: flex; flex-direction: column; gap: 14px; }
.sb-header { display: flex; justify-content: space-between; font-size: 13px; font-weight: 500; color: var(--text-mid); margin-bottom: 6px; }
.sb-count  { font-weight: 700; color: var(--text-dark); }
.sb-track  { height: 8px; background: var(--cream-dark); border-radius: 4px; overflow: hidden; }
.sb-fill   { height: 100%; border-radius: 4px; transition: width 0.4s; }

.rescue-list { display: flex; flex-direction: column; gap: 10px; }
.rescue-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 12px;
  background: var(--cream);
  border-radius: var(--radius-sm);
}
.rescue-icon { font-size: 20px; }
.rescue-info { flex: 1; }
.rescue-name { display: block; font-size: 14px; font-weight: 600; }
.rescue-loc  { font-size: 12px; color: var(--text-light); }

.quick-actions { background: var(--white); border-radius: var(--radius-md); padding: 22px; box-shadow: var(--shadow-sm); }
.quick-actions h3 { font-size: 15px; font-weight: 700; color: var(--text-dark); margin-bottom: 16px; }
.qa-grid { display: flex; gap: 12px; flex-wrap: wrap; }
.qa-btn {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 6px;
  padding: 14px 18px;
  background: var(--cream);
  border-radius: var(--radius-md);
  font-size: 13px;
  font-weight: 600;
  color: var(--text-dark);
  text-decoration: none;
  transition: background 0.2s, transform 0.15s;
  min-width: 90px;
  text-align: center;
}
.qa-btn span:first-child { font-size: 24px; }
.qa-btn:hover { background: var(--green-pale); transform: translateY(-2px); }

@media (max-width: 900px) {
  .kpi-grid  { grid-template-columns: 1fr 1fr; }
  .dash-grid { grid-template-columns: 1fr; }
}
@media (max-width: 560px) {
  .kpi-grid { grid-template-columns: 1fr; }
}
</style>
