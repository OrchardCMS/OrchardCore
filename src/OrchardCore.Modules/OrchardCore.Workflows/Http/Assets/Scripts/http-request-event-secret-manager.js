
function initializeHttpRequestEventSecretManager(element) {
    if (!element) {
        return;
    }

    var vueMultiselect = Vue.component('vue-multiselect', window.VueMultiselect.default);

    var v = new Vue({
        el: element,
        components: { 'vue-multiselect': vueMultiselect },
        data: function () {
            var secretData = JSON.parse(element.dataset.secretData);
            var workflowTypeId = $('[data-workflow-type-unique-id]').data('workflow-type-unique-id');
            var activityId = $('[data-activity-id]').data('activity-id');

            var antiforgeryHeaderName = $('[data-antiforgery-header-name]').data('antiforgery-header-name');
            var antiforgeryToken = $('[data-antiforgery-token]').data('antiforgery-token');
            var headers = {};

            headers[antiforgeryHeaderName] = antiforgeryToken;

            return {
                secrets: secretData.secrets,
                selected: secretData.selected,
                workflowTypeId: workflowTypeId,
                activityId: activityId,
                linkUrl: secretData.linkUrl,
                createUrl: secretData.createUrl,
                headers: headers,
                linkError: secretData.linkError,
                createError: secretData.createError
            }
        },
        computed: {
            isLinked() {
                return this.selected.name === 'None' ||
                    (this.selected.activityId === this.activityId &&
                        this.selected.workflowTypeId === this.workflowTypeId);
            },
            hasSecret() {
                return this.selected.name !== 'None';
            }
        },
        methods: {
            create(secretName) {
                var self = this;
                $.ajax({
                    url: self.createUrl,
                    method: 'POST',
                    headers: self.headers,
                    data: {
                        secretName: secretName,
                        workflowTypeId: self.workflowTypeId,
                        activityId: self.activityId,
                        tokenLifeSpan: $('#token-lifespan').val()
                    },
                    success: function (data) {
                        self.secrets.push(data);
                        self.selected = data;
                    },
                    error: function () {
                        alert(self.createError);
                    }
                });
            },
            linkSecret() {
                var self = this;
                $.ajax({
                    url: self.linkUrl,
                    method: 'POST',
                    headers: self.headers,
                    data: {
                        secretName: self.selected.name,
                        workflowTypeId: self.workflowTypeId,
                        activityId: self.activityId,
                        tokenLifeSpan: $('#token-lifespan').val()
                    },
                    success: function (data) {
                        self.selected.workflowTypeId = data.workflowTypeId;
                        self.selected.activityId = data.activityId;
                    },
                    error: function () {
                        alert(self.linkError);
                    }
                });
            }
        }
    });
}
