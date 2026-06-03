<template>

  <footer class="footer">

    <div class="footer-inner container">

      <!-- BRAND -->

      <div class="footer-brand">

        <div class="footer-logo-wrap">

          <img
            src="/img-vol/logo.png"
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

  background:
    linear-gradient(
      180deg,
      #92A894 0%,
      #819681 45%,
      #738873 100%
    );

  overflow: hidden;

  color: white;
}

/* DECORACIÓN */

.footer::before {

  content: '';

  position: absolute;

  width: 420px;

  height: 420px;

  border-radius: 50%;

  background:
    rgba(255,255,255,0.04);

  top: -180px;

  right: -100px;
}

.footer::after {

  content: '';

  position: absolute;

  width: 260px;

  height: 260px;

  border-radius: 50%;

  background:
    rgba(255,255,255,0.03);

  bottom: -120px;

  left: -80px;
}

/* CONTENT */

.footer-inner {

  position: relative;

  z-index: 2;

  max-width: 1240px;

  margin: auto;

  display: grid;

  grid-template-columns:
    1.7fr 1fr 1fr 1fr;

  gap: 55px;

  padding:
    70px 24px
    42px;
}

/* BRAND */

.footer-brand {

  display: flex;

  flex-direction: column;

  padding-left: 18px;
}

.footer-logo-wrap {

  margin-bottom: 20px;

  display: flex;

  justify-content: flex-start;
}

.footer-logo-image {

  width: 230px;

  object-fit: contain;

  opacity: 0.98;

  transform: translateX(8px);
}

.footer-brand p {

  max-width: 320px;

  font-size: 15px;

  line-height: 1.9;

  color:
    rgba(255,255,255,0.84);

  margin-left: 10px;
}

/* SOCIAL */

.social-links {

  display: flex;

  gap: 14px;

  margin-top: 30px;

  margin-left: 10px;
}

.social-links a {

  width: 50px;

  height: 50px;

  border-radius: 50%;

  background:
    rgba(255,255,255,0.12);

  display: flex;

  align-items: center;

  justify-content: center;

  transition: 0.35s ease;

  backdrop-filter: blur(10px);
}

.social-links a:hover {

  background: #F9C17A;

  transform:
    translateY(-4px);
}

.social-icon {

  width: 21px;

  height: 21px;

  object-fit: contain;
}

/* COLUMNAS */

.footer-col {

  display: flex;

  flex-direction: column;

  padding-top: 12px;
}

.footer-col h4 {

  font-size: 12px;

  font-weight: 800;

  letter-spacing: 2px;

  text-transform: uppercase;

  color: #F9E4C4;

  margin-bottom: 22px;
}

.footer-col a,
.footer-col span {

  font-size: 15px;

  line-height: 1.7;

  color:
    rgba(255,255,255,0.82);

  text-decoration: none;

  margin-bottom: 12px;

  transition: 0.3s ease;
}

.footer-col a:hover {

  color: #F9C17A;

  transform:
    translateX(4px);
}

/* BOTTOM */

.footer-bottom {

  position: relative;

  z-index: 2;

  border-top:
    1px solid rgba(255,255,255,0.08);

  text-align: center;

  padding:
    18px 20px;

  font-size: 13px;

  color:
    rgba(255,255,255,0.62);
}

/* RESPONSIVE */

@media (max-width: 980px) {

  .footer-inner {

    grid-template-columns:
      1fr 1fr;

    gap: 44px;
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

    padding-top: 56px;
  }

  .footer-logo-wrap {

    justify-content: center;
  }

  .footer-logo-image {

    width: 200px;

    transform: none;
  }

  .footer-brand p {

    margin-left: auto;

    margin-right: auto;
  }

  .social-links {

    justify-content: center;

    margin-left: 0;
  }

  .footer-col a,
  .footer-col span {

    justify-content: center;
  }

}

</style>