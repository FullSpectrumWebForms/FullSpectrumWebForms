﻿using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTests
{
    public class Program
    {
        public static void Main( string[] args )
        {
            var tests = new UnitTest1();
            tests.TestButtonText().Wait();
        }
    }
}
