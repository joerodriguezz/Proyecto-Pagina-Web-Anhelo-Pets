<script setup>
import { ref } from 'vue'
import NavBar from '../components/NavBar.vue'
import FooterBar from '../components/FooterBar.vue'
import {
  Search,
  Plus,
  Envelope,
  UserCheck,
  HomeHeart,
  Heart,
  Cat,
  Home,
  Building,
  Calendar
} from '@boxicons/vue';

const hoveredStep = ref(null)

const iconComponents = {
  search: Search,
  plus: Plus,
  envelope: Envelope,
  userCheck: UserCheck,
  homeHeart: HomeHeart
}

const steps = [
  {
    number: '01',
    icon: 'search',
    color: '#92A894',
    badge: 'Público · Sin cuenta requerida',
    badgeColor: '#F4F6F4',
    badgeText: '#92A894',
    title: 'Explora el catálogo',
    description:
      'Navega las mascotas disponibles filtrando por tipo, raza, edad o sexo. Cada perfil incluye fotos, historial de salud y personalidad del animal.',
  },
  {
    number: '02',
    icon: 'plus',
    color: '#F9C17A',
    badge: 'Registro gratuito',
    badgeColor: '#FFF1DD',
    badgeText: '#F9C17A',
    title: 'Crea tu cuenta',
    description:
      'Regístrate con tu nombre, cédula, correo y teléfono. Solo toma un minuto y es necesario para enviar tu solicitud de adopción.',
  },
  {
    number: '03',
    icon: 'envelope',
    color: '#92A894',
    badge: 'Respuesta en 48 horas',
    badgeColor: '#F4F6F4',
    badgeText: '#92A894',
    title: 'Envía tu solicitud',
    description:
      'Completa el formulario de adopción para la mascota que elegiste. El equipo de Anhelo Pets revisará tu solicitud y te contactará en menos de 48 horas.',
  },
  {
    number: '04',
    icon: 'userCheck',
    color: '#92A894',
    badge: 'Seguimiento personalizado',
    badgeColor: '#F4F6F4',
    badgeText: '#92A894',
    title: 'Aprobación y coordinación',
    description:
      'Una vez aprobada tu solicitud, coordinamos la entrega de la mascota. Si fue rechazada, te explicamos el motivo y puedes volver a intentarlo.',
  },
  {
    number: '05',
    icon: 'homeHeart',
    color: '#92A894',
    badge: 'Vacunada · Desparasitada · Lista',
    badgeColor: '#F4F6F4',
    badgeText: '#92A894',
    title: '¡Bienvenido a casa!',
    description:
      'Tu nueva mascota llega a su hogar definitivo. Nos aseguramos de que venga vacunada, desparasitada y con su historial médico completo.',
  },
]

const stats = [
  {
    value: '+125',
    label: 'Animales rescatados',
    icon: 'heart'
  },
  {
    value: '26',
    label: 'Mascotas en adopción',
    icon: 'cat'
  },
  {
    value: '+115',
    label: 'Hogares encontrados',
    icon: 'home'
  },
  {
    value: '5',
    label: 'Casas cuna',
    icon: 'building'
  },
  {
    value: '3 años',
    label: 'Trabajando para ellos',
    icon: 'calendar'
  },
]
</script>

<template>
  <NavBar />

  <section class="hero">

    <div class="hero-inner">
      <div class="hero-image-wrap">
        <div class="hero-img-placeholder">
          <img class="img-hero" src="/img-perros/Hola.PNG" alt="">
        </div>
      </div>

      <div class="hero-content">
        <h1>Cada animal merece<br>un hogar con <span class="highlight">amor.</span></h1>
        <p>Rescatamos, cuidamos y encontramos familias responsables para perros y gatos en necesidad. Únete a nuestra misión.</p>
        <div class="hero-actions">
          <RouterLink to="/mascotas" class="btn-primary">Ver mascotas</RouterLink>
        </div>
      </div>
    </div>
  </section>

  <section class="stats-bar">

  <div class="stats-inner container">

    <div
      v-for="stat in stats"
      :key="stat.label"
      class="stat-item"
    >

      <div class="stat-icon-wrap">

  <Heart
    v-if="stat.icon === 'heart'"
    class="stat-icon"
  />

  <Cat
    v-if="stat.icon === 'cat'"
    class="stat-icon"
  />

  <Home
    v-if="stat.icon === 'home'"
    class="stat-icon"
  />

  <Building
    v-if="stat.icon === 'building'"
    class="stat-icon building-fix"
  />

  <Calendar
    v-if="stat.icon === 'calendar'"
    class="stat-icon"
  />

</div>

      <span class="stat-value">
        {{ stat.value }}
      </span>

      <span class="stat-label">
        {{ stat.label }}
      </span>

    </div>

  </div>

</section>

  <section class="how-section">
    <h2 class="section-title">¿Cómo adoptar?</h2>
    <p class="section-subtitle">5 pasos simples para encontrar a tu compañero ideal</p>

    <div class="steps-modern">

  <div
    v-for="(step, idx) in steps"
    :key="idx"
    class="step-modern"
  >

    <div class="step-number">
      {{ step.number }}
    </div>

    <div class="step-circle">

      <component
        v-if="iconComponents[step.icon]"
        :is="iconComponents[step.icon]"
        class="modern-icon"
      />

    </div>

    <div
      v-if="idx !== steps.length - 1"
      class="step-line"
    ></div>

    <h3 class="modern-title">
      {{ step.title }}
    </h3>

    <p class="modern-description">
      {{ step.description }}
    </p>

  </div>

</div>

    <div class="cta-banner">
      <div class="cta-text">
        <strong>¿Lista para comenzar?</strong>
        <span>Hay 26 mascotas esperando un hogar ahora mismo.</span>
      </div>
      <RouterLink to="/mascotas" class="btn-primary">Ver mascotas disponibles →</RouterLink>
    </div>
  </section>

  <section class="featured-section">
    <div class="container">
      <h2 class="section-title">Mascotas destacadas</h2>
      <p class="section-subtitle">Conoce a algunos de nuestros amigos que esperan hogar</p>

      <div class="pets-grid">

  <div
    v-for="pet in featuredPets"
    :key="pet.name"
    class="pet-card"
  >

    <div class="pet-photo">
      <img
        :src="pet.image"
        :alt="pet.name"
        class="pet-image"
      >
    </div>

    <div class="pet-info">

      <div class="pet-header">

        <span class="pet-name">
          {{ pet.name }}
        </span>

        <span class="badge badge-green">
          {{ pet.status }}
        </span>

      </div>

      <p class="pet-meta">
        {{ pet.type }} · {{ pet.age }} · {{ pet.sex }}
      </p>


<p class="pet-description">
  {{ pet.personality }}
</p>

      <RouterLink
        to="/mascotas"
        class="btn-outline-green pet-btn"
      >
        Ver perfil
      </RouterLink>

    </div>

  </div>

</div>

      <div class="see-all-wrap">

  <RouterLink
    to="/mascotas"
    class="btn-all-pets"
  >
    Ver todas las mascotas →
  </RouterLink>

</div>
    </div>
  </section>

  <section class="help-strip">

  <div class="container">

    <h2 class="section-title">
      ¿Cómo puedes ayudar?
    </h2>

    <p class="section-subtitle">
      Cada pequeña acción puede cambiar la vida de un animal rescatado.
    </p>

    <div class="help-grid">

      <div class="help-card">

        <div class="help-image-wrap">
          <img src="/img-ayuda/donar.png" class="help-image">
        </div>

        <h3>Dona</h3>

        <p>
          Tu contribución ayuda con alimento, rescates,
          medicamentos y tratamientos veterinarios.
        </p>

        <RouterLink
          to="/nosotros#donacion"
          class="btn-primary"
        >
          Hacer una donación
        </RouterLink>

      </div>

      <div class="help-card">

        <div class="help-image-wrap">
          <img src="/img-ayuda/casa.png" class="help-image">
        </div>

        <h3>Sé casa cuna</h3>

        <p>
          Brinda un hogar temporal mientras encuentran
          una familia definitiva y segura.
        </p>

        <RouterLink
          to="/voluntarios"
          class="btn-primary"
        >
          Quiero ser casa cuna
        </RouterLink>

      </div>

      <div class="help-card">

        <div class="help-image-wrap">
          <img src="/img-ayuda/voluntario.png" class="help-image">
        </div>

        <h3>Voluntariado</h3>

        <p>
          Ayuda en eventos, rescates, transporte,
          limpieza y cuidado de animales.
        </p>

        <RouterLink
          to="/voluntarios"
          class="btn-primary"
        >
          Ser voluntario
        </RouterLink>

      </div>

    </div>

  </div>

</section>
 

  <FooterBar />
</template>

<script>
export default {
  data() {
    return {
      featuredPets: [

        {
          name: 'Bartolo',
          emoji: '🐶',
          bg: '#F4F6F4',
          image: '/img-perros/Bartolo.PNG',
          type: 'Perro',
          breed: 'Criollo',
          age: '5 meses',
          sex: 'Macho',
          status: 'Disponible',
          personality: 'Muy juguetón y sociable. Le encanta correr y jugar en el pasto.'
        },

        {
          name: 'Mojito',
          emoji: '🐶',
          bg: '#FFF1DD',
          image: '/img-perros/Mojito.jpg',
          type: 'Perro',
          breed: 'Criollo',
          age: '13 años',
          sex: 'Macho',
          status: 'Disponible',
          personality: 'Tranquilo y cariñoso. Disfruta dormir en lugares cómodos y recibir mimos.'
        },

        {
          name: 'Lola',
          emoji: '🐱',
          bg: '#F4F6F4',
          image: '/img-perros/Lola.PNG',
          type: 'Gata',
          breed: 'Doméstica',
          age: '1 año y 5 meses',
          sex: 'Hembra',
          status: 'Disponible',
          personality: 'Curiosa y tranquila. Le gusta descansar durante el día y explorar rincones.'
        },

        {
          name: 'Bala',
          emoji: '🐶',
          bg: '#FFF1DD',
          image: '/img-perros/Bala.PNG',
          type: 'Perro',
          breed: 'Criollo',
          age: '7 años',
          sex: 'Macho',
          status: 'Disponible',
          personality: 'Reservado pero muy noble. Prefiere lugares tranquilos y compañía calmada.'
        },

      ],
    }
  },
}
</script>

<style scoped>

:root {
  --primary: #92A894;
  --accent: #F9C17A;

  --background: #FAFAFA;
  --background-soft: #F4F6F4;

  --text-dark: #3A473C;
  --text-light: #6C756D;

  --white: #FFFFFF;
  --border: rgba(146,168,148,0.10);
}


/* ─── HERO ─── */
.hero {
  background: #FAFAFA;
  min-height: 100vh;
  display: flex;
  align-items: center;
  overflow: visible;
  padding: 0;
}

.hero-inner {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
  max-width: 100%;
  gap: 0;
}

.hero-image-wrap {
  width: 60%;
  margin-left: 0;
  display: flex;
  align-items: center;
  justify-content: flex-start;
}

.hero-img-placeholder {
  width: 100%;
  display: flex;
  align-items: center;
  justify-content: flex-start;
}

.img-hero {
  width: 1050px;
  max-width: none;
  height: auto;
  display: block;
}

.hero-content {
  width: 45%;
  padding-right: 80px;
}

.hero-content h1 {
  font-size: 92px;
  font-weight: 800;
  line-height: 0.95;
  color: #3A473C;
  margin-bottom: 30px;
  letter-spacing: -4px;
}

.hero-content p {
  font-size: 22px;
  color: #6C756D;
  line-height: 1.8;
  max-width: 560px;
  margin-bottom: 40px;
}

.dog-emoji { font-size: 120px; filter: drop-shadow(0 8px 20px rgba(0,0,0,0.2)); }

.hero-tag {
  display: inline-block;
  background: #F4F6F4;
  color: #92A894;
  border-radius: 999px;
  padding: 6px 16px;
  font-size: 13px;
  font-weight: 600;
  margin-bottom: 16px;
}
.hero-content h1 {
  font-size: 72px;
  font-weight: 800;
  color: #3A473C;
  line-height: 1.05;
  margin-bottom: 24px;
  letter-spacing: -2px;
}

.highlight {
  color: #F9C17A;
}

.hero-content p {
  font-size: 19px;
  color: #6C756D;
  line-height: 1.9;
  max-width: 520px;
  margin-bottom: 36px;
}
.hero-actions {
  display: flex;
  gap: 14px;
  flex-wrap: wrap;
}

.hero-content {
  width: 42%;
  padding-right: 60px;
}

/* ─── STATS BAR ─── */
.stats-bar {
  background: linear-gradient(
    to bottom,
    #F4F6F4 0%,
    #FAFAFA 100%
  );

  padding: 20px 0 90px;

  margin-top: -60px;

  position: relative;

  z-index: 3;

  border-top-left-radius: 40px;
  border-top-right-radius: 40px;
}
.stats-inner {
  display: flex;
  justify-content: center;
  gap: 26px;
  flex-wrap: wrap;
}

.stat-item {
  width: 170px;
  height: 230px;
  background: white;
  border-radius: 18px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 14px;
  box-shadow: 0 4px 18px rgba(58,71,60,0.04);
  border: 1px solid rgba(146,168,148,0.10);
  transition: 0.25s ease;
}

.stat-icon-wrap {
  width: 72px;
  height: 72px;
  border-radius: 50%;
  background: rgba(146,168,148,0.12);
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
}

.stat-icon {
  width: 36px;
  height: 36px;
  color: #92A894;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  top: -1px;
}

.building-fix {
  transform: translateY(-4px);
}

.stat-item:hover {
  transform: translateY(-5px);
}
.stat-value {
  font-size: 42px;
  font-weight: 800;
  color: #3A473C;
  line-height: 1;
}
.stat-label {
  font-size: 14px;
  font-weight: 500;
  color: #3A473C;
  text-align: center;
  max-width: 120px;
  line-height: 1.5;
}

/* ─── HOW SECTION ─── */
.how-section {
  background: #F4F6F4;
  padding: 70px 24px 0;
  height: 77vh;
}

.steps-modern {
  max-width: 1180px;
  margin: 40px auto 0;
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 24px;
  position: relative;
}

.step-modern {
  flex: 1;
  position: relative;
  text-align: center;
}

.step-number {
  width: 38px;
  height: 38px;
  margin: 0 auto 18px;
  border-radius: 50%;
  background: #92A894;
  color: white;
  font-size: 14px;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
}

.step-circle {
  width: 86px;
  height: 86px;
  margin: 0 auto;
  border-radius: 50%;
  background: white;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  z-index: 2;
  transition: 0.25s ease;
  border: 1px solid rgba(146,168,148,0.10);
}

.step-modern:hover .step-circle {
  transform: translateY(-4px);
  box-shadow: 0 12px 30px rgba(58,71,60,0.08);
}

.modern-icon {
  width: 36px;
  height: 36px;
  color: #92A894;
}
.step-line {
  position: absolute;
  top: 57px;
  left: 58%;
  width: 100%;
  height: 2px;
  background: rgba(146,168,148,0.20);
  z-index: 1;
}

.modern-title {
  font-size: 15px;
  font-weight: 700;
  color: #3A473C;
  margin-top: 28px;
  margin-bottom: 14px;
  line-height: 1.3;
}

.modern-description {
  font-size: 13px;
  line-height: 1.7;
  color: #6C756D;
  max-width: 190px;
  margin: 0 auto;
}

/* Responsive */
@media (max-width: 1100px) {

  .steps-modern {
    flex-direction: column;
    gap: 50px;
    align-items: center;
  }

  .step-modern {
    max-width: 400px;
  }

  .step-line {
    display: none;
  }

  .how-section {
    height: auto;
    padding-bottom: 70px;
  }
}

/* CTA Banner */
.cta-banner {
  max-width: 900px;
  margin: 0 auto;
  margin-top: 80px;
  background: #92A894;
  border-radius: 30px;
  padding: 26px 36px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 20px;
  flex-wrap: wrap;
  margin-bottom: -20px;
  position: relative;
  z-index: 1;
  box-shadow: 0 10px 30px rgba(58,71,60,0.08);
}

.cta-text {
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.cta-text strong { font-size: 20px; color: white; }
.cta-text span   { font-size: 14px; color: rgba(255,255,255,0.75); }

/* ─── FEATURES PETS ─── */
.featured-section {
  background: #FAFAFA;

  padding: 80px 24px 60px;

  border-top: 1px solid rgba(146,168,148,0.10);
}
.pets-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 20px;
  margin-bottom: 36px;
}
.pet-card {
  background: white;
  border: 1px solid rgba(146,168,148,0.10);
  border-radius: 30px;
  overflow: hidden;
  box-shadow: 0 10px 35px rgba(58,71,60,0.05);
  transition: box-shadow 0.2s, transform 0.2s;
}
.pet-card:hover {
  box-shadow: 0 16px 42px rgba(58,71,60,0.10);
  transform: translateY(-3px);
}

.pet-photo {
  height: 240px;
  overflow: hidden;
}

.pet-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.pet-info  { padding: 16px; }
.pet-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 6px;
}

.pet-name  {
  font-size: 18px;
  font-weight: 700;
  color: #3A473C;
}

.badge-green {
  background-color: #F4F6F4;
  color: #92A894;
}

.pet-meta  {
  font-size: 13px;
  color: #6C756D;
  margin-bottom: 14px;
}

.pet-description {
  font-size: 13px;
  color: #6C756D;
  line-height: 1.6;
  margin-bottom: 14px;
}

.btn-outline-green {
  border: 1px solid #92A894;
  color: #92A894;
  background: transparent;
  border-radius: 999px;
  text-decoration: none;
  display: inline-flex;
}

.btn-outline-green:hover {
  background: #92A894;
  color: white;
}

.pet-btn   {
  font-size: 13px;
  padding: 7px 16px;
  width: 100%;
  justify-content: center;
}
.see-all-wrap { text-align: center; }

.btn-all-pets {
  color: #92A894;
  font-weight: 600;
  text-decoration: none;
}

/* ─── HELP STRIP ─── */

.help-strip {
  background: #F4F6F4;
  padding: 90px 24px;
}

.help-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 28px;
  margin-top: 50px;
}

.help-card {
  background: rgba(255,255,255,0.75);

  backdrop-filter: blur(14px);

  border-radius: 28px;

  padding: 34px 28px;

  border: 1px solid rgba(255,255,255,0.6);

  text-align: center;

  box-shadow:
    0 8px 24px rgba(58,71,60,0.04),
    inset 0 1px 0 rgba(255,255,255,0.5);

  transition: all 0.35s ease;

  position: relative;

  overflow: hidden;
}

.help-card::before {
  content: '';

  position: absolute;

  top: -120%;

  left: -40%;

  width: 180%;

  height: 180%;

  background: linear-gradient(
    120deg,
    transparent,
    rgba(255,255,255,0.35),
    transparent
  );

  transform: rotate(25deg);

  transition: 0.8s ease;
}

.help-card:hover::before {
  top: 120%;
}

.help-card:hover {
  transform: translateY(-12px) scale(1.02);

  box-shadow:
    0 22px 50px rgba(58,71,60,0.10),
    inset 0 1px 0 rgba(255,255,255,0.7);
}

.help-image-wrap {
  width: 100px;
  height: 100px;

  margin: 0 auto 24px;

  border-radius: 50%;

  background: rgba(146,168,148,0.12);

  display: flex;
  align-items: center;
  justify-content: center;

  transition: 0.35s ease;
}

.help-card:hover .help-image-wrap {
  transform: scale(1.08);
}

.help-image {
  width: 58px;
  height: 58px;
  object-fit: contain;
}

.help-card h3 {
  font-size: 24px;
  font-weight: 700;
  margin-bottom: 14px;
  color: #3A473C;
}

.help-card p {
  font-size: 15px;
  line-height: 1.7;
  color: #6C756D;
  margin-bottom: 24px;
}

.btn-primary {
  background: #92A894;
  color: white;
  border-radius: 999px;
  padding: 12px 28px;
  text-decoration: none;
  display: inline-block;
}

.btn-primary:hover {
  background: #7C927E;
}

/* ─── Responsive ─── */
@media (max-width: 900px) {
  .hero-inner { flex-direction: column; text-align: center; gap: 30px; }
  .hero-content p { margin: 0 auto 28px; }
  .hero-actions { justify-content: center; }
  .pets-grid { grid-template-columns: 1fr 1fr; }
  .help-grid { grid-template-columns: 1fr 1fr; }
  .steps-track { flex-wrap: wrap; justify-content: center; gap: 24px; min-height: auto; }
  .track-line  { display: none; }
  .step-card   { position: static; transform: none; width: 100%; margin-top: 14px; }
  .step-pop-enter-from { transform: translateY(6px); }
  .step-pop-enter-to   { transform: translateY(0); }
}
@media (max-width: 560px) {
  .pets-grid { grid-template-columns: 1fr; }
  .help-grid { grid-template-columns: 1fr; }
  .cta-banner { flex-direction: column; text-align: center; }
}
</style>