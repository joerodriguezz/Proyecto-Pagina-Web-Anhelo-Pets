<script setup>
import { ref } from 'vue'

const activeTab = ref('historial')

const historial = ref([
  { id:1, mascota:'Luna',  fecha:'2026-04-01', diagnostico:'Revisión general',      vet:'Dr. Castro', obs:'Peso normal, buen apetito' },
  { id:2, mascota:'Rocky', fecha:'2026-03-20', diagnostico:'Herida en pata trasera', vet:'Dr. Castro', obs:'Curación completa, sin complicaciones' },
  { id:3, mascota:'Mochi', fecha:'2026-03-10', diagnostico:'Control de vacunas',    vet:'Dra. López', obs:'Al día con el calendario de vacunación' },
])

const vacunas = ref([
  { id:1, mascota:'Luna',  tipo:'Antirrábica',     fecha:'2026-03-01', dosis:'Anual'   },
  { id:2, mascota:'Rocky', tipo:'Polivalente',     fecha:'2026-02-15', dosis:'1ra dosis' },
  { id:3, mascota:'Mochi', tipo:'Triple Felina',   fecha:'2026-02-10', dosis:'Refuerzo' },
])

const tratamientos = ref([
  { id:1, mascota:'Luna',  tipo:'Desparasitación', fecha:'2026-03-05', obs:'Preventivo trimestral' },
  { id:2, mascota:'Rocky', tipo:'Limpieza dental', fecha:'2026-02-28', obs:'Sin complicaciones' },
  { id:3, mascota:'Canela',tipo:'Antipulgas',      fecha:'2026-02-20', obs:'Aplicación mensual' },
])
</script>

<template>
  <div class="view-container">
    <header class="page-header">
      <div>
        <h1 class="admin-page-title">Control de Salud</h1>
        <p class="admin-page-sub">Historial médico, vacunas y tratamientos</p>
      </div>
      
      <div class="tab-buttons">
        <button class="tab-btn" :class="{ active: activeTab === 'historial' }" @click="activeTab = 'historial'">Historial</button>
        <button class="tab-btn" :class="{ active: activeTab === 'vacunas' }" @click="activeTab = 'vacunas'">Vacunas</button>
        <button class="tab-btn" :class="{ active: activeTab === 'tratamientos' }" @click="activeTab = 'tratamientos'">Tratamientos</button>
      </div>
    </header>

    <div v-if="activeTab === 'historial'" class="tab-content">
      <div class="form-panel">
        <h3>Registrar estado de salud</h3>
        <div class="form-grid">
          <div class="form-group">
            <label>Mascota *</label>
            <select class="custom-select">
              <option>Seleccionar</option>
              <option>Luna</option>
              <option>Mochi</option>
              <option>Rocky</option>
            </select>
          </div>
          <div class="form-group">
            <label>Fecha *</label>
            <input type="date" class="custom-input" />
          </div>
          <div class="form-group full-width">
            <label>Diagnóstico *</label>
            <input placeholder="Diagnóstico médico..." class="custom-input" />
          </div>
          <div class="form-group full-width">
            <label>Observaciones</label>
            <textarea placeholder="Observaciones adicionales..." class="custom-textarea"></textarea>
          </div>
        </div>
        <div class="form-actions">
          <button class="btn-save">Guardar registro</button>
        </div>
        <div class="readonly-notice">
          Los registros médicos no pueden modificarse ni eliminarse una vez guardados (SAL-006)
        </div>
      </div>

      <div class="table-wrapper">
        <table class="data-table">
          <thead>
            <tr>
              <th>Mascota</th>
              <th>Fecha</th>
              <th>Diagnóstico</th>
              <th>Veterinario</th>
              <th>Observaciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="r in historial" :key="r.id">
              <td class="font-semibold">{{ r.mascota }}</td>
              <td class="text-secondary">{{ r.fecha }}</td>
              <td class="font-medium">{{ r.diagnostico }}</td>
              <td>{{ r.vet }}</td>
              <td class="text-secondary">{{ r.obs }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <div v-if="activeTab === 'vacunas'" class="tab-content">
      <div class="form-panel">
        <h3>Registrar vacuna</h3>
        <div class="form-grid">
          <div class="form-group">
            <label>Mascota *</label>
            <select class="custom-select">
              <option>Seleccionar</option>
              <option>Luna</option>
              <option>Mochi</option>
            </select>
          </div>
          <div class="form-group">
            <label>Tipo de vacuna *</label>
            <input placeholder="Ej. Antirrábica" class="custom-input" />
          </div>
          <div class="form-group">
            <label>Fecha *</label>
            <input type="date" class="custom-input" />
          </div>
          <div class="form-group">
            <label>Dosis *</label>
            <input placeholder="Ej. 1ra dosis" class="custom-input" />
          </div>
        </div>
        <div class="form-actions">
          <button class="btn-save">Guardar vacuna</button>
        </div>
      </div>

      <div class="table-wrapper">
        <table class="data-table">
          <thead>
            <tr>
              <th>Mascota</th>
              <th>Vacuna</th>
              <th>Fecha</th>
              <th>Dosis</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="v in vacunas" :key="v.id">
              <td class="font-semibold">{{ v.mascota }}</td>
              <td class="font-medium">{{ v.tipo }}</td>
              <td class="text-secondary">{{ v.fecha }}</td>
              <td><span class="badge badge-olive">{{ v.dosis }}</span></td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <div v-if="activeTab === 'tratamientos'" class="tab-content">
      <div class="form-panel">
        <h3>Registrar tratamiento</h3>
        <div class="form-grid">
          <div class="form-group">
            <label>Mascota *</label>
            <select class="custom-select">
              <option>Seleccionar</option>
              <option>Luna</option>
              <option>Rocky</option>
            </select>
          </div>
          <div class="form-group">
            <label>Tipo de tratamiento *</label>
            <input placeholder="Ej. Desparasitación" class="custom-input" />
          </div>
          <div class="form-group">
            <label>Fecha *</label>
            <input type="date" class="custom-input" />
          </div>
          <div class="form-group full-width">
            <label>Observaciones</label>
            <textarea placeholder="Descripción del tratamiento..." class="custom-textarea"></textarea>
          </div>
        </div>
        <div class="form-actions">
          <button class="btn-save">Guardar tratamiento</button>
        </div>
      </div>

      <div class="table-wrapper">
        <table class="data-table">
          <thead>
            <tr>
              <th>Mascota</th>
              <th>Tratamiento</th>
              <th>Fecha</th>
              <th>Observaciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="t in tratamientos" :key="t.id">
              <td class="font-semibold">{{ t.mascota }}</td>
              <td class="font-medium">{{ t.tipo }}</td>
              <td class="text-secondary">{{ t.fecha }}</td>
              <td class="text-secondary">{{ t.obs }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* ── Estructura de Encabezado ── */
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

/* ── Sistema de Pestañas (Tabs) ── */
.tab-buttons { 
  display: flex; 
  gap: 8px; 
}

.tab-btn {
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

.tab-btn:hover {
  background: #F4F6F4;
  color: #3A473C;
}

.tab-btn.active { 
  background: #92A894; 
  border-color: #92A894; 
  color: white; 
}

/* ── Panel de Formulario ── */
.form-panel { 
  background: white; 
  border-radius: 24px; 
  padding: 28px; 
  box-shadow: 0 4px 20px rgba(58, 71, 60, 0.02); 
  margin-bottom: 28px;
}

.form-panel h3 { 
  font-size: 18px; 
  font-weight: 800; 
  color: #3A473C;
  margin-bottom: 24px; 
  letter-spacing: -0.5px;
}

.form-grid { 
  display: grid; 
  grid-template-columns: 1fr 1fr; 
  gap: 16px; 
}

.full-width { 
  grid-column: 1 / -1; 
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-group label {
  font-size: 14px;
  font-weight: 700;
  color: #3A473C;
}

.custom-input, .custom-select, .custom-textarea {
  width: 100%;
  padding: 13px 16px;
  border-radius: 14px;
  border: 2px solid #F4F6F4;
  background-color: #F4F6F4;
  font-size: 14px;
  color: #3A473C;
  transition: all 0.3s ease;
  outline: none;
  box-sizing: border-box;
}

.custom-textarea {
  height: 100px;
  resize: vertical;
  font-family: inherit;
}

.custom-select {
  appearance: none;
  background-image: url("data:image/svg+xml;utf8,<svg xmlns='http://www.w3.org/2000/svg' width='24' height='24' viewBox='0 0 24 24' fill='none' stroke='%236C756D' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'><polyline points='6 9 12 15 18 9'></polyline></svg>");
  background-repeat: no-repeat;
  background-position: right 14px center;
  background-size: 16px;
  padding-right: 40px;
}

.custom-input:focus, .custom-select:focus, .custom-textarea:focus {
  background-color: white;
  border-color: #92A894;
  box-shadow: 0 6px 15px rgba(146, 168, 148, 0.05);
}

.form-actions { 
  display: flex; 
  gap: 12px; 
  margin-top: 24px; 
}

.btn-save {
  padding: 13px 24px;
  border-radius: 12px;
  border: none;
  background-color: #3A473C;
  color: white;
  font-size: 14px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s ease;
}

.btn-save:hover {
  background-color: #2D372F;
}

/* Nota informativa de auditoría */
.readonly-notice {
  margin-top: 20px; 
  padding: 14px 18px;
  background: #FFFBF4; 
  border-radius: 14px;
  font-size: 13px; 
  color: #B37D3E; 
  border-left: 4px solid #F9C17A;
  font-weight: 600;
}

/* ── Estilos de Tablas ── */
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

.font-semibold { font-weight: 600; }
.font-medium { font-weight: 500; }
.text-secondary { font-size: 13px; color: #6C756D; }

/* Badges de Dosis */
.badge-olive { 
  background: rgba(146, 168, 148, 0.15); 
  color: #5A6E5C; 
  padding: 4px 10px;
  border-radius: 8px;
  font-size: 12px;
  font-weight: 700;
  display: inline-block;
}

/* Ajustes Responsivos */
@media (max-width: 768px) {
  .page-header { flex-direction: column; align-items: flex-start; gap: 14px; }
  .tab-buttons { width: 100%; justify-content: space-between; }
  .tab-btn { flex: 1; text-align: center; padding: 10px 0; }
  .form-grid { grid-template-columns: 1fr; gap: 0; }
}
</style>