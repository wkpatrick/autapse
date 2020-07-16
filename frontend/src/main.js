import Vue from 'vue'
import App from './App.vue'
import Buefy from 'buefy'
import VueNativeSock from 'vue-native-websocket'
import 'buefy/dist/buefy.css'

Vue.use(VueNativeSock, 'ws://10.0.0.130:8412/?password=hackme')
Vue.use(Buefy)
Vue.config.productionTip = false

new Vue({
  render: h => h(App),
}).$mount('#app')
