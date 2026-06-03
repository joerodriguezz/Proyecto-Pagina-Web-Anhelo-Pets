<script setup>
const moduleMessage = 'El historial medico, vacunas y tratamientos todavia no tienen API persistente.'
</script>

<template>
  <div class="view-container">
    <header class="page-header">
      <div>
        <h1 class="admin-page-title">Control de salud</h1>
        <p class="admin-page-sub">Solo se mostraran registros medicos reales cuando el modulo este conectado a DB</p>
      </div>
    </header>

    <div class="empty-state">
      <h2>Sin datos reales para mostrar</h2>
      <p>{{ moduleMessage }}</p>
    </div>
  </div>
</template>

<style scoped>
.view-container { background-color: transparent; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 32px; }
.admin-page-title { font-size: 28px; font-weight: 800; color: #3A473C; letter-spacing: 0; }
.admin-page-sub { font-size: 14px; color: #6C756D; margin-top: 4px; font-weight: 500; }
.empty-state {
  background: white;
  border-radius: 8px;
  padding: 32px;
  box-shadow: 0 4px 20px rgba(58, 71, 60, 0.02);
}
.empty-state h2 { color: #3A473C; font-size: 18px; font-weight: 800; margin-bottom: 8px; }
.empty-state p { color: #6C756D; font-size: 14px; }
</style>
