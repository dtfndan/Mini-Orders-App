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
    <div>
        <h1>Listado de Ordenes</h1>
        <button @click="router.push('/create')" class="create-btn">Crear Nueva Orden</button>
        <div v-if="loading">Cargando...</div>
        <div v-else-if="error" class="error-message">Error: {{ error }}</div>
        <table v-else class="order-table">
            <thead>
                <tr>
                    <th>ID</th>
                    <th>Cliente</th>
                    <th>Fecha</th>
                    <th>Total</th>
                    <th>Acciones</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="order in orders" :key="order.id">
                    <td>{{ order.id.substring(0, 8) }}...</td>
                    <td>{{ order.client }}</td>
                    <td>{{ new Date(order.date).toLocaleDateString() }}</td>
                    <td>{{ order.total.toFixed(2) }}</td>
                    <td>
                        <button @click="viewDetails(order.id)" class="details-btn">Ver Detalles</button>
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
</template>