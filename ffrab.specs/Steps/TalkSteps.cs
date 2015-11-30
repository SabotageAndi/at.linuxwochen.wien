using System;
using ffrab.specs.Support;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace ffrab.specs.Steps
{
    [Binding]
    class TalkSteps
    {
        private readonly AppiumDriverContext _appiumDriverContext;

        public TalkSteps(AppiumDriverContext appiumDriverContext)
        {
            _appiumDriverContext = appiumDriverContext;
        }

        [When(@"I choose the '(.*)' talk")]
        public void WhenIChooseTheTalk(string talkName)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"I see the details of this talk")]
        public void ThenISeeTheDetailsOfThisTalk(Table table)
        {
            //var appiumDriver = _appiumDriverContext.Driver;
            var fieldValueRows = table.CreateSet<FieldValueRow>();

            foreach (var fieldValueRow in fieldValueRows)
            {
                //AppiumWebElement element = appiumDriver.FindElementByAccessibilityId(fieldValueRow.Field);
                //element.Text.Should().Be(fieldValueRow.Value);
            }
        }
    }
}
