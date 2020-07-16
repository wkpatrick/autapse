<template>
    <div>
        <b-progress type="is-info" :value="progressPercent" :max="1" :show-value="true"
                    :format="'percent'"></b-progress>
    </div>
</template>

<script>
    import Vue from 'vue';

    export default Vue.extend({ //Vue.Extend is REQUIRED
        name: "ProgressBarCell",
        data() {
            return {
                progressPercent: 0
            }
        },
        mounted() {
            this.progressPercent = this.params.value;
        },
        methods: {
            refresh(params) {
                this.params = params;
                this.progressPercent = this.params.value;
                return true;
            }
        }
    })
</script>

<style scoped>
</style>
