<script setup>
import { ref } from 'vue'
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

const statusClass = s => ({ 'Pendiente':'badge-yellow', 'Aprobada':'badge-green', 'Rechazada':'badge-red' }[s] || 'badge-gray')
</script>

<script>
import { computed } from 'vue'
export default {
  computed: {
    filtered() {
      if (this.filterStatus === 'Todos') return this.solicitudes
      return this.solicitudes.filter(s => s.status === this.filterStatus)
    }
  }
}
</script>

<template>
  <div>
    <div class="page-header">
      <div>
        <h1 class="admin-page-title">💛 Solicitudes de Adopción</h1>
        <p class="admin-page-sub">Gestión y seguimiento de solicitudes de adopción</p>
      </div>
      <div class="filter-chips">
        <button v-for="s in ['Todos','Pendiente','Aprobada','Rechazada']" :key="s" class="chip" :class="{ active: filterStatus === s }" @click="filterStatus = s">
          {{ s }}
        </button>
      </div>
    </div>

    <!-- Summary -->
    <div class="summary-row">
      <div class="sum-card pending"><span>⏳</span><strong>{{ solicitudes.filter(s=>s.status==='Pendiente').length }}</strong><span>Pendientes</span></div>
      <div class="sum-card approved"><span>✅</span><strong>{{ solicitudes.filter(s=>s.status==='Aprobada').length }}</strong><span>Aprobadas</span></div>
      <div class="sum-card rejected"><span>❌</span><strong>{{ solicitudes.filter(s=>s.status==='Rechazada').length }}</strong><span>Rechazadas</span></div>
      <div class="sum-card total"><span>📋</span><strong>{{ solicitudes.length }}</strong><span>Total</span></div>
    </div>

    <div class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr><th>ID</th><th>Solicitante</th><th>Cédula</th><th>Teléfono</th><th>Mascota</th><th>Fecha</th><th>Estado</th><th>Acciones</th></tr>
        </thead>
        <tbody>
          <tr v-for="s in filtered" :key="s.id">
            <td><code style="font-size:11px;background:var(--cream);padding:2px 6px;border-radius:4px">{{ s.id }}</code></td>
            <td><strong>{{ s.solicitante }}</strong></td>
            <td style="font-size:12px;color:var(--text-light)">{{ s.cedula }}</td>
            <td style="font-size:13px">{{ s.telefono }}</td>
            <td>🐾 {{ s.mascota }}</td>
            <td style="font-size:12px;color:var(--text-light)">{{ s.fecha }}</td>
            <td><span class="badge" :class="statusClass(s.status)">{{ s.status }}</span></td>
            <td>
              <div class="action-btns">
                <Icon name="Check" class="action-btn approve" title="Aprobar" v-if="s.status==='Pendiente'" fallback="✅" />
                <Icon name="X" class="action-btn reject" title="Rechazar" v-if="s.status==='Pendiente'" fallback="❌" />
                <Icon name="Show" class="action-btn view" title="Ver detalle" fallback="👁️" />
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<style scoped>
.page-header { display:flex; justify-content:space-between; align-items:flex-start; margin-bottom:24px; flex-wrap:wrap; gap:14px; }
.admin-page-title { font-size:22px; font-weight:800; color:var(--text-dark); }
.admin-page-sub   { font-size:13px; color:var(--text-light); margin-top:4px; }

.filter-chips { display:flex; gap:8px; flex-wrap:wrap; }
.chip {
  padding:6px 14px; border-radius:var(--radius-full);
  border:1.5px solid var(--border); background:var(--white);
  color:var(--text-mid); font-size:13px; font-weight:500; cursor:pointer; transition:all 0.2s;
}
.chip.active { background:var(--green); border-color:var(--green); color:var(--white); }

.summary-row { display:flex; gap:14px; margin-bottom:20px; flex-wrap:wrap; }
.sum-card {
  flex:1; min-width:100px;
  background:var(--white);
  border-radius:var(--radius-md);
  padding:16px;
  display:flex;
  flex-direction:column;
  align-items:center;
  gap:4px;
  box-shadow:var(--shadow-sm);
  font-size:13px;
  color:var(--text-mid);
  text-align:center;
  border-top:3px solid transparent;
}
.sum-card strong { font-size:26px; font-weight:800; color:var(--text-dark); }
.sum-card span:first-child { font-size:22px; }
.pending  { border-color:#F5C840; }
.approved { border-color:var(--green); }
.rejected { border-color:#E74C3C; }
.total    { border-color:var(--text-light); }

.action-btns { display:flex; gap:6px; }
.action-btn {
  width:30px; height:30px; border:1px solid var(--border);
  border-radius:6px; background:var(--white); cursor:pointer;
  font-size:14px; display:flex; align-items:center; justify-content:center;
  transition:background 0.15s;
}
.action-btn:hover { background:var(--cream); }
</style>
