<script setup>
import { ref } from 'vue'
import NavBar from '../components/NavBar.vue'
import FooterBar from '../components/FooterBar.vue'
import { Search, Plus, Envelope, UserCheck, HomeHeart}  from '@boxicons/vue';

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
    color: '#4F9E70',
    badge: 'Público · Sin cuenta requerida',
    badgeColor: '#E8F5EE',
    badgeText: '#2A5C42',
    title: 'Explora el catálogo',
    description:
      'Navega las mascotas disponibles filtrando por tipo, raza, edad o sexo. Cada perfil incluye fotos, historial de salud y personalidad del animal.',
  },
  {
    number: '02',
    icon: 'plus',
    color: '#5B9BD5',
    badge: 'Registro gratuito',
    badgeColor: '#E8F0FF',
    badgeText: '#1A4F9F',
    title: 'Crea tu cuenta',
    description:
      'Regístrate con tu nombre, cédula, correo y teléfono. Solo toma un minuto y es necesario para enviar tu solicitud de adopción.',
  },
  {
    number: '03',
    icon: 'envelope',
    color: '#E8A838',
    badge: 'Respuesta en 48 horas',
    badgeColor: '#FFF4DC',
    badgeText: '#8A5A00',
    title: 'Envía tu solicitud',
    description:
      'Completa el formulario de adopción para la mascota que elegiste. El equipo de Anhelo Pets revisará tu solicitud y te contactará en menos de 48 horas.',
  },
  {
    number: '04',
    icon: 'userCheck',
    color: '#6DB96D',
    badge: 'Seguimiento personalizado',
    badgeColor: '#EDFBED',
    badgeText: '#276127',
    title: 'Aprobación y coordinación',
    description:
      'Una vez aprobada tu solicitud, coordinamos la entrega de la mascota. Si fue rechazada, te explicamos el motivo y puedes volver a intentarlo.',
  },
  {
    number: '05',
    icon: 'homeHeart',
    color: '#9E6BC4',
    badge: 'Vacunada · Desparasitada · Lista',
    badgeColor: '#F4EDFF',
    badgeText: '#5C2A8A',
    title: '¡Bienvenido a casa!',
    description:
      'Tu nueva mascota llega a su hogar definitivo. Nos aseguramos de que venga vacunada, desparasitada y con su historial médico completo.',
  },
]

const stats = [
  { value: '+125', label: 'Animales rescatados' },
  { value: '26',   label: 'Mascotas en adopción' },
  { value: '+115', label: 'Hogares encontrados' },
  { value: '5',    label: 'Casas cuna' },
  { value: '3 años', label: 'Trabajando para ellos' },
]
</script>

<template>
  <NavBar />

  <!-- ══════════════════════════════════
       HERO
  ══════════════════════════════════ -->
  <section class="hero">
    <div class="hero-inner container">
      <div class="hero-image-wrap">
        <div class="hero-img-placeholder">
          <img class="img-hero" src="/img-perros/perro1-sin-bg.png" alt="">
        </div>
      </div>

      <div class="hero-content">
        <span class="hero-tag">Fundación Anhelo Pets</span>
        <h1>Cada animal merece<br>un hogar con amor.</h1>
        <p>Rescatamos, cuidamos y encontramos familias responsables para perros y gatos en necesidad. Únete a nuestra misión.</p>
        <div class="hero-actions">
          <RouterLink to="/mascotas" class="btn-yellow">Ver mascotas</RouterLink>
          <RouterLink to="/voluntarios" class="btn-outline">Ser casa cuna</RouterLink>
        </div>
      </div>
    </div>
  </section>

  <!-- ══════════════════════════════════
       STATS BAR
  ══════════════════════════════════ -->
  <section class="stats-bar">
    <div class="stats-inner container">
      <div v-for="stat in stats" :key="stat.label" class="stat-item">
        <span class="stat-value">{{ stat.value }}</span>
        <span class="stat-label">{{ stat.label }}</span>
      </div>
    </div>
  </section>

  <!-- ══════════════════════════════════
       CÓMO ADOPTAR
  ══════════════════════════════════ -->
  <section class="how-section">
    <h2 class="section-title">¿Cómo adoptar?</h2>
    <p class="section-subtitle">5 pasos simples para encontrar a tu compañero ideal</p>

    <!-- Timeline with hover-expandable steps -->
    <div class="steps-track">
      <!-- Connecting line -->
      <div class="track-line"></div>

      <div
        v-for="(step, idx) in steps"
        :key="idx"
        class="step-wrapper"
        @mouseenter="hoveredStep = idx"
        @mouseleave="hoveredStep = null"
      >
        <!-- Circle indicator (always visible) -->
        <div
          class="step-dot"
          :style="{ background: step.color, boxShadow: `0 0 0 5px ${step.color}22` }"
          :class="{ active: hoveredStep === idx }"
        >
          <span class="dot-icon">
            <component
              v-if="iconComponents[step.icon]"
              :is="iconComponents[step.icon]"
              class="boxicon"
            />
            <span v-else>{{ step.icon }}</span>
          </span>
        </div>

        <!-- Card (visible on hover) -->
        <Transition name="step-pop">
          <div
            v-if="hoveredStep === idx"
            class="step-card"
            :style="{ borderTop: `4px solid ${step.color}` }"
          >
            <div class="step-card-head">
              <span class="step-icon-lg">
                <component
                  v-if="iconComponents[step.icon]"
                  :is="iconComponents[step.icon]"
                  class="boxicon"
                />
              </span>
              <span class="step-number-label">PASO {{ step.number }}</span>
            </div>
            <h3 class="step-title">{{ step.title }}</h3>
            <p class="step-desc">{{ step.description }}</p>
            <span
              class="badge"
              :style="{ background: step.badgeColor, color: step.badgeText }"
            >{{ step.badge }}</span>
          </div>
        </Transition>
      </div>
    </div>

    <!-- CTA Banner -->
    <div class="cta-banner">
      <div class="cta-text">
        <strong>¿Lista para comenzar?</strong>
        <span>Hay 26 mascotas esperando un hogar ahora mismo.</span>
      </div>
      <RouterLink to="/mascotas" class="btn-yellow">Ver mascotas disponibles →</RouterLink>
    </div>
  </section>

  <!-- ══════════════════════════════════
       FEATURED PETS TEASER
  ══════════════════════════════════ -->
  <section class="featured-section">
    <div class="container">
      <h2 class="section-title">Mascotas destacadas</h2>
      <p class="section-subtitle">Conoce a algunos de nuestros amigos que esperan hogar</p>

      <div class="pets-grid">
        <div v-for="pet in featuredPets" :key="pet.name" class="pet-card">
          <div class="pet-photo" :style="{ background: pet.bg }">
            <span class="pet-emoji">{{ pet.emoji }}</span>
          </div>
          <div class="pet-info">
            <div class="pet-header">
              <span class="pet-name">{{ pet.name }}</span>
              <span class="badge badge-green">{{ pet.status }}</span>
            </div>
            <p class="pet-meta">{{ pet.type }} · {{ pet.breed }} · {{ pet.age }}</p>
            <RouterLink to="/mascotas" class="btn-outline-green pet-btn">Ver perfil</RouterLink>
          </div>
        </div>
      </div>

      <div class="see-all-wrap">
        <RouterLink to="/mascotas" class="btn-primary">Ver todas las mascotas 🐾</RouterLink>
      </div>
    </div>
  </section>

  <!-- ══════════════════════════════════
       DONATE / HELP STRIP
  ══════════════════════════════════ -->
  <section class="help-strip">
    <div class="container help-grid">
      <div class="help-card">
        <span class="help-icon">💰</span>
        <h3>Dona</h3>
        <p>Tu contribución ayuda a pagar tratamientos médicos, comida y albergue para los animales rescatados.</p>
        <RouterLink to="/nosotros#donacion" class="btn-primary">Hacer una donación</RouterLink>
      </div>
      <div class="help-card">
        <span class="help-icon">🏡</span>
        <h3>Sé casa cuna</h3>
        <p>Ofrece un hogar temporal mientras la mascota espera ser adoptada. Nosotros cubrimos los gastos médicos.</p>
        <RouterLink to="/voluntarios" class="btn-primary">Quiero ser casa cuna</RouterLink>
      </div>
      <div class="help-card">
        <span class="help-icon">🤝</span>
        <h3>Voluntariado</h3>
        <p>Dedica unas horas de tu tiempo para ayudar en eventos de adopción, cuidado de animales y más.</p>
        <RouterLink to="/voluntarios" class="btn-primary">Ser voluntario</RouterLink>
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
        { name: 'Luna',    emoji: '🐕', bg: '#E8F5EE', type: 'Perro',  breed: 'Mestiza',   age: '2 años',   status: 'Disponible' },
        { name: 'Mochi',   emoji: '🐈', bg: '#FFF0E8', type: 'Gato',   breed: 'Siamés',    age: '1 año',    status: 'Disponible' },
        { name: 'Rocky',   emoji: '🐕', bg: '#EEF0FF', type: 'Perro',  breed: 'Labrador',  age: '3 años',   status: 'Disponible' },
        { name: 'Canela',  emoji: '🐕', bg: '#FFF8E1', type: 'Perro',  breed: 'Shih Tzu',  age: '4 meses',  status: 'En proceso' },
      ],
    }
  },
}
</script>

<style scoped>

/* ─── HERO ─── */
.hero {
  background: linear-gradient(135deg, var(--green-dark) 0%, var(--green) 60%, var(--green-light) 100%);
  min-height: 520px;
  display: flex;
  align-items: center;
}
.hero-inner {
  display: flex;
  align-items: center;
  gap: 60px;
  padding-top: 60px;
  padding-bottom: 60px;
}
.hero-image-wrap {
  flex-shrink: 0;
}
.hero-img-placeholder {
  width: 340px;
  height: 360px;
  background: rgba(255,255,255,0.12);
  border-radius: var(--radius-lg);
  display: flex;
  align-items: center;
  justify-content: center;
  border: 2px solid rgba(255,255,255,0.2);
}

.img-hero {
    width: auto;
    height: 300px;
}

.dog-emoji { font-size: 120px; filter: drop-shadow(0 8px 20px rgba(0,0,0,0.2)); }

.hero-tag {
  display: inline-block;
  background: rgba(255,255,255,0.18);
  color: var(--white);
  border-radius: var(--radius-full);
  padding: 4px 14px;
  font-size: 13px;
  font-weight: 600;
  letter-spacing: 0.5px;
  margin-bottom: 16px;
}
.hero-content h1 {
  font-size: 44px;
  font-weight: 800;
  color: var(--white);
  line-height: 1.15;
  margin-bottom: 16px;
}
.hero-content p {
  font-size: 17px;
  color: rgba(255,255,255,0.87);
  line-height: 1.7;
  max-width: 480px;
  margin-bottom: 28px;
}
.hero-actions {
  display: flex;
  gap: 14px;
  flex-wrap: wrap;
}

/* ─── STATS BAR ─── */
.stats-bar {
  background: var(--light);
  padding: 10px 0;
}
.stats-inner {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  flex-wrap: wrap;
  height: 30vh;
}
.stat-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 2px;
  min-height: 105px;
  min-width: 190px;
  background-color: var(--yellow);
  border-radius: 10px;
}
.stat-value {
  font-size: 36px;
  font-weight: 600;
  color: #000;
  line-height: 1.1;
}
.stat-label {
  font-size: 12px;
  color: var(--green-dark);
  opacity: 0.75;
  text-align: center;
  color: #000;

}

/* ─── HOW SECTION ─── */
.how-section {
  background: var(--cream);
  padding: 70px 24px 0;
  height: 77vh;
}

.steps-track {
  position: relative;
  max-width: 900px;
  margin: 0 auto;
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  padding: 60px 0 40px;
  min-height: 450px;
}

/* horizontal connecting line */
.track-line {
  position: absolute;
  top: 88px;   /* center of dots */
  left: 40px;
  right: 40px;
  height: 3px;
  background: linear-gradient(to right, var(--green-light), var(--green-pale));
  border-radius: 4px;
  z-index: 0;
}

.step-wrapper {
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: center;
  width: 140px;
  cursor: pointer;
  z-index: 1;
}

/* Circle dot */
.step-dot {
  width: 64px;
  height: 64px;
  border-radius: 50%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  transition: transform 0.25s, box-shadow 0.25s;
  position: relative;
  z-index: 2;
}
.step-dot.active,
.step-wrapper:hover .step-dot {
  transform: scale(1.18);
}


.dot-icon {
  font-size: 22px;
  display: block;
  line-height: 1;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  flex: 1;
}

.boxicon {
  width: 22px;
  height: 22px;
  display: block;
}

.dot-num {
  font-size: 10px;
  font-weight: 700;
  color: var(--white);
  opacity: 0.85;
  letter-spacing: 0.5px;
}

/* Expanded card */
.step-card {
  position: absolute;
  top: 80px;
  left: 50%;
  transform: translateX(-50%);
  width: 230px;
  background: var(--white);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-lg);
  padding: 18px;
  z-index: 10;
  pointer-events: none;
}

/* keep last 2 cards from overflowing right */


.step-card-head {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 8px;
}
.step-icon-lg { font-size: 22px; }
.step-number-label {
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 1px;
  color: var(--text-light);
}
.step-title {
  font-size: 16px;
  font-weight: 700;
  color: var(--text-dark);
  margin-bottom: 8px;
}
.step-desc {
  font-size: 13px;
  color: var(--text-mid);
  line-height: 1.5;
  margin-bottom: 10px;
}

/* Transition */
.step-pop-enter-active  { transition: opacity 0.2s, transform 0.22s; }
.step-pop-leave-active  { transition: opacity 0.15s; }
.step-pop-enter-from    { opacity: 0; transform: translateX(-50%) translateY(10px); }
.step-pop-enter-to      { opacity: 1; transform: translateX(-50%) translateY(0); }
.step-pop-leave-to      { opacity: 0; }

/* CTA Banner */
.cta-banner {
  max-width: 900px;
  margin: 0 auto;
  background: var(--green-dark);
  border-radius: var(--radius-lg);
  padding: 26px 36px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 20px;
  flex-wrap: wrap;
  margin-bottom: -20px;
  position: relative;
  z-index: 1;
}
.cta-text {
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.cta-text strong { font-size: 20px; color: var(--white); }
.cta-text span   { font-size: 14px; color: rgba(255,255,255,0.75); }

/* ─── FEATURED PETS ─── */
.featured-section {
  background: var(--white);
  padding: 80px 24px 60px;
}
.pets-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 20px;
  margin-bottom: 36px;
}
.pet-card {
  background: var(--white);
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  overflow: hidden;
  box-shadow: var(--shadow-sm);
  transition: box-shadow 0.2s, transform 0.2s;
}
.pet-card:hover {
  box-shadow: var(--shadow-md);
  transform: translateY(-3px);
}
.pet-photo {
  height: 180px;
  display: flex;
  align-items: center;
  justify-content: center;
}
.pet-emoji { font-size: 72px; }
.pet-info  { padding: 16px; }
.pet-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 6px;
}
.pet-name  { font-size: 18px; font-weight: 700; color: var(--text-dark); }
.pet-meta  { font-size: 13px; color: var(--text-light); margin-bottom: 14px; }
.pet-btn   { font-size: 13px; padding: 7px 16px; width: 100%; justify-content: center; }
.see-all-wrap { text-align: center; }

/* ─── HELP STRIP ─── */
.help-strip {
  background: var(--cream);
  padding: 60px 24px;
}
.help-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 24px;
}
.help-card {
  background: var(--white);
  border-radius: var(--radius-lg);
  padding: 32px 28px;
  text-align: center;
  box-shadow: var(--shadow-sm);
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
}
.help-icon {
  font-size: 44px;
  display: block;
  margin-bottom: 4px;
}
.help-card h3 {
  font-size: 22px;
  font-weight: 700;
  color: var(--text-dark);
}
.help-card p {
  font-size: 14px;
  color: var(--text-mid);
  line-height: 1.6;
  flex: 1;
}

/* ─── Responsive ─── */
@media (max-width: 900px) {
  .hero-inner { flex-direction: column; text-align: center; gap: 30px; }
  .hero-img-placeholder { width: 220px; height: 220px; }
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
