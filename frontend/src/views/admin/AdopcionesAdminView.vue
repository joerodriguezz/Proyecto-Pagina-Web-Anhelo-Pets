<script setup>
import { ref, computed } from 'vue'
import Icon from '../../components/Icon.vue'

const filterStatus = ref('Todos')

const solicitudes = ref([
  { id:'ADO-001', solicitante:'Ana Rodríguez',  cedula:'1-1234-5678', telefono:'8812-1234', mascota:'Luna',   fecha:'2026-04-09', status:'Pendiente'  },
  { id:'ADO-002', solicitante:'Carlos Mora',    cedula:'2-2345-6789', telefono:'8901-2345', mascota:'Rocky',  fecha:'2026-04-08', status:'Aprobada'   },
  { id:'ADO-003', solicitante:'Sofía Vega',     cedula:'3-3456-7890', telefono:'8723-4567', mascota:'Mochi',  fecha:'2026-04-07', status:'Pendiente'  },
  { id:'ADO-004', solicitante:'Diego Salas',    cedula:'1-4567-8901', telefono:'8634-5678', mascota:'Bella',  fecha:'2026-04-05', status:'Rechazada'  },
  { id:'ADO-005', solicitante:'Laura Jiménez',  cedula:'4-5678-9012', telefono:'8545-6789', mascota:'Nube',   fecha:'2026-04-03', status:'Aprobada'   },
  { id:'ADO-006', solicitante:'Marco Ureña',    cedula:'2-6789-0123', telefono:'8456-7890', mascota:'Canela', fecha:'2026-04-01', status:'Pendiente'  },
])

const filtered = computed(() => {
  if (filterStatus.value === 'Todos') return solicitudes.value
  return solicitudes.value.filter(s => s.status === filterStatus.value)
})

const statusClass = (status) => {
  return {
    'Pendiente': 'badge-yellow',
    'Aprobada': 'badge-green',
    'Rechazada': 'badge-red'
  }[status] || 'badge-gray'
}
</script>

<template>
  <div class="view-container">
    <header class="page-header">
      <div>
        <h1 class="admin-page-title">Solicitudes de Adopción</h1>
        <p class="admin-page-sub">Gestión y seguimiento de solicitudes de adopción</p>
      </div>
      
      <div class="filter-chips">
        <button 
          v-for="s in ['Todos','Pendiente','Aprobada','Rechazada']" 
          :key="s" 
          class="chip" 
          :class="{ active: filterStatus === s }" 
          @click="filterStatus = s"
        >
          {{ s }}
        </button>
      </div>
    </header>

    <div class="summary-row">
      <div class="sum-card pending">
        <span class="sum-label">Pendientes</span>
        <strong class="sum-value">{{ solicitudes.filter(s => s.status === 'Pendiente').length }}</strong>
      </div>
      <div class="sum-card approved">
        <span class="sum-label">Aprobadas</span>
        <strong class="sum-value">{{ solicitudes.filter(s => s.status === 'Aprobada').length }}</strong>
      </div>
      <div class="sum-card rejected">
        <span class="sum-label">Rechazadas</span>
        <strong class="sum-value">{{ solicitudes.filter(s => s.status === 'Rechazada').length }}</strong>
      </div>
      <div class="sum-card total">
        <span class="sum-label">Total</span>
        <strong class="sum-value">{{ solicitudes.length }}</strong>
      </div>
    </div>

    <div class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Solicitante</th>
            <th>Cédula</th>
            <th>Teléfono</th>
            <th>Mascota</th>
            <th>Fecha</th>
            <th>Estado</th>
            <th class="text-right">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="s in filtered" :key="s.id">
            <td><span class="id-code">{{ s.id }}</span></td>
            <td class="font-semibold">{{ s.solicitante }}</td>
            <td class="text-secondary">{{ s.cedula }}</td>
            <td>{{ s.telefono }}</td>
            <td class="font-medium">{{ s.mascota }}</td>
            <td class="text-secondary">{{ s.fecha }}</td>
            <td><span class="badge" :class="statusClass(s.status)">{{ s.status }}</span></td>
            <td>
              <div class="action-btns">
                <button v-if="s.status === 'Pendiente'" class="action-btn approve" title="Aprobar">
                  <Icon name="Check" />
                </button>
                <button v-if="s.status === 'Pendiente'" class="action-btn reject" title="Rechazar">
                  <Icon name="X" />
                </button>
                <button class="action-btn view" title="Ver detalle">
                  <Icon name="Show" />
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<style scoped>
/* ── Estructura de Contenedor ── */
.view-container {
  background-color: transparent;
}

.page-header { 
  display: flex; 
  justify-content: space-between; 
  align-items: center; 
  margin-bottom: 32px; 
  flex-wrap: wrap; 
  gap: 16px; 
}

.admin-page-title { 
  font-size: 28px; 
  font-weight: 800; 
  color: #3A473C; 
  letter-spacing: -0.5px;
}

.admin-page-sub { 
  font-size: 14px; 
  color: #6C756D; 
  margin-top: 4px; 
  font-weight: 500;
}

/* ── Filtros (Chips) ── */
.filter-chips { 
  display: flex; 
  gap: 8px; 
  flex-wrap: wrap; 
}

.chip {
  padding: 8px 18px; 
  border-radius: 99px;
  border: 2px solid #F4F6F4; 
  background: white;
  color: #6C756D; 
  font-size: 13px; 
  font-weight: 700; 
  cursor: pointer; 
  transition: all 0.25s ease;
}

.chip:hover {
  background: #F4F6F4;
  color: #3A473C;
}

.chip.active { 
  background: #92A894; 
  border-color: #92A894; 
  color: white; 
}

/* ── Tarjetas de Resumen ── */
.summary-row { 
  display: flex; 
  gap: 20px; 
  margin-bottom: 32px; 
  flex-wrap: wrap; 
}

.sum-card {
  flex: 1; 
  min-width: 140px;
  background: white;
  border-radius: 20px;
  padding: 20px;
  display: flex;
  flex-direction: column-reverse;
  gap: 6px;
  box-shadow: 0 4px 20px rgba(58, 71, 60, 0.02);
  border-top: 4px solid transparent;
}

.sum-value { 
  font-size: 28px; 
  font-weight: 800; 
  color: #3A473C; 
  line-height: 1;
}

.sum-label {
  font-size: 13px;
  font-weight: 600;
  color: #6C756D;
}

.pending  { border-color: #F9C17A; }
.approved { border-color: #92A894; }
.rejected { border-color: #EB7777; }
.total    { border-color: #6C756D; }

/* ── Estilos de Tabla ── */
.table-wrapper {
  background: white;
  border-radius: 24px;
  padding: 24px;
  box-shadow: 0 4px 20px rgba(58, 71, 60, 0.02);
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
  vertical-align: middle;
}

.id-code {
  font-size: 12px;
  font-family: monospace;
  background: #F4F6F4;
  padding: 4px 8px;
  border-radius: 8px;
  color: #3A473C;
  font-weight: 600;
}

.font-semibold { font-weight: 600; }
.font-medium { font-weight: 500; }
.text-secondary { font-size: 13px; color: #6C756D; }
.text-right { text-align: right; }

/* ── Badges de Estado ── */
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
.badge-gray   { background: #F4F6F4; color: #6C756D; }

/* ── Botones de Acción ── */
.action-btns { 
  display: flex; 
  gap: 8px; 
  justify-content: flex-end;
}

.action-btn {
  width: 34px; 
  height: 34px; 
  border: 2px solid #F4F6F4;
  border-radius: 10px; 
  background: white; 
  cursor: pointer;
  display: flex; 
  align-items: center; 
  justify-content: center;
  transition: all 0.2s ease;
  color: #6C756D;
}

.action-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

.action-btn.approve:hover {
  background: rgba(146, 168, 148, 0.15);
  border-color: #92A894;
  color: #7C927E;
}

.action-btn.reject:hover {
  background: rgba(235, 119, 119, 0.1);
  border-color: #EB7777;
  color: #C45252;
}

.action-btn.view:hover {
  background: #F4F6F4;
  border-color: #6C756D;
  color: #3A473C;
}

/* ── Ajustes Responsivos ── */
@media (max-width: 768px) {
  .page-header { flex-direction: column; align-items: flex-start; }
  .filter-chips { width: 100%; }
  .summary-row { display: grid; grid-template-columns: 1fr 1fr; }
}
</style>