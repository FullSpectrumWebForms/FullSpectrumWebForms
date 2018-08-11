namespace controls.html {
    export class templateContainer extends div {

        get TemplatePath(): string {
            return this.getPropertyValue<this, string>("TemplatePath");
        }
        set TemplatePath(value: string) {
            this.setPropertyValue<this>("TemplatePath", value);
        }


        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
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
}
core.controlTypes['TemplateContainer'] = () => new controls.html.templateContainer();