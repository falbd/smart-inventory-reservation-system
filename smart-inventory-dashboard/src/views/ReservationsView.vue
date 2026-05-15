<template>
  <main class="page">
    <header class="topbar">
      <div class="title-section">
        <button class="back-btn" @click="router.push('/dashboard')">
          ←
        </button>

        <div>
          <h1>Reservations</h1>
          <p>Manage pending, confirmed, cancelled, and expired reservations.</p>
        </div>
      </div>
    </header>

    <section class="card">
      <div class="section-header">
        <h2>Reservation List</h2>
        <button class="refresh-btn" @click="loadReservations">Refresh</button>
      </div>

      <div class="table-wrapper">
        <table>
          <thead>
            <tr>
              <th>Reservation ID</th>
              <th>Product ID</th>
              <th>Quantity</th>
              <th>Status</th>
              <th>Created</th>
              <th>Expires</th>
              <th>Actions</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="reservation in reservations" :key="reservation.id">
              <td>{{ reservation.id }}</td>
              <td>{{ reservation.productId }}</td>
              <td>{{ reservation.quantity }}</td>
              <td>
                <span class="badge" :class="reservation.status.toLowerCase()">
                  {{ reservation.status }}
                </span>
              </td>
              <td>{{ formatDate(reservation.createdAtUtc) }}</td>
              <td>{{ formatDate(reservation.expiresAtUtc) }}</td>
              <td>
                <div class="actions" v-if="reservation.status === 'Pending'">
                  <button class="confirm-btn" @click="confirmReservation(reservation.id)">
                    Confirm
                  </button>

                  <button class="cancel-btn" @click="cancelReservation(reservation.id)">
                    Cancel
                  </button>
                </div>

                <span v-else class="muted">No actions</span>
              </td>
            </tr>
          </tbody>
        </table>
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
const reservations = ref([])

async function loadReservations() {
  try {
    const response = await api.get('/reservations')
    reservations.value = response.data
  } catch {
    toast.error('Failed to load reservations')
  }
}

async function confirmReservation(id) {
  try {
    await api.post(`/reservations/${id}/confirm`)

    toast.success('Reservation confirmed')

     setTimeout(async () => {
      await loadReservations()
    }, 600)

  } catch {
    toast.error('Failed to confirm reservation')
  }
}

async function cancelReservation(id) {
  try {
    await api.post(`/reservations/${id}/cancel`)

    toast.success('Reservation cancelled')

    setTimeout(async () => {
      await loadReservations()
    }, 600)

  } catch {
    toast.error('Failed to cancel reservation')
  }
}

function formatDate(value) {
  return new Date(value).toLocaleString()
}

onMounted(async () => {
  await loadReservations()
})
</script>

<style scoped>
.page {
  min-height: 100vh;
  padding: 32px;
  background: #0f172a;
  color: white;
}

.topbar {
  margin-bottom: 32px;
}

.title-section {
  display: flex;
  align-items: flex-start;
  gap: 16px;
}

.title-section .back-btn {
  margin-top: 4px;
}

.title-section h1 {
  margin: 0;
}

.title-section p {
  margin-top: 14px;
  line-height: 1.5;
  color: #94a3b8;
}

.topbar p {
  color: #94a3b8;
  margin-top: 6px;
}

.card {
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

.table-wrapper {
  overflow-x: auto;
  border-radius: 16px;
}

table {
  width: 100%;
  border-collapse: collapse;
}

thead {
  background: #0f172a;
}

th,
td {
  padding: 16px;
  border-bottom: 1px solid #334155;
  text-align: left;
}

th {
  color: #cbd5e1;
  font-size: 14px;
  font-weight: 600;
}

td {
  color: #e2e8f0;
  font-size: 14px;
}

tbody tr {
  transition: 0.2s ease;
}

tbody tr:hover {
  background: rgba(59, 130, 246, 0.12);
}

.badge {
  padding: 6px 10px;
  border-radius: 999px;
  font-size: 13px;
  font-weight: bold;
}

.pending {
  background: #f97316;
  color: white;
}

.confirmed {
  background: #16a34a;
  color: white;
}

.cancelled {
  background: #dc2626;
  color: white;
}

.expired {
  background: #64748b;
  color: white;
}

.actions {
  display: flex;
  gap: 8px;
}

button {
  border: none;
  color: white;
  padding: 9px 14px;
  border-radius: 10px;
  cursor: pointer;
  font-weight: bold;
  transition: 0.2s ease;
}

.confirm-btn {
  background: #16a34a;
}

.confirm-btn:hover {
  background: #15803d;
}

.cancel-btn {
  background: #dc2626;
}

.cancel-btn:hover {
  background: #b91c1c;
}

.refresh-btn {
  background: #2563eb;
}

.refresh-btn:hover {
  background: #1d4ed8;
}

.back-btn {
  width: 46px;
  height: 46px;
  border-radius: 12px;
  background: #2563eb;
  font-size: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.back-btn:hover {
  background: #1d4ed8;
}

.muted {
  color: #94a3b8;
}

@media (max-width: 900px) {
  .section-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
  }

  table {
    display: block;
    overflow-x: auto;
  }
}
</style>