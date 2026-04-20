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
        <h1>Bienvenido de vuelta</h1>
        <p>Inicia sesión para ver el estado de tu solicitud de adopción o continuar el proceso.</p>
      </div>
      <div class="auth-stats">
        <div class="as-item"><span class="as-num">+125</span><span class="as-lab">Rescates</span></div>
        <div class="as-item"><span class="as-num">26</span><span class="as-lab">En adopción</span></div>
        <div class="as-item"><span class="as-num">+115</span><span class="as-lab">Adoptados</span></div>
      </div>
    </div>

    <div class="auth-right">
      <div class="auth-card">
        <h2>Iniciar sesión</h2>
        <p class="auth-sub">Accede con tu correo y contraseña</p>

        <div class="form-group">
          <label class="form-label">Correo electrónico</label>
          <input class="form-input" type="email" placeholder="correo@ejemplo.com" />
        </div>

        <div class="form-group">
          <label class="form-label">Contraseña</label>
          <input class="form-input" type="password" placeholder="••••••••" />
        </div>

        <div class="auth-options">
          <label class="checkbox-label">
            <input type="checkbox" />
            <span>Recordarme</span>
          </label>
          <a href="#" class="forgot-link">¿Olvidaste tu contraseña?</a>
        </div>

        <button class="btn-primary auth-btn">Iniciar sesión</button>

        <div class="auth-divider"><span>o</span></div>

        <!-- Admin shortcut for demo -->
        <RouterLink to="/admin" class="btn-outline-green auth-btn">
          🔧 Acceso administrador (demo)
        </RouterLink>

        <p class="auth-footer-text">
          ¿No tienes cuenta?
          <RouterLink to="/registro" class="auth-link">Regístrate gratis</RouterLink>
        </p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.auth-page {
  min-height: 100vh;
  display: grid;
  grid-template-columns: 1fr 1fr;
}
.auth-left {
  background: linear-gradient(160deg, var(--green-dark) 0%, var(--green) 100%);
  padding: 50px;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  color: var(--white);
}
.logo { font-size: 22px; font-weight: 700; color: var(--white); }
.logo strong { color: var(--yellow); }

.auth-hero-text h1 { font-size: 38px; font-weight: 800; color: var(--white); line-height: 1.2; margin-bottom: 16px; }
.auth-hero-text p  { font-size: 16px; color: rgba(255,255,255,0.8); line-height: 1.7; }

.auth-stats { display: flex; gap: 24px; }
.as-item { display: flex; flex-direction: column; gap: 2px; }
.as-num  { font-size: 28px; font-weight: 800; color: var(--yellow); }
.as-lab  { font-size: 12px; color: rgba(255,255,255,0.7); }

.auth-right {
  background: var(--cream);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 40px 24px;
}
.auth-card {
  background: var(--white);
  border-radius: var(--radius-lg);
  padding: 40px;
  width: 100%;
  max-width: 420px;
  box-shadow: var(--shadow-md);
}
.auth-card h2 { font-size: 26px; font-weight: 800; color: var(--text-dark); margin-bottom: 6px; }
.auth-sub { font-size: 14px; color: var(--text-light); margin-bottom: 28px; }

.auth-options {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  font-size: 13px;
}
.checkbox-label { display: flex; align-items: center; gap: 8px; color: var(--text-mid); cursor: pointer; }
.checkbox-label input { accent-color: var(--green); }
.forgot-link { color: var(--green); font-weight: 600; }

.auth-btn { width: 100%; justify-content: center; padding: 13px; font-size: 15px; margin-bottom: 10px; }

.auth-divider {
  text-align: center;
  position: relative;
  margin: 14px 0;
  color: var(--text-light);
  font-size: 13px;
}
.auth-divider::before {
  content:'';
  position: absolute;
  left: 0; right: 0; top: 50%;
  height: 1px;
  background: var(--border);
}
.auth-divider span {
  position: relative;
  background: var(--white);
  padding: 0 10px;
}

.auth-footer-text { text-align: center; font-size: 14px; color: var(--text-mid); margin-top: 20px; }
.auth-link { color: var(--green); font-weight: 700; }

@media (max-width: 768px) {
  .auth-page { grid-template-columns: 1fr; }
  .auth-left  { display: none; }
}
</style>
