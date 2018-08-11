var gen;
(function (gen) {
    var utility;
    (function (utility) {
        function setCurrentTitle(title) {
            $('#MU_Title').text(title);
            document.title = title;
        }
        utility.setCurrentTitle = setCurrentTitle;
        function setMenuIcon(menu, icon) {
            var span_icon = menu.children().children();
            span_icon.removeAttr('class');
            span_icon.addClass(icon);
        }
        utility.setMenuIcon = setMenuIcon;
        function setMenuText(menu, text) {
            var children = menu.children();
            var c = children.children().remove(); // remove the icon
            var t = children.text(text); // set the text
            children.prepend(c); // add the icon in front of the text
        }
        utility.setMenuText = setMenuText;
        function addMenuOption(onClick, text, icon) {
            var menu = $('<a class="app-bar-element" data-flexdirection="reverse" style= "margin-top: 0px; margin-bottom: 0px; font-size:0.8rem"> </a>');
            menu.append($('<span style="padding-left: 10px;" > <span class="' + icon + '" style= "padding-right: 5px;" > </span>' + text + '</span>'));
            if (typeof onClick === 'string')
                menu.attr('onclick', onClick);
            else if (onClick)
                menu.click(onClick);
            $('#ContentMenuOptions').append(menu);
            return menu;
        }
        utility.addMenuOption = addMenuOption;
    })(utility = gen.utility || (gen.utility = {}));
})(gen || (gen = {}));
//# sourceMappingURL=po_utility.js.map