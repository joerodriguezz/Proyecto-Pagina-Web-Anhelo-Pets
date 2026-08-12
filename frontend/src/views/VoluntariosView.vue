<script setup>
import {
  ref,
  computed,
  watch,
  onMounted,
  onBeforeUnmount
} from 'vue'
import NavBar from '../components/NavBar.vue'
import FooterBar from '../components/FooterBar.vue'
import { ubicacionesCR } from '../data/ubicaciones'
import { submitVolunteerApplication, getMyVolunteerApplication, parseApplicationDetails } from '../services/volunteerServices'
import { useAuthStore } from '../stores/useAuthStore'

/* ─── USUARIO ─────────────────────────────────────────── */

const authStore = useAuthStore()

// /api/auth/me no trae dirección (vive en otra tabla y no forma parte de
// la sesión) — nombre/correo/cédula/teléfono sí se pueden pre-llenar.
const usuarioActivo = computed(() => {
  if (!authStore.user) return null
  return {
    nombre: [authStore.user.firstName, authStore.user.lastName].filter(Boolean).join(' '),
    correo: authStore.user.email,
    cedula: authStore.user.nationalId || '',
    telefono: authStore.user.phonePrimary || ''
  }
})

/* ─── BENEFICIOS ──────────────────────────────────────── */

const benefits = [
  {
    icon: 'bx bxs-heart',
    title: 'Apoyo veterinario',
    text: 'La fundación cubre controles y atención médica para cada mascota.'
  },
  {
    icon: 'bx bxs-bowl-hot',
    title: 'Alimento incluido',
    text: 'Nosotros proporcionamos comida y todos los suministros necesarios.'
  },
  {
    icon: 'bx bxs-shield-plus',
    title: 'Seguimiento constante',
    text: 'Acompañamiento y soporte durante todo el proceso de cuidado.'
  },
  {
    icon: 'bx bxs-home-heart',
    title: 'Impacto real',
    text: 'Tu apoyo impulsa directamente rescates y adopciones exitosas.'
  }
]

const volunteerTypes = [
  { value: 'Casa cuna', label: 'Casa cuna' },
  { value: 'Eventos de adopción', label: 'Eventos de adopción' },
  { value: 'Transporte', label: 'Transporte' },
  { value: 'Veterinaria', label: 'Veterinaria' },
  { value: 'Redes sociales', label: 'Redes sociales' },
  { value: 'Rescatista', label: 'Rescatista' }
]

/* ─── INFORMACIÓN DINÁMICA POR TIPO DE VOLUNTARIADO ──── */

const volunteerInfo = {
  'Casa cuna': {
    title: 'Casa cuna',
    description: 'Brinda un hogar temporal a mascotas rescatadas mientras encuentran una familia definitiva.',
    responsibilities: [
      'Hospedar mascotas temporalmente.',
      'Brindar alimento y agua.',
      'Dar cariño y socialización.',
      'Informar cambios de salud.',
      'Coordinar citas veterinarias.'
    ]
  },
  'Eventos de adopción': {
    title: 'Eventos de adopción',
    description: 'Apoya en ferias y jornadas de adopción para conectar mascotas con nuevas familias.',
    responsibilities: [
      'Atender al público en el evento.',
      'Mostrar y presentar a las mascotas.',
      'Explicar el proceso de adopción.',
      'Organizar el espacio del evento.',
      'Apoyar en el traslado de mascotas.'
    ]
  },
  'Transporte': {
    title: 'Transporte',
    description: 'Colabora trasladando mascotas entre hogares, veterinarias y eventos de adopción.',
    responsibilities: [
      'Trasladar mascotas de forma segura.',
      'Coordinar horarios de traslado.',
      'Mantener el vehículo en condiciones adecuadas.',
      'Apoyar en traslados de emergencia.',
      'Confirmar entregas con la fundación.'
    ]
  },
  'Veterinaria': {
    title: 'Veterinaria',
    description: 'Brinda apoyo profesional en la atención médica de las mascotas rescatadas.',
    responsibilities: [
      'Realizar consultas y revisiones.',
      'Apoyar en esterilizaciones.',
      'Atender emergencias médicas.',
      'Dar seguimiento a tratamientos.',
      'Asesorar sobre cuidados de salud.'
    ]
  },
  'Redes sociales': {
    title: 'Redes sociales',
    description: 'Ayuda a difundir rescates, adopciones y campañas a través de contenido digital.',
    responsibilities: [
      'Crear contenido para redes.',
      'Editar fotos y videos.',
      'Redactar publicaciones.',
      'Gestionar comunidad y mensajes.',
      'Apoyar campañas de difusión.'
    ]
  },
  'Rescatista': {
    title: 'Rescatista',
    description: 'Participa directamente en el rescate de mascotas en situación de calle o riesgo.',
    responsibilities: [
      'Atender reportes de rescate.',
      'Capturar mascotas de forma segura.',
      'Brindar primeros auxilios básicos.',
      'Coordinar traslado a veterinaria.',
      'Dar seguimiento al caso rescatado.'
    ]
  }
}

const currentVolunteerInfo = computed(() =>
  volunteerType.value ? volunteerInfo[volunteerType.value] : null
)

/* ─── CAMPOS BASE ─────────────────────────────────────── */

const fullName      = ref('')
const idCard        = ref('')
const email         = ref('')
const phone         = ref('')
const volunteerType = ref('')
const submitted     = ref(false)

/* ─── TELÉFONO ────────────────────────────────────────── */

const dropdownRef       = ref(null)
const showCodeDropdown  = ref(false)
const codeSearch        = ref('')

const selectedCountry = ref({ name: 'Costa Rica', code: '+506' })

const phoneCodesList = [
  { name: 'Afganistán', code: '+93' },
  { name: 'Albania', code: '+355' },
  { name: 'Alemania', code: '+49' },
  { name: 'Andorra', code: '+376' },
  { name: 'Angola', code: '+244' },
  { name: 'Antigua y Barbuda', code: '+1-268' },
  { name: 'Arabia Saudita', code: '+966' },
  { name: 'Argelia', code: '+213' },
  { name: 'Argentina', code: '+54' },
  { name: 'Armenia', code: '+374' },
  { name: 'Australia', code: '+61' },
  { name: 'Austria', code: '+43' },
  { name: 'Azerbaiyán', code: '+994' },
  { name: 'Bahamas', code: '+1-242' },
  { name: 'Bangladés', code: '+880' },
  { name: 'Barbados', code: '+1-246' },
  { name: 'Baréin', code: '+973' },
  { name: 'Bélgica', code: '+32' },
  { name: 'Belice', code: '+501' },
  { name: 'Benín', code: '+229' },
  { name: 'Bielorrusia', code: '+375' },
  { name: 'Bolivia', code: '+591' },
  { name: 'Bosnia y Herzegovina', code: '+387' },
  { name: 'Brasil', code: '+55' },
  { name: 'Brunéi', code: '+673' },
  { name: 'Bulgaria', code: '+359' },
  { name: 'Camboya', code: '+855' },
  { name: 'Camerún', code: '+237' },
  { name: 'Canadá', code: '+1' },
  { name: 'Catar', code: '+974' },
  { name: 'Chile', code: '+56' },
  { name: 'China', code: '+86' },
  { name: 'Chipre', code: '+357' },
  { name: 'Colombia', code: '+57' },
  { name: 'Corea del Sur', code: '+82' },
  { name: 'Costa Rica', code: '+506' },
  { name: 'Croacia', code: '+385' },
  { name: 'Cuba', code: '+53' },
  { name: 'Dinamarca', code: '+45' },
  { name: 'Ecuador', code: '+593' },
  { name: 'Egipto', code: '+20' },
  { name: 'El Salvador', code: '+503' },
  { name: 'Emiratos Árabes Unidos', code: '+971' },
  { name: 'Eslovaquia', code: '+421' },
  { name: 'Eslovenia', code: '+386' },
  { name: 'España', code: '+34' },
  { name: 'Estados Unidos', code: '+1' },
  { name: 'Estonia', code: '+372' },
  { name: 'Etiopía', code: '+251' },
  { name: 'Filipinas', code: '+63' },
  { name: 'Finlandia', code: '+358' },
  { name: 'Francia', code: '+33' },
  { name: 'Georgia', code: '+995' },
  { name: 'Grecia', code: '+30' },
  { name: 'Guatemala', code: '+502' },
  { name: 'Haití', code: '+509' },
  { name: 'Honduras', code: '+504' },
  { name: 'Hungría', code: '+36' },
  { name: 'India', code: '+91' },
  { name: 'Indonesia', code: '+62' },
  { name: 'Irak', code: '+964' },
  { name: 'Irán', code: '+98' },
  { name: 'Irlanda', code: '+353' },
  { name: 'Islandia', code: '+354' },
  { name: 'Israel', code: '+972' },
  { name: 'Italia', code: '+39' },
  { name: 'Jamaica', code: '+1-876' },
  { name: 'Japón', code: '+81' },
  { name: 'Jordania', code: '+962' },
  { name: 'Kazajistán', code: '+7' },
  { name: 'Kenia', code: '+254' },
  { name: 'Kuwait', code: '+965' },
  { name: 'Laos', code: '+856' },
  { name: 'Letonia', code: '+371' },
  { name: 'Líbano', code: '+961' },
  { name: 'Libia', code: '+218' },
  { name: 'Lituania', code: '+370' },
  { name: 'Luxemburgo', code: '+352' },
  { name: 'Madagascar', code: '+261' },
  { name: 'Malasia', code: '+60' },
  { name: 'México', code: '+52' },
  { name: 'Marruecos', code: '+212' },
  { name: 'Nepal', code: '+977' },
  { name: 'Nicaragua', code: '+505' },
  { name: 'Nigeria', code: '+234' },
  { name: 'Noruega', code: '+47' },
  { name: 'Nueva Zelanda', code: '+64' },
  { name: 'Omán', code: '+968' },
  { name: 'Países Bajos', code: '+31' },
  { name: 'Pakistán', code: '+92' },
  { name: 'Panamá', code: '+507' },
  { name: 'Paraguay', code: '+595' },
  { name: 'Perú', code: '+51' },
  { name: 'Polonia', code: '+48' },
  { name: 'Portugal', code: '+351' },
  { name: 'Reino Unido', code: '+44' },
  { name: 'República Dominicana', code: '+1-809' },
  { name: 'Rumania', code: '+40' },
  { name: 'Rusia', code: '+7' },
  { name: 'Singapur', code: '+65' },
  { name: 'Sudáfrica', code: '+27' },
  { name: 'Suecia', code: '+46' },
  { name: 'Suiza', code: '+41' },
  { name: 'Tailandia', code: '+66' },
  { name: 'Turquía', code: '+90' },
  { name: 'Ucrania', code: '+380' },
  { name: 'Uganda', code: '+256' },
  { name: 'Uruguay', code: '+598' },
  { name: 'Venezuela', code: '+58' },
  { name: 'Vietnam', code: '+84' },
  { name: 'Yemen', code: '+967' },
  { name: 'Zambia', code: '+260' },
  { name: 'Zimbabue', code: '+263' }
]

// El backend guarda phone_primary como "{código} {número}" (ej. "+506 88888888").
function splitPhoneWithCode(fullPhone) {
  const trimmed = String(fullPhone || '').trim()
  if (!trimmed) return { code: null, number: '' }
  const spaceIdx = trimmed.indexOf(' ')
  if (spaceIdx === -1) return { code: null, number: trimmed.replace(/\D/g, '') }
  const codePart   = trimmed.slice(0, spaceIdx)
  const numberPart = trimmed.slice(spaceIdx + 1).replace(/\D/g, '')
  return { code: phoneCodesList.find(c => c.code === codePart) || null, number: numberPart }
}

const filteredCodes = computed(() => {
  const q = codeSearch.value.toLowerCase()
  return phoneCodesList.filter(
    i => i.name.toLowerCase().includes(q) || i.code.includes(q)
  )
})

function selectCode(item) {
  selectedCountry.value = item
  showCodeDropdown.value = false
  codeSearch.value = ''
}

function filterPhoneInput() {
  phone.value = phone.value.replace(/[^0-9]/g, '')
}

function handleClickOutside(e) {
  if (dropdownRef.value && !dropdownRef.value.contains(e.target)) {
    showCodeDropdown.value = false
  }
}

onBeforeUnmount(() => {
  document.removeEventListener('click', handleClickOutside)
})

/* ─── UBICACIÓN ───────────────────────────────────────── */

const provincia = ref('')
const canton    = ref('')
const distrito  = ref('')

const provincias = computed(() =>
  ubicacionesCR ? Object.keys(ubicacionesCR) : []
)

const cantones = computed(() => {
  if (!provincia.value || !ubicacionesCR) return []
  return Object.keys(ubicacionesCR[provincia.value] || {})
})

const distritos = computed(() => {
  if (!provincia.value || !canton.value || !ubicacionesCR) return []
  return ubicacionesCR[provincia.value]?.[canton.value] || []
})

watch(provincia, () => {
  canton.value   = ''
  distrito.value = ''
})

watch(canton, () => {
  distrito.value = ''
})

/* ─── DATOS ESPECÍFICOS POR TIPO ──────────────────────── */

// Casa cuna
const cc_maxMascotas    = ref('')
const cc_tipoVivienda   = ref('')
const cc_patioCerrado   = ref('')
const cc_otrasMascotas  = ref('')
const cc_ninos          = ref('')
const cc_tiempoDisp     = ref('')
const cc_puedeRecibir   = ref([])
const cc_comentarios    = ref('')

// Eventos de adopción
const ev_participadoAntes  = ref('')
const ev_experienciaPublico= ref('')
const ev_disponibilidad    = ref([])
const ev_horario           = ref('')
const ev_habilidades       = ref([])
const ev_transportePropio  = ref('')

// Transporte
const tr_tipoVehiculo  = ref('')
const tr_cobertura     = ref('')
const tr_disponibilidad= ref([])
const tr_licencia      = ref('')
const tr_puedeTransp   = ref([])

// Veterinaria
const vet_profesion    = ref('')
const vet_colegiado    = ref('')
const vet_especialidades = ref([])
const vet_disponibilidad = ref([])
const vet_clinica      = ref('')

// Redes sociales
const rs_red           = ref('')
const rs_experiencia   = ref([])
const rs_portafolio    = ref('')
const rs_horasSemanales= ref('')
const rs_programas     = ref([])

// Rescatista
const re_anosExp       = ref('')
const re_cantRescates  = ref('')
const re_equipo        = ref([])
const re_disponibilidad= ref('')
const re_zonaProvincia = ref('')
const re_zonaCanton    = ref('')
const re_capacitacion  = ref('')

const re_zonaCantones = computed(() => {
  if (!re_zonaProvincia.value || !ubicacionesCR) return []
  return Object.keys(ubicacionesCR[re_zonaProvincia.value] || {})
})

watch(re_zonaProvincia, () => { re_zonaCanton.value = '' })

/* ─── HELPERS CHECKBOX ────────────────────────────────── */

function toggleCheck(arr, val) {
  const i = arr.indexOf(val)
  if (i === -1) arr.push(val)
  else arr.splice(i, 1)
}

/* ─── ESTADO / COMPUTED ───────────────────────────────── */

const loggedIn = computed(() => usuarioActivo.value !== null)

/* La solicitud real vive en la base de datos, no en el usuario local */
const solicitudActual = ref(null)
const cargandoSolicitud = ref(false)

// El estado real usa 'Aprobado'/'Rechazado' (concuerda con "voluntario");
// esta vista ya usa las formas femeninas ('Aprobada'/'Rechazada') en toda
// la plantilla, así que se traduce una sola vez aquí.
const ESTADO_DB_A_VISTA = {
  Aprobado: 'Aprobada',
  Rechazado: 'Rechazada',
  Pendiente: 'Pendiente'
}

/* ─── PERMITIR NUEVA SOLICITUD TRAS SER APROBADO ──────── */

// El usuario decide explícitamente que quiere postularse a otro tipo
// de voluntariado una vez que su solicitud actual fue aprobada.
const quiereNuevaSolicitud = ref(false)

// Si ya tiene una solicitud aprobada, ese tipo de voluntariado queda
// bloqueado para no volver a solicitarlo; el resto de tipos sigue disponible.
const tipoBloqueado = computed(() =>
  solicitudActual.value?.estado === 'Aprobada' ? solicitudActual.value.tipo : null
)

const tiposDisponibles = computed(() => {
  if (!tipoBloqueado.value) return volunteerTypes
  return volunteerTypes.filter(t => t.value !== tipoBloqueado.value)
})

// El formulario para una nueva solicitud solo se muestra cuando ya existe
// una aprobada y el usuario pidió explícitamente enviar otra.
const mostrarFormularioNuevo = computed(() =>
  quiereNuevaSolicitud.value && solicitudActual.value?.estado === 'Aprobada'
)

function iniciarNuevaSolicitud() {
  quiereNuevaSolicitud.value = true
  volunteerType.value = ''
}

async function cargarSolicitudActual() {
  if (!usuarioActivo.value?.correo) {
    solicitudActual.value = null
    return
  }

  cargandoSolicitud.value = true
  try {
    const { data } = await getMyVolunteerApplication(usuarioActivo.value.correo)
    solicitudActual.value = data
      ? {
          tipo: data.volunteerType,
          estado: ESTADO_DB_A_VISTA[data.validationStatus] || data.validationStatus
        }
      : null
  } catch {
    solicitudActual.value = null
  } finally {
    cargandoSolicitud.value = false
  }
}

/* ─── VALIDACIÓN ──────────────────────────────────────── */

const baseValid = computed(() =>
  fullName.value &&
  idCard.value &&
  email.value &&
  phone.value &&
  provincia.value &&
  canton.value &&
  distrito.value &&
  volunteerType.value
)

const specificValid = computed(() => {
  switch (volunteerType.value) {
    case 'Casa cuna':
      return cc_maxMascotas.value && cc_tipoVivienda.value &&
             cc_patioCerrado.value && cc_otrasMascotas.value &&
             cc_ninos.value && cc_tiempoDisp.value &&
             cc_puedeRecibir.value.length > 0
    case 'Eventos de adopción':
      return ev_participadoAntes.value && ev_experienciaPublico.value &&
             ev_disponibilidad.value.length > 0 && ev_horario.value &&
             ev_habilidades.value.length > 0 && ev_transportePropio.value
    case 'Transporte':
      return tr_tipoVehiculo.value && tr_cobertura.value &&
             tr_disponibilidad.value.length > 0 && tr_licencia.value &&
             tr_puedeTransp.value.length > 0
    case 'Veterinaria':
      return vet_profesion.value && vet_especialidades.value.length > 0 &&
             vet_disponibilidad.value.length > 0
    case 'Redes sociales':
      return rs_red.value && rs_experiencia.value.length > 0 &&
             rs_horasSemanales.value && rs_programas.value.length > 0
    case 'Rescatista':
      return re_anosExp.value && re_cantRescates.value &&
             re_equipo.value.length > 0 && re_disponibilidad.value &&
             re_zonaProvincia.value && re_zonaCanton.value &&
             re_capacitacion.value
    default: return false
  }
})

const formValid = computed(() => baseValid.value && specificValid.value)

/* ─── AUTOCOMPLETAR ───────────────────────────────────── */

function prellenarDesdeSesion() {
  if (!usuarioActivo.value) return
  fullName.value = usuarioActivo.value.nombre || ''
  email.value    = usuarioActivo.value.correo || ''
  idCard.value   = usuarioActivo.value.cedula || ''

  const { code, number } = splitPhoneWithCode(usuarioActivo.value.telefono)
  if (code) selectedCountry.value = code
  phone.value = number
}

// authStore.user llega async (fetch a /api/auth/me) — si todavía no resolvió
// al montar el componente, esto reintenta cargar la solicitud y pre-llenar
// el formulario en cuanto aparezca la sesión.
watch(usuarioActivo, (nuevo) => {
  if (nuevo) {
    cargarSolicitudActual()
    prellenarDesdeSesion()
  }
})

onMounted(() => {
  document.addEventListener('click', handleClickOutside)
  cargarSolicitudActual()
  prellenarDesdeSesion()
})

/* ─── GUARDAR ─────────────────────────────────────────── */

function buildDatosEspecificos() {
  switch (volunteerType.value) {
    case 'Casa cuna':
      return {
        maxMascotas:   cc_maxMascotas.value,
        tipoVivienda:  cc_tipoVivienda.value,
        patioCerrado:  cc_patioCerrado.value,
        otrasMascotas: cc_otrasMascotas.value,
        ninos:         cc_ninos.value,
        tiempoDisp:    cc_tiempoDisp.value,
        puedeRecibir:  [...cc_puedeRecibir.value],
        comentarios:   cc_comentarios.value
      }
    case 'Eventos de adopción':
      return {
        participadoAntes:   ev_participadoAntes.value,
        experienciaPublico: ev_experienciaPublico.value,
        disponibilidad:     [...ev_disponibilidad.value],
        horario:            ev_horario.value,
        habilidades:        [...ev_habilidades.value],
        transportePropio:   ev_transportePropio.value
      }
    case 'Transporte':
      return {
        tipoVehiculo:  tr_tipoVehiculo.value,
        cobertura:     tr_cobertura.value,
        disponibilidad:[...tr_disponibilidad.value],
        licencia:      tr_licencia.value,
        puedeTransp:   [...tr_puedeTransp.value]
      }
    case 'Veterinaria':
      return {
        profesion:     vet_profesion.value,
        colegiado:     vet_colegiado.value,
        especialidades:[...vet_especialidades.value],
        disponibilidad:[...vet_disponibilidad.value],
        clinica:       vet_clinica.value
      }
    case 'Redes sociales':
      return {
        red:           rs_red.value,
        experiencia:   [...rs_experiencia.value],
        portafolio:    rs_portafolio.value,
        horasSemanales:rs_horasSemanales.value,
        programas:     [...rs_programas.value]
      }
    case 'Rescatista':
      return {
        anosExp:       re_anosExp.value,
        cantRescates:  re_cantRescates.value,
        equipo:        [...re_equipo.value],
        disponibilidad:re_disponibilidad.value,
        zonaProvincia: re_zonaProvincia.value,
        zonaCanton:    re_zonaCanton.value,
        capacitacion:  re_capacitacion.value
      }
    default: return {}
  }
}

const enviandoSolicitud = ref(false)
const errorEnvio = ref('')

async function submitVolunteer() {
  if (!formValid.value || !usuarioActivo.value) return

  errorEnvio.value = ''
  enviandoSolicitud.value = true

  const telefonoCompleto = `${selectedCountry.value.code} ${phone.value}`

  try {
    await submitVolunteerApplication({
      email:             usuarioActivo.value.correo,
      nationalId:        idCard.value,
      volunteerType:     volunteerType.value,
      applicationDetails: buildDatosEspecificos(),
      phonePrimary:      telefonoCompleto,
      city:              provincia.value,
      town:              canton.value,
      district:          distrito.value
    })

    submitted.value = true
    quiereNuevaSolicitud.value = false
    await cargarSolicitudActual()
  } catch (e) {
    errorEnvio.value = e?.response?.data?.message || 'No se pudo enviar la solicitud. Intenta de nuevo.'
  } finally {
    enviandoSolicitud.value = false
  }
}
</script>

<template>
  <NavBar />

  <!-- ═══════════════════════════════════════ HERO -->
  <section class="hero">
    <img
      src="/img-vol/herovoluntarios.PNG"
      class="hero-image"
      alt="Voluntarios de Anhelo Pets"
    >
    <div class="hero-overlay"></div>
    <div class="hero-content">
      <div class="hero-inner">
        
        <h1>
          Sé parte de una<br>
          <span class="hero-accent">segunda oportunidad</span>
        </h1>
        <p>
          Forma parte de nuestra red de voluntarios y hogares temporales
          para brindar amor, cuidado y nuevas oportunidades a quienes más lo necesitan.
        </p>
        <div class="hero-stats">
          <div class="stat">
            <strong>+120</strong>
            <span>Voluntarios activos</span>
          </div>
          <div class="stat-divider"></div>
          <div class="stat">
            <strong>+340</strong>
            <span>Rescates apoyados</span>
          </div>
          <div class="stat-divider"></div>
          <div class="stat">
            <strong>6</strong>
            <span>Tipos de apoyo</span>
          </div>
        </div>
      </div>
    </div>
  </section>

  <!-- ═══════════════════════════════════════ MAIN -->
  <section class="main-section">
    <div class="container vol-grid">

      <!-- ───────────── LEFT -->
      <div class="left-col">

        <div class="intro-card">
          <div class="intro-img-wrap">
            <img src="/img-vol/casavol.PNG" alt="Voluntariado Anhelo Pets">
          </div>
          <div class="intro-body">
  
            <h2>¿Qué es una casa cuna?</h2>
            <p>
              Un hogar temporal que recibe mascotas rescatadas mientras encuentran
              una familia definitiva y segura. Con tu apoyo, cada animal tiene
              una segunda oportunidad real.
            </p>
          </div>
        </div>

        <div class="benefits-grid">
          <div
            v-for="item in benefits"
            :key="item.title"
            class="benefit-card"
          >
            <div class="benefit-icon">
              <i :class="item.icon"></i>
            </div>
            <div class="benefit-body">
              <h3>{{ item.title }}</h3>
              <p>{{ item.text }}</p>
            </div>
          </div>
        </div>

        <div class="types-strip">
          <template v-if="!currentVolunteerInfo">
            <div class="types-strip-label">Tipos de voluntariado</div>
            <div class="types-list">
              <div
                v-for="t in tiposDisponibles"
                :key="t.value"
                class="type-pill"
                :class="{ active: volunteerType === t.value }"
              >
                <span class="type-icon">{{ t.icon }}</span>
                {{ t.label }}
              </div>
            </div>
          </template>

          <template v-else>
            <div class="types-strip-label">{{ currentVolunteerInfo.title }}</div>
            <p class="vol-info-desc">{{ currentVolunteerInfo.description }}</p>
            <div class="vol-info-subtitle">Responsabilidades:</div>
            <ul class="vol-info-list">
              <li v-for="(r, idx) in currentVolunteerInfo.responsibilities" :key="idx">
                <i class='bx bx-check'></i>
                <span>{{ r }}</span>
              </li>
            </ul>
          </template>
        </div>
      </div>

      <!-- ───────────── FORM -->
      <div class="form-card">

        <!-- No logueado -->
        <div v-if="!loggedIn" class="state-box warning-box">
          <div class="state-icon">
            <i class='bx bxs-lock-alt'></i>
          </div>
          <h3>Inicia sesión primero</h3>
          <p>Debes tener una cuenta activa para enviar una solicitud de voluntariado.</p>
        </div>

        <!-- Ya tiene solicitud (y no eligió enviar una nueva tras ser aprobado) -->
        <div v-else-if="solicitudActual && !mostrarFormularioNuevo" class="state-box status-box">
          <div
            class="status-badge-large"
            :class="{
              'status-green':  solicitudActual.estado === 'Aprobada',
              'status-red':    solicitudActual.estado === 'Rechazada',
              'status-orange': solicitudActual.estado === 'Pendiente'
            }"
          >
            <i class='bx'
              :class="{
                'bxs-check-circle': solicitudActual.estado === 'Aprobada',
                'bxs-x-circle':     solicitudActual.estado === 'Rechazada',
                'bxs-time-five':    solicitudActual.estado === 'Pendiente'
              }"
            ></i>
          </div>
          <h3>Tu solicitud está en proceso</h3>

          <div class="status-row">
            <span class="status-label">Tipo:</span>
            <strong>{{ solicitudActual.tipo }}</strong>
          </div>
          <div class="status-row">
            <span class="status-label">Estado:</span>
            <span
              class="badge"
              :class="{
                'badge-green':  solicitudActual.estado === 'Aprobada',
                'badge-red':    solicitudActual.estado === 'Rechazada',
                'badge-orange': solicitudActual.estado === 'Pendiente'
              }"
            >{{ solicitudActual.estado }}</span>
          </div>

          <p class="status-msg" v-if="solicitudActual.estado === 'Pendiente'">
            Nuestro equipo está revisando tu solicitud. Pronto nos pondremos en contacto.
          </p>
          <p class="status-msg" v-if="solicitudActual.estado === 'Aprobada'">
            ¡Felicidades! Ya formas parte del equipo de voluntarios de Anhelo Pets.
          </p>
          <p class="status-msg" v-if="solicitudActual.estado === 'Rechazada'">
            Tu solicitud no fue aprobada en esta ocasión. Puedes contactarnos para más información.
          </p>

          <!-- Solo si ya fue aprobada: puede postularse a otro tipo de voluntariado -->
          <button
            v-if="solicitudActual.estado === 'Aprobada'"
            type="button"
            class="btn-nueva-solicitud"
            @click="iniciarNuevaSolicitud"
          >
            <i class='bx bx-plus'></i>
            Enviar otra solicitud de voluntariado
          </button>
        </div>

        <!-- Enviado con éxito -->
        <div v-else-if="submitted" class="state-box success-box">
          <div class="success-icon">
            <i class='bx bxs-check-circle'></i>
          </div>
          <h3>¡Solicitud enviada!</h3>
          <p>
            Gracias por querer formar parte de Anhelo Pets.
            Nuestro equipo revisará tu solicitud y se pondrá en contacto contigo.
          </p>
        </div>

        <!-- Formulario -->
        <div v-else class="form-body">
          <div class="form-header">
            <h2>Registro de voluntario</h2>
            <p>Completa el formulario y nos pondremos en contacto contigo.</p>
          </div>

          <!-- Sección: Datos personales -->
          <div class="form-section-title">
            <span class="section-dot"></span>
            Datos personales
          </div>

          <div class="form-row two-col">
            <div class="form-group">
              <label>Nombre completo</label>
              <input v-model="fullName" type="text" placeholder="María González" disabled>
            </div>
            <div class="form-group">
              <label>Cédula</label>
              <input v-model="idCard" type="text" placeholder="1-2345-6789">
            </div>
          </div>

          <div class="form-row two-col">
            <div class="form-group">
              <label>Correo electrónico</label>
              <input v-model="email" type="email" placeholder="correo@ejemplo.com" disabled>
            </div>
            <div class="form-group">
              <label>Teléfono</label>
              <div class="phone-wrap">
                <div class="phone-code-selector" ref="dropdownRef">
                  <button
                    type="button"
                    class="code-btn"
                    @click="showCodeDropdown = !showCodeDropdown"
                  >
                    <span>{{ selectedCountry.code }}</span>
                    <i class='bx bx-chevron-down'></i>
                  </button>
                  <div v-if="showCodeDropdown" class="code-dropdown">
                    <div class="dropdown-search-wrap">
                      <input v-model="codeSearch" type="text" placeholder="Buscar país...">
                    </div>
                    <div class="dropdown-list">
                      <div
                        v-for="item in filteredCodes"
                        :key="item.name"
                        class="country-option"
                        @click="selectCode(item)"
                      >
                        <span>{{ item.name }}</span>
                        <strong>{{ item.code }}</strong>
                      </div>
                    </div>
                  </div>
                </div>
                <input
                  v-model="phone"
                  type="text"
                  class="phone-number-input"
                  placeholder="88888888"
                  @input="filterPhoneInput"
                >
              </div>
            </div>
          </div>

          <!-- Sección: Ubicación -->
          <div class="form-section-title">
            <span class="section-dot"></span>
            Ubicación
          </div>

          <div class="form-row three-col">
            <div class="form-group">
              <label>Provincia</label>
              <div class="select-wrap">
                <select v-model="provincia">
                  <option value="">Seleccione</option>
                  <option v-for="p in provincias" :key="p" :value="p">{{ p }}</option>
                </select>
                <i class='bx bx-chevron-down'></i>
              </div>
            </div>
            <div class="form-group">
              <label>Cantón</label>
              <div class="select-wrap">
                <select v-model="canton" :disabled="!provincia">
                  <option value="">Seleccione</option>
                  <option v-for="c in cantones" :key="c" :value="c">{{ c }}</option>
                </select>
                <i class='bx bx-chevron-down'></i>
              </div>
            </div>
            <div class="form-group">
              <label>Distrito</label>
              <div class="select-wrap">
                <select v-model="distrito" :disabled="!canton">
                  <option value="">Seleccione</option>
                  <option v-for="d in distritos" :key="d" :value="d">{{ d }}</option>
                </select>
                <i class='bx bx-chevron-down'></i>
              </div>
            </div>
          </div>

          <!-- Sección: Tipo de voluntariado -->
          <div class="form-section-title">
            <span class="section-dot"></span>
            Tipo de voluntariado
          </div>

          <div v-if="tipoBloqueado" class="info-note">
            <i class='bx bxs-info-circle'></i>
            Ya tienes una solicitud aprobada como <strong>{{ tipoBloqueado }}</strong>. Puedes elegir otro tipo de apoyo.
          </div>

          <div class="type-selector-grid">
            <button
              v-for="t in tiposDisponibles"
              :key="t.value"
              type="button"
              class="type-btn"
              :class="{ selected: volunteerType === t.value }"
              @click="volunteerType = t.value"
            >
              <span class="type-btn-icon">{{ t.icon }}</span>
              <span class="type-btn-label">{{ t.label }}</span>
            </button>
          </div>

          <!-- ─── Campos dinámicos ─────────────────────── -->
          <Transition name="fields-fade">
            <div v-if="volunteerType" class="dynamic-fields">

              <div class="form-section-title">
                <span class="section-dot accent"></span>
                Información específica
              </div>

              <!-- ──────────── CASA CUNA -->
              <template v-if="volunteerType === 'Casa cuna'">
                <div class="form-row two-col">
                  <div class="form-group">
                    <label>Cantidad máxima de mascotas</label>
                    <input v-model="cc_maxMascotas" type="number" min="1" placeholder="Ej. 2">
                  </div>
                  <div class="form-group">
                    <label>Tipo de vivienda</label>
                    <div class="select-wrap">
                      <select v-model="cc_tipoVivienda">
                        <option value="">Seleccione</option>
                        <option>Casa</option>
                        <option>Apartamento</option>
                        <option>Finca</option>
                      </select>
                      <i class='bx bx-chevron-down'></i>
                    </div>
                  </div>
                </div>

                <div class="form-row three-col">
                  <div class="form-group">
                    <label>¿Tiene patio cerrado?</label>
                    <div class="radio-group">
                      <label class="radio-opt">
                        <input type="radio" v-model="cc_patioCerrado" value="Sí">
                        <span>Sí</span>
                      </label>
                      <label class="radio-opt">
                        <input type="radio" v-model="cc_patioCerrado" value="No">
                        <span>No</span>
                      </label>
                    </div>
                  </div>
                  <div class="form-group">
                    <label>¿Tiene otras mascotas?</label>
                    <div class="radio-group">
                      <label class="radio-opt">
                        <input type="radio" v-model="cc_otrasMascotas" value="Sí">
                        <span>Sí</span>
                      </label>
                      <label class="radio-opt">
                        <input type="radio" v-model="cc_otrasMascotas" value="No">
                        <span>No</span>
                      </label>
                    </div>
                  </div>
                  <div class="form-group">
                    <label>¿Hay niños en la vivienda?</label>
                    <div class="radio-group">
                      <label class="radio-opt">
                        <input type="radio" v-model="cc_ninos" value="Sí">
                        <span>Sí</span>
                      </label>
                      <label class="radio-opt">
                        <input type="radio" v-model="cc_ninos" value="No">
                        <span>No</span>
                      </label>
                    </div>
                  </div>
                </div>

                <div class="form-group">
                  <label>Tiempo estimado de disponibilidad</label>
                  <input v-model="cc_tiempoDisp" type="text" placeholder="Ej. 1 mes, indefinido...">
                </div>

                <div class="form-group">
                  <label>Puede recibir</label>
                  <div class="check-group">
                    <label
                      v-for="op in ['Cachorros','Adultos','Adultos mayores','Casos médicos']"
                      :key="op"
                      class="check-opt"
                    >
                      <input
                        type="checkbox"
                        :checked="cc_puedeRecibir.includes(op)"
                        @change="toggleCheck(cc_puedeRecibir, op)"
                      >
                      <span>{{ op }}</span>
                    </label>
                  </div>
                </div>

                <div class="form-group">
                  <label>Comentarios adicionales</label>
                  <textarea v-model="cc_comentarios" placeholder="Cuéntanos algo más sobre tu hogar..."></textarea>
                </div>
              </template>

              <!-- ──────────── EVENTOS DE ADOPCIÓN -->
              <template v-if="volunteerType === 'Eventos de adopción'">
                <div class="form-group">
                  <label>¿Ha participado antes en eventos?</label>
                  <div class="radio-group">
                    <label class="radio-opt">
                      <input type="radio" v-model="ev_participadoAntes" value="Sí">
                      <span>Sí</span>
                    </label>
                    <label class="radio-opt">
                      <input type="radio" v-model="ev_participadoAntes" value="No">
                      <span>No</span>
                    </label>
                  </div>
                </div>

                <div class="form-group">
                  <label>Experiencia en atención al público</label>
                  <textarea v-model="ev_experienciaPublico" placeholder="Describe tu experiencia..."></textarea>
                </div>

                <div class="form-row two-col">
                  <div class="form-group">
                    <label>Disponibilidad</label>
                    <div class="check-group">
                      <label
                        v-for="op in ['Sábados','Domingos','Entre semana']"
                        :key="op"
                        class="check-opt"
                      >
                        <input
                          type="checkbox"
                          :checked="ev_disponibilidad.includes(op)"
                          @change="toggleCheck(ev_disponibilidad, op)"
                        >
                        <span>{{ op }}</span>
                      </label>
                    </div>
                  </div>
                  <div class="form-group">
                    <label>Horario disponible</label>
                    <input v-model="ev_horario" type="text" placeholder="Ej. 8am – 2pm">
                  </div>
                </div>

                <div class="form-group">
                  <label>Habilidades</label>
                  <div class="check-group wrap">
                    <label
                      v-for="op in ['Atención al público','Organización','Fotografía','Manejo de mascotas']"
                      :key="op"
                      class="check-opt"
                    >
                      <input
                        type="checkbox"
                        :checked="ev_habilidades.includes(op)"
                        @change="toggleCheck(ev_habilidades, op)"
                      >
                      <span>{{ op }}</span>
                    </label>
                  </div>
                </div>

                <div class="form-group">
                  <label>¿Tiene transporte propio?</label>
                  <div class="radio-group">
                    <label class="radio-opt">
                      <input type="radio" v-model="ev_transportePropio" value="Sí">
                      <span>Sí</span>
                    </label>
                    <label class="radio-opt">
                      <input type="radio" v-model="ev_transportePropio" value="No">
                      <span>No</span>
                    </label>
                  </div>
                </div>
              </template>

              <!-- ──────────── TRANSPORTE -->
              <template v-if="volunteerType === 'Transporte'">
                <div class="form-row two-col">
                  <div class="form-group">
                    <label>Tipo de vehículo</label>
                    <div class="select-wrap">
                      <select v-model="tr_tipoVehiculo">
                        <option value="">Seleccione</option>
                        <option>Carro</option>
                        <option>Moto</option>
                        <option>Pick-up</option>
                        <option>SUV</option>
                      </select>
                      <i class='bx bx-chevron-down'></i>
                    </div>
                  </div>
                  <div class="form-group">
                    <label>Cobertura</label>
                    <div class="select-wrap">
                      <select v-model="tr_cobertura">
                        <option value="">Seleccione</option>
                        <option>Cantón</option>
                        <option>Provincia</option>
                        <option>Todo el país</option>
                      </select>
                      <i class='bx bx-chevron-down'></i>
                    </div>
                  </div>
                </div>

                <div class="form-row two-col">
                  <div class="form-group">
                    <label>Disponibilidad</label>
                    <div class="check-group">
                      <label
                        v-for="op in ['Mañanas','Tardes','Noches','Emergencias']"
                        :key="op"
                        class="check-opt"
                      >
                        <input
                          type="checkbox"
                          :checked="tr_disponibilidad.includes(op)"
                          @change="toggleCheck(tr_disponibilidad, op)"
                        >
                        <span>{{ op }}</span>
                      </label>
                    </div>
                  </div>
                  <div class="form-group">
                    <label>¿Licencia vigente?</label>
                    <div class="radio-group">
                      <label class="radio-opt">
                        <input type="radio" v-model="tr_licencia" value="Sí">
                        <span>Sí</span>
                      </label>
                      <label class="radio-opt">
                        <input type="radio" v-model="tr_licencia" value="No">
                        <span>No</span>
                      </label>
                    </div>
                  </div>
                </div>

                <div class="form-group">
                  <label>Puede transportar</label>
                  <div class="check-group wrap">
                    <label
                      v-for="op in ['Mascotas pequeñas','Mascotas medianas','Mascotas grandes','Traslados veterinarios']"
                      :key="op"
                      class="check-opt"
                    >
                      <input
                        type="checkbox"
                        :checked="tr_puedeTransp.includes(op)"
                        @change="toggleCheck(tr_puedeTransp, op)"
                      >
                      <span>{{ op }}</span>
                    </label>
                  </div>
                </div>
              </template>

              <!-- ──────────── VETERINARIA -->
              <template v-if="volunteerType === 'Veterinaria'">
                <div class="form-row two-col">
                  <div class="form-group">
                    <label>Profesión</label>
                    <div class="select-wrap">
                      <select v-model="vet_profesion">
                        <option value="">Seleccione</option>
                        <option>Médico veterinario</option>
                        <option>Estudiante</option>
                        <option>Asistente veterinario</option>
                      </select>
                      <i class='bx bx-chevron-down'></i>
                    </div>
                  </div>
                  <div class="form-group">
                    <label>Número de colegiado <span class="optional">(si aplica)</span></label>
                    <input v-model="vet_colegiado" type="text" placeholder="Ej. 1234">
                  </div>
                </div>

                <div class="form-row two-col">
                  <div class="form-group">
                    <label>Especialidades</label>
                    <div class="check-group">
                      <label
                        v-for="op in ['Medicina general','Cirugía','Emergencias','Rehabilitación','Dermatología']"
                        :key="op"
                        class="check-opt"
                      >
                        <input
                          type="checkbox"
                          :checked="vet_especialidades.includes(op)"
                          @change="toggleCheck(vet_especialidades, op)"
                        >
                        <span>{{ op }}</span>
                      </label>
                    </div>
                  </div>
                  <div class="form-group">
                    <label>Disponibilidad</label>
                    <div class="check-group">
                      <label
                        v-for="op in ['Consultas','Esterilizaciones','Emergencias']"
                        :key="op"
                        class="check-opt"
                      >
                        <input
                          type="checkbox"
                          :checked="vet_disponibilidad.includes(op)"
                          @change="toggleCheck(vet_disponibilidad, op)"
                        >
                        <span>{{ op }}</span>
                      </label>
                    </div>
                  </div>
                </div>

                <div class="form-group">
                  <label>Clínica donde trabaja <span class="optional">(opcional)</span></label>
                  <input v-model="vet_clinica" type="text" placeholder="Nombre de la clínica">
                </div>
              </template>

              <!-- ──────────── REDES SOCIALES -->
              <template v-if="volunteerType === 'Redes sociales'">
                <div class="form-row two-col">
                  <div class="form-group">
                    <label>Red principal</label>
                    <div class="select-wrap">
                      <select v-model="rs_red">
                        <option value="">Seleccione</option>
                        <option>Instagram</option>
                        <option>Facebook</option>
                        <option>TikTok</option>
                        <option>X</option>
                      </select>
                      <i class='bx bx-chevron-down'></i>
                    </div>
                  </div>
                  <div class="form-group">
                    <label>Horas disponibles por semana</label>
                    <input v-model="rs_horasSemanales" type="number" min="1" placeholder="Ej. 5">
                  </div>
                </div>

                <div class="form-row two-col">
                  <div class="form-group">
                    <label>Experiencia</label>
                    <div class="check-group">
                      <label
                        v-for="op in ['Diseño gráfico','Fotografía','Video','Copywriting','Community Manager']"
                        :key="op"
                        class="check-opt"
                      >
                        <input
                          type="checkbox"
                          :checked="rs_experiencia.includes(op)"
                          @change="toggleCheck(rs_experiencia, op)"
                        >
                        <span>{{ op }}</span>
                      </label>
                    </div>
                  </div>
                  <div class="form-group">
                    <label>Programas que maneja</label>
                    <div class="check-group">
                      <label
                        v-for="op in ['Canva','Photoshop','CapCut','Illustrator']"
                        :key="op"
                        class="check-opt"
                      >
                        <input
                          type="checkbox"
                          :checked="rs_programas.includes(op)"
                          @change="toggleCheck(rs_programas, op)"
                        >
                        <span>{{ op }}</span>
                      </label>
                    </div>
                  </div>
                </div>

                <div class="form-group">
                  <label>Portafolio o perfil <span class="optional">(URL)</span></label>
                  <input v-model="rs_portafolio" type="url" placeholder="https://...">
                </div>
              </template>

              <!-- ──────────── RESCATISTA -->
              <template v-if="volunteerType === 'Rescatista'">
                <div class="form-row two-col">
                  <div class="form-group">
                    <label>Años de experiencia</label>
                    <input v-model="re_anosExp" type="number" min="0" placeholder="Ej. 3">
                  </div>
                  <div class="form-group">
                    <label>Cantidad aproximada de rescates</label>
                    <input v-model="re_cantRescates" type="number" min="0" placeholder="Ej. 20">
                  </div>
                </div>

                <div class="form-row two-col">
                  <div class="form-group">
                    <label>Equipo disponible</label>
                    <div class="check-group">
                      <label
                        v-for="op in ['Transportadora','Correas','Jaulas trampa','Botiquín']"
                        :key="op"
                        class="check-opt"
                      >
                        <input
                          type="checkbox"
                          :checked="re_equipo.includes(op)"
                          @change="toggleCheck(re_equipo, op)"
                        >
                        <span>{{ op }}</span>
                      </label>
                    </div>
                  </div>
                  <div class="form-group">
                    <label>Disponibilidad</label>
                    <div class="select-wrap">
                      <select v-model="re_disponibilidad">
                        <option value="">Seleccione</option>
                        <option>Emergencias 24/7</option>
                        <option>Solo fines de semana</option>
                        <option>Entre semana</option>
                      </select>
                      <i class='bx bx-chevron-down'></i>
                    </div>
                  </div>
                </div>

                <div class="form-row two-col">
                  <div class="form-group">
                    <label>Zona de cobertura – Provincia</label>
                    <div class="select-wrap">
                      <select v-model="re_zonaProvincia">
                        <option value="">Seleccione</option>
                        <option v-for="p in provincias" :key="p" :value="p">{{ p }}</option>
                      </select>
                      <i class='bx bx-chevron-down'></i>
                    </div>
                  </div>
                  <div class="form-group">
                    <label>Zona de cobertura – Cantón</label>
                    <div class="select-wrap">
                      <select v-model="re_zonaCanton" :disabled="!re_zonaProvincia">
                        <option value="">Seleccione</option>
                        <option v-for="c in re_zonaCantones" :key="c" :value="c">{{ c }}</option>
                      </select>
                      <i class='bx bx-chevron-down'></i>
                    </div>
                  </div>
                </div>

                <div class="form-group">
                  <label>¿Tiene capacitación en manejo animal?</label>
                  <div class="radio-group">
                    <label class="radio-opt">
                      <input type="radio" v-model="re_capacitacion" value="Sí">
                      <span>Sí</span>
                    </label>
                    <label class="radio-opt">
                      <input type="radio" v-model="re_capacitacion" value="No">
                      <span>No</span>
                    </label>
                  </div>
                </div>
              </template>

            </div>
          </Transition>

          <!-- SUBMIT -->
          <p v-if="errorEnvio" class="submit-error">{{ errorEnvio }}</p>

          <button
            class="submit-btn"
            :disabled="!formValid || enviandoSolicitud"
            @click="submitVolunteer"
          >
            <i class='bx bxs-heart'></i>
            {{ enviandoSolicitud ? 'Enviando...' : 'Registrarme como voluntario' }}
          </button>

        </div><!-- /form-body -->
      </div><!-- /form-card -->
    </div>
  </section>

  <FooterBar />
</template>

<style scoped>

/* ═══════════════════════════════════════════════════════
   HERO — se conserva la foto, dimensiones y overlay original.
   Solo se refina tipografía, badge y detalles de los stats.
══════════════════════════════════════════════════════ */

.hero {
  position: relative;
  height: 430px;
  overflow: hidden;
  background: #1e2a1f;
}

.hero-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
  object-position: center 52%;
  filter: brightness(0.72) contrast(1.05);
  transform: scale(1.04);
}

.hero-overlay {
  position: absolute;
  inset: 0;
  background: linear-gradient(
    90deg,
    rgba(0,0,0,0.74) 0%,
    rgba(0,0,0,0.46) 35%,
    rgba(0,0,0,0.14) 70%,
    rgba(0,0,0,0) 100%
  );
}

.hero-content {
  position: absolute;
  left: 7%;
  bottom: 55px;
  max-width: 560px;
  z-index: 2;
}

.hero-inner {
  max-width: 560px;
  color: white;
}

.hero-badge {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  background: rgba(249,193,122,0.16);
  border: 1px solid rgba(249,193,122,0.38);
  color: #F9C17A;
  padding: 7px 16px;
  border-radius: 999px;
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 1.4px;
  text-transform: uppercase;
  margin-bottom: 22px;
  backdrop-filter: blur(2px);
}

.hero-badge i { font-size: 14px; }

.hero-inner h1 {
  font-size: 52px;
  line-height: 0.98;
  font-weight: 800;
  letter-spacing: -2.6px;
  color: white;
  margin: 0 0 22px;
  text-shadow: 0 2px 24px rgba(0,0,0,0.25);
}

.hero-accent { color: #F9C17A; }

.hero-inner > p {
  font-size: 15px;
  line-height: 1.75;
  color: rgba(255,255,255,0.90);
  max-width: 420px;
  margin-bottom: 0;
}

.hero-stats {
  display: flex;
  align-items: center;
  gap: 26px;
  margin-top: 30px;
  padding-top: 26px;
  border-top: 1px solid rgba(255,255,255,0.14);
}

.stat {
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.stat strong {
  font-size: 27px;
  font-weight: 800;
  color: #fff;
  line-height: 1;
  letter-spacing: -0.5px;
}

.stat span {
  font-size: 11.5px;
  color: rgba(255,255,255,0.62);
  font-weight: 600;
  letter-spacing: 0.03em;
}

.stat-divider {
  width: 1px;
  height: 34px;
  background: rgba(255,255,255,0.16);
}

/* ═══════════════════════════════════════════════════════
   MAIN SECTION
══════════════════════════════════════════════════════ */
.main-section {
  background:
    radial-gradient(ellipse 900px 500px at 15% 0%, rgba(146,168,148,0.06), transparent),
    #FAFAF8;
  padding: 96px 24px 130px;
}

.container {
  max-width: 1200px;
  margin: 0 auto;
}

.vol-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 26px;
  align-items: start;
}

/* ═══════════════════════════════════════════════════════
   LEFT COL
══════════════════════════════════════════════════════ */
.left-col {
  display: flex;
  flex-direction: column;
  gap: 22px;
}

/* INTRO CARD */
.intro-card {
  background: white;
  border-radius: 26px;
  overflow: hidden;
  border: 1px solid rgba(146,168,148,0.14);
  box-shadow: 0 1px 2px rgba(58,71,60,0.03), 0 14px 34px -12px rgba(58,71,60,0.10);
}

.intro-img-wrap {
  height: 230px;
  overflow: hidden;
  position: relative;
}

.intro-img-wrap::after {
  content: '';
  position: absolute;
  inset: 0;
  background: linear-gradient(180deg, rgba(0,0,0,0) 55%, rgba(20,26,20,0.28) 100%);
}

.intro-img-wrap img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.6s cubic-bezier(0.16,1,0.3,1);
}

.intro-card:hover .intro-img-wrap img {
  transform: scale(1.045);
}

.intro-body {
  padding: 30px 32px 32px;
}

.intro-label {
  display: inline-block;
  background: #E7F1E8;
  color: #3A473C;
  font-size: 11px;
  font-weight: 800;
  letter-spacing: 0.1em;
  text-transform: uppercase;
  padding: 5px 12px;
  border-radius: 999px;
  margin-bottom: 14px;
}

.intro-body h2 {
  font-size: 32px;
  font-weight: 800;
  color: #3A473C;
  line-height: 1.12;
  letter-spacing: -0.8px;
  margin: 0 0 12px;
}

.intro-body p {
  font-size: 14.5px;
  line-height: 1.85;
  color: #6C756D;
}

/* BENEFITS GRID */
.benefits-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 14px;
}

.benefit-card {
  background: white;
  border-radius: 20px;
  padding: 20px;
  display: flex;
  gap: 14px;
  border: 1px solid rgba(146,168,148,0.14);
  box-shadow: 0 1px 2px rgba(58,71,60,0.02), 0 8px 20px -10px rgba(58,71,60,0.08);
  transition: box-shadow 0.25s ease, transform 0.25s ease, border-color 0.25s ease;
}

.benefit-card:hover {
  box-shadow: 0 1px 2px rgba(58,71,60,0.03), 0 16px 32px -12px rgba(58,71,60,0.14);
  transform: translateY(-3px);
  border-color: rgba(146,168,148,0.28);
}

.benefit-icon {
  width: 46px;
  height: 46px;
  min-width: 46px;
  border-radius: 50%;
  background: #E7F1E8;
  display: flex;
  align-items: center;
  justify-content: center;
}

.benefit-icon i {
  font-size: 20px;
  color: #3A473C;
}

.benefit-body h3 {
  font-size: 14.5px;
  font-weight: 800;
  color: #3A473C;
  margin: 0 0 6px;
  letter-spacing: -0.1px;
}

.benefit-body p {
  font-size: 12.5px;
  line-height: 1.7;
  color: #6C756D;
  margin: 0;
}

/* TYPES STRIP */
.types-strip {
  background: white;
  border-radius: 22px;
  padding: 24px 26px;
  border: 1px solid rgba(146,168,148,0.14);
  box-shadow: 0 1px 2px rgba(58,71,60,0.02), 0 8px 20px -10px rgba(58,71,60,0.08);
}

.types-strip-label {
  font-size: 11px;
  font-weight: 800;
  letter-spacing: 0.1em;
  text-transform: uppercase;
  color: #3A473C;
  margin-bottom: 16px;
}

.types-list {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.type-pill {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 8px 15px;
  border-radius: 999px;
  background: #F4F7F4;
  font-size: 12.5px;
  font-weight: 600;
  color: #6C756D;
  border: 1.5px solid transparent;
  transition: all 0.2s;
}

.type-pill.active {
  background: #E7F1E8;
  color: #3A473C;
  border-color: #3A473C;
}

.type-icon { font-size: 15px; }

/* TYPES STRIP — INFO DINÁMICA */
.vol-info-desc {
  font-size: 13px;
  line-height: 1.75;
  color: #6C756D;
  margin: 0 0 18px;
}

.vol-info-subtitle {
  font-size: 11px;
  font-weight: 800;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: #3A473C;
  margin-bottom: 12px;
}

.vol-info-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.vol-info-list li {
  display: flex;
  align-items: flex-start;
  gap: 10px;
  font-size: 13px;
  color: #3A473C;
  font-weight: 600;
  line-height: 1.55;
}

.vol-info-list li i {
  width: 20px;
  height: 20px;
  min-width: 20px;
  border-radius: 50%;
  background: #E7F1E8;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  color: #5A8060;
  font-size: 13px;
  margin-top: 1px;
}

/* ═══════════════════════════════════════════════════════
   FORM CARD
══════════════════════════════════════════════════════ */
.form-card {
  background: white;
  border-radius: 28px;
  padding: 38px 36px;
  border: 1px solid rgba(146,168,148,0.14);
  box-shadow: 0 1px 2px rgba(58,71,60,0.03), 0 20px 44px -16px rgba(58,71,60,0.14);
  position: sticky;
  /* Espacio suficiente para que la tarjeta no quede tapada por el NavBar
     fijo al hacer scroll. Ajusta este valor a la altura real de tu NavBar
     más el espacio de aire que quieras dejar (por defecto: navbar ~80px + 24px de aire). */
  top: 104px;
  z-index: 5;
}

/* ─── STATE BOXES ───────────────────────── */
.state-box {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  padding: 50px 20px;
  gap: 14px;
}

.state-box h3 {
  font-size: 24px;
  font-weight: 800;
  color: #3A473C;
  margin: 0;
  letter-spacing: -0.4px;
}

.state-box p {
  color: #6C756D;
  line-height: 1.8;
  max-width: 340px;
  margin: 0;
  font-size: 13.5px;
}

.state-icon {
  width: 68px;
  height: 68px;
  border-radius: 22px;
  background: #FFF1DD;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 6px;
}

.state-icon i {
  font-size: 30px;
  color: #F9C17A;
}

.success-icon {
  width: 68px;
  height: 68px;
  border-radius: 50%;
  background: #E7F1E8;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 4px;
}

.success-icon i {
  font-size: 32px;
  color: #3A473C;
}

/* Status */
.status-badge-large {
  width: 76px;
  height: 76px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 6px;
}

.status-badge-large i { font-size: 34px; }

.status-green  { background: rgba(146,168,148,0.16); }
.status-green i { color: #5A8060; }
.status-red    { background: rgba(235,119,119,0.13); }
.status-red i  { color: #C45252; }
.status-orange { background: rgba(249,193,122,0.18); }
.status-orange i { color: #D18C3A; }

.status-row {
  display: flex;
  align-items: center;
  gap: 10px;
}

.status-label {
  font-size: 13.5px;
  color: #6C756D;
}

.badge {
  display: inline-block;
  padding: 5px 14px;
  border-radius: 999px;
  font-size: 12.5px;
  font-weight: 700;
}

.badge-green  { background: rgba(146,168,148,0.18); color: #4E5F50; }
.badge-red    { background: rgba(235,119,119,0.15); color: #C45252; }
.badge-orange { background: rgba(249,193,122,0.20); color: #C67B26; }

.status-msg {
  font-size: 13.5px;
  color: #6C756D;
  line-height: 1.75;
}

/* Botón "enviar otra solicitud" (solo tras aprobación) */
.btn-nueva-solicitud {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  margin-top: 10px;
  padding: 12px 22px;
  border-radius: 14px;
  border: 1.5px solid #3A473C;
  background: transparent;
  color: #3A473C;
  font-size: 13.5px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s ease;
  font-family: inherit;
}

.btn-nueva-solicitud:hover {
  background: #3A473C;
  color: white;
}

.btn-nueva-solicitud i { font-size: 16px; }

/* ─── FORM BODY ───────────────────────── */
.form-header {
  margin-bottom: 30px;
}

.form-header h2 {
  font-size: 30px;
  font-weight: 800;
  color: #3A473C;
  margin: 0 0 8px;
  letter-spacing: -0.7px;
}

.form-header p {
  color: #6C756D;
  font-size: 13.5px;
  line-height: 1.7;
}

/* SECTION TITLE */
.form-section-title {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 11.5px;
  font-weight: 800;
  letter-spacing: 0.1em;
  text-transform: uppercase;
  color: #3A473C;
  margin: 10px 0 18px;
}

.section-dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  background: #3A473C;
  flex-shrink: 0;
  box-shadow: 0 0 0 4px rgba(58,71,60,0.08);
}

.section-dot.accent {
  background: #F9C17A;
  box-shadow: 0 0 0 4px rgba(249,193,122,0.18);
}

/* NOTA: tipo bloqueado por aprobación previa */
.info-note {
  display: flex;
  align-items: flex-start;
  gap: 9px;
  background: #FFF6E9;
  border: 1px solid rgba(249,193,122,0.35);
  border-radius: 14px;
  padding: 12px 16px;
  font-size: 12.5px;
  color: #6C756D;
  line-height: 1.6;
  margin-bottom: 14px;
}

.info-note i {
  font-size: 16px;
  color: #D18C3A;
  flex-shrink: 0;
  margin-top: 1px;
}

.info-note strong {
  color: #3A473C;
}

/* FORM LAYOUT */
.form-row {
  display: grid;
  gap: 16px;
  margin-bottom: 0;
}

.two-col   { grid-template-columns: 1fr 1fr; }
.three-col { grid-template-columns: 1fr 1fr 1fr; }

.form-group {
  display: flex;
  flex-direction: column;
  margin-bottom: 18px;
}

.form-group label {
  font-size: 12.5px;
  font-weight: 700;
  color: #3A473C;
  margin-bottom: 8px;
}

.optional {
  font-weight: 500;
  color: #9BA99C;
}

/* INPUTS */
.form-group input[type="text"],
.form-group input[type="email"],
.form-group input[type="number"],
.form-group input[type="url"],
.form-group textarea {
  width: 100%;
  box-sizing: border-box;
  height: 48px;
  border: 1.5px solid #E5ECE6;
  border-radius: 14px;
  padding: 0 16px;
  background: #FBFBF9;
  font-size: 13.5px;
  color: #3A473C;
  outline: none;
  transition: border-color 0.2s, box-shadow 0.2s, background 0.2s;
  font-family: inherit;
}

.form-group input:focus,
.form-group textarea:focus {
  border-color: #6E8870;
  background: #fff;
  box-shadow: 0 0 0 4px rgba(110,136,112,0.12);
}

.form-group input:disabled {
  background: #F4F6F4;
  color: #9BA99C;
  cursor: not-allowed;
}

.form-group textarea {
  height: auto;
  min-height: 104px;
  padding: 14px 16px;
  resize: vertical;
  line-height: 1.6;
}

/* PHONE */
.phone-wrap {
  display: flex;
  gap: 10px;
}

.phone-code-selector {
  position: relative;
  flex-shrink: 0;
}

.code-btn {
  height: 48px;
  width: 94px;
  padding: 0 12px;
  border-radius: 14px;
  border: 1.5px solid #E5ECE6;
  background: #FBFBF9;
  display: flex;
  align-items: center;
  justify-content: space-between;
  color: #3A473C;
  font-weight: 700;
  font-size: 13px;
  cursor: pointer;
  transition: border-color 0.2s, background 0.2s;
  font-family: inherit;
}

.code-btn:hover { border-color: #6E8870; background: #fff; }

.phone-number-input {
  flex: 1;
  min-width: 0;
}

.code-dropdown {
  position: absolute;
  top: 54px;
  left: 0;
  width: 260px;
  background: white;
  border-radius: 18px;
  border: 1.5px solid #E5ECE6;
  box-shadow: 0 20px 44px -8px rgba(58,71,60,0.22);
  overflow: hidden;
  z-index: 1000;
}

.dropdown-search-wrap { padding: 12px; }

.dropdown-search-wrap input {
  width: 100%;
  box-sizing: border-box;
  height: 40px;
  border: 1.5px solid #E5ECE6;
  border-radius: 12px;
  padding: 0 12px;
  font-size: 13px;
  outline: none;
  font-family: inherit;
}

.dropdown-search-wrap input:focus {
  border-color: #6E8870;
}

.dropdown-list {
  max-height: 226px;
  overflow-y: auto;
}

.country-option {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 11px 16px;
  cursor: pointer;
  transition: background 0.15s;
}

.country-option:hover { background: #F4F7F4; }

.country-option span {
  font-size: 13px;
  color: #3A473C;
}

.country-option strong {
  font-size: 13px;
  font-weight: 600;
  color: #6C756D;
}

/* SELECT */
.select-wrap { position: relative; }

.select-wrap select {
  width: 100%;
  box-sizing: border-box;
  height: 48px;
  border: 1.5px solid #E5ECE6;
  border-radius: 14px;
  padding: 0 42px 0 16px;
  background: #FBFBF9;
  font-size: 13.5px;
  color: #3A473C;
  outline: none;
  appearance: none;
  cursor: pointer;
  transition: border-color 0.2s, box-shadow 0.2s, background 0.2s;
  font-family: inherit;
}

.select-wrap select:focus {
  border-color: #6E8870;
  background: #fff;
  box-shadow: 0 0 0 4px rgba(110,136,112,0.12);
}

.select-wrap select:disabled {
  background: #F4F6F4;
  color: #9BA99C;
  cursor: not-allowed;
}

.select-wrap i {
  position: absolute;
  right: 14px;
  top: 50%;
  transform: translateY(-50%);
  font-size: 19px;
  color: #6C756D;
  pointer-events: none;
}

/* TYPE SELECTOR */
.type-selector-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 10px;
  margin-bottom: 10px;
}

.type-btn {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 7px;
  padding: 16px 8px;
  border-radius: 18px;
  border: 1.5px solid #E5ECE6;
  background: #FBFBF9;
  cursor: pointer;
  transition: all 0.2s;
  font-family: inherit;
}

.type-btn:hover {
  border-color: #6E8870;
  background: #F4F7F4;
}

.type-btn.selected {
  border-color: #3A473C;
  background: #E7F1E8;
  box-shadow: 0 4px 14px -4px rgba(58,71,60,0.22);
}

.type-btn-icon { font-size: 22px; line-height: 1; }

.type-btn-label {
  font-size: 11.5px;
  font-weight: 700;
  color: #3A473C;
  text-align: center;
  line-height: 1.3;
}

/* DYNAMIC FIELDS TRANSITION */
.fields-fade-enter-active,
.fields-fade-leave-active {
  transition: opacity 0.3s ease, transform 0.3s ease;
}

.fields-fade-enter-from,
.fields-fade-leave-to {
  opacity: 0;
  transform: translateY(8px);
}

.dynamic-fields {
  border-top: 1.5px solid #E7F1E8;
  margin-top: 10px;
  padding-top: 6px;
}

/* RADIO & CHECK */
.radio-group,
.check-group {
  display: flex;
  flex-direction: column;
  gap: 9px;
  margin-top: 2px;
}

.check-group.wrap {
  flex-direction: row;
  flex-wrap: wrap;
  gap: 8px;
}

.radio-opt,
.check-opt {
  display: flex;
  align-items: center;
  gap: 9px;
  cursor: pointer;
  font-size: 13px;
  color: #3A473C;
  font-weight: 600;
}

.radio-opt input[type="radio"],
.check-opt input[type="checkbox"] {
  width: 17px;
  height: 17px;
  accent-color: #3A473C;
  cursor: pointer;
  flex-shrink: 0;
}

.check-group.wrap .check-opt {
  background: #F4F7F4;
  border: 1.5px solid #E5ECE6;
  border-radius: 11px;
  padding: 7px 13px;
  transition: all 0.15s;
}

.check-group.wrap .check-opt:has(input:checked) {
  background: #E7F1E8;
  border-color: #3A473C;
}

/* SUBMIT */
.submit-error {
  color: #C45252;
  background: rgba(235,119,119,0.13);
  border-radius: 10px;
  padding: 10px 14px;
  font-size: 13px;
  font-weight: 600;
  margin-bottom: 14px;
  text-align: center;
}

.submit-btn {
  width: 100%;
  height: 54px;
  border: none;
  border-radius: 17px;
  background: linear-gradient(135deg, #3A473C 0%, #6E8870 100%);
  color: white;
  font-size: 14.5px;
  font-weight: 700;
  font-family: inherit;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  margin-top: 12px;
  transition: opacity 0.2s, transform 0.15s, box-shadow 0.2s;
  box-shadow: 0 10px 26px -8px rgba(58,71,60,0.42);
}

.submit-btn:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 14px 32px -8px rgba(58,71,60,0.48);
}

.submit-btn:disabled {
  opacity: 0.38;
  cursor: not-allowed;
  box-shadow: none;
}

.submit-btn i { font-size: 18px; }

/* ═══════════════════════════════════════════════════════
   RESPONSIVE
══════════════════════════════════════════════════════ */
@media (max-width: 1100px) {
  .vol-grid { grid-template-columns: 1fr; }
  .form-card { position: static; }
}

@media (max-width: 720px) {
  .hero { height: 520px; }

  .hero-inner h1 {
    font-size: 40px;
    letter-spacing: -1.5px;
  }

  .hero-stats { gap: 16px; }
  .stat strong { font-size: 22px; }

  .main-section { padding: 56px 16px 80px; }

  .benefits-grid { grid-template-columns: 1fr; }

  .two-col,
  .three-col { grid-template-columns: 1fr; }

  .type-selector-grid { grid-template-columns: repeat(2, 1fr); }

  .form-card { padding: 26px 20px; }

  .code-dropdown { width: 100%; left: 0; }
}

@media (max-width: 400px) {
  .type-selector-grid { grid-template-columns: 1fr 1fr; }
}

/* ── MOBILE RESPONSIVE adicional ── */
@media (max-width: 768px) {
  .hero { height: 400px; }

  .hero-content {
    left: 16px;
    right: 16px;
    bottom: 36px;
    max-width: 100%;
  }

  .hero-inner h1 {
    font-size: 36px;
    letter-spacing: -1.5px;
    margin: 0 0 16px;
  }

  .hero-inner > p { font-size: 14px; }

  .hero-stats {
    gap: 12px;
    margin-top: 20px;
    padding-top: 18px;
    flex-wrap: wrap;
  }

  .stat strong { font-size: 22px; }
  .stat span { font-size: 11px; }

  .main-section { padding: 48px 16px 72px; }

  .intro-body { padding: 24px 20px 26px; }
  .intro-body h2 { font-size: 25px; }

  .benefits-grid {
    grid-template-columns: 1fr;
    gap: 12px;
  }

  .form-card {
    padding: 24px 18px;
    border-radius: 24px;
    position: static;
  }

  .form-header h2 { font-size: 24px; }

  .two-col,
  .three-col {
    grid-template-columns: 1fr;
    gap: 0;
  }

  .type-selector-grid {
    grid-template-columns: repeat(2, 1fr);
    gap: 8px;
  }

  .type-btn { padding: 13px 6px; }
  .type-btn-label { font-size: 11px; }

  .submit-btn {
    height: 50px;
    font-size: 14px;
  }

  .state-box { padding: 38px 16px; }
  .state-box h3 { font-size: 21px; }

  .code-dropdown {
    width: calc(100vw - 48px);
    max-width: 280px;
  }
}

@media (max-width: 400px) {
  .hero-inner h1 { font-size: 30px; }
  .type-selector-grid { grid-template-columns: 1fr 1fr; }
}

</style>