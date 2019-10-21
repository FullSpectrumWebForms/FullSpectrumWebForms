declare var fsw_delayed_loader_refs: string[];

fsw_delayed_loader_refs = fsw_delayed_loader_refs.reverse();

var load = function () {
    if (fsw_delayed_loader_refs.length == 0) {
        core.manager.initialize();
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
        (script as HTMLLinkElement).href = src;
        (script as HTMLLinkElement).rel = "stylesheet";
    }
    else
        (script as HTMLScriptElement).src = src;
    document.head.appendChild(script); //or something of the likes
}

load();