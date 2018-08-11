namespace gen {

    function __handleAjaxCommunicationError(options: JQueryAjaxSettings) {
        const divID = '__ajaxCommErrorDiv';
        if (!document.getElementById(divID))// not init yet
        {
            var ajaxCommErrorDiv = $('<div id="' + divID + '" data-role="dialog" data-overlay="true" data-width="25rem" data-height="200px" data-close-button="false" data-overlay-color="#222222" ></div>');
            ajaxCommErrorDiv.append($('<h1 class="text-light"> Connection perdu </h1>'));
            ajaxCommErrorDiv.append($('<hr class="thin bg-orange"/>'));

            ajaxCommErrorDiv.append("<div>Une erreur est survenus lors de la connection au serveur.</div>");
            ajaxCommErrorDiv.append("<div>Reconnection en cours, veuillez patientez...</div>");
            ajaxCommErrorDiv.append("<br/>");
            ajaxCommErrorDiv.append("<br/>");
            ajaxCommErrorDiv.append("<div>Si le problème persiste, contactez votre administrateur réseau</div>");

            ajaxCommErrorDiv.appendTo('body');
        }
        setTimeout(function () {
            $('#' + divID).data('dialog').open();

            var previousSuccessFunction = options.success as any;

            options.success = function (data: any, textStatus: string, jqXHR: JQueryXHR) {
                $('#' + divID).data('dialog').close();
                previousSuccessFunction(data, textStatus, jqXHR);
            }

            setTimeout(function () {
                $.ajax(options);
            }, 5000);
        }, 10);
    }
    export function modSQL(storedProcedureName: string, parameters: any): JQueryDeferred<any> {
        return gen.ajax({
            url: "/GenServices.asmx/ModSQL",
            data: {
                method: storedProcedureName,
                param: parameters
            }
        });
    }
    var _alreadyInFastLoginMode = false; // true if an ajax called return 'not login' and the user as not entered it's login info yet ( 17/01/2017, PAR )
    // you need to configure 'success' and 'data' and 'url'
    export function ajax(options: JQueryAjaxSettings, doNotParseToJson?: boolean): JQueryDeferred<any> {
        options = $.extend({
            data: {},
            contentType: "application/json; charset=utf-8",
            type: "POST",
            dataType: "json",
        }, options);

        if (typeof options.data != "string" && !doNotParseToJson)
            options.data = JSON.stringify(options.data);

        var callbacksOptions: {
            complete?(jqXHR: JQueryXHR, textStatus: string): any;
            success?(data: any, textStatus: string, jqXHR: JQueryXHR): any
        } = {};
        // remove all callbacks from the options
        if (options.complete) {
            callbacksOptions.complete = options.complete as any;
            delete options.complete;
        }
        callbacksOptions.success = options.success as any;

        var deferred = $.Deferred<any>();
        var error = options.error as any;
        options.error = function (xhr, ajaxOptions, thrownError) {
            if (xhr.readyState == 0) {
                // Network error (i.e. connection refused, access denied due to CORS, etc.)
                __handleAjaxCommunicationError(options);
            }
            else if (xhr.status == 500 && xhr.responseJSON && xhr.responseJSON.Message == "NO_ACCESS") {
                if (_alreadyInFastLoginMode) { // if an other ajax call is already showing the login form ( 17/01/2017, PAR )
                    var handle = setInterval(function () {// wait 'till the login is completed by the other ajax call, and then finish this current ajax call
                        if (!_alreadyInFastLoginMode) {
                            clearInterval(handle);
                            $.ajax(options);
                        }
                    }, 500);
                }
                else { // it's the first ajax call that got an NO_ACCESS message
                    _alreadyInFastLoginMode = true;
                    var iFrame = $('<iframe src="/Login.aspx" width="100%" height="100%" style="position:absolute;top:0px;left:0px;z-index:99999"></iframe>');
                    var counter = 0;
                    iFrame.on('load', function () {
                        if (counter) { // login completed
                            iFrame.remove();
                            _alreadyInFastLoginMode = false;
                            $.ajax(options);
                        }
                        ++counter;
                    });
                    $('body').append(iFrame);
                }
            }
            else if (error) {
                error(xhr, ajaxOptions, thrownError);
                if (callbacksOptions.complete)
                    callbacksOptions.complete(xhr, xhr.statusText);
                deferred.reject(xhr);
            }
            else {
                alert(xhr.responseText);
                if (callbacksOptions.complete)
                    callbacksOptions.complete(xhr, xhr.statusText);
                deferred.reject(xhr);
            }
        };
        options.success = function (data: any, textStatus: string, jqXHR: JQueryXHR) {
            if (callbacksOptions.success)
                callbacksOptions.success(data, textStatus, jqXHR);
            if (callbacksOptions.complete)
                callbacksOptions.complete(jqXHR, jqXHR.statusText);
            deferred.resolve(data);
        }

        $.ajax(options);

        return deferred;
    }
    export function ajax_sync(options: JQueryAjaxSettings): JQueryDeferred<any> {
        options.async = false;
        return this.ajax(options);
    }
}
var gen_ajax = gen;