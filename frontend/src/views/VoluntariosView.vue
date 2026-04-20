<script setup>
import NavBar from '../components/NavBar.vue'
import FooterBar from '../components/FooterBar.vue'
</script>

<template>
  <NavBar />

  <div class="page-hero">
    <h1>🤝 Voluntarios y Casas Cuna</h1>
    <p>Únete a nuestra red y ayuda a cambiar vidas de animales</p>
  </div>

  <section class="container vol-section">
    <div class="vol-grid">
      <!-- Form -->
      <div class="vol-form card">
        <h2>Registro de Voluntario</h2>
        <p class="form-subtitle">Completa el formulario y nos pondremos en contacto contigo.</p>

        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Nombre completo *</label>
            <input class="form-input" placeholder="Ej. María González" />
          </div>
          <div class="form-group">
            <label class="form-label">Cédula *</label>
            <input class="form-input" placeholder="1-2345-6789" />
          </div>
        </div>

        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Correo electrónico *</label>
            <input class="form-input" type="email" placeholder="correo@ejemplo.com" />
          </div>
          <div class="form-group">
            <label class="form-label">Teléfono *</label>
            <input class="form-input" placeholder="+506 8xxx-xxxx" />
          </div>
        </div>

        <div class="form-group">
          <label class="form-label">Dirección *</label>
          <input class="form-input" placeholder="Provincia, cantón, distrito..." />
        </div>

        <div class="form-group">
          <label class="form-label">Tipo de voluntariado *</label>
          <select class="form-select">
            <option>Seleccione una opción</option>
            <option>Casa cuna (hogar temporal)</option>
            <option>Apoyo en eventos de adopción</option>
            <option>Transporte de mascotas</option>
            <option>Cuidado en veterinaria</option>
            <option>Redes sociales y comunicación</option>
            <option>Otro</option>
          </select>
        </div>

        <div class="form-group">
          <label class="form-label">¿Por qué quieres ser voluntario?</label>
          <textarea class="form-textarea" placeholder="Cuéntanos tu motivación..."></textarea>
        </div>

        <div class="form-group checkbox-group">
          <label class="checkbox-label">
            <input type="checkbox" />
            <span>Acepto los términos y condiciones de voluntariado</span>
          </label>
          <label class="checkbox-label">
            <input type="checkbox" />
            <span>Autorizo el uso de mis datos para contacto por parte de Anhelo Pets</span>
          </label>
        </div>

        <button class="btn-primary" style="width:100%;justify-content:center;padding:14px;">
          🤝 Registrarme como voluntario
        </button>
      </div>

      <!-- Info panels -->
      <div class="vol-info">
        <div class="card info-card green-card">
          <span class="info-icon">🏡</span>
          <h3>¿Qué es una Casa Cuna?</h3>
          <p>Una casa cuna es un hogar temporal que acoge a una mascota rescatada mientras espera ser adoptada. La fundación cubre todos los gastos médicos y de alimentación.</p>
          <ul class="info-list">
            <li>✅ No tienes que pagar ningún costo</li>
            <li>✅ Recibimos el animal ya vacunado</li>
            <li>✅ Apoyo veterinario permanente</li>
            <li>✅ Duración promedio: 4-8 semanas</li>
          </ul>
        </div>

        <div class="card info-card">
          <span class="info-icon">⭐</span>
          <h3>Beneficios de ser voluntario</h3>
          <ul class="info-list">
            <li>🐾 Contribuir al bienestar animal</li>
            <li>📜 Certificado de voluntariado</li>
            <li>👥 Comunidad de amantes de animales</li>
            <li>📸 Participación en eventos especiales</li>
            <li>💙 Satisfacción de hacer la diferencia</li>
          </ul>
        </div>

        <div class="stats-mini">
          <div class="mini-stat" v-for="s in miniStats" :key="s.label">
            <span class="mini-icon">{{ s.icon }}</span>
            <span class="mini-value">{{ s.value }}</span>
            <span class="mini-label">{{ s.label }}</span>
          </div>
        </div>
      </div>
    </div>
  </section>

  <FooterBar />
</template>

<script>
export default {
  data() {
    return {
      miniStats: [
        { icon:'👥', value:'40+', label:'Voluntarios activos' },
        { icon:'🏠', value:'5',   label:'Casas cuna' },
        { icon:'🐾', value:'115+',label:'Animales ayudados' },
      ]
    }
  }
}
</script>

<style scoped>
.vol-section { padding: 50px 24px 70px; }
.vol-grid { display: grid; grid-template-columns: 1fr 400px; gap: 36px; align-items: start; }

.vol-form h2 { font-size: 22px; font-weight: 800; color: var(--text-dark); margin-bottom: 6px; }
.form-subtitle { font-size: 14px; color: var(--text-light); margin-bottom: 24px; }
.form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 14px; }

.checkbox-group { display: flex; flex-direction: column; gap: 10px; }
.checkbox-label { display: flex; align-items: flex-start; gap: 10px; font-size: 13px; color: var(--text-mid); cursor: pointer; }
.checkbox-label input { margin-top: 2px; accent-color: var(--green); }

.info-card { margin-bottom: 18px; }
.info-icon { font-size: 36px; display: block; margin-bottom: 12px; }
.info-card h3 { font-size: 18px; font-weight: 700; color: var(--text-dark); margin-bottom: 10px; }
.info-card p { font-size: 14px; color: var(--text-mid); line-height: 1.6; margin-bottom: 14px; }
.info-list { display: flex; flex-direction: column; gap: 8px; font-size: 14px; color: var(--text-mid); padding-left: 0; }
.info-list li { list-style: none; }

.green-card { border-top: 4px solid var(--green); }

.stats-mini {
  display: flex;
  gap: 12px;
}
.mini-stat {
  flex: 1;
  background: var(--green-dark);
  border-radius: var(--radius-md);
  padding: 16px 12px;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
  text-align: center;
}
.mini-icon  { font-size: 22px; }
.mini-value { font-size: 22px; font-weight: 800; color: var(--yellow); }
.mini-label { font-size: 11px; color: rgba(255,255,255,0.75); font-weight: 500; }

@media (max-width: 900px) {
  .vol-grid { grid-template-columns: 1fr; }
  .form-row  { grid-template-columns: 1fr; }
}
</style>
