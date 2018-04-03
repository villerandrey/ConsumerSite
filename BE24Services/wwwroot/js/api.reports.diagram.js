(function ($) {
    $.widget("b24.reportDiagrams",
        {
            options: {
                dataSource: null,
                id: 0,
                form: null,
                searchInput: null,
                ui: {
                    companyInfoChart: null,
                    industryInfoChart: null,
                    marketInfoChart: null,
                    companiesRussianChart: null,
                    commodityTurnoverChart: null,
                    geoPolitikChart: null,
                    demandChart: null,
                    be24Chart: null,
                    company_info: null,
                    company_d_goriz_block: null,
                    buttonSortByName: null,
                    buttonSortByRost: null,
                    buttonSortByPoztiv: null,
                    buttonSortByNegativ: null,
                    buttonSortByRiski: null
                },
                sortAsc: true,
                titleAsc: true,
                rostPrcAsc: true,
                pozitivPrc: true,
                negativPrc: true,
                riskiPrc: true
            },
            _create: function () {
                var self = this, element = self.element, opts = self.options;
                for (var key in opts.ui) {
                    opts.ui[key] = element.find("." + key);
                }

                function sortByText(a, b) {
                    var aName = a[opts.sortField].toLowerCase();
                    var bName = b[opts.sortField].toLowerCase();
                    if (opts.sortAsc) {
                        return ((aName < bName) ? -1 : ((aName > bName) ? 1 : 0));
                    } else {
                        return ((aName > bName) ? -1 : ((aName < bName) ? 1 : 0));
                    }
                }

                function sortByNumber(a, b) {
                    var aInt = a[opts.sortField];
                    var bInt = b[opts.sortField];
                    if (opts.sortAsc) {
                        return ((aInt < bInt) ? -1 : ((aInt > bInt) ? 1 : 0));
                    } else {
                        return ((aInt > bInt) ? -1 : ((aInt < bInt) ? 1 : 0));
                    }
                }

                function sort(sortFunction, e) {
                    var sortAccDesc = opts.sortField + "Asc";
                    opts.sortAsc = opts[sortAccDesc];
                    console.log(opts.sortAsc);
                    e.removeClass('asc desc');
                    e.siblings().removeClass('asc desc');
                    if (opts.sortAsc) {
                        e.addClass('asc');
                    }
                    else {
                        e.addClass('desc');
                    }
                    opts[sortAccDesc] = !opts[sortAccDesc];
                    opts.marketArray = opts.marketArray.sort(sortFunction);
                    self.renderMarketList();

                }

                opts.ui.buttonSortByName.click(function () {
                    opts.sortField = "title";
                    sort(sortByText, $(this));
                });
                opts.ui.buttonSortByRost.click(function () {
                    opts.sortField = "rostPrc";
                    sort(sortByNumber, $(this));
                });
                opts.ui.buttonSortByPoztiv.click(function () {
                    opts.sortField = "pozitivPrc";
                    sort(sortByNumber, $(this));
                });
                opts.ui.buttonSortByNegativ.click(function () {
                    opts.sortField = "negativPrc";
                    sort(sortByNumber, $(this));
                });
                opts.ui.buttonSortByRiski.click(function () {
                    opts.sortField = "riskiPrc";
                    sort(sortByNumber, $(this));
                });


            },
            setChartData: function (block, data) {
                var self = this, element = self.element, opts = self.options;
                var summa = data.rost + data.pozitiv + data.negativ + data.riski;
                var countText = ((data.count !== undefined && summa !== 0) ? " (" + data.count.toString() + ") " : "");

                var caption;
                if (data.countToLeft === undefined || !data.countToLeft) {
                    caption = data.title + countText;
                } else {
                    caption = countText + "<br/>" + data.title;
                }

                block.find(".title").html(caption).attr("title", data.title);


                function isWebkit() {
                    return navigator.userAgent.match(/chrome/i) || navigator.userAgent.match(/opera/i);
                }
                var colors = isWebkit() ? ['#60c9cc', '#5eb568', '#fdb368', '#ff6550'] : ['#61c9cc', '#5fb569', '#fdb369', '#ff6651'];

                colors[0]

                block.find(".chart").drawDoughnutChart([
                    { title: "Рост", value: summa > 0 ? data.rost : 100, color: summa > 0 ? colors[0] : "#d2d2d2" },
                    { title: "Позитив", value: data.pozitiv, color: summa > 0 ? colors[1] : "#cccccc" },
                    { title: "Негатив", value: data.negativ, color: summa > 0 ? colors[2] : "#dedede" },
                    { title: "Риски", value: data.riski, color: summa > 0 ? colors[3] : "#d9d9d9" }
                ]);

                function getPercent(value) {
                    var percent = (summa > 0 ? (Math.round((value / summa) * 100)) : 0);
                    return "<span>" + percent.toString() + "%</span>";
                }

                block.find(".rost").html(getPercent(data.rost));
                block.find(".pozitiv").html(getPercent(data.pozitiv));
                block.find(".nagativ").html(getPercent(data.negativ));
                block.find(".riski").html(getPercent(data.riski));
            },

            open: function() {
                var self = this, element = self.element, opts = self.options;
                var emptyData = {
                    title: "",
                    count: null,
                    rost: 0,
                    pozitiv: 0,
                    negativ: 0,
                    riski: 0
                }

                opts.ui.company_d_goriz_block.html("");

                api.get(api.url.getBublikData, { bublictype: 1 }).done(function(obj) {
                    var data = obj.returnObject !== null ? obj.returnObject[0] : emptyData;
                    self.setChartData(opts.ui.companyInfoChart,
                        {
                            title: data.title,
                            count: data.count,
                            countToLeft: true,
                            rost: data.rost,
                            pozitiv: data.pozitiv,
                            negativ: data.negativ,
                            riski: data.riski
                        });
                });

                api.get(api.url.getBublikData, { bublictype: 2 }).done(function(obj) {
                    var data = obj.returnObject !== null ? obj.returnObject[0] : emptyData;
                    self.setChartData(opts.ui.industryInfoChart,
                        {
                            title: data.title,
                            count: data.count,
                            countToLeft: true,
                            rost: data.rost,
                            pozitiv: data.pozitiv,
                            negativ: data.negativ,
                            riski: data.riski
                        });
                });

                api.get(api.url.getBublikData, { bublictype: 3 }).done(function(obj) {
                    var data = obj.returnObject !== null ? obj.returnObject[0] : emptyData;
                    self.setChartData(opts.ui.marketInfoChart,
                        {
                            title: data.title,
                            count: data.count,
                            countToLeft: true,
                            rost: data.rost,
                            pozitiv: data.pozitiv,
                            negativ: data.negativ,
                            riski: data.riski
                        });
                });

                api.get(api.url.getReportBlockCompany).done(function(obj) {
                    var data = obj.returnObject, ci = opts.ui.company_info;
                    ci.find(".fullname").html(data ? data.fullname : "");
                    ci.find(".shortname").html(data ? data.shortname : "");
                    ci.find(".description").html(data ? data.description : "");
                    ci.find(".sectorName").html("<b>Отрасль:</b> " + (data ? data.sectorName : ""));
                    ci.find(".countryName").html("<b>Страна:</b> " + (data ? data.countryName : ""));
                    ci.find(".regionrfName").html("<b>Регион:</b> " + (data ? data.regionrfName : ""));
                    ci.find(".legaladdress").html(data ? data.legaladdress : "");
                    ci.find(".inn").html("<b>ИНН:</b> " + (data ? data.inn : ""));
                });

                api.get(api.url.getReportBlockRynki).done(function(obj) {
                    var array = obj.returnObject;
                    if ($.isArray(array) && array.length > 0) {
                        opts.marketArray = array;
                        self.computeMarketProcent();
                        self.renderMarketList();
                    }
                });


                api.get(api.url.getReportBublikMakro).done(function(obj) {
                    emptyData.title = "Компании РФ";
                    var data = (obj.returnObject !== null && obj.returnObject[0] !== null) ? obj.returnObject[0] : emptyData;
                    self.setChartData(opts.ui.companiesRussianChart,
                        {
                            title: "Компании РФ",
                            count: data.count,
                            rost: data.rost,
                            pozitiv: data.pozitiv,
                            negativ: data.negativ,
                            riski: data.riski
                        });

                    emptyData.title = "Товарооборот";
                    data = (obj.returnObject !== null && obj.returnObject[1] !== null) ? obj.returnObject[1] : emptyData;
                    self.setChartData(opts.ui.commodityTurnoverChart,
                        {
                            title: "Товарооборот",
                            count: data.count,
                            rost: data.rost,
                            pozitiv: data.pozitiv,
                            negativ: data.negativ,
                            riski: data.riski
                        });

                    emptyData.title = "Геополитика";
                    data = (obj.returnObject !== null && obj.returnObject[2] !== null) ? obj.returnObject[2] : emptyData;
                    self.setChartData(opts.ui.geoPolitikChart,
                        {
                            title: "Геополитика",
                            count: data.count,
                            rost: data.rost,
                            pozitiv: data.pozitiv,
                            negativ: data.negativ,
                            riski: data.riski
                        });

                    emptyData.title = "Спрос";
                    data = (obj.returnObject !== null && obj.returnObject[3] !== null) ? obj.returnObject[3] : emptyData;
                    self.setChartData(opts.ui.demandChart,
                        {
                            title: "Спрос",
                            count: data.count,
                            rost: data.rost,
                            pozitiv: data.pozitiv,
                            negativ: data.negativ,
                            riski: data.riski
                        });

                    emptyData.title = "Be24";
                    data = (obj.returnObject !== null && obj.returnObject[4] !== null) ? obj.returnObject[4] : emptyData;
                    self.setChartData(opts.ui.be24Chart,
                        {
                            title: "Be24",
                            count: data.count,
                            rost: data.rost,
                            pozitiv: data.pozitiv,
                            negativ: data.negativ,
                            riski: data.riski
                        });

                });
            },
            close: function() {
                var self = this, element = self.element, opts = self.options;
                $('.chart').empty();
                $('.chartGorizontal').empty();
                $('.chartGorizontal').css('width', '0px');
            },
            computeMarketProcent: function () {
                var self = this, element = self.element, opts = self.options;
                function getPercent(value, summa) {
                    var percent = (summa > 0 ? ((value / summa) * 100) : 0);
                    return percent;
                }
                $.each(opts.marketArray, function (index, data) {
                    var summa = data.rost + data.pozitiv + data.negativ + data.riski;
                    data.rostPrc = getPercent(data.rost, summa);
                    data.pozitivPrc = getPercent(data.pozitiv, summa);
                    data.negativPrc = getPercent(data.negativ, summa);
                    data.riskiPrc = getPercent(data.riski, summa);
                });
            },
            renderMarketList: function() {
                var self = this, element = self.element, opts = self.options;
                var block = opts.ui.company_d_goriz_block;
                block.html("");
                $.each(opts.marketArray, function (index, data) {
                    var title = data.title + " (" + data.count.toString() + ")";
                    var $html = $('<div class="line_list"><div class="title">' +
                        title +
                        '</div><div class="diagrama"><div class="chartGorizontal"></div></div></div>');
                    block.append($html);

                    var summa = data.rostPrc + data.pozitivPrc + data.negativPrc + data.riskiPrc;
                    
                    $html.find(".chartGorizontal").drawHorizontalChart([
                        {
                            title: "Рост",
                            value: summa > 0 ? data.rostPrc : 100,
                            color: summa > 0 ? "#61c9cc" : "#d2d2d2"
                        },
                        {
                            title: "Позитив",
                            value: data.pozitivPrc,
                            color: summa > 0 ? "#5fb569" : "#cccccc"
                        },
                        {
                            title: "Негатив",
                            value: data.negativPrc,
                            color: summa > 0 ? "#fdb369" : "#dedede"
                        },
                        {
                            title: "Риски",
                            value: data.riskiPrc, // data.riskiPrc,
                            color: summa > 0 ? "#ff6651" : "#d9d9d9"
                        }
                    ]);
                });
            },
            setOptions: function (options) {
                var self = this;
                options && $.extend(self.options, options);
            }
        });
})(jQuery);

(function ($) {
    jQuery.fn.drawHorizontalChart = function (data, options) {
        var elem = $(this);
        options = $.extend({
            width: 100,
            bg: "red"
        }, options);

        var make = function () {
            elem.css("width", options.width + '%');
            for (var i = 0, len = data.length; i < len; i++) {
                elem.append('<div class="elemGD" style="width: ' + data[i].value + '%; background: ' + data[i].color + ';"></div>');
            }
        };

        return this.each(make);

    };
})(jQuery);


(function ($, undefined) {
    $.fn.drawDoughnutChart = function (data, options) {
        var $this = this,
            W = $this.width(),
            H = $this.height(),
            centerX = W / 2,
            centerY = H / 2,
            cos = Math.cos,
            sin = Math.sin,
            PI = Math.PI,
            settings = $.extend({
                segmentShowStroke: true,
                segmentStrokeColor: "#0C1013",
                segmentStrokeWidth: 0,
                baseColor: "rgba(0,0,0,0)",
                baseOffset: 8,
                edgeOffset: 1,//offset from edge of $this
                percentageInnerCutout: 65,
                animation: true,
                animationSteps: 90,
                animationEasing: "easeInOutExpo",
                animateRotate: true,
                tipOffsetX: -8,
                tipOffsetY: -45,
                tipClass: "doughnutTip",
                summaryClass: "doughnutSummary",
                summaryTitle: "TOTAL:",
                summaryTitleClass: "doughnutSummaryTitle",
                summaryNumberClass: "doughnutSummaryNumber",
                beforeDraw: function () { },
                afterDrawed: function () { },
                onPathEnter: function (e, data) { },
                onPathLeave: function (e, data) { }
            }, options),
            animationOptions = {
                linear: function (t) {
                    return t;
                },
                easeInOutExpo: function (t) {
                    var v = t < .5 ? 8 * t * t * t * t : 1 - 8 * (--t) * t * t * t;
                    return (v > 1) ? 1 : v;
                }
            },
            requestAnimFrame = function () {
                return window.requestAnimationFrame ||
                    window.webkitRequestAnimationFrame ||
                    window.mozRequestAnimationFrame ||
                    window.oRequestAnimationFrame ||
                    window.msRequestAnimationFrame ||
                    function (callback) {
                        window.setTimeout(callback, 1000 / 60);
                    };
            }();

        settings.beforeDraw.call($this);

        var $svg = $('<svg width="' + W + '" height="' + H + '" viewBox="0 0 ' + W + ' ' + H + '" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink"></svg>').appendTo($this),
            $paths = [],
            easingFunction = animationOptions[settings.animationEasing],
            doughnutRadius = Min([H / 2, W / 2]) - settings.edgeOffset,
            cutoutRadius = doughnutRadius * (settings.percentageInnerCutout / 100),
            segmentTotal = 0;

        //Draw base doughnut
        var baseDoughnutRadius = doughnutRadius + settings.baseOffset,
            baseCutoutRadius = cutoutRadius - settings.baseOffset;
        $(document.createElementNS('http://www.w3.org/2000/svg', 'path'))
            .attr({
                "d": getHollowCirclePath(baseDoughnutRadius, baseCutoutRadius),
                "fill": settings.baseColor
            })
            .appendTo($svg);

        //Set up pie segments wrapper
        var $pathGroup = $(document.createElementNS('http://www.w3.org/2000/svg', 'g'));
        $pathGroup.attr({ opacity: 0 }).appendTo($svg);

        //Set up tooltip
        var $tip = $('<div class="' + settings.tipClass + '" />').appendTo('body').hide(),
            tipW = $tip.width(),
            tipH = $tip.height();

        //Set up center text area
        var summarySize = (cutoutRadius - (doughnutRadius - cutoutRadius)) * 2,
            $summary = $('<div class="' + settings.summaryClass + '" />')
                .appendTo($this)
                .css({
                    width: summarySize + "px",
                    height: summarySize + "px",
                    "margin-left": -(summarySize / 2) + "px",
                    "margin-top": -(summarySize / 2) + "px"
                });
        var $summaryTitle = $('<p class="' + settings.summaryTitleClass + '">' + settings.summaryTitle + '</p>').appendTo($summary);
        var $summaryNumber = $('<p class="' + settings.summaryNumberClass + '"></p>').appendTo($summary).css({ opacity: 0 });

        for (var i = 0, len = data.length; i < len; i++) {
            segmentTotal += data[i].value;
            $paths[i] = $(document.createElementNS('http://www.w3.org/2000/svg', 'path'))
                .attr({
                    "stroke-width": settings.segmentStrokeWidth,
                    "stroke": settings.segmentStrokeColor,
                    "fill": data[i].color,
                    "data-order": i
                })
                .appendTo($pathGroup)
                .on("mouseenter", pathMouseEnter)
                .on("mouseleave", pathMouseLeave)
                .on("mousemove", pathMouseMove);
        }

        //Animation start
        animationLoop(drawPieSegments);

        //Functions
        function getHollowCirclePath(doughnutRadius, cutoutRadius) {
            //Calculate values for the path.
            //We needn't calculate startRadius, segmentAngle and endRadius, because base doughnut doesn't animate.
            var startRadius = -1.570,// -Math.PI/2
                segmentAngle = 6.2831,// 1 * ((99.9999/100) * (PI*2)),
                endRadius = 4.7131,// startRadius + segmentAngle
                startX = centerX + cos(startRadius) * doughnutRadius,
                startY = centerY + sin(startRadius) * doughnutRadius,
                endX2 = centerX + cos(startRadius) * cutoutRadius,
                endY2 = centerY + sin(startRadius) * cutoutRadius,
                endX = centerX + cos(endRadius) * doughnutRadius,
                endY = centerY + sin(endRadius) * doughnutRadius,
                startX2 = centerX + cos(endRadius) * cutoutRadius,
                startY2 = centerY + sin(endRadius) * cutoutRadius;
            var cmd = [
                'M', startX, startY,
                'A', doughnutRadius, doughnutRadius, 0, 1, 1, endX, endY,//Draw outer circle
                'Z',//Close path
                'M', startX2, startY2,//Move pointer
                'A', cutoutRadius, cutoutRadius, 0, 1, 0, endX2, endY2,//Draw inner circle
                'Z'
            ];
            cmd = cmd.join(' ');
            return cmd;
        };
        function pathMouseEnter(e) {
            /*
            var order = $(this).data().order;
            $tip.text(data[order].title + ": " + data[order].value)
                .fadeIn(200);
            settings.onPathEnter.apply($(this), [e, data]);
            */
        }
        function pathMouseLeave(e) {
            /*
            $tip.hide();
            settings.onPathLeave.apply($(this), [e, data]);
            */
        }
        function pathMouseMove(e) {
            /*
            $tip.css({
                top: e.pageY + settings.tipOffsetY,
                left: e.pageX - $tip.width() / 2 + settings.tipOffsetX
            });
            */
        }
        function drawPieSegments(animationDecimal) {
            var startRadius = -PI / 2,//-90 degree
                rotateAnimation = 1;
            if (settings.animation && settings.animateRotate) rotateAnimation = animationDecimal;//count up between0~1

            drawDoughnutText(animationDecimal, segmentTotal);

            $pathGroup.attr("opacity", animationDecimal);

            //If data have only one value, we draw hollow circle(#1).
            if (data.length === 1 && (4.7122 < (rotateAnimation * ((data[0].value / segmentTotal) * (PI * 2)) + startRadius))) {
                $paths[0].attr("d", getHollowCirclePath(doughnutRadius, cutoutRadius));
                return;
            }
            for (var i = 0, len = data.length; i < len; i++) {
                var segmentAngle = rotateAnimation * ((data[i].value / segmentTotal) * (PI * 2)),
                    endRadius = startRadius + segmentAngle,
                    largeArc = ((endRadius - startRadius) % (PI * 2)) > PI ? 1 : 0,
                    startX = centerX + cos(startRadius) * doughnutRadius,
                    startY = centerY + sin(startRadius) * doughnutRadius,
                    endX2 = centerX + cos(startRadius) * cutoutRadius,
                    endY2 = centerY + sin(startRadius) * cutoutRadius,
                    endX = centerX + cos(endRadius) * doughnutRadius,
                    endY = centerY + sin(endRadius) * doughnutRadius,
                    startX2 = centerX + cos(endRadius) * cutoutRadius,
                    startY2 = centerY + sin(endRadius) * cutoutRadius;
                var cmd = [
                    'M', startX, startY,//Move pointer
                    'A', doughnutRadius, doughnutRadius, 0, largeArc, 1, endX, endY,//Draw outer arc path
                    'L', startX2, startY2,//Draw line path(this line connects outer and innner arc paths)
                    'A', cutoutRadius, cutoutRadius, 0, largeArc, 0, endX2, endY2,//Draw inner arc path
                    'Z'//Cloth path
                ];
                $paths[i].attr("d", cmd.join(' '));
                startRadius += segmentAngle;
            }
        }
        function drawDoughnutText(animationDecimal, segmentTotal) {
            $summaryNumber
                .css({ opacity: animationDecimal })
                .text((segmentTotal * animationDecimal).toFixed(1));
        }
        function animateFrame(cnt, drawData) {
            var easeAdjustedAnimationPercent = (settings.animation) ? CapValue(easingFunction(cnt), null, 0) : 1;
            drawData(easeAdjustedAnimationPercent);
        }
        function animationLoop(drawData) {
            var animFrameAmount = (settings.animation) ? 1 / CapValue(settings.animationSteps, Number.MAX_VALUE, 1) : 1,
                cnt = (settings.animation) ? 0 : 1;
            requestAnimFrame(function () {
                cnt += animFrameAmount;
                animateFrame(cnt, drawData);
                if (cnt <= 1) {
                    requestAnimFrame(arguments.callee);
                } else {
                    settings.afterDrawed.call($this);
                }
            });
        }
        function Max(arr) {
            return Math.max.apply(null, arr);
        }
        function Min(arr) {
            return Math.min.apply(null, arr);
        }
        function isNumber(n) {
            return !isNaN(parseFloat(n)) && isFinite(n);
        }
        function CapValue(valueToCap, maxValue, minValue) {
            if (isNumber(maxValue) && valueToCap > maxValue) return maxValue;
            if (isNumber(minValue) && valueToCap < minValue) return minValue;
            return valueToCap;
        }
        return $this;
    };
})(jQuery);

