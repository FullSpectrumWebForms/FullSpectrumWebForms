namespace controls.html {
    export class templateContainer extends div {

        get TemplatePath(): string {
            return this.getPropertyValue<this, string>("TemplatePath");
        }
        set TemplatePath(value: string) {
            this.setPropertyValue<this>("TemplatePath", value);
        }

        get TemplatePathAlternative(): string {
            return this.getPropertyValue<this, string>("TemplatePathAlternative");
        }
        set TemplatePathAlternative(value: string) {
            this.setPropertyValue<this>("TemplatePathAlternative", value);
        }

        tryGet(url: string, catchError: boolean) {
            let str = sessionStorage.getItem('templateContainer.' + url);
            if (str)
                return str;
            let config: any = {
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

        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);

            let that = this;
            let str = this.tryGet(this.TemplatePath, this.TemplatePathAlternative != null);

            if (!str && this.TemplatePathAlternative != null)
                str = this.tryGet(this.TemplatePathAlternative, false);

            this.element.html(str);
        }

    }
}
core.controlTypes['TemplateContainer'] = () => new controls.html.templateContainer();