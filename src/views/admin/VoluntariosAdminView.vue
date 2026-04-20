<template>
  <div>
    <div class="page-header">
      <div>
        <h1 class="admin-page-title">🤝 Voluntarios</h1>
        <p class="admin-page-sub">Gestión y seguimiento de voluntarios registrados</p>
      </div>
    </div>

    <div class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr><th>ID</th><th>Nombre</th><th>Cédula</th><th>Teléfono</th><th>Correo</th><th>Tipo</th><th>Estado</th><th>Acciones</th></tr>
        </thead>
        <tbody>
          <tr v-for="v in voluntarios" :key="v.id">
            <td><code style="font-size:11px;background:var(--cream);padding:2px 6px;border-radius:4px">{{ v.id }}</code></td>
            <td><strong>{{ v.nombre }}</strong></td>
            <td style="font-size:12px;color:var(--text-light)">{{ v.cedula }}</td>
            <td>{{ v.telefono }}</td>
            <td style="font-size:13px">{{ v.correo }}</td>
            <td><span class="badge badge-blue" style="font-size:11px">{{ v.tipo }}</span></td>
            <td><span class="badge" :class="v.activo?'badge-green':'badge-gray'">{{ v.activo?'Activo':'Inactivo' }}</span></td>
            <td>
              <div class="action-btns">
                <Icon name="Show" class="action-btn" title="Ver detalle" fallback="👁️" />
                <Icon name="Edit" class="action-btn" title="Editar" fallback="✏️" />
                <Icon name="Lock" class="action-btn" title="Desactivar" fallback="🔒" />
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
      voluntarios: [
        { id:'V-001', nombre:'Familia Mora',   cedula:'1-1111-2222', telefono:'8812-3456', correo:'mora@gmail.com',   tipo:'Casa cuna',  activo:true  },
        { id:'V-002', nombre:'Gabriela Torres',cedula:'2-2222-3333', telefono:'8723-4567', correo:'gaby@gmail.com',   tipo:'Eventos',    activo:true  },
        { id:'V-003', nombre:'Familia Vega',   cedula:'3-3333-4444', telefono:'8765-4321', correo:'vega@hotmail.com', tipo:'Casa cuna',  activo:true  },
        { id:'V-004', nombre:'Andrés Matamoros',cedula:'1-4444-5555',telefono:'8634-5678', correo:'andres@gmail.com', tipo:'Transporte', activo:true  },
        { id:'V-005', nombre:'Paula Chacón',   cedula:'4-5555-6666', telefono:'8545-6789', correo:'paula@gmail.com',  tipo:'Redes',      activo:false },
        { id:'V-006', nombre:'Familia Salas',  cedula:'2-6666-7777', telefono:'8900-1122', correo:'salas@gmail.com',  tipo:'Casa cuna',  activo:true  },
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
