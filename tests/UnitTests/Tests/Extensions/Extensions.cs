using FSW.Controls.Html;
using FSW.Semantic.Controls.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Tests.Extensions
{
    public class Extensions
    {
        private FSW.Diagnostic.UnitTestsManager UnitTestsManager => FSW.Diagnostic.UnitTestsManager.GetUnitTestsManager<Extensions>();


        [Fact(DisplayName = "Test semantic transition")]
        public async Task ExtensionsTest()
        {
            using (var x = await UnitTestsManager.CreateEmptyPageTest())
            {
                var errorTask = new TaskCompletionSource<object>();
                Button bt;
                using (x.ServerSideLock)
                {
                    x.Container.Children.Add(bt = new Button());
                    bt.Extensions.Add<Transition>();

                    bt.Visible = HtmlControlBase.VisibleState.None;


                    x.Page.OverrideErrorHandle = (error) =>
                    {
                        errorTask.TrySetResult(null);
                        return Task.CompletedTask;
                    };
                }

                var displayStyle = await x.DiagnosticManager.GetElementStyle(bt, "display");
                Assert.Equal("none", displayStyle);

                using (x.ServerSideLock)
                    bt.Transition(Transition.Animation.SlideDown);

                await x.KeepAlive(TimeSpan.FromSeconds(0.2));

                displayStyle = await x.DiagnosticManager.GetElementStyle(bt, "display");
                Assert.Equal("inline-block", displayStyle);

                Assert.False(errorTask.Task.Wait(TimeSpan.FromSeconds(0.2)));
            }
        }
    }
}
