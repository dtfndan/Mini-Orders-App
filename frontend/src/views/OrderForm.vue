<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import type { OrderCreationDto } from '../types/Order';

const router = useRouter();
const API_BASE_URL = 'http://localhost:5000';

const form = ref<OrderCreationDto>({
    client: '',
    date: new Date().toISOString().substring(0, 10),
    total: 0,
});

const error = ref<{[key: string]: string}>({});
const globalError = ref<string | null>(null);
const successMessage = ref<string | null>(null);

function validateForm() {
    error.value = {};
    if (!form.value.client) {
        error.value.client = 'El nombre del cliente es obligatorio.';
    }
    if (form.value.total <= 0) {
        error.value.total = 'El total debe ser mayor que 0.';
    }
    return Object.keys(error.value).length === 0;
}

async function handleSubmit() {
    globalError.value = null;
    successMessage.value = null;

    if (!validateForm()) {
        return;
    }

    try{
        const orderData = {
            ...form.value,
            date: new Date(form.value.date).toISOString(),
            total: Number(form.value.total)
        }

        const response = await fetch(`${API_BASE_URL}/orders`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(orderData),
        });

        if (response.status === 400){
            const data = await response.json();
            globalError.value = data.message || 'Error en la solicitud.';
            return;
        }

        if (!response.ok){
            throw new Error(`Error ${response.status}: No se pudo crear la orden.`);
        }

        successMessage.value = 'Orden creada exitosamente.';
    } catch (err) {
        globalError.value = (err as Error).message;
    }
}

</script>

<template>
  <div class="container mx-auto px-4 sm:px-6 lg:px-8 max-w-lg">
    
    <h1 class="text-4xl font-extrabold mb-8 text-gray-900 text-center">Crear Nueva Orden</h1>

    <div v-if="globalError" class="bg-red-100 border border-red-400 text-red-700 p-4 rounded-lg mb-4">
      <strong class="font-bold">Error:</strong> {{ globalError }}
    </div>
    <div v-if="successMessage" class="bg-green-100 border border-green-400 text-green-700 p-4 rounded-lg mb-4">
      <strong class="font-bold">¡Éxito!</strong> {{ successMessage }}
    </div>

    <form @submit.prevent="handleSubmit" class="bg-white p-8 rounded-xl shadow-2xl border border-gray-100">
      
      <div class="mb-5">
        <label for="client" class="block text-sm font-medium text-gray-700 mb-2">Nombre del Cliente</label>
        <input 
          id="client" 
          v-model="form.client" 
          type="text" 
          placeholder="Ej: Roger Smith S.A."
          class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition duration-150"
        />
        <p v-if="error.client" class="text-red-500 text-xs mt-1 font-semibold">{{ error.client }}</p>
      </div>

      <div class="mb-5">
        <label for="date" class="block text-sm font-medium text-gray-700 mb-2">Fecha de Orden</label>
        <input 
          id="date" 
          v-model="form.date" 
          type="date"
          class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition duration-150"
        />
      </div>

      <div class="mb-8">
        <label for="total" class="block text-sm font-medium text-gray-700 mb-2">Monto Total ($)</label>
        <input 
          id="total" 
          v-model.number="form.total" 
          type="number" 
          step="0.01" 
          min="0.01"
          placeholder="0.00"
          class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition duration-150"
        />
        <p v-if="error.total" class="text-red-500 text-xs mt-1 font-semibold">{{ error.total }}</p>
      </div>

      <div class="flex justify-between space-x-4 border-t pt-6">
        <button 
          type="button" 
          @click="router.push('/')"
          class="flex-1 px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-100 transition duration-150 font-medium"
        >
          Cancelar / Volver
        </button>
        <button 
          type="submit"
          class="flex-1 px-4 py-2 bg-indigo-600 text-white font-semibold rounded-lg shadow-md hover:bg-indigo-700 transition duration-150 transform hover:scale-[1.01]"
        >
          Guardar Orden
        </button>
      </div>
    </form>
  </div>
</template>