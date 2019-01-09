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
            get TemplatePathAlternative() {
                return this.getPropertyValue("TemplatePathAlternative");
            }
            set TemplatePathAlternative(value) {
                this.setPropertyValue("TemplatePathAlternative", value);
            }
            tryGet(url, catchError) {
                let str = sessionStorage.getItem('templateContainer.' + url);
                if (str)
                    return str;
                let config = {
                    async: false,
                    url: url + "?" + moment().dayOfYear() + '_' + moment().hours() + '_' + moment().minutes() + '_' + moment().seconds(),
                    method: 'GET',
                    success: function (data) {
                        str = data;
                    }
                };
                if (catchError) {
                    config.error = function () {
                    };
                }
                $.ajax(config);
                if (str)
                    sessionStorage.setItem('templateContainer.' + url, str);
                return str;
            }
            initialize(type, index, id, properties) {
                super.initialize(type, index, id, properties);
                let that = this;
                let str = this.tryGet(this.TemplatePath, this.TemplatePathAlternative != null);
                if (!str && this.TemplatePathAlternative != null)
                    str = this.tryGet(this.TemplatePathAlternative, false);
                this.element.html(str);
            }
        }
        html.templateContainer = templateContainer;
    })(html = controls.html || (controls.html = {}));
})(controls || (controls = {}));
core.controlTypes['TemplateContainer'] = () => new controls.html.templateContainer();
for (let i = 0; i < sessionStorage.length; ++i) {
    let key = sessionStorage.key(i);
    if (key.startsWith('templateContainer.'))
        sessionStorage.removeItem(key);
}
//# sourceMappingURL=templateContainer.js.map