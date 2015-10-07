Feature: Conference
	

Scenario: List supported Conferences

	When I open the conference choose screen
	Then I see following conferences:
		| title                         |
		| Vienna Mobility Quality Night |
		| Linuxwochen 2015              |
