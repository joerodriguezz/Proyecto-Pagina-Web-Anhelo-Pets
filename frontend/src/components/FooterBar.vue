<template>

  <footer class="footer">

    <div class="footer-inner container">

      <!-- BRAND -->

      <div class="footer-brand">

        <div class="footer-logo-wrap">

          <img
            src="/img-home/logo.png"
            class="footer-logo-image"
          >

        </div>

        <p>
          Fundación dedicada al rescate,
          cuidado y adopción responsable
          de animales.
        </p>

        <!-- REDES -->

        <div class="social-links">

          <a
            href="https://www.facebook.com/share/1CQVfzw137/?mibextid=wwXIfr"
            target="_blank"
            aria-label="Facebook"
          >

            <img
              src="https://cdn.jsdelivr.net/gh/devicons/devicon/icons/facebook/facebook-original.svg"
              class="social-icon"
            >

          </a>

          <a
            href="https://www.instagram.com/anhelopets_salvandovidas?igsh=ZjRlcnluNW82NDIz"
            target="_blank"
            aria-label="Instagram"
          >

            <img
              src="https://upload.wikimedia.org/wikipedia/commons/a/a5/Instagram_icon.png"
              class="social-icon"
            >

          </a>

          <a
            href="mailto:fundacionanhelopets@gmail.com"
            aria-label="Correo"
          >

            <img
              src="https://cdn-icons-png.flaticon.com/512/732/732200.png"
              class="social-icon"
            >

          </a>

        </div>

      </div>

      <!-- NAVEGACIÓN -->

      <div class="footer-col">

        <h4>
          Navegación
        </h4>

        <RouterLink to="/">
          Inicio
        </RouterLink>

        <RouterLink to="/mascotas">
          Mascotas
        </RouterLink>

        <RouterLink to="/rescates">
          Rescates
        </RouterLink>

        <RouterLink to="/voluntarios">
          Voluntarios
        </RouterLink>

        <RouterLink to="/nosotros">
          Nosotros
        </RouterLink>

      </div>

      <!-- ADOPCIÓN -->

      <div class="footer-col">

        <h4>
          Adopción
        </h4>

        <RouterLink to="/mascotas">
          Ver catálogo
        </RouterLink>

        <RouterLink to="/nosotros#requisitos">
          Requisitos
        </RouterLink>

        <RouterLink to="/nosotros#faq">
          Preguntas frecuentes
        </RouterLink>

        <RouterLink to="/registro">
          Crear cuenta
        </RouterLink>

      </div>

      <!-- CONTACTO -->

      <div class="footer-col">

        <h4>
          Contacto
        </h4>

        <span class="contact-item">
          Quepos, Puntarenas, Costa Rica
        </span>

        <span class="contact-item">
          +506 8840-3334
        </span>

        <span class="contact-item">
          fundacionanhelopets@gmail.com
        </span>

      </div>

    </div>

    <!-- FOOTER BOTTOM -->

    <div class="footer-bottom">

      <p>
        © 2026 Fundación Anhelo Pets ·
        Todos los derechos reservados
      </p>

    </div>

  </footer>

</template>

<style scoped>

.footer {
  position: relative;
  background: linear-gradient(
     180deg,

    #3A473C 0%,

    #344036 50%,

    #2D372F 100%
  );
  overflow: hidden;
  color: white;
}

.footer::before {
  content: '';
  position: absolute;
  width: 300px;
  height: 300px;
  border-radius: 50%;
  background: rgba(255,255,255,0.04);
  top: -120px;
  right: -80px;
}

.footer::after {
  content: '';
  position: absolute;
  width: 180px;
  height: 180px;
  border-radius: 50%;
  background: rgba(255,255,255,0.03);
  bottom: -90px;
  left: -50px;
}

.footer-inner {
  position: relative;
  z-index: 2;
  max-width: 1240px;
  margin: auto;
  display: grid;
  grid-template-columns: 1.7fr 1fr 1fr 1fr;
  gap: 40px;
  padding: 45px 24px 28px;
}

.footer-brand {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  margin-top: -15px;
}

.footer-logo-wrap {
  display: flex;
  justify-content: center;
  margin-bottom: 0;
}

.footer-logo-image {
  width: 180px;
  object-fit: contain;
  opacity: .98;
}

.footer-brand p {

  margin-top: 0;

  max-width: 300px;

  font-size: 14px;

  line-height: 1.6;

  color: rgba(255,255,255,.80);

}

.social-links {
  display: flex;
  gap: 12px;
  margin-top: 20px;
}

.social-links a {
  width: 42px;
  height: 42px;
  border-radius: 50%;
  background: rgba(255,255,255,.10);
  display: flex;
  align-items: center;
  justify-content: center;
  transition: .3s ease;
  backdrop-filter: blur(10px);
}

.social-links a:hover {
  background: #F5E6D3;
  transform: translateY(-3px);
}

.social-icon {
  width: 18px;
  height: 18px;
  object-fit: contain;
  transition: 0.3s ease;
}

.footer-col {
  display: flex;
  flex-direction: column;
  padding-top: 6px;
}

.footer-col h4 {
  font-size: 11px;
  font-weight: 800;
  letter-spacing: 2px;
  text-transform: uppercase;
  color: #E8D8C3;
  margin-bottom: 16px;
}

.footer-col a,
.footer-col span {
  font-size: 14px;
  line-height: 1.6;
  color: rgba(255,255,255,.80);
  text-decoration: none;
  margin-bottom: 10px;
  transition: .25s ease;
}

.footer-col a:hover {
  color: #F5E6D3;
  transform: translateX(3px);
}

.footer-bottom {
  position: relative;
  z-index: 2;
  border-top: 1px solid rgba(255,255,255,.12);
  text-align: center;
  padding: 12px 20px;
  font-size: 12px;
  color: rgba(255,255,255,.60);
}

@media (max-width: 980px) {

  .footer-inner {
    grid-template-columns: 1fr 1fr;
    gap: 35px;
  }

  .footer-brand {
    grid-column: 1 / -1;
    padding-left: 0;
  }

}

@media (max-width: 640px) {

  .footer-inner {
    grid-template-columns: 1fr;
    text-align: center;
    padding-top: 40px;
  }

  .footer-logo-wrap {
    justify-content: center;
  }

  .footer-logo-image {
    width: 160px;
  }

  .footer-brand p {
    margin-left: auto;
    margin-right: auto;
  }

  .social-links {
    justify-content: center;
  }

}

</style>