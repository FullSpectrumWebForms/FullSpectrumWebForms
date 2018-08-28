using FSW.Controls.Html;
using FSW.Diagnostic.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSW.Diagnostic.UnitTests
{
    public class HtmlPageResult: IDisposable
    {
        public EmptyPageResult EmptyPageResult;

        public DiagnosticManager DiagnosticManager => EmptyPageResult.DiagnosticManager;
        public Core.FSWPage Page => EmptyPageResult.Page;
        public Div Container => EmptyPageResult.Container;

        public TemplateContainer TemplateContainer;

        public Core.FSWPage.PageLock ServerSideLock => Page.ServerSideLock;

        public  void Dispose()
        {
            EmptyPageResult.Dispose();
        }
    }
}
