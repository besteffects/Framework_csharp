using Framework_csharp.Tests;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.Extensions;
using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Framework_csharp.Utilities
{

    [TestFixture(TypeArgs = new Type[] { typeof(ChromeDriver) })] //Change to FirefoxDriver, IEDriver to run tests in Firefox, IE
    public class MultiBrowserTest<TWebDriver> //generic
     where TWebDriver : IWebDriver, new() //IWebDriver and default constructor must be implemented
    {
        //variable for cookie
        private const string CookieFileName = "auth-cookie.txt";

        /// <summary>
        /// Element, that exists at all pages for authorized user
        /// </summary>
        private static readonly By authorizedLocator = By.XPath("//li[@class='some-element']");
                                                                                                // private static readonly string SiteId = ConfigurationManager.AppSettings["siteId"];
        private static readonly string MySite = ConfigurationManager.AppSettings["MySite"];

        protected IWebDriver driver;

        protected bool IsAuthorized
        {
            get
            {
                return driver.FindElements(authorizedLocator).Any();
            }
        }

        [SetUp]
        public virtual void FixtureSetUp()
        {
            var screenShotFolder = GetScreenshotFolder();
            foreach (var f in Directory.EnumerateFiles(screenShotFolder))
            {
                File.Delete(f);
            }

            InitWebDriver();
            var authCookie = ReadAuthCookie();
            NavigateToApp();
            driver.Manage().Window.Position = new Point(0, 0);
            driver.Manage().Window.Size = new Size(1600, 900); //Bug in ChromeDriver for teamcity. Custom resolution is not set            
                                                               // ((IJavaScriptExecutor)driver).ExecuteScript("window.resizeTo(1366,768);");
                                                               // driver.Manage().Window.Maximize();
            Console.WriteLine("Fixture setup window size is " + driver.Manage().Window.Size);
            if (!string.IsNullOrEmpty(authCookie))
            {
                driver.Manage().Cookies.AddCookie(new Cookie("site-auth", authCookie, "/", DateTime.Now.AddDays(7)));
                // driver.Manage().Cookies.AddCookie(new Cookie("sid", SiteId, "/", DateTime.Now.AddDays(7)));
                NavigateToApp();
            }
        }

        [TearDown]
        public virtual void TearDown()
        {
            if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
            {
                TakeScreenshot(TestContext.CurrentContext.Test.Name);
            }
            if (driver != null)
            {
                driver.Quit();
            }
        }

        protected void InitWebDriver()
        {
            if (typeof(TWebDriver) == typeof(FirefoxDriver))
            {
                FirefoxOptions fp = new FirefoxOptions();
                fp = new FirefoxOptions();
                fp.SetPreference("browser.startup.homepage", "about:blank");
                fp.SetPreference("browser.startup.homepage_override.mstone", "ignore");
                fp.SetPreference("startup.homepage_welcome_url", "about:blank");
                fp.SetPreference("startup.homepage_welcome_url.additional", "about:blank");
                driver = new FirefoxDriver(fp);
            }
            else if (typeof(TWebDriver) == typeof(ChromeDriver))
            {
                ChromeOptions options = new ChromeOptions();
                options.AddUserProfilePreference("credentials_enable_service", false);
                options.AddUserProfilePreference("profile.password_manager_enabled", false);
                // options.AddArgument("--start-maximized");
                driver = new ChromeDriver(options);
            }
            else
            {
                driver = new TWebDriver();
            }
            Console.WriteLine("InitWebDriver is ok");
            Console.WriteLine("TWEBDRIVER IS OK");
            TimeoutScope.SetDefaults(driver);// overall timeout    
                                             //DesiredCapabilities ieCapabilities = DesiredCapabilities.InternetExplorer(); // Settings to improve workability of IEDriver
                                             // ieCapabilities.SetCapability("nativeEvents", false);
                                             // ieCapabilities.SetCapability("unexpectedAlertBehaviour", "accept");
                                             //   ieCapabilities.SetCapability("ignoreProtectedModeSettings", true);
                                             //  ieCapabilities.SetCapability("disable-popup-blocking", true);
                                             //  ieCapabilities.SetCapability("enablePersistentHover", true);
        }

        protected void NavigateToApp()
        {
            driver.Navigate().GoToUrl(MySite);
            driver.Manage().Window.Position = new Point(0, 0);
            driver.Manage().Window.Size = new Size(1600, 900); //Set resolution for all tests globally         
                                                               //((IJavaScriptExecutor)driver).ExecuteScript("window.resizeTo(1680, 1050);");
                                                               //  driver.Manage().Window.Maximize();
            Console.WriteLine("Window sze when navigating to app " + driver.Manage().Window.Size);
        }

        protected void EnsureAuthorized()
        {
            using (driver.TimeoutScope(6000))
            {
                if (!IsAuthorized)
                {
                    Authorize();
                }
            }
        }

        protected void Authorize()
        {
            driver.Navigate().GoToUrl(MySite + "/User/Login");

            driver.Manage().Window.Position = new Point(0, 0);
            driver.Manage().Window.Size = new Size(1600, 900); //Set resolution for all tests globally
                                                               //  ((IJavaScriptExecutor)driver).ExecuteScript("window.resizeTo(1680, 1050);");
                                                               //  driver.Manage().Window.Maximize();
            Console.WriteLine("Window size before autorize " + driver.Manage().Window.Size);
            var cookie = driver.Manage().Cookies.GetCookieNamed("site-auth1");
            if (cookie != null)
            {
                SaveAuthCookie(cookie.Value);
            }
        }

        private string ReadAuthCookie()
        {
            //Console.WriteLine(CookieFileName);
            //Console.WriteLine(File.ReadAllText(CookieFileName));
            if (File.Exists(CookieFileName))
            {
                return File.ReadAllText(CookieFileName);
            }
            return null;
        }

        //Saves 
        private void SaveAuthCookie(string data)
        {
            File.WriteAllText(CookieFileName, data);
        }

        private void TakeScreenshot(string name)
        {
            var screen = driver.TakeScreenshot();
            var fileName = Path.Combine(GetScreenshotFolder(), name + ".jpg");
            screen.SaveAsFile(fileName, ScreenshotImageFormat.Jpeg);
            Console.WriteLine("Screenshot captured to: {0}", fileName);
        }

        private string GetScreenshotFolder()
        {
            var dir = Path.Combine(TestContext.CurrentContext.TestDirectory, "Screenshots", GetSafeClassName());
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return dir;
        }

        private string GetSafeClassName()
        {
            var t = GetType();
            var typeName = t.Name;
            if (t.IsGenericType)
            {
                typeName = typeName.Remove(typeName.IndexOf("`", StringComparison.Ordinal));
                typeName += "_" + t.GetGenericArguments().First().Name;
            }
            return typeName;
        }
    }
}

