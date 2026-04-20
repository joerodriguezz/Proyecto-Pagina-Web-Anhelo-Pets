<template>
  <div>
    <div class="page-header">
      <div>
        <h1 class="admin-page-title">💰 Donaciones</h1>
        <p class="admin-page-sub">Historial y control de donaciones recibidas</p>
      </div>
    </div>

    <!-- Summary -->
    <div class="don-summary">
      <div class="don-card total-mes">
        <span class="don-label">Total este mes</span>
        <span class="don-value">₡ 124.000</span>
      </div>
      <div class="don-card total-año">
        <span class="don-label">Total este año</span>
        <span class="don-value">₡ 890.500</span>
      </div>
      <div class="don-card count">
        <span class="don-label">Donaciones</span>
        <span class="don-value">{{ donaciones.length }}</span>
      </div>
    </div>

    <!-- Donation Form (public-facing demo) -->
    <div class="form-panel" style="margin-bottom:20px;">
      <h3>💛 Realizar una donación</h3>
      <div class="form-grid">
        <div class="form-group"><label class="form-label">Nombre del donante *</label><input class="form-input" placeholder="Tu nombre" /></div>
        <div class="form-group">
          <label class="form-label">Monto (₡) *</label>
          <input class="form-input" type="number" min="1" placeholder="0" />
        </div>
      </div>
      <div class="form-actions"><button class="btn-primary">💛 Donar ahora</button></div>
    </div>

    <div class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr><th>ID</th><th>Donante</th><th>Monto</th><th>Fecha</th><th>Comprobante</th></tr>
        </thead>
        <tbody>
          <tr v-for="d in donaciones" :key="d.id">
            <td><code style="font-size:11px;background:var(--cream);padding:2px 6px;border-radius:4px">{{ d.id }}</code></td>
            <td><strong>{{ d.donante }}</strong></td>
            <td><span class="monto">₡ {{ d.monto.toLocaleString() }}</span></td>
            <td style="font-size:12px;color:var(--text-light)">{{ d.fecha }}</td>
            <td><Icon name="File" class="action-btn" title="Generar comprobante" fallback="📄" /></td>
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
      donaciones: [
        { id:'D-001', donante:'María Solano',   monto:25000, fecha:'2026-04-09' },
        { id:'D-002', donante:'Roberto Arias',  monto:10000, fecha:'2026-04-07' },
        { id:'D-003', donante:'Anónimo',        monto:5000,  fecha:'2026-04-05' },
        { id:'D-004', donante:'Empresa XYZ',    monto:50000, fecha:'2026-04-02' },
        { id:'D-005', donante:'Carolina Pérez', monto:15000, fecha:'2026-03-29' },
        { id:'D-006', donante:'Luis Herrera',   monto:8000,  fecha:'2026-03-25' },
        { id:'D-007', donante:'Familia Solís',  monto:30000, fecha:'2026-03-20' },
      ]
    }
  }
}
</script>

<style scoped>
.page-header { display:flex; justify-content:space-between; align-items:flex-start; margin-bottom:24px; }
.admin-page-title { font-size:22px; font-weight:800; }
.admin-page-sub   { font-size:13px; color:var(--text-light); margin-top:4px; }

.don-summary { display:flex; gap:16px; margin-bottom:24px; }
.don-card {
  flex:1;
  background:var(--white);
  border-radius:var(--radius-md);
  padding:20px 24px;
  box-shadow:var(--shadow-sm);
  display:flex; flex-direction:column; gap:6px;
}
.don-label { font-size:12px; color:var(--text-light); font-weight:600; text-transform:uppercase; letter-spacing:0.5px; }
.don-value { font-size:28px; font-weight:800; color:var(--green-dark); }
.total-mes { border-top:3px solid var(--yellow); }
.total-año { border-top:3px solid var(--green); }
.count     { border-top:3px solid var(--text-light); }

.form-panel { background:var(--white); border:1px solid var(--border); border-radius:var(--radius-md); padding:24px; box-shadow:var(--shadow-sm); }
.form-panel h3 { font-size:16px; font-weight:700; margin-bottom:18px; }
.form-grid { display:grid; grid-template-columns:1fr 1fr; gap:14px; }
.form-actions { margin-top:18px; }

.monto { font-weight:700; color:var(--green-dark); }
.action-btn { width:30px; height:30px; border:1px solid var(--border); border-radius:6px; background:var(--white); cursor:pointer; font-size:14px; display:flex; align-items:center; justify-content:center; }
.action-btn:hover { background:var(--cream); }
</style>
