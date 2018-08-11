var controls;
(function (controls) {
    var html;
    (function (html) {
        class flowChart extends html.htmlControlBase {
            initialize(type, index, id, properties) {
                super.initialize(type, index, id, properties);
            }
        }
        html.flowChart = flowChart;
    })(html = controls.html || (controls.html = {}));
})(controls || (controls = {}));
core.controlTypes['FlowChart'] = () => new controls.html.flowChart();
//# sourceMappingURL=flowChart.js.map