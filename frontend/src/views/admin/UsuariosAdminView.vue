<script setup>
import { ref } from 'vue'
import Icon from '../../components/Icon.vue'

const usuarios = ref([
  { id:'U-001', nombre:'Shirley Valverde', cedula:'1-0932-0528', correo:'shirley@anhelopets.cr', telefono:'8840-3334', rol:'Admin',   activo:true  },
  { id:'U-002', nombre:'Ana Rodríguez',    cedula:'1-1234-5678', correo:'ana@gmail.com',          telefono:'8812-1234', rol:'Usuario', activo:true  },
  { id:'U-003', nombre:'Carlos Mora',      cedula:'2-2345-6789', correo:'carlos@outlook.com',     telefono:'8901-2345', rol:'Usuario', activo:true  },
  { id:'U-004', nombre:'Sofía Vega',       cedula:'3-3456-7890', correo:'sofia@gmail.com',        telefono:'8723-4567', rol:'Usuario', activo:true  },
  { id:'U-005', nombre:'Diego Salas',      cedula:'1-4567-8901', correo:'diego@hotmail.com',      telefono:'8634-5678', rol:'Usuario', activo:false },
  { id:'U-006', nombre:'Laura Jiménez',    cedula:'4-5678-9012', correo:'laura@gmail.com',        telefono:'8545-6789', rol:'Usuario', activo:true  },
])
</script>

<template>
  <div class="view-container">
    <header class="page-header">
      <div>
        <h1 class="admin-page-title">Gestión de Usuarios</h1>
        <p class="admin-page-sub">Control de cuentas y roles del sistema</p>
      </div>
    </header>

    <div class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Nombre</th>
            <th>Cédula</th>
            <th>Correo</th>
            <th>Teléfono</th>
            <th>Rol</th>
            <th>Estado</th>
            <th class="text-right">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="u in usuarios" :key="u.id">
            <td><span class="id-code">{{ u.id }}</span></td>
            <td class="font-semibold">{{ u.nombre }}</td>
            <td class="text-secondary">{{ u.cedula }}</td>
            <td class="text-email">{{ u.correo }}</td>
            <td>{{ u.telefono }}</td>
            <td>
              <span class="badge" :class="u.rol === 'Admin' ? 'badge-peach' : 'badge-gray'">
                {{ u.rol }}
              </span>
            </td>
            <td>
              <span class="badge" :class="u.activo ? 'badge-green' : 'badge-red'">
                {{ u.activo ? 'Activo' : 'Inactivo' }}
              </span>
            </td>
            <td>
              <div class="action-btns">
                <button class="action-btn" title="Editar">
                  <Icon name="Edit" />
                </button>
                <button class="action-btn" title="Cambiar rol">
                  <Icon name="Key" />
                </button>
                <button class="action-btn status-toggle" :class="{ 'is-active': u.activo }" :title="u.activo ? 'Desactivar' : 'Activar'">
                  <Icon :name="u.activo ? 'Lock' : 'Unlock'" />
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
/* ── Contenedor General ── */
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
.text-email { font-size: 13px; color: #3A473C; }
.text-right { text-align: right; }

/* ── Badges de Estado y Roles ── */
.badge {
  padding: 6px 12px;
  border-radius: 10px;
  font-size: 12px;
  font-weight: 700;
  display: inline-block;
}

.badge-peach  { background: rgba(249, 193, 122, 0.2); color: #D18C3A; } /* Tono de acento AnheloPets */
.badge-gray   { background: #F4F6F4; color: #6C756D; }
.badge-green  { background: rgba(146, 168, 148, 0.2); color: #5A6E5C; }
.badge-red    { background: rgba(235, 119, 119, 0.15); color: #C45252; }

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

/* Transiciones específicas de bloqueo/desbloqueo */
.action-btn.status-toggle:hover {
  background: rgba(249, 193, 122, 0.15);
  border-color: #F9C17A;
  color: #D18C3A;
}

.action-btn.status-toggle.is-active:hover {
  background: rgba(235, 119, 119, 0.1);
  border-color: #EB7777;
  color: #C45252;
}

/* Ajustes Responsivos */
@media (max-width: 768px) {
  .page-header { flex-direction: column; align-items: flex-start; gap: 8px; }
}
</style>