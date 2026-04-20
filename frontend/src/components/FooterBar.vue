<template>
  <footer class="footer">
    <div class="footer-inner container">
      <div class="footer-brand">
        <span class="footer-logo">🐾 Anhelo<strong>Pets</strong></span>
        <p>Fundación dedicada al rescate,<br>cuidado y adopción responsable de animales.</p>
        <div class="social-links">
          <a href="#" aria-label="Facebook">📘</a>
          <a href="#" aria-label="Instagram">📷</a>
          <a href="#" aria-label="WhatsApp">💬</a>
        </div>
      </div>

      <div class="footer-col">
        <h4>Navegación</h4>
        <RouterLink to="/">Inicio</RouterLink>
        <RouterLink to="/mascotas">Mascotas</RouterLink>
        <RouterLink to="/rescates">Rescates</RouterLink>
        <RouterLink to="/voluntarios">Voluntarios</RouterLink>
        <RouterLink to="/nosotros">Nosotros</RouterLink>
      </div>

      <div class="footer-col">
        <h4>Adopción</h4>
        <RouterLink to="/mascotas">Ver catálogo</RouterLink>
        <RouterLink to="/nosotros#requisitos">Requisitos</RouterLink>
        <RouterLink to="/nosotros#faq">Preguntas frecuentes</RouterLink>
        <RouterLink to="/registro">Crear cuenta</RouterLink>
      </div>

      <div class="footer-col">
        <h4>Contacto</h4>
        <span>📍 San José, Costa Rica</span>
        <span>📞 +506 8840-3334</span>
        <span>✉️ info@anhelopets.cr</span>
      </div>
    </div>

    <div class="footer-bottom">
      <p>© 2026 Fundación Anhelo Pets · Todos los derechos reservados</p>
    </div>
  </footer>
</template>

<style scoped>
.footer {
  background: var(--green-dark);
  color: rgba(255,255,255,0.85);
  margin-top: auto;
}

.footer-inner {
  display: grid;
  grid-template-columns: 2fr 1fr 1fr 1fr;
  gap: 40px;
  padding-top: 50px;
  padding-bottom: 40px;
}

.footer-brand p {
  font-size: 14px;
  line-height: 1.6;
  margin-top: 10px;
  opacity: 0.8;
}

.footer-logo {
  font-size: 20px;
  font-weight: 700;
  color: var(--white);
}
.footer-logo strong { color: var(--yellow); }

.social-links {
  display: flex;
  gap: 12px;
  margin-top: 16px;
  font-size: 20px;
}
.social-links a {
  opacity: 0.8;
  transition: opacity 0.2s, transform 0.2s;
}
.social-links a:hover { opacity: 1; transform: scale(1.15); }

.footer-col h4 {
  font-size: 13px;
  font-weight: 700;
  letter-spacing: 1px;
  text-transform: uppercase;
  color: var(--yellow);
  margin-bottom: 14px;
}
.footer-col a,
.footer-col span {
  display: block;
  font-size: 14px;
  margin-bottom: 8px;
  opacity: 0.8;
  transition: opacity 0.2s;
  cursor: pointer;
}
.footer-col a:hover { opacity: 1; text-decoration: underline; }

.footer-bottom {
  border-top: 1px solid rgba(255,255,255,0.12);
  text-align: center;
  padding: 18px 24px;
  font-size: 13px;
  opacity: 0.6;
}

@media (max-width: 768px) {
  .footer-inner {
    grid-template-columns: 1fr 1fr;
    gap: 30px;
  }
  .footer-brand { grid-column: 1 / -1; }
}
@media (max-width: 480px) {
  .footer-inner { grid-template-columns: 1fr; }
}
</style>
