<template>
  <div class="modal-card">
    <div class="modal-card-head">
      Settings
    </div>
    <section class="modal-card-body">
      <b-field label="Synapse IP Address" align="left">
        <b-input v-model="synapseIpAddress"></b-input>
      </b-field>
      <b-field label="Default Download Path" align="left">
        <b-input v-model="defaultFilePath"></b-input>
      </b-field>
    </section>
    <footer class="modal-card-foot">
      <b-button class="button is-success" @click="saveSettings">Save</b-button>
      <b-button class="button" @click="$parent.close()">Cancel</b-button>
    </footer>
  </div>
</template>

<script>
export default {
  name: "Settings",
  data() {
    return {
      defaultFilePath: "",
      synapseIpAddress: ""
    }
  },
  created() {
    this.loadSettings()
  },
  methods: {
    loadSettings() {
      let storage = window.localStorage
      this.synapseIpAddress = storage.getItem('synapseIpAddress');
      this.defaultFilePath = storage.getItem('defaultFilePath');
    },
    saveSettings() {
      let storage = window.localStorage
      storage.setItem('defaultFilePath', this.defaultFilePath);
      storage.setItem('synapseIpAddress', this.synapseIpAddress);
      this.$parent.close();
      location.reload();
    }
  }
}
</script>

<style scoped>

</style>
