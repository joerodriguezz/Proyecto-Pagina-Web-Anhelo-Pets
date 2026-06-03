<script setup>
import { ref } from 'vue'
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

// Formulario de registro de rescates
// El formulario de registro de rescates se eliminó de esta vista
</script>

<template>

  <div class="rescates-page">

    <NavBar />

    <!-- HERO -->

    <section class="hero">

      <img
        src="/img-rescates/IMG_1494.JPG"
        class="hero-image"
      >

      <div class="hero-overlay"></div>

      <div class="hero-content">

        <span class="hero-tag">
          HISTORIAS REALES
        </span>

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

          <span>
            RESCATES
          </span>

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

              <span class="story-label">
                Rescate
              </span>

              <h3>
                {{ story.title }}
              </h3>

              <p>
                {{ story.text }}
              </p>

            </div>

          </div>

        </div>

        <!-- IMPACT -->

        <div class="impact-grid">

          <div class="impact-item">

            <h3>
              +350
            </h3>

            <p>
              Mascotas rescatadas
            </p>

          </div>

          <div class="impact-item">

            <h3>
              +120
            </h3>

            <p>
              Adopciones exitosas
            </p>

          </div>

          <div class="impact-item">

            <h3>
              +80
            </h3>

            <p>
              Voluntarios activos
            </p>

          </div>

        </div>


      </div>

    </section>

    <!-- Se removió el formulario de registro de rescates de esta vista -->

    <FooterBar />

  </div>

</template>

<style scoped>

/* PALETA */

.rescates-page {

  background: #FAFAFA;
}

/* HERO */

.hero {

  position: relative;

  height: 560px;

  overflow: hidden;
}

.hero-image {

  width: 100%;

  height: 100%;

  object-fit: cover;

  object-position: center 55%;

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
      rgba(58,71,60,0.82) 0%,
      rgba(58,71,60,0.45) 45%,
      rgba(58,71,60,0.10) 100%
    );
}

.hero-content {

  position: absolute;

  left: 8%;

  bottom: 55px;

  max-width: 540px;

  z-index: 2;
}

.hero-tag {

  display: inline-flex;

  padding: 10px 18px;

  border-radius: 999px;

  background: rgba(255,255,255,0.12);

  backdrop-filter: blur(10px);

  border:
    1px solid rgba(255,255,255,0.16);

  color: white;

  font-size: 11px;

  font-weight: 700;

  letter-spacing: 1.5px;

  margin-bottom: 28px;
}

.hero-content h1 {

  font-size: 60px;

  line-height: 0.95;

  letter-spacing: -3px;

  color: white;

  font-weight: 800;

  margin-bottom: 24px;
}

.hero-content h1 span {

  color: #F9C17A;
}

.hero-content p {

  font-size: 17px;

  line-height: 1.9;

  color: rgba(255,255,255,0.88);

  max-width: 430px;
}

/* STORIES */

.stories-section {

  padding:
    130px 24px
    170px;
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

  color: #92A894;

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

  background: white;

  border-radius: 30px;

  overflow: hidden;

  border:
    1px solid rgba(146,168,148,0.10);

  transition: 0.4s ease;

  box-shadow:
    0 10px 35px rgba(58,71,60,0.05);
}

.story-card:hover {

  transform:
    translateY(-8px);

  box-shadow:
    0 16px 42px rgba(58,71,60,0.10);
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

  background: #FFF1DD;

  color: #D89A47;

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

  background: white;

  border-radius: 28px;

  padding: 44px;

  text-align: center;

  border:
    1px solid rgba(146,168,148,0.10);

  box-shadow:
    0 10px 35px rgba(58,71,60,0.04);
}

.impact-item h3 {

  font-size: 54px;

  color: #92A894;

  font-weight: 800;

  margin-bottom: 12px;
}

.impact-item p {

  color: #6C756D;

  font-size: 15px;

  line-height: 1.7;
}

/* Estilos del formulario de registro eliminados porque no se muestran en esta vista */

/* RESPONSIVE */

@media (max-width: 1000px) {

  .stories-grid,
  .impact-grid {

    grid-template-columns: 1fr;
  }

}

@media (max-width: 700px) {

  .hero {

    height: 500px;
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
      90px 20px
      120px;
  }

}

</style>