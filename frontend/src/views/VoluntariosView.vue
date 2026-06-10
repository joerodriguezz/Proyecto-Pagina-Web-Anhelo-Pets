<script setup>
import {
  ref,
  computed,
  onMounted,
  onBeforeUnmount
} from 'vue'
import NavBar from '../components/NavBar.vue'
import FooterBar from '../components/FooterBar.vue'

const usuarioActivo = ref(
  JSON.parse(
    localStorage.getItem('anhelo_usuario_actual')
  )
)

/* BENEFICIOS */

const benefits = [
  {
    icon: 'bx bxs-heart',
    title: 'Apoyo veterinario',
    text: 'La fundación cubre controles y atención médica.'
  },
  {
    icon: 'bx bxs-bowl-hot',
    title: 'Alimento incluido',
    text: 'Nosotros proporcionamos comida y suministros.'
  },
  {
    icon: 'bx bxs-shield-plus',
    title: 'Seguimiento constante',
    text: 'Acompañamiento durante todo el proceso.'
  },
  {
    icon: 'bx bxs-home-heart',
    title: 'Impacto real',
    text: 'Ayudas directamente a rescates y adopciones.'
  }
]

/* FORM */

const fullName = ref('')
const idCard = ref('')
const email = ref('')
const phone = ref('')
const address = ref('')
const volunteerType = ref('')
const motivation = ref('')

const submitted = ref(false)

const loggedIn = computed(() => usuarioActivo.value !== null)

const solicitudActual = computed(() =>
  usuarioActivo.value?.solicitudVoluntario || null
)

/* TELÉFONO */

const dropdownRef = ref(null)
const showCodeDropdown = ref(false)
const codeSearch = ref('')

const selectedCountry = ref({
  name: 'Costa Rica',
  code: '+506'
})

const phoneCodesList = [
  { name: 'Afganistán', code: '+93' },
  { name: 'Albania', code: '+355' },
  { name: 'Alemania', code: '+49' },
  { name: 'Andorra', code: '+376' },
  { name: 'Angola', code: '+244' },
  { name: 'Antigua y Barbuda', code: '+1-268' },
  { name: 'Arabia Saudita', code: '+966' },
  { name: 'Argelia', code: '+213' },
  { name: 'Argentina', code: '+54' },
  { name: 'Armenia', code: '+374' },
  { name: 'Australia', code: '+61' },
  { name: 'Austria', code: '+43' },
  { name: 'Azerbaiyán', code: '+994' },
  { name: 'Bahamas', code: '+1-242' },
  { name: 'Bangladés', code: '+880' },
  { name: 'Barbados', code: '+1-246' },
  { name: 'Baréin', code: '+973' },
  { name: 'Bélgica', code: '+32' },
  { name: 'Belice', code: '+501' },
  { name: 'Benín', code: '+229' },
  { name: 'Bielorrusia', code: '+375' },
  { name: 'Bolivia', code: '+591' },
  { name: 'Bosnia y Herzegovina', code: '+387' },
  { name: 'Brasil', code: '+55' },
  { name: 'Brunéi', code: '+673' },
  { name: 'Bulgaria', code: '+359' },
  { name: 'Camboya', code: '+855' },
  { name: 'Camerún', code: '+237' },
  { name: 'Canadá', code: '+1' },
  { name: 'Catar', code: '+974' },
  { name: 'Chile', code: '+56' },
  { name: 'China', code: '+86' },
  { name: 'Chipre', code: '+357' },
  { name: 'Colombia', code: '+57' },
  { name: 'Corea del Sur', code: '+82' },
  { name: 'Costa Rica', code: '+506' },
  { name: 'Croacia', code: '+385' },
  { name: 'Cuba', code: '+53' },
  { name: 'Dinamarca', code: '+45' },
  { name: 'Ecuador', code: '+593' },
  { name: 'Egipto', code: '+20' },
  { name: 'El Salvador', code: '+503' },
  { name: 'Emiratos Árabes Unidos', code: '+971' },
  { name: 'Eslovaquia', code: '+421' },
  { name: 'Eslovenia', code: '+386' },
  { name: 'España', code: '+34' },
  { name: 'Estados Unidos', code: '+1' },
  { name: 'Estonia', code: '+372' },
  { name: 'Etiopía', code: '+251' },
  { name: 'Filipinas', code: '+63' },
  { name: 'Finlandia', code: '+358' },
  { name: 'Francia', code: '+33' },
  { name: 'Georgia', code: '+995' },
  { name: 'Grecia', code: '+30' },
  { name: 'Guatemala', code: '+502' },
  { name: 'Haití', code: '+509' },
  { name: 'Honduras', code: '+504' },
  { name: 'Hungría', code: '+36' },
  { name: 'India', code: '+91' },
  { name: 'Indonesia', code: '+62' },
  { name: 'Irak', code: '+964' },
  { name: 'Irán', code: '+98' },
  { name: 'Irlanda', code: '+353' },
  { name: 'Islandia', code: '+354' },
  { name: 'Israel', code: '+972' },
  { name: 'Italia', code: '+39' },
  { name: 'Jamaica', code: '+1-876' },
  { name: 'Japón', code: '+81' },
  { name: 'Jordania', code: '+962' },
  { name: 'Kazajistán', code: '+7' },
  { name: 'Kenia', code: '+254' },
  { name: 'Kuwait', code: '+965' },
  { name: 'Laos', code: '+856' },
  { name: 'Letonia', code: '+371' },
  { name: 'Líbano', code: '+961' },
  { name: 'Libia', code: '+218' },
  { name: 'Lituania', code: '+370' },
  { name: 'Luxemburgo', code: '+352' },
  { name: 'Madagascar', code: '+261' },
  { name: 'Malasia', code: '+60' },
  { name: 'México', code: '+52' },
  { name: 'Marruecos', code: '+212' },
  { name: 'Nepal', code: '+977' },
  { name: 'Nicaragua', code: '+505' },
  { name: 'Nigeria', code: '+234' },
  { name: 'Noruega', code: '+47' },
  { name: 'Nueva Zelanda', code: '+64' },
  { name: 'Omán', code: '+968' },
  { name: 'Países Bajos', code: '+31' },
  { name: 'Pakistán', code: '+92' },
  { name: 'Panamá', code: '+507' },
  { name: 'Paraguay', code: '+595' },
  { name: 'Perú', code: '+51' },
  { name: 'Polonia', code: '+48' },
  { name: 'Portugal', code: '+351' },
  { name: 'Reino Unido', code: '+44' },
  { name: 'República Dominicana', code: '+1-809' },
  { name: 'Rumania', code: '+40' },
  { name: 'Rusia', code: '+7' },
  { name: 'Singapur', code: '+65' },
  { name: 'Sudáfrica', code: '+27' },
  { name: 'Suecia', code: '+46' },
  { name: 'Suiza', code: '+41' },
  { name: 'Tailandia', code: '+66' },
  { name: 'Turquía', code: '+90' },
  { name: 'Ucrania', code: '+380' },
  { name: 'Uganda', code: '+256' },
  { name: 'Uruguay', code: '+598' },
  { name: 'Venezuela', code: '+58' },
  { name: 'Vietnam', code: '+84' },
  { name: 'Yemen', code: '+967' },
  { name: 'Zambia', code: '+260' },
  { name: 'Zimbabue', code: '+263' }
]

const filteredCodes = computed(() => {
  const query = codeSearch.value.toLowerCase()
  return phoneCodesList.filter(item =>
    item.name.toLowerCase().includes(query) ||
    item.code.includes(query)
  )
})

function selectCode(item) {
  selectedCountry.value = item
  showCodeDropdown.value = false
  codeSearch.value = ''
}

function filterPhoneInput() {
  phone.value = phone.value.replace(/[^0-9]/g, '')
}

function handleClickOutside(event) {
  if (
    dropdownRef.value &&
    !dropdownRef.value.contains(event.target)
  ) {
    showCodeDropdown.value = false
  }
}

onBeforeUnmount(() => {
  document.removeEventListener('click', handleClickOutside)
})

/* AUTOCOMPLETAR */

onMounted(() => {
  document.addEventListener('click', handleClickOutside)

  if (usuarioActivo.value) {
    fullName.value  = usuarioActivo.value.nombre    || ''
    idCard.value    = usuarioActivo.value.cedula    || ''
    email.value     = usuarioActivo.value.correo    || ''
    address.value   = usuarioActivo.value.direccion || ''

    if (usuarioActivo.value.telefono) {
      const telefonoCompleto = usuarioActivo.value.telefono
      const partes = telefonoCompleto.split(' ')

      if (partes.length >= 2) {
        const codigoEncontrado = phoneCodesList.find(
          c => c.code === partes[0]
        )
        if (codigoEncontrado) {
          selectedCountry.value = codigoEncontrado
        } else {
          selectedCountry.value = { name: '', code: partes[0] }
        }
        phone.value = partes.slice(1).join(' ')
      }
    }
  }
})

/* GUARDAR */

function submitVolunteer() {
  const usuarios = JSON.parse(
    localStorage.getItem('anhelo_usuarios')
  ) || []

  const usuarioIndex = usuarios.findIndex(u =>
    u.correo === usuarioActivo.value.correo
  )

  if (usuarioIndex !== -1) {
    const telefonoCompleto =
      `${selectedCountry.value.code} ${phone.value}`

    usuarios[usuarioIndex].solicitudVoluntario = {
      nombre:     fullName.value,
      cedula:     idCard.value,
      correo:     email.value,
      telefono:   telefonoCompleto,
      direccion:  address.value,
      tipo:       volunteerType.value,
      motivacion: motivation.value,
      estado:     'Pendiente'
    }

    // Actualizar cedula y dirección directamente en el usuario
    usuarios[usuarioIndex].cedula    = idCard.value
    usuarios[usuarioIndex].direccion = address.value
    usuarios[usuarioIndex].telefono  = telefonoCompleto

    localStorage.setItem(
      'anhelo_usuarios',
      JSON.stringify(usuarios)
    )

    localStorage.setItem(
      'anhelo_usuario_actual',
      JSON.stringify(usuarios[usuarioIndex])
    )

    usuarioActivo.value = usuarios[usuarioIndex]
  }

  submitted.value = true
}
</script>

<template>
  <NavBar />

  <!-- HERO -->
  <section class="hero">
    <img
      src="/img-vol/Voluntarios.JPG"
      class="hero-image"
      alt="Voluntarios"
    >
    <div class="hero-overlay"></div>
    <div class="hero-content">
      <div class="hero-text">
        <h1>Sé parte de una segunda oportunidad</h1>
        <p>
          Forma parte de nuestra red de voluntarios
          y hogares temporales para brindar amor,
          cuidado y nuevas oportunidades.
        </p>
      </div>
    </div>
  </section>

  <!-- SECTION -->
  <section class="volunteer-section">
    <div class="container volunteer-grid">

      <!-- LEFT -->
      <div class="left-side">
        <div class="intro-card">
          <div class="intro-image-wrap">
            <img
              src="/img-vol/Voluntariado.JPG"
              class="intro-image"
            >
          </div>
          <div class="intro-content">
            <h2>¿Qué es una casa cuna?</h2>
            <p>
              Un hogar temporal que recibe mascotas
              rescatadas mientras encuentran una
              familia definitiva y segura.
            </p>
          </div>
        </div>

        <!-- BENEFITS -->
        <div class="benefits-grid">
          <div
            class="benefit-card"
            v-for="item in benefits"
            :key="item.title"
          >
            <div class="benefit-icon">
              <i :class="item.icon"></i>
            </div>
            <div>
              <h3>{{ item.title }}</h3>
              <p>{{ item.text }}</p>
            </div>
          </div>
        </div>
      </div>

      <!-- FORM -->
      <div class="vol-form">

        <!-- No logueado -->
        <div v-if="!loggedIn" class="login-warning">
          Debes iniciar sesión para enviar una solicitud de voluntariado.
        </div>

        <!-- Ya tiene solicitud -->
        <div
          v-else-if="solicitudActual"
          class="request-status"
        >
          <h3>Estado de tu solicitud</h3>

          <p>
            Estado actual:
            <span
              class="badge"
              :class="
                solicitudActual.estado === 'Aprobada'
                  ? 'badge-green'
                  : solicitudActual.estado === 'Rechazada'
                    ? 'badge-red'
                    : 'badge-peach'
              "
            >
              {{ solicitudActual.estado }}
            </span>
          </p>

          <p v-if="solicitudActual.estado === 'Pendiente'">
            Nuestro equipo está revisando tu solicitud.
          </p>
          <p v-if="solicitudActual.estado === 'Aprobada'">
            Ya formas parte del equipo de voluntarios.
          </p>
          <p v-if="solicitudActual.estado === 'Rechazada'">
            Tu solicitud no fue aprobada actualmente.
          </p>
        </div>

        <!-- Formulario disponible -->
        <div
          v-else-if="!submitted && loggedIn && !solicitudActual"
        >
          <div class="form-top">
            <h2>Registro de voluntario</h2>
            <p>Completa el formulario y nos pondremos en contacto contigo.</p>
          </div>

          <!-- ROW -->
          <div class="form-row">
            <div class="form-group">
              <label>Nombre completo</label>
              <input
                v-model="fullName"
                type="text"
                placeholder="Ej. María González"
                disabled
              >
            </div>
            <div class="form-group">
              <label>Cédula</label>
              <input
                v-model="idCard"
                type="text"
                placeholder="1-2345-6789"
                disabled
              >
            </div>
          </div>

          <!-- ROW -->
          <div class="form-row">
            <div class="form-group">
              <label>Correo electrónico</label>
              <input
                v-model="email"
                type="email"
                placeholder="correo@ejemplo.com"
                disabled
              >
            </div>
            <div class="form-group">
              <label>Teléfono</label>
              <div class="phone-wrapper">
                <div class="phone-code-selector" ref="dropdownRef">
                  <button
                    type="button"
                    class="code-btn"
                    @click="showCodeDropdown = !showCodeDropdown"
                  >
                    <span>{{ selectedCountry.code }}</span>
                    <i class='bx bx-chevron-down'></i>
                  </button>

                  <div v-if="showCodeDropdown" class="code-dropdown">
                    <div class="dropdown-search">
                      <input
                        v-model="codeSearch"
                        type="text"
                        placeholder="Buscar país..."
                      >
                    </div>
                    <div class="dropdown-list">
                      <div
                        v-for="item in filteredCodes"
                        :key="item.name"
                        class="country-option"
                        @click="selectCode(item)"
                      >
                        <span>{{ item.name }}</span>
                        <strong>{{ item.code }}</strong>
                      </div>
                    </div>
                  </div>
                </div>

                <input
                  v-model="phone"
                  type="text"
                  class="phone-input"
                  placeholder="88888888"
                  @input="filterPhoneInput"
                >
              </div>
            </div>
          </div>

          <!-- ADDRESS -->
          <div class="form-group">
            <label>Dirección</label>
            <input
              v-model="address"
              type="text"
              placeholder="Provincia, cantón y distrito"
            >
          </div>

          <!-- SELECT -->
          <div class="form-group">
            <label>Tipo de voluntariado</label>
            <div class="custom-select-wrap">
              <select v-model="volunteerType" class="custom-select">
                <option value="">Seleccione una opción</option>
                <option>Casa cuna</option>
                <option>Eventos de adopción</option>
                <option>Transporte</option>
                <option>Veterinaria</option>
                <option>Redes sociales</option>
                <option>Rescatista</option>
              </select>
              <i class='bx bx-chevron-down'></i>
            </div>
          </div>

          <!-- MOTIVATION -->
          <div class="form-group">
            <label>Motivación</label>
            <textarea
              v-model="motivation"
              placeholder="Cuéntanos por qué deseas ayudar..."
            ></textarea>
          </div>

          <!-- BUTTON -->
          <button
            class="submit-btn"
            @click="submitVolunteer"
            :disabled="
              !fullName ||
              !idCard ||
              !email ||
              !phone ||
              !address ||
              !volunteerType ||
              !motivation
            "
          >
            Registrarme como voluntario
          </button>
        </div>

        <!-- SUCCESS -->
        <div v-else class="success-box">
          <i class='bx bxs-check-circle'></i>
          <h3>Registro enviado</h3>
          <p>
            Gracias por formar parte de Anhelo Pets.
            Nuestro equipo revisará tu solicitud.
          </p>
        </div>

      </div>
    </div>
  </section>

  <FooterBar />
</template>

<style scoped>
.login-warning {
  background: rgba(249,193,122,0.16);
  border: 1px solid rgba(249,193,122,0.4);
  color: #9A6A2E;
  padding: 24px;
  border-radius: 20px;
  font-weight: 700;
  text-align: center;
  line-height: 1.6;
}

/* HERO */
.hero {
  position: relative;
  height: 520px;
  overflow: hidden;
  background: #FAFAFA;
}

.hero-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
  object-position: center 52%;
  filter: brightness(0.88) contrast(1.02);
  transform: scale(1.03);
}

.hero-overlay {
  position: absolute;
  inset: 0;
  background: linear-gradient(
    90deg,
    rgba(58,71,60,0.58) 8%,
    rgba(58,71,60,0.20) 55%,
    rgba(58,71,60,0.04) 100%
  );
}

.hero-content {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  padding: 0 8%;
}

.hero-text {
  max-width: 560px;
  color: white;
}

.hero-text h1 {
  font-size: 68px;
  line-height: 0.92;
  font-weight: 800;
  letter-spacing: -4px;
  margin-bottom: 22px;
}

.hero-text p {
  font-size: 17px;
  line-height: 1.9;
  color: rgba(255,255,255,0.90);
}

/* SECTION */
.volunteer-section {
  background: #FAFAFA;
  padding: 90px 24px 120px;
}

.container {
  max-width: 1180px;
  margin: auto;
}

.volunteer-grid {
  display: grid;
  grid-template-columns: 0.9fr 0.82fr;
  gap: 24px;
  align-items: start;
}

/* LEFT */
.left-side {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

/* INTRO */
.intro-card {
  background: white;
  border-radius: 34px;
  overflow: hidden;
  border: 1px solid rgba(146,168,148,0.10);
  box-shadow: 0 14px 40px rgba(0,0,0,0.04);
}

.intro-image-wrap {
  height: 250px;
  overflow: hidden;
}

.intro-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.intro-content {
  padding: 34px;
}

.intro-content h2 {
  font-size: 44px;
  line-height: 1;
  font-weight: 800;
  color: #3A473C;
  margin-bottom: 18px;
}

.intro-content p {
  font-size: 15px;
  line-height: 1.9;
  color: #667085;
}

/* BENEFITS */
.benefits-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.benefit-card {
  background: white;
  border-radius: 24px;
  padding: 20px;
  display: flex;
  gap: 14px;
  min-height: 140px;
  border: 1px solid rgba(146,168,148,0.10);
}

.benefit-icon {
  width: 50px;
  height: 50px;
  border-radius: 16px;
  background: rgba(146,168,148,0.14);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.benefit-icon i {
  font-size: 24px;
  color: #92A894;
}

.benefit-card h3 {
  font-size: 16px;
  font-weight: 800;
  color: #3A473C;
  margin-bottom: 8px;
}

.benefit-card p {
  font-size: 14px;
  line-height: 1.7;
  color: #667085;
}

/* FORM */
.vol-form {
  background: white;
  border-radius: 30px;
  padding: 30px;
  width: 100%;
  max-width: 560px;
  min-width: 0;
  overflow: visible;
  box-shadow: 0 10px 28px rgba(0,0,0,0.04);
  border: 1px solid rgba(146,168,148,0.10);
}

.form-top {
  margin-bottom: 30px;
}

.form-top h2 {
  font-size: 38px;
  font-weight: 800;
  color: #3A473C;
  margin: 0 0 10px;
}

.form-top p {
  color: #667085;
  line-height: 1.7;
}

/* ROWS */
.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 18px;
}

.form-group {
  display: flex;
  flex-direction: column;
  margin-bottom: 18px;
}

.form-group label {
  font-size: 14px;
  font-weight: 700;
  color: #3A473C;
  margin-bottom: 8px;
}

/* INPUTS */
.form-group input,
.custom-select,
.form-group textarea {
  width: 100%;
  min-width: 0;
  height: 54px;
  border: 1px solid #E5ECE6;
  border-radius: 16px;
  padding: 0 16px;
  background: #FCFCFC;
  font-size: 14px;
  color: #3A473C;
  outline: none;
  box-sizing: border-box;
}

.form-group input:disabled {
  background: #F4F6F4;
  color: #8A9C8C;
  cursor: not-allowed;
}

.form-group textarea {
  min-height: 120px;
  padding-top: 14px;
  resize: vertical;
  height: auto;
}

/* PHONE */
.phone-wrapper {
  display: flex;
  align-items: center;
  gap: 10px;
  width: 100%;
}

.phone-code-selector {
  position: relative;
  width: 110px;
  min-width: 110px;
  height: 54px;
  flex-shrink: 0;
}

.code-btn {
  width: 100px;
  min-width: 100px;
  height: 54px;
  padding: 0 14px;
  border-radius: 16px;
  border: 1px solid #E5ECE6;
  background: #FCFCFC;
  display: flex;
  align-items: center;
  justify-content: space-between;
  box-sizing: border-box;
  flex-shrink: 0;
  color: #000;
  font-weight: 700;
  cursor: pointer;
}

.phone-input {
  flex: 1;
  width: 100%;
  min-width: 0;
}

/* DROPDOWN */
.code-dropdown {
  position: absolute;
  top: 62px;
  left: 0;
  width: 250px;
  background: white;
  border-radius: 20px;
  border: 1px solid #E7ECE8;
  overflow: hidden;
  z-index: 999;
  box-shadow: 0 18px 40px rgba(0,0,0,0.08);
}

.dropdown-search {
  padding: 12px;
}

.dropdown-search input {
  width: 100%;
  height: 46px;
  border: 1px solid #E5ECE6;
  border-radius: 14px;
  padding: 0 14px;
  outline: none;
}

.dropdown-list {
  max-height: 240px;
  overflow-y: auto;
}

.country-option {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 16px;
  cursor: pointer;
  transition: 0.2s;
}

.country-option:hover {
  background: #F7F9F7;
}

.country-option span {
  font-size: 14px;
  color: #3A473C;
}

.country-option strong {
  color: #3A473C;
  font-size: 14px;
  font-weight: 400;
}

/* SELECT */
.custom-select-wrap {
  position: relative;
}

.custom-select {
  appearance: none;
  cursor: pointer;
}

.custom-select-wrap i {
  position: absolute;
  right: 18px;
  top: 50%;
  transform: translateY(-50%);
  font-size: 22px;
  color: #6C756D;
  pointer-events: none;
}

/* BUTTON */
.submit-btn {
  width: 100%;
  height: 54px;
  border: none;
  border-radius: 16px;
  background: linear-gradient(135deg, #92A894 0%, #7E947F 100%);
  color: white;
  font-size: 14px;
  font-weight: 700;
  margin-top: 6px;
  cursor: pointer;
}

.submit-btn:disabled {
  opacity: 0.45;
  cursor: not-allowed;
}

/* REQUEST STATUS */
.request-status {
  padding: 30px 10px;
}

.request-status h3 {
  font-size: 28px;
  font-weight: 800;
  color: #3A473C;
  margin-bottom: 20px;
}

.request-status p {
  color: #667085;
  line-height: 1.8;
  display: flex;
  align-items: center;
  gap: 10px;
}

/* BADGES */
.badge {
  padding: 6px 14px;
  border-radius: 999px;
  font-size: 13px;
  font-weight: 700;
  display: inline-block;
}

.badge-green {
  background: rgba(146,168,148,0.18);
  color: #5A6E5C;
}

.badge-red {
  background: rgba(235,119,119,0.16);
  color: #C45252;
}

.badge-peach {
  background: rgba(249,193,122,0.18);
  color: #D18C3A;
}

/* SUCCESS */
.success-box {
  text-align: center;
  padding: 40px 10px;
}

.success-box i {
  font-size: 70px;
  color: #92A894;
  margin-bottom: 18px;
  display: block;
}

.success-box h3 {
  font-size: 32px;
  color: #3A473C;
  margin-bottom: 10px;
}

.success-box p {
  color: #667085;
  line-height: 1.8;
}

/* RESPONSIVE */
@media (max-width: 980px) {
  .volunteer-grid {
    grid-template-columns: 1fr;
  }
  .vol-form {
    max-width: 100%;
  }
}

@media (max-width: 700px) {
  .hero {
    height: 440px;
  }
  .hero-text h1 {
    font-size: 42px;
  }
  .benefits-grid {
    grid-template-columns: 1fr;
  }
  .form-row {
    grid-template-columns: 1fr;
  }
  .code-dropdown {
    width: 100%;
  }
  .vol-form {
    padding: 24px;
  }
}
</style>