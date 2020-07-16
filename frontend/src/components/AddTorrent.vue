<template>
    <div class="modal-card">
        <div class="modal-card-head">
            Add Torrent
        </div>
        <section class="modal-card-body">
            <b-table :data="uploadedTorrents" :selected.sync="selectedUploadedTorrent">
                <template slot-scope="props">
                    <b-table-column field="name" label="Name">
                        {{props.row.displayName}}
                    </b-table-column>
                </template>
            </b-table>
            <b-field label="Start Download">
                <b-checkbox v-model="selectedUploadedTorrent.start"></b-checkbox>
            </b-field>
            <b-field label="Download Path" align="left">
                <b-input v-model="filePath"></b-input>
            </b-field>
            <b-field label="Path But Not Typed" align="left">
                <b-select placeholder="File Location" v-model="selectedPath" expanded v-on:input="loadDirectory"
                          v-bind:loading="pathListLoading" v-bind:disabled="pathListLoading">
                    <option
                            v-for="option in paths"
                            :value="option"
                            :key="option">
                        {{option}}
                    </option>
                </b-select>
            </b-field>
            <section v-on:input="addTorrentFromFile($event)">
                <b-field>
                    <b-upload v-model="dropFiles"
                              multiple
                              drag-drop
                              accept=".torrent">
                        <a class="button is-primary">
                            <b-icon icon="upload"></b-icon>
                            <span>Click to upload</span>
                        </a>
                    </b-upload>
                </b-field>
            </section>

            <b-field label="Torrent URL" align="left">
                <b-input v-model="torrentURL"></b-input>
            </b-field>
            <b-button class="button is-success" @click="addTorrentFromURL">Download Torrent</b-button>
        </section>
        <footer class="modal-card-foot">
            <b-button class="button is-success" @click="addTorrent">Add Torrent</b-button>
            <b-button class="button" @click="$parent.close()">Cancel</b-button>
        </footer>
    </div>
</template>

<script>

    export default {
        name: "AddTorrent",
        data() {
            return {
                torrentURL: "",
                filePath: "",
                dropFiles: [],
                uploadedTorrents: [],
                selectedUploadedTorrent: {
                    path: "",
                    start: false
                },
                paths: [],
                selectedPath: "",
                pathListLoading: false
            }
        },
        components: {},
        created() {
            this.filePath = window.localStorage.getItem('defaultFilePath');
            this.getInitialDirectories();
        },
        mounted() {
        },
        methods: {
            addTorrent() {
                this.$emit('add-torrent', JSON.parse(JSON.stringify(this.uploadedTorrents)));
                this.$parent.close();
            },
            addTorrentFromFile(event) {
                this.processNewTorrent(event.target.files)
            },
            addTorrentFromURL() {
                let self = this;
                fetch('/api/torrent/url', {
                    method: 'POST',
                    body: JSON.stringify([this.torrentURL]),
                    headers: {'Content-type': 'application/json'}
                })
                    .then(function (response) {
                        return response.json()
                    })
                    .then(function (json) {
                        self.uploadedTorrents.push(json[0])
                    })
            },
            processNewTorrent(fileList) {
                let formData = new FormData();
                for (var i = 0; i != fileList.length; i++) {
                    formData.append("files", fileList[i]);
                }
                this.selectedUploadedTorrent = {
                    path: "",
                    start: false
                };
                let self = this;
                fetch('/api/torrent/file', {
                    method: 'POST',
                    body: formData
                })
                    .then(function (response) {
                        return response.json()
                    })
                    .then(function (json) {
                        json.forEach(torrent => self.uploadedTorrents.push(torrent))
                    })
            },
            toBuffer(ab) { //from https://stackoverflow.com/questions/8609289/convert-a-binary-nodejs-buffer-to-javascript-arraybuffer
                var buf = Buffer.alloc(ab.byteLength);
                var view = new Uint8Array(ab);
                for (var i = 0; i < buf.length; ++i) {
                    buf[i] = view[i];
                }
                return buf;
            },
            onSelectionChanged() {
                let selectedRow = this.gridApi.getSelectedRows();
                this.selectedUploadedTorrent = selectedRow;
            },
            getInitialDirectories() {
                this.loadDirectory('')
            },
            async loadDirectory(path) {
                if (path === '..') {
                    path = this.goUpDirectory(this.filePath)
                }
                this.pathListLoading = true;
                this.filePath = path;
                this.paths = [];
                this.paths = await fetch('/api/location/relative', {
                    method: 'POST',
                    body: JSON.stringify(path),
                    headers: {'Content-type': 'application/json'}
                })
                    .then(function (response) {
                        return response.json()
                    })
                    .then(function (json) {
                        return json
                    })
                this.pathListLoading = false;
                this.selectedPath = null; //So that it doenst auto-select .. when > 1 dir deep. When that happens, it wont update when you try to select it again
                if (this.filePath.length > 1) {
                    this.paths.unshift('..')
                }
            },
            goUpDirectory(path) {
                let separator = '\\'
                if (!path.includes(separator)) {
                    separator = '/'
                }
                let split = path.split(separator)
                let cutoff = split.length - 1;
                let retVal = ''
                for (let i = 0; i < cutoff; i++) {
                    if (i > 0) {
                        retVal += separator;
                    }
                    retVal += split[i]
                }
                return retVal
            }
        },
        watch: {
            filePath: function (newPath) {
                //Handling updating the selected torrent path
                if (this.selectedUploadedTorrent.infoHash) {
                    this.selectedUploadedTorrent.path = newPath;
                }
            },
            selectedUploadedTorrent: function (newSelected,) {
                if (newSelected.path) {
                    this.filePath = newSelected.path;
                    this.loadDirectory(this.filePath)
                } else {
                    this.filePath = '';
                    this.loadDirectory(this.filePath)
                }
            }
        }
    }
</script>

<style scoped>

</style>
