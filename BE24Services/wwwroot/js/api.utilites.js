

kendo.data.binders.widget.mask = kendo.data.Binder.extend({
    init: function (widget, bindings, options) {
        kendo.data.Binder.fn.init.call(this, widget.element[0], bindings, options);
        this.widget = widget;
    },
    refresh: function () {
        var value = this.bindings.mask.get();
        this.widget.setOptions({ mask: value });
    }
});

api.disableFormEnterKeySubmit = function (form) {
    form.on('keyup keypress', function (e) {
        var keyCode = e.keyCode || e.which;
        if (keyCode === 13) {
            e.preventDefault();
            return false;
        }
    });
}

api.toDate = function toDate(str) {
    try {
        return new Date(str);
    } catch (e) {
        return (null);
    }
}

api.consts = {
    classifiers: {
        demands: 3,
        parts: 4,
        categories: 2,
        indicators: 1,
        regions: 5,
        parameters: 30,
        industries: 31,
        markets: 32,
        countries: 33,
        macroRegions: 7
    },
    defaults: {
        russia: 330175
    }
}







$.browser = {
    msie: function() {
        return (navigator.appName === 'Microsoft Internet Explorer') || navigator.userAgent.match(/Trident\/./i);
    }
}

api.showValidationError = function(validator) {
    var errors = validator.errors();
    var text = "Ошибка!<br>";
    for (var i = 0; i < errors.length; i++) {
        text += (errors[i] + "<br/>");
    }
    api.showErrorMessage(text);
}

api.checkPassword = function(p, e) {
    var password = p ? p.trim() : "", email = e ? e.trim() : "";
    if (!password) return (false);
    if (password.length < 7) return (false);
    if (password.indexOf(email) >= 0) return (false);
    if (email.indexOf("@")) {
        var data = email.split("@");
        if (password.indexOf(data[0]) >= 0) return (false);
    }
    return (true);
}

api.showErrorMessage = function (message, opts) {
    var options = { life: 10000, theme: "_Error_State", corners: "" };
    $.jGrowl(message, opts ? $.extend(options, opts) : options);
}

api.showSuccessMessage = function (message, opts) {
    var options = { life: 3000, theme: "_Info_State", corners: "" };
    $.jGrowl(message, opts ? $.extend(options, opts) : options);
}

api.showWarningMessage = function (message, opts) {
    var options = { life: 10000, theme: "_Warning_State", corners: "" };
    $.jGrowl(message, opts ? $.extend(options, opts) : options);
}

api.getStandardClassifierDataSource = function (id, pageSize) {
    return new kendo.data.DataSource({
        transport: {
            read: function (operation) {
                api.get(api.url.getStandardClassifier, {
                    classifierId: id
                }).done(function (ret) {
                    var obj = ret.returnObject;
                    var gridData = [];
                    for (var key in obj.elements) {
                        var item = obj.elements[key];
                        gridData.push({
                            id: item.id,
                            name: item.name,
                            systemname: item.systemname,
                            description: item.description
                        });
                    }
                    operation.success(gridData);
                });
            }
        },
        schema: {
            model: {
                fields: {
                    id: { type: "number" },
                    name: { type: "string" },
                    systemname: { type: "string" },
                    description: { type: "string" }
                }
            }
        },
        pageSize: pageSize
    });
},

api.getCategoryName = function(category) {
    switch (category) {
        default:
        case 200:
            return "Компания";
        case 201:
            return "Отрасль";
        case 202:
            return "Рынок";
        case 203:
            return "Спрос";
            return "";
    }
}

api.getClassifierData = function(obj, isDataSource) {
    var ret = [];
    for (var key in obj.elements) {
        var item = obj.elements[key];
        isDataSource ? ret.push({ id: item.id, name: item.name }) : ret.push(item);
    }
    return (ret);
}

api.block = function(block) {
    var blockScreen = $("#blockScreen");
    block ? blockScreen.addClass("open") : blockScreen.removeClass("open");
}

api.getCurrentPath = function (url) {
    var strHttpServer = location.hostname; 
    var port = location.port === "" ? "80" : location.port;
    var currentPathName = unescape(location.pathname);
    var currentPath = currentPathName.substring(0, currentPathName.lastIndexOf("/"));
    currentPath = location.protocol + "//" + strHttpServer + ":" + port; //+ currentPath;
    return (currentPath + url);
}
api.sessionOut = function () {
    api.showErrorMessage("Сервер закрыл сессию");
    setTimeout(function () {
        document.location.href = api.getCurrentPath("");
    }, 2000);
}

api.get = function (url, data) {
    var deferred = $.Deferred();
    $.getJSON(api.getCurrentPath(url), data)
        .done(function (result, status) {
            if (status === 'success' && (result.d || result)) {
                var resData = result.d || result;
                if (resData.message.isError) {
                    if (resData.message.messageCode === "5001") {
                        deferred.reject({ message: 'Ajax error' });
                        api.sessionOut();
                    } else {
                        api.showErrorMessage(resData.message.messageText + "(" + url + ")");
                        deferred.resolve(resData);
                    }
                } else {
                    deferred.resolve(resData);
                }
            } else {
                deferred.reject({ message: 'Ajax error' });
                document.location.href = api.getCurrentPath("");
            }
        })
        .fail(function (result) {
            if (result.status === 0 || result.readyState === 0) {
                return;
            }
            deferred.reject({ message: result.statusText });
        });
    return deferred;
};

$.postJSON = function (url, data, callback) {
    return jQuery.ajax({
        'type': 'POST',
        'url': url,
        'contentType': 'application/json',
        'data': $.toJSON(data),
        'dataType': 'json',
        'success': callback
    });
};

api.post = function (url, data) {
    var deferred = $.Deferred();
    var dataStr = JSON.stringify(data);
    $.ajax({
        url: api.getCurrentPath(url),
        data: dataStr,
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataFilter: function (data) { return data; },
        success: function (data) {
            var resData = data.d || data;
            if (resData.message.isError) {
                if (resData.message.messageCode === "5001") {
                    deferred.reject({ message: 'Ajax error' });
                    api.sessionOut();
                } else {
                    api.showErrorMessage(resData.message.messageText + "(" + url + ")");
                    deferred.resolve(resData);
                }
            } else {
                deferred.resolve(resData);
            }
        },
        error: function (xmlHttpRequest, textStatus, errorThrown) {
            deferred.reject({ message: textStatus });
            document.location.href = api.getCurrentPath("");
        }
    });
    return deferred;
};

api.getRangeWeek = function(start) {
    start = start || 0;
    var today = new Date();
    today.setHours(0, 0, 0, 0);
    var day = today.getDay() - start;
    var date = today.getDate() - day + 1;
    var last = date + 6;
    var range = {
        dateStart: new Date(new Date().setDate(date)),
        dateEnd: new Date(new Date().setDate(last))
    }
    if (range.dateEnd > today) range.dateEnd = today;
    return (range);
}

api.getRangeMonth = function() {
    var date = new Date();
    var range = {
        dateStart: new Date(date.getFullYear(), date.getMonth(), 1),
        dateEnd: new Date(date.getFullYear(), date.getMonth() + 1, 0)
    }
    if (range.dateEnd > date) range.dateEnd = date;
    return (range);
}

api.getRangeQuarter = function() {
    var today = new Date(), quarter = Math.floor((today.getMonth() / 3));
    var dateStart = new Date(today.getFullYear(), quarter * 3, 1);
    var dateEnd = new Date(dateStart.getFullYear(), dateStart.getMonth() + 3, 0);
    if (dateEnd > today) dateEnd = today;
    return ({ dateStart: dateStart, dateEnd: dateEnd });
}

api.getRangeYear = function () {
    var today = new Date();
    var range = {
        dateStart: new Date(today.getFullYear(), 0, 1),
        dateEnd: new Date(today.getFullYear(), 11, 31)
    }
    if (range.dateEnd > today) range.dateEnd = today;
    return (range);
}






api.openWindowByPost = function(url, data, title) {
    var dataStr = JSON.stringify(data);
    $.ajax({
        url: url,
        data: dataStr,
        type: 'POST',
        success: function (response) {
            var w = window.open();
            $(w.document.body).html(response);
            if (title) w.document.title = title;
        }
    });
}

api.getDateRange = function (rangeType, isKendo) {
    var ret;
    switch (rangeType) {
        case "today":
            var today = new Date();
            ret = ({ dateStart: today, dateEnd: today });
            break;
        case "week":
            ret = api.getRangeWeek();
            break;
        case "month":
            ret = api.getRangeMonth();
            break;
        case "quartal":
            ret = api.getRangeQuarter();
            break;
        case "year":
            ret = api.getRangeYear();
            break;
        default:
            return (null);
    }
    if (isKendo) {
        return {
            dateStart: kendo.toString(kendo.parseDate(ret.dateStart), 'dd.MM.yyyy'),
            dateEnd: kendo.toString(kendo.parseDate(ret.dateEnd), 'dd.MM.yyyy')
        }
    }
    return (ret);
}







