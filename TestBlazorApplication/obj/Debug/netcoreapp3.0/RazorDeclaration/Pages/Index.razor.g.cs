#pragma checksum "C:\Users\pascal.arnold\Source\Repos\FullSpectrumWebForms\TestBlazorApplication\Pages\Index.razor" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "9f9b35dc2146d851982cef71397bacd47db04075"
// <auto-generated/>
#pragma warning disable 1591
#pragma warning disable 0414
#pragma warning disable 0649
#pragma warning disable 0169

namespace TestBlazorApplication.Pages
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
#nullable restore
#line 1 "C:\Users\pascal.arnold\Source\Repos\FullSpectrumWebForms\TestBlazorApplication\_Imports.razor"
using System.Net.Http;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "C:\Users\pascal.arnold\Source\Repos\FullSpectrumWebForms\TestBlazorApplication\_Imports.razor"
using Microsoft.AspNetCore.Authorization;

#line default
#line hidden
#nullable disable
#nullable restore
#line 3 "C:\Users\pascal.arnold\Source\Repos\FullSpectrumWebForms\TestBlazorApplication\_Imports.razor"
using Microsoft.AspNetCore.Components.Authorization;

#line default
#line hidden
#nullable disable
#nullable restore
#line 4 "C:\Users\pascal.arnold\Source\Repos\FullSpectrumWebForms\TestBlazorApplication\_Imports.razor"
using Microsoft.AspNetCore.Components.Forms;

#line default
#line hidden
#nullable disable
#nullable restore
#line 5 "C:\Users\pascal.arnold\Source\Repos\FullSpectrumWebForms\TestBlazorApplication\_Imports.razor"
using Microsoft.AspNetCore.Components.Routing;

#line default
#line hidden
#nullable disable
#nullable restore
#line 6 "C:\Users\pascal.arnold\Source\Repos\FullSpectrumWebForms\TestBlazorApplication\_Imports.razor"
using Microsoft.AspNetCore.Components.Web;

#line default
#line hidden
#nullable disable
#nullable restore
#line 7 "C:\Users\pascal.arnold\Source\Repos\FullSpectrumWebForms\TestBlazorApplication\_Imports.razor"
using Microsoft.JSInterop;

#line default
#line hidden
#nullable disable
#nullable restore
#line 8 "C:\Users\pascal.arnold\Source\Repos\FullSpectrumWebForms\TestBlazorApplication\_Imports.razor"
using TestBlazorApplication;

#line default
#line hidden
#nullable disable
#nullable restore
#line 9 "C:\Users\pascal.arnold\Source\Repos\FullSpectrumWebForms\TestBlazorApplication\_Imports.razor"
using TestBlazorApplication.Shared;

#line default
#line hidden
#nullable disable
    [Microsoft.AspNetCore.Components.RouteAttribute("/")]
    public partial class Index : Microsoft.AspNetCore.Components.ComponentBase
    {
        #pragma warning disable 1998
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
        }
        #pragma warning restore 1998
#nullable restore
#line 11 "C:\Users\pascal.arnold\Source\Repos\FullSpectrumWebForms\TestBlazorApplication\Pages\Index.razor"
      

    static MyPage Page = new MyPage();

    string Text = "yo";
    static bool Done = false;
    protected override void OnInitialized()
    {
        if (!Done)
        {
            Done = true;
            return;
        }

        System.Threading.Tasks.Task.Run(() =>
        {
            System.Threading.Thread.Sleep(5000);
            Page.Container.Width = "200px";
            Page.Container.Height = "200px";
            Page.Container.BackgroundColor = System.Drawing.Color.Blue;
            Page.Container.OnClicked += OnContainerClicked;
        });
    }

    private void OnContainerClicked(FSW.Controls.Html.HtmlControlBase htmlControlBase)
    {
        InvokeAsync(() =>
        {
            Text += " yo";
            StateHasChanged();
        });

    }


#line default
#line hidden
#nullable disable
    }
}
#pragma warning restore 1591
