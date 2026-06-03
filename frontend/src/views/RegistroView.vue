<script setup>
import { RouterLink } from 'vue-router'
</script>

<template>
  <div class="auth-container">
    <div class="auth-visual">
      <div class="brand-wrapper">
        <RouterLink to="/" class="logo-link">
          Anhelo<span class="peach">Pets</span>
        </RouterLink>
      </div>
      
      <div class="visual-content">
        <h1 class="visual-title">Únete a nuestra <br>comunidad</h1>
        <p class="visual-description">
          Crea tu cuenta gratuita y da el primer paso para adoptar una mascota o apoyar nuestra fundación.
        </p>
      </div>

      <div class="visual-footer">
        <p>© Anhelo Pets. Dedicados al bienestar animal.</p>
      </div>
    </div>

    <div class="auth-form-side">
      <div class="form-container">
        <header class="form-header">
          <h2>Crear cuenta</h2>
          <p>Completa tus datos personales</p>
        </header>

        <form class="main-form" @submit.prevent>
          <div class="form-row">
            <div class="input-group">
              <label>Nombre completo *</label>
              <input placeholder="María González" class="custom-input" />
            </div>
            <div class="input-group">
              <label>Cédula *</label>
              <input placeholder="1-2345-6789" class="custom-input" />
            </div>
          </div>

          <div class="input-group">
            <label>Correo electrónico *</label>
            <input type="email" placeholder="correo@ejemplo.com" class="custom-input" />
          </div>

          <div class="form-row">
            <div class="input-group">
              <label>Edad *</label>
              <input type="number" placeholder="25" min="18" class="custom-input" />
            </div>
            <div class="input-group">
              <label>Nacionalidad *</label>
              <select class="custom-select">
                <option>Costa Rica</option>
                <option>Nicaragua</option>
                <option>Colombia</option>
                <option>Otro</option>
              </select>
            </div>
          </div>

          <div class="form-row">
            <div class="input-group">
              <label>Teléfono *</label>
              <input placeholder="+506 8xxx-xxxx" class="custom-input" />
            </div>
            <div class="input-group">
              <label>Provincia *</label>
              <select class="custom-select">
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

          <div class="input-group">
            <label>Dirección completa *</label>
            <input placeholder="Cantón, distrito, señas..." class="custom-input" />
          </div>

          <div class="form-row">
            <div class="input-group">
              <label>Contraseña *</label>
              <input type="password" placeholder="Mínimo 8 caracteres" class="custom-input" />
            </div>
            <div class="input-group">
              <label>Confirmar contraseña *</label>
              <input type="password" placeholder="Repetir contraseña" class="custom-input" />
            </div>
          </div>

          <div class="form-utils">
            <label class="custom-checkbox">
              <input type="checkbox" />
              <span class="label-text">
                Acepto los <a href="#" class="inner-link">términos y condiciones</a> y la <a href="#" class="inner-link">política de privacidad</a>
              </span>
            </label>
          </div>

          <button type="submit" class="btn-register">Crear mi cuenta</button>
        </form>

        <footer class="form-footer">
          <p>¿Ya tienes cuenta? 
            <RouterLink to="/login" class="login-link">Iniciar sesión</RouterLink>
          </p>
        </footer>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* ── Estructura Principal ── */
.auth-container {
  min-height: 100vh;
  display: flex;
  background-color: #FAFAFA;
}

/* ── Lado Visual (Izquierdo) ── */
.auth-visual {
  flex: 1;
  position: relative;
  background: linear-gradient(135deg, #92A894 0%, #7C927E 100%);
  padding: 60px;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  overflow: hidden;
  color: white;
}

.logo-link {
  font-size: 28px;
  font-weight: 800;
  color: white;
  text-decoration: none;
  letter-spacing: -1px;
}

.peach { color: #F9C17A; }

.visual-content {
  max-width: 440px;
  margin-auto: 0;
  z-index: 2;
}

.visual-title {
  font-size: 52px;
  line-height: 1.1;
  font-weight: 800;
  margin-bottom: 20px;
  letter-spacing: -2px;
}

.visual-description {
  font-size: 18px;
  line-height: 1.6;
  opacity: 0.9;
}

.visual-footer {
  z-index: 2;
  font-size: 13px;
  opacity: 0.6;
}

/* ── Lado Formulario (Derecho) ── */
.auth-form-side {
  flex: 1.3;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 50px 40px;
  background-color: #FAFAFA;
}

.form-container {
  width: 100%;
  max-width: 520px;
}

.form-header h2 {
  font-size: 32px;
  font-weight: 800;
  color: #3A473C;
  margin-bottom: 4px;
}

.form-header p {
  color: #6C756D;
  font-size: 15px;
  margin-bottom: 32px;
}

/* ── Filas y Grupos de Entrada ── */
.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.input-group {
  margin-bottom: 18px;
}

.input-group label {
  display: block;
  font-size: 14px;
  font-weight: 700;
  color: #3A473C;
  margin-bottom: 8px;
}

.custom-input, .custom-select {
  width: 100%;
  padding: 13px 16px;
  border-radius: 14px;
  border: 2px solid #F4F6F4;
  background-color: #F4F6F4;
  font-size: 15px;
  color: #3A473C;
  transition: all 0.3s ease;
  outline: none;
  box-sizing: border-box;
}

.custom-select {
  appearance: none;
  background-image: url("data:image/svg+xml;utf8,<svg xmlns='http://www.w3.org/2000/svg' width='24' height='24' viewBox='0 0 24 24' fill='none' stroke='%236C756D' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'><polyline points='6 9 12 15 18 9'></polyline></svg>");
  background-repeat: no-repeat;
  background-position: right 14px center;
  background-size: 16px;
  padding-right: 40px;
}

.custom-input:focus, .custom-select:focus {
  background-color: white;
  border-color: #92A894;
  box-shadow: 0 8px 20px rgba(146, 168, 148, 0.08);
}

/* ── Checkbox y Links ── */
.form-utils {
  margin: 24px 0;
}

.custom-checkbox {
  display: flex;
  align-items: flex-start;
  gap: 10px;
  cursor: pointer;
  font-size: 13px;
  color: #6C756D;
  line-height: 1.4;
}

.custom-checkbox input {
  accent-color: #92A894;
  margin-top: 2px;
}

.inner-link {
  color: #92A894;
  text-decoration: none;
  font-weight: 700;
}

/* ── Botón Registro ── */
.btn-register {
  width: 100%;
  padding: 16px;
  border-radius: 16px;
  border: none;
  background-color: #92A894;
  color: white;
  font-size: 16px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.3s ease;
}

.btn-register:hover {
  background-color: #7C927E;
  transform: translateY(-2px);
  box-shadow: 0 10px 25px rgba(146, 168, 148, 0.2);
}

/* ── Footer ── */
.form-footer {
  text-align: center;
  margin-top: 32px;
  font-size: 15px;
  color: #6C756D;
}

.login-link {
  color: #92A894;
  font-weight: 800;
  text-decoration: none;
}

/* ── Responsivo ── */
@media (max-width: 1024px) {
  .visual-title { font-size: 40px; }
  .auth-visual { padding: 40px; }
}

@media (max-width: 850px) {
  .auth-visual { display: none; }
  .auth-form-side { flex: 1; background-color: white; padding: 40px 24px; }
  .form-row { grid-template-columns: 1fr; gap: 0; }
}
</style>