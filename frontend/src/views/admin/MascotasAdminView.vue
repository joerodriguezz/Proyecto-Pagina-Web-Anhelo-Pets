<script setup>
import { ref } from 'vue'
import Icon from '../../components/Icon.vue'

const showForm = ref(false)
const editMode = ref(false)

const pets = ref([
  { id:1, name:'Luna',   type:'Perro', breed:'Mestiza',          age:'2 años',  sex:'Hembra', status:'Disponible', health:'Saludable' },
  { id:2, name:'Mochi',  type:'Gato',  breed:'Siamés',           age:'1 año',   sex:'Hembra', status:'Disponible', health:'Saludable' },
  { id:3, name:'Rocky',  type:'Perro', breed:'Labrador',         age:'3 años',  sex:'Macho',  status:'Disponible', health:'Saludable' },
  { id:4, name:'Canela', type:'Perro', breed:'Shih Tzu',         age:'4 meses', sex:'Hembra', status:'En proceso', health:'Vacunada'  },
  { id:5, name:'Tigre',  type:'Gato',  breed:'Doméstico',        age:'5 años',  sex:'Macho',  status:'Disponible', health:'Saludable' },
  { id:6, name:'Bella',  type:'Perro', breed:'Poodle',           age:'2 años',  sex:'Hembra', status:'Disponible', health:'Saludable' },
  { id:7, name:'Max',    type:'Perro', breed:'Golden Retriever', age:'1 año',   sex:'Macho',  status:'Adoptada',   health:'Saludable' },
])

const statusColor = s => ({ 
  'Disponible': 'badge-green', 
  'En proceso': 'badge-peach', 
  'Adoptada': 'badge-blue' 
}[s] || 'badge-gray')
</script>

<template>
  <div class="view-container">
    <header class="page-header">
      <div>
        <h1 class="admin-page-title">Gestión de Mascotas</h1>
        <p class="admin-page-sub">Registro y control de animales de la fundación</p>
      </div>
      <button class="btn-toggle-form" :class="{ 'btn-cancel': showForm }" @click="showForm = !showForm; editMode = false">
        {{ showForm ? 'Cancelar' : 'Nueva mascota' }}
      </button>
    </header>

    <Transition name="slide-down">
      <div v-if="showForm" class="form-panel">
        <h3>{{ editMode ? 'Editar mascota' : 'Registrar nueva mascota' }}</h3>
        
        <form class="form-grid" @submit.prevent>
          <div class="form-group">
            <label>Nombre *</label>
            <input placeholder="Nombre de la mascota" class="custom-input" />
          </div>
          <div class="form-group">
            <label>Tipo *</label>
            <select class="custom-select">
              <option>Perro</option>
              <option>Gato</option>
            </select>
          </div>
          <div class="form-group">
            <label>Raza *</label>
            <input placeholder="Raza" class="custom-input" />
          </div>
          <div class="form-group">
            <label>Edad *</label>
            <input placeholder="Ej. 2 años" class="custom-input" />
          </div>
          <div class="form-group">
            <label>Sexo *</label>
            <select class="custom-select">
              <option>Macho</option>
              <option>Hembra</option>
            </select>
          </div>
          <div class="form-group">
            <label>Estado *</label>
            <select class="custom-select">
              <option>Disponible</option>
              <option>En proceso</option>
              <option>Adoptada</option>
            </select>
          </div>
          <div class="form-group full-width">
            <label>Descripción</label>
            <textarea placeholder="Descripción de la mascota..." class="custom-textarea"></textarea>
          </div>
        </form>
        
        <div class="form-actions">
          <button class="btn-save">Guardar mascota</button>
          <button class="btn-discard" @click="showForm = false">Cancelar</button>
        </div>
      </div>
    </Transition>

    <div class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Nombre</th>
            <th>Tipo</th>
            <th>Raza</th>
            <th>Edad</th>
            <th>Sexo</th>
            <th>Salud</th>
            <th>Estado</th>
            <th class="text-right">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="p in pets" :key="p.id">
            <td><span class="id-code">{{ p.id }}</span></td>
            <td class="font-semibold">{{ p.name }}</td>
            <td>{{ p.type }}</td>
            <td class="text-secondary">{{ p.breed }}</td>
            <td>{{ p.age }}</td>
            <td>{{ p.sex }}</td>
            <td><span class="badge badge-health">{{ p.health }}</span></td>
            <td><span class="badge" :class="statusColor(p.status)">{{ p.status }}</span></td>
            <td>
              <div class="action-btns">
                <button class="action-btn" @click="showForm = true; editMode = true" title="Editar">
                  <Icon name="Edit" />
                </button>
                <button class="action-btn" title="Ver historial">
                  <Icon name="Clipboard" />
                </button>
                <button class="action-btn archive-btn" title="Archivar">
                  <Icon name="Archive" />
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
/* ── Estructura General ── */
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
  grid-template-columns: repeat(3, 1fr); 
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

.badge-green  { background: rgba(146, 168, 148, 0.2); color: #5A6E5C; }
.badge-peach  { background: rgba(249, 193, 122, 0.2); color: #D18C3A; }
.badge-blue   { background: rgba(130, 160, 180, 0.15); color: #4A6070; }
.badge-gray   { background: #F4F6F4; color: #6C756D; }
.badge-health { background: #EDF1EE; color: #5A6E5C; font-size: 11px; }

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
  background: #F4F6F4;
  border-color: #6C756D;
  color: #3A473C;
  transform: translateY(-1px);
}

.action-btn.archive-btn:hover {
  background: rgba(249, 193, 122, 0.15);
  border-color: #F9C17A;
  color: #D18C3A;
}

/* Animación */
.slide-down-enter-active { transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1); }
.slide-down-leave-active { transition: all 0.2s ease; }
.slide-down-enter-from   { opacity: 0; transform: translateY(-12px); }
.slide-down-leave-to     { opacity: 0; transform: translateY(-6px); }

/* Ajustes Responsivos */
@media (max-width: 992px) {
  .form-grid { grid-template-columns: 1fr 1fr; }
}

@media (max-width: 768px) {
  .page-header { flex-direction: column; align-items: flex-start; gap: 14px; }
  .btn-toggle-form { width: 100%; }
  .form-grid { grid-template-columns: 1fr; gap: 0; }
}
</style>