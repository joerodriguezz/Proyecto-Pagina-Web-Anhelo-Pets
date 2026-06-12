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

  <section class="history-section">

    <div class="history-card">

  <!-- IMAGEN -->

<div class="history-image-side">

  <img
    src="/img-nosotros/inicioanhelo.PNG"
    class="history-main-image"
  >

</div>

<!-- TEXTO -->

<div class="history-content-side">

  <h2>
    El inicio de
    Anhelo Pets
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

    <p>
      Hoy, gracias al esfuerzo de voluntarios, casas cuna,
      adoptantes y personas solidarias, Anhelo Pets continúa
      transformando historias de abandono en historias de
      recuperación, esperanza y nuevos comienzos.
    </p>

  </div>

</div>
    </div>

  </section>


  
<section class="about-background">

  

  <!-- MISIÓN Y VISIÓN -->

  <section class="purpose-section">

    <div class="purpose-container">

      <div class="purpose-header">

        <h2>
    
        </h2>

      </div>

      <div class="purpose-grid">

        <div class="purpose-box">

          <h3>Misión</h3>

          <p>
            Rescatar, rehabilitar y proteger animales en situación
            de abandono o vulnerabilidad, promoviendo adopciones
            responsables y creando conciencia sobre el bienestar
            animal mediante el trabajo comunitario y la educación.
          </p>

        </div>

        <div class="purpose-box">

          <h3>Visión</h3>

          <p>
            Ser una organización referente en protección animal
            en Costa Rica, construyendo una sociedad donde cada
            mascota tenga acceso a una vida digna, segura y llena
            de amor dentro de una familia responsable.
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

.purpose-section,


/* GENERAL */

.history-section,
.purpose-section,
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
      rgba(58,71,60,0.38) 0%,
      rgba(58,71,60,0.10) 40%,
      rgba(58,71,60,0.02) 100%
    );
}

.hero-content {

  position: absolute;

  left: 7%;

  bottom: 38px;

  max-width: 360px;

  z-index: 2;
}

.hero-content h1 {

  font-size: 50px;

  line-height: 0.92;

  font-weight: 800;

  color: white;

  margin-bottom: 12px;

  letter-spacing: -3px;
}

.hero-content p {

  font-size: 15px;

  line-height: 1.6;

  color: rgba(255,255,255,0.92);
}

/* HISTORY */

.history-section {

  padding:
    28px 24px
    30px;
}

.history-card {

  max-width: 1120px;

  margin: 0 auto;

  display: grid;

  grid-template-columns: 0.7fr 1.3fr;

  overflow: hidden;

  border-radius: 28px;

  background: white;

  border:
    1px solid rgba(146,168,148,0.10);

  box-shadow:
    0 14px 35px rgba(58,71,60,0.05);
}

/* IMAGE SIDE */

.history-image-side {

  overflow: hidden;
}

.history-main-image {

  width: 100%;

  height: 420px;

  object-fit: cover;

  object-position: center;

  transition: 0.8s ease;
}

.history-card:hover .history-main-image {

  transform: scale(1.03);
}

/* CONTENT SIDE */

.history-content-side {

  padding: 28px;

  display: flex;

  flex-direction: column;

  justify-content: center;
}

.history-label {

  display: inline-flex;

  padding: 9px 18px;

  border-radius: 999px;

  background:
    rgba(146,168,148,0.14);

  color: #3A473C;

  font-size: 11px;

  font-weight: 700;

  letter-spacing: 1px;

  margin-bottom: 16px;

  width: fit-content;
}

.history-content-side h2 {

  font-size: 46px;

  line-height: 0.94;

  font-weight: 800;

  color: #3A473C;

  letter-spacing: -3px;

  margin-bottom: 16px;
}

.history-text {

  display: flex;

  flex-direction: column;

  gap: 8px;
}

.history-text p {

  font-size: 15px;

  line-height: 1.7;

  color: #687168;

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

/* MISIÓN Y VISIÓN */

.purpose-section {

  padding:
    20px 24px
    80px;

  background: transparent;
}

.purpose-container {

  max-width: 1000px;

  margin: 0 auto;
}

.purpose-header {

  text-align: center;

  margin-bottom: 40px;
}

.purpose-header span {

  color: #3A473C;

  font-size: 12px;

  font-weight: 700;

  letter-spacing: 1px;
}

.purpose-header h2 {

  font-size: 46px;

  font-weight: 800;

  color: #3A473C;

  margin-top: 14px;

  letter-spacing: -2px;

  line-height: 1;
}

.purpose-grid {

  display: grid;

  grid-template-columns:
    repeat(2, 1fr);

  gap: 24px;
}

.purpose-box {

  background: rgba(255,255,255,0.88);

  backdrop-filter: blur(12px);

  -webkit-backdrop-filter: blur(12px);

  border: 1px solid rgba(255,255,255,0.30);

  border-radius: 28px;

  padding: 28px;

  box-shadow:
    0 12px 30px rgba(58,71,60,0.10);
}

.purpose-box:hover {

  transform:
    translateY(-6px);

  box-shadow:
    0 18px 42px rgba(58,71,60,0.10);
}

.purpose-box h3 {

  font-size: 28px;

  font-weight: 800;

  color: #3A473C;

  margin-bottom: 16px;
}

.purpose-box p {

  font-size: 15px;

  line-height: 1.8;

  color: #687168;

  margin: 0;
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

  .history-card {

    grid-template-columns: 1fr;
  }

  .history-main-image {

    height: 420px;
  }

  .team-grid {

    grid-template-columns: 1fr 1fr;
  }

}

@media (max-width: 700px) {

    .purpose-grid {

    grid-template-columns: 1fr;
  }

  .purpose-header h2 {

    font-size: 36px;
  }

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

  .history-content-side {

    padding: 28px;
  }

  .history-content-side h2 {

    font-size: 36px;
  }

  .team-grid {

    grid-template-columns: 1fr;
  }

  .gallery-item {

    width: 250px;

    height: 180px;
  }

}
</style>