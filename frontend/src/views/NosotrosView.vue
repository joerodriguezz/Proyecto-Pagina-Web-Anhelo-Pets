<script setup>
import NavBar from '../components/NavBar.vue'
import FooterBar from '../components/FooterBar.vue'

const faqs = [
  { q:'¿Cuánto tiempo tarda el proceso de adopción?', a:'El proceso generalmente toma entre 5 y 10 días hábiles, dependiendo de la evaluación de la solicitud y la coordinación de la entrega.' },
  { q:'¿Hay algún costo por adoptar?', a:'No cobramos por la adopción. Sin embargo, las mascotas vienen ya vacunadas y desparasitadas, lo que representa un ahorro para ti.' },
  { q:'¿Puedo adoptar si vivo en apartamento?', a:'Sí, dependiendo del tamaño y temperamento de la mascota. Evaluamos cada caso de forma individual.' },
  { q:'¿Hacen visita domiciliaria?', a:'En algunos casos podemos coordinar una visita virtual o presencial para verificar que el ambiente sea adecuado para la mascota.' },
  { q:'¿Puedo devolver una mascota si no funciona?', a:'Lo ideal es que el vínculo sea para siempre, pero entendemos que pueden surgir situaciones. En ese caso, la mascota regresa a la Fundación para ser reubicada.' },
]
</script>

<template>
  <NavBar />

  <div class="page-hero">
    <h1>💚 Nosotros</h1>
    <p>Conoce la historia y misión de la Fundación Anhelo Pets</p>
  </div>

  <!-- Mission / Vision -->
  <section class="about-section container">
    <div class="about-grid">
      <div class="about-text">
        <h2>¿Quiénes somos?</h2>
        <p>La Fundación Anhelo Pets nació en 2023 con la misión de rescatar, cuidar y encontrar hogares responsables para perros y gatos en Costa Rica. Somos una organización sin fines de lucro conformada por voluntarios apasionados por el bienestar animal.</p>
        <p>Desde nuestra fundación hemos rescatado más de 125 animales, encontrado hogar a más de 115 mascotas y contamos con una red de 5 casas cuna activas en el Gran Área Metropolitana.</p>
      </div>
      <div class="about-image">
        <div class="about-img-box">
          <span>🐾</span>
          <p>Fundación<br><strong>Anhelo Pets</strong></p>
          <small>Costa Rica · Desde 2023</small>
        </div>
      </div>
    </div>

    <div class="mission-grid">
      <div class="mission-card">
        <span class="m-icon">🎯</span>
        <h3>Misión</h3>
        <p>Rescatar y proteger animales en situación de calle o abandono, brindándoles atención médica, alimentación y afecto hasta encontrar un hogar permanente y responsable.</p>
      </div>
      <div class="mission-card">
        <span class="m-icon">🌟</span>
        <h3>Visión</h3>
        <p>Ser la fundación líder en Costa Rica en rescate y adopción responsable de animales, contribuyendo a una cultura de respeto y amor hacia los seres vivos.</p>
      </div>
      <div class="mission-card">
        <span class="m-icon">💎</span>
        <h3>Valores</h3>
        <ul class="values-list">
          <li>🐾 Amor y respeto por los animales</li>
          <li>🤝 Compromiso con las familias adoptantes</li>
          <li>🌿 Responsabilidad y transparencia</li>
          <li>❤️ Comunidad y voluntariado</li>
        </ul>
      </div>
    </div>
  </section>

  <!-- Adoption Requirements -->
  <section id="requisitos" class="req-section">
    <div class="container">
      <h2 class="section-title">📋 Requisitos de Adopción</h2>
      <p class="section-subtitle">Para garantizar el bienestar de nuestras mascotas</p>

      <div class="req-grid">
        <div class="req-card" v-for="r in requirements" :key="r.title">
          <span class="req-icon">{{ r.icon }}</span>
          <h4>{{ r.title }}</h4>
          <p>{{ r.desc }}</p>
        </div>
      </div>
    </div>
  </section>

  <!-- Contact -->
  <section id="contacto" class="contact-section container">
    <h2 class="section-title">📬 Contáctanos</h2>
    <p class="section-subtitle">Estamos disponibles para responder tus dudas</p>

    <div class="contact-grid">
      <div class="contact-info">
        <div class="contact-item"><span>📍</span><div><strong>Dirección</strong><p>San José, Costa Rica</p></div></div>
        <div class="contact-item"><span>📞</span><div><strong>Teléfono</strong><p>+506 8840-3334</p></div></div>
        <div class="contact-item"><span>✉️</span><div><strong>Correo</strong><p>info@anhelopets.cr</p></div></div>
        <div class="contact-item"><span>🕐</span><div><strong>Horario</strong><p>Lunes a Viernes: 8am – 6pm<br>Sábados: 9am – 2pm</p></div></div>
      </div>
      <div class="contact-form card">
        <h3>Envíanos un mensaje</h3>
        <div class="form-group">
          <label class="form-label">Nombre</label>
          <input class="form-input" placeholder="Tu nombre completo" />
        </div>
        <div class="form-group">
          <label class="form-label">Correo electrónico</label>
          <input class="form-input" type="email" placeholder="correo@ejemplo.com" />
        </div>
        <div class="form-group">
          <label class="form-label">Mensaje</label>
          <textarea class="form-textarea" placeholder="¿En qué podemos ayudarte?"></textarea>
        </div>
        <button class="btn-primary" style="width:100%;justify-content:center;">Enviar mensaje ✉️</button>
      </div>
    </div>
  </section>

  <!-- FAQ -->
  <section id="faq" class="faq-section">
    <div class="container">
      <h2 class="section-title">❓ Preguntas Frecuentes</h2>
      <p class="section-subtitle">Resolvemos tus dudas sobre adopciones</p>

      <div class="faq-list">
        <details v-for="f in faqs" :key="f.q" class="faq-item">
          <summary>{{ f.q }}</summary>
          <p>{{ f.a }}</p>
        </details>
      </div>
    </div>
  </section>

  <FooterBar />
</template>

<script>
export default {
  data() {
    return {
      requirements: [
        { icon:'🪪', title:'Mayor de edad',          desc:'Debes ser mayor de 18 años y presentar identificación oficial vigente.' },
        { icon:'🏠', title:'Espacio adecuado',        desc:'Tu hogar debe contar con espacio suficiente para la mascota y un ambiente seguro.' },
        { icon:'💰', title:'Capacidad económica',     desc:'Demostrar que puedes cubrir gastos básicos: alimentación, veterinaria y cuidados.' },
        { icon:'⏰', title:'Tiempo y dedicación',     desc:'Comprometerse a dedicar tiempo diario al cuidado, paseos y compañía de la mascota.' },
        { icon:'👨‍👩‍👧', title:'Acuerdo familiar',      desc:'Todos en el hogar deben estar de acuerdo con la adopción y comprometidos con el cuidado.' },
        { icon:'📝', title:'Formulario completo',     desc:'Llenar el formulario de adopción con datos personales verídicos y completos.' },
      ]
    }
  }
}
</script>

<style scoped>
.about-section { padding: 60px 24px; }
.about-grid { display: grid; grid-template-columns: 1fr auto; gap: 50px; align-items: center; margin-bottom: 50px; }
.about-text h2 { font-size: 28px; font-weight: 800; color: var(--green-dark); margin-bottom: 16px; }
.about-text p  { font-size: 15px; color: var(--text-mid); line-height: 1.7; margin-bottom: 14px; }
.about-img-box {
  width: 260px; height: 260px;
  background: linear-gradient(135deg, var(--green) 0%, var(--green-dark) 100%);
  border-radius: var(--radius-lg);
  display: flex; flex-direction: column;
  align-items: center; justify-content: center;
  color: var(--white); text-align: center; gap: 8px;
  font-size: 52px;
}
.about-img-box p { font-size: 16px; margin: 0; }
.about-img-box strong { color: var(--yellow); }
.about-img-box small { font-size: 12px; opacity: 0.7; }

.mission-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 24px; }
.mission-card {
  background: var(--white);
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  padding: 28px;
  box-shadow: var(--shadow-sm);
}
.m-icon { font-size: 36px; display: block; margin-bottom: 12px; }
.mission-card h3 { font-size: 20px; font-weight: 700; color: var(--green-dark); margin-bottom: 10px; }
.mission-card p  { font-size: 14px; color: var(--text-mid); line-height: 1.6; }
.values-list { font-size: 14px; color: var(--text-mid); display: flex; flex-direction: column; gap: 8px; padding-left: 0; }
.values-list li { list-style: none; }

.req-section { background: var(--cream); padding: 60px 24px; }
.req-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(240px, 1fr)); gap: 20px; }
.req-card {
  background: var(--white);
  border-radius: var(--radius-md);
  padding: 24px;
  box-shadow: var(--shadow-sm);
  text-align: center;
}
.req-icon { font-size: 36px; display: block; margin-bottom: 12px; }
.req-card h4 { font-size: 16px; font-weight: 700; color: var(--text-dark); margin-bottom: 8px; }
.req-card p  { font-size: 13px; color: var(--text-mid); line-height: 1.5; }

.contact-section { padding: 60px 24px; }
.contact-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 40px; align-items: start; }
.contact-info { display: flex; flex-direction: column; gap: 20px; }
.contact-item { display: flex; gap: 14px; align-items: flex-start; font-size: 24px; }
.contact-item div strong { display: block; font-size: 15px; font-weight: 700; color: var(--text-dark); }
.contact-item div p { font-size: 14px; color: var(--text-mid); margin-top: 2px; }
.contact-form h3 { font-size: 20px; font-weight: 700; margin-bottom: 20px; color: var(--text-dark); }

.faq-section { background: var(--cream); padding: 60px 24px; }
.faq-list { max-width: 720px; margin: 0 auto; display: flex; flex-direction: column; gap: 12px; }
.faq-item {
  background: var(--white);
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  overflow: hidden;
}
.faq-item summary {
  padding: 18px 22px;
  font-size: 15px;
  font-weight: 600;
  color: var(--text-dark);
  cursor: pointer;
  list-style: none;
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.faq-item summary::-webkit-details-marker { display: none; }
.faq-item summary::after { content: '▾'; color: var(--green); font-size: 18px; }
.faq-item[open] summary::after { content: '▴'; }
.faq-item p { padding: 0 22px 18px; font-size: 14px; color: var(--text-mid); line-height: 1.6; }

@media (max-width: 768px) {
  .about-grid { grid-template-columns: 1fr; }
  .about-image { display: none; }
  .mission-grid { grid-template-columns: 1fr; }
  .contact-grid { grid-template-columns: 1fr; }
}
</style>
