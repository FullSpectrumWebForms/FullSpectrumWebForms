declare var Cookies: {
    get: (name: string) => any,
    set: (name: string, value: any) => void
};

declare var Noty: any;
namespace gen.utility {

    export function setCurrentTitle(title: string): void {
        $('#MU_Title').text(title);
        document.title = title;
    }
    export function setMenuIcon(menu: JQuery, icon: string) {
        var span_icon = menu.children().children();
        span_icon.removeAttr('class');
        span_icon.addClass(icon);
    }
    export function setMenuText(menu: JQuery, text: string) {
        var children = menu.children();
        var c = children.children().remove(); // remove the icon
        var t = children.text(text); // set the text
        children.prepend(c); // add the icon in front of the text
    }
    export function addMenuOption(onClick: any, text: string, icon: string): JQuery {
        var menu = $('<a class="app-bar-element" data-flexdirection="reverse" style= "margin-top: 0px; margin-bottom: 0px; font-size:0.8rem"> </a>');
        menu.append($('<span style="padding-left: 10px;" > <span class="' + icon + '" style= "padding-right: 5px;" > </span>' + text + '</span>'));

        if (typeof onClick === 'string')
            menu.attr('onclick', onClick);
        else if (onClick)
            menu.click(onClick);

        $('#ContentMenuOptions').append(menu);
        return menu;
    }

}