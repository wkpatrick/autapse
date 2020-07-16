<template>
    <div id="layout-container">
        <div id="buttons" align="left">
            <b-button @click="isAddTorrentModalActive = true" type="is-success">Add Torrent</b-button>
            <b-button @click="deleteTorrent" type="is-danger">Remove Torrent</b-button>
            <b-button @click="pauseTorrent">Pause Torrent</b-button>
            <b-button @click="resumeTorrent">Resume Torrent</b-button>
            <b-button @click="isSettingsModalActive = true">Settings</b-button>
        </div>
        <div id="menu">
            <b-select native-size="9" multiple expanded style="overflow: hidden" v-model="stateFilter">
                <option
                        v-for="option in globalStates"
                        :value="option.state"
                        :key="option.state">
                    {{ option.formatted }}
                </option>
            </b-select>
            <b-select native-size="9" multiple expanded v-model="trackerFilter">
                <option
                        v-for="option in globalTrackerList"
                        :value="option.tracker"
                        :key="option.tracker">
                    {{ option.formatted }}
                </option>
            </b-select>
        </div>
        <div id="torrents">
            <ag-grid-vue style="width: 100%; height: 100%;"
                         class="ag-theme-alpine"
                         :columnDefs="columnDefs"
                         :defaultColDef="defaultColDef"
                         :rowData="filteredTorrents"
                         :gridOptions="gridOptions"
                         :frameworkComponents="frameworkComponents"
                         :rowSelection="rowSelection"
                         @selection-changed="onSelectionChanged">
            </ag-grid-vue>
        </div>
        <div id="selected-torrent">
            <SelectedTorrent v-bind:torrent="checkedTorrents[0]"></SelectedTorrent>
        </div>

        <div id="server-info">
            <ServerInfo v-bind:server="server"></ServerInfo>
        </div>

        <b-modal :active.sync="isAddTorrentModalActive" has-modal-card>
            <AddTorrent v-on:add-torrent="addTorrent"></AddTorrent>
        </b-modal>

        <b-modal :active.sync="isSettingsModalActive" has-modal-card>
            <Settings></Settings>
        </b-modal>
    </div>
</template>

<script>
    import bytes from 'bytes';
    import parseTorrent from 'parse-torrent';
    import {AgGridVue} from "ag-grid-vue";
    import SelectedTorrent from "@/components/SelectedTorrent";
    import AddTorrent from "@/components/AddTorrent";
    import ServerInfo from "@/components/ServerInfo";
    import Settings from "@/components/Settings";
    import ProgressBarCell from "@/components/ProgressBarCell";

    export default {
        name: "TorrentTable",
        // eslint-disable-next-line vue/no-unused-components
        components: {Settings, ServerInfo, AddTorrent, SelectedTorrent, AgGridVue, ProgressBarCell},
        data() {
            return {
                server: {},
                torrentIDs: [],
                torrents: {},
                checkedTorrents: [],
                serial: 1,
                isAddTorrentModalActive: false,
                isSettingsModalActive: false,
                bufferedTorrents: [], //When we want to upload a .torrent, we send an UPLOAD_TORRENT, and stash the BufferArray here to upload
                columnDefs: [
                    {
                        headerName: 'Name',
                        field: 'name',
                        resizable: true,
                        cellStyle: {'text-align': 'left'},
                        checkboxSelection: true
                    },
                    {
                        headerName: 'Size',
                        field: 'size',
                        resizable: true,
                        valueFormatter: this.agFormatBytes,
                        type: 'rightAligned'
                    },
                    {
                        headerName: 'Progress', field: 'progress', resizable: true, cellRenderer: 'progressBarRenderer',
                        cellStyle: {'padding-top': '8px', 'padding-bottom': '8px'}, //This should be improved somehow
                    },
                    {
                        headerName: 'Rate Down',
                        field: 'rate_down',
                        resizable: true,
                        valueFormatter: this.agFormatBitsPerSecond, type: 'rightAligned'
                    },
                    {
                        headerName: 'Rate Up',
                        field: 'rate_up',
                        resizable: true,
                        valueFormatter: this.agFormatBitsPerSecond, type: 'rightAligned'
                    },
                    {
                        headerName: 'Downloaded',
                        field: 'transferred_down',
                        resizable: true,
                        valueFormatter: this.agFormatBytes,
                        type: 'rightAligned'
                    },
                    {
                        headerName: 'Uploaded',
                        field: 'transferred_up',
                        resizable: true,
                        valueFormatter: this.agFormatBytes,
                        type: 'rightAligned'
                    },
                    {headerName: 'Status', field: 'status', resizable: true},
                ],
                defaultColDef: {
                    sortable: true
                },
                gridOptions: {},
                gridApi: {},
                rowSelection: 'multiple',
                frameworkComponents: {
                    progressBarRenderer: ProgressBarCell
                },
                menuActive: true,
                stateFilter: [],
                trackerFilter: []
            }
        },
        created() {
            this.$options.sockets.onopen = () => this.createInitialFilters();
            // Listen for messages
            this.$options.sockets.onmessage = (data) => this.parseMessage(JSON.parse(data.data));
        },
        mounted() {
            this.gridApi = this.gridOptions.api;
            this.gridApi.sizeColumnsToFit();
        },
        destroyed() {
            delete this.$options.sockets.onmessage;
        },
        methods: {
            createInitialFilters() {
                let torrentMessage = {
                    type: "FILTER_SUBSCRIBE",
                    kind: "torrent",
                    criteria: []
                };
                this.sendData(torrentMessage);

                let serverMessage = {
                    type: "FILTER_SUBSCRIBE",
                    kind: "server",
                    criteria: []
                };
                this.sendData(serverMessage);

                let filesMessage = {
                    type: "FILTER_SUBSCRIBE",
                    kind: "file",
                    criteria: []
                };

                this.sendData(filesMessage);
            },
            parseMessage(message) {
                console.log('Recieved from server: ' + JSON.stringify(message))
                console.log(message.type)
                switch (message.type) {
                    case "RPC_VERSION":
                        console.log("recvd RPC version");
                        break;
                    case "RESOURCES_EXTANT":
                        message.ids.forEach(id => {
                            if (id.includes('.')) {//Server ids are formatted like 0.1.0-I7OmTRMddcEzYxM compared to torrent ids, which are all uppercase alphanumeric
                                this.server.id = id;
                                let subMsg = {};
                                subMsg.type = "SUBSCRIBE"
                                subMsg.ids = [this.server.id];
                                this.sendData(subMsg)
                            } else {
                                this.torrentIDs.push(id) //This includes torrent and file IDs
                            }
                        });
                        this.updateSubscribes();
                        break;
                    case "UPDATE_RESOURCES":
                        console.log('updating resources');
                        this.updateTorrents(message.resources);
                        break;
                    case "RESOURCES_REMOVED":
                        console.log('Deleting resources: ' + message.ids);
                        this.removeTorrents(message.ids);
                        break;
                    case "TRANSFER_OFFER":
                        console.log('Sending torrent with serial: ' + message.serial)
                        this.handleTorrentUpload(message)

                }
            },
            updateSubscribes() {
                let subMsg = {};
                subMsg.type = "SUBSCRIBE"
                subMsg.ids = this.torrentIDs
                this.sendData(subMsg)
            },
            updateTorrents(resources) {
                resources.forEach(resource => {
                    if (resource.type === 'server') {
                        this.server = Object.assign({}, this.server, resource)
                    } else {
                        switch (resource.type) {
                            case "torrent":
                                if (this.torrents[resource.id]) {
                                    resource = Object.assign(this.torrents[resource.id], resource)
                                }
                                this.$set(this.torrents, resource.id, resource);
                                break;
                            case "file":
                                if (this.torrents[resource.torrent_id]) {
                                    // eslint-disable-next-line no-empty
                                    if (!this.torrents[resource.torrent_id].torrent_files) {
                                        this.$set(this.torrents[resource.torrent_id], 'torrent_files', {});
                                    }
                                    if (this.torrents[resource.torrent_id].torrent_files[resource.id]) {
                                        resource = Object.assign(this.torrents[resource.torrent_id].torrent_files[resource.id], resource)
                                        this.$set(this.torrents[resource.torrent_id].torrent_files, resource.id, resource);
                                    } else {
                                        this.$set(this.torrents[resource.torrent_id].torrent_files, resource.id, resource);
                                    }
                                }
                                break;
                        }
                    }
                });
                this.gridApi.refreshCells();
            },
            removeTorrents(ids) {
                ids.forEach(id => {
                    if (this.torrents[id]) {
                        this.$delete(this.torrents, id);
                    }
                })
            },
            sendData(data) {
                data.serial = this.serial;
                this.$socket.send(JSON.stringify(data))
                this.serial++;
            },
            formatBytes(value) {
                return bytes(value)
            },
            agFormatBytes(params) {
                return bytes(params.value)
            },
            formatBitsPerSecond(bits, decimals = 2) {  //Modified from https://stackoverflow.com/questions/15900485/correct-way-to-convert-size-in-bytes-to-kb-mb-gb-in-javascript
                if (bits === 0) {
                    return '';
                }

                const k = 1024;
                const dm = decimals < 0 ? 0 : decimals;
                const sizes = ['Bits/s', 'KiB/s', 'MiB/s', 'GiB/s', 'TiB/s', 'PiB/s', 'EiB/s', 'ZiB/s', 'YiB/s'];

                const i = Math.floor(Math.log(bits) / Math.log(k));

                return parseFloat((bits / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
            },
            agFormatBitsPerSecond(params) {
                let decimals = 2;
                let bits = params.value;
                if (bits === 0) {
                    return '';
                }

                const k = 1024;
                const dm = decimals < 0 ? 0 : decimals;
                const sizes = ['Bits/s', 'KiB/s', 'MiB/s', 'GiB/s', 'TiB/s', 'PiB/s', 'EiB/s', 'ZiB/s', 'YiB/s'];

                const i = Math.floor(Math.log(bits) / Math.log(k));

                return parseFloat((bits / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
            },
            deleteTorrent() {
                this.checkedTorrents.forEach(torrent => {
                    let msg = {
                        type: "REMOVE_RESOURCE",
                        id: torrent.id,
                        artifacts: false
                    }
                    this.sendData(msg)
                })

            },
            async addTorrent(torrentList) {
                console.log('Adding torrents')
                console.log(torrentList);
                for (let torrent of torrentList) {
                    let msg = {}
                    if (torrent.torrentURL && torrent.torrentURL.includes('magnet:?')) {
                        msg = {
                            type: "UPLOAD_MAGNET",
                            uri: torrent.torrentURL,
                            path: torrent.path,
                            start: torrent.start
                        }
                    } else {
                        console.log('Downloading cached torrent')
                        let data = await this.getCachedTorrent(torrent.infoHash)
                        console.log(data)

                        let tempbuf = this.toBuffer(this._base64ToArrayBuffer(data.data));
                        console.log(Buffer.isBuffer(tempbuf));
                        console.log(parseTorrent(tempbuf));

                        msg = {
                            type: "UPLOAD_TORRENT",
                            size: tempbuf.byteLength,
                            path: torrent.path,
                            start: torrent.start
                        }

                        let parsedTorrent = {file: tempbuf, serial: this.serial}
                        this.bufferedTorrents.push(parsedTorrent)
                        /**
                         //TODO: Redo
                         //Fetch the torrent from the url
                         await fetch(torrent.torrentURL)
                         .then(response => this.checkResponseStatus(response) && response.arrayBuffer())
                         .then(buffer => {
                                console.log(buffer.byteLength)
                                msg = {
                                    type: "UPLOAD_TORRENT",
                                    size: buffer.byteLength,
                                    path: torrent.filePath,
                                    start: torrent.startTorrent
                                }
                                let torrent = {file: buffer, serial: this.serial}
                                //We need to wait for a TRANSFER_OFFER from the server, so we store this torrent until we get one with a matching serial
                                this.bufferedTorrents.push(torrent)
                            })
                         **/
                    }
                    this.sendData(msg)
                }
            },
            getCachedTorrent(hash) {
                return fetch('api/torrent/saved', {
                    method: 'POST',
                    body: JSON.stringify(hash),
                    headers: {'Content-type': 'application/json'}
                })
                    .then(function (response) {
                        return response.json()
                    })
                    .then(function (json) {
                        console.log(json)
                        return json;
                    })
            },
            async handleTorrentUpload(offer) {
                for (let i = 0; i < this.bufferedTorrents.length; i++) {
                    if (this.bufferedTorrents[i].serial === offer.serial) {
                        let response = await fetch('http://10.0.0.130:8412', { //TODO: Load ip from config
                            method: 'POST',
                            headers: {'Authorization': 'Bearer ' + offer.token},
                            body: this.bufferedTorrents[i].file
                        })
                        if (response.ok) {
                            console.log('good POST')
                        }
                        console.log(response)
                        break;
                    }
                }
            },
            resumeTorrent() {
                this.checkedTorrents.forEach(torrent => {
                    let msg = {
                        type: "RESUME_TORRENT",
                        id: torrent.id
                    }
                    this.sendData(msg)
                })
            },
            pauseTorrent() {
                this.checkedTorrents.forEach(torrent => {
                    let msg = {
                        type: "PAUSE_TORRENT",
                        id: torrent.id
                    }
                    this.sendData(msg)
                })

            },
            checkResponseStatus(response) {
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status} - ${response.statusText}`);
                }
                return response;
            },
            onSelectionChanged() {
                this.checkedTorrents = this.gridApi.getSelectedRows();
            },
            toBuffer(ab) { //from https://stackoverflow.com/questions/8609289/convert-a-binary-nodejs-buffer-to-javascript-arraybuffer
                var buf = Buffer.alloc(ab.byteLength);
                var view = new Uint8Array(ab);
                for (var i = 0; i < buf.length; ++i) {
                    buf[i] = view[i];
                }
                return buf;
            },
            _base64ToArrayBuffer(base64) { //from https://stackoverflow.com/questions/21797299/convert-base64-string-to-arraybuffer/21797381
                var binary_string = window.atob(base64);
                var len = binary_string.length;
                var bytes = new Uint8Array(len);
                for (var i = 0; i < len; i++) {
                    bytes[i] = binary_string.charCodeAt(i);
                }
                return bytes.buffer;
            }
        },
        computed: {
            filteredTorrents: function () {
                let vals = Object.values(this.torrents);
                vals = vals.filter(torrent => {
                    let stateFilterRetVal = false;
                    let trackerFilterRetVal = false;
                    if (this.stateFilter !== []) {
                        if (this.stateFilter.includes('all') || this.stateFilter.length === 0) {
                            return true;
                        }
                        stateFilterRetVal = this.stateFilter.includes(torrent.status);
                    }
                    if (this.trackerFilter !== []) {
                        let retVal = false;
                        torrent.tracker_urls.forEach(url => {
                            if (this.trackerFilter.includes('all') || this.trackerFilter.includes(url)) {
                                retVal = true;
                            }
                        })
                        trackerFilterRetVal = retVal;
                    }
                    return stateFilterRetVal || trackerFilterRetVal;
                })
                return vals;
            },
            globalTrackerList: function () {
                let trackers = {};
                let trackerList = [];
                //Gather all of the tracker urls into a dictionary
                let torrents = Object.values(this.torrents);
                torrents.forEach(torrent => {
                    torrent.tracker_urls.forEach(tracker => {
                        if (trackers[tracker]) {
                            trackers[tracker]++;
                        } else {
                            trackers[tracker] = 1;
                        }
                    })
                })
                trackerList.push({
                    'tracker': 'all',
                    'count': '∞',
                    'formatted': 'all (∞)'
                })

                Object.keys(trackers).forEach(tracker => {
                    trackerList.push({
                        'tracker': tracker,
                        'count': trackers[tracker],
                        'formatted': tracker + ' (' + trackers[tracker] + ')'
                    });
                })
                return trackerList;
            },
            globalStates: function () {
                let states = ["all", "paused", "pending", "leeching", "idle", "seeding", "hashing", "magnet", "error"]
                let formattedStates = [];
                let torrents = Object.values(this.torrents)
                states.forEach(state => {
                    let matches = torrents.filter(torrent => {
                        if (state === "all") {
                            return true
                        }
                        return torrent.status === state;
                    })
                    formattedStates.push({
                        "state": state,
                        "count": matches.length,
                        "formatted": state + ' (' + matches.length + ')'
                    })
                })
                return formattedStates;
            }
        }
    }
</script>

<style scoped>
    #layout-container {
        display: grid;
        grid-template-columns: 10% 90%;
        grid-template-rows: 10vh 50vh 30vh 10vh;
        grid-template-areas:
                "buttons buttons"
                "menu torrents"
                "menu selected-torrent"
                "server-info server-info"
    }

    #buttons {
        grid-area: buttons;
    }

    #torrents {
        grid-area: torrents;
    }

    #selected-torrent {
        grid-area: selected-torrent;
    }

    #server-info {
        grid-area: server-info;
    }

    #menu {
        grid-area: menu;
    }
</style>
