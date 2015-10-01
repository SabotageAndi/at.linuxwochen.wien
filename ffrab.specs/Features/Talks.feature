Feature: Talks
	

Scenario: Seeing the details of the choosen talk
	Given I am attending the 'Vienna Mobile Quality Night'
	When I choose the 'Mobile UI Testautomation mit SpecFlow' talk
	Then I see the details of this talk
		| field      | value                                 |
		| title      | Mobile UI Testautomation mit SpecFlow |
		| presenter  | Andreas Willich                       |
		| start time | 21:05                                 |
		| duration   | 0:5                                   |
