using FSW.Controls.Html;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using Xunit;

namespace UnitTests
{
    public class UnitTest1: IDisposable
    {
        static FSW.Diagnostic.UnitTestsManager UnitTestsManager;

        public UnitTest1()
        {
            if( UnitTestsManager is null )
                UnitTestsManager = new FSW.Diagnostic.UnitTestsManager();
        }

        public void Dispose()
        {

        }

        [Fact]
        public async System.Threading.Tasks.Task TestButtonText()
        {
            using (var emptyTest = await UnitTestsManager.CreateEmptyPageTest())
            {
                Button bt;
                using (emptyTest.ServerSideLock)
                {
                    bt = new Button(emptyTest.Page)
                    {
                        Text = "BT_Text"
                    };

                    emptyTest.Container.Children.Add(bt);
                }

                var text = await emptyTest.DiagnosticManager.GetElementText(bt);

                Assert.Equal("BT_Text", text);
                Assert.Equal(bt.Text, text);
            }
        }
    }
}
