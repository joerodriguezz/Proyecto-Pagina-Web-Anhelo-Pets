<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import NavBar from '../components/NavBar.vue'
import FooterBar from '../components/FooterBar.vue'
import { getAnimals, mapDtoToPet } from '../services/petServices.js'

const router = useRouter()

const filterType   = ref('Todos')
const filterSex    = ref('Todos')
const filterStatus = ref('Todos')
const searchQuery  = ref('')

const pets = ref([])
const isLoading = ref(false)
const error = ref('')

// Estados visibles al público: "Inactiva" (archivada) y "En rescate"
// (recién ingresada, aún sin evaluar) son internos y nunca deben mostrarse.
const PUBLIC_STATUSES = ['Disponible', 'En proceso', 'Adoptada']

const filtered = computed(() =>
  pets.value.filter(pet => {
    if (pet.status === 'Adoptada') return false // esas van en "Historias felices", no en el catálogo
    const matchType   = filterType.value   === 'Todos' || pet.type === filterType.value
    const matchSex    = filterSex.value    === 'Todos' || pet.sex  === filterSex.value
    const matchStatus = filterStatus.value === 'Todos' || pet.status === filterStatus.value
    const search      = searchQuery.value.toLowerCase().trim()
    const matchSearch = !search ||
      pet.name.toLowerCase().includes(search) ||
      pet.type.toLowerCase().includes(search) ||
      (pet.description || '').toLowerCase().includes(search)
    return matchType && matchSex && matchStatus && matchSearch
  })
)

const adoptedPets = computed(() => pets.value.filter(pet => pet.status === 'Adoptada'))

function buildQueryParams() {
  // status:'Todos' siempre — el filtro de estado (chip Disponible/En proceso)
  // se aplica en el cliente, porque el backend por defecto solo trae
  // "Disponible" y eso ocultaba "En proceso" (y "Adoptada") incluso con
  // el chip "Todos" seleccionado.
  const params = { status: 'Todos' }

  if (filterType.value !== 'Todos') params.type = filterType.value
  if (filterSex.value !== 'Todos') params.sex = filterSex.value
  if (searchQuery.value.trim()) params.search = searchQuery.value.trim()

  return params
}

async function loadAnimals() {
  isLoading.value = true
  error.value = ''

  try {
    const response = await getAnimals(buildQueryParams())
    const todas = (response.data || []).map(mapDtoToPet)
    pets.value = todas.filter(pet => PUBLIC_STATUSES.includes(pet.status))
  } catch (err) {
    console.error('Error cargando mascotas:', err)
    error.value = 'No se pudieron cargar las mascotas.'
    pets.value = []
  } finally {
    isLoading.value = false
  }
}

onMounted(loadAnimals)

watch([filterType, filterSex, filterStatus, searchQuery], loadAnimals)

const statusColor = status => ({
  'Disponible': 'badge-green',
  'En proceso': 'badge-yellow',
}[status] || 'badge-gray')

function mainImage(pet) {
  if (pet.images && pet.images.length > 0) return pet.images[0].preview
  return pet.image || ''
}

function goAdopt(pet) {
  router.push({ name: 'adoptar', params: { id: pet.id }, query: { name: pet.name } })
}

function clearFilters() {
  filterType.value = 'Todos'
  filterSex.value  = 'Todos'
  filterStatus.value = 'Todos'
  searchQuery.value  = ''
}
</script>

<template>
  <NavBar />

  <!-- ══ HERO ══ -->
  <section class="page-hero">
    <div class="hero-content">
      <h1>Encuentra a tu<br/>nuevo compañero</h1>
      <p>Perros y gatos rescatados que esperan<br/>una familia, un hogar y una segunda oportunidad.</p>
    </div>
  </section>

  <!-- ══ CATÁLOGO PRINCIPAL ══ -->
  <section class="catalog-section container">

    <!-- Filtros -->
    <div class="filters-bar">
      <div class="search-wrap">
        <i class='bx bx-search search-icon'></i>
        <input v-model="searchQuery" class="search-input" placeholder="Buscar mascota..." />
      </div>

      <div class="filters-grid">
        <div class="filter-group">
          <label>Tipo</label>
          <div class="filter-chips">
            <button v-for="t in ['Todos','Perro','Gato']" :key="t" type="button"
              class="chip" :class="{ active: filterType === t }" @click="filterType = t">{{ t }}</button>
          </div>
        </div>

        <div class="filter-group">
          <label>Sexo</label>
          <div class="filter-chips">
            <button v-for="s in ['Todos','Macho','Hembra']" :key="s" type="button"
              class="chip" :class="{ active: filterSex === s }" @click="filterSex = s">{{ s }}</button>
          </div>
        </div>

        <div class="filter-group">
          <label>Estado</label>
          <div class="filter-chips">
            <button v-for="st in ['Todos','Disponible','En proceso']" :key="st" type="button"
              class="chip" :class="{ active: filterStatus === st }" @click="filterStatus = st">{{ st }}</button>
          </div>
        </div>
      </div>
    </div>

    <div v-if="isLoading" class="results-top">
      <p class="results-count">Cargando mascotas...</p>
    </div>

    <div v-else class="results-top">
      <p class="results-count">
        {{ filtered.length }} mascota{{ filtered.length !== 1 ? 's' : '' }}
        encontrada{{ filtered.length !== 1 ? 's' : '' }}
      </p>
      <button class="clear-btn" @click="clearFilters">Limpiar filtros</button>
    </div>

    <!-- Grid de mascotas -->
    <div v-if="filtered.length" class="pets-grid">
      <div v-for="pet in filtered" :key="pet.id" class="pet-card">
        <div class="pet-photo">
          <img :src="mainImage(pet)" :alt="pet.name" class="pet-image" />
          <div class="pet-photo-overlay"></div>
          <span class="badge floating-badge" :class="statusColor(pet.status)">{{ pet.status }}</span>
          <div v-if="pet.images && pet.images.length > 1" class="gallery-dots">
            <span v-for="(_, i) in pet.images" :key="i" class="dot" :class="{ active: i === 0 }"></span>
          </div>
        </div>

        <div class="pet-body">
          <div class="pet-row">
            <h3 class="pet-name">{{ pet.name }}</h3>
            <span class="pet-age">{{ pet.age }}</span>
          </div>

          <p class="pet-meta">
            {{ pet.type }} · {{ pet.sex }}
          </p>

          <p class="pet-desc">{{ pet.description || pet.desc }}</p>

          <p v-if="pet.healthBasic" class="pet-health">
            <i class='bx bx-plus-medical health-icon'></i>{{ pet.healthBasic }}
          </p>

          <button
            v-if="pet.status === 'Disponible'"
            class="pet-btn"
            @click="goAdopt(pet)"
          >
            Quiero adoptar
          </button>

          <div v-else-if="pet.status === 'En proceso'" class="en-proceso-block">
            <p class="en-proceso-msg">
              🔍 Ya estamos evaluando solicitudes para {{ pet.name }}, pero puedes aplicar de todas formas.
            </p>
            <button class="pet-btn pet-btn-secondary" @click="goAdopt(pet)">
              Enviar solicitud igual
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Estado vacío -->
    <div v-else class="empty-state">
      <i class='bx bx-search-alt'></i>
      <h3>No encontramos mascotas</h3>
      <p>Intenta cambiar los filtros o realizar otra búsqueda.</p>
    </div>
  </section>

  <!-- ══ HISTORIAS FELICES ══ -->
  <section v-if="adoptedPets.length > 0" class="happy-section container">
    <div class="happy-header">
      <span class="happy-emoji"></span>
      <div>
        <h2 class="happy-title">Historias felices</h2>
        <p class="happy-sub">Estas mascotas ya encontraron un hogar para siempre.</p>
      </div>
    </div>

    <div class="happy-grid">
      <div v-for="pet in adoptedPets" :key="pet.id" class="happy-card">
        <div class="happy-photo">
          <img :src="mainImage(pet)" :alt="pet.name" />
          <div class="happy-overlay">
            <span class="happy-badge">Adoptada</span>
          </div>
        </div>
        <div class="happy-body">
          <p class="happy-name">{{ pet.name }}</p>
          <p class="happy-meta">{{ pet.type }} · {{ pet.age }}</p>
        </div>
      </div>
    </div>
  </section>

  <FooterBar />
</template>

<style scoped>

/* ══ TOKENS ANHELO PETS ══ */
/*
  --ap-dark:      #3A473C  Verde oscuro principal
  --ap-mid:       #92A894  Verde secundario
  --ap-light:     #E7EEE7  Verde claro
  --ap-gold:      #C9A06A  Dorado de acento
  --ap-bg:        #FAFAFA  Fondo principal
  --ap-white:     #FFFFFF
  --ap-text-sec:  #6C756D  Texto secundario
  --ap-border:    #E8ECE8  Bordes y separadores
  --ap-hover-dk:  #7C927E  Verde hover
  --ap-hover-sf:  #F4F6F4  Verde suave hover
  --ap-hover-lt:  #DCE5DC  Verde claro hover
  --ap-text-dk:   #2F352F  Texto oscuro
  --ap-success:   #3A6640  Éxito
  --ap-success-bg:#E7EEE7
  --ap-warn:      #C9A06A  Advertencia
  --ap-warn-bg:   #FEF3E2
  --ap-error:     #C45252
  --ap-error-bg:  #FDEAEA
*/

/* ══ HERO ══ */
.page-hero {
  position: relative;
  height: 430px;
  background-image:
    linear-gradient(
      90deg,
      rgba(0,0,0,0.72) 0%,
      rgba(0,0,0,0.45) 35%,
      rgba(0,0,0,0.12) 70%,
      rgba(0,0,0,0) 100%
    ),
    url('/img-mascotas/heromascotas.PNG');
  background-size: cover;
  background-position: center 38%;
  background-repeat: no-repeat;
  overflow: hidden;
}

.hero-content {
  position: absolute;
  left: 7%;
  bottom: 100px;
  max-width: 560px;
  z-index: 2;
}

.page-hero h1 {
  font-size: 62px;
  line-height: 0.95;
  font-weight: 800;
  color: #FFFFFF;
  letter-spacing: -3px;
  margin-bottom: 24px;
}

.page-hero p {
  font-size: 16px;
  color: rgba(255,255,255,0.92);
  line-height: 1.7;
  max-width: 420px;
  margin: 0;
}

@media (max-width: 700px) {
  .page-hero { height: 360px; }
  .hero-content { left: 24px; right: 24px; bottom: 30px; max-width: 300px; }
  .page-hero h1 { font-size: 38px; line-height: 0.95; }
  .page-hero p  { font-size: 14px; line-height: 1.6; }
}

/* ══ CATÁLOGO ══ */
.catalog-section {
  padding: 0 24px 60px;
  margin-top: -70px;
  position: relative;
  z-index: 5;
}

/* ── Barra de filtros ── */
.filters-bar {
  background: #FFFFFF;
  border: 1px solid #E8ECE8;
  border-radius: 20px;
  padding: 28px 32px;
  margin-bottom: 28px;
  box-shadow: 0 8px 32px rgba(58,71,60,0.07);
}

.search-wrap {
  position: relative;
  margin-bottom: 24px;
}

.search-input {
  width: 100%;
  height: 52px;
  border-radius: 14px;
  border: 1.5px solid #E8ECE8;
  background: #FAFAFA;
  padding-left: 50px;
  font-size: 15px;
  color: #2F352F;
  outline: none;
  box-sizing: border-box;
  transition: border-color 0.2s, box-shadow 0.2s;
}

.search-input::placeholder {
  color: #92A894;
}

.search-input:focus {
  border-color: #92A894;
  box-shadow: 0 0 0 3px rgba(146,168,148,0.15);
}

.search-icon {
  position: absolute;
  left: 17px;
  top: 50%;
  transform: translateY(-50%);
  font-size: 20px;
  color: #92A894;
  pointer-events: none;
}

.filters-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 32px;
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.filter-group label {
  font-size: 11px;
  font-weight: 700;
  color: #6C756D;
  text-transform: uppercase;
  letter-spacing: 0.08em;
}

.filter-chips {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.chip {
  border: 1.5px solid #E8ECE8;
  padding: 8px 18px;
  border-radius: 999px;
  background: #FAFAFA;
  color: #6C756D;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.18s, border-color 0.18s, color 0.18s;
}

.chip:hover:not(.active) {
  background: #F4F6F4;
  border-color: #92A894;
  color: #3A473C;
}

.chip.active {
  background: #3A473C;
  border-color: #3A473C;
  color: #FFFFFF;
}

/* ── Resultados top ── */
.results-top {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin: 0 0 22px;
}

.results-count {
  font-size: 14px;
  font-weight: 600;
  color: #6C756D;
}

.clear-btn {
  border: none;
  background: transparent;
  color: #3A473C;
  font-weight: 700;
  cursor: pointer;
  font-size: 13px;
  padding: 6px 14px;
  border-radius: 999px;
  transition: background 0.18s, color 0.18s;
}

.clear-btn:hover {
  background: #E7EEE7;
  color: #2F352F;
}

/* ══ GRID DE MASCOTAS ══
   flex + justify-content:center (no CSS grid) para que la última fila,
   cuando queda incompleta, se centre en vez de dejar un hueco vacío
   pegado a un solo lado. */
.pets-grid {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 24px;
}

/* ── Tarjeta ── */
.pet-card {
  background: #FFFFFF;
  border-radius: 22px;
  overflow: hidden;
  border: 1px solid #E8ECE8;
  box-shadow: 0 4px 16px rgba(58,71,60,0.05);
  display: flex;
  flex-direction: column;
  flex: 1 1 320px;
  max-width: 380px;
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}

.pet-card:hover {
  transform: translateY(-6px);
  box-shadow: 0 16px 40px rgba(58,71,60,0.11);
}

/* ── Foto ── */
.pet-photo {
  position: relative;
  width: 100%;
  height: 220px;
  overflow: hidden;
  background: #E7EEE7;
}

.pet-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
  transition: transform 0.5s ease;
}

.pet-card:hover .pet-image {
  transform: scale(1.04);
}

.pet-photo-overlay {
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  height: 55%;
  background: linear-gradient(to top, rgba(47,53,47,0.45) 0%, transparent 100%);
  pointer-events: none;
}

.floating-badge {
  position: absolute;
  top: 14px;
  right: 14px;
  z-index: 2;
}

.gallery-dots {
  position: absolute;
  bottom: 10px;
  left: 50%;
  transform: translateX(-50%);
  display: flex;
  gap: 5px;
  z-index: 2;
}

.dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: rgba(255,255,255,0.45);
}

.dot.active {
  background: #FFFFFF;
}

/* ── Cuerpo de tarjeta ── */
.pet-body {
  padding: 18px 20px 20px;
  display: flex;
  flex-direction: column;
  flex: 1;
}

.pet-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 4px;
}

.pet-name {
  font-size: 20px;
  font-weight: 800;
  color: #2F352F;
  letter-spacing: -0.3px;
}

.pet-age {
  font-size: 12px;
  font-weight: 600;
  color: #6C756D;
  background: #F4F6F4;
  padding: 3px 10px;
  border-radius: 999px;
}

.pet-meta {
  color: #92A894;
  font-size: 13px;
  font-weight: 500;
  margin-bottom: 10px;
}

.pet-desc {
  font-size: 13px;
  line-height: 1.7;
  color: #6C756D;
  margin-bottom: 12px;
  flex: 1;
  min-height: 88px;
}

.pet-health {
  font-size: 12px;
  color: #6C756D;
  margin-bottom: 14px;
  display: flex;
  align-items: flex-start;
  gap: 6px;
  min-height: 40px;
}

.health-icon {
  color: #3A473C;
  font-size: 13px;
  margin-top: 2px;
  flex-shrink: 0;
}

/* ── Botones ── */
.pet-btn {
  width: 100%;
  height: 46px;
  border: none;
  border-radius: 12px;
  background: #3A473C;
  color: #FFFFFF;
  font-weight: 700;
  font-size: 14px;
  cursor: pointer;
  transition: background 0.2s ease, transform 0.15s ease;
  letter-spacing: 0.01em;
}

.pet-btn:hover {
  background: #7C927E;
}

.pet-btn:active {
  transform: scale(0.98);
}

.pet-btn.pet-btn-secondary {
  background: transparent;
  border: 1.5px solid #3A473C;
  color: #3A473C;
}

.pet-btn.pet-btn-secondary:hover {
  background: #E7EEE7;
  border-color: #7C927E;
  color: #2F352F;
}



.en-proceso-msg {
  font-size: 12px;
  color: #92A894;
  background: #FEF3E2;
  border-left: 3px solid #C9A06A;
  border-radius: 0 8px 8px 0;
  padding: 10px 12px;
  margin-bottom: 10px;
  line-height: 1.55;
}

/* ══ BADGES ══ */
.badge {
  padding: 5px 13px;
  border-radius: 999px;
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.02em;
}

.badge-green {
  background: #E7EEE7;
  color: #3A6640;
}

.badge-yellow {
  background: #FEF3E2;
  color: #C9A06A;
}

.badge-gray {
  background: #F4F6F4;
  color: #6C756D;
}

/* ══ ESTADO VACÍO ══ */
.empty-state {
  text-align: center;
  padding: 80px 20px;
}

.empty-state i {
  font-size: 60px;
  color: #92A894;
  margin-bottom: 16px;
  display: block;
}

.empty-state h3 {
  font-size: 24px;
  font-weight: 800;
  color: #3A473C;
  margin-bottom: 8px;
}

.empty-state p {
  color: #6C756D;
  font-size: 15px;
}

/* ══ HISTORIAS FELICES ══ */
.happy-section {
  padding: 0 24px 80px;
}

.happy-header {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 28px;
}

.happy-emoji {
  font-size: 34px;
}

.happy-title {
  font-size: 26px;
  font-weight: 800;
  color: #2F352F;
  margin: 0 0 4px;
  letter-spacing: -0.5px;
}

.happy-sub {
  font-size: 14px;
  color: #6C756D;
  margin: 0;
}

.happy-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
  gap: 14px;
}

.happy-card {
  border-radius: 16px;
  overflow: hidden;
  border: 1px solid #E8ECE8;
  background: #FFFFFF;
  box-shadow: 0 4px 14px rgba(58,71,60,0.05);
  transition: transform 0.25s ease, box-shadow 0.25s ease;
}

.happy-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 10px 28px rgba(58,71,60,0.09);
}

.happy-photo {
  position: relative;
  height: 170px;
  overflow: hidden;
}

.happy-photo img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
  filter: saturate(0.65) brightness(0.95);
  transition: filter 0.35s ease;
}

.happy-card:hover .happy-photo img {
  filter: saturate(1) brightness(1);
}

.happy-overlay {
  position: absolute;
  inset: 0;
  background: linear-gradient(to top, rgba(47,53,47,0.55) 0%, transparent 50%);
  display: flex;
  align-items: flex-end;
  padding: 12px;
}

.happy-badge {
  background: #3A473C;
  color: #C9A06A;
  font-size: 11px;
  font-weight: 700;
  padding: 4px 11px;
  border-radius: 999px;
  letter-spacing: 0.02em;
}

.happy-body {
  padding: 12px 14px;
}

.happy-name {
  font-weight: 800;
  color: #2F352F;
  font-size: 14px;
  margin: 0 0 2px;
}

.happy-meta {
  font-size: 12px;
  color: #6C756D;
  margin: 0;
}

/* ══ RESPONSIVO ══ */
@media (max-width: 900px) {
  .page-hero { height: 460px; }
  .page-hero h1 { font-size: 46px; }
  .filters-grid { flex-direction: column; gap: 20px; }
  .results-top { flex-direction: column; align-items: flex-start; gap: 10px; }
}

@media (max-width: 560px) {
  .page-hero { height: 390px; padding: 0 24px; }
  .page-hero h1 { font-size: 38px; }
  .page-hero p  { font-size: 15px; }
  .filters-bar  { padding: 20px; }
  .filters-grid { gap: 18px; }
  .filter-chips { overflow-x: auto; flex-wrap: nowrap; padding-bottom: 4px; }
  .pet-card { flex-basis: 100%; max-width: none; }
  .happy-grid { grid-template-columns: repeat(2, 1fr); }
  .pet-btn { height: 52px; font-size: 15px; }
}

@media (prefers-reduced-motion: reduce) {
  .pet-card,
  .pet-image,
  .happy-card,
  .happy-photo img,
  .pet-btn,
  .chip,
  .clear-btn {
    transition: none;
  }
}

/* ── MOBILE RESPONSIVE adicional ── */
@media (max-width: 768px) {
  .filters-bar {
    padding: 18px 16px;
    border-radius: 16px;
  }

  .search-input {
    height: 46px;
    font-size: 14px;
  }

  .filters-grid {
    gap: 16px;
  }

  .chip {
    padding: 7px 14px;
    font-size: 12px;
  }

  .pets-grid {
    gap: 18px;
  }

  .pet-card {
    flex-basis: 100%;
    max-width: none;
  }

  .pet-photo {
    height: 200px;
  }

  .pet-name {
    font-size: 18px;
  }

  .happy-section {
    padding: 0 16px 56px;
  }

  .happy-grid {
    grid-template-columns: repeat(2, 1fr);
    gap: 10px;
  }

  .happy-photo {
    height: 140px;
  }
}

@media (max-width: 400px) {
  .happy-grid {
    grid-template-columns: 1fr;
  }
}


</style>