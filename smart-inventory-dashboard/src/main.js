import './assets/main.css'
import { Toaster } from 'vue-sonner'
import 'vue-sonner/style.css'
import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.component('AppToaster', Toaster)
app.mount('#app')

