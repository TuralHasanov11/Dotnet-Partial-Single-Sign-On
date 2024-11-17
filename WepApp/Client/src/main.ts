import './assets/main.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import identity from './plugins/identity'

import App from './App.vue'
import router from './router'

const app = createApp(App)

app.use(createPinia())
app.use(identity)
app.use(router)

app.mount('#app')
