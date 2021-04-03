import Vue from 'vue'
import App from './App.vue'
import Buefy from 'buefy'
import VueNativeSock from 'vue-native-websocket'
import 'buefy/dist/buefy.css'

Vue.use(VueNativeSock, 'ws://localhost:8412/?password=hackme', {
  connectManually: true
});
Vue.use(Buefy)
Vue.config.productionTip = false

new Vue({
  render: h => h(App),
}).$mount('#app')
