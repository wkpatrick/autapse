<template>
    <div id="app">
        <TorrentTable></TorrentTable>
    </div>
</template>

<script>
    import TorrentTable from "@/components/TorrentTable";

    export default {
        name: 'App',
        components: {
            TorrentTable: TorrentTable
        }
    }
</script>

<style lang="scss">
    @import "../node_modules/ag-grid-community/dist/styles/ag-grid.css";
    @import "../node_modules/ag-grid-community/dist/styles/ag-theme-alpine.css";

    #app {
        font-family: Avenir, Helvetica, Arial, sans-serif;
        -webkit-font-smoothing: antialiased;
        -moz-osx-font-smoothing: grayscale;
        color: #2c3e50;
    }

    html {
        overflow-y: auto !important;
    }

    select {
        overflow-y: auto !important;
    }
</style>
