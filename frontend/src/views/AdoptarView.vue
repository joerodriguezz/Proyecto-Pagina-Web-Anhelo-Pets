<script setup>
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import NavBar from '../components/NavBar.vue'
import FooterBar from '../components/FooterBar.vue'

const route = useRoute()
const router = useRouter()

const petId = route.params.id || ''
const petName = route.query.name || ''

const fullName = ref('')
const email = ref('')
const phone = ref('')
const address = ref('')
const message = ref('')
const submitted = ref(false)

function submitForm() {
  console.log('Solicitud de adopción', { petId, petName, fullName: fullName.value, email: email.value, phone: phone.value, address: address.value, message: message.value })
  submitted.value = true
}

function goBack() {
  router.push({ name: 'mascotas' })
}
</script>

<template>
  <NavBar />

  <div class="page-hero">
    <h1>💛 Solicitud de adopción</h1>
    <p class="section-subtitle">Completa el formulario y nos pondremos en contacto contigo.</p>
  </div>

  <section class="container" style="padding:24px 20px 60px;">
    <div class="card" style="max-width:760px; margin: 0 auto; padding: 22px;">
      <h2 class="section-title">Adoptar: {{ petName || 'Mascota' }}</h2>

      <div v-if="!submitted">
        <label class="form-label">Nombre completo</label>
        <input class="form-input" v-model="fullName" placeholder="Tu nombre completo" />

        <label class="form-label">Correo electrónico</label>
        <input class="form-input" v-model="email" type="email" placeholder="tu@ejemplo.com" />

        <label class="form-label">Teléfono</label>
        <input class="form-input" v-model="phone" placeholder="(55) 1234 5678" />

        <label class="form-label">Dirección</label>
        <input class="form-input" v-model="address" placeholder="Tu dirección" />

        <label class="form-label">Mensaje</label>
        <textarea class="form-input" v-model="message" rows="4" placeholder="Cuéntanos por qué te interesa esta mascota"></textarea>

        <div style="display:flex; gap:12px; margin-top:14px;">
          <button class="btn-primary" @click="submitForm">Enviar solicitud</button>
          <button class="btn-outline-green" @click="goBack">Volver al catálogo</button>
        </div>
      </div>

      <div v-else style="text-align:center; padding:30px 10px;">
        <h3>Gracias, {{ fullName }}!</h3>
        <p>Hemos recibido tu solicitud para adoptar <strong>{{ petName }}</strong>. Te contactaremos pronto al correo proporcionado.</p>
        <button class="btn-primary" @click="goBack">Volver al catálogo</button>
      </div>
    </div>
  </section>

  <FooterBar />
</template>

<style scoped>
.card { background: var(--white); border: 1px solid var(--border); border-radius: var(--radius-lg); box-shadow: var(--shadow-sm); }
.form-label { display:block; margin-top:12px; margin-bottom:6px; font-weight:600; }
.form-input { width:100%; padding:10px 12px; border-radius:8px; border:1px solid var(--border); }
</style>
