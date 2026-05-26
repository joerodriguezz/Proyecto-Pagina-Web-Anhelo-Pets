<script setup>
import { ref } from 'vue'
import Icon from '../../components/Icon.vue'

const voluntarios = ref([
  { id:'V-001', nombre:'Familia Mora',   cedula:'1-1111-2222', telefono:'8812-3456', correo:'mora@gmail.com',   tipo:'Casa cuna',  activo:true  },
  { id:'V-002', nombre:'Gabriela Torres',cedula:'2-2222-3333', telefono:'8723-4567', correo:'gaby@gmail.com',   tipo:'Eventos',    activo:true  },
  { id:'V-003', nombre:'Familia Vega',   cedula:'3-3333-4444', telefono:'8765-4321', correo:'vega@hotmail.com', tipo:'Casa cuna',  activo:true  },
  { id:'V-004', nombre:'Andrés Matamoros',cedula:'1-4444-5555',telefono:'8634-5678', correo:'andres@gmail.com', tipo:'Transporte', activo:true  },
  { id:'V-005', nombre:'Paula Chacón',   cedula:'4-5555-6666', telefono:'8545-6789', correo:'paula@gmail.com',  tipo:'Redes',      activo:false },
  { id:'V-006', nombre:'Familia Salas',  cedula:'2-6666-7777', telefono:'8900-1122', correo:'salas@gmail.com',  tipo:'Casa cuna',  activo:true  },
])
</script>

<template>
  <div class="view-container">
    <header class="page-header">
      <div>
        <h1 class="admin-page-title">Voluntarios</h1>
        <p class="admin-page-sub">Gestión y seguimiento de voluntarios registrados</p>
      </div>
    </header>

    <div class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Nombre</th>
            <th>Cédula</th>
            <th>Teléfono</th>
            <th>Correo</th>
            <th>Tipo</th>
            <th>Estado</th>
            <th class="text-right">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="v in voluntarios" :key="v.id">
            <td><span class="id-code">{{ v.id }}</span></td>
            <td class="font-semibold">{{ v.nombre }}</td>
            <td class="text-secondary">{{ v.cedula }}</td>
            <td>{{ v.telefono }}</td>
            <td class="text-email">{{ v.correo }}</td>
            <td>
              <span class="badge badge-type">
                {{ v.tipo }}
              </span>
            </td>
            <td>
              <span class="badge" :class="v.activo ? 'badge-green' : 'badge-gray'">
                {{ v.activo ? 'Activo' : 'Inactivo' }}
              </span>
            </td>
            <td>
              <div class="action-btns">
                <button class="action-btn" title="Ver detalle">
                  <Icon name="Show" />
                </button>
                <button class="action-btn" title="Editar">
                  <Icon name="Edit" />
                </button>
                <button class="action-btn disable-vol" title="Desactivar">
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

/* ── Badges de Estado y Categorías ── */
.badge {
  padding: 6px 12px;
  border-radius: 10px;
  font-size: 12px;
  font-weight: 700;
  display: inline-block;
}

.badge-type {
  background: rgba(249, 193, 122, 0.2); /* Tono durazno para categorizar tipos */
  color: #D18C3A;
  padding: 6px 12px;
  border-radius: 10px;
  font-size: 11px;
  font-weight: 700;
}

.badge-green  { background: rgba(146, 168, 148, 0.2); color: #5A6E5C; }
.badge-gray   { background: #F4F6F4; color: #6C756D; }

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

.action-btn.disable-vol:hover {
  background: rgba(235, 119, 119, 0.1);
  border-color: #EB7777;
  color: #C45252;
}

/* Ajustes Responsivos */
@media (max-width: 768px) {
  .page-header { flex-direction: column; align-items: flex-start; gap: 8px; }
}
</style>