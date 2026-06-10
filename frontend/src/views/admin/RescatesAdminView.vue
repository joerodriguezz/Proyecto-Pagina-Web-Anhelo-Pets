<script setup>
import { ref, computed, watch } from 'vue'
import Icon from '../../components/Icon.vue'

import {
  ubicacionesCR
} from '../../data/ubicaciones'

/* =========================
   RESCATES
========================= */

const rescates = ref([])

/* =========================
   UI
========================= */

const showForm = ref(false)

const editMode = ref(false)

const rescueIndex = ref(null)

const successMessage = ref(false)

const errorMessage = ref(false)

const showDetailModal = ref(false)

const rescueSelected = ref(null)

const filtroEstado = ref('Todos')

/* =========================
   USUARIO ACTUAL
========================= */

const usuarioActual = ref({

  nombre:'Shirley Valverde',
  rol:'Admin'

})

/* =========================
   VOLUNTARIOS
========================= */

const voluntarios = ref(

  JSON.parse(
    localStorage.getItem(
      'anhelo_voluntarios'
    )
  ) || []

)

/* =========================
   CASAS CUNA
========================= */

const casasCunaDisponibles =
computed(() => {

  return voluntarios.value.filter(v =>

    v.activo && (

      v.tipo === 'Casa cuna' ||
      v.tipo === 'Casa Cuna'

    )

  )

})

/* =========================
   RESCATISTAS
========================= */

const rescatistasDisponibles =
computed(() => {

  return voluntarios.value.filter(v =>

    v.activo &&
    v.tipo === 'Rescatista'

  )

})

/* =========================
   UBICACIONES
========================= */

const provincia = ref('')
const canton = ref('')
const distrito = ref('')

const provincias = Object.keys(
  ubicacionesCR
)

const cantonesDisponibles =
computed(() => {

  if (!provincia.value) {

    return []

  }

  return Object.keys(

    ubicacionesCR[
      provincia.value
    ]

  )

})

const distritosDisponibles =
computed(() => {

  if (
    !provincia.value ||
    !canton.value
  ) {

    return []

  }

  return ubicacionesCR[
    provincia.value
  ][canton.value]

})

watch(provincia, () => {

  if (!editMode.value) {

    canton.value = ''
    distrito.value = ''

  }

})

watch(canton, () => {

  if (!editMode.value) {

    distrito.value = ''

  }

})

/* =========================
   FILTRO
========================= */

const rescatesFiltrados =
computed(() => {

  if (
    filtroEstado.value === 'Todos'
  ) {

    return rescates.value

  }

  return rescates.value.filter(r =>

    r.estado === filtroEstado.value

  )

})

/* =========================
   FORMULARIO
========================= */

const mascota = ref('')

const edad = ref('')

const sexo = ref('')

const tieneRaza = ref('No')

const raza = ref('')

const fechaRescate = ref('')

const descripcion = ref('')

const casaCuna = ref('')

const rescatista = ref('')

const estado = ref('Activo')

/* =========================
   TOAST
========================= */

function mostrarToast() {

  successMessage.value = true

  setTimeout(() => {

    successMessage.value = false

  }, 2400)

}

function mostrarError() {

  errorMessage.value = true

  setTimeout(() => {

    errorMessage.value = false

  }, 2400)

}

/* =========================
   FECHA CREACION
========================= */

function obtenerFechaActual() {

  return new Date().toLocaleString(

    'es-CR',

    {

      year:'numeric',
      month:'2-digit',
      day:'2-digit',
      hour:'2-digit',
      minute:'2-digit'

    }

  )

}

/* =========================
   GUARDAR
========================= */

function guardarRescate() {

  if (

    !mascota.value ||
    !edad.value ||
    !sexo.value ||
    !fechaRescate.value ||
    !descripcion.value ||
    !provincia.value ||
    !canton.value ||
    !distrito.value

  ) {

    mostrarError()

    return

  }

  const ubicacionFinal =

    `${provincia.value} · ${canton.value} · ${distrito.value}`

  if (editMode.value) {

    rescates.value[
      rescueIndex.value
    ] = {

      ...rescates.value[
        rescueIndex.value
      ],

      mascota: mascota.value,

      edad: edad.value,

      sexo: sexo.value,

      raza:
        tieneRaza.value === 'Si'
          ? raza.value
          : 'Sin raza',

      fechaRescate:
        fechaRescate.value,

      ubicacion:
        ubicacionFinal,

      descripcion:
        descripcion.value,

      casaCuna:
        casaCuna.value || 'Sin asignar',

      rescatista:
        rescatista.value,

      estado:
        estado.value

    }

    editMode.value = false

    rescueIndex.value = null

  } else {

    const nuevo = {

      id:
        `R-${String(
          rescates.value.length + 1
        ).padStart(3, '0')}`,

      mascota:
        mascota.value,

      edad:
        edad.value,

      sexo:
        sexo.value,

      raza:
        tieneRaza.value === 'Si'
          ? raza.value
          : 'Sin raza',

      fechaRescate:
        fechaRescate.value,

      fechaCreacion:
        obtenerFechaActual(),

      creadoPor:
        usuarioActual.value.nombre,

      ubicacion:
        ubicacionFinal,

      descripcion:
        descripcion.value,

      casaCuna:
        casaCuna.value || 'Sin asignar',

      rescatista:
        rescatista.value,

      estado:
        estado.value

    }

    rescates.value.unshift(
      nuevo
    )

  }

  limpiarFormulario()

  showForm.value = false

  mostrarToast()

}

/* =========================
   EDITAR
========================= */

function editarRescate(index) {

  const r =
    rescates.value[index]

  mascota.value =
    r.mascota

  edad.value =
    r.edad

  sexo.value =
    r.sexo

  fechaRescate.value =
    r.fechaRescate

  descripcion.value =
    r.descripcion

  casaCuna.value =
    r.casaCuna

  rescatista.value =
    r.rescatista

  estado.value =
    r.estado

  if (
    r.raza &&
    r.raza !== 'Sin raza'
  ) {

    tieneRaza.value = 'Si'

    raza.value = r.raza

  } else {

    tieneRaza.value = 'No'

    raza.value = ''

  }

  const partes =
    r.ubicacion.split(' · ')

  provincia.value =
    partes[0]

  canton.value =
    partes[1]

  distrito.value =
    partes[2]

  rescueIndex.value =
    index

  editMode.value = true

  showForm.value = true

}

/* =========================
   CERRAR
========================= */

function cerrarRescate(index) {

  rescates.value[index]
    .estado = 'Cerrado'

  mostrarToast()

}

/* =========================
   VER DETALLE
========================= */

function verDetalle(rescate) {

  rescueSelected.value =
    rescate

  showDetailModal.value =
    true

}

/* =========================
   LIMPIAR
========================= */

function limpiarFormulario() {

  mascota.value = ''

  edad.value = ''

  sexo.value = ''

  tieneRaza.value = 'No'

  raza.value = ''

  fechaRescate.value = ''

  descripcion.value = ''

  casaCuna.value = ''

  rescatista.value = ''

  estado.value = 'Activo'

  provincia.value = ''

  canton.value = ''

  distrito.value = ''

}
</script>

<template>

  <div class="view-container">

    <!-- TOAST -->

    <Transition name="toast">

      <div
        v-if="successMessage"
        class="toast-success"
      >

        Cambios guardados correctamente

      </div>

    </Transition>

    <Transition name="toast">

      <div
        v-if="errorMessage"
        class="toast-error"
      >

        Completa todos los campos

      </div>

    </Transition>

    <!-- HEADER -->

    <header class="page-header">

      <h1 class="admin-page-title">
        Gestión de Rescates
      </h1>

      <button
        class="btn-toggle-form"
        @click="showForm = !showForm"
      >

        {{
          showForm
            ? 'Cerrar formulario'
            : 'Nuevo rescate'
        }}

      </button>

    </header>

    <!-- FILTROS -->

    <div class="filters-wrap">

      <button
        class="filter-btn"
        :class="{ active: filtroEstado === 'Todos' }"
        @click="filtroEstado = 'Todos'"
      >
        Todos
      </button>

      <button
        class="filter-btn"
        :class="{ active: filtroEstado === 'Activo' }"
        @click="filtroEstado = 'Activo'"
      >
        Activos
      </button>

      <button
        class="filter-btn"
        :class="{ active: filtroEstado === 'Cerrado' }"
        @click="filtroEstado = 'Cerrado'"
      >
        Cerrados
      </button>

    </div>

    <!-- FORM -->

    <Transition name="slide-down">

      <div
        v-if="showForm"
        class="form-panel"
      >

        <div class="form-grid">

          <div>

            <label class="input-label">
              Nombre mascota
            </label>

            <input
              class="custom-input"
              v-model="mascota"
            >

          </div>

          <div>

            <label class="input-label">
              Edad
            </label>

            <input
              class="custom-input"
              v-model="edad"
            >

          </div>

          <div>

            <label class="input-label">
              Sexo
            </label>

            <select
              class="custom-input"
              v-model="sexo"
            >

              <option value="">
                Seleccionar
              </option>

              <option>
                Macho
              </option>

              <option>
                Hembra
              </option>

            </select>

          </div>

          <div>

            <label class="input-label">
              ¿Tiene raza?
            </label>

            <select
              class="custom-input"
              v-model="tieneRaza"
            >

              <option>
                No
              </option>

              <option>
                Si
              </option>

            </select>

          </div>

          <div
            v-if="tieneRaza === 'Si'"
            class="full-width"
          >

            <label class="input-label">
              Raza
            </label>

            <input
              class="custom-input"
              v-model="raza"
            >

          </div>

          <div>

            <label class="input-label">
              Fecha rescate
            </label>

            <input
              type="date"
              class="custom-input"
              v-model="fechaRescate"
            >

          </div>

          <div>

            <label class="input-label">
              Provincia
            </label>

            <select
              class="custom-input"
              v-model="provincia"
            >

              <option value="">
                Seleccionar
              </option>

              <option
                v-for="p in provincias"
                :key="p"
              >

                {{ p }}

              </option>

            </select>

          </div>

          <div>

            <label class="input-label">
              Cantón
            </label>

            <select
              class="custom-input"
              v-model="canton"
            >

              <option value="">
                Seleccionar
              </option>

              <option
                v-for="c in cantonesDisponibles"
                :key="c"
              >

                {{ c }}

              </option>

            </select>

          </div>

          <div>

            <label class="input-label">
              Distrito
            </label>

            <select
              class="custom-input"
              v-model="distrito"
            >

              <option value="">
                Seleccionar
              </option>

              <option
                v-for="d in distritosDisponibles"
                :key="d"
              >

                {{ d }}

              </option>

            </select>

          </div>

          <div>

            <label class="input-label">
              Casa cuna
            </label>

            <select
              class="custom-input"
              v-model="casaCuna"
            >

              <option value="">
                Sin asignar
              </option>

              <option
                v-for="c in casasCunaDisponibles"
                :key="c.id"
                :value="c.nombre"
              >

                {{ c.nombre }}

              </option>

            </select>

          </div>

          <div>

            <label class="input-label">
              Rescatista
            </label>

            <select
              class="custom-input"
              v-model="rescatista"
            >

              <option value="">
                Seleccionar
              </option>

              <option
                v-for="r in rescatistasDisponibles"
                :key="r.id"
                :value="r.nombre"
              >

                {{ r.nombre }}

              </option>

            </select>

          </div>

          <div>

            <label class="input-label">
              Estado
            </label>

            <select
              class="custom-input"
              v-model="estado"
            >

              <option>
                Activo
              </option>

              <option>
                Cerrado
              </option>

            </select>

          </div>

          <div class="full-width">

            <label class="input-label">
              Descripción rescate
            </label>

            <textarea
              class="custom-textarea"
              v-model="descripcion"
            ></textarea>

          </div>

        </div>

        <div class="form-actions">

          <button
            class="btn-save"
            @click="guardarRescate"
          >

            {{
              editMode
                ? 'Actualizar rescate'
                : 'Guardar rescate'
            }}

          </button>

        </div>

      </div>

    </Transition>

    <!-- TABLA -->

    <div class="table-wrapper">

      <table class="data-table">

        <thead>

          <tr>

            <th>ID</th>
            <th>Mascota</th>
            <th>Rescatista</th>
            <th>Casa cuna</th>
            <th>Estado</th>
            <th>Acciones</th>

          </tr>

        </thead>

        <tbody>

          <tr
            v-for="(r,index) in rescatesFiltrados"
            :key="r.id"
          >

            <td>
              {{ r.id }}
            </td>

            <td>
              {{ r.mascota }}
            </td>

            <td>
              {{ r.rescatista }}
            </td>

            <td>
              {{ r.casaCuna }}
            </td>

            <td>

              <span
                class="badge"
                :class="
                  r.estado === 'Activo'
                  ? 'badge-green'
                  : 'badge-gray'
                "
              >

                {{ r.estado }}

              </span>

            </td>

            <td>

              <div class="action-btns">

                <button
                  class="action-btn"
                  @click="verDetalle(r)"
                >

                  Ver

                </button>

                <button
                  class="action-btn"
                  @click="editarRescate(index)"
                >

                  Editar

                </button>

                <button
                  class="action-btn close-btn"
                  @click="cerrarRescate(index)"
                >

                  Cerrar

                </button>

              </div>

            </td>

          </tr>

        </tbody>

      </table>

    </div>

    <!-- MODAL -->

    <div
      v-if="showDetailModal"
      class="modal-overlay"
      @click.self="
        showDetailModal = false
      "
    >

      <div class="detail-modal">

        <h2>
          Detalles del rescate
        </h2>

        <p><strong>ID:</strong> {{ rescueSelected?.id }}</p>

        <p><strong>Mascota:</strong> {{ rescueSelected?.mascota }}</p>

        <p><strong>Edad:</strong> {{ rescueSelected?.edad }}</p>

        <p><strong>Sexo:</strong> {{ rescueSelected?.sexo }}</p>

        <p><strong>Raza:</strong> {{ rescueSelected?.raza }}</p>

        <p><strong>Fecha rescate:</strong> {{ rescueSelected?.fechaRescate }}</p>

        <p><strong>Fecha creación:</strong> {{ rescueSelected?.fechaCreacion }}</p>

        <p><strong>Creado por:</strong> {{ rescueSelected?.creadoPor }}</p>

        <p><strong>Ubicación:</strong> {{ rescueSelected?.ubicacion }}</p>

        <p><strong>Casa cuna:</strong> {{ rescueSelected?.casaCuna }}</p>

        <p><strong>Rescatista:</strong> {{ rescueSelected?.rescatista }}</p>

        <p><strong>Estado:</strong> {{ rescueSelected?.estado }}</p>

        <p><strong>Descripción:</strong> {{ rescueSelected?.descripcion }}</p>

      </div>

    </div>

  </div>

</template>

<style scoped>

.view-container {

  padding: 20px;

}

.page-header {

  display: flex;

  justify-content: space-between;

  align-items: center;

  margin-bottom: 30px;

}

.admin-page-title {

  font-size: 38px;

  font-weight: 900;

}

.btn-toggle-form,
.btn-save {

  height: 58px;

  padding: 0 28px;

  border: none;

  border-radius: 18px;

  background: #92A894;

  color: white;

  font-weight: 800;

  cursor: pointer;

}

.filters-wrap {

  display: flex;

  gap: 12px;

  margin-bottom: 30px;

}

.filter-btn {

  height: 50px;

  padding: 0 22px;

  border-radius: 14px;

  border: none;

  background: white;

  cursor: pointer;

}

.filter-btn.active {

  background: #92A894;

  color: white;

}

.form-panel {

  background: white;

  padding: 34px;

  border-radius: 28px;

  margin-bottom: 34px;

}

.form-grid {

  display: grid;

  grid-template-columns: 1fr 1fr;

  gap: 20px;

}

.full-width {

  grid-column: 1 / -1;

}

.input-label {

  display: block;

  margin-bottom: 8px;

  font-size: 13px;

  font-weight: 700;

  color: #445046;

}

.custom-input,
.custom-textarea {

  width: 100%;

  padding: 18px;

  border-radius: 18px;

  border: 1px solid #E3E8E3;

  background: #F7F8F7;

}

.custom-textarea {

  min-height: 140px;

}

.form-actions {

  margin-top: 24px;

}

.table-wrapper {

  overflow-x: auto;

}

.data-table {

  width: 100%;

  border-spacing: 0 16px;

}

.data-table tr {

  background: white;

}

.data-table td {

  padding: 20px;

}

.badge {

  padding: 10px 16px;

  border-radius: 999px;

  font-size: 12px;

  font-weight: 800;

}

.badge-green {

  background: #E5F4E7;

  color: #46704B;

}

.badge-gray {

  background: #EEF1F3;

  color: #667085;

}

.action-btns {

  display: flex;

  gap: 10px;

}

.action-btn {

  height: 42px;

  padding: 0 18px;

  border: none;

  border-radius: 12px;

  background: #EEF2EE;

  cursor: pointer;

}

.close-btn {

  background: #FFE9E9;

  color: #B54747;

}

.modal-overlay {

  position: fixed;

  inset: 0;

  background: rgba(0,0,0,0.35);

  display: flex;

  align-items: center;

  justify-content: center;

}

.detail-modal {

  width: 700px;

  max-width: 95%;

  background: white;

  padding: 30px;

  border-radius: 28px;

}

.toast-success,
.toast-error {

  position: fixed;

  top: 20px;

  right: 20px;

  padding: 16px 22px;

  border-radius: 14px;

  color: white;

  z-index: 999;

}

.toast-success {

  background: #5F8663;

}

.toast-error {

  background: #D14F4F;

}

@media (max-width: 920px) {

  .form-grid {

    grid-template-columns: 1fr;

  }

}
</style>