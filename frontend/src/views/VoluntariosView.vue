<script setup>
import { computed, ref } from 'vue'
import { RouterLink } from 'vue-router'
import NavBar from '../components/NavBar.vue'
import FooterBar from '../components/FooterBar.vue'
import { volunteersApi } from '../services/api'

const storedUser = localStorage.getItem('authUser')
const authUser = ref(storedUser ? JSON.parse(storedUser) : null)
const loading = ref(false)
const successMessage = ref('')
const errorMessage = ref('')

const volunteerForm = ref({
  nationalId: '',
  volunteerType: '',
  motivation: '',
})

const isLoggedIn = computed(() => Boolean(authUser.value?.userId))

async function registerVolunteer() {
  if (!isLoggedIn.value) {
    errorMessage.value = 'Debes iniciar sesion antes de registrarte como voluntario.'
    return
  }

  loading.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    await volunteersApi.create({
      userId: authUser.value.userId,
      nationalId: volunteerForm.value.nationalId,
      volunteerType: volunteerForm.value.volunteerType,
      motivation: volunteerForm.value.motivation,
      createdBy: 'frontend',
    })

    successMessage.value = 'Solicitud de voluntariado enviada para validacion.'
    volunteerForm.value = {
      nationalId: '',
      volunteerType: '',
      motivation: '',
    }
  } catch (error) {
    errorMessage.value = error.message || 'No se pudo registrar la solicitud.'
  } finally {
    loading.value = false
  }
}

const benefits = [

  {
    icon:'bx bxs-heart',
    title:'Apoyo veterinario',
    text:'La fundación cubre controles y atención médica.'
  },

  {
    icon:'bx bxs-bowl-hot',
    title:'Alimento incluido',
    text:'Nosotros proporcionamos comida y suministros.'
  },

  {
    icon:'bx bxs-shield-plus',
    title:'Seguimiento constante',
    text:'Acompañamiento durante todo el proceso.'
  },

  {
    icon:'bx bxs-home-heart',
    title:'Impacto real',
    text:'Ayudas directamente a rescates y adopciones.'
  }

]
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

        <h1>
          Sé parte de una
          segunda oportunidad
        </h1>

        <p>
          Forma parte de nuestra red de voluntarios
          y hogares temporales para brindar amor,
          cuidado y nuevas oportunidades.
        </p>

      </div>

    </div>

  </section>

  <!-- CONTENT -->

  <section class="volunteer-section">

    <div class="container volunteer-grid">

      <!-- LEFT -->

      <div class="left-side">

        <!-- INTRO -->

        <div class="intro-card">

          <div class="intro-image-wrap">

            <img
              src="/img-vol/Voluntariado.JPG"
              class="intro-image"
            >

          </div>

          <div class="intro-content">

            <span>
              HOGARES TEMPORALES
            </span>

            <h2>
              ¿Qué es una
              casa cuna?
            </h2>

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

              <h3>
                {{ item.title }}
              </h3>

              <p>
                {{ item.text }}
              </p>

            </div>

          </div>

        </div>

      </div>

      <!-- FORM -->

      <form class="vol-form" @submit.prevent="registerVolunteer">

        <div class="form-top">

          <span>
            REGISTRO
          </span>

          <h2>
            Registro de voluntario
          </h2>

          <p>
            Esta solicitud se asocia a tu cuenta registrada para que podamos validarla.
          </p>

        </div>

        <div v-if="!isLoggedIn" class="login-notice">
          Para registrarte como voluntario primero debes
          <RouterLink to="/login">iniciar sesion</RouterLink>.
        </div>

        <div class="form-group">

          <label>
            Cedula
          </label>

          <input
            v-model="volunteerForm.nationalId"
            type="text"
            placeholder="1-2345-6789"
            :disabled="!isLoggedIn"
            required
          >

        </div>

        <div class="form-group">

          <label>
            Tipo de voluntariado
          </label>

          <select
            v-model="volunteerForm.volunteerType"
            :disabled="!isLoggedIn"
            required
          >

            <option value="" disabled>
              Seleccione una opcion
            </option>

            <option>
              Casa cuna
            </option>

            <option>
              Eventos de adopcion
            </option>

            <option>
              Transporte
            </option>

            <option>
              Veterinaria
            </option>

            <option>
              Redes sociales
            </option>

          </select>

        </div>

        <div class="form-group">

          <label>
            Motivacion
          </label>

          <textarea
            v-model="volunteerForm.motivation"
            :disabled="!isLoggedIn"
            placeholder="Cuentanos por que deseas ayudar..."
            required
          ></textarea>

        </div>

        <p v-if="errorMessage" class="form-error">{{ errorMessage }}</p>
        <p v-if="successMessage" class="form-success">{{ successMessage }}</p>

        <button class="submit-btn" :disabled="loading || !isLoggedIn">
          {{ loading ? 'Enviando...' : 'Registrarme como voluntario' }}
        </button>

      </form>

    </div>

  </section>

  <FooterBar />

</template>

<style scoped>

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

  filter:
    brightness(0.88)
    contrast(1.02);

  transform: scale(1.03);
}

.hero-overlay {

  position: absolute;

  inset: 0;

  background:
    linear-gradient(
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

  justify-content: flex-start;

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

  color:
    rgba(255,255,255,0.90);

  max-width: 470px;
}

/* SECTION */

.volunteer-section {

  background: #FAFAFA;

  padding:
    90px 24px
    120px;
}

.container {

  max-width: 1240px;

  margin: auto;
}

.volunteer-grid {

  display: grid;

  grid-template-columns:
    0.95fr 1.05fr;

  gap: 34px;

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

  border:
    1px solid rgba(146,168,148,0.10);

  box-shadow:
    0 14px 40px rgba(0,0,0,0.04);
}

.intro-image-wrap {

  height: 250px;

  overflow: hidden;
}

.intro-image {

  width: 100%;

  height: 100%;

  object-fit: cover;

  object-position: center 62%;

  transition: 0.6s ease;
}

.intro-card:hover .intro-image {

  transform: scale(1.04);
}

.intro-content {

  padding: 34px;
}

.intro-content span {

  color: #92A894;

  font-size: 12px;

  font-weight: 700;

  letter-spacing: 1px;
}

.intro-content h2 {

  font-size: 46px;

  line-height: 0.95;

  font-weight: 800;

  letter-spacing: -2px;

  color: #3A473C;

  margin:
    18px 0;
}

.intro-content p {

  font-size: 15px;

  line-height: 1.9;

  color: #667085;
}

/* BENEFITS */

.benefits-grid {

  display: grid;

  grid-template-columns:
    1fr 1fr;

  gap: 18px;
}

.benefit-card {

  background: white;

  border-radius: 26px;

  padding: 24px;

  display: flex;

  gap: 16px;

  align-items: flex-start;

  border:
    1px solid rgba(146,168,148,0.10);

  box-shadow:
    0 10px 28px rgba(0,0,0,0.03);

  transition: 0.3s ease;
}

.benefit-card:hover {

  transform: translateY(-5px);

  box-shadow:
    0 18px 36px rgba(0,0,0,0.06);
}

.benefit-icon {

  width: 52px;

  height: 52px;

  border-radius: 18px;

  background:
    rgba(146,168,148,0.14);

  display: flex;

  align-items: center;

  justify-content: center;

  flex-shrink: 0;
}

.benefit-icon i {

  font-size: 25px;

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

  border-radius: 34px;

  padding: 40px;

  border:
    1px solid rgba(146,168,148,0.10);

  box-shadow:
    0 14px 40px rgba(0,0,0,0.04);
}

.form-top {

  margin-bottom: 30px;
}

.form-top span {

  color: #92A894;

  font-size: 12px;

  font-weight: 700;

  letter-spacing: 1px;
}

.form-top h2 {

  font-size: 38px;

  line-height: 1;

  font-weight: 800;

  letter-spacing: -2px;

  color: #3A473C;

  margin:
    14px 0 10px;
}

.form-top p {

  font-size: 15px;

  line-height: 1.8;

  color: #667085;
}

.form-row {

  display: grid;

  grid-template-columns:
    1fr 1fr;

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

.form-group input,
.form-group select,
.form-group textarea {

  border:
    1px solid #E5ECE6;

  border-radius: 16px;

  padding: 15px 16px;

  font-size: 14px;

  background: #FCFCFC;

  outline: none;

  transition: 0.25s ease;

  font-family: inherit;

  color: #3A473C;
}

.form-group textarea {

  min-height: 130px;

  resize: vertical;
}

.form-group input:focus,
.form-group select:focus,
.form-group textarea:focus {

  border-color: #92A894;

  background: white;

  box-shadow:
    0 0 0 4px rgba(146,168,148,0.10);
}

/* BUTTON */

.submit-btn {

  width: 100%;

  height: 56px;

  border: none;

  border-radius: 18px;

  background:
    linear-gradient(
      135deg,
      #92A894 0%,
      #7E947F 100%
    );

  color: white;

  font-size: 15px;

  font-weight: 700;

  cursor: pointer;

  transition: 0.3s ease;
}

.submit-btn:disabled {
  cursor: not-allowed;
  opacity: 0.7;
}

.login-notice,
.form-error,
.form-success {
  padding: 12px 14px;
  border-radius: 12px;
  font-size: 14px;
  font-weight: 700;
  line-height: 1.4;
}

.login-notice {
  background: #FFF1DD;
  color: #9A5E11;
}

.login-notice a {
  color: #7C4A0B;
  font-weight: 800;
}

.form-error {
  background: #FEE4E2;
  color: #B42318;
}

.form-success {
  background: #E7F1E8;
  color: #4F6F55;
}

.submit-btn:hover {

  transform: translateY(-2px);

  box-shadow:
    0 14px 28px rgba(146,168,148,0.18);
}

/* RESPONSIVE */

@media (max-width: 980px) {

  .volunteer-grid {

    grid-template-columns: 1fr;
  }

  .hero-content {

    justify-content: center;

    text-align: center;

    padding: 0 24px;
  }

  .hero-text {

    max-width: 100%;
  }

  .hero-text h1 {

    font-size: 50px;
  }

}

@media (max-width: 700px) {

  .hero {

    height: 440px;
  }

  .hero-text h1 {

    font-size: 40px;
  }

  .hero-text p {

    font-size: 15px;
  }

  .benefits-grid {

    grid-template-columns: 1fr;
  }

  .form-row {

    grid-template-columns: 1fr;
  }

  .intro-content,
  .vol-form {

    padding: 28px;
  }

  .intro-content h2 {

    font-size: 36px;
  }

}

</style>
