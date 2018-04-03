(function ($) {
    $.widget("kendo.fileList", {
        options: {
            ui: {
                items: null,
                btnAdd: null,
                btnClear: null,
                uploaderArea: null
            },
            uploadHandler: null,
            onUploadStarted: function () { },
            onUploadFinished: function (result) { },
            onUploadRemove: function() { return $.Deferred().resolve(); },
            maxChunkSize: 5000000, // 5Mb
            maxFileSize: '250mb',
            onClear: null,
            onRemove: null,
            data: []
        },
        getCurrentPath: function (url) {
            var strHttpServer = location.hostname; 
            var port = location.port === "" ? "80" : location.port;
            var currentPathName = unescape(location.pathname);
            var currentPath = currentPathName.substring(0, currentPathName.lastIndexOf("/"));
            currentPath = location.protocol + "//" + strHttpServer + ":" + port + currentPath;
            return (currentPath + url);
        },
        _create: function () {
            var self = this, element = self.element, opts = self.options, ui = opts.ui;
            var html =
                '<div deselectable="on">' +
                    '<ul deselectable="on" class="items" class="k-reset">' +
                        '<li class="k-button btnAdd" deselectable="on">' +
                            '<span deselectable="on">Новый</span><span unselectable="on" class="k-select"></span><span style="color: green;" class="k-icon k-i-add"></span>' +
                            '<input class="uploaderArea" type="file" name="files" size="1" style="cursor: pointer; margin-left: -25px; -moz-opacity: 0; filter: alpha(opacity=0); opacity: 0; width:40px;">'+
                        '</li>' +
                        '<li class="k-button btnClear" deselectable="on">' +
                            '<span deselectable="on">Очистить</span><span unselectable="on"  class="k-select"></span><span style="color: red;" class="k-icon k-i-delete"></span>' +
                        '</li>' +
                    '</ul>' +
                '</div>';

            element.html(html);
            for (var key in ui) ui[key] = element.find("." + key);

            ui.uploaderArea.attr({
                'id': 'upload_button_1',
                'data-url': self.getCurrentPath(opts.uploadHandler)
            }).fileupload({
                dataType: 'json',
                autoUpload: false,
                maxFileSize: opts.maxFileSize,
                maxChunkSize: opts.maxChunkSize,
                singleFileUploads: true,
                add: function (e, data) {
                    if (data.files[0].size && data.files[0].size > opts.maxFileSize) {
                        api.showWarningMessage('Загрузка вложения невозможна, файл ' + data.files[0].name + ' больше ' + opts.maxFileSize.toUpperCase());
                        return;
                    }
                    var item = {
                        id: -1,
                        filename: data.files[0].name,
                        name: data.files[0].name,
                        size: data.files[0].size,
                        status: "attach",
                        uploadInfo: data
                    }
                    self.addItem(item);
                },
                done: function (e, data) {
                    if (data.result) {
                        if (data.result.Error && data.result.Error.Message) {
                            $.wb.util.showErrorMessage(data.result.Error.Message);
                            return;
                        }
                        var file = data.result.ReturnObject;
                        for (var i = 0; i < opts.data.length; i++) {
                            if (opts.data[i].status === "attach") {
                                var item = opts.data[i].file;
                                if (item.name === file.name && item.size === file.size && file.eof) {
                                    item.id = file.id;
                                    item.status = "unchanged";
                                }
                            }
                        }
                    }
                }
            }).bind('fileuploadsubmit', function (e, data) {
                if ($.isFunction(opts.getAdditionalData)) {
                    data.formData = opts.getAdditionalData();
                } else {
                    data.formData = {};
                }
            });
            ui.btnClear.click(function() {
                opts.data = [];
                ui.items.find(".fileItem").remove();
                self.updateClearButton();
            }).hide();
        },
        updateClearButton: function () {
            var self = this, opts = self.options, ui = opts.ui;
            ui.btnClear.toggle(opts.data.length > 1);
        },
        addItem: function (file) {
            var self = this, element = self.element, opts = self.options, ui = opts.ui;
            var $html = $('<li class="k-button fileItem" deselectable="on">' +
                '<a class="loadFile"><span deselectable="on">' +
                file.name +
                '</span></a><span unselectable="on" aria-label="delete" class="k-select">' +
                '<span class="k-icon k-i-close"></span></span></li>');
            var node = { html: $html, file: file }
            opts.data.push(node);
            $html.find(".k-i-close").bind("click",
                function (e) {
                    for (var i = 0; i < opts.data.length; i++) {
                        if (opts.data[i].html === node.html) {
                            node.html.remove();
                            node.file.status = null;
                            opts.data.splice(i, 1);
                            self.updateClearButton();
                        }
                    }
                });
            $html.find(".loadFile").bind("click", function () {
                window.location.href = self.getCurrentPath(api.url.download + "?attachmentId=" + file.id);
            });
            ui.items.find('>li:nth-last-child(2)').before($html);
            self.updateClearButton();
        },
        getData: function() {
            var self = this, opts = self.options, ret = [];
            $.each(opts.data, function (i, item) {
                ret.push({ id: item.file.id, name: item.file.name });
            });
            return (ret);
        },
        value: function(files) {
            var self = this, opts = self.options, ui = opts.ui;
            if (!files) return (self.getData());
            opts.data = [];
            ui.items.find(".fileItem").remove();
            opts.count = files.length;
            $.each(files, function (i, file) {
                var item = {
                    id: file.id,
                    filename: file.filename,
                    name: file.filename,
                    size: file.size || 0,
                    status: "unchanged",
                    uploadInfo: null
                }
                self.addItem(item);
            });
            self.updateClearButton();
            return (self.getData());
        },
        getAttachmentList: function () {
            var self = this, opts = self.options, ui = opts.ui, fileList = [];
            opts.data.forEach(function (upload, i) {
                if (upload.file.status === 'attach') fileList.push(upload.file);
            });
            return (fileList);
        },
        startUploader: function () {
            var self = this, opts = self.options;
            var fileAttachList = self.getAttachmentList();
            opts.duringUpload = $.Deferred();
            function doUpload() {
                if (fileAttachList.length > 0) {
                    // Загружаем только новые файлы 
                    opts.cntUpload = 0;
                    opts.completeUpload = 0;
                    fileAttachList.forEach(function (upload, i) {
                        opts.cntUpload === 0 && opts.onUploadStarted();
                        opts.cntUpload++;
                        upload.uploadInfo.submit().complete(function (result, textStatus, jqXhr) {
                            var isError, obj;
                            if (result.responseText) {
                                obj = JSON.parse(result.responseText);
                                isError = (typeof obj == 'object' && obj !== null && obj.Error);
                            } else {
                                isError = result.Message.IsError;
                                obj = result.Message;
                            }
                            upload.uploadInfo = upload.status = undefined;
                            opts.completeUpload++;
                            if (isError) {
                                opts.duringUpload.reject(obj);
                            } else {
                                if (opts.cntUpload === opts.completeUpload) {
                                    opts.onUploadFinished(self.getData());
                                    opts.duringUpload.resolve(obj);
                                }
                            }
                        });
                    });
                } else {
                    opts.onUploadFinished([]);
                    opts.duringUpload.resolve(null);
                }
            }
            doUpload();
            return opts.duringUpload;
        },
        setOptions: function (options) {
            var self = this;
            options && $.extend(self.options, options);
        }
    });
})(jQuery);

