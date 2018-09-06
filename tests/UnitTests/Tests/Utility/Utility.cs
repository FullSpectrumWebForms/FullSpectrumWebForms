using FSW.Controls.Html;
using FSW.Semantic.Controls.ServerSide;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests
{
    public class Utility
    {
        private FSW.Diagnostic.UnitTestsManager UnitTestsManager => FSW.Diagnostic.UnitTestsManager.GetUnitTestsManager<ListViewTests>();


        [Fact]
        public async Task WatchVariableValue()
        {
            using (var x = await UnitTestsManager.CreateEmptyPageTest())
            {
                var watchedVariable = "test 2";
                TextBox textBox;
                var nbTrigger = 0;

                using (x.ServerSideLock)
                {
                    x.Container.Children.Add(textBox = new TextBox(x.Page)
                    {
                        Text = "test"
                    });

                    textBox.RegisterVariableWatch(() => watchedVariable, () =>
                    {
                        textBox.Text = watchedVariable;
                        ++nbTrigger;
                    });


                    watchedVariable = "test 3";
                }

                var text = await x.DiagnosticManager.GetElementVal<string>(textBox);

                Assert.Equal("test 3", text);
                Assert.Equal("test 3", textBox.Text);
                Assert.Equal(1, nbTrigger);
            }
        }

        private class TestClass
        {
            public string Value1;
            public int Value2;
            public double Value3;
        }
        [Fact]
        public async Task WatchVariableFields()
        {
            using (var x = await UnitTestsManager.CreateEmptyPageTest())
            {
                var watchedVariable = new TestClass()
                {
                    Value1 = "test1",
                    Value2 = 2,
                    Value3 = 3.3
                };

                TextBox textBox;
                var nbTrigger = 0;

                using (x.ServerSideLock)
                {
                    x.Container.Children.Add(textBox = new TextBox(x.Page)
                    {
                        Text = "test"
                    });

                    textBox.RegisterVariableWatch(() => watchedVariable, () =>
                    {
                        textBox.Text = watchedVariable.Value1;
                        ++nbTrigger;
                    });


                    watchedVariable.Value1 = "test2";
                }
                using (x.ServerSideLock)
                    watchedVariable.Value2 = 5;

                var text = await x.DiagnosticManager.GetElementVal<string>(textBox);

                Assert.Equal("test2", text);
                Assert.Equal("test2", textBox.Text);
                Assert.Equal(2, nbTrigger);
            }
        }


    }
}
