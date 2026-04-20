<script setup>
import NavBar from '../components/NavBar.vue'
import FooterBar from '../components/FooterBar.vue'

const rescates = [
  { id:'R-001', fecha:'2026-03-15', ubicacion:'Desamparados, San José', descripcion:'Perro encontrado amarrado en poste, con signos de desnutrición.', mascota:'Luna', estado:'Activo', casaCuna:'Familia Mora' },
  { id:'R-002', fecha:'2026-03-08', ubicacion:'Cartago Centro', descripcion:'Gata con cachorros abandonados en caja de cartón.', mascota:'Mochi', estado:'Activo', casaCuna:'Familia Vega' },
  { id:'R-003', fecha:'2026-02-22', ubicacion:'Heredia, Barreal', descripcion:'Perro atropellado, tratamiento veterinario completado.', mascota:'Rocky', estado:'Activo', casaCuna:'Familia Salas' },
  { id:'R-004', fecha:'2026-02-10', ubicacion:'Alajuela Centro', descripcion:'Cachorra encontrada en mercado, aparentemente saludable.', mascota:'Canela', estado:'Activo', casaCuna:'Familia Pérez' },
  { id:'R-005', fecha:'2026-01-30', ubicacion:'Goicoechea, San José', descripcion:'Gato mayor rescatado de calle, requirió cirugía dental.', mascota:'Max', estado:'Cerrado', casaCuna:'—' },
]

const statusClass = s => s === 'Activo' ? 'badge-green' : 'badge-gray'
</script>

<template>
  <NavBar />

  <div class="page-hero">
    <h1>🚨 Gestión de Rescates</h1>
    <p>Historial de animales rescatados por la Fundación Anhelo Pets</p>
  </div>

  <section class="container rescates-section">
    <!-- Summary cards -->
    <div class="summary-grid">
      <div class="summary-card">
        <span class="s-icon">📋</span>
        <div>
          <div class="s-value">{{ rescates.length }}</div>
          <div class="s-label">Total rescates</div>
        </div>
      </div>
      <div class="summary-card">
        <span class="s-icon">✅</span>
        <div>
          <div class="s-value">{{ rescates.filter(r => r.estado === 'Activo').length }}</div>
          <div class="s-label">Activos</div>
        </div>
      </div>
      <div class="summary-card">
        <span class="s-icon">🏠</span>
        <div>
          <div class="s-value">{{ rescates.filter(r => r.casaCuna !== '—').length }}</div>
          <div class="s-label">En casa cuna</div>
        </div>
      </div>
      <div class="summary-card">
        <span class="s-icon">🔒</span>
        <div>
          <div class="s-value">{{ rescates.filter(r => r.estado === 'Cerrado').length }}</div>
          <div class="s-label">Cerrados</div>
        </div>
      </div>
    </div>

    <!-- Table -->
    <div class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Fecha</th>
            <th>Ubicación</th>
            <th>Descripción</th>
            <th>Mascota</th>
            <th>Casa cuna</th>
            <th>Estado</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="r in rescates" :key="r.id">
            <td><code style="background:var(--cream);padding:2px 8px;border-radius:4px;font-size:12px;">{{ r.id }}</code></td>
            <td>{{ r.fecha }}</td>
            <td>📍 {{ r.ubicacion }}</td>
            <td style="max-width:240px;font-size:13px;">{{ r.descripcion }}</td>
            <td>🐾 {{ r.mascota }}</td>
            <td>{{ r.casaCuna }}</td>
            <td><span class="badge" :class="statusClass(r.estado)">{{ r.estado }}</span></td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Info section about houses -->
    <div class="casas-section">
      <h2 class="section-title" style="text-align:left;margin-bottom:20px;">🏡 Casas Cuna Activas</h2>
      <div class="casas-grid">
        <div v-for="casa in casasCuna" :key="casa.nombre" class="card casa-card">
          <div class="casa-header">
            <span class="casa-icon">🏠</span>
            <div>
              <h3>{{ casa.nombre }}</h3>
              <p class="casa-location">📍 {{ casa.direccion }}</p>
            </div>
          </div>
          <div class="casa-details">
            <span>📞 {{ casa.telefono }}</span>
            <span>👤 {{ casa.responsable }}</span>
            <span class="badge badge-yellow">{{ casa.mascotas }} mascota{{ casa.mascotas !== 1 ? 's' : '' }}</span>
          </div>
        </div>
      </div>
    </div>
  </section>

  <FooterBar />
</template>

<script>
export default {
  data() {
    return {
      casasCuna: [
        { nombre:'Familia Mora',  direccion:'Desamparados, San José', telefono:'8812-3456', responsable:'Ana Mora',    mascotas:1 },
        { nombre:'Familia Vega',  direccion:'Cartago Centro',         telefono:'8765-4321', responsable:'Pedro Vega',  mascotas:2 },
        { nombre:'Familia Salas', direccion:'Heredia, Barreal',       telefono:'8900-1122', responsable:'Lucía Salas', mascotas:1 },
        { nombre:'Familia Pérez', direccion:'Alajuela Centro',        telefono:'8543-9900', responsable:'Mario Pérez', mascotas:1 },
      ]
    }
  }
}
</script>

<style scoped>
.rescates-section { padding: 40px 24px 60px; }

.summary-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 16px;
  margin-bottom: 30px;
}
.summary-card {
  background: var(--white);
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  padding: 20px;
  display: flex;
  align-items: center;
  gap: 16px;
  box-shadow: var(--shadow-sm);
}
.s-icon  { font-size: 32px; }
.s-value { font-size: 28px; font-weight: 800; color: var(--green-dark); }
.s-label { font-size: 12px; color: var(--text-light); font-weight: 500; }

.casas-section { margin-top: 48px; }
.casas-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
  gap: 20px;
}
.casa-card { padding: 20px; }
.casa-header {
  display: flex;
  gap: 14px;
  align-items: flex-start;
  margin-bottom: 14px;
}
.casa-icon { font-size: 32px; }
.casa-header h3 { font-size: 16px; font-weight: 700; margin-bottom: 4px; }
.casa-location { font-size: 12px; color: var(--text-light); }
.casa-details {
  display: flex;
  flex-direction: column;
  gap: 6px;
  font-size: 13px;
  color: var(--text-mid);
}

@media (max-width: 768px) {
  .summary-grid { grid-template-columns: 1fr 1fr; }
}
@media (max-width: 480px) {
  .summary-grid { grid-template-columns: 1fr; }
}
</style>
