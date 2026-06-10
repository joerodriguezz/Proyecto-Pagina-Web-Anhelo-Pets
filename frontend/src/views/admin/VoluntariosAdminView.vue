<script setup>
import { ref, computed } from 'vue'
import Icon from '../../components/Icon.vue'

const voluntarios = ref([])
const filtroEstado = ref('Todos')

function cargarVoluntarios() {
  const usuarios =
    JSON.parse(localStorage.getItem('anhelo_usuarios')) || []

  voluntarios.value = usuarios.filter(
    u => u.solicitudVoluntario
  )
}

cargarVoluntarios()

const voluntariosFiltrados = computed(() => {
  if (filtroEstado.value === 'Todos') {
    return voluntarios.value
  }

  return voluntarios.value.filter(
    v =>
      v.solicitudVoluntario?.estado ===
      filtroEstado.value
  )
})

function aprobarSolicitud(usuario) {
  const usuarios =
    JSON.parse(localStorage.getItem('anhelo_usuarios')) || []

  const index = usuarios.findIndex(
    u => u.id === usuario.id
  )

  if (index !== -1) {
    usuarios[index].solicitudVoluntario.estado =
      'Aprobada'

    usuarios[index].rol = 'Voluntario'

    usuarios[index].tipoVoluntario =
      usuarios[index].solicitudVoluntario.tipo

    localStorage.setItem(
      'anhelo_usuarios',
      JSON.stringify(usuarios)
    )
  }

  cargarVoluntarios()
}

function rechazarSolicitud(usuario) {
  const usuarios =
    JSON.parse(localStorage.getItem('anhelo_usuarios')) || []

  const index = usuarios.findIndex(
    u => u.id === usuario.id
  )

  if (index !== -1) {
    usuarios[index].solicitudVoluntario.estado =
      'Rechazada'

    usuarios[index].rol = 'Usuario'

    localStorage.setItem(
      'anhelo_usuarios',
      JSON.stringify(usuarios)
    )
  }

  cargarVoluntarios()
}

function desactivarVoluntario(usuario) {
  const usuarios =
    JSON.parse(localStorage.getItem('anhelo_usuarios')) || []

  const index = usuarios.findIndex(
    u => u.id === usuario.id
  )

  if (index !== -1) {
    usuarios[index].solicitudVoluntario.estado =
      'Inactivo'

    localStorage.setItem(
      'anhelo_usuarios',
      JSON.stringify(usuarios)
    )
  }

  cargarVoluntarios()
}
</script>

<template>
  <div class="view-container">

    <header class="page-header">
      <div>
        <h1 class="admin-page-title">
          Voluntarios
        </h1>

        <p class="admin-page-sub">
          Gestión de solicitudes y voluntarios
        </p>
      </div>
    </header>

    <div class="tabs">
      <button
        class="tab-btn"
        :class="{ active: filtroEstado === 'Todos' }"
        @click="filtroEstado = 'Todos'"
      >
        Todos
      </button>

      <button
        class="tab-btn"
        :class="{ active: filtroEstado === 'Pendiente' }"
        @click="filtroEstado = 'Pendiente'"
      >
        Pendientes
      </button>

      <button
        class="tab-btn"
        :class="{ active: filtroEstado === 'Aprobada' }"
        @click="filtroEstado = 'Aprobada'"
      >
        Aprobados
      </button>

      <button
        class="tab-btn"
        :class="{ active: filtroEstado === 'Rechazada' }"
        @click="filtroEstado = 'Rechazada'"
      >
        Rechazados
      </button>

      <button
        class="tab-btn"
        :class="{ active: filtroEstado === 'Inactivo' }"
        @click="filtroEstado = 'Inactivo'"
      >
        Inactivos
      </button>
    </div>

    <div class="table-wrapper">
      <table class="data-table">

        <thead>
          <tr>
            <th>ID</th>
            <th>Nombre</th>
            <th>Correo</th>
            <th>Teléfono</th>
            <th>Tipo</th>
            <th>Estado</th>
            <th class="text-right">
              Acciones
            </th>
          </tr>
        </thead>

        <tbody>

          <tr
            v-if="voluntariosFiltrados.length === 0"
          >
            <td
              colspan="7"
              class="empty-row"
            >
              No hay registros
            </td>
          </tr>

          <tr
            v-for="v in voluntariosFiltrados"
            :key="v.id"
          >
            <td>
              <span class="id-code">
                {{ v.id }}
              </span>
            </td>

            <td class="font-semibold">
              {{
                v.solicitudVoluntario?.nombre ||
                v.nombre
              }}
            </td>

            <td class="text-email">
              {{
                v.solicitudVoluntario?.correo ||
                v.correo
              }}
            </td>

            <td>
              {{
                v.solicitudVoluntario?.telefono ||
                '—'
              }}
            </td>

            <td>
              <span class="badge badge-type">
                {{
                  v.solicitudVoluntario?.tipo ||
                  '—'
                }}
              </span>
            </td>

            <td>
              <span
                class="badge"
                :class="
                  v.solicitudVoluntario?.estado === 'Aprobada'
                    ? 'badge-green'
                    : v.solicitudVoluntario?.estado === 'Rechazada'
                    ? 'badge-red'
                    : v.solicitudVoluntario?.estado === 'Inactivo'
                    ? 'badge-gray'
                    : 'badge-peach'
                "
              >
                {{
                  v.solicitudVoluntario?.estado
                }}
              </span>
            </td>

            <td>

              <div
                v-if="
                  v.solicitudVoluntario?.estado ===
                  'Pendiente'
                "
                class="action-btns"
              >
                <button
                  class="approve-btn"
                  @click="aprobarSolicitud(v)"
                >
                  Aprobar
                </button>

                <button
                  class="reject-btn"
                  @click="rechazarSolicitud(v)"
                >
                  Rechazar
                </button>
              </div>

              <div
                v-else
                class="action-btns"
              >
                <button
                  class="action-btn"
                  title="Ver"
                >
                  <Icon name="Show" />
                </button>

                <button
                  class="action-btn"
                  title="Editar"
                >
                  <Icon name="Edit" />
                </button>

                <button
                  v-if="
                    v.solicitudVoluntario?.estado ===
                    'Aprobada'
                  "
                  class="action-btn disable-vol"
                  @click="desactivarVoluntario(v)"
                >
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
.view-container {
  background: transparent;
}

.page-header {
  margin-bottom: 20px;
}

.admin-page-title {
  font-size: 28px;
  font-weight: 800;
  color: #3A473C;
}

.admin-page-sub {
  color: #6C756D;
  margin-top: 4px;
}

.tabs {
  display: flex;
  gap: 10px;
  margin-bottom: 20px;
  flex-wrap: wrap;
}

.tab-btn {
  border: none;
  background: #F4F6F4;
  padding: 10px 14px;
  border-radius: 10px;
  cursor: pointer;
  font-weight: 700;
  color: #6C756D;
}

.tab-btn.active {
  background: #92A894;
  color: white;
}

.table-wrapper {
  background: white;
  border-radius: 24px;
  padding: 24px;
  overflow-x: auto;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
}

.data-table th {
  text-align: left;
  padding-bottom: 16px;
  color: #6C756D;
  font-size: 13px;
}

.data-table td {
  padding: 16px 0;
  border-top: 1px solid #F4F6F4;
}

.id-code {
  font-size: 12px;
  font-family: monospace;
  background: #F4F6F4;
  padding: 4px 8px;
  border-radius: 8px;
}

.font-semibold {
  font-weight: 600;
}

.text-email {
  color: #6C756D;
}

.empty-row {
  text-align: center;
  padding: 30px 0;
  color: #6C756D;
}

.badge {
  padding: 6px 12px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 700;
}

.badge-type {
  background: rgba(249,193,122,.2);
  color: #D18C3A;
}

.badge-green {
  background: rgba(146,168,148,.2);
  color: #5A6E5C;
}

.badge-red {
  background: rgba(235,119,119,.15);
  color: #C45252;
}

.badge-gray {
  background: #F4F6F4;
  color: #6C756D;
}

.badge-peach {
  background: rgba(249,193,122,.2);
  color: #D18C3A;
}

.action-btns {
  display: flex;
  gap: 8px;
}

.approve-btn {
  border: none;
  background: rgba(146,168,148,.2);
  color: #5A6E5C;
  padding: 8px 12px;
  border-radius: 10px;
  cursor: pointer;
  font-weight: 700;
}

.reject-btn {
  border: none;
  background: rgba(235,119,119,.15);
  color: #C45252;
  padding: 8px 12px;
  border-radius: 10px;
  cursor: pointer;
  font-weight: 700;
}

.action-btn {
  width: 34px;
  height: 34px;
  border: 2px solid #F4F6F4;
  border-radius: 10px;
  background: white;
  cursor: pointer;
}

.disable-vol:hover {
  color: #C45252;
}
</style>