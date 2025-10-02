<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import type { Order } from '../types/Order';

const orders = ref<Order[]>([]);
const loading = ref(true);
const error = ref<string | null>(null);
const router = useRouter();

const API_BASE_URL = 'http://localhost:5000';

async function fechtOrders() {
    loading.value = true;
    error.value = null;
    try {
        const response = await fetch(`${API_BASE_URL}/orders`);
        if (!response.ok) {
            throw new Error('Failed to fetch orders');
        }
        orders.value = await response.json();
    } catch (err) {
        error.value = (err as Error).message;
    } finally {
        loading.value = false;
    }
}

function viewDetails(id: string) {
    alert(`View details for order ${id}`); // implementar componente de detalles
}

onMounted(fechtOrders);
</script>

<template>
  <div class="container mx-auto px-4 sm:px-6 lg:px-8">
    <h1 class="text-4xl font-extrabold mb-8 text-gray-900">Gestión de Órdenes</h1>
    
    <div class="flex justify-between items-center mb-6">
      <h2 class="text-2xl font-semibold text-indigo-700">Listado de Órdenes</h2>
      
      <button 
        @click="router.push('/create')" 
        class="bg-indigo-600 hover:bg-indigo-700 text-white font-medium py-2 px-4 rounded-lg shadow-md hover:shadow-lg transition duration-300 transform hover:scale-[1.01]"
      >
        <span class="mr-1">+</span> Crear Nueva Orden
      </button>
    </div>

    <div v-if="loading" class="text-center py-12 text-xl text-gray-500 bg-white rounded-xl shadow-md">
      Cargando órdenes...
    </div>
    <div v-else-if="error" class="bg-red-100 border border-red-400 text-red-700 p-4 rounded-lg shadow-md mb-6" role="alert">
      <strong class="font-bold">Error de Conexión:</strong>
      <span class="block sm:inline ml-2">{{ error }}</span>
    </div>
    
    <div v-else class="bg-white shadow-xl rounded-xl overflow-hidden">
      <table class="min-w-full divide-y divide-gray-200">
        <thead class="bg-gray-50">
          <tr>
            <th class="px-6 py-3 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">ID</th>
            <th class="px-6 py-3 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Cliente</th>
            <th class="px-6 py-3 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Fecha</th>
            <th class="px-6 py-3 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Total</th>
            <th class="px-6 py-3 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Acciones</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-100">
          <tr v-for="(order, index) in orders" :key="order.id" :class="{'bg-gray-50': index % 2 === 0, 'bg-white': index % 2 !== 0, 'hover:bg-indigo-50': true}">
            <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{{ order.id.substring(0, 8) }}...</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-600">{{ order.client }}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-600">{{ new Date(order.date).toLocaleDateString() }}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900 font-bold text-right text-lg">${{ order.total.toFixed(2) }}</td>
            <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
              <button 
                @click="viewDetails(order.id)"
                class="text-indigo-600 hover:text-indigo-800 font-medium transition duration-150 p-1"
              >
                Detalles
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    
    <div v-if="orders.length === 0 && !loading && !error" class="text-center py-12 text-gray-500 text-xl bg-white rounded-xl shadow-md mt-6">
      No hay órdenes registradas. ¡Crea la primera para empezar!
    </div>

  </div>
</template>