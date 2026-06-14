<script setup>
import NavBar from '../components/NavBar.vue'
import FooterBar from '../components/FooterBar.vue'

const stories = [

  {
    image:'/img-rescates/Rosita.jpg',
    title:'Rosita descubrió el amor',
    text:'Rosita pasó gran parte de su vida siendo utilizada únicamente para tener crías. Vivía en condiciones de abandono, con desnutrición y sin conocer el cuidado ni la seguridad de un hogar. Permanecía amarrada porque la consideraban peligrosa, pero al ser rescatada demostró todo lo contrario: era noble, tranquila y solo necesitaba amor.'
  },

  {
    image:'/img-rescates/estrella.jpg',
    title:'Estrellita volvió a confiar',
    text:'Estrellita fue rescatada en Quepos después de sufrir abuso y abandono. A pesar del miedo y las secuelas, poco a poco volvió a sentirse segura. Actualmente continúa rodeada de cuidado y amor dentro de la fundación.'
  },

  {
    image:'/img-rescates/Kira.jpg',
    title:'La historia de Kira',
    text:'Kira fue rescatada en Puriscal después de sufrir abuso y llegar en condiciones críticas. Necesitó una compleja reconstrucción y meses de recuperación para volver a sanar. Hoy disfruta de una nueva vida junto a su familia adoptiva.'
  }

]
</script>

<template>

  <div class="rescates-page">

    <NavBar />

    <!-- HERO -->

    <section class="hero">

      <img
        src="/img-rescates/herorescates.PNG"
        class="hero-image"
      >

      <div class="hero-overlay"></div>

      <div class="hero-content">

        <h1>
          Cada rescate
          merece una
          <span>segunda oportunidad</span>
        </h1>

        <p>
          Historias de empatía, recuperación
          y nuevas oportunidades para seguir viviendo.
        </p>

      </div>

    </section>

    <!-- STORIES -->

    <section class="stories-section">

      <div class="container">

        <div class="stories-header">

    

          <h2>
            Historias que
            dejaron huella
          </h2>

          <p>
            Cada rescate representa esperanza,
            cuidado y un nuevo comienzo.
          </p>

        </div>

        <div class="stories-grid">

          <div
            class="story-card"
            v-for="story in stories"
            :key="story.title"
          >

            <div class="story-image-wrap">

              <img
                :src="story.image"
                class="story-image"
              >

            </div>

            <div class="story-content">

            

              <h3>
                {{ story.title }}
              </h3>

              <p>
                {{ story.text }}
              </p>

            </div>

          </div>

        </div>

      

      </div>

    </section>

    <FooterBar />

  </div>

</template>

<style scoped>

/* PALETA */

.rescates-page {

  background: #FFFFFF;
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

  object-position: center 30%;

  transform: scale(1.02);

  filter:
    brightness(0.72)
    contrast(1.03);
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

.hero-tag {

  display: inline-flex;

  padding: 10px 18px;

  border-radius: 999px;

  background: rgba(255,255,255,0.12);

  backdrop-filter: blur(10px);

  border: 1px solid rgba(255,255,255,0.16);

  color: white;

  font-size: 11px;

  font-weight: 700;

  letter-spacing: 1.5px;

  margin-bottom: 24px;
}

.hero-content h1 {

  font-size: 62px;

  line-height: 0.95;

  letter-spacing: -3px;

  font-weight: 800;

  color: white;

  margin-bottom: 24px;
}

.hero-content h1 span {

  color: #C9A06A;
}

.hero-content p {

  font-size: 16px;

  line-height: 1.7;

  color: rgba(255,255,255,0.92);

  max-width: 420px;

  margin: 0;
}

/* STORIES */

.stories-section {

  background: #FAF8F5;

  padding:
    100px 24px
    120px;
}
.container {

  max-width: 1240px;

  margin: auto;
}

.stories-header {

  text-align: center;

  margin-bottom: 72px;
}

.stories-header span {

  color: #C9A06A;

  font-size: 12px;

  font-weight: 700;

  letter-spacing: 1.5px;
}

.stories-header h2 {

  font-size: 58px;

  line-height: 0.96;

  letter-spacing: -3px;

  color: #3A473C;

  font-weight: 800;

  margin:
    18px 0;
}

.stories-header p {

  color: #6C756D;

  font-size: 16px;

  line-height: 1.9;

  max-width: 620px;

  margin: auto;
}

.stories-grid {

  display: grid;

  grid-template-columns:
    repeat(3,1fr);

  gap: 30px;
}

.story-card {

  background: #FFFFFF;

  border-radius: 30px;

  overflow: hidden;

  border: 2px solid #F4F6F4;

  transition: 0.4s ease;

  box-shadow:
    0 10px 35px rgba(58,71,60,0.05);
}

.story-card:hover {
  transform: translateY(-8px);
  border-color: #3A473C;
  box-shadow: 0 16px 42px rgba(58,71,60,0.08);
}

.story-image-wrap {

  height: 290px;

  overflow: hidden;
}

.story-image {

  width: 100%;

  height: 100%;

  object-fit: cover;

  transition: 0.8s ease;
}

.story-card:hover .story-image {

  transform: scale(1.05);
}

.story-content {

  padding: 32px;
}

.story-label {

  display: inline-flex;

  padding: 8px 15px;

  border-radius: 999px;

  background: #F5E6D3;

  color: #A07A52;

  font-size: 12px;

  font-weight: 700;

  margin-bottom: 18px;
}

.story-content h3 {

  font-size: 30px;

  line-height: 1.05;

  color: #3A473C;

  font-weight: 800;

  margin-bottom: 18px;
}

.story-content p {

  color: #687168;

  font-size: 15px;

  line-height: 1.9;
}

/* IMPACT */

.impact-grid {

  margin-top: 95px;

  display: grid;

  grid-template-columns:
    repeat(3,1fr);

  gap: 24px;
}

.impact-item {

  background: #F4F6F4;

  border-radius: 28px;

  padding: 44px;

  text-align: center;

  border: 1px solid #EEF3F0;

  box-shadow:
    0 10px 35px rgba(58,71,60,0.04);
}

.impact-item h3 {

  font-size: 54px;

  color: #3A473C;

  font-weight: 800;

  margin-bottom: 12px;
}

.impact-item p {

  color: #6C756D;

  font-size: 15px;

  line-height: 1.7;
}

/* RESPONSIVE */

@media (max-width: 1000px) {

  .stories-grid,
  .impact-grid {

    grid-template-columns: 1fr;
  }

}

@media (max-width: 700px) {

  .hero {

    height: 430px;
  }

  .hero-content {

    left: 24px;

    right: 24px;

    bottom: 40px;
  }

  .hero-content h1 {

    font-size: 44px;
  }

  .stories-header h2 {

    font-size: 40px;
  }


  .stories-section {

    padding:
      70px 20px
      90px;
  }

}

/* ── MOBILE RESPONSIVE ── */
@media (max-width: 768px) {
  .hero {
    height: 320px;
  }

  .hero-content {
    left: 16px;
    right: 16px;
    bottom: 32px;
    max-width: 100%;
  }

  .hero-content h1 {
    font-size: 36px;
    letter-spacing: -1.5px;
    margin-bottom: 14px;
  }

  .hero-content p {
    font-size: 14px;
    max-width: 100%;
  }

  .stories-section {
    padding: 60px 16px 80px;
  }

  .stories-header {
    margin-bottom: 40px;
  }

  .stories-header h2 {
    font-size: 36px;
    letter-spacing: -2px;
    margin: 10px 0;
  }

  .stories-header p {
    font-size: 14px;
  }

  .stories-grid {
    grid-template-columns: 1fr;
    gap: 20px;
  }

  .story-card {
    border-radius: 22px;
  }

  .story-image-wrap {
    height: 220px;
  }

  .story-content {
    padding: 22px 20px;
  }

  .story-content h3 {
    font-size: 22px;
    margin-bottom: 12px;
  }

  .story-content p {
    font-size: 14px;
  }
}

@media (max-width: 480px) {
  .hero {
    height: 280px;
  }

  .hero-content h1 {
    font-size: 28px;
  }

  .stories-header h2 {
    font-size: 28px;
  }

  .story-image-wrap {
    height: 180px;
  }
}

</style>