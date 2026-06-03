<script setup>
import { ref } from 'vue'
import Icon from '../../components/Icon.vue'

const donaciones = ref([
  { id:'D-001', donante:'María Solano',   monto:25000, fecha:'2026-04-09' },
  { id:'D-002', donante:'Roberto Arias',  monto:10000, fecha:'2026-04-07' },
  { id:'D-003', donante:'Anónimo',        monto:5000,  fecha:'2026-04-05' },
  { id:'D-004', donante:'Empresa XYZ',    monto:50000, fecha:'2026-04-02' },
  { id:'D-005', donante:'Carolina Pérez', monto:15000, fecha:'2026-03-29' },
  { id:'D-006', donante:'Luis Herrera',   monto:8000,  fecha:'2026-03-25' },
  { id:'D-007', donante:'Familia Solís',  monto:30000, fecha:'2026-03-20' },
])
</script>

<template>
  <div class="view-container">
    <header class="page-header">
      <div>
        <h1 class="admin-page-title">Donaciones</h1>
        <p class="admin-page-sub">Historial y control de donaciones recibidas</p>
      </div>
    </header>

    <div class="don-summary">
      <div class="don-card total-mes">
        <span class="don-label">Total este mes</span>
        <strong class="don-value">₡ 124.000</strong>
      </div>
      <div class="don-card total-año">
        <span class="don-label">Total este año</span>
        <strong class="don-value">₡ 890.500</strong>
      </div>
      <div class="don-card count">
        <span class="don-label">Donaciones</span>
        <strong class="don-value">{{ donaciones.length }}</strong>
      </div>
    </div>

    <div class="form-panel">
      <h3>Realizar una donación</h3>
      <form class="form-grid" @submit.prevent>
        <div class="form-group">
          <label>Nombre del donante *</label>
          <input placeholder="Tu nombre" class="custom-input" />
        </div>
        <div class="form-group">
          <label>Monto (₡) *</label>
          <input type="number" min="1" placeholder="0" class="custom-input" />
        </div>
      </form>
      <div class="form-actions">
        <button class="btn-save">Donar ahora</button>
      </div>
    </div>

    <div class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Donante</th>
            <th>Monto</th>
            <th>Fecha</th>
            <th class="text-right">Comprobante</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="d in donaciones" :key="d.id">
            <td><span class="id-code">{{ d.id }}</span></td>
            <td class="font-semibold">{{ d.donante }}</td>
            <td><span class="monto">₡ {{ d.monto.toLocaleString() }}</span></td>
            <td class="text-secondary">{{ d.fecha }}</td>
            <td>
              <div class="action-btns">
                <button class="action-btn" title="Generar comprobante">
                  <Icon name="File" />
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

/* ── Tarjetas de Resumen Financiero ── */
.don-summary { 
  display: flex; 
  gap: 20px; 
  margin-bottom: 32px; 
  flex-wrap: wrap;
}

.don-card {
  flex: 1;
  min-width: 200px;
  background: white;
  border-radius: 20px;
  padding: 24px;
  box-shadow: 0 4px 20px rgba(58, 71, 60, 0.02);
  display: flex; 
  flex-direction: column; 
  gap: 8px;
  border-top: 4px solid transparent;
}

.don-label { 
  font-size: 12px; 
  color: #6C756D; 
  font-weight: 700; 
  text-transform: uppercase; 
  letter-spacing: 0.5px; 
}

.don-value { 
  font-size: 28px; 
  font-weight: 800; 
  color: #3A473C; 
  line-height: 1;
}

.total-mes { border-color: #F9C17A; } /* Acento durazno/amarillo */
.total-año { border-color: #92A894; } /* Verde oliva */
.count     { border-color: #6C756D; } /* Gris medio */

/* ── Panel del Formulario ── */
.form-panel { 
  background: white; 
  border-radius: 24px; 
  padding: 28px; 
  box-shadow: 0 4px 20px rgba(58, 71, 60, 0.02); 
  margin-bottom: 32px;
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

.custom-input {
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

.custom-input:focus {
  background-color: white;
  border-color: #92A894;
  box-shadow: 0 6px 15px rgba(146, 168, 148, 0.05);
}

.form-actions { 
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

/* ── Estilos de la Tabla ── */
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
.text-secondary { font-size: 13px; color: #6C756D; }
.text-right { text-align: right; }

.monto { 
  font-weight: 700; 
  color: #5A6E5C; /* Verde oscuro suave para el dinero */
}

/* ── Botón de Acción Comprobante ── */
.action-btns { 
  display: flex; 
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
  background: #F4F6F4;
  border-color: #6C756D;
  color: #3A473C;
  transform: translateY(-1px);
}

/* Ajustes Responsivos */
@media (max-width: 768px) {
  .don-summary { display: grid; grid-template-columns: 1fr; gap: 14px; }
  .form-grid { grid-template-columns: 1fr; gap: 0; }
}
</style>