<template>
  <main class="page">
    <section class="card">
      <button class="back-btn" @click="router.push('/dashboard')">←</button>

      <h1>Create Product</h1>

      <div class="form">
        <input v-model="product.name" placeholder="Product name" />
        <input v-model="product.sku" placeholder="SKU" />
        <input v-model.number="product.price" type="number" placeholder="Price" />
        <input v-model.number="product.initialStock" type="number" placeholder="Initial stock" />

        <button @click="createProduct" :disabled="loading">
          {{ loading ? 'Creating...' : 'Create Product' }}
        </button>
      </div>
    </section>
  </main>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import api from '@/services/api'
import { toast } from 'vue-sonner'

const router = useRouter()
const loading = ref(false)


const product = ref({
  name: '',
  sku: '',
  price: 0,
  initialStock: 0,
})

async function createProduct() {
  loading.value = true


  try {
    await api.post('/products', product.value)

    product.value = {
      name: '',
      sku: '',
      price: 0,
      initialStock: 0,
    }

    toast.success('Product created successfully.')

    setTimeout(() => {
      router.push('/dashboard')
    }, 600)
    
  } catch {
    toast.error('Failed to create product.')
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

input {
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