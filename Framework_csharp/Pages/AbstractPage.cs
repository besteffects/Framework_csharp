using OpenQA.Selenium;

namespace Framework_csharp.Pages
{
    public abstract class AbstractTracktorPage
    {
        protected IWebDriver driver;
        internal protected AbstractTracktorPage(IWebDriver driver)
        {
            this.driver = driver;
            SeleniumExtras.PageObjects.PageFactory.InitElements(driver, this);
        }
    }
}
