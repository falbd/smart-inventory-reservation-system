<template>
  <main class="page">
    <section class="card">
      <button class="back-btn" @click="router.push('/dashboard')">
        ←
      </button>

      <h1>Create Reservation</h1>
      <p>Create inventory reservations.</p>

      <div class="form">
        <select v-model="reservation.productId">
          <option disabled value="">
            Select product
          </option>

          <option
            v-for="product in products"
            :key="product.id"
            :value="product.id"
          >
            {{ product.name }}
            (Available: {{ product.quantityAvailable }})
          </option>
        </select>

        <input
          v-model.number="reservation.quantity"
          type="number"
          placeholder="Quantity"
        />

        <button @click="createReservation" :disabled="loading">
          {{ loading ? 'Creating...' : 'Create Reservation' }}
        </button>
      </div>
    </section>
  </main>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import api from '@/services/api'
import { toast } from 'vue-sonner'

const router = useRouter()

const loading = ref(false)


const products = ref([])

const reservation = ref({
  productId: '',
  quantity: 1,
})

async function loadProducts() {
  const response = await api.get('/products')
  products.value = response.data
}

async function createReservation() {
  loading.value = true
  

  try {
    await api.post('/reservations', reservation.value)

    reservation.value = {
      productId: '',
      quantity: 1,
    }

    await loadProducts()

    toast.success('Reservation created successfully.')
    setTimeout(() => {
    router.push('/reservations')
  }, 600)
  
  } catch{
      toast.error('Failed to create reservation.')
  } finally {
    loading.value = false
  }
}

onMounted(async () => {
  await loadProducts()
})
</script>

<style scoped>
.page {
  min-height: 100vh;
  display: grid;
  place-items: center;
  background: #0f172a;
  color: white;
  padding: 24px;
}

.card {
  width: 100%;
  max-width: 520px;
  background: #1e293b;
  padding: 32px;
  border-radius: 20px;
  box-shadow: 0 20px 45px rgba(0, 0, 0, 0.3);
}

.card p {
  color: #94a3b8;
}

.form {
  display: grid;
  gap: 14px;
  margin-top: 24px;
}

input,
select {
  padding: 14px;
  border-radius: 12px;
  border: 1px solid #334155;
  background: #0f172a;
  color: white;
}

input::placeholder {
  color: #64748b;
}

button {
  padding: 14px;
  border: none;
  border-radius: 12px;
  background: #2563eb;
  color: white;
  cursor: pointer;
  font-weight: bold;
}

button:hover {
  background: #1d4ed8;
}

button:disabled {
  opacity: 0.7;
}

.back-btn {
  margin-bottom: 20px;
  background: #334155;
}

</style>