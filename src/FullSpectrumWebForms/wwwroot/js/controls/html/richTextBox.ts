declare var Quill;

namespace controls.html {

    export class richTextBox extends htmlControlBase {

        get Text(): string {
            return this.getPropertyValue<this, string>("Text");
        }
        set Text(value: string) {
            this.setPropertyValue<this>("Text", value);
        }

        get Contents(): string {
            return JSON.stringify(this.getPropertyValue<this, string>("Contents"));
        }
        set Contents(value: string) {
            this.setPropertyValue<this>("Contents", value);
        }


        elementQuill: any;

        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);
            let that = this;
            this.getProperty<this, string>("Text").onChangedFromServer.register(this.onTextChangedFromServer.bind(this), true);
            this.getProperty<this, string>("Contents").onChangedFromServer.register(this.onContentsChangedFromServer.bind(this), true);

            this.elementQuill = new Quill('#' + this.element.uniqueId()[0].id, {
                modules: {
                    toolbar: [
                        [{ header: [1, 2, false] }],
                        ['bold', 'italic', 'underline'],
                        ['image', 'link'], [{ 'list': 'ordered' }, { 'list': 'bullet' }]]
                },
                theme: 'snow',
            });

            var toolbar = this.elementQuill.getModule('toolbar');
            toolbar.addHandler('link', function (value) {
                if (value && that.elementQuill.getSelection().length != 0) {

                    var href = prompt('Entrer le URL');

                    if (!(href as any).startsWith('http://'))
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

        protected initializeHtmlElement(): void {
            this.element = $('<div></div>');
            this.appendElementToParent();
        }

        onTextChangedFromServer(property: core.controlProperty<string[]>, args: { old: string[], new: string[] }) {
            this.elementQuill.setText(this.Text);
        }

        onContentsChangedFromServer(property: core.controlProperty<string[]>, args: { old: string[], new: string[] }) {

            var text = JSON.parse(this.Contents);

            if ('"' + text + '"'  == this.Contents)
                this.elementQuill.setText(JSON.parse(this.Contents));
            else      
                this.elementQuill.updateContents(text);
        }

    }
}
core.controlTypes['RichTextBox'] = () => new controls.html.richTextBox();