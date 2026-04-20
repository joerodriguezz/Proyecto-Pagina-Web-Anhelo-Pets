<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import NavBar from '../components/NavBar.vue'
import FooterBar from '../components/FooterBar.vue'

const filterType   = ref('Todos')
const filterSex    = ref('Todos')
const filterStatus = ref('Disponible')
const searchQuery  = ref('')

const pets = [
  { id:1, name:'Luna',    emoji:'🐕', bg:'#E8F5EE', type:'Perro', breed:'Mestiza',       age:'2 años',  sex:'Hembra', status:'Disponible',  desc:'Luna es muy juguetona y cariñosa. Le encanta correr en espacios abiertos.' },
  { id:2, name:'Mochi',   emoji:'🐈', bg:'#FFF0E8', type:'Gato',  breed:'Siamés',        age:'1 año',   sex:'Hembra', status:'Disponible',  desc:'Mochi es tranquila e independiente. Perfecta para apartamentos.' },
  { id:3, name:'Rocky',   emoji:'🐕', bg:'#EEF0FF', type:'Perro', breed:'Labrador',      age:'3 años',  sex:'Macho',  status:'Disponible',  desc:'Rocky es leal y protector. Excelente compañero para familias.' },
  { id:4, name:'Canela',  emoji:'🐕', bg:'#FFF8E1', type:'Perro', breed:'Shih Tzu',      age:'4 meses', sex:'Hembra', status:'En proceso',  desc:'Cachorra muy sociable. Se lleva bien con niños y otros animales.' },
  { id:5, name:'Tigre',   emoji:'🐈', bg:'#FFF0F0', type:'Gato',  breed:'Doméstico',     age:'5 años',  sex:'Macho',  status:'Disponible',  desc:'Tigre es curioso y activo. Necesita un ambiente estimulante.' },
  { id:6, name:'Bella',   emoji:'🐕', bg:'#F0F0FF', type:'Perro', breed:'Poodle',        age:'2 años',  sex:'Hembra', status:'Disponible',  desc:'Bella es muy inteligente y aprende trucos rápidamente.' },
  { id:7, name:'Max',     emoji:'🐕', bg:'#EFFFEF', type:'Perro', breed:'Golden Retriever', age:'1 año', sex:'Macho',  status:'Adoptada',   desc:'Max encontró su hogar feliz. Gracias a su familia.' },
  { id:8, name:'Sombra',  emoji:'🐈', bg:'#F5F0FF', type:'Gato',  breed:'Angora',        age:'3 años',  sex:'Hembra', status:'Disponible',  desc:'Sombra es elegante y cariñosa. Le gusta la tranquilidad.' },
  { id:9, name:'Duque',   emoji:'🐕', bg:'#FFF5E8', type:'Perro', breed:'Beagle',        age:'4 años',  sex:'Macho',  status:'En proceso',  desc:'Duque tiene mucha energía y adora explorar.' },
  { id:10, name:'Nube',   emoji:'🐈', bg:'#F0FAFF', type:'Gato',  breed:'Persa',         age:'2 años',  sex:'Hembra', status:'Disponible',  desc:'Nube es suave y tranquila. Perfecta para un hogar calmado.' },
]

const filtered = computed(() => {
  return pets.filter(p => {
    const matchType   = filterType.value   === 'Todos'     || p.type   === filterType.value
    const matchSex    = filterSex.value    === 'Todos'     || p.sex    === filterSex.value
    const matchStatus = filterStatus.value === 'Todos'     || p.status === filterStatus.value
    const matchSearch = searchQuery.value === '' || p.name.toLowerCase().includes(searchQuery.value.toLowerCase()) || p.breed.toLowerCase().includes(searchQuery.value.toLowerCase())
    return matchType && matchSex && matchStatus && matchSearch
  })
})

const statusColor = s => ({ 'Disponible': 'badge-green', 'En proceso': 'badge-yellow', 'Adoptada': 'badge-gray' }[s] || 'badge-gray')

const router = useRouter()

function goAdopt(pet) {
  router.push({ name: 'adoptar', params: { id: pet.id }, query: { name: pet.name } })
}
</script>

<template>
  <NavBar />

  <div class="page-hero">
    <h1>🐾 Catálogo de Mascotas</h1>
    <p>Encuentra a tu compañero ideal entre nuestros animales disponibles para adopción</p>
  </div>

  <section class="catalog-section container">
    <!-- Filters -->
    <div class="filters-bar">
      <div class="search-wrap">
        <span class="search-icon">🔍</span>
        <input v-model="searchQuery" class="form-input search-input" placeholder="Buscar por nombre o raza..." />
      </div>

      <div class="filter-group">
        <label class="form-label">Tipo</label>
        <div class="filter-chips">
          <button v-for="t in ['Todos','Perro','Gato']" :key="t" class="chip" :class="{ active: filterType === t }" @click="filterType = t">{{ t }}</button>
        </div>
      </div>

      <div class="filter-group">
        <label class="form-label">Sexo</label>
        <div class="filter-chips">
          <button v-for="s in ['Todos','Macho','Hembra']" :key="s" class="chip" :class="{ active: filterSex === s }" @click="filterSex = s">{{ s }}</button>
        </div>
      </div>

      <div class="filter-group">
        <label class="form-label">Estado</label>
        <div class="filter-chips">
          <button v-for="st in ['Todos','Disponible','En proceso','Adoptada']" :key="st" class="chip" :class="{ active: filterStatus === st }" @click="filterStatus = st">{{ st }}</button>
        </div>
      </div>
    </div>

    <!-- Results info -->
    <p class="results-count">{{ filtered.length }} mascota{{ filtered.length !== 1 ? 's' : '' }} encontrada{{ filtered.length !== 1 ? 's' : '' }}</p>

    <!-- Grid -->
    <div class="pets-grid" v-if="filtered.length">
      <div v-for="pet in filtered" :key="pet.id" class="pet-card">
        <div class="pet-photo" :style="{ background: pet.bg }">
          <span>{{ pet.emoji }}</span>
        </div>
        <div class="pet-body">
          <div class="pet-row">
            <span class="pet-name">{{ pet.name }}</span>
            <span class="badge" :class="statusColor(pet.status)">{{ pet.status }}</span>
          </div>
          <p class="pet-meta">{{ pet.type }} · {{ pet.breed }} · {{ pet.age }} · {{ pet.sex }}</p>
          <p class="pet-desc">{{ pet.desc }}</p>
          <button class="btn-primary pet-btn" :disabled="pet.status !== 'Disponible'" @click="goAdopt(pet)">
            {{ pet.status === 'Disponible' ? '💛 Adoptar' : pet.status === 'En proceso' ? 'En proceso' : '✅ Adoptado' }}
          </button>
        </div>
      </div>
    </div>

    <div v-else class="empty-state">
      <span>🔍</span>
      <p>No se encontraron mascotas con esos filtros.</p>
      <button class="btn-outline-green" @click="filterType='Todos'; filterSex='Todos'; filterStatus='Disponible'; searchQuery=''">Limpiar filtros</button>
    </div>
  </section>

  <FooterBar />
</template>

<style scoped>
.catalog-section { padding: 40px 24px 60px; }

.filters-bar {
  background: var(--white);
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
  padding: 24px;
  margin-bottom: 28px;
  display: flex;
  flex-wrap: wrap;
  gap: 20px;
  align-items: flex-end;
  box-shadow: var(--shadow-sm);
}

.search-wrap {
  position: relative;
  flex: 1;
  min-width: 200px;
}
.search-icon { position: absolute; left: 12px; top: 50%; transform: translateY(-50%); font-size: 16px; }
.search-input { padding-left: 38px; }

.filter-group { display: flex; flex-direction: column; gap: 6px; }
.filter-chips { display: flex; gap: 8px; flex-wrap: wrap; }
.chip {
  padding: 6px 14px;
  border-radius: var(--radius-full);
  border: 1.5px solid var(--border);
  background: var(--white);
  color: var(--text-mid);
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}
.chip:hover { border-color: var(--green); color: var(--green); }
.chip.active { background: var(--green); border-color: var(--green); color: var(--white); }

.results-count { font-size: 14px; color: var(--text-light); margin-bottom: 20px; }

.pets-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
  gap: 22px;
}
.pet-card {
  background: var(--white);
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  overflow: hidden;
  box-shadow: var(--shadow-sm);
  transition: box-shadow 0.2s, transform 0.2s;
}
.pet-card:hover { box-shadow: var(--shadow-md); transform: translateY(-3px); }
.pet-photo { height: 180px; display: flex; align-items: center; justify-content: center; font-size: 70px; }
.pet-body { padding: 16px; }
.pet-row { display: flex; align-items: center; justify-content: space-between; margin-bottom: 6px; }
.pet-name { font-size: 18px; font-weight: 700; }
.pet-meta { font-size: 12px; color: var(--text-light); margin-bottom: 10px; }
.pet-desc { font-size: 13px; color: var(--text-mid); line-height: 1.5; margin-bottom: 14px; }
.pet-btn  { width: 100%; justify-content: center; font-size: 14px; }
.pet-btn:disabled { opacity: 0.5; cursor: not-allowed; }

.empty-state {
  text-align: center;
  padding: 60px 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 14px;
  font-size: 60px;
}
.empty-state p { font-size: 16px; color: var(--text-mid); }
</style>
