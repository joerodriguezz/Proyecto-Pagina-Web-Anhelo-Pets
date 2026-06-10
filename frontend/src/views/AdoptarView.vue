<script setup>
import { ref, onMounted, computed, onBeforeUnmount } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import NavBar from '../components/NavBar.vue'
import FooterBar from '../components/FooterBar.vue'
import { usePetsStore } from '../stores/usePetsStore'

import {
  countryList,
  phoneCodesList
} from '../data/paises'

const store = usePetsStore()

const route = useRoute()
const router = useRouter()


/* ---------------- CONTROL ---------------- */

const submitted = ref(false)
const showTermsModal = ref(false)

const dropdownContainer = ref(null)

const usuarioActual = ref(null)
const yaTieneSolicitud = ref(false)

/* ---------------- GALERÍA ---------------- */

const currentImageIndex = ref(0)

const petImages = computed(() => {

  const pet = store.pets.find(p =>

    p.name === selectedPet.value

  )

  if (
    pet &&
    pet.images &&
    pet.images.length > 0
  ) {

    return pet.images.map(img => img.preview)

  }

  return ['/img-mascotas/mascotas.jpg']

})

/* ---------------- PAÍSES ---------------- */

const countrySearch = ref('')
const showCountryDropdown = ref(false)

const filteredCountries = computed(() => {

  if (!countrySearch.value) return []

  return countryList.filter(country =>

    country
      .toLowerCase()
      .includes(
        countrySearch.value.toLowerCase()
      )

  )

})

function selectCountry(country) {

  countrySearch.value = country
  showCountryDropdown.value = false

}

/* ---------------- TELÉFONOS ---------------- */

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

function handleClickOutside(event) {

  if (
    dropdownContainer.value &&
    !dropdownContainer.value.contains(event.target)
  ) {

    showCodeDropdown.value = false

  }

}

/* ---------------- FORMULARIO ---------------- */

const fullName = ref('')
const email = ref('')
const idCard = ref('')
const phone = ref('')
const isWhatsApp = ref(false)
const age = ref('')
const livesInCostaRica = ref('')
const fullAddress = ref('')
const selectedPet = ref('')

const whyThisPet = ref('')
const adoptionReasons = ref('')
const householdMembers = ref('')
const otherPetsDetails = ref('')
const profession = ref('')
const dailyRoutine = ref('')
const hoursAlone = ref('')

const termsAccepted = ref(false)

function filterPhoneNumber() {

  phone.value =
    phone.value.replace(/\D/g, '')

}

/* ---------------- MOUNTED ---------------- */

onMounted(() => {

  const usuarioGuardado = JSON.parse(

    localStorage.getItem(
      'anhelo_usuario_actual'
    )

  )

  if (usuarioGuardado) {

    usuarioActual.value =
      usuarioGuardado

    fullName.value =
      usuarioGuardado.nombre || ''

    email.value =
      usuarioGuardado.correo || ''

    idCard.value =
      usuarioGuardado.cedula || ''

    age.value =
      usuarioGuardado.edad || ''

    fullAddress.value =
      usuarioGuardado.direccion || ''

  }

  if (route.query.name) {

    selectedPet.value =
      route.query.name

  }

  const solicitudes = JSON.parse(

    localStorage.getItem(
      'anhelo_solicitudes'
    )

  ) || []

  const solicitudExistente =
    solicitudes.find(s =>

      s.usuarioId ===
      usuarioGuardado?.id

    )

  if (solicitudExistente) {

    yaTieneSolicitud.value = true

    phone.value =
      solicitudExistente.telefono
        ?.replace(/\D/g, '')
        || ''

    livesInCostaRica.value =
      solicitudExistente.viveEnCR === 'Sí'
        ? 'si'
        : 'no'

    countrySearch.value =
      solicitudExistente.paisExtranjero || ''

    whyThisPet.value =
      solicitudExistente.porqueMascota || ''

    adoptionReasons.value =
      solicitudExistente.motivos || ''

    householdMembers.value =
      solicitudExistente.hogar || ''

    otherPetsDetails.value =
      solicitudExistente.otrasMascotas || ''

    profession.value =
      solicitudExistente.profesion || ''

    dailyRoutine.value =
      solicitudExistente.rutina || ''

    hoursAlone.value =
      solicitudExistente.horasSola || ''

  }

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

/* ---------------- ENVIAR ---------------- */

function submitForm() {

  const solicitudes = JSON.parse(

    localStorage.getItem(
      'anhelo_solicitudes'
    )

  ) || []

  const solicitudDuplicada =
    solicitudes.find(s =>

      s.usuarioId === usuarioActual.value?.id
      &&
      s.mascota === selectedPet.value

    )

  if (solicitudDuplicada) {

    alert(
      'Ya enviaste una solicitud para esta mascota.'
    )

    return

  }

  const ultimoNumero =
  solicitudes.length + 1

const idGenerado =
  `ADO-${String(ultimoNumero).padStart(3, '0')}`

const nuevaSolicitud = {

  id:
    idGenerado,

    usuarioId:
      usuarioActual.value?.id || '',

    solicitante:
      fullName.value,

    cedula:
      idCard.value,

    email:
      email.value,

    telefono:
      `${selectedCountryObject.value.code} ${phone.value}`,

    edad:
      age.value,

    whatsapp:
      isWhatsApp.value
        ? 'Sí'
        : 'No',

    viveEnCR:
      livesInCostaRica.value === 'si'
        ? 'Sí'
        : 'No',

    paisExtranjero:
      countrySearch.value,

    direccion:
      fullAddress.value,

    mascota:
      selectedPet.value,

    porqueMascota:
      whyThisPet.value,

    motivos:
      adoptionReasons.value,

    hogar:
      householdMembers.value,

    otrasMascotas:
      otherPetsDetails.value,

    profesion:
      profession.value,

    rutina:
      dailyRoutine.value,

    horasSola:
      hoursAlone.value,

    fecha:
      new Date()
        .toISOString()
        .split('T')[0],

    estado:
      'Pendiente'

  }

  solicitudes.push(
    nuevaSolicitud
  )

  localStorage.setItem(
    'anhelo_solicitudes',
    JSON.stringify(solicitudes)
  )

  submitted.value = true

  window.scrollTo({
    top: 0,
    behavior: 'smooth'
  })

}

function goBack() {

  router.push({
    name: 'mascotas'
  })

}
</script>

<template>
  <NavBar />

  <div class="page-hero">

    <div class="hero-content">

      <span class="hero-tag">
        Proceso Responsable
      </span>

      <h1>
        Formulario de Adopción
      </h1>

      <div class="hero-divider-line"></div>

      <p class="section-subtitle">
        Completa los siguientes datos para procesar tu postulación de forma segura.
      </p>

    </div>

  </div>

  <section class="container form-section">

    <div class="card">

      <div v-if="!submitted">

        <form @submit.prevent="submitForm">

          <div
            class="aesthetic-pet-card"
            v-if="selectedPet"
          >

            <div class="carousel-container">

              <img
                :src="petImages[currentImageIndex]"
                :alt="selectedPet"
                class="carousel-image"
              />

              <div
                class="carousel-controls"
                v-if="petImages.length > 1"
              >

                <button
                  type="button"
                  class="carousel-arrow"
                  @click="prevImage"
                >
                  ‹
                </button>

                <button
                  type="button"
                  class="carousel-arrow"
                  @click="nextImage"
                >
                  ›
                </button>

              </div>

            </div>

            <div class="pet-card-info">

              <span class="pet-card-tag">
                Estás aplicando para la adopción de
              </span>

              <h2 class="pet-card-name">
                {{ selectedPet }}
              </h2>

              <p class="pet-card-notice">
                Nuestro equipo revisará tu solicitud y se comunicará contigo.
              </p>

            </div>

          </div>

          <h3 class="form-section-title">
            Información personal
          </h3>

          <label class="form-label">
            Nombre completo *
          </label>

          <input
            class="form-input"
            v-model="fullName"
            required
          />

          <div class="grid-2-col">

            <div>

              <label class="form-label">
                Cédula *
              </label>

              <input
                class="form-input"
                v-model="idCard"
                required
              />

            </div>

            <div>

              <label class="form-label">
                Correo electrónico *
              </label>

              <input
                class="form-input"
                type="email"
                v-model="email"
                required
              />

            </div>

          </div>

          <div class="grid-2-col">

            <div>

              <label class="form-label">
                Número de teléfono *
              </label>

              <div class="phone-group">

                <div
                  class="code-dropdown-wrapper"
                  ref="dropdownContainer"
                >

                  <button
                    type="button"
                    class="code-selector-btn"
                    @click="showCodeDropdown = !showCodeDropdown"
                  >

                    <span>
                      {{ selectedCountryObject.code }}
                    </span>

                  </button>

                  <div
                    v-if="showCodeDropdown"
                    class="code-dropdown-box"
                  >

                    <div class="search-box-container">

                      <input
                        type="text"
                        class="dropdown-search-input"
                        v-model="codeSearchQuery"
                        placeholder="Buscar país o código..."
                      />

                    </div>

                    <ul class="code-results-list">

                      <li
                        v-for="item in filteredPhoneCodes"
                        :key="item.name"
                        @click="selectPhoneCode(item)"
                      >

                        <span>
                          {{ item.name }}
                        </span>

                        <strong>
                          {{ item.code }}
                        </strong>

                      </li>

                    </ul>

                  </div>

                </div>

                <input
                  class="form-input"
                  v-model="phone"
                  @input="filterPhoneNumber"
                  required
                />

              </div>

            </div>

            <div>

              <label class="form-label">
                Edad *
              </label>

              <input
                class="form-input"
                type="number"
                v-model="age"
                required
              />

            </div>

          </div>

          <div class="checkbox-wrapper">

            <input
              type="checkbox"
              v-model="isWhatsApp"
            />

            <label>
              Este número tiene WhatsApp
            </label>

          </div>

          <label class="form-label">
            ¿Vives en Costa Rica? *
          </label>

          <div class="radio-cards-group">

            <div
              class="radio-card"
              :class="{ active: livesInCostaRica === 'si' }"
              @click="livesInCostaRica = 'si'"
            >
              Sí
            </div>

            <div
              class="radio-card"
              :class="{ active: livesInCostaRica === 'no' }"
              @click="livesInCostaRica = 'no'"
            >
              No
            </div>

          </div>

          <div
            v-if="livesInCostaRica === 'no'"
            class="dynamic-field"
          >

            <label class="form-label">
              País *
            </label>

            <div class="autocomplete-wrapper">

              <input
                class="form-input"
                v-model="countrySearch"
                @input="showCountryDropdown = true"
              />

              <ul
                v-if="
                  showCountryDropdown
                  &&
                  filteredCountries.length
                "
                class="autocomplete-results"
              >

                <li
                  v-for="country in filteredCountries"
                  :key="country"
                  @click="selectCountry(country)"
                >
                  {{ country }}
                </li>

              </ul>

            </div>

          </div>

          <label class="form-label">
            Dirección exacta *
          </label>

          <textarea
            class="form-input"
            rows="2"
            v-model="fullAddress"
            required
          ></textarea>

          <hr class="form-divider" />

          <h3 class="form-section-title">
            Detalles de la adopción
          </h3>

          <label class="form-label">
            ¿Qué te hace sentir que esta mascota es ideal para ti?
          </label>

          <textarea
            class="form-input"
            rows="2"
            v-model="whyThisPet"
          ></textarea>

          <label class="form-label">
            ¿Cuáles son tus motivos para adoptar? *
          </label>

          <textarea
            class="form-input"
            rows="2"
            v-model="adoptionReasons"
            required
          ></textarea>

          <hr class="form-divider" />

          <h3 class="form-section-title">
            Entorno y estilo de vida
          </h3>

          <label class="form-label">
            ¿Con quiénes vives? *
          </label>

          <textarea
            class="form-input"
            rows="2"
            v-model="householdMembers"
            required
          ></textarea>

          <label class="form-label">
            Otras mascotas
          </label>

          <textarea
            class="form-input"
            rows="2"
            v-model="otherPetsDetails"
          ></textarea>

          <label class="form-label">
            Profesión *
          </label>

          <input
            class="form-input"
            v-model="profession"
            required
          />

          <label class="form-label">
            Rutina diaria *
          </label>

          <textarea
            class="form-input"
            rows="2"
            v-model="dailyRoutine"
            required
          ></textarea>

          <label class="form-label">
            ¿Cuántas horas estaría sola la mascota? *
          </label>

          <input
            class="form-input"
            v-model="hoursAlone"
            required
          />

          <hr class="form-divider" />

          <div class="terms-container">

            <div class="checkbox-wrapper">

              <input
                type="checkbox"
                v-model="termsAccepted"
                required
              />

              <label>
                Acepto los términos y condiciones
              </label>

            </div>

          </div>

          <div class="actions-group">

            <button
              type="submit"
              class="pet-btn"
              :disabled="!termsAccepted"
            >
              Enviar solicitud
            </button>

            <button
              type="button"
              class="pet-btn-outline"
              @click="goBack"
            >
              Volver
            </button>

          </div>

        </form>

      </div>

      <div
        v-else
        class="success-screen"
      >

        <h3>
          ¡Solicitud enviada!
        </h3>

        <p>
          Gracias por adoptar responsablemente.
        </p>

      </div>

    </div>

  </section>

  <FooterBar />

  
</template>

<style scoped>
.page-hero {
  position: relative;
  background: #3A473C;
  height: 260px;
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
  padding: 0 24px;
}

.hero-content {
  max-width: 580px;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.hero-tag {
  font-size: 11px;
  text-transform: uppercase;
  letter-spacing: 2px;
  color: #92A894;
  font-weight: 700;
  margin-bottom: 12px;
}

.page-hero h1 {
  font-size: 38px;
  font-weight: 800;
  color: #F4F6F4;
  margin: 0 0 16px 0;
  line-height: 1.1;
}

.hero-divider-line {
  width: 40px;
  height: 2px;
  background: #92A894;
  margin-bottom: 16px;
  border-radius: 2px;
}

.section-subtitle {
  font-size: 14px;
  color: #DCE4DD;
  line-height: 1.6;
  max-width: 460px;
}

.form-section {
  padding: 0 24px 60px;
  margin-top: -40px;
  position: relative;
  z-index: 5;
}

.card {
  max-width: 720px;
  margin: 0 auto;
  padding: 40px;
  background: white;
  border-radius: 34px;
  border: 1px solid rgba(146, 168, 148, 0.14);
  box-shadow: 0 20px 50px rgba(58, 71, 60, 0.05);
}

.aesthetic-pet-card {
  display: flex;
  background: #F4F6F4;
  border-radius: 24px;
  overflow: hidden;
  margin-bottom: 34px;
}

.carousel-container {
  position: relative;
  width: 220px;
  height: 220px;
  flex-shrink: 0;
}

.carousel-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.carousel-controls {
  position: absolute;
  top: 50%;
  left: 0;
  width: 100%;
  transform: translateY(-50%);
  display: flex;
  justify-content: space-between;
  padding: 0 10px;
}

.carousel-arrow {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  border: none;
  background: rgba(255,255,255,0.9);
  cursor: pointer;
  font-size: 18px;
  font-weight: bold;
}

.carousel-dots {
  position: absolute;
  bottom: 12px;
  left: 50%;
  transform: translateX(-50%);
  display: flex;
  gap: 6px;
}

.dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: rgba(255,255,255,0.5);
}

.dot.active {
  width: 14px;
  border-radius: 8px;
  background: white;
}

.pet-card-info {
  padding: 26px;
  display: flex;
  flex-direction: column;
  justify-content: center;
}

.pet-card-tag {
  font-size: 11px;
  letter-spacing: 1.5px;
  text-transform: uppercase;
  color: #6C756D;
  font-weight: 700;
}

.pet-card-name {
  font-size: 32px;
  font-weight: 800;
  color: #3A473C;
  margin: 8px 0;
}

.pet-card-notice {
  font-size: 13px;
  line-height: 1.6;
  color: #5F6A61;
}

.form-section-title {
  font-size: 18px;
  font-weight: 700;
  color: #3A473C;
  margin: 28px 0 16px;
}

.form-label {
  display: block;
  margin-top: 16px;
  margin-bottom: 8px;
  font-size: 14px;
  font-weight: 700;
  color: #3A473C;
}

.form-input {
  width: 100%;
  height: 52px;
  border-radius: 14px;
  border: 1px solid #DCE4DD;
  background: #FAFAFA;
  padding: 0 16px;
  font-size: 14px;
  color: #3A473C;
  outline: none;
  transition: 0.2s ease;
}

.form-input:focus {
  border-color: #92A894;
  box-shadow: 0 0 0 4px rgba(146,168,148,0.12);
}

textarea.form-input {
  height: auto;
  min-height: 90px;
  padding: 14px 16px;
  resize: vertical;
}

.grid-2-col {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.phone-group {
  display: flex;
  gap: 8px;
}

.code-dropdown-wrapper {
  position: relative;
}

.code-selector-btn {
  width: 95px;
  height: 52px;
  border-radius: 14px;
  border: 1px solid #DCE4DD;
  background: #FAFAFA;
  font-weight: 700;
  cursor: pointer;
}

.code-dropdown-box {
  position: absolute;
  top: 105%;
  left: 0;
  width: 260px;
  background: white;
  border-radius: 16px;
  border: 1px solid #DCE4DD;
  overflow: hidden;
  z-index: 100;
  box-shadow: 0 12px 30px rgba(0,0,0,0.1);
}

.search-box-container {
  padding: 10px;
  border-bottom: 1px solid #ECEFEC;
}

.dropdown-search-input {
  width: 100%;
  height: 38px;
  border-radius: 10px;
  border: 1px solid #DCE4DD;
  padding: 0 12px;
}

.code-results-list {
  max-height: 220px;
  overflow-y: auto;
}

.code-results-list li {
  padding: 12px 14px;
  cursor: pointer;
  display: flex;
  justify-content: space-between;
}

.code-results-list li:hover {
  background: #F4F6F4;
}

.dropdown-country-code {
  color: #92A894;
  font-weight: 700;
}

.checkbox-wrapper {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-top: 16px;
}

.radio-cards-group {
  display: flex;
  gap: 16px;
  margin-top: 10px;
}

.radio-card {
  flex: 1;
  padding: 16px;
  border-radius: 16px;
  border: 1px solid #DCE4DD;
  background: #FAFAFA;
  cursor: pointer;
  transition: 0.2s ease;
}

.radio-card.active {
  border-color: #92A894;
  background: #F4F6F4;
}

.dynamic-field {
  background: #F4F6F4;
  padding: 18px;
  border-radius: 16px;
  margin-top: 18px;
}

.autocomplete-wrapper {
  position: relative;
}

.autocomplete-results {
  position: absolute;
  top: 100%;
  left: 0;
  width: 100%;
  background: white;
  border-radius: 0 0 14px 14px;
  border: 1px solid #DCE4DD;
  max-height: 200px;
  overflow-y: auto;
  z-index: 100;
}

.autocomplete-results li {
  padding: 12px 14px;
  cursor: pointer;
}

.autocomplete-results li:hover {
  background: #F4F6F4;
}

.form-divider {
  border: none;
  height: 1px;
  background: rgba(146,168,148,0.18);
  margin: 34px 0;
}

.terms-container {
  background: #F4F6F4;
  padding: 20px;
  border-radius: 18px;
  margin-bottom: 30px;
}

.terms-btn-link {
  border: none;
  background: transparent;
  color: #92A894;
  text-decoration: underline;
  cursor: pointer;
  font-weight: 700;
}

.actions-group {
  display: flex;
  gap: 16px;
}

.pet-btn,
.pet-btn-outline {
  flex: 1;
  height: 62px;
  border-radius: 18px;
  font-size: 16px;
  font-weight: 800;
  cursor: pointer;
  transition: 0.2s ease;
}

.pet-btn {
  border: none;
  background: #3A473C;
  color: white;
}

.pet-btn:hover {
  background: #2E382F;
}

.pet-btn-outline {
  background: transparent;
  border: 2px solid #3A473C;
  color: #3A473C;
}

.pet-btn-outline:hover {
  background: #F4F6F4;
}

.success-screen {
  text-align: center;
  padding: 20px;
}

.success-tag {
  font-size: 11px;
  letter-spacing: 2px;
  text-transform: uppercase;
  color: #92A894;
  font-weight: 700;
}

.success-summary-box {
  margin-top: 24px;
  background: #FAFAFA;
  border-radius: 18px;
  padding: 24px;
}

.highlight-text {
  background: #E8EFE9;
  padding: 3px 8px;
  border-radius: 8px;
  font-weight: 700;
}

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-card {
  width: 100%;
  max-width: 520px;
  background: white;
  border-radius: 24px;
  overflow: hidden;
}

.modal-header,
.modal-footer {
  padding: 20px 24px;
}

.modal-body {
  padding: 0 24px 24px;
  line-height: 1.7;
}

.close-modal-btn {
  border: none;
  background: transparent;
  font-size: 26px;
  cursor: pointer;
}

.error-banner {
  padding: 20px;
  flex-direction: column;
  align-items: center;
  gap: 12px;
  background: #FDF2F2;
}

.clear-btn {
  border: none;
  background: transparent;
  text-decoration: underline;
  font-weight: 700;
  cursor: pointer;
}

@media (max-width: 768px) {

  .grid-2-col {
    grid-template-columns: 1fr;
  }

  .actions-group {
    flex-direction: column;
  }

  .aesthetic-pet-card {
    flex-direction: column;
  }

  .carousel-container {
    width: 100%;
    height: 260px;
  }

  .card {
    padding: 24px;
  }
}
</style>