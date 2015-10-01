using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Appium;

namespace ffrab.specs.Support
{
    public class AppiumDriverContext
    {
        public AppiumDriver<AppiumWebElement> Driver { get; set; }
    }
}
