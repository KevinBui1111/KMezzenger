String.prototype.format = function () {
    var s = this;
    for (var i = 0; i < arguments.length; i++) {
        var reg = new RegExp("\\(p" + i + "\\)", "g");
        s = s.replace(reg, arguments[i]);
    }

    return s;
}

String.prototype.endsWith = function (suffix) {
    return this.indexOf(suffix, this.length - suffix.length) !== -1;
};

function formatFromJson(val) {
    if (!val) return "";
    var dx = new Date(parseInt(val.substr(6)));

    return formatDate(dx, "dd/MM/yyyy");
}
function formatDatetimeFromJson(val) {
    if (!val) return "";
    var dx = new Date(parseInt(val.substr(6)));

    return formatDate(dx, "dd/MM/yyyy HH:mm");
}
function format_time_from_json(val) {
    if (!val) return "";
    var dx = new Date(parseInt(val.substr(6)));

    return formatDate(dx, "hh:mm tt");
}
function post_to_url(path, params) {
    var method = 'post';

    var form = document.createElement('form');
    form.setAttribute('method', method);
    form.setAttribute('action', path);

    for (var key in params) {
        if (params.hasOwnProperty(key)) {
            var hiddenField = document.createElement('input');
            hiddenField.setAttribute('type', 'hidden');
            hiddenField.setAttribute('name', key);
            hiddenField.setAttribute('value', params[key]);

            form.appendChild(hiddenField);
        }
    }

    document.body.appendChild(form);
    form.submit();
}

function clearFileInput(oldInput) {
    $(oldInput).replaceWith($(oldInput).clone());
}
// This optional function html-encodes messages for display in the page.
function htmlEncode(value) {
    var encodedValue = $('<div />').text(value).html();
    return encodedValue;
}
function decodeHTML(e) {
    return $("<div/>").html(e).text();
}

function ShowLoading() {
    $('#loadingDiv').show();
    $('#loadingDiv strong').after('<p>' + formatDate(new Date, 'dd-MM-yyyy HH:mm') + '</p>');
    $("#wrapDiv").toggleClass('blur');
}
function HideLoading() {
    $('#loadingDiv').hide();
    $("#wrapDiv").toggleClass('blur');
}
