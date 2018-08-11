if (!Array.prototype.find) {
    Object.defineProperty(Array.prototype, 'find', {
        enumerable: false,
        configurable: true,
        writable: true,
        value: function (predicate) {
            if (this == null) {
                throw new TypeError('Array.prototype.find called on null or undefined');
            }
            if (typeof predicate !== 'function') {
                throw new TypeError('predicate must be a function');
            }
            var list = Object(this);
            var length = list.length >>> 0;
            var thisArg = arguments[1];
            var value;
            for (var i = 0; i < length; i++) {
                if (i in list) {
                    value = list[i];
                    if (predicate.call(thisArg, value, i, list)) {
                        return value;
                    }
                }
            }
            return undefined;
        }
    });
}
if (!Array.prototype.findIndex) {
    Object.defineProperty(Array.prototype, 'findIndex', {
        enumerable: false,
        configurable: true,
        writable: true,
        value: function (predicate) {
            if (this == null) {
                throw new TypeError('Array.prototype.find called on null or undefined');
            }
            if (typeof predicate !== 'function') {
                throw new TypeError('predicate must be a function');
            }
            var list = Object(this);
            var length = list.length >>> 0;
            var thisArg = arguments[1];
            var value;
            for (var i = 0; i < length; i++) {
                if (i in list) {
                    value = list[i];
                    if (predicate.call(thisArg, value, i, list)) {
                        return i;
                    }
                }
            }
            return -1;
        }
    });
}
if (!Array.prototype.includes) {
    Object.defineProperty(Array.prototype, 'includes', {
        enumerable: false,
        configurable: true,
        writable: true,
        value: function (value) {
            if (this == null) {
                throw new TypeError('Array.prototype.includes called on null or undefined');
            }
            var list = Object(this);
            var length = list.length >>> 0;
            var thisArg = arguments[1];
            var value;
            for (var i = 0; i < length; i++) {
                if (list[i] === value)
                    return list[i];
            }
            return undefined;
        }
    });
}
function call_function(a) {
    a();
}
var gen_tools = {};
var gen_utility;
(function (gen_utility) {
    let select2;
    (function (select2_1) {
        function _getSelect2(select2) {
            if (typeof select2 === "string")
                return $((select2.charAt(0) === '#' ? '' : '#') + select2);
            else if (select2 instanceof jQuery)
                return select2;
            return $(select2);
        }
        select2_1._getSelect2 = _getSelect2;
        ;
        function getSelectedValuesAndIds(select2_) {
            var select2 = _getSelect2(select2_);
            var data = select2.select2('data');
            if (!data)
                return [];
            var res = [];
            for (var i = 0; i < data.length; ++i)
                res[i] = { id: data[i].id, value: data[i].text };
            return res;
        }
        select2_1.getSelectedValuesAndIds = getSelectedValuesAndIds;
        ;
        // return a single select2 id or the first if tags
        // return null if nothing
        // provide id, jquery object or element object
        function getSelectedID(select2_) {
            var select2 = _getSelect2(select2_);
            var values = select2.val();
            if (values) {
                if (typeof values === "string")
                    return values;
                return values[0];
            }
            return null;
        }
        select2_1.getSelectedID = getSelectedID;
        ;
        // return a single select2 text or the first if tags
        // return null if nothing
        // provide text, jquery object or element object
        function getSelectedText(select2_) {
            var select2 = _getSelect2(select2_);
            var data = select2.select2('data');
            if (!data || !data[0])
                return "";
            var res = data[0].text;
            return res;
        }
        select2_1.getSelectedText = getSelectedText;
        ;
        // get all the ids in a select2 ( probably a tag select2, but not needed... )
        // return null if nothing
        // provide id, jquery object or element object
        function getSelectedIDs(select2_) {
            var select2 = _getSelect2(select2_);
            var values = select2.val();
            if (values) {
                if (typeof values === "string")
                    return [values];
                return values;
            }
            return null;
        }
        select2_1.getSelectedIDs = getSelectedIDs;
        ;
        // empty a select2 of its values
        function empty(select2_, ignoreChangeTrigger = false) {
            var select2 = _getSelect2(select2_);
            select2.empty();
            if (!ignoreChangeTrigger)
                select2.trigger('change');
        }
        select2_1.empty = empty;
        ;
        // set the selected values of a select2 using a selector. Provided value and index
        function setSelectedBySelector(select2_, values, func) {
            var select2 = _getSelect2(select2_);
            if (getOptions(select2).ajax)
                setValueBySelector(select2, values, func);
            else {
                if (!$.isArray(values))
                    values = [values];
                var values_ = [];
                var ids = [];
                for (var i = 0; i < values.length; ++i) {
                    var cur = func(values[i], i);
                    values_[i] = cur.value;
                    ids[i] = cur.id;
                }
                setSelected(select2, values_, ids);
            }
        }
        select2_1.setSelectedBySelector = setSelectedBySelector;
        ;
        function isTags(select2) {
            return getOptions(select2).tags;
        }
        select2_1.isTags = isTags;
        // set the selected value of a select2
        // if a non-ajax select2, then the value will be ignore, and the id will be used instead
        function setSelected(select2_, value, id) {
            var select2 = this._getSelect2(select2_);
            if (isAjax(select2) || isTags(select2))
                setValue(select2, value, id);
            else
                setSelectedById(select2, id);
        }
        select2_1.setSelected = setSelected;
        ;
        // set the selected value of a non-ajax select2
        // for setting selected value of a ajax select2, use setValue or setSelected
        function setSelectedById(select2_, id) {
            var select2 = _getSelect2(select2_);
            if (isTags(select2) || isAjax(select2))
                setValue(select2, id, id);
            select2.val(id);
            select2.trigger('change');
        }
        select2_1.setSelectedById = setSelectedById;
        ;
        // set the value of a select2
        // key and id can be a string or a string array in case of multiple tags
        // if no key(s) is specified, select2 will be left empty
        // if the select2 is not a tag ajax, all other options are removed, and these ones are put
        function setValue(select2_, value, id) {
            var select2 = _getSelect2(select2_);
            empty(select2, true);
            if (!id)
                id = value;
            if (typeof value === "string") {
                value = [value];
                id = [id];
            }
            else if (!value) {
                select2.trigger('change');
                return;
            }
            var options = getOptions(select2);
            var isTagOrAjax = options.tags === true || options.ajax;
            for (var i = 0; i < value.length; ++i) {
                if (!isTagOrAjax)
                    select2.append($("<option></option>").val(id[i]).text(value[i]));
                else
                    select2.append($("<option selected></option>").val(id[i]).text(value[i]));
            }
            select2.trigger('change');
        }
        select2_1.setValue = setValue;
        ;
        // like setValue, but provide an entire object. Keys will be ids
        function setValueByObjectKeys(select2, values) {
            var ids = [];
            var keys = Object.keys(values);
            for (var i = 0; i < keys.length; ++i)
                ids[i] = values[keys[i]];
            this.setValue(select2, keys, ids);
        }
        select2_1.setValueByObjectKeys = setValueByObjectKeys;
        ;
        // like setValue, but value is get through a selector. Provided an id + id_index
        function setValueByIdSelector(select2, ids, func) {
            var values = [];
            for (var i = 0; i < ids.length; ++i)
                values[i] = func(ids[i], i);
            setValue(select2, values, ids);
        }
        select2_1.setValueByIdSelector = setValueByIdSelector;
        ;
        // like setValue, but get value and id through a selector. Provided a value
        // must return {id,value}
        function setValueBySelector(select2, values, func) {
            if (!$.isArray(values))
                values = [values];
            var ids = [];
            var values_ = [];
            for (var i = 0; i < values.length; ++i) {
                var res = func(values[i]);
                ids.push(res.id);
                values_.push(res.value);
            }
            setValue(select2, values_, ids);
        }
        select2_1.setValueBySelector = setValueBySelector;
        ;
        function isMultiple(select2) {
            var options = getOptions(select2);
            if (!options)
                return false;
            return options.multiple;
        }
        select2_1.isMultiple = isMultiple;
        ;
        function isAjax(select2) {
            var options = getOptions(select2);
            if (!options)
                return false;
            return options.ajax;
        }
        select2_1.isAjax = isAjax;
        ;
        // get the select2 options
        function getOptions(select2_) {
            var select2 = _getSelect2(select2_);
            var data = select2.data('select2');
            if (!data)
                return undefined;
            return data.options.options;
        }
        select2_1.getOptions = getOptions;
    })(select2 = gen_utility.select2 || (gen_utility.select2 = {}));
    ;
    let url;
    (function (url_1) {
        function getParameter(name) {
            return decodeURIComponent((new RegExp('[?|&]' + name + '=' + '([^&;]+?)(&|#|;|$)').exec(location.search) || [null, ''])[1].replace(/\+/g, '%20')) || null;
        }
        url_1.getParameter = getParameter;
        ;
        function getParameters(names) {
            var parameters = {};
            for (var i = 0; i < names.length; ++i)
                parameters[names[i]] = this.getParameter(names[i]);
            return parameters;
        }
        url_1.getParameters = getParameters;
        ;
        function change(url, title, state) {
            if (title)
                document.title = title;
            window.history.pushState(state, document.title, url);
        }
        url_1.change = change;
        ;
        function setParameter(name, value) {
            var o = {};
            o[name] = value;
            this.setParameters(o);
        }
        url_1.setParameter = setParameter;
        ;
        function setParameters(values) {
            var keys = Object.keys(values);
            var url = null;
            for (var i = 0; i < keys.length; ++i)
                url = this._UpdateQueryString(keys[i], values[keys[i]], url);
            this.change(url);
        }
        url_1.setParameters = setParameters;
        ;
        function getWithoutParameter() {
            return document.location.protocol + '//' + document.location.host + document.location.pathname;
        }
        url_1.getWithoutParameter = getWithoutParameter;
        ;
        function _UpdateQueryString(key, value, url) {
            if (!url)
                url = window.location.href;
            var re = new RegExp("([?&])" + key + "=.*?(&|#|$)(.*)", "gi"), hash;
            if (re.test(url)) {
                if (typeof value !== 'undefined' && value !== null)
                    return url.replace(re, '$1' + key + "=" + value + '$2$3');
                else {
                    hash = url.split('#');
                    url = hash[0].replace(re, '$1$3').replace(/(&|\?)$/, '');
                    if (typeof hash[1] !== 'undefined' && hash[1] !== null)
                        url += '#' + hash[1];
                    return url;
                }
            }
            else {
                if (typeof value !== 'undefined' && value !== null) {
                    var separator = url.indexOf('?') !== -1 ? '&' : '?';
                    hash = url.split('#');
                    url = hash[0] + separator + key + '=' + value;
                    if (typeof hash[1] !== 'undefined' && hash[1] !== null)
                        url += '#' + hash[1];
                    return url;
                }
                else
                    return url;
            }
        }
        url_1._UpdateQueryString = _UpdateQueryString;
    })(url = gen_utility.url || (gen_utility.url = {}));
    ;
    function filterFloat(value) {
        if (/^(\-|\+)?([0-9]+(\.[0-9]+)?|Infinity)$/
            .test(value))
            return Number(value);
        return NaN;
    }
    gen_utility.filterFloat = filterFloat;
    function filterInt(value) {
        if (/^(\-|\+)?([0-9]+(\[0-9]+)?|Infinity)$/
            .test(value))
            return parseInt(value);
        return NaN;
    }
    gen_utility.filterInt = filterInt;
    function showMessage(caption, content, type) {
        new Noty({
            layout: 'topCenter',
            theme: 'mint',
            timeout: 3000,
            progressBar: true,
            text: caption + ': ' + content,
            type: type,
            animation: {
                open: 'noty_effects_open',
                close: 'noty_effects_close'
            },
        }).show();
        // $.Notify({
        //     caption: caption,
        //     content: content,
        //     type: type
        // });
    }
    gen_utility.showMessage = showMessage;
})(gen_utility || (gen_utility = {}));
var utility = gen_utility; // old version support
//# sourceMappingURL=utility.js.map