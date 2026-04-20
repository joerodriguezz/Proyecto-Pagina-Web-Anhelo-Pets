<script setup>
import { ref } from 'vue'
const activeTab = ref('historial')
</script>

<template>
  <div>
    <div class="page-header">
      <div>
        <h1 class="admin-page-title">🏥 Control de Salud</h1>
        <p class="admin-page-sub">Historial médico, vacunas y tratamientos</p>
      </div>
      <div class="tab-buttons">
        <button class="tab-btn" :class="{ active: activeTab==='historial' }"   @click="activeTab='historial'">📋 Historial</button>
        <button class="tab-btn" :class="{ active: activeTab==='vacunas' }"     @click="activeTab='vacunas'">💉 Vacunas</button>
        <button class="tab-btn" :class="{ active: activeTab==='tratamientos' }" @click="activeTab='tratamientos'">💊 Tratamientos</button>
      </div>
    </div>

    <!-- Historial Tab -->
    <div v-if="activeTab==='historial'">
      <div class="form-panel">
        <h3>Registrar estado de salud</h3>
        <div class="form-grid">
          <div class="form-group"><label class="form-label">Mascota *</label><select class="form-select"><option>Seleccionar</option><option>Luna</option><option>Mochi</option><option>Rocky</option></select></div>
          <div class="form-group"><label class="form-label">Fecha *</label><input class="form-input" type="date" /></div>
          <div class="form-group full-width"><label class="form-label">Diagnóstico *</label><input class="form-input" placeholder="Diagnóstico médico..." /></div>
          <div class="form-group full-width"><label class="form-label">Observaciones</label><textarea class="form-textarea" placeholder="Observaciones adicionales..."></textarea></div>
        </div>
        <div class="form-actions"><button class="btn-primary">💾 Guardar registro</button></div>
        <p class="readonly-notice">⚠️ Los registros médicos no pueden modificarse ni eliminarse una vez guardados (SAL-006)</p>
      </div>

      <div class="table-wrapper" style="margin-top:20px;">
        <table class="data-table">
          <thead><tr><th>Mascota</th><th>Fecha</th><th>Diagnóstico</th><th>Veterinario</th><th>Observaciones</th></tr></thead>
          <tbody>
            <tr v-for="r in historial" :key="r.id">
              <td>🐾 {{ r.mascota }}</td>
              <td style="font-size:12px;color:var(--text-light)">{{ r.fecha }}</td>
              <td><strong>{{ r.diagnostico }}</strong></td>
              <td>{{ r.vet }}</td>
              <td style="font-size:13px;color:var(--text-mid)">{{ r.obs }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Vacunas Tab -->
    <div v-if="activeTab==='vacunas'">
      <div class="form-panel">
        <h3>Registrar vacuna</h3>
        <div class="form-grid">
          <div class="form-group"><label class="form-label">Mascota *</label><select class="form-select"><option>Seleccionar</option><option>Luna</option><option>Mochi</option></select></div>
          <div class="form-group"><label class="form-label">Tipo de vacuna *</label><input class="form-input" placeholder="Ej. Antirrábica" /></div>
          <div class="form-group"><label class="form-label">Fecha *</label><input class="form-input" type="date" /></div>
          <div class="form-group"><label class="form-label">Dosis *</label><input class="form-input" placeholder="Ej. 1ra dosis" /></div>
        </div>
        <div class="form-actions"><button class="btn-primary">💾 Guardar vacuna</button></div>
      </div>

      <div class="table-wrapper" style="margin-top:20px;">
        <table class="data-table">
          <thead><tr><th>Mascota</th><th>Vacuna</th><th>Fecha</th><th>Dosis</th></tr></thead>
          <tbody>
            <tr v-for="v in vacunas" :key="v.id">
              <td>🐾 {{ v.mascota }}</td>
              <td>💉 {{ v.tipo }}</td>
              <td style="font-size:12px;color:var(--text-light)">{{ v.fecha }}</td>
              <td><span class="badge badge-blue">{{ v.dosis }}</span></td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Tratamientos Tab -->
    <div v-if="activeTab==='tratamientos'">
      <div class="form-panel">
        <h3>Registrar tratamiento</h3>
        <div class="form-grid">
          <div class="form-group"><label class="form-label">Mascota *</label><select class="form-select"><option>Seleccionar</option><option>Luna</option><option>Rocky</option></select></div>
          <div class="form-group"><label class="form-label">Tipo de tratamiento *</label><input class="form-input" placeholder="Ej. Desparasitación" /></div>
          <div class="form-group"><label class="form-label">Fecha *</label><input class="form-input" type="date" /></div>
          <div class="form-group full-width"><label class="form-label">Observaciones</label><textarea class="form-textarea" placeholder="Descripción del tratamiento..."></textarea></div>
        </div>
        <div class="form-actions"><button class="btn-primary">💾 Guardar tratamiento</button></div>
      </div>

      <div class="table-wrapper" style="margin-top:20px;">
        <table class="data-table">
          <thead><tr><th>Mascota</th><th>Tratamiento</th><th>Fecha</th><th>Observaciones</th></tr></thead>
          <tbody>
            <tr v-for="t in tratamientos" :key="t.id">
              <td>🐾 {{ t.mascota }}</td>
              <td>💊 {{ t.tipo }}</td>
              <td style="font-size:12px;color:var(--text-light)">{{ t.fecha }}</td>
              <td style="font-size:13px;color:var(--text-mid)">{{ t.obs }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  data() {
    return {
      historial: [
        { id:1, mascota:'Luna',  fecha:'2026-04-01', diagnostico:'Revisión general',      vet:'Dr. Castro', obs:'Peso normal, buen apetito' },
        { id:2, mascota:'Rocky', fecha:'2026-03-20', diagnostico:'Herida en pata trasera', vet:'Dr. Castro', obs:'Curación completa, sin complicaciones' },
        { id:3, mascota:'Mochi', fecha:'2026-03-10', diagnostico:'Control de vacunas',    vet:'Dra. López', obs:'Al día con el calendario de vacunación' },
      ],
      vacunas: [
        { id:1, mascota:'Luna',  tipo:'Antirrábica',     fecha:'2026-03-01', dosis:'Anual'   },
        { id:2, mascota:'Rocky', tipo:'Polivalente',     fecha:'2026-02-15', dosis:'1ra dosis' },
        { id:3, mascota:'Mochi', tipo:'Triple Felina',   fecha:'2026-02-10', dosis:'Refuerzo' },
      ],
      tratamientos: [
        { id:1, mascota:'Luna',  tipo:'Desparasitación', fecha:'2026-03-05', obs:'Preventivo trimestral' },
        { id:2, mascota:'Rocky', tipo:'Limpieza dental', fecha:'2026-02-28', obs:'Sin complicaciones' },
        { id:3, mascota:'Canela',tipo:'Antipulgas',      fecha:'2026-02-20', obs:'Aplicación mensual' },
      ],
    }
  }
}
</script>

<style scoped>
.page-header { display:flex; justify-content:space-between; align-items:flex-start; margin-bottom:24px; flex-wrap:wrap; gap:14px; }
.admin-page-title { font-size:22px; font-weight:800; }
.admin-page-sub   { font-size:13px; color:var(--text-light); margin-top:4px; }

.tab-buttons { display:flex; gap:8px; }
.tab-btn {
  padding:8px 16px; border-radius:var(--radius-full);
  border:1.5px solid var(--border); background:var(--white);
  font-size:13px; font-weight:600; cursor:pointer; transition:all 0.2s;
  color:var(--text-mid);
}
.tab-btn.active { background:var(--green); border-color:var(--green); color:var(--white); }

.form-panel { background:var(--white); border:1px solid var(--border); border-radius:var(--radius-md); padding:24px; box-shadow:var(--shadow-sm); }
.form-panel h3 { font-size:16px; font-weight:700; margin-bottom:18px; }
.form-grid { display:grid; grid-template-columns:1fr 1fr; gap:14px; }
.full-width { grid-column:1/-1; }
.form-actions { display:flex; gap:12px; margin-top:18px; }
.readonly-notice {
  margin-top:14px; padding:10px 14px;
  background:#FFF8E1; border-radius:var(--radius-sm);
  font-size:12px; color:#8A6A00; border-left:3px solid var(--yellow);
}
</style>
