#pragma checksum "/home/ouilol/Downloads/FullSpectrumWebForms/ProjectTemplate/Pages/index.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "bcd06f9432fb173b3a60551850b94b72b30be723"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Pages_index), @"mvc.1.0.razor-page", @"/Pages/index.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure.RazorPageAttribute(@"/Pages/index.cshtml", typeof(AspNetCore.Pages_index), null)]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"bcd06f9432fb173b3a60551850b94b72b30be723", @"/Pages/index.cshtml")]
    public class Pages_index : global::Microsoft.AspNetCore.Mvc.RazorPages.Page
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#line 3 "/home/ouilol/Downloads/FullSpectrumWebForms/ProjectTemplate/Pages/index.cshtml"
  
     Layout = "~/Pages/_layout.cshtml";

#line default
#line hidden
            BeginContext(115, 50, true);
            WriteLiteral("<button id=\"BT_Test\" data-po-Text=\"Hey!\"></button>");
            EndContext();
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<FSW_ASPC.ModelBase<ProjectTemplate.Pages.IndexPage>> Html { get; private set; }
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<FSW_ASPC.ModelBase<ProjectTemplate.Pages.IndexPage>> ViewData => (global::Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<FSW_ASPC.ModelBase<ProjectTemplate.Pages.IndexPage>>)PageContext?.ViewData;
        public FSW_ASPC.ModelBase<ProjectTemplate.Pages.IndexPage> Model => ViewData.Model;
    }
}
#pragma warning restore 1591
