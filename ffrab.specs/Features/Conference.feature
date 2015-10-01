Feature: Conference
	

Scenario: List supported Conferences

	When I open the conference choose screen
	Then I see following conferences:
		| title            |
		| Linuxwochen 2013 |
		| Linuxwochen 2014 |
		| Linuxwochen 2015 |
