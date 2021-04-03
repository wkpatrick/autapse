<template>
  <div v-if="this.torrent" style="overflow-y: scroll; height: 100%">
    <b>Selected Torrent!</b><br>
    {{ torrent.name }}<br>
    <b>Error: </b><br>
    {{torrent.error}}<br>
    <b>Created</b><br>
    {{ torrent.created }}<br>
    <b>Save Location</b><br>
    {{ torrent.path }}<br>
    <b>Files</b><br>
    <span v-for="file in torrent.torrent_files" :key="file.id">
            {{ file.path }}<br>
        </span>
    <b>Trackers</b><br>
    <span v-for="tracker in torrent.tracker_urls" :key="tracker">
            {{ tracker }}<br>
        </span>
  </div>
</template>

<script>
export default {
  name: "SelectedTorrent",
  data() {
    return {}
  },
  props: {
    torrent: Object
  }
}
</script>

<style scoped>

</style>
