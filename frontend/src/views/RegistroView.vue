<script setup>

/* ─────────────────────────────
   IMPORTS
───────────────────────────── */

import {
  ref,
  computed,
  onMounted,
  onBeforeUnmount
} from 'vue'

import {
  RouterLink,
  useRouter
} from 'vue-router'

import {
  countryList,
  phoneCodesList
} from '../data/paises'
import { authApi } from '../services/api'

const router = useRouter()

/* ─────────────────────────────
   ESTADOS
───────────────────────────── */

const nombre = ref('')
const correo = ref('')
const telefono = ref('')
const cedula = ref('')
const password = ref('')
const confirmarPassword = ref('')

const error = ref('')
const success = ref(false)
const loading = ref(false)

/* ─────────────────────────────
   PAISES
───────────────────────────── */

const countrySearch = ref('')

const showCountryDropdown = ref(false)

const filteredCountries = computed(() => {

  if (!countrySearch.value) {

    return countryList

  }

  return countryList.filter(country =>

    country.toLowerCase().includes(

      countrySearch.value.toLowerCase()

    )

  )

})

function selectCountry(country) {

  countrySearch.value = country

  showCountryDropdown.value = false

}

/* ─────────────────────────────
   CODIGOS TELEFONICOS
───────────────────────────── */

const dropdownContainer = ref(null)

const showCodeDropdown = ref(false)

const codeSearchQuery = ref('')

const selectedCountryObject = ref({

  name: 'Costa Rica',
  code: '+506'

})

const filteredPhoneCodes = computed(() => {

  const query =

    codeSearchQuery.value
      .toLowerCase()
      .trim()

  if (!query) {

    return phoneCodesList

  }

  return phoneCodesList.filter(item =>

    item.name
      .toLowerCase()
      .includes(query)

    ||

    item.code.includes(query)

  )

})

function selectPhoneCode(item) {

  selectedCountryObject.value = item

  showCodeDropdown.value = false

  codeSearchQuery.value = ''

}

/* ─────────────────────────────
   CLICK AFUERA
───────────────────────────── */

function handleClickOutside(event) {

  if (

    dropdownContainer.value &&

    !dropdownContainer.value.contains(
      event.target
    )

  ) {

    showCodeDropdown.value = false
    showCountryDropdown.value = false

  }

}

onMounted(() => {

  document.addEventListener(
    'click',
    handleClickOutside
  )

})

onBeforeUnmount(() => {

  document.removeEventListener(
    'click',
    handleClickOutside
  )

})

/* ─────────────────────────────
   TELEFONO SOLO NUMEROS
───────────────────────────── */

function filterPhoneNumber() {

  telefono.value =

    telefono.value.replace(
      /\D/g,
      ''
    )

}

/* ─────────────────────────────
   REGISTRO
───────────────────────────── */

function splitFullName(value) {
  const parts = value.trim().split(/\s+/)
  return {
    firstName: parts[0] || '',
    middleName: parts.length > 3 ? parts.slice(1, -2).join(' ') : '',
    lastName: parts.length > 1 ? parts[parts.length - 2] : parts[0] || '',
    secondLastName: parts.length > 2 ? parts[parts.length - 1] : ''
  }
}

function defaultBirthDate() {
  const date = new Date()
  date.setFullYear(date.getFullYear() - 18)
  return date.toISOString().slice(0, 10)
}

async function crearCuenta() {

  error.value = ''
  success.value = false

  if (

    !nombre.value ||
    !correo.value ||
    !telefono.value ||
    !cedula.value ||
    !password.value ||
    !confirmarPassword.value ||
    !countrySearch.value

  ) {

    error.value =
      'Completa todos los campos'

    return

  }

  if (

    password.value !==
    confirmarPassword.value

  ) {

    error.value =
      'Las contraseñas no coinciden'

    return

  }

  if (

    password.value.length < 8

  ) {

    error.value =
      'La contraseña debe tener mínimo 8 caracteres'

    return

  }

  loading.value = true

  try {

    const nameParts = splitFullName(nombre.value)
    const username = correo.value.split('@')[0] || cedula.value
    const telefonoCompleto = `${selectedCountryObject.value.code} ${telefono.value}`

    const nuevoUsuario = await authApi.register({
      username,
      password: password.value,
      nationalId: cedula.value,
      ...nameParts,
      birthDate: defaultBirthDate(),
      nationality: countrySearch.value,
      email: correo.value,
      phonePrimary: telefonoCompleto,
      phoneSecondary: '',
      city: countrySearch.value === 'Costa Rica' ? 'San Jose' : countrySearch.value,
      town: 'Sin especificar',
      addressLine: 'Sin especificar',
      createdBy: 'frontend'
    })

    localStorage.setItem('authUser', JSON.stringify(nuevoUsuario))
    localStorage.setItem('anhelo_usuario_actual', JSON.stringify({
      id: nuevoUsuario.userId,
      nombre: `${nuevoUsuario.firstName || nameParts.firstName} ${nuevoUsuario.lastName || nameParts.lastName}`.trim(),
      correo: nuevoUsuario.email || correo.value,
      cedula: cedula.value,
      telefono: telefonoCompleto,
      pais: countrySearch.value,
      rol: 'Usuario',
      activo: true
    }))

    success.value = true

    setTimeout(() => {

      router.push('/')

    }, 1000)

  } catch (apiError) {

    error.value =
      apiError.message || 'No se pudo crear la cuenta'

  } finally {

    loading.value = false

  }

}

</script>

<template>

  <div class="auth-container">

    <!-- VISUAL -->

    <div class="auth-visual">

      <RouterLink
        to="/"
        class="logo-link"
      >

        Anhelo

        <span class="peach">
          Pets
        </span>

      </RouterLink>

      <div class="visual-content">

        <h1 class="visual-title">

          Únete a nuestra
          comunidad

        </h1>

        <p class="visual-description">

          Crea tu cuenta y
          forma parte de
          Anhelo Pets.

        </p>

      </div>

    </div>

    <!-- FORM -->

    <div class="auth-form-side">

      <div class="form-container">

        <div class="form-header">

          <h2>
            Crear cuenta
          </h2>

          <p>
            Completa tu información
          </p>

        </div>

        <!-- ERROR -->

        <div
          v-if="error"
          class="error-box"
        >

          {{ error }}

        </div>

        <!-- SUCCESS -->

        <div
          v-if="success"
          class="success-box"
        >

          Cuenta creada correctamente

        </div>

        <!-- FORM -->

        <form
          @submit.prevent="
            crearCuenta
          "
        >

          <!-- NOMBRE -->

          <div class="input-group">

            <label>
              Nombre completo *
            </label>

            <input
              v-model="nombre"
              class="custom-input"
              placeholder="María González"
            />

          </div>

          <!-- CORREO -->

          <div class="input-group">

            <label>
              Correo electrónico *
            </label>

            <input
              v-model="correo"
              type="email"
              class="custom-input"
              placeholder="correo@ejemplo.com"
            />

          </div>

          <!-- PAIS -->

          <div class="input-group">

            <label>
              País *
            </label>

            <div class="autocomplete-wrapper">

              <input
                v-model="countrySearch"
                class="custom-input"
                placeholder="Buscar país..."
                @focus="
                  showCountryDropdown = true
                "
              />

              <div
                v-if="
                  showCountryDropdown
                "
                class="autocomplete-dropdown"
              >

                <button
                  v-for="country in filteredCountries"
                  :key="country"
                  type="button"
                  class="dropdown-item"
                  @click="
                    selectCountry(country)
                  "
                >

                  {{ country }}

                </button>

              </div>

            </div>

          </div>

          <!-- TELEFONO -->

          <div class="form-row">

            <!-- CODIGO -->

            <div
              class="input-group"
              ref="dropdownContainer"
            >

              <label>
                Código país *
              </label>

              <button
                type="button"
                class="phone-code-btn"
                @click="
                  showCodeDropdown =
                  !showCodeDropdown
                "
              >

                <span>

                  {{
                    selectedCountryObject.name
                  }}

                </span>

                <strong>

                  {{
                    selectedCountryObject.code
                  }}

                </strong>

              </button>

              <div
                v-if="showCodeDropdown"
                class="phone-dropdown"
              >

                <input
                  v-model="codeSearchQuery"
                  class="dropdown-search"
                  placeholder="Buscar..."
                />

                <div class="phone-list">

                  <button
                    v-for="item in filteredPhoneCodes"
                    :key="item.code + item.name"
                    type="button"
                    class="phone-option"
                    @click="
                      selectPhoneCode(item)
                    "
                  >

                    <span>
                      {{ item.name }}
                    </span>

                    <strong>
                      {{ item.code }}
                    </strong>

                  </button>

                </div>

              </div>

            </div>

            <!-- NUMERO -->

            <div class="input-group">

              <label>
                Teléfono *
              </label>

              <input
                v-model="telefono"
                @input="filterPhoneNumber"
                class="custom-input"
                placeholder="88888888"
              />

            </div>

          </div>

          <!-- CEDULA -->

          <div class="input-group">

            <label>
              Cédula *
            </label>

            <input
              v-model="cedula"
              class="custom-input"
              placeholder="1-2345-6789"
            />

          </div>

          <!-- PASSWORD -->

          <div class="form-row">

            <div class="input-group">

              <label>
                Contraseña *
              </label>

              <input
                v-model="password"
                type="password"
                class="custom-input"
                placeholder="Mínimo 8 caracteres"
              />

            </div>

            <div class="input-group">

              <label>
                Confirmar contraseña *
              </label>

              <input
                v-model="confirmarPassword"
                type="password"
                class="custom-input"
                placeholder="Repetir contraseña"
              />

            </div>

          </div>

          <!-- BOTON -->

          <button
            type="submit"
            class="btn-register"
          >

            Crear mi cuenta

          </button>

          <!-- LOGIN -->

          <div class="login-link-container">

            <span>
              ¿Ya tienes una cuenta?
            </span>

            <RouterLink
              to="/login"
              class="login-link"
            >

              Inicia sesión

            </RouterLink>

          </div>

        </form>

      </div>

    </div>

  </div>

</template>

<style scoped>

.auth-container {

  min-height: 100vh;

  display: flex;

  background: #FAFAFA;
}

.auth-visual {

  flex: 1;

  background:
    linear-gradient(
      135deg,
      #92A894,
      #7C927E
    );

  padding: 60px;

  color: white;
}

.logo-link {

  font-size: 30px;

  font-weight: 800;

  color: white;

  text-decoration: none;
}

.peach {

  color: #F9C17A;
}

.visual-content {

  margin-top: 120px;
}

.visual-title {

  font-size: 58px;

  font-weight: 800;

  line-height: 1.1;
}

.visual-description {

  margin-top: 22px;

  font-size: 18px;

  line-height: 1.7;

  max-width: 420px;
}

.auth-form-side {

  flex: 1.1;

  display: flex;

  justify-content: center;

  align-items: center;

  padding: 50px;
}

.form-container {

  width: 100%;

  max-width: 540px;
}

.form-header h2 {

  font-size: 36px;

  color: #2F3B31;

  margin-bottom: 6px;
}

.form-header p {

  color: #667085;

  margin-bottom: 30px;
}

.form-row {

  display: grid;

  grid-template-columns: 1fr 1fr;

  gap: 16px;
}

.input-group {

  margin-bottom: 20px;

  position: relative;
}

.input-group label {

  display: block;

  margin-bottom: 8px;

  font-size: 14px;

  font-weight: 700;

  color: #2F3B31;
}

.custom-input {

  width: 100%;

  height: 56px;

  border-radius: 16px;

  border: 2px solid #EEF2EE;

  background: #F8FAF8;

  padding: 0 18px;

  font-size: 14px;

  outline: none;

  transition: 0.25s ease;

  box-sizing: border-box;
}

.custom-input:focus {

  border-color: #92A894;

  background: white;
}

.autocomplete-dropdown,
.phone-dropdown {

  position: absolute;

  top: calc(100% + 8px);

  left: 0;

  width: 100%;

  background: white;

  border-radius: 18px;

  box-shadow:
    0 18px 40px rgba(0,0,0,0.08);

  max-height: 260px;

  overflow: hidden;

  z-index: 999;
}

.dropdown-item,
.phone-option {

  width: 100%;

  border: none;

  background: white;

  padding: 14px 18px;

  text-align: left;

  cursor: pointer;

  font-size: 14px;

  display: flex;

  justify-content: space-between;
}

.dropdown-item:hover,
.phone-option:hover {

  background: #F5F7F5;
}

.phone-code-btn {

  width: 100%;

  height: 56px;

  border-radius: 16px;

  border: 2px solid #EEF2EE;

  background: #F8FAF8;

  padding: 0 16px;

  display: flex;

  justify-content: space-between;

  align-items: center;

  cursor: pointer;
}

.dropdown-search {

  width: 100%;

  height: 50px;

  border: none;

  border-bottom:
    1px solid #EEF2EE;

  padding: 0 16px;

  outline: none;
}

.phone-list {

  max-height: 200px;

  overflow-y: auto;
}

.btn-register {

  width: 100%;

  height: 58px;

  border: none;

  border-radius: 18px;

  background:
    linear-gradient(
      135deg,
      #92A894,
      #7C927E
    );

  color: white;

  font-size: 15px;

  font-weight: 800;

  cursor: pointer;

  margin-top: 10px;
}

.login-link-container {

  margin-top: 24px;

  display: flex;

  justify-content: center;

  gap: 6px;

  font-size: 14px;

  color: #667085;
}

.login-link {

  color: #7C927E;

  font-weight: 700;

  text-decoration: none;
}

.login-link:hover {

  text-decoration: underline;
}

.error-box {

  background:
    rgba(235,119,119,0.12);

  color: #C45252;

  padding: 16px;

  border-radius: 16px;

  margin-bottom: 20px;

  font-weight: 700;
}

.success-box {

  background:
    rgba(111,133,114,0.12);

  color: #5C715E;

  padding: 16px;

  border-radius: 16px;

  margin-bottom: 20px;

  font-weight: 700;
}

@media (max-width: 900px) {

  .auth-visual {

    display: none;
  }

  .form-row {

    grid-template-columns: 1fr;
  }

}

</style>
