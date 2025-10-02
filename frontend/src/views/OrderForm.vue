<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import type { OrderCreationDto } from '../types/Order';

const router = useRouter();
const API_BASE_URL = 'https://localhost:5000/api';

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
    <div>
        <h1>Crear Nueva Orden</h1>
        <div v-if="globalError" class="error-message">{{ globalError }}</div>
        <div v-if="successMessage" class="success-message">{{ successMessage }}</div>

        <form @submit.prevent="handleSubmit" class="order-form">
            <div>
                <label for="client">Cliente</label>
                <input id="client" v-model="form.client" type="text" />
                <p v-if="error.client" class="error-message">{{ error.client }}</p>
            </div>

            <div>
                <label for="date">Fecha</label>
                <input id="date" v-model="form.date" type="date" />
            </div>
            
            <div>
                <label for="total">Total</label>
                <input id="total" v-model.number="form.total" type="number" step="0.01" min="0" />
                <p v-if="error.total" class="error-message">{{ error.total }}</p>
            </div>

            <button type="submit">Crear Orden</button>
            <button type="button" @click="router.push('/')">Cancelar</button>
        </form>
    </div>
</template>