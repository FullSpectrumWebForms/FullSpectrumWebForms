using FSW.Controls.Html;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests
{
    public class TextBoxTests
    {
        private FSW.Diagnostic.UnitTestsManager UnitTestsManager => FSW.Diagnostic.UnitTestsManager.GetUnitTestsManager<TextBoxTests>();


        [Fact(DisplayName = "Setting text at initialization")]
        public async Task TextBoxText()
        {
            using (var x = await UnitTestsManager.CreateEmptyPageTest())
            {
                TextBox tb;
                using (x.ServerSideLock)
                {
                    x.Container.Children.Add(tb = new TextBox(x.Page)
                    {
                        Text = "TB_Text"
                    });
                }

                var text = await x.DiagnosticManager.GetElementVal<string>(tb);

                Assert.Equal("TB_Text", text);
                Assert.Equal(tb.Text, text);
            }
        }
        [Fact(DisplayName = "Click not working")]
        public async Task TextBoxClicked()
        {
            using (var x = await UnitTestsManager.CreateEmptyPageTest())
            {
                TextBox tb;
                var waitingTask = new TaskCompletionSource<object>();
                using (x.ServerSideLock)
                {
                    x.Container.Children.Add(tb = new TextBox());

                    tb.OnClicked += (obj) =>
                    {
                        Assert.Equal(tb, obj);
                        waitingTask.TrySetResult(null);
                    };
                }

                await x.DiagnosticManager.ClickOnElement(tb);

                Assert.False(waitingTask.Task.Wait(TimeSpan.FromSeconds(0.2)));
            }
        }
        [Fact(DisplayName = "Click working")]
        public async Task TextBoxClicked2()
        {
            using (var x = await UnitTestsManager.CreateEmptyPageTest())
            {
                TextBox tb;
                var waitingTask = new TaskCompletionSource<object>();
                using (x.ServerSideLock)
                {
                    x.Container.Children.Add(tb = new TextBox(x.Page));

                    tb.OnClicked += (obj) =>
                    {
                        Assert.Equal(tb, obj);
                        waitingTask.TrySetResult(null);
                    };
                }

                await x.DiagnosticManager.ClickOnElement(tb);

                Assert.True(waitingTask.Task.Wait(TimeSpan.FromSeconds(0.2)));
            }
        }
        [Fact(DisplayName = "Text changed event, from server")]
        public async Task TextChangedFromServer()
        {
            using (var x = await UnitTestsManager.CreateEmptyPageTest())
            {
                TextBox tb;
                var waitingTask = new TaskCompletionSource<object>();
                using (x.ServerSideLock)
                {
                    x.Container.Children.Add(tb = new TextBox(x.Page)
                    {
                        Text = "test"
                    });

                    tb.OnTextChanged += (obj, oldValue, newValue) =>
                    {
                        Assert.Equal(tb, obj);
                        waitingTask.TrySetResult(null);
                    };
                }
                using (x.ServerSideLock)
                    tb.Text = "test2";

                var val = await x.DiagnosticManager.GetElementVal<string>(tb);

                Assert.Equal(val, tb.Text);
                Assert.Equal("test2", val);
                Assert.False(waitingTask.Task.Wait(TimeSpan.FromSeconds(0.2)));
            }
        }
        [Fact(DisplayName = "Text set and changed from client")]
        public async Task TextChangedFromClient()
        {
            using (var x = await UnitTestsManager.CreateEmptyPageTest())
            {
                TextBox tb;
                var waitingTask = new TaskCompletionSource<object>();
                using (x.ServerSideLock)
                {
                    x.Container.Children.Add(tb = new TextBox(x.Page)
                    {
                        Text = "test"
                    });

                    tb.OnTextChanged += (obj, oldValue, newValue) =>
                    {
                        Assert.Equal(tb, obj);
                        Assert.Equal(tb.Text, newValue);
                        Assert.Equal("test", oldValue);
                        Assert.Equal("test2", tb.Text);

                        waitingTask.TrySetResult(null);
                    };
                }

                await x.DiagnosticManager.SetElementVal(tb, "test2");
                Assert.False(waitingTask.Task.Wait(TimeSpan.FromSeconds(0.2)));

                await x.DiagnosticManager.TriggerChange(tb);
                Assert.True(waitingTask.Task.Wait(TimeSpan.FromSeconds(0.2)));
            }
        }
        //[Fact(DisplayName = "Text set, instant feedback from client")]
        //public async Task TextChangedFromClient2()
        //{
        //    using (var x = await UnitTestsManager.CreateEmptyPageTest(true))
        //    {
        //        TextBox tb;
        //        var waitingTask = new TaskCompletionSource<object>();
        //        using (x.ServerSideLock)
        //        {
        //            x.Container.Children.Add(tb = new TextBox(x.Page)
        //            {
        //                Text = "test",
        //                InstantFeedback = TimeSpan.FromSeconds(1)
        //            });
        //
        //            tb.OnTextChanged += (obj, oldValue, newValue) =>
        //            {
        //                Assert.Equal(tb, obj);
        //                Assert.Equal(tb.Text, newValue);
        //                Assert.Equal("test", oldValue);
        //                Assert.Equal("testtest2", tb.Text);
        //
        //                waitingTask.TrySetResult(null);
        //            };
        //        }
        //
        //        await x.DiagnosticManager.SendKeys(tb, "test2");
        //        Assert.False(waitingTask.Task.Wait(TimeSpan.FromSeconds(0.2)));
        //
        //        Assert.True(waitingTask.Task.Wait(TimeSpan.FromSeconds(1)));
        //    }
        //}
    }
}
