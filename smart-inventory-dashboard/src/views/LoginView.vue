<template>
  <main class="page">
    <section class="card">
      <h1>Smart Inventory</h1>
      <p class="subtitle">Login to manage inventory reservations</p>

      <form @submit.prevent="handleLogin" class="form">
        <input v-model="email" type="email" placeholder="Email" />
        <input v-model="password" type="password" placeholder="Password" />

        <button type="submit" :disabled="loading">
          {{ loading ? 'Logging in...' : 'Login' }}
        </button>
         <button class="link-btn" @click="router.push('/register')">
          Create new account
        </button>
      </form>
    </section>
  </main>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { toast } from 'vue-sonner'

const email = ref('')
const password = ref('')
const loading = ref(false)

const router = useRouter()
const auth = useAuthStore()

async function handleLogin() {
  loading.value = true

  try {
    await auth.login(email.value, password.value)

    toast.success('Login successful')

    setTimeout(() => {
      router.push('/dashboard')
    }, 600)

    router.push('/dashboard')
  } catch {
    toast.error('Invalid email or password')
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.page {
  min-height: 100vh;
  display: grid;
  place-items: center;
  background: linear-gradient(135deg, #0f172a, #1e293b);
  padding: 24px;
}

.card {
  width: 100%;
  max-width: 430px;
  padding: 36px;
  border-radius: 20px;
  background: #ffffff;
  box-shadow: 0 25px 60px rgba(0, 0, 0, 0.35);
}

h1 {
  margin: 0;
  font-size: 32px;
  color: #0f172a;
}

.subtitle {
  color: #64748b;
  margin: 10px 0 28px;
}

.form {
  display: grid;
  gap: 12px;
}

input {
  padding: 14px;
  border: 1px solid #cbd5e1;
  border-radius: 12px;
  font-size: 15px;
  outline: none;
}

input:focus {
  border-color: #2563eb;
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.15);
}

button {
  padding: 14px;
  border: none;
  border-radius: 12px;
  background: #2563eb;
  color: white;
  font-weight: bold;
  cursor: pointer;
  transition: 0.2s ease;
}

button:hover {
  background: #1d4ed8;
}

button:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}


.link-btn {
  width: 100%;
  margin-top: 6px;
  background: #e2e8f0;
  color: #0f172a;
}

.link-btn:hover {
  background: #cbd5e1;
}
</style>