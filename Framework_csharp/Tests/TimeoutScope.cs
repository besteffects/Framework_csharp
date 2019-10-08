using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Framework_csharp.Tests
{
    public sealed class TimeoutScope : IDisposable  //this class contains methods for default timeout and for changing of default timeouut
                                                    //sealed because of warning Warning    CA1063    Provide an overridable implementation of Dispose(bool) on 'TimeoutScope' or mark the type as sealed. A call to Dispose(false) should only clean up native resources. A call to Dispose(true) should clean up both managed and native resources.
    {
        public const int DefaultTimeout = 10000;

        private IWebDriver driver;

        public TimeoutScope(IWebDriver driver)
        {
            this.driver = driver;
        }

        public static void SetDefaults(IWebDriver driver)
        {
            SetTimeout(driver, DefaultTimeout);
        }

        public TimeoutScope SetTimeout(int timeout)
        {
            SetTimeout(driver, timeout);
            return this;
        }

        public void Dispose()
        {
            SetDefaults(driver);
        }

        private static void SetTimeout(IWebDriver driver, int timeout)
        {
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(timeout);
        }
    }
}
