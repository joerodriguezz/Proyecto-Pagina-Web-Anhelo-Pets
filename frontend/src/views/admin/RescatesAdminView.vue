<template>
  <div>
    <div class="page-header">
      <div>
        <h1 class="admin-page-title">🚨 Gestión de Rescates</h1>
        <p class="admin-page-sub">Registro y seguimiento de animales rescatados</p>
      </div>
      <button class="btn-primary" @click="showForm = !showForm">
        {{ showForm ? '✕ Cancelar' : '＋ Nuevo rescate' }}
      </button>
    </div>

    <Transition name="slide-down">
      <div v-if="showForm" class="form-panel">
        <h3>Registrar nuevo rescate</h3>
        <div class="form-grid">
          <div class="form-group"><label class="form-label">Mascota asociada *</label><select class="form-select"><option>Seleccionar mascota</option><option>Luna</option><option>Rocky</option><option>Mochi</option></select></div>
          <div class="form-group"><label class="form-label">Fecha de rescate *</label><input class="form-input" type="date" /></div>
          <div class="form-group full-width"><label class="form-label">Ubicación *</label><input class="form-input" placeholder="Cantón, distrito, señas exactas..." /></div>
          <div class="form-group full-width"><label class="form-label">Descripción *</label><textarea class="form-textarea" placeholder="Descripción de las condiciones del rescate..."></textarea></div>
          <div class="form-group"><label class="form-label">Casa cuna asignada</label><select class="form-select"><option>Sin asignar</option><option>Familia Mora</option><option>Familia Vega</option></select></div>
          <div class="form-group"><label class="form-label">Estado</label><select class="form-select"><option>Activo</option><option>Cerrado</option></select></div>
        </div>
        <div class="form-actions">
          <button class="btn-primary">💾 Guardar rescate</button>
          <button class="btn-outline-green" @click="showForm = false">Cancelar</button>
        </div>
      </div>
    </Transition>

    <div class="table-wrapper" style="margin-top:20px;">
      <table class="data-table">
        <thead>
          <tr><th>ID</th><th>Mascota</th><th>Fecha</th><th>Ubicación</th><th>Casa cuna</th><th>Estado</th><th>Acciones</th></tr>
        </thead>
        <tbody>
          <tr v-for="r in rescates" :key="r.id">
            <td><code style="font-size:11px;background:var(--cream);padding:2px 6px;border-radius:4px">{{ r.id }}</code></td>
            <td>🐾 {{ r.mascota }}</td>
            <td style="font-size:12px;color:var(--text-light)">{{ r.fecha }}</td>
            <td style="font-size:13px">📍 {{ r.ubicacion }}</td>
            <td>{{ r.casaCuna }}</td>
            <td><span class="badge" :class="r.estado==='Activo'?'badge-green':'badge-gray'">{{ r.estado }}</span></td>
            <td>
                <div class="action-btns">
                  <Icon name="Edit" class="action-btn" title="Editar" fallback="✏️" />
                  <Icon name="Clipboard" class="action-btn" title="Ver historial" fallback="📋" />
                  <Icon name="Lock" class="action-btn" title="Cerrar rescate" fallback="🔒" />
                </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script>
import Icon from '../../components/Icon.vue'

export default {
  components: { Icon },
  data() {
    return {
      showForm: false,
      rescates: [
        { id:'R-001', mascota:'Luna',   fecha:'2026-03-15', ubicacion:'Desamparados, SJ',  casaCuna:'Familia Mora',  estado:'Activo'  },
        { id:'R-002', mascota:'Mochi',  fecha:'2026-03-08', ubicacion:'Cartago Centro',    casaCuna:'Familia Vega',  estado:'Activo'  },
        { id:'R-003', mascota:'Rocky',  fecha:'2026-02-22', ubicacion:'Heredia, Barreal',  casaCuna:'Familia Salas', estado:'Activo'  },
        { id:'R-004', mascota:'Canela', fecha:'2026-02-10', ubicacion:'Alajuela Centro',   casaCuna:'Familia Pérez', estado:'Activo'  },
        { id:'R-005', mascota:'Max',    fecha:'2026-01-30', ubicacion:'Goicoechea, SJ',    casaCuna:'—',             estado:'Cerrado' },
      ]
    }
  }
}
</script>

<style scoped>
.page-header { display:flex; justify-content:space-between; align-items:flex-start; margin-bottom:24px; }
.admin-page-title { font-size:22px; font-weight:800; }
.admin-page-sub   { font-size:13px; color:var(--text-light); margin-top:4px; }
.form-panel { background:var(--white); border:1px solid var(--border); border-radius:var(--radius-md); padding:24px; box-shadow:var(--shadow-sm); }
.form-panel h3 { font-size:16px; font-weight:700; margin-bottom:18px; }
.form-grid { display:grid; grid-template-columns:1fr 1fr; gap:14px; }
.full-width { grid-column:1/-1; }
.form-actions { display:flex; gap:12px; margin-top:18px; }
.action-btns { display:flex; gap:6px; }
.action-btn { width:30px; height:30px; border:1px solid var(--border); border-radius:6px; background:var(--white); cursor:pointer; font-size:14px; display:flex; align-items:center; justify-content:center; }
.action-btn:hover { background:var(--cream); }
.slide-down-enter-active { transition:all 0.25s; }
.slide-down-leave-active { transition:all 0.2s; }
.slide-down-enter-from   { opacity:0; transform:translateY(-10px); }
.slide-down-leave-to     { opacity:0; }
</style>
