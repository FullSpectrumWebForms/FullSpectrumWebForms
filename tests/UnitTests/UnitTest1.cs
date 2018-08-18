using FSW.Controls.Html;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests
{
    public class ButtonTests : IDisposable
    {
        private static FSW.Diagnostic.UnitTestsManager UnitTestsManager;

        public ButtonTests()
        {
            if (UnitTestsManager is null)
                UnitTestsManager = new FSW.Diagnostic.UnitTestsManager();
        }

        public void Dispose()
        {

        }

        [Fact]
        public async Task TestButtonText()
        {
            using (var x = await UnitTestsManager.CreateEmptyPageTest())
            {
                Button bt;
                using (x.ServerSideLock)
                {
                    x.Container.Children.Add(bt = new Button(x.Page)
                    {
                        Text = "BT_Text"
                    });
                }

                var text = await x.DiagnosticManager.GetElementText(bt);

                Assert.Equal("BT_Text", text);
                Assert.Equal(bt.Text, text);
            }
        }
        [Fact]
        public async Task TestButtonClicked()
        {
            using (var x = await UnitTestsManager.CreateEmptyPageTest())
            {
                Button bt;
                var waitingTask = new TaskCompletionSource<object>();
                using (x.ServerSideLock)
                {
                    x.Container.Children.Add(bt = new Button());

                    bt.OnButtonClicked += (obj) =>
                    {
                        Assert.Equal(bt, obj);
                        waitingTask.TrySetResult(null);
                    };
                }

                await x.DiagnosticManager.ClickOnElement(bt);

                Assert.True(waitingTask.Task.Wait(TimeSpan.FromSeconds(0.2)));
            }
        }
        [Fact]
        public async Task TestDisabledButtonClicked()
        {
            using (var x = await UnitTestsManager.CreateEmptyPageTest())
            {
                Button bt;
                var waitingTask = new TaskCompletionSource<object>();
                using (x.ServerSideLock)
                {
                    x.Container.Children.Add(bt = new Button(x.Page)
                    {
                        State = State.Disabled
                    });

                    bt.OnButtonClicked += (obj) =>
                    {
                        Assert.Equal(bt, obj);
                        waitingTask.TrySetResult(null);
                    };
                }

                await x.DiagnosticManager.ClickOnElement(bt);

                Assert.False(waitingTask.Task.Wait(TimeSpan.FromSeconds(0.2)));
            }
        }
    }
}
