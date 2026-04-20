<script setup>
import { RouterLink } from 'vue-router'
</script>

<template>
  <div class="auth-page">
    <div class="auth-left">
      <div class="auth-brand">
        <RouterLink to="/" class="logo">🐾 Anhelo<strong>Pets</strong></RouterLink>
      </div>
      <div class="auth-hero-text">
        <h1>Únete a nuestra comunidad</h1>
        <p>Crea tu cuenta gratuita y da el primer paso para adoptar una mascota o apoyar nuestra fundación.</p>
        <ul class="perks">
          <li>✅ Envía solicitudes de adopción</li>
          <li>✅ Sigue el estado de tu proceso</li>
          <li>✅ Accede a información exclusiva</li>
          <li>✅ Conecta con nuestra comunidad</li>
        </ul>
      </div>
      <p class="auth-quote">"Adoptar es un acto de amor que cambia dos vidas: la tuya y la del animal."</p>
    </div>

    <div class="auth-right">
      <div class="auth-card">
        <h2>Crear cuenta</h2>
        <p class="auth-sub">Completa tus datos personales</p>

        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Nombre completo *</label>
            <input class="form-input" placeholder="María González" />
          </div>
          <div class="form-group">
            <label class="form-label">Cédula *</label>
            <input class="form-input" placeholder="1-2345-6789" />
          </div>
        </div>

        <div class="form-group">
          <label class="form-label">Correo electrónico *</label>
          <input class="form-input" type="email" placeholder="correo@ejemplo.com" />
        </div>

        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Edad *</label>
            <input class="form-input" type="number" placeholder="25" min="18" />
          </div>
          <div class="form-group">
            <label class="form-label">Nacionalidad *</label>
            <select class="form-select">
              <option>Costa Rica</option>
              <option>Nicaragua</option>
              <option>Colombia</option>
              <option>Otro</option>
            </select>
          </div>
        </div>

        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Teléfono *</label>
            <input class="form-input" placeholder="+506 8xxx-xxxx" />
          </div>
          <div class="form-group">
            <label class="form-label">Provincia *</label>
            <select class="form-select">
              <option>San José</option>
              <option>Alajuela</option>
              <option>Cartago</option>
              <option>Heredia</option>
              <option>Guanacaste</option>
              <option>Puntarenas</option>
              <option>Limón</option>
            </select>
          </div>
        </div>

        <div class="form-group">
          <label class="form-label">Dirección completa *</label>
          <input class="form-input" placeholder="Cantón, distrito, señas..." />
        </div>

        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Contraseña *</label>
            <input class="form-input" type="password" placeholder="Mínimo 8 caracteres" />
          </div>
          <div class="form-group">
            <label class="form-label">Confirmar contraseña *</label>
            <input class="form-input" type="password" placeholder="Repetir contraseña" />
          </div>
        </div>

        <div class="form-group">
          <label class="checkbox-label">
            <input type="checkbox" />
            <span>Acepto los <a href="#" class="auth-link">términos y condiciones</a> y la <a href="#" class="auth-link">política de privacidad</a></span>
          </label>
        </div>

        <button class="btn-primary auth-btn">Crear mi cuenta</button>

        <p class="auth-footer-text">
          ¿Ya tienes cuenta?
          <RouterLink to="/login" class="auth-link">Iniciar sesión</RouterLink>
        </p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.auth-page {
  min-height: 100vh;
  display: grid;
  grid-template-columns: 1fr 1.4fr;
}
.auth-left {
  background: linear-gradient(160deg, var(--green-dark) 0%, var(--green) 100%);
  padding: 50px;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  color: var(--white);
  position: sticky;
  top: 0;
  height: 100vh;
}
.logo { font-size: 22px; font-weight: 700; color: var(--white); }
.logo strong { color: var(--yellow); }

.auth-hero-text h1 { font-size: 34px; font-weight: 800; color: var(--white); line-height: 1.2; margin-bottom: 16px; }
.auth-hero-text p  { font-size: 15px; color: rgba(255,255,255,0.8); line-height: 1.7; margin-bottom: 20px; }

.perks { display: flex; flex-direction: column; gap: 10px; font-size: 14px; color: rgba(255,255,255,0.9); }
.perks li { list-style: none; }

.auth-quote {
  font-size: 14px;
  font-style: italic;
  color: rgba(255,255,255,0.65);
  border-left: 3px solid var(--yellow);
  padding-left: 14px;
  line-height: 1.5;
}

.auth-right {
  background: var(--cream);
  padding: 40px 24px;
  overflow-y: auto;
}
.auth-card {
  background: var(--white);
  border-radius: var(--radius-lg);
  padding: 40px;
  max-width: 560px;
  margin: 0 auto;
  box-shadow: var(--shadow-md);
}
.auth-card h2 { font-size: 26px; font-weight: 800; margin-bottom: 6px; }
.auth-sub { font-size: 14px; color: var(--text-light); margin-bottom: 24px; }

.form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 14px; }

.checkbox-label { display: flex; align-items: flex-start; gap: 10px; font-size: 13px; color: var(--text-mid); cursor: pointer; }
.checkbox-label input { margin-top: 2px; accent-color: var(--green); }

.auth-btn { width: 100%; justify-content: center; padding: 13px; font-size: 15px; margin-top: 10px; }
.auth-footer-text { text-align: center; font-size: 14px; color: var(--text-mid); margin-top: 20px; }
.auth-link { color: var(--green); font-weight: 700; }

@media (max-width: 768px) {
  .auth-page { grid-template-columns: 1fr; }
  .auth-left  { display: none; }
  .form-row   { grid-template-columns: 1fr; }
}
</style>
