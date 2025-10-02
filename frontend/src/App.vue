<template>
  <header>
    <nav>
      <router-link to="/">Listado de Órdenes</router-link>
      <router-link to="/create">Crear Orden</router-link>
    </nav>
  </header>
  
  <main>
    <router-view />
  </main>
</template>

<script setup lang="ts">
</script>

<style scoped>
nav a {
  margin-right: 15px;
}
</style>