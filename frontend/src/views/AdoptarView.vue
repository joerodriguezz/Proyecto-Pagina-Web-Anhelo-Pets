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


const mascotaSeleccionada =
  store.pets.find(
    p => p.name === selectedPet.value
  )


const nuevaSolicitud = {

  id:
    idGenerado,
  
    petId:
      mascotaSeleccionada?.id || '',  

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

  <!-- ───── HERO ───── -->
  <div class="page-hero">
    <div class="hero-bg-layer"></div>
    <div class="hero-content">
      <span class="hero-tag">Proceso Responsable</span>
      <h1>Formulario de Adopción</h1>
      <div class="hero-divider-line"></div>
      <p class="section-subtitle">
        Completa los siguientes datos para procesar tu postulación de forma segura.
      </p>
    </div>
  </div>

  <!-- ───── FORMULARIO ───── -->
  <section class="container form-section">
    <div class="card">

      <div v-if="!submitted">
        <form @submit.prevent="submitForm">

          <!-- Tarjeta mascota -->
          <div class="aesthetic-pet-card" v-if="selectedPet">
            <div class="carousel-container">
              <img
                :src="petImages[currentImageIndex]"
                :alt="selectedPet"
                class="carousel-image"
              />
              <div class="carousel-controls" v-if="petImages.length > 1">
                <button type="button" class="carousel-arrow" @click="prevImage">‹</button>
                <button type="button" class="carousel-arrow" @click="nextImage">›</button>
              </div>
            </div>
            <div class="pet-card-info">
              <span class="pet-card-tag">Estás aplicando para la adopción de</span>
              <h2 class="pet-card-name">{{ selectedPet }}</h2>
              <p class="pet-card-notice">
                Nuestro equipo revisará tu solicitud y se comunicará contigo.
              </p>
              <div class="pet-card-badge">Solicitud en proceso</div>
            </div>
          </div>

      
          <!-- ── Bloque 1: Información personal ── -->
          <div class="form-block">
            <div class="block-header">
              <div class="block-header-left">
                <span class="block-number">01</span>
                <div>
                  <h3 class="form-section-title">Información personal</h3>
                  <p class="block-subtitle">Tus datos de identificación y contacto</p>
                </div>
              </div>
            </div>

            <div class="fields-wrapper">
              <div class="field-group">
                <label class="form-label">Nombre completo <span class="req">*</span></label>
                <input class="form-input" v-model="fullName" required placeholder="Tu nombre completo" />
              </div>

              <div class="grid-2-col">
                <div class="field-group">
                  <label class="form-label">Cédula <span class="req">*</span></label>
                  <input class="form-input" v-model="idCard" required placeholder="Número de cédula" />
                </div>
                <div class="field-group">
                  <label class="form-label">Correo electrónico <span class="req">*</span></label>
                  <input class="form-input" type="email" v-model="email" required placeholder="correo@ejemplo.com" />
                </div>
              </div>

              <div class="grid-2-col">
                <div class="field-group">
                  <label class="form-label">Número de teléfono <span class="req">*</span></label>
                  <div class="phone-group">
                    <div class="code-dropdown-wrapper" ref="dropdownContainer">
                      <button
                        type="button"
                        class="code-selector-btn"
                        @click="showCodeDropdown = !showCodeDropdown"
                      >
                        <span>{{ selectedCountryObject.code }}</span>
                        <svg class="chevron-icon" viewBox="0 0 10 6" fill="none">
                          <path d="M1 1l4 4 4-4" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
                        </svg>
                      </button>
                      <div v-if="showCodeDropdown" class="code-dropdown-box">
                        <div class="search-box-container">
                          <input
                            type="text"
                            class="dropdown-search-input"
                            v-model="codeSearchQuery"
                            placeholder="Buscar país o código..."
                          />
                        </div>
                        <ul class="code-results-list">
                          <li v-for="item in filteredPhoneCodes" :key="item.name" @click="selectPhoneCode(item)">
                            <span>{{ item.name }}</span>
                            <strong>{{ item.code }}</strong>
                          </li>
                        </ul>
                      </div>
                    </div>
                    <input
                      class="form-input phone-number-input"
                      v-model="phone"
                      @input="filterPhoneNumber"
                      required
                      placeholder="Número"
                    />
                  </div>
                  <div class="checkbox-wrapper">
                    <input type="checkbox" id="whatsapp-check" v-model="isWhatsApp" />
                    <label for="whatsapp-check" class="checkbox-label">Este número tiene WhatsApp</label>
                  </div>
                </div>

                <div class="field-group">
                  <label class="form-label">Edad <span class="req">*</span></label>
                  <input class="form-input" type="number" v-model="age" required placeholder="Tu edad" />
                </div>
              </div>

              <div class="field-group">
                <label class="form-label">¿Vives en Costa Rica? <span class="req">*</span></label>
                <div class="radio-cards-group">
                  <div class="radio-card" :class="{ active: livesInCostaRica === 'si' }" @click="livesInCostaRica = 'si'">
                    Sí
                  </div>
                  <div class="radio-card" :class="{ active: livesInCostaRica === 'no' }" @click="livesInCostaRica = 'no'">
                    No
                  </div>
                </div>
              </div>

              <div v-if="livesInCostaRica === 'no'" class="dynamic-field">
                <label class="form-label">País <span class="req">*</span></label>
                <div class="autocomplete-wrapper">
                  <input
                    class="form-input"
                    v-model="countrySearch"
                    @input="showCountryDropdown = true"
                    placeholder="Escribe tu país..."
                  />
                  <ul v-if="showCountryDropdown && filteredCountries.length" class="autocomplete-results">
                    <li v-for="country in filteredCountries" :key="country" @click="selectCountry(country)">
                      {{ country }}
                    </li>
                  </ul>
                </div>
              </div>

              <div class="field-group">
                <label class="form-label">Dirección exacta <span class="req">*</span></label>
                <textarea class="form-input" rows="2" v-model="fullAddress" required placeholder="Provincia, cantón, distrito y dirección detallada"></textarea>
              </div>
            </div>
          </div>

          <!-- ── Bloque 2: Detalles de la adopción ── -->
          <div class="form-block">
            <div class="block-header">
              <div class="block-header-left">
                <span class="block-number">02</span>
                <div>
                  <h3 class="form-section-title">Detalles de la adopción</h3>
                  <p class="block-subtitle">Cuéntanos sobre tus motivaciones</p>
                </div>
              </div>
            </div>

            <div class="fields-wrapper">
              <div class="field-group">
                <label class="form-label">¿Qué te hace sentir que esta mascota es ideal para ti? <span class="req-optional">(Opcional)</span></label>
                <textarea class="form-input" rows="2" v-model="whyThisPet" placeholder="Comparte lo que sientes sobre esta mascota..."></textarea>
              </div>

              <div class="field-group">
                <label class="form-label">¿Cuáles son tus motivos para adoptar? <span class="req">*</span></label>
                <textarea class="form-input" rows="2" v-model="adoptionReasons" required placeholder="Describe tus razones para adoptar..."></textarea>
              </div>
            </div>
          </div>

          <!-- ── Bloque 3: Entorno y estilo de vida ── -->
          <div class="form-block">
            <div class="block-header">
              <div class="block-header-left">
                <span class="block-number">03</span>
                <div>
                  <h3 class="form-section-title">Entorno y estilo de vida</h3>
                  <p class="block-subtitle">Información sobre tu hogar y rutina</p>
                </div>
              </div>
            </div>

            <div class="fields-wrapper">
              <div class="field-group">
                <label class="form-label">¿Con quiénes vives? <span class="req">*</span></label>
                <textarea class="form-input" rows="2" v-model="householdMembers" required placeholder="Describe con quiénes compartes el hogar..."></textarea>
              </div>

              <div class="field-group">
                <label class="form-label">Otras mascotas <span class="req-optional">(Opcional)</span></label>
                <textarea class="form-input" rows="2" v-model="otherPetsDetails" placeholder="¿Tienes otras mascotas? Descríbelas..."></textarea>
              </div>

              <div class="field-group">
                <label class="form-label">Profesión <span class="req">*</span></label>
                <input class="form-input" v-model="profession" required placeholder="Tu ocupación actual" />
              </div>

              <div class="field-group">
                <label class="form-label">Rutina diaria <span class="req">*</span></label>
                <textarea class="form-input" rows="2" v-model="dailyRoutine" required placeholder="Describe un día típico en tu vida..."></textarea>
              </div>

              <div class="field-group">
                <label class="form-label">¿Cuántas horas estaría sola la mascota? <span class="req">*</span></label>
                <input class="form-input" v-model="hoursAlone" required placeholder="Número de horas aproximado" />
              </div>
            </div>
          </div>

          <!-- ── Acciones ── -->
          <div class="actions-group">
            <button type="submit" class="pet-btn" :disabled="!termsAccepted">
              Enviar solicitud
            </button>
            <button type="button" class="pet-btn-outline" @click="goBack">
              Volver
            </button>
          </div>

        </form>
      </div>

      <!-- Estado de éxito -->
      <div v-else class="success-screen">
        <div class="success-icon-wrap">
          <svg viewBox="0 0 48 48" fill="none">
            <circle cx="24" cy="24" r="23" stroke="#3A473C" stroke-width="1.5"/>
            <path d="M14 24l7 7 13-14" stroke="#3A473C" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"/>
          </svg>
        </div>
        <h3>¡Solicitud enviada!</h3>
        <p>Gracias por adoptar responsablemente.</p>
      </div>

          <!-- ── Términos y Condiciones ── -->
          <div class="tc-wrapper">
            <label class="tc-checkbox-row">
              <input
                type="checkbox"
                v-model="termsAccepted"
                required
                class="tc-checkbox"
              />
              <span class="tc-checkbox-label">
                He leído y acepto los
                <span class="tc-tooltip-anchor">
                  <span class="tc-trigger">Términos y Condiciones</span>
                  <span class="tc-tooltip">
                    <span class="tc-tooltip-title">Términos y Condiciones de Adopción</span>
                    <ul class="tc-tooltip-list">
                      <li>La información proporcionada debe ser verídica y comprobable.</li>
                      <li>La fundación puede verificar los datos suministrados en cualquier momento.</li>
                      <li>El envío del formulario no garantiza la aprobación de la adopción.</li>
                      <li>La fundación puede aprobar o rechazar solicitudes según criterios de bienestar animal.</li>
                      <li>La fundación puede realizar entrevistas, visitas domiciliarias o seguimientos post-adopción.</li>
                      <li>El adoptante se compromete a brindar bienestar, alimentación adecuada y atención veterinaria.</li>
                      <li>La fundación puede solicitar información adicional durante el proceso.</li>
                    </ul>
                  </span>
                </span>
                de adopción
                <span class="req">*</span>
              </span>
            </label>
          </div>

    </div>

  </section>

  <FooterBar />
</template>

<style scoped>

/* ═══════════════════════════════════
   HERO
═══════════════════════════════════ */

.page-hero {
  position: relative;
  background: #2D372F;
  height: 430px;
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
  padding: 0 24px;
  overflow: hidden;
}

.hero-bg-layer {
  position: absolute;
  inset: 0;
  background:
    radial-gradient(ellipse 70% 60% at 50% 110%, rgba(201,160,106,0.12) 0%, transparent 70%),
    radial-gradient(ellipse 50% 40% at 80% -10%, rgba(58,71,60,0.6) 0%, transparent 60%);
  pointer-events: none;
}

.hero-content {
  position: relative;
  z-index: 2;
  max-width: 600px;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0;
}

.hero-tag {
  display: inline-block;
  font-size: 10px;
  text-transform: uppercase;
  letter-spacing: 3px;
  color: #C9A06A;
  font-weight: 700;
  margin-bottom: 20px;
  padding: 6px 16px;
  border: 1px solid rgba(201,160,106,0.35);
  border-radius: 100px;
  background: rgba(201,160,106,0.08);
}

.page-hero h1 {
  font-size: 46px;
  font-weight: 800;
  color: #F4F6F4;
  margin: 0 0 20px 0;
  line-height: 1.08;
  letter-spacing: -1.5px;
}

.hero-divider-line {
  width: 36px;
  height: 2px;
  background: #C9A06A;
  margin-bottom: 20px;
  border-radius: 2px;
}

.section-subtitle {
  font-size: 15px;
  color: rgba(220,228,221,0.78);
  line-height: 1.7;
  max-width: 420px;
}

/* ═══════════════════════════════════
   LAYOUT PRINCIPAL
═══════════════════════════════════ */

.form-section {
  padding: 0 24px 72px;
  margin-top: -56px;
  position: relative;
  z-index: 5;
}

.card {
  max-width: 740px;
  margin: 0 auto;
  padding: 48px;
  background: #fff;
  border-radius: 28px;
  border: 1px solid rgba(146, 168, 148, 0.12);
  box-shadow:
    0 2px 4px rgba(58,71,60,0.03),
    0 8px 24px rgba(58,71,60,0.06),
    0 32px 64px rgba(58,71,60,0.06);
}

/* ═══════════════════════════════════
   TARJETA DE MASCOTA
═══════════════════════════════════ */

.aesthetic-pet-card {
  display: flex;
  background: #F4F6F4;
  border-radius: 20px;
  overflow: hidden;
  margin-bottom: 40px;
  border: 1px solid rgba(58,71,60,0.07);
}

.carousel-container {
  position: relative;
  width: 200px;
  height: 200px;
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
  width: 30px;
  height: 30px;
  border-radius: 50%;
  border: none;
  background: rgba(255,255,255,0.92);
  cursor: pointer;
  font-size: 17px;
  font-weight: bold;
  color: #3A473C;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 2px 8px rgba(0,0,0,0.12);
  transition: background 0.2s;
}

.carousel-arrow:hover {
  background: #fff;
}

.pet-card-info {
  padding: 28px 30px;
  display: flex;
  flex-direction: column;
  justify-content: center;
  gap: 6px;
}

.pet-card-tag {
  font-size: 10px;
  letter-spacing: 2px;
  text-transform: uppercase;
  color: #7A876B;
  font-weight: 700;
}

.pet-card-name {
  font-size: 30px;
  font-weight: 800;
  color: #2D372F;
  letter-spacing: -0.8px;
  margin: 2px 0 4px;
  line-height: 1.1;
}

.pet-card-notice {
  font-size: 13px;
  line-height: 1.6;
  color: #5F6A61;
  margin: 0;
}

.pet-card-badge {
  display: inline-flex;
  align-items: center;
  margin-top: 10px;
  padding: 5px 12px;
  background: rgba(201,160,106,0.12);
  color: #A07840;
  border-radius: 100px;
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.5px;
  border: 1px solid rgba(201,160,106,0.25);
  width: fit-content;
}

/* ═══════════════════════════════════
   BLOQUES DE SECCIÓN
═══════════════════════════════════ */

.form-block {
  margin-bottom: 8px;
  border: 1px solid rgba(58,71,60,0.07);
  border-radius: 20px;
  overflow: hidden;
}

.block-header {
  padding: 22px 28px;
  background: #F8FAF8;
  border-bottom: 1px solid rgba(58,71,60,0.06);
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.block-header-left {
  display: flex;
  align-items: center;
  gap: 16px;
}

.block-number {
  font-size: 12px;
  font-weight: 800;
  color: #C9A06A;
  letter-spacing: 1px;
  background: rgba(201,160,106,0.12);
  border: 1px solid rgba(201,160,106,0.2);
  border-radius: 8px;
  padding: 4px 10px;
  flex-shrink: 0;
}

.form-section-title {
  font-size: 16px;
  font-weight: 700;
  color: #2D372F;
  margin: 0;
  line-height: 1.2;
}

.block-subtitle {
  font-size: 12px;
  color: #7A876B;
  margin: 3px 0 0;
  font-weight: 400;
}

.fields-wrapper {
  padding: 28px;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

/* ═══════════════════════════════════
   LABELS E INPUTS
═══════════════════════════════════ */

.field-group {
  display: flex;
  flex-direction: column;
  gap: 0;
}

.form-label {
  display: block;
  margin-bottom: 8px;
  font-size: 13px;
  font-weight: 700;
  color: #3A473C;
  letter-spacing: 0.1px;
}

.req {
  color: #C9A06A;
  font-weight: 700;
  margin-left: 2px;
}

.req-optional {
  color: #9AA49B;
  font-weight: 400;
  font-size: 12px;
  margin-left: 4px;
}

.form-input {
  width: 100%;
  height: 50px;
  border-radius: 12px;
  border: 1.5px solid #DCE4DD;
  background: #FAFAFA;
  padding: 0 16px;
  font-size: 14px;
  color: #2D372F;
  outline: none;
  transition: border-color 0.18s ease, box-shadow 0.18s ease, background 0.18s ease;
  box-sizing: border-box;
}

.form-input::placeholder {
  color: #B0BAB2;
  font-weight: 400;
}

.form-input:hover {
  border-color: #B0C4B2;
  background: #FDFEFE;
}

.form-input:focus {
  border-color: #3A473C;
  background: #fff;
  box-shadow: 0 0 0 3px rgba(58,71,60,0.08);
}

textarea.form-input {
  height: auto;
  min-height: 88px;
  padding: 14px 16px;
  resize: vertical;
  line-height: 1.6;
}

.grid-2-col {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

/* ═══════════════════════════════════
   TELÉFONO
═══════════════════════════════════ */

.phone-group {
  display: flex;
  gap: 8px;
  align-items: flex-start;
}

.code-dropdown-wrapper {
  position: relative;
  flex-shrink: 0;
}

.code-selector-btn {
  height: 50px;
  min-width: 90px;
  border-radius: 12px;
  border: 1.5px solid #DCE4DD;
  background: #FAFAFA;
  font-weight: 700;
  font-size: 13px;
  cursor: pointer;
  color: #2D372F;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  padding: 0 12px;
  transition: border-color 0.18s, background 0.18s;
}

.code-selector-btn:hover {
  border-color: #B0C4B2;
  background: #F4F6F4;
}

.chevron-icon {
  width: 10px;
  height: 6px;
  color: #7A876B;
  flex-shrink: 0;
}

.phone-number-input {
  flex: 1;
}

.code-dropdown-box {
  position: absolute;
  top: calc(100% + 6px);
  left: 0;
  width: 270px;
  background: white;
  border-radius: 16px;
  border: 1.5px solid #DCE4DD;
  overflow: hidden;
  z-index: 200;
  box-shadow: 0 8px 30px rgba(0,0,0,0.10);
}

.search-box-container {
  padding: 10px;
  border-bottom: 1px solid #ECEFEC;
}

.dropdown-search-input {
  width: 100%;
  height: 38px;
  border-radius: 10px;
  border: 1.5px solid #DCE4DD;
  padding: 0 12px;
  font-size: 13px;
  outline: none;
  background: #FAFAFA;
  color: #2D372F;
  box-sizing: border-box;
}

.dropdown-search-input:focus {
  border-color: #3A473C;
}

.code-results-list {
  max-height: 220px;
  overflow-y: auto;
  list-style: none;
  margin: 0;
  padding: 4px 0;
}

.code-results-list li {
  padding: 10px 14px;
  cursor: pointer;
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 13px;
  color: #3A473C;
  transition: background 0.15s;
}

.code-results-list li:hover {
  background: #F4F6F4;
}

.code-results-list li strong {
  color: #C9A06A;
  font-weight: 700;
}

/* ═══════════════════════════════════
   CHECKBOX
═══════════════════════════════════ */

.checkbox-wrapper {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-top: 12px;
}

.checkbox-label {
  font-size: 13px;
  color: #5F6A61;
  cursor: pointer;
  user-select: none;
}

/* ═══════════════════════════════════
   RADIO CARDS
═══════════════════════════════════ */

.radio-cards-group {
  display: flex;
  gap: 12px;
  margin-top: 2px;
}

.radio-card {
  flex: 1;
  padding: 14px 20px;
  border-radius: 12px;
  border: 1.5px solid #DCE4DD;
  background: #FAFAFA;
  cursor: pointer;
  transition: border-color 0.18s, background 0.18s, color 0.18s;
  font-size: 14px;
  font-weight: 600;
  color: #7A876B;
  text-align: center;
}

.radio-card:hover {
  border-color: #B0C4B2;
  background: #F4F6F4;
}

.radio-card.active {
  border-color: #3A473C;
  background: #EEF2EE;
  color: #2D372F;
  font-weight: 700;
}

/* ═══════════════════════════════════
   CAMPO DINÁMICO (PAÍS)
═══════════════════════════════════ */

.dynamic-field {
  background: #F8FAF8;
  padding: 20px;
  border-radius: 14px;
  border: 1px solid rgba(58,71,60,0.07);
}

.autocomplete-wrapper {
  position: relative;
}

.autocomplete-results {
  position: absolute;
  top: calc(100% + 4px);
  left: 0;
  width: 100%;
  background: white;
  border-radius: 12px;
  border: 1.5px solid #DCE4DD;
  max-height: 200px;
  overflow-y: auto;
  z-index: 100;
  box-shadow: 0 8px 24px rgba(0,0,0,0.08);
  list-style: none;
  padding: 4px 0;
  margin: 0;
}

.autocomplete-results li {
  padding: 11px 14px;
  cursor: pointer;
  font-size: 13px;
  color: #3A473C;
  transition: background 0.15s;
}

.autocomplete-results li:hover {
  background: #F4F6F4;
}

/* ═══════════════════════════════════
   TÉRMINOS Y CONDICIONES — TOOLTIP
═══════════════════════════════════ */

.tc-wrapper {
  margin-bottom: 24px;
  padding: 20px 24px;
  background: #F8FAF8;
  border-radius: 14px;
  border: 1px solid rgba(58,71,60,0.08);
}

.tc-checkbox-row {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  cursor: pointer;
}

.tc-checkbox {
  width: 17px;
  height: 17px;
  flex-shrink: 0;
  margin-top: 2px;
  cursor: pointer;
  accent-color: #3A473C;
}

.tc-checkbox-label {
  font-size: 13px;
  font-weight: 600;
  color: #3A473C;
  line-height: 1.6;
  cursor: pointer;
  user-select: none;
}

.tc-tooltip-anchor {
  position: relative;
  display: inline;
}

.tc-trigger {
  color: #3A473C;
  text-decoration: underline;
  text-decoration-color: #C9A06A;
  text-underline-offset: 3px;
  text-decoration-thickness: 1.5px;
  cursor: pointer;
  font-weight: 700;
}

.tc-tooltip {
  display: none;
  position: absolute;
  bottom: calc(100% + 10px);
  left: 50%;
  transform: translateX(-50%);
  width: 320px;
  background: #2D372F;
  border-radius: 14px;
  padding: 18px 20px;
  z-index: 300;
  box-shadow: 0 8px 32px rgba(0,0,0,0.18);
  pointer-events: none;
}

.tc-tooltip::after {
  content: '';
  position: absolute;
  top: 100%;
  left: 50%;
  transform: translateX(-50%);
  border: 7px solid transparent;
  border-top-color: #2D372F;
}

.tc-tooltip-title {
  display: block;
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 1px;
  text-transform: uppercase;
  color: #C9A06A;
  margin-bottom: 12px;
}

.tc-tooltip-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.tc-tooltip-list li {
  font-size: 12px;
  color: rgba(220,228,221,0.88);
  line-height: 1.55;
  padding-left: 14px;
  position: relative;
}

.tc-tooltip-list li::before {
  content: '';
  position: absolute;
  left: 0;
  top: 7px;
  width: 4px;
  height: 4px;
  border-radius: 50%;
  background: #C9A06A;
}

.tc-tooltip-anchor:hover .tc-tooltip,
.tc-tooltip-anchor:focus-within .tc-tooltip {
  display: block;
}

/* ═══════════════════════════════════
   BOTONES
═══════════════════════════════════ */

.actions-group {
  display: flex;
  gap: 12px;
}

.pet-btn,
.pet-btn-outline {
  flex: 1;
  height: 54px;
  border-radius: 14px;
  font-size: 15px;
  font-weight: 700;
  cursor: pointer;
  transition: background 0.2s ease, color 0.2s ease, border-color 0.2s ease, transform 0.15s ease, box-shadow 0.2s ease;
  letter-spacing: 0.2px;
}

.pet-btn {
  border: none;
  background: #3A473C;
  color: #fff;
  box-shadow: 0 4px 14px rgba(58,71,60,0.22);
}

.pet-btn:hover:not(:disabled) {
  background: #2D372F;
  transform: translateY(-1px);
  box-shadow: 0 6px 20px rgba(58,71,60,0.28);
}

.pet-btn:active:not(:disabled) {
  transform: translateY(0);
}

.pet-btn:disabled {
  background: #B0BAB2;
  box-shadow: none;
  cursor: not-allowed;
}

.pet-btn-outline {
  background: transparent;
  border: 2px solid #DCE4DD;
  color: #3A473C;
}

.pet-btn-outline:hover {
  border-color: #3A473C;
  background: #F4F6F4;
}

/* ═══════════════════════════════════
   PANTALLA ÉXITO
═══════════════════════════════════ */

.success-screen {
  text-align: center;
  padding: 40px 20px;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 16px;
}

.success-icon-wrap svg {
  width: 64px;
  height: 64px;
  margin-bottom: 8px;
}

.success-screen h3 {
  font-size: 26px;
  font-weight: 800;
  color: #2D372F;
  margin: 0;
}

.success-screen p {
  font-size: 15px;
  color: #5F6A61;
  margin: 0;
}

/* ═══════════════════════════════════
   RESPONSIVE
═══════════════════════════════════ */

@media (max-width: 768px) {

  .page-hero {
    height: 380px;
  }

  .page-hero h1 {
    font-size: 32px;
    letter-spacing: -0.8px;
  }



  .fields-wrapper {
    padding: 20px;
    gap: 16px;
  }

  .card {
    padding: 24px 20px;
    border-radius: 20px;
  }

  .block-header {
    padding: 18px 20px;
  }

  .grid-2-col {
    grid-template-columns: 1fr;
  }

  .actions-group {
    flex-direction: column;
  }

  .carousel-container {
    width: 100%;
    height: 240px;
  }

  .pet-card-info {
    padding: 22px 20px;
  }

}

@media (max-width: 480px) {

  .form-section {
    padding: 0 12px 48px;
  }

  .page-hero h1 {
    font-size: 26px;
  }

  .block-number {
    display: none;
  }

}


/* ── MOBILE RESPONSIVE ── */
@media (max-width: 768px) {
  .page-hero {
    height: 300px;
    padding: 0 16px;
  }

  .page-hero h1 {
    font-size: 30px;
    letter-spacing: -0.5px;
    margin: 0 0 14px;
  }

  .section-subtitle {
    font-size: 13px;
  }

  .form-section {
    padding: 0 12px 48px;
    margin-top: -40px;
  }

  .card {
    padding: 24px 16px;
    border-radius: 18px;
  }

  .aesthetic-pet-card {
    flex-direction: column;
  }

  .carousel-container {
    width: 100%;
    height: 200px;
    flex-shrink: 0;
  }

  .pet-card-info {
    padding: 18px 16px 20px;
  }

  .pet-card-name {
    font-size: 22px;
  }

  .block-header {
    padding: 14px 16px;
  }

  .block-header-left {
    gap: 10px;
  }

  .form-section-title {
    font-size: 14px;
  }

  .fields-wrapper {
    padding: 16px;
    gap: 14px;
  }

  .grid-2-col {
    grid-template-columns: 1fr;
    gap: 14px;
  }

  .phone-group {
    flex-direction: row;
    gap: 8px;
  }

  .code-selector-btn {
    min-width: 80px;
    height: 46px;
    font-size: 12px;
  }

  .form-input {
    height: 46px;
    font-size: 13px;
  }

  .code-dropdown-box {
    width: calc(100vw - 48px);
    max-width: 300px;
  }

  .radio-cards-group {
    gap: 8px;
  }

  .radio-card {
    padding: 12px 14px;
    font-size: 13px;
  }

  .actions-group {
    flex-direction: column;
    gap: 10px;
  }

  .pet-btn,
  .pet-btn-outline {
    height: 50px;
    font-size: 14px;
  }

  .tc-wrapper {
    padding: 14px 16px;
  }

  .tc-tooltip {
    width: min(280px, calc(100vw - 32px));
    left: 0;
    transform: translateX(0);
    position: fixed;
    bottom: 80px;
    top: auto;
  }

  .tc-tooltip::after {
    display: none;
  }
}

@media (max-width: 480px) {
  .page-hero h1 {
    font-size: 24px;
  }

  .hero-tag {
    font-size: 9px;
    letter-spacing: 2px;
    padding: 5px 12px;
  }

  .block-number {
    display: none;
  }

  .card {
    padding: 18px 12px;
  }
}

</style>