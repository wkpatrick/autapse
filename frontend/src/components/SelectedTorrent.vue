<template>
<div v-if="this.torrent">
    <h1>Selected Torrent!</h1>
    {{torrent.name}}
</div>
</template>

<script>
    export default {
        name: "SelectedTorrent",
        data(){
            return {

            }
        },
        props: {
            torrent: Object
        }
    }
</script>

<style scoped>

</style>
