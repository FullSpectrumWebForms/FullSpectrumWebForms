var controls;
(function (controls) {
    var html;
    (function (html) {
        class richTextBox extends html.htmlControlBase {
            get Text() {
                return this.getPropertyValue("Text");
            }
            set Text(value) {
                this.setPropertyValue("Text", value);
            }
            get Contents() {
                return JSON.stringify(this.getPropertyValue("Contents"));
            }
            set Contents(value) {
                this.setPropertyValue("Contents", value);
            }
            initialize(type, index, id, properties) {
                super.initialize(type, index, id, properties);
                let that = this;
                this.getProperty("Text").onChangedFromServer.register(this.onTextChangedFromServer.bind(this), true);
                this.getProperty("Contents").onChangedFromServer.register(this.onContentsChangedFromServer.bind(this), true);
                this.elementQuill = new Quill('#' + this.element.uniqueId()[0].id, {
                    modules: {
                        toolbar: [
                            [{ header: [1, 2, false] }],
                            ['bold', 'italic', 'underline'],
                            ['image', 'link'], [{ 'list': 'ordered' }, { 'list': 'bullet' }]
                        ]
                    },
                    theme: 'snow',
                });
                var toolbar = this.elementQuill.getModule('toolbar');
                toolbar.addHandler('link', function (value) {
                    if (value && that.elementQuill.getSelection().length != 0) {
                        var href = prompt('Entrer le URL');
                        if (!href.startsWith('http://'))
                            href = 'http://' + href;
                        that.elementQuill.format('link', href);
                    }
                    else
                        that.elementQuill.format('link', false);
                });
                this.elementQuill.on('text-change', function (delta, oldDelta, source) {
                    if (source == 'user') {
                        core.manager.lockPropertyUpdate();
                        that.Text = that.elementQuill.getText().split('\n')[0];
                        that.Contents = JSON.stringify(that.elementQuill.getContents().ops);
                        core.manager.unlockPropertyUpdate();
                    }
                });
            }
            initializeHtmlElement() {
                this.element = $('<div></div>');
                this.appendElementToParent();
            }
            onTextChangedFromServer(property, args) {
                this.elementQuill.setText(this.Text);
            }
            onContentsChangedFromServer(property, args) {
                var text = JSON.parse(this.Contents);
                if ('"' + text + '"' == this.Contents)
                    this.elementQuill.setText(JSON.parse(this.Contents));
                else
                    this.elementQuill.updateContents(text);
            }
        }
        html.richTextBox = richTextBox;
    })(html = controls.html || (controls.html = {}));
})(controls || (controls = {}));
core.controlTypes['RichTextBox'] = () => new controls.html.richTextBox();
//# sourceMappingURL=richTextBox.js.map