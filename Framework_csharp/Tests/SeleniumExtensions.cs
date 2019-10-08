using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Interactions;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace Framework_csharp.Tests
{
    public static class SeleniumExtensions
    {
        private static readonly Regex WhitespaceRegex = new Regex(@"\s", RegexOptions.Multiline | RegexOptions.Compiled);

        public static void AttemptClick(this IWebElement el) // trying to click an element for n times
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    el.Click();
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to make click on {0}: {1}", el, ex.Message);
                    Thread.Sleep(500);
                }
            }

            throw new InvalidOperationException(string.Format("Failed to make click on {0}", el));
        }

        public static void DragAndDrop(this IWebDriver driver, By element, By target)
        {
            var actions = new Actions(driver);
            actions.DragAndDrop(driver.FindElement(element), driver.FindElement(target)).Perform();
        }

        public static void ClickOn(this IWebDriver driver, By elementExpression)
        {
            var el = driver.FindElement(elementExpression);
            el.AttemptClick();
        }

        public static void ClickOnAndWait(this IWebDriver driver, By elementExpression)
        {
            driver.FindElement(elementExpression).Click();
            driver.WaitForAjax(35000);
        }

        /// <remarks>
        /// extension must be static, in static class. this must be present before first argument. It is required for conditional links
        /// </remarks>
        /// 
        public static IWebDriver GoToRelativeUrl(this IWebDriver driver, string relativeUrl)
        {
            var url = new Uri(driver.Url);
            driver.Navigate().GoToUrl(new Uri(url, relativeUrl));
            return driver;
        }

        public static void DismissAlertIfPresent(this IWebDriver driver)
        {
            if (IsAlertPresent(driver))
            {
                driver.SwitchTo().Alert()
                .Dismiss();
            }
        }

        public static bool IsAlertPresent(this IWebDriver driver)
        {
            try
            {
                driver.SwitchTo().Alert();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// method, which inputs text into Field locator and presses Enter. It must be used for drop-downs
        /// </summary>        
        public static void SelectInFilterCombobox(this IWebDriver driver, By locator, string text)
        {
            var el = driver.FindElement(locator);
            el.SendKeys(text);
            el.SendKeys(Keys.Tab);
        }

        /// <summary>
        /// method, which selects text in Field locator and presses Enter. It must be used for drop-downs with predefined selection where filter does not work
        /// </summary>        
        public static void SelectInRegularComboboxDown(this IWebDriver driver, By locator, int elementPosition)
        {
            //StringBuilder sb = new StringBuilder();            
            //for (int i = 0; i < elementPosition; i++)
            //{
            //    sb.Append(Keys.Down);
            //}
            //sb.Append(Keys.Enter);
            //var result = sb.ToString();
            var el = driver.FindElement(locator);
            var downSequence = Enumerable.Repeat(Keys.Down, elementPosition + 1);// Creating a list of repeating elements. Keys.Down- list element
            var keys = string.Join(string.Empty, downSequence) + Keys.Enter;
            el.SendKeys(keys);
        }

        public static void SelectInRegularComboboxUp(this IWebDriver driver, By locator, int elementPosition)
        {
            var el = driver.FindElement(locator);
            var upSequence = Enumerable.Repeat(Keys.Up, elementPosition + 1);// Creating a list of repeating elements. Keys.Up- list element
            var keys = string.Join(string.Empty, upSequence) + Keys.Enter;
            el.SendKeys(keys);
        }

        public static void SelectInComboboxByValue(this IWebDriver driver, By locator, string value)
        {
            var element = driver.FindElement(locator);
            SelectElement selector = new SelectElement(element);
            selector.SelectByValue(value);
        }

        public static void SelectInComboboxByText(this IWebDriver driver, By locator, string text)
        {
            var element = driver.FindElement(locator);
            SelectElement selector = new SelectElement(element);
            selector.SelectByText(text);
        }

        public static void SelectInComboboxByIndex(this IWebDriver driver, By locator, int index)
        {
            var element = driver.FindElement(locator);
            SelectElement selector = new SelectElement(element);
            selector.SelectByIndex(index);
        }

        public static void WaitForPageReady(this IWebDriver driver, int timeout = 10000)
        {
            //var wait = new webdriverwait(driver, timespan.frommilliseconds(timeout));
            //wait.until(d => (bool)((ijavascriptexecutor)d).executescript("return (document.readystate == 'complete' && jquery.active == 0)"));
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(5);
        }

        public static void WaitForPageLoad(this IWebDriver driver, int maxWaitTimeInSeconds = 10)
        {
            string state = string.Empty;
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(maxWaitTimeInSeconds));
                //Checks every 500 ms whether predicate returns true if returns exit otherwise keep trying till it returns true
                wait.Until(d =>
                {
                    try
                    {
                        state = ((IJavaScriptExecutor)driver).ExecuteScript(@"return document.readyState").ToString();
                    }
                    catch (InvalidOperationException)
                    {
                        //Ignore
                    }
                    catch (NoSuchWindowException)
                    {
                        //when popup is closed, switch to last windows
                        driver.SwitchTo().Window(driver.WindowHandles.Last());
                    }
                    //In IE there are chances we may get state as loaded instead of complete
                    return (state.Equals("complete", StringComparison.InvariantCultureIgnoreCase) || state.Equals("loaded", StringComparison.InvariantCultureIgnoreCase));
                });
            }
            catch (TimeoutException)
            {
                //sometimes Page remains in Interactive mode and never becomes Complete, then we can still try to access the controls
                if (!state.Equals("interactive", StringComparison.InvariantCultureIgnoreCase))
                    throw;
            }
            catch (NullReferenceException)
            {
                //sometimes Page remains in Interactive mode and never becomes Complete, then we can still try to access the controls
                if (!state.Equals("interactive", StringComparison.InvariantCultureIgnoreCase))
                    throw;
            }
            catch (WebDriverException)
            {
                if (driver.WindowHandles.Count == 1)
                {
                    driver.SwitchTo().Window(driver.WindowHandles[0]);
                }
                state = ((IJavaScriptExecutor)driver).ExecuteScript(@"return document.readyState").ToString();
                if (!(state.Equals("complete", StringComparison.InvariantCultureIgnoreCase) || state.Equals("loaded", StringComparison.InvariantCultureIgnoreCase)))
                    throw;
            }
        }

        public static void WaitForAjax(this IWebDriver driver, int timeout = 10000) // method for avoiding unexpected warnings
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(timeout));
            wait.Until(d => (bool)((IJavaScriptExecutor)d).ExecuteScript("return jQuery.active == 0"));
        }

        //public static void JavaScriptClick(this IWebDriver driver, By locator)
        //{
        //    var button = driver.FindElement(locator);
        //    JavaScriptClick(driver, button);
        //    var executor = (IJavaScriptExecutor)driver;
        //    executor.ExecuteScript("arguments[0].click();", button);
        //}

        public static void JavaScriptClick(this IWebDriver driver, IWebElement element)
        {
            var executor = (IJavaScriptExecutor)driver;
            executor.ExecuteScript("arguments[0].click();", element);
        }

        public static void JavaScriptClick(this IWebElement element, IWebDriver driver)
        {
            var executor = (IJavaScriptExecutor)driver;
            executor.ExecuteScript("arguments[0].click();", element);
        }

        public static void JavaScriptClick(this IWebDriver driver, By locator) //PageObject
        {
            var button = driver.FindElement(locator);
            JavaScriptClick(driver, button);
        }


        /// <summary>
        /// Uses overloaded JavaScriptClick methods
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="locator">Place where to click</param>
        /// <param name="timeout">Milliseconds</param>
        /// 
        public static void JavaScriptClickAndWaitForAjax(this IWebDriver driver, By locator, int timeout = 10000)
        {
            var button = driver.FindElement(locator);
            JavaScriptClick(driver, button);
            driver.WaitForAjax(timeout);
        }

        //PageObject
        public static void WaitElementVisible(this IWebDriver driver, By locator, int timeout = 20000) //Explicit wait. waits until element is visible
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(timeout));
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(locator));
            }
            catch (WebDriverTimeoutException ex)
            {
                Console.WriteLine("WaitElementVisible failed on locator {0}: {1}", locator, ex.Message);
            }
        }

        //for PageFactory
        public static void WaitElementVisible(this IWebElement element, IWebDriver driver, int timeout = 10000)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(timeout));
                wait.Until(d => element.Displayed);
            }
            catch (WebDriverTimeoutException ex)
            {
                Console.WriteLine("WaitElementVisible failed on element {0}: {1}", element, ex.Message);
            }
        }

        /// <summary>
        /// Alternative way to wait until element becomes clickable
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="locator"></param>
        /// <param name="timeout">Milliseconds.40000 default</param>
        public static void WaitElementClickable(this IWebDriver driver, By locator, int timeout = 10000) //for By class
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(timeout));
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(locator));
            }
            catch (WebDriverTimeoutException ex)
            {
                Console.WriteLine("WaitElementClickable failed on locator {0}: {1}", locator, ex.Message);
            }
        }

        //for PageFactory
        public static void WaitElementIsClickable(this IWebElement element, IWebDriver driver, int timeout = 10000)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(timeout));
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(element));
            }
            catch (WebDriverTimeoutException ex)
            {
                Console.WriteLine("WaitElementClickable failed on element {0}: {1}", element, ex.Message);
            }
        }

        /// <summary>
        /// Explicit wait. waits until element is present at DOM of a page
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="locator"></param>
        public static void WaitElementExists(this IWebDriver driver, By locator, int timeout = 10000)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(timeout));
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(locator));
            }
            catch (WebDriverTimeoutException ex)
            {
                Console.WriteLine("WaitElementExists failed on locator {0}: {1}", locator, ex.Message);
            }
        }

        public static void ScrollDownToGoal(this IWebDriver driver, By locator)
        {
            var goal = driver.FindElement(locator);
            var executor = (IJavaScriptExecutor)driver;
            executor.ExecuteScript("arguments[0].scrollIntoView();", goal);
        }

        public static void GetInnerHtml(this IWebDriver driver, By locator, string attribute)
        {
            var element = driver.FindElement(locator);
            var innerHtml = element.GetAttribute(attribute);
        }

        public static IWebDriver WaitUntilLoaderHidden(this IWebDriver driver)
        {
            driver.WaitUntilNotVisible(By.CssSelector("#some-loader-name"));
            return driver;
        }

        /// <summary>
        /// Explicit wait. waits until element is NOT visible
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="locator"></param>
        public static void WaitUntilNotVisible(this IWebDriver driver, By locator)
        {
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                wait.Until(d =>
                {
                    var el = d.FindElements(locator).FirstOrDefault();
                    return el == null || !el.Displayed;
                });
            }
            catch (StaleElementReferenceException ex)
            {
                Console.WriteLine("Attempting to recover from StaleElementReferenceException : {0}", ex.Message);
            }
            // Returns true because stale element reference implies that element
            // is no longer visible.
            catch (NoSuchElementException e)
            {
                Console.WriteLine("Attempting to recover from NoSuchElementException : {0}", e.Message);
            }
            // Returns true because the element is not present in DOM. The
            // try block checks if the element is present but is invisible.
            finally
            {
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            }
        }

        public static void WaitUntilNotVisible(this IWebElement element, IWebDriver driver)
        {
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.StalenessOf(element));
            }
            catch (StaleElementReferenceException ex)
            {
                Console.WriteLine("Attempting to recover from StaleElementReferenceException : {0}", ex.Message);
            }
            catch (NoSuchElementException e)
            {
                Console.WriteLine("Attempting to recover from NoSuchElementException : {0}", e.Message);
            }
            finally
            {
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            }
        }

        public static TimeoutScope TimeoutScope(this IWebDriver driver, int newTimeout)
        {
            return new TimeoutScope(driver).SetTimeout(newTimeout);
        }

        public static bool CompareWithoutWhitespaces(this string a, string b)
        {
            return a.RemoveWhitespaces() == b.RemoveWhitespaces();
        }

        public static string RemoveWhitespaces(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return string.Empty;
            }

            return WhitespaceRegex.Replace(s, "");
        }


    }
}
