<template>
  <div>
    <nav class="level">
      <div class="level-left">
        <div class="level-item"> Free Space: {{ formatBytes(server.free_space) }}</div>
        <div class="level-item"> Download: {{ formatBitsPerSecond(server.rate_down) }}</div>
        <div class="level-item"> Upload: {{ formatBitsPerSecond(server.rate_up) }}</div>
        <div class="level-item"> Downloaded: {{ formatBytes(server.ses_transferred_down) }}</div>
        <div class="level-item"> Uploaded: {{ formatBytes(server.ses_transferred_up) }}</div>
      </div>
    </nav>
  </div>
</template>

<script>
import bytes from 'bytes';

export default {
  name: "ServerInfo",
  props: {
    server: Object
  },
  methods: {
    formatBytes(value) {
      return bytes(value)
    },
    formatBitsPerSecond(bits, decimals = 2) {  //Modified from https://stackoverflow.com/questions/15900485/correct-way-to-convert-size-in-bytes-to-kb-mb-gb-in-javascript
      if (bits === 0) {
        return '0 Bits/s';
      }

      const k = 1024;
      const dm = decimals < 0 ? 0 : decimals;
      const sizes = ['Bits/s', 'KiB/s', 'MiB/s', 'GiB/s', 'TiB/s', 'PiB/s', 'EiB/s', 'ZiB/s', 'YiB/s'];

      const i = Math.floor(Math.log(bits) / Math.log(k));

      return parseFloat((bits / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
    }
  }
}
</script>

<style scoped>
 .level {
   position: absolute;
   bottom: 0;
   left: 0;
 }
</style>
