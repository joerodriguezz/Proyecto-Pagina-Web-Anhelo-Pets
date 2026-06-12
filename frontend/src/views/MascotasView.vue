<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import NavBar from '../components/NavBar.vue'
import FooterBar from '../components/FooterBar.vue'
import { usePetsStore } from '../stores/usePetsStore'

const router = useRouter()
const store  = usePetsStore()

// ─────────────────────────────────────────────
// Filtros
// ─────────────────────────────────────────────
const filterType   = ref('Todos')
const filterSex    = ref('Todos')
const filterStatus = ref('Todos')
const searchQuery  = ref('')

// ─────────────────────────────────────────────
// Catálogo: solo Disponible y En proceso
// La computed del store ya excluye Adoptadas e Inactivas.
// Encima aplicamos los filtros del usuario.
// ─────────────────────────────────────────────
const filtered = computed(() =>
  store.publicPets.filter(pet => {
    const matchType   = filterType.value   === 'Todos' || pet.type === filterType.value
    const matchSex    = filterSex.value    === 'Todos' || pet.sex  === filterSex.value
    const matchStatus = filterStatus.value === 'Todos' || pet.status === filterStatus.value
    const search      = searchQuery.value.toLowerCase().trim()
    const matchSearch = !search ||
      pet.name.toLowerCase().includes(search) ||
      pet.type.toLowerCase().includes(search) ||
      (pet.description || '').toLowerCase().includes(search) ||
      (pet.personality || '').toLowerCase().includes(search)
    return matchType && matchSex && matchStatus && matchSearch
  })
)

// ─────────────────────────────────────────────
// "Historias felices" — mascotas Adoptadas
// ─────────────────────────────────────────────
const adoptedPets = computed(() => store.adoptedPets)

// ─────────────────────────────────────────────
// Badge de estado
// ─────────────────────────────────────────────
const statusColor = status => ({
  'Disponible': 'badge-green',
  'En proceso': 'badge-yellow',
}[status] || 'badge-gray')

// ─────────────────────────────────────────────
// Imagen principal de la mascota
// ─────────────────────────────────────────────
function mainImage(pet) {
  if (pet.images && pet.images.length > 0) return pet.images[0].preview
  // fallback a imagen estática si la mascota viene de datos seed
  return pet.image || ''
}

// ─────────────────────────────────────────────
// Ir a adoptar
// ─────────────────────────────────────────────
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

        <!-- Solo los estados visibles al público -->
        <div class="filter-group">
          <label>Estado</label>
          <div class="filter-chips">
            <button v-for="st in ['Todos','Disponible','En proceso']" :key="st" type="button"
              class="chip" :class="{ active: filterStatus === st }" @click="filterStatus = st">{{ st }}</button>
          </div>
        </div>
      </div>
    </div>

    <div class="results-top">
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
          <span class="badge floating-badge" :class="statusColor(pet.status)">{{ pet.status }}</span>
          <!-- Galería dot indicator -->
          <div v-if="pet.images && pet.images.length > 1" class="gallery-dots">
            <span v-for="(_, i) in pet.images" :key="i" class="dot" :class="{ active: i === 0 }"></span>
          </div>
        </div>

        <div class="pet-body">
          <div class="pet-row">
            <h3 class="pet-name">{{ pet.name }}</h3>
            <span class="pet-age">{{ pet.age }}</span>
          </div>

          <!-- Info pública: tipo · sexo · tamaño -->
          <p class="pet-meta">
            {{ pet.type }} · {{ pet.sex }}<template v-if="pet.size"> · {{ pet.size }}</template>
          </p>

          <!-- Personalidad (pública) -->
          <p v-if="pet.personality" class="pet-personality">
            <span class="pill-tag">{{ pet.personality }}</span>
          </p>

          <!-- Descripción pública -->
          <p class="pet-desc">{{ pet.description || pet.desc }}</p>

          <!-- Salud básica (pública, sin historial) -->
          <p v-if="pet.healthBasic" class="pet-health">
            <i class='bx bx-plus-medical health-icon'></i>{{ pet.healthBasic }}
          </p>

          <!-- Botón según estado -->
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
            <span class="happy-badge">Adoptada ♥</span>
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
/* ══ Hero ══ */
.page-hero {
  position: relative;
  background-image:
    linear-gradient(rgba(58,71,60,.45), rgba(58,71,60,.48)),
    url('/img-mascotas/heromascotas.PNG');
  background-size: cover;
  background-position: center 38%;
  height: 520px;
  display: flex;
  align-items: center;
  justify-content: flex-start;
  padding: 0 7%;
}
.hero-content { position: relative; z-index: 2; max-width: 520px; }
.page-hero h1 { font-size: 58px; line-height: .95; font-weight: 800; color: white; letter-spacing: -3px; margin-bottom: 18px; }
.page-hero p  { font-size: 17px; color: rgba(255,255,255,.92); line-height: 1.8; }

/* ══ Catálogo ══ */
.catalog-section {
  padding: 0 24px 60px;
  margin-top: -70px;
  position: relative;
  z-index: 5;
}

.filters-bar {
  background: rgba(255,255,255,.92);
  backdrop-filter: blur(12px);
  border: 1px solid rgba(146,168,148,.14);
  border-radius: 34px;
  padding: 34px;
  margin-bottom: 34px;
  box-shadow: 0 20px 50px rgba(58,71,60,.06);
}

.search-wrap { position: relative; margin-bottom: 30px; }
.search-input {
  width: 100%;
  height: 58px;
  border-radius: 18px;
  border: 1px solid #DCE4DD;
  background: #FAFAFA;
  padding-left: 52px;
  font-size: 15px;
  color: #3A473C;
  outline: none;
  box-sizing: border-box;
}
.search-input:focus { border-color: #3A473C; box-shadow: 0 0 0 4px rgba(146,168,148,.12); }
.search-icon { position: absolute; left: 18px; top: 50%; transform: translateY(-50%); font-size: 22px; color: #3A473C; }

.filters-grid { display: flex; flex-wrap: wrap; gap: 36px; }
.filter-group { display: flex; flex-direction: column; gap: 10px; }
.filter-group label { font-size: 14px; font-weight: 700; color: #3A473C; }
.filter-chips { display: flex; gap: 10px; flex-wrap: wrap; }
.chip { border: none; padding: 10px 18px; border-radius: 999px; background: #F4F6F4; color: #5E6A60; font-size: 14px; font-weight: 600; cursor: pointer; transition: all .2s; }
.chip.active { background: #3A473C; color: white; }
.chip:hover:not(.active) { background: #E8EDE8; }

.results-top { display: flex; justify-content: space-between; align-items: center; margin: 36px 0 24px; }
.results-count { font-size: 15px; font-weight: 600; color: #6C756D; }
.clear-btn { border: none; background: transparent; color: #3A473C; font-weight: 700; cursor: pointer; font-size: 14px; }
.clear-btn:hover { color: #5A7A5C; }

.pets-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(270px, 1fr)); gap: 26px; }

/* ══ Tarjeta de mascota ══ */
.pet-card {
  background: white;
  border-radius: 26px;
  overflow: hidden;
  border: 1px solid rgba(146,168,148,.10);
  transition: .35s ease;
  box-shadow: 0 10px 30px rgba(58,71,60,.05);

  display: flex;
  flex-direction: column;
}

.pet-card:hover { transform: translateY(-8px); box-shadow: 0 22px 50px rgba(58,71,60,.10); }

.pet-photo { position: relative; width: 100%; height: 240px; overflow: hidden; background: #F4F6F4; }
.pet-image { width: 100%; height: 100%; object-fit: cover; transition: .5s ease; display: block; }
.pet-card:hover .pet-image { transform: scale(1.05); }
.floating-badge { position: absolute; top: 16px; right: 16px; }

/* Indicador de galería */
.gallery-dots { position: absolute; bottom: 10px; left: 50%; transform: translateX(-50%); display: flex; gap: 5px; }
.dot { width: 6px; height: 6px; border-radius: 50%; background: rgba(255,255,255,.5); }
.dot.active { background: white; }

.pet-body {
  padding: 20px;

  display: flex;
  flex-direction: column;
  flex: 1;
}
.pet-row { display: flex; justify-content: space-between; align-items: center; margin-bottom: 6px; }
.pet-name { font-size: 22px; font-weight: 800; color: #3A473C; }
.pet-age  { font-size: 13px; color: #3A473C; font-weight: 600; }
.pet-meta { color: #7A847B; font-size: 14px; margin-bottom: 10px; }

.pet-personality { margin-bottom: 10px; }
.pill-tag { background: rgba(146,168,148,.15); color: #5A6E5C; font-size: 12px; font-weight: 700; padding: 4px 12px; border-radius: 999px; }

.pet-desc {
  font-size: 14px;
  line-height: 1.7;
  color: #5F6A61;
  margin-bottom: 12px;

  min-height: 140px;
}

.pet-health {
  font-size: 13px;
  color: #6C756D;
  margin-bottom: 16px;
  display: flex;
  align-items: flex-start;
  gap: 6px;

  min-height: 48px;
}
.health-icon { color: #3A473C; font-size: 14px; }

.pet-btn { width: 100%; height: 48px; border: none; border-radius: 14px; background: #3A473C; color: white; font-weight: 700; font-size: 14px; cursor: pointer; transition: all .2s; }
.pet-btn:hover { background: #7E9580; }
.pet-btn.pet-btn-secondary { background: transparent; border: 2px solid #3A473C; color: #5A7A5C; }
.pet-btn.pet-btn-secondary:hover { background: rgba(146,168,148,.1); }

.en-proceso-block { }
.en-proceso-msg { font-size: 13px; color: #8C6A30; background: rgba(249,193,122,.15); border-radius: 10px; padding: 10px 12px; margin-bottom: 10px; line-height: 1.5; }

/* ══ Badges ══ */
.badge { padding: 7px 14px; border-radius: 999px; font-size: 12px; font-weight: 700; }
.badge-green  { background: #E7F1E8; color: #5B7A61; }
.badge-yellow { background: #FFF1DD; color: #D89A47; }
.badge-gray   { background: #ECEFEC; color: #6C756D; }

/* ══ Estado vacío ══ */
.empty-state { text-align: center; padding: 100px 20px; }
.empty-state i { font-size: 70px; color: #3A473C; margin-bottom: 18px; display: block; }
.empty-state h3 { font-size: 28px; color: #3A473C; margin-bottom: 10px; }
.empty-state p  { color: #6C756D; }

/* ══ Historias felices ══ */
.happy-section {
  padding: 0 24px 80px;
}
.happy-header { display: flex; align-items: center; gap: 16px; margin-bottom: 32px; }
.happy-emoji  { font-size: 36px; }
.happy-title  { font-size: 28px; font-weight: 800; color: #3A473C; margin: 0 0 4px; letter-spacing: -0.5px; }
.happy-sub    { font-size: 15px; color: #6C756D; margin: 0; }

.happy-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(180px, 1fr)); gap: 16px; }

.happy-card { border-radius: 20px; overflow: hidden; box-shadow: 0 6px 20px rgba(58,71,60,.06); background: white; }
.happy-photo { position: relative; height: 180px; }
.happy-photo img { width: 100%; height: 100%; object-fit: cover; display: block; filter: saturate(.7); }
.happy-overlay { position: absolute; inset: 0; background: linear-gradient(to top, rgba(58,71,60,.6) 0%, transparent 50%); display: flex; align-items: flex-end; padding: 12px; }
.happy-badge {
  background: #3A473C;
  color: #F9C17A;
  font-size: 12px;
  font-weight: 700;
  padding: 4px 12px;
  border-radius: 999px;
}
.happy-body { padding: 12px 14px; }
.happy-name { font-weight: 800; color: #3A473C; font-size: 15px; margin: 0 0 2px; }
.happy-meta { font-size: 13px; color: #6C756D; margin: 0; }

/* ══ Responsivo ══ */
@media (max-width: 900px) {
  .page-hero { height: 460px; }
  .page-hero h1 { font-size: 46px; }
  .filters-grid { flex-direction: column; gap: 24px; }
  .results-top { flex-direction: column; align-items: flex-start; gap: 12px; }
}
@media (max-width: 560px) {
  .page-hero { height: 390px; padding: 0 24px; }
  .page-hero h1 { font-size: 38px; }
  .page-hero p  { font-size: 15px; }
  .filters-bar  { padding: 24px; }
  .pets-grid { grid-template-columns: 1fr; }
  .happy-grid { grid-template-columns: repeat(2, 1fr); }
}
</style>