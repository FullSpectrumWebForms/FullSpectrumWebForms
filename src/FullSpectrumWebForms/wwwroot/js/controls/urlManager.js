var controls;
(function (controls) {
    class urlManager extends core.controlBase {
        initialize(type, index, id, properties) {
            super.initialize(type, index, id, properties);
        }
        updateUrlWithoutReloading(data) {
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
        redirect(data) {
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
        openNewTab(data) {
            if (data.parameters) {
                let keys = Object.keys(data.parameters);
                let params = [];
                for (let i = 0; i < keys.length; ++i)
                    params.push(encodeURIComponent(keys[i]) + '=' + encodeURIComponent(data.parameters[keys[i]]));
                if (params.length != 0)
                    data.url += '?' + params.join('&');
            }
            window.open(data.url);
        }
    }
    controls.urlManager = urlManager;
})(controls || (controls = {}));
core.controlTypes['UrlManager'] = () => new controls.urlManager();
//# sourceMappingURL=urlManager.js.map