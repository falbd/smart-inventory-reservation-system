<template>
  <main class="dashboard">
    <header class="topbar">
      <div>
        <h1>Smart Inventory Dashboard</h1>

        <p>
          Welcome,
          <strong>{{ auth.user?.fullName }}</strong>
          ({{ auth.user?.role }})
        </p>
      </div>

      <div class="topbar-actions">
        <button
          v-if="auth.isAdmin"
          class="nav-btn"
          @click="router.push('/products/create')"
        >
          Create Product
        </button>

        <button
          class="nav-btn"
          @click="router.push('/reservations/create')"
        >
          Create Reservation
        </button>

        <button
        class="nav-btn"
        @click="router.push('/reservations')"
      >
        Reservations
      </button>

        <button class="logout-btn" @click="handleLogout">
          Logout
        </button>
      </div>
    </header>

    <section class="cards">
      <div class="card products-card">
        <h3>Total Products</h3>
        <p>{{ products.length }}</p>
      </div>

      <div class="card available-card">
        <h3>Total Available Stock</h3>
        <p>{{ totalAvailable }}</p>
      </div>

      <div class="card reserved-card">
        <h3>Total Reserved Stock</h3>
        <p>{{ totalReserved }}</p>
      </div>
    </section>

    <section class="inventory-section">
      <div class="section-header">
        <h2>Inventory</h2>

        <button class="refresh-btn" @click="loadProducts">
          Refresh
        </button>
      </div>

      <div class="table-wrapper">
        <table>
          <thead>
            <tr>
              <th>Product</th>
              <th>SKU</th>
              <th>Price</th>
              <th>Available</th>
              <th>Reserved</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="product in products" :key="product.id">
              <td>{{ product.name }}</td>
              <td>{{ product.sku }}</td>
              <td>{{ product.price }} SAR</td>
              <td>{{ product.quantityAvailable }}</td>
              <td>{{ product.quantityReserved }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  </main>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import api from '@/services/api'
import { createInventoryConnection } from '@/services/signalr'

const router = useRouter()
const auth = useAuthStore()

const products = ref([])

const totalAvailable = computed(() =>
  products.value.reduce((sum, p) => sum + p.quantityAvailable, 0),
)

const totalReserved = computed(() =>
  products.value.reduce((sum, p) => sum + p.quantityReserved, 0),
)

async function loadProducts() {
  const response = await api.get('/products')
  products.value = response.data
}

async function handleLogout() {
  await auth.logout()
  router.push('/login')
}

function handleStockUpdated(stock) {
  const product = products.value.find((x) => x.id === stock.productId)

  if (!product) return

  product.quantityAvailable = stock.quantityAvailable
  product.quantityReserved = stock.quantityReserved
}

onMounted(async () => {
  await loadProducts()

  const connection = createInventoryConnection(handleStockUpdated)

  try {
    await connection.start()
    console.log('SignalR connected')
  } catch (error) {
    console.error(error)
  }
})
</script>

<style scoped>
.dashboard {
  min-height: 100vh;
  padding: 32px;
  background: #0f172a;
  color: white;
}

.topbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 32px;
  gap: 20px;
}

.topbar p {
  color: #cbd5e1;
}

.topbar-actions {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
}

.cards {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 20px;
  margin-bottom: 32px;
}

.card {
  border-radius: 18px;
  padding: 24px;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.25);
  transition: 0.2s ease;
}

.card:hover {
  transform: translateY(-3px);
}

.products-card {
  background: linear-gradient(135deg, #2563eb, #1d4ed8);
}

.available-card {
  background: linear-gradient(135deg, #059669, #047857);
}

.reserved-card {
  background: linear-gradient(135deg, #dc2626, #b91c1c);
}

.card h3 {
  margin-bottom: 12px;
  color: rgba(255, 255, 255, 0.85);
  font-size: 16px;
}

.card p {
  font-size: 36px;
  font-weight: bold;
  color: white;
}

.inventory-section {
  background: #1e293b;
  border-radius: 18px;
  padding: 24px;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.25);
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.section-header h2 {
  margin: 0;
}

.table-wrapper {
  overflow-x: auto;
  border-radius: 16px;
}

table {
  width: 100%;
  border-collapse: collapse;
  overflow: hidden;
}

thead {
  background: #0f172a;
}

th {
  text-align: left;
  padding: 18px;
  color: #cbd5e1;
  font-size: 14px;
  font-weight: 600;
  letter-spacing: 0.5px;
}

td {
  padding: 18px;
  border-bottom: 1px solid #334155;
  color: #e2e8f0;
  font-size: 15px;
}

tbody tr {
  transition: 0.2s ease;
}

tbody tr:nth-child(even) {
  background: rgba(255, 255, 255, 0.02);
}

tbody tr:hover {
  background: rgba(59, 130, 246, 0.12);
}

td:first-child {
  font-weight: 600;
  color: white;
}

td:nth-child(4) {
  color: #22c55e;
  font-weight: bold;
}

td:nth-child(5) {
  color: #f97316;
  font-weight: bold;
}

.logout-btn,
.refresh-btn,
.nav-btn {
  border: none;
  color: white;
  padding: 10px 18px;
  border-radius: 12px;
  cursor: pointer;
  transition: 0.2s ease;
  font-weight: bold;
}

.nav-btn {
  background: #059669;
}

.nav-btn:hover {
  background: #047857;
}

.logout-btn,
.refresh-btn {
  background: #2563eb;
}

.logout-btn:hover,
.refresh-btn:hover {
  background: #1d4ed8;
}

@media (max-width: 900px) {
  .cards {
    grid-template-columns: 1fr;
  }

  .topbar {
    flex-direction: column;
    align-items: flex-start;
  }

  .topbar-actions {
    width: 100%;
  }

  table {
    display: block;
    overflow-x: auto;
  }
}
</style>