import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import LoginView from '@/views/LoginView.vue'
import DashboardView from '@/views/DashboardView.vue'
import CreateProductView from '@/views/CreateProductView.vue'
import CreateReservationView from '@/views/CreateReservationView.vue'
import ReservationsView from '@/views/ReservationsView.vue'
import RegisterView from '@/views/RegisterView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/dashboard',
    },
    {
      path: '/login',
      name: 'login',
      component: LoginView,
    },
    {
    path: '/register',
    name: 'register',
    component: RegisterView,
    },
    {
      path: '/dashboard',
      name: 'dashboard',
      component: DashboardView,
      meta: {
        requiresAuth: true,
      },
    },
    {
      path: '/products/create',
      name: 'create-product',
      component: CreateProductView,
      meta: {
        requiresAuth: true,
        requiresAdmin: true,
      },
    },
    {
      path: '/reservations',
      name: 'reservations',
      component: ReservationsView,
      meta: {
        requiresAuth: true,
      },
    },
    {
      path: '/reservations/create',
      name: 'create-reservation',
      component: CreateReservationView,
      meta: {
        requiresAuth: true,
      },
    },
  ],
})

router.beforeEach(async (to) => {
  const auth = useAuthStore()

  if (auth.token && !auth.user) {
    try {
      await auth.fetchProfile()
    } catch {
      auth.logout()
      return '/login'
    }
  }

  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    return '/login'
  }

  if (to.meta.requiresAdmin && !auth.isAdmin) {
    return '/dashboard'
  }

  if (to.path === '/login' && auth.isAuthenticated) {
    return '/dashboard'
  }
})

export default router