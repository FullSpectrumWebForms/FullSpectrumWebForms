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
    public class ListViewTests
    {
        private FSW.Diagnostic.UnitTestsManager UnitTestsManager => FSW.Diagnostic.UnitTestsManager.GetUnitTestsManager<ListViewTests>();

        public class TestDataItem
        {
            public TestDataItem(string obj1)
            {
                Obj1 = obj1;
            }
            public string Obj1;
        }


        [Fact]
        public async Task FillAndSelect()
        {
            using (var x = await UnitTestsManager.CreateEmptyPageTest())
            {
                ListView<TestDataItem> listView;
                TestDataItem item1;
                using (x.ServerSideLock)
                {
                    x.Container.Children.Add(listView = new ListView<TestDataItem>());

                    listView.Items.Set(new[]
                    {
                        item1 = new TestDataItem("test1"),
                        new TestDataItem("test2"),
                        new TestDataItem("test3"),
                    });

                    Assert.Null(listView.SelectedIndex);
                    Assert.Null(listView.SelectedItem);
                }
                using (x.ServerSideLock)
                {
                    listView.SelectedItem = listView.GetItem(listView.FindItemIndex(item1));

                    Assert.Same(listView.SelectedItem.Data, item1);
                    Assert.Equal(0, listView.SelectedIndex);
                }

            }
        }
        [Fact]
        public async Task InsertionBeforeAndAfter()
        {
            using (var x = await UnitTestsManager.CreateEmptyPageTest())
            {
                ListView<TestDataItem> listView;
                TestDataItem item1 = null;
                using (x.ServerSideLock)
                {

                    x.Container.Children.Add(listView = new ListView<TestDataItem>());
                    var nbTrigger = 0;
                    listView.OnItemSelected += (item) =>
                    {
                        ++nbTrigger;
                        Assert.Same(item.Data, item1);
                        Assert.Equal(1, nbTrigger);
                    };


                    listView.Items.Set(new[]
                    {
                        item1 = new TestDataItem("test1"),
                        new TestDataItem("test2"),
                        new TestDataItem("test3"),
                    });

                    Assert.Null(listView.SelectedIndex);
                    Assert.Null(listView.SelectedItem);
                }

                using (x.ServerSideLock)
                {
                    listView.SelectedIndex = 0;
                    Assert.Same(listView.SelectedItem.Data, item1);

                    listView.Items.Insert(0, new TestDataItem("test0"));

                    Assert.Equal(1, listView.SelectedIndex);
                    Assert.Same(listView.SelectedItem.Data, item1);
                }
                using (x.ServerSideLock)
                {
                    listView.Items.Insert(2, new TestDataItem("test0"));

                    Assert.Equal(1, listView.SelectedIndex);
                    Assert.Same(listView.SelectedItem.Data, item1);
                }
            }
        }
        [Fact]
        public async Task RemoveAfter()
        {
            using (var x = await UnitTestsManager.CreateEmptyPageTest())
            {
                ListView<TestDataItem> listView;
                TestDataItem item2 = null;
                var nbTrigger = 0;
                using (x.ServerSideLock)
                {

                    x.Container.Children.Add(listView = new ListView<TestDataItem>());
                    listView.OnItemSelected += (item) =>
                    {
                        ++nbTrigger;
                        Assert.Same(item.Data, item2);
                        Assert.Equal(1, nbTrigger);
                    };


                    listView.Items.Set(new[]
                    {
                         new TestDataItem("test1"),
                        item2 =new TestDataItem("test2"),
                        new TestDataItem("test3"),
                    });
                }

                using (x.ServerSideLock)
                {
                    listView.SelectedIndex = 1;
                    Assert.Same(listView.SelectedItem.Data, item2);

                    listView.Items.Remove(listView.Items.Last());

                    Assert.Equal(1, listView.SelectedIndex);
                    Assert.Same(listView.SelectedItem.Data, item2);
                }
                Assert.Equal(1, nbTrigger);
            }
        }
        [Fact]
        public async Task RemoveBefore()
        {
            using (var x = await UnitTestsManager.CreateEmptyPageTest())
            {
                ListView<TestDataItem> listView;
                TestDataItem item2 = null;
                using (x.ServerSideLock)
                {

                    x.Container.Children.Add(listView = new ListView<TestDataItem>());


                    listView.Items.Set(new[]
                    {
                         new TestDataItem("test1"),
                        item2 =new TestDataItem("test2"),
                        new TestDataItem("test3"),
                    });
                }

                using (x.ServerSideLock)
                {
                    listView.SelectedIndex = 1;

                    var nbTrigger = 0;
                    listView.OnItemSelected += (item) =>
                    {
                        ++nbTrigger;
                    };

                    Assert.Same(listView.SelectedItem.Data, item2);

                    listView.Items.Remove(listView.Items.First());

                    Assert.Equal(0, listView.SelectedIndex);
                    Assert.Same(listView.SelectedItem.Data, item2);
                    Assert.Equal(0, nbTrigger);
                }
            }
        }
        [Fact]
        public async Task RemoveSame()
        {
            using (var x = await UnitTestsManager.CreateEmptyPageTest())
            {
                ListView<TestDataItem> listView;
                TestDataItem item2 = null;
                using (x.ServerSideLock)
                {

                    x.Container.Children.Add(listView = new ListView<TestDataItem>());


                    listView.Items.Set(new[]
                    {
                         new TestDataItem("test1"),
                        item2 =new TestDataItem("test2"),
                        new TestDataItem("test3"),
                    });
                }

                using (x.ServerSideLock)
                {
                    listView.SelectedIndex = 1;

                    var nbTrigger = 0;
                    listView.OnItemSelected += (item) =>
                    {
                        Assert.Null(item);
                        Assert.Equal(1, ++nbTrigger);
                    };

                    Assert.Same(listView.SelectedItem.Data, item2);

                    listView.Items.Remove(item2);

                    Assert.Null(listView.SelectedIndex);
                    Assert.Null(listView.SelectedItem);
                    Assert.Equal(1, nbTrigger);
                }
            }
        }
        [Fact]
        public async Task Clear()
        {
            using (var x = await UnitTestsManager.CreateEmptyPageTest())
            {
                ListView<TestDataItem> listView;
                TestDataItem item2 = null;
                using (x.ServerSideLock)
                {

                    x.Container.Children.Add(listView = new ListView<TestDataItem>());


                    listView.Items.Set(new[]
                    {
                        new TestDataItem("test1"),
                        item2 = new TestDataItem("test2"),
                        new TestDataItem("test3"),
                    });
                }

                using (x.ServerSideLock)
                {
                    listView.SelectedIndex = 1;

                    var nbTrigger = 0;
                    listView.OnItemSelected += (item) =>
                    {
                        Assert.Null(item);
                        Assert.Equal(1, ++nbTrigger);
                    };

                    Assert.Same(listView.SelectedItem.Data, item2);

                    listView.Items.Clear();

                    Assert.Null(listView.SelectedIndex);
                    Assert.Null(listView.SelectedItem);
                    Assert.Equal(1, nbTrigger);
                }
            }
        }
        [Fact]
        public async Task SetItemsWithoutNewIndex()
        {
            using (var x = await UnitTestsManager.CreateEmptyPageTest())
            {
                ListView<TestDataItem> listView;
                TestDataItem item2 = null;
                using (x.ServerSideLock)
                {

                    x.Container.Children.Add(listView = new ListView<TestDataItem>());

                    listView.Items.Set(new[]
                    {
                        new TestDataItem("test1"),
                        item2 = new TestDataItem("test2"),
                        new TestDataItem("test3"),
                    });
                }

                using (x.ServerSideLock)
                {
                    listView.SelectedIndex = 1;

                    var nbTrigger = 0;
                    listView.OnItemSelected += (item) =>
                    {
                        Assert.Null(item);
                        Assert.Equal(1, ++nbTrigger);
                    };

                    Assert.Same(listView.SelectedItem.Data, item2);

                    listView.Items.Set(new[]
                    {
                        new TestDataItem("test4"),
                        item2 = new TestDataItem("test5"),
                        new TestDataItem("test6"),
                    });


                    Assert.Null(listView.SelectedIndex);
                    Assert.Null(listView.SelectedItem);
                    Assert.Equal(1, nbTrigger);
                }
            }
        }

    }
}
