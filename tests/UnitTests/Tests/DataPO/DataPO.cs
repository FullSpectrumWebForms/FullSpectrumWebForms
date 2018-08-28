using FSW.Controls.Html;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Tests.DataPO
{
    public class DataPO
    {
        private FSW.Diagnostic.UnitTestsManager UnitTestsManager => FSW.Diagnostic.UnitTestsManager.GetUnitTestsManager<DataPO>();


        [Fact]
        public async Task DataPOClasses()
        {
            using (var x = await UnitTestsManager.CreatePageTestFromHTML())
            {
                var waitingTask = new TaskCompletionSource<object>();
                var waitingTask2 = new TaskCompletionSource<object>();
                var errorTask = new TaskCompletionSource<object>();
                Button bt;
                using (x.ServerSideLock)
                {
                    x.TemplateContainer.GetTemplateControl("#BT_Test", out bt);
                    bt.OnButtonClicked += (obj) =>
                    {
                        Assert.Equal(bt, obj);
                        waitingTask.TrySetResult(null);
                    };
                    bt.OnClicked += (obj) =>
                    {
                        Assert.Equal(bt, obj);
                        waitingTask2.TrySetResult(null);
                    };
                    x.Page.OverrideErrorHandle = (error) =>
                    {
                        errorTask.TrySetResult(null);
                        return Task.CompletedTask;
                    };
                }

                await x.DiagnosticManager.ClickOnElement(bt);

                Assert.False(waitingTask.Task.Wait(TimeSpan.FromSeconds(0.2)));
                Assert.False(errorTask.Task.Wait(TimeSpan.FromSeconds(0.2)));
                Assert.True(waitingTask2.Task.Wait(TimeSpan.FromSeconds(0.2)));
            }
        }

        [Fact]
        public async Task DataPOText()
        {
            using (var x = await UnitTestsManager.CreatePageTestFromHTML())
            {
                var errorTask = new TaskCompletionSource<object>();
                Button bt;
                using (x.ServerSideLock)
                {
                    x.TemplateContainer.GetTemplateControl("#BT_Test", out bt);

                    x.Page.OverrideErrorHandle = (error) =>
                    {
                        errorTask.TrySetResult(null);
                        return Task.CompletedTask;
                    };
                }

                await x.DiagnosticManager.ClickOnElement(bt);

                var text = await x.DiagnosticManager.GetElementText(bt);
                Assert.Equal("test2", text);
                Assert.Equal(text, bt.Text);

                Assert.False(errorTask.Task.Wait(TimeSpan.FromSeconds(0.2)));
            }
        }
    }
}
