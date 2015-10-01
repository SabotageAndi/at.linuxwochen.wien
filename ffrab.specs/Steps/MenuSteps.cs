using System;
using TechTalk.SpecFlow;

namespace ffrab.specs.Steps
{
    [Binding]
    public class MenuSteps
    {
        [Given(@"no conference is selected")]
        public void GivenNoConferenceIsSelected()
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"the menu is shown")]
        public void WhenTheMenuIsShown()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"following menu items are shown:")]
        public void ThenFollowingMenuItemsAreShown(Table table)
        {
            ScenarioContext.Current.Pending();
        }
    }
}
