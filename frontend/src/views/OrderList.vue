<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { PencilSquareIcon, TrashIcon, EyeIcon, XMarkIcon, ChevronLeftIcon, ChevronRightIcon, MagnifyingGlassIcon, ExclamationTriangleIcon } from '@heroicons/vue/24/outline';
import type { Order } from '../types/Order';

const orders = ref<Order[]>([]);
const loading = ref(true);
const error = ref<string | null>(null);
const router = useRouter();

const API_BASE_URL = 'http://localhost:5000';

const showModal = ref(false);
const selectedOrder = ref<Order | null>(null); 

const showDeleteConfirm = ref(false);
const orderToDelete = ref<string | null>(null);

const currentPage = ref(1);
const perPage = 5

const totalPages = computed(() => {
    return Math.ceil(filteredOrders.value.length / perPage);
})

const paginatedOrders = computed(() => {
    if (currentPage.value > totalPages.value && totalPages.value >= 1) {
        currentPage.value = 1;
    } else if (totalPages.value === 0) {
        return [];
    }

    const start = (currentPage.value - 1) * perPage;
    const end = start + perPage;
    return filteredOrders.value.slice(start, end);
});

const searchTerm = ref(''); 

const filteredOrders = computed(() => {
    if (!searchTerm.value) {
        return orders.value;
    }

    const lowerCaseSearch = searchTerm.value.toLowerCase();

    return orders.value.filter(order => {
        return order.id.toLowerCase().includes(lowerCaseSearch) ||
               order.client.toLowerCase().includes(lowerCaseSearch) ||
               order.total.toString().includes(lowerCaseSearch);
    });
});

function goToPage(page: number) {
    if (page >= 1 && page <= totalPages.value) {
        currentPage.value = page;
    }
}

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
    const order = orders.value.find(o => o.id === id);
    if (order){
        selectedOrder.value = order;
        showModal.value = true;
    }
}

function handleEdit(id: string) {
    router.push(`/edit/${id}`);
}

function handleDelete(id: string) {
    orderToDelete.value = id;
    showDeleteConfirm.value = true;
}

async function confirmDelete() {
    const id = orderToDelete.value;
    if (!id) return;
    
    showDeleteConfirm.value = false;
    orderToDelete.value = null;

    try {
        const response = await fetch(`${API_BASE_URL}/orders/${id}`, {
            method: 'DELETE',
        });

        if (response.status === 204) {
            console.log(`Orden ${id} eliminada`);
            // Filtramos la lista localmente
            orders.value = orders.value.filter(order => order.id !== id);
        } else if (response.status === 404) {
            error.value = 'La orden que intentas eliminar no existe.';
            setTimeout(() => error.value = null, 3000);
        } else {
            throw new Error(`Error ${response.status}: No se pudo eliminar la orden.`);
        }
    } catch (err) {
        error.value = `Error al eliminar: ${(err as Error).message}`;
        setTimeout(() => error.value = null, 5000);
        console.error("Error deleting order:", err);
    }
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

    <div class="mb-6 relative">
      <input
        type="text"
        v-model="searchTerm"
        placeholder="Buscar por ID, Cliente o Total..."
        class="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg shadow-sm focus:ring-indigo-500 focus:border-indigo-500 transition duration-150"
      >
      <MagnifyingGlassIcon class="absolute left-3 top-1/2 transform -translate-y-1/2 h-5 w-5 text-gray-400" />
    </div>
    
    <div v-if="loading" class="text-center py-12 text-xl text-gray-500 bg-white rounded-xl shadow-md">
      Cargando órdenes...
    </div>
    <div v-else-if="error" class="bg-red-100 border border-red-400 text-red-700 p-4 rounded-lg shadow-md mb-6" role="alert">
      <strong class="font-bold">Error:</strong>
      <span class="block sm:inline ml-2">{{ error }}</span>
    </div>
    
    <div v-else-if="filteredOrders.length === 0" class="text-center py-12 text-gray-500 text-xl bg-white rounded-xl shadow-md">
      No se encontraron órdenes que coincidan con "{{ searchTerm }}".
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
          <tr v-for="(order, index) in paginatedOrders" :key="order.id" :class="{'bg-gray-50': index % 2 === 0, 'bg-white': index % 2 !== 0, 'hover:bg-indigo-50': true}">
            <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{{ order.id.substring(0, 8) }}...</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-600">{{ order.client }}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-600">{{ new Date(order.date).toLocaleDateString() }}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900 font-bold text-right text-lg">${{ order.total.toFixed(2) }}</td>
            
            <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium space-x-2">
              
              <button @click.stop="viewDetails(order.id)" title="Ver Detalles" class="text-gray-500 hover:text-gray-900 transition duration-150 p-1 rounded hover:bg-gray-100">
                <EyeIcon class="h-5 w-5 inline-block" />
              </button>
              <button @click="handleEdit(order.id)" title="Editar Orden" class="text-indigo-600 hover:text-indigo-800 transition duration-150 p-1 rounded hover:bg-indigo-100">
                <PencilSquareIcon class="h-5 w-5 inline-block" />
              </button>
              <button @click="handleDelete(order.id)" title="Eliminar Orden" class="text-red-600 hover:text-red-800 transition duration-150 p-1 rounded hover:bg-red-100">
                <TrashIcon class="h-5 w-5 inline-block" />
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    
    <div v-if="orders.length === 0 && !loading && !error" class="text-center py-12 text-gray-500 text-xl bg-white rounded-xl shadow-md mt-6">
      No hay órdenes registradas. ¡Crea la primera para empezar!
    </div>
    
    <div v-if="totalPages > 1 && !loading" class="flex justify-center items-center mt-6 p-4 bg-white rounded-xl shadow-md space-x-2">
      
      <button
        @click="goToPage(currentPage - 1)"
        :disabled="currentPage === 1"
        :class="{'opacity-50 cursor-not-allowed': currentPage === 1}"
        class="p-2 rounded-lg text-indigo-600 hover:bg-indigo-50 transition duration-150"
      >
        <ChevronLeftIcon class="h-5 w-5" />
      </button>

      <div class="flex space-x-1">
        <button
          v-for="page in totalPages"
          :key="page"
          @click="goToPage(page)"
          :class="{
            'bg-indigo-600 text-white font-bold': page === currentPage,
            'text-gray-700 hover:bg-gray-200': page !== currentPage
          }"
          class="px-4 py-2 rounded-lg text-sm transition duration-150"
        >
          {{ page }}
        </button>
      </div>

      <button
        @click="goToPage(currentPage + 1)"
        :disabled="currentPage === totalPages"
        :class="{'opacity-50 cursor-not-allowed': currentPage === totalPages}"
        class="p-2 rounded-lg text-indigo-600 hover:bg-indigo-50 transition duration-150"
      >
        <ChevronRightIcon class="h-5 w-5" />
      </button>
      
    </div>

  </div>

  <div 
    v-if="showModal && selectedOrder" 
    class="fixed inset-0 z-50 overflow-y-auto" 
    aria-labelledby="modal-title" 
    role="dialog" 
    aria-modal="true"
    @click.self="showModal = false"
  >
    <div class="flex items-center justify-center min-h-screen p-4 relative z-50">
      
      <div class="bg-white rounded-lg shadow-2xl transform transition-all max-w-lg w-full p-6 border border-indigo-400">
        
        <div class="flex justify-between items-center border-b pb-3 mb-4">
          <h3 class="text-2xl font-bold text-gray-900" id="modal-title">Detalles de la Orden</h3>
          <button @click="showModal = false" class="text-gray-400 hover:text-gray-600">
            <XMarkIcon class="h-6 w-6" />
          </button>
        </div>

        <div class="space-y-4">
          
          <div class="flex justify-between border-b pb-2">
            <span class="text-sm font-medium text-gray-500">ID Completo:</span>
            <span class="text-sm font-mono text-gray-800 break-all">{{ selectedOrder.id }}</span>
          </div>

          <div class="flex justify-between border-b pb-2">
            <span class="text-lg font-medium text-gray-700">Cliente:</span>
            <span class="text-lg font-semibold text-indigo-600">{{ selectedOrder.client }}</span>
          </div>

          <div class="flex justify-between border-b pb-2">
            <span class="text-lg font-medium text-gray-700">Fecha:</span>
            <span class="text-lg text-gray-800">{{ new Date(selectedOrder.date).toLocaleDateString() }}</span>
          </div>

          <div class="flex justify-between border-b pb-2">
            <span class="text-lg font-medium text-gray-700">Monto Total:</span>
            <span class="text-2xl font-extrabold text-green-600">${{ selectedOrder.total.toFixed(2) }}</span>
          </div>

        </div>

        <div class="mt-5 sm:mt-6">
          <button 
            type="button" 
            @click="showModal = false" 
            class="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-indigo-600 text-base font-medium text-white hover:bg-indigo-700 focus:outline-none transition duration-150"
          >
            Cerrar
          </button>
        </div>

      </div>
    </div>
  </div>

  <div 
    v-if="showDeleteConfirm" 
    class="fixed inset-0 z-50 overflow-y-auto" 
    aria-labelledby="delete-modal-title" 
    role="dialog" 
    aria-modal="true"
    @click.self="showDeleteConfirm = false; orderToDelete = null"
  >
    <div class="flex items-center justify-center min-h-screen p-4 relative z-50">
      
      <div class="bg-white rounded-lg shadow-2xl transform transition-all max-w-sm w-full p-6 border border-red-400">
        
        <div class="sm:flex sm:items-start">
          <div class="mx-auto flex-shrink-0 flex items-center justify-center h-12 w-12 rounded-full bg-red-100 sm:mx-0 sm:h-10 sm:w-10">
            <ExclamationTriangleIcon class="h-6 w-6 text-red-600" aria-hidden="true" />
          </div>
          <div class="mt-3 text-center sm:mt-0 sm:ml-4 sm:text-left">
            <h3 class="text-lg leading-6 font-bold text-gray-900" id="delete-modal-title">
              Confirmar Eliminación
            </h3>
            <div class="mt-2">
              <p class="text-sm text-gray-500">
                Estás seguro que deseas eliminar esta orden?
              </p>
            </div>
          </div>
        </div>
        
        <div class="mt-5 sm:mt-6 sm:flex sm:flex-row-reverse space-y-3 sm:space-y-0 sm:space-x-3 sm:space-x-reverse">
          <button 
            type="button" 
            @click="confirmDelete" 
            class="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-red-600 text-base font-medium text-white hover:bg-red-700 focus:outline-none transition duration-150 sm:ml-3 sm:w-auto sm:text-sm"
          >
            Sí, Eliminar
          </button>
          <button 
            type="button" 
            @click="showDeleteConfirm = false; orderToDelete = null" 
            class="w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none transition duration-150 sm:w-auto sm:text-sm"
          >
            Cancelar
          </button>
        </div>

      </div>
    </div>
  </div>

</template>