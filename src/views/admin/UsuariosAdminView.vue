<template>
  <div>
    <div class="page-header">
      <div>
        <h1 class="admin-page-title">👥 Gestión de Usuarios</h1>
        <p class="admin-page-sub">Control de cuentas y roles del sistema</p>
      </div>
    </div>

    <div class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr><th>ID</th><th>Nombre</th><th>Cédula</th><th>Correo</th><th>Teléfono</th><th>Rol</th><th>Estado</th><th>Acciones</th></tr>
        </thead>
        <tbody>
          <tr v-for="u in usuarios" :key="u.id">
            <td><code style="font-size:11px;background:var(--cream);padding:2px 6px;border-radius:4px">{{ u.id }}</code></td>
            <td><strong>{{ u.nombre }}</strong></td>
            <td style="font-size:12px;color:var(--text-light)">{{ u.cedula }}</td>
            <td style="font-size:13px">{{ u.correo }}</td>
            <td>{{ u.telefono }}</td>
            <td><span class="badge" :class="u.rol==='Admin'?'badge-blue':'badge-gray'">{{ u.rol }}</span></td>
            <td><span class="badge" :class="u.activo?'badge-green':'badge-red'">{{ u.activo ? 'Activo' : 'Inactivo' }}</span></td>
            <td>
              <div class="action-btns">
                <Icon name="Edit" class="action-btn" title="Editar" fallback="✏️" />
                <Icon name="Key" class="action-btn" title="Cambiar rol" fallback="🔑" />
                <Icon :name="u.activo ? 'Lock' : 'Unlock'" class="action-btn" :title="u.activo?'Desactivar':'Activar'" :fallback="u.activo ? '🔒' : '🔓'" />
              </div>
            </td>
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
      usuarios: [
        { id:'U-001', nombre:'Shirley Valverde', cedula:'1-0932-0528', correo:'shirley@anhelopets.cr', telefono:'8840-3334', rol:'Admin',   activo:true  },
        { id:'U-002', nombre:'Ana Rodríguez',    cedula:'1-1234-5678', correo:'ana@gmail.com',          telefono:'8812-1234', rol:'Usuario', activo:true  },
        { id:'U-003', nombre:'Carlos Mora',      cedula:'2-2345-6789', correo:'carlos@outlook.com',     telefono:'8901-2345', rol:'Usuario', activo:true  },
        { id:'U-004', nombre:'Sofía Vega',       cedula:'3-3456-7890', correo:'sofia@gmail.com',        telefono:'8723-4567', rol:'Usuario', activo:true  },
        { id:'U-005', nombre:'Diego Salas',      cedula:'1-4567-8901', correo:'diego@hotmail.com',      telefono:'8634-5678', rol:'Usuario', activo:false },
        { id:'U-006', nombre:'Laura Jiménez',    cedula:'4-5678-9012', correo:'laura@gmail.com',        telefono:'8545-6789', rol:'Usuario', activo:true  },
      ]
    }
  }
}
</script>

<style scoped>
.page-header { display:flex; justify-content:space-between; align-items:flex-start; margin-bottom:24px; }
.admin-page-title { font-size:22px; font-weight:800; }
.admin-page-sub   { font-size:13px; color:var(--text-light); margin-top:4px; }
.action-btns { display:flex; gap:6px; }
.action-btn { width:30px; height:30px; border:1px solid var(--border); border-radius:6px; background:var(--white); cursor:pointer; font-size:14px; display:flex; align-items:center; justify-content:center; }
.action-btn:hover { background:var(--cream); }
</style>
