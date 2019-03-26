namespace controls {

    export class urlManager extends core.controlBase {


        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);
        }

        updateUrlWithoutReloading(data: { url: string, parameters: { [name: string]: string } }) {

            if (data.parameters) {
                let keys = Object.keys(data.parameters);
                let params = [];
                for (let i = 0; i < keys.length; ++i)
                    params.push(encodeURIComponent(keys[i]) + '=' + encodeURIComponent(data.parameters[keys[i]]));
                if (params.length != 0)
                    data.url += '?' + params.join('&');
            }

            window.history.pushState(null, null, data.url);
        }
        redirect(data: { url: string, parameters: { [name: string]: string } }) {
            if (data.parameters) {
                let keys = Object.keys(data.parameters);
                let params = [];
                for (let i = 0; i < keys.length; ++i)
                    params.push(encodeURIComponent(keys[i]) + '=' + encodeURIComponent(data.parameters[keys[i]]));
                if (params.length != 0)
                    data.url += '?' + params.join('&');
            }
            window.location.replace(data.url);
        }
    }
}
core.controlTypes['UrlManager'] = () => new controls.urlManager();