var controls;
(function (controls) {
    var html;
    (function (html) {
        class templateContainer extends html.div {
            get TemplatePath() {
                return this.getPropertyValue("TemplatePath");
            }
            set TemplatePath(value) {
                this.setPropertyValue("TemplatePath", value);
            }
            initialize(type, index, id, properties) {
                super.initialize(type, index, id, properties);
                let str = sessionStorage.getItem('templateContainer.' + this.TemplatePath);
                if (!str) {
                    $.ajax({
                        async: false,
                        url: this.TemplatePath + "?" + moment().dayOfYear() + '_' + moment().hours() + '_' + moment().minutes() + '_' + moment().seconds(),
                        method: 'GET',
                        success: function (data) {
                            str = data;
                        }
                    });
                    sessionStorage.setItem('templateContainer.' + this.TemplatePath, str);
                }
                this.element.html(str);
            }
        }
        html.templateContainer = templateContainer;
    })(html = controls.html || (controls.html = {}));
})(controls || (controls = {}));
core.controlTypes['TemplateContainer'] = () => new controls.html.templateContainer();
//# sourceMappingURL=templateContainer.js.map