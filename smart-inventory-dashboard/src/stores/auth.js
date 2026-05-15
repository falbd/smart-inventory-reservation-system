import { defineStore } from 'pinia'
import api from '@/services/api'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null,
    token: localStorage.getItem('token'),
  }),

  getters: {
    isAuthenticated: (state) => !!state.token,
    isAdmin: (state) => state.user?.role === 'Admin',
    isStaff: (state) => state.user?.role === 'Staff',
  },

  actions: {
    async login(email, password) {
      const response = await api.post('/auth/login', {
        email,
        password,
      })

      this.token = response.data.token
      localStorage.setItem('token', this.token)

      await this.fetchProfile()
    },

    async fetchProfile() {
      if (!this.token) return

      const response = await api.get('/auth/me')
      this.user = response.data
    },

    async logout() {
      try {
        await api.post('/auth/logout')
      } catch {
        // ignore
      }

      localStorage.removeItem('token')
      this.token = null
      this.user = null
    },
  },
})