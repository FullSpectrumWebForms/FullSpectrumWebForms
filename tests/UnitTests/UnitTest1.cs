using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using Xunit;

namespace UnitTests
{
    public class UnitTest1: IDisposable
    {
        FSW.Diagnostic.UnitTestsManager UnitTestsManager = new FSW.Diagnostic.UnitTestsManager();

        public UnitTest1()
        {
        }

        public void Dispose()
        {

        }

        [Fact]
        public void Test1()
        {

        }
    }
}
