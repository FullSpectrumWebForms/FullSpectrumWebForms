namespace controls {

    export class redirect extends core.controlBase {

        
        initialize(type: string, index: number, id: string, properties: { property: string, value: any }[]) {
            super.initialize(type, index, id, properties);
        }
        redirect(url: string) {
            window.location.replace(url);
            
        }
    }
}
core.controlTypes['Redirect'] = () => new controls.redirect();