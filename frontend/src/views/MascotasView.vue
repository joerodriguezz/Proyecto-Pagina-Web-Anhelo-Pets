<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import NavBar from '../components/NavBar.vue'
import FooterBar from '../components/FooterBar.vue'

const router = useRouter()

/* FILTROS */

const filterType = ref('Todos')
const filterSex = ref('Todos')
const filterStatus = ref('Todos')
const searchQuery = ref('')

/* MASCOTAS */

const pets = [

  {
    id:1,
    name:'Bartolo',
    image:'/img-perros/Bartolo.PNG',
    type:'Perro',
    age:'5 meses',
    sex:'Macho',
    time:'Hace 1 mes',
    status:'Disponible',
    desc:'Muy juguetón y sociable. Le encanta correr, jugar y recibir cariño.'
  },

  {
    id:2,
    name:'Mojito',
    image:'/img-perros/Mojito.jpg',
    type:'Perro',
    age:'13 años',
    sex:'Macho',
    time:'Hace 2 años',
    status:'Disponible',
    desc:'Tranquilo y cariñoso. Ama descansar en lugares cómodos y salir a pasear.'
  },

  {
    id:3,
    name:'Lola',
    image:'/img-perros/Lola.PNG',
    type:'Gato',
    age:'1 año y 5 meses',
    sex:'Hembra',
    time:'Hace 4 meses',
    status:'Disponible',
    desc:'Curiosa y tranquila. Le encanta dormir y explorar rincones.'
  },

  {
    id:4,
    name:'Ramona',
    image:'/img-perros/Ramona.PNG',
    type:'Gato',
    age:'1 año',
    sex:'Hembra',
    time:'Hace 2 meses',
    status:'Disponible',
    desc:'Muy juguetona y activa. Siempre busca algo nuevo por explorar.'
  },

  {
    id:5,
    name:'Mavis',
    image:'/img-perros/Mavis.PNG',
    type:'Gato',
    age:'4 meses',
    sex:'Hembra',
    time:'Hace 1 mes',
    status:'Disponible',
    desc:'Pequeña y energética. Ama jugar y correr todo el día.'
  },

  {
    id:6,
    name:'Manchas',
    image:'/img-perros/Manchas.PNG',
    type:'Gato',
    age:'1 año y 2 meses',
    sex:'Hembra',
    time:'Hace 3 meses',
    status:'Disponible',
    desc:'Observadora y tranquila. Prefiere lugares altos y silenciosos.'
  },

  {
    id:7,
    name:'Kiwi',
    image:'/img-perros/Kiwi.PNG',
    type:'Perro',
    age:'5 años',
    sex:'Macho',
    time:'En proceso',
    status:'En proceso',
    desc:'Muy energético y amigable. Ama correr y jugar al aire libre.'
  },

  {
    id:8,
    name:'Cloe',
    image:'/img-perros/Cloe.PNG',
    type:'Gato',
    age:'4 años',
    sex:'Hembra',
    time:'En proceso',
    status:'En proceso',
    desc:'Cariñosa y calmada. Prefiere ambientes tranquilos.'
  },

  {
    id:9,
    name:'Ares',
    image:'/img-perros/Ares.PNG',
    type:'Perro',
    age:'2 años',
    sex:'Macho',
    time:'Adoptado',
    status:'Adoptada',
    desc:'Juguetón y muy cariñoso. Siempre busca atención.'
  },

  {
    id:10,
    name:'Bala',
    image:'/img-perros/Bala.PNG',
    type:'Perro',
    age:'7 años',
    sex:'Macho',
    time:'Hace 4 años',
    status:'Disponible',
    desc:'Reservado pero noble. Le gustan los lugares tranquilos.'
  },

]

/* FILTRADO */

const filtered = computed(() => {

  return pets.filter((pet) => {

    const petType =
      pet.type.toLowerCase()

    const selectedType =
      filterType.value.toLowerCase()

    const matchType =
      selectedType === 'todos'
      || petType === selectedType

    const petSex =
      pet.sex.toLowerCase()

    const selectedSex =
      filterSex.value.toLowerCase()

    const matchSex =
      selectedSex === 'todos'
      || petSex === selectedSex

    const petStatus =
      pet.status.toLowerCase()

    const selectedStatus =
      filterStatus.value.toLowerCase()

    const matchStatus =
      selectedStatus === 'todos'
      || petStatus === selectedStatus

    const search =
      searchQuery.value
        .toLowerCase()
        .trim()

    const matchSearch =
      search === ''
      || pet.name.toLowerCase().includes(search)
      || pet.type.toLowerCase().includes(search)
      || pet.desc.toLowerCase().includes(search)

    return (
      matchType
      && matchSex
      && matchStatus
      && matchSearch
    )

  })

})

/* BADGES */

const statusColor = (status) => {

  return {
    'Disponible': 'badge-green',
    'En proceso': 'badge-yellow',
    'Adoptada': 'badge-gray'
  }[status]

}

/* ADOPTAR */

function goAdopt(pet) {

  router.push({
    name: 'adoptar',
    params: { id: pet.id },
    query: { name: pet.name }
  })

}
</script>

<template>

  <NavBar />

  <!-- HERO -->

  <section class="page-hero">

    <div class="hero-overlay"></div>

    <div class="hero-content">

      <h1>
        Encuentra a tu
        nuevo compañero
      </h1>

      <p>
        Perros y gatos rescatados que esperan
        una familia, un hogar y una segunda oportunidad.
      </p>

    </div>

  </section>

  <!-- CONTENIDO -->

  <section class="catalog-section container">

    <!-- FILTROS -->

    <div class="filters-bar">

      <!-- BUSCADOR -->

      <div class="search-wrap">

        <i class='bx bx-search search-icon'></i>

        <input
          v-model="searchQuery"
          class="search-input"
          placeholder="Buscar mascota..."
        />

      </div>

      <!-- FILTROS -->

      <div class="filters-grid">

        <div class="filter-group">

          <label>Tipo</label>

          <div class="filter-chips">

            <button
              v-for="t in ['Todos','Perro','Gato']"
              :key="t"
              type="button"
              class="chip"
              :class="{ active: filterType === t }"
              @click="filterType = t"
            >
              {{ t }}
            </button>

          </div>

        </div>

        <div class="filter-group">

          <label>Sexo</label>

          <div class="filter-chips">

            <button
              v-for="s in ['Todos','Macho','Hembra']"
              :key="s"
              type="button"
              class="chip"
              :class="{ active: filterSex === s }"
              @click="filterSex = s"
            >
              {{ s }}
            </button>

          </div>

        </div>

        <div class="filter-group">

          <label>Estado</label>

          <div class="filter-chips">

            <button
              v-for="st in ['Todos','Disponible','En proceso','Adoptada']"
              :key="st"
              type="button"
              class="chip"
              :class="{ active: filterStatus === st }"
              @click="filterStatus = st"
            >
              {{ st }}
            </button>

          </div>

        </div>

      </div>

    </div>

    <!-- RESULTADOS -->

    <div class="results-top">

      <p class="results-count">
        {{ filtered.length }}
        mascota{{ filtered.length !== 1 ? 's' : '' }}
        encontrada{{ filtered.length !== 1 ? 's' : '' }}
      </p>

      <button
        class="clear-btn"
        @click="
          filterType='Todos';
          filterSex='Todos';
          filterStatus='Todos';
          searchQuery=''
        "
      >
        Limpiar filtros
      </button>

    </div>

    <!-- GRID -->

    <div
      class="pets-grid"
      v-if="filtered.length"
    >

      <div
        v-for="pet in filtered"
        :key="pet.id"
        class="pet-card"
      >

        <div class="pet-photo">

          <img
            :src="pet.image"
            :alt="pet.name"
            class="pet-image"
          >

          <span
            class="badge floating-badge"
            :class="statusColor(pet.status)"
          >
            {{ pet.status }}
          </span>

        </div>

        <div class="pet-body">

          <div class="pet-row">

            <h3 class="pet-name">
              {{ pet.name }}
            </h3>

            <span class="pet-time">
              {{ pet.time }}
            </span>

          </div>

          <p class="pet-meta">
            {{ pet.type }}
            ·
            {{ pet.age }}
            ·
            {{ pet.sex }}
          </p>

          <p class="pet-desc">
            {{ pet.desc }}
          </p>

          <button
            class="pet-btn"
            :disabled="pet.status !== 'Disponible'"
            @click="goAdopt(pet)"
          >

            <span
              v-if="pet.status === 'Disponible'"
            >
              Adoptar
            </span>

            <span
              v-else-if="pet.status === 'En proceso'"
            >
              En proceso
            </span>

            <span v-else>
              Adoptado
            </span>

          </button>

        </div>

      </div>

    </div>

    <!-- EMPTY -->

    <div
      v-else
      class="empty-state"
    >

      <i class='bx bx-search-alt'></i>

      <h3>
        No encontramos mascotas
      </h3>

      <p>
        Intenta cambiar los filtros
        o realizar otra búsqueda.
      </p>

    </div>

  </section>

  <FooterBar />

</template>

<style scoped>

.page-hero {

  position: relative;

  background-image:
    linear-gradient(
      rgba(58,71,60,0.45),
      rgba(58,71,60,0.48)
    ),
    url('/img-mascotas/mascotas.jpg');

  background-size: cover;

  background-position: center 38%;

  height: 520px;

  display: flex;

  align-items: center;

  justify-content: flex-start;

  padding: 0 7%;
}

.hero-content {

  position: relative;

  z-index: 2;

  max-width: 520px;
}

.page-hero h1 {

  font-size: 58px;

  line-height: 0.95;

  font-weight: 800;

  color: white;

  letter-spacing: -3px;

  margin-bottom: 18px;
}

.page-hero p {

  font-size: 17px;

  color: rgba(255,255,255,0.92);

  line-height: 1.8;
}

.catalog-section {

  padding: 0 24px 60px;

  margin-top: -70px;

  position: relative;

  z-index: 5;
}

.filters-bar {

  background: rgba(255,255,255,0.92);

  backdrop-filter: blur(12px);

  border: 1px solid rgba(146,168,148,0.14);

  border-radius: 34px;

  padding: 34px;

  margin-bottom: 34px;

  box-shadow:
    0 20px 50px rgba(58,71,60,0.06);
}

.search-wrap {

  position: relative;

  margin-bottom: 30px;
}

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
}

.search-input:focus {

  border-color: #92A894;

  box-shadow:
    0 0 0 4px rgba(146,168,148,0.12);
}

.search-icon {

  position: absolute;

  left: 18px;

  top: 50%;

  transform: translateY(-50%);

  font-size: 22px;

  color: #92A894;
}

.filters-grid {

  display: flex;

  flex-wrap: wrap;

  gap: 36px;
}

.filter-group {

  display: flex;

  flex-direction: column;

  gap: 10px;
}

.filter-group label {

  font-size: 14px;

  font-weight: 700;

  color: #3A473C;
}

.filter-chips {

  display: flex;

  gap: 10px;

  flex-wrap: wrap;
}

.chip {

  border: none;

  padding: 10px 18px;

  border-radius: 999px;

  background: #F4F6F4;

  color: #5E6A60;

  font-size: 14px;

  font-weight: 600;

  cursor: pointer;
}

.chip.active {

  background: #92A894;

  color: white;
}

.results-top {

  display: flex;

  justify-content: space-between;

  align-items: center;

  margin:
    36px 0
    24px;
}

.results-count {

  font-size: 15px;

  font-weight: 600;

  color: #6C756D;
}

.clear-btn {

  border: none;

  background: transparent;

  color: #92A894;

  font-weight: 700;

  cursor: pointer;
}

.pets-grid {

  display: grid;

  grid-template-columns:
    repeat(auto-fill, minmax(270px, 1fr));

  gap: 26px;
}

.pet-card {

  background: white;

  border-radius: 26px;

  overflow: hidden;

  border: 1px solid rgba(146,168,148,0.10);

  transition: 0.35s ease;

  box-shadow:
    0 10px 30px rgba(58,71,60,0.05);
}

.pet-card:hover {

  transform: translateY(-8px);

  box-shadow:
    0 22px 50px rgba(58,71,60,0.10);
}

.pet-photo {

  position: relative;

  width: 100%;

  height: 240px;

  overflow: hidden;

  background: #F4F6F4;
}

.pet-image {

  width: 100%;

  height: 100%;

  object-fit: cover;

  transition: 0.5s ease;
}

.pet-card:hover .pet-image {

  transform: scale(1.05);
}

.floating-badge {

  position: absolute;

  top: 16px;

  right: 16px;
}

.pet-body {

  padding: 20px;
}

.pet-row {

  display: flex;

  justify-content: space-between;

  align-items: center;

  margin-bottom: 10px;
}

.pet-name {

  font-size: 24px;

  font-weight: 800;

  color: #3A473C;
}

.pet-time {

  font-size: 12px;

  color: #92A894;
}

.pet-meta {

  color: #7A847B;

  font-size: 14px;

  margin-bottom: 14px;
}

.pet-desc {

  font-size: 14px;

  line-height: 1.7;

  color: #5F6A61;

  margin-bottom: 18px;
}

.pet-btn {

  width: 100%;

  height: 48px;

  border: none;

  border-radius: 14px;

  background: #92A894;

  color: white;

  font-weight: 700;

  cursor: pointer;
}

.pet-btn:hover {

  background: #7E9580;
}

.pet-btn:disabled {

  background: #DADFDA;

  color: #7B817C;

  cursor: not-allowed;
}

.badge {

  padding: 7px 14px;

  border-radius: 999px;

  font-size: 12px;

  font-weight: 700;
}

.badge-green {

  background: #E7F1E8;

  color: #5B7A61;
}

.badge-yellow {

  background: #FFF1DD;

  color: #D89A47;
}

.badge-gray {

  background: #ECEFEC;

  color: #6C756D;
}

.empty-state {

  text-align: center;

  padding: 100px 20px;
}

.empty-state i {

  font-size: 70px;

  color: #92A894;

  margin-bottom: 18px;
}

.empty-state h3 {

  font-size: 28px;

  color: #3A473C;

  margin-bottom: 10px;
}

.empty-state p {

  color: #6C756D;
}

@media (max-width: 900px) {

  .page-hero {

    height: 460px;
  }

  .page-hero h1 {

    font-size: 46px;
  }

  .filters-grid {

    flex-direction: column;

    gap: 24px;
  }

  .results-top {

    flex-direction: column;

    align-items: flex-start;

    gap: 12px;
  }
}

@media (max-width: 560px) {

  .page-hero {

    height: 390px;

    padding: 0 24px;
  }

  .page-hero h1 {

    font-size: 38px;
  }

  .page-hero p {

    font-size: 15px;
  }

  .filters-bar {

    padding: 24px;
  }

  .pets-grid {

    grid-template-columns: 1fr;
  }
}

</style>