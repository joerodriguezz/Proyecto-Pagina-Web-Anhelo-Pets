<script setup>
import NavBar from '../components/NavBar.vue'
import FooterBar from '../components/FooterBar.vue'


const gallery = [

  '/img-nosotros/1.JPG',
  '/img-nosotros/2.JPG',
  '/img-nosotros/3.JPG',
  '/img-nosotros/4.JPG',
  '/img-nosotros/5.JPG',
  '/img-nosotros/6.JPG',
  '/img-nosotros/7.JPG',
  '/img-nosotros/8.JPG',
  '/img-nosotros/9.JPG',
  '/img-nosotros/10.JPG'

]
</script>

<template>

  <NavBar />

  <!-- HERO -->

  <section class="hero">

    <img
      src="/img-nosotros/hero.PNG"
      class="hero-image"
    >

    <div class="hero-overlay"></div>

    <div class="hero-content">

      <h1>
        Una historia nacida
        del amor por rescatar
      </h1>

      <p>
        Personas, hogares y segundas oportunidades
        que cambiaron cientos de vidas.
      </p>

    </div>

  </section>

  <!-- HISTORIA -->

  <section class="history-section-new">

    <div class="history-layout">

      <div class="history-photo">

        <img
          src="/img-nosotros/inicioanhelo.PNG"
          class="history-photo-img"
        >

        <svg class="history-paw" viewBox="0 0 24 24" fill="white">
          <path d="M12 14c-3 0-6 2-6 5a1 1 0 0 0 1 1h10a1 1 0 0 0 1-1c0-3-3-5-6-5zM6.5 9A2.5 2.5 0 1 0 6.5 4a2.5 2.5 0 0 0 0 5zm11 0A2.5 2.5 0 1 0 17.5 4a2.5 2.5 0 0 0 0 5zM3.5 13A2 2 0 1 0 3.5 9a2 2 0 0 0 0 4zm17 0a2 2 0 1 0 0-4 2 2 0 0 0 0 4z"/>
        </svg>

      </div>

      <div class="history-panel">

        <h2>
          El inicio de<br>Anhelo Pets
        </h2>

        <div class="history-text">

          <p>
            Todo comenzó con un simple acto de empatía.
          </p>

          <p>
            Al ver la realidad que enfrentaban muchos perros y gatos
            en situación de abandono en Quepos, nació el deseo de
            ayudar, rescatar y brindar una segunda oportunidad a
            quienes más lo necesitaban.
          </p>

          <p>
            Lo que inició con pequeños rescates y apoyo voluntario
            fue creciendo hasta convertirse en una comunidad unida
            por el amor y el compromiso con el bienestar animal.
          </p>

        </div>

        <div class="history-quote">

          <p>
            Hoy transformamos historias de abandono en historias de
            recuperación, esperanza y nuevos comienzos.
          </p>

        </div>

      </div>

    </div>

  </section>


  <section class="about-background">

    <!-- MISIÓN Y VISIÓN -->

    <section class="purpose-section-new">

      <div class="purpose-container-new">

        <div class="purpose-header-new">

        </div>

        <div class="purpose-grid-new">

          <div class="purpose-box-light">

            <h3>Misión</h3>

            <p>
              Cumplir con las campañas de castración que realizamos
              para brindar una mejor calidad de vida a los animales,
              prevenir la sobrepoblación y contribuir al bienestar de una comunidad que,
              en muchas ocasiones, ha sido olvidada.
            </p>

          </div>

          <div class="purpose-box-dark">

         

            <h3>Visión</h3>

            <p>
              Rescatar, rehabilitar y encontrar familias responsables para cada uno de los animales
              que la Fundación Anhelo Pet's acoge es nuestro propósito.
              Aspiramos a que, en el futuro, más personas se unan a la protección y el bienestar animal,
              promoviendo la conciencia y el compromiso con esta causa tan importante.
            </p>

          </div>

        </div>

      </div>

    </section>

    <!-- GALERÍA -->

    <section class="gallery-section">

      <div class="gallery-track">

        <div
          class="gallery-item"
          v-for="(img, index) in gallery"
          :key="index"
        >

          <img
            :src="img"
            class="gallery-image"
          >

        </div>

      </div>

    </section>

  </section>


  <FooterBar />

</template>

<style scoped>

.about-background {

  background-image: url('/img-nosotros/fondo.PNG');

  background-size: cover;

  background-position: center;

  background-repeat: no-repeat;

  position: relative;
}

.about-background::before {

  content: '';

  position: absolute;

  inset: 0;

  background: rgba(58,71,60,0.55);
}

/* GENERAL */

.history-section-new,
.purpose-section-new,
.gallery-section,
.team-section {

  background: #FAFAFA;
}

/* HERO */

.hero {

  position: relative;

  height: 430px;

  overflow: hidden;
}

.hero-image {

  width: 100%;

  height: 100%;

  object-fit: cover;

  object-position: center 34%;

  transform: scale(1);

  filter:
    brightness(0.72)
    contrast(1.02);
}

.hero-overlay {

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

.hero-content {

  position: absolute;

  left: 7%;

  bottom: 55px;

  max-width: 560px;

  z-index: 2;
}

.hero-content h1 {

  font-size: 62px;

  line-height: 0.95;

  font-weight: 800;

  letter-spacing: -3px;

  color: white;

  margin-bottom: 24px;
}

.hero-content p {

  font-size: 16px;

  line-height: 1.7;

  color: rgba(255,255,255,0.92);

  max-width: 420px;

  margin: 0;
}

@media (max-width: 700px) {

  .hero {

    height: 360px;
  }

  .hero-content {

    left: 24px;

    right: 24px;

    bottom: 30px;

    max-width: 300px;
  }

  .hero-content h1 {

    font-size: 38px;

    line-height: 0.95;
  }

  .hero-content p {

    font-size: 14px;

    line-height: 1.6;
  }
}

/* HISTORY (nuevo) */

.history-section-new {

  padding: 40px 24px 70px;
}

.history-layout {

  max-width: 1100px;

  margin: 0 auto;

  display: grid;

  grid-template-columns: 0.85fr 1.15fr;

  align-items: center;
}

.history-photo {

  position: relative;

  z-index: 2;

  border-radius: 24px;

  overflow: hidden;

  box-shadow: 0 20px 45px rgba(58,71,60,0.22);
}

.history-photo-img {

  width: 100%;

  height: 460px;

  object-fit: cover;

  display: block;

  transition: 0.8s ease;
}

.history-photo:hover .history-photo-img {

  transform: scale(1.04);
}

.history-paw {

  position: absolute;

  bottom: 16px;

  right: 16px;

  width: 70px;

  height: 70px;

  opacity: 0.35;

  pointer-events: none;
}

.history-panel {

  background: white;

  border-radius: 24px;

  padding: 44px 40px;

  margin-left: -70px;

  position: relative;

  z-index: 3;

  box-shadow: 0 18px 40px rgba(58,71,60,0.10);
}

.history-panel h2 {

  font-size: 38px;

  font-weight: 800;

  color: #3A473C;

  letter-spacing: -1.5px;

  line-height: 1.05;

  margin-bottom: 18px;
}

.history-text {

  display: flex;

  flex-direction: column;

  gap: 10px;
}

.history-text p {

  font-size: 14.5px;

  line-height: 1.75;

  color: #687168;

  margin: 0;
}

.history-quote {

  border-left: 3px solid #92A894;

  padding-left: 16px;

  margin-top: 22px;
}

.history-quote p {

  font-size: 14.5px;

  line-height: 1.75;

  color: #3A473C;

  font-weight: 500;

  margin: 0;
}

/* GALLERY */

.gallery-section {

  position: relative;

  z-index: 2;

  background: transparent;

  padding:
    60px 0
    90px;

  overflow: hidden;
}

.gallery-track {

  display: flex;

  gap: 24px;

  width: max-content;

  padding-left: 24px;

  animation:
    scrollGallery 65s linear infinite;
}

@keyframes scrollGallery {

  from {

    transform: translateX(0);
  }

  to {

    transform: translateX(-35%);
  }

}

.gallery-item {

  width: 360px;

  height: 250px;

  border-radius: 28px;

  overflow: hidden;

  flex-shrink: 0;

  box-shadow:
    0 14px 35px rgba(58,71,60,0.08);
}

.gallery-image {

  width: 100%;

  height: 100%;

  object-fit: cover;

  object-position: center;

  transition: 0.6s ease;
}

.gallery-item:hover .gallery-image {

  transform: scale(1.08);
}

/* MISIÓN Y VISIÓN (nuevo) */

.purpose-section-new {

  padding: 20px 24px 80px;

  background: transparent;
}

.purpose-container-new {

  max-width: 1100px;

  margin: 0 auto;
}

.purpose-header-new {

  text-align: center;

  margin-bottom: 38px;
}

.purpose-header-new span {

  color: #3A473C;

  font-size: 12px;

  font-weight: 700;

  letter-spacing: 1px;
}

.purpose-header-new h2 {

  font-size: 38px;

  font-weight: 800;

  color: #3A473C;

  letter-spacing: -1.5px;

  margin: 10px 0 0;
}

.purpose-grid-new {

  display: grid;

  grid-template-columns: 0.9fr 1.1fr;

  gap: 22px;
}

.purpose-box-light {

  background: rgba(255,255,255,0.88);

  backdrop-filter: blur(12px);

  -webkit-backdrop-filter: blur(12px);

  border: 1px solid rgba(255,255,255,0.30);

  border-radius: 24px;

  padding: 36px 30px;

  box-shadow: 0 14px 32px rgba(58,71,60,0.10);

  display: flex;

  flex-direction: column;

  justify-content: center;
}

.purpose-box-light h3 {

  font-size: 24px;

  font-weight: 800;

  color: #3A473C;

  margin-bottom: 14px;
}

.purpose-box-light p {

  font-size: 14.5px;

  line-height: 1.8;

  color: #687168;

  margin: 0;
}

.purpose-box-dark {

  background: #3A473C;

  border-radius: 24px;

  padding: 36px 30px;

  position: relative;

  overflow: hidden;

  display: flex;

  flex-direction: column;

  justify-content: center;
}

.purpose-leaf {

  position: absolute;

  top: -20px;

  right: -20px;

  width: 140px;

  height: 140px;

  opacity: 0.10;

  pointer-events: none;
}

.purpose-box-dark h3 {

  font-size: 24px;

  font-weight: 800;

  color: white;

  margin-bottom: 14px;

  position: relative;

  z-index: 1;
}

.purpose-box-dark p {

  font-size: 14.5px;

  line-height: 1.8;

  color: rgba(255,255,255,0.82);

  margin: 0;

  position: relative;

  z-index: 1;
}

/* TEAM */

.team-section {

  padding:
    30px 24px
    100px;
}

.section-heading {

  text-align: center;

  margin-bottom: 44px;
}

.section-heading span {

  color: #3A473C;

  font-size: 12px;

  font-weight: 700;

  letter-spacing: 1px;
}

.section-heading h2 {

  font-size: 46px;

  font-weight: 800;

  color: #3A473C;

  margin:
    14px 0 10px;

  letter-spacing: -2px;
}

.section-heading p {

  font-size: 15px;

  color: #6C756D;
}

.team-grid {

  max-width: 1200px;

  margin: 0 auto;

  display: grid;

  grid-template-columns:
    repeat(3, 1fr);

  gap: 24px;
}

.team-card {

  background: white;

  border-radius: 28px;

  overflow: hidden;

  transition: 0.35s ease;

  border:
    1px solid rgba(146,168,148,0.10);

  box-shadow:
    0 10px 30px rgba(58,71,60,0.05);
}

.team-card:hover {

  transform:
    translateY(-8px);

  box-shadow:
    0 18px 42px rgba(58,71,60,0.10);
}

.team-image-wrap {

  overflow: hidden;

  height: 270px;
}

.team-image {

  width: 100%;

  height: 100%;

  object-fit: cover;

  transition: 0.6s ease;
}

.team-card:hover .team-image {

  transform: scale(1.08);
}

.team-content {

  padding: 24px;
}

.team-role {

  font-size: 11px;

  font-weight: 700;

  letter-spacing: 1px;

  color: #3A473C;

  display: block;

  margin-bottom: 10px;
}

.team-content h3 {

  font-size: 22px;

  font-weight: 800;

  color: #3A473C;

  margin-bottom: 12px;
}

.team-content p {

  font-size: 14px;

  line-height: 1.8;

  color: #687168;
}

/* RESPONSIVE */

@media (max-width: 980px) {

  .history-layout {

    grid-template-columns: 1fr;
  }

  .history-photo-img {

    height: 340px;
  }

  .history-panel {

    margin-left: 0;

    margin-top: -50px;
  }

  .purpose-grid-new {

    grid-template-columns: 1fr;
  }

  .team-grid {

    grid-template-columns: 1fr 1fr;
  }

}

@media (max-width: 700px) {

  .hero {

    height: 360px;
  }

  .hero-content {

    left: 24px;

    right: 24px;

    bottom: 30px;

    max-width: 300px;
  }

  .hero-content h1 {

    font-size: 38px;
  }

  .hero-content p {

    font-size: 14px;
  }

  .history-section-new {

    padding: 24px 16px 40px;
  }

  .history-photo-img {

    height: 240px;
  }

  .history-panel {

    padding: 28px 22px;

    border-radius: 20px;
  }

  .history-panel h2 {

    font-size: 28px;
  }

  .purpose-section-new {

    padding: 16px 16px 56px;
  }

  .purpose-header-new h2 {

    font-size: 28px;
  }

  .purpose-box-light,
  .purpose-box-dark {

    padding: 26px 22px;

    border-radius: 20px;
  }

  .team-grid {

    grid-template-columns: 1fr;
  }

  .gallery-item {

    width: 250px;

    height: 180px;
  }

}

/* ── MOBILE RESPONSIVE ── */
@media (max-width: 768px) {
  .hero {
    height: 300px;
  }

  .hero-content {
    left: 16px;
    right: 16px;
    bottom: 24px;
    max-width: 100%;
  }

  .hero-content h1 {
    font-size: 32px;
    letter-spacing: -1.5px;
    margin-bottom: 12px;
  }

  .hero-content p {
    font-size: 14px;
  }

  .gallery-section {
    padding: 40px 0 60px;
  }

  .gallery-item {
    width: 220px;
    height: 160px;
    border-radius: 18px;
  }
}

@media (max-width: 480px) {
  .hero {
    height: 260px;
  }

  .hero-content h1 {
    font-size: 26px;
  }

  .history-panel h2 {
    font-size: 26px;
  }
}

</style>