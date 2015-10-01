using TechTalk.SpecFlow;

namespace ffrab.specs.Steps
{
    [Binding]
    public class ConferenceSteps
    {
        [When(@"I open the conference choose screen")]
        public void WhenIOpenTheConferenceChooseScreen()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"I see following conferences:")]
        public void ThenISeeFollowingConferences(Table table)
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"I am attending the '(.*)'")]
        public void GivenIAmAttendingThe(string p0)
        {
            ScenarioContext.Current.Pending();
        }
    }
}
