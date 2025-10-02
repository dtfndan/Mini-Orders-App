<template>
  <div class="min-h-screen bg-gray-50 antialiased">

    <header class="bg-white shadow-sm">
      <nav class="container mx-auto px-4 sm:px-6 lg:px-8 py-3 flex justify-start space-x-6 border-b border-gray-100">
        <h1 class="text-2xl font-bold text-gray-800">Mini Orders App</h1>
        <router-link to="/" class="text-lg font-semibold text-gray-800 hover:text-indigo-600 transition duration-150">
          Listado de Órdenes
        </router-link>
        <router-link to="/create" class="text-lg font-semibold text-gray-800 hover:text-indigo-600 transition duration-150">
          Crear Orden
        </router-link>
      </nav>
    </header>

    <main class="py-10">
      <router-view />
    </main>

  </div>
</template>

<script setup lang="ts">
</script>

<style scoped>
.router-link-active {
  color: #4F46E5; 
  border-bottom: 2px solid #4F46E5;
  padding-bottom: 8px;
}
</style>