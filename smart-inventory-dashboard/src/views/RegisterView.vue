<template>
  <main class="page">
    <section class="card">
      <h1>Create Account</h1>
      <p class="subtitle">Register a new Smart Inventory user</p>

      <form @submit.prevent="handleRegister" class="form">
        <input v-model="fullName" placeholder="Full name" />
        <input v-model="email" type="email" placeholder="Email" />
        <input v-model="password" type="password" placeholder="Password" />

        <select v-model="role">
          <option value="Staff">Staff</option>
          <option value="Admin">Admin</option>
        </select>

        <button type="submit" :disabled="loading">
          {{ loading ? 'Creating...' : 'Register' }}
        </button>
      </form>

      <button class="link-btn" @click="router.push('/login')">
        Already have an account? Login
      </button>
    </section>
  </main>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import api from '@/services/api'
import { toast } from 'vue-sonner'

const router = useRouter()

const fullName = ref('')
const email = ref('')
const password = ref('')
const role = ref('Staff')
const loading = ref(false)

async function handleRegister() {
  loading.value = true

  try {
    await api.post('/auth/register', {
      fullName: fullName.value,
      email: email.value,
      password: password.value,
      role: role.value,
    })

    toast.success('Account created successfully')

    setTimeout(() => {
      router.push('/login')
    }, 600)

  } catch (err) {
    toast.error(
      err?.response?.data ||
      'Failed to create account.',
    )
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
  gap: 14px;
}

input,
select {
  padding: 14px;
  border: 1px solid #cbd5e1;
  border-radius: 12px;
  font-size: 15px;
  outline: none;
}

input:focus,
select:focus {
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
}

button:hover {
  background: #1d4ed8;
}

button:disabled {
  opacity: 0.7;
}

.link-btn {
  width: 100%;
  margin-top: 14px;
  background: #e2e8f0;
  color: #0f172a;
}

.link-btn:hover {
  background: #cbd5e1;
}

</style>