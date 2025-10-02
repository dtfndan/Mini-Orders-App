import {createRouter, createWebHistory} from 'vue-router'
import OrderList from '../views/OrderList.vue'
import OrderForm from '../views/OrderForm.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
        path: '/',
        name: 'order-list',
        component: OrderList
    },
    {
        path: '/create',
        name: 'order-create',
        component: OrderForm
    },
    {
        path: '/edit/:id',
        name: 'order-edit',
        component: OrderForm,
        props: true
    }
],
});

export default router;