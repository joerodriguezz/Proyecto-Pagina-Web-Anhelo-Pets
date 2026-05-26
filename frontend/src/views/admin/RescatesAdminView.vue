<script setup>
import { ref } from 'vue'
import Icon from '../../components/Icon.vue'

const showForm = ref(false)

const rescates = ref([
  { id:'R-001', mascota:'Luna',   fecha:'2026-03-15', ubicacion:'Desamparados, SJ',  casaCuna:'Familia Mora',  estado:'Activo'  },
  { id:'R-002', mascota:'Mochi',  fecha:'2026-03-08', ubicacion:'Cartago Centro',    casaCuna:'Familia Vega',  estado:'Activo'  },
  { id:'R-003', mascota:'Rocky',  fecha:'2026-02-22', ubicacion:'Heredia, Barreal',  casaCuna:'Familia Salas', estado:'Activo'  },
  { id:'R-004', mascota:'Canela', fecha:'2026-02-10', ubicacion:'Alajuela Centro',   casaCuna:'Familia Pérez', estado:'Activo'  },
  { id:'R-005', mascota:'Max',    fecha:'2026-01-30', ubicacion:'Goicoechea, SJ',    casaCuna:'—',             estado:'Cerrado' },
])
</script>

<template>
  <div class="view-container">
    <header class="page-header">
      <div>
        <h1 class="admin-page-title">Gestión de Rescates</h1>
        <p class="admin-page-sub">Registro y seguimiento de animales rescatados</p>
      </div>
      <button class="btn-toggle-form" :class="{ 'btn-cancel': showForm }" @click="showForm = !showForm">
        {{ showForm ? 'Cancelar' : 'Nuevo rescate' }}
      </button>
    </header>

    <Transition name="slide-down">
      <div v-if="showForm" class="form-panel">
        <h3>Registrar nuevo rescate</h3>
        
        <form class="form-grid" @submit.prevent>
          <div class="form-group">
            <label>Mascota asociada *</label>
            <select class="custom-select">
              <option>Seleccionar mascota</option>
              <option>Luna</option>
              <option>Rocky</option>
              <option>Mochi</option>
            </select>
          </div>
          
          <div class="form-group">
            <label>Fecha de rescate *</label>
            <input type="date" class="custom-input" />
          </div>
          
          <div class="form-group full-width">
            <label>Ubicación *</label>
            <input placeholder="Cantón, distrito, señas exactas..." class="custom-input" />
          </div>
          
          <div class="form-group full-width">
            <label>Descripción *</label>
            <textarea placeholder="Descripción de las condiciones del rescate..." class="custom-textarea"></textarea>
          </div>
          
          <div class="form-group">
            <label>Casa cuna asignada</label>
            <select class="custom-select">
              <option>Sin asignar</option>
              <option>Familia Mora</option>
              <option>Familia Vega</option>
            </select>
          </div>
          
          <div class="form-group">
            <label>Estado</label>
            <select class="custom-select">
              <option>Activo</option>
              <option>Cerrado</option>
            </select>
          </div>
        </form>

        <div class="form-actions">
          <button class="btn-save">Guardar rescate</button>
          <button class="btn-discard" @click="showForm = false">Descartar</button>
        </div>
      </div>
    </Transition>

    <div class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Mascota</th>
            <th>Fecha</th>
            <th>Ubicación</th>
            <th>Casa cuna</th>
            <th>Estado</th>
            <th class="text-right">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="r in rescates" :key="r.id">
            <td><span class="id-code">{{ r.id }}</span></td>
            <td class="font-semibold">{{ r.mascota }}</td>
            <td class="text-secondary">{{ r.fecha }}</td>
            <td class="text-location">{{ r.ubicacion }}</td>
            <td class="font-medium">{{ r.casaCuna }}</td>
            <td>
              <span class="badge" :class="r.estado === 'Activo' ? 'badge-green' : 'badge-gray'">
                {{ r.estado }}
              </span>
            </td>
            <td>
              <div class="action-btns">
                <button class="action-btn" title="Editar">
                  <Icon name="Edit" />
                </button>
                <button class="action-btn" title="Ver historial">
                  <Icon name="Clipboard" />
                </button>
                <button class="action-btn close-rescue" title="Cerrar rescate">
                  <Icon name="Lock" />
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
/* ── Encabezado de la Vista ── */
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

/* Botón disparador del formulario */
.btn-toggle-form {
  padding: 12px 24px;
  border-radius: 14px;
  border: none;
  background-color: #92A894;
  color: white;
  font-size: 14px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.25s ease;
}

.btn-toggle-form:hover {
  background-color: #7C927E;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(146, 168, 148, 0.15);
}

.btn-toggle-form.btn-cancel {
  background-color: #F4F6F4;
  color: #6C756D;
}

.btn-toggle-form.btn-cancel:hover {
  background-color: #EBEFEA;
  color: #3A473C;
  box-shadow: none;
}

/* ── Panel del Formulario ── */
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

.btn-discard {
  padding: 13px 24px;
  border-radius: 12px;
  border: 2px solid #F4F6F4;
  background-color: transparent;
  color: #6C756D;
  font-size: 14px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s ease;
}

.btn-discard:hover {
  background-color: #F4F6F4;
  color: #3A473C;
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
.font-medium { font-weight: 500; }
.text-secondary { font-size: 13px; color: #6C756D; }
.text-location { font-size: 13px; color: #3A473C; }
.text-right { text-align: right; }

/* Badges de Estado */
.badge {
  padding: 6px 12px;
  border-radius: 10px;
  font-size: 12px;
  font-weight: 700;
  display: inline-block;
}

.badge-green { background: rgba(146, 168, 148, 0.2); color: #5A6E5C; }
.badge-gray  { background: #F4F6F4; color: #6C756D; }

/* Botones de Acciones */
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
  background: #F4F6F4;
  border-color: #6C756D;
  color: #3A473C;
  transform: translateY(-1px);
}

.action-btn.close-rescue:hover {
  background: rgba(235, 119, 119, 0.1);
  border-color: #EB7777;
  color: #C45252;
}

/* ── Animaciones Desplegables ── */
.slide-down-enter-active { transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1); }
.slide-down-leave-active { transition: all 0.2s ease; }
.slide-down-enter-from   { opacity: 0; transform: translateY(-12px); }
.slide-down-leave-to     { opacity: 0; transform: translateY(-6px); }

/* Ajustes Responsivos */
@media (max-width: 768px) {
  .page-header { flex-direction: column; align-items: flex-start; gap: 14px; }
  .btn-toggle-form { width: 100%; }
  .form-grid { grid-template-columns: 1fr; gap: 0; }
}
</style>