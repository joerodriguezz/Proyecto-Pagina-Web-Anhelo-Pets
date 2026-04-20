<script setup>
import { ref } from 'vue'
import Icon from '../../components/Icon.vue'

const showForm = ref(false)
const editMode = ref(false)

const pets = ref([
  { id:1, name:'Luna',   type:'Perro', breed:'Mestiza',        age:'2 años',  sex:'Hembra', status:'Disponible', health:'Saludable' },
  { id:2, name:'Mochi',  type:'Gato',  breed:'Siamés',         age:'1 año',   sex:'Hembra', status:'Disponible', health:'Saludable' },
  { id:3, name:'Rocky',  type:'Perro', breed:'Labrador',       age:'3 años',  sex:'Macho',  status:'Disponible', health:'Saludable' },
  { id:4, name:'Canela', type:'Perro', breed:'Shih Tzu',       age:'4 meses', sex:'Hembra', status:'En proceso', health:'Vacunada'  },
  { id:5, name:'Tigre',  type:'Gato',  breed:'Doméstico',      age:'5 años',  sex:'Macho',  status:'Disponible', health:'Saludable' },
  { id:6, name:'Bella',  type:'Perro', breed:'Poodle',         age:'2 años',  sex:'Hembra', status:'Disponible', health:'Saludable' },
  { id:7, name:'Max',    type:'Perro', breed:'Golden Retriever',age:'1 año',  sex:'Macho',  status:'Adoptada',   health:'Saludable' },
])

const statusColor = s => ({ 'Disponible': 'badge-green', 'En proceso': 'badge-yellow', 'Adoptada': 'badge-blue' }[s] || 'badge-gray')
</script>

<template>
  <div>
    <div class="page-header">
      <div>
        <h1 class="admin-page-title">🐾 Gestión de Mascotas</h1>
        <p class="admin-page-sub">Registro y control de animales de la fundación</p>
      </div>
      <button class="btn-primary" @click="showForm = !showForm; editMode = false">
        {{ showForm ? '✕ Cancelar' : '＋ Nueva mascota' }}
      </button>
    </div>

    <!-- Form -->
    <Transition name="slide-down">
      <div v-if="showForm" class="form-panel">
        <h3>{{ editMode ? 'Editar mascota' : 'Registrar nueva mascota' }}</h3>
        <div class="form-grid">
          <div class="form-group"><label class="form-label">Nombre *</label><input class="form-input" placeholder="Nombre de la mascota" /></div>
          <div class="form-group">
            <label class="form-label">Tipo *</label>
            <select class="form-select"><option>Perro</option><option>Gato</option></select>
          </div>
          <div class="form-group"><label class="form-label">Raza *</label><input class="form-input" placeholder="Raza" /></div>
          <div class="form-group"><label class="form-label">Edad *</label><input class="form-input" placeholder="Ej. 2 años" /></div>
          <div class="form-group">
            <label class="form-label">Sexo *</label>
            <select class="form-select"><option>Macho</option><option>Hembra</option></select>
          </div>
          <div class="form-group">
            <label class="form-label">Estado *</label>
            <select class="form-select"><option>Disponible</option><option>En proceso</option><option>Adoptada</option></select>
          </div>
          <div class="form-group full-width"><label class="form-label">Descripción</label><textarea class="form-textarea" placeholder="Descripción de la mascota..."></textarea></div>
          <div class="form-group full-width"><label class="form-label">Fotografía</label><input class="form-input" type="file" accept="image/*" /></div>
        </div>
        <div class="form-actions">
          <button class="btn-primary">💾 Guardar mascota</button>
          <button class="btn-outline-green" @click="showForm = false">Cancelar</button>
        </div>
      </div>
    </Transition>

    <!-- Table -->
    <div class="table-wrapper" style="margin-top:20px;">
      <table class="data-table">
        <thead>
          <tr>
            <th>ID</th><th>Nombre</th><th>Tipo</th><th>Raza</th><th>Edad</th><th>Sexo</th><th>Salud</th><th>Estado</th><th>Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="p in pets" :key="p.id">
            <td><code style="font-size:11px;background:var(--cream);padding:2px 6px;border-radius:4px">{{ p.id }}</code></td>
            <td><strong>{{ p.name }}</strong></td>
            <td>{{ p.type === 'Perro' ? '🐕' : '🐈' }} {{ p.type }}</td>
            <td>{{ p.breed }}</td>
            <td>{{ p.age }}</td>
            <td>{{ p.sex }}</td>
            <td><span class="badge badge-green" style="font-size:11px;">{{ p.health }}</span></td>
            <td><span class="badge" :class="statusColor(p.status)">{{ p.status }}</span></td>
            <td>
              <div class="action-btns">
                <Icon name="Edit" class="action-btn edit" @click="showForm = true; editMode = true" title="Editar" fallback="✏️" />
                <Icon name="Clipboard" class="action-btn view" title="Ver historial" fallback="📋" />
                <Icon name="Archive" class="action-btn archive" title="Archivar" fallback="📦" />
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<style scoped>
.page-header { display:flex; justify-content:space-between; align-items:flex-start; margin-bottom:24px; }
.admin-page-title { font-size:22px; font-weight:800; color:var(--text-dark); }
.admin-page-sub   { font-size:13px; color:var(--text-light); margin-top:4px; }

.form-panel {
  background:var(--white);
  border:1px solid var(--border);
  border-radius:var(--radius-md);
  padding:24px;
  margin-bottom:20px;
  box-shadow:var(--shadow-sm);
}
.form-panel h3 { font-size:16px; font-weight:700; color:var(--text-dark); margin-bottom:18px; }
.form-grid { display:grid; grid-template-columns:repeat(3,1fr); gap:14px; }
.full-width { grid-column:1/-1; }
.form-actions { display:flex; gap:12px; margin-top:18px; }

.action-btns { display:flex; gap:6px; }
.action-btn {
  width:30px; height:30px;
  border:1px solid var(--border);
  border-radius:6px;
  background:var(--white);
  cursor:pointer;
  font-size:14px;
  display:flex; align-items:center; justify-content:center;
  transition:background 0.15s;
}
.action-btn:hover { background:var(--cream); }

.slide-down-enter-active { transition:all 0.25s ease; }
.slide-down-leave-active { transition:all 0.2s ease; }
.slide-down-enter-from   { opacity:0; transform:translateY(-12px); }
.slide-down-leave-to     { opacity:0; transform:translateY(-6px); }
</style>
