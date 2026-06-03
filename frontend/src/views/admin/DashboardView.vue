<template>
  <div class="dashboard">
    <header class="dash-header">
      <h1 class="dash-title">Panel de control</h1>
      <p class="dash-sub">Resumen general del sistema · Fundación Anhelo Pets</p>
    </header>

    <div class="kpi-grid">
      <div v-for="kpi in kpis" :key="kpi.label" class="kpi-card" :style="{ borderTop: `4px solid ${kpi.color}` }">
        <div class="kpi-info">
          <span class="kpi-label">{{ kpi.label }}</span>
          <div class="kpi-value">{{ kpi.value }}</div>
        </div>
        <div class="kpi-trend-wrapper">
          <span class="kpi-trend" :class="kpi.up ? 'up' : 'down'">
            <span class="trend-arrow">{{ kpi.up ? '↑' : '↓' }}</span> {{ kpi.trend }}
          </span>
        </div>
      </div>
    </div>

    <div class="dash-grid">
      <div class="dash-card">
        <div class="dash-card-head">
          <h3>Solicitudes recientes</h3>
          <RouterLink to="/admin/adopciones" class="view-all">Ver todas</RouterLink>
        </div>
        <div class="table-wrapper">
          <table class="data-table">
            <thead>
              <tr>
                <th>Solicitante</th>
                <th>Mascota</th>
                <th>Fecha</th>
                <th>Estado</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="s in recentAdoptions" :key="s.id">
                <td class="font-semibold">{{ s.name }}</td>
                <td>{{ s.pet }}</td>
                <td class="text-date">{{ s.date }}</td>
                <td><span class="badge" :class="s.statusClass">{{ s.status }}</span></td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <div class="dash-card-group">
        <div class="dash-card">
          <div class="dash-card-head">
            <h3>Estado de mascotas</h3>
            <RouterLink to="/admin/mascotas" class="view-all">Gestionar</RouterLink>
          </div>
          <div class="status-bars">
            <div v-for="s in petStatus" :key="s.label" class="status-bar-item">
              <div class="sb-header">
                <span class="sb-label">{{ s.label }}</span>
                <span class="sb-count">{{ s.count }}</span>
              </div>
              <div class="sb-track">
                <div class="sb-fill" :style="{ width: s.pct + '%', background: s.color }"></div>
              </div>
            </div>
          </div>
        </div>

        <div class="dash-card">
          <div class="dash-card-head">
            <h3>Rescates activos</h3>
            <RouterLink to="/admin/rescates" class="view-all">Ver todos</RouterLink>
          </div>
          <div class="rescue-list">
            <div v-for="r in activeRescues" :key="r.id" class="rescue-item">
              <div class="rescue-info">
                <span class="rescue-name">{{ r.pet }}</span>
                <span class="rescue-loc">{{ r.location }}</span>
              </div>
              <span class="badge badge-active">{{ r.status }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="quick-actions">
      <h3>Acciones rápidas</h3>
      <div class="qa-grid">
        <RouterLink v-for="qa in quickActions" :key="qa.label" :to="qa.to" class="qa-btn">
          {{ qa.label }}
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
        { label:'Mascotas registradas', value:42,  color:'#92A894', trend:'5 este mes',  up:true  },
        { label:'Adopciones activas',   value:8,   color:'#F9C17A', trend:'2 esta semana', up:true },
        { label:'Rescates activos',     value:12,  color:'#7C927E', trend:'3 nuevos',    up:true  },
        { label:'Usuarios registrados', value:156, color:'#92A894', trend:'12 nuevos',   up:true  },
        { label:'Donaciones (mes)',     value:'₡124.000', color:'#F9C17A', trend:'20%', up:true },
        { label:'Voluntarios activos',  value:40,  color:'#7C927E', trend:'Estables',     up:false },
      ],
      recentAdoptions: [
        { id:1, name:'Ana Rodríguez',  pet:'Luna',   date:'2026-04-09', status:'Pendiente',  statusClass:'badge-yellow' },
        { id:2, name:'Carlos Mora',    pet:'Rocky',  date:'2026-04-08', status:'Aprobada',   statusClass:'badge-green'  },
        { id:3, name:'Sofía Vega',     pet:'Mochi',  date:'2026-04-07', status:'Pendiente',  statusClass:'badge-yellow' },
        { id:4, name:'Diego Salas',    pet:'Bella',  date:'2026-04-05', status:'Rechazada',  statusClass:'badge-red'    },
        { id:5, name:'Laura Jiménez',  pet:'Nube',   date:'2026-04-03', status:'Aprobada',   statusClass:'badge-green'  },
      ],
      petStatus: [
        { label:'Disponible',  count:26, pct:62, color:'#92A894' },
        { label:'En proceso',  count:8,  pct:19, color:'#F9C17A' },
        { label:'Adoptada',    count:8,  pct:19, color:'#7C927E' },
      ],
      activeRescues: [
        { id:1, pet:'Luna',   location:'Desamparados, SJ', status:'Activo' },
        { id:2, pet:'Mochi',  location:'Cartago Centro',   status:'Activo' },
        { id:3, pet:'Rocky',  location:'Heredia, Barreal', status:'Activo' },
      ],
      quickActions: [
        { label:'Nueva mascota',     to:'/admin/mascotas'   },
        { label:'Nueva adopción',    to:'/admin/adopciones' },
        { label:'Registrar rescate', to:'/admin/rescates'   },
        { label:'Control de salud',  to:'/admin/salud'      },
        { label:'Nuevo usuario',     to:'/admin/usuarios'   },
        { label:'Ver donaciones',    to:'/admin/donaciones' },
      ]
    }
  }
}
</script>

<style scoped>
/* ── Estilos Generales ── */
.dashboard {
  padding: 40px;
  background-color: #FAFAFA;
  min-height: 100vh;
  font-family: 'Inter', sans-serif;
}

.dash-header {
  margin-bottom: 32px;
}

.dash-title {
  font-size: 32px;
  font-weight: 800;
  color: #3A473C;
  letter-spacing: -1px;
  margin-bottom: 6px;
}

.dash-sub {
  font-size: 14px;
  color: #6C756D;
  font-weight: 500;
}

/* ── Grid de KPIs ── */
.kpi-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 20px;
  margin-bottom: 32px;
}

.kpi-card {
  background: white;
  border-radius: 20px;
  padding: 24px;
  box-shadow: 0 4px 20px rgba(58, 71, 60, 0.02);
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  height: 110px;
  position: relative;
  transition: transform 0.3s ease;
}

.kpi-card:hover {
  transform: translateY(-3px);
}

.kpi-info {
  display: flex;
  flex-direction: column-reverse;
  gap: 6px;
}

.kpi-value {
  font-size: 30px;
  font-weight: 800;
  color: #3A473C;
  line-height: 1;
}

.kpi-label {
  font-size: 13px;
  color: #6C756D;
  font-weight: 600;
}

.kpi-trend-wrapper {
  align-self: flex-end;
}

.kpi-trend {
  font-size: 12px;
  font-weight: 700;
  padding: 4px 10px;
  border-radius: 99px;
  background: #F4F6F4;
  color: #6C756D;
}

.kpi-trend.up {
  color: #7C927E;
  background: rgba(146, 168, 148, 0.15);
}

.trend-arrow {
  font-weight: 800;
}

/* ── Estructura de Paneles ── */
.dash-grid {
  display: grid;
  grid-template-columns: 1.6fr 1fr;
  gap: 24px;
  margin-bottom: 32px;
}

.dash-card-group {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.dash-card {
  background: white;
  border-radius: 24px;
  padding: 28px;
  box-shadow: 0 4px 20px rgba(58, 71, 60, 0.02);
}

.dash-card-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 24px;
}

.dash-card-head h3 {
  font-size: 18px;
  font-weight: 800;
  color: #3A473C;
  letter-spacing: -0.5px;
}

.view-all {
  font-size: 13px;
  color: #92A894;
  font-weight: 700;
  text-decoration: none;
  transition: color 0.2s;
}

.view-all:hover {
  color: #7C927E;
}

/* ── Tablas de Datos ── */
.table-wrapper {
  overflow-x: auto;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
}

.data-table th {
  font-size: 13px;
  font-weight: 700;
  color: #6C756D;
  padding-bottom: 16px;
  border-bottom: 1px solid #F4F6F4;
}

.data-table td {
  padding: 16px 0;
  font-size: 14px;
  color: #3A473C;
  border-bottom: 1px solid #FAFAFA;
}

.font-semibold {
  font-weight: 600;
}

.text-date {
  font-size: 13px;
  color: #6C756D;
}

/* ── Badges / Etiquetas ── */
.badge {
  padding: 6px 12px;
  border-radius: 10px;
  font-size: 12px;
  font-weight: 700;
  display: inline-block;
}

.badge-yellow { background: rgba(249, 193, 122, 0.2); color: #D18C3A; }
.badge-green  { background: rgba(146, 168, 148, 0.2); color: #5A6E5C; }
.badge-red    { background: rgba(235, 119, 119, 0.15); color: #C45252; }
.badge-active { background: #F4F6F4; color: #3A473C; }

/* ── Barras de Estado ── */
.status-bars {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.status-bar-item {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.sb-header {
  display: flex;
  justify-content: space-between;
  font-size: 14px;
}

.sb-label {
  font-weight: 600;
  color: #6C756D;
}

.sb-count {
  font-weight: 700;
  color: #3A473C;
}

.sb-track {
  height: 8px;
  background: #F4F6F4;
  border-radius: 99px;
  overflow: hidden;
}

.sb-fill {
  height: 100%;
  border-radius: 99px;
  transition: width 0.6s cubic-bezier(0.4, 0, 0.2, 1);
}

/* ── Lista de Rescates ── */
.rescue-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.rescue-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 14px 18px;
  background: #F4F6F4;
  border-radius: 16px;
}

.rescue-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.rescue-name {
  font-size: 14px;
  font-weight: 700;
  color: #3A473C;
}

.rescue-loc {
  font-size: 12px;
  color: #6C756D;
}

/* ── Acciones Rápidas ── */
.quick-actions {
  background: white;
  border-radius: 24px;
  padding: 28px;
  box-shadow: 0 4px 20px rgba(58, 71, 60, 0.02);
}

.quick-actions h3 {
  font-size: 18px;
  font-weight: 800;
  color: #3A473C;
  margin-bottom: 20px;
  letter-spacing: -0.5px;
}

.qa-grid {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
}

.qa-btn {
  padding: 14px 24px;
  background: #F4F6F4;
  border-radius: 14px;
  font-size: 14px;
  font-weight: 700;
  color: #3A473C;
  text-decoration: none;
  transition: all 0.25s ease;
  border: 1px solid transparent;
}

.qa-btn:hover {
  background: white;
  border-color: #92A894;
  color: #92A894;
  transform: translateY(-2px);
  box-shadow: 0 6px 15px rgba(146, 168, 148, 0.1);
}

/* ── Media Queries (Responsivo) ── */
@media (max-width: 1100px) {
  .kpi-grid { grid-template-columns: repeat(2, 1fr); }
  .dash-grid { grid-template-columns: 1fr; }
}

@media (max-width: 650px) {
  .kpi-grid { grid-template-columns: 1fr; }
  .dashboard { padding: 20px; }
  .qa-grid { display: grid; grid-template-columns: 1fr 1fr; }
}
</style>