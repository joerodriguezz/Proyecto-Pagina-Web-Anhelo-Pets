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
  Calendar,
  ChevronDown
} from '@boxicons/vue';

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
    color: '#2D7A58',
    badge: 'Público · Sin cuenta requerida',
    badgeColor: '#EEF3F0',
    badgeText: '#5F8F7B',
    title: 'Explora el catálogo',
    description:
      'Navega las mascotas disponibles filtrando por tipo, raza, edad o sexo. Cada perfil incluye fotos, historial de salud y personalidad del animal.',
  },
  {
    number: '02',
    icon: 'plus',
    color: '#5F8F7B',
    badge: 'Registro gratuito',
    badgeColor: '#EEF3F0',
    badgeText: '#5F8F7B',
    title: 'Crea tu cuenta',
    description:
      'Regístrate con tu nombre, cédula, correo y teléfono. Solo toma un minuto y es necesario para enviar tu solicitud de adopción.',
  },
  {
    number: '03',
    icon: 'envelope',
    color: '#C59B6D',
    badge: 'Respuesta en 48 horas',
    badgeColor: '#F6EFE7',
    badgeText: '#A07A52',
    title: 'Envía tu solicitud',
    description:
      'Completa el formulario de adopción para la mascota que elegiste. El equipo de Anhelo Pets revisará tu solicitud y te contactará en menos de 48 horas.',
  },
  {
    number: '04',
    icon: 'userCheck',
    color: '#6F9D89',
    badge: 'Seguimiento personalizado',
    badgeColor: '#EEF3F0',
    badgeText: '#5F8F7B',
    title: 'Aprobación y coordinación',
    description:
      'Una vez aprobada tu solicitud, coordinamos la entrega de la mascota. Si fue rechazada, te explicamos el motivo y puedes volver a intentarlo.',
  },
  {
    number: '05',
    icon: 'homeHeart',
    color: '#7D8F87',
    badge: 'Vacunada · Desparasitada · Lista',
    badgeColor: '#EEF1EF',
    badgeText: '#6F7F78',
    title: '¡Bienvenido a casa!',
    description:
      'Tu nueva mascota llega a su hogar definitivo. Nos aseguramos de que venga vacunada, desparasitada y con su historial médico completo.',
  },
]

const stats = [
  { value: '+125', label: 'Animales rescatados', icon: 'heart' },
  { value: '26',   label: 'Mascotas en adopción', icon: 'cat' },
  { value: '+115', label: 'Hogares encontrados', icon: 'home' },
  { value: '5',    label: 'Casas cuna', icon: 'building' },
  { value: '3 años', label: 'Trabajando para ellos', icon: 'calendar' },
]

// ── FAQ ──
const faqs = [
  {
    question: '¿Quiénes pueden adoptar una mascota?',
    answer: 'Cualquier persona mayor de edad que cumpla con los requisitos y pase el proceso de evaluación de adopción.',
  },
  {
    question: '¿Las mascotas se entregan vacunadas?',
    answer: 'Sí. Todas las mascotas disponibles para adopción se entregan con su control veterinario al día según su edad y condición.',
  },
  {
    question: '¿Cuánto tarda el proceso de adopción?',
    answer: 'Generalmente entre 24 y 72 horas después de enviar la solicitud, dependiendo de la revisión y disponibilidad del equipo.',
  },
  {
    question: '¿Puedo adoptar si vivo en apartamento?',
    answer: 'Sí, siempre que el espacio sea adecuado para la mascota y se garantice su bienestar.',
  },
  {
    question: '¿Cómo puedo ayudar si no puedo adoptar?',
    answer: 'Puedes realizar donaciones, convertirte en voluntario o apoyar compartiendo nuestras publicaciones.',
  },
]

const openFaq = ref(null)

function toggleFaq(index) {
  openFaq.value = openFaq.value === index ? null : index
}
</script>

<template>
  <NavBar />

  <!-- ══════════════════════════════════
       HERO
  ══════════════════════════════════ -->
<section class="hero">
  <div class="hero-inner">

    <div class="hero-content">

      <h1>
        Cada animal merece<br>
        un hogar con <span class="highlight">AMOR</span>
      </h1>

      <p>
        Rescatamos, cuidamos y encontramos familias responsables
        para perros y gatos en necesidad.
      </p>

      <div class="hero-actions">
        <RouterLink to="/donar" class="btn-primary">Hacer una donación</RouterLink>
        <RouterLink to="/voluntarios" class="btn-secondary">Ser voluntario</RouterLink>
      </div>

    </div>

  </div>
</section>


  <!-- ══════════════════════════════════
       CÓMO ADOPTAR
  ══════════════════════════════════ -->
  <section class="how-section">
    <h2 class="section-title">¿Cómo adoptar?</h2>
    <p class="section-subtitle">5 pasos simples para encontrar a tu compañero ideal</p>

    <div class="steps-modern">
      <div
        v-for="(step, idx) in steps"
        :key="idx"
        class="step-modern"
      >
        <div class="step-number">{{ step.number }}</div>
        <div class="step-circle">
          <component
            v-if="iconComponents[step.icon]"
            :is="iconComponents[step.icon]"
            class="modern-icon"
          />
        </div>
        <div v-if="idx !== steps.length - 1" class="step-line"></div>
        <h3 class="modern-title">{{ step.title }}</h3>
        <p class="modern-description">{{ step.description }}</p>
      </div>
    </div>

    <!-- CTA Banner -->
    <div class="cta-banner">
      <div class="cta-text">
        <strong>¿Lista para comenzar?</strong>
        <span>Hay mascotas esperando un hogar ahora mismo.</span>
      </div>
      <RouterLink to="/mascotas" class="cta-button">
  Ver mascotas disponibles →
</RouterLink>
    </div>
  </section>

  <!-- ══════════════════════════════════
       PREGUNTAS FRECUENTES (FAQ)
  ══════════════════════════════════ -->
  <section class="faq-section">
    <div class="container">
      <h2 class="section-title">Preguntas frecuentes</h2>
      <p class="section-subtitle">Resolvemos las dudas más comunes sobre el proceso de adopción</p>

      <div class="faq-list">
        <div
          v-for="(faq, idx) in faqs"
          :key="idx"
          class="faq-item"
          :class="{ open: openFaq === idx }"
        >
          <button class="faq-question" @click="toggleFaq(idx)">
            <span>{{ faq.question }}</span>
            <ChevronDown class="faq-icon" />
          </button>
          <div class="faq-answer-wrap">
            <p class="faq-answer">{{ faq.answer }}</p>
          </div>
        </div>
      </div>
    </div>
  </section>

  <FooterBar />
</template>

<style scoped>

/* ─── HERO ─── */

.hero {
  height: 430px;

  background-image: url('/img-home/herohome.jpg');
  background-size: cover;
  background-position: right 20%;
  background-repeat: no-repeat;

  display: flex;
  align-items: center;

  position: relative;
  overflow: hidden;
}

.hero::before {
  content: '';

  position: absolute;
  inset: 0;

  background:
    linear-gradient(
      90deg,
      rgba(0,0,0,0.72) 0%,
      rgba(0,0,0,0.45) 35%,
      rgba(0,0,0,0.12) 70%,
      rgba(0,0,0,0) 100%
    );
}

.hero-inner {
  width: 100%;
  max-width: 1400px;
  margin: 0 auto;
  padding: 0 80px;
}

.hero-content {
  position: relative;
  z-index: 2;

  max-width: 560px;

  margin-left: 0;
  margin-top: 0;
}

.hero-content h1 {
  font-size: 62px;
  font-weight: 800;

  color: white;

  line-height: 0.95;

  margin-bottom: 24px;

  letter-spacing: -3px;
}

.highlight {
  color: #E4C28A;
}

.hero-content p {
  font-size: 16px;

  color: rgba(255,255,255,0.92);

  line-height: 1.7;

  max-width: 420px;
}

/* ─── HERO ACTIONS ─── */
.hero-actions {
  display: flex;
  gap: 14px;
  flex-wrap: wrap;
  margin-top: 36px;
  position: relative;
  z-index: 2;
}

.hero-actions .btn-primary {
  background: #3A473C;
  color: #FFFFFF;
  border: 2px solid #3A473C;
  padding: 10px 20px;
  border-radius: 10px;
  font-size: 13px;
  font-weight: 700;
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  transition: 0.25s ease;
}

.hero-actions .btn-primary:hover {
  background: #2D372F;
  border-color: #2D372F;
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(58, 71, 60, 0.15);
}

.hero-actions .btn-secondary {
  background: transparent;
  color: #FFFFFF;
  border: 2px solid rgba(255, 255, 255, 0.6);
  padding: 10px 20px;
  border-radius: 10px;
  font-size: 13px;
  font-weight: 700;
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  transition: 0.25s ease;
}

.hero-actions .btn-secondary:hover {
  background: rgba(255, 255, 255, 0.12);
  border-color: #FFFFFF;
  transform: translateY(-2px);
}

/* ─── STATS BAR ─── */
.stats-bar {
  background: linear-gradient(to bottom, #F4F6F4 0%, #FFFFFF 100%);
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
  width: 160px;
  height: 210px;
  background: #FFFFFF;
  border-radius: 18px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  box-shadow: 0 4px 18px rgba(58, 71, 60, 0.02);
  border: 1px solid #F4F6F4;
  transition: 0.25s ease;
}

.stat-icon-wrap {
  width: 66px;
  height: 66px;
  border-radius: 50%;
  background: rgba(146, 168, 148, 0.15);
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
}

.stat-icon {
  width: 32px;
  height: 32px;
  color: #5A6E5C;
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
  border-color: #3A473C;
}

.stat-value {
  font-size: 38px;
  font-weight: 800;
  color: #3A473C;
  line-height: 1;
}

.stat-label {
  font-size: 13px;
  font-weight: 700;
  color: #6C756D;
  text-align: center;
  max-width: 110px;
  line-height: 1.4;
}
/* ─── HOW SECTION ─── */
.how-section {
  background: #FAF8F5;
  padding: 40px 24px 20px;
}
.steps-modern {
  max-width: 1000px;
  margin: 25px auto 0;
  display: flex;
  justify-content: center;
  align-items: flex-start;
  gap: 40px;
  position: relative;
}

.step-modern {
  width: 150px;
  flex: none;
  position: relative;
  text-align: center;
}

.step-number {
  width: 38px;
  height: 38px;
  margin: 0 auto 18px;
  border-radius: 50%;
  background: #3A473C;
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
  background: #FFFFFF;
  border: 2px solid #EBEFEA;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  z-index: 2;
  transition: 0.25s ease;
}

.step-modern:hover .step-circle {
  transform: translateY(-4px);
  border-color: #3A473C;
  box-shadow: 0 12px 30px rgba(58, 71, 60, 0.05);
}

.modern-icon {
  width: 36px;
  height: 36px;
  color: #5A6E5C;
}

.step-line {
  position: absolute;
  top: 57px;
  left: 58%;
  width: 100%;
  height: 2px;
  background: #EBEFEA;
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

@media (max-width: 1100px) {
  .steps-modern { flex-direction: column; gap: 50px; align-items: center; }
  .step-modern  { max-width: 400px; }
  .step-line    { display: none; }
  .how-section  { height: auto; padding-bottom: 70px; }
}

/* CTA Banner */
.cta-banner {
  max-width: 900px;
  margin: 80px auto 0;
  background: #3A473C;
  border-radius: 24px;
  padding: 26px 36px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 20px;
  flex-wrap: wrap;
  margin-bottom: 40px;
  position: relative;
  z-index: 1;
  box-shadow: 0 10px 30px rgba(58, 71, 60, 0.12);
}


.cta-text {
  display: flex;
  flex-direction: column;
  gap: 6px;
  text-align: center;
  align-items: center;
}

.cta-text strong {
  font-size: 24px;
  color: #FFFFFF;
  font-weight: 800;
}

.cta-text span {
  font-size: 15px;
  color: #E8D8C3;
  font-weight: 500;
}

.cta-button {
  background: #F5E6D3;
  color: #3A473C;
  padding: 14px 28px;
  border-radius: 14px;
  text-decoration: none;
  font-weight: 700;
  transition: 0.25s ease;
}

.cta-button:hover {
  background: #E8D8C3;
  color: #3A473C;
}

/* ─── FAQ SECTION ─── */
.faq-section {
  position: relative;

  background-image: url('/img-home/fondohome.PNG');
  background-size: cover;
  background-position: center;
  background-repeat: no-repeat;

  padding: 80px 24px 90px;

  overflow: hidden;
}

.faq-section::before {
  content: '';
  position: absolute;
  inset: 0;

  background: rgba(58, 71, 60, 0.72);
}

.faq-section .container {
  position: relative;
  z-index: 2;
}

.faq-section .section-title {
  color: #FFFFFF;
}

.faq-section .section-subtitle {
  color: rgba(255,255,255,0.85);
}

.faq-list {
  max-width: 800px;
  margin: 40px auto 0;
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.faq-item {
  background: rgba(255,255,255,0.92);
  backdrop-filter: blur(8px);

  border: 2px solid rgba(255,255,255,0.15);
  border-radius: 18px;

  overflow: hidden;
  transition: 0.25s ease;
}

.faq-item.open,
.faq-item:hover {
  border-color: #3A473C;
}

.faq-question {
  width: 100%;

  display: flex;
  align-items: center;
  justify-content: space-between;

  gap: 16px;
  padding: 20px 24px;

  background: transparent;
  border: none;

  cursor: pointer;
  text-align: left;

  font-size: 16px;
  font-weight: 700;
  color: #3A473C;

  font-family: inherit;
}

.faq-icon {
  width: 22px;
  height: 22px;

  color: #5A6E5C;

  flex-shrink: 0;
  transition: transform 0.25s ease;
}

.faq-item.open .faq-icon {
  transform: rotate(180deg);
}

.faq-answer-wrap {
  max-height: 0;
  overflow: hidden;
  transition: max-height 0.3s ease;
}

.faq-item.open .faq-answer-wrap {
  max-height: 240px;
}

.faq-answer {
  padding: 0 24px 20px;

  font-size: 14px;
  line-height: 1.8;

  color: #6C756D;

  margin: 0;
}

/* ─── Títulos de sección ─── */
.section-title {
  font-size: 36px;
  font-weight: 800;
  color: #3A473C;
  text-align: center;
  margin-bottom: 12px;
  letter-spacing: -0.8px;
}

.section-subtitle {
  font-size: 16px;
  color: #6C756D;
  text-align: center;
  font-weight: 500;
  margin-bottom: 0;
}

.btn-all-pets {
  display: inline-block;
  padding: 14px 32px;
  border-radius: 14px;
  border: 2px solid #3A473C;
  color: #3A473C;
  font-weight: 700;
  font-size: 15px;
  text-decoration: none;
  transition: 0.2s ease;
}

.btn-all-pets:hover {
  background: #3A473C;
  color: white;
}

/* ─── Responsive ─── */
@media (max-width: 900px) {
  .hero-inner      { flex-direction: column; text-align: center; gap: 30px; }
  .hero-content p  { margin: 0 auto 28px; }
  .hero-actions    { justify-content: center; }
}

@media (max-width: 560px) {
  .hero-actions    { flex-direction: column; width: 100%; }
  .hero-actions .btn-primary,
  .hero-actions .btn-secondary { width: 100%; }
}

/* ── MOBILE RESPONSIVE ── */
@media (max-width: 768px) {
  .hero {
    height: 380px;
  }

  .hero-inner {
    padding: 0 20px;
  }

  .hero-content h1 {
    font-size: 42px;
    letter-spacing: -2px;
    margin-bottom: 16px;
  }

  .hero-content p {
    font-size: 14px;
    max-width: 100%;
  }

  .hero-actions {
    margin-top: 24px;
    gap: 10px;
  }

  .how-section {
    padding: 36px 16px 40px;
  }

  .section-title {
    font-size: 28px;
    letter-spacing: -0.5px;
  }

  .section-subtitle {
    font-size: 14px;
  }

  .steps-modern {
    flex-direction: column;
    align-items: center;
    gap: 36px;
  }

  .step-modern {
    width: 100%;
    max-width: 320px;
  }

  .step-line {
    display: none;
  }

  .modern-title {
    font-size: 14px;
    margin-top: 18px;
    margin-bottom: 8px;
  }

  .modern-description {
    font-size: 13px;
    max-width: 100%;
  }

  .cta-banner {
    flex-direction: column;
    text-align: center;
    padding: 22px 20px;
    border-radius: 18px;
    gap: 14px;
    margin: 40px 0 0;
  }

  .cta-text strong {
    font-size: 20px;
  }

  .cta-text span {
    font-size: 13px;
  }

  .cta-button {
    width: 100%;
    text-align: center;
    padding: 14px 20px;
    border-radius: 12px;
  }

  .faq-section {
    padding: 56px 16px 64px;
  }

  .faq-list {
    margin-top: 28px;
    gap: 10px;
  }

  .faq-question {
    font-size: 14px;
    padding: 16px 18px;
  }

  .faq-answer {
    font-size: 13px;
    padding: 0 18px 16px;
  }
}

@media (max-width: 480px) {
  .hero {
    height: 340px;
  }

  .hero-content h1 {
    font-size: 34px;
    letter-spacing: -1.5px;
  }

  .hero-actions .btn-primary,
  .hero-actions .btn-secondary {
    width: 100%;
    justify-content: center;
  }
}
</style>