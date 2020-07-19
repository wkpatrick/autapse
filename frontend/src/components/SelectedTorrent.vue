<template>
    <div v-if="this.torrent">
        <h1>Selected Torrent!</h1>
        {{torrent.name}}
        <h2>Created</h2>
        {{torrent.created}}
        <h2>Save Location</h2>
        {{torrent.path}}
        <h2>Files</h2>
        <span v-for="file in torrent.torrent_files" :key="file.id">
            {{file.path}}
        </span>
        <h2>Trackers</h2>
        <span v-for="tracker in torrent.tracker_urls" :key="tracker">
            {{tracker}}
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
