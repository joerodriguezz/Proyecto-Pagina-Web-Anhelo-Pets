<script setup>
import { ref } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import { authApi } from '../services/api'

const router = useRouter()
const loading = ref(false)
const errorMessage = ref('')

const form = ref({
  username: '',
  nationalId: '',
  firstName: '',
  middleName: '',
  lastName: '',
  secondLastName: '',
  age: '',
  nationality: 'Costa Rica',
  email: '',
  phonePrimary: '',
  phoneSecondary: '',
  city: 'San Jose',
  town: '',
  addressLine: '',
  password: '',
  confirmPassword: '',
  acceptedTerms: false,
})

function birthDateFromAge(age) {
  const numericAge = Number(age)
  if (!Number.isFinite(numericAge) || numericAge < 1) {
    return ''
  }

  const today = new Date()
  const year = today.getFullYear() - numericAge
  return `${year}-${String(today.getMonth() + 1).padStart(2, '0')}-${String(today.getDate()).padStart(2, '0')}`
}

async function register() {
  if (form.value.password !== form.value.confirmPassword) {
    errorMessage.value = 'Las contrasenas no coinciden.'
    return
  }

  if (!form.value.acceptedTerms) {
    errorMessage.value = 'Debes aceptar los terminos y condiciones.'
    return
  }

  if (!birthDateFromAge(form.value.age)) {
    errorMessage.value = 'La edad ingresada no es valida.'
    return
  }

  loading.value = true
  errorMessage.value = ''

  try {
    await authApi.register({
      username: form.value.username,
      password: form.value.password,
      nationalId: form.value.nationalId,
      firstName: form.value.firstName,
      middleName: form.value.middleName,
      lastName: form.value.lastName,
      secondLastName: form.value.secondLastName,
      birthDate: birthDateFromAge(form.value.age),
      nationality: form.value.nationality,
      email: form.value.email,
      phonePrimary: form.value.phonePrimary,
      phoneSecondary: form.value.phoneSecondary,
      city: form.value.city,
      town: form.value.town,
      addressLine: form.value.addressLine,
      createdBy: 'frontend',
    })

    router.push('/login')
  } catch (error) {
    errorMessage.value = error.message || 'No se pudo crear la cuenta.'
  } finally {
    loading.value = false
  }
}
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
        <h1 class="visual-title">Unete a nuestra <br>comunidad</h1>
        <p class="visual-description">
          Crea tu cuenta gratuita y da el primer paso para adoptar una mascota o apoyar nuestra fundacion.
        </p>
      </div>

      <div class="visual-footer">
        <p>Anhelo Pets. Dedicados al bienestar animal.</p>
      </div>
    </div>

    <div class="auth-form-side">
      <div class="form-container">
        <header class="form-header">
          <h2>Crear cuenta</h2>
          <p>Completa los datos requeridos por el registro</p>
        </header>

        <form class="main-form" @submit.prevent="register">
          <div class="input-group">
            <label>Usuario *</label>
            <input v-model="form.username" placeholder="maria.gonzalez" class="custom-input" required />
          </div>

          <div class="input-group">
            <label>Cedula *</label>
            <input v-model="form.nationalId" placeholder="1-2345-6789" class="custom-input" required />
          </div>

          <div class="form-row">
            <div class="input-group">
              <label>Primer nombre *</label>
              <input v-model="form.firstName" placeholder="Maria" class="custom-input" required />
            </div>
            <div class="input-group">
              <label>Segundo nombre</label>
              <input v-model="form.middleName" placeholder="Fernanda" class="custom-input" />
            </div>
          </div>

          <div class="form-row">
            <div class="input-group">
              <label>Primer apellido *</label>
              <input v-model="form.lastName" placeholder="Gonzalez" class="custom-input" required />
            </div>
            <div class="input-group">
              <label>Segundo apellido</label>
              <input v-model="form.secondLastName" placeholder="Mora" class="custom-input" />
            </div>
          </div>

          <div class="form-row">
            <div class="input-group">
              <label>Edad *</label>
              <input v-model="form.age" type="number" min="1" max="120" placeholder="25" class="custom-input" required />
            </div>
            <div class="input-group">
              <label>Correo electronico *</label>
              <input v-model="form.email" type="email" placeholder="correo@ejemplo.com" class="custom-input" required />
            </div>
          </div>

          <div class="form-row">
            <div class="input-group">
              <label>Telefono principal *</label>
              <input v-model="form.phonePrimary" placeholder="+506 8888-8888" class="custom-input" required />
            </div>
            <div class="input-group">
              <label>Telefono secundario</label>
              <input v-model="form.phoneSecondary" placeholder="+506 2222-2222" class="custom-input" />
            </div>
          </div>

          <div class="form-row">
            <div class="input-group">
              <label>Provincia *</label>
              <select v-model="form.city" class="custom-select" required>
                <option>San Jose</option>
                <option>Alajuela</option>
                <option>Cartago</option>
                <option>Heredia</option>
                <option>Guanacaste</option>
                <option>Puntarenas</option>
                <option>Limon</option>
              </select>
            </div>
            <div class="input-group">
              <label>Nacionalidad *</label>
              <input v-model="form.nationality" placeholder="Costa Rica" class="custom-input" required />
            </div>
          </div>

          <div class="input-group">
            <label>Canton / distrito *</label>
            <input v-model="form.town" placeholder="Turrialba, La Suiza" class="custom-input" required />
          </div>

          <div class="input-group">
            <label>Direccion completa *</label>
            <input v-model="form.addressLine" placeholder="Senas exactas" class="custom-input" required />
          </div>

          <div class="form-row">
            <div class="input-group">
              <label>Contrasena *</label>
              <input v-model="form.password" type="password" placeholder="Minimo 8 caracteres" class="custom-input" required />
            </div>
            <div class="input-group">
              <label>Confirmar contrasena *</label>
              <input v-model="form.confirmPassword" type="password" placeholder="Repetir contrasena" class="custom-input" required />
            </div>
          </div>

          <div class="form-utils">
            <label class="custom-checkbox">
              <input v-model="form.acceptedTerms" type="checkbox" />
              <span class="label-text">
                Acepto los <a href="#" class="inner-link">terminos y condiciones</a> y la
                <a href="#" class="inner-link">politica de privacidad</a>
              </span>
            </label>
          </div>

          <p v-if="errorMessage" class="form-error">{{ errorMessage }}</p>

          <button type="submit" class="btn-register" :disabled="loading">
            {{ loading ? 'Creando...' : 'Crear mi cuenta' }}
          </button>
        </form>

        <footer class="form-footer">
          <p>Ya tienes cuenta?
            <RouterLink to="/login" class="login-link">Iniciar sesion</RouterLink>
          </p>
        </footer>
      </div>
    </div>
  </div>
</template>

<style scoped>
.auth-container {
  min-height: 100vh;
  display: flex;
  background-color: #FAFAFA;
}

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
  margin: auto 0;
  z-index: 2;
}

.visual-title {
  font-size: 52px;
  line-height: 1.1;
  font-weight: 800;
  margin-bottom: 20px;
  letter-spacing: 0;
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
  max-width: 560px;
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

.custom-input,
.custom-select {
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

.custom-input:focus,
.custom-select:focus {
  background-color: white;
  border-color: #92A894;
  box-shadow: 0 8px 20px rgba(146, 168, 148, 0.08);
}

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

.inner-link,
.login-link {
  color: #92A894;
  text-decoration: none;
  font-weight: 800;
}

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

.btn-register:disabled {
  cursor: not-allowed;
  opacity: 0.75;
}

.form-error {
  color: #B42318;
  font-size: 14px;
  font-weight: 700;
  margin: -8px 0 16px;
}

.form-footer {
  text-align: center;
  margin-top: 32px;
  font-size: 15px;
  color: #6C756D;
}

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
