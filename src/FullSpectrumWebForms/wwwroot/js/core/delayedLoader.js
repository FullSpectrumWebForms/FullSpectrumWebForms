fsw_delayed_loader_refs = fsw_delayed_loader_refs.reverse();
var load = function () {
    if (fsw_delayed_loader_refs.length == 0) {
        let internalHandle = setInterval(function () {
            let pageId_ = $('#pageId');
            if (pageId_.length == 0)
                return;
            clearInterval(internalHandle);
            core.manager.initialize();
        }, 10);
        return;
    }
    let src = fsw_delayed_loader_refs.pop();
    if (!src) { // happens when there aren't any css files, FSW server will leave empty items for it. We can just skip them
        load();
        return;
    }
    let script = document.createElement(src.startsWith('/fsw.min.css') ? 'link' : 'script');
    script.onload = function () {
        load();
    };
    if (src.startsWith('/fsw.min.css')) {
        script.href = src;
        script.rel = "stylesheet";
    }
    else
        script.src = src;
    document.head.appendChild(script); //or something of the likes
};
load();
//# sourceMappingURL=delayedLoader.js.map