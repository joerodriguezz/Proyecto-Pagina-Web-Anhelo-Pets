<script setup>
import { ref, computed } from 'vue'
import NavBar from '../components/NavBar.vue'
import FooterBar from '../components/FooterBar.vue'
import { usePetsStore } from '../stores/usePetsStore'
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

const petsStore = usePetsStore()

const hoveredStep = ref(null)

// ── Mascotas destacadas dinámicas desde el store ──
const featuredPets = computed(() =>
  petsStore.pets
    .filter(p => p.status === 'Disponible')
    .slice(0, 4)
)

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
</script>

<template>
  <NavBar />

  <!-- ══════════════════════════════════
       HERO
  ══════════════════════════════════ -->
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

  <!-- ══════════════════════════════════
       STATS BAR
  ══════════════════════════════════ -->
  <section class="stats-bar">
    <div class="stats-inner container">
      <div
        v-for="stat in stats"
        :key="stat.label"
        class="stat-item"
      >
        <div class="stat-icon-wrap">
          <Heart     v-if="stat.icon === 'heart'"    class="stat-icon" />
          <Cat       v-if="stat.icon === 'cat'"      class="stat-icon" />
          <Home      v-if="stat.icon === 'home'"     class="stat-icon" />
          <Building  v-if="stat.icon === 'building'" class="stat-icon building-fix" />
          <Calendar  v-if="stat.icon === 'calendar'" class="stat-icon" />
        </div>
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
      <RouterLink to="/mascotas" class="btn-primary">Ver mascotas disponibles →</RouterLink>
    </div>
  </section>

  <!-- ══════════════════════════════════
       FEATURED PETS TEASER
  ══════════════════════════════════ -->
  <section class="featured-section">
    <div class="container">
      <h2 class="section-title">Mascotas destacadas</h2>
      <p class="section-subtitle">Conoce a algunos de nuestros amigos que esperan hogar</p>

      <!-- Sin mascotas disponibles -->
      <div v-if="featuredPets.length === 0" class="empty-pets">
        <p>No hay mascotas disponibles en este momento.</p>
      </div>

      <div v-else class="pets-grid">
        <div
          v-for="pet in featuredPets"
          :key="pet.id"
          class="pet-card"
        >
          <div class="pet-photo">
            <img
              :src="pet.images?.[0]?.preview || '/img-mascotas/mascotas.jpg'"
              :alt="pet.name"
              class="pet-image"
            >
          </div>

          <div class="pet-info">
            <div class="pet-header">
              <span class="pet-name">{{ pet.name }}</span>
              <span class="badge badge-green">{{ pet.status }}</span>
            </div>

            <p class="pet-meta">
              {{ pet.type }} · {{ pet.age }} · {{ pet.sex }}
            </p>

            <p class="pet-description">
              {{ pet.personality }}
            </p>

            <RouterLink to="/mascotas" class="btn-outline-green pet-btn">
              Ver perfil
            </RouterLink>
          </div>
        </div>
      </div>

      <div class="see-all-wrap">
        <RouterLink to="/mascotas" class="btn-all-pets">
          Ver todas las mascotas →
        </RouterLink>
      </div>
    </div>
  </section>

  <!-- ══════════════════════════════════
       DONATE / HELP STRIP
  ══════════════════════════════════ -->
  <section class="help-strip">
    <div class="container">
      <h2 class="section-title">¿Cómo puedes ayudar?</h2>
      <p class="section-subtitle">Cada pequeña acción puede cambiar la vida de un animal rescatado.</p>

      <div class="help-grid">

        <!-- DONAR -->
        <div class="help-card">
          <div class="help-image-wrap">
            <img src="/img-ayuda/donar.png" class="help-image">
          </div>
          <h3>Dona</h3>
          <p>Tu contribución ayuda con alimento, rescates, medicamentos y tratamientos veterinarios.</p>
          <RouterLink to="/donar" class="btn-primary">Hacer una donación</RouterLink>
        </div>

        <!-- CASA CUNA -->
        <div class="help-card">
          <div class="help-image-wrap">
            <img src="/img-ayuda/casa.png" class="help-image">
          </div>
          <h3>Sé casa cuna</h3>
          <p>Brinda un hogar temporal mientras encuentran una familia definitiva y segura.</p>
          <RouterLink to="/voluntarios" class="btn-primary">Quiero ser casa cuna</RouterLink>
        </div>

        <!-- VOLUNTARIO -->
        <div class="help-card">
          <div class="help-image-wrap">
            <img src="/img-ayuda/voluntario.png" class="help-image">
          </div>
          <h3>Voluntariado</h3>
          <p>Ayuda en eventos, rescates, transporte, limpieza y cuidado de animales.</p>
          <RouterLink to="/voluntarios" class="btn-primary">Ser voluntario</RouterLink>
        </div>

      </div>
    </div>
  </section>

  <FooterBar />
</template>

<style scoped>

/* ─── HERO ─── */
.hero {
  background: #FFFFFF;
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
  width: 42%;
  padding-right: 60px;
}

.hero-content h1 {
  font-size: 72px;
  font-weight: 800;
  color: #3A473C;
  line-height: 1.05;
  margin-bottom: 24px;
  letter-spacing: -2px;
}

.highlight { color: #92A894; }

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

/* Interacciones y Botones del Hero (Forzado de colores correctos) */
.hero-actions .btn-primary,
.hero-actions .btn-success,
.hero-actions a[href*="mascotas"],
.hero-actions a[href*="disponibles"] {
  background: #3A473C !important;
  color: #FFFFFF !important;
  border: 2px solid #3A473C !important;
  padding: 14px 28px;
  border-radius: 14px;
  font-size: 15px;
  font-weight: 700;
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  transition: 0.25s ease;
}

.hero-actions .btn-primary:hover,
.hero-actions .btn-success:hover,
.hero-actions a[href*="mascotas"]:hover,
.hero-actions a[href*="disponibles"]:hover {
  background: #2D372F !important;
  border-color: #2D372F !important;
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(58, 71, 60, 0.15);
}

.hero-actions .btn-secondary,
.hero-actions .btn-outline-success,
.hero-actions a[href*="dona"],
.hero-actions a[href*="cuna"],
.hero-actions a[href*="voluntario"] {
  background: transparent !important;
  color: #3A473C !important;
  border: 2px solid #92A894 !important;
  padding: 14px 28px;
  border-radius: 14px;
  font-size: 15px;
  font-weight: 700;
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  transition: 0.25s ease;
}

.hero-actions .btn-secondary:hover,
.hero-actions .btn-outline-success:hover,
.hero-actions a[href*="dona"]:hover,
.hero-actions a[href*="cuna"]:hover,
.hero-actions a[href*="voluntario"]:hover {
  background: #F4F6F4 !important;
  border-color: #3A473C !important;
  color: #3A473C !important;
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
  width: 170px;
  height: 230px;
  background: #FFFFFF;
  border-radius: 18px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 14px;
  box-shadow: 0 4px 18px rgba(58, 71, 60, 0.02);
  border: 1px solid #F4F6F4;
  transition: 0.25s ease;
}

.stat-icon-wrap {
  width: 72px;
  height: 72px;
  border-radius: 50%;
  background: rgba(146, 168, 148, 0.15);
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
}

.stat-icon {
  width: 36px;
  height: 36px;
  color: #5A6E5C;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  top: -1px;
}

.building-fix { transform: translateY(-4px); }

.stat-item:hover { 
  transform: translateY(-5px); 
  border-color: #92A894;
}

.stat-value {
  font-size: 42px;
  font-weight: 800;
  color: #3A473C;
  line-height: 1;
}

.stat-label {
  font-size: 14px;
  font-weight: 700;
  color: #6C756D;
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
  border-color: #92A894;
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
  margin-bottom: -20px;
  position: relative;
  z-index: 1;
  box-shadow: 0 10px 30px rgba(58, 71, 60, 0.12);
}

.cta-text { display: flex; flex-direction: column; gap: 4px; }
.cta-text strong { font-size: 20px; color: #FFFFFF; font-weight: 800; }
.cta-text span   { font-size: 14px; color: #92A894; font-weight: 500; }

/* ─── FEATURED PETS (FOTOS EXTRAÍDAS TOTALMENTE) ─── */
.featured-section {
  background: #FFFFFF;
  padding: 80px 24px 60px;
  border-top: 2px solid #F4F6F4;
}

.pets-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 20px;
  margin-bottom: 36px;
}

.pet-card {
  background: #FFFFFF;
  border: 2px solid #F4F6F4;
  border-radius: 20px;
  overflow: hidden;
  box-shadow: 0 4px 20px rgba(58, 71, 60, 0.02);
  transition: all 0.25s ease;
}

.pet-card:hover {
  box-shadow: 0 10px 25px rgba(58, 71, 60, 0.05);
  border-color: #92A894;
  transform: translateY(-3px);
}

.pet-info    { padding: 24px; }

.pet-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 8px;
}

.pet-name { font-size: 20px; font-weight: 800; color: #3A473C; letter-spacing: -0.3px; }
.pet-meta { font-size: 13px; color: #6C756D; font-weight: 600; margin-bottom: 14px; }

.pet-description {
  font-size: 14px;
  color: #3A473C;
  line-height: 1.6;
  margin-bottom: 18px;
}

.pet-btn {
  font-size: 13px;
  padding: 10px 16px;
  width: 100%;
  border-radius: 12px;
  border: 2px solid #F4F6F4;
  background: #F4F6F4;
  color: #3A473C;
  font-weight: 700;
  cursor: pointer;
  display: flex;
  justify-content: center;
  align-items: center;
  transition: all 0.2s ease;
}

.pet-card:hover .pet-btn {
  background: #3A473C;
  border-color: #3A473C;
  color: #FFFFFF;
}

.see-all-wrap { text-align: center; }

.empty-pets {
  text-align: center;
  padding: 40px;
  color: #6C756D;
  font-size: 15px;
  font-weight: 500;
}

.badge { padding: 6px 12px; border-radius: 10px; font-size: 12px; font-weight: 700; display: inline-block; }
.badge-green { background: rgba(146, 168, 148, 0.2); color: #5A6E5C; }

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
  background: rgba(255, 255, 255, 0.85);
  backdrop-filter: blur(14px);
  border-radius: 28px;
  padding: 34px 28px;
  border: 2px solid #FFFFFF;
  text-align: center;
  box-shadow: 0 8px 24px rgba(58, 71, 60, 0.02);
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
  background: linear-gradient(120deg, transparent, rgba(255, 255, 255, 0.4), transparent);
  transform: rotate(25deg);
  transition: 0.8s ease;
}

.help-card:hover::before { top: 120%; }

.help-card:hover {
  transform: translateY(-12px) scale(1.02);
  border-color: #92A894;
  box-shadow: 0 22px 50px rgba(58, 71, 60, 0.06);
}

.help-image-wrap {
  width: 100px;
  height: 100px;
  margin: 0 auto 24px;
  border-radius: 50%;
  background: #FAFAFA;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: 0.35s ease;
}

.help-card:hover .help-image-wrap { transform: scale(1.08); background: #FFFFFF; }

.help-image { width: 58px; height: 58px; object-fit: contain; }

.help-card h3 { font-size: 24px; font-weight: 800; margin-bottom: 14px; color: #3A473C; letter-spacing: -0.5px; }
.help-card p  { font-size: 15px; line-height: 1.7; color: #6C756D; margin-bottom: 24px; }

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
  .pets-grid       { grid-template-columns: 1fr 1fr; }
  .help-grid       { grid-template-columns: 1fr 1fr; }
}

@media (max-width: 560px) {
  .pets-grid  { grid-template-columns: 1fr; }
  .help-grid  { grid-template-columns: 1fr; }
  .cta-banner { flex-direction: column; text-align: center; }
}
</style>